using Mediator;
using SA.Operations.DataAccess.Interface.Repositories;
using Shared.Contract.Exceptions;

namespace SA.Operations.Application.Commands.Beneficiaries;

public sealed class DeleteBeneficiaryCommandHandler(
    IBeneficiaryRepository beneficiaryRepository) : ICommandHandler<DeleteBeneficiaryCommand, DeleteBeneficiaryResult>
{
    public async ValueTask<DeleteBeneficiaryResult> Handle(DeleteBeneficiaryCommand command, CancellationToken ct)
    {
        var beneficiaries = await beneficiaryRepository.GetByApplicationIdAsync(command.ApplicationId, ct);
        var beneficiary = beneficiaries.FirstOrDefault(b => b.Id == command.BeneficiaryId && b.TenantId == command.TenantId);
        if (beneficiary is null)
            throw new NotFoundException("OPS_BENEFICIARY_NOT_FOUND", "Beneficiario no encontrado.");

        beneficiaryRepository.Delete(beneficiary);
        await beneficiaryRepository.SaveChangesAsync(ct);

        return new DeleteBeneficiaryResult(beneficiary.Id);
    }
}
