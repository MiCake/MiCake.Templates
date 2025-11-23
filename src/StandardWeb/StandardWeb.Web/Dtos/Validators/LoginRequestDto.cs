using FluentValidation;
using StandardWeb.Common.Helpers;
using StandardWeb.Contracts.Dtos.Identity;

namespace StandardWeb.Web.Dtos.Validators;

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