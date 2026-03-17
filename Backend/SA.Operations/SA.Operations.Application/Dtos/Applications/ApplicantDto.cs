namespace SA.Operations.Application.Dtos.Applications;

public sealed record ApplicantDto(
    Guid Id,
    string FirstName,
    string LastName,
    string DocumentType,
    string DocumentNumber,
    DateOnly? BirthDate,
    string? Gender,
    string? Occupation,
    int ApplicationCount);
