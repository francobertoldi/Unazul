using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SA.Operations.Application.Interfaces;
using SA.Operations.DataAccess.EntityFramework.Persistence;
using Shared.Auth;

namespace SA.Operations.Tests.E2E.Fixtures;

public sealed class OperationsWebAppFactory : WebApplicationFactory<SA.Operations.Api.Program>
{
    public static readonly Guid TestTenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    public static readonly Guid TestUserId = Guid.Parse("00000000-0000-0000-0000-000000000002");
    private const string TestJwtKey = "test-jwt-secret-key-minimum-32-chars-long!!";
    private const string TestIssuer = "SA.Operations.Tests";
    private const string TestAudience = "SA.Unazul.Tests";
    private readonly string _dbName = $"OperationsTestDb_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Remove the real DbContext and interceptor registrations
            RemoveService<DbContextOptions<OperationsDbContext>>(services);
            RemoveService<TenantRlsInterceptor>(services);

            // Remove real external service clients
            RemoveService<IIntegrationEventPublisher>(services);
            RemoveService<ICatalogServiceClient>(services);
            RemoveService<IConfigServiceClient>(services);
            RemoveService<IFileStorageService>(services);

            // Add InMemory database
            services.AddDbContext<OperationsDbContext>(options =>
            {
                options.UseInMemoryDatabase(_dbName);
            });

            // Register test stubs
            services.AddSingleton<IIntegrationEventPublisher, NoOpTestEventPublisher>();
            services.AddSingleton<ICatalogServiceClient, TestCatalogClient>();
            services.AddSingleton<IConfigServiceClient, TestConfigClient>();
            services.AddSingleton<IFileStorageService, TestFileStorageService>();

            // Configure JWT for testing
            services.Configure<Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerOptions>(
                Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme,
                options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = TestIssuer,
                        ValidAudience = TestAudience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestJwtKey))
                    };
                });
        });
    }

    public string GenerateJwtToken(Guid? userId = null, Guid? tenantId = null, string username = "testuser")
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestJwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, (userId ?? TestUserId).ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("tenant_id", (tenantId ?? TestTenantId).ToString()),
            new Claim("username", username),
            new Claim(ClaimTypes.Name, username)
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

    // -- Internal test stubs --

    private sealed class NoOpTestEventPublisher : IIntegrationEventPublisher
    {
        public Task PublishAsync<T>(T @event, CancellationToken ct = default) where T : class
            => Task.CompletedTask;
    }

    private sealed class TestCatalogClient : ICatalogServiceClient
    {
        public Task<CatalogProductResult?> ValidateProductAndPlanAsync(
            Guid productId, Guid planId, CancellationToken ct = default)
        {
            return Task.FromResult<CatalogProductResult?>(
                new CatalogProductResult(productId, "Test Product", planId, "Test Plan", true));
        }

        public Task<IReadOnlyList<CommissionPlanResult>> GetCommissionPlansAsync(
            Guid[] productIds, Guid[] planIds, CancellationToken ct = default)
        {
            return Task.FromResult<IReadOnlyList<CommissionPlanResult>>(
                Array.Empty<CommissionPlanResult>());
        }
    }

    private sealed class TestConfigClient : IConfigServiceClient
    {
        public Task<NotificationTemplateResult?> GetNotificationTemplateAsync(
            Guid templateId, CancellationToken ct = default)
        {
            return Task.FromResult<NotificationTemplateResult?>(
                new NotificationTemplateResult(templateId, "Test Template", "Test Subject", "Hello {{name}}", "Email"));
        }
    }

    private sealed class TestFileStorageService : IFileStorageService
    {
        public Task<string> SaveDocumentAsync(
            Guid tenantId, Guid applicationId, Guid documentId,
            string originalFileName, Stream content, CancellationToken ct = default)
        {
            return Task.FromResult($"/test/documents/{tenantId}/{applicationId}/{documentId}/{originalFileName}");
        }

        public Task DeleteDocumentAsync(string filePath, CancellationToken ct = default)
            => Task.CompletedTask;

        public Task<string?> GenerateSettlementExcelAsync(
            Guid tenantId, Guid settlementId, object summaryData, object itemsData,
            CancellationToken ct = default)
        {
            return Task.FromResult<string?>($"/test/settlements/{tenantId}/{settlementId}/settlement.xlsx");
        }

        public Task<Stream?> GetFileStreamAsync(string filePath, CancellationToken ct = default)
        {
            return Task.FromResult<Stream?>(new MemoryStream(Encoding.UTF8.GetBytes("test-file-content")));
        }
    }
}
