namespace SA.Operations.Api.ViewModels.Beneficiaries;

public sealed record CreateBeneficiaryRequest(
    string FirstName,
    string LastName,
    string Relationship,
    decimal Percentage);

public sealed record UpdateBeneficiaryRequest(
    string FirstName,
    string LastName,
    string Relationship,
    decimal Percentage);
