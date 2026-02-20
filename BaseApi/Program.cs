using BaseApi.Data;
using BaseApi.Extensions;
using Duende.IdentityServer.EntityFramework.Mappers;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add Base Infrastructure (IdentityServer, Health, Metrics, Caching, etc.)
builder.Services.AddBaseInfrastructure(builder.Configuration);

var app = builder.Build();

// Use Base Infrastructure
app.UseBaseInfrastructure();

// Optional Seeding Logic
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    context.Database.EnsureCreated();

    // Only seed if empty and requested (or always in this demo)
    if (!context.Clients.Any())
    {
        foreach (var client in BaseApi.Data.Config.Clients)
        {
            context.Clients.Add(client.ToEntity());
        }
        
        foreach (var resource in BaseApi.Data.Config.IdentityResources)
        {
            context.IdentityResources.Add(resource.ToEntity());
        }
        
        foreach (var scopeItem in BaseApi.Data.Config.ApiScopes)
        {
            context.ApiScopes.Add(scopeItem.ToEntity());
        }
        
        context.SaveChanges();
    }
}

app.Run();

public partial class Program { }
