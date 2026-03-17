namespace SA.Identity.Api.ViewModels.Roles;

public sealed record RoleResponse(
    Guid Id,
    string Name,
    string? Description,
    bool IsSystem,
    int PermissionCount,
    int UserCount);
