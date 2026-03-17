using Mediator;
using SA.Operations.Application.Dtos.Settlements;
using SA.Operations.Application.Interfaces;
using SA.Operations.DataAccess.Interface.Repositories;

namespace SA.Operations.Application.Commands.Settlements;

public sealed class PreviewSettlementCommandHandler(
    IApplicationRepository applicationRepository,
    IApplicantRepository applicantRepository,
    ICatalogServiceClient catalogClient) : ICommandHandler<PreviewSettlementCommand, SettlementPreviewDto>
{
    public async ValueTask<SettlementPreviewDto> Handle(PreviewSettlementCommand command, CancellationToken ct)
    {
        // Get approved applications in the date range
        var approvedApps = await applicationRepository.GetApprovedByDateRangeAsync(
            command.TenantId,
            command.EntityId,
            command.DateFrom,
            command.DateTo,
            ct);

        if (approvedApps.Count == 0)
            return new SettlementPreviewDto([], []);

        // Gather unique product/plan combos for commission lookup
        var productIds = approvedApps.Select(a => a.ProductId).Distinct().ToArray();
        var planIds = approvedApps.Select(a => a.PlanId).Distinct().ToArray();

        var commissionPlans = await catalogClient.GetCommissionPlansAsync(productIds, planIds, ct);
        var commissionLookup = commissionPlans.ToDictionary(
            c => (c.ProductId, c.PlanId),
            c => c);

        // Batch-load applicants
        var applicantIds = approvedApps.Select(a => a.ApplicantId).Distinct();
        var applicants = await applicantRepository.GetByIdsAsync(applicantIds, ct);

        // Build preview items
        var items = new List<SettlementPreviewItemDto>();
        foreach (var app in approvedApps)
        {
            var applicantName = applicants.TryGetValue(app.ApplicantId, out var applicant)
                ? $"{applicant.FirstName} {applicant.LastName}"
                : "N/A";

            commissionLookup.TryGetValue((app.ProductId, app.PlanId), out var commission);

            items.Add(new SettlementPreviewItemDto(
                app.Id,
                app.Code,
                applicantName,
                app.ProductName ?? string.Empty,
                app.PlanName ?? string.Empty,
                commission?.CommissionType,
                commission?.CommissionValue,
                commission?.CommissionValue ?? 0m,
                commission?.Currency ?? "ARS",
                commission?.FormulaDescription));
        }

        // Group totals by currency
        var totals = items
            .GroupBy(i => i.Currency)
            .Select(g => new SettlementTotalDto(
                g.Key,
                g.Sum(i => i.CalculatedAmount),
                g.Count()))
            .ToArray();

        return new SettlementPreviewDto(items.ToArray(), totals);
    }
}
