# Postman Collection Explanation

This is a collection of the current endpoints for the lis-facade-api-backend

## Running Endpoints Locally
The included endpoints are set to call to your local system while running the project, if you wish to run the endpoints locally, you will need to run set their EndpointService file to use the fake implementation of them, as the real one will not work locally. 

## Here is an example of how:
### Change this:
```
public class GetAnimalByIdentifierService(
    [FromKeyedServices(CattleApiClientKeys.Real)] ICattleApiClient cattleApi,
    ILogger<GetAnimalByIdentifierService> logger)
```
### To this:
```
public class GetAnimalByIdentifierService(
    [FromKeyedServices(CattleApiClientKeys.Fake)] ICattleApiClient cattleApi,
    ILogger<GetAnimalByIdentifierService> logger)
```

## Running Endpoints on DEV
If you wish to test against an environment, such as DEV, you will need to change the address to use the DEV address, as well as create a DEV API key on the CDP that will be added as the value for the x-api-key. The x-api-key is not needed for local endpoints.

## Current endpoints

Health Check (/health)

Get Animals by Identifier (/animals/{{identifier}})