using Riok.Mapperly.Abstractions;
using SA.Identity.Api.ViewModels.Permissions;
using SA.Identity.Application.Queries.Permissions;

namespace SA.Identity.Api.Mappers.Permissions;

[Mapper]
public static partial class PermissionMapper
{
    public static partial PermissionGroupResponse ToPermissionGroupResponse(PermissionGroupDto dto);
    public static partial PermissionItemResponse ToPermissionItemResponse(PermissionItemDto dto);

    public static IReadOnlyList<PermissionGroupResponse> ToPermissionGroupResponses(
        IReadOnlyList<PermissionGroupDto> dtos)
    {
        return dtos.Select(ToPermissionGroupResponse).ToList();
    }
}
