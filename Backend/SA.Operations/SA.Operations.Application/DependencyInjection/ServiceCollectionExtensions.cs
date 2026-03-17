using Microsoft.Extensions.DependencyInjection;

namespace SA.Operations.Application.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Mediator.SourceGenerator auto-registers handlers via source generation.
        // No manual handler registration needed.
        return services;
    }
}
