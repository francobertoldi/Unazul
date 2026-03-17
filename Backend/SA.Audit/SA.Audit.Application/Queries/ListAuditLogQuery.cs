using Mediator;
using SA.Audit.Application.Dtos;
using Shared.Pagination;

namespace SA.Audit.Application.Queries;

public readonly record struct ListAuditLogQuery(
    Guid TenantId,
    int Page,
    int PageSize,
    Guid? UserId = null,
    string? Operation = null,
    string? Module = null,
    DateTimeOffset? From = null,
    DateTimeOffset? To = null,
    string Sort = "occurred_at",
    string Order = "desc") : IQuery<PagedResult<AuditLogDto>>;
