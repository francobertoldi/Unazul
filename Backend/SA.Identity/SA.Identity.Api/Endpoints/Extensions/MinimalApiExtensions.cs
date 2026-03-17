using SA.Identity.Api.Endpoints.Auth;
using SA.Identity.Api.Endpoints.Permissions;
using SA.Identity.Api.Endpoints.Roles;
using SA.Identity.Api.Endpoints.Users;

namespace SA.Identity.Api.Endpoints.Extensions;

public static class MinimalApiExtensions
{
    public static WebApplication MapIdentityEndpoints(this WebApplication app)
    {
        AuthEndpoints.Map(app);
        UserEndpoints.Map(app);
        RoleEndpoints.Map(app);
        PermissionEndpoints.Map(app);

        return app;
    }
}
