using BaseApi.Infrastructure.Persistence;
using BaseApi.Extensions;
using Duende.IdentityServer.EntityFramework.Mappers;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseSentry(options =>
{
    options.Dsn = builder.Configuration["Sentry:Dsn"] ?? string.Empty;
    options.TracesSampleRate = double.TryParse(builder.Configuration["Sentry:TracesSampleRate"], out var rate) ? rate : 0.1;
    options.AttachStacktrace = true;
});

// Add Base Infrastructure (IdentityServer, Health, Metrics, Caching, etc.)
builder.Services.AddBaseInfrastructure(builder.Configuration);

var app = builder.Build();

// Use Base Infrastructure
app.UseBaseInfrastructure("Base API", "Shared Identity & Infrastructure");

// Optional Seeding Logic
try
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.EnsureCreated();

        // Only seed if empty and requested (or always in this demo)
        if (!context.Clients.Any())
        {
            foreach (var client in Config.Clients)
            {
                context.Clients.Add(client.ToEntity());
            }
            
            foreach (var resource in Config.IdentityResources)
            {
                context.IdentityResources.Add(resource.ToEntity());
            }
            
            foreach (var scopeItem in Config.ApiScopes)
            {
                context.ApiScopes.Add(scopeItem.ToEntity());
            }
            
            context.SaveChanges();
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"[Warning] Database initialization failed: {ex.Message}. The app will proceed without database-dependent features.");
}

app.Run();

public partial class Program { }
