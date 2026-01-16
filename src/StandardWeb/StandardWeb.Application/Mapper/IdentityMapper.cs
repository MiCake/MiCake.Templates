using AutoMapper;
using StandardWeb.Contracts.Dtos.Identity;
using StandardWeb.Domain.Models.Identity;

namespace StandardWeb.Application.Mapper;

public class IdentityMapper : Profile
{
    public IdentityMapper()
    {
        CreateMap<User, UserDto>();
    }
}
