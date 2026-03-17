using Mediator;
using SA.Catalog.DataAccess.Interface.Repositories;
using Shared.Contract.Exceptions;

namespace SA.Catalog.Application.Commands.Coverages;

public sealed class DeleteCoverageCommandHandler(
    ICoverageRepository coverageRepository) : ICommandHandler<DeleteCoverageCommand>
{
    public async ValueTask<Unit> Handle(DeleteCoverageCommand command, CancellationToken ct)
    {
        var coverage = await coverageRepository.GetByIdAsync(command.Id, ct)
            ?? throw new NotFoundException("CAT_COVERAGE_NOT_FOUND", "Cobertura no encontrada.");

        coverageRepository.Delete(coverage);
        await coverageRepository.SaveChangesAsync(ct);

        return Unit.Value;
    }
}
