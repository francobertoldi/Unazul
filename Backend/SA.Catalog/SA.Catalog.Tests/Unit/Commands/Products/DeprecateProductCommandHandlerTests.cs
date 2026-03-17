using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using SA.Catalog.Application.Commands.Products;
using SA.Catalog.DataAccess.Interface.Repositories;
using SA.Catalog.Domain.Entities;
using Shared.Contract.Enums;
using Xunit;

namespace SA.Catalog.Tests.Unit.Commands.Products;

public sealed class DeprecateProductCommandHandlerTests
{
    private readonly IProductRepository _productRepository = Substitute.For<IProductRepository>();
    private readonly DeprecateProductCommandHandler _sut;

    public DeprecateProductCommandHandlerTests()
    {
        _sut = new DeprecateProductCommandHandler(_productRepository);
    }

    [Fact]
    public async Task TP_CAT_09_02_Deprecate_ActiveProduct_Succeeds()
    {
        // Arrange
        var product = Product.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "Test", "PROD001", null,
            ProductStatus.Active, new DateOnly(2025, 1, 1), null, Guid.NewGuid());
        var command = new DeprecateProductCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        _productRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(product);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(global::Mediator.Unit.Value);
        product.Status.Should().Be(ProductStatus.Deprecated);
        _productRepository.Received(1).Update(product);
        await _productRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_CAT_09_04_Deprecate_AlreadyDeprecated_ThrowsCAT_PRODUCT_DEPRECATED()
    {
        // Arrange
        var product = Product.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "Test", "PROD001", null,
            ProductStatus.Active, new DateOnly(2025, 1, 1), null, Guid.NewGuid());
        product.Deprecate(Guid.NewGuid());
        var command = new DeprecateProductCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        _productRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(product);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("CAT_PRODUCT_DEPRECATED");
    }

    [Fact]
    public async Task Deprecate_ProductNotFound_ThrowsCAT_PRODUCT_NOT_FOUND()
    {
        // Arrange
        var command = new DeprecateProductCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        _productRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .ReturnsNull();

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("CAT_PRODUCT_NOT_FOUND");
    }
}
