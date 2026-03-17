namespace SA.Identity.Application.Dtos.Roles;

public sealed record RoleDetailDto(
    Guid Id,
    string Name,
    string? Description,
    bool IsSystem,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyList<RolePermissionDto> Permissions);

public sealed record RolePermissionDto(Guid Id, string Module, string Action, string Code, string? Description);
