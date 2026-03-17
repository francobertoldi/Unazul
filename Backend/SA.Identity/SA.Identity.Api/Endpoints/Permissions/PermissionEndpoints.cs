using Mediator;
using SA.Identity.Api.Mappers.Permissions;
using SA.Identity.Api.ViewModels.Permissions;
using SA.Identity.Application.Queries.Permissions;

namespace SA.Identity.Api.Endpoints.Permissions;

public static class PermissionEndpoints
{
    public static void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/v1/permissions")
            .WithTags("Permissions")
            .WithOpenApi()
            .RequireAuthorization();

        // Get all permissions grouped by module
        group.MapGet("/", async (IMediator mediator) =>
        {
            var result = await mediator.Send(new ListPermissionsQuery());
            return Results.Ok(PermissionMapper.ToPermissionGroupResponses(result));
        })
        .Produces<IReadOnlyList<PermissionGroupResponse>>(200);
    }
}
