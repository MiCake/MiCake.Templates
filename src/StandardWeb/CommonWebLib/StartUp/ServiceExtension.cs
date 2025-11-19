using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        var rawOrigins = configuration.GetValue<string>("AllowedOrigins");
        var parsedOrigins = rawOrigins?
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToArray() ?? Array.Empty<string>();

        var allowAnyOrigin = parsedOrigins.Length == 0 || parsedOrigins.Contains("*");
        var wildcardOrigins = parsedOrigins.Where(origin => origin.Contains('*') && origin != "*").ToArray();
        var strictOrigins = parsedOrigins.Except(wildcardOrigins).Where(origin => origin != "*").ToArray();

        services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy", builder =>
            {
                if (allowAnyOrigin)
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                    return;
                }

                if (strictOrigins.Length > 0)
                {
                    builder.WithOrigins(strictOrigins);
                }

                if (wildcardOrigins.Length > 0)
                {
                    // Allow wildcard hostnames (e.g. https://*.contoso.com) without exposing the app to arbitrary origins.
                    builder.SetIsOriginAllowed(origin =>
                    {
                        if (strictOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase))
                        {
                            return true;
                        }

                        return wildcardOrigins.Any(pattern => MatchesWildcardOrigin(pattern, origin));
                    });
                }

                builder.AllowAnyMethod()
                       .AllowAnyHeader()
                       .AllowCredentials();
            });
        });

        return services;
    }

    private static bool MatchesWildcardOrigin(string pattern, string origin)
    {
        var regexPattern = "^" + Regex.Escape(pattern).Replace("\\*", ".*") + "$";
        return Regex.IsMatch(origin, regexPattern, RegexOptions.IgnoreCase);
    }
}
