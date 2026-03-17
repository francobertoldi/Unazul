namespace SA.Audit.Api.ViewModels.AuditLog;

public sealed record AuditLogListResponse(
    IReadOnlyList<AuditLogResponse> Items,
    int Total,
    int Page,
    int PageSize);
