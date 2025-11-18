namespace StandardWeb.Common;

/// <summary>
/// Non-generic operation result for operations that don't return data
/// </summary>
public class OperationResult
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ErrorCode { get; set; }

    public static OperationResult Success()
    {
        return new OperationResult
        {
            IsSuccess = true
        };
    }

    public static OperationResult Failure(string errorMessage, string? errorCode = null)
    {
        return new OperationResult
        {
            IsSuccess = false,
            ErrorMessage = errorMessage,
            ErrorCode = errorCode
        };
    }
}

/// <summary>
/// Generic operation result for operations that return data
/// </summary>
/// <typeparam name="T">Type of data returned</typeparam>
public class OperationResult<T>
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ErrorCode { get; set; }
    public T Data { get; set; } = default!;

    public static OperationResult<T> Success(T data)
    {
        return new OperationResult<T>
        {
            IsSuccess = true,
            Data = data
        };
    }

    public static OperationResult<T> Failure(string errorMessage, string? errorCode = null)
    {
        return new OperationResult<T>
        {
            IsSuccess = false,
            ErrorMessage = errorMessage,
            ErrorCode = errorCode
        };
    }
}