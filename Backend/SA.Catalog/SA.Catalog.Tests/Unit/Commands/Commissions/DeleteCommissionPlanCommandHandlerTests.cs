using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using SA.Catalog.Application.Commands.Commissions;
using SA.Catalog.DataAccess.Interface.Repositories;
using SA.Catalog.Domain.Entities;
using Shared.Contract.Enums;
using Xunit;

namespace SA.Catalog.Tests.Unit.Commands.Commissions;

public sealed class DeleteCommissionPlanCommandHandlerTests
{
    private readonly ICommissionPlanRepository _commissionPlanRepository = Substitute.For<ICommissionPlanRepository>();
    private readonly DeleteCommissionPlanCommandHandler _sut;

    public DeleteCommissionPlanCommandHandlerTests()
    {
        _sut = new DeleteCommissionPlanCommandHandler(_commissionPlanRepository);
    }

    [Fact]
    public async Task TP_CAT_08_05_Delete_Unassigned_Succeeds()
    {
        // Arrange
        var commission = CommissionPlan.Create(Guid.NewGuid(), "COM001", "Test", CommissionValueType.FixedPerSale, 1500m, null);
        var command = new DeleteCommissionPlanCommand(Guid.NewGuid(), Guid.NewGuid());

        _commissionPlanRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(commission);
        _commissionPlanRepository.CountAssignedPlansAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(0);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(global::Mediator.Unit.Value);
        _commissionPlanRepository.Received(1).Delete(commission);
        await _commissionPlanRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_CAT_08_07_Delete_Assigned_ThrowsCAT_COMMISSION_IN_USE()
    {
        // Arrange
        var commission = CommissionPlan.Create(Guid.NewGuid(), "COM001", "Test", CommissionValueType.FixedPerSale, 1500m, null);
        var command = new DeleteCommissionPlanCommand(Guid.NewGuid(), Guid.NewGuid());

        _commissionPlanRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(commission);
        _commissionPlanRepository.CountAssignedPlansAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(2);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("CAT_COMMISSION_IN_USE");
    }

    [Fact]
    public async Task Delete_NotFound_ThrowsCAT_COMMISSION_PLAN_NOT_FOUND()
    {
        // Arrange
        var command = new DeleteCommissionPlanCommand(Guid.NewGuid(), Guid.NewGuid());

        _commissionPlanRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .ReturnsNull();

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("CAT_COMMISSION_PLAN_NOT_FOUND");
    }
}
