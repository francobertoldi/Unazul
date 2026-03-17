using Mediator;
using SA.Organization.Application.Interfaces;
using SA.Organization.DataAccess.Interface.Repositories;
using Shared.Contract.Events;
using Shared.Contract.Exceptions;

namespace SA.Organization.Application.Commands.Entities;

public sealed class DeleteEntityCommandHandler(
    IEntityRepository entityRepository,
    IIntegrationEventPublisher eventPublisher) : ICommandHandler<DeleteEntityCommand>
{
    public async ValueTask<Unit> Handle(DeleteEntityCommand command, CancellationToken ct)
    {
        var entity = await entityRepository.GetByIdAsync(command.Id, ct)
            ?? throw new NotFoundException("ORG_ENTITY_NOT_FOUND", "Entidad no encontrada.");

        var hasBranches = await entityRepository.HasBranchesAsync(command.Id, ct);
        if (hasBranches)
        {
            throw new ConflictException("ORG_ENTITY_HAS_BRANCHES", "No se puede eliminar una entidad que tiene sucursales asociadas.");
        }

        await entityRepository.DeleteAsync(entity, ct);
        await entityRepository.SaveChangesAsync(ct);

        await eventPublisher.PublishAsync(new EntityDeletedEvent(
            entity.TenantId,
            entity.Id,
            entity.Name,
            command.DeletedBy,
            DateTimeOffset.UtcNow,
            Guid.CreateVersion7()), ct);

        return Unit.Value;
    }
}
