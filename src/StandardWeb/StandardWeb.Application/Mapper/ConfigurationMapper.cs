using AutoMapper;
using StandardWeb.Contracts.Dtos.Configuration;
using StandardWeb.Domain.Models.Configuration;

namespace StandardWeb.Application.Mapper;

public class ConfigurationMapper : Profile
{
    public ConfigurationMapper()
    {
        CreateMap<AppSetting, AppSettingDto>()
            .ForMember(dest => dest.SettingGroup, opt => opt.MapFrom(src => src.SettingGroup.ToString()))
            .ForMember(dest => dest.DataType, opt => opt.MapFrom(src => src.DataType.ToString()));
    }
}
