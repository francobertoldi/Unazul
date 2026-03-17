using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using SA.Catalog.Application.Commands.Families;
using SA.Catalog.DataAccess.Interface.Repositories;
using SA.Catalog.Domain.Entities;
using Xunit;

namespace SA.Catalog.Tests.Unit.Commands.Families;

public sealed class UpdateProductFamilyCommandHandlerTests
{
    private readonly IProductFamilyRepository _familyRepository = Substitute.For<IProductFamilyRepository>();
    private readonly UpdateProductFamilyCommandHandler _sut;

    public UpdateProductFamilyCommandHandlerTests()
    {
        _sut = new UpdateProductFamilyCommandHandler(_familyRepository);
    }

    [Fact]
    public async Task TP_CAT_01_04_Update_ValidDescription_Succeeds()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var family = ProductFamily.Create(Guid.NewGuid(), "PREST001", "Original", Guid.NewGuid());
        var command = new UpdateProductFamilyCommand(Guid.NewGuid(), familyId, "Updated description", Guid.NewGuid());

        _familyRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(family);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(global::Mediator.Unit.Value);
        _familyRepository.Received(1).Update(family);
        await _familyRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_CAT_01_12_Update_FamilyNotFound_ThrowsCAT_FAMILY_NOT_FOUND()
    {
        // Arrange
        var command = new UpdateProductFamilyCommand(Guid.NewGuid(), Guid.NewGuid(), "Updated", Guid.NewGuid());

        _familyRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .ReturnsNull();

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("CAT_FAMILY_NOT_FOUND");
    }
}
