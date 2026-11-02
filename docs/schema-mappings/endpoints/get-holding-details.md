# GET /v1/holdings/{county}/{parish}/{holding}: API schema mapping

This is an evaluation and schema mapping from the Register an Animal API to the LIS API for the get holding details endpoint. The Register an Animal API source code defines its request and response payloads; the LIS API contract defines the possible LIS counterpart. This review follows object, endpoint, request, response, and breaking-change mappings.

**N/A** means no corresponding LIS holding field or operation exists, not that the Register an Animal field is absent. LIS `site` is an *animal-related location model*, not a holding-details response. Register an Animal currently reads holding data from the keeper-data API (KRDS).

## Object Mapping

| Register animal/cattle API object | LIS API object | Notes |
|---|---|---|
| `HoldingDetailedResponse` (extends `HoldingResponse`) | **N/A** | LIS has no standalone holding-details response. The `site` fields in animal resources can describe different locations and cannot stand in for a holding. |
| `HoldingMarkResponse`, `HoldingAssociationResponse`, `HoldingRoleResponse` | **N/A** | Nested holding details have no equivalent LIS holding models. |
| `ProblemDetails` (errors) | **N/A for this endpoint** | Other LIS endpoints use `Errors[]` containing LIS `ProblemDetails`; compare shapes below, but no corresponding holding operation emits them. |

## Endpoint Mapping

| Register an animal/cattle API endpoint | LIS API endpoint | Equivalence / transformation |
|---|---|---|
| `GET /v1/holdings/{county}/{parish}/{holding}` | **No equivalent** | LIS has no operation for retrieving holding details by CPH. `GET /animals` returns animal summaries and `GET /animals/{identifier}` needs an animal identifier; neither returns a holding. |

The Register an Animal API also has a **separate** `GET /v1/holdings/` search endpoint, which returns paginated holdings; it is not a LIS endpoint or an equivalent of this single-holding lookup. The details endpoint calls KRDS `GET api/v2/holdings/{county}/{parish}/{holding}`.

## Request Mapping

There is **no request body**. All three path strings are required. The Register an Animal service validates `county` as two digits, `parish` as three digits, and `holding` as four digits before calling KRDS.

| Register an Animal field | Data type | LIS field | Data type 2 | Transformation | Missing fields | Datatype mismatch | Enum/value mismatch | Cardinality differences | Breaking change | Notes |
|---|---|---|---|---|---|---|---|---|---|---|
| `county` (path, required) | string, 2 digits | N/A | N/A | No LIS holding-details request to translate into | Yes: endpoint | N/A | N/A | N/A | Endpoint-level blocker | First CPH segment passed to KRDS. |
| `parish` (path, required) | string, 3 digits | N/A | N/A | No LIS holding-details request to translate into | Yes: endpoint | N/A | N/A | N/A | Endpoint-level blocker | Second CPH segment passed to KRDS. |
| `holding` (path, required) | string, 4 digits | N/A | N/A | No LIS holding-details request to translate into | Yes: endpoint | N/A | N/A | N/A | Endpoint-level blocker | Third CPH segment passed to KRDS. |

**Headers (not body fields):** The endpoint requires service-to-service API-key authentication and one nonblank `x-cdp-request-id` correlation header. LIS has different subscription-key and access-token credentials on its operations, but no corresponding holding operation. A slash-separated LIS `siteIdentifier` used to search *animals* is not evidence that the CPH path segments can be translated into a LIS holding lookup.

## Response Mapping

### `200 OK` — `HoldingDetailedResponse`; no equivalent LIS holding response

The service returns **one** holding. `Nullable` below reflects the declared DTO type, not a promise that a field is always populated. `Required` is "Not explicit" because DTO declarations do not establish a JSON-schema `required` rule. Collections default to empty lists. There is no LIS holding-details response to compare field by field. Similar LIS `site` property names describe locations on animals, not an available holding response, and are **not mapped** below. Repeated "Yes if LIS-only" values refer to the **same conditional endpoint-level blocker**, not separate confirmed breaking changes; the existing KRDS-backed route supplies these fields.

| Register an Animal field | Data type | Nullable | Required | LIS field | Data type 2 | Nullable 2 | Required 2 | Transformation | Missing fields | Datatype mismatch | Enum/value mismatch | Cardinality differences | Possible breaking change | Notes |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `cph` | string | No | Not explicit | N/A for holdings | N/A | N/A | N/A | Needs a holding identifier from a holding source | Yes | N/A | N/A | One per holding | Endpoint-level blocker | KRDS identifier, or slash-joined request CPH if blank. LIS animal-site identifiers cannot establish a holding response. |
| `name` | string | Yes | Not explicit | N/A for holdings | N/A | N/A | N/A | Needs holding name from a holding source | Yes | N/A | N/A | One per holding | Endpoint-level blocker | LIS animal `site.name` is not established as the holding name. |
| `holdingType` | string | Yes | Not explicit | N/A for holdings | N/A | N/A | N/A | Needs holding type from a holding source | Yes | N/A | N/A | One per holding | Endpoint-level blocker | LIS animal-site activity type is not necessarily KRDS holding type. |
| `startDate` | date-time (`DateTimeOffset`) | Yes | Not explicit | N/A | N/A | N/A | N/A | Needs holding start date | Yes | N/A | N/A | N/A | Yes if LIS-only | From KRDS. |
| `endDate` | date-time (`DateTimeOffset`) | Yes | Not explicit | N/A | N/A | N/A | N/A | Needs holding end date | Yes | N/A | N/A | N/A | Yes if LIS-only | From KRDS. |
| `active` | boolean | No | Not explicit | N/A | N/A | N/A | N/A | Derived from holding dates | Yes | N/A | N/A | N/A | Yes if LIS-only | False if start is in the future or end is in the past, otherwise true (UTC). |
| `address[]` | array of strings | No | Not explicit | N/A | N/A | N/A | N/A | Needs holding address | Yes | N/A | N/A | Zero or more lines | Yes if LIS-only | Nonblank KRDS address lines are trimmed and ordered. |
| `keeperName` | string | Yes | Not explicit | N/A | N/A | N/A | N/A | Needs keeper association | Yes | N/A | N/A | First matching association | Yes if LIS-only | Name of the first KRDS association with a `Keeper` role. |
| `herdMarks[]` | array of `HoldingMarkResponse` | No | Not explicit | N/A | N/A | N/A | N/A | Needs holding marks | Yes | N/A | N/A | Zero or more objects | Yes if LIS-only | Includes marks with no end date and a nonblank identifier. |
| `herdMarks[].mark` | string | Yes | Not explicit | N/A | N/A | N/A | N/A | Needs mark identifier | Yes | N/A | N/A | Per mark | Yes if LIS-only | Included marks are nonblank; DTO type is nullable. |
| `herdMarks[].startDate` | date-time (`DateTimeOffset`) | Yes | Not explicit | N/A | N/A | N/A | N/A | Needs mark start date | Yes | N/A | N/A | Per mark | Yes if LIS-only | From KRDS. |
| `herdMarks[].endDate` | date-time (`DateTimeOffset`) | Yes | Not explicit | N/A | N/A | N/A | N/A | Needs mark end date | Yes | N/A | N/A | Per mark | Yes if LIS-only | Current filter includes only marks with null end date. |
| `herdMarks[].species[]` | array of strings | No | Not explicit | N/A | N/A | N/A | N/A | Needs mark species | Yes | N/A | N/A | Zero or more per mark | Yes if LIS-only | Distinct from holding-wide allowed species. |
| `allowedSpecies[]` | array of strings | No | Not explicit | N/A | N/A | N/A | N/A | Needs holding allowance | Yes | N/A | N/A | Zero or more | Yes if LIS-only | Blank values removed; LIS animal species does not indicate a holding's allowance. |
| `associations[]` | array of `HoldingAssociationResponse` | No | Not explicit | N/A | N/A | N/A | N/A | Needs holding associations | Yes | N/A | N/A | Zero or more objects | Yes if LIS-only | KRDS associations mapped to nested objects. |
| `associations[].customerNumber` | string | Yes | Not explicit | N/A | N/A | N/A | N/A | Needs party number | Yes | N/A | N/A | Per association | Yes if LIS-only |  |
| `associations[].title` | string | Yes | Not explicit | N/A | N/A | N/A | N/A | Needs party title | Yes | N/A | N/A | Per association | Yes if LIS-only |  |
| `associations[].firstName` | string | Yes | Not explicit | N/A | N/A | N/A | N/A | Needs party given name | Yes | N/A | N/A | Per association | Yes if LIS-only |  |
| `associations[].lastName` | string | Yes | Not explicit | N/A | N/A | N/A | N/A | Needs party family name | Yes | N/A | N/A | Per association | Yes if LIS-only |  |
| `associations[].name` | string | Yes | Not explicit | N/A | N/A | N/A | N/A | Needs party name | Yes | N/A | N/A | Per association | Yes if LIS-only |  |
| `associations[].partyType` | string | Yes | Not explicit | N/A | N/A | N/A | N/A | Needs party type | Yes | N/A | Unconfirmed | Per association | Yes if LIS-only |  |
| `associations[].email` | string | Yes | Not explicit | N/A | N/A | N/A | N/A | Needs party email | Yes | N/A | N/A | Per association | Yes if LIS-only |  |
| `associations[].mobile` | string | Yes | Not explicit | N/A | N/A | N/A | N/A | Needs party mobile | Yes | N/A | N/A | Per association | Yes if LIS-only |  |
| `associations[].telephone` | string | Yes | Not explicit | N/A | N/A | N/A | N/A | Needs party telephone | Yes | N/A | N/A | Per association | Yes if LIS-only |  |
| `associations[].address[]` | array of strings | No | Not explicit | N/A | N/A | N/A | N/A | Needs party address | Yes | N/A | N/A | Zero or more per association | Yes if LIS-only | Nonblank KRDS address parts are trimmed. |
| `associations[].roles[]` | array of `HoldingRoleResponse` | No | Not explicit | N/A | N/A | N/A | N/A | Needs party roles | Yes | N/A | N/A | Zero or more per association | Yes if LIS-only |  |
| `associations[].roles[].code` | string | Yes | Not explicit | N/A | N/A | N/A | N/A | Needs role code | Yes | N/A | N/A | Per role | Yes if LIS-only |  |
| `associations[].roles[].species[]` | array of strings | No | Not explicit | N/A | N/A | N/A | N/A | Needs role species | Yes | N/A | N/A | Zero or more per role | Yes if LIS-only | Distinct from mark species and holding-wide allowed species. |

### `401 Unauthorized` — `ProblemDetails`

Invalid or missing service-to-service credentials cause a `401` problem response. The API-key rejection explicitly sets `type`, `title`, `status`, and `detail`; `instance` depends on the problem-details writer. The same general `ProblemDetails` shape applies to other errors, with possible `traceId`, `correlationId`, or `code` extensions. There is **no LIS holding-details error response** to map. LIS animal operations document `403 Forbidden` with `Errors[]`, but similarly named fields are not a translation for this endpoint.

| Register an Animal field | Data type | Nullable | Required | LIS field | Data type 2 | Nullable 2 | Required 2 | Transformation | Missing fields | Datatype mismatch | Enum/value mismatch | Cardinality differences | Possible breaking change | Notes |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `type` | string | No for API-key rejection | Explicitly set for rejection | N/A for holdings | N/A | N/A | N/A | No LIS holding error to translate | Yes: endpoint | N/A | N/A | N/A | Endpoint-level blocker | Problem-type URI set locally. |
| `title` | string | No for API-key rejection | Explicitly set for rejection | N/A for holdings | N/A | N/A | N/A | No LIS holding error to translate | Yes: endpoint | N/A | N/A | N/A | Endpoint-level blocker | `Unauthorized` for API-key rejection. |
| `status` | integer | No for API-key rejection | Explicitly set for rejection | N/A for holdings | N/A | N/A | N/A | No LIS holding error to translate | Yes: endpoint | N/A | N/A | N/A | Endpoint-level blocker | `401` cannot be inferred from LIS animal `403`. |
| `detail` | string | No for API-key rejection | Explicitly set for rejection | N/A for holdings | N/A | N/A | N/A | No LIS holding error to translate | Yes: endpoint | N/A | N/A | N/A | Endpoint-level blocker | Describes the local API-key requirement. |
| `instance` | string | Conditional | Not explicit | N/A for holdings | N/A | N/A | N/A | No LIS holding error to translate | Yes: endpoint | N/A | N/A | N/A | Endpoint-level blocker | Exception handler sets request path; API-key rejection does not explicitly set it. |

### `400 Bad Request` — `ProblemDetails`

The same five `ProblemDetails` fields listed above are relevant. Malformed CPH segments are rejected locally, and KRDS may reject an invalid CPH; the exception handler sets `type`, `title`, `status`, `detail`, and `instance` and adds trace/correlation identifiers. Missing or invalid `x-cdp-request-id` also returns `400`, with a `code: missing_header` extension. There is no LIS holding `400` to map this response to.

### `404 Not Found` — `ProblemDetails`

The same five `ProblemDetails` fields apply. A KRDS holding not found becomes `404`, with the exception handler setting `type`, `title`, `status`, `detail`, and `instance`. LIS `GET /animals` also documents `404`, but **an animal search not finding animals does not establish that a holding is absent**. No LIS holding `404` exists.

The Register an Animal exception handler can also return `500` or `503` for failures. LIS animal operations document `403`, `404`, `500`, and `503` as `Errors[]` with 1–1000 LIS `ProblemDetails` items; LIS `type`, `status`, `title`, and `detail` are required in that schema. The error body shape, content type, and meaning differ. Similar field names do **not** establish an error mapping for this holding endpoint.

## Breaking changes and decisions

| Finding | Status / impact | Decision needed |
|---|---|---|
| LIS API lacks a holding-details endpoint or standalone holding response. | **Confirmed blocker for LIS-only translation**, not for the existing KRDS-backed route. | Confirm whether the facade should continue using KRDS; if LIS-only is required, obtain a holding-data operation. |
| LIS `site` is embedded in animal resources and does not provide a holding address, keeper, marks, allowed species, dates, or associations. | **Missing attributes and availability/cardinality differences** for LIS-only translation. | Keep an authoritative holding-data source or agree a consumer-facing behavior change. |
| LIS animal-site identifiers, names, and activity types are not a holding-details response. | **No justified string joining, nested flattening, or array-selection mapping** for this endpoint. | Evaluate LIS animal/site fields against relevant animal operations instead; do not invent a holding lookup. |
| Register an Animal uses a `ProblemDetails` object for holding failures; LIS has no corresponding holding operation or errors. | **Unmapped error behavior.** Unrelated LIS animal errors do not establish holding error translation. | Define error handling only when an actual holding data source is identified. |

**Result:** Register an Animal serves this endpoint using KRDS. It has no equivalent LIS API operation, so a field-by-field LIS request/response translation cannot currently be completed. The tables retain the full Register an Animal payload and identify the missing LIS operation and attributes without presenting animal or site models as substitute holdings.
