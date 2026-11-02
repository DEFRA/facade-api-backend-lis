# Postman Collection Explanation

This is a collection of the current endpoints for the lis-facade-api-backend

The included endpoints are set to call to your local system while running the project, if you wish to test against an environment, such as DEV, you will need to change the address to use the DEV address, as well as create a DEV API key on the CDP that will be added as the value for the x-api-key. The x-api-key is not needed for local endpoints.

## Current endpoints

Health Check (/health)

Get Animals by Identifier (/animals/{{identifier}})