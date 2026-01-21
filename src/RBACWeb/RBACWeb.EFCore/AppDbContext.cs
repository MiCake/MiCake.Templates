using MiCake.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RBACWeb.Domain.Models.Authorization;
using RBACWeb.Domain.Models.Configuration;
using RBACWeb.Domain.Models.Identity;

namespace RBACWeb.EFCore;

public class AppDbContext(DbContextOptions options) : MiCakeDbContext(options)
{
    #region  Identity Module

    public DbSet<User> User { get; set; }

    #endregion

    #region Authorization Module

    public DbSet<Role> Roles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<Resource> Resources { get; set; }
    public DbSet<DataScope> DataScopes { get; set; }

    #endregion

    #region Configuration Module

    public DbSet<AppSetting> AppSettings { get; set; }

    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        #region Identity Module

        modelBuilder.Entity<User>(builder =>
        {
            // Configure ContactInfo value object
            builder.OwnsOne(u => u.Contact, contact =>
            {
                // Indexes for searching
                contact.HasIndex(c => c.PhoneNumber);
                contact.HasIndex(c => c.Email);
            });

            // Configure Password value object
            builder.OwnsOne(u => u.Credential);

            // Configure PersonalInfo value object
            builder.OwnsOne(u => u.Profile);
        });

        modelBuilder.Entity<ExternalLoginProvider>(builder =>
        {
            builder.HasIndex(x => new { x.UserId, x.ProviderType, x.ProviderKey }).IsUnique();
            builder.HasIndex(x => x.ProviderKey);
            builder.HasOne(x => x.User).WithMany(u => u.ExternalLogins).HasForeignKey(x => x.UserId);
        });

        modelBuilder.Entity<UserToken>(builder =>
        {
            builder.HasIndex(x => new { x.UserId, x.Type }).IsUnique();
            builder.HasOne(x => x.User).WithMany(u => u.UserTokens).HasForeignKey(x => x.UserId);
        });

        modelBuilder.Entity<UserLoginHistory>(builder =>
        {
            builder.HasIndex(x => x.CreatedAt);
            builder.HasOne(x => x.User).WithMany(u => u.LoginHistory).HasForeignKey(x => x.UserId);
        });

        modelBuilder.Entity<UserRole>(builder =>
        {
            builder.HasIndex(x => new { x.UserId, x.RoleId }).IsUnique();
            builder.HasOne(x => x.User).WithMany(u => u.UserRoles).HasForeignKey(x => x.UserId);
            builder.HasOne(x => x.Role).WithMany().HasForeignKey(x => x.RoleId);
        });

        #endregion

        #region Authorization Module

        modelBuilder.Entity<Role>(builder =>
        {
            builder.HasIndex(x => x.Code).IsUnique();
        });

        modelBuilder.Entity<Permission>(builder =>
        {
            builder.HasIndex(x => x.Code).IsUnique();
            builder.HasOne(x => x.Resource).WithMany(r => r.Permissions).HasForeignKey(x => x.ResourceId);
        });

        modelBuilder.Entity<Resource>(builder =>
        {
            builder.HasIndex(x => x.Code).IsUnique();
            builder.HasOne(x => x.Parent).WithMany(r => r.Children).HasForeignKey(x => x.ParentId);
        });

        modelBuilder.Entity<DataScope>(builder =>
        {
            builder.HasIndex(x => x.Code).IsUnique();
        });

        modelBuilder.Entity<RolePermission>(builder =>
        {
            builder.HasIndex(x => new { x.RoleId, x.PermissionId }).IsUnique();
            builder.HasOne(x => x.Role).WithMany(r => r.RolePermissions).HasForeignKey(x => x.RoleId);
            builder.HasOne(x => x.Permission).WithMany(p => p.RolePermissions).HasForeignKey(x => x.PermissionId);
        });

        modelBuilder.Entity<RoleDataScope>(builder =>
        {
            builder.HasIndex(x => new { x.RoleId, x.DataScopeId }).IsUnique();
            builder.HasOne(x => x.Role).WithMany(r => r.RoleDataScopes).HasForeignKey(x => x.RoleId);
            builder.HasOne(x => x.DataScope).WithMany(ds => ds.RoleDataScopes).HasForeignKey(x => x.DataScopeId);
        });

        #endregion

        #region Configuration Module

        modelBuilder.Entity<AppSetting>(builder =>
        {
            builder.HasKey(x => x.Id);

            // Unique constraint on SettingGroup + Key
            builder.HasIndex(x => new { x.SettingGroup, x.Key }).IsUnique();
            builder.HasIndex(x => x.SettingGroup);
            builder.HasIndex(x => x.UpdatedAt);
        });

        #endregion

        // Call base method to configure MiCake modules.
        base.OnModelCreating(modelBuilder);
    }
}
