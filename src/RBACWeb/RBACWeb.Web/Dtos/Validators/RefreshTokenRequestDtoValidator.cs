using FluentValidation;
using RBACWeb.Web.Dtos.Identity;

namespace RBACWeb.Web.Dtos.Validators;

public class RefreshTokenRequestDtoValidator : AbstractValidator<RefreshTokenRequestDto>
{
    public RefreshTokenRequestDtoValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty()
            .WithMessage("Refresh token cannot be empty.");
    }
}