using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using SA.Catalog.Application.Commands.Families;
using SA.Catalog.DataAccess.Interface.Repositories;
using SA.Catalog.Domain.Entities;
using Xunit;

namespace SA.Catalog.Tests.Unit.Commands.Families;

public sealed class DeleteProductFamilyCommandHandlerTests
{
    private readonly IProductFamilyRepository _familyRepository = Substitute.For<IProductFamilyRepository>();
    private readonly DeleteProductFamilyCommandHandler _sut;

    public DeleteProductFamilyCommandHandlerTests()
    {
        _sut = new DeleteProductFamilyCommandHandler(_familyRepository);
    }

    [Fact]
    public async Task TP_CAT_01_05_Delete_FamilyWithoutProducts_Succeeds()
    {
        // Arrange
        var family = ProductFamily.Create(Guid.NewGuid(), "PREST001", "Loans", Guid.NewGuid());
        var command = new DeleteProductFamilyCommand(Guid.NewGuid(), Guid.NewGuid());

        _familyRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(family);
        _familyRepository.CountProductsAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(0);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(global::Mediator.Unit.Value);
        _familyRepository.Received(1).Delete(family);
        await _familyRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_CAT_01_08_Delete_FamilyWithProducts_ThrowsCAT_FAMILY_HAS_PRODUCTS()
    {
        // Arrange
        var family = ProductFamily.Create(Guid.NewGuid(), "PREST001", "Loans", Guid.NewGuid());
        var command = new DeleteProductFamilyCommand(Guid.NewGuid(), Guid.NewGuid());

        _familyRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(family);
        _familyRepository.CountProductsAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(3);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("CAT_FAMILY_HAS_PRODUCTS");
    }

    [Fact]
    public async Task Delete_FamilyNotFound_ThrowsCAT_FAMILY_NOT_FOUND()
    {
        // Arrange
        var command = new DeleteProductFamilyCommand(Guid.NewGuid(), Guid.NewGuid());

        _familyRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .ReturnsNull();

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("CAT_FAMILY_NOT_FOUND");
    }
}
