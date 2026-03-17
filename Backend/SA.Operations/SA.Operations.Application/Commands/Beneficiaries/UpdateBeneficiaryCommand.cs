using Mediator;

namespace SA.Operations.Application.Commands.Beneficiaries;

public readonly record struct UpdateBeneficiaryCommand(
    Guid BeneficiaryId,
    Guid ApplicationId,
    Guid TenantId,
    string FirstName,
    string LastName,
    string Relationship,
    decimal Percentage) : ICommand<UpdateBeneficiaryResult>;

public sealed record UpdateBeneficiaryResult(Guid Id, string FullName, decimal Percentage);
