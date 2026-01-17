using MiCake.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StandardWeb.Domain.Models.Configuration;
using StandardWeb.Domain.Models.Identity;

namespace StandardWeb.EFCore;

public class AppDbContext(DbContextOptions options) : MiCakeDbContext(options)
{
    #region  Identity Module

    public DbSet<User> User { get; set; }
    public DbSet<UserLoginHistory> UserLoginHistory { get; set; }

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
