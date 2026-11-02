# GET /v1/users/{subject}

This is an evaluation and schema mapping from the Register an Animal API to the LIS API for the user details endpoint. Register an Animal's specification defines its request and response; the LIS API contract defines possible counterparts. The review follows object, endpoint, request, response, and error mappings.

**N/A** means no equivalent LIS field or operation exists; it does not mean the Register an Animal field is absent.

**Scope:** This document evaluates Register an Animal `GET /v1/users/{subject}` (get a user's details and associated CPHs). The LIS API has no equivalent operation, so no LIS endpoint or schema is mapped. Register an Animal `POST /v1/users` is referenced only because its description also returns user details and CPHs. Its response schema is not compared here.

## Overview

### Object Mapping

| Register an Animal API object | LIS API object |
|-------------------------------|----------------|
| UserDetailsResponse | N/A (no LIS object) |
| UserCphResponse | N/A (no LIS object) |
| ProblemDetails (single object) | Errors (array of ProblemDetails) |

### Endpoint Mapping

| Register an Animal API endpoint | LIS API endpoint |
|---------------------------------|------------------|
| GET /v1/users/{subject} | N/A (no LIS endpoint) |

### Endpoint Descriptions

* **GET /v1/users/{subject}** - Get a user's details and associated CPHs - Retrieves a user's details and the CPHs they are associated with from the keeper data service, by identity provider subject. The associations are those captured when the user last signed in.
* **LIS API** - No operation in `lis-api.json` returns user details or CPHs.

## Request Mapping

No LIS request exists for this operation. The only request input is the `subject` path parameter, which has no LIS source. Register an Animal sends no request body.

### Path parameter

| Register an Animal field | Data type | Nullable | Required | LIS field | LIS data type | LIS nullable | LIS required | Transformation | Missing fields | Datatype mismatch | Enum/value mismatch | Cardinality differences | Possible breaking change | Notes |
|--------------------------|-----------|----------|----------|-----------|-----------|----------|----------|----------------|----------------|-------------------|---------------------|------------------------|--------------------------|-------|
| subject (path) | string | No | Yes | N/A | — | — | — | no direct equivalent; no LIS source | Yes | N/A | N/A | N/A | Yes | Path parameter. No LIS field carries the identity provider subject. Breaking only if this endpoint is exposed to LIS clients. |

**Request notes**
* `subject` is a required path string with no format, length or pattern limits.
* Do not implement the request until it is decided whether LIS clients can call this endpoint.

**LIS read-only fields in the request schema**

None. LIS has no request schema for this operation.

**Register an Animal request fields without a confirmed LIS mapping**

* `subject`

## Response Mapping

There is no LIS response to map to, so every field in the `200` response is unmapped.

### Response codes compared

* Register an Animal `GET /v1/users/{subject}` declares `200`, `400`, `401`, `404` and `500`.
* LIS declares no codes for this operation, because no LIS operation exists.
* Across the LIS operations, the codes used are `200`, `202`, `204`, `400`, `403`, `404`, `500` and `503`. No LIS operation declares `401`.

### 200 OK (no LIS counterpart)

#### User details

| Register an Animal field | Data type | Nullable | Required | LIS field | LIS data type | LIS nullable | LIS required | Transformation | Missing fields | Datatype mismatch | Enum/value mismatch | Cardinality differences | Possible breaking change | Notes |
|--------------------------|-----------|----------|----------|-----------|-------------|------------|------------|----------------|----------------|-------------------|---------------------|------------------------|--------------------------|-------|
| subject | string | No | Not explicit | N/A | — | — | — | no direct equivalent; no LIS user object | Yes | N/A | N/A | N/A | No | Identity provider subject claim. Information is lost, but the message is still valid. |
| email | string | No | Not explicit | N/A | — | — | — | no direct equivalent; no LIS user object | Yes | N/A | N/A | N/A | No | Personal data. Confirm before any sharing with LIS clients. |
| firstName | string | Yes | Not explicit | N/A | — | — | — | no direct equivalent; no LIS user object | Yes | N/A | N/A | N/A | No | Personal data. |
| lastName | string | Yes | Not explicit | N/A | — | — | — | no direct equivalent; no LIS user object | Yes | N/A | N/A | N/A | No | Personal data. |
| displayName | string | Yes | Not explicit | N/A | — | — | — | no direct equivalent; no LIS user object | Yes | N/A | N/A | N/A | No | Personal data. |
| cphs | array of `UserCphResponse` | No | Not explicit | N/A | — | — | — | no direct equivalent; no LIS user object | Yes | N/A | N/A | Zero or more CPHs per user | No | Associations captured at the user's last sign-in. May be empty, as no `minItems` is set. |

#### CPH details (`cphs[]`)

| Register an Animal field | Data type | Nullable | Required | LIS field | LIS data type | LIS nullable | LIS required | Transformation | Missing fields | Datatype mismatch | Enum/value mismatch | Cardinality differences | Possible breaking change | Notes |
|--------------------------|-----------|----------|----------|-----------|-------------|------------|------------|----------------|----------------|-------------------|---------------------|------------------------|--------------------------|-------|
| cphs[].cph | string | No | Not explicit | N/A | — | — | — | no direct equivalent; no LIS user object | Yes | N/A | N/A | One per CPH | No | County, parish and holding (CPH) number. |
| cphs[].holdingId | string | Yes | Not explicit | N/A | — | — | — | no direct equivalent; no LIS user object | Yes | N/A | N/A | One per CPH | No | Keeper data identifier of the holding. |
| cphs[].holdingName | string | Yes | Not explicit | N/A | — | — | — | no direct equivalent; no LIS user object | Yes | N/A | N/A | One per CPH | No | Name of the holding, where the source carries one. |
| cphs[].role | string | No | Not explicit | N/A | — | — | — | no direct equivalent; no LIS user object | Yes | N/A | N/A | One per CPH | No | Role the user holds on the holding. |

**Response notes**
* Register an Animal does not mark any response field as required, so any field may be absent.
* `subject` and `email` are non-nullable strings. `firstName`, `lastName`, `displayName`, `cphs[].holdingId` and `cphs[].holdingName` are nullable.
* The CPH associations reflect the user's last sign-in, not current keeper data.
* Email and names are personal data. It is not clear whether they may be shared with LIS clients. Confirm before any mapping is agreed.

### Error responses

#### 400 Bad Request — ProblemDetails

Register an Animal returns `application/problem+json` with a `ProblemDetails` object for `400`. No LIS user operation exists, so there is no LIS error to map to. The LIS side is N/A.

| Register an Animal field | Data type | Nullable | Required | LIS field | LIS data type | LIS nullable | LIS required | Transformation | Missing fields | Datatype mismatch | Enum/value mismatch | Cardinality differences | Possible breaking change | Notes |
|--------------------------|-----------|----------|----------|-----------|-----------|----------|----------|----------------|----------------|-------------------|---------------------|------------------------|--------------------------|-------|
| type | string \| null | Yes | No | N/A | — | — | — | No LIS user error to translate | Yes | N/A | N/A | N/A | No | Problem type URI. Source may be null. |
| title | string \| null | Yes | No | N/A | — | — | — | No LIS user error to translate | Yes | N/A | N/A | N/A | No | Short summary. Source may be null. |
| status | integer (int32) \| string \| null | Yes | No | N/A | — | — | — | No LIS user error to translate | Yes | N/A | N/A | N/A | No | Source may be an integer, an integer-formatted string or null. |
| detail | string \| null | Yes | No | N/A | — | — | — | No LIS user error to translate | Yes | N/A | N/A | N/A | No | Explanation of the error. Source may be null. |
| instance | string \| null | Yes | No | N/A | — | — | — | No LIS user error to translate | Yes | N/A | N/A | N/A | No | Request path for local errors. |

**Error notes**
* Register an Animal returns one `ProblemDetails` object. LIS error responses on other operations use an `Errors` array with 1 to 1000 items, so the single object would need wrapping if this endpoint is exposed. That is a cardinality change.

#### 404 Not Found — ProblemDetails

Register an Animal returns the same `ProblemDetails` shape for `404`, so the table above applies. No LIS user operation exists to produce a `404`.

#### 500 Internal Server Error — ProblemDetails

Register an Animal returns the same `ProblemDetails` shape for `500`, so the table above applies. No LIS user operation exists to produce a `500`.

#### 401 Unauthorized — ProblemDetails

* Register an Animal declares `401` for a rejected `x-api-key`. No LIS operation declares `401`, so there is no LIS status to map to.
* Do not pass `401` through to LIS clients unchanged. Do not convert it to `403` automatically, because it may mean the facade's own key was rejected rather than the LIS client lacking permission. Handling will need to be agreed to.

### Response headers

* Register an Animal declares no response headers for this operation.
* LIS defines `LI-Request-Id` and `LI-Correlation-Id` headers, used on its error responses. Whether the facade returns them for this endpoint is unconfirmed.

### Authentication and headers

* Register an Animal uses the `x-api-key` header (ApiKey, service-to-service, interim until AWS STS). This applies only to the facade's call to Register an Animal.
* LIS requires the `Ocp-Apim-Subscription-Key` header and a bearer `Authorization` JWT on its operations. This is not mapped, because no LIS operation exists for this endpoint.
* LIS defines a correlation ID header parameter. It is not mapped.

## Assumptions

| Area | Assumption |
|------|------------|
| Endpoint | It is not clear whether LIS clients need this user lookup. This document assumes it is not exposed through LIS until confirmed. (unconfirmed) |
| `subject` | Request: no LIS field -> Register an Animal `subject` (path). No mapping exists. |
| Response fields | Register an Animal response fields have no LIS destination and remain unmapped. |
| Error body | No LIS error response is mapped for `400`, `404` or `500`, because no LIS user operation exists. If the endpoint is exposed, Register an Animal `ProblemDetails` -> LIS `Errors` array (mapping unconfirmed). |
| `401` | Not passed through to LIS clients, and not converted to `403` (unconfirmed). |

## Breaking-change summary

Missing fields with Possible breaking change set to No means information is lost because LIS has nowhere to hold it, but the message is still valid.

| Category | Potential breaking change |
|----------|---------------------------|
| Missing data | Register an Animal response fields `subject`, `email`, `firstName`, `lastName`, `displayName`, `cphs` and the `cphs[]` items have no LIS field. Information is lost, but the message is still valid. Possible breaking change: No. |
| Missing input | Register an Animal `subject` path parameter has no LIS source. If this endpoint is exposed, the facade cannot build a valid request without a source. Possible breaking change: Yes. |
| Error contract | Register an Animal returns one `ProblemDetails` object, while LIS uses an `Errors` array. This only matters if the endpoint is exposed to LIS clients, where returning the error unchanged would not match the LIS contract. Possible breaking change: Yes if exposed. |
| Datatype | Register an Animal `status` may be an integer, an integer-formatted string or null. LIS requires an integer from 100 to 599. Copying a string or null unchanged would not match. Possible breaking change: Yes. |
| Nullability | Register an Animal `type`, `title` and `detail` may be null. LIS requires non-null values. A null cannot fill a required LIS field. Possible breaking change: Yes. |
| Status codes | Register an Animal `401` has no LIS equivalent, and no LIS operation declares `401`. Passing it through unchanged would give an undeclared status. Possible breaking change: Yes. |
| Security | The Register an Animal `x-api-key` and the LIS subscription key and token are separate. This is handled at facade level and does not change the payload. Possible breaking change: No. |

## Recommendations and remaining gaps

| Area | Recommendation or remaining gap |
|------|---------------------------------|
| Endpoint scope | Confirm whether LIS clients need this endpoint. If not, the facade keeps it internal and this document records the outcome. The LIS API has no equivalent operation. |
| `subject` source | If the endpoint is exposed, confirm where the facade gets `subject` from (LIS request, access token claim or other). Do not assume the LIS access token subject equals the Register an Animal identity provider subject. |
| Personal data | Confirm whether email, first name, last name and display name may be sent to LIS clients. Register an Animal returns them. The LIS API has no agreed field for them. |
| CPH data freshness | Agree whether LIS clients can accept CPH associations from the user's last sign-in, as the Register an Animal description states. |
| Error body | If the endpoint is exposed, agree how the single Register an Animal `ProblemDetails` becomes an LIS `Errors` array (1 to 1000 items), and how null `type`, `title`, `detail` and `status` values are handled (unconfirmed). |
| `401` | Agree how the facade handles a Register an Animal `401`. Do not convert it automatically to LIS `403`. |
| `503` | Register an Animal declares no `503` for this endpoint, while LIS uses `503` on other operations. Agree whether the facade returns `503` when Register an Animal is unavailable. |
| `POST /v1/users` | Referenced only. Its description also returns user details and CPHs. Compare its response schema if LIS exposes user data. |

**Result:** Register an Animal returns a user's details and associated CPHs. The LIS API has no equivalent operation or response fields, so this endpoint cannot be mapped to LIS. Nothing is mapped until the endpoint scope decision in the Recommendations table is agreed.
