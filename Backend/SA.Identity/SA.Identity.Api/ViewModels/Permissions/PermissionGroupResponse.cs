namespace SA.Identity.Api.ViewModels.Permissions;

public sealed record PermissionGroupResponse(
    string Module,
    IReadOnlyList<PermissionItemResponse> Permissions);

public sealed record PermissionItemResponse(
    Guid Id,
    string Action,
    string Code,
    string? Description);
