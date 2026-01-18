using MiCake.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StandardWeb.Domain.Models.Configuration;
using StandardWeb.Domain.Models.Identity;

namespace StandardWeb.EFCore;

public class AppDbContext(DbContextOptions options) : MiCakeDbContext(options)
{
    #region  Identity Module

    public DbSet<User> User { get; set; }

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
            builder.HasIndex(x => x.RecordedAt);
            builder.HasOne(x => x.User).WithMany(u => u.LoginHistory).HasForeignKey(x => x.UserId);
        });

        #endregion

        #region Configuration Module

        modelBuilder.Entity<AppSetting>(builder =>
        {
            // Unique constraint on SettingGroup + Key
            builder.HasIndex(x => new { x.SettingGroup, x.Key }).IsUnique();
            builder.HasIndex(x => x.SettingGroup);
        });

        #endregion

        // Call base method to configure MiCake modules.
        base.OnModelCreating(modelBuilder);
    }
}
