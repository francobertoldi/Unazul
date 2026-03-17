using Microsoft.Extensions.DependencyInjection;
using SA.Catalog.Application.Interfaces;
using SA.Catalog.Infrastructure.Services;

namespace SA.Catalog.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddScoped<IEntityValidationService, EntityValidationService>();

        return services;
    }
}
