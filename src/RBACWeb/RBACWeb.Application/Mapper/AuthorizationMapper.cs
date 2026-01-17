using AutoMapper;
using RBACWeb.Contracts.Dtos.Authorization;
using RBACWeb.Domain.Models.Authorization;
using RBACWeb.Domain.Models.Identity;

namespace RBACWeb.Application.Mapper;

public class AuthorizationMapper : Profile
{
    public AuthorizationMapper()
    {
        // Role mappings
        CreateMap<Role, RoleDto>();
        CreateMap<Role, RoleDetailDto>()
            .ForMember(dest => dest.Permissions, opt => opt.MapFrom(src =>
                src.RolePermissions.Where(rp => rp.IsGranted).Select(rp => rp.Permission)))
            .ForMember(dest => dest.DataScopes, opt => opt.MapFrom(src =>
                src.RoleDataScopes.Select(rds => rds.DataScope)));

        // Permission mappings
        CreateMap<Permission, PermissionDto>()
            .ForMember(dest => dest.ResourceName, opt => opt.MapFrom(src => src.Resource.Name));

        // Resource mappings
        CreateMap<Resource, ResourceDto>();
        CreateMap<Resource, ResourceTreeDto>()
            .ForMember(dest => dest.Children, opt => opt.MapFrom(src => src.Children));

        // DataScope mappings
        CreateMap<DataScope, DataScopeDto>();

        // UserRole mappings
        CreateMap<UserRole, UserRoleDto>()
            .ForMember(dest => dest.RoleCode, opt => opt.MapFrom(src => src.Role.Code))
            .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role.Name));
    }
}
