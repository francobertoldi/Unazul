using Mediator;
using SA.Organization.Application.Dtos.Tenants;
using SA.Organization.DataAccess.Interface.Repositories;
using Shared.Pagination;

namespace SA.Organization.Application.Queries.Tenants;

public sealed class ListTenantsQueryHandler(
    ITenantRepository tenantRepository) : IQueryHandler<ListTenantsQuery, PagedResult<TenantDto>>
{
    public async ValueTask<PagedResult<TenantDto>> Handle(ListTenantsQuery query, CancellationToken ct)
    {
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var skip = (query.Page - 1) * pageSize;

        var (tenants, total) = await tenantRepository.ListAsync(
            skip, pageSize, query.Search, query.Status, query.Sort, query.Order, ct);

        var items = tenants
            .Select(t => new TenantDto(
                t.Id,
                t.Name,
                t.Identifier,
                t.Status.ToString(),
                t.Address,
                t.City,
                t.Province,
                t.Phone,
                t.Email,
                t.CreatedAt))
            .ToList();

        return new PagedResult<TenantDto>(items, total, query.Page, pageSize);
    }
}
