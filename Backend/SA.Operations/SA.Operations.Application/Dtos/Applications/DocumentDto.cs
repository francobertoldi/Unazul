namespace SA.Operations.Application.Dtos.Applications;

public sealed record DocumentDto(
    Guid Id,
    string Name,
    string DocumentType,
    string FileUrl,
    string Status,
    DateTimeOffset CreatedAt);
