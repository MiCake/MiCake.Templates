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
    public DbSet<UserLoginHistory> UserLoginHistory { get; set; }

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
            builder.HasIndex(x => x.PhoneNumber).IsUnique();
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
            builder.HasIndex(x => x.RecordedAt);
            builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId);
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
            builder.ToTable("Roles");
            builder.HasIndex(x => x.Code).IsUnique();
        });

        modelBuilder.Entity<Permission>(builder =>
        {
            builder.ToTable("Permissions");
            builder.HasIndex(x => x.Code).IsUnique();
            builder.HasOne(x => x.Resource).WithMany(r => r.Permissions).HasForeignKey(x => x.ResourceId);
        });

        modelBuilder.Entity<Resource>(builder =>
        {
            builder.ToTable("Resources");
            builder.HasIndex(x => x.Code).IsUnique();
            builder.HasOne(x => x.Parent).WithMany(r => r.Children).HasForeignKey(x => x.ParentId);
        });

        modelBuilder.Entity<DataScope>(builder =>
        {
            builder.ToTable("DataScopes");
            builder.HasIndex(x => x.Code).IsUnique();
        });

        modelBuilder.Entity<RolePermission>(builder =>
        {
            builder.ToTable("RolePermissions");
            builder.HasIndex(x => new { x.RoleId, x.PermissionId }).IsUnique();
            builder.HasOne(x => x.Role).WithMany(r => r.RolePermissions).HasForeignKey(x => x.RoleId);
            builder.HasOne(x => x.Permission).WithMany(p => p.RolePermissions).HasForeignKey(x => x.PermissionId);
        });

        modelBuilder.Entity<RoleDataScope>(builder =>
        {
            builder.ToTable("RoleDataScopes");
            builder.HasIndex(x => new { x.RoleId, x.DataScopeId }).IsUnique();
            builder.HasOne(x => x.Role).WithMany(r => r.RoleDataScopes).HasForeignKey(x => x.RoleId);
            builder.HasOne(x => x.DataScope).WithMany(ds => ds.RoleDataScopes).HasForeignKey(x => x.DataScopeId);
        });

        #endregion

        #region Configuration Module

        modelBuilder.Entity<AppSetting>(builder =>
        {
            builder.ToTable("AppSettings");
            builder.HasKey(x => x.Id);

            // Unique constraint on SettingGroup + Key
            builder.HasIndex(x => new { x.SettingGroup, x.Key }).IsUnique();
            builder.HasIndex(x => x.SettingGroup);
            builder.HasIndex(x => x.UpdatedAt);

            // Properties
            builder.Property(x => x.SettingGroup).IsRequired();
            builder.Property(x => x.Key).HasMaxLength(100).IsRequired();
            builder.Property(x => x.Value).IsRequired();
            builder.Property(x => x.DataType).IsRequired();
            builder.Property(x => x.Description).HasMaxLength(500);
            builder.Property(x => x.IsEncrypted).IsRequired();
            builder.Property(x => x.ValidationPattern).HasMaxLength(500);

            // Note: Audit fields (CreatedBy, CreatedAt, ModifiedBy, UpdatedAt) 
            // are auto-configured by MiCake framework
        });

        #endregion

        // Call base method to configure MiCake modules.
        base.OnModelCreating(modelBuilder);
    }
}
