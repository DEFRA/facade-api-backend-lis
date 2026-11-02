# GET /v1/cattle/{earTag}

This document is the mapping reference for implementing the facade translation layer and identifying any required contract changes. The tables show the corresponding fields, transformations, compatibility differences, and remaining mapping gaps for these endpoints.

## Overview

### Object Mapping

| Register an Animal API object | LIS API object |
|-----------------------------------|----------------|
| Cattle                            | Animal         |

### Endpoint Mapping

| Register an Animal API endpoint | LIS API Endpoint |
|---------------------------------|------------------|
| GET /v1/cattle/{earTag}         | GET /animals/{identifier} |


### Endpoint Descriptions

* **GET /v1/cattle/{earTag}** - Get the cattle details - Retrieves the details of the cattle using its ear tag.
* **GET /animals/{identifier}** - Retrieves a specific animal by identifier - Retrieves the details of a single animal using its unique identifier. If the specified animal exists, the full resource is returned. If the animal does not exist, an appropriate error response is returned.

## Request Mapping

| Register an Animal field | Data type | Lis field | Data type | Transformation | Missing fields | Datatype mismatch | Enum/value mismatch | Cardinality differences | Breaking change | Notes |
|--------------------------|-----------|------------|------------|----------------|----------------|-------------------|---------------------|------------------------|-----------------|-------|
| earTag                   | string    | identifier | string     | rename         | No             | No                | No                  |      No                  | No              |       |

**Notes:** 
* Both path parameters are required strings. LIS documents an example identifier (`UK123456654321`); the Register an Animal schema does not constrain the format. 
* The Register an Animal endpoint uses `x-api-key`; LIS requires both `Ocp-Apim-Subscription-Key` and a bearer access token.

## Response Mapping

### 200 OK mapped to 200 OK

#### Animal details and dates

| Register an Animal field | Data type | Nullable | Required | LIS field | Data type | Nullable | Required | Transformation | Missing fields | Datatype mismatch | Enum/value mismatch | Cardinality differences | Possible breaking change | Notes |
|--------------------------|-----------|----------|----------|-----------|------------|----------|----------|----------------|----------------|-------------------|---------------------|------------------------|--------------------------|-------|
| earTag | string | no | no | animal.identifier | string | no | yes | rename and structural nest change | no | no | no | no | yes | Required-field mismatch: Register an Animal may omit earTag, while LIS requires animal.identifier. Agree how to supply the identifier when earTag is absent, including whether the request identifier is a valid fallback. Non-null field | |
| species | string \| null | yes | no | animal.species | string (enum) | no | yes | structural nest change, string/enum change | no | no | yes | no | yes | LIS enum is `bison`, `buffalo`, `cattle`, `deer`, `goats`, `sheep`; Register an animal allows any string or null. Nullability mismatch. |
| sex | string \| null | yes | no | animal.sex | string (enum) | no | yes | structural nest change, string/enum change | no | no | yes | no | yes | LIS enum is `female`, `male`; Register an Animal allows any string or null. Nullability mismatch. |
| dateBirth | string(date) \| null | yes | no | birth.date | string(date) | no | no | rename and structural nest change | no | no | no | no | yes | Nullability mismatch.|
| dateRegistered | string(date) \| null | yes | no | registration.date | string(date) | no | yes | rename and structural nest change | no | no | no | no | yes | Nullability mismatch. |
| dateOnCph | string(date) \| null | yes | no | N/A | — | — | — | no direct equivalent | yes | no | no | no | yes | LIS `createdDateTime` could be the correct mapping field? However, this is a date-time field for when the record was created, not necessarily the date the animal arrived on the holding? |

#### Current location

| Register an Animal field | Data type | Nullable | Required | LIS field | Data type | Nullable | Required | Transformation | Missing fields | Datatype mismatch | Enum/value mismatch | Cardinality differences | Possible breaking change | Notes |
|--------------------------|-----------|----------|----------|-----------|------------|----------|----------|----------------|----------------|-------------------|---------------------|------------------------|--------------------------|-------|
| currentLocation.identifier | string \| null | yes | no | currentSite.identifiers[].identifier | string | no | yes | rename, structural nest change and array mapping | no | no | no | yes | yes | Nullability mismatch. |
| currentLocation.subIdentifier | string \| null | yes | no | N/A | — | — | — | no direct equivalent | yes | no | no | no | yes | LIS site identifiers have `identifier` and identifier `type`; neither is defined as a sub-identifier. Could be retrieved from identifier field but unsure of split/trandformation necessary? Nullability mismatch.|
| currentLocation.fullIdentifier | string \| null | yes | no | N/A | — | — | — | no confirmed equivalent | yes | no | no | no | yes | LIS site identifiers have `identifier` and identifier `type`; neither is defined as a sub-identifier. Could be retrieved from identifier field but unsure of split/transformation necessary? Nullability mismatch. |
| currentLocation.name | string \| null | yes | no | currentSite.name | string (read-only) | no | no | rename and different nesting object fix | no | no | no | no | yes | `currentSite` and `name` are optional; `name` is not nullable when present. Nullability mismatch. |
| currentLocation.premisesType | string \| null | yes | no | currentSite.type.type | string | no | yes | rename and structural nest change | no | no | no | no | yes | LIS describes `type.type` as a site activity type (for example, Market or Abattoir)(Not Enum); confirm equivalence to premises type. Nullability mismatch. |

#### Registered location

| Register an Animal field | Data type | Nullable | Required | LIS field | Data type | Nullable | Required | Transformation | Missing fields | Datatype mismatch | Enum/value mismatch | Cardinality differences | Possible breaking change | Notes |
|--------------------------|-----------|----------|----------|-----------|------------|----------|----------|----------------|----------------|-------------------|---------------------|------------------------|--------------------------|-------|
| registeredLocation.identifier | string \| null | yes | no | registration.site.identifiers[].identifier | string | no | yes | rename, structural nest change and array mapping | no | no | no | yes | yes | Registered location is assumed to map to registration.site, not homeSite. Nullability mismatch. |
| registeredLocation.subIdentifier | string \| null | yes | no | N/A | — | — | — | no direct equivalent | yes | no | no | no | yes | LIS site identifiers have `identifier` and identifier `type`; neither is defined as a sub-identifier. |
| registeredLocation.fullIdentifier | string \| null | yes | no | N/A | — | — | — | no confirmed equivalent | yes | no | no | no | yes | LIS site identifier values are an array; confirm whether one is the full identifier and how to select it. |
| registeredLocation.name | string \| null | yes | no | registration.site.name | string (read-only) | no | no | rename and structural nest change | no | no | no | no | yes | Nullability mismatch. |
| registeredLocation.premisesType | string \| null | yes | no | registration.site.type.type | string | no | yes | rename and structural nest change | no | no | no | no | yes | LIS describes `type.type` as a site activity type (for example, Market or Abattoir) but this is not an Enum. Nullability mismatch. |

#### Death location

| Register an Animal field | Data type | Nullable | Required | LIS field | Data type | Nullable | Required | Transformation | Missing fields | Datatype mismatch | Enum/value mismatch | Cardinality differences | Possible breaking change | Notes |
|--------------------------|-----------|----------|----------|-----------|------------|----------|----------|----------------|----------------|-------------------|---------------------|------------------------|--------------------------|-------|
| deathLocation.identifier | string \| null | yes | no | death.site.identifiers[].identifier | string | no | yes | rename, structural nest change and array mapping | no | no | no | yes | yes | LIS may return multiple identifier types; confirm which type corresponds to this field.  Nullability mismatch. |
| deathLocation.subIdentifier | string \| null | yes | no | N/A | — | — | — | no direct equivalent | yes | no | no | no | yes | LIS site identifiers have `identifier` and identifier `type`; neither is defined as a sub-identifier. Could be retrieved from identifier field but unsure of split/transformation necessary? Nullability mismatch. |
| deathLocation.fullIdentifier | string \| null | yes | no | N/A | — | — | — | no confirmed equivalent | yes | no | no | no | yes | LIS site identifier values are an array; confirm whether one is the full identifier and how to select it. Could be retrieved from identifier field but unsure of split/transformation necessary? Nullability mismatch. |
| deathLocation.name | string \| null | yes | no | death.site.name | string (read-only) | no | no | rename and structural nest change | no | no | no | no | yes | Nullability mismatch |
| deathLocation.premisesType | string \| null | yes | no | death.site.type.type | string | no | yes | rename and structural nest change | no | no | no | no | yes | LIS describes `type.type` as a site activity type (for example, Market or Abattoir) but this is not an Enum. `death.site` is optional. Nullability mismatch. |

#### Breed and status

| Register an Animal field | Data type | Nullable | Required | LIS field | Data type | Nullable | Required | Transformation | Missing fields | Datatype mismatch | Enum/value mismatch | Cardinality differences | Possible breaking change | Notes |
|--------------------------|-----------|----------|----------|-----------|------------|----------|----------|----------------|----------------|-------------------|---------------------|------------------------|--------------------------|-------|
| breed | string \| null | yes | no | N/A | — | — | — | no confirmed equivalent; leave unmapped | yes | no | no | no | yes | Leave unmapped. `breed` could possibly mean the same as Register an Animal's `breedName`, which maps to LIS `breed.name`, but this is not confirmed. |
| breedCode | string \| null | yes | no | breed.code | string | no | no | rename and structural nest change | no | no | no | no | yes | Nullability mismatch |
| breedName | string \| null | yes | no | breed.name | string (read-only) | no | no | rename and structural nest change | no | no | no | no | yes | Nullability mismatch. |
| state | string \| null | yes | no | animal.state | string (enum, read-only) | no | no | structural nest change and value translation | no | no | yes | no | yes | LIS Enum values are `registered`, `dead`, `lost`, `stolen`; Register an animal permits any string and gives Alive/Dead as examples. Confirm value translation. Nullability mismatch. |
| restrictionStatus | string \| null | yes | no | N/A | — | — | — | no direct equivalent | yes | no | no | no | yes | No corresponding restriction-status property is defined in the LIS `Animal` response schema. |

#### Parent details

| Register an Animal field | Data type | Nullable | Required | LIS field | Data type | Nullable | Required | Transformation | Missing fields | Datatype mismatch | Enum/value mismatch | Cardinality differences | Possible breaking change | Notes |
|--------------------------|-----------|----------|----------|-----------|------------|----------|----------|----------------|----------------|-------------------|---------------------|------------------------|--------------------------|-------|
| damType | string \| null | yes | no | N/A | — | — | — | no direct equivalent; leave unmapped | yes | no | no | no | yes | Leave unmapped. LIS exposes separate `importParents.geneticDam` and `importParents.birthDam` objects. `damType` could possibly be derived from these, but no derivation rule is confirmed. LIS also exposes `registration.geneticDam` and `registration.birthDam`; all four parent objects are `ParentAnimal` objects, but no derivation rule from them to `damType` is confirmed. |
| geneticDamEarTag | string \| null | yes | no | `registration.geneticDam.identifier` / `importParents.geneticDam.identifier` | string | no | yes | rename and structural nest change | no | no | no | no | yes | Nullability mismatch. Both `registration.geneticDam` and `importParents.geneticDam` are `ParentAnimal` objects; which identifier to use if both are present is unconfirmed. |
| surrogateDamEarTag | string \| null | yes | no | `registration.birthDam.identifier` / `importParents.birthDam.identifier` | string | no | yes | rename and structural nest change | no | no | no | no | yes | The LIS `birthDam` description identifies the animal giving birth; confirm this is the source's surrogate dam. Nullability mismatch. Both `registration.birthDam` and `importParents.birthDam` are `ParentAnimal` objects; which identifier to use if both are present is unconfirmed. |
| sireEarTag | string \| null | yes | no | `registration.sire.identifier` / `importParents.sire.identifier` | string | no | yes | rename and structural nest change | no | no | no | no | yes | Nullability mismatch. Both `registration.sire` and `importParents.sire` are `ParentAnimal` objects; which identifier to use if both are present is unconfirmed. |
| sireName | string \| null | yes | no | `registration.sire.name` / `importParents.sire.name` | string (read-only) | no | no | rename and structural nest change | no | no | no | no | yes | Register an Animal schema says sireName is always null because CADS does not carry it. Nullability mismatch. Both `registration.sire` and `importParents.sire` are `ParentAnimal` objects; which name to use if both are present is unconfirmed. |


#### Response structure notes

* The APIs group fields differently: Register an Animal has top-level fields and nested `currentLocation`, `registeredLocation` and `deathLocation` objects, while LIS groups fields under `animal`, `birth`, `registration`, and other objects.
* LIS response fields not listed in the mapping tables are not mapped into the Register an Animal response.
* Required LIS response fields still need a source when building the LIS response.
* In the LIS Required column, “yes” means the field is required within its containing object. Optional parent objects may be absent, and identifiers arrays may be empty, so this does not guarantee a value is available to map.

### 401 Unauthorized / 403 Forbidden status mapping unconfirmed

* Note: Register an Animal declares `401 Unauthorized`, while LIS declares `403 Forbidden`; no translation between these statuses is confirmed. A downstream 401 may indicate rejection of the facade's API key, so it must not automatically become a consumer-facing 403. Agree the appropriate LIS error handling. Below shows these two statuses mapped but this should be confirmed.

| Register an Animal field | Data type | Nullable | Required | LIS field | Data type | Nullable | Required | Transformation | Missing fields | Datatype mismatch | Enum/value mismatch | Cardinality differences | Breaking change | Notes |
|--------------------------|-----------|----------|----------|-----------|------------|----------|-----------|----------------|----------------|-------------------|---------------------|------------------------|-----------------|-------|
| type | string \| null | yes | no | type | string (uri) | no | yes | direct mapping | no | no | no | no | yes | LIS requires a non-null URI; source field is nullable and optional. |
| title | string \| null | yes | no | title | string | no | yes | direct mapping | no | no | no | no | yes | LIS requires a non-null value; source field is nullable and optional. |
| status | integer(int32) \| string \| null | yes | no | status | integer(int32) | no | yes | string-to-integer conversion where needed, range validation and alignment with the facade's HTTP response status | no | yes | no | no | yes | Register an Animal permits an integer, an integer-formatted string, or null; LIS requires an integer between 100 and 599. For example, "404" needs conversion to 404. Missing or null values need an agreed handling rule. |
| detail | string \| null | yes | no | detail | string | no | yes | direct mapping | no | no | no | no | yes | LIS requires a non-null value; source field is nullable and optional. |
| instance | string \| null | yes | no | N/A | — | — | — | no direct equivalent | yes | no | no | no | yes | LIS ProblemDetails schema has no `instance` property. |


**Notes:** 
* This status translation is not established by matching the code descriptions alone: source `401` means unauthorized, while LIS declares `403 Forbidden` and does not declare `401` for this endpoint. 
* Register an Animal returns `application/problem+json` with a ProblemDetails object; LIS returns `application/json` with an array of ProblemDetails items (`Errors`). LIS ProblemDetails also has an errors property that can contain another ProblemDetails object; Register an Animal ProblemDetails has no corresponding field.
* Register an Animal to LIS: `type` must use URI format; `type` and `title` must not exceed 1,024 characters, and `detail` must not exceed 4,096 characters. `status` must be an integer between 100 and 599; string values need conversion and null values need an agreed handling rule.
* Both APIs allow an integer `status`, but Register an Animal also allows an integer-formatted string or null. The `int32` format does not remove those permitted types.

### 404 Not Found mapped to 404 Not Found

**Notes:** Mapping is same as `401 Unauthorized unconfirmed mapping to 403 Forbidden`

LIS also declares `500` and `503` responses, which are not declared by the Register an Animal endpoint. 

## Register an Animal fields without a confirmed LIS mapping

The fields below have `N/A` in the LIS field column. The tables above retain the reasons and any potential alternatives.

### 200 OK response

- `dateOnCph`
- `currentLocation.subIdentifier`
- `currentLocation.fullIdentifier`
- `registeredLocation.subIdentifier`
- `registeredLocation.fullIdentifier`
- `deathLocation.subIdentifier`
- `deathLocation.fullIdentifier`
- `breed`
- `restrictionStatus`
- `damType`

### Error response

- `instance`

## Assumptions

| Area | Assumption |
|------|------------|
| `earTag` | Register an Animal's earTag represents the same animal identifier as LIS animal.identifier. |
| `currentLocation` | currentLocation maps to currentSite. |
| `deathLocation` | deathLocation maps to death.site, not the carcass collection site. |
| `registeredLocation.name` | registeredLocation.name maps to registration.site.name, not homeSite in LIS. |
| `subIdentifier` and `fullIdentifier` | Register an Animal's currentLocation, registeredLocation and deathLocation objects all have subIdentifier and fullIdentifier fields that don't have direct field maps so remain unmapped. Further knowledge would be required to determine if these could be derived from the LIS identifier field, but this could not be confirmed. |
| `premisesType` | Register an Animal's premisesType fields for current, registered and death locations are assumed to mean the same as the corresponding LIS site activity type. |
| `registeredLocation.premisesType` | registeredLocation.premisesType from Register an Animal is assumed to map to the registration.site.type.type field in LIS |
| `surrogateDamEarTag` | surrogateDamEarTag maps to importParents.birthDam.identifier. LIS also exposes registration.birthDam.identifier; both parent objects are ParentAnimal, but their equivalence and source precedence are unconfirmed. |
| `dateOnCph` | dateOnCph remains unmapped as no clear map field. Further knowledge would be required to determine if LIS createdDateTime could be a suitable map, but this could not be confirmed. |
| `breed` and `damType` | breed and damType remain unmapped as no clear map fields. Further knowledge would be required to determine if they could be derived from breed.name and importParents.geneticDam and importParents.birthDam objects, but this could not be confirmed. LIS also exposes registration.geneticDam and registration.birthDam as ParentAnimal objects; no derivation rule is confirmed. |
| `restrictionStatus` | restrictionStatus from Register an Animal remains unmapped as no clear map field. |


## Breaking-change summary

Compatibility is considered in both directions: Register an Animal to LIS, and LIS to Register an Animal. This compares the contracts.

These are potential breaks if the facade does not handle the differences below. The direction matters: a non-null LIS value can populate a nullable Register an Animal field, but a null Register an Animal value cannot populate a non-null LIS field. Renames and nesting changes need transformation but are not breaking changes on their own.

| Category | Potential breaking change |
|----------|---------------------------|
| Nullability | Register an Animal to LIS: null values cannot be copied into non-null LIS fields, such as `sex` to `animal.sex` or `dateRegistered` to `registration.date`. LIS to Register an Animal: non-null values can populate the corresponding nullable fields. Missing optional LIS values still need an agreed null or omission rule. |
| Required fields | Register an Animal to LIS: an optional field may be absent even though LIS requires its equivalent, such as `earTag` to `animal.identifier` or `species` to `animal.species`. A nested LIS field may only be required when its parent object is present. In either direction, all required destination fields must be supplied. |
| Datatype | Register an Animal `status` may be an integer, an integer-formatted string, or null; LIS `status` must be an integer between 100 and 599. Copying a string or null unchanged would not match the LIS contract. |
| Allowed values | Register an Animal to LIS: `species`, `sex` and `state` allow strings outside the enums accepted by `animal.species`, `animal.sex` and `animal.state`. These need validation or an agreed translation. LIS to Register an Animal: the enum strings fit the string datatype, but their meaning still needs to match caller expectations. |
| Missing data | `dateOnCph`, `breed`, `restrictionStatus`, `damType`, location `subIdentifier`/`fullIdentifier` fields, and error `instance` have no confirmed LIS mapping. Callers may no longer receive values they previously relied on. |
| Cardinality | `currentSite.identifiers[]`, `registration.site.identifiers[]` and `death.site.identifiers[]` can contain several identifiers. Selecting the wrong entry could return an incorrect `currentLocation.identifier`, `registeredLocation.identifier` or `deathLocation.identifier`. |
| Values and behaviour | Mapping `state` to `animal.state` needs an agreed translation for values such as Alive and Dead into the LIS enum. Where `sireName` is null or missing, omit `importParents.sire.name`; populate it only if `sireName` is available and `sireEarTag` supplies the required `importParents.sire.identifier`. |
| Response structure | The facade must rename and restructure fields to match the destination contract. For this implementation, returning the Register an Animal response unchanged would not match the LIS response. |
| Error contract | Register an Animal returns one ProblemDetails object using application/problem+json, while LIS expects an Errors array using application/json. Returning the Register an Animal error unchanged would not match the LIS contract. Agree how downstream Register an Animal 401 responses and service failures map to the facade’s LIS error responses. |

## Recommendations and remaining gaps 

| Area | Recommendation or remaining gap |
|------|---------------------------------|
| Location identifiers | Agree which identifier type to assign when mapping currentLocation.identifier, registeredLocation.identifier and deathLocation.identifier into currentSite.identifiers[], registration.site.identifiers[] and death.site.identifiers[]. Each LIS array entry requires identifier and type. Do not assume the type is holdingNumber without confirmation. |
| `state` | Agree how state maps to animal.state, which allows registered, dead, lost and stolen. Do not assume Alive means registered; confirm the translation and handling of unsupported values. Omit animal.state if unavailable, as it is optional. |
| `sex` | Map sex to animal.sex using female or male. Confirm any spelling or case conversions and agree how to handle missing, null or unsupported values, as animal.sex is required. |
| `species` | Map species to animal.species using bison, buffalo, cattle, deer, goats or sheep. Confirm any spelling or case conversions and agree how to handle missing, null or unsupported values, as animal.species is required. |
| Missing values | Where Register an Animal cannot supply a value, return null or omit the field only where the LIS schema allows it. Agree how to handle missing required LIS values. |
| `earTag` | Agree how to provide the required `animal.identifier` when `earTag` is absent; do not assume the request identifier is a valid fallback. |
| `sireName` | Where sireName is null or missing, omit importParents.sire.name. If sireName becomes available, map it to importParents.sire.name. Only include importParents.sire when its required identifier can also be supplied from sireEarTag. LIS also exposes registration.sire as a ParentAnimal; confirm whether it is a source for sireName and which path takes precedence. |
| Parent fields | Confirm the mapping rule in each direction. For LIS to Register an Animal, agree whether to use `registration.*` or `importParents.*` when both parent objects are present. For Register an Animal to LIS, confirm which LIS parent object or objects each source field should populate. Until agreed, treat the paths and their precedence as unresolved. |
| `403` / `401` | Agree how the facade handles a downstream Register an Animal 401. Do not automatically convert it to LIS 403: it may mean the facade’s x-api-key was rejected rather than the consumer lacking permission. Handle consumer authentication separately and document the agreed error response in the LIS contract. |
| Error response body | Recommendation to convert one Register an Animal ProblemDetails object into a LIS Errors array containing one ProblemDetails object. Agree how to handle missing or null values for the required LIS fields, ensure status matches the facade’s HTTP response status, and use application/json. |
| `500` / `503` | Agree when the facade returns LIS 500 or 503 for downstream Register an Animal failures, timeouts or unexpected responses. Return the LIS Errors array using application/json. |
| Register an Animal -> LIS | Identify where to obtain required LIS values with no documented source mapping, including registration.category, site identifier type and site type.description. If birth is populated, birth.site, birth.assistedBirthFlag, birth.multipleBirthsFlag and birth.embryoTransferFlag are also required. Do not invent values to complete these objects.
