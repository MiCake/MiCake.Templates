namespace StandardWeb.Application.ErrorCodes;

/// <summary>
/// Base error codes used across the application.
/// </summary>
public abstract class BaseErrorCodes
{
    public const string InvalidInput = "9900";
    public const string InvalidOperation = "9901";
    public const string NotFound = "9902";

    
    public const string UnexpectedError = "9998";
    public const string InternalError = "9999";
}