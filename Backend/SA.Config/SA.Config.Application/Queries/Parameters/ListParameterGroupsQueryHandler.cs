using Mediator;
using SA.Config.Application.Dtos.Parameters;
using SA.Config.DataAccess.Interface.Repositories;

namespace SA.Config.Application.Queries.Parameters;

public sealed class ListParameterGroupsQueryHandler(
    IParameterGroupRepository parameterGroupRepository) : IQueryHandler<ListParameterGroupsQuery, IReadOnlyList<CategoryDto>>
{
    public async ValueTask<IReadOnlyList<CategoryDto>> Handle(ListParameterGroupsQuery query, CancellationToken ct)
    {
        var groups = await parameterGroupRepository.GetAllOrderedAsync(ct);

        var categories = groups
            .GroupBy(g => g.Category)
            .OrderBy(c => c.Key)
            .Select(c => new CategoryDto(
                c.Key,
                c.OrderBy(g => g.SortOrder)
                 .Select(g => new ParameterGroupDto(g.Id, g.Code, g.Name, g.Icon, g.SortOrder))
                 .ToList()))
            .ToList();

        return categories;
    }
}
