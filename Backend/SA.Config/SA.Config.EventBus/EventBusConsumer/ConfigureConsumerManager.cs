using MassTransit;

namespace SA.Config.EventBus.EventBusConsumer;

public static class ConfigureConsumerManager
{
    public static void ConfigureConsumers(IBusRegistrationConfigurator configurator)
    {
        // Register consumers here as they are added.
        // Example: configurator.AddConsumer<EntityDeletedEventConsumer>();
    }
}
