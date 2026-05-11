using AutoMapper;
using StandardWeb.Contracts.Dtos.Identity;
using StandardWeb.Domain.Models.Identity;

namespace StandardWeb.Application.Mapper;

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
