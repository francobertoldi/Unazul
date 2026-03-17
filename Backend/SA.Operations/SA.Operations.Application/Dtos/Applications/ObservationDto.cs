namespace SA.Operations.Application.Dtos.Applications;

public sealed record ObservationDto(
    Guid Id,
    string ObservationType,
    string Content,
    string UserName,
    DateTimeOffset CreatedAt);
