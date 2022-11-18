using MiCake.EntityFrameworkCore;
using MiCakeTemplate.Domain.AuthContext;
using MiCakeTemplate.Domain.TodoContext;
using MiCakeTemplate.EFCore.ModelConfigurations;
using Microsoft.EntityFrameworkCore;

namespace MiCakeTemplate.EFCore
{
    public class AppDbContext : MiCakeDbContext
    {
        public DbSet<User> User { get; set; }

        public DbSet<TodoItem> TodoItem { get; set; }

#pragma warning disable CS8618
        public AppDbContext(DbContextOptions options, IServiceProvider serviceProvider) : base(options, serviceProvider)
#pragma warning restore CS8618
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new UserConfiguration());
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }
    }
}
