namespace SA.Operations.Application.Dtos.Applications;

public sealed record BeneficiaryDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Relationship,
    decimal Percentage);
