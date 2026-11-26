using Mapster;
using StandardWeb.Contracts.Dtos.Identity;
using StandardWeb.Domain.Models.Identity;

namespace StandardWeb.Application.Mapper;

public class IdentityMapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<User, UserDto>()
            .Map(dest => dest.FirstName, src => src.Profile.FirstName)
            .Map(dest => dest.LastName, src => src.Profile.LastName)
            .Map(dest => dest.DisplayName, src => src.Profile.DisplayName);
    }
}
