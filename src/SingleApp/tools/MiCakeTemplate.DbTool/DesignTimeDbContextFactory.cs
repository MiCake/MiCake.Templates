using MiCake;
using MiCake.Core.Modularity;
using MiCakeTemplate.EFCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;

namespace MiCakeTemplate.DbTool
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var testConStr = "< your db connection string is here >";

            var services = new ServiceCollection();
            services.AddLogging();
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseNpgsql(testConStr, x => x.MigrationsAssembly(typeof(DesignTimeDbContextFactory).Assembly.FullName));
            });
            // Add MiCake
            services.AddMiCakeServices<EmptyMiCakeModule, AppDbContext>().Build();

            return services.BuildServiceProvider().GetService<AppDbContext>()!;
        }
    }

    class EmptyMiCakeModule : MiCakeModule { }
}