namespace SA.Identity.Api.ViewModels.Roles;

public sealed record UpdateRoleRequest(
    string Name,
    string? Description,
    Guid[] PermissionIds);
