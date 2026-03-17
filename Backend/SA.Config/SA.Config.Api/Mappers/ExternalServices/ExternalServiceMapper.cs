using Riok.Mapperly.Abstractions;
using SA.Config.Api.ViewModels.ExternalServices;
using SA.Config.Application.Dtos.ExternalServices;

namespace SA.Config.Api.Mappers.ExternalServices;

[Mapper]
public static partial class ExternalServiceMapper
{
    public static partial ExternalServiceResponse ToResponse(ExternalServiceDto dto);
    public static partial TestConnectionResponse ToTestConnectionResponse(TestResultDto dto);

    public static List<ExternalServiceResponse> ToResponseList(IReadOnlyList<ExternalServiceDto> dtos)
    {
        return dtos.Select(ToResponse).ToList();
    }
}
