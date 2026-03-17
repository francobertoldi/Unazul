using Mediator;
using SA.Organization.Application.Interfaces;
using SA.Organization.DataAccess.Interface.Repositories;
using Shared.Contract.Events;
using Shared.Contract.Exceptions;

namespace SA.Organization.Application.Commands.Tenants;

public sealed class DeleteTenantCommandHandler(
    ITenantRepository tenantRepository,
    IIntegrationEventPublisher eventPublisher) : ICommandHandler<DeleteTenantCommand>
{
    public async ValueTask<Unit> Handle(DeleteTenantCommand command, CancellationToken ct)
    {
        var tenant = await tenantRepository.GetByIdAsync(command.Id, ct)
            ?? throw new NotFoundException("ORG_TENANT_NOT_FOUND", "Tenant no encontrado.");

        var entityCount = await tenantRepository.CountEntitiesAsync(command.Id, ct);
        if (entityCount > 0)
        {
            throw new ConflictException("ORG_TENANT_HAS_ENTITIES", "No se puede eliminar un tenant que tiene entidades asociadas.");
        }

        await tenantRepository.DeleteAsync(tenant, ct);
        await tenantRepository.SaveChangesAsync(ct);

        await eventPublisher.PublishAsync(new TenantDeletedEvent(
            tenant.Id,
            tenant.Name,
            command.DeletedBy,
            DateTimeOffset.UtcNow,
            Guid.CreateVersion7()), ct);

        return Unit.Value;
    }
}
