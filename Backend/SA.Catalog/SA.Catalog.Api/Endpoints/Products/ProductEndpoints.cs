using Mediator;
using SA.Catalog.Api.Mappers;
using SA.Catalog.Api.ViewModels.Products;
using SA.Catalog.Application.Commands.Products;
using SA.Catalog.Application.Queries.Products;
using Shared.Auth;
using Shared.Contract.Models;

namespace SA.Catalog.Api.Endpoints.Products;

public static class ProductEndpoints
{
    public static void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/v1/products")
            .WithTags("Products")
            .WithOpenApi()
            .RequireAuthorization();

        // List products (paginated with filters)
        group.MapGet("/", async (
            int? page,
            int? page_size,
            string? search,
            string? status,
            Guid? family_id,
            Guid? entity_id,
            string? sort_by,
            string? sort_dir,
            IMediator mediator,
            ICurrentUser currentUser) =>
        {
            var result = await mediator.Send(new ListProductsQuery(
                currentUser.TenantId,
                page ?? 1,
                page_size ?? 20,
                search,
                status,
                family_id,
                entity_id,
                sort_by,
                sort_dir ?? "asc"));

            return Results.Ok(new
            {
                data = ProductMapper.ToListResponses(result.Items),
                total = result.Total,
                page = result.Page,
                pageSize = result.PageSize
            });
        })
        .Produces(200);

        // Create product
        group.MapPost("/", async (
            CreateProductRequest request,
            IMediator mediator,
            ICurrentUser currentUser) =>
        {
            var result = await mediator.Send(new CreateProductCommand(
                currentUser.TenantId,
                request.EntityId,
                request.FamilyId,
                request.Name,
                request.Code,
                request.Description,
                request.Status,
                request.ValidFrom,
                request.ValidTo,
                currentUser.UserId));

            return Results.Created($"/api/v1/products/{result}", new { id = result });
        })
        .Produces(201)
        .Produces<ErrorResponse>(404)
        .Produces<ErrorResponse>(409)
        .Produces<ErrorResponse>(422);

        // Get product detail
        group.MapGet("/{id:guid}", async (
            Guid id,
            IMediator mediator,
            ICurrentUser currentUser) =>
        {
            var result = await mediator.Send(new GetProductDetailQuery(currentUser.TenantId, id));
            return Results.Ok(ProductMapper.ToDetailResponse(result));
        })
        .Produces<ProductDetailResponse>(200)
        .Produces<ErrorResponse>(404);

        // Update product
        group.MapPut("/{id:guid}", async (
            Guid id,
            UpdateProductRequest request,
            IMediator mediator,
            ICurrentUser currentUser) =>
        {
            await mediator.Send(new UpdateProductCommand(
                currentUser.TenantId,
                id,
                request.Name,
                request.Code,
                request.Description,
                request.Status,
                request.ValidFrom,
                request.ValidTo,
                currentUser.UserId));

            return Results.Ok(new { id });
        })
        .Produces(200)
        .Produces<ErrorResponse>(404)
        .Produces<ErrorResponse>(422);

        // Delete product
        group.MapDelete("/{id:guid}", async (
            Guid id,
            IMediator mediator,
            ICurrentUser currentUser) =>
        {
            await mediator.Send(new DeleteProductCommand(currentUser.TenantId, id));
            return Results.NoContent();
        })
        .Produces(204)
        .Produces<ErrorResponse>(404)
        .Produces<ErrorResponse>(409);

        // Deprecate product
        group.MapPut("/{id:guid}/deprecate", async (
            Guid id,
            IMediator mediator,
            ICurrentUser currentUser) =>
        {
            await mediator.Send(new DeprecateProductCommand(
                currentUser.TenantId, id, currentUser.UserId));
            return Results.Ok(new { id });
        })
        .Produces(200)
        .Produces<ErrorResponse>(404)
        .Produces<ErrorResponse>(409);

        // Export products
        group.MapGet("/export", async (
            string? format,
            string? search,
            string? status,
            Guid? family_id,
            Guid? entity_id,
            IMediator mediator,
            ICurrentUser currentUser) =>
        {
            var result = await mediator.Send(new ExportProductsQuery(
                currentUser.TenantId,
                format ?? "csv",
                search,
                status,
                family_id,
                entity_id));

            return Results.File(result.Data, result.ContentType, result.FileName);
        })
        .Produces(200)
        .Produces<ErrorResponse>(422);
    }
}
