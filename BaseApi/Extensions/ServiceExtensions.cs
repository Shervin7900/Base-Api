using BaseApi.Infrastructure.Persistence;
using BaseApi.Domain.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MongoDB.Driver;
using FluentValidation;
using AutoMapper;

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

        services.AddScoped<Domain.Interfaces.ISqlUnitOfWork, Infrastructure.Persistence.SqlUnitOfWork>();
        services.AddScoped(typeof(Domain.Interfaces.IGenericRepository<>), typeof(Infrastructure.Persistence.EFGenericRepository<>));
    }
    public static void AddMongoPersistence(this IServiceCollection services, string connectionString)
    {
        if (connectionString == "InMemory" || string.IsNullOrEmpty(connectionString))
        {
            // Skip or register a mock if needed. For now, we skip to avoid connection attempts.
            return;
        }
        var settings = MongoClientSettings.FromConnectionString(connectionString);
        services.AddSingleton<IMongoClient>(new MongoClient(settings));
        services.AddScoped<Domain.Interfaces.IMongoUnitOfWork>(sp => 
        {
             var client = sp.GetRequiredService<IMongoClient>();
             return new MongoUnitOfWork(client, "BaseDb"); // Or from config
        });
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
        
        if (redisConnectionString != "InMemory" && !string.IsNullOrEmpty(redisConnectionString))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnectionString;
                options.InstanceName = "BaseApi_";
            });
        }

        services.AddScoped<Domain.Interfaces.IRedisUnitOfWork, Infrastructure.Persistence.RedisUnitOfWork>();

#pragma warning disable EXTEXP0018
        services.AddHybridCache(); 
#pragma warning restore EXTEXP0018
    }

    public static void AddSystematicHealthChecks(this IServiceCollection services, string sqlConnectionString, string redisConnectionString, string mongoConnectionString)
    {
        var builder = services.AddHealthChecks();
        
        if (sqlConnectionString != "InMemory")
        {
            builder.AddSqlServer(sqlConnectionString, name: "SQL Server", tags: new[] { "ready" });
        }
        
        if (redisConnectionString != "InMemory" && !string.IsNullOrEmpty(redisConnectionString))
        {
            builder.AddRedis(redisConnectionString, name: "Redis Cache", tags: new[] { "ready" });
        }
        
        if (mongoConnectionString != "InMemory" && !string.IsNullOrEmpty(mongoConnectionString))
        {
            builder.AddMongoDb(_ => new MongoClient(mongoConnectionString), name: "MongoDB", tags: new[] { "ready" });
        }

        services.AddHealthChecksUI(setup =>
        {
            setup.AddHealthCheckEndpoint("Liveness Check", "/health/live");
            setup.AddHealthCheckEndpoint("Readiness Check", "/health/ready");
            setup.SetEvaluationTimeInSeconds(30);
        }).AddInMemoryStorage();
    }

    public static void AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            // Use full type name (namespace + class) to avoid schema ID conflicts
            // e.g. BasketApi.Features.Basket.Get.Request vs BasketApi.Features.Basket.Update.Request
            options.CustomSchemaIds(type =>
                type.FullName?.Replace("+", ".") ?? type.Name);
        });
    }

    public static void AddStandardCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("DefaultCors", builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            });
        });
    }

    public static void AddApiVersioningConfig(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
        }).AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });
    }

    public static void AddAutoMapperConfig(this IServiceCollection services, params System.Reflection.Assembly[] assemblies)
    {
        services.AddAutoMapper(assemblies);
    }

    public static void AddFluentValidationConfig(this IServiceCollection services, params System.Reflection.Assembly[] assemblies)
    {
        services.AddValidatorsFromAssemblies(assemblies);
    }

    public static void AddStandardCompression(this IServiceCollection services)
    {
        services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
        });
    }
}
