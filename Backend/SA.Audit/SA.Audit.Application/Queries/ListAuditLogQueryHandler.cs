using Mediator;
using SA.Audit.Application.Dtos;
using SA.Audit.DataAccess.Interface.Repositories;
using SA.Audit.Domain;
using Shared.Pagination;

namespace SA.Audit.Application.Queries;

public sealed class ListAuditLogQueryHandler(
    IAuditLogRepository repository) : IQueryHandler<ListAuditLogQuery, PagedResult<AuditLogDto>>
{
    public async ValueTask<PagedResult<AuditLogDto>> Handle(ListAuditLogQuery query, CancellationToken ct)
    {
        if (query.Operation is not null && !AuditOperationType.IsValid(query.Operation))
            throw new InvalidOperationException("AUD_INVALID_OPERATION");

        if (query.From.HasValue && query.To.HasValue && query.From.Value > query.To.Value)
            throw new InvalidOperationException("AUD_INVALID_DATE_RANGE");

        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var skip = (page - 1) * pageSize;

        var (items, total) = await repository.ListAsync(
            query.TenantId, skip, pageSize,
            query.UserId, query.Operation, query.Module,
            query.From, query.To,
            query.Sort, query.Order, ct);

        var dtos = items.Select(a => new AuditLogDto(
            a.Id, a.TenantId, a.UserId, a.UserName,
            a.Operation, a.Module, a.Action, a.Detail,
            a.IpAddress, a.EntityType, a.EntityId,
            a.ChangesJson, a.OccurredAt)).ToList();

        return new PagedResult<AuditLogDto>(dtos, total, page, pageSize);
    }
}
