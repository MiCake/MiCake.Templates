using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using StandardWeb.Common.Auth;

namespace CommonWebLib.StartUp;

public static class ServiceExtension
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        services.AddScoped<InfrastructureTools>();

        return services;
    }

    public static IServiceCollection AddJWTAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtConfigOptions>(configuration.GetSection("Jwt"));

        var jwtConfig = configuration.GetSection("Jwt").Get<JwtConfigOptions>() ?? throw new InvalidOperationException("JWT configuration is missing.");
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = "Bearer";
            options.DefaultChallengeScheme = "Bearer";
        })
        .AddJwtBearer("Bearer", options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtConfig.Issuer,
                ValidAudience = jwtConfig.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.SecretKey))
            };
        });

        services.AddAuthorizationBuilder()
            .AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"))
            .AddPolicy("FranchiseeOnly", policy => policy.RequireRole("Franchisee"));

        return services;
    }


    public static IServiceCollection AddCorsPolicy(this IServiceCollection services, IConfiguration configuration)
    {
        var allowedOrigins = configuration.GetSection("AllowedOrigins").Get<string>()?.Split(',')
            .Select(origin => origin.Trim())
            .Where(origin => !string.IsNullOrEmpty(origin))
            .ToArray() ?? Array.Empty<string>();

        // Register CORS policy
        services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy", builder =>
            {
                if (allowedOrigins.Length > 0)
                {
                    builder.WithOrigins(allowedOrigins)
                           .SetIsOriginAllowedToAllowWildcardSubdomains();
                }
                else
                {
                    // Fallback for development
                    builder.AllowAnyOrigin();
                }
                builder.AllowAnyMethod();
                builder.AllowAnyHeader();

                // Note: AllowCredentials() cannot be used with AllowAnyOrigin()
                if (allowedOrigins.Length > 0)
                {
                    builder.AllowCredentials();
                }
            });
        });

        return services;
    }
}
