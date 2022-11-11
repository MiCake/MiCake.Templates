using MiCake;
using MiCake.AspNetCore.Identity;
using MiCake.Identity.Authentication.JwtToken;
using MiCakeTemplate.EFCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace MiCakeTemplate.Api
{
    internal static class AddAppCoreServiceExtension
    {
        public static IServiceCollection AddAppCoreService(this IServiceCollection services, IConfiguration configuration)
        {
            // Add EFCore
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseNpgsql(configuration.GetConnectionString("Postgres"));
            });

            // Add JWT authentication 
            var jwtSecurityKey = configuration["JwtSetting:SecurityKey"];
            var jwtTokenLifetime = configuration.GetSection("JwtSetting:AccessTokenLifetime").Get<int>();
            var jwtIssuer = $"{nameof(MiCakeTemplate)}.API";

            if (string.IsNullOrWhiteSpace(jwtSecurityKey) || jwtTokenLifetime <= 0)
            {
                throw new ArgumentException("Error from Jwt configuration.");
            }

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                    {
                        options.TokenValidationParameters = new TokenValidationParameters()
                        {
                            ValidateAudience = true,
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.Default.GetBytes(jwtSecurityKey)),
                            ValidIssuer = jwtIssuer,
                            ValidAudience = jwtIssuer,
                        };
                    });

            // Add MiCake
            services.AddMiCakeServices<MiCakeTemplateModule, AppDbContext>()
                    .UseIdentity<int>()
                    .UseJwt(options =>
                    {
                        options.AccessTokenLifetime = (uint)jwtTokenLifetime;
                        options.Issuer = jwtIssuer;
                        options.Audience = jwtIssuer;
                        options.IssuerSigningKey = new SymmetricSecurityKey(Encoding.Default.GetBytes(jwtSecurityKey));
                        options.RefreshTokenMode = RefreshTokenUsageMode.Recreate;
                        options.DeleteRefreshTokenWhenExchange = true;
                    })
                    .Build();

            return services;
        }
    }
}
