using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using SA.Catalog.Application.Commands.Products;
using SA.Catalog.DataAccess.Interface.Repositories;
using SA.Catalog.Domain.Entities;
using Shared.Contract.Enums;
using Xunit;

namespace SA.Catalog.Tests.Unit.Commands.Products;

public sealed class UpdateProductCommandHandlerTests
{
    private readonly IProductRepository _productRepository = Substitute.For<IProductRepository>();
    private readonly UpdateProductCommandHandler _sut;

    public UpdateProductCommandHandlerTests()
    {
        _sut = new UpdateProductCommandHandler(_productRepository);
    }

    [Fact]
    public async Task TP_CAT_03_03_Update_Succeeds_VersionIncrements()
    {
        // Arrange
        var product = Product.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "Original", "PROD001", "Desc",
            ProductStatus.Active, new DateOnly(2025, 1, 1), null, Guid.NewGuid());
        var originalVersion = product.Version;
        var command = new UpdateProductCommand(
            Guid.NewGuid(), Guid.NewGuid(),
            "Updated", "PROD001", "Updated desc",
            "Active", "2025-01-01", null, Guid.NewGuid());

        _productRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(product);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(global::Mediator.Unit.Value);
        product.Version.Should().Be(originalVersion + 1);
        _productRepository.Received(1).Update(product);
        await _productRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_CAT_03_09_Update_DeprecatedProduct_ThrowsCAT_PRODUCT_DEPRECATED()
    {
        // Arrange
        var product = Product.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "Original", "PROD001", null,
            ProductStatus.Active, new DateOnly(2025, 1, 1), null, Guid.NewGuid());
        product.Deprecate(Guid.NewGuid());

        var command = new UpdateProductCommand(
            Guid.NewGuid(), Guid.NewGuid(),
            "Updated", "PROD001", null,
            "Active", "2025-01-01", null, Guid.NewGuid());

        _productRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(product);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("CAT_PRODUCT_DEPRECATED");
    }

    [Fact]
    public async Task Update_ProductNotFound_ThrowsCAT_PRODUCT_NOT_FOUND()
    {
        // Arrange
        var command = new UpdateProductCommand(
            Guid.NewGuid(), Guid.NewGuid(),
            "Updated", "PROD001", null,
            "Active", "2025-01-01", null, Guid.NewGuid());

        _productRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .ReturnsNull();

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("CAT_PRODUCT_NOT_FOUND");
    }

    [Fact]
    public async Task Update_InvalidStatus_ThrowsCAT_INVALID_STATUS()
    {
        // Arrange
        var product = Product.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "Original", "PROD001", null,
            ProductStatus.Active, new DateOnly(2025, 1, 1), null, Guid.NewGuid());

        var command = new UpdateProductCommand(
            Guid.NewGuid(), Guid.NewGuid(),
            "Updated", "PROD001", null,
            "InvalidStatus", "2025-01-01", null, Guid.NewGuid());

        _productRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(product);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("CAT_INVALID_STATUS");
    }
}
