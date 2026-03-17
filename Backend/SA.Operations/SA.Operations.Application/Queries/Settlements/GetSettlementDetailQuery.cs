using Mediator;
using SA.Operations.Application.Dtos.Settlements;

namespace SA.Operations.Application.Queries.Settlements;

public readonly record struct GetSettlementDetailQuery(
    Guid SettlementId,
    Guid TenantId) : IQuery<SettlementDetailDto>;
