namespace SA.Audit.Api.ViewModels.AuditLog;

public sealed record AuditLogResponse(
    Guid Id,
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
    DateTimeOffset OccurredAt);
