namespace SA.Operations.Api.ViewModels.Beneficiaries;

public sealed record BeneficiaryResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Relationship,
    decimal Percentage);
