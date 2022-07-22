using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

using Zixoan.Cuber.Server.Config;

namespace Zixoan.Cuber.Server.Web.Middleware;

public class HeaderApiKeyMiddleware
{
    private readonly RequestDelegate next;
    private readonly string headerName;
    private readonly string headerValue;

    public HeaderApiKeyMiddleware(
        RequestDelegate next,
        IOptions<CuberOptions> cuberOptions)
    {
        this.next = next;
        this.headerName = cuberOptions.Value.Web.ApiKeyHeaderName;
        this.headerValue = cuberOptions.Value.Web.ApiKey;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(this.headerName, out StringValues headerValues))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { Message = $"Request header {this.headerName} for api key not set" });
            return;
        }

        if (headerValues != this.headerValue)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { Message = $"Api key mismatch" });
            return;
        }

        await next.Invoke(context);
    }
}