using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using SA.Config.Application.Queries.Parameters;
using SA.Config.DataAccess.Interface.Repositories;
using SA.Config.Domain.Entities;
using SA.Config.Domain.Enums;
using Xunit;

namespace SA.Config.Tests.Unit.Queries.Parameters;

public sealed class ListParametersQueryHandlerTests
{
    private readonly IParameterGroupRepository _parameterGroupRepository = Substitute.For<IParameterGroupRepository>();
    private readonly IParameterRepository _parameterRepository = Substitute.For<IParameterRepository>();
    private readonly ListParametersQueryHandler _sut;

    private static readonly Guid TenantId = Guid.NewGuid();

    public ListParametersQueryHandlerTests()
    {
        _sut = new ListParametersQueryHandler(_parameterGroupRepository, _parameterRepository);
    }

    private static ParameterGroup CreateGroup(string code = "general.company")
    {
        return ParameterGroup.Create(code, "Empresa", "General", "building", 1);
    }

    private static Parameter CreateParameter(
        Guid groupId,
        string key = "company.name",
        string value = "Unazul",
        ParameterType type = ParameterType.Text,
        string? parentKey = null)
    {
        return Parameter.Create(TenantId, groupId, key, value, type, "Description", parentKey, Guid.NewGuid());
    }

    [Fact]
    public async Task TP_CFG_02_01_Returns_Parameters_For_Existing_Group()
    {
        // Arrange
        var group = CreateGroup();
        var groupId = group.Id;
        var parameters = new List<Parameter>
        {
            CreateParameter(groupId, "company.name", "Unazul"),
            CreateParameter(groupId, "company.ruc", "123456789"),
        };

        _parameterGroupRepository.GetByIdAsync(groupId, Arg.Any<CancellationToken>())
            .Returns(group);
        _parameterRepository.GetByGroupIdAsync(groupId, null, Arg.Any<CancellationToken>())
            .Returns(parameters.AsReadOnly());

        // Act
        var result = await _sut.Handle(new ListParametersQuery(groupId, null), CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result[0].Key.Should().Be("company.name");
        result[0].Value.Should().Be("Unazul");
        result[1].Key.Should().Be("company.ruc");
    }

    [Fact]
    public async Task TP_CFG_02_02_Select_Type_Includes_Options()
    {
        // Arrange
        var group = CreateGroup();
        var groupId = group.Id;
        var parameter = CreateParameter(groupId, "currency", "USD", ParameterType.Select);
        var opt1 = ParameterOption.Create(parameter.Id, TenantId, "USD", "US Dollar", 0);
        var opt2 = ParameterOption.Create(parameter.Id, TenantId, "EUR", "Euro", 1);
        parameter.UpdateValue(parameter.Value, new List<ParameterOption> { opt2, opt1 }, Guid.NewGuid());

        _parameterGroupRepository.GetByIdAsync(groupId, Arg.Any<CancellationToken>())
            .Returns(group);
        _parameterRepository.GetByGroupIdAsync(groupId, null, Arg.Any<CancellationToken>())
            .Returns(new List<Parameter> { parameter }.AsReadOnly());

        // Act
        var result = await _sut.Handle(new ListParametersQuery(groupId, null), CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result[0].Type.Should().Be(ParameterType.Select);
        result[0].Options.Should().HaveCount(2);
        result[0].Options[0].SortOrder.Should().Be(0);
        result[0].Options[0].OptionValue.Should().Be("USD");
        result[0].Options[1].SortOrder.Should().Be(1);
        result[0].Options[1].OptionValue.Should().Be("EUR");
    }

    [Fact]
    public async Task TP_CFG_02_03_ParentKey_Filters_Results()
    {
        // Arrange
        var group = CreateGroup();
        var groupId = group.Id;
        var parentKey = "region.norte";
        var parameters = new List<Parameter>
        {
            CreateParameter(groupId, "city.lima", "Lima", parentKey: parentKey),
        };

        _parameterGroupRepository.GetByIdAsync(groupId, Arg.Any<CancellationToken>())
            .Returns(group);
        _parameterRepository.GetByGroupIdAsync(groupId, parentKey, Arg.Any<CancellationToken>())
            .Returns(parameters.AsReadOnly());

        // Act
        var result = await _sut.Handle(new ListParametersQuery(groupId, parentKey), CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result[0].ParentKey.Should().Be(parentKey);

        // Verify the repository was called with the parentKey filter
        await _parameterRepository.Received(1).GetByGroupIdAsync(groupId, parentKey, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_CFG_02_04_Returns_404_For_NonExistent_Group()
    {
        // Arrange
        var nonExistentGroupId = Guid.NewGuid();
        _parameterGroupRepository.GetByIdAsync(nonExistentGroupId, Arg.Any<CancellationToken>())
            .ReturnsNull();

        // Act
        Func<Task> act = async () => await _sut.Handle(
            new ListParametersQuery(nonExistentGroupId, null), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("CFG_GROUP_NOT_FOUND");
    }
}
