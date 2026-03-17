using FluentAssertions;
using NSubstitute;
using SA.Catalog.Application.Queries.Products;
using SA.Catalog.DataAccess.Interface.Repositories;
using SA.Catalog.Domain.Entities;
using Xunit;

namespace SA.Catalog.Tests.Unit.Queries;

public sealed class ExportProductsQueryHandlerTests
{
    private readonly IProductRepository _productRepository = Substitute.For<IProductRepository>();
    private readonly ExportProductsQueryHandler _sut;

    public ExportProductsQueryHandlerTests()
    {
        _sut = new ExportProductsQueryHandler(_productRepository);
    }

    [Fact]
    public async Task TP_CAT_02_06_Export_Xlsx_Succeeds()
    {
        // Arrange
        _productRepository.CountForExportAsync(null, null, null, null, true, Arg.Any<CancellationToken>())
            .Returns(5);
        _productRepository.ListForExportAsync(null, null, null, null, true, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<Product>() as IReadOnlyList<Product>);

        var query = new ExportProductsQuery(Guid.NewGuid(), "xlsx", null, null, null, null);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.ContentType.Should().Be("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        result.FileName.Should().Be("productos.xlsx");
        result.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task Export_ExceedsLimit_ThrowsCAT_EXPORT_LIMIT_EXCEEDED()
    {
        // Arrange
        _productRepository.CountForExportAsync(null, null, null, null, true, Arg.Any<CancellationToken>())
            .Returns(10_001);

        var query = new ExportProductsQuery(Guid.NewGuid(), "xlsx", null, null, null, null);

        // Act
        Func<Task> act = async () => await _sut.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("CAT_EXPORT_LIMIT_EXCEEDED");
    }

    [Fact]
    public async Task Export_CsvFormat_ReturnsCsvContentType()
    {
        // Arrange
        _productRepository.CountForExportAsync(null, null, null, null, true, Arg.Any<CancellationToken>())
            .Returns(2);
        _productRepository.ListForExportAsync(null, null, null, null, true, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<Product>() as IReadOnlyList<Product>);

        var query = new ExportProductsQuery(Guid.NewGuid(), "csv", null, null, null, null);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.ContentType.Should().Be("text/csv");
        result.FileName.Should().Be("productos.csv");
    }
}
