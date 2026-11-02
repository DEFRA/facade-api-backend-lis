# GET /v1/holdings/{county}/{parish}/{holding}/bundles: API schema mapping

This evaluates the Register an Animal request and response for listing registration bundles on a holding against the LIS OpenAPI specification. The Register an Animal repository defines the endpoint and DTOs; the LIS specification defines possible counterparts. **N/A** means no LIS *bundle-list* field or operation exists, not that the Register an Animal field is absent. **Related only** identifies a real field on a *different* LIS animal or request-status operation; it is not a direct mapping for a local submission. Neither similar names nor animal attributes establish an equivalent bundle resource.

## Object Mapping

| Register animal/cattle API object | LIS API object | Notes |
|---|---|---|
| Three required CPH path segments | N/A | LIS has no operation to list registration bundles by CPH. `siteIdentifier` on LIS animal search selects animals, not locally recorded submissions. |
| `BundleResponse[]` | N/A | The response lists locally stored submissions, not animals returned by LIS `GET /animals`. |
| `BundleAnimalResponse[]` inside each bundle | N/A | These are members of a submission; an LIS animal summary cannot identify the submission to which they belong or its processing status. |
| `BundleAnimalErrorResponse[]` inside each animal | N/A | These are locally recorded validation/processing errors, not LIS HTTP `Errors[]`. |
| Local `ProblemDetails` on failure | N/A for a bundle-list operation | Errors from other LIS operations cannot be assumed to have the same cause or status. |

## Endpoint Mapping

| Register an animal/cattle API endpoint | LIS API endpoint | Equivalence / transformation |
|---|---|---|
| `GET /v1/holdings/{county}/{parish}/{holding}/bundles` | **No equivalent** | Returns all locally stored registration submissions for one exact CPH, newest first, with their animals and per-animal errors. No LIS operation lists bundles or submissions by holding. |

The route joins the three segments as `county/parish/holding`, URI-unescapes the resulting string, and passes it to `GetBundlesForHoldingAsync`. That service reads the **local database**: submissions whose stored `CountyParishHolding` equals the supplied CPH, ordered by `CreatedAt` descending. It includes their animals and errors, without filtering by submission status, and returns `200` with an empty array when none exist. The separate `/v1/holdings/{cph}/bundles` route uses the same helper but is **not** the endpoint evaluated here.

LIS `GET /animals` lists animals at a site; LIS `GET /requeststatus/{requestId}` retrieves the status of **one** asynchronous request by its request ID. Its `RequestStatus` result can contain an `AnimalResponse`, but it does not list local registration submissions, their members, or their errors by CPH. LIS `POST /animals` creates an animal, not a bundle-list lookup. The LIS `batches` schema belongs to movement contracts; there is no registration-bundle listing operation. Do not equate LIS animal `state` or asynchronous request `status` with a local bundle or animal processing `status`.

## Request Mapping

There is **no request body or query filter**. The three required path strings are joined with slashes; this route has no CPH-format validation specific to bundles. Matching against the stored CPH string is exact in the database query. The route has no LIS request to translate into.

| Register an Animal field | Data type | LIS field | Data type 2 | Transformation | Missing fields | Datatype mismatch | Enum/value mismatch | Cardinality differences | Breaking change | Notes |
|---|---|---|---|---|---|---|---|---|---|---|
| `county` (required path) | string | N/A | N/A | Join with `parish` and `holding` to query local submissions; no LIS bundle query exists | Yes: endpoint | N/A | N/A | Three segments to one local CPH | Endpoint-level blocker | No route-specific numeric-length validation. |
| `parish` (required path) | string | N/A | N/A | Join with other path segments for local CPH lookup | Yes: endpoint | N/A | N/A | Three segments to one local CPH | Endpoint-level blocker | Do not map to a LIS animal filter. |
| `holding` (required path) | string | N/A | N/A | Join with other path segments for local CPH lookup | Yes: endpoint | N/A | N/A | Three segments to one local CPH | Endpoint-level blocker | No LIS list-by-holding for submissions. |

**Headers (not request-body fields):** Register an Animal requires service-to-service API-key authorization and one nonblank `x-cdp-request-id` header on this versioned route. LIS operations have their own authentication; none is a bundle-list operation. A future facade must preserve the local authorization and correlation contract independently of any LIS calls.

## Response Mapping

### `200 OK` — array of `BundleResponse`

The response is an unpaged JSON array, with zero or more submissions. Each submission contains zero or more animals; each animal contains zero or more locally stored errors. `Nullable` below reflects DTO types; `Required` is **Not explicit** because C# DTO initializers do not alone define OpenAPI requiredness. The fields below inventory this response; **related-only LIS fields are analogues on other operations, not data sources proven to reproduce submitted values**. An ear tag in LIS does not establish local bundle membership, and similarly named LIS `status`/`errors` fields have different meanings.

| Register an Animal field | Data type | Nullable | Required | LIS field | Data type 2 | Nullable 2 | Required 2 | Transformation | Missing fields | Datatype mismatch | Enum/value mismatch | Cardinality differences | Possible breaking change | Notes |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| response `[]` | array of `BundleResponse` | No if returned | Not explicit | N/A | N/A | N/A | N/A | Requires local submissions by CPH | Yes: bundle list | N/A | N/A | Zero or more bundles | Endpoint-level blocker | No LIS bundle collection to transform. |
| `[].id` | GUID | No | Not explicit | **Related only:** LIS async accepted `id` / request-status path `requestId` | UUID | Not established | Not equivalent | Preserve local submission identifier; do not substitute LIS request ID | Yes: same submission identity/link | Both GUID-like | Identity semantics unverified | One per bundle vs one per LIS request | Endpoint-level blocker | No stored link from this local submission to a LIS request is shown. |
| `[].clientReference` | string | No in DTO | Not explicit | N/A | N/A | N/A | N/A | Read from local submission | Yes | N/A | N/A | One per bundle | Endpoint-level blocker | Client-supplied reference, not an animal identifier. |
| `[].countyParishHolding` | string | No in DTO | Not explicit | **Related only:** LIS `GET /animals` query `siteIdentifier`; `Animal.registration.site.identifiers[].identifier` | string | Not established | Not equivalent | Return the CPH stored with the submission | Yes: stored bundle CPH | No for strings | Site/CPH equivalence unconfirmed | One bundle CPH vs site identifiers on animals | Endpoint-level blocker | Neither LIS value proves the CPH under which a bundle was submitted. |
| `[].submittedBy` | string | No in DTO | Not explicit | N/A | N/A | N/A | N/A | Read from local submission | Yes | N/A | N/A | One per bundle | Endpoint-level blocker | Submitter identity is local metadata. |
| `[].status` | string | No in DTO | Not explicit | N/A | N/A | N/A | N/A | Read local submission processing status | Yes | N/A | N/A | One per bundle | Endpoint-level blocker | Local values include `submitted`, `pending`, `processing`, `complete`, `error`; LIS asynchronous status has different resource identity/semantics. |
| `[].createdAt` | date-time (`DateTimeOffset`) | No | Not explicit | **Related only:** LIS `Animal.createdDateTime` / async accepted `requestDate` | date-time | Not established | Not equivalent | Read local submission timestamp; cannot substitute an animal or LIS-request timestamp | Yes: submission creation time | Same general format | Events differ | One per bundle vs per animal/request | Endpoint-level blocker | Used to order bundles newest first. |
| `[].animals[]` | array of `BundleAnimalResponse` | No in DTO | Not explicit | N/A | N/A | N/A | N/A | Include locally associated submission animals | Yes: bundle membership | N/A | N/A | Zero or more per bundle | Endpoint-level blocker | LIS `GET /animals` does not group animals by submission. |
| `[].animals[].id` | GUID | No | Not explicit | N/A | N/A | N/A | N/A | Read local submission-animal ID | Yes | N/A | N/A | One per bundle animal | Endpoint-level blocker | Not an ear tag or LIS animal identifier. |
| `[].animals[].submissionId` | GUID | No | Not explicit | N/A | N/A | N/A | N/A | Preserve link to local bundle ID | Yes | N/A | N/A | One per bundle animal | Endpoint-level blocker | LIS animals have no local submission foreign key. |
| `[].animals[].status` | string | No in DTO | Not explicit | N/A | N/A | N/A | N/A | Read local animal processing status | Yes | N/A | N/A | One per bundle animal | Endpoint-level blocker | Not LIS `animal.state`. |
| `[].animals[].earTag` | string | No in DTO | Not explicit | **Related only:** LIS `Animal.animal.identifier` / `AnimalSummary.animal.identifier` | string | Not declared nullable | Required if animal exists | Preserve submitted ear tag; confirm identity before using LIS identifier for other operations | Yes: local membership | No for strings | Identifier format unconfirmed | One per bundle animal vs independent LIS animal | Endpoint-level blocker | A matching animal does not identify the bundle it belongs to. |
| `[].animals[].dateBirth` | date (`DateOnly`) | Yes | Not explicit | **Related only:** LIS `Animal.birth.date` | date string; optional | Not declared nullable | No | Preserve submitted date, not an unverified current LIS value | Yes: submitted snapshot and membership | Date format conversion | Values can differ over time | One per bundle animal vs independent LIS animal | Endpoint-level blocker | LIS list `AnimalSummary` does not carry birth date; detail does. |
| `[].animals[].sex` | string | Yes | Not explicit | **Related only:** LIS `Animal.animal.sex` / `AnimalSummary.animal.sex` | enum `female`/`male` string | Not declared nullable | Required if animal exists | Preserve submitted value; current animal value may differ | Yes: submitted snapshot and membership | No for present strings | Local free string vs LIS enum | One per bundle animal vs independent LIS animal | Endpoint-level blocker | Not a safe lookup for historical submitted sex. |
| `[].animals[].breed` | string | Yes | Not explicit | **Related only:** LIS `Animal.breed.name` / `breed.code` | optional strings in nested object | Not declared nullable | No | Preserve submitted breed string; no proven code/name equivalence | Yes: submitted snapshot and membership | Nested object vs string | Code/name and historic value unverified | One per bundle animal vs independent LIS animal | Endpoint-level blocker | Original submitted value may not equal current LIS breed. |
| `[].animals[].damType` | string | Yes | Not explicit | **Related only:** LIS `Animal.registration.birthDam` / `geneticDam` | optional parent objects | Not established | No | No proven conversion from parent objects to local `damType` | Yes: submitted dam-type choice | Object vs string | Local dam type semantics unverified | Per submission animal vs independent animal | Endpoint-level blocker | Presence of LIS parent records is not a submitted dam-type value. |
| `[].animals[].damGeneticEarTag` | string | Yes | Not explicit | **Related only:** LIS `Animal.registration.geneticDam.identifier` | string if parent exists | Not declared nullable | No | Preserve submitted parent tag; LIS detail may describe a parent, not the submitted value | Yes: submitted snapshot and membership | No for strings | Parent identity/role requires verification | Per submission animal vs independent animal | Endpoint-level blocker | LIS also has `importParents.geneticDam`; neither establishes bundle history. |
| `[].animals[].damSurrogateEarTag` | string | Yes | Not explicit | **Related only:** LIS `Animal.registration.birthDam.identifier` | string if parent exists | Not declared nullable | No | No confirmed surrogate-to-birth-dam rule; preserve submitted tag | Yes: submitted surrogate relationship | No for strings | Birth dam need not prove surrogate-dam type | Per submission animal vs independent animal | Endpoint-level blocker | LIS also has `importParents.birthDam`; no `surrogateDam` field. |
| `[].animals[].sireEarTag` | string | Yes | Not explicit | **Related only:** LIS `Animal.registration.sire.identifier` | string if parent exists | Not declared nullable | No | Preserve submitted sire tag; LIS parentage would require identity and temporal checks | Yes: submitted snapshot and membership | No for strings | Parent role plausible, not provenance | Per submission animal vs independent animal | Endpoint-level blocker | LIS also has `importParents.sire`. |
| `[].animals[].sireName` | string | Yes | Not explicit | **Related only:** LIS `Animal.registration.sire.name` | optional read-only string | Not declared nullable | No | Preserve submitted sire name; LIS read-only parent name is not proven to be submitted name | Yes: submitted snapshot and membership | No for strings | Current/display name may differ | Per submission animal vs independent animal | Endpoint-level blocker | Not a recoverable bundle input from current LIS animal details. |
| `[].animals[].errors[]` | array of `BundleAnimalErrorResponse` | No in DTO | Not explicit | N/A | N/A | N/A | N/A | Include locally stored per-animal errors | Yes | N/A | N/A | Zero or more per bundle animal | Endpoint-level blocker | LIS HTTP error arrays describe failed API requests, not submission-animal errors. |
| `[].animals[].errors[].id` | GUID | No | Not explicit | N/A | N/A | N/A | N/A | Read local error ID | Yes | N/A | N/A | One per error | Endpoint-level blocker |  |
| `[].animals[].errors[].animalId` | GUID | No | Not explicit | N/A | N/A | N/A | N/A | Preserve local animal foreign key | Yes | N/A | N/A | One per error | Endpoint-level blocker |  |
| `[].animals[].errors[].errorCode` | string | No in DTO | Not explicit | N/A | N/A | N/A | N/A | Read local validation/processing code | Yes | N/A | N/A | One per error | Endpoint-level blocker |  |
| `[].animals[].errors[].errorText` | string | No in DTO | Not explicit | N/A | N/A | N/A | N/A | Read local validation/processing message | Yes | N/A | N/A | One per error | Endpoint-level blocker |  |
| `[].animals[].errors[].createdAt` | date-time (`DateTime?`) | Yes | Not explicit | N/A | N/A | N/A | N/A | Read local error timestamp | Yes | N/A | N/A | One per error | Endpoint-level blocker | Unlike bundle `createdAt`, nullable in the DTO. |

### `401 Unauthorized` — local `ProblemDetails`

A rejected or missing service-to-service API key yields local `401` with explicitly set `type`, `title`, `status`, and `detail`; `instance` is not explicitly set in this authentication challenge. There is **no LIS bundle-list `401`** to map. LIS `GET /requeststatus/{requestId}` documents `404`, `500` and `503` error arrays for its *different* single-request lookup, not an error contract for this endpoint.

| Register an Animal field | Data type | Nullable | Required | LIS field | Data type 2 | Nullable 2 | Required 2 | Transformation | Missing fields | Datatype mismatch | Enum/value mismatch | Cardinality differences | Possible breaking change | Notes |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `type` | string | No on API-key rejection | Explicitly set | N/A | N/A | N/A | N/A | Generated by local authentication | Yes: operation | N/A | N/A | N/A | Endpoint-level blocker |  |
| `title` | string | No on API-key rejection | Explicitly set | N/A | N/A | N/A | N/A | Generated by local authentication | Yes: operation | N/A | N/A | N/A | Endpoint-level blocker |  |
| `status` | integer (`401`) | No | Explicitly set | N/A | N/A | N/A | N/A | Preserve locally determined authorization failure | Yes: operation | N/A | N/A | N/A | Endpoint-level blocker | Not inferred from another LIS operation. |
| `detail` | string | No on API-key rejection | Explicitly set | N/A | N/A | N/A | N/A | Generated by local authentication | Yes: operation | N/A | N/A | N/A | Endpoint-level blocker |  |
| `instance` | string | Conditional | Not explicit | N/A | N/A | N/A | N/A | Not explicitly set by authentication challenge | Yes: operation | N/A | N/A | N/A | Endpoint-level blocker | Exception-handler errors may set it separately. |

### `400 Bad Request`, `404 Not Found`, `500 Internal Server Error`, `503 Service Unavailable`

A missing or nonblank-required `x-cdp-request-id` returns local `400 ProblemDetails` with `code: missing_header`, `type`, `title`, `status`, `detail`, `instance` and `traceId`. The exception handler also maps argument errors to `400`, `NotFoundException` to `404`, service unavailability to `503`, and unhandled exceptions to `500`, adding `traceId` and `correlationId`. **No bundles** is not a `404`: the query returns `200` with `[]`. The bundle service does not call LIS or CADS; its normal database lookup does not raise a not-found exception for an empty result. These failure bodies have no LIS bundle-list status/body mapping.

## Breaking changes and decisions

| Finding | Status / impact | Decision needed |
|---|---|---|
| LIS has no operation to list local registration bundles by holding. | **Endpoint-level blocker.** | Keep an authoritative local submission store/lookup, or agree an explicit new LIS contract. Do not substitute animal or request-status searches. |
| LIS individual-animal schemas contain similar identifiers, birth, sex, breed and parentage fields. | **Related field concepts, not direct bundle response mappings.** | Preserve submitted snapshots and bundle membership; investigate any animal-field translation separately from this local-history endpoint. |
| Every returned ID, client reference, submitter, processing status, timestamp, membership link and error belongs to local submissions. | **No direct LIS response translation.** | Define how the facade retains and reads those records; do not fabricate local history from current animal data. |
| LIS `GET /requeststatus/{requestId}` looks up one asynchronous request, not all submissions for a CPH. | **Identifier, cardinality, and resource mismatch.** | Only connect it to local submissions if a separately defined and stored request-ID relationship actually exists; it cannot replace this endpoint. |
| Empty result is `200 []`, and local correlation/authentication errors are produced independently. | **Behavior and failure-contract mismatch.** | Preserve the empty-list and local error behavior rather than interpreting an unrelated LIS `404` as no bundles. |

**Result:** This endpoint is a **local submission-history lookup** and has no documented LIS equivalent. A LIS animal operation may be relevant to registering an individual animal elsewhere, but does not supply a list of bundles, their member records, or processing errors.
