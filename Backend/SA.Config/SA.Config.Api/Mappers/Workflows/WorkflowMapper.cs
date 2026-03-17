using Riok.Mapperly.Abstractions;
using SA.Config.Api.ViewModels.Workflows;
using SA.Config.Application.Dtos.Workflows;

namespace SA.Config.Api.Mappers.Workflows;

[Mapper]
public static partial class WorkflowMapper
{
    public static partial WorkflowListItemResponse ToListItemResponse(WorkflowListDto dto);
    public static partial WorkflowDetailResponse ToDetailResponse(WorkflowDetailDto dto);
    public static partial WorkflowStateResponse ToStateResponse(WorkflowStateDto dto);
    public static partial WorkflowStateConfigResponse ToStateConfigResponse(WorkflowStateConfigDto dto);
    public static partial WorkflowStateFieldResponse ToStateFieldResponse(WorkflowStateFieldDto dto);
    public static partial WorkflowTransitionResponse ToTransitionResponse(WorkflowTransitionDto dto);
    public static partial ActivateResponse ToActivateResponse(WorkflowSummaryDto dto);

    public static WorkflowListResponse ToListResponse(
        IReadOnlyList<WorkflowListDto> items, int total, int page, int pageSize)
    {
        return new WorkflowListResponse(
            items.Select(ToListItemResponse).ToList(),
            total,
            page,
            pageSize);
    }
}
