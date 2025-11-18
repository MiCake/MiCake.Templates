using MiCake;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;
using CommonWebLib.StartUp;
using CommonWebLib.ServiceExtensions;
using StandardWeb.Domain;
using StandardWeb.Web;
using StandardWeb.Web.StartUp;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;

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

    builder.Services.AddControllers();
    builder.Services.Configure<ApiBehaviorOptions>(options =>
    {
        options.InvalidModelStateResponseFactory = actionContext =>
        {
            return CustomModelStateResponseFactory.CreateResponse(actionContext);
        };
    });
    builder.Services.AddOpenApi();

    builder.Services.AddMySql<AppDbContext>(builder.Configuration.GetConnectionString("DefaultConnection"), ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection")));

    builder.Services.RegisterHttpClients(builder.Configuration);

    builder.Services.AddMiCakeWithDefault<AppModule, AppDbContext>().Build();

    builder.Services.AddJWTAuthentication(builder.Configuration);
    builder.Services.AddCorsPolicy(builder.Configuration);
    builder.Services.AddAutoMapper(typeof(Program).Assembly);
    builder.Services.AddValidatorsFromAssemblyContaining<Program>();
    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.RegisterOptions(builder.Configuration);
    builder.Services.AddCoreServices();

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