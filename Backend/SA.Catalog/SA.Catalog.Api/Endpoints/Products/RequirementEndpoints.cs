using Mediator;
using SA.Catalog.Api.ViewModels.Requirements;
using SA.Catalog.Application.Commands.Requirements;
using Shared.Auth;
using Shared.Contract.Models;

namespace SA.Catalog.Api.Endpoints.Products;

public static class RequirementEndpoints
{
    public static void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/v1/products/{productId:guid}/requirements")
            .WithTags("Product Requirements")
            .WithOpenApi()
            .RequireAuthorization();

        // Create requirement
        group.MapPost("/", async (
            Guid productId,
            CreateRequirementRequest request,
            IMediator mediator,
            ICurrentUser currentUser) =>
        {
            var result = await mediator.Send(new CreateRequirementCommand(
                currentUser.TenantId,
                productId,
                request.Name,
                request.Type,
                request.IsMandatory,
                request.Description,
                currentUser.UserId));

            return Results.Created(
                $"/api/v1/products/{productId}/requirements/{result}",
                new { id = result });
        })
        .Produces(201)
        .Produces<ErrorResponse>(404)
        .Produces<ErrorResponse>(422);

        // Update requirement
        group.MapPut("/{requirementId:guid}", async (
            Guid productId,
            Guid requirementId,
            UpdateRequirementRequest request,
            IMediator mediator,
            ICurrentUser currentUser) =>
        {
            await mediator.Send(new UpdateRequirementCommand(
                currentUser.TenantId,
                requirementId,
                productId,
                request.Name,
                request.Type,
                request.IsMandatory,
                request.Description,
                currentUser.UserId));

            return Results.Ok(new { id = requirementId });
        })
        .Produces(200)
        .Produces<ErrorResponse>(404)
        .Produces<ErrorResponse>(422);

        // Delete requirement
        group.MapDelete("/{requirementId:guid}", async (
            Guid productId,
            Guid requirementId,
            IMediator mediator,
            ICurrentUser currentUser) =>
        {
            await mediator.Send(new DeleteRequirementCommand(
                currentUser.TenantId, requirementId, productId));
            return Results.NoContent();
        })
        .Produces(204)
        .Produces<ErrorResponse>(404);
    }
}
