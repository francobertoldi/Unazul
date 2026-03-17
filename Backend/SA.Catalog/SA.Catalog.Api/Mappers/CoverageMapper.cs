using Riok.Mapperly.Abstractions;
using SA.Catalog.Api.ViewModels.Coverages;
using SA.Catalog.Application.Dtos;

namespace SA.Catalog.Api.Mappers;

[Mapper]
public static partial class CoverageMapper
{
    public static partial CoverageResponse ToResponse(this CoverageDto dto);
}
