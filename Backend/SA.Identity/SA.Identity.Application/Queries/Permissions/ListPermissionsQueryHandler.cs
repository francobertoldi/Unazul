using Mediator;
using SA.Identity.DataAccess.Interface.Repositories;

namespace SA.Identity.Application.Queries.Permissions;

public sealed class ListPermissionsQueryHandler(
    IPermissionRepository permissionRepository) : IQueryHandler<ListPermissionsQuery, IReadOnlyList<PermissionGroupDto>>
{
    public async ValueTask<IReadOnlyList<PermissionGroupDto>> Handle(ListPermissionsQuery query, CancellationToken ct)
    {
        var allPermissions = await permissionRepository.GetAllAsync(ct);

        var grouped = allPermissions
            .GroupBy(p => p.Module)
            .OrderBy(g => g.Key)
            .Select(g => new PermissionGroupDto(
                g.Key,
                g.OrderBy(p => p.Code)
                    .Select(p => new PermissionItemDto(p.Id, p.Action, p.Code, p.Description))
                    .ToList()))
            .ToList();

        return grouped;
    }
}
