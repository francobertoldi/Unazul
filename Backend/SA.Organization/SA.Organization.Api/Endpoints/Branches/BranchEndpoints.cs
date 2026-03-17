using Mediator;
using SA.Organization.Api.Mappers.Branches;
using SA.Organization.Api.ViewModels.Branches;
using SA.Organization.Application.Commands.Branches;
using SA.Organization.Application.Queries.Branches;
using Shared.Auth;

namespace SA.Organization.Api.Endpoints.Branches;

public static class BranchEndpoints
{
    public static void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/v1/entities/{entityId:guid}/branches")
            .WithTags("Branches")
            .WithOpenApi()
            .RequireAuthorization();

        // List branches by entity
        group.MapGet("/", async (
            Guid entityId,
            IMediator mediator) =>
        {
            var result = await mediator.Send(new ListBranchesByEntityQuery(entityId));
            return Results.Ok(BranchMapper.ToBranchResponses(result));
        })
        .Produces<IReadOnlyList<BranchResponse>>(200);

        // Create branch
        group.MapPost("/", async (
            Guid entityId,
            CreateBranchRequest request,
            IMediator mediator) =>
        {
            var result = await mediator.Send(new CreateBranchCommand(
                entityId,
                request.Name,
                request.Code,
                request.Address,
                request.City,
                request.Province,
                request.ZipCode,
                request.Country,
                request.Phone,
                request.Email,
                request.IsActive));

            return Results.Created(
                $"/api/v1/entities/{entityId}/branches/{result.Id}",
                BranchMapper.ToBranchResponse(result));
        })
        .Produces<BranchResponse>(201);

        // Update branch
        group.MapPut("/{id:guid}", async (
            Guid entityId,
            Guid id,
            UpdateBranchRequest request,
            IMediator mediator) =>
        {
            var result = await mediator.Send(new UpdateBranchCommand(
                entityId,
                id,
                request.Name,
                request.Address,
                request.City,
                request.Province,
                request.ZipCode,
                request.Country,
                request.Phone,
                request.Email,
                request.IsActive));

            return Results.Ok(BranchMapper.ToBranchResponse(result));
        })
        .Produces<BranchResponse>(200);

        // Delete branch
        group.MapDelete("/{id:guid}", async (
            Guid entityId,
            Guid id,
            IMediator mediator,
            ICurrentUser currentUser) =>
        {
            await mediator.Send(new DeleteBranchCommand(entityId, id, currentUser.UserId));
            return Results.NoContent();
        })
        .Produces(204);
    }
}
