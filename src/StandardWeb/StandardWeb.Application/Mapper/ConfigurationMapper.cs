using Mapster;
using StandardWeb.Contracts.Dtos.Configuration;
using StandardWeb.Domain.Models.Configuration;

namespace StandardWeb.Application.Mapper;

public class ConfigurationMapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<AppSetting, AppSettingDto>()
            .Map(dest => dest.SettingGroup, src => src.SettingGroup.ToString())
            .Map(dest => dest.DataType, src => src.DataType.ToString());
    }
}
