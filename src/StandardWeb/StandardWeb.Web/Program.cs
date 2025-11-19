using MiCake;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;
using CommonWebLib.StartUp;
using CommonWebLib.ServiceExtensions;
using StandardWeb.Domain;
using StandardWeb.Web;
using StandardWeb.Web.StartUp;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog for structured logging with support for Seq and other sinks
// Reads configuration from appsettings.json under "Serilog" section
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

try
{
    Log.Information("Starting web application");

    // Replace default ASP.NET Core logging with Serilog
    builder.Host.UseSerilog();

    // Register core API services: controllers, validation, AutoMapper, OpenAPI
    builder.Services.AddWebApiDefaults(builder.Configuration, typeof(Program).Assembly);

    // Configure MySQL with EF Core and auto-detect server version
    builder.Services.AddMySql<AppDbContext>(builder.Configuration.GetConnectionString("DefaultConnection"), ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection")));

    // Register HTTP clients with Polly retry policies
    builder.Services.RegisterHttpClients(builder.Configuration);

    // Initialize MiCake framework with application module and DbContext
    builder.Services.AddMiCakeWithDefault<AppModule, AppDbContext>().Build();

    // Configure JWT bearer authentication
    builder.Services.AddJWTAuthentication(builder.Configuration);

    // Configure CORS policy from appsettings
    builder.Services.AddCorsPolicy(builder.Configuration);

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        // Enable OpenAPI endpoint for Swagger/Scalar documentation
        app.MapOpenApi();
        app.MapScalarApiReference();
    }

    // Map controller endpoints
    app.MapControllers();

    // Apply CORS policy before authentication
    app.UseCors("CorsPolicy");

    // Enable authentication and authorization middleware
    app.UseAuthentication();
    app.UseAuthorization();

    // Start MiCake framework services
    app.StartMiCake();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    // Ensure all buffered log events are written before shutdown
    Log.CloseAndFlush();
}