namespace SA.Config.Api.ViewModels.Workflows;

public sealed record WorkflowDetailResponse(
    Guid Id,
    string Name,
    string? Description,
    string Status,
    int Version,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    Guid CreatedBy,
    Guid UpdatedBy,
    IReadOnlyList<WorkflowStateResponse> States,
    IReadOnlyList<WorkflowTransitionResponse> Transitions);

public sealed record WorkflowStateResponse(
    Guid Id,
    string Name,
    string? Label,
    string Type,
    decimal PositionX,
    decimal PositionY,
    IReadOnlyList<WorkflowStateConfigResponse> Configs,
    IReadOnlyList<WorkflowStateFieldResponse> Fields);

public sealed record WorkflowStateConfigResponse(string Key, string Value);

public sealed record WorkflowStateFieldResponse(string FieldName, string FieldType, bool IsRequired, int SortOrder);

public sealed record WorkflowTransitionResponse(
    Guid Id,
    Guid FromStateId,
    Guid ToStateId,
    string? Label,
    string? Condition,
    int? SlaHours);
