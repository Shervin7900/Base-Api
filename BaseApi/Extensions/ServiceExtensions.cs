using BaseApi.Infrastructure.Persistence;
using BaseApi.Domain.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MongoDB.Driver;

namespace BaseApi.Extensions;

public static class ServiceExtensions
{
    public static void AddDatabaseContext(this IServiceCollection services, string connectionString)
    {
        if (connectionString == "InMemory")
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase("InMemoryDbForTesting"));
        }
        else
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));
        }
    }

    public static void AddMongoPersistence(this IServiceCollection services, string connectionString)
    {
        var settings = MongoClientSettings.FromConnectionString(connectionString);
        services.AddSingleton<IMongoClient>(new MongoClient(settings));
    }

    public static void AddIdentityAndIdentityServer(this IServiceCollection services, bool useInMemoryStores = false)
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
            });

        if (useInMemoryStores)
        {
            builder.AddInMemoryClients(Config.Clients)
                   .AddInMemoryApiScopes(Config.ApiScopes)
                   .AddInMemoryIdentityResources(Config.IdentityResources);
        }
        else
        {
            builder.AddConfigurationStore<ApplicationDbContext>()
                   .AddOperationalStore<ApplicationDbContext>();
        }

        builder.AddAspNetIdentity<ApplicationUser>()
               .AddDeveloperSigningCredential();

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
            options.Cookie.Name = "BaseApi.Auth";
        });
    }

    public static void AddAdvancedCaching(this IServiceCollection services, string redisConnectionString)
    {
        services.AddMemoryCache();
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
            options.InstanceName = "BaseApi_";
        });

#pragma warning disable EXTEXP0018
        services.AddHybridCache(); 
#pragma warning restore EXTEXP0018
    }

    public static void AddSystematicHealthChecks(this IServiceCollection services, string sqlConnectionString, string redisConnectionString, string mongoConnectionString)
    {
        services.AddHealthChecks()
            .AddSqlServer(sqlConnectionString, name: "SQL Server")
            .AddRedis(redisConnectionString, name: "Redis Cache")
            .AddMongoDb(_ => new MongoClient(mongoConnectionString), name: "MongoDB");

        services.AddHealthChecksUI(setup =>
        {
            setup.AddHealthCheckEndpoint("Basic Health Check", "/health");
            setup.SetEvaluationTimeInSeconds(30);
        }).AddInMemoryStorage();
    }

    public static void AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddSwaggerGen();
    }
}
