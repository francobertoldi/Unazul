using Riok.Mapperly.Abstractions;
using SA.Organization.Api.ViewModels.Entities;
using SA.Organization.Application.Dtos.Entities;

namespace SA.Organization.Api.Mappers.Entities;

[Mapper]
public static partial class EntityMapper
{
    public static partial EntityListResponse ToEntityListResponse(EntityDto dto);
    public static partial EntityDetailResponse ToEntityDetailResponse(EntityDetailDto dto);
    public static partial EntityChannelResponse ToEntityChannelResponse(EntityChannelDto dto);

    public static IReadOnlyList<EntityListResponse> ToEntityListResponses(IReadOnlyList<EntityDto> dtos)
    {
        return dtos.Select(ToEntityListResponse).ToList();
    }
}
