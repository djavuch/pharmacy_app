namespace PharmacyApp.Domain.Exceptions;

public abstract class AppExceptions : Exception
{
    public int StatusCode { get; }
    public string ErrorMessage { get; }

    protected AppExceptions(string message, int statusCode, string errorMessage) : base(message)
    {
        StatusCode = statusCode;
        ErrorMessage = errorMessage;
    }

    public class NotFoundException : AppExceptions
    {
        public NotFoundException(string message)
            : base(message, 404, "NOT_FOUND")
        {
        }

        public NotFoundException(string entity, object key)
            : base($"{entity} with id '{key}' was not found.", 404, "NOT_FOUND")
        {
        }
    }

    public class ConflictException : AppExceptions
    {
        public ConflictException(string message)
            : base(message, 409, "CONFLICT")
        {
        }
    }

    public class BadRequestException : AppExceptions
    {
        public BadRequestException(string message)
            : base(message, 400, "BAD_REQUEST")
        {
        }
    }

    public class UnauthorizedException : AppExceptions
    {
        public UnauthorizedException(string message)
            : base(message, 401, "UNAUTHORIZED")
        {
        }
    }

    public class ForbiddenException : AppExceptions
    {
        public ForbiddenException(string message)
            : base(message, 403, "FORBIDDEN")
        {
        }
    }
}