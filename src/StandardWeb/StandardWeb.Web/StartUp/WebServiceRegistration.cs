using FluentValidation;
using FluentValidation.AspNetCore;
using StandardWeb.Application;

namespace StandardWeb.Web.StartUp;

/// <summary>
/// Centralizes service registrations that are specific to the StandardWeb API host.
/// </summary>
public static class WebServiceRegistration
{
    /// <summary>
    /// Registers the baseline MVC + validation + mapping stack for the API host.
    /// </summary>
    public static IServiceCollection AddWebApiDefaults(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers();

        services.AddOpenApi();
        services.AddAutoMapper([typeof(WebModule).Assembly, typeof(ApplicationModule).Assembly]);
        services.AddValidatorsFromAssembly(typeof(WebModule).Assembly);
        services.AddFluentValidationAutoValidation();
        services.AddFluentValidationClientsideAdapters();
        services.RegisterOptions(configuration);
        services.AddCoreServices();

        return services;
    }

    /// <summary>
    /// Registers strongly typed options from configuration for the API host.
    /// </summary>
    private static IServiceCollection RegisterOptions(this IServiceCollection services, IConfiguration configuration)
    {
        // Add additional option registrations here as needed

        return services;
    }

    private static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        // Add additional core service registrations here as needed
        services.AddScoped<InfrastructureTools>();

        return services;
    }
}
