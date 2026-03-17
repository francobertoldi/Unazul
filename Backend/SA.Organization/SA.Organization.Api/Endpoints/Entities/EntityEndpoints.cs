using Mediator;
using SA.Organization.Api.Mappers.Entities;
using SA.Organization.Api.ViewModels.Entities;
using SA.Organization.Application.Commands.Entities;
using SA.Organization.Application.Queries.Entities;
using Shared.Auth;
using Shared.Pagination;

namespace SA.Organization.Api.Endpoints.Entities;

public static class EntityEndpoints
{
    public static void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/v1/entities")
            .WithTags("Entities")
            .WithOpenApi()
            .RequireAuthorization();

        // List entities
        group.MapGet("/", async (
            int? page,
            int? page_size,
            string? search,
            string? status,
            string? type,
            string? sort,
            string? order,
            IMediator mediator,
            ICurrentUser currentUser) =>
        {
            var result = await mediator.Send(new ListEntitiesQuery(
                page ?? 1,
                page_size ?? 20,
                search,
                status,
                type,
                sort,
                order));

            return Results.Ok(new PagedResult<EntityListResponse>(
                EntityMapper.ToEntityListResponses(result.Items),
                result.Total,
                result.Page,
                result.PageSize));
        })
        .Produces<PagedResult<EntityListResponse>>(200);

        // Create entity
        group.MapPost("/", async (
            CreateEntityRequest request,
            IMediator mediator,
            ICurrentUser currentUser) =>
        {
            var result = await mediator.Send(new CreateEntityCommand(
                currentUser.TenantId,
                request.Name,
                request.Cuit,
                request.Type,
                request.Status,
                request.Address,
                request.City,
                request.Province,
                request.ZipCode,
                request.Country,
                request.Phone,
                request.Email,
                request.Channels));

            return Results.Created($"/api/v1/entities/{result.Id}",
                EntityMapper.ToEntityDetailResponse(result));
        })
        .Produces<EntityDetailResponse>(201);

        // Get entity detail
        group.MapGet("/{id:guid}", async (
            Guid id,
            IMediator mediator) =>
        {
            var result = await mediator.Send(new GetEntityDetailQuery(id));
            return Results.Ok(EntityMapper.ToEntityDetailResponse(result));
        })
        .Produces<EntityDetailResponse>(200);

        // Update entity
        group.MapPut("/{id:guid}", async (
            Guid id,
            UpdateEntityRequest request,
            IMediator mediator) =>
        {
            var result = await mediator.Send(new UpdateEntityCommand(
                id,
                request.Name,
                request.Type,
                request.Status,
                request.Address,
                request.City,
                request.Province,
                request.ZipCode,
                request.Country,
                request.Phone,
                request.Email,
                request.Channels));

            return Results.Ok(EntityMapper.ToEntityDetailResponse(result));
        })
        .Produces<EntityDetailResponse>(200);

        // Delete entity
        group.MapDelete("/{id:guid}", async (
            Guid id,
            IMediator mediator,
            ICurrentUser currentUser) =>
        {
            await mediator.Send(new DeleteEntityCommand(id, currentUser.UserId));
            return Results.NoContent();
        })
        .Produces(204);

        // Export entities
        group.MapGet("/export", async (
            string? format,
            string? search,
            string? status,
            string? type,
            IMediator mediator) =>
        {
            var bytes = await mediator.Send(new ExportEntitiesQuery(
                format ?? "xlsx",
                search,
                status,
                type));

            var isXlsx = string.Equals(format, "xlsx", StringComparison.OrdinalIgnoreCase)
                         || string.IsNullOrWhiteSpace(format);

            var contentType = isXlsx
                ? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                : "text/csv";

            var fileName = isXlsx ? "entities.xlsx" : "entities.csv";

            return Results.File(bytes, contentType, fileName);
        })
        .Produces<byte[]>(200);
    }
}
