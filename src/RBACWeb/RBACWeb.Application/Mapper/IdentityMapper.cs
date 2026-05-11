using AutoMapper;
using RBACWeb.Contracts.Dtos.Identity;
using RBACWeb.Domain.Models.Identity;

namespace RBACWeb.Application.Mapper;

public class IdentityMapper : Profile
{
    public IdentityMapper()
    {
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.Profile.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.Profile.LastName))
            .ForMember(dest => dest.DisplayName, opt => opt.MapFrom(src => src.Profile.DisplayName));
    }
}
