using FluentAssertions;
using NSubstitute;
using SA.Catalog.Application.Commands.Commissions;
using SA.Catalog.DataAccess.Interface.Repositories;
using SA.Catalog.Domain.Entities;
using Xunit;

namespace SA.Catalog.Tests.Unit.Commands.Commissions;

public sealed class CreateCommissionPlanCommandHandlerTests
{
    private readonly ICommissionPlanRepository _commissionPlanRepository = Substitute.For<ICommissionPlanRepository>();
    private readonly CreateCommissionPlanCommandHandler _sut;

    public CreateCommissionPlanCommandHandlerTests()
    {
        _sut = new CreateCommissionPlanCommandHandler(_commissionPlanRepository);
    }

    [Fact]
    public async Task TP_CAT_08_02_Create_FixedPerSale_Succeeds()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var command = new CreateCommissionPlanCommand(
            tenantId, "COM001", "Fixed commission", "FixedPerSale", 1500m, null, Guid.NewGuid());

        _commissionPlanRepository.ExistsByCodeAsync(tenantId, "COM001", Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        await _commissionPlanRepository.Received(1).AddAsync(Arg.Any<CommissionPlan>(), Arg.Any<CancellationToken>());
        await _commissionPlanRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_CAT_08_03_Create_PercentageCapital_WithMaxAmount_Succeeds()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var command = new CreateCommissionPlanCommand(
            tenantId, "COM002", "Percentage commission", "PercentageCapital", 5.0m, 10000m, Guid.NewGuid());

        _commissionPlanRepository.ExistsByCodeAsync(tenantId, "COM002", Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task TP_CAT_08_06_Create_DuplicateCode_ThrowsCAT_DUPLICATE_COMMISSION_CODE()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var command = new CreateCommissionPlanCommand(
            tenantId, "COM001", "Duplicate", "FixedPerSale", 1500m, null, Guid.NewGuid());

        _commissionPlanRepository.ExistsByCodeAsync(tenantId, "COM001", Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("CAT_DUPLICATE_COMMISSION_CODE");
    }

    [Fact]
    public async Task TP_CAT_08_08_Create_EmptyCode_ThrowsCAT_MISSING_REQUIRED_FIELDS()
    {
        // Arrange
        var command = new CreateCommissionPlanCommand(
            Guid.NewGuid(), "", "Description", "FixedPerSale", 1500m, null, Guid.NewGuid());

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("CAT_MISSING_REQUIRED_FIELDS");
    }
}
