using MiCake.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StandardWeb.Domain.Models.Identity;

namespace StandardWeb.Domain;

public class AppDbContext(DbContextOptions options) : MiCakeDbContext(options)
{
    #region  Identity Module

    public DbSet<User> User { get; set; }
    public DbSet<UserLoginHistory> UserLoginHistory { get; set; }
    public DbSet<ExternalLoginProvider> ExternalLoginProviders { get; set; }
    public DbSet<UserToken> UserTokens { get; set; }

    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(builder =>
        {
            builder.HasIndex(x => x.PhoneNumber).IsUnique();
        });

        modelBuilder.Entity<ExternalLoginProvider>(builder =>
        {
            builder.HasIndex(x => new { x.UserId, x.ProviderType, x.ProviderKey }).IsUnique();
            builder.HasIndex(x => x.ProviderKey);
            builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId);
        });

        modelBuilder.Entity<UserToken>(builder =>
        {
            builder.HasIndex(x => new { x.UserId, x.Type }).IsUnique();
            builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId);
        });

        modelBuilder.Entity<UserLoginHistory>(builder =>
        {
            builder.HasIndex(x => x.RecordedAt);
            builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId);
        });

        base.OnModelCreating(modelBuilder);
    }
}
