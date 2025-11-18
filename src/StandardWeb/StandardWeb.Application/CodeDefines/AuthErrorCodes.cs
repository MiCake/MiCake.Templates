namespace StandardWeb.Application.CodeDefines;

public class AuthErrorCodes : BaseErrorCodes
{
    public const string InvalidCredentials = "1000";
    public const string UserAlreadyExists = "1001";
    public const string UserNotFound = "1002";
    public const string TokenGenerationFailed = "1003";
    public const string UserInvalidStatus = "1004";
    public const string InvalidOtpCode = "1005";
    public const string InvalidToken = "1006";

    // WeChat login related error codes
    public const string WeChatCodeInvalid = "1100";
    public const string WeChatApiError = "1101";
    public const string WeChatAccountAlreadyBound = "1102";
    public const string ExternalLoginNotFound = "1103";
    public const string CannotUnbindOnlyLogin = "1104";
}