using FluentAssertions;
using NSubstitute;
using SA.Operations.Application.Queries.Settlements;
using SA.Operations.DataAccess.Interface.Repositories;
using SA.Operations.Domain.Entities;
using Xunit;

namespace SA.Operations.Tests.Unit.Queries.Settlements;

public sealed class GetSettlementDetailQueryHandlerTests
{
    private static readonly Guid TenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    private readonly ISettlementRepository _settlementRepo = Substitute.For<ISettlementRepository>();

    private readonly GetSettlementDetailQueryHandler _sut;

    public GetSettlementDetailQueryHandlerTests()
    {
        _sut = new GetSettlementDetailQueryHandler(_settlementRepo);
    }

    [Fact(DisplayName = "TP_OPS_17_01: Returns detail with items and totals")]
    public async Task TP_OPS_17_01_Returns_Detail_With_Items_And_Totals()
    {
        // Arrange
        var settlement = Settlement.Create(TenantId, Guid.NewGuid(), "Admin User", 2);

        var total = SettlementTotal.Create(settlement.Id, TenantId, "ARS", 15000m, 2);
        settlement.Totals.Add(total);

        var item = SettlementItem.Create(
            settlement.Id, TenantId, Guid.NewGuid(), "OPS-001",
            "John Doe", "Product A", "Plan X",
            "Percentage", 10m, 1500m, "ARS", "10% of premium");
        settlement.Items.Add(item);

        _settlementRepo.GetByIdWithDetailsAsync(settlement.Id, Arg.Any<CancellationToken>())
            .Returns(settlement);

        var query = new GetSettlementDetailQuery(settlement.Id, TenantId);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(settlement.Id);
        result.SettledByName.Should().Be("Admin User");
        result.Totals.Should().HaveCount(1);
        result.Totals[0].Currency.Should().Be("ARS");
        result.Totals[0].TotalAmount.Should().Be(15000m);
        result.Items.Should().HaveCount(1);
        result.Items[0].AppCode.Should().Be("OPS-001");
        result.Items[0].CalculatedAmount.Should().Be(1500m);
    }

    [Fact(DisplayName = "TP_OPS_17_02: Throws when not found")]
    public async Task TP_OPS_17_02_Throws_When_Not_Found()
    {
        // Arrange
        _settlementRepo.GetByIdWithDetailsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Settlement?)null);

        var query = new GetSettlementDetailQuery(Guid.NewGuid(), TenantId);

        // Act
        Func<Task> act = async () => await _sut.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("OPS_SETTLEMENT_NOT_FOUND");
    }
}
