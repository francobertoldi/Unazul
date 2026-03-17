using Mediator;
using SA.Organization.Application.Dtos.Entities;
using SA.Organization.DataAccess.Interface.Repositories;
using SA.Organization.Domain.Entities;
using Shared.Contract.Exceptions;

namespace SA.Organization.Application.Queries.Entities;

public sealed class GetEntityDetailQueryHandler(
    IEntityRepository entityRepository) : IQueryHandler<GetEntityDetailQuery, EntityDetailDto>
{
    public async ValueTask<EntityDetailDto> Handle(GetEntityDetailQuery query, CancellationToken ct)
    {
        var entity = await entityRepository.GetByIdWithDetailsAsync(query.Id, ct)
            ?? throw new NotFoundException("ORG_ENTITY_NOT_FOUND", "Entidad no encontrada.");

        return EntityMapper.ToDetailDto(entity);
    }
}
