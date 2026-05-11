using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using RBACWeb.Application.Constants;
using RBACWeb.Common.Auth;
using RBACWeb.Web.Authorization;

namespace RBACWeb.Web.StartUp;

public static class StartUpServiceExtension
{
    public static void RegisterHttpClients(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();

        services.AddHttpClient(HttpClientNames.Default, client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .AddPolicyHandler(PollyHelper.GetRetryPolicy())
        .AddPolicyHandler(PollyHelper.GetCircuitBreakerPolicy());

        // Additional HttpClient registrations can be added here
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

        // Register authorization services with dynamic policy provider
        services.AddRBACAuthorization();

        return services;
    }

    /// <summary>
    /// Registers RBAC authorization services including policy provider and authorization handlers.
    /// </summary>
    public static IServiceCollection AddRBACAuthorization(this IServiceCollection services)
    {
        // Register custom policy provider that creates policies dynamically for permission requirements
        services.AddSingleton<IAuthorizationPolicyProvider, AuthorizationPolicyProvider>();

        // Register authorization handlers
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

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
                ConfigureCorsPolicy(builder, allowAnyOrigin, strictOrigins, wildcardOrigins);
            });
        });

        return services;
    }

    private static void ConfigureCorsPolicy(Microsoft.AspNetCore.Cors.Infrastructure.CorsPolicyBuilder builder, bool allowAnyOrigin, string[] strictOrigins, string[] wildcardOrigins)
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
            builder.SetIsOriginAllowed(origin => IsOriginAllowed(origin, strictOrigins, wildcardOrigins));
        }

        builder.AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials();
    }

    private static bool IsOriginAllowed(string origin, string[] strictOrigins, string[] wildcardOrigins)
    {
        if (strictOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase))
        {
            return true;
        }

        return wildcardOrigins.Any(pattern => MatchesWildcardOrigin(pattern, origin));
    }

    private static bool MatchesWildcardOrigin(string pattern, string origin)
    {
        var regexPattern = "^" + Regex.Escape(pattern).Replace("\\*", ".*") + "$";
        return Regex.IsMatch(origin, regexPattern, RegexOptions.IgnoreCase);
    }
}