namespace SA.Identity.Api.ViewModels.Roles;

public sealed record CreateRoleRequest(
    string Name,
    string? Description,
    Guid[] PermissionIds);
