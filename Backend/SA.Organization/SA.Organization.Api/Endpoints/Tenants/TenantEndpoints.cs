using Mediator;
using SA.Organization.Api.Mappers.Tenants;
using SA.Organization.Api.ViewModels.Tenants;
using SA.Organization.Application.Commands.Tenants;
using SA.Organization.Application.Queries.Tenants;
using Shared.Auth;
using Shared.Pagination;

namespace SA.Organization.Api.Endpoints.Tenants;

public static class TenantEndpoints
{
    public static void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/v1/tenants")
            .WithTags("Tenants")
            .WithOpenApi()
            .RequireAuthorization();

        // List tenants
        group.MapGet("/", async (
            int? page,
            int? page_size,
            string? search,
            string? status,
            string? sort,
            string? order,
            IMediator mediator) =>
        {
            var result = await mediator.Send(new ListTenantsQuery(
                page ?? 1,
                page_size ?? 20,
                search,
                status,
                sort,
                order));

            return Results.Ok(new PagedResult<TenantListResponse>(
                TenantMapper.ToTenantListResponses(result.Items),
                result.Total,
                result.Page,
                result.PageSize));
        })
        .Produces<PagedResult<TenantListResponse>>(200);

        // Create tenant
        group.MapPost("/", async (
            CreateTenantRequest request,
            IMediator mediator) =>
        {
            var result = await mediator.Send(new CreateTenantCommand(
                request.Name,
                request.Identifier,
                request.Status,
                request.Address,
                request.City,
                request.Province,
                request.ZipCode,
                request.Country,
                request.Phone,
                request.Email,
                request.LogoUrl));

            return Results.Created($"/api/v1/tenants/{result.Id}",
                TenantMapper.ToTenantListResponse(result));
        })
        .Produces<TenantListResponse>(201);

        // Get tenant detail
        group.MapGet("/{id:guid}", async (
            Guid id,
            IMediator mediator) =>
        {
            var result = await mediator.Send(new GetTenantDetailQuery(id));
            return Results.Ok(TenantMapper.ToTenantDetailResponse(result));
        })
        .Produces<TenantDetailResponse>(200);

        // Update tenant
        group.MapPut("/{id:guid}", async (
            Guid id,
            UpdateTenantRequest request,
            IMediator mediator) =>
        {
            var result = await mediator.Send(new UpdateTenantCommand(
                id,
                request.Name,
                request.Status,
                request.Address,
                request.City,
                request.Province,
                request.ZipCode,
                request.Country,
                request.Phone,
                request.Email,
                request.LogoUrl));

            return Results.Ok(TenantMapper.ToTenantListResponse(result));
        })
        .Produces<TenantListResponse>(200);

        // Delete tenant
        group.MapDelete("/{id:guid}", async (
            Guid id,
            IMediator mediator,
            ICurrentUser currentUser) =>
        {
            await mediator.Send(new DeleteTenantCommand(id, currentUser.UserId));
            return Results.NoContent();
        })
        .Produces(204);

        // Export tenants
        group.MapGet("/export", async (
            string? format,
            string? search,
            string? status,
            IMediator mediator) =>
        {
            var bytes = await mediator.Send(new ExportTenantsQuery(
                format ?? "xlsx",
                search,
                status));

            var isXlsx = string.Equals(format, "xlsx", StringComparison.OrdinalIgnoreCase)
                         || string.IsNullOrWhiteSpace(format);

            var contentType = isXlsx
                ? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                : "text/csv";

            var fileName = isXlsx ? "tenants.xlsx" : "tenants.csv";

            return Results.File(bytes, contentType, fileName);
        })
        .Produces<byte[]>(200);
    }
}
