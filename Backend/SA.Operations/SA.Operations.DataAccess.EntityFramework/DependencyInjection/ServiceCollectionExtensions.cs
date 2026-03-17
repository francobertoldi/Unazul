using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SA.Operations.DataAccess.EntityFramework.Persistence;
using SA.Operations.DataAccess.EntityFramework.Repositories;
using SA.Operations.DataAccess.Interface.Repositories;
using Shared.Auth;

namespace SA.Operations.DataAccess.EntityFramework.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDataAccessServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<OperationsDbContext>((sp, options) =>
        {
            options.UseNpgsql(configuration.GetConnectionString("OperationsDb"))
                   .UseSnakeCaseNamingConvention()
                   .AddInterceptors(sp.GetRequiredService<TenantRlsInterceptor>());
        });

        services.AddScoped<IApplicationRepository, ApplicationRepository>();
        services.AddScoped<IApplicantRepository, ApplicantRepository>();
        services.AddScoped<IBeneficiaryRepository, BeneficiaryRepository>();
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<IObservationRepository, ObservationRepository>();
        services.AddScoped<ITraceEventRepository, TraceEventRepository>();
        services.AddScoped<IContactRepository, ContactRepository>();
        services.AddScoped<IAddressRepository, AddressRepository>();
        services.AddScoped<ISettlementRepository, SettlementRepository>();

        return services;
    }
}
