using Microsoft.AspNetCore.Http;
using PharmacyApp.Domain.Common;

namespace PharmacyApp.Presentation.Extensions;

public static class ErrorTypeHttpExtensions
{
    public static int ToStatusCode(this ErrorType errorType)
    {
        return errorType switch
        {
            ErrorType.None => StatusCodes.Status200OK,
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ErrorType.UnprocessableEntity => StatusCodes.Status422UnprocessableEntity,
            _ => StatusCodes.Status500InternalServerError
        };
    }
}
