using AutoMapper;
using RBACWeb.Contracts.Dtos.Identity;
using RBACWeb.Domain.Models.Identity;

namespace RBACWeb.Application.Mapper;

public class IdentityMapper : Profile
{
    public IdentityMapper()
    {
        CreateMap<User, UserDto>();
    }
}
