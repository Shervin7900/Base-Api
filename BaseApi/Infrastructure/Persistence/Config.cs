using BaseApi.Domain.Entities;
using Duende.IdentityServer;
using Duende.IdentityServer.Models;

namespace BaseApi.Infrastructure.Persistence;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        new List<IdentityResource>
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
        };

    public static IEnumerable<ApiScope> ApiScopes =>
        new List<ApiScope>
        {
            new ApiScope("discount.read", "Read Access to Discount API"),
            new ApiScope("discount.write", "Write Access to Discount API"),
        };

    public static IEnumerable<Client> Clients =>
        new List<Client>
        {
            // interactive ASP.NET Core Web App
            new Client
            {
                ClientId = "web",
                ClientSecrets = { new Secret("secret".Sha256()) },

                AllowedGrantTypes = GrantTypes.Code,
                
                // where to redirect to after login
                RedirectUris = { "https://localhost:7002/signin-oidc" },

                // where to redirect to after logout
                PostLogoutRedirectUris = { "https://localhost:7002/signout-callback-oidc" },

                AllowedScopes = new List<string>
                {
                    "openid",
                    "profile",
                    "discount.read"
                }
            },
            // machine to machine client
            new Client
            {
                ClientId = "client",
                ClientSecrets = { new Secret("secret".Sha256()) },

                AllowedGrantTypes = GrantTypes.ClientCredentials,
                // scopes that client has access to
                AllowedScopes = { "discount.read", "discount.write" }
            }
        };
}
