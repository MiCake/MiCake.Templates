using EfCoreMigrationApp.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using StandardWeb.Domain;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((hostContext, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: false);
        config.AddEnvironmentVariables();
    })
    .ConfigureServices((hostContext, services) =>
    {
        var configuration = hostContext.Configuration;

        // Register DbContext with MariaDB
        services.AddDbContext<AppDbContext>(options =>
           {
               options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOptions => sqlOptions.MigrationsAssembly(typeof(MigrationService).Assembly.FullName));

               options.EnableSensitiveDataLogging();
               options.LogTo(Console.WriteLine, LogLevel.Information);
           });

        // Register services
        services.AddScoped<MigrationService>();
    })
    .Build();

// Run the migration
using (var scope = host.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        Console.WriteLine("Starting EF Core MariaDB Migration Application...");

        var migrationService = services.GetRequiredService<MigrationService>();
        await migrationService.MigrateAsync();

        Console.WriteLine("Migration completed successfully!");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred during migration.");
        Console.WriteLine($"Migration failed: {ex.Message}");
    }
}

// To create a migration, run:
// dotnet ef migrations add InitialCreate --project EfCoreMigrationApp
//
// To apply migrations:
// dotnet ef database update --project EfCoreMigrationApp

// to apply rollback:
// dotnet ef database update LastGoodMigration --project EfCoreMigrationApp
// to remove the last migration:
// dotnet ef migrations remove --project EfCoreMigrationApp

// dotnet ef database update 0     -- put 0 means rollback all migrations