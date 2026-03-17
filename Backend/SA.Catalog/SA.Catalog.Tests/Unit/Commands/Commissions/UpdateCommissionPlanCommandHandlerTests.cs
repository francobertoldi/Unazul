using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using SA.Catalog.Application.Commands.Commissions;
using SA.Catalog.DataAccess.Interface.Repositories;
using SA.Catalog.Domain.Entities;
using Shared.Contract.Enums;
using Xunit;

namespace SA.Catalog.Tests.Unit.Commands.Commissions;

public sealed class UpdateCommissionPlanCommandHandlerTests
{
    private readonly ICommissionPlanRepository _commissionPlanRepository = Substitute.For<ICommissionPlanRepository>();
    private readonly UpdateCommissionPlanCommandHandler _sut;

    public UpdateCommissionPlanCommandHandlerTests()
    {
        _sut = new UpdateCommissionPlanCommandHandler(_commissionPlanRepository);
    }

    [Fact]
    public async Task TP_CAT_08_04_Update_Succeeds()
    {
        // Arrange
        var commissionId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var commission = CommissionPlan.Create(tenantId, "COM001", "Original", CommissionValueType.FixedPerSale, 1500m, null);

        _commissionPlanRepository.GetByIdAsync(commissionId, Arg.Any<CancellationToken>())
            .Returns(commission);
        _commissionPlanRepository.ExistsByCodeExcludingAsync(tenantId, "COM001-UPD", commissionId, Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new UpdateCommissionPlanCommand(
            tenantId, commissionId, "COM001-UPD", "Updated", "FixedPerSale", 2000m, null, Guid.NewGuid());

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(global::Mediator.Unit.Value);
        _commissionPlanRepository.Received(1).Update(commission);
        await _commissionPlanRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Update_CommissionNotFound_ThrowsCAT_COMMISSION_PLAN_NOT_FOUND()
    {
        // Arrange
        var command = new UpdateCommissionPlanCommand(
            Guid.NewGuid(), Guid.NewGuid(), "COM001", "Test", "FixedPerSale", 100m, null, Guid.NewGuid());

        _commissionPlanRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .ReturnsNull();

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("CAT_COMMISSION_PLAN_NOT_FOUND");
    }

    [Fact]
    public async Task Update_DuplicateCodeExcludingSelf_ThrowsCAT_DUPLICATE_COMMISSION_CODE()
    {
        // Arrange
        var commissionId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var commission = CommissionPlan.Create(tenantId, "COM001", "Original", CommissionValueType.FixedPerSale, 1500m, null);

        _commissionPlanRepository.GetByIdAsync(commissionId, Arg.Any<CancellationToken>())
            .Returns(commission);
        _commissionPlanRepository.ExistsByCodeExcludingAsync(tenantId, "COM002", commissionId, Arg.Any<CancellationToken>())
            .Returns(true);

        var command = new UpdateCommissionPlanCommand(
            tenantId, commissionId, "COM002", "Updated", "FixedPerSale", 1500m, null, Guid.NewGuid());

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("CAT_DUPLICATE_COMMISSION_CODE");
    }
}
