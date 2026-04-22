using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PharmacyApp.Domain.Exceptions;
using static PharmacyApp.Domain.Exceptions.AppExceptions;

namespace PharmacyApp.Presentation.Exceptions;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        httpContext.Response.ContentType = "application/json";

        object body;

        switch (exception)
        {
            case FluentValidationException validationEx:
                httpContext.Response.StatusCode = validationEx.ErrorType.ToStatusCode();
                body = new
                {
                    code = validationEx.ErrorMessage,
                    message = validationEx.Message,
                    errors = validationEx.Errors,
                    traceId = httpContext.TraceIdentifier
                };
                break;

            case AppExceptions appEx:
                httpContext.Response.StatusCode = appEx.ErrorType.ToStatusCode();
                body = new
                {
                    code = appEx.ErrorMessage,
                    message = appEx.Message,
                    traceId = httpContext.TraceIdentifier
                };
                break;

            default:
                _logger.LogError(exception, "Unhandled exception");
                httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                body = new
                {
                    code = "INTERNAL_SERVER_ERROR",
                    message = "An unexpected error occurred.",
                    traceId = httpContext.TraceIdentifier
                };
                break;
        }

        await httpContext.Response.WriteAsync(
            JsonSerializer.Serialize(body),
            cancellationToken);

        return true;
    }
}
