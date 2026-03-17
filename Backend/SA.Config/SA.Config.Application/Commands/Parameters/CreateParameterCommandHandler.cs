using Mediator;
using SA.Config.Application.Dtos.Parameters;
using SA.Config.Application.Interfaces;
using SA.Config.DataAccess.Interface.Repositories;
using SA.Config.Domain.Entities;
using SA.Config.Domain.Enums;
using Shared.Contract.Events;
using Shared.Contract.Exceptions;

namespace SA.Config.Application.Commands.Parameters;

public sealed class CreateParameterCommandHandler(
    IParameterRepository parameterRepository,
    IParameterGroupRepository parameterGroupRepository,
    IIntegrationEventPublisher eventPublisher) : ICommandHandler<CreateParameterCommand, ParameterDto>
{
    public async ValueTask<ParameterDto> Handle(CreateParameterCommand command, CancellationToken ct)
    {
        var group = await parameterGroupRepository.GetByIdAsync(command.GroupId, ct);
        if (group is null)
        {
            throw new NotFoundException("CFG_GROUP_NOT_FOUND", "Grupo de parametros no encontrado.");
        }

        var keyExists = await parameterRepository.ExistsByKeyAsync(command.TenantId, command.GroupId, command.Key, ct);
        if (keyExists)
        {
            throw new ConflictException("CFG_DUPLICATE_KEY", "La clave del parametro ya existe en este grupo.");
        }

        ValidateValueCoherence(command.Value, command.Type);
        ValidateOptionsRequired(command.Type, command.Options);

        var parameter = Parameter.Create(
            command.TenantId,
            command.GroupId,
            command.Key,
            command.Value,
            command.Type,
            command.Description,
            command.ParentKey,
            command.CreatedBy);

        await parameterRepository.AddAsync(parameter, ct);

        if (command.Options is { Length: > 0 })
        {
            var options = command.Options
                .Select((o, i) => ParameterOption.Create(parameter.Id, command.TenantId, o.OptionValue, o.OptionLabel, i))
                .ToList();

            await parameterRepository.ReplaceOptionsAsync(parameter.Id, options, ct);
            await parameterRepository.SaveChangesAsync(ct);
        }

        await eventPublisher.PublishAsync(new ParameterUpdatedEvent(
            TenantId: command.TenantId,
            ParameterId: parameter.Id,
            ParameterCode: command.Key,
            OldValue: null,
            NewValue: command.Value,
            UpdatedBy: command.CreatedBy,
            OccurredAt: DateTimeOffset.UtcNow,
            CorrelationId: Guid.CreateVersion7()), ct);

        var saved = await parameterRepository.GetByIdAsync(parameter.Id, ct);

        return MapToDto(saved!);
    }

    internal static void ValidateValueCoherence(string value, ParameterType type)
    {
        switch (type)
        {
            case ParameterType.Number when !double.TryParse(value, out _):
                throw new ValidationException("CFG_INVALID_NUMBER_VALUE", "El valor no es un numero valido.");
            case ParameterType.Boolean when value is not ("true" or "false"):
                throw new ValidationException("CFG_INVALID_BOOLEAN_VALUE", "El valor debe ser 'true' o 'false'.");
        }
    }

    internal static void ValidateOptionsRequired(ParameterType type, CreateParameterOptionInput[]? options)
    {
        if (type is ParameterType.Select or ParameterType.List && (options is null || options.Length == 0))
        {
            throw new ValidationException("CFG_OPTIONS_REQUIRED", "Los tipos Select y List requieren opciones.");
        }
    }

    internal static ParameterDto MapToDto(Parameter p)
    {
        return new ParameterDto(
            p.Id,
            p.Key,
            p.Value,
            p.Type,
            p.Description,
            p.ParentKey,
            p.Options
                .OrderBy(o => o.SortOrder)
                .Select(o => new ParameterOptionDto(o.Id, o.OptionValue, o.OptionLabel, o.SortOrder))
                .ToList());
    }
}
