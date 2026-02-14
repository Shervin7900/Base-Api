using DiscountManager.Api.Data;
using DiscountManager.Api.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace DiscountManager.Api.Extensions;

public static class ServiceExtensions
{
    public static void AddDatabaseContext(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));
    }

    public static void AddIdentityAndIdentityServer(this IServiceCollection services)
    {
        services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        var builder = services.AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;
                options.EmitStaticAudienceClaim = true;
            })
            .AddConfigurationStore<ApplicationDbContext>()
            .AddOperationalStore<ApplicationDbContext>()
            .AddAspNetIdentity<ApplicationUser>();

        builder.AddDeveloperSigningCredential();

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.Authority = "https://localhost:7001";
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false
            };
        })
        .AddCookie(options =>
        {
            options.Cookie.Name = "DiscountManager.Auth";
        });
    }

    public static void AddAdvancedCaching(this IServiceCollection services, string redisConnectionString)
    {
        services.AddMemoryCache();
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
            options.InstanceName = "DiscountManager_";
        });

#pragma warning disable EXTEXP0018
        services.AddHybridCache(); 
#pragma warning restore EXTEXP0018
    }

    public static void AddSystematicHealthChecks(this IServiceCollection services, string sqlConnectionString, string redisConnectionString)
    {
        services.AddHealthChecks()
            .AddSqlServer(sqlConnectionString, name: "SQL Server")
            .AddRedis(redisConnectionString, name: "Redis Cache");
    }
}
