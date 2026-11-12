namespace LisFacadeApiBackend.Utils.Http;

public sealed class CorrelationIdMiddleware(RequestDelegate next, string headerName)
{
    public Task InvokeAsync(HttpContext context)
    {
        var values = context.Request.Headers[headerName];
        if (values.Count != 1 || string.IsNullOrWhiteSpace(values[0]))
        {
            context.Request.Headers[headerName] = Guid.NewGuid().ToString();
        }

        return next(context);
    }
}
