using Mediator;
using SA.Operations.DataAccess.Interface.Repositories;
using SA.Operations.Domain.Entities;
using SA.Operations.Domain.Enums;
using Shared.Contract.Exceptions;

namespace SA.Operations.Application.Commands.Observations;

public sealed class CreateObservationCommandHandler(
    IApplicationRepository applicationRepository,
    IObservationRepository observationRepository) : ICommandHandler<CreateObservationCommand, CreateObservationResult>
{
    public async ValueTask<CreateObservationResult> Handle(CreateObservationCommand command, CancellationToken ct)
    {
        var app = await applicationRepository.GetByIdAsync(command.ApplicationId, ct);
        if (app is null || app.TenantId != command.TenantId)
            throw new NotFoundException("OPS_APPLICATION_NOT_FOUND", "Solicitud no encontrada.");

        var observation = ApplicationObservation.Create(
            command.ApplicationId,
            command.TenantId,
            ObservationType.Manual,
            command.Content,
            command.UserId,
            command.UserName);

        await observationRepository.AddAsync(observation, ct);
        await observationRepository.SaveChangesAsync(ct);

        return new CreateObservationResult(observation.Id, observation.Content, observation.CreatedAt);
    }
}
