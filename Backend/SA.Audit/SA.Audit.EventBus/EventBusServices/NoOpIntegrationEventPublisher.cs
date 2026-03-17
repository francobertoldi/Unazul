using Microsoft.Extensions.Logging;
using SA.Audit.Application.Interfaces;

namespace SA.Audit.EventBus.EventBusServices;

public sealed class NoOpIntegrationEventPublisher(ILogger<NoOpIntegrationEventPublisher> logger) : IIntegrationEventPublisher
{
    public Task PublishAsync<T>(T @event, CancellationToken ct = default) where T : class
    {
        logger.LogDebug("NoOp publisher: event {EventType} was not published (EventBus not configured)", typeof(T).Name);
        return Task.CompletedTask;
    }
}
