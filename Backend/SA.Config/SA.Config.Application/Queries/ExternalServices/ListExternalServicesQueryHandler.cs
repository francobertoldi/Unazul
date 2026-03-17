using Mediator;
using SA.Config.Application.Dtos.ExternalServices;
using SA.Config.DataAccess.Interface.Repositories;

namespace SA.Config.Application.Queries.ExternalServices;

public sealed class ListExternalServicesQueryHandler(
    IExternalServiceRepository externalServiceRepository) : IQueryHandler<ListExternalServicesQuery, IReadOnlyList<ExternalServiceDto>>
{
    public async ValueTask<IReadOnlyList<ExternalServiceDto>> Handle(ListExternalServicesQuery query, CancellationToken ct)
    {
        var services = await externalServiceRepository.GetAllByTenantAsync(ct);

        return services
            .Select(s => new ExternalServiceDto(
                s.Id,
                s.Name,
                s.Description,
                s.Type,
                s.BaseUrl,
                s.Status,
                s.AuthType,
                s.TimeoutMs,
                s.MaxRetries,
                s.LastTestedAt,
                s.LastTestSuccess))
            .ToList();
    }
}
