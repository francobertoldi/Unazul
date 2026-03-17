using Mediator;
using SA.Catalog.Api.Mappers;
using SA.Catalog.Api.ViewModels.Coverages;
using SA.Catalog.Application.Commands.Coverages;
using Shared.Auth;
using Shared.Contract.Models;

namespace SA.Catalog.Api.Endpoints.Products;

public static class CoverageEndpoints
{
    public static void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/v1/products/{productId:guid}/plans/{planId:guid}/coverages")
            .WithTags("Plan Coverages")
            .WithOpenApi()
            .RequireAuthorization();

        // Add coverage
        group.MapPost("/", async (
            Guid productId,
            Guid planId,
            AddCoverageRequest request,
            IMediator mediator,
            ICurrentUser currentUser) =>
        {
            var result = await mediator.Send(new AddCoverageCommand(
                currentUser.TenantId,
                planId,
                request.Name,
                request.CoverageType,
                request.SumInsured,
                request.Premium,
                request.GracePeriodDays,
                currentUser.UserId));

            return Results.Created(
                $"/api/v1/products/{productId}/plans/{planId}/coverages/{result}",
                new { id = result });
        })
        .Produces(201)
        .Produces<ErrorResponse>(404)
        .Produces<ErrorResponse>(422);

        // Update coverage
        group.MapPut("/{coverageId:guid}", async (
            Guid productId,
            Guid planId,
            Guid coverageId,
            UpdateCoverageRequest request,
            IMediator mediator,
            ICurrentUser currentUser) =>
        {
            await mediator.Send(new UpdateCoverageCommand(
                currentUser.TenantId,
                coverageId,
                request.SumInsured,
                request.Premium,
                request.GracePeriodDays));

            return Results.Ok(new { id = coverageId });
        })
        .Produces(200)
        .Produces<ErrorResponse>(404)
        .Produces<ErrorResponse>(422);

        // Delete coverage
        group.MapDelete("/{coverageId:guid}", async (
            Guid productId,
            Guid planId,
            Guid coverageId,
            IMediator mediator,
            ICurrentUser currentUser) =>
        {
            await mediator.Send(new DeleteCoverageCommand(currentUser.TenantId, coverageId));
            return Results.NoContent();
        })
        .Produces(204)
        .Produces<ErrorResponse>(404);
    }
}
