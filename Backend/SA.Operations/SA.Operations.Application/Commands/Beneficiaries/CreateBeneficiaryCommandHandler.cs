using Mediator;
using SA.Operations.DataAccess.Interface.Repositories;
using SA.Operations.Domain.Entities;
using Shared.Contract.Exceptions;

namespace SA.Operations.Application.Commands.Beneficiaries;

public sealed class CreateBeneficiaryCommandHandler(
    IApplicationRepository applicationRepository,
    IBeneficiaryRepository beneficiaryRepository) : ICommandHandler<CreateBeneficiaryCommand, CreateBeneficiaryResult>
{
    public async ValueTask<CreateBeneficiaryResult> Handle(CreateBeneficiaryCommand command, CancellationToken ct)
    {
        var app = await applicationRepository.GetByIdAsync(command.ApplicationId, ct);
        if (app is null || app.TenantId != command.TenantId)
            throw new NotFoundException("OPS_APPLICATION_NOT_FOUND", "Solicitud no encontrada.");

        var beneficiary = Beneficiary.Create(
            command.ApplicationId,
            command.TenantId,
            command.FirstName,
            command.LastName,
            command.Relationship,
            command.Percentage);

        await beneficiaryRepository.AddAsync(beneficiary, ct);
        await beneficiaryRepository.SaveChangesAsync(ct);

        return new CreateBeneficiaryResult(beneficiary.Id, $"{beneficiary.FirstName} {beneficiary.LastName}", beneficiary.Percentage);
    }
}
