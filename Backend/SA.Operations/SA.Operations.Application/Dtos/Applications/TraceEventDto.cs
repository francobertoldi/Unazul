namespace SA.Operations.Application.Dtos.Applications;

public sealed record TraceEventDto(
    Guid Id,
    string State,
    string Action,
    string UserName,
    DateTimeOffset OccurredAt,
    TraceEventDetailDto[] Details);
