using MiCake;
using MiCakeTemplate.Api;
using MiCakeTemplate.Api.Middlewares;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Add serilog
    builder.Host.UseSerilog((context, services, configuration) =>
    {
        configuration.ReadFrom.Configuration(context.Configuration)
                     .ReadFrom.Services(services)
                     .Enrich.FromLogContext();
    });

    // Add services to the container.
    builder.Services.AddControllers();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwagger();

    builder.Services.AddAppCoreService(builder.Configuration);
    builder.Services.AddHttpLogging(opt =>
    {
        opt.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.All;
    });



    var app = builder.Build();

    app.UseHttpLogging();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseMiddleware<AppExceptionHandlerMiddleware>();

    // Add CORS
    app.UseCors(builder =>
    {
        builder.WithOrigins(app.Configuration.GetSection("CorsAllowedOrigin").Get<string[]>() ?? Array.Empty<string>())
               .SetIsOriginAllowedToAllowWildcardSubdomains()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

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