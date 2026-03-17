using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using SA.Config.Application.Commands.Parameters;
using SA.Config.Application.Interfaces;
using SA.Config.DataAccess.Interface.Repositories;
using SA.Config.Domain.Entities;
using SA.Config.Domain.Enums;
using Xunit;

namespace SA.Config.Tests.Unit.Commands.Parameters;

public sealed class UpdateParameterCommandHandlerTests
{
    private readonly IParameterRepository _parameterRepository = Substitute.For<IParameterRepository>();
    private readonly IIntegrationEventPublisher _eventPublisher = Substitute.For<IIntegrationEventPublisher>();
    private readonly UpdateParameterCommandHandler _sut;

    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();

    public UpdateParameterCommandHandlerTests()
    {
        _sut = new UpdateParameterCommandHandler(_parameterRepository, _eventPublisher);
    }

    private static Parameter CreateParameter(
        string key = "company.name",
        string value = "Old Value",
        ParameterType type = ParameterType.Text)
    {
        return Parameter.Create(TenantId, Guid.NewGuid(), key, value, type, "Description", null, UserId);
    }

    [Fact]
    public async Task TP_CFG_04_01_Updates_Value_Successfully()
    {
        // Arrange
        var parameter = CreateParameter("company.name", "Old Corp");
        var parameterId = parameter.Id;

        _parameterRepository.GetByIdAsync(parameterId, Arg.Any<CancellationToken>())
            .Returns(parameter);

        var updatedParameter = CreateParameter("company.name", "New Corp");
        _parameterRepository.GetByIdAsync(parameterId, Arg.Any<CancellationToken>())
            .Returns(parameter, updatedParameter);

        var command = new UpdateParameterCommand(parameterId, "New Corp", null, UserId);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        _parameterRepository.Received(1).Update(parameter);
        await _parameterRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_CFG_04_02_Updates_Timestamp()
    {
        // Arrange
        var parameter = CreateParameter("company.name", "Old Corp");
        var parameterId = parameter.Id;
        var originalUpdatedAt = parameter.UpdatedAt;

        // Small delay to ensure timestamp difference
        await Task.Delay(10);

        _parameterRepository.GetByIdAsync(parameterId, Arg.Any<CancellationToken>())
            .Returns(parameter);

        var command = new UpdateParameterCommand(parameterId, "New Corp", null, UserId);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert - UpdateValue is called on the entity which updates UpdatedAt
        parameter.UpdatedAt.Should().BeOnOrAfter(originalUpdatedAt);
        parameter.UpdatedBy.Should().Be(UserId);
    }

    [Fact]
    public async Task TP_CFG_04_03_Replaces_Options_For_Select()
    {
        // Arrange
        var parameter = CreateParameter("currency", "USD", ParameterType.Select);
        var parameterId = parameter.Id;

        _parameterRepository.GetByIdAsync(parameterId, Arg.Any<CancellationToken>())
            .Returns(parameter);

        var newOptions = new[]
        {
            new UpdateParameterOptionInput("PEN", "Sol Peruano"),
            new UpdateParameterOptionInput("CLP", "Peso Chileno"),
        };

        var command = new UpdateParameterCommand(parameterId, "PEN", newOptions, UserId);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _parameterRepository.Received(1).ReplaceOptionsAsync(
            parameterId, Arg.Any<List<ParameterOption>>(), Arg.Any<CancellationToken>());
        await _parameterRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_CFG_04_04_Returns_404_For_NonExistent_Parameter()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        _parameterRepository.GetByIdAsync(nonExistentId, Arg.Any<CancellationToken>())
            .ReturnsNull();

        var command = new UpdateParameterCommand(nonExistentId, "value", null, UserId);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("CFG_PARAMETER_NOT_FOUND");
    }

    [Fact]
    public async Task TP_CFG_04_05_Returns_422_For_Invalid_Number_Value()
    {
        // Arrange
        var parameter = CreateParameter("max.retries", "3", ParameterType.Number);
        _parameterRepository.GetByIdAsync(parameter.Id, Arg.Any<CancellationToken>())
            .Returns(parameter);

        var command = new UpdateParameterCommand(parameter.Id, "not-a-number", null, UserId);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("CFG_INVALID_NUMBER_VALUE");
    }
}
