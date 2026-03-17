using MassTransit;
using SA.Catalog.Application.Interfaces;
using Shared.Contract.Events;

namespace SA.Catalog.EventBus;

public sealed class EventBusServicePublisher(IPublishEndpoint publishEndpoint) : IIntegrationEventPublisher
{
    public async Task PublishAsync(DomainEvent domainEvent, CancellationToken ct = default)
    {
        await publishEndpoint.Publish(domainEvent, ct);
    }
}
