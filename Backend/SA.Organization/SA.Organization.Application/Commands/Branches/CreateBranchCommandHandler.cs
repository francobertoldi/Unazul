using Mediator;
using SA.Organization.Application.Dtos.Entities;
using SA.Organization.DataAccess.Interface.Repositories;
using SA.Organization.Domain.Entities;
using Shared.Contract.Exceptions;

namespace SA.Organization.Application.Commands.Branches;

public sealed class CreateBranchCommandHandler(
    IEntityRepository entityRepository,
    IBranchRepository branchRepository) : ICommandHandler<CreateBranchCommand, BranchDto>
{
    public async ValueTask<BranchDto> Handle(CreateBranchCommand command, CancellationToken ct)
    {
        var entity = await entityRepository.GetByIdAsync(command.EntityId, ct)
            ?? throw new NotFoundException("ORG_ENTITY_NOT_FOUND", "Entidad no encontrada.");

        var codeExists = await branchRepository.ExistsByCodeAsync(entity.TenantId, command.Code, ct);
        if (codeExists)
        {
            throw new ConflictException("ORG_DUPLICATE_BRANCH_CODE", "El código de sucursal ya existe.");
        }

        var branch = Branch.Create(
            command.EntityId,
            entity.TenantId,
            command.Name,
            command.Code,
            command.Address,
            command.City,
            command.Province,
            command.ZipCode,
            command.Country,
            command.Phone,
            command.Email,
            command.IsActive);

        await branchRepository.AddAsync(branch, ct);
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
