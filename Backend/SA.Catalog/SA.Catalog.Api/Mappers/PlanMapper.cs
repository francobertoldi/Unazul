using Riok.Mapperly.Abstractions;
using SA.Catalog.Api.ViewModels.Plans;
using SA.Catalog.Application.Dtos;

namespace SA.Catalog.Api.Mappers;

[Mapper]
public static partial class PlanMapper
{
    public static partial PlanResponse ToResponse(this ProductPlanDto dto);
}
