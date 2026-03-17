using Mediator;
using SA.Config.Application.Dtos.Parameters;

namespace SA.Config.Application.Queries.Parameters;

public readonly record struct ListParameterGroupsQuery() : IQuery<IReadOnlyList<CategoryDto>>;
