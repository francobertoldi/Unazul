using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SA.Operations.Application.Interfaces;
using SA.Operations.EventBus.EventBusConsumer;
using SA.Operations.EventBus.EventBusServices;
using SA.Operations.EventBus.Options;

namespace SA.Operations.EventBus;

public static class EventBusOperationsExtension
{
    public static IServiceCollection AddEventBusServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var settings = configuration.GetSection(EventBusSettings.SectionName).Get<EventBusSettings>();

        if (settings is null || string.IsNullOrWhiteSpace(settings.HostAddress))
        {
            services.AddSingleton<IIntegrationEventPublisher, NoOpIntegrationEventPublisher>();
            return services;
        }

        services.AddMassTransit(bus =>
        {
            ConfigureConsumerManager.ConfigureConsumers(bus);

            bus.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(new Uri(settings.HostAddress), h =>
                {
                    h.Username(settings.User);
                    h.Password(settings.Password);
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        services.AddScoped<IIntegrationEventPublisher, EventBusServicePublisher>();

        return services;
    }
}
