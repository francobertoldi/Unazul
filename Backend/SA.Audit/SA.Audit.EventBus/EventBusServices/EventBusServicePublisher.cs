using MassTransit;
using SA.Audit.Application.Interfaces;

namespace SA.Audit.EventBus.EventBusServices;

public sealed class EventBusServicePublisher(IPublishEndpoint publishEndpoint) : IIntegrationEventPublisher
{
    public async Task PublishAsync<T>(T @event, CancellationToken ct = default) where T : class
    {
        await publishEndpoint.Publish(@event, ct);
    }
}
