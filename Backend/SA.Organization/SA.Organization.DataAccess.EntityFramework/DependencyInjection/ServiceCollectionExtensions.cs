using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SA.Organization.DataAccess.EntityFramework.Persistence;
using SA.Organization.DataAccess.EntityFramework.Repositories;
using SA.Organization.DataAccess.Interface.Repositories;
using Shared.Auth;

namespace SA.Organization.DataAccess.EntityFramework.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDataAccessServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<OrganizationDbContext>((sp, options) =>
        {
            options.UseNpgsql(configuration.GetConnectionString("OrganizationDb"))
                   .UseSnakeCaseNamingConvention()
                   .AddInterceptors(sp.GetRequiredService<TenantRlsInterceptor>());
        });

        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IEntityRepository, EntityRepository>();
        services.AddScoped<IEntityChannelRepository, EntityChannelRepository>();
        services.AddScoped<IBranchRepository, BranchRepository>();

        return services;
    }
}
