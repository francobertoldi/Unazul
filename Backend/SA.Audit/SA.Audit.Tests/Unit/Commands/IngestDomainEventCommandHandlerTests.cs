using FluentAssertions;
using NSubstitute;
using SA.Audit.Application.Commands;
using SA.Audit.DataAccess.Interface.Repositories;
using SA.Audit.Domain.Entities;
using Xunit;

namespace SA.Audit.Tests.Unit.Commands;

public sealed class IngestDomainEventCommandHandlerTests
{
    private readonly IAuditLogRepository _repo = Substitute.For<IAuditLogRepository>();
    private readonly IngestDomainEventCommandHandler _sut;

    public IngestDomainEventCommandHandlerTests()
    {
        _sut = new IngestDomainEventCommandHandler(_repo);
    }

    private static IngestDomainEventCommand CreateValidCommand(
        Guid? tenantId = null,
        Guid? userId = null,
        string userName = "admin",
        string operation = "Crear",
        string module = "Usuarios",
        string action = "CrearUsuario",
        string? detail = "some detail",
        string? ipAddress = "192.168.1.1",
        string? entityType = "User",
        Guid? entityId = null,
        string? changesJson = "{}",
        DateTimeOffset? occurredAt = null,
        Guid? correlationId = null) =>
        new(
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
            occurredAt ?? DateTimeOffset.UtcNow.AddMinutes(-1),
            correlationId ?? Guid.NewGuid());

    [Fact]
    public async Task TP_AUD_36_Ingest_Success_Creates_AuditLog()
    {
        // Arrange
        var command = CreateValidCommand();

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _repo.Received(1).AddAsync(Arg.Any<AuditLog>(), Arg.Any<CancellationToken>());
        await _repo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_AUD_37_Ingest_With_Null_Optional_Fields_Succeeds()
    {
        // Arrange
        var command = CreateValidCommand(
            detail: null,
            entityType: null,
            entityId: null,
            changesJson: null);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _repo.Received(1).AddAsync(Arg.Any<AuditLog>(), Arg.Any<CancellationToken>());
        await _repo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_AUD_40_Ingest_With_Empty_TenantId_Throws_MissingFields()
    {
        // Arrange
        var command = CreateValidCommand(tenantId: Guid.Empty);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("AUD_MISSING_REQUIRED_FIELDS");
    }

    [Fact]
    public async Task TP_AUD_41_Ingest_With_Empty_UserId_Throws_MissingFields()
    {
        // Arrange
        var command = CreateValidCommand(userId: Guid.Empty);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("AUD_MISSING_REQUIRED_FIELDS");
    }

    [Fact]
    public async Task TP_AUD_41b_Ingest_With_Null_UserName_Throws_MissingFields()
    {
        // Arrange
        var command = CreateValidCommand(userName: null!);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("AUD_MISSING_REQUIRED_FIELDS");
    }

    [Fact]
    public async Task TP_AUD_42_Ingest_With_Invalid_Operation_Throws()
    {
        // Arrange
        var command = CreateValidCommand(operation: "InvalidOp");

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("AUD_INVALID_OPERATION");
    }

    [Fact]
    public async Task TP_AUD_43_Ingest_With_Future_OccurredAt_Throws()
    {
        // Arrange
        var command = CreateValidCommand(occurredAt: DateTimeOffset.UtcNow.AddHours(1));

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("AUD_INVALID_OCCURRED_AT");
    }

    [Fact]
    public async Task TP_AUD_41c_Ingest_With_Null_Module_Throws_MissingFields()
    {
        // Arrange
        var command = CreateValidCommand(module: null!);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("AUD_MISSING_REQUIRED_FIELDS");
    }
}
