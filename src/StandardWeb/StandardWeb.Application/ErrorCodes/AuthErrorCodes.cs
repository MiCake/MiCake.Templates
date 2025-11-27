namespace StandardWeb.Application.ErrorCodes;

/// <summary>
/// Authentication related error codes.
/// </summary>
public class AuthErrorCodes : BaseErrorCodes
{
    public const string InvalidCredentials = "1000";
    public const string UserAlreadyExists = "1001";
    public const string UserNotFound = "1002";
    public const string TokenGenerationFailed = "1003";
    public const string UserInvalidStatus = "1004";
    public const string InvalidOtpCode = "1005";
    public const string InvalidToken = "1006";
}