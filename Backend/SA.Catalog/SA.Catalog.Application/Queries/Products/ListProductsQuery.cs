using Mediator;
using SA.Catalog.Application.Dtos;
using Shared.Pagination;

namespace SA.Catalog.Application.Queries.Products;

public readonly record struct ListProductsQuery(
    Guid TenantId, int Page, int PageSize,
    string? Search, string? Status, Guid? FamilyId, Guid? EntityId,
    string? SortBy, string Order) : IQuery<PagedResult<ProductListDto>>;
