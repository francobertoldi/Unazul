namespace SA.Audit.Api.Endpoints.Extensions;

public static class MinimalApiExtensions
{
    public static WebApplication MapAuditEndpoints(this WebApplication app)
    {
        AuditLog.AuditLogEndpoints.Map(app);
        return app;
    }
}
