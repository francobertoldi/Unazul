using Riok.Mapperly.Abstractions;
using SA.Identity.Api.ViewModels.Roles;
using SA.Identity.Application.Dtos.Roles;

namespace SA.Identity.Api.Mappers.Roles;

[Mapper]
public static partial class RoleMapper
{
    public static partial RoleResponse ToRoleResponse(RoleDto dto);

    public static RoleListResponse ToRoleListResponse(
        IReadOnlyList<RoleDto> items, int total, int page, int pageSize)
    {
        return new RoleListResponse(
            items.Select(ToRoleResponse).ToList(),
            total,
            page,
            pageSize);
    }
}
