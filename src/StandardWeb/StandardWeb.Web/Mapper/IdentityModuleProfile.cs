using AutoMapper;
using StandardWeb.Application.Models;
using StandardWeb.Domain.Models.Identity;
using StandardWeb.Web.Dtos.Identity;

namespace StandardWeb.Web.Mapper;

public class IdentityModuleProfile : Profile
{
    public IdentityModuleProfile()
    {
        CreateMap<UserLoginResult, LoginResponseDto>();
        CreateMap<User, UserDto>();
    }
}
