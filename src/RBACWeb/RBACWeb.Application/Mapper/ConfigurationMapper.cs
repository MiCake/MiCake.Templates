using AutoMapper;
using RBACWeb.Contracts.Dtos.Configuration;
using RBACWeb.Domain.Models.Configuration;

namespace RBACWeb.Application.Mapper;

public class ConfigurationMapper : Profile
{
    public ConfigurationMapper()
    {
        CreateMap<AppSetting, AppSettingDto>()
            .ForMember(dest => dest.SettingGroup, opt => opt.MapFrom(src => src.SettingGroup.ToString()))
            .ForMember(dest => dest.DataType, opt => opt.MapFrom(src => src.DataType.ToString()));
    }
}
