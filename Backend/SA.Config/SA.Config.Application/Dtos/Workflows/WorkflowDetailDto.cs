namespace SA.Config.Application.Dtos.Workflows;

public sealed record WorkflowDetailDto(
    Guid Id,
    string Name,
    string? Description,
    string Status,
    int Version,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    Guid CreatedBy,
    Guid UpdatedBy,
    IReadOnlyList<WorkflowStateDto> States,
    IReadOnlyList<WorkflowTransitionDto> Transitions);

public sealed record WorkflowStateDto(
    Guid Id,
    string Name,
    string? Label,
    string Type,
    decimal PositionX,
    decimal PositionY,
    IReadOnlyList<WorkflowStateConfigDto> Configs,
    IReadOnlyList<WorkflowStateFieldDto> Fields);

public sealed record WorkflowStateConfigDto(string Key, string Value);

public sealed record WorkflowStateFieldDto(string FieldName, string FieldType, bool IsRequired, int SortOrder);

public sealed record WorkflowTransitionDto(
    Guid Id,
    Guid FromStateId,
    Guid ToStateId,
    string? Label,
    string? Condition,
    int? SlaHours);
