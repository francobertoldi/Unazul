using Riok.Mapperly.Abstractions;
using SA.Audit.Api.ViewModels.AuditLog;
using SA.Audit.Application.Dtos;
using Shared.Pagination;

namespace SA.Audit.Api.Mappers.AuditLog;

[Mapper]
public static partial class AuditMapper
{
    public static partial AuditLogResponse ToAuditLogResponse(AuditLogDto dto);

    public static partial IReadOnlyList<AuditLogResponse> ToAuditLogResponses(IReadOnlyList<AuditLogDto> dtos);

    public static AuditLogListResponse ToAuditLogListResponse(PagedResult<AuditLogDto> result)
    {
        return new AuditLogListResponse(
            ToAuditLogResponses(result.Items),
            result.Total,
            result.Page,
            result.PageSize);
    }
}
