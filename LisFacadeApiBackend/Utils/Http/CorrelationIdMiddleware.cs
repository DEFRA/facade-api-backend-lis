namespace LisFacadeApiBackend.Utils.Http;

public sealed class CorrelationIdMiddleware(RequestDelegate next, string headerName)
{
    public Task InvokeAsync(HttpContext context)
    {
        if (string.IsNullOrWhiteSpace(context.Request.Headers[headerName].ToString()))
        {
            context.Request.Headers[headerName] = Guid.NewGuid().ToString();
        }

        return next(context);
    }
}
