using Mediator;
using SA.Config.Application.Dtos.ExternalServices;

namespace SA.Config.Application.Queries.ExternalServices;

public readonly record struct ListExternalServicesQuery() : IQuery<IReadOnlyList<ExternalServiceDto>>;
