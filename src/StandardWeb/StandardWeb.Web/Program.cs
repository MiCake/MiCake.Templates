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

// Configure Serilog to read from appsettings.json (including Seq)
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

try
{
    Log.Information("Starting web application");

    builder.Host.UseSerilog();

    builder.Services.AddWebApiDefaults(builder.Configuration, typeof(Program).Assembly);

    builder.Services.AddMySql<AppDbContext>(builder.Configuration.GetConnectionString("DefaultConnection"), ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection")));

    builder.Services.RegisterHttpClients(builder.Configuration);

    builder.Services.AddMiCakeWithDefault<AppModule, AppDbContext>().Build();

    builder.Services.AddJWTAuthentication(builder.Configuration);
    builder.Services.AddCorsPolicy(builder.Configuration);

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference();
    }
    app.MapControllers();

    app.UseCors("CorsPolicy");
    app.UseAuthentication();
    app.UseAuthorization();

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