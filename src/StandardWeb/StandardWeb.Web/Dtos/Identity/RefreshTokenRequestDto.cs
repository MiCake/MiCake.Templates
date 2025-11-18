using FluentValidation;

namespace StandardWeb.Web.Dtos.Identity;

public class RefreshTokenRequestDto
{
    public string RefreshToken { get; set; } = null!;
}

#region Validators

public class RefreshTokenRequestDtoValidator : AbstractValidator<RefreshTokenRequestDto>
{
    public RefreshTokenRequestDtoValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty()
            .WithMessage("Refresh token cannot be empty.");
    }
}

#endregion
