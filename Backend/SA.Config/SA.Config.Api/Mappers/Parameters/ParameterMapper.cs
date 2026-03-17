using Riok.Mapperly.Abstractions;
using SA.Config.Api.ViewModels.Parameters;
using SA.Config.Application.Dtos.Parameters;

namespace SA.Config.Api.Mappers.Parameters;

[Mapper]
public static partial class ParameterMapper
{
    public static partial ParameterGroupResponse ToParameterGroupResponse(ParameterGroupDto dto);
    public static partial ParameterResponse ToParameterResponse(ParameterDto dto);
    public static partial ParameterOptionResponse ToParameterOptionResponse(ParameterOptionDto dto);

    public static IReadOnlyList<CategoryResponse> ToCategoryResponses(IReadOnlyList<CategoryDto> dtos)
    {
        return dtos.Select(c => new CategoryResponse(
            c.Name,
            c.Groups.Select(ToParameterGroupResponse).ToList()))
            .ToList();
    }

    public static IReadOnlyList<ParameterResponse> ToParameterResponses(IReadOnlyList<ParameterDto> dtos)
    {
        return dtos.Select(ToParameterResponse).ToList();
    }
}
