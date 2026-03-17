namespace SA.Config.Api.ViewModels.Workflows;

public sealed record CreateWorkflowRequest(
    string Name,
    string? Description,
    WorkflowStateRequest[] States,
    WorkflowTransitionRequest[] Transitions);

public sealed record WorkflowStateRequest(
    string Name,
    string? Label,
    string Type,
    decimal PositionX,
    decimal PositionY,
    StateConfigRequest[]? Configs,
    StateFieldRequest[]? Fields);

public sealed record StateConfigRequest(string Key, string Value);

public sealed record StateFieldRequest(string FieldName, string FieldType, bool IsRequired, int SortOrder);

public sealed record WorkflowTransitionRequest(
    int FromStateIndex,
    int ToStateIndex,
    string? Label,
    string? Condition,
    int? SlaHours);
