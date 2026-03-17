using MassTransit;
using Mediator;
using SA.Audit.Application.Commands;
using Shared.Contract.Events;

namespace SA.Audit.EventBus.EventBusConsumer;

public sealed class DomainEventConsumer(IMediator mediator) : IConsumer<DomainEvent>
{
    public async Task Consume(ConsumeContext<DomainEvent> context)
    {
        var msg = context.Message;

        var command = new IngestDomainEventCommand(
            msg.TenantId,
            msg.UserId,
            msg.UserName,
            msg.Operation,
            msg.Module,
            msg.Action,
            msg.Detail,
            msg.IpAddress,
            msg.EntityType,
            msg.EntityId,
            msg.ChangesJson,
            msg.OccurredAt,
            msg.CorrelationId);

        await mediator.Send(command, context.CancellationToken);
    }
}
