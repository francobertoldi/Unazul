using Mediator;
using SA.Identity.Api.Mappers.Roles;
using SA.Identity.Api.ViewModels.Roles;
using SA.Identity.Application.Commands.Roles;
using SA.Identity.Application.Queries.Roles;
using Shared.Auth;

namespace SA.Identity.Api.Endpoints.Roles;

public static class RoleEndpoints
{
    public static void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/v1/roles")
            .WithTags("Roles")
            .WithOpenApi()
            .RequireAuthorization();

        // RF-SEC-09: List roles
        group.MapGet("/", async (
            int page,
            int page_size,
            string? search,
            string? sort_by,
            string? sort_dir,
            bool? export,
            IMediator mediator,
            ICurrentUser currentUser) =>
        {
            var result = await mediator.Send(new ListRolesQuery(
                currentUser.TenantId,
                page,
                page_size,
                search,
                sort_by,
                sort_dir,
                export ?? false));

            return Results.Ok(RoleMapper.ToRoleListResponse(
                result.Items, result.Total, result.Page, result.PageSize));
        })
        .Produces<RoleListResponse>(200);

        // RF-SEC-10: Create role
        group.MapPost("/", async (
            CreateRoleRequest request,
            IMediator mediator,
            ICurrentUser currentUser) =>
        {
            var result = await mediator.Send(new CreateRoleCommand(
                currentUser.TenantId,
                request.Name,
                request.Description,
                request.PermissionIds,
                currentUser.UserId));

            return Results.Created($"/api/v1/roles/{result.Id}", result);
        })
        .Produces(201);

        // RF-SEC-11: Update role
        group.MapPut("/{id:guid}", async (
            Guid id,
            UpdateRoleRequest request,
            IMediator mediator,
            ICurrentUser currentUser) =>
        {
            var result = await mediator.Send(new UpdateRoleCommand(
                id,
                currentUser.TenantId,
                request.Name,
                request.Description,
                request.PermissionIds,
                currentUser.UserId));

            return Results.Ok(result);
        })
        .Produces(200);

        // RF-SEC-12: Delete role
        group.MapDelete("/{id:guid}", async (
            Guid id,
            IMediator mediator,
            ICurrentUser currentUser) =>
        {
            await mediator.Send(new DeleteRoleCommand(
                id,
                currentUser.TenantId,
                currentUser.UserId));

            return Results.NoContent();
        })
        .Produces(204);
    }
}
