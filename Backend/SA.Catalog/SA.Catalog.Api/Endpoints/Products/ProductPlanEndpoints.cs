using Mediator;
using SA.Catalog.Api.Mappers;
using SA.Catalog.Api.ViewModels.Plans;
using SA.Catalog.Application.Commands.Plans;
using Shared.Auth;
using Shared.Contract.Models;

namespace SA.Catalog.Api.Endpoints.Products;

public static class ProductPlanEndpoints
{
    public static void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/v1/products/{productId:guid}/plans")
            .WithTags("Product Plans")
            .WithOpenApi()
            .RequireAuthorization();

        // Create plan
        group.MapPost("/", async (
            Guid productId,
            CreateProductPlanRequest request,
            IMediator mediator,
            ICurrentUser currentUser) =>
        {
            var result = await mediator.Send(new CreateProductPlanCommand(
                currentUser.TenantId,
                productId,
                request.Name,
                request.Code,
                request.Price,
                request.Currency,
                request.Installments,
                request.CommissionPlanId,
                request.LoanAttributes,
                request.InsuranceAttributes,
                request.AccountAttributes,
                request.CardAttributes,
                request.InvestmentAttributes,
                request.Coverages?.Select(c => new CoverageInput(
                    c.Name, c.CoverageType, c.SumInsured, c.Premium, c.GracePeriodDays)).ToArray(),
                currentUser.UserId));

            return Results.Created($"/api/v1/products/{productId}/plans/{result}", new { id = result });
        })
        .Produces(201)
        .Produces<ErrorResponse>(404)
        .Produces<ErrorResponse>(422);

        // Update plan
        group.MapPut("/{planId:guid}", async (
            Guid productId,
            Guid planId,
            UpdateProductPlanRequest request,
            IMediator mediator,
            ICurrentUser currentUser) =>
        {
            await mediator.Send(new UpdateProductPlanCommand(
                currentUser.TenantId,
                planId,
                productId,
                request.Name,
                request.Code,
                request.Price,
                request.Currency,
                request.Installments,
                request.CommissionPlanId,
                request.LoanAttributes,
                request.InsuranceAttributes,
                request.AccountAttributes,
                request.CardAttributes,
                request.InvestmentAttributes,
                request.Coverages,
                currentUser.UserId));

            return Results.Ok(new { id = planId });
        })
        .Produces(200)
        .Produces<ErrorResponse>(404)
        .Produces<ErrorResponse>(422);

        // Delete plan
        group.MapDelete("/{planId:guid}", async (
            Guid productId,
            Guid planId,
            IMediator mediator,
            ICurrentUser currentUser) =>
        {
            await mediator.Send(new DeleteProductPlanCommand(currentUser.TenantId, planId));
            return Results.NoContent();
        })
        .Produces(204)
        .Produces<ErrorResponse>(404);
    }
}
