using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SA.Operations.Application.Interfaces;
using SA.Operations.Infrastructure.Options;
using SA.Operations.Infrastructure.Services;

namespace SA.Operations.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Options
        services.Configure<StorageSettings>(configuration.GetSection(StorageSettings.SectionName));
        services.Configure<CatalogServiceSettings>(configuration.GetSection(CatalogServiceSettings.SectionName));
        services.Configure<ConfigServiceSettings>(configuration.GetSection(ConfigServiceSettings.SectionName));

        // Services
        services.AddScoped<IFileStorageService, FileStorageService>();
        services.AddScoped<ICatalogServiceClient, CatalogServiceClient>();
        services.AddScoped<IConfigServiceClient, ConfigServiceClient>();

        return services;
    }
}
