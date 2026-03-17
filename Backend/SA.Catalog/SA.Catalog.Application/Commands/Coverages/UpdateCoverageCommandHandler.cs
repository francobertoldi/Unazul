using Mediator;
using SA.Catalog.DataAccess.Interface.Repositories;
using Shared.Contract.Exceptions;

namespace SA.Catalog.Application.Commands.Coverages;

public sealed class UpdateCoverageCommandHandler(
    ICoverageRepository coverageRepository) : ICommandHandler<UpdateCoverageCommand>
{
    public async ValueTask<Unit> Handle(UpdateCoverageCommand command, CancellationToken ct)
    {
        if (command.SumInsured <= 0)
            throw new ValidationException("CAT_INVALID_SUM_INSURED", "La suma asegurada debe ser mayor a cero.");

        var coverage = await coverageRepository.GetByIdAsync(command.Id, ct)
            ?? throw new NotFoundException("CAT_COVERAGE_NOT_FOUND", "Cobertura no encontrada.");

        coverage.Update(command.SumInsured, command.Premium, command.GracePeriodDays);
        coverageRepository.Update(coverage);
        await coverageRepository.SaveChangesAsync(ct);

        return Unit.Value;
    }
}
