using Mediator;
using SA.Identity.Api.Mappers.Users;
using SA.Identity.Api.ViewModels.Users;
using SA.Identity.Application.Commands.Users;
using SA.Identity.Application.Queries.Users;
using Shared.Auth;
using Shared.Contract.Enums;

namespace SA.Identity.Api.Endpoints.Users;

public static class UserEndpoints
{
    public static void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/v1/users")
            .WithTags("Users")
            .WithOpenApi()
            .RequireAuthorization();

        // RF-SEC-06: List users
        group.MapGet("/", async (
            int page,
            int page_size,
            string? search,
            UserStatus? status,
            Guid? role,
            Guid? entity_id,
            string? sort_by,
            string? sort_dir,
            bool? export,
            IMediator mediator,
            ICurrentUser currentUser) =>
        {
            var result = await mediator.Send(new ListUsersQuery(
                currentUser.TenantId,
                page,
                page_size,
                search,
                status,
                role,
                entity_id,
                sort_by,
                sort_dir,
                export ?? false));

            return Results.Ok(UserMapper.ToUserListResponse(
                result.Items, result.Total, result.Page, result.PageSize));
        })
        .Produces<UserListResponse>(200);

        // RF-SEC-07: Create user
        group.MapPost("/", async (
            CreateUserRequest request,
            IMediator mediator,
            ICurrentUser currentUser) =>
        {
            var result = await mediator.Send(new CreateUserCommand(
                currentUser.TenantId,
                request.Username,
                request.Password,
                request.Email,
                request.FirstName,
                request.LastName,
                request.EntityId,
                request.EntityName,
                request.RoleIds,
                request.Assignments.Select(a =>
                    new CreateUserAssignmentInput(a.ScopeType, a.ScopeId, a.ScopeName)).ToArray(),
                currentUser.UserId));

            return Results.Created($"/api/v1/users/{result.Id}",
                UserMapper.ToUserDetailResponse(result));
        })
        .Produces<UserDetailResponse>(201);

        // RF-SEC-07: Update user
        group.MapPut("/{id:guid}", async (
            Guid id,
            UpdateUserRequest request,
            IMediator mediator,
            ICurrentUser currentUser) =>
        {
            var result = await mediator.Send(new UpdateUserCommand(
                id,
                currentUser.TenantId,
                request.Email,
                request.FirstName,
                request.LastName,
                request.EntityId,
                request.EntityName,
                request.Status,
                request.Avatar,
                request.RoleIds,
                request.Assignments.Select(a =>
                    new UpdateUserAssignmentInput(a.ScopeType, a.ScopeId, a.ScopeName)).ToArray(),
                currentUser.UserId));

            return Results.Ok(UserMapper.ToUserDetailResponse(result));
        })
        .Produces<UserDetailResponse>(200);

        // RF-SEC-08: Get user detail
        group.MapGet("/{id:guid}", async (
            Guid id,
            IMediator mediator) =>
        {
            var result = await mediator.Send(new GetUserDetailQuery(id));
            return Results.Ok(UserMapper.ToUserDetailResponse(result));
        })
        .Produces<UserDetailResponse>(200);
    }
}
