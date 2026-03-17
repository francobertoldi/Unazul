using SA.Operations.Api.Mappers.Settlements;
using SA.Operations.Api.ViewModels.Settlements;
using SA.Operations.Application.Interfaces;
using SA.Operations.DataAccess.Interface.Repositories;
using SA.Operations.Domain.Entities;
using Shared.Auth;
using Shared.Contract.Models;
using Shared.Pagination;

namespace SA.Operations.Api.Endpoints.Settlements;

public static class SettlementEndpoints
{
    public static void Map(WebApplication app)
    {
        var settlements = app.MapGroup("/api/v1/settlements")
            .WithTags("Settlements")
            .WithOpenApi()
            .RequireAuthorization();

        // RF-OPS-12: Preview settlement
        settlements.MapPost("/preview", async (
            SettlementPreviewRequest request,
            IApplicationRepository applicationRepository,
            ICatalogServiceClient catalogClient,
            ICurrentUser currentUser) =>
        {
            var applications = await applicationRepository.GetApprovedByDateRangeAsync(
                currentUser.TenantId,
                request.EntityId,
                request.DateFrom,
                request.DateTo);

            if (applications.Count == 0)
                return Results.Ok(new SettlementPreviewResponse(0, [], []));

            var productIds = applications.Select(a => a.ProductId).Distinct().ToArray();
            var planIds = applications.Select(a => a.PlanId).Distinct().ToArray();
            var commissions = await catalogClient.GetCommissionPlansAsync(productIds, planIds);

            var commissionLookup = commissions.ToDictionary(c => (c.ProductId, c.PlanId));

            var items = applications.Select(app =>
            {
                commissionLookup.TryGetValue((app.ProductId, app.PlanId), out var comm);
                return new SettlementPreviewItemResponse(
                    app.Id,
                    app.Code,
                    string.Empty, // Applicant name resolved via join in handler
                    app.ProductName,
                    app.PlanName,
                    comm?.CommissionType,
                    comm?.CommissionValue,
                    comm?.CommissionValue ?? 0,
                    comm?.Currency ?? "ARS",
                    comm?.FormulaDescription);
            }).ToList();

            var totals = items
                .GroupBy(i => i.Currency)
                .Select(g => new SettlementTotalResponse(g.Key, g.Sum(i => i.CalculatedAmount), g.Count()))
                .ToList();

            return Results.Ok(new SettlementPreviewResponse(applications.Count, items, totals));
        })
        .Produces<SettlementPreviewResponse>(200);

        // RF-OPS-13: Confirm settlement
        settlements.MapPost("/", async (
            ConfirmSettlementRequest request,
            IApplicationRepository applicationRepository,
            ISettlementRepository settlementRepository,
            ICatalogServiceClient catalogClient,
            IFileStorageService storageService,
            ICurrentUser currentUser) =>
        {
            var applications = await applicationRepository.GetApprovedByDateRangeAsync(
                currentUser.TenantId,
                request.EntityId,
                request.DateFrom,
                request.DateTo);

            if (applications.Count == 0)
                return Results.Json(
                    new ErrorResponse("No hay solicitudes aprobadas en el rango indicado.", "OPS_NO_APPROVED_APPLICATIONS"),
                    statusCode: 422);

            var productIds = applications.Select(a => a.ProductId).Distinct().ToArray();
            var planIds = applications.Select(a => a.PlanId).Distinct().ToArray();
            var commissions = await catalogClient.GetCommissionPlansAsync(productIds, planIds);
            var commissionLookup = commissions.ToDictionary(c => (c.ProductId, c.PlanId));

            var settlement = Settlement.Create(
                currentUser.TenantId,
                currentUser.UserId,
                currentUser.UserName,
                applications.Count);

            await settlementRepository.AddAsync(settlement);

            foreach (var app in applications)
            {
                commissionLookup.TryGetValue((app.ProductId, app.PlanId), out var comm);

                var item = SettlementItem.Create(
                    settlement.Id,
                    currentUser.TenantId,
                    app.Id,
                    app.Code,
                    string.Empty, // Applicant name resolved via join
                    app.ProductName,
                    app.PlanName,
                    comm?.CommissionType,
                    comm?.CommissionValue,
                    comm?.CommissionValue ?? 0,
                    comm?.Currency ?? "ARS",
                    comm?.FormulaDescription);

                settlement.Items.Add(item);
            }

            // Build totals by currency
            var totals = settlement.Items
                .GroupBy(i => i.Currency)
                .Select(g => SettlementTotal.Create(
                    settlement.Id,
                    currentUser.TenantId,
                    g.Key,
                    g.Sum(i => i.CalculatedAmount),
                    g.Count()));

            foreach (var total in totals)
                settlement.Totals.Add(total);

            // Transition all applications to Settled
            var appIds = applications.Select(a => a.Id).ToList();
            await applicationRepository.BatchTransitionToSettledAsync(appIds, currentUser.UserId);

            // Generate Excel
            var excelPath = await storageService.GenerateSettlementExcelAsync(
                currentUser.TenantId,
                settlement.Id,
                settlement.Totals,
                settlement.Items);

            if (excelPath is not null)
                settlement.SetExcelUrl(excelPath);

            settlementRepository.Update(settlement);
            await settlementRepository.SaveChangesAsync();

            return Results.Created(
                $"/api/v1/settlements/{settlement.Id}",
                SettlementMapper.ToDetailResponse(settlement));
        })
        .Produces<SettlementDetailResponse>(201)
        .Produces<ErrorResponse>(422);

        // RF-OPS-16: List settlement history
        settlements.MapGet("/", async (
            int page,
            int pageSize,
            string? sort,
            string? order,
            ISettlementRepository repository) =>
        {
            var pagination = new PaginationRequest(page, pageSize, sort, order ?? "desc");
            var (items, total) = await repository.ListAsync(
                pagination.Skip,
                pagination.ClampedPageSize,
                sortBy: pagination.Sort,
                sortDir: order ?? "desc");

            var mapped = items.Select(SettlementMapper.ToListResponse).ToList();
            return Results.Ok(new PagedResult<SettlementListResponse>(mapped, total, page, pageSize));
        })
        .Produces<PagedResult<SettlementListResponse>>(200);

        // RF-OPS-17: Get settlement detail
        settlements.MapGet("/{id:guid}", async (
            Guid id,
            ISettlementRepository repository) =>
        {
            var settlement = await repository.GetByIdWithDetailsAsync(id);
            if (settlement is null)
                return Results.Json(
                    new ErrorResponse("Liquidacion no encontrada.", "OPS_SETTLEMENT_NOT_FOUND"),
                    statusCode: 404);

            return Results.Ok(SettlementMapper.ToDetailResponse(settlement));
        })
        .Produces<SettlementDetailResponse>(200)
        .Produces<ErrorResponse>(404);

        // RF-OPS-18: Download settlement Excel
        settlements.MapGet("/{id:guid}/download", async (
            Guid id,
            ISettlementRepository repository,
            IFileStorageService storageService) =>
        {
            var settlement = await repository.GetByIdWithDetailsAsync(id);
            if (settlement is null)
                return Results.Json(
                    new ErrorResponse("Liquidacion no encontrada.", "OPS_SETTLEMENT_NOT_FOUND"),
                    statusCode: 404);

            if (string.IsNullOrEmpty(settlement.ExcelUrl))
                return Results.Json(
                    new ErrorResponse("El archivo Excel no esta disponible.", "OPS_EXCEL_NOT_FOUND"),
                    statusCode: 404);

            var stream = await storageService.GetFileStreamAsync(settlement.ExcelUrl);
            if (stream is null)
                return Results.Json(
                    new ErrorResponse("El archivo Excel no fue encontrado en el servidor.", "OPS_EXCEL_FILE_MISSING"),
                    statusCode: 404);

            return Results.File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"liquidacion_{settlement.Id}.xlsx");
        })
        .Produces(200)
        .Produces<ErrorResponse>(404);
    }
}
