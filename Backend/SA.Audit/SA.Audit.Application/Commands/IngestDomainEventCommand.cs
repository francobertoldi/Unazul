using Mediator;

namespace SA.Audit.Application.Commands;

public readonly record struct IngestDomainEventCommand(
    Guid TenantId,
    Guid UserId,
    string UserName,
    string Operation,
    string Module,
    string Action,
    string? Detail,
    string? IpAddress,
    string? EntityType,
    Guid? EntityId,
    string? ChangesJson,
    DateTimeOffset OccurredAt,
    Guid CorrelationId) : ICommand;
