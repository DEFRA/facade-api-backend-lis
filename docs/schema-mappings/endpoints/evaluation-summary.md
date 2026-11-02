# Register an Animal and LIS: Endpoint Mapping Summary

## Contents

- [Purpose and scope](#purpose-and-scope)
  - [How to read the status](#how-to-read-the-status)
- [At a glance](#at-a-glance)
- [Endpoint overview map](#endpoint-overview-map)
- [Mapped endpoints](#mapped-endpoints)
- [LIS API endpoints not mapped](#lis-api-endpoints-not-mapped)
- [Register an Animal API endpoints not mapped](#register-an-animal-api-endpoints-not-mapped)
- [Cross-cutting issues](#cross-cutting-issues)
- [Outstanding decisions and questions](#outstanding-decisions-and-questions)

## Purpose and scope

- **Purpose:** a single view of which LIS endpoints have a Register an Animal counterpart, what can and cannot be mapped in the facade, and why.
- **Sources:** LIS OpenAPI specification (version 1.0.0-beta.2) and Register an Animal OpenAPI specification (version 1.0).
- **Organisation:** tables are organised by **LIS endpoint** first. LIS endpoints are shown without the `/v1` prefix, which LIS includes in its base URL rather than in each path.
- **Detail:** each row links to its endpoint evaluation, which contains field inventories, error responses and outstanding questions.
- **“Routine”** means a straightforward field or shape transformation **after** identity and business semantics are confirmed; it does not mean the whole endpoint can be implemented from LIS alone.

### How to read the status

| Status | Meaning |
|---|---|
| 🟢 **Mappable** | The endpoints correspond and their request and response can be mapped using routine transformations (renaming, flattening or nesting), once the documented assumptions are confirmed. |
| 🟡 **Partial** | A related endpoint exists, but missing data, value differences or open decisions prevent a complete translation. |
| 🔴 **No equivalent** | No corresponding endpoint exists in the other API. |

**N/A** means there is no equivalent endpoint in that API.

## At a glance

| | 🟢 Mappable | 🟡 Partial | 🔴 No equivalent |
|---|---|---|---|
| LIS endpoints (21) | 0 | 4 | 17 |
| Register an Animal endpoints (12) | 0 | 5 | 7 |

**None of the twelve routes has a confirmed, complete LIS-only mapping.**

1. **LIS endpoints:** of the 21 LIS endpoints, **four have a Register an Animal counterpart, all partial**. `GET /animals/{identifier}` is **the closest direct match**, with most fields mapped through renaming and restructuring once enum translations and unmapped fields are agreed.
2. **Cattle lists:** `GET /animals` is the closest match for the two cattle-list routes and contains some **potentially routine field transformations**, but needs additional data and agreed rules to preserve the Register an Animal response.
3. **Registration:** `POST /animals` and `GET /requeststatus/{requestId}` depend on **an agreed create-then-validate flow** and **a stored link between the LIS request and the Register an Animal bundle**.
4. **LIS-only endpoints:** the remaining 17 LIS endpoints (animal update/delete, reference data, lost and found, movements and hauliers) **have no Register an Animal equivalent**.
5. **Register an Animal-only endpoints:** holding and bundle routes **need their existing authoritative data sources** (KRDS or Register an Animal's local database). The get user details and get detailed health check endpoints both **have no equivalent** for implementation from the LIS api. Create or refresh user account (`POST /v1/users`) likewise **has no LIS equivalent**.

## Endpoint overview map

| LIS endpoint | Register an Animal endpoint | Status |
|---|---|---|
| `GET /animals` | `GET /v1/holdings/{county}/{parish}/{holding}/cattle`<br>`GET /v1/holdings/{cph}/cattle` | 🟡 **Partial** |
| `GET /animals/{identifier}` | `GET /v1/cattle/{earTag}` | 🟡 **Partial** |
| `POST /animals` | `POST /v1/registrations` | 🟡 **Partial** |
| `GET /requeststatus/{requestId}` | `POST /v1/registrations/{id}/validate` | 🟡 **Partial** (async only) |
| Animal update and delete (2 endpoints) | N/A | 🔴 **No equivalent** |
| Reference data: breeds, genotypes, death reasons (3 endpoints) | N/A | 🔴 **No equivalent** |
| Lost and found (4 endpoints) | N/A | 🔴 **No equivalent** |
| Movements, hauliers and rejection reasons (8 endpoints) | N/A | 🔴 **No equivalent** |
| N/A | Holding details and holding search (2 endpoints) | 🔴 **No equivalent** |
| N/A | Bundles for a holding (2 endpoints) | 🔴 **No equivalent** |
| N/A | User details and user account (2 endpoints) | 🔴 **No equivalent** |
| N/A | Detailed health check (1 endpoint) | 🔴 **No equivalent** |

## Mapped endpoints

| LIS endpoint | Register an Animal endpoint | Status | Main issue / work needed&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; | Details |
|---|---|---|---|---|
| `GET /animals` | <code>GET /v1/holdings/<wbr>{county}/<wbr>{parish}/<wbr>{holding}/<wbr>cattle</code><br><code>GET /v1/holdings/<wbr>{cph}/<wbr>cattle</code> | 🟡 **Partial** | **Related, partial field mapping only.**<br>**CPH to `siteIdentifier`** and **`species=cattle`** need agreement.<br><br>Flattening animal identifier, sex and breed is relatively routine **if semantics match**, but LIS list results **lack birth/arrival dates, local processing status and per-animal errors**.<br><br>**LIS paging, missing filters and local-record merging** prevent a direct full response translation.<br><br>For the `{cph}` route, the single path value is unescaped rather than joined from three segments; accepted spelling/encoding and equivalence to LIS `siteIdentifier` must be confirmed. | [get-cattle-on-holding.md](./get-cattle-on-holding.md)<br>[get-cattle-by-CPH.md](./get-cattle-by-CPH.md) |
| `GET /animals/{identifier}` | `GET /v1/cattle/{earTag}` | 🟡 **Partial** | **Closest direct match.**<br>`earTag` to `identifier` is a rename, and most animal, location, breed and parent fields map through **renaming and restructuring**.<br><br>`sex`, `species` and `state` **need agreed translation into the LIS enums**, **null values cannot fill required LIS fields**, and site identifier types and parent-object precedence need agreeing.<br><br>`dateOnCph`, `breed`, `restrictionStatus`, `damType` and location sub/full identifiers have **no confirmed mapping**. | [get-cattle-details.md](./get-cattle-details.md) |
| `POST /animals` | `POST /v1/registrations` | 🟡 **Partial** | **Mapping unconfirmed.**<br>One LIS animal maps to a one-animal Register an Animal bundle, with candidate mappings for identifier, birth date, sex, breed and parents.<br><br>LIS requires `animal.species`, `registration.date` and `registration.category` (plus birth site and flags if `birth` is supplied), which **Register an Animal has no fields for**.<br><br>A Register an Animal `201` means **a bundle was created, not that the animal is registered**; sync `200`/async `202` handling, and whether the facade must also call validate, **are undecided**. | [post-cattle-registration-bundle.md](./post-cattle-registration-bundle.md) |
| `GET /requeststatus/{requestId}` | <code>POST /v1/registrations/<wbr>{id}/<wbr>validate</code> | 🟡 **Partial** | **Mapping unconfirmed; async only.**<br>For this facade mapping, the endpoint tracks `POST /animals` requests made with `LI-Request-Mode: async` (the default is `sync`); LIS also uses it for other asynchronous operations.<br><br>LIS `requestId` is **not known to be the Register an Animal bundle `id`**, so the facade would need to store that link.<br><br>`isValid` and `status` are candidates for LIS `Pending`/`Completed`/`Failed`, but **nothing maps to `Pending`** and **`isValid: true` does not prove registration**.<br><br>For a single-animal submission, `animalResults[].earTag` is a candidate for `result.identifier` once registration is confirmed; per-animal validity and validation errors **have no LIS field**, and validate is a `POST` that **may not be safe to repeat** on each status check. | [post-cattle-validate-registration-bundle.md](./post-cattle-validate-registration-bundle.md) |

## LIS API endpoints not mapped

These LIS API endpoints currently do not map to a Register an Animal endpoint. Summaries are taken from the LIS OpenAPI specification.

| LIS API endpoint | Summary |
|---|---|
| `PUT /animals/{identifier}` | Updates an existing animal record (sync or async). |
| `DELETE /animals/{identifier}` | Deletes an existing animal record (sync or async). |
| `GET /breeds` | Retrieves breed records. |
| `GET /genotypes` | Retrieves genotype records. |
| `GET /deathreasons` | Retrieves death reason records. |
| `GET /lostfounds` | Retrieves lost and found records. |
| `POST /lostfounds` | Creates a new lost and found record (sync or async). |
| `GET /lostfounds/{id}` | Retrieves a specific lost and found record by ID. |
| `PUT /lostfounds/{id}` | Updates an existing lost and found record (sync or async). |
| `GET /movements` | Retrieves movement records. |
| `POST /movements` | Creates a new movement record (sync or async). |
| `GET /movements/{movementNumber}` | Retrieves a specific movement by movement number. |
| `PUT /movements/{movementNumber}` | Updates an existing movement record (sync or async). |
| `DELETE /movements/{movementNumber}` | Deletes an existing movement record (synchronous or asynchronous). |
| `POST /movements/{movementNumber}/confirmations` | Creates a confirmation for a specific movement (sync or async). |
| `GET /hauliers` | Retrieve a list of hauliers. |
| `GET /rejectionreasons` | Retrieves movement rejection reasons. |

## Register an Animal API endpoints not mapped

These Register an Animal API endpoints currently do not map to a LIS endpoint. Summaries are taken from the Register an Animal OpenAPI specification.

| Register an Animal API endpoint | Summary | Main issue / work needed | Details |
|---|---|---|---|
| `GET /v1/holdings` | Search holdings. | **Holding search comes from KRDS.**<br>LIS animal search cannot search holdings by free text or return holding summaries and their paging metadata.<br><br>**Retain a holding-search source.** | [search-holdings.md](./search-holdings.md) |
| `GET /v1/holdings/{county}/{parish}/{holding}` | Get the holding details. | **Holding details come from KRDS, not LIS.**<br>LIS has no holding resource, and the `site` embedded in LIS animal resources cannot supply holding address, keeper, marks, dates or associations.<br><br>**Retain an authoritative holding source.** | [get-holding-details.md](./get-holding-details.md) |
| `GET /v1/holdings/{county}/{parish}/{holding}/bundles`<br>`GET /v1/holdings/{cph}/bundles` | Get the bundles for the holding. | **Lists submissions stored locally by Register an Animal.**<br>LIS cannot reconstruct bundle IDs, membership, submitted values, statuses, timestamps or per-animal errors, and `GET /requeststatus/{requestId}` tracks **only one request**.<br><br>The single `{cph}` route also matches stored CPHs exactly, so hyphen- and slash-separated values **must not be assumed to be interchangeable**. | [get-bundles-for-a-holding.md](./get-bundles-for-a-holding.md)<br>[get-bundles-by-CPH.md](./get-bundles-by-CPH.md) |
| `GET /v1/users/{subject}` | Get a user's details and associated CPHs. | **LIS has no user account system** that is an equivalent for this endpoint. | [get-user-details.md](./get-user-details.md) |
| `POST /v1/users` | Create or refresh a user account. | **LIS has no operation to create or refresh a user**, so nothing is mapped.<br><br>**Confirm whether LIS clients need this endpoint** (it is understood to be used by the BE4FE); if exposed, the source of the `sub`/`email` claims, sharing of personal data and `409` handling would need agreeing. | [post-user-account.md](./post-user-account.md) |
| `GET /health/detailed` | Get detailed health status. | **LIS does not have a health check endpoint** that could be used by the facade here, it may be worthwhile to **add a health check endpoint anyway** to allow the facade to check for issues on the Register an Animal API. | [get-detailed-health.md](./get-detailed-health.md) |

## Cross-cutting issues

Authentication, correlation headers and error-body/status translation also need treatment when introducing LIS; similar status codes or field names alone do not establish equivalence. These apply to every mapped endpoint.

| Area | Issue |
|---|---|
| **Authentication** | LIS API clients use `Ocp-Apim-Subscription-Key` and a bearer token; Register an Animal uses `x-api-key`. **The two need handling separately.** |
| **Headers** | LIS `LI-Request-Id`, `LI-Correlation-Id`, `LI-Request-Mode` and `Retry-After` **have no Register an Animal equivalent**; Register an Animal requires `x-cdp-request-id`. |
| **Error body** | Register an Animal returns **one `ProblemDetails` object** (`application/problem+json`); LIS returns **an `Errors[]` array** of `ProblemDetails` (`application/json`) with required, length-limited `type`, `title`, `status` and `detail`. |
| **Status codes** | A Register an Animal `401` may mean **the facade's own `x-api-key` was rejected**, so it **should not be converted automatically to LIS `401` or `403`**.<br><br>Mapping of `500`/`503`, timeouts and unexpected responses **needs agreeing**. |

## Outstanding decisions and questions

| # | Decision needed | Affects (LIS Endpoints) | Details |
|---|---|---|---|
| 1 | Whether **CPH and LIS `siteIdentifier` identify the same thing**, and which CPH formats (slash- or hyphen-separated) are accepted. | `GET /animals` | [get-cattle-on-holding.md](./get-cattle-on-holding.md)<br>[get-cattle-by-CPH.md](./get-cattle-by-CPH.md) |
| 2 | Whether **locally stored submission records are included** in animal lists, and how paging and `count` behave. | `GET /animals` | [get-cattle-on-holding.md](./get-cattle-on-holding.md) |
| 3 | **Translation of `species`, `sex` and `state`/`status` values** to and from the LIS enums, including unsupported or null values,<br><br>and **which LIS parent object (`importParents` or `registration`) takes precedence** for sire and dam values. | `GET /animals`<br>`GET /animals/{identifier}`<br>`POST /animals` | [get-cattle-details.md](./get-cattle-details.md)<br>[get-cattle-on-holding.md](./get-cattle-on-holding.md)<br>[post-cattle-registration-bundle.md](./post-cattle-registration-bundle.md) |
| 4 | How to handle **LIS request fields that Register an Animal cannot represent** (for example `animal.species`, `registration.date` and `registration.category`), and how to supply **required LIS response fields absent or null in Register an Animal** (for example site identifier type and `registration.category`). | `GET /animals/{identifier}`<br>`POST /animals` | [get-cattle-details.md](./get-cattle-details.md)<br>[post-cattle-registration-bundle.md](./post-cattle-registration-bundle.md) |
| 5 | **Create-then-validate flow:** whether and when the facade calls validate, sync `200` versus async `202` behaviour, and what a passing validation proves. | `POST /animals`<br>`GET /requeststatus/{requestId}` | [post-cattle-registration-bundle.md](./post-cattle-registration-bundle.md)<br>[post-cattle-validate-registration-bundle.md](./post-cattle-validate-registration-bundle.md) |
| 6 | Where the facade **stores the link** between the LIS `requestId` and the Register an Animal bundle `id`. | `POST /animals`<br>`GET /requeststatus/{requestId}` | [post-cattle-validate-registration-bundle.md](./post-cattle-validate-registration-bundle.md) |
| 7 | **Error translation** (`ProblemDetails` to `Errors[]`) and **the LIS status returned for a Register an Animal `401`**. | All mapped endpoints | [Cross-cutting issues](#cross-cutting-issues) |
| 8 | Whether **holdings, bundles, users and health endpoints stay outside the facade** and continue to use their existing data sources,<br><br>including whether **user personal data (email and names) may be shared** with LIS clients. | Register an Animal endpoints not mapped | [Register an Animal API endpoints not mapped](#register-an-animal-api-endpoints-not-mapped) |

**Result:** **None of the Register an Animal endpoints can be used through the facade as a complete, confirmed LIS mapping today.**

Four LIS endpoints (`GET /animals`, `GET /animals/{identifier}`, `POST /animals` and `GET /requeststatus/{requestId}`) are **partially mappable once the decisions above are agreed**; all other endpoints in either API **have no equivalent**.

Further endpoint-specific decisions, assumptions and recommendations are recorded in each linked endpoint evaluation.
