using Mediator;
using SA.Catalog.Application.Dtos;
using SA.Catalog.DataAccess.Interface.Repositories;
using Shared.Contract.Enums;
using Shared.Pagination;

namespace SA.Catalog.Application.Queries.Products;

public sealed class ListProductsQueryHandler(
    IProductRepository productRepository) : IQueryHandler<ListProductsQuery, PagedResult<ProductListDto>>
{
    public async ValueTask<PagedResult<ProductListDto>> Handle(ListProductsQuery query, CancellationToken ct)
    {
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var skip = (query.Page - 1) * pageSize;

        // Validate status if provided
        if (!string.IsNullOrWhiteSpace(query.Status)
            && !Enum.TryParse<ProductStatus>(query.Status, true, out _))
            throw new InvalidOperationException("CAT_INVALID_STATUS");

        // Exclude deprecated by default (RN-CAT-15)
        ProductStatus? parsedStatus = null;
        if (!string.IsNullOrWhiteSpace(query.Status) && Enum.TryParse<ProductStatus>(query.Status, true, out var s))
            parsedStatus = s;
        var excludeDeprecated = !parsedStatus.HasValue;

        var (products, total) = await productRepository.ListAsync(
            skip, pageSize,
            query.Search, parsedStatus, query.FamilyId, query.EntityId,
            excludeDeprecated, query.SortBy ?? "name", query.Order, ct);

        var items = products
            .Select(p => new ProductListDto(
                p.Id, p.Name, p.Code, p.Description,
                p.Status.ToString().ToLowerInvariant(),
                p.FamilyId,
                p.Family?.Code ?? string.Empty,
                p.Family?.Description ?? string.Empty,
                p.EntityId, p.ValidFrom, p.ValidTo,
                p.Plans.Count, p.CreatedAt))
            .ToList();

        return new PagedResult<ProductListDto>(items, total, query.Page, pageSize);
    }
}
