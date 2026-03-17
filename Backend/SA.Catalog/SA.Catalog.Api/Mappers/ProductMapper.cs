using Riok.Mapperly.Abstractions;
using SA.Catalog.Api.ViewModels.Products;
using SA.Catalog.Application.Dtos;

namespace SA.Catalog.Api.Mappers;

[Mapper]
public static partial class ProductMapper
{
    public static partial ProductListResponse ToListResponse(this ProductListDto dto);
    public static partial ProductDetailResponse ToDetailResponse(this ProductDetailDto dto);

    public static IReadOnlyList<ProductListResponse> ToListResponses(IReadOnlyList<ProductListDto> dtos)
    {
        return dtos.Select(ToListResponse).ToList();
    }
}
