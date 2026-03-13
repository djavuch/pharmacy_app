namespace PharmacyApp.Domain.Exceptions;

public class ValidationException : AppExceptions
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(IDictionary<string, string[]> errors)
        : base("One or more validation errors occurred.", 400, "VALIDATION_ERROR")
    {
        Errors = errors;
    }

    public ValidationException(string propertyName, string errorMessage)
        : base("Validation failed.", 400, "VALIDATION_ERROR")
    {
        Errors = new Dictionary<string, string[]>
        {
            { propertyName, new[] { errorMessage } }
        };
    }
}