using Mediator;
using SA.Catalog.Api.Mappers;
using SA.Catalog.Api.ViewModels.CommissionPlans;
using SA.Catalog.Application.Commands.Commissions;
using SA.Catalog.Application.Queries.Commissions;
using Shared.Auth;
using Shared.Contract.Models;

namespace SA.Catalog.Api.Endpoints.CommissionPlans;

public static class CommissionPlanEndpoints
{
    public static void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/v1/commission-plans")
            .WithTags("Commission Plans")
            .WithOpenApi()
            .RequireAuthorization();

        // List commission plans (paginated)
        group.MapGet("/", async (
            int? page,
            int? page_size,
            string? search,
            IMediator mediator,
            ICurrentUser currentUser) =>
        {
            var result = await mediator.Send(new ListCommissionPlansQuery(
                currentUser.TenantId,
                page ?? 1,
                page_size ?? 20,
                search));

            return Results.Ok(new
            {
                data = CommissionPlanMapper.ToResponses(result.Items),
                total = result.Total,
                page = result.Page,
                pageSize = result.PageSize
            });
        })
        .Produces(200);

        // Create commission plan
        group.MapPost("/", async (
            CreateCommissionPlanRequest request,
            IMediator mediator,
            ICurrentUser currentUser) =>
        {
            var result = await mediator.Send(new CreateCommissionPlanCommand(
                currentUser.TenantId,
                request.Code,
                request.Description,
                request.Type,
                request.Value,
                request.MaxAmount,
                currentUser.UserId));

            return Results.Created($"/api/v1/commission-plans/{result}", new { id = result });
        })
        .Produces(201)
        .Produces<ErrorResponse>(409)
        .Produces<ErrorResponse>(422);

        // Update commission plan
        group.MapPut("/{id:guid}", async (
            Guid id,
            UpdateCommissionPlanRequest request,
            IMediator mediator,
            ICurrentUser currentUser) =>
        {
            await mediator.Send(new UpdateCommissionPlanCommand(
                currentUser.TenantId,
                id,
                request.Code,
                request.Description,
                request.Type,
                request.Value,
                request.MaxAmount,
                currentUser.UserId));

            return Results.Ok(new { id });
        })
        .Produces(200)
        .Produces<ErrorResponse>(404)
        .Produces<ErrorResponse>(422);

        // Delete commission plan
        group.MapDelete("/{id:guid}", async (
            Guid id,
            IMediator mediator,
            ICurrentUser currentUser) =>
        {
            await mediator.Send(new DeleteCommissionPlanCommand(currentUser.TenantId, id));
            return Results.NoContent();
        })
        .Produces(204)
        .Produces<ErrorResponse>(404);
    }
}
