namespace SA.Identity.Api.ViewModels.Roles;

public sealed record RoleListResponse(
    IReadOnlyList<RoleResponse> Items,
    int Total,
    int Page,
    int PageSize);
