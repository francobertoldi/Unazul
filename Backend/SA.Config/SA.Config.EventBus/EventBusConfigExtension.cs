using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SA.Config.Application.Interfaces;
using SA.Config.EventBus.EventBusConsumer;
using SA.Config.EventBus.EventBusServices;
using SA.Config.EventBus.Options;

namespace SA.Config.EventBus;

public static class EventBusConfigExtension
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
