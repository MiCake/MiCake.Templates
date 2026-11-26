using Mapster;
using RBACWeb.Contracts.Dtos.Configuration;
using RBACWeb.Domain.Models.Configuration;

namespace RBACWeb.Application.Mapper;

public class ConfigurationMapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<AppSetting, AppSettingDto>()
            .Map(dest => dest.SettingGroup, src => src.SettingGroup.ToString())
            .Map(dest => dest.DataType, src => src.DataType.ToString());
    }
}
