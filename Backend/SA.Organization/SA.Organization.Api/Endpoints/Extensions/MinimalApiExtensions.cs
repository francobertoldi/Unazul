using SA.Organization.Api.Endpoints.Branches;
using SA.Organization.Api.Endpoints.Entities;
using SA.Organization.Api.Endpoints.Tenants;

namespace SA.Organization.Api.Endpoints.Extensions;

public static class MinimalApiExtensions
{
    public static WebApplication MapOrganizationEndpoints(this WebApplication app)
    {
        TenantEndpoints.Map(app);
        EntityEndpoints.Map(app);
        BranchEndpoints.Map(app);

        return app;
    }
}
