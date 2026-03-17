using FluentAssertions;
using NSubstitute;
using SA.Catalog.Application.Dtos;
using SA.Catalog.Application.Queries.Families;
using SA.Catalog.DataAccess.Interface.Repositories;
using SA.Catalog.Domain.Entities;
using Xunit;

namespace SA.Catalog.Tests.Unit.Queries;

public sealed class ListProductFamiliesQueryHandlerTests
{
    private readonly IProductFamilyRepository _familyRepository = Substitute.For<IProductFamilyRepository>();
    private readonly ListProductFamiliesQueryHandler _sut;

    public ListProductFamiliesQueryHandlerTests()
    {
        _sut = new ListProductFamiliesQueryHandler(_familyRepository);
    }

    [Fact]
    public async Task TP_CAT_01_01_List_Paginated_WithProductCount_ReturnsResults()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var family = ProductFamily.Create(tenantId, "PREST001", "Loans", Guid.NewGuid());
        var families = new List<ProductFamily> { family };

        _familyRepository.ListAsync(0, 10, null, Arg.Any<CancellationToken>())
            .Returns((families.AsReadOnly() as IReadOnlyList<ProductFamily>, 1));
        _familyRepository.CountProductsAsync(family.Id, Arg.Any<CancellationToken>())
            .Returns(5);

        var query = new ListProductFamiliesQuery(tenantId, 1, 10, null);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Total.Should().Be(1);
        result.Items[0].ProductCount.Should().Be(5);
        result.Items[0].Category.Should().Be("loan");
    }

    [Fact]
    public async Task List_Empty_ReturnsEmptyResult()
    {
        // Arrange
        _familyRepository.ListAsync(0, 10, null, Arg.Any<CancellationToken>())
            .Returns((Array.Empty<ProductFamily>() as IReadOnlyList<ProductFamily>, 0));

        var query = new ListProductFamiliesQuery(Guid.NewGuid(), 1, 10, null);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().BeEmpty();
        result.Total.Should().Be(0);
    }

    [Fact]
    public async Task List_SearchFilter_AppliedToRepository()
    {
        // Arrange
        _familyRepository.ListAsync(0, 10, "PREST", Arg.Any<CancellationToken>())
            .Returns((Array.Empty<ProductFamily>() as IReadOnlyList<ProductFamily>, 0));

        var query = new ListProductFamiliesQuery(Guid.NewGuid(), 1, 10, "PREST");

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        await _familyRepository.Received(1).ListAsync(0, 10, "PREST", Arg.Any<CancellationToken>());
    }
}
