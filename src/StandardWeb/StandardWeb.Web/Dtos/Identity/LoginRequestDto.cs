using FluentValidation;
using StandardWeb.Common.Helpers;

namespace StandardWeb.Web.Dtos.Identity;

public class LoginRequestDto
{
    public string? PhoneNumber { get; set; }
    public string? Password { get; set; }
    public string? OtpCode { get; set; }
}

public class LoginResponseDto
{
    public UserDto User { get; set; } = null!;
    public bool NeedOtpVerification { get; set; }
    public string Token { get; set; } = null!;
    public DateTime Expiration { get; set; }

    public string RefreshToken { get; set; } = null!;
    public DateTime RefreshTokenExpiration { get; set; }
}


#region Validators

public class LoginRequestDtoValidator : AbstractValidator<LoginRequestDto>
{
    public LoginRequestDtoValidator()
    {
        RuleFor(x => x.PhoneNumber).NotEmpty().WithMessage("请填写手机号码.");
        RuleFor(x => x.PhoneNumber).Must(FormatChecker.IsValidPhoneNumber).WithMessage("手机号码格式不正确.");
        RuleFor(x => x.Password).NotEmpty().WithMessage("请填写密码.");
    }
}

#endregion