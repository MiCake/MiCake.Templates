using System.Text;
using System.Text.RegularExpressions;
using Microsoft.IdentityModel.Tokens;
using StandardWeb.Application.Constants;
using StandardWeb.Common.Auth;

namespace StandardWeb.Web.StartUp;

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