using MiCake.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MiCakeTemplate.EFCore
{
    public class AppDbContext : MiCakeDbContext
    {
        public AppDbContext(DbContextOptions options, IServiceProvider serviceProvider) : base(options, serviceProvider)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }
    }
}
