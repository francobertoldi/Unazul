namespace SA.Operations.Application.Dtos.Applications;

public sealed record ApplicationListDto(
    Guid Id,
    string Code,
    string Status,
    Guid EntityId,
    string ProductName,
    string PlanName,
    string ApplicantName,
    DateTimeOffset CreatedAt);
