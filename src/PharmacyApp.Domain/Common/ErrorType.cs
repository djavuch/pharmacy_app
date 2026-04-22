namespace PharmacyApp.Domain.Common;

public enum ErrorType
{
    None = 0,
    Validation = 1,
    NotFound = 2,
    Conflict = 3,
    Unauthorized = 4,
    Forbidden = 5,
    UnprocessableEntity = 6,
    Unexpected = 7
}
