using Mediator;
using SA.Identity.Application.Dtos.Roles;

namespace SA.Identity.Application.Queries.Permissions;

public readonly record struct ListPermissionsQuery() : IQuery<IReadOnlyList<PermissionGroupDto>>;

public sealed record PermissionGroupDto(
    string Module,
    IReadOnlyList<PermissionItemDto> Permissions);

public sealed record PermissionItemDto(
    Guid Id,
    string Action,
    string Code,
    string? Description);
