using Mediator;
using SA.Config.DataAccess.Interface.Repositories;
using Shared.Contract.Exceptions;

namespace SA.Config.Application.Commands.Parameters;

public sealed class DeleteParameterGroupCommandHandler(
    IParameterGroupRepository parameterGroupRepository) : ICommandHandler<DeleteParameterGroupCommand>
{
    public async ValueTask<Unit> Handle(DeleteParameterGroupCommand command, CancellationToken ct)
    {
        var group = await parameterGroupRepository.GetByIdAsync(command.Id, ct);
        if (group is null)
        {
            throw new NotFoundException("CFG_GROUP_NOT_FOUND", "Grupo de parametros no encontrado.");
        }

        var hasParameters = await parameterGroupRepository.HasParametersAsync(command.Id, ct);
        if (hasParameters)
        {
            throw new ConflictException("CFG_GROUP_HAS_PARAMETERS", "El grupo tiene parametros asociados y no puede eliminarse.");
        }

        await parameterGroupRepository.DeleteAsync(group, ct);
        await parameterGroupRepository.SaveChangesAsync(ct);

        return Unit.Value;
    }
}
