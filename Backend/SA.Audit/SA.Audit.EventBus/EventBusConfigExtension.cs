using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SA.Audit.Application.Interfaces;
using SA.Audit.EventBus.EventBusConsumer;
using SA.Audit.EventBus.EventBusServices;
using SA.Audit.EventBus.Options;

namespace SA.Audit.EventBus;

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
