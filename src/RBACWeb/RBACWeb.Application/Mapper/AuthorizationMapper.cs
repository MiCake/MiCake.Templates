using Mapster;
using RBACWeb.Contracts.Dtos.Authorization;
using RBACWeb.Domain.Models.Authorization;
using RBACWeb.Domain.Models.Identity;

namespace RBACWeb.Application.Mapper;

public class AuthorizationMapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // Role mappings
        config.NewConfig<Role, RoleDto>();
        config.NewConfig<Role, RoleDetailDto>()
            .Map(dest => dest.Permissions, src =>
                src.RolePermissions.Where(rp => rp.IsGranted).Select(rp => rp.Permission))
            .Map(dest => dest.DataScopes, src =>
                src.RoleDataScopes.Select(rds => rds.DataScope));

        // Permission mappings
        config.NewConfig<Permission, PermissionDto>()
            .Map(dest => dest.ResourceName, src => src.Resource != null ? src.Resource.Name : null);

        // Resource mappings
        config.NewConfig<Resource, ResourceDto>();
        config.NewConfig<Resource, ResourceTreeDto>()
            .Map(dest => dest.Children, src => src.Children);

        // DataScope mappings
        config.NewConfig<DataScope, DataScopeDto>();

        // UserRole mappings
        config.NewConfig<UserRole, UserRoleDto>()
            .Map(dest => dest.RoleName, src => src.Role.Name);
    }
}
