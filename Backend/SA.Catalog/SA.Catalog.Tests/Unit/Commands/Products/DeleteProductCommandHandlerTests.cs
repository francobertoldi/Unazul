using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using SA.Catalog.Application.Commands.Products;
using SA.Catalog.DataAccess.Interface.Repositories;
using SA.Catalog.Domain.Entities;
using Shared.Contract.Enums;
using Xunit;

namespace SA.Catalog.Tests.Unit.Commands.Products;

public sealed class DeleteProductCommandHandlerTests
{
    private readonly IProductRepository _productRepository = Substitute.For<IProductRepository>();
    private readonly IProductRequirementRepository _requirementRepository = Substitute.For<IProductRequirementRepository>();
    private readonly DeleteProductCommandHandler _sut;

    public DeleteProductCommandHandlerTests()
    {
        _sut = new DeleteProductCommandHandler(_productRepository, _requirementRepository);
    }

    [Fact]
    public async Task TP_CAT_09_01_Delete_ProductWithoutPlans_Succeeds()
    {
        // Arrange
        var product = Product.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "Test", "PROD001", null,
            ProductStatus.Active, new DateOnly(2025, 1, 1), null, Guid.NewGuid());
        var command = new DeleteProductCommand(Guid.NewGuid(), Guid.NewGuid());

        _productRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(product);
        _productRepository.HasPlansAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(global::Mediator.Unit.Value);
        _productRepository.Received(1).Delete(product);
        await _requirementRepository.Received(1).DeleteByProductIdAsync(command.Id, Arg.Any<CancellationToken>());
        await _productRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_CAT_09_03_Delete_ProductWithPlans_ThrowsCAT_PRODUCT_HAS_PLANS()
    {
        // Arrange
        var product = Product.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "Test", "PROD001", null,
            ProductStatus.Active, new DateOnly(2025, 1, 1), null, Guid.NewGuid());
        var command = new DeleteProductCommand(Guid.NewGuid(), Guid.NewGuid());

        _productRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(product);
        _productRepository.HasPlansAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("CAT_PRODUCT_HAS_PLANS");
    }

    [Fact]
    public async Task Delete_ProductNotFound_ThrowsCAT_PRODUCT_NOT_FOUND()
    {
        // Arrange
        var command = new DeleteProductCommand(Guid.NewGuid(), Guid.NewGuid());

        _productRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .ReturnsNull();

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("CAT_PRODUCT_NOT_FOUND");
    }
}
