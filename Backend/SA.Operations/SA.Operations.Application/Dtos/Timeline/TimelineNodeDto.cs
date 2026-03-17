using SA.Operations.Application.Dtos.Applications;

namespace SA.Operations.Application.Dtos.Timeline;

public sealed record TimelineNodeDto(
    string State,
    string Type,
    string? Action,
    string? UserName,
    DateTimeOffset? OccurredAt,
    TraceEventDetailDto[]? Details);
