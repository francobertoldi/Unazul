using Riok.Mapperly.Abstractions;
using SA.Catalog.Api.ViewModels.CommissionPlans;
using SA.Catalog.Application.Dtos;

namespace SA.Catalog.Api.Mappers;

[Mapper]
public static partial class CommissionPlanMapper
{
    public static partial CommissionPlanResponse ToResponse(this CommissionPlanDto dto);

    public static IReadOnlyList<CommissionPlanResponse> ToResponses(IReadOnlyList<CommissionPlanDto> dtos)
    {
        return dtos.Select(ToResponse).ToList();
    }
}
