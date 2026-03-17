using Microsoft.Extensions.Logging;
using SA.Catalog.Application.Interfaces;
using Shared.Contract.Events;

namespace SA.Catalog.EventBus;

public sealed class NoOpIntegrationEventPublisher(ILogger<NoOpIntegrationEventPublisher> logger) : IIntegrationEventPublisher
{
    public Task PublishAsync(DomainEvent domainEvent, CancellationToken ct = default)
    {
        logger.LogDebug("NoOp publisher: event {EventType} was not published (EventBus not configured)", nameof(DomainEvent));
        return Task.CompletedTask;
    }
}
