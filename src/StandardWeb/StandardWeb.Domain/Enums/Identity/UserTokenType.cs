namespace StandardWeb.Domain.Enums.Identity;

public enum UserTokenType
{
    ResetPassword = 1,
    EmailVerification = 2,
    PhoneVerification = 3,
    TwoFactorAuthentication = 4,
    JwtRefreshToken = 5,
}
