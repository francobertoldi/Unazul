using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using SA.Catalog.Application.Commands.Products;
using SA.Catalog.Application.Interfaces;
using SA.Catalog.DataAccess.Interface.Repositories;
using SA.Catalog.Domain.Entities;
using Xunit;

namespace SA.Catalog.Tests.Unit.Commands.Products;

public sealed class CreateProductCommandHandlerTests
{
    private readonly IProductRepository _productRepository = Substitute.For<IProductRepository>();
    private readonly IProductFamilyRepository _familyRepository = Substitute.For<IProductFamilyRepository>();
    private readonly IEntityValidationService _entityValidationService = Substitute.For<IEntityValidationService>();
    private readonly CreateProductCommandHandler _sut;

    public CreateProductCommandHandlerTests()
    {
        _sut = new CreateProductCommandHandler(_productRepository, _familyRepository, _entityValidationService);
    }

    [Fact]
    public async Task TP_CAT_03_01_Create_WithValidFields_ReturnsGuid()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var entityId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        var command = new CreateProductCommand(
            tenantId, entityId, familyId,
            "Test Product", "PROD001", "A product",
            "Active", "2025-01-01", "2025-12-31", Guid.NewGuid());

        _familyRepository.GetByIdAsync(familyId, Arg.Any<CancellationToken>())
            .Returns(ProductFamily.Create(tenantId, "PREST001", "Loans", Guid.NewGuid()));
        _entityValidationService.ValidateEntityExistsAsync(tenantId, entityId, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        await _productRepository.Received(1).AddAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>());
        await _productRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_CAT_03_02_Create_WithStatusDraft_Succeeds()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var entityId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        var command = new CreateProductCommand(
            tenantId, entityId, familyId,
            "Draft Product", "PROD002", null,
            "Draft", "2025-01-01", null, Guid.NewGuid());

        _familyRepository.GetByIdAsync(familyId, Arg.Any<CancellationToken>())
            .Returns(ProductFamily.Create(tenantId, "SEG001", "Insurance", Guid.NewGuid()));
        _entityValidationService.ValidateEntityExistsAsync(tenantId, entityId, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task TP_CAT_03_04_Create_EntityNotFound_ThrowsCAT_ENTITY_NOT_FOUND()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        var command = new CreateProductCommand(
            tenantId, Guid.NewGuid(), familyId,
            "Test Product", "PROD001", null,
            "Active", "2025-01-01", null, Guid.NewGuid());

        _familyRepository.GetByIdAsync(familyId, Arg.Any<CancellationToken>())
            .Returns(ProductFamily.Create(tenantId, "PREST001", "Loans", Guid.NewGuid()));
        _entityValidationService.ValidateEntityExistsAsync(tenantId, Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("CAT_ENTITY_NOT_FOUND");
    }

    [Fact]
    public async Task TP_CAT_03_05_Create_FamilyNotFound_ThrowsCAT_FAMILY_NOT_FOUND()
    {
        // Arrange
        var command = new CreateProductCommand(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "Test Product", "PROD001", null,
            "Active", "2025-01-01", null, Guid.NewGuid());

        _familyRepository.GetByIdAsync(command.FamilyId, Arg.Any<CancellationToken>())
            .ReturnsNull();

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("CAT_FAMILY_NOT_FOUND");
    }

    [Fact]
    public async Task TP_CAT_03_06_Create_EmptyName_ThrowsCAT_MISSING_REQUIRED_FIELDS()
    {
        // Arrange
        var command = new CreateProductCommand(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "", "PROD001", null,
            "Active", "2025-01-01", null, Guid.NewGuid());

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("CAT_MISSING_REQUIRED_FIELDS");
    }

    [Fact]
    public async Task TP_CAT_03_07_Create_ValidToBeforeValidFrom_ThrowsCAT_INVALID_DATE_RANGE()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var entityId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        var command = new CreateProductCommand(
            tenantId, entityId, familyId,
            "Test Product", "PROD001", null,
            "Active", "2025-12-31", "2025-01-01", Guid.NewGuid());

        _familyRepository.GetByIdAsync(familyId, Arg.Any<CancellationToken>())
            .Returns(ProductFamily.Create(tenantId, "PREST001", "Loans", Guid.NewGuid()));
        _entityValidationService.ValidateEntityExistsAsync(tenantId, entityId, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("CAT_INVALID_DATE_RANGE");
    }
}
