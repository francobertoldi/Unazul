using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using SA.Config.Application.Commands.Parameters;
using SA.Config.DataAccess.Interface.Repositories;
using SA.Config.Domain.Entities;
using Xunit;

namespace SA.Config.Tests.Unit.Commands.Parameters;

public sealed class DeleteParameterGroupCommandHandlerTests
{
    private readonly IParameterGroupRepository _parameterGroupRepository = Substitute.For<IParameterGroupRepository>();
    private readonly DeleteParameterGroupCommandHandler _sut;

    public DeleteParameterGroupCommandHandlerTests()
    {
        _sut = new DeleteParameterGroupCommandHandler(_parameterGroupRepository);
    }

    [Fact]
    public async Task TP_CFG_06_02_Deletes_Empty_Group()
    {
        // Arrange
        var group = ParameterGroup.Create("general.company", "Empresa", "General", "building", 1);
        _parameterGroupRepository.GetByIdAsync(group.Id, Arg.Any<CancellationToken>())
            .Returns(group);
        _parameterGroupRepository.HasParametersAsync(group.Id, Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new DeleteParameterGroupCommand(group.Id);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Mediator.Unit.Value);
        await _parameterGroupRepository.Received(1).DeleteAsync(group, Arg.Any<CancellationToken>());
        await _parameterGroupRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_CFG_06_04_Returns_409_When_Group_Has_Parameters()
    {
        // Arrange
        var group = ParameterGroup.Create("general.company", "Empresa", "General", "building", 1);
        _parameterGroupRepository.GetByIdAsync(group.Id, Arg.Any<CancellationToken>())
            .Returns(group);
        _parameterGroupRepository.HasParametersAsync(group.Id, Arg.Any<CancellationToken>())
            .Returns(true);

        var command = new DeleteParameterGroupCommand(group.Id);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("CFG_GROUP_HAS_PARAMETERS");
    }

    [Fact]
    public async Task TP_CFG_06_05_Returns_404_For_NonExistent()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        _parameterGroupRepository.GetByIdAsync(nonExistentId, Arg.Any<CancellationToken>())
            .ReturnsNull();

        var command = new DeleteParameterGroupCommand(nonExistentId);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("CFG_GROUP_NOT_FOUND");
    }
}
