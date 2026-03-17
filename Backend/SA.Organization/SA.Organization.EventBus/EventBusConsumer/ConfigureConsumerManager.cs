using MassTransit;

namespace SA.Organization.EventBus.EventBusConsumer;

public static class ConfigureConsumerManager
{
    public static void ConfigureConsumers(IBusRegistrationConfigurator configurator)
    {
        // Organization only publishes events, no consumers needed yet.
    }
}
