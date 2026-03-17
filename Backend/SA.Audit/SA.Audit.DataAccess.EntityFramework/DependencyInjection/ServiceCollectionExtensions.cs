using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SA.Audit.DataAccess.EntityFramework.Persistence;
using SA.Audit.DataAccess.EntityFramework.Repositories;
using SA.Audit.DataAccess.Interface.Repositories;

namespace SA.Audit.DataAccess.EntityFramework.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDataAccessServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AuditDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("AuditDb"))
                   .UseSnakeCaseNamingConvention();
        });

        services.AddScoped<IAuditLogRepository, AuditLogRepository>();

        return services;
    }
}
