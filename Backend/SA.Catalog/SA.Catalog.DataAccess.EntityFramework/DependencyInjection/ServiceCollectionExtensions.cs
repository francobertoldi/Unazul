using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SA.Catalog.DataAccess.EntityFramework.Persistence;
using SA.Catalog.DataAccess.EntityFramework.Repositories;
using SA.Catalog.DataAccess.Interface.Repositories;
using Shared.Auth;

namespace SA.Catalog.DataAccess.EntityFramework.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDataAccessServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<CatalogDbContext>((sp, options) =>
        {
            options.UseNpgsql(configuration.GetConnectionString("CatalogDb"))
                   .UseSnakeCaseNamingConvention()
                   .AddInterceptors(sp.GetRequiredService<TenantRlsInterceptor>());
        });

        services.AddScoped<IProductFamilyRepository, ProductFamilyRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductPlanRepository, ProductPlanRepository>();
        services.AddScoped<ICoverageRepository, CoverageRepository>();
        services.AddScoped<IProductRequirementRepository, ProductRequirementRepository>();
        services.AddScoped<ICommissionPlanRepository, CommissionPlanRepository>();
        services.AddScoped<IPlanLoanAttributesRepository, PlanLoanAttributesRepository>();
        services.AddScoped<IPlanInsuranceAttributesRepository, PlanInsuranceAttributesRepository>();
        services.AddScoped<IPlanAccountAttributesRepository, PlanAccountAttributesRepository>();
        services.AddScoped<IPlanCardAttributesRepository, PlanCardAttributesRepository>();
        services.AddScoped<IPlanInvestmentAttributesRepository, PlanInvestmentAttributesRepository>();

        return services;
    }
}
