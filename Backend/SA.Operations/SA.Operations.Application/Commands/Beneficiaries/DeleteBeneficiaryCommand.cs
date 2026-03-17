using Mediator;

namespace SA.Operations.Application.Commands.Beneficiaries;

public readonly record struct DeleteBeneficiaryCommand(
    Guid BeneficiaryId,
    Guid ApplicationId,
    Guid TenantId) : ICommand<DeleteBeneficiaryResult>;

public sealed record DeleteBeneficiaryResult(Guid Id);
