using FluentAssertions;
using NSubstitute;
using SA.Catalog.Application.Commands.Families;
using SA.Catalog.DataAccess.Interface.Repositories;
using SA.Catalog.Domain.Entities;
using Xunit;

namespace SA.Catalog.Tests.Unit.Commands.Families;

public sealed class CreateProductFamilyCommandHandlerTests
{
    private readonly IProductFamilyRepository _familyRepository = Substitute.For<IProductFamilyRepository>();
    private readonly CreateProductFamilyCommandHandler _sut;

    public CreateProductFamilyCommandHandlerTests()
    {
        _sut = new CreateProductFamilyCommandHandler(_familyRepository);
    }

    [Fact]
    public async Task TP_CAT_01_02_Create_WithValidPRESTPrefix_Succeeds()
    {
        // Arrange
        var command = new CreateProductFamilyCommand(
            Guid.NewGuid(), "PREST001", "Personal Loans", Guid.NewGuid());

        _familyRepository.ExistsByCodeAsync(command.TenantId, command.Code, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        await _familyRepository.Received(1).AddAsync(Arg.Any<ProductFamily>(), Arg.Any<CancellationToken>());
        await _familyRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_CAT_01_03_Create_WithValidSEGPrefix_Succeeds()
    {
        // Arrange
        var command = new CreateProductFamilyCommand(
            Guid.NewGuid(), "SEG001", "Insurance Products", Guid.NewGuid());

        _familyRepository.ExistsByCodeAsync(command.TenantId, command.Code, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        await _familyRepository.Received(1).AddAsync(Arg.Any<ProductFamily>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_CAT_01_06_Create_InvalidPrefix_ThrowsCAT_INVALID_PREFIX()
    {
        // Arrange
        var command = new CreateProductFamilyCommand(
            Guid.NewGuid(), "XYZABC001", "Bad prefix", Guid.NewGuid());

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("CAT_INVALID_PREFIX");
    }

    [Fact]
    public async Task TP_CAT_01_07_Create_DuplicateCode_ThrowsCAT_DUPLICATE_FAMILY_CODE()
    {
        // Arrange
        var command = new CreateProductFamilyCommand(
            Guid.NewGuid(), "PREST001", "Personal Loans", Guid.NewGuid());

        _familyRepository.ExistsByCodeAsync(command.TenantId, command.Code, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("CAT_DUPLICATE_FAMILY_CODE");
    }

    [Fact]
    public async Task TP_CAT_01_09_Create_EmptyCode_ThrowsCAT_MISSING_REQUIRED_FIELDS()
    {
        // Arrange
        var command = new CreateProductFamilyCommand(
            Guid.NewGuid(), "", "Description", Guid.NewGuid());

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("CAT_MISSING_REQUIRED_FIELDS");
    }
}
