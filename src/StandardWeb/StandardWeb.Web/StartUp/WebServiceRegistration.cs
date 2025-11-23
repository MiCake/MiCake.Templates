using System.Reflection;
using FluentValidation;
using FluentValidation.AspNetCore;
using StandardWeb.Common.Auth;

namespace StandardWeb.Web.StartUp;

/// <summary>
/// Centralizes service registrations that are specific to the StandardWeb API host.
/// </summary>
public static class WebServiceRegistration
{
    /// <summary>
    /// Registers the baseline MVC + validation + mapping stack for the API host.
    /// </summary>
    public static IServiceCollection AddWebApiDefaults(this IServiceCollection services, IConfiguration configuration, Assembly webAssembly)
    {
        services.AddControllers();

        services.AddOpenApi();
        services.AddAutoMapper(webAssembly);
        services.AddValidatorsFromAssembly(webAssembly);
        services.AddFluentValidationAutoValidation();
        services.AddFluentValidationClientsideAdapters();
        services.RegisterOptions(configuration);
        services.AddCoreServices();

        return services;
    }

    /// <summary>
    /// Registers strongly typed options from configuration for the API host.
    /// </summary>
    public static IServiceCollection RegisterOptions(this IServiceCollection services, IConfiguration configuration)
    {
        // JWT configuration
        services.Configure<JwtConfigOptions>(configuration.GetSection("Jwt"));

        return services;
    }

    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        services.AddScoped<InfrastructureTools>();

        return services;
    }
}
