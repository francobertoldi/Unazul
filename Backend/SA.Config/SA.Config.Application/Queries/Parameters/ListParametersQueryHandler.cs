using Mediator;
using SA.Config.Application.Dtos.Parameters;
using SA.Config.DataAccess.Interface.Repositories;
using Shared.Contract.Exceptions;

namespace SA.Config.Application.Queries.Parameters;

public sealed class ListParametersQueryHandler(
    IParameterGroupRepository parameterGroupRepository,
    IParameterRepository parameterRepository) : IQueryHandler<ListParametersQuery, IReadOnlyList<ParameterDto>>
{
    public async ValueTask<IReadOnlyList<ParameterDto>> Handle(ListParametersQuery query, CancellationToken ct)
    {
        var group = await parameterGroupRepository.GetByIdAsync(query.GroupId, ct);
        if (group is null)
        {
            throw new NotFoundException("CFG_GROUP_NOT_FOUND", "Grupo de parametros no encontrado.");
        }

        var parameters = await parameterRepository.GetByGroupIdAsync(query.GroupId, query.ParentKey, ct);

        return parameters
            .Select(p => new ParameterDto(
                p.Id,
                p.Key,
                p.Value,
                p.Type,
                p.Description,
                p.ParentKey,
                p.Options
                    .OrderBy(o => o.SortOrder)
                    .Select(o => new ParameterOptionDto(o.Id, o.OptionValue, o.OptionLabel, o.SortOrder))
                    .ToList()))
            .ToList();
    }
}
