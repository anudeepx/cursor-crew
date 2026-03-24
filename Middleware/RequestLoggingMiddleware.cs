using Microsoft.AspNetCore.Mvc.Controllers;

namespace RetailOrderingWebsite.Middleware;

public sealed class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        var actionDescriptor = endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>();

        if (actionDescriptor is null)
        {
            _logger.LogInformation("Handling {Method} {Path}", context.Request.Method, context.Request.Path);
        }
        else
        {
            _logger.LogInformation(
                "Handling {Method} {Path} -> {Controller}/{Action}",
                context.Request.Method,
                context.Request.Path,
                actionDescriptor.ControllerName,
                actionDescriptor.ActionName);
        }

        await _next(context);
    }
}
