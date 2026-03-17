using SA.Config.Api.Endpoints.ExternalServices;
using SA.Config.Api.Endpoints.NotificationTemplates;
using SA.Config.Api.Endpoints.Parameters;
using SA.Config.Api.Endpoints.Workflows;

namespace SA.Config.Api.Endpoints.Extensions;

public static class MinimalApiExtensions
{
    public static WebApplication MapConfigEndpoints(this WebApplication app)
    {
        ParameterGroupEndpoints.Map(app);
        ParameterEndpoints.Map(app);
        ExternalServiceEndpoints.Map(app);
        NotificationTemplateEndpoints.Map(app);
        WorkflowEndpoints.Map(app);

        return app;
    }
}
