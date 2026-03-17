using Mediator;

namespace SA.Operations.Application.Commands.Beneficiaries;

public readonly record struct CreateBeneficiaryCommand(
    Guid ApplicationId,
    Guid TenantId,
    string FirstName,
    string LastName,
    string Relationship,
    decimal Percentage) : ICommand<CreateBeneficiaryResult>;

public sealed record CreateBeneficiaryResult(Guid Id, string FullName, decimal Percentage);
