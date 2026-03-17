namespace SA.Operations.Application.Dtos.Applications;

public sealed record ContactDto(
    Guid Id,
    string Type,
    string? Email,
    string? PhoneCode,
    string? Phone);
