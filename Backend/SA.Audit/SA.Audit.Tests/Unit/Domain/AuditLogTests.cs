using System.Reflection;
using FluentAssertions;
using SA.Audit.Domain.Entities;
using Xunit;

namespace SA.Audit.Tests.Unit.Domain;

public sealed class AuditLogTests
{
    private static AuditLog CreateSampleAuditLog(
        Guid? tenantId = null,
        Guid? userId = null,
        string userName = "admin",
        string operation = "Crear",
        string module = "Usuarios",
        string action = "CrearUsuario",
        string? detail = "Detalle de prueba",
        string ipAddress = "192.168.1.1",
        string? entityType = "User",
        Guid? entityId = null,
        string? changesJson = "{\"name\":\"test\"}",
        DateTimeOffset? occurredAt = null)
    {
        return AuditLog.Create(
            tenantId ?? Guid.NewGuid(),
            userId ?? Guid.NewGuid(),
            userName,
            operation,
            module,
            action,
            detail,
            ipAddress,
            entityType,
            entityId ?? Guid.NewGuid(),
            changesJson,
            occurredAt ?? DateTimeOffset.UtcNow);
    }

    [Fact]
    public void TP_AUD_36_Create_Returns_AuditLog_With_All_Fields()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var entityId = Guid.NewGuid();
        var occurredAt = DateTimeOffset.UtcNow.AddMinutes(-5);

        // Act
        var log = AuditLog.Create(
            tenantId, userId, "admin", "Crear", "Usuarios", "CrearUsuario",
            "Detalle", "10.0.0.1", "User", entityId, "{}", occurredAt);

        // Assert
        log.Id.Should().NotBeEmpty();
        log.TenantId.Should().Be(tenantId);
        log.UserId.Should().Be(userId);
        log.UserName.Should().Be("admin");
        log.Operation.Should().Be("Crear");
        log.Module.Should().Be("Usuarios");
        log.Action.Should().Be("CrearUsuario");
        log.Detail.Should().Be("Detalle");
        log.IpAddress.Should().Be("10.0.0.1");
        log.EntityType.Should().Be("User");
        log.EntityId.Should().Be(entityId);
        log.ChangesJson.Should().Be("{}");
        log.OccurredAt.Should().Be(occurredAt);
    }

    [Fact]
    public void TP_AUD_37_Create_With_Null_Optional_Fields()
    {
        // Arrange & Act
        var log = AuditLog.Create(
            Guid.NewGuid(), Guid.NewGuid(), "user1", "Login", "Auth", "LoginAction",
            null, "127.0.0.1", null, null, null, DateTimeOffset.UtcNow);

        // Assert
        log.Detail.Should().BeNull();
        log.EntityType.Should().BeNull();
        log.EntityId.Should().BeNull();
        log.ChangesJson.Should().BeNull();
    }

    [Fact]
    public void TP_AUD_36b_Create_Generates_NonEmpty_Id()
    {
        // Arrange & Act
        var log1 = CreateSampleAuditLog();
        var log2 = CreateSampleAuditLog();

        // Assert
        log1.Id.Should().NotBeEmpty();
        log2.Id.Should().NotBeEmpty();
        log1.Id.Should().NotBe(log2.Id);
    }

    [Fact]
    public void TP_AUD_01_AuditLog_Has_No_Public_Setters()
    {
        // Arrange
        var properties = typeof(AuditLog).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        // Act & Assert
        foreach (var prop in properties)
        {
            var setter = prop.GetSetMethod(nonPublic: false);
            setter.Should().BeNull($"Property '{prop.Name}' should not have a public setter");
        }
    }
}
