# Postman Collection Explanation

This is a collection of the current endpoints for the lis-facade-api-backend

## Running Endpoints Locally
Each endpoint's cattle client is chosen by its client mode, which is set in the `CattleApi:ClientModes` section of `appsettings.json`. To run the endpoints locally, set them to `fake`, as the real client will not work locally. Endpoints that are not listed default to `fake`. 

### Example:
Both `GetAnimals` and `GetAnimalByIdentifier` are already set to `fake` in `appsettings.json`, so no code changes are needed:
```
"CattleApi": {
  "ClientModes": {
    "GetAnimals": "fake",
    "GetAnimalByIdentifier": "fake"
  }
}
```
To override a value locally without editing `appsettings.json`, either set the environment variable `CattleApi__ClientModes__GetAnimals=fake` (and the same for `GetAnimalByIdentifier`), or add the same section to `appsettings.Local.json`, which is loaded when it exists.

If an endpoint returns `404`, its feature flag may be disabled. The flags are in the `FeatureManagement` section of `appsettings.json`, and are described in the main [README](../../README.md).

## Running Endpoints on DEV
If you wish to test against an environment, such as DEV, you will need to change the address to use the DEV address, as well as create a DEV API key on the CDP that will be added as the value for the x-api-key. The x-api-key is not needed for local endpoints.

## Current endpoints

Health Check (/health)

Get Animals (/animals)

Get Animals by Identifier (/animals/{{identifier}})