using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SA.Audit.DataAccess.EntityFramework.Persistence;
using SA.Audit.DataAccess.EntityFramework.Repositories;
using SA.Audit.Domain.Entities;
using Xunit;

namespace SA.Audit.Tests.Integration.Repository;

public sealed class AuditLogRepositoryTests : IDisposable
{
    private readonly AuditDbContext _db;
    private readonly AuditLogRepository _sut;

    public AuditLogRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AuditDbContext>()
            .UseInMemoryDatabase("AuditTestDb_" + Guid.NewGuid())
            .Options;

        _db = new AuditDbContext(options);
        _sut = new AuditLogRepository(_db);
    }

    public void Dispose()
    {
        _db.Dispose();
    }

    private static AuditLog CreateSampleAuditLog(
        Guid tenantId,
        string module = "Usuarios",
        string operation = "Crear",
        DateTimeOffset? occurredAt = null) =>
        AuditLog.Create(
            tenantId,
            Guid.NewGuid(),
            "admin",
            operation,
            module,
            "TestAction",
            null,
            "127.0.0.1",
            null,
            null,
            null,
            occurredAt ?? DateTimeOffset.UtcNow.AddMinutes(-10));

    [Fact]
    public async Task TP_AUD_16_Repository_List_Returns_Correct_Page()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        for (int i = 0; i < 15; i++)
        {
            await _sut.AddAsync(CreateSampleAuditLog(tenantId, occurredAt: DateTimeOffset.UtcNow.AddMinutes(-i)));
        }
        await _sut.SaveChangesAsync();

        // Act - get page 2 with page size 5 (skip 5, take 5)
        var (items, total) = await _sut.ListAsync(tenantId, 5, 5);

        // Assert
        total.Should().Be(15);
        items.Should().HaveCount(5);
    }

    [Fact]
    public async Task TP_AUD_19_Repository_List_Filters_By_TenantId()
    {
        // Arrange
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        await _sut.AddAsync(CreateSampleAuditLog(tenantA));
        await _sut.AddAsync(CreateSampleAuditLog(tenantA));
        await _sut.AddAsync(CreateSampleAuditLog(tenantB));
        await _sut.SaveChangesAsync();

        // Act
        var (itemsA, totalA) = await _sut.ListAsync(tenantA, 0, 100);
        var (itemsB, totalB) = await _sut.ListAsync(tenantB, 0, 100);

        // Assert
        totalA.Should().Be(2);
        totalB.Should().Be(1);
        itemsA.Should().OnlyContain(x => x.TenantId == tenantA);
        itemsB.Should().OnlyContain(x => x.TenantId == tenantB);
    }

    [Fact]
    public async Task TP_AUD_01b_Repository_Add_And_List_Roundtrip()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var log = CreateSampleAuditLog(tenantId, module: "Config", operation: "Editar");

        // Act
        await _sut.AddAsync(log);
        await _sut.SaveChangesAsync();

        var (items, total) = await _sut.ListAsync(tenantId, 0, 10);

        // Assert
        total.Should().Be(1);
        items.Should().HaveCount(1);
        items[0].Id.Should().Be(log.Id);
        items[0].Module.Should().Be("Config");
        items[0].Operation.Should().Be("Editar");
    }

    [Fact]
    public async Task TP_AUD_08_Repository_List_Sorts_By_Module_Asc()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        await _sut.AddAsync(CreateSampleAuditLog(tenantId, module: "Zonas"));
        await _sut.AddAsync(CreateSampleAuditLog(tenantId, module: "Auth"));
        await _sut.AddAsync(CreateSampleAuditLog(tenantId, module: "Config"));
        await _sut.SaveChangesAsync();

        // Act
        var (items, _) = await _sut.ListAsync(tenantId, 0, 10, sort: "module", order: "asc");

        // Assert
        items.Should().HaveCount(3);
        items[0].Module.Should().Be("Auth");
        items[1].Module.Should().Be("Config");
        items[2].Module.Should().Be("Zonas");
    }
}
