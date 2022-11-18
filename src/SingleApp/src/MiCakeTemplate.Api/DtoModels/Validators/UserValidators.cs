using FluentValidation;
using MiCakeTemplate.Api.DtoModels.UserContext;

namespace MiCakeTemplate.Api.DtoModels.Validators
{
    public class CreateUserDtoValidators : AbstractValidator<CreateUserDto>
    {
        public CreateUserDtoValidators()
        {
            RuleFor(s => s.UserName).NotEmpty().WithMessage("用户名不能为空");
            RuleFor(s => s.Password).NotEmpty().WithMessage("密码不能为空");
        }
    }

    public class LoginDtoValidators : AbstractValidator<LoginDto>
    {
        public LoginDtoValidators()
        {
            RuleFor(s => s.UserName).NotEmpty().WithMessage("用户名不能为空");
            RuleFor(s => s.Password).NotEmpty().WithMessage("密码不能为空");
        }
    }
}
