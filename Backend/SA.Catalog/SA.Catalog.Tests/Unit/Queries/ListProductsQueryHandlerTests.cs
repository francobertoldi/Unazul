using FluentAssertions;
using NSubstitute;
using SA.Catalog.Application.Dtos;
using SA.Catalog.Application.Queries.Products;
using SA.Catalog.DataAccess.Interface.Repositories;
using SA.Catalog.Domain.Entities;
using Shared.Contract.Enums;
using Xunit;

namespace SA.Catalog.Tests.Unit.Queries;

public sealed class ListProductsQueryHandlerTests
{
    private readonly IProductRepository _productRepository = Substitute.For<IProductRepository>();
    private readonly ListProductsQueryHandler _sut;

    public ListProductsQueryHandlerTests()
    {
        _sut = new ListProductsQueryHandler(_productRepository);
    }

    [Fact]
    public async Task TP_CAT_02_01_List_Paginated_WithPlanCount_ReturnsResults()
    {
        // Arrange
        var product = Product.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "Test", "PROD001", null,
            ProductStatus.Active, new DateOnly(2025, 1, 1), null, Guid.NewGuid());
        var products = new List<Product> { product };

        _productRepository.ListAsync(0, 10, null, null, null, null, true, "name", "asc", Arg.Any<CancellationToken>())
            .Returns((products.AsReadOnly() as IReadOnlyList<Product>, 1));

        var query = new ListProductsQuery(Guid.NewGuid(), 1, 10, null, null, null, null, null, "asc");

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Total.Should().Be(1);
    }

    [Fact]
    public async Task TP_CAT_02_02_List_SearchFilter_Applied()
    {
        // Arrange
        _productRepository.ListAsync(0, 10, "loan", null, null, null, true, "name", "asc", Arg.Any<CancellationToken>())
            .Returns((Array.Empty<Product>() as IReadOnlyList<Product>, 0));

        var query = new ListProductsQuery(Guid.NewGuid(), 1, 10, "loan", null, null, null, null, "asc");

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        await _productRepository.Received(1).ListAsync(
            0, 10, "loan", null, null, null, true, "name", "asc", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_CAT_02_03_List_StatusFilter_Works()
    {
        // Arrange
        _productRepository.ListAsync(0, 10, null, ProductStatus.Active, null, null, false, "name", "asc", Arg.Any<CancellationToken>())
            .Returns((Array.Empty<Product>() as IReadOnlyList<Product>, 0));

        var query = new ListProductsQuery(Guid.NewGuid(), 1, 10, null, "Active", null, null, null, "asc");

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        await _productRepository.Received(1).ListAsync(
            0, 10, null, ProductStatus.Active, null, null, false, "name", "asc", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_CAT_02_04_List_FamilyFilter_Works()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        _productRepository.ListAsync(0, 10, null, null, familyId, null, true, "name", "asc", Arg.Any<CancellationToken>())
            .Returns((Array.Empty<Product>() as IReadOnlyList<Product>, 0));

        var query = new ListProductsQuery(Guid.NewGuid(), 1, 10, null, null, familyId, null, null, "asc");

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        await _productRepository.Received(1).ListAsync(
            0, 10, null, null, familyId, null, true, "name", "asc", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_CAT_02_07_List_DeprecatedExcludedByDefault()
    {
        // Arrange — no status filter means excludeDeprecated=true
        _productRepository.ListAsync(0, 10, null, null, null, null, true, "name", "asc", Arg.Any<CancellationToken>())
            .Returns((Array.Empty<Product>() as IReadOnlyList<Product>, 0));

        var query = new ListProductsQuery(Guid.NewGuid(), 1, 10, null, null, null, null, null, "asc");

        // Act
        await _sut.Handle(query, CancellationToken.None);

        // Assert — excludeDeprecated=true (5th bool param)
        await _productRepository.Received(1).ListAsync(
            0, 10, null, null, null, null, true, "name", "asc", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_CAT_02_10_List_PageSizeOver100_ClampedTo100()
    {
        // Arrange
        _productRepository.ListAsync(0, 100, null, null, null, null, true, "name", "asc", Arg.Any<CancellationToken>())
            .Returns((Array.Empty<Product>() as IReadOnlyList<Product>, 0));

        var query = new ListProductsQuery(Guid.NewGuid(), 1, 500, null, null, null, null, null, "asc");

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.PageSize.Should().Be(100);
        await _productRepository.Received(1).ListAsync(
            0, 100, null, null, null, null, true, "name", "asc", Arg.Any<CancellationToken>());
    }
}
