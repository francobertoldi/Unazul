using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using SA.Catalog.Application.Commands.Requirements;
using SA.Catalog.DataAccess.Interface.Repositories;
using SA.Catalog.Domain.Entities;
using Shared.Contract.Enums;
using Xunit;

namespace SA.Catalog.Tests.Unit.Commands.Requirements;

public sealed class CreateRequirementCommandHandlerTests
{
    private readonly IProductRepository _productRepository = Substitute.For<IProductRepository>();
    private readonly IProductRequirementRepository _requirementRepository = Substitute.For<IProductRequirementRepository>();
    private readonly CreateRequirementCommandHandler _sut;

    public CreateRequirementCommandHandlerTests()
    {
        _sut = new CreateRequirementCommandHandler(_productRepository, _requirementRepository);
    }

    private Product CreateActiveProduct()
    {
        return Product.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "Test", "PROD001", null,
            ProductStatus.Active, new DateOnly(2025, 1, 1), null, Guid.NewGuid());
    }

    [Fact]
    public async Task TP_CAT_07_01_Create_DocumentRequirement_Succeeds()
    {
        // Arrange
        var product = CreateActiveProduct();
        var productId = Guid.NewGuid();
        _productRepository.GetByIdAsync(productId, Arg.Any<CancellationToken>()).Returns(product);

        var command = new CreateRequirementCommand(
            Guid.NewGuid(), productId, "DNI", "document", true, "National ID", Guid.NewGuid());

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        await _requirementRepository.Received(1).AddAsync(Arg.Any<ProductRequirement>(), Arg.Any<CancellationToken>());
        await _requirementRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_CAT_07_02_Create_DataRequirement_Succeeds()
    {
        // Arrange
        var product = CreateActiveProduct();
        var productId = Guid.NewGuid();
        _productRepository.GetByIdAsync(productId, Arg.Any<CancellationToken>()).Returns(product);

        var command = new CreateRequirementCommand(
            Guid.NewGuid(), productId, "Income", "data", false, "Monthly income", Guid.NewGuid());

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task TP_CAT_07_05_Create_EmptyName_ThrowsCAT_MISSING_REQUIRED_FIELDS()
    {
        // Arrange
        var command = new CreateRequirementCommand(
            Guid.NewGuid(), Guid.NewGuid(), "", "document", true, null, Guid.NewGuid());

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("CAT_MISSING_REQUIRED_FIELDS");
    }

    [Fact]
    public async Task TP_CAT_07_06_Create_InvalidType_ThrowsCAT_INVALID_REQUIREMENT_TYPE()
    {
        // Arrange
        var command = new CreateRequirementCommand(
            Guid.NewGuid(), Guid.NewGuid(), "Test Req", "invalid_type", true, null, Guid.NewGuid());

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("CAT_INVALID_REQUIREMENT_TYPE");
    }
}
