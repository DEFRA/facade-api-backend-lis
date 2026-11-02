# POST /v1/users

This is an evaluation and schema mapping from the Register an Animal API to the LIS API for the create or refresh user account endpoint. This document is the mapping reference for implementing the facade translation layer and identifying any required contract changes. The tables show the corresponding fields, transformations, compatibility differences, and remaining mapping gaps for these endpoints.

**N/A** means no equivalent LIS field or operation exists; it does not mean the Register an Animal field is absent.

**Scope:** This document evaluates Register an Animal `POST /v1/users` (create or refresh a user account). The LIS API has no equivalent operation, so no LIS endpoint or schema is mapped. Register an Animal `GET /v1/users/{subject}` returns the same `UserDetailsResponse` and is covered in the separate `GET /v1/users/{subject}` mapping document (`get-user-details.md`). It is mentioned here only where it affects this mapping.

## Overview

### Object Mapping

| Register an Animal API object | LIS API object |
|-------------------------------|----------------|
| CreateUserRequest | N/A (no LIS object) |
| UserDetailsResponse | N/A (no LIS object) |
| UserCphResponse (the items in `cphs[]`) | N/A (no LIS object) |
| ProblemDetails (single object) | Errors (array of ProblemDetails) |

### Endpoint Mapping

| Register an Animal API endpoint | LIS API endpoint |
|---------------------------------|------------------|
| POST /v1/users | N/A (no LIS endpoint) |

### Endpoint Descriptions

* **POST /v1/users** - Create or refresh a user account - Ensures a user account exists in the keeper data service for the supplied identity provider claims (`sub`, `email`, `given_name`, `family_name`) and returns the user's details and associated CPHs. An existing account (matched by subject, or by an email with no subject yet) is refreshed; otherwise one is created. Returns 409 when the email belongs to a different account and 503 when the keeper data service is temporarily unavailable.

## Request Mapping

No LIS request exists for this operation, so no field has a LIS source. Register an Animal accepts an optional JSON body (`CreateUserRequest`) and no parameters. The spec allows the body to be `null`. When it is supplied, none of its fields is required and each can be a string or `null`.

### Request body (CreateUserRequest)

| Register an Animal field | Data type | Nullable | Required | LIS field | Data type | Nullable | Required | Transformation | Missing fields | Datatype mismatch | Enum/value mismatch | Cardinality differences | Possible breaking change | Notes |
|--------------------------|-----------|----------|----------|-----------|-----------|----------|----------|----------------|----------------|-------------------|---------------------|-------------------------|--------------------------|-------|
| sub | string | Yes | No | N/A | — | — | — | no direct equivalent; no LIS source | Yes | N/A | N/A | N/A | Yes | The identity provider subject claim. The schema allows a body without it, but whether the service accepts such a call is unconfirmed, and it is not clear that an account can be created or refreshed without `sub` or `email`. It could come from a LIS token, which is unconfirmed (see the request notes). |
| email | string | Yes | No | N/A | — | — | — | no direct equivalent; no LIS source | Yes | N/A | N/A | N/A | Yes | The email address claim. Personal data. No LIS request field holds a user email. LIS hauliers have an `email` field, but it describes a haulier, not a user. Same account-matching question as `sub`. |
| given_name | string | Yes | No | N/A | — | — | — | no direct equivalent; no LIS source | Yes | N/A | N/A | N/A | No | The given name claim. Personal data. No LIS request field holds it. Optional, so the facade can still send a valid message without it. |
| family_name | string | Yes | No | N/A | — | — | — | no direct equivalent; no LIS source | Yes | N/A | N/A | N/A | No | The family name claim. Personal data. No LIS request field holds it. Optional, so the facade can still send a valid message without it. |

**Request notes:**
* The fields sub, email, given_name, family_name are supplied identity provider claims according to the endpoint description. However, the Register an Animal JSON doesn't state more information on this. It is understood that the identity provider is Defra ID (unconfirmed).
* At the schema level, the body can be omitted or be `null`, no claim (`sub`, `email`, `given_name`, `family_name`) is required, and each field can be `null`. This does not establish whether the service accepts a call without claims or what it does in that case; runtime validation needs confirmation. LIS has no field for any of them.
* Register an Animal renames claims between its request and response: `sub` -> `subject`, `given_name` -> `firstName` and `family_name` -> `lastName` (`email` is unchanged). The `sub` and `subject` descriptions are identical; the two name pairings are assumed from their meaning. These are not LIS mappings.
* The request schema marks `sub` and `email` as optional, but the endpoint description says an account is matched by `sub`, or by `email` if no `sub` is stored yet. If neither is sent, it is not clear which account would be matched or whether a new one would be created. The facade may need to make sure at least one is sent.
* Do not implement this request until it is decided whether LIS API clients, or the facade itself, will call this endpoint. If the facade calls it, the source of the claims also needs to be decided.

**LIS read-only fields in the request schema:** None. LIS has no request schema for this operation.

**Register an Animal request fields without a confirmed LIS mapping:**

* `sub`
* `email`
* `given_name`
* `family_name`

## Response Mapping

There is no LIS response to map to, so every field in the `200` response is unmapped.

### Response codes compared

* Register an Animal `POST /v1/users` declares `200 OK`, `400 Bad Request`, `401 Unauthorized`, `409 Conflict`, `500 Internal Server Error` and `503 Service Unavailable`.
* LIS declares no codes for this operation, because no LIS operation exists.
* Across the LIS operations, the codes used are `200`, `202`, `204`, `400`, `403`, `404`, `500` and `503`. No LIS operation declares `401` or `409`. LIS uses `500` and `503` with the Errors array, but only for its own operations.

### 200 OK (no LIS counterpart)

Register an Animal returns `UserDetailsResponse`. None of its fields, or of the items in `cphs[]`, is listed as required.

#### User details

| Register an Animal field | Data type | Nullable | Required | LIS field | Data type | Nullable | Required | Transformation | Missing fields | Datatype mismatch | Enum/value mismatch | Cardinality differences | Possible breaking change | Notes |
|--------------------------|-----------|----------|----------|-----------|-----------|----------|----------|----------------|----------------|-------------------|---------------------|-------------------------|--------------------------|-------|
| subject | string | No | No | N/A | — | — | — | no direct equivalent; no LIS user object | Yes | N/A | N/A | N/A | No | The identity provider subject claim. Information is lost, but the message is still valid. |
| email | string | No | No | N/A | — | — | — | no direct equivalent; no LIS user object | Yes | N/A | N/A | N/A | No | Personal data. LIS hauliers have an `email` field, but it describes a haulier. Confirm before any sharing with LIS API clients. |
| firstName | string | Yes | No | N/A | — | — | — | no direct equivalent; no LIS user object | Yes | N/A | N/A | N/A | No | Personal data. Presumably comes from the request `given_name`; the spec does not say. |
| lastName | string | Yes | No | N/A | — | — | — | no direct equivalent; no LIS user object | Yes | N/A | N/A | N/A | No | Personal data. Presumably comes from the request `family_name`; the spec does not say. |
| displayName | string | Yes | No | N/A | — | — | — | no direct equivalent; no LIS user object | Yes | N/A | N/A | N/A | No | Personal data. The request has no matching claim, so it is not clear how this is filled. It could be `firstName` and `lastName` joined with a space, or passed through from the keeper data service, which is understood to be the case. Confirm which (unconfirmed). |
| cphs | array of `UserCphResponse` | No | No | N/A | — | — | — | no direct equivalent; no LIS user object | Yes | N/A | N/A | Zero or more CPHs per user | No | The CPHs the user was associated with when they last signed in. May be empty, as no `minItems` is set. |

#### CPH details (`cphs[]`)

| Register an Animal field | Data type | Nullable | Required | LIS field | Data type | Nullable | Required | Transformation | Missing fields | Datatype mismatch | Enum/value mismatch | Cardinality differences | Possible breaking change | Notes |
|--------------------------|-----------|----------|----------|-----------|-----------|----------|----------|----------------|----------------|-------------------|---------------------|-------------------------|--------------------------|-------|
| cphs[].cph | string | No | No | N/A | — | — | — | no direct equivalent; no LIS user object | Yes | N/A | N/A | One per CPH | No | County, parish and holding (CPH) number. LIS site identifiers can hold a CPH, but they describe a site on an animal, not a user's association. |
| cphs[].holdingId | string | Yes | No | N/A | — | — | — | no direct equivalent; no LIS user object | Yes | N/A | N/A | One per CPH | No | Keeper data identifier of the holding. |
| cphs[].holdingName | string | Yes | No | N/A | — | — | — | no direct equivalent; no LIS user object | Yes | N/A | N/A | One per CPH | No | Name of the holding, where the source carries one. LIS site `name` is read-only and belongs to a site on an animal, so it is not treated as an equivalent. |
| cphs[].role | string | No | No | N/A | — | — | — | no direct equivalent; no LIS user object | Yes | N/A | N/A | One per CPH | No | Role the user holds on the holding. It is a plain string with no fixed list of values (no enum), so any text is possible and the possible roles are not known. |

**Response notes:**
* Field paths start at the response body; `[]` means an array item. Parent objects are described in the notes, except `cphs`, which has its own row because of its cardinality.
* `subject` and `email` are non-nullable strings. `firstName`, `lastName`, `displayName`, `cphs[].holdingId` and `cphs[].holdingName` are nullable.
* Email and names are personal data. It is not clear whether they may be shared with LIS API clients. Confirm before any mapping is agreed.
* The spec does not say what is different between a created and a refreshed account in the response. It is understood that the `POST` returns `200` in both cases (unconfirmed).

### Error responses

#### 400 Bad Request — ProblemDetails

Register an Animal returns `application/problem+json` with a `ProblemDetails` object for `400`. No LIS user operation exists, so there is no LIS error to map to. The LIS side is N/A.

| Register an Animal field | Data type | Nullable | Required | LIS field | Data type | Nullable | Required | Transformation | Missing fields | Datatype mismatch | Enum/value mismatch | Cardinality differences | Possible breaking change | Notes |
|--------------------------|-----------|----------|----------|-----------|-----------|----------|----------|----------------|----------------|-------------------|---------------------|-------------------------|--------------------------|-------|
| type | string \| null | Yes | No | N/A | — | — | — | No LIS user error to translate | Yes | N/A | N/A | N/A | No | Problem type URI. Source may be null. |
| title | string \| null | Yes | No | N/A | — | — | — | No LIS user error to translate | Yes | N/A | N/A | N/A | No | Short summary. Source may be null. |
| status | integer (int32) \| string \| null | Yes | No | N/A | — | — | — | No LIS user error to translate | Yes | N/A | N/A | N/A | No | Source may be an integer, an integer-formatted string or null. |
| detail | string \| null | Yes | No | N/A | — | — | — | No LIS user error to translate | Yes | N/A | N/A | N/A | No | Explanation of the error. Source may be null. |
| instance | string \| null | Yes | No | N/A | — | — | — | No LIS user error to translate | Yes | N/A | N/A | N/A | No | Request path for local errors. LIS ProblemDetails has no `instance` field. |

**Error notes:**
* Register an Animal returns one `ProblemDetails` object. LIS error responses on other operations use an `Errors` array with 1 to 1000 items, so the single object would need wrapping if this endpoint is exposed. That is a cardinality change.
* LIS requires non-null `type`, `title`, `status` and `detail`, with length limits, and `status` must be an integer from 100 to 599. Register an Animal allows these to be missing or null. These differences would need rules if a LIS counterpart is added. They are not worked through here.
* If the LIS error contract is adopted, every option must return a root `Errors` array containing 1 to 1000 valid LIS `ProblemDetails` items. Options include translating the upstream problem or generating a facade-written problem, possibly selected by status code. The optional nested `errors` property can carry another translated problem within an array item, but cannot replace the root array; both parent and nested problems must satisfy the required LIS fields. The translation approach is a choice for the team.

#### 409 Conflict — ProblemDetails

Register an Animal returns the same `ProblemDetails` shape for `409`, so the table above applies. The spec says `409` is returned when the email belongs to a different account. No LIS operation declares `409`, so there is no LIS status for it. Passing the email conflict back to a LIS API client would also be a personal-data question.

#### 500 Internal Server Error — ProblemDetails

Register an Animal returns the same `ProblemDetails` shape for `500`, so the table above applies. LIS declares `500` with the Errors array on its own operations, but no LIS user operation exists to produce one.

#### 503 Service Unavailable — ProblemDetails

Register an Animal returns the same `ProblemDetails` shape for `503`, so the table above applies. The spec says `503` is returned when the keeper data service is temporarily unavailable. LIS declares `503` with the Errors array on its own operations, but only `GET /requeststatus/{requestId}` declares a `Retry-After` header on its `503`. Register an Animal declares no response headers, so there is no source for `Retry-After`. Agree how the facade would handle this if the endpoint is exposed.

#### 401 Unauthorized — ProblemDetails

* Register an Animal declares `401` for a rejected `x-api-key`. No LIS operation declares `401`, so there is no LIS status to map to.
* Do not pass `401` through to LIS API clients unchanged. Do not convert it to `403` automatically, because it may mean the facade's own key was rejected rather than the LIS API client lacking permission. Handling needs to be agreed.

### Response headers

* Register an Animal declares no response headers for this operation.
* LIS defines `LI-Request-Id` and `LI-Correlation-Id` headers on its own operations, and `Retry-After` on its `202` responses and on the `503` of `GET /requeststatus/{requestId}`. Whether the facade would return them for this endpoint is unconfirmed.

### Authentication and headers

* Register an Animal uses the `x-api-key` header (ApiKey, service-to-service, interim until AWS STS). This applies only to the facade's call to Register an Animal.
* LIS applies both `Ocp-Apim-Subscription-Key` and bearer-token authentication globally. This is not mapped, because no LIS operation exists for this endpoint.
* LIS defines a correlation ID header parameter on its operations. It is not mapped, because no LIS operation exists for this endpoint.

## Assumptions

These are the working assumptions this document relies on. Items marked **(unconfirmed)** need agreement before any implementation.

| Area | Assumption |
|------|------------|
| Intended client | **(unconfirmed)** It is understood that this operation is used by the BE4FE. It is not clear whether LIS API clients would use it. The Register an Animal spec does not say this. It is assumed this endpoint is not exposed through LIS until confirmed, because LIS has no user operation. |
| No LIS counterpart | No LIS endpoint, schema or field for user accounts exists in the LIS spec reviewed, so no mapping is proposed. The LIS spec was searched for user, account, profile and identity operations, and `GET /hauliers` and the site identifiers were considered and rejected as equivalents. |
| Request claims | Request: no LIS field -> Register an Animal `sub`, `email`, `given_name` and `family_name`. No mapping exists. |
| Response fields | Register an Animal response fields have no LIS destination and remain unmapped. |
| Error body | No LIS error response is mapped for `400`, `409`, `500` or `503`, because no LIS user operation exists. If the endpoint is exposed, Register an Animal `ProblemDetails` -> LIS `Errors` array (mapping unconfirmed). |
| `401` | **(unconfirmed)** A Register an Animal `401` is assumed to mean the facade's `x-api-key` was rejected (it is the only security scheme the Register an Animal JSON declares), not that the LIS API client's credentials are wrong. It is not assumed to be passed through unchanged or converted to `403`; the LIS status needs to be agreed. |

## Breaking-change summary

Compatibility would be considered in both directions: LIS to Register an Animal for requests, and Register an Animal to LIS for responses. Because there is no LIS counterpart, no field comparison is possible, so the table lists the gaps that would matter if a LIS counterpart were added.

In the mapping tables, **Missing fields: Yes** with **Possible breaking change: No** means information is lost because LIS has nowhere to hold it, but the facade can still produce a valid message, so this is not a breaking change.

| Category | Potential breaking change |
|----------|---------------------------|
| LIS coverage | The LIS spec has no operation to create or refresh a user. A LIS API client cannot use this Register an Animal capability through LIS. |
| Missing data | Register an Animal response fields `subject`, `email`, `firstName`, `lastName`, `displayName`, `cphs` and the `cphs[]` items have no LIS field. Information is lost, but the message is still valid. Possible breaking change: No. |
| Missing input | Register an Animal request fields `sub`, `email`, `given_name` and `family_name` have no LIS source. The schema makes the body optional, but whether the service accepts a call without claims is unconfirmed, and it is not clear that an account can be created or refreshed without `sub` or `email`. Possible breaking change: Yes for `sub` and `email`, if this endpoint is exposed. |
| Error contract | Register an Animal returns one `ProblemDetails` object (`application/problem+json`), while LIS uses an `Errors` array (`application/json`). This only matters if the endpoint is exposed to LIS API clients, where returning the error unchanged would not match the LIS contract. Possible breaking change: Yes if exposed. |
| Datatype | Register an Animal error `status` may be an integer, an integer-formatted string or null. LIS requires an integer from 100 to 599. Copying a string or null unchanged would not match. Possible breaking change: Yes if exposed. |
| Nullability | Register an Animal error `type`, `title`, `detail` and `status` may be null or missing. LIS requires non-null values. A null cannot fill a required LIS field. Possible breaking change: Yes if exposed. |
| Status codes | Register an Animal `401`, `409` and `503` have no LIS user equivalent, and no LIS operation declares `401` or `409`. Passing them through unchanged would give undeclared statuses. Only one LIS `503` (`GET /requeststatus/{requestId}`) declares a `Retry-After` header, which Register an Animal does not provide. Possible breaking change: Yes if exposed. |
| Security | The Register an Animal `x-api-key` and the LIS subscription key and token are separate. This is handled at facade level and does not change the payload. Possible breaking change: No. |

## Recommendations and remaining gaps

| Area | Recommendation or remaining gap |
|------|---------------------------------|
| Endpoint scope | Confirm whether LIS clients need this endpoint. If not, the facade keeps it internal and this document records the outcome. The LIS API has no equivalent operation. |
| Intended client | Ask the Register an Animal owners who this operation is for (it is understood to be used by the BE4FE) and whether LIS API clients would ever need it. |
| Source of request fields | If the endpoint is exposed, agree where `sub`, `email`, `given_name` and `family_name` come from. Do not assume a LIS token claim equals a Register an Animal identifier. |
| Personal data | Register an Animal accepts and returns email and names (personal data), and LIS has no field for them. If the endpoint is exposed, agree whether they may be sent to LIS API clients. |
| Error body | If the endpoint is exposed, agree how the single Register an Animal `ProblemDetails` becomes a LIS `Errors` array (1 to 1000 items), how null `type`, `title`, `detail` and `status` values are handled, and which of the alternatives listed under the `400` section is used (unconfirmed). |
| `409` | Agree how an email conflict is handled if the endpoint is exposed. No LIS status describes it, and the message touches personal data. |
| `401` | Do not assume a Register an Animal `401` means the LIS API client's credentials are wrong. It may mean the facade's `x-api-key` was rejected. Do not convert it automatically to LIS `403`. |
| `500` and `503` | If the endpoint is exposed, agree how a Register an Animal `500` or `503`, timeouts, failures and unexpected Register an Animal responses become LIS `500` or `503`, using the LIS error format.|
| Spec detail | Ask the Register an Animal owners for descriptions of the `400`, `401` and `500` responses, the allowed `cphs[].role` values, how `displayName` is filled (joined names or passed through from the keeper data service), and what happens when both `sub` and `email` are missing. |
| `GET /v1/users/{subject}` | Covered in `get-user-details.md`. |

**Result:** Register an Animal creates or refreshes a user account and returns the user's details and associated CPHs. The LIS API has no equivalent operation, request fields or response fields, so this endpoint cannot be mapped to LIS. Nothing is mapped until the endpoint scope decision in the Recommendations table is agreed.
