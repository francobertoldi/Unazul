using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using SA.Catalog.Application.Interfaces;
using SA.Catalog.DataAccess.EntityFramework.Persistence;
using Shared.Auth;
using Shared.Contract.Events;

namespace SA.Catalog.Tests.E2E.Fixtures;

public sealed class CatalogWebAppFactory : WebApplicationFactory<SA.Catalog.Api.Program>
{
    private readonly string _dbName = "CatalogTestDb_" + Guid.NewGuid();

    // Must match appsettings.json Jwt section (or overridden config)
    private const string JwtKey = "super-secret-key-for-development-only-min-32-chars!";
    private const string JwtIssuer = "unazul-identity";
    private const string JwtAudience = "unazul-api";

    public static readonly Guid TestTenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid TestUserId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid TestEntityId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.UseDefaultServiceProvider(options =>
        {
            // Mediator source-gen registers handlers as singletons while
            // EF repositories are scoped. Disable scope validation for tests.
            options.ValidateScopes = false;
            options.ValidateOnBuild = false;
        });

        return base.CreateHost(builder);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Override the Jwt:Key so AuthServiceExtensions can find it
        builder.UseSetting("Jwt:Key", JwtKey);
        builder.UseSetting("Jwt:Issuer", JwtIssuer);
        builder.UseSetting("Jwt:Audience", JwtAudience);

        builder.ConfigureServices(services =>
        {
            // ---------------------------------------------------------------
            // Remove ALL EF Core, Npgsql, and NamingConventions registrations
            // ---------------------------------------------------------------
            var descriptorsToRemove = services
                .Where(d =>
                    d.ServiceType == typeof(DbContextOptions<CatalogDbContext>)
                    || d.ServiceType == typeof(DbContextOptions)
                    || d.ServiceType == typeof(CatalogDbContext)
                    || d.ServiceType.FullName?.Contains("EntityFrameworkCore") == true
                    || d.ServiceType.FullName?.Contains("Npgsql") == true
                    || d.ImplementationType?.FullName?.Contains("Npgsql") == true
                    || d.ImplementationType?.FullName?.Contains("NamingConventions") == true)
                .ToList();
            foreach (var d in descriptorsToRemove) services.Remove(d);

            // ---------------------------------------------------------------
            // Remove TenantRlsInterceptor (singleton consuming scoped ICurrentUser)
            // ---------------------------------------------------------------
            var rlsDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(TenantRlsInterceptor));
            if (rlsDescriptor != null) services.Remove(rlsDescriptor);

            // Add a no-op TenantRlsInterceptor (avoids scoped ICurrentUser issue)
            services.AddSingleton(new TenantRlsInterceptor(new TestCurrentUser()));

            // ---------------------------------------------------------------
            // Replace MassTransit with in-memory test harness
            // ---------------------------------------------------------------
            var massTransitDescriptors = services
                .Where(d => d.ServiceType.FullName?.Contains("MassTransit") == true
                         || d.ImplementationType?.FullName?.Contains("MassTransit") == true)
                .ToList();
            foreach (var d in massTransitDescriptors) services.Remove(d);

            services.AddMassTransitTestHarness();

            // ---------------------------------------------------------------
            // Replace IIntegrationEventPublisher with NoOp
            // ---------------------------------------------------------------
            var eventPubDescriptor = services.FirstOrDefault(
                d => d.ServiceType == typeof(IIntegrationEventPublisher));
            if (eventPubDescriptor != null) services.Remove(eventPubDescriptor);
            services.AddScoped<IIntegrationEventPublisher, TestNoOpEventPublisher>();

            // ---------------------------------------------------------------
            // Replace IEntityValidationService with stub returning true
            // ---------------------------------------------------------------
            var entityValDescriptor = services.FirstOrDefault(
                d => d.ServiceType == typeof(IEntityValidationService));
            if (entityValDescriptor != null) services.Remove(entityValDescriptor);
            services.AddScoped<IEntityValidationService, TestEntityValidationService>();

            // ---------------------------------------------------------------
            // Add InMemory database (replaces Npgsql from production config)
            // ---------------------------------------------------------------
            services.AddDbContext<CatalogDbContext>(options =>
            {
                options.UseInMemoryDatabase(_dbName);
            });
        });

        builder.UseEnvironment("Development");
    }

    /// <summary>
    /// Creates an HttpClient with a Bearer token containing the specified permissions.
    /// </summary>
    public HttpClient CreateAuthenticatedClient(params string[] permissions)
    {
        var client = CreateClient();
        var token = GenerateTestJwt(TestTenantId, TestUserId, permissions);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private static string GenerateTestJwt(Guid tenantId, Guid userId, string[] permissions)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("tenant_id", tenantId.ToString()),
            new("user_id", userId.ToString()),
            new("user_name", "Test User"),
        };

        foreach (var perm in permissions)
            claims.Add(new Claim("permissions", perm));

        var token = new JwtSecurityToken(
            issuer: JwtIssuer,
            audience: JwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Stub ICurrentUser used only to satisfy the TenantRlsInterceptor constructor.
    /// </summary>
    private sealed class TestCurrentUser : ICurrentUser
    {
        public Guid UserId => Guid.Empty;
        public Guid TenantId => Guid.Empty;
        public string UserName => "test";
        public string? IpAddress => null;
        public IReadOnlyList<string> Permissions => Array.Empty<string>();
        public bool HasPermission(string permissionCode) => false;
    }
}

internal sealed class TestNoOpEventPublisher : IIntegrationEventPublisher
{
    public Task PublishAsync(DomainEvent domainEvent, CancellationToken ct = default)
        => Task.CompletedTask;
}

internal sealed class TestEntityValidationService : IEntityValidationService
{
    public Task<bool> ValidateEntityExistsAsync(Guid tenantId, Guid entityId, CancellationToken ct = default)
        => Task.FromResult(true);
}
