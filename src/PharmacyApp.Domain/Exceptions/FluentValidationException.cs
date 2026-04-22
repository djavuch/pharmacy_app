using PharmacyApp.Domain.Common;

namespace PharmacyApp.Domain.Exceptions;

public class FluentValidationException : AppExceptions
{
    public IDictionary<string, string[]> Errors { get; }

    public FluentValidationException(IDictionary<string, string[]> errors)
        : base("One or more validation errors occurred.", ErrorType.Validation, "VALIDATION_ERROR")
    {
        Errors = errors;
    }

    public FluentValidationException(string propertyName, string errorMessage)
        : base("Validation failed.", ErrorType.Validation, "VALIDATION_ERROR")
    {
        Errors = new Dictionary<string, string[]>
        {
            { propertyName, [errorMessage] }
        };
    }
}
