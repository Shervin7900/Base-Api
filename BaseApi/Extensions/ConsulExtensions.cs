using Consul;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BaseApi.Extensions;

public static class ConsulExtensions
{
    public static IServiceCollection AddConsulConfig(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IConsulClient, ConsulClient>(p => new ConsulClient(consulConfig =>
        {
            var address = configuration["Consul:Address"] ?? "http://localhost:8500";
            consulConfig.Address = new Uri(address);
        }));

        return services;
    }

    public static IApplicationBuilder RegisterWithConsul(this IApplicationBuilder app, IConfiguration configuration, IHostApplicationLifetime lifetime)
    {
        var consulClient = app.ApplicationServices.GetRequiredService<IConsulClient>();
        var logger = app.ApplicationServices.GetRequiredService<ILoggerFactory>().CreateLogger("ConsulRegistration");

        var serviceName = configuration["Consul:ServiceName"] ?? "Unknown-Service";
        var serviceId = $"{serviceName}-{Guid.NewGuid()}";
        var serviceAddress = configuration["Consul:ServiceAddress"] ?? "localhost";
        var servicePort = int.Parse(configuration["Consul:ServicePort"] ?? "8080");

        var registration = new AgentServiceRegistration()
        {
            ID = serviceId,
            Name = serviceName,
            Address = serviceAddress,
            Port = servicePort,
            Check = new AgentServiceCheck()
            {
                HTTP = $"http://{serviceAddress}:{servicePort}/health/live",
                Interval = TimeSpan.FromSeconds(10),
                Timeout = TimeSpan.FromSeconds(5)
            }
        };

        logger.LogInformation("Registering with Consul: {ServiceId}", serviceId);
        consulClient.Agent.ServiceDeregister(registration.ID).Wait();
        consulClient.Agent.ServiceRegister(registration).Wait();

        lifetime.ApplicationStopping.Register(() =>
        {
            logger.LogInformation("Deregistering from Consul: {ServiceId}", serviceId);
            consulClient.Agent.ServiceDeregister(registration.ID).Wait();
        });

        return app;
    }
}
