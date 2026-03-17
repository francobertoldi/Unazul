using Mediator;
using SA.Organization.Application.Interfaces;
using SA.Organization.DataAccess.Interface.Repositories;
using Shared.Contract.Events;
using Shared.Contract.Exceptions;

namespace SA.Organization.Application.Commands.Branches;

public sealed class DeleteBranchCommandHandler(
    IBranchRepository branchRepository,
    IIntegrationEventPublisher eventPublisher) : ICommandHandler<DeleteBranchCommand>
{
    public async ValueTask<Unit> Handle(DeleteBranchCommand command, CancellationToken ct)
    {
        var branch = await branchRepository.GetByIdAsync(command.Id, ct)
            ?? throw new NotFoundException("ORG_BRANCH_NOT_FOUND", "Sucursal no encontrada.");

        if (branch.EntityId != command.EntityId)
        {
            throw new ValidationException("ORG_BRANCH_NOT_IN_ENTITY", "La sucursal no pertenece a la entidad especificada.");
        }

        await branchRepository.DeleteAsync(branch, ct);
        await branchRepository.SaveChangesAsync(ct);

        await eventPublisher.PublishAsync(new BranchDeletedEvent(
            branch.TenantId,
            branch.EntityId,
            branch.Id,
            branch.Name,
            command.DeletedBy,
            DateTimeOffset.UtcNow,
            Guid.CreateVersion7()), ct);

        return Unit.Value;
    }
}
