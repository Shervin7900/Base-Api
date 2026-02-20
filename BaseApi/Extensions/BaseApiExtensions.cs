using BaseApi.Data;
using BaseApi.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BaseApi.Extensions;

public static class BaseApiExtensions
{
    public static IServiceCollection AddBaseInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? "Server=(localdb)\\mssqllocaldb;Database=BaseApi;Trusted_Connection=True;MultipleActiveResultSets=true";
        var redisConnectionString = configuration.GetConnectionString("Redis") ?? "localhost:6379";
        var mongoConnectionString = configuration.GetConnectionString("Mongo") ?? "mongodb://localhost:27017/BaseApi";

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

        return services;
    }

    public static IApplicationBuilder UseBaseInfrastructure(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Base API v1");
                options.RoutePrefix = "swagger";
                options.DocumentTitle = "Base API - API & Dashboards";
            });
        }

        app.UseHttpsRedirection();
        app.UseIdentityServer();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseVectorMetrics();

        app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            ResponseWriter = HealthChecks.UI.Client.UIResponseWriter.WriteHealthCheckUIResponse
        });

        app.MapHealthChecksUI(setup =>
        {
            setup.UIPath = "/health-ui";
        });

        app.MapControllers();

        return app;
    }
}
