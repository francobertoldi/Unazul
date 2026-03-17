using SA.Catalog.Api.Endpoints.CommissionPlans;
using SA.Catalog.Api.Endpoints.ProductFamilies;
using SA.Catalog.Api.Endpoints.Products;

namespace SA.Catalog.Api.Endpoints.Extensions;

public static class MinimalApiExtensions
{
    public static WebApplication MapCatalogEndpoints(this WebApplication app)
    {
        ProductFamilyEndpoints.Map(app);
        ProductEndpoints.Map(app);
        ProductPlanEndpoints.Map(app);
        CoverageEndpoints.Map(app);
        RequirementEndpoints.Map(app);
        CommissionPlanEndpoints.Map(app);

        return app;
    }
}
