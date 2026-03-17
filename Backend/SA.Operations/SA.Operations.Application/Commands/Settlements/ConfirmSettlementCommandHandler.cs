using Mediator;
using SA.Operations.Application.Events;
using SA.Operations.Application.Interfaces;
using SA.Operations.DataAccess.Interface.Repositories;
using SA.Operations.Domain.Entities;
using Shared.Contract.Exceptions;

namespace SA.Operations.Application.Commands.Settlements;

public sealed class ConfirmSettlementCommandHandler(
    IApplicationRepository applicationRepository,
    IApplicantRepository applicantRepository,
    ISettlementRepository settlementRepository,
    ICatalogServiceClient catalogClient,
    IFileStorageService fileStorageService,
    IIntegrationEventPublisher eventPublisher) : ICommandHandler<ConfirmSettlementCommand, ConfirmSettlementResult>
{
    public async ValueTask<ConfirmSettlementResult> Handle(ConfirmSettlementCommand command, CancellationToken ct)
    {
        // Get approved applications in the date range
        var approvedApps = await applicationRepository.GetApprovedByDateRangeAsync(
            command.TenantId,
            command.EntityId,
            command.DateFrom,
            command.DateTo,
            ct);

        if (approvedApps.Count == 0)
            throw new ValidationException("OPS_NO_APPROVED_APPLICATIONS", "No hay solicitudes aprobadas en el rango de fechas indicado.");

        // Gather commission plans
        var productIds = approvedApps.Select(a => a.ProductId).Distinct().ToArray();
        var planIds = approvedApps.Select(a => a.PlanId).Distinct().ToArray();
        var commissionPlans = await catalogClient.GetCommissionPlansAsync(productIds, planIds, ct);
        var commissionLookup = commissionPlans.ToDictionary(
            c => (c.ProductId, c.PlanId),
            c => c);

        // Create settlement
        var settlement = Settlement.Create(
            command.TenantId,
            command.SettledBy,
            command.SettledByName,
            approvedApps.Count);

        // Batch-load applicants
        var applicantIds = approvedApps.Select(a => a.ApplicantId).Distinct();
        var applicantMap = await applicantRepository.GetByIdsAsync(applicantIds, ct);

        // Create settlement items
        foreach (var app in approvedApps)
        {
            var applicantName = applicantMap.TryGetValue(app.ApplicantId, out var applicant)
                ? $"{applicant.FirstName} {applicant.LastName}"
                : "N/A";

            commissionLookup.TryGetValue((app.ProductId, app.PlanId), out var commission);

            var item = SettlementItem.Create(
                settlement.Id,
                command.TenantId,
                app.Id,
                app.Code,
                applicantName,
                app.ProductName ?? string.Empty,
                app.PlanName ?? string.Empty,
                commission?.CommissionType,
                commission?.CommissionValue,
                commission?.CommissionValue ?? 0m,
                commission?.Currency ?? "ARS",
                commission?.FormulaDescription);

            settlement.Items.Add(item);
        }

        // Create totals grouped by currency
        var totalsByCurrency = settlement.Items
            .GroupBy(i => i.Currency)
            .Select(g => SettlementTotal.Create(
                settlement.Id,
                command.TenantId,
                g.Key,
                g.Sum(i => i.CalculatedAmount),
                g.Count()))
            .ToList();

        foreach (var total in totalsByCurrency)
            settlement.Totals.Add(total);

        // Persist settlement
        await settlementRepository.AddAsync(settlement, ct);
        await settlementRepository.SaveChangesAsync(ct);

        // Batch transition all approved apps to Settled
        var appIds = approvedApps.Select(a => a.Id).ToList();
        await applicationRepository.BatchTransitionToSettledAsync(appIds, command.SettledBy, ct);

        // Generate Excel
        var summaryData = totalsByCurrency.Select(t => new { t.Currency, t.TotalAmount, t.ItemCount }).ToArray();
        var itemsData = settlement.Items.Select(i => new
        {
            i.AppCode,
            i.ApplicantName,
            i.ProductName,
            i.PlanName,
            i.CommissionType,
            i.CommissionValue,
            i.CalculatedAmount,
            i.Currency,
            i.FormulaDescription
        }).ToArray();

        var excelUrl = await fileStorageService.GenerateSettlementExcelAsync(
            command.TenantId,
            settlement.Id,
            summaryData,
            itemsData,
            ct);

        if (excelUrl is not null)
        {
            settlement.SetExcelUrl(excelUrl);
            settlementRepository.Update(settlement);
            await settlementRepository.SaveChangesAsync(ct);
        }

        // Publish event
        await eventPublisher.PublishAsync(new CommissionsSettledEvent(
            settlement.Id,
            command.TenantId,
            settlement.Items.Count,
            DateTimeOffset.UtcNow,
            Guid.CreateVersion7()), ct);

        return new ConfirmSettlementResult(settlement.Id, settlement.Items.Count, excelUrl);
    }
}
