using Mediator;
using SA.Config.Application.Dtos.Workflows;

namespace SA.Config.Application.Commands.Workflows;

public readonly record struct CreateWorkflowCommand(
    Guid TenantId,
    string Name,
    string? Description,
    WorkflowStateInput[] States,
    WorkflowTransitionInput[] Transitions,
    Guid CreatedBy) : ICommand<WorkflowSummaryDto>;

public sealed record WorkflowStateInput(
    string Name,
    string? Label,
    string Type,
    decimal PositionX,
    decimal PositionY,
    StateConfigInput[]? Configs,
    StateFieldInput[]? Fields);

public sealed record StateConfigInput(string Key, string Value);

public sealed record StateFieldInput(string FieldName, string FieldType, bool IsRequired, int SortOrder);

public sealed record WorkflowTransitionInput(
    int FromStateIndex,
    int ToStateIndex,
    string? Label,
    string? Condition,
    int? SlaHours);
