namespace Infrastructure.Middleware;
public class CsrfHeaderMiddleware
{
    private static readonly HashSet<string> SafeMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        "GET", "HEAD", "OPTIONS", "TRACE"
    };

    private readonly RequestDelegate _next;

    public CsrfHeaderMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        if (!SafeMethods.Contains(context.Request.Method)
            && context.Request.Cookies.ContainsKey("AuthToken")
            && !context.Request.Headers.ContainsKey("X-Requested-With"))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("{\"Message\":\"طلب غير صالح\"}");
            return;
        }

        await _next(context);
    }
}
