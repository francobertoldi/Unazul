using SA.Operations.Api.Mappers.Observations;
using SA.Operations.Api.ViewModels.Observations;
using SA.Operations.DataAccess.Interface.Repositories;
using SA.Operations.Domain.Entities;
using Shared.Auth;
using Shared.Contract.Models;

namespace SA.Operations.Api.Endpoints.Observations;

public static class ObservationEndpoints
{
    public static void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/v1/applications/{applicationId:guid}/observations")
            .WithTags("Observations")
            .WithOpenApi()
            .RequireAuthorization();

        // RF-OPS-11: Create observation
        group.MapPost("/", async (
            Guid applicationId,
            CreateObservationRequest request,
            IApplicationRepository applicationRepository,
            IObservationRepository repository,
            ICurrentUser currentUser) =>
        {
            var application = await applicationRepository.GetByIdAsync(applicationId);
            if (application is null)
                return Results.Json(
                    new ErrorResponse("Solicitud no encontrada.", "OPS_APPLICATION_NOT_FOUND"),
                    statusCode: 404);

            var observation = ApplicationObservation.Create(
                applicationId,
                currentUser.TenantId,
                request.ObservationType,
                request.Content,
                currentUser.UserId,
                currentUser.UserName);

            await repository.AddAsync(observation);
            await repository.SaveChangesAsync();

            return Results.Created(
                $"/api/v1/applications/{applicationId}/observations/{observation.Id}",
                ObservationMapper.ToResponse(observation));
        })
        .Produces<ObservationResponse>(201)
        .Produces<ErrorResponse>(404);
    }
}
