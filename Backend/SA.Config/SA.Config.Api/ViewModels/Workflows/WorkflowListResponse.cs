namespace SA.Config.Api.ViewModels.Workflows;

public sealed record WorkflowListResponse(
    IReadOnlyList<WorkflowListItemResponse> Items,
    int Total,
    int Page,
    int PageSize);

public sealed record WorkflowListItemResponse(
    Guid Id,
    string Name,
    string? Description,
    string Status,
    int Version,
    DateTime CreatedAt,
    DateTime UpdatedAt);
