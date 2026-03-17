using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SA.Config.DataAccess.EntityFramework.Persistence;
using SA.Config.DataAccess.EntityFramework.Repositories;
using SA.Config.DataAccess.Interface.Repositories;
using Shared.Auth;

namespace SA.Config.DataAccess.EntityFramework.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDataAccessServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ConfigDbContext>((sp, options) =>
        {
            options.UseNpgsql(configuration.GetConnectionString("ConfigDb"))
                   .UseSnakeCaseNamingConvention()
                   .AddInterceptors(sp.GetRequiredService<TenantRlsInterceptor>());
        });

        services.AddScoped<IParameterGroupRepository, ParameterGroupRepository>();
        services.AddScoped<IParameterRepository, ParameterRepository>();
        services.AddScoped<IExternalServiceRepository, ExternalServiceRepository>();
        services.AddScoped<IWorkflowRepository, WorkflowRepository>();
        services.AddScoped<INotificationTemplateRepository, NotificationTemplateRepository>();

        return services;
    }
}
