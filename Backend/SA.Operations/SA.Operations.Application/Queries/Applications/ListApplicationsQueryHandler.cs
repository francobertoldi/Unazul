using Mediator;
using SA.Operations.Application.Dtos.Applications;
using SA.Operations.DataAccess.Interface.Repositories;
using SA.Operations.Domain.Enums;
using Shared.Pagination;

namespace SA.Operations.Application.Queries.Applications;

public sealed class ListApplicationsQueryHandler(
    IApplicationRepository applicationRepository,
    IApplicantRepository applicantRepository) : IQueryHandler<ListApplicationsQuery, PagedResult<ApplicationListDto>>
{
    public async ValueTask<PagedResult<ApplicationListDto>> Handle(ListApplicationsQuery query, CancellationToken ct)
    {
        ApplicationStatus? statusFilter = null;
        if (!string.IsNullOrWhiteSpace(query.Status) && Enum.TryParse<ApplicationStatus>(query.Status, true, out var parsed))
        {
            statusFilter = parsed;
        }

        var pagination = new PaginationRequest(query.Page, query.PageSize, query.SortBy, query.SortDir ?? "desc");

        var result = await applicationRepository.ListAsync(
            pagination.Skip,
            pagination.ClampedPageSize,
            query.Search,
            statusFilter,
            query.EntityId,
            pagination.Sort,
            query.SortDir ?? "desc",
            ct);

        var applicantIds = result.Items.Select(a => a.ApplicantId).Distinct();
        var applicants = await applicantRepository.GetByIdsAsync(applicantIds, ct);

        var dtos = new List<ApplicationListDto>(result.Items.Count);
        foreach (var app in result.Items)
        {
            var applicantName = applicants.TryGetValue(app.ApplicantId, out var applicant)
                ? $"{applicant.FirstName} {applicant.LastName}"
                : string.Empty;

            dtos.Add(new ApplicationListDto(
                app.Id,
                app.Code,
                app.Status.ToString(),
                app.EntityId,
                app.ProductName,
                app.PlanName,
                applicantName,
                app.CreatedAt));
        }

        return new PagedResult<ApplicationListDto>(dtos, result.Total, query.Page, query.PageSize);
    }
}
