namespace SA.Config.Application.Dtos.Workflows;

public sealed record WorkflowSummaryDto(
    Guid Id,
    string Name,
    string Status,
    int Version,
    DateTime CreatedAt);
