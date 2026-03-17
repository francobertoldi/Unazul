using Mediator;
using SA.Config.Application.Dtos.Parameters;
using SA.Config.Application.Interfaces;
using SA.Config.DataAccess.Interface.Repositories;
using SA.Config.Domain.Entities;
using Shared.Contract.Events;
using Shared.Contract.Exceptions;

namespace SA.Config.Application.Commands.Parameters;

public sealed class UpdateParameterCommandHandler(
    IParameterRepository parameterRepository,
    IIntegrationEventPublisher eventPublisher) : ICommandHandler<UpdateParameterCommand, ParameterDto>
{
    public async ValueTask<ParameterDto> Handle(UpdateParameterCommand command, CancellationToken ct)
    {
        var parameter = await parameterRepository.GetByIdAsync(command.Id, ct);
        if (parameter is null)
        {
            throw new NotFoundException("CFG_PARAMETER_NOT_FOUND", "Parametro no encontrado.");
        }

        CreateParameterCommandHandler.ValidateValueCoherence(command.Value, parameter.Type);

        var oldValue = parameter.Value;

        List<ParameterOption>? newOptions = null;
        if (command.Options is not null)
        {
            newOptions = command.Options
                .Select((o, i) => ParameterOption.Create(parameter.Id, parameter.TenantId, o.OptionValue, o.OptionLabel, i))
                .ToList();
        }

        parameter.UpdateValue(command.Value, newOptions, command.UpdatedBy);
        parameterRepository.Update(parameter);

        if (command.Options is not null)
        {
            await parameterRepository.ReplaceOptionsAsync(parameter.Id, newOptions!, ct);
        }

        await parameterRepository.SaveChangesAsync(ct);

        await eventPublisher.PublishAsync(new ParameterUpdatedEvent(
            TenantId: parameter.TenantId,
            ParameterId: parameter.Id,
            ParameterCode: parameter.Key,
            OldValue: oldValue,
            NewValue: command.Value,
            UpdatedBy: command.UpdatedBy,
            OccurredAt: DateTimeOffset.UtcNow,
            CorrelationId: Guid.CreateVersion7()), ct);

        var saved = await parameterRepository.GetByIdAsync(parameter.Id, ct);

        return CreateParameterCommandHandler.MapToDto(saved!);
    }
}
