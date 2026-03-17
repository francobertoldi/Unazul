using Mediator;
using SA.Operations.Application.Dtos.Applications;

namespace SA.Operations.Application.Queries.Applications;

public readonly record struct GetApplicationDetailQuery(
    Guid ApplicationId,
    Guid TenantId) : IQuery<ApplicationDetailDto>;
