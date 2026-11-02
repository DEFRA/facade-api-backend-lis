using System.Net;
using LisFacadeApiBackend.Facade.LIS.Error;

namespace LisFacadeApiBackend.Facade.Endpoints;

public static class LisErrorResults
{
    public static IResult NotFound() => Create(
        StatusCodes.Status404NotFound, "Not found",
        "The requested resource was not found",
        "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.5");

    public static IResult Forbidden() => Create(
        StatusCodes.Status403Forbidden, "Forbidden",
        "The resource could not be returned as the requestor is not authorized",
        "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.4");

    public static IResult ServerError() => Create(
        StatusCodes.Status500InternalServerError, "Server Error",
        "The server encountered an unexpected error",
        "https://datatracker.ietf.org/doc/html/rfc9110#section-15.6.1");

    public static IResult ServiceUnavailable() => Create(
        StatusCodes.Status503ServiceUnavailable, "Service Unavailable",
        "The service is currently unavailable",
        "https://datatracker.ietf.org/doc/html/rfc9110#section-15.6.4");

    public static IResult FromUpstream(HttpStatusCode? status) => status switch
    {
        HttpStatusCode.NotFound => NotFound(),
        HttpStatusCode.Forbidden => Forbidden(),
        HttpStatusCode.ServiceUnavailable or HttpStatusCode.BadGateway or HttpStatusCode.GatewayTimeout => ServiceUnavailable(),
        _ => ServerError()
    };

    private static IResult Create(int status, string title, string detail, string type) =>
        TypedResults.Json(
            new LisErrors
            {
                new LisProblemDetails { Type = type, Status = status, Title = title, Detail = detail }
            },
            statusCode: status);
}
