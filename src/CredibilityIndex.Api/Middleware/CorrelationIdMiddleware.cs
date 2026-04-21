public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private const string CorrelationIdHeader = "X-Correlation-ID";

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Use the existing TraceIdentifier or a custom Guid
        var correlationId = context.TraceIdentifier;

        // Apply to Response Header
        context.Response.Headers.Append(CorrelationIdHeader, correlationId);

        // Push to Serilog LogContext so all logs in this request have this ID
        using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }
}