using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SA.Identity.Application.Interfaces;
using SA.Identity.DataAccess.EntityFramework.Persistence;
using SA.Identity.DataAccess.EntityFramework.Seed;
using SA.Identity.Domain.Entities;
using Shared.Auth;

namespace SA.Identity.Tests.E2E.Fixtures;

public sealed class IdentityWebAppFactory : WebApplicationFactory<SA.Identity.Api.Program>
{
    /// <summary>
    /// Fixed tenant used across all E2E tests.
    /// </summary>
    public static readonly Guid TestTenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    /// <summary>
    /// Fixed admin user ID.
    /// </summary>
    public static readonly Guid AdminUserId = Guid.Parse("00000000-0000-0000-0000-000000000002");

    public const string AdminUsername = "admin";
    public const string AdminPassword = "P@ssw0rd!";
    public const string AdminEmail = "admin@test.com";

    private const string TestJwtKey = "test-jwt-secret-key-minimum-32-chars-long!!";

    private readonly string _dbName = $"IdentityTestDb_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Provide test configuration so that AddUnazulAuth and AddInfrastructureServices
        // find the required JWT settings even without appsettings.Development.json.
        // Use UseSetting which adds to the WebApplicationBuilder's configuration
        // before services are registered.
        builder.UseSetting("Jwt:Key", TestJwtKey);
        builder.UseSetting("Jwt:Issuer", "SA.Identity.Tests");
        builder.UseSetting("Jwt:Audience", "SA.Unazul.Tests");
        builder.UseSetting("JwtOptions:Secret", TestJwtKey);
        builder.UseSetting("JwtOptions:Issuer", "SA.Identity.Tests");
        builder.UseSetting("JwtOptions:Audience", "SA.Unazul.Tests");
        builder.UseSetting("JwtOptions:AccessTokenExpirationMinutes", "15");
        builder.UseSetting("JwtOptions:RefreshTokenExpirationDays", "7");
        builder.UseSetting("ConnectionStrings:IdentityDb", "Host=localhost;Database=unused");
        builder.UseSetting("EventBusSettings:HostAddress", "");

        builder.ConfigureServices(services =>
        {
            // Remove ALL descriptors whose service type relates to IdentityDbContext
            // or its options. This ensures the original factory lambda (which calls
            // sp.GetRequiredService<TenantRlsInterceptor>()) is completely gone.
            var dbContextDescriptors = services.Where(d =>
                d.ServiceType == typeof(IdentityDbContext) ||
                d.ServiceType == typeof(DbContextOptions<IdentityDbContext>) ||
                d.ServiceType == typeof(DbContextOptions) ||
                (d.ServiceType.IsGenericType &&
                 d.ServiceType.GetGenericTypeDefinition() == typeof(IDbContextOptionsConfiguration<>) &&
                 d.ServiceType.GetGenericArguments()[0] == typeof(IdentityDbContext))
            ).ToList();

            foreach (var descriptor in dbContextDescriptors)
            {
                services.Remove(descriptor);
            }

            // Remove TenantRlsInterceptor (not needed for InMemory)
            RemoveService<TenantRlsInterceptor>(services);

            // Re-add IdentityDbContext backed by InMemory provider
            services.AddDbContext<IdentityDbContext>(options =>
                options.UseInMemoryDatabase(_dbName));

            // Replace IIntegrationEventPublisher with NoOp
            RemoveService<IIntegrationEventPublisher>(services);
            services.AddSingleton<IIntegrationEventPublisher, NoOpTestEventPublisher>();

            // Seed test data
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
            db.Database.EnsureCreated();
            SeedTestData(db, scope.ServiceProvider.GetRequiredService<IPasswordService>());
        });

        // Use "Testing" environment to avoid scope validation that rejects
        // Singleton Mediator handlers consuming Scoped repositories.
        builder.UseEnvironment("Testing");
    }

    private static void RemoveService<T>(IServiceCollection services)
    {
        var descriptors = services.Where(d => d.ServiceType == typeof(T)).ToList();
        foreach (var descriptor in descriptors)
        {
            services.Remove(descriptor);
        }
    }

    private static void SeedTestData(IdentityDbContext db, IPasswordService passwordService)
    {
        // Permissions are already seeded via HasData in OnModelCreating.
        // Seed admin user with bcrypt password.
        var hashedPassword = passwordService.Hash(AdminPassword);

        var adminUser = User.Create(
            TestTenantId,
            AdminUsername,
            hashedPassword,
            AdminEmail,
            "Admin",
            "User",
            null,
            null,
            AdminUserId);

        // Use reflection to set the deterministic Id
        typeof(User).GetProperty(nameof(User.Id))!.SetValue(adminUser, AdminUserId);

        db.Users.Add(adminUser);
        db.SaveChanges();

        // Create a system role "Super Admin" with all permissions
        var superAdminRole = Role.Create(TestTenantId, "Super Admin", "Full access", AdminUserId);
        typeof(Role).GetProperty(nameof(Role.IsSystem))!.SetValue(superAdminRole, true);
        db.Roles.Add(superAdminRole);
        db.SaveChanges();

        // Link all permissions to Super Admin role
        var allPermissions = db.Permissions.ToList();
        foreach (var perm in allPermissions)
        {
            db.RolePermissions.Add(RolePermission.Create(superAdminRole.Id, perm.Id));
        }
        db.SaveChanges();

        // Assign admin user to Super Admin role
        db.UserRoles.Add(UserRole.Create(AdminUserId, superAdminRole.Id));
        db.SaveChanges();
    }
}

/// <summary>
/// NoOp event publisher for E2E tests.
/// </summary>
internal sealed class NoOpTestEventPublisher : IIntegrationEventPublisher
{
    public Task PublishAsync<T>(T @event, CancellationToken ct = default) where T : class
        => Task.CompletedTask;
}
