using FluentAssertions;
using SA.Config.DataAccess.EntityFramework.Seed;
using Xunit;

namespace SA.Config.Tests.Integration.Seed;

public sealed class ParameterGroupSeedTests
{
    [Fact]
    public void Seed_Has_14_Groups()
    {
        // Act
        var groups = ParameterGroupSeedData.GetSeedObjects();

        // Assert
        groups.Should().HaveCount(14);
    }

    [Fact]
    public void Seed_Groups_Have_4_Categories()
    {
        // Act
        var groups = ParameterGroupSeedData.GetSeedObjects();

        // Assert
        var categories = groups
            .Select(g => (string)g.GetType().GetProperty("Category")!.GetValue(g)!)
            .Distinct()
            .ToList();

        categories.Should().HaveCount(4);
        categories.Should().Contain("General");
        categories.Should().Contain("Tecnico");
        categories.Should().Contain("Notificaciones");
        categories.Should().Contain("Datos");
    }

    [Fact]
    public void Seed_Groups_Have_Unique_Codes()
    {
        // Act
        var groups = ParameterGroupSeedData.GetSeedObjects();

        // Assert
        var codes = groups
            .Select(g => (string)g.GetType().GetProperty("Code")!.GetValue(g)!)
            .ToList();

        codes.Should().OnlyHaveUniqueItems();
    }
}
