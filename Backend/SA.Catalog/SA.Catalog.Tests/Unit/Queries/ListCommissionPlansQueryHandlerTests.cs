using FluentAssertions;
using NSubstitute;
using SA.Catalog.Application.Dtos;
using SA.Catalog.Application.Queries.Commissions;
using SA.Catalog.DataAccess.Interface.Repositories;
using SA.Catalog.Domain.Entities;
using Shared.Contract.Enums;
using Xunit;

namespace SA.Catalog.Tests.Unit.Queries;

public sealed class ListCommissionPlansQueryHandlerTests
{
    private readonly ICommissionPlanRepository _commissionPlanRepository = Substitute.For<ICommissionPlanRepository>();
    private readonly ListCommissionPlansQueryHandler _sut;

    public ListCommissionPlansQueryHandlerTests()
    {
        _sut = new ListCommissionPlansQueryHandler(_commissionPlanRepository);
    }

    [Fact]
    public async Task TP_CAT_08_01_List_WithAssignedPlanCount_ReturnsResults()
    {
        // Arrange
        var commission = CommissionPlan.Create(Guid.NewGuid(), "COM001", "Test", CommissionValueType.FixedPerSale, 1500m, null);
        var plans = new List<CommissionPlan> { commission };

        _commissionPlanRepository.ListAsync(0, 10, null, Arg.Any<CancellationToken>())
            .Returns((plans.AsReadOnly() as IReadOnlyList<CommissionPlan>, 1));
        _commissionPlanRepository.CountAssignedPlansAsync(commission.Id, Arg.Any<CancellationToken>())
            .Returns(3);

        var query = new ListCommissionPlansQuery(Guid.NewGuid(), 1, 10, null);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Total.Should().Be(1);
        result.Items[0].AssignedPlanCount.Should().Be(3);
        result.Items[0].Type.Should().Be("fixedpersale");
    }

    [Fact]
    public async Task List_Empty_ReturnsEmptyResult()
    {
        // Arrange
        _commissionPlanRepository.ListAsync(0, 10, null, Arg.Any<CancellationToken>())
            .Returns((Array.Empty<CommissionPlan>() as IReadOnlyList<CommissionPlan>, 0));

        var query = new ListCommissionPlansQuery(Guid.NewGuid(), 1, 10, null);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().BeEmpty();
        result.Total.Should().Be(0);
    }

    [Fact]
    public async Task List_SearchFilter_Applied()
    {
        // Arrange
        _commissionPlanRepository.ListAsync(0, 10, "COM", Arg.Any<CancellationToken>())
            .Returns((Array.Empty<CommissionPlan>() as IReadOnlyList<CommissionPlan>, 0));

        var query = new ListCommissionPlansQuery(Guid.NewGuid(), 1, 10, "COM");

        // Act
        await _sut.Handle(query, CancellationToken.None);

        // Assert
        await _commissionPlanRepository.Received(1).ListAsync(0, 10, "COM", Arg.Any<CancellationToken>());
    }
}
