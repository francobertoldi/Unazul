using FluentAssertions;
using SA.Catalog.Domain;
using Xunit;

namespace SA.Catalog.Tests.Unit.Domain;

public sealed class ProductCategoryHelperTests
{
    [Theory]
    [InlineData("PREST001", "loan")]
    [InlineData("SEG001", "insurance")]
    [InlineData("CTA001", "account")]
    [InlineData("TARJETA001", "card")]
    [InlineData("INV001", "investment")]
    public void GetCategoryFromCode_WithValidPrefix_ReturnsCorrectCategory(string code, string expected)
    {
        // Act
        var result = ProductCategory.GetCategoryFromCode(code);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void GetCategoryFromCode_WithUnknownPrefix_ReturnsNull()
    {
        // Act
        var result = ProductCategory.GetCategoryFromCode("UNKNOWN001");

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [InlineData("PREST001", true)]
    [InlineData("SEG001", true)]
    [InlineData("CTA001", true)]
    [InlineData("TARJETA001", true)]
    [InlineData("INV001", true)]
    [InlineData("UNKNOWN", false)]
    [InlineData("XYZ123", false)]
    public void IsValidPrefix_ReturnsExpectedResult(string code, bool expected)
    {
        // Act
        var result = ProductCategory.IsValidPrefix(code);

        // Assert
        result.Should().Be(expected);
    }
}
