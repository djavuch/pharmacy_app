using PharmacyApp.Domain.Common;

namespace PharmacyApp.Domain.Exceptions;

public abstract class AppExceptions : Exception
{
    public ErrorType ErrorType { get; }
    public string ErrorMessage { get; }

    protected AppExceptions(string message, ErrorType errorType, string errorMessage) : base(message)
    {
        ErrorType = errorType;
        ErrorMessage = errorMessage;
    }

    public class NotFoundException : AppExceptions
    {
        public NotFoundException(string message)
            : base(message, ErrorType.NotFound, "NOT_FOUND")
        {
        }

        public NotFoundException(string entity, object key)
            : base($"{entity} with id '{key}' was not found.", ErrorType.NotFound, "NOT_FOUND")
        {
        }
    }

    public class ConflictException : AppExceptions
    {
        public ConflictException(string message)
            : base(message, ErrorType.Conflict, "CONFLICT")
        {
        }
    }

    public class BadRequestException : AppExceptions
    {
        public BadRequestException(string message)
            : base(message, ErrorType.Validation, "BAD_REQUEST")
        {
        }
    }

    public class UnauthorizedException : AppExceptions
    {
        public UnauthorizedException(string message)
            : base(message, ErrorType.Unauthorized, "UNAUTHORIZED")
        {
        }
    }

    public sealed class AppException : AppExceptions
    {
        public AppException(string message)
            : base(message, ErrorType.UnprocessableEntity, "UNPROCESSABLE_ENTITY")
        {
        }
    }

    public class ForbiddenException : AppExceptions
    {
        public ForbiddenException(string message)
            : base(message, ErrorType.Forbidden, "FORBIDDEN")
        {
        }
    }
}
