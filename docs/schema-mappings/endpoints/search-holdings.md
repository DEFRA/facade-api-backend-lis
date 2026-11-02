# GET /v1/holdings: API schema mapping

This is an evaluation and schema mapping from the Register an Animal API to the LIS API for the generic search holdings endpoint. Register an Animal's repository defines its request and response; the LIS API contract defines possible counterparts. The review follows object, endpoint, request, response, and breaking-change mappings.

**N/A** means no equivalent LIS holding field or operation exists; it does not mean the Register an Animal field is absent. Similarities with LIS animal models are mentioned only to explain why they **must not** be used as holdings mappings. Register an Animal obtains holding search results from the keeper-data API (KRDS); whether a future facade may keep that source is an open architectural decision.

## Object Mapping

| Register animal/cattle API object | LIS API object | Notes |
|---|---|---|
| `HoldingSearchRequest` (query parameters) | N/A | LIS has no holdings search; animal-search parameters do not identify a generic holding search. |
| `PaginatedResponse<HoldingResponse>` | N/A | LIS has no collection of holding summaries or matching pagination envelope. |
| `HoldingResponse` | N/A | LIS `site` is embedded within some animal details, not the LIS animal-list items; it is not a holding summary. |
| `ValidationProblem` / `ProblemDetails` | N/A for a holdings operation | LIS error arrays belong to other operations, not a LIS holdings-search contract. |

## Endpoint Mapping

| Register an animal/cattle API endpoint | LIS API endpoint | Equivalence / transformation |
|---|---|---|
| `GET /v1/holdings?search=&page=&pageSize=&sort=&order=` | **No equivalent** | Register an Animal searches holdings by optional text and returns a page of holdings. LIS has no operation that performs this search or returns holding summaries. |

Register an Animal calls KRDS `GET api/v2/holdings` with the supplied query parameters. All five query parameters are optional; no request body or holding path parameter is used. LIS `GET /animals` searches **animals** at a required `siteIdentifier` for a required `species`; it is not a fallback holdings search. Map Register an Animal cattle/animal operations to LIS animal operations in their own endpoint evaluations.

## Request Mapping

| Register an Animal field | Data type | LIS field | Data type 2 | Transformation | Missing fields | Datatype mismatch | Enum/value mismatch | Cardinality differences | Breaking change | Notes |
|---|---|---|---|---|---|---|---|---|---|---|
| `search` (optional query) | string; at most 200 characters; restricted character set | N/A | N/A | No LIS free-text holding-search operation to receive this term | Yes: LIS holding search | N/A | N/A | N/A | Endpoint-level blocker | Blank/whitespace term is not forwarded to KRDS; terms can be names, not necessarily CPHs. |
| `page` (optional query) | integer, at least 1 | N/A for holdings | N/A | Would need a holdings pagination rule on an equivalent endpoint | Yes: LIS holding search | N/A | N/A | N/A | Endpoint-level blocker | LIS *animal* search also names a `page` parameter, but it pages animals, not holdings. |
| `pageSize` (optional query) | integer, 1–100 | N/A for holdings | N/A | Would need a holdings page-size rule on an equivalent endpoint | Yes: LIS holding search | N/A | N/A | N/A | Endpoint-level blocker | The 1–100 constraint belongs to Register an Animal. |
| `sort` (optional query) | string, `asc` or `desc` (case-insensitive) | N/A for holdings | N/A | No equivalent holdings sort operation | Yes: LIS holding search | N/A | N/A | N/A | Endpoint-level blocker | Same words on animal search do not establish holdings ordering. |
| `order` (optional query) | string: `cph`, `identifier`, `name`, `holdingType`, `startDate`, `endDate` (case-insensitive) | N/A for holdings | N/A | No LIS holding sort keys to map | Yes: LIS holding search | N/A | N/A | N/A | Endpoint-level blocker | LIS animal `order` values (`animalIdentifier`, `state`) are not holding sort keys. |

**Request validation:** Register an Animal rejects `page < 1`, `pageSize` outside 1–100, invalid `sort`/`order`, and `search` longer than 200 characters or outside its allowed character set. The allowed search characters are Unicode letters/numbers, whitespace, and `/ ' @ . + ( ) , & -`. The code forwards nonblank `search`, `sort`, and `order`, and forwards `page`/`pageSize` when present. Absence of filters leaves the KRDS search behavior to KRDS; no LIS equivalent default search is defined.

**Headers (not body fields):** The versioned endpoint requires service-to-service API-key authentication and one nonblank `x-cdp-request-id` correlation header. LIS operations use separate subscription-key and access-token credentials; credential translation is not a request-field mapping.

## Response Mapping

### `200 OK` — `PaginatedResponse<HoldingResponse>`

The response contains a page of `items[]` (zero or more holding summaries) and pagination metadata. `Nullable` reflects the Register an Animal DTO types. `Required` is **Not explicit**: DTO initialization does not by itself establish JSON-schema requiredness. KRDS supplies counts and paging flags; the API renames KRDS `values` to `items` and maps each holding. An empty result is `200` with `items: []` and `count: 0`, not a missing-holding `404`. LIS does not return a holdings-search response. Its separate animal-search response also has `count` and `values`, but counts and lists animals; **none of its fields is a mapping for this endpoint**.

| Register an Animal field | Data type | Nullable | Required | LIS field | Data type 2 | Nullable 2 | Required 2 | Transformation | Missing fields | Datatype mismatch | Enum/value mismatch | Cardinality differences | Possible breaking change | Notes |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `count` | integer | No | Not explicit | N/A for holdings | N/A | N/A | N/A | Needs count of holdings on this page | Yes | N/A | N/A | Scalar | Endpoint-level blocker | Register an Animal forwards KRDS `count`; LIS animal `count` cannot count holdings. |
| `totalCount` | integer | No | Not explicit | N/A | N/A | N/A | N/A | Needs total matching holdings | Yes | N/A | N/A | Scalar | Endpoint-level blocker | From KRDS; LIS animal search does not expose total count either. |
| `page` | integer | No | Not explicit | N/A | N/A | N/A | N/A | Needs authoritative current holdings page | Yes | N/A | N/A | Scalar | Endpoint-level blocker | KRDS returns 1-based current page. |
| `pageSize` | integer | No | Not explicit | N/A | N/A | N/A | N/A | Needs applied holdings page size | Yes | N/A | N/A | Scalar | Endpoint-level blocker | KRDS returns actual page size. |
| `totalPages` | integer | No | Not explicit | N/A | N/A | N/A | N/A | Could calculate from known `totalCount`/`pageSize` if available | Yes | N/A | N/A | Scalar | Depends on source | LIS does not return the inputs required for reliable derivation. |
| `hasNextPage` | boolean | No | Not explicit | N/A | N/A | N/A | N/A | Needs total/pages or upstream next-page metadata | Yes | N/A | N/A | Scalar | Depends on source | From KRDS. |
| `hasPreviousPage` | boolean | No | Not explicit | N/A | N/A | N/A | N/A | Could derive from actual current page if known | Yes | N/A | N/A | Scalar | Depends on source | From KRDS; requested page is not proof of response page. |
| `items[]` | array of `HoldingResponse` | No | Not explicit | N/A for holdings | N/A | N/A | N/A | Requires holding summary items | Yes: LIS holding items | N/A | N/A | Zero or more holdings | Endpoint-level blocker | LIS animal `values` contains `AnimalSummary`, not holdings; renaming an array cannot change its content. |
| `items[].cph` | string | No | Not explicit | N/A in a holding search | N/A | N/A | N/A | Needs each holding's CPH | Yes | N/A | N/A | One per holding | Endpoint-level blocker | KRDS identifier is used; if missing, the service passes an empty string for a search item. |
| `items[].name` | string | Yes | Not explicit | N/A in a holding search | N/A | N/A | N/A | Needs each holding name | Yes | N/A | N/A | One per holding | Endpoint-level blocker | LIS animal list items do not include holding or site name. |
| `items[].holdingType` | string | Yes | Not explicit | N/A in a holding search | N/A | N/A | N/A | Needs each holding type | Yes | N/A | N/A | One per holding | Endpoint-level blocker | Animal site activity, even if separately retrieved, is not necessarily holding type. |
| `items[].startDate` | date-time (`DateTimeOffset`) | Yes | Not explicit | N/A | N/A | N/A | N/A | Requires holding start date | Yes | N/A | N/A | Per item | Depends on source | From KRDS. |
| `items[].endDate` | date-time (`DateTimeOffset`) | Yes | Not explicit | N/A | N/A | N/A | N/A | Requires holding end date | Yes | N/A | N/A | Per item | Depends on source | From KRDS. |
| `items[].active` | boolean | No | Not explicit | N/A | N/A | N/A | N/A | Derived from holding start/end dates and current UTC time | Yes: source dates | N/A | N/A | Per item | Depends on source | False if start is future or end is past; otherwise true. |
| `items[].address[]` | array of strings | No | Not explicit | N/A | N/A | N/A | N/A | Flatten ordered KRDS address components; no LIS holding address | Yes | N/A | N/A | Zero or more lines per item | Depends on source | Blank components omitted and remaining lines trimmed. |
| `items[].keeperName` | string | Yes | Not explicit | N/A | N/A | N/A | N/A | Select name of first association with `Keeper` role | Yes | N/A | N/A | Many associations to one name | Depends on source | No keeper association yields null. |

The search response does **not** contain the detail-only `herdMarks[]`, `allowedSpecies[]`, or `associations[]`. Those belong to the separate get-holding-details evaluation, not to `items[]` here.

### `401 Unauthorized` — `ProblemDetails`

Missing/invalid service-to-service API key produces `401` with a problem-details body. The API-key rejection sets `type`, `title`, `status`, and `detail`; `instance` is not explicitly set by that rejection path. No LIS holdings operation has an error response to map here. LIS animal search uses an unrelated `403 Forbidden` with `application/json` `Errors[]`; similarly named error fields do not establish a `403` → `401` mapping.

| Register an Animal field | Data type | Nullable | Required | LIS field | Data type 2 | Nullable 2 | Required 2 | Transformation | Missing fields | Datatype mismatch | Enum/value mismatch | Cardinality differences | Possible breaking change | Notes |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `type` | string | No on API-key rejection | Explicitly set | N/A for holdings | N/A | N/A | N/A | No LIS holdings error to translate | Yes: endpoint | N/A | N/A | N/A | Endpoint-level blocker | Local problem-type URI. |
| `title` | string | No on API-key rejection | Explicitly set | N/A for holdings | N/A | N/A | N/A | No LIS holdings error to translate | Yes: endpoint | N/A | N/A | N/A | Endpoint-level blocker | `Unauthorized` for API-key rejection. |
| `status` | integer | No on API-key rejection | Explicitly set | N/A for holdings | N/A | N/A | N/A | No LIS holdings error to translate | Yes: endpoint | N/A | N/A | N/A | Endpoint-level blocker | `401` cannot be inferred from unrelated LIS animal `403`. |
| `detail` | string | No on API-key rejection | Explicitly set | N/A for holdings | N/A | N/A | N/A | No LIS holdings error to translate | Yes: endpoint | N/A | N/A | N/A | Endpoint-level blocker | Explains invalid or missing API key. |
| `instance` | string | Conditional | Not explicit | N/A for holdings | N/A | N/A | N/A | May be set to this request path for local errors | Yes: endpoint | N/A | N/A | N/A | Endpoint-level blocker | Not explicitly set by API-key rejection. |

### `400 Bad Request` — `ValidationProblem` or `ProblemDetails`

Invalid `search`, `page`, `pageSize`, `sort`, or `order` returns a validation problem with standard problem fields **and an `errors` dictionary of field names to message arrays** (for example, `errors.PageSize`). A missing/nonblank-required `x-cdp-request-id` returns a `400 ProblemDetails` with `code: missing_header`. KRDS `400` also becomes a local `400 ProblemDetails`. The shared `type`, `title`, `status`, `detail`, and `instance` concepts are described in the `401` table; validation `errors` has **no demonstrated mapping** to LIS's `Errors[]` (an array of separate problem objects). These are different content shapes and failure causes.

### `500 Internal Server Error` / `503 Service Unavailable`

The exception handler may return `500` or `503` for failures. Neither is a successful empty search and neither is interchangeable with LIS animal-search errors. Their problem body can include `type`, `title`, `status`, `detail`, `instance`, `traceId`, and `correlationId`; there is no corresponding LIS holdings operation to map these statuses to.

## Breaking changes and decisions

| Finding | Status / impact | Decision needed |
|---|---|---|
| LIS has no generic holdings search operation or holding-summary result model. | **Confirmed blocker to LIS-only search translation**, not to the current KRDS-backed endpoint. | Confirm whether the facade may keep a holding-data source; otherwise request a LIS holdings-search contract. |
| Register an Animal accepts optional arbitrary `search` text; LIS animal search requires `siteIdentifier` and `species`. | **Input semantics and requiredness mismatch.** No general string-to-identifier transformation exists. | Define a real holdings search operation before mapping generic search terms. |
| Some pagination parameter names resemble LIS animal-search names, but the sorted and counted resources differ. | **No valid holdings field mapping** follows from matching names; animal `order` values are not holding sort keys. | Define pagination and ordering against a real holdings operation, not animal search. |
| Register an Animal returns a page envelope and holding summaries; LIS offers no holdings result. | **Missing holdings data and pagination metadata.** Renaming LIS animal `values` to `items` would not create holdings. | Retain an authoritative holding source or agree a consumer-facing behavior change. |
| Register an Animal `400` may include field-error dictionary; there is no LIS holdings error response. | **Unmapped error behavior**, distinct from unrelated LIS animal error formats. | Define error handling by cause once a holdings data source is agreed. |

**Result:** Register an Animal searches holdings through KRDS and returns paginated `HoldingResponse` items. The LIS API **cannot search holdings or construct the specified holding-search response**. LIS animal endpoints should instead be evaluated against Register an Animal cattle/animal endpoints, not this holdings search.
