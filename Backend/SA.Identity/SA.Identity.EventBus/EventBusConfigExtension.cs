using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SA.Identity.Application.Interfaces;
using SA.Identity.EventBus.EventBusConsumer;
using SA.Identity.EventBus.EventBusServices;
using SA.Identity.EventBus.Options;

namespace SA.Identity.EventBus;

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
