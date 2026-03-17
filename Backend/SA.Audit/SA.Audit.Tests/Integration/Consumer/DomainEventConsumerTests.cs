using FluentAssertions;
using MassTransit;
using Mediator;
using NSubstitute;
using SA.Audit.Application.Commands;
using SA.Audit.EventBus.EventBusConsumer;
using Shared.Contract.Events;
using Xunit;

namespace SA.Audit.Tests.Integration.Consumer;

public sealed class DomainEventConsumerTests
{
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly DomainEventConsumer _sut;

    public DomainEventConsumerTests()
    {
        _sut = new DomainEventConsumer(_mediator);
    }

    private static DomainEvent CreateSampleDomainEvent(
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

    private static ConsumeContext<DomainEvent> CreateConsumeContext(
        DomainEvent message,
        CancellationToken ct = default)
    {
        var context = Substitute.For<ConsumeContext<DomainEvent>>();
        context.Message.Returns(message);
        context.CancellationToken.Returns(ct);
        return context;
    }

    [Fact]
    public async Task TP_AUD_36_Consumer_Sends_IngestCommand_On_ValidEvent()
    {
        // Arrange
        var domainEvent = CreateSampleDomainEvent();
        var context = CreateConsumeContext(domainEvent);

        _mediator.Send(Arg.Any<IngestDomainEventCommand>(), Arg.Any<CancellationToken>())
            .Returns(Mediator.Unit.Value);

        // Act
        await _sut.Consume(context);

        // Assert
        await _mediator.Received(1).Send(
            Arg.Any<IngestDomainEventCommand>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_AUD_37_Consumer_Passes_Null_Optional_Fields()
    {
        // Arrange
        var domainEvent = new DomainEvent(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "admin",
            "Crear",
            "Usuarios",
            "CrearUsuario",
            null,
            "192.168.1.1",
            null,
            null,
            null,
            DateTimeOffset.UtcNow.AddMinutes(-1),
            Guid.NewGuid());
        var context = CreateConsumeContext(domainEvent);

        _mediator.Send(Arg.Any<IngestDomainEventCommand>(), Arg.Any<CancellationToken>())
            .Returns(Mediator.Unit.Value);

        // Act
        await _sut.Consume(context);

        // Assert
        await _mediator.Received(1).Send(
            Arg.Is<IngestDomainEventCommand>(c =>
                c.Detail == null &&
                c.EntityType == null &&
                c.EntityId == null &&
                c.ChangesJson == null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_AUD_36b_Consumer_Maps_All_Fields_Correctly()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var entityId = Guid.NewGuid();
        var correlationId = Guid.NewGuid();
        var occurredAt = DateTimeOffset.UtcNow.AddMinutes(-5);

        var domainEvent = CreateSampleDomainEvent(
            tenantId: tenantId,
            userId: userId,
            userName: "testuser",
            operation: "Editar",
            module: "Sucursales",
            action: "EditarSucursal",
            detail: "edit detail",
            ipAddress: "10.0.0.5",
            entityType: "Branch",
            entityId: entityId,
            changesJson: "{\"key\":\"val\"}",
            occurredAt: occurredAt,
            correlationId: correlationId);

        var context = CreateConsumeContext(domainEvent);

        _mediator.Send(Arg.Any<IngestDomainEventCommand>(), Arg.Any<CancellationToken>())
            .Returns(Mediator.Unit.Value);

        // Act
        await _sut.Consume(context);

        // Assert
        await _mediator.Received(1).Send(
            Arg.Is<IngestDomainEventCommand>(c =>
                c.TenantId == tenantId &&
                c.UserId == userId &&
                c.UserName == "testuser" &&
                c.Operation == "Editar" &&
                c.Module == "Sucursales" &&
                c.Action == "EditarSucursal" &&
                c.Detail == "edit detail" &&
                c.IpAddress == "10.0.0.5" &&
                c.EntityType == "Branch" &&
                c.EntityId == entityId &&
                c.ChangesJson == "{\"key\":\"val\"}" &&
                c.OccurredAt == occurredAt &&
                c.CorrelationId == correlationId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_AUD_38_Consumer_Propagates_CancellationToken()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var domainEvent = CreateSampleDomainEvent();
        var context = CreateConsumeContext(domainEvent, cts.Token);

        _mediator.Send(Arg.Any<IngestDomainEventCommand>(), Arg.Any<CancellationToken>())
            .Returns(Mediator.Unit.Value);

        // Act
        await _sut.Consume(context);

        // Assert
        await _mediator.Received(1).Send(
            Arg.Any<IngestDomainEventCommand>(),
            cts.Token);
    }

    [Fact]
    public async Task TP_AUD_39_Consumer_Maps_CorrelationId()
    {
        // Arrange
        var correlationId = Guid.NewGuid();
        var domainEvent = CreateSampleDomainEvent(correlationId: correlationId);
        var context = CreateConsumeContext(domainEvent);

        _mediator.Send(Arg.Any<IngestDomainEventCommand>(), Arg.Any<CancellationToken>())
            .Returns(Mediator.Unit.Value);

        // Act
        await _sut.Consume(context);

        // Assert
        await _mediator.Received(1).Send(
            Arg.Is<IngestDomainEventCommand>(c => c.CorrelationId == correlationId),
            Arg.Any<CancellationToken>());
    }
}
