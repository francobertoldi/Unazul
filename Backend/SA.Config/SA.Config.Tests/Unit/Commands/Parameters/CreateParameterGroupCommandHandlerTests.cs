using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using SA.Config.Application.Commands.Parameters;
using SA.Config.DataAccess.Interface.Repositories;
using SA.Config.Domain.Entities;
using Xunit;

namespace SA.Config.Tests.Unit.Commands.Parameters;

public sealed class CreateParameterGroupCommandHandlerTests
{
    private readonly IParameterGroupRepository _parameterGroupRepository = Substitute.For<IParameterGroupRepository>();
    private readonly CreateParameterGroupCommandHandler _sut;

    public CreateParameterGroupCommandHandlerTests()
    {
        _sut = new CreateParameterGroupCommandHandler(_parameterGroupRepository);
    }

    [Fact]
    public async Task TP_CFG_06_01_Creates_Group_Successfully()
    {
        // Arrange
        _parameterGroupRepository.GetByCodeAsync("general.company", Arg.Any<CancellationToken>())
            .ReturnsNull();

        var command = new CreateParameterGroupCommand(
            "general.company", "Empresa", "General", "building", 1);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Code.Should().Be("general.company");
        result.Name.Should().Be("Empresa");
        result.Icon.Should().Be("building");
        result.SortOrder.Should().Be(1);
        result.Id.Should().NotBeEmpty();

        await _parameterGroupRepository.Received(1).AddAsync(Arg.Any<ParameterGroup>(), Arg.Any<CancellationToken>());
        await _parameterGroupRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_CFG_06_03_Returns_409_For_Duplicate_Code()
    {
        // Arrange
        var existingGroup = ParameterGroup.Create("general.company", "Empresa", "General", "building", 1);
        _parameterGroupRepository.GetByCodeAsync("general.company", Arg.Any<CancellationToken>())
            .Returns(existingGroup);

        var command = new CreateParameterGroupCommand(
            "general.company", "Empresa Duplicada", "General", "building", 2);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("CFG_DUPLICATE_GROUP_CODE");
    }
}
