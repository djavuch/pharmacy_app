namespace PharmacyApp.Domain.Common;

public class Result<T>
{
    public T? Value { get; }
    public bool IsSuccess { get; }
    public string Message { get; }
    public ErrorType ErrorType { get; }

    private Result(T value)
    {
        Value = value;
        IsSuccess = true;
        Message = string.Empty;
        ErrorType = ErrorType.None;
    }

    private Result(T value, string message)
    {
        Value = value;
        IsSuccess = true;
        Message = message;
        ErrorType = ErrorType.None;
    }

    private Result(string message, ErrorType errorType)
    {
        IsSuccess = false;
        Message = message;
        ErrorType = errorType;
        Value = default;
    }

    public static Result<T> Success(T value) => new(value);
    public static Result<T> Success(T value, string message) => new(value, message);

    public static Result<T> Failure(string message, ErrorType errorType = ErrorType.Unexpected)
        => new(message, errorType);

    public static Result<T> NotFound(string message) => new(message, ErrorType.NotFound);
    public static Result<T> BadRequest(string message) => new(message, ErrorType.Validation);
    public static Result<T> Conflict(string message) => new(message, ErrorType.Conflict);
    public static Result<T> Unauthorized(string message) => new(message, ErrorType.Unauthorized);
    public static Result<T> Forbidden(string message) => new(message, ErrorType.Forbidden);
}

public class Result
{
    public bool IsSuccess { get; }
    public string Message { get; }
    public ErrorType ErrorType { get; }

    private Result(bool isSuccess, string message = "", ErrorType errorType = ErrorType.None)
    {
        IsSuccess = isSuccess;
        Message = message;
        ErrorType = errorType;
    }

    public static Result Success() => new(true);

    public static Result Failure(string message, ErrorType errorType = ErrorType.Unexpected)
        => new(false, message, errorType);

    public static Result NotFound(string message) => new(false, message, ErrorType.NotFound);
    public static Result BadRequest(string message) => new(false, message, ErrorType.Validation);
    public static Result Conflict(string message) => new(false, message, ErrorType.Conflict);
    public static Result Unauthorized(string message) => new(false, message, ErrorType.Unauthorized);
    public static Result Forbidden(string message) => new(false, message, ErrorType.Forbidden);
}
