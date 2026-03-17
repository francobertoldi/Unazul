using FluentAssertions;
using NSubstitute;
using SA.Operations.Application.Commands.Settlements;
using SA.Operations.Application.Dtos.Settlements;
using SA.Operations.Application.Interfaces;
using SA.Operations.DataAccess.Interface.Repositories;
using SA.Operations.Domain.Entities;
using SA.Operations.Domain.Enums;
using Xunit;
using AppEntity = SA.Operations.Domain.Entities.Application;

namespace SA.Operations.Tests.Unit.Commands.Settlements;

public sealed class PreviewSettlementCommandHandlerTests
{
    private readonly IApplicationRepository _applicationRepository = Substitute.For<IApplicationRepository>();
    private readonly IApplicantRepository _applicantRepository = Substitute.For<IApplicantRepository>();
    private readonly ICatalogServiceClient _catalogClient = Substitute.For<ICatalogServiceClient>();
    private readonly PreviewSettlementCommandHandler _sut;

    public PreviewSettlementCommandHandlerTests()
    {
        _sut = new PreviewSettlementCommandHandler(_applicationRepository, _applicantRepository, _catalogClient);
    }

    private static AppEntity CreateApprovedApp(Guid tenantId, Guid applicantId, Guid productId, Guid planId, string code)
    {
        var app = AppEntity.Create(tenantId, Guid.NewGuid(), applicantId, code, productId, planId, "Product", "Plan", Guid.NewGuid());
        typeof(AppEntity).GetProperty("Status")!.SetValue(app, ApplicationStatus.Approved);
        return app;
    }

    [Fact]
    public async Task TP_OPS_13_01_ReturnsEmpty_WhenNoApprovedApps()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var command = new PreviewSettlementCommand(tenantId, null, DateTime.UtcNow.AddDays(-30), DateTime.UtcNow);

        _applicationRepository.GetApprovedByDateRangeAsync(tenantId, null, command.DateFrom, command.DateTo, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<AppEntity>() as IReadOnlyList<AppEntity>);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalsByCurrency.Should().BeEmpty();
    }

    [Fact]
    public async Task TP_OPS_13_02_ReturnsPreviewWithItems()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var applicantId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var planId = Guid.NewGuid();
        var app = CreateApprovedApp(tenantId, applicantId, productId, planId, "OPS-001");
        var applicant = Applicant.Create(tenantId, "John", "Doe", DocumentType.DNI, "12345678", null, null, null);
        typeof(Applicant).GetProperty("Id")!.SetValue(applicant, applicantId);

        var command = new PreviewSettlementCommand(tenantId, null, DateTime.UtcNow.AddDays(-30), DateTime.UtcNow);

        _applicationRepository.GetApprovedByDateRangeAsync(tenantId, null, command.DateFrom, command.DateTo, Arg.Any<CancellationToken>())
            .Returns(new List<AppEntity> { app } as IReadOnlyList<AppEntity>);

        _applicantRepository.GetByIdAsync(applicantId, Arg.Any<CancellationToken>())
            .Returns(applicant);

        var commissions = new List<CommissionPlanResult>
        {
            new(productId, planId, "Fixed", 100m, "ARS", "Fixed 100 ARS")
        };
        _catalogClient.GetCommissionPlansAsync(Arg.Any<Guid[]>(), Arg.Any<Guid[]>(), Arg.Any<CancellationToken>())
            .Returns(commissions as IReadOnlyList<CommissionPlanResult>);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items[0].ApplicationId.Should().Be(app.Id);
        result.Items[0].AppCode.Should().Be("OPS-001");
        result.Items[0].CalculatedAmount.Should().Be(100m);
    }

    [Fact]
    public async Task TP_OPS_13_03_GroupsTotalsByCurrency()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var applicantId = Guid.NewGuid();
        var productId1 = Guid.NewGuid();
        var planId1 = Guid.NewGuid();
        var productId2 = Guid.NewGuid();
        var planId2 = Guid.NewGuid();

        var app1 = CreateApprovedApp(tenantId, applicantId, productId1, planId1, "OPS-001");
        var app2 = CreateApprovedApp(tenantId, applicantId, productId2, planId2, "OPS-002");

        var applicant = Applicant.Create(tenantId, "John", "Doe", DocumentType.DNI, "12345678", null, null, null);
        typeof(Applicant).GetProperty("Id")!.SetValue(applicant, applicantId);

        var command = new PreviewSettlementCommand(tenantId, null, DateTime.UtcNow.AddDays(-30), DateTime.UtcNow);

        _applicationRepository.GetApprovedByDateRangeAsync(tenantId, null, command.DateFrom, command.DateTo, Arg.Any<CancellationToken>())
            .Returns(new List<AppEntity> { app1, app2 } as IReadOnlyList<AppEntity>);

        _applicantRepository.GetByIdAsync(applicantId, Arg.Any<CancellationToken>())
            .Returns(applicant);

        var commissions = new List<CommissionPlanResult>
        {
            new(productId1, planId1, "Fixed", 100m, "ARS", "Fixed 100 ARS"),
            new(productId2, planId2, "Fixed", 50m, "USD", "Fixed 50 USD")
        };
        _catalogClient.GetCommissionPlansAsync(Arg.Any<Guid[]>(), Arg.Any<Guid[]>(), Arg.Any<CancellationToken>())
            .Returns(commissions as IReadOnlyList<CommissionPlanResult>);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.TotalsByCurrency.Should().HaveCount(2);
        result.TotalsByCurrency.Should().Contain(t => t.Currency == "ARS" && t.TotalAmount == 100m);
        result.TotalsByCurrency.Should().Contain(t => t.Currency == "USD" && t.TotalAmount == 50m);
    }

    [Fact]
    public async Task TP_OPS_13_04_UsesApplicantName()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var applicantId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var planId = Guid.NewGuid();
        var app = CreateApprovedApp(tenantId, applicantId, productId, planId, "OPS-001");

        var applicant = Applicant.Create(tenantId, "Maria", "Garcia", DocumentType.DNI, "87654321", null, null, null);
        typeof(Applicant).GetProperty("Id")!.SetValue(applicant, applicantId);

        var command = new PreviewSettlementCommand(tenantId, null, DateTime.UtcNow.AddDays(-30), DateTime.UtcNow);

        _applicationRepository.GetApprovedByDateRangeAsync(tenantId, null, command.DateFrom, command.DateTo, Arg.Any<CancellationToken>())
            .Returns(new List<AppEntity> { app } as IReadOnlyList<AppEntity>);

        _applicantRepository.GetByIdAsync(applicantId, Arg.Any<CancellationToken>())
            .Returns(applicant);

        var commissions = new List<CommissionPlanResult>
        {
            new(productId, planId, "Fixed", 100m, "ARS", null)
        };
        _catalogClient.GetCommissionPlansAsync(Arg.Any<Guid[]>(), Arg.Any<Guid[]>(), Arg.Any<CancellationToken>())
            .Returns(commissions as IReadOnlyList<CommissionPlanResult>);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Items[0].ApplicantName.Should().Be("Maria Garcia");
    }
}
