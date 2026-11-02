# GET /v1/holdings/{cph}/bundles: API schema mapping

This evaluates the single-CPH Register an Animal route against the LIS OpenAPI specification. It is a distinct route from `GET /v1/holdings/{county}/{parish}/{holding}/bundles`, but both use the same service and response. The repository defines Register an Animal behavior; the LIS specification defines possible LIS counterparts. **N/A** means no LIS *bundle-list* field or operation, not that the concept is absent from every LIS model. **Related only** identifies a real field on another LIS operation that cannot be used to reconstruct a submitted bundle without more evidence.

## Object Mapping

| Register animal/cattle API object | LIS API object | Notes |
|---|---|---|
| Required `cph` path parameter | N/A | LIS has no registration-bundle lookup by CPH. `siteIdentifier` on LIS animal search selects animals, not submissions. |
| `BundleResponse[]` | N/A | Local submission records, not LIS animal or asynchronous request records. |
| `BundleAnimalResponse[]` nested in each bundle | N/A | LIS animals are not grouped by local submission. Some individual animal attributes are related only; see response inventory. |
| `BundleAnimalErrorResponse[]` nested in each animal | N/A | Local per-animal processing errors, not LIS HTTP `Errors[]`. |
| Local `ProblemDetails` | N/A for a bundle-list operation | Error models on other LIS endpoints do not establish an error mapping for this route. |

## Endpoint Mapping

| Register an animal/cattle API endpoint | LIS API endpoint | Equivalence / transformation |
|---|---|---|
| `GET /v1/holdings/{cph}/bundles` | **No equivalent** | Lists locally stored registration submissions for one exact CPH, newest first, including their animals and errors. LIS has no operation to list bundles or submissions by holding. |

The route binds one `cph` path value, passes it through `Uri.UnescapeDataString` in the shared helper, and calls `GetBundlesForHoldingAsync`. Unlike the three-part route, it does **not** join `county/parish/holding`. The service queries the local database with `Submission.CountyParishHolding == cph`, orders by `CreatedAt` descending, includes the animals and their errors, and does not filter by status. A hyphenated single-segment example (`12-345-6789`) is shown in route tests; this proves routing, **not** that it matches a slash-separated CPH stored in the database. A slash-separated URL is tested through the separate three-part route. No automatic hyphen-to-slash conversion is implemented by this helper.

LIS `GET /animals` returns animals at a site, not submitted bundles. `GET /requeststatus/{requestId}` looks up **one** asynchronous request by its request ID, not submissions by CPH. `POST /animals` creates an individual animal; the LIS `batches` schema belongs to movement contracts. None provides a bundle-list response or a confirmed link to local bundle IDs.

## Request Mapping

There is **no request body or query filter**. The path supplies one required string. The route has no CPH-format validator; successful inclusion of a submission depends on an exact match to the stored CPH value after URI unescaping. There is no LIS bundle-list request to translate into.

| Register an Animal field | Data type | LIS field | Data type 2 | Transformation | Missing fields | Datatype mismatch | Enum/value mismatch | Cardinality differences | Breaking change | Notes |
|---|---|---|---|---|---|---|---|---|---|---|
| `cph` (required path) | string, one path segment | N/A for bundles; **related only:** LIS animal-list `siteIdentifier` | required string query on a different operation | Unescape path value and use for exact local submission lookup; no LIS bundle lookup exists | Yes: endpoint | Path vs query placement on unrelated animal search | Site identifier and stored CPH equivalence not proven | One CPH to zero or more local bundles | Endpoint-level blocker | A hyphenated path may not match slash-separated stored CPH; do not silently normalize without an agreed rule. |

**Headers (not request-body fields):** The versioned route requires service-to-service API-key authorization and one nonblank `x-cdp-request-id`. LIS operations use different credentials, but none is a bundle-list operation. Any facade should preserve local authorization and correlation independently of unrelated LIS operations.

## Response Mapping

### `200 OK` — array of `BundleResponse`

The endpoint returns an unpaged JSON array: zero or more locally stored submissions, each with zero or more animals and each animal with zero or more errors. No matches returns `200 []`, **not** `404`. The DTO types determine the `Nullable` column; `Required` remains **Not explicit**, since C# property initializers alone do not declare OpenAPI required fields. The table inventories the complete response. LIS fields marked **related only** describe an animal or a separate asynchronous request, not the historical bundle record or its membership.

| Register an Animal field | Data type | Nullable | Required | LIS field | Data type 2 | Nullable 2 | Required 2 | Transformation | Missing fields | Datatype mismatch | Enum/value mismatch | Cardinality differences | Possible breaking change | Notes |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| response `[]` | array of `BundleResponse` | No if returned | Not explicit | N/A | N/A | N/A | N/A | Retrieve all local submissions for the exact CPH | Yes: bundle list | N/A | N/A | Zero or more bundles | Endpoint-level blocker | No LIS collection to transform into this result. |
| `[].id` | GUID | No | Not explicit | **Related only:** LIS async accepted `id` / request-status path `requestId` | UUID | Not established | Not equivalent | Preserve local submission ID; do not substitute LIS request ID | Yes: proven identity link | GUID-like | Resource identities differ | One per bundle vs one per LIS request | Endpoint-level blocker | No stored link to a LIS request is shown. |
| `[].clientReference` | string | No in DTO | Not explicit | N/A | N/A | N/A | N/A | Read locally stored client reference | Yes | N/A | N/A | One per bundle | Endpoint-level blocker | Not an animal identifier. |
| `[].countyParishHolding` | string | No in DTO | Not explicit | **Related only:** LIS animal-list `siteIdentifier` / `Animal.registration.site.identifiers[].identifier` | string | Not established | Not equivalent | Return the CPH stored on the submission | Yes: submission's CPH | No for strings | CPH and site identity unverified | One per bundle vs identifiers for an animal site | Endpoint-level blocker | Returned stored value need not be a normalized form of the request. |
| `[].submittedBy` | string | No in DTO | Not explicit | N/A | N/A | N/A | N/A | Read local submitter | Yes | N/A | N/A | One per bundle | Endpoint-level blocker | No LIS bundle submitter field. |
| `[].status` | string | No in DTO | Not explicit | N/A; **related only:** LIS `RequestStatus.status` | enum `Pending`, `Completed`, `Failed` on a different resource | Not established | Not equivalent | Read local submission processing status | Yes: local status | Free string vs enum | Local values include `submitted`, `pending`, `processing`, `complete`, `error` | One per bundle vs one per async request | Endpoint-level blocker | LIS `animal.state` is also unrelated. |
| `[].createdAt` | date-time (`DateTimeOffset`) | No | Not explicit | **Related only:** LIS `Animal.createdDateTime` / async accepted `requestDate` | date-time | Not established | Not equivalent | Read local submission timestamp | Yes: local submission creation | Same general format | Different event times | One per bundle vs animal/request | Endpoint-level blocker | Determines descending bundle order. |
| `[].animals[]` | array of `BundleAnimalResponse` | No in DTO | Not explicit | N/A | N/A | N/A | N/A | Read local submission's members | Yes: bundle membership | N/A | N/A | Zero or more per bundle | Endpoint-level blocker | LIS animal search is not grouped by local submission. |
| `[].animals[].id` | GUID | No | Not explicit | N/A | N/A | N/A | N/A | Read local submission-animal ID | Yes | N/A | N/A | One per member | Endpoint-level blocker | Not a LIS animal identifier. |
| `[].animals[].submissionId` | GUID | No | Not explicit | N/A | N/A | N/A | N/A | Preserve local bundle foreign key | Yes | N/A | N/A | One per member | Endpoint-level blocker | LIS has no equivalent membership link. |
| `[].animals[].status` | string | No in DTO | Not explicit | N/A | N/A | N/A | N/A | Read local animal processing status | Yes | N/A | N/A | One per member | Endpoint-level blocker | Not LIS `animal.state` or async request status. |
| `[].animals[].earTag` | string | No in DTO | Not explicit | **Related only:** LIS `Animal.animal.identifier` / `AnimalSummary.animal.identifier` | string | Not declared nullable | Required if animal exists | Preserve submitted ear tag; confirm identity for separate animal operations | Yes: submitted value and membership | No for strings | Identifier format unverified | One per member vs independent LIS animal | Endpoint-level blocker | A matching ear tag does not establish bundle membership. |
| `[].animals[].dateBirth` | date (`DateOnly`) | Yes | Not explicit | **Related only:** LIS `Animal.birth.date` | optional date string | Not declared nullable | No | Preserve submitted birth date, not unverified current LIS data | Yes: submitted snapshot and membership | Date format conversion | Historical value can differ | One per member vs independent animal | Endpoint-level blocker | LIS `AnimalSummary` has no birth date. |
| `[].animals[].sex` | string | Yes | Not explicit | **Related only:** LIS `Animal.animal.sex` / `AnimalSummary.animal.sex` | enum `female`/`male` string | Not declared nullable | Required if animal exists | Preserve submitted value rather than reconstructing it | Yes: submitted snapshot and membership | No for strings | Local free string vs LIS enum | One per member vs independent animal | Endpoint-level blocker | Current animal data does not prove original submission value. |
| `[].animals[].breed` | string | Yes | Not explicit | **Related only:** LIS `Animal.breed.name` / `Animal.breed.code` | optional nested strings | Not declared nullable | No | Preserve submitted breed string | Yes: submitted snapshot and membership | Nested object vs string | Code/name and historical value unverified | One per member vs independent animal | Endpoint-level blocker | Do not assume submitted string was a LIS breed name. |
| `[].animals[].damType` | string | Yes | Not explicit | **Related only:** LIS `Animal.registration.birthDam` / `geneticDam` | optional parent objects | Not established | No | No proven conversion from parent objects to submitted dam type | Yes: submitted dam-type choice | Object vs string | Semantics unverified | Per member vs independent animal | Endpoint-level blocker | Parentage presence is not the submitted choice. |
| `[].animals[].damGeneticEarTag` | string | Yes | Not explicit | **Related only:** LIS `Animal.registration.geneticDam.identifier` | string when parent exists | Not declared nullable | No | Preserve submitted genetic-dam ear tag | Yes: submitted snapshot and membership | No for strings | Role/identity requires confirmation | Per member vs independent animal | Endpoint-level blocker | `importParents.geneticDam` is another non-historical LIS attribute. |
| `[].animals[].damSurrogateEarTag` | string | Yes | Not explicit | **Related only:** LIS `Animal.registration.birthDam.identifier` | string when parent exists | Not declared nullable | No | No confirmed surrogate-to-birth-dam rule; preserve submitted tag | Yes: submitted surrogate relationship | No for strings | Birth dam does not establish surrogate type | Per member vs independent animal | Endpoint-level blocker | No LIS `surrogateDam` field. |
| `[].animals[].sireEarTag` | string | Yes | Not explicit | **Related only:** LIS `Animal.registration.sire.identifier` | string when parent exists | Not declared nullable | No | Preserve submitted sire ear tag | Yes: submitted snapshot and membership | No for strings | Role plausible, provenance unproven | Per member vs independent animal | Endpoint-level blocker | LIS `importParents.sire` is also not a submitted snapshot. |
| `[].animals[].sireName` | string | Yes | Not explicit | **Related only:** LIS `Animal.registration.sire.name` | optional read-only string | Not declared nullable | No | Preserve submitted sire name | Yes: submitted snapshot and membership | No for strings | Display name may differ | Per member vs independent animal | Endpoint-level blocker | No proof the LIS name was the submitted name. |
| `[].animals[].errors[]` | array of `BundleAnimalErrorResponse` | No in DTO | Not explicit | N/A | N/A | N/A | N/A | Include locally stored per-animal errors | Yes | N/A | N/A | Zero or more per member | Endpoint-level blocker | LIS HTTP `Errors[]` is not a bundle member's errors. |
| `[].animals[].errors[].id` | GUID | No | Not explicit | N/A | N/A | N/A | N/A | Read local error ID | Yes | N/A | N/A | One per error | Endpoint-level blocker |  |
| `[].animals[].errors[].animalId` | GUID | No | Not explicit | N/A | N/A | N/A | N/A | Preserve local animal foreign key | Yes | N/A | N/A | One per error | Endpoint-level blocker |  |
| `[].animals[].errors[].errorCode` | string | No in DTO | Not explicit | N/A | N/A | N/A | N/A | Read local validation/processing code | Yes | N/A | N/A | One per error | Endpoint-level blocker |  |
| `[].animals[].errors[].errorText` | string | No in DTO | Not explicit | N/A | N/A | N/A | N/A | Read local validation/processing message | Yes | N/A | N/A | One per error | Endpoint-level blocker |  |
| `[].animals[].errors[].createdAt` | date-time (`DateTime?`) | Yes | Not explicit | N/A | N/A | N/A | N/A | Read local error timestamp | Yes | N/A | N/A | One per error | Endpoint-level blocker | Unlike bundle `createdAt`, nullable in the DTO. |

### `401 Unauthorized` — local `ProblemDetails`

A rejected or missing local API key yields `401` with explicitly set `type`, `title`, `status`, and `detail`; `instance` is not explicitly set in the authentication challenge. There is no LIS bundle-list `401`. Error arrays on unrelated LIS operations cannot establish a mapping for this local failure.

| Register an Animal field | Data type | Nullable | Required | LIS field | Data type 2 | Nullable 2 | Required 2 | Transformation | Missing fields | Datatype mismatch | Enum/value mismatch | Cardinality differences | Possible breaking change | Notes |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `type` | string | No on API-key rejection | Explicitly set | N/A | N/A | N/A | N/A | Generated by local authentication | Yes: operation | N/A | N/A | N/A | Endpoint-level blocker |  |
| `title` | string | No on API-key rejection | Explicitly set | N/A | N/A | N/A | N/A | Generated by local authentication | Yes: operation | N/A | N/A | N/A | Endpoint-level blocker |  |
| `status` | integer (`401`) | No | Explicitly set | N/A | N/A | N/A | N/A | Preserve local authorization failure | Yes: operation | N/A | N/A | N/A | Endpoint-level blocker | Not LIS request-status or animal-state status. |
| `detail` | string | No on API-key rejection | Explicitly set | N/A | N/A | N/A | N/A | Generated by local authentication | Yes: operation | N/A | N/A | N/A | Endpoint-level blocker |  |
| `instance` | string | Conditional | Not explicit | N/A | N/A | N/A | N/A | Not explicitly set by authentication challenge | Yes: operation | N/A | N/A | N/A | Endpoint-level blocker | Local exception handling may set it separately. |

### `400 Bad Request`, `404 Not Found`, `500 Internal Server Error`, `503 Service Unavailable`

Missing or invalid `x-cdp-request-id` yields local `400 ProblemDetails` with `code: missing_header`, `type`, `title`, `status`, `detail`, `instance`, and `traceId`. The exception handler also maps argument errors to `400`, `NotFoundException` to `404`, service unavailability to `503`, and other unhandled failures to `500`, adding `traceId` and `correlationId`. An empty bundle list is `200 []`; the local database query does not raise not-found on zero matches. These statuses and problem bodies have no LIS bundle-list equivalent.

## Breaking changes and decisions

| Finding | Status / impact | Decision needed |
|---|---|---|
| No LIS operation lists registration bundles by CPH. | **Endpoint-level blocker.** | Keep an authoritative local submission lookup or explicitly extend the LIS contract; do not substitute animal or request-status search. |
| Single `cph` segment is URI-unescaped and matched exactly against locally stored CPH. | **Route-specific format risk.** A hyphenated value routes, but may not match a slash-separated stored value. | Agree accepted CPH spellings and any canonicalization without losing existing bundle results; the three-part route is separately evaluated. |
| Local IDs, submitter, processing status, timestamps, bundle membership, and per-animal errors cannot be reconstructed from LIS animals. | **No direct response translation.** | Preserve local submission history and original submitted values. Treat individual-animal LIS fields as related only, not bundle mappings. |
| LIS async request-status lookup addresses one request ID, not all submissions for a holding. | **Identity and cardinality mismatch.** | Only associate it with bundles if a separately defined request-ID link exists; it cannot implement this list endpoint. |
| Empty result and local authorization/correlation failures are independent of LIS. | **Failure-contract mismatch.** | Preserve `200 []` and locally generated error handling; do not reinterpret a LIS `404` as no bundles. |

**Result:** This CPH route is a local submission-history lookup with **no LIS bundle-list equivalent**. It shares the three-part route's service and response mapping; its distinct issue is the single bound CPH string and exact stored-CPH matching.
