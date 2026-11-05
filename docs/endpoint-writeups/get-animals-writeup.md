# GET /animals: Write-up

This write-up covers the open issues, assumptions and trade-offs in the `GET /animals` change. Each section describes a concern, the reason it exists, and what it means for callers or maintainers. Each section cites the code where the behaviour is implemented.

## 1. Invalid query parameters return 404

**Where:** [CattleEndpoints.cs](../../LisFacadeApiBackend/Facade/Endpoints/CattleEndpoints.cs), line 57. Validation lives in [GetAnimalsRequestValidator.cs](../../LisFacadeApiBackend/Facade/Endpoints/Validators/GetAnimalsRequestValidator.cs).

**Decision:** Invalid or missing query parameters (`species`, `siteIdentifier`, `page`, `pageSize`, `order`, `sort`) return 404. LIS documents no 400 for this operation, so the facade mirrors it.

**Issue:** A 404 is ambiguous, however it is the closest error response that LIS returns. A caller who mistypes `species=cattl` receives the same response as one asking for a site that does not exist. The only distinction is in the server logs, which record the validation message.

## 2. Non-cattle species return an empty list

**Where:** [GetAnimalsTranslator.cs](../../LisFacadeApiBackend/Facade/Services/Cattle/Translators/Animal/GetAnimalsTranslator.cs), lines 17–19.

**Decision:** Any species other than cattle returns `200 OK` with `Count = 0` and no upstream call is made. The upstream cattle API only holds cattle.

**Issue:** Current API only contains cattle, so other animals cannot be returned. Routing by species could be added later, which would change this behaviour e.g. the translator checks which species is in the request and filters passes through to the correct service

## 3. Site identifier is assumed to be a CPH

**Where:** [GetAnimalsTranslator.cs](../../LisFacadeApiBackend/Facade/Services/Cattle/Translators/Animal/GetAnimalsTranslator.cs), line 22. The schema mapping is in [get-cattle-by-CPH.md](../schema-mappings/endpoints/get-cattle-by-CPH.md).

**Decision:** `siteIdentifier` is split on `/` into county, parish and holding, and passed to `GetCattleForHoldingMultiSegmentAsync`. LIS expects slash-separated segments according to the schema (for example `28/822/2977`). An identifier that does not split into three non-empty segments returns 404 without an upstream call.

```
{
    "name": "siteIdentifier",
    "in": "query",
    "allowEmptyValue": false,
    "required": true,
    "schema": {
        "type": "string"
    },
    "description": "The end-point returns animals for this site.",
    "example": "28/822/2977"
},
```

**Issue:** The site identifier format is taken from the LIS schema example. We have not validated that siteIdentifiers from LIS match the CPH used for Register an Animal, and hyphen-separated identifiers are now rejected rather than matched.

## 4. Upstream status values are not verified against LIS

**Where:** [GetAnimalsTranslator.cs](../../LisFacadeApiBackend/Facade/Services/Cattle/Translators/Animal/GetAnimalsTranslator.cs), lines 42–44.

**Decision:** The upstream `Status` is parsed into the LIS `AnimalState` enum. Sorting by `order=state` uses the same values.

**Issue:** The contracts do not confirm that the two value sets match. Any upstream status that does not parse becomes `null`, so the animal is still returned but with no state and it sorts first. This is silent. It should be confirmed against the contracts and, if the sets differ, mapped explicitly.

## 5. Paging is applied in the facade

**Where:** [GetAnimalsTranslator.cs](../../LisFacadeApiBackend/Facade/Services/Cattle/Translators/Animal/GetAnimalsTranslator.cs), lines 49–61.

**Decision:** The upstream API is unpaged, so the facade fetches every animal for the site, sorts them, and then slices out the requested page. `Count` is the total before slicing.

**Issues:**

- Every request loads the whole herd, regardless of `pageSize`. This may matter for large holdings.
- `Count` is computed after animals with invalid sex are removed (see section 6), so it is not the number of animals upstream holds.

## 6. Animals with invalid sex are silently skipped

**Where:** [GetAnimalsTranslator.cs](../../LisFacadeApiBackend/Facade/Services/Cattle/Translators/Animal/GetAnimalsTranslator.cs), lines 30–39 (loop) and 66–73 (`ToSummary` null return).

**Decision:** `ToSummary` returns `null` when the sex cannot be parsed. The animal is dropped and a warning is logged, so one bad record does not fail the whole list. We can assume that the sexs used by both APIs should match, but haven't been able to confirm

**Issue:** The caller receives no indication that animals are missing. Because `Count` is calculated after the skip, the count also excludes them. A client has no way to tell that the list is incomplete.

## 7. Field nullability

**Where:** [GetAnimalsTranslator.cs](../../LisFacadeApiBackend/Facade/Services/Cattle/Translators/Animal/GetAnimalsTranslator.cs), lines 66–91 (`ToSummary`).

- **Sex** is a string upstream, not an enum. It can be invalid, which leads to the skip described in section 6, although this is unlikely.
- **Identifier** is required in LIS, but it is taken from the upstream ear tag.
- **Species** is hard-coded to cattle, because the upstream API does not supply it. In the future when more species are used it could potentially be figured out from the breed information.
- **State and status** are both optional and nullable in the models. Both are affected by the assumption in section 4.
- **Breed** is nullable in both the upstream and LIS. The translator sets it to `null` only when both breed code and name are missing.

## Summary

| # | Area | Risk | Action needed |
|---|------|------|---------------|
| 1 | Invalid parameters return 404 | Callers cannot tell a bad request from a missing site | N/A, LIS fault|
| 2 | Non-cattle species | Empty result hides whether the site exists | Revisit when other species are supported |
| 3 | Site identifier as CPH | Identifiers not in `county/parish/holding` form return 404 | Confirm LIS siteIdentifier format matches CPH used for Register an Animal |
| 4 | Status values | Unverified mapping; unparseable states sort first | Verify against contracts |
| 5 | Facade paging | Whole herd fetched per request; odd behaviour without `pageSize` | N/A, cattle doesn't use paging |
| 6 | Invalid sex skipped | Silent data omission; `Count` excludes skipped animals | Verify against contacts |
| 7 | Field nullability | Partial mapping; species is hard-coded | N/A, current mapping is best we can provide |
