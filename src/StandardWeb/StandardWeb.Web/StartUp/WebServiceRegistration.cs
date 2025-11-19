using System.Reflection;
using CommonWebLib.StartUp;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
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

        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = CustomModelStateResponseFactory.CreateResponse;
        });

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
    /// Binds strongly typed configuration sections that other layers expect.
    /// </summary>
    public static IServiceCollection RegisterOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<AESEncryptionOptions>()
            .Bind(configuration.GetSection("AESEncryption"))
            .Validate(o => !string.IsNullOrWhiteSpace(o.Key), "AESEncryption:Key must be configured.")
            .ValidateOnStart();

        return services;
    }
}
