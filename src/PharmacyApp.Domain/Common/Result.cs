namespace PharmacyApp.Domain.Common;

public class Result<T>
{
    public T? Value { get; }
    public bool IsSuccess { get; }
    public string Message { get; }
    public int ErrorCode { get; }

    private Result(T value)
    {
        Value = value;
        IsSuccess = true; 
        Message = string.Empty;
        ErrorCode = 0;
    }
    
    private Result(T value, string message) 
    {
        Value = value;
        IsSuccess = true;
        Message = message;  
        ErrorCode = 0;
    }

    private Result(string message, int errorCode)
    {
        IsSuccess = false;
        Message = message;
        ErrorCode = errorCode;
        Value = default;
    }
    
    public static Result<T> Success(T value) => new (value);
    public static Result<T> Success(T value, string message) => new(value, message);
    public static Result<T> Failure(string message, int errorCode ) => new (message, errorCode);
    
    public static Result<T> NotFound(string message) => new(message, 404);
    public static Result<T> BadRequest(string message) => new(message, 400);
    public static Result<T> Conflict(string message) => new(message, 409);
    public static Result<T> Unauthorized(string message) => new(message, 401);
    public static Result<T> Forbidden(string message) => new(message, 403);
}

public class Result
{
    public bool IsSuccess { get; }
    public string Message { get; }
    public int ErrorCode { get; }

    private Result(bool isSuccess, string message = "", int errorCode = 0)
    {
        IsSuccess = isSuccess;
        Message = message;
        ErrorCode = errorCode;
    }

    public static Result Success() => new(true);
    public static Result Failure(string message, int errorCode) => new(false, message, errorCode);

    public static Result NotFound(string message) => new(false, message, 404);
    public static Result BadRequest(string message) => new(false, message, 400);
    public static Result Conflict(string message) => new(false, message, 409);
    public static Result Unauthorized(string message) => new(false, message, 401);
    public static Result Forbidden(string message) => new(false, message, 403);
}