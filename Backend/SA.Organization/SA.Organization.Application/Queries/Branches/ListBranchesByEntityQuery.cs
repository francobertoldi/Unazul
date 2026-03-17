using Mediator;
using SA.Organization.Application.Dtos.Entities;

namespace SA.Organization.Application.Queries.Branches;

public readonly record struct ListBranchesByEntityQuery(Guid EntityId) : IQuery<IReadOnlyList<BranchDto>>;
