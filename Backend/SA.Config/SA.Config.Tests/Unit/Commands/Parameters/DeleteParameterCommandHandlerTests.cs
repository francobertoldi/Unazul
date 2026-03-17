using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using SA.Config.Application.Commands.Parameters;
using SA.Config.Application.Interfaces;
using SA.Config.DataAccess.Interface.Repositories;
using SA.Config.Domain.Entities;
using SA.Config.Domain.Enums;
using Shared.Contract.Events;
using Xunit;

namespace SA.Config.Tests.Unit.Commands.Parameters;

public sealed class DeleteParameterCommandHandlerTests
{
    private readonly IParameterRepository _parameterRepository = Substitute.For<IParameterRepository>();
    private readonly IIntegrationEventPublisher _eventPublisher = Substitute.For<IIntegrationEventPublisher>();
    private readonly DeleteParameterCommandHandler _sut;

    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();

    public DeleteParameterCommandHandlerTests()
    {
        _sut = new DeleteParameterCommandHandler(_parameterRepository, _eventPublisher);
    }

    private static Parameter CreateParameter(
        string key = "company.name",
        string value = "Unazul",
        ParameterType type = ParameterType.Text)
    {
        return Parameter.Create(TenantId, Guid.NewGuid(), key, value, type, "Description", null, UserId);
    }

    [Fact]
    public async Task TP_CFG_05_01_Deletes_Successfully_Returns_204()
    {
        // Arrange
        var parameter = CreateParameter();
        _parameterRepository.GetByIdAsync(parameter.Id, Arg.Any<CancellationToken>())
            .Returns(parameter);

        var command = new DeleteParameterCommand(parameter.Id);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Mediator.Unit.Value);
        await _parameterRepository.Received(1).DeleteAsync(parameter, Arg.Any<CancellationToken>());
        await _parameterRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_CFG_05_02_Deletes_With_Options_Cascaded()
    {
        // Arrange
        var parameter = CreateParameter("currency", "USD", ParameterType.Select);
        var opt1 = ParameterOption.Create(parameter.Id, TenantId, "USD", "US Dollar", 0);
        var opt2 = ParameterOption.Create(parameter.Id, TenantId, "EUR", "Euro", 1);
        parameter.UpdateValue("USD", new List<ParameterOption> { opt1, opt2 }, UserId);

        _parameterRepository.GetByIdAsync(parameter.Id, Arg.Any<CancellationToken>())
            .Returns(parameter);

        var command = new DeleteParameterCommand(parameter.Id);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert - DeleteAsync is responsible for cascading options deletion
        result.Should().Be(Mediator.Unit.Value);
        await _parameterRepository.Received(1).DeleteAsync(parameter, Arg.Any<CancellationToken>());
        await _parameterRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());

        // Verify event published with correct old value
        await _eventPublisher.Received(1).PublishAsync(
            Arg.Is<ParameterUpdatedEvent>(e =>
                e.ParameterCode == "currency" &&
                e.OldValue == "USD" &&
                e.NewValue == null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_CFG_05_03_Returns_404_For_NonExistent()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        _parameterRepository.GetByIdAsync(nonExistentId, Arg.Any<CancellationToken>())
            .ReturnsNull();

        var command = new DeleteParameterCommand(nonExistentId);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("CFG_PARAMETER_NOT_FOUND");
    }
}
