using Riok.Mapperly.Abstractions;
using SA.Config.Api.ViewModels.NotificationTemplates;
using SA.Config.Application.Dtos.NotificationTemplates;

namespace SA.Config.Api.Mappers.NotificationTemplates;

[Mapper]
public static partial class NotificationTemplateMapper
{
    public static partial NotificationTemplateResponse ToResponse(NotificationTemplateDto dto);
    public static partial NotificationTemplateDetailResponse ToDetailResponse(NotificationTemplateDetailDto dto);

    public static NotificationTemplateListResponse ToListResponse(
        IReadOnlyList<NotificationTemplateDto> items, int total, int page, int pageSize)
    {
        return new NotificationTemplateListResponse(
            items.Select(ToResponse).ToList(),
            total,
            page,
            pageSize);
    }
}
