﻿using FluentValidation;
using FluentValidation.AspNetCore;
using MiCake;
using MiCake.AspNetCore.Identity;
using MiCake.Identity.Authentication.JwtToken;
using MiCakeTemplate.Api.Extentsions;
using MiCakeTemplate.EFCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace MiCakeTemplate.Api
{
    internal static class AddAppCoreServiceExtension
    {
        public static IServiceCollection AddAppCoreService(this IServiceCollection services, IConfiguration configuration)
        {
            // Add FluentValidation, use it to validate dto models.
            // see https://docs.fluentvalidation.net/en/latest/aspnet.html#getting-started
            services.AddFluentValidationAutoValidation();
            services.AddValidatorsFromAssemblyContaining<Program>();

            // Add EFCore
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("SqlServer"));
            });

            // Add JWT authentication 
            var jwtSecurityKey = configuration["JwtSetting:SecurityKey"];
            var jwtTokenLifetime = configuration.GetSection("JwtSetting:AccessTokenLifetime").Get<int>();
            string jwtIssuer = $"{nameof(MiCakeTemplate)}.API", audience = $"{nameof(MiCakeTemplate)}.API";

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
                            ValidAudience = audience,
                        };
                    });

            // Add MiCake
            services.AddMiCakeServices<WebAppModule, AppDbContext>()
                    .UseIdentity<int>()
                    .UseJwt(options =>              // if you don't want to use jwt which created by MiCake, you can remove this method.
                    {
                        options.AccessTokenLifetime = (uint)jwtTokenLifetime;
                        options.Issuer = jwtIssuer;
                        options.Audience = audience;
                        options.IssuerSigningKey = new SymmetricSecurityKey(Encoding.Default.GetBytes(jwtSecurityKey));
                        options.RefreshTokenMode = RefreshTokenUsageMode.Recreate;
                        options.DeleteRefreshTokenWhenExchange = true;
                    })
                    .Build();

            return services;
        }

        public static IServiceCollection AddSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.OperationFilter<SwaggerApiResponseFilter>();
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Description = "Authorization format : Bearer {token}",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
                        new List<string>()
                    }
                });
            });

            return services;
        }
    }
}
