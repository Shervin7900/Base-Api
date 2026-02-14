using DiscountManager.Api.Models;
using Duende.IdentityServer.EntityFramework.Entities;
using Duende.IdentityServer.EntityFramework.Extensions;
using Duende.IdentityServer.EntityFramework.Interfaces;
using Duende.IdentityServer.EntityFramework.Options;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DiscountManager.Api.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IConfigurationDbContext, IPersistedGrantDbContext
{
    private readonly IOptions<ConfigurationStoreOptions> _configurationStoreOptions;
    private readonly IOptions<OperationalStoreOptions> _operationalStoreOptions;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        IOptions<ConfigurationStoreOptions> configurationStoreOptions,
        IOptions<OperationalStoreOptions> operationalStoreOptions)
        : base(options)
    {
        _configurationStoreOptions = configurationStoreOptions;
        _operationalStoreOptions = operationalStoreOptions;
    }

    public DbSet<Client> Clients { get; set; }
    public DbSet<ClientCorsOrigin> ClientCorsOrigins { get; set; }
    public DbSet<IdentityResource> IdentityResources { get; set; }
    public DbSet<ApiResource> ApiResources { get; set; }
    public DbSet<ApiScope> ApiScopes { get; set; }
    public DbSet<IdentityProvider> IdentityProviders { get; set; }
    public DbSet<PersistedGrant> PersistedGrants { get; set; }
    public DbSet<DeviceFlowCodes> DeviceFlowCodes { get; set; }
    public DbSet<Key> Keys { get; set; }
    public DbSet<ServerSideSession> ServerSideSessions { get; set; }
    public DbSet<PushedAuthorizationRequest> PushedAuthorizationRequests { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        builder.ConfigureClientContext(_configurationStoreOptions.Value);
        builder.ConfigureResourcesContext(_configurationStoreOptions.Value);
        builder.ConfigurePersistedGrantContext(_operationalStoreOptions.Value);
    }

    Task<int> IConfigurationDbContext.SaveChangesAsync() => base.SaveChangesAsync();
    Task<int> IPersistedGrantDbContext.SaveChangesAsync() => base.SaveChangesAsync();
}
