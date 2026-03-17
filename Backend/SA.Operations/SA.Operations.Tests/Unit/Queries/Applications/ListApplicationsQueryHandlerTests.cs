using FluentAssertions;
using NSubstitute;
using SA.Operations.Application.Queries.Applications;
using SA.Operations.DataAccess.Interface.Repositories;
using SA.Operations.Domain.Entities;
using SA.Operations.Domain.Enums;
using Xunit;
namespace SA.Operations.Tests.Unit.Queries.Applications;

public sealed class ListApplicationsQueryHandlerTests
{
    private static readonly Guid TenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    private readonly IApplicationRepository _applicationRepo = Substitute.For<IApplicationRepository>();
    private readonly IApplicantRepository _applicantRepo = Substitute.For<IApplicantRepository>();

    private readonly ListApplicationsQueryHandler _sut;

    public ListApplicationsQueryHandlerTests()
    {
        _sut = new ListApplicationsQueryHandler(_applicationRepo, _applicantRepo);
    }

    [Fact(DisplayName = "TP_OPS_03_01: Returns paged list")]
    public async Task TP_OPS_03_01_Returns_Paged_List()
    {
        // Arrange
        var app = SA.Operations.Domain.Entities.Application.Create(
            TenantId, Guid.NewGuid(), Guid.NewGuid(), "OPS-001",
            Guid.NewGuid(), Guid.NewGuid(), "Product A", "Plan X", Guid.NewGuid());

        var applicant = Applicant.Create(
            TenantId, "John", "Doe", DocumentType.DNI, "12345678", null, null, null);

        _applicationRepo.ListAsync(
            Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<ApplicationStatus?>(),
            Arg.Any<Guid?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns((new[] { app } as IReadOnlyList<SA.Operations.Domain.Entities.Application>, 1));

        _applicantRepo.GetByIdAsync(app.ApplicantId, Arg.Any<CancellationToken>()).Returns(applicant);

        var query = new ListApplicationsQuery(TenantId, null, null, null, 1, 10, null, null, false);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Total.Should().Be(1);
        result.Items[0].Code.Should().Be("OPS-001");
        result.Items[0].ApplicantName.Should().Be("John Doe");
    }

    [Fact(DisplayName = "TP_OPS_03_02: Returns empty list")]
    public async Task TP_OPS_03_02_Returns_Empty_List()
    {
        // Arrange
        _applicationRepo.ListAsync(
            Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<ApplicationStatus?>(),
            Arg.Any<Guid?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns((Array.Empty<SA.Operations.Domain.Entities.Application>() as IReadOnlyList<SA.Operations.Domain.Entities.Application>, 0));

        var query = new ListApplicationsQuery(TenantId, null, null, null, 1, 10, null, null, false);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().BeEmpty();
        result.Total.Should().Be(0);
    }
}
