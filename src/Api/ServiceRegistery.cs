using System.Text;
using System.Text.Json;
using Application.Common.Settings;
using Domain.Entities;
using Infrastructure.Context;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceRegistery
{
    public static void ConfigureDb(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<DatabaseContext>(
            opts => opts.UseSqlServer(connectionString,
            b => b.MigrationsAssembly("Infrastructure")));
    }

    public static void ConfigureIdentity(this IServiceCollection services)
    {
        services.AddIdentity<User, UserRole>(options =>
        {
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 4;
            options.Password.RequiredUniqueChars = 0;
            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedEmail = true;
        })
            .AddEntityFrameworkStores<DatabaseContext>()
            .AddDefaultTokenProviders();
    }

    public static void ConfigureAuth(this IServiceCollection services, IConfiguration config)
    {
        var bearerTokenSettings = config.GetSection("BearerTokenSettings").Get(typeof(BearerTokenSettings)) as BearerTokenSettings;

        services.Configure<BearerTokenSettings>(config.GetSection("BearerTokenSettings"));

        services
            .AddAuthentication(options =>
            {
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(cfg =>
            {
                cfg.RequireHttpsMetadata = false;
                cfg.SaveToken = true;
                cfg.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = bearerTokenSettings?.Issuer,
                    ValidateIssuer = true,
                    ValidAudience = bearerTokenSettings?.Audiance,
                    ValidateAudience = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(bearerTokenSettings.Secret)),
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
                cfg.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        context.Response.StatusCode = 401;
                        context.Response.ContentType = "application/json";
                        return context.Response.WriteAsync(ProduceUnAuthorizedResponse());
                    },
                    OnTokenValidated = context =>
                    {
                        var tokenValidatorService = context.HttpContext.RequestServices.GetRequiredService<ITokenValidatorService>();
                        return tokenValidatorService.ValidateAsync(context);
                    },
                    OnMessageReceived = context =>
                    {
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = 401;
                        context.Response.ContentType = "application/json";
                        return context.Response.WriteAsync(ProduceUnAuthorizedResponse());
                    },
                    OnForbidden = context =>
                    {
                        context.Response.StatusCode = 403;
                        context.Response.ContentType = "application/json";
                        return context.Response.WriteAsync(ProduceAccessDeniedResponse());
                    }
                };
            });
    }

    private static string ProduceUnAuthorizedResponse()
    {
        return JsonSerializer.Serialize(new { });
    }

    private static string ProduceAccessDeniedResponse()
    {
        return JsonSerializer.Serialize(new { });
    }

}
