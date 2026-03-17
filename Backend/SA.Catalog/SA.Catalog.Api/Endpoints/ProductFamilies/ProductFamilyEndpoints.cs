using Mediator;
using SA.Catalog.Api.Mappers;
using SA.Catalog.Api.ViewModels.Families;
using SA.Catalog.Application.Commands.Families;
using SA.Catalog.Application.Queries.Families;
using Shared.Auth;
using Shared.Contract.Models;

namespace SA.Catalog.Api.Endpoints.ProductFamilies;

public static class ProductFamilyEndpoints
{
    public static void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/v1/product-families")
            .WithTags("Product Families")
            .WithOpenApi()
            .RequireAuthorization();

        // List product families (paginated)
        group.MapGet("/", async (
            int? page,
            int? page_size,
            string? search,
            IMediator mediator,
            ICurrentUser currentUser) =>
        {
            var result = await mediator.Send(new ListProductFamiliesQuery(
                currentUser.TenantId,
                page ?? 1,
                page_size ?? 20,
                search));

            return Results.Ok(new
            {
                data = ProductFamilyMapper.ToResponses(result.Items),
                total = result.Total,
                page = result.Page,
                pageSize = result.PageSize
            });
        })
        .Produces(200);

        // Create product family
        group.MapPost("/", async (
            CreateProductFamilyRequest request,
            IMediator mediator,
            ICurrentUser currentUser) =>
        {
            var result = await mediator.Send(new CreateProductFamilyCommand(
                currentUser.TenantId,
                request.Code,
                request.Description,
                currentUser.UserId));

            return Results.Created($"/api/v1/product-families/{result}", new { id = result });
        })
        .Produces(201)
        .Produces<ErrorResponse>(409)
        .Produces<ErrorResponse>(422);

        // Update product family
        group.MapPut("/{id:guid}", async (
            Guid id,
            UpdateProductFamilyRequest request,
            IMediator mediator,
            ICurrentUser currentUser) =>
        {
            await mediator.Send(new UpdateProductFamilyCommand(
                currentUser.TenantId,
                id,
                request.Description,
                currentUser.UserId));

            return Results.Ok(new { id });
        })
        .Produces(200)
        .Produces<ErrorResponse>(404)
        .Produces<ErrorResponse>(422);

        // Delete product family
        group.MapDelete("/{id:guid}", async (
            Guid id,
            IMediator mediator,
            ICurrentUser currentUser) =>
        {
            await mediator.Send(new DeleteProductFamilyCommand(currentUser.TenantId, id));
            return Results.NoContent();
        })
        .Produces(204)
        .Produces<ErrorResponse>(404)
        .Produces<ErrorResponse>(409);
    }
}
