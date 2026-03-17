using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using SA.Catalog.Application.Dtos;
using SA.Catalog.Application.Queries.Products;
using SA.Catalog.DataAccess.Interface.Repositories;
using SA.Catalog.Domain.Entities;
using Shared.Contract.Enums;
using Xunit;

namespace SA.Catalog.Tests.Unit.Queries;

public sealed class GetProductDetailQueryHandlerTests
{
    private readonly IProductRepository _productRepository = Substitute.For<IProductRepository>();
    private readonly GetProductDetailQueryHandler _sut;

    public GetProductDetailQueryHandlerTests()
    {
        _sut = new GetProductDetailQueryHandler(_productRepository);
    }

    private static Product CreateProductWithFamily(string familyCode, Guid? tenantId = null)
    {
        var tid = tenantId ?? Guid.NewGuid();
        var familyId = Guid.NewGuid();

        // We create a product — the Family navigation property is private set,
        // so we rely on the mock returning a full product from GetByIdWithDetailsAsync
        var product = Product.Create(
            tid, Guid.NewGuid(), familyId,
            "Test Product", "PROD001", "Description",
            ProductStatus.Active, new DateOnly(2025, 1, 1), new DateOnly(2025, 12, 31),
            Guid.NewGuid());

        return product;
    }

    [Fact]
    public async Task TP_CAT_04_01_Detail_WithLoanPlans_ReturnsDetailDto()
    {
        // Arrange
        var product = CreateProductWithFamily("PREST001");
        var productId = Guid.NewGuid();

        _productRepository.GetByIdWithDetailsAsync(productId, Arg.Any<CancellationToken>())
            .Returns(product);

        var query = new GetProductDetailQuery(Guid.NewGuid(), productId);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ProductDetailDto>();
        result.Name.Should().Be("Test Product");
        result.Plans.Should().NotBeNull();
        result.Requirements.Should().NotBeNull();
    }

    [Fact]
    public async Task TP_CAT_04_02_Detail_WithInsurance_IncludesInsuranceCategory()
    {
        // Arrange
        var product = CreateProductWithFamily("SEG001");
        var productId = Guid.NewGuid();

        _productRepository.GetByIdWithDetailsAsync(productId, Arg.Any<CancellationToken>())
            .Returns(product);

        var query = new GetProductDetailQuery(Guid.NewGuid(), productId);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Plans.Should().NotBeNull();
    }

    [Fact]
    public async Task TP_CAT_04_04_Detail_ProductWithoutPlans_ReturnsEmptyArrays()
    {
        // Arrange
        var product = CreateProductWithFamily("PREST001");
        var productId = Guid.NewGuid();

        _productRepository.GetByIdWithDetailsAsync(productId, Arg.Any<CancellationToken>())
            .Returns(product);

        var query = new GetProductDetailQuery(Guid.NewGuid(), productId);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Plans.Should().BeEmpty();
        result.Requirements.Should().BeEmpty();
    }

    [Fact]
    public async Task TP_CAT_04_05_Detail_ProductNotFound_ThrowsCAT_PRODUCT_NOT_FOUND()
    {
        // Arrange
        var productId = Guid.NewGuid();
        _productRepository.GetByIdWithDetailsAsync(productId, Arg.Any<CancellationToken>())
            .ReturnsNull();

        var query = new GetProductDetailQuery(Guid.NewGuid(), productId);

        // Act
        Func<Task> act = async () => await _sut.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("CAT_PRODUCT_NOT_FOUND");
    }

    [Fact]
    public async Task TP_CAT_04_03_Detail_WithCard_ReturnsCardCategory()
    {
        // Arrange
        var product = CreateProductWithFamily("TARJETA001");
        var productId = Guid.NewGuid();

        _productRepository.GetByIdWithDetailsAsync(productId, Arg.Any<CancellationToken>())
            .Returns(product);

        var query = new GetProductDetailQuery(Guid.NewGuid(), productId);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Version.Should().Be(1);
    }
}
