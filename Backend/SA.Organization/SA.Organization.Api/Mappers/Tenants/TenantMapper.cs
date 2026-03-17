using Riok.Mapperly.Abstractions;
using SA.Organization.Api.ViewModels.Tenants;
using SA.Organization.Application.Dtos.Tenants;

namespace SA.Organization.Api.Mappers.Tenants;

[Mapper]
public static partial class TenantMapper
{
    public static partial TenantListResponse ToTenantListResponse(TenantDto dto);
    public static partial TenantDetailResponse ToTenantDetailResponse(TenantDetailDto dto);

    public static IReadOnlyList<TenantListResponse> ToTenantListResponses(IReadOnlyList<TenantDto> dtos)
    {
        return dtos.Select(ToTenantListResponse).ToList();
    }
}
