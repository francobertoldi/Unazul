using SA.Operations.Api.Mappers.Applications;
using SA.Operations.Api.ViewModels.Applications;
using SA.Operations.DataAccess.Interface.Repositories;
using SA.Operations.Domain.Entities;
using SA.Operations.Domain.Enums;
using Shared.Auth;
using Shared.Contract.Models;
using Shared.Pagination;
using DomainApplication = SA.Operations.Domain.Entities.Application;

namespace SA.Operations.Api.Endpoints.Applications;

public static class ApplicationEndpoints
{
    public static void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/v1/applications")
            .WithTags("Applications")
            .WithOpenApi()
            .RequireAuthorization();

        // RF-OPS-01: List applications with filters
        group.MapGet("/", async (
            int page,
            int pageSize,
            string? search,
            ApplicationStatus? status,
            Guid? entityId,
            string? sort,
            string? order,
            IApplicationRepository repository) =>
        {
            var pagination = new PaginationRequest(page, pageSize, sort, order ?? "asc");
            var (items, total) = await repository.ListAsync(
                pagination.Skip,
                pagination.ClampedPageSize,
                search,
                status,
                entityId,
                pagination.Sort,
                order);

            var mapped = items.Select(a => ApplicationMapper.ToListResponse(a)).ToList();
            return Results.Ok(new PagedResult<ApplicationListResponse>(mapped, total, page, pageSize));
        })
        .Produces<PagedResult<ApplicationListResponse>>(200);

        // RF-OPS-03: Create application
        group.MapPost("/", async (
            CreateApplicationRequest request,
            IApplicationRepository repository,
            IApplicantRepository applicantRepository,
            ICurrentUser currentUser) =>
        {
            try
            {
                var applicant = await applicantRepository.GetByIdAsync(request.ApplicantId);
                if (applicant is null)
                    return Results.Json(
                        new ErrorResponse("Solicitante no encontrado.", "OPS_APPLICANT_NOT_FOUND"),
                        statusCode: 404);

                var seq = await repository.GetNextSequenceAsync(currentUser.TenantId);
                var code = $"OPS-{seq:D6}";

                var application = DomainApplication.Create(
                    currentUser.TenantId,
                    request.EntityId,
                    request.ApplicantId,
                    code,
                    request.ProductId,
                    request.PlanId,
                    string.Empty, // Will be populated by catalog validation in Application layer
                    string.Empty,
                    currentUser.UserId);

                await repository.AddAsync(application);
                await repository.SaveChangesAsync();

                return Results.Created(
                    $"/api/v1/applications/{application.Id}",
                    ApplicationMapper.ToDetailResponse(application));
            }
            catch (InvalidOperationException ex) when (ex.Message == "OPS_APPLICANT_NOT_FOUND")
            {
                return Results.Json(
                    new ErrorResponse("Solicitante no encontrado.", "OPS_APPLICANT_NOT_FOUND"),
                    statusCode: 404);
            }
        })
        .Produces<ApplicationDetailResponse>(201)
        .Produces<ErrorResponse>(404);

        // RF-OPS-04: Update draft application
        group.MapPut("/{id:guid}", async (
            Guid id,
            UpdateApplicationDraftRequest request,
            IApplicationRepository repository,
            ICurrentUser currentUser) =>
        {
            try
            {
                var application = await repository.GetByIdAsync(id);
                if (application is null)
                    return Results.Json(
                        new ErrorResponse("Solicitud no encontrada.", "OPS_APPLICATION_NOT_FOUND"),
                        statusCode: 404);

                if (application.Status != ApplicationStatus.Draft)
                    return Results.Json(
                        new ErrorResponse("Solo se pueden editar solicitudes en borrador.", "OPS_NOT_DRAFT"),
                        statusCode: 422);

                application.UpdateDraft(
                    request.EntityId,
                    request.ProductId,
                    request.PlanId,
                    null,
                    null,
                    currentUser.UserId);

                repository.Update(application);
                await repository.SaveChangesAsync();

                return Results.Ok(ApplicationMapper.ToDetailResponse(application));
            }
            catch (InvalidOperationException ex) when (ex.Message == "OPS_APPLICATION_NOT_FOUND")
            {
                return Results.Json(
                    new ErrorResponse("Solicitud no encontrada.", "OPS_APPLICATION_NOT_FOUND"),
                    statusCode: 404);
            }
        })
        .Produces<ApplicationDetailResponse>(200)
        .Produces<ErrorResponse>(404)
        .Produces<ErrorResponse>(422);

        // RF-OPS-06: Get application detail
        group.MapGet("/{id:guid}", async (
            Guid id,
            IApplicationRepository repository) =>
        {
            var application = await repository.GetByIdAsync(id);
            if (application is null)
                return Results.Json(
                    new ErrorResponse("Solicitud no encontrada.", "OPS_APPLICATION_NOT_FOUND"),
                    statusCode: 404);

            return Results.Ok(ApplicationMapper.ToDetailResponse(application));
        })
        .Produces<ApplicationDetailResponse>(200)
        .Produces<ErrorResponse>(404);

        // RF-OPS-05: Transition application status
        group.MapPut("/{id:guid}/status", async (
            Guid id,
            TransitionStatusRequest request,
            IApplicationRepository repository,
            ITraceEventRepository traceRepository,
            ICurrentUser currentUser) =>
        {
            var application = await repository.GetByIdAsync(id);
            if (application is null)
                return Results.Json(
                    new ErrorResponse("Solicitud no encontrada.", "OPS_APPLICATION_NOT_FOUND"),
                    statusCode: 404);

            var previousStatus = application.Status;
            var success = application.TransitionStatus(request.NewStatus, currentUser.UserId);
            if (!success)
                return Results.Json(
                    new ErrorResponse(
                        $"Transicion invalida de {previousStatus} a {request.NewStatus}.",
                        "OPS_INVALID_TRANSITION"),
                    statusCode: 422);

            repository.Update(application);

            var traceEvent = TraceEvent.Create(
                application.Id,
                currentUser.TenantId,
                request.NewStatus.ToString(),
                $"Status changed from {previousStatus} to {request.NewStatus}",
                currentUser.UserId,
                currentUser.UserName);

            await traceRepository.AddAsync(traceEvent);
            await repository.SaveChangesAsync();

            return Results.Ok(ApplicationMapper.ToDetailResponse(application));
        })
        .Produces<ApplicationDetailResponse>(200)
        .Produces<ErrorResponse>(404)
        .Produces<ErrorResponse>(422);

        // RF-OPS-07: Get application timeline
        group.MapGet("/{id:guid}/timeline", async (
            Guid id,
            IApplicationRepository applicationRepository,
            ITraceEventRepository traceRepository) =>
        {
            var exists = await applicationRepository.ExistsByIdAsync(id);
            if (!exists)
                return Results.Json(
                    new ErrorResponse("Solicitud no encontrada.", "OPS_APPLICATION_NOT_FOUND"),
                    statusCode: 404);

            var events = await traceRepository.GetByApplicationIdAsync(id);
            var mapped = events.Select(ApplicationMapper.ToTimelineResponse).ToList();
            return Results.Ok(mapped);
        })
        .Produces<IReadOnlyList<TimelineEventResponse>>(200)
        .Produces<ErrorResponse>(404);
    }
}
