using Mediator;
using SA.Organization.Application.Dtos.Entities;
using SA.Organization.DataAccess.Interface.Repositories;
using Shared.Contract.Exceptions;

namespace SA.Organization.Application.Commands.Branches;

public sealed class UpdateBranchCommandHandler(
    IBranchRepository branchRepository) : ICommandHandler<UpdateBranchCommand, BranchDto>
{
    public async ValueTask<BranchDto> Handle(UpdateBranchCommand command, CancellationToken ct)
    {
        var branch = await branchRepository.GetByIdAsync(command.Id, ct)
            ?? throw new NotFoundException("ORG_BRANCH_NOT_FOUND", "Sucursal no encontrada.");

        if (branch.EntityId != command.EntityId)
        {
            throw new ValidationException("ORG_BRANCH_NOT_IN_ENTITY", "La sucursal no pertenece a la entidad especificada.");
        }

        branch.Update(
            command.Name,
            command.Address,
            command.City,
            command.Province,
            command.ZipCode,
            command.Country,
            command.Phone,
            command.Email,
            command.IsActive);

        branchRepository.Update(branch);
        await branchRepository.SaveChangesAsync(ct);

        return new BranchDto(
            branch.Id,
            branch.EntityId,
            branch.Name,
            branch.Code,
            branch.Address,
            branch.City,
            branch.Province,
            branch.ZipCode,
            branch.Country,
            branch.Phone,
            branch.Email,
            branch.IsActive,
            branch.CreatedAt,
            branch.UpdatedAt);
    }
}
