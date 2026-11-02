# GET /v1/holdings/{cph}/cattle: API schema mapping

This evaluates the **single-CPH path route** in the Register an Animal repository against LIS `GET /animals`. It is a separate endpoint from `GET /v1/holdings/{county}/{parish}/{holding}/cattle`, although both call the same cattle-list helper and return the same DTO. The LIS OpenAPI specification is the source for LIS fields. **Candidate** means a possible mapping needing confirmation; **N/A** means the LIS list contract provides no corresponding field. Neither means an unverified translation is safe to implement.

## Object Mapping

| Register animal/cattle API object | LIS API object | Notes |
|---|---|---|
| `cph` path parameter and `CattleFilter` query parameters | `GET /animals` query parameters | A CPH-to-`siteIdentifier` mapping is plausible but not established. LIS also requires `species` and has no ear-tag, breed, or sex query filters. |
| `CattleResponse[]` | LIS `200` response with `count` and `values` containing `AnimalSummary` items | Different envelope/paging and insufficient LIS list fields for complete translation. |
| `CattleErrorResponse` for each returned animal | N/A | LIS HTTP `Errors[]` describes failures of the whole request, not local submission errors per animal. |
| Endpoint `ProblemDetails` | LIS `Errors[]` on documented failure statuses | Similar problem-detail field names do not establish equivalent causes, status codes, or body shapes. |

## Endpoint Mapping

| Register an animal/cattle API endpoint | LIS API endpoint | Equivalence / transformation |
|---|---|---|
| `GET /v1/holdings/{cph}/cattle?earTag=&breed=&sex=` | **Related, not equivalent:** `GET /animals?species=cattle&siteIdentifier=...` | Both concern animals at a location. Register an Animal returns an unpaged array built from CADS, an optional CADS data patch, and eligible local submission animals. LIS returns a paged animal-summary response and does not document the local merge or all output fields. |

The route passes its bound `cph` to the shared helper, which applies `Uri.UnescapeDataString` and calls `GetCattleForHoldingAsync`. Unlike the three-part route, it **does not join `county/parish/holding`**. The router has a separate three-part route for slash-separated CPHs; the one-part route is tested with a hyphen-separated example. Neither route proves that a hyphen-separated CPH is the same site identifier in LIS, or that upstream/local data normalizes that spelling. The service uses the supplied CPH in the CADS request and compares it to local submissions' stored CPH. LIS paths are relative to the LIS server's `/v1` prefix.

## Request Mapping

There is **no request body**. `cph` is a required path string, with no route-specific CPH format validator. All three filter strings are optional; blank filters are ignored. Filters apply **after** CADS results have been merged with local submitted/error records. Ear-tag matching strips spaces, ignores case, and matches substrings; breed matches code, name, or stored breed by case-insensitive equality; sex matches by case-insensitive equality after trimming.

| Register an Animal field | Data type | LIS field | Data type 2 | Transformation | Missing fields | Datatype mismatch | Enum/value mismatch | Cardinality differences | Breaking change | Notes |
|---|---|---|---|---|---|---|---|---|---|---|
| `cph` (required path) | string | **Candidate:** `siteIdentifier` | required string query | Decode the path value, then pass as site identifier **only after** confirming LIS accepts its actual spelling/format and both sides mean the same location | Equivalence/accepted format unconfirmed | Path vs query placement | CPH and LIS site identity unverified | One path value to one query value | Possible | Do not automatically rewrite hyphens or infer three segments; doing so could also change local-record matching. |
| No Register an Animal query field | — | `species` | required string enum, includes `cattle` | Supply `cattle` under an agreed cattle-only facade policy | Additional required LIS input | N/A | Needs explicit rule | Extra input | Possible | Species is not supplied in this route's request. |
| `earTag` (optional query) | string; space-insensitive substring | N/A on LIS list request | N/A | Potential local post-filter over **all** fetched animals, with equivalent matching | Yes: LIS filter | No for identifiers | Identity/normalization unverified | All pages must be considered | Possible | LIS `animal.identifier` is an output field, not a list query filter. |
| `breed` (optional query) | string; equality against code/name/breed | N/A on LIS list request | N/A | Potential post-filter against mapped breed fields, including any locally merged records | Yes: LIS filter | Nested LIS breed object | Code/name semantics unverified | All pages must be considered | Possible | Local records may carry a breed string without code/name. |
| `sex` (optional query) | string; case-insensitive equality | N/A on LIS list request | N/A | Potential post-filter after collecting all pages and merging local records | Yes: LIS filter | No for strings | LIS output is an enum; Register an Animal accepts arbitrary filter strings | All pages must be considered | Possible | Invalid nonblank sex does not have a dedicated validator on this route. |
| No Register an Animal paging input | — | `page`, `pageSize` | optional integers | An unpaged array would require reading all relevant LIS pages using a confirmed completion rule | Pagination strategy absent | N/A | N/A | Paged upstream to unpaged output | Possible | LIS `order` and `sort` also exist but have no input counterparts here; ordering after merging must be defined if significant. |

**Headers (not body fields):** Versioned Register an Animal endpoints require service-to-service API-key authorization and one nonblank `x-cdp-request-id`. LIS uses subscription-key and access-token security; its success response documents `LI-Request-Id` and `LI-Correlation-Id`. Credential and correlation policies are not field renames.

## Response Mapping

### `200 OK` — array of `CattleResponse`

The helper returns a single unpaged JSON array, possibly empty. CADS is read page by page (up to a service cap); an optional data patch can supplement it. Local records are included when their submission is submitted or they have errors. Matching ear tags receive local `status` and `errors`; local-only records are appended. LIS returns an object with `count` and `values` holding `AnimalSummary` items. Its schema declares `values.items` but omits `values.type: array`, so **confirm the collection shape**. LIS `AnimalSummary.animal` and `breed` are not marked required at their top level. `Nullable` for Register an Animal describes DTO types, and `Required` is not explicitly established just by DTO initialization.

| Register an Animal field | Data type | Nullable | Required | LIS field | Data type 2 | Nullable 2 | Required 2 | Transformation | Missing fields | Datatype mismatch | Enum/value mismatch | Cardinality differences | Possible breaking change | Notes |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| response `[]` | array of `CattleResponse` | No if returned | Not explicit | `values` within LIS `200` object | `AnimalSummary` items; array type omitted in OAS | Not explicit | Not explicit | Confirm list shape, gather all pages, map items, and merge locally sourced animals if required | LIS alone has no local-only animals | Object vs array | N/A | Paged LIS to unpaged result | **Yes for direct translation** | `count` cannot be assumed to include locally merged or subsequently filtered animals. |
| `[].earTag` | string | No in DTO | Not explicit | `values[].animal.identifier` | string | Not declared nullable | Required if `animal` exists | Flatten and rename only after confirming identifier semantics | Conditional | No for strings | Ear-tag identity/format unconfirmed | One per animal | Possible | CADS can default a missing identifier to empty string. |
| `[].dateBirth` | date (`DateOnly`) | Yes | Not explicit | N/A in `AnimalSummary` | N/A | N/A | N/A | Cannot derive from LIS list response | Yes | N/A | N/A | Per animal | **Yes if needed** | A separate LIS single-animal detail request includes a birth date; enrichment is an unproven separate design. |
| `[].dateOnCph` | date (`DateOnly`) | Yes | Not explicit | N/A in `AnimalSummary` | N/A | N/A | N/A | Cannot derive holding-arrival date from this list | Yes | N/A | N/A | Per animal | **Yes if needed** | Do not use a registration or creation date as a substitute. |
| `[].sex` | string | Yes | Not explicit | `values[].animal.sex` | enum `female`/`male` string | Not declared nullable | Required if `animal` exists | Flatten; decide casing and handling of null/non-enum values | Conditional | No for present string | Free string versus restricted LIS enum | Per animal | Possible | CADS can use capitalized values. |
| `[].breed` | string | Yes | Not explicit | **Candidate:** `values[].breed.name` | optional read-only string | Not declared nullable | No | Flatten only after confirming breed-name semantics for CADS and local records | Conditional | Nested object to string | Local breed can denote code or name | Per animal | Possible | CADS sets `breed` from breed name. |
| `[].breedCode` | string | Yes | Not explicit | `values[].breed.code` | optional string | Not declared nullable | No | Flatten if present; code systems need confirmation | Conditional | Nested object to string | Code system unverified | Per animal | Possible | Local-only records do not populate this field. |
| `[].breedName` | string | Yes | Not explicit | `values[].breed.name` | optional read-only string | Not declared nullable | No | Flatten if present; preserve null otherwise | Conditional | Nested object to string | Naming/casing unverified | Per animal | Possible | Local-only records do not populate this field. |
| `[].status` | string | No in DTO | Not explicit | **Not equivalent:** `values[].animal.state` | optional enum: `registered`, `dead`, `lost`, `stolen` | Not declared nullable | No | No safe translation without business rule and authoritative status source | Yes: CADS/local submission status | String versus enum | Status is not LIS life-cycle state | Per animal | **Yes for direct translation** | CADS examples include `Alive` and `Passport Produced`; local processing status can replace CADS status. |
| `[].errors[]` | array of `CattleErrorResponse` | No in DTO | Not explicit | N/A | N/A | N/A | N/A | Requires local processing errors | Yes | N/A | N/A | Zero or more per animal | **Yes for direct translation** | LIS HTTP `Errors[]` concerns an entire failed request. |
| `[].errors[].errorCode` | string | No in DTO | Not explicit | N/A | N/A | N/A | N/A | Retrieve from local submission error | Yes | N/A | N/A | One per error | **Yes for direct translation** | No LIS list equivalent. |
| `[].errors[].errorText` | string | No in DTO | Not explicit | N/A | N/A | N/A | N/A | Retrieve from local submission error | Yes | N/A | N/A | One per error | **Yes for direct translation** | No LIS list equivalent. |

**Other LIS fields:** `animal.species` could check cattle-only assumptions; `animal.name`, `animal.state`, and `breed.crossBreed` have no directly equivalent field in the Register an Animal cattle-list response. LIS `count` has no direct output field and is not necessarily the final item count.

### `401 Unauthorized` — Register an Animal `ProblemDetails`

A rejected local API key yields `401` with explicitly set `type`, `title`, `status`, and `detail`; `instance` is not explicitly set in that rejection path. LIS animal list documents **`403`**, not `401`, using an `application/json` **array** of 1–1000 `ProblemDetails`. Field-level resemblance below is not authorization-status equivalence.

| Register an Animal field | Data type | Nullable | Required | LIS field | Data type 2 | Nullable 2 | Required 2 | Transformation | Missing fields | Datatype mismatch | Enum/value mismatch | Cardinality differences | Possible breaking change | Notes |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `type` | string | No on API-key rejection | Explicitly set | `Errors[].type` (schema comparison only) | URI string | No | Yes per error | Only map for a corresponding downstream failure | No name gap | LIS URI constraints | N/A | Array to object | Possible | Locally generated `401` is not a LIS error. |
| `title` | string | No on API-key rejection | Explicitly set | `Errors[].title` (comparison only) | string | No | Yes per error | Do not relabel cause as local authentication failure | No name gap | No | `Forbidden` and `Unauthorized` differ | Array to object | Possible |  |
| `status` | integer | No on API-key rejection | Explicitly set | `Errors[].status` (comparison only) | integer, 100–599 | No | Yes per error | Choose status by cause; do not turn all LIS `403`s into `401`s | No name gap | No | `403` vs `401` | Array to object | Possible |  |
| `detail` | string | No on API-key rejection | Explicitly set | `Errors[].detail` (comparison only) | string | No | Yes per error | Define safe selection of LIS detail for a true downstream error | No name gap | No | Cause may differ | Array to object | Possible |  |
| `instance` | string | Conditional | Not explicit | N/A | N/A | N/A | N/A | Use current request path if generated locally | Yes | N/A | N/A | Array to object | Possible | Set by the local exception handler for mapped exceptions. |

### `404 Not Found` — Register an Animal `ProblemDetails`

CADS holding-not-found becomes a local `404` problem with `type`, `title`, `status`, `detail`, and `instance`. LIS `GET /animals` documents `404` as `Errors[]`, but does not state whether it denotes an unknown site or something else. Do **not** convert an empty successful list into `404` without a confirmed rule. LIS errors require an array-to-object conversion only if the underlying failure means the same thing.

### `400 Bad Request`, `500 Internal Server Error`, `503 Service Unavailable`

A missing/nonblank-required `x-cdp-request-id` produces local `400 ProblemDetails` with `code: missing_header`; other request failures can produce `400`. Local exception handling can return `500` or `503` with `traceId` and `correlationId` extensions. LIS animal list documents `500` and `503` as `application/json` `Errors[]`, and does **not** document `400` for `GET /animals`. Do not ascribe local authentication/correlation errors to LIS or treat their response formats as identical.

## Breaking changes and decisions

| Finding | Status / impact | Decision needed |
|---|---|---|
| One bound `cph` string is unescaped, not joined from three path fields; the service uses that value for both CADS and exact stored-CPH matching. | **Route-specific format risk.** A hyphenated value proven to route is not proven to identify the same LIS site or match stored local submissions. | Confirm accepted CPH spellings, URI encoding and LIS `siteIdentifier` identity; specify any canonicalization without changing existing local-record inclusion. |
| LIS requires `species` and does not offer list filters for ear tag, breed, or sex. | **Additional input and missing filters.** | Agree cattle-only species selection and complete-page local post-filtering if LIS supplies the list. |
| LIS response is paged summaries inside an object, not one array incorporating submitted/error animals. | **Shape, paging, and source gap.** | Define page completion, ordering, local enrichment/deduplication, and count behavior. |
| Birth date, date on CPH, CADS/submission status, and per-animal processing errors cannot be sourced from LIS `AnimalSummary`. | **Direct translation blocked for complete output.** | Identify authoritative supplementary data or explicitly change the output contract; do not fabricate values. |
| LIS sex and breed values/nesting differ; LIS `state` is not Cattle `status`. | **Conditional field mappings and semantic mismatch.** | Validate identifier, code/name, casing, null handling, and status meaning. |
| Local `401` and problem objects differ from LIS `403` and error arrays; `404` meanings are not established. | **Error/status mismatch.** | Specify status-specific failure mappings and keep local errors separate from LIS errors. |

**Result:** LIS `GET /animals` is a relevant animal-list comparison for this endpoint, **not an equivalent complete implementation**. This route differs from the three-part cattle route only in how it receives its CPH; its service, filters, response fields, and unresolved LIS compatibility gaps are shared.
