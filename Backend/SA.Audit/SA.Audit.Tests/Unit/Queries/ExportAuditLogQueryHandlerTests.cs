using FluentAssertions;
using NSubstitute;
using SA.Audit.Application.Queries;
using SA.Audit.DataAccess.Interface.Repositories;
using SA.Audit.Domain.Entities;
using Xunit;

namespace SA.Audit.Tests.Unit.Queries;

public sealed class ExportAuditLogQueryHandlerTests
{
    private readonly IAuditLogRepository _repo = Substitute.For<IAuditLogRepository>();
    private readonly ExportAuditLogQueryHandler _sut;
    private readonly Guid _tenantId = Guid.NewGuid();

    public ExportAuditLogQueryHandlerTests()
    {
        _sut = new ExportAuditLogQueryHandler(_repo);
    }

    private static AuditLog CreateSampleAuditLog(Guid? tenantId = null) =>
        AuditLog.Create(
            tenantId ?? Guid.NewGuid(),
            Guid.NewGuid(),
            "admin",
            "Crear",
            "Usuarios",
            "CrearUsuario",
            "Detalle",
            "127.0.0.1",
            "User",
            Guid.NewGuid(),
            "{}",
            DateTimeOffset.UtcNow.AddMinutes(-10));

    [Fact]
    public async Task TP_AUD_22_Export_Xlsx_Returns_ValidFile()
    {
        // Arrange
        var logs = new List<AuditLog> { CreateSampleAuditLog(_tenantId), CreateSampleAuditLog(_tenantId) };
        var query = new ExportAuditLogQuery(_tenantId, "xlsx");

        _repo.CountAsync(_tenantId, null, null, null, null, null, Arg.Any<CancellationToken>())
            .Returns(2);
        _repo.ListForExportAsync(_tenantId, null, null, null, null, null, Arg.Any<CancellationToken>())
            .Returns(logs.AsReadOnly());

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Data.Should().NotBeEmpty();
        result.ContentType.Should().Be("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        result.FileName.Should().EndWith(".xlsx");
    }

    [Fact]
    public async Task TP_AUD_23_Export_Csv_Returns_ValidFile()
    {
        // Arrange
        var logs = new List<AuditLog> { CreateSampleAuditLog(_tenantId) };
        var query = new ExportAuditLogQuery(_tenantId, "csv");

        _repo.CountAsync(_tenantId, null, null, null, null, null, Arg.Any<CancellationToken>())
            .Returns(1);
        _repo.ListForExportAsync(_tenantId, null, null, null, null, null, Arg.Any<CancellationToken>())
            .Returns(logs.AsReadOnly());

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Data.Should().NotBeEmpty();
        result.ContentType.Should().Be("text/csv");
        result.FileName.Should().EndWith(".csv");
    }

    [Fact]
    public async Task TP_AUD_24_Export_Empty_Returns_File_With_Headers_Only()
    {
        // Arrange
        var query = new ExportAuditLogQuery(_tenantId, "csv");

        _repo.CountAsync(_tenantId, null, null, null, null, null, Arg.Any<CancellationToken>())
            .Returns(0);
        _repo.ListForExportAsync(_tenantId, null, null, null, null, null, Arg.Any<CancellationToken>())
            .Returns(new List<AuditLog>().AsReadOnly());

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Data.Should().NotBeEmpty("even empty exports should contain headers");
        result.ContentType.Should().Be("text/csv");
    }

    [Fact]
    public async Task TP_AUD_30_Export_Over_10000_Throws_LimitExceeded()
    {
        // Arrange
        var query = new ExportAuditLogQuery(_tenantId, "xlsx");

        _repo.CountAsync(_tenantId, null, null, null, null, null, Arg.Any<CancellationToken>())
            .Returns(10_001);

        // Act
        Func<Task> act = async () => await _sut.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("AUD_EXPORT_LIMIT_EXCEEDED");
    }

    [Fact]
    public async Task TP_AUD_28_Export_Invalid_Format_Throws()
    {
        // Arrange
        var query = new ExportAuditLogQuery(_tenantId, "pdf");

        // Act
        Func<Task> act = async () => await _sut.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("AUD_INVALID_FORMAT");
    }

    [Fact]
    public async Task TP_AUD_29_Export_Inverted_DateRange_Throws()
    {
        // Arrange
        var from = DateTimeOffset.UtcNow;
        var to = DateTimeOffset.UtcNow.AddDays(-7);
        var query = new ExportAuditLogQuery(_tenantId, "xlsx", From: from, To: to);

        // Act
        Func<Task> act = async () => await _sut.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("AUD_INVALID_DATE_RANGE");
    }

    [Fact]
    public async Task TP_AUD_25_Export_Exactly_10000_Succeeds()
    {
        // Arrange
        var query = new ExportAuditLogQuery(_tenantId, "csv");

        _repo.CountAsync(_tenantId, null, null, null, null, null, Arg.Any<CancellationToken>())
            .Returns(10_000);
        _repo.ListForExportAsync(_tenantId, null, null, null, null, null, Arg.Any<CancellationToken>())
            .Returns(new List<AuditLog>().AsReadOnly());

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Data.Should().NotBeEmpty();
    }
}
