using SA.Operations.Api.ViewModels.Beneficiaries;
using SA.Operations.Domain.Entities;

namespace SA.Operations.Api.Mappers.Beneficiaries;

public static class BeneficiaryMapper
{
    public static BeneficiaryResponse ToResponse(Beneficiary b)
    {
        return new BeneficiaryResponse(
            b.Id,
            b.FirstName,
            b.LastName,
            b.Relationship,
            b.Percentage);
    }
}
