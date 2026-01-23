using EfCoreMigrationApp.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using RBACWeb.EFCore;
using EfCoreMigrationApp.Services.SeedServices;

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
        services.AddScoped<SeedOrchestrator>();

        // Register all seed services here:
        services.AddScoped<ISeedService, AppSettingSeedService>();
    })
    .Build();

// Run the migration and seed
using (var scope = host.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        Console.WriteLine("=".PadRight(60, '='));
        Console.WriteLine("EF Core Migration & Seed Application");
        Console.WriteLine("=".PadRight(60, '='));
        Console.WriteLine();

        // Step 1: Run migrations
        Console.WriteLine("Step 1: Running database migrations...");
        var migrationService = services.GetRequiredService<MigrationService>();
        await migrationService.MigrateAsync();
        Console.WriteLine("✓ Migrations completed successfully!");
        Console.WriteLine();

        // Step 2: Run seed services
        Console.WriteLine("Step 2: Running data seed services...");
        var seedOrchestrator = services.GetRequiredService<SeedOrchestrator>();
        await seedOrchestrator.ExecuteAllAsync();
        Console.WriteLine("✓ Seed services completed successfully!");
        Console.WriteLine();

        Console.WriteLine("=".PadRight(60, '='));
        Console.WriteLine("All operations completed successfully!");
        Console.WriteLine("=".PadRight(60, '='));
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred during migration or seeding.");
        Console.WriteLine();
        Console.WriteLine("×".PadRight(60, '×'));
        Console.WriteLine($"Operation failed: {ex.Message}");
        Console.WriteLine("×".PadRight(60, '×'));
        Environment.Exit(1);
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