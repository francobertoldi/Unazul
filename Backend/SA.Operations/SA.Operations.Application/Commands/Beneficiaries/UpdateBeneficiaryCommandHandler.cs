using Mediator;
using SA.Operations.DataAccess.Interface.Repositories;
using Shared.Contract.Exceptions;

namespace SA.Operations.Application.Commands.Beneficiaries;

public sealed class UpdateBeneficiaryCommandHandler(
    IBeneficiaryRepository beneficiaryRepository) : ICommandHandler<UpdateBeneficiaryCommand, UpdateBeneficiaryResult>
{
    public async ValueTask<UpdateBeneficiaryResult> Handle(UpdateBeneficiaryCommand command, CancellationToken ct)
    {
        var beneficiaries = await beneficiaryRepository.GetByApplicationIdAsync(command.ApplicationId, ct);
        var beneficiary = beneficiaries.FirstOrDefault(b => b.Id == command.BeneficiaryId && b.TenantId == command.TenantId);
        if (beneficiary is null)
            throw new NotFoundException("OPS_BENEFICIARY_NOT_FOUND", "Beneficiario no encontrado.");

        beneficiary.Update(
            command.FirstName,
            command.LastName,
            command.Relationship,
            command.Percentage);

        beneficiaryRepository.Update(beneficiary);
        await beneficiaryRepository.SaveChangesAsync(ct);

        return new UpdateBeneficiaryResult(beneficiary.Id, $"{beneficiary.FirstName} {beneficiary.LastName}", beneficiary.Percentage);
    }
}
