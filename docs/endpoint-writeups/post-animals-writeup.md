# POST /animals: Write-up

This write-up covers the open issues, assumptions and trade-offs in the `POST /animals` change. Each section describes a concern, the reason it exists, and what it means for callers or maintainers. Each section cites the code where the behaviour is implemented. The findings come from reading the code. The endpoint has not been run against the fake client as part of this review.

The flow is: validate the LIS request, create a Register an Animal registration bundle, then return the ear tag as the animal identifier.

## Play by play: how `POST /animals` works

This is the step-by-step path for a single `POST /animals` request, as implemented in the code.

1. **Route and feature flag.** The route is registered in [CattleEndpoints.cs](../../LisFacadeApiBackend/Facade/Endpoints/CattleEndpoints.cs), lines 47–54, and is gated by the `PostAnimals` feature flag.
2. **Request received.** The handler logs `POST /animals received` and reads the JSON body into the LIS `Animal` model. Malformed JSON, an empty body, an invalid value (such as an unsupported `sex`) or a missing required property returns LIS `400`, with the JSON path and reason in `detail` (see section 6).
3. **LIS validation.** [PostAnimalsRequestValidator.cs](../../LisFacadeApiBackend/Facade/Endpoints/Validators/PostAnimalsRequestValidator.cs) `TryBuild` checks the identifier, species, sex, category, registration date, CPH format and birth date. Any failure returns LIS `400` with the validator message in `detail`, and no upstream call is made.
4. **Translate to a bundle request.** [PostAnimalsTranslator.cs](../../LisFacadeApiBackend/Facade/Services/Cattle/Translators/Animal/PostAnimalsTranslator.cs) `TranslateRequest` builds a `RegistrationBundleRequest`. The CPH is the first non-blank site identifier. The animal's ear tag, date of birth, lowercase sex, breed code, dam and sire are mapped across (see section 8).
5. **Create the bundle.** `PostAnimalsService.CreateBundleAsync` calls `CreateRegistrationBundleAsync` on the Cattle API client. The client is chosen by the `PostAnimals` client mode in configuration and defaults to the fake client. A non-2xx response is logged and thrown as an `HttpRequestException`. A 2xx response is read as a `BundleResponse`. An empty body throws `InvalidOperationException`, which returns `500`.
6. **Success.** The translator returns an `AnimalResponse` with the ear tag as `identifier` and `/animals/{ear tag}` as the self link. The endpoint returns `200`. The facade does not call Register an Animal's validate operation, so `200` means the bundle was created, not that it passed validation.
7. **Create failures.** An `HttpRequestException` from step 5 is mapped by the endpoint. A `400` upstream becomes LIS `400`. A `502`, `503` or `504` becomes `503`. Any other status becomes `500` (see section 5).
8. **Catch-all.** Any other exception, except cancellation, is logged and returned as `500`. Cancellation is not caught by the handler.

## 1. The real Cattle API client is not implemented

**Where:** [CattleApiClient.cs](../../LisFacadeApiBackend/Facade/Services/Cattle/ApiClient/CattleApiClient.cs).

**Decision:** `CreateRegistrationBundleAsync` throws `NotImplementedException`. Only the fake client implements it. The validate method has been removed from the Cattle API client, because Register an Animal no longer provides that operation.

**Issue:** According to discussions with the Register an Animal team, all the endpoints besides the get ones are still being developed, including the this one. Until we have been told this endpoint is ready, a real implementation will not be written.

## 2. The validate call has been removed

**Where:** [PostAnimalsService.cs](../../LisFacadeApiBackend/Facade/Services/Cattle/EndpointServices/Animal/PostAnimalsService.cs), [PostAnimalsTranslator.cs](../../LisFacadeApiBackend/Facade/Services/Cattle/Translators/Animal/PostAnimalsTranslator.cs), and the `PostAnimals` handler in [CattleEndpoints.cs](../../LisFacadeApiBackend/Facade/Endpoints/CattleEndpoints.cs).

**Decision:** Register an Animal's `POST /v1/registrations/{id}/validate` has been removed from the upstream API, so the facade no longer calls it. The 400 for validation failures, the 202 for validation unavailable, and the per-animal error reporting in `detail` have been removed with it. The `ValidationDetailBuilder`, the validation exceptions and the validation models have also been deleted.

## 3. A 200 does not confirm the bundle is valid

**Where:** [PostAnimalsTranslator.cs](../../LisFacadeApiBackend/Facade/Services/Cattle/Translators/Animal/PostAnimalsTranslator.cs), `Translate`.

**Issue:** The endpoint returns `200` as soon as the bundle is created. If Register an Animal rejects the content of the bundle later, the caller is not told. Callers should treat `200` as "accepted for registration", not "registered". This was a deliberate choice, and it should be confirmed with LIS and Register an Animal.

## 4. The LIS contract still documents 202 for POST /animals

**Where:** [lis-api.json](../../LisFacadeApiBackend/lis-api.json), the `POST /animals` responses, which reference `AsyncAcceptedResponse`.

**Issue:** The endpoint no longer returns `202`, despite the LIS-API documenting it as a response, as I cannot fidn an appropriate time to return it.

## 5. Error mapping for the create call


**Where:** [CattleEndpoints.cs](../../LisFacadeApiBackend/Facade/Endpoints/CattleEndpoints.cs), `PostAnimals`. The create call does not use `FromUpstream`, which is still used by GET.

**Decision:** The endpoint returns only the status codes documented for `POST /animals` in [lis-api.json](../../LisFacadeApiBackend/lis-api.json): `200`, `400`, `500` and `503`. Upstream failures on create map as follows:

| Upstream status | LIS response |
|-----------------|--------------|
| `400` | `400` |
| `502`, `503`, `504` | `503` |
| Any other failure (for example `401`, `403`, `404`) | `500` |

**Assumptions:**

- An upstream `401` means the facade's key was rejected. That is a service-side fault, so it is returned as `500`. The caller cannot fix it.
- An upstream `403` or `404` on create is a service-side problem for the caller, so it is returned as `500`. The spec does not document either code for `POST /animals`.
- The `Produces` list on the endpoint matches the spec's `POST /animals` responses.

## 6. Validation checks and error reporting

**Where:** [PostAnimalsRequestValidator.cs](../../LisFacadeApiBackend/Facade/Endpoints/Validators/PostAnimalsRequestValidator.cs), `TryBuild`, called from the `PostAnimals` handler in [CattleEndpoints.cs](../../LisFacadeApiBackend/Facade/Endpoints/CattleEndpoints.cs).

**Decision:** The handler reads the body into the LIS `Animal` model. `TryBuild` then runs these checks in order and stops at the first failure:

1. `animal.identifier` is not blank.
2. `animal.species` is `cattle`.
3. `animal.sex` is a defined value. `Enum.IsDefined` catches numeric values that the JSON reader accepts.
4. `registration.category` is a defined value, checked the same way.
5. `registration.date` is set. A value equal to `default(DateOnly)` is rejected.
6. The first non-blank `registration.site` identifier is in `county/parish/holding` format (see section 8).
7. `birth.date`, when present, is no more than one UTC day ahead of the current UTC date. UTC+14 is the furthest ahead of UTC, so a birth date that is today locally can be one day ahead in UTC.

A failed check returns LIS `400` with its message in `detail`, and no upstream call is made. Only one message is returned per request.

**JSON errors:** Malformed JSON, an empty body, an unsupported enum value (for example `sex: "unknown"`) and a missing required property are rejected by the JSON reader before `TryBuild` runs. The handler returns LIS `400` with the JSON path and reason in `detail`, for example `The request body is invalid at $.animal.sex: ...`. The `required` modifiers on the model are what make the reader reject missing properties.

**Remaining issues:**

- Missing required fields are reported with the JSON path, not the LIS field name, and not by the validator. Making these fields nullable on `Animal`, `AnimalRecordDetails` and `AnimalRegistration` would let the validator report them by LIS field name. That is a wider change, that, if we decide to make, will have it's own ticket.
- `registration.date` and `category` are validated but not sent to Register an Animal. The mapping documents list them as unmapped ([post-cattle-registration-bundle.md](../schema-mappings/endpoints/post-cattle-registration-bundle.md), line 417), so the translator drops them. This is a translator issue, not a validator one.

## 7. CPH format is validated, as in GET

**Where:** [PostAnimalsRequestValidator.cs](../../LisFacadeApiBackend/Facade/Endpoints/Validators/PostAnimalsRequestValidator.cs), `TryBuild`. The translator sends the same identifier, [PostAnimalsTranslator.cs](../../LisFacadeApiBackend/Facade/Services/Cattle/Translators/Animal/PostAnimalsTranslator.cs), `TranslateRequest`.

**Decision:** The first non-blank `registration.site` identifier must split on `/` into exactly three non-blank segments, and none of them may be `.` or `..`. This is the same rule as [GetAnimalsService.cs](../../LisFacadeApiBackend/Facade/Services/Cattle/EndpointServices/Animal/GetAnimalsService.cs), line 15. Anything else returns LIS `400`.

**Assumption:** The POST CPH uses the same `county/parish/holding` format as GET. The evaluation summary says hyphen- and slash-separated values "must not be assumed to be interchangeable", so this should be confirmed with LIS.

## 8. Parent mapping and breed

**Where:** [PostAnimalsTranslator.cs](../../LisFacadeApiBackend/Facade/Services/Cattle/Translators/Animal/PostAnimalsTranslator.cs), `TranslateRequest`, `ToDam` and `ToSire`.

**Decision:**

- Dam: the genetic dam comes from `registration.geneticDam`, and the surrogate (birth) dam from `importParents.birthDam`. A dam object is sent when either is present.
- Sire: `registration.sire` is used, falling back to `importParents.sire`.
- Breed: only `breed.code` is sent. A breed with no code is sent as `null`. The name is not used, because Register an Animal's breed is a 4-character code.

**Assumptions:**

- The dam split matches the GET mapping's split.
- The sire fallback to `importParents` is a choice. It is not in the mapping document.

## Summary

| # | Area | Risk | Action needed |
|---|------|------|---------------|
| 1 | Real client not implemented | `500` if mode is switched to `real` | Implement before going live |
| 2 | 200 does not confirm validity | Register an Animal's validation is no longer called, so invalid bundles are reported as success | Agree whether this is acceptable; revisit if LIS needs validation feedback |
| 3 | `lis-api.json` still documents 202 | Contract shows a response the endpoint no longer returns | Update `lis-api.json` once the change is agreed |
| 4 | Create error mapping | Only spec codes (`400`, `500`, `503`) are returned | Confirm `401`/`403`/`404` → `500` with LIS |
| 5 | Validation checks and error reporting | Validator returns the first failure only; missing JSON fields are reported by the JSON reader with the JSON path; `registration.date` and `category` not sent | Make the model fields nullable (wider change in future ticket); map `date` and `category` |
| 6 | CPH format | Validated as `county/parish/holding`, same as GET | Confirm the format with LIS |
| 7 | Parent mapping and breed | Breed sent as code only; sire falls back to `importParents` | Confirm the sire fallback |
