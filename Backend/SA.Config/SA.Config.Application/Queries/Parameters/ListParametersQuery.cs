using Mediator;
using SA.Config.Application.Dtos.Parameters;

namespace SA.Config.Application.Queries.Parameters;

public readonly record struct ListParametersQuery(
    Guid GroupId,
    string? ParentKey) : IQuery<IReadOnlyList<ParameterDto>>;
