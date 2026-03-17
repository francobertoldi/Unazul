using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SA.Config.Application.Interfaces;
using SA.Config.DataAccess.EntityFramework.Persistence;
using Shared.Auth;

namespace SA.Config.Tests.E2E.Fixtures;

public sealed class ConfigWebAppFactory : WebApplicationFactory<SA.Config.Api.Program>
{
    public static readonly Guid TestTenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    public static readonly Guid TestUserId = Guid.Parse("00000000-0000-0000-0000-000000000002");

    private const string TestJwtKey = "test-jwt-secret-key-minimum-32-chars-long!!";
    private const string TestIssuer = "SA.Config.Tests";
    private const string TestAudience = "SA.Unazul.Tests";

    private readonly string _dbName = $"ConfigTestDb_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("Jwt:Key", TestJwtKey);
        builder.UseSetting("Jwt:Issuer", TestIssuer);
        builder.UseSetting("Jwt:Audience", TestAudience);
        builder.UseSetting("ConnectionStrings:ConfigDb", "Host=localhost;Database=unused");
        builder.UseSetting("EventBusSettings:HostAddress", "");
        builder.UseSetting("Encryption:Key", "dGVzdC1lbmNyeXB0aW9uLWtleS0zMmNoYXJz");

        builder.ConfigureServices(services =>
        {
            // Remove ALL descriptors related to ConfigDbContext
            var dbContextDescriptors = services.Where(d =>
                d.ServiceType == typeof(ConfigDbContext) ||
                d.ServiceType == typeof(DbContextOptions<ConfigDbContext>) ||
                d.ServiceType == typeof(DbContextOptions) ||
                (d.ServiceType.IsGenericType &&
                 d.ServiceType.GetGenericTypeDefinition() == typeof(IDbContextOptionsConfiguration<>) &&
                 d.ServiceType.GetGenericArguments()[0] == typeof(ConfigDbContext))
            ).ToList();

            foreach (var descriptor in dbContextDescriptors)
            {
                services.Remove(descriptor);
            }

            // Remove TenantRlsInterceptor (not needed for InMemory)
            RemoveService<TenantRlsInterceptor>(services);

            // Re-add ConfigDbContext backed by InMemory provider
            services.AddDbContext<ConfigDbContext>(options =>
                options.UseInMemoryDatabase(_dbName));

            // Replace IIntegrationEventPublisher with NoOp
            RemoveService<IIntegrationEventPublisher>(services);
            services.AddSingleton<IIntegrationEventPublisher, NoOpTestEventPublisher>();

            // Replace IEncryptionService with pass-through
            RemoveService<IEncryptionService>(services);
            services.AddSingleton<IEncryptionService, PassThroughEncryptionService>();

            // Register IHttpClientFactory (required by TestExternalServiceCommandHandler)
            services.AddHttpClient();

            // Ensure DB is created with seed data
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ConfigDbContext>();
            db.Database.EnsureCreated();
        });

        builder.UseEnvironment("Testing");
    }

    public string GenerateJwtToken(
        Guid? userId = null,
        Guid? tenantId = null,
        string username = "testuser")
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestJwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, (userId ?? TestUserId).ToString()),
            new(JwtRegisteredClaimNames.UniqueName, username),
            new("tenant_id", (tenantId ?? TestTenantId).ToString()),
        };

        var token = new JwtSecurityToken(
            issuer: TestIssuer,
            audience: TestAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static void RemoveService<T>(IServiceCollection services)
    {
        var descriptors = services.Where(d => d.ServiceType == typeof(T)).ToList();
        foreach (var descriptor in descriptors)
        {
            services.Remove(descriptor);
        }
    }
}

internal sealed class NoOpTestEventPublisher : IIntegrationEventPublisher
{
    public Task PublishAsync<T>(T @event, CancellationToken ct = default) where T : class
        => Task.CompletedTask;
}

internal sealed class PassThroughEncryptionService : IEncryptionService
{
    public string Encrypt(string plainText) => $"enc_{plainText}";
    public string Decrypt(string cipherText) => cipherText.StartsWith("enc_")
        ? cipherText[4..]
        : cipherText;
}
