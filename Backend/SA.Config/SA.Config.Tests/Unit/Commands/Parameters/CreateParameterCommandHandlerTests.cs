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

public sealed class CreateParameterCommandHandlerTests
{
    private readonly IParameterRepository _parameterRepository = Substitute.For<IParameterRepository>();
    private readonly IParameterGroupRepository _parameterGroupRepository = Substitute.For<IParameterGroupRepository>();
    private readonly IIntegrationEventPublisher _eventPublisher = Substitute.For<IIntegrationEventPublisher>();
    private readonly CreateParameterCommandHandler _sut;

    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();

    public CreateParameterCommandHandlerTests()
    {
        _sut = new CreateParameterCommandHandler(
            _parameterRepository,
            _parameterGroupRepository,
            _eventPublisher);
    }

    private static ParameterGroup CreateGroup(string code = "general.company")
    {
        return ParameterGroup.Create(code, "Empresa", "General", "building", 1);
    }

    private void SetupGroupExists(ParameterGroup group)
    {
        _parameterGroupRepository.GetByIdAsync(group.Id, Arg.Any<CancellationToken>())
            .Returns(group);
    }

    private void SetupKeyNotExists(Guid groupId, string key)
    {
        _parameterRepository.ExistsByKeyAsync(TenantId, groupId, key, Arg.Any<CancellationToken>())
            .Returns(false);
    }

    private void SetupGetByIdReturnsCreated()
    {
        _parameterRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var id = callInfo.ArgAt<Guid>(0);
                var param = Parameter.Create(TenantId, Guid.NewGuid(), "key", "value", ParameterType.Text, "desc", null, UserId);
                return param;
            });
    }

    [Fact]
    public async Task TP_CFG_03_01_Creates_Text_Parameter_Successfully()
    {
        // Arrange
        var group = CreateGroup();
        SetupGroupExists(group);
        SetupKeyNotExists(group.Id, "company.name");

        var savedParameter = Parameter.Create(TenantId, group.Id, "company.name", "Unazul S.A.", ParameterType.Text, "Razon social", null, UserId);
        _parameterRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(savedParameter);

        var command = new CreateParameterCommand(
            TenantId, group.Id, "company.name", "Unazul S.A.", ParameterType.Text,
            "Razon social", null, null, UserId);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Key.Should().Be("company.name");
        result.Value.Should().Be("Unazul S.A.");
        result.Type.Should().Be(ParameterType.Text);
        result.Description.Should().Be("Razon social");
        result.ParentKey.Should().BeNull();

        await _parameterRepository.Received(1).AddAsync(Arg.Any<Parameter>(), Arg.Any<CancellationToken>());
        await _parameterRepository.Received().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_CFG_03_02_Creates_Select_With_Options()
    {
        // Arrange
        var group = CreateGroup();
        SetupGroupExists(group);
        SetupKeyNotExists(group.Id, "currency");

        var options = new[]
        {
            new CreateParameterOptionInput("USD", "US Dollar"),
            new CreateParameterOptionInput("EUR", "Euro"),
        };

        var savedParameter = Parameter.Create(TenantId, group.Id, "currency", "USD", ParameterType.Select, "Moneda", null, UserId);
        var opt1 = ParameterOption.Create(savedParameter.Id, TenantId, "USD", "US Dollar", 0);
        var opt2 = ParameterOption.Create(savedParameter.Id, TenantId, "EUR", "Euro", 1);
        savedParameter.UpdateValue("USD", new List<ParameterOption> { opt1, opt2 }, UserId);

        _parameterRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(savedParameter);

        var command = new CreateParameterCommand(
            TenantId, group.Id, "currency", "USD", ParameterType.Select,
            "Moneda", null, options, UserId);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Key.Should().Be("currency");
        result.Type.Should().Be(ParameterType.Select);
        result.Options.Should().HaveCount(2);
        result.Options[0].OptionValue.Should().Be("USD");
        result.Options[1].OptionValue.Should().Be("EUR");

        await _parameterRepository.Received(1).ReplaceOptionsAsync(
            Arg.Any<Guid>(), Arg.Any<IEnumerable<ParameterOption>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_CFG_03_03_Creates_With_ParentKey()
    {
        // Arrange
        var group = CreateGroup();
        SetupGroupExists(group);
        SetupKeyNotExists(group.Id, "city.lima");

        var savedParameter = Parameter.Create(TenantId, group.Id, "city.lima", "Lima", ParameterType.Text, "Ciudad", "region.norte", UserId);
        _parameterRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(savedParameter);

        var command = new CreateParameterCommand(
            TenantId, group.Id, "city.lima", "Lima", ParameterType.Text,
            "Ciudad", "region.norte", null, UserId);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.ParentKey.Should().Be("region.norte");
        result.Key.Should().Be("city.lima");
    }

    [Fact]
    public async Task TP_CFG_03_04_Returns_409_For_Duplicate_Key()
    {
        // Arrange
        var group = CreateGroup();
        SetupGroupExists(group);
        _parameterRepository.ExistsByKeyAsync(TenantId, group.Id, "company.name", Arg.Any<CancellationToken>())
            .Returns(true);

        var command = new CreateParameterCommand(
            TenantId, group.Id, "company.name", "Unazul", ParameterType.Text,
            "Razon social", null, null, UserId);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("CFG_DUPLICATE_KEY");
    }

    [Fact]
    public async Task TP_CFG_03_05_Returns_404_For_NonExistent_Group()
    {
        // Arrange
        var nonExistentGroupId = Guid.NewGuid();
        _parameterGroupRepository.GetByIdAsync(nonExistentGroupId, Arg.Any<CancellationToken>())
            .ReturnsNull();

        var command = new CreateParameterCommand(
            TenantId, nonExistentGroupId, "key", "value", ParameterType.Text,
            "desc", null, null, UserId);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("CFG_GROUP_NOT_FOUND");
    }

    [Fact]
    public async Task TP_CFG_03_06_Returns_422_For_Select_Without_Options()
    {
        // Arrange
        var group = CreateGroup();
        SetupGroupExists(group);
        SetupKeyNotExists(group.Id, "currency");

        var command = new CreateParameterCommand(
            TenantId, group.Id, "currency", "USD", ParameterType.Select,
            "Moneda", null, null, UserId);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("CFG_OPTIONS_REQUIRED");
    }

    [Fact]
    public async Task TP_CFG_03_07_Returns_422_For_Empty_Required_Fields()
    {
        // Arrange - List type also requires options
        var group = CreateGroup();
        SetupGroupExists(group);
        SetupKeyNotExists(group.Id, "tags");

        var command = new CreateParameterCommand(
            TenantId, group.Id, "tags", "tag1", ParameterType.List,
            "Tags", null, Array.Empty<CreateParameterOptionInput>(), UserId);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("CFG_OPTIONS_REQUIRED");
    }

    [Fact]
    public async Task TP_CFG_03_09_Publishes_ParameterUpdatedEvent()
    {
        // Arrange
        var group = CreateGroup();
        SetupGroupExists(group);
        SetupKeyNotExists(group.Id, "company.name");

        var savedParameter = Parameter.Create(TenantId, group.Id, "company.name", "Unazul", ParameterType.Text, "Razon social", null, UserId);
        _parameterRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(savedParameter);

        var command = new CreateParameterCommand(
            TenantId, group.Id, "company.name", "Unazul", ParameterType.Text,
            "Razon social", null, null, UserId);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _eventPublisher.Received(1).PublishAsync(
            Arg.Is<ParameterUpdatedEvent>(e =>
                e.TenantId == TenantId &&
                e.ParameterCode == "company.name" &&
                e.OldValue == null &&
                e.NewValue == "Unazul" &&
                e.UpdatedBy == UserId),
            Arg.Any<CancellationToken>());
    }
}
