using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;
using StandardWeb.Domain;
using StandardWeb.Web;
using StandardWeb.Web.StartUp;
using MiCake.Core;
using StandardWeb.Common.Time;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

try
{
    Log.Information("Starting web application");
    builder.Host.UseSerilog();

    // Register core API services: controllers, validation, AutoMapper, OpenAPI
    builder.Services.AddWebApiDefaults(builder.Configuration);
    builder.Services.AddNpgsql<AppDbContext>(builder.Configuration.GetConnectionString("DefaultConnection"));

    // Initialize MiCake framework with application module and DbContext
    builder.Services.AddMiCakeWithDefault<WebModule, AppDbContext>(
        opt =>
        {
            opt.AuditConfig = audit =>
            {
                audit.AuditTimeProvider = () => TimeNow.Now;
            };
        }
    ).Build();

    builder.Services.RegisterHttpClients(builder.Configuration);
    builder.Services.AddJWTAuthentication(builder.Configuration);
    builder.Services.AddCorsPolicy(builder.Configuration);
    builder.Services.AddDistributedMemoryCache();   // use memory cache as the IDistributedCache implementation

    var app = builder.Build();
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference(opt =>
        {
            opt.AddPreferredSecuritySchemes("BearerAuth")
            .AddHttpAuthentication("BearerAuth", scheme => scheme.Token = "");
        });
    }

    app.MapControllers();
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
    Log.CloseAndFlush();
}