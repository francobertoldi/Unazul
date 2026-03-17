using MassTransit;

namespace SA.Audit.EventBus.EventBusConsumer;

public static class ConfigureConsumerManager
{
    public static void ConfigureConsumers(IBusRegistrationConfigurator bus)
    {
        bus.AddConsumer<DomainEventConsumer>(cfg =>
        {
            cfg.UseMessageRetry(r => r.Exponential(3, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(45), TimeSpan.FromSeconds(5)));
        });
    }
}
