using FluentAssertions;
using NSubstitute;
using SA.Operations.Application.Commands.Settlements;
using SA.Operations.Application.Events;
using SA.Operations.Application.Interfaces;
using SA.Operations.DataAccess.Interface.Repositories;
using SA.Operations.Domain.Entities;
using SA.Operations.Domain.Enums;
using Xunit;
using AppEntity = SA.Operations.Domain.Entities.Application;

namespace SA.Operations.Tests.Unit.Commands.Settlements;

public sealed class ConfirmSettlementCommandHandlerTests
{
    private readonly IApplicationRepository _applicationRepository = Substitute.For<IApplicationRepository>();
    private readonly IApplicantRepository _applicantRepository = Substitute.For<IApplicantRepository>();
    private readonly ISettlementRepository _settlementRepository = Substitute.For<ISettlementRepository>();
    private readonly ICatalogServiceClient _catalogClient = Substitute.For<ICatalogServiceClient>();
    private readonly IFileStorageService _fileStorageService = Substitute.For<IFileStorageService>();
    private readonly IIntegrationEventPublisher _eventPublisher = Substitute.For<IIntegrationEventPublisher>();
    private readonly ConfirmSettlementCommandHandler _sut;

    public ConfirmSettlementCommandHandlerTests()
    {
        _sut = new ConfirmSettlementCommandHandler(
            _applicationRepository,
            _applicantRepository,
            _settlementRepository,
            _catalogClient,
            _fileStorageService,
            _eventPublisher);
    }

    private static AppEntity CreateApprovedApp(Guid tenantId, Guid applicantId, Guid productId, Guid planId, string code)
    {
        var app = AppEntity.Create(tenantId, Guid.NewGuid(), applicantId, code, productId, planId, "Product", "Plan", Guid.NewGuid());
        typeof(AppEntity).GetProperty("Status")!.SetValue(app, ApplicationStatus.Approved);
        return app;
    }

    private void SetupApprovedApps(Guid tenantId, Guid? entityId, DateTime dateFrom, DateTime dateTo, List<AppEntity> apps)
    {
        _applicationRepository.GetApprovedByDateRangeAsync(tenantId, entityId, dateFrom, dateTo, Arg.Any<CancellationToken>())
            .Returns(apps as IReadOnlyList<AppEntity>);
    }

    private void SetupCommissions(params CommissionPlanResult[] commissions)
    {
        _catalogClient.GetCommissionPlansAsync(Arg.Any<Guid[]>(), Arg.Any<Guid[]>(), Arg.Any<CancellationToken>())
            .Returns(commissions.ToList() as IReadOnlyList<CommissionPlanResult>);
    }

    [Fact]
    public async Task TP_OPS_14_01_ConfirmSettlement_Successfully()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var applicantId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var planId = Guid.NewGuid();
        var app = CreateApprovedApp(tenantId, applicantId, productId, planId, "OPS-001");

        var applicant = Applicant.Create(tenantId, "John", "Doe", DocumentType.DNI, "12345678", null, null, null);
        typeof(Applicant).GetProperty("Id")!.SetValue(applicant, applicantId);

        var dateFrom = DateTime.UtcNow.AddDays(-30);
        var dateTo = DateTime.UtcNow;
        var command = new ConfirmSettlementCommand(tenantId, null, dateFrom, dateTo, Guid.NewGuid(), "Admin");

        SetupApprovedApps(tenantId, null, dateFrom, dateTo, [app]);
        SetupCommissions(new CommissionPlanResult(productId, planId, "Fixed", 100m, "ARS", null));

        _applicantRepository.GetByIdAsync(applicantId, Arg.Any<CancellationToken>()).Returns(applicant);
        _fileStorageService.GenerateSettlementExcelAsync(tenantId, Arg.Any<Guid>(), Arg.Any<object>(), Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns("https://storage.example.com/settlements/excel.xlsx");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.SettlementId.Should().NotBeEmpty();
        result.ItemsCount.Should().Be(1);
        result.ExcelUrl.Should().Be("https://storage.example.com/settlements/excel.xlsx");

        await _settlementRepository.Received(1).AddAsync(Arg.Any<Settlement>(), Arg.Any<CancellationToken>());
        await _settlementRepository.Received().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_OPS_14_02_ThrowsOpsNoApprovedApplications_WhenEmpty()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var dateFrom = DateTime.UtcNow.AddDays(-30);
        var dateTo = DateTime.UtcNow;
        var command = new ConfirmSettlementCommand(tenantId, null, dateFrom, dateTo, Guid.NewGuid(), "Admin");

        SetupApprovedApps(tenantId, null, dateFrom, dateTo, []);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("OPS_NO_APPROVED_APPLICATIONS");
    }

    [Fact]
    public async Task TP_OPS_14_03_CallsBatchTransitionToSettledAsync()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var applicantId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var planId = Guid.NewGuid();
        var app = CreateApprovedApp(tenantId, applicantId, productId, planId, "OPS-001");
        var settledBy = Guid.NewGuid();

        var applicant = Applicant.Create(tenantId, "John", "Doe", DocumentType.DNI, "12345678", null, null, null);
        typeof(Applicant).GetProperty("Id")!.SetValue(applicant, applicantId);

        var dateFrom = DateTime.UtcNow.AddDays(-30);
        var dateTo = DateTime.UtcNow;
        var command = new ConfirmSettlementCommand(tenantId, null, dateFrom, dateTo, settledBy, "Admin");

        SetupApprovedApps(tenantId, null, dateFrom, dateTo, [app]);
        SetupCommissions(new CommissionPlanResult(productId, planId, "Fixed", 100m, "ARS", null));

        _applicantRepository.GetByIdAsync(applicantId, Arg.Any<CancellationToken>()).Returns(applicant);
        _fileStorageService.GenerateSettlementExcelAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<object>(), Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns((string?)null);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _applicationRepository.Received(1).BatchTransitionToSettledAsync(
            Arg.Is<IReadOnlyList<Guid>>(ids => ids.Count == 1 && ids[0] == app.Id),
            settledBy,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_OPS_14_04_GeneratesExcel()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var applicantId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var planId = Guid.NewGuid();
        var app = CreateApprovedApp(tenantId, applicantId, productId, planId, "OPS-001");

        var applicant = Applicant.Create(tenantId, "John", "Doe", DocumentType.DNI, "12345678", null, null, null);
        typeof(Applicant).GetProperty("Id")!.SetValue(applicant, applicantId);

        var dateFrom = DateTime.UtcNow.AddDays(-30);
        var dateTo = DateTime.UtcNow;
        var command = new ConfirmSettlementCommand(tenantId, null, dateFrom, dateTo, Guid.NewGuid(), "Admin");

        SetupApprovedApps(tenantId, null, dateFrom, dateTo, [app]);
        SetupCommissions(new CommissionPlanResult(productId, planId, "Fixed", 100m, "ARS", null));

        _applicantRepository.GetByIdAsync(applicantId, Arg.Any<CancellationToken>()).Returns(applicant);
        _fileStorageService.GenerateSettlementExcelAsync(tenantId, Arg.Any<Guid>(), Arg.Any<object>(), Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns("https://storage.example.com/excel.xlsx");

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _fileStorageService.Received(1).GenerateSettlementExcelAsync(
            tenantId,
            Arg.Any<Guid>(),
            Arg.Any<object>(),
            Arg.Any<object>(),
            Arg.Any<CancellationToken>());

        // Verify the settlement was updated with the Excel URL and saved again
        _settlementRepository.Received(1).Update(Arg.Any<Settlement>());
    }

    [Fact]
    public async Task TP_OPS_14_05_PublishesCommissionsSettledEvent()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var applicantId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var planId = Guid.NewGuid();
        var app = CreateApprovedApp(tenantId, applicantId, productId, planId, "OPS-001");

        var applicant = Applicant.Create(tenantId, "John", "Doe", DocumentType.DNI, "12345678", null, null, null);
        typeof(Applicant).GetProperty("Id")!.SetValue(applicant, applicantId);

        var dateFrom = DateTime.UtcNow.AddDays(-30);
        var dateTo = DateTime.UtcNow;
        var command = new ConfirmSettlementCommand(tenantId, null, dateFrom, dateTo, Guid.NewGuid(), "Admin");

        SetupApprovedApps(tenantId, null, dateFrom, dateTo, [app]);
        SetupCommissions(new CommissionPlanResult(productId, planId, "Fixed", 100m, "ARS", null));

        _applicantRepository.GetByIdAsync(applicantId, Arg.Any<CancellationToken>()).Returns(applicant);
        _fileStorageService.GenerateSettlementExcelAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<object>(), Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns((string?)null);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _eventPublisher.Received(1).PublishAsync(
            Arg.Is<CommissionsSettledEvent>(e =>
                e.TenantId == tenantId &&
                e.ItemsCount == 1),
            Arg.Any<CancellationToken>());
    }
}
