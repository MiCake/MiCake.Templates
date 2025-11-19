namespace StandardWeb.Common;

/// <summary>
/// Represents the result of an operation without returning data.
/// Used to indicate success or failure with optional error details.
/// </summary>
public class OperationResult
{
    /// <summary>
    /// Indicates whether the operation succeeded.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Descriptive error message when operation fails.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Machine-readable error code for client handling.
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// Creates a successful operation result.
    /// </summary>
    public static OperationResult Success()
    {
        return new OperationResult
        {
            IsSuccess = true
        };
    }

    /// <summary>
    /// Creates a failed operation result with error details.
    /// </summary>
    /// <param name="errorMessage">Human-readable error description</param>
    /// <param name="errorCode">Optional error code for categorization</param>
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
/// Represents the result of an operation that returns data.
/// Combines success/failure status with returned data.
/// </summary>
/// <typeparam name="T">Type of data returned on success</typeparam>
public class OperationResult<T>
{
    /// <summary>
    /// Indicates whether the operation succeeded.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Descriptive error message when operation fails.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Machine-readable error code for client handling.
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// Data returned on successful operation.
    /// </summary>
    public T Data { get; set; } = default!;

    /// <summary>
    /// Creates a successful operation result with data.
    /// </summary>
    /// <param name="data">The data to return</param>
    public static OperationResult<T> Success(T data)
    {
        return new OperationResult<T>
        {
            IsSuccess = true,
            Data = data
        };
    }

    /// <summary>
    /// Creates a failed operation result with error details.
    /// </summary>
    /// <param name="errorMessage">Human-readable error description</param>
    /// <param name="errorCode">Optional error code for categorization</param>
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