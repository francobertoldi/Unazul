using SA.Operations.Api.Endpoints.Applicants;
using SA.Operations.Api.Endpoints.Applications;
using SA.Operations.Api.Endpoints.Beneficiaries;
using SA.Operations.Api.Endpoints.Documents;
using SA.Operations.Api.Endpoints.Messages;
using SA.Operations.Api.Endpoints.Observations;
using SA.Operations.Api.Endpoints.Settlements;

namespace SA.Operations.Api.Endpoints.Extensions;

public static class MinimalApiExtensions
{
    public static WebApplication MapOperationsEndpoints(this WebApplication app)
    {
        ApplicationEndpoints.Map(app);
        ApplicantEndpoints.Map(app);
        BeneficiaryEndpoints.Map(app);
        DocumentEndpoints.Map(app);
        ObservationEndpoints.Map(app);
        MessageEndpoints.Map(app);
        SettlementEndpoints.Map(app);

        return app;
    }
}
