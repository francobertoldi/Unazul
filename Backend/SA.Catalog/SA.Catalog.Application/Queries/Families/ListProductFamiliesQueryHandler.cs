using Mediator;
using SA.Catalog.Application.Dtos;
using SA.Catalog.DataAccess.Interface.Repositories;
using SA.Catalog.Domain;
using Shared.Pagination;

namespace SA.Catalog.Application.Queries.Families;

public sealed class ListProductFamiliesQueryHandler(
    IProductFamilyRepository familyRepository) : IQueryHandler<ListProductFamiliesQuery, PagedResult<ProductFamilyDto>>
{
    public async ValueTask<PagedResult<ProductFamilyDto>> Handle(ListProductFamiliesQuery query, CancellationToken ct)
    {
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var skip = (query.Page - 1) * pageSize;

        var (families, total) = await familyRepository.ListAsync(
            skip, pageSize, query.Search, ct);

        var familyIds = families.Select(f => f.Id);
        var productCounts = await familyRepository.CountProductsBatchAsync(familyIds, ct);

        var items = families.Select(family => new ProductFamilyDto(
            family.Id, family.Code, family.Description,
            ProductCategory.GetCategoryFromCode(family.Code) ?? "unknown",
            productCounts.GetValueOrDefault(family.Id, 0), family.CreatedAt))
            .ToList();

        return new PagedResult<ProductFamilyDto>(items, total, query.Page, pageSize);
    }
}
