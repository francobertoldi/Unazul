using Mediator;
using SA.Identity.Application.Dtos.Roles;
using SA.Identity.DataAccess.Interface.Repositories;
using Shared.Pagination;

namespace SA.Identity.Application.Queries.Roles;

public sealed class ListRolesQueryHandler(
    IRoleRepository roleRepository) : IQueryHandler<ListRolesQuery, PagedResult<RoleDto>>
{
    private const int MaxExportSize = 10_000;

    public async ValueTask<PagedResult<RoleDto>> Handle(ListRolesQuery query, CancellationToken ct)
    {
        int pageSize;
        int skip;

        if (query.Export)
        {
            pageSize = MaxExportSize;
            skip = 0;
        }
        else
        {
            pageSize = Math.Clamp(query.PageSize, 1, 100);
            skip = (query.Page - 1) * pageSize;
        }

        var (items, total) = await roleRepository.ListAsync(
            query.TenantId,
            skip,
            pageSize,
            query.Search,
            query.SortBy,
            query.SortDir,
            ct);

        var dtos = items.Select(r => new RoleDto(
            r.Id,
            r.Name,
            r.Description,
            r.IsSystem,
            r.RolePermissions.Count,
            r.UserRoles.Count))
            .ToList();

        return new PagedResult<RoleDto>(dtos, total, query.Export ? 1 : query.Page, pageSize);
    }
}
