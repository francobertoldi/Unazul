using Riok.Mapperly.Abstractions;
using SA.Organization.Api.ViewModels.Branches;
using SA.Organization.Application.Dtos.Entities;

namespace SA.Organization.Api.Mappers.Branches;

[Mapper]
public static partial class BranchMapper
{
    public static partial BranchResponse ToBranchResponse(BranchDto dto);

    public static IReadOnlyList<BranchResponse> ToBranchResponses(IReadOnlyList<BranchDto> dtos)
    {
        return dtos.Select(ToBranchResponse).ToList();
    }
}
