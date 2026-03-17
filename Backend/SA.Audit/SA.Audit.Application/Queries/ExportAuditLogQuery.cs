using Mediator;

namespace SA.Audit.Application.Queries;

public readonly record struct ExportAuditLogQuery(
    Guid TenantId,
    string Format,
    Guid? UserId = null,
    string? Operation = null,
    string? Module = null,
    DateTimeOffset? From = null,
    DateTimeOffset? To = null) : IQuery<(byte[] Data, string ContentType, string FileName)>;
