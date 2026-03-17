using Riok.Mapperly.Abstractions;
using SA.Catalog.Api.ViewModels.Families;
using SA.Catalog.Application.Dtos;

namespace SA.Catalog.Api.Mappers;

[Mapper]
public static partial class ProductFamilyMapper
{
    public static partial ProductFamilyResponse ToResponse(this ProductFamilyDto dto);

    public static IReadOnlyList<ProductFamilyResponse> ToResponses(IReadOnlyList<ProductFamilyDto> dtos)
    {
        return dtos.Select(ToResponse).ToList();
    }
}
