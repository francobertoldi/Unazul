namespace SA.Identity.Application.Interfaces;

public interface IIntegrationEventPublisher
{
    Task PublishAsync<T>(T @event, CancellationToken ct = default) where T : class;
}
