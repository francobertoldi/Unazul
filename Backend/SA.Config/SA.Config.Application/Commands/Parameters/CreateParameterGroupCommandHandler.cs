using Mediator;
using SA.Config.Application.Dtos.Parameters;
using SA.Config.DataAccess.Interface.Repositories;
using SA.Config.Domain.Entities;
using Shared.Contract.Exceptions;

namespace SA.Config.Application.Commands.Parameters;

public sealed class CreateParameterGroupCommandHandler(
    IParameterGroupRepository parameterGroupRepository) : ICommandHandler<CreateParameterGroupCommand, ParameterGroupDto>
{
    public async ValueTask<ParameterGroupDto> Handle(CreateParameterGroupCommand command, CancellationToken ct)
    {
        var existing = await parameterGroupRepository.GetByCodeAsync(command.Code, ct);
        if (existing is not null)
        {
            throw new ConflictException("CFG_DUPLICATE_GROUP_CODE", "El codigo de grupo ya existe.");
        }

        var group = ParameterGroup.Create(
            command.Code,
            command.Name,
            command.Category,
            command.Icon,
            command.SortOrder);

        await parameterGroupRepository.AddAsync(group, ct);
        await parameterGroupRepository.SaveChangesAsync(ct);

        return new ParameterGroupDto(group.Id, group.Code, group.Name, group.Icon, group.SortOrder);
    }
}
