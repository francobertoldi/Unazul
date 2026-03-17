using Mediator;
using SA.Audit.DataAccess.Interface.Repositories;
using SA.Audit.Domain;
using SA.Audit.Domain.Entities;
using Shared.Contract.Exceptions;

namespace SA.Audit.Application.Commands;

public sealed class IngestDomainEventCommandHandler(
    IAuditLogRepository repository) : ICommandHandler<IngestDomainEventCommand>
{
    public async ValueTask<Unit> Handle(IngestDomainEventCommand command, CancellationToken ct)
    {
        if (command.TenantId == Guid.Empty)
            throw new ValidationException("AUD_MISSING_REQUIRED_FIELDS", "Faltan campos obligatorios en el evento.");

        if (command.UserId == Guid.Empty)
            throw new ValidationException("AUD_MISSING_REQUIRED_FIELDS", "Faltan campos obligatorios en el evento.");

        if (string.IsNullOrWhiteSpace(command.UserName))
            throw new ValidationException("AUD_MISSING_REQUIRED_FIELDS", "Faltan campos obligatorios en el evento.");

        if (!AuditOperationType.IsValid(command.Operation))
            throw new InvalidOperationException("AUD_INVALID_OPERATION");
            throw new ValidationException("AUD_INVALID_OPERATION", "Tipo de operacion invalido.");

        if (string.IsNullOrWhiteSpace(command.Module))
            throw new ValidationException("AUD_MISSING_REQUIRED_FIELDS", "Faltan campos obligatorios en el evento.");

        if (string.IsNullOrWhiteSpace(command.Action))
            throw new ValidationException("AUD_MISSING_REQUIRED_FIELDS", "Faltan campos obligatorios en el evento.");

        if (string.IsNullOrWhiteSpace(command.IpAddress))
            throw new ValidationException("AUD_MISSING_REQUIRED_FIELDS", "Faltan campos obligatorios en el evento.");

        if (command.OccurredAt > DateTimeOffset.UtcNow.AddMinutes(5))
            throw new InvalidOperationException("AUD_INVALID_OCCURRED_AT");
            throw new ValidationException("AUD_INVALID_OCCURRED_AT", "La fecha del evento no puede ser futura.");

        var auditLog = AuditLog.Create(
            command.TenantId,
            command.UserId,
            command.UserName,
            command.Operation,
            command.Module,
            command.Action,
            command.Detail,
            command.IpAddress,
            command.EntityType,
            command.EntityId,
            command.ChangesJson,
            command.OccurredAt);

        await repository.AddAsync(auditLog, ct);
        await repository.SaveChangesAsync(ct);

        return Unit.Value;
    }
}
