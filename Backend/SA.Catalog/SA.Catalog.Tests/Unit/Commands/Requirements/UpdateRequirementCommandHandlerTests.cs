using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using SA.Catalog.Application.Commands.Requirements;
using SA.Catalog.DataAccess.Interface.Repositories;
using SA.Catalog.Domain.Entities;
using Shared.Contract.Enums;
using Xunit;

namespace SA.Catalog.Tests.Unit.Commands.Requirements;

public sealed class UpdateRequirementCommandHandlerTests
{
    private readonly IProductRepository _productRepository = Substitute.For<IProductRepository>();
    private readonly IProductRequirementRepository _requirementRepository = Substitute.For<IProductRequirementRepository>();
    private readonly UpdateRequirementCommandHandler _sut;

    public UpdateRequirementCommandHandlerTests()
    {
        _sut = new UpdateRequirementCommandHandler(_productRepository, _requirementRepository);
    }

    [Fact]
    public async Task TP_CAT_07_03_Update_Requirement_Succeeds()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var requirementId = Guid.NewGuid();

        var product = Product.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "Test", "PROD001", null,
            ProductStatus.Active, new DateOnly(2025, 1, 1), null, Guid.NewGuid());
        var requirement = ProductRequirement.Create(productId, Guid.NewGuid(), "DNI", "document", true, null);

        _productRepository.GetByIdAsync(productId, Arg.Any<CancellationToken>()).Returns(product);
        _requirementRepository.GetByIdAsync(requirementId, Arg.Any<CancellationToken>()).Returns(requirement);

        var command = new UpdateRequirementCommand(
            Guid.NewGuid(), requirementId, productId,
            "Updated DNI", "data", false, "Updated desc", Guid.NewGuid());

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(global::Mediator.Unit.Value);
        _requirementRepository.Received(1).Update(requirement);
        await _requirementRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_CAT_07_08_Update_RequirementNotFound_ThrowsCAT_REQUIREMENT_NOT_FOUND()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = Product.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "Test", "PROD001", null,
            ProductStatus.Active, new DateOnly(2025, 1, 1), null, Guid.NewGuid());

        _productRepository.GetByIdAsync(productId, Arg.Any<CancellationToken>()).Returns(product);
        _requirementRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).ReturnsNull();

        var command = new UpdateRequirementCommand(
            Guid.NewGuid(), Guid.NewGuid(), productId,
            "Test", "document", true, null, Guid.NewGuid());

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("CAT_REQUIREMENT_NOT_FOUND");
    }
}
