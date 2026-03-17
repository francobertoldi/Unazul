namespace SA.Config.Application.Dtos.Workflows;

public sealed record WorkflowListDto(
    Guid Id,
    string Name,
    string? Description,
    string Status,
    int Version,
    DateTime CreatedAt,
    DateTime UpdatedAt);
