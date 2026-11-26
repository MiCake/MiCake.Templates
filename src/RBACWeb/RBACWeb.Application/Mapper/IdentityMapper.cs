using Mapster;
using RBACWeb.Contracts.Dtos.Identity;
using RBACWeb.Domain.Models.Identity;

namespace RBACWeb.Application.Mapper;

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
