using System.Reflection;
using Infrastructure.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenIddict.Validation.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Infrastructure.Auth.OpenIddict;

public static class Extensions
{
    public static IServiceCollection AddAuthValidation(this IServiceCollection services, IConfiguration config)
    {
        var authOptions = services.BindValidateReturn<OpenIddictOptions>(config);

        services.AddOpenIddict()
        .AddValidation(options =>
        {
            options.SetIssuer(authOptions.IssuerUrl!);
            options.UseIntrospection()
                   .SetClientId(authOptions.ClientId!)
                   .SetClientSecret(authOptions.ClientSecret!);
            options.UseSystemNetHttp();
            options.UseAspNetCore();
        });

        services.AddAuthentication(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
        services.AddAuthorization();
        return services;
    }

    public static void ConfigureAuthServer<T>(this WebApplicationBuilder builder, Assembly dbContextAssembly, string connectionName = "DefaultConnection") where T : DbContext
    {
        builder.Services.AddOpenIddict()
        // Register the OpenIddict core components.
        .AddCore(options =>// Configure OpenIddict to use the EF Core stores/models.
                            options.UseEntityFrameworkCore().UseDbContext<T>())
        // Register the OpenIddict server components.
        .AddServer(options =>
        {
            //enable endpoint
            options.SetAuthorizationEndpointUris("connect/authorize")
                   .SetIntrospectionEndpointUris("connect/introspect")
                   .SetLogoutEndpointUris("connect/logout")  
                   .SetUserinfoEndpointUris("connect/userinfo")
                   .SetTokenEndpointUris("connect/token");
            //enable Crendentials flow
            options.AllowClientCredentialsFlow();
            // Register scopes (permissions)
            options.RegisterScopes(Scopes.Email, Scopes.Profile, Scopes.Roles);
            options.DisableAccessTokenEncryption();
            //Register the singing and encryption credentials
            options.AddDevelopmentEncryptionCertificate().AddDevelopmentSigningCertificate();
            // Register the ASP.NET Core host and configure the ASP.NET Core-specific options.
            options.UseAspNetCore().EnableTokenEndpointPassthrough().DisableTransportSecurityRequirement();
        })
        .AddValidation(options =>
        {
            options.UseLocalServer();
            options.UseAspNetCore();
        });

        builder.Services.AddAuthentication(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
        builder.Services.AddAuthorization();

        string? connectionString = builder.Configuration.GetConnectionString(connectionName);
        if (!builder.Environment.IsDevelopment() && connectionString == null)
            throw new ArgumentNullException(nameof(connectionString));

        builder.Services.AddDbContext<T>(options =>
        {
            //configure  Entity Framework core to use microsoft SQL Server 
          options.UseNpgsql(connectionString, m => m.MigrationsAssembly(dbContextAssembly.FullName));
          // Register the entity sets needed by OpenIddict
            options.UseOpenIddict();
        });
    }
}
