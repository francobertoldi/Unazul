using FluentAssertions;
using NSubstitute;
using SA.Operations.Application.Queries.Settlements;
using SA.Operations.DataAccess.Interface.Repositories;
using SA.Operations.Domain.Entities;
using Xunit;

namespace SA.Operations.Tests.Unit.Queries.Settlements;

public sealed class ListSettlementsQueryHandlerTests
{
    private static readonly Guid TenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    private readonly ISettlementRepository _settlementRepo = Substitute.For<ISettlementRepository>();

    private readonly ListSettlementsQueryHandler _sut;

    public ListSettlementsQueryHandlerTests()
    {
        _sut = new ListSettlementsQueryHandler(_settlementRepo);
    }

    [Fact(DisplayName = "TP_OPS_16_01: Returns paged list")]
    public async Task TP_OPS_16_01_Returns_Paged_List()
    {
        // Arrange
        var settlement = Settlement.Create(TenantId, Guid.NewGuid(), "Admin User", 5);

        _settlementRepo.ListAsync(
            Arg.Any<int>(), Arg.Any<int>(), Arg.Any<Guid?>(),
            Arg.Any<DateTimeOffset?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<Guid?>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns((new[] { settlement } as IReadOnlyList<Settlement>, 1));

        var query = new ListSettlementsQuery(TenantId, null, null, null, 1, 10, null, null);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Total.Should().Be(1);
        result.Items[0].SettledByName.Should().Be("Admin User");
        result.Items[0].OperationCount.Should().Be(5);
    }

    [Fact(DisplayName = "TP_OPS_16_02: Returns empty")]
    public async Task TP_OPS_16_02_Returns_Empty()
    {
        // Arrange
        _settlementRepo.ListAsync(
            Arg.Any<int>(), Arg.Any<int>(), Arg.Any<Guid?>(),
            Arg.Any<DateTimeOffset?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<Guid?>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns((Array.Empty<Settlement>() as IReadOnlyList<Settlement>, 0));

        var query = new ListSettlementsQuery(TenantId, null, null, null, 1, 10, null, null);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().BeEmpty();
        result.Total.Should().Be(0);
    }
}
