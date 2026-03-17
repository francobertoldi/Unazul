using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SA.Organization.DataAccess.EntityFramework.Persistence;
using Shared.Auth;

namespace SA.Organization.Tests.E2E.Fixtures;

public sealed class OrganizationWebAppFactory : WebApplicationFactory<SA.Organization.Api.Program>
{
    private readonly string _dbName = "OrganizationTestDb_" + Guid.NewGuid();

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
        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration
            var dbDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<OrganizationDbContext>));
            if (dbDescriptor != null) services.Remove(dbDescriptor);

            // Remove TenantRlsInterceptor (singleton consuming scoped ICurrentUser)
            var rlsDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(TenantRlsInterceptor));
            if (rlsDescriptor != null) services.Remove(rlsDescriptor);

            // Add a no-op TenantRlsInterceptor (avoids scoped ICurrentUser issue)
            services.AddSingleton(new TenantRlsInterceptor(new TestCurrentUser()));

            // Replace MassTransit with in-memory test harness to avoid RabbitMQ connection
            var massTransitDescriptors = services
                .Where(d => d.ServiceType.FullName?.Contains("MassTransit") == true
                         || d.ImplementationType?.FullName?.Contains("MassTransit") == true)
                .ToList();
            foreach (var d in massTransitDescriptors) services.Remove(d);

            // Re-add MassTransit with test harness (in-memory, no RabbitMQ)
            services.AddMassTransitTestHarness();

            // Add InMemory database for testing
            services.AddDbContext<OrganizationDbContext>(options =>
            {
                options.UseInMemoryDatabase(_dbName);
            });
        });

        builder.UseEnvironment("Development");
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
