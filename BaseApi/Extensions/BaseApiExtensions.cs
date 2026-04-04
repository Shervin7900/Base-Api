using BaseApi.Infrastructure.Persistence;
using BaseApi.Domain.Entities;
using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;

namespace BaseApi.Extensions;

public static class BaseApiExtensions
{
    public static IServiceCollection AddBaseInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? "Server=(localdb)\\mssqllocaldb;Database=BaseStore;Trusted_Connection=True;MultipleActiveResultSets=true";
        var redisConnectionString = configuration.GetConnectionString("Redis") ?? "localhost:6379";
        var mongoConnectionString = configuration.GetConnectionString("Mongo") ?? "mongodb://localhost:27017/BaseStore";

        services.AddDatabaseContext(connectionString);
        services.AddMongoPersistence(mongoConnectionString);
        services.AddIdentityAndIdentityServer(useInMemoryStores: connectionString == "InMemory");
        services.AddAdvancedCaching(redisConnectionString);
        services.AddSystematicHealthChecks(connectionString, redisConnectionString, mongoConnectionString);
        services.AddVectorMetrics();
        services.AddSwaggerDocumentation();
        services.AddHttpClient();

        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddFastEndpoints();
        services.SwaggerDocument();

        // New Base Infrastructure Needs
        services.AddStandardCors();
        services.AddApiVersioningConfig();
        services.AddStandardCompression();
        
        // Registering standard assemblies (calling project's calling assembly)
        var assembly = System.Reflection.Assembly.GetCallingAssembly();
        services.AddAutoMapperConfig(assembly);
        services.AddFluentValidationConfig(assembly);

        return services;
    }

    public static IApplicationBuilder UseBaseInfrastructure(this WebApplication app, string apiName = "Base API", string apiTitle = "Base API - API & Dashboards")
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", $"{apiName} v1");
                options.RoutePrefix = "swagger";
                options.DocumentTitle = apiTitle;
            });
        }

        app.UseCors("DefaultCors");
        app.UseResponseCompression();
        
        // Base Middlewares
        app.UseMiddleware<Infrastructure.Middlewares.ExceptionMiddleware>();
        app.UseMiddleware<Infrastructure.Middlewares.PerformanceMiddleware>();
        app.UseMiddleware<Infrastructure.Middlewares.LoggingMiddleware>();

        app.UseHttpsRedirection();
        app.UseIdentityServer();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseVectorMetrics();

        app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("live"),
            ResponseWriter = async (httpContext, report) =>
            {
                httpContext.Response.ContentType = "application/json";
                var result = new
                {
                    Status = report.Status.ToString(),
                    Description = report.Status.ToString(),
                    Data = report.Entries.ToDictionary(e => e.Key, e => e.Value.Status.ToString())
                };
                await httpContext.Response.WriteAsync(JsonSerializer.Serialize(result));
            }
        }).ShortCircuit();

        app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready"),
            ResponseWriter = async (httpContext, report) =>
            {
                httpContext.Response.ContentType = "application/json";
                var result = new
                {
                    Status = report.Status.ToString(),
                    Description = report.Status.ToString(),
                    Data = report.Entries.ToDictionary(e => e.Key, e => e.Value.Status.ToString())
                };
                await httpContext.Response.WriteAsync(JsonSerializer.Serialize(result));
            }        
        }).ShortCircuit();

        app.MapHealthChecksUI(setup =>
        {
            setup.UIPath = "/health-ui";
        });

        app.MapControllers();
        app.UseFastEndpoints();

        return app;
    }
}
