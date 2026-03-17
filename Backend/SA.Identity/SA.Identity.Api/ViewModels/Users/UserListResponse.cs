namespace SA.Identity.Api.ViewModels.Users;

public sealed record UserListResponse(
    IReadOnlyList<UserResponse> Items,
    int Total,
    int Page,
    int PageSize);
