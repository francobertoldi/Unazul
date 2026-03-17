using Mediator;
using SA.Organization.Application.Dtos.Entities;
using SA.Organization.DataAccess.Interface.Repositories;
using Shared.Contract.Exceptions;

namespace SA.Organization.Application.Queries.Branches;

public sealed class ListBranchesByEntityQueryHandler(
    IEntityRepository entityRepository,
    IBranchRepository branchRepository) : IQueryHandler<ListBranchesByEntityQuery, IReadOnlyList<BranchDto>>
{
    public async ValueTask<IReadOnlyList<BranchDto>> Handle(ListBranchesByEntityQuery query, CancellationToken ct)
    {
        var entity = await entityRepository.GetByIdAsync(query.EntityId, ct)
            ?? throw new NotFoundException("ORG_ENTITY_NOT_FOUND", "Entidad no encontrada.");

        var branches = await branchRepository.ListByEntityAsync(query.EntityId, ct);

        return branches
            .Select(b => new BranchDto(
                b.Id,
                b.EntityId,
                b.Name,
                b.Code,
                b.Address,
                b.City,
                b.Province,
                b.ZipCode,
                b.Country,
                b.Phone,
                b.Email,
                b.IsActive,
                b.CreatedAt,
                b.UpdatedAt))
            .ToList();
    }
}
