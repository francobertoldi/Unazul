namespace SA.Audit.Application.Dtos;

public sealed record AuditLogDto(
    Guid Id,
    Guid TenantId,
    Guid UserId,
    string UserName,
    string Operation,
    string Module,
    string Action,
    string? Detail,
    string IpAddress,
    string? EntityType,
    Guid? EntityId,
    string? ChangesJson,
    DateTimeOffset OccurredAt);
