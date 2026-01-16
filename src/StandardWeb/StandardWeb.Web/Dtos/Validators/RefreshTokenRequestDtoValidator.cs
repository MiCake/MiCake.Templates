using FluentValidation;
using StandardWeb.Web.Dtos.Identity;

namespace StandardWeb.Web.Dtos.Validators;

public class RefreshTokenRequestDtoValidator : AbstractValidator<RefreshTokenRequestDto>
{
    public RefreshTokenRequestDtoValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty()
            .WithMessage("Refresh token cannot be empty.");
    }
}