using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SA.Config.Application.Interfaces;
using SA.Config.Infrastructure.Options;
using SA.Config.Infrastructure.Services;

namespace SA.Config.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<EncryptionOptions>(configuration.GetSection(EncryptionOptions.SectionName));

        services.AddSingleton<IEncryptionService, AesEncryptionService>();

        return services;
    }
}
