using Shared.Contract.Events;

namespace SA.Catalog.Application.Interfaces;

public interface IIntegrationEventPublisher
{
    Task PublishAsync(DomainEvent domainEvent, CancellationToken ct = default);
}
