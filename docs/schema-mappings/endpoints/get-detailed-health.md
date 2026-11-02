# GET /health/detailed

This document is the mapping reference for implementing the facade translation layer and identifying any required contract changes. The tables show the corresponding fields, transformations, compatibility differences, and remaining mapping gaps for these endpoints.

**Scope:** This document maps the Register an Animal `GET /health/detailed` operation to the LIS API. The LIS API has no health operation, so no LIS operation is mapped as a counterpart. No other operation is referenced.

I suggest that we add a new endpoint that doesn't exist in the LIS api and calls to this one to allow us to run a health check on the cattle api

## Overview

### Object Mapping

| Register an Animal API object | LIS API object |
|-------------------------------|----------------|
| HealthCheckResponse | N/A (no LIS health object) |
| ComponentHealthResult | N/A (no LIS health object) |

### Endpoint Mapping

| Register an Animal API endpoint | LIS API endpoint |
|---------------------------------|------------------|
| GET /health/detailed | N/A (no LIS equivalent) |

### Endpoint Descriptions

* **GET /health/detailed** - Get detailed health status - Performs and returns a detailed health check report for Register an Animal's dependencies, including database, queue, and background jobs.
* **N/A** - The LIS API has no health or service-status operation.

## Request Mapping

The Register an Animal operation takes no path parameters, query parameters or request body, so there are no request fields to map.

**Request notes**
* No request fields exist. Register an Animal does not declare any headers for this operation.
* The LIS API has no equivalent operation, so no LIS request field can feed this operation.

**LIS read-only fields in the request schema**
* None. The LIS API has no health request schema.

**Register an Animal request fields without a confirmed LIS mapping**
* None. The operation has no request fields.

## Response Mapping

Register an Animal returns a `HealthCheckResponse` body. The LIS API has no operation that returns a health report, so no LIS response field exists for any Register an Animal response field.

**Response codes comparison**
* Register an Animal declares `200 OK` and `503 Service Unavailable` for this operation. Both use the same `HealthCheckResponse` schema.
* The LIS API declares no response for this operation because it has no equivalent operation. There is nothing to compare these codes against.

### 200 OK mapped to no LIS response

Register an Animal's `200` body contains the fields below. Every field has no LIS counterpart, so each is listed as unmapped.

| Register an Animal field | Reason / impact |
|--------------------------|-----------------|
| `status` (string, required) | No LIS health data to take this from. The overall status value is lost. Message still valid. |
| `totalDurationSeconds` (number or string, required) | No LIS field to take the total check duration from. Information lost. Message still valid. |
| `entries` (object, required) | No LIS field for component results. The whole component map is lost. Message still valid. |
| `entries.<componentName>.status` (string, required) | No LIS field for per-component status. Information lost. Message still valid. |
| `entries.<componentName>.description` (string or null) | No LIS field for component descriptions. Information lost. Message still valid. |
| `entries.<componentName>.durationSeconds` (number or string, default 0) | No LIS field for per-component duration. Information lost. Message still valid. |
| `entries.<componentName>.data` (object or null) | The LIS API has no health contract, so there is no LIS field for component metadata. The spec does not declare the object's properties. The object could still be passed through as opaque JSON if LIS had a field to receive it. Information lost because LIS has nowhere to hold it. Message still valid. |

### 503 Service Unavailable mapped to no LIS response (mapping unconfirmed)

Register an Animal uses the same `HealthCheckResponse` body for `503` as for `200`, so the field table above applies. The spec gives no rule for which `status` values lead to `503`. The LIS API declares no `503` for this operation because it has no equivalent operation.

**Response notes**
* `totalDurationSeconds` and `durationSeconds` are typed as either a number or a string, with a numeric pattern. A pass-through can keep either representation, so no single type has to be chosen. Normalising to one type is needed only if the facade adopts a narrower contract.
* `entries` is a map with dynamic keys. The spec does not list the component names, so the field paths use `<componentName>`.
* The spec gives examples for `status` (Healthy, Degraded, Unhealthy) but does not define a closed list of values.

**Error response mapping**
* Register an Animal declares no error response other than `503` for this operation. The `503` body is a `HealthCheckResponse`, not a `ProblemDetails`.
* The LIS API declares no error responses for this operation because it has no equivalent operation.

**Response headers**
* Register an Animal declares no response headers on `200` or `503`.
* The LIS API declares no response headers for this operation.

**Authentication and headers**
* Register an Animal: the operation declares no `security`, and the root document sets no global security. The `ApiKey` scheme (`x-api-key` header) is defined in the spec but is not applied to this operation. It is not confirmed whether this endpoint is meant to be public.
* LIS: `ApiSubscriptionKey` (`Ocp-Apim-Subscription-Key`) and `ApiAccessToken` (bearer JWT) apply globally. They have no operation to attach to here.
* No header parameters are declared for this operation in Register an Animal.

## Assumptions

| Assumption | Source |
|------------|--------|
| Request: the Register an Animal operation takes no parameters or body, so no LIS request field maps to it. | Register an Animal spec |
| Response: the LIS API has no health operation, so no field of `HealthCheckResponse` or `ComponentHealthResult` has a LIS field to map to. | LIS spec |
| The `200` and `503` responses share one schema in Register an Animal, so one field table covers both. | Register an Animal spec |
| The component names under `entries` are not listed in the spec and are treated as dynamic keys. | Register an Animal spec |

## Breaking-change summary

Missing fields with Possible breaking change: No means information is lost because the LIS API has nowhere to hold it, but the message is still valid.

| Finding | Impact |
|---------|--------|
| LIS API: no health operation exists, so every Register an Animal health field is a missing field. | Information loss only. Possible breaking change: No. |
| Register an Animal: `totalDurationSeconds` and `durationSeconds` accept either number or string. | Either representation can pass through unchanged. Normalising to one type is needed only if the facade adopts a narrower contract. Not a LIS mismatch because LIS has no matching field. |
| Register an Animal: `entries.<componentName>.data` is an object or null. The spec declares no properties for it, so it could be passed through as opaque JSON. | Information loss only because the LIS API has no health field to receive it, not because of the unspecified properties. Possible breaking change: No. |
| Register an Animal: `503` uses the same body as `200` and the spec does not say which status values cause `503`. | Facade cannot tell from the spec when `503` applies. |

## Recommendations and remaining gaps

| Item | Recommendation |
|------|----------------|
| New health check endpoint | I recommend we add a call to this endpoint from a new health check endpoint that we can expose from the facade to allow check for if the cattle api is running from it|
| Component keys in `entries` are dynamic. | Confirm the list of component names with the Register an Animal team, if the facade needs to rely on them. |
| `status` and `description` have example values only. | Confirm the allowed values with the Register an Animal team. |
