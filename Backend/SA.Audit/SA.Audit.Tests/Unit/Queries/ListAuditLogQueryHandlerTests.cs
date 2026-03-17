using FluentAssertions;
using NSubstitute;
using SA.Audit.Application.Queries;
using SA.Audit.DataAccess.Interface.Repositories;
using SA.Audit.Domain.Entities;
using Xunit;

namespace SA.Audit.Tests.Unit.Queries;

public sealed class ListAuditLogQueryHandlerTests
{
    private readonly IAuditLogRepository _repo = Substitute.For<IAuditLogRepository>();
    private readonly ListAuditLogQueryHandler _sut;
    private readonly Guid _tenantId = Guid.NewGuid();

    public ListAuditLogQueryHandlerTests()
    {
        _sut = new ListAuditLogQueryHandler(_repo);
    }

    private static AuditLog CreateSampleAuditLog(
        Guid? tenantId = null,
        string module = "Usuarios",
        string operation = "Crear") =>
        AuditLog.Create(
            tenantId ?? Guid.NewGuid(),
            Guid.NewGuid(),
            "admin",
            operation,
            module,
            "TestAction",
            null,
            "127.0.0.1",
            null,
            null,
            null,
            DateTimeOffset.UtcNow.AddMinutes(-10));

    [Fact]
    public async Task TP_AUD_01_List_Without_Filters_Returns_Paginated_Result()
    {
        // Arrange
        var log1 = CreateSampleAuditLog(_tenantId);
        var log2 = CreateSampleAuditLog(_tenantId);
        var query = new ListAuditLogQuery(_tenantId, 1, 20);

        _repo.ListAsync(_tenantId, 0, 20, null, null, null, null, null, "occurred_at", "desc", Arg.Any<CancellationToken>())
            .Returns((new List<AuditLog> { log1, log2 }.AsReadOnly(), 2));

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(2);
        result.Total.Should().Be(2);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(20);
    }

    [Fact]
    public async Task TP_AUD_02_List_With_UserId_Filter_Passes_To_Repository()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new ListAuditLogQuery(_tenantId, 1, 20, UserId: userId);

        _repo.ListAsync(_tenantId, 0, 20, userId, null, null, null, null, "occurred_at", "desc", Arg.Any<CancellationToken>())
            .Returns((new List<AuditLog>().AsReadOnly(), 0));

        // Act
        await _sut.Handle(query, CancellationToken.None);

        // Assert
        await _repo.Received(1).ListAsync(
            _tenantId, 0, 20, userId, null, null, null, null, "occurred_at", "desc", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_AUD_03_List_With_Operation_Filter_Passes_To_Repository()
    {
        // Arrange
        var query = new ListAuditLogQuery(_tenantId, 1, 20, Operation: "Crear");

        _repo.ListAsync(_tenantId, 0, 20, null, "Crear", null, null, null, "occurred_at", "desc", Arg.Any<CancellationToken>())
            .Returns((new List<AuditLog>().AsReadOnly(), 0));

        // Act
        await _sut.Handle(query, CancellationToken.None);

        // Assert
        await _repo.Received(1).ListAsync(
            _tenantId, 0, 20, null, "Crear", null, null, null, "occurred_at", "desc", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_AUD_04_List_With_Module_Filter_Passes_To_Repository()
    {
        // Arrange
        var query = new ListAuditLogQuery(_tenantId, 1, 20, Module: "Usuarios");

        _repo.ListAsync(_tenantId, 0, 20, null, null, "Usuarios", null, null, "occurred_at", "desc", Arg.Any<CancellationToken>())
            .Returns((new List<AuditLog>().AsReadOnly(), 0));

        // Act
        await _sut.Handle(query, CancellationToken.None);

        // Assert
        await _repo.Received(1).ListAsync(
            _tenantId, 0, 20, null, null, "Usuarios", null, null, "occurred_at", "desc", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_AUD_05_List_With_DateRange_Filter_Passes_To_Repository()
    {
        // Arrange
        var from = DateTimeOffset.UtcNow.AddDays(-7);
        var to = DateTimeOffset.UtcNow;
        var query = new ListAuditLogQuery(_tenantId, 1, 20, From: from, To: to);

        _repo.ListAsync(_tenantId, 0, 20, null, null, null, from, to, "occurred_at", "desc", Arg.Any<CancellationToken>())
            .Returns((new List<AuditLog>().AsReadOnly(), 0));

        // Act
        await _sut.Handle(query, CancellationToken.None);

        // Assert
        await _repo.Received(1).ListAsync(
            _tenantId, 0, 20, null, null, null, from, to, "occurred_at", "desc", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_AUD_12_List_With_Invalid_Operation_Throws()
    {
        // Arrange
        var query = new ListAuditLogQuery(_tenantId, 1, 20, Operation: "InvalidOp");

        // Act
        Func<Task> act = async () => await _sut.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("AUD_INVALID_OPERATION");
    }

    [Fact]
    public async Task TP_AUD_11_List_With_Inverted_DateRange_Throws()
    {
        // Arrange
        var from = DateTimeOffset.UtcNow;
        var to = DateTimeOffset.UtcNow.AddDays(-7);
        var query = new ListAuditLogQuery(_tenantId, 1, 20, From: from, To: to);

        // Act
        Func<Task> act = async () => await _sut.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("AUD_INVALID_DATE_RANGE");
    }

    [Fact]
    public async Task TP_AUD_07_List_Empty_Result_Returns_Zero_Total()
    {
        // Arrange
        var query = new ListAuditLogQuery(_tenantId, 1, 20);

        _repo.ListAsync(_tenantId, 0, 20, null, null, null, null, null, "occurred_at", "desc", Arg.Any<CancellationToken>())
            .Returns((new List<AuditLog>().AsReadOnly(), 0));

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().BeEmpty();
        result.Total.Should().Be(0);
    }
}
