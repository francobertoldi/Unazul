namespace SA.Operations.Application.Dtos.Timeline;

public sealed record TimelineDto(
    string CurrentStatus,
    string? WorkflowStage,
    TimelineNodeDto[] Nodes);
