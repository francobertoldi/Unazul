using FluentAssertions;
using NSubstitute;
using SA.Config.Application.Queries.Parameters;
using SA.Config.DataAccess.Interface.Repositories;
using SA.Config.Domain.Entities;
using Xunit;

namespace SA.Config.Tests.Unit.Queries.Parameters;

public sealed class ListParameterGroupsQueryHandlerTests
{
    private readonly IParameterGroupRepository _parameterGroupRepository = Substitute.For<IParameterGroupRepository>();
    private readonly ListParameterGroupsQueryHandler _sut;

    public ListParameterGroupsQueryHandlerTests()
    {
        _sut = new ListParameterGroupsQueryHandler(_parameterGroupRepository);
    }

    private static ParameterGroup CreateGroup(string code, string name, string category, string icon, int sortOrder)
    {
        return ParameterGroup.Create(code, name, category, icon, sortOrder);
    }

    [Fact]
    public async Task TP_CFG_01_01_Returns_Categories_Grouped_By_Category()
    {
        // Arrange
        var groups = new List<ParameterGroup>
        {
            CreateGroup("general.company", "Empresa", "General", "building", 1),
            CreateGroup("general.locale", "Idioma", "General", "globe", 2),
            CreateGroup("billing.tax", "Impuestos", "Facturacion", "receipt", 1),
        };
        _parameterGroupRepository.GetAllOrderedAsync(Arg.Any<CancellationToken>())
            .Returns(groups.AsReadOnly());

        // Act
        var result = await _sut.Handle(new ListParameterGroupsQuery(), CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result[0].Name.Should().Be("Facturacion");
        result[0].Groups.Should().HaveCount(1);
        result[1].Name.Should().Be("General");
        result[1].Groups.Should().HaveCount(2);
    }

    [Fact]
    public async Task TP_CFG_01_02_Returns_Groups_Ordered_By_SortOrder()
    {
        // Arrange
        var groups = new List<ParameterGroup>
        {
            CreateGroup("general.locale", "Idioma", "General", "globe", 3),
            CreateGroup("general.company", "Empresa", "General", "building", 1),
            CreateGroup("general.region", "Region", "General", "map", 2),
        };
        _parameterGroupRepository.GetAllOrderedAsync(Arg.Any<CancellationToken>())
            .Returns(groups.AsReadOnly());

        // Act
        var result = await _sut.Handle(new ListParameterGroupsQuery(), CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        var category = result[0];
        category.Groups.Should().HaveCount(3);
        category.Groups[0].Code.Should().Be("general.company");
        category.Groups[0].SortOrder.Should().Be(1);
        category.Groups[1].Code.Should().Be("general.region");
        category.Groups[1].SortOrder.Should().Be(2);
        category.Groups[2].Code.Should().Be("general.locale");
        category.Groups[2].SortOrder.Should().Be(3);
    }

    [Fact]
    public async Task Returns_Empty_When_No_Groups()
    {
        // Arrange
        _parameterGroupRepository.GetAllOrderedAsync(Arg.Any<CancellationToken>())
            .Returns(new List<ParameterGroup>().AsReadOnly());

        // Act
        var result = await _sut.Handle(new ListParameterGroupsQuery(), CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }
}
