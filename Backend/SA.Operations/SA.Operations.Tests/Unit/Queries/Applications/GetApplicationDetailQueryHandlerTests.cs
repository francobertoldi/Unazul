using FluentAssertions;
using NSubstitute;
using SA.Operations.Application.Dtos.Applications;
using SA.Operations.Application.Queries.Applications;
using SA.Operations.DataAccess.Interface.Repositories;
using SA.Operations.Domain.Entities;
using SA.Operations.Domain.Enums;
using Xunit;
namespace SA.Operations.Tests.Unit.Queries.Applications;

public sealed class GetApplicationDetailQueryHandlerTests
{
    private static readonly Guid TenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly Guid AppId = Guid.Parse("00000000-0000-0000-0000-000000000010");
    private static readonly Guid ApplicantId = Guid.Parse("00000000-0000-0000-0000-000000000020");

    private readonly IApplicationRepository _applicationRepo = Substitute.For<IApplicationRepository>();
    private readonly IApplicantRepository _applicantRepo = Substitute.For<IApplicantRepository>();
    private readonly IContactRepository _contactRepo = Substitute.For<IContactRepository>();
    private readonly IAddressRepository _addressRepo = Substitute.For<IAddressRepository>();
    private readonly IBeneficiaryRepository _beneficiaryRepo = Substitute.For<IBeneficiaryRepository>();
    private readonly IDocumentRepository _documentRepo = Substitute.For<IDocumentRepository>();
    private readonly IObservationRepository _observationRepo = Substitute.For<IObservationRepository>();
    private readonly ITraceEventRepository _traceEventRepo = Substitute.For<ITraceEventRepository>();

    private readonly GetApplicationDetailQueryHandler _sut;

    public GetApplicationDetailQueryHandlerTests()
    {
        _sut = new GetApplicationDetailQueryHandler(
            _applicationRepo,
            _applicantRepo,
            _contactRepo,
            _addressRepo,
            _beneficiaryRepo,
            _documentRepo,
            _observationRepo,
            _traceEventRepo);
    }

    // -- helpers --

    private static SA.Operations.Domain.Entities.Application CreateApplication(Guid? tenantId = null, Guid? applicantId = null)
    {
        return SA.Operations.Domain.Entities.Application.Create(
            tenantId ?? TenantId,
            Guid.NewGuid(),
            applicantId ?? ApplicantId,
            "OPS-001",
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Product A",
            "Plan X",
            Guid.NewGuid());
    }

    private static Applicant CreateApplicant()
    {
        return Applicant.Create(
            TenantId,
            "John",
            "Doe",
            DocumentType.DNI,
            "12345678",
            new DateOnly(1990, 1, 1),
            Gender.Male,
            "Engineer");
    }

    private void SetupHappyPath(SA.Operations.Domain.Entities.Application app, Applicant applicant)
    {
        _applicationRepo.GetByIdAsync(app.Id, Arg.Any<CancellationToken>()).Returns(app);
        _applicantRepo.GetByIdAsync(app.ApplicantId, Arg.Any<CancellationToken>()).Returns(applicant);
        _applicantRepo.CountApplicationsAsync(applicant.Id, Arg.Any<CancellationToken>()).Returns(1);
        _contactRepo.GetByApplicantIdAsync(applicant.Id, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<ApplicantContact>());
        _addressRepo.GetByApplicantIdAsync(applicant.Id, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<ApplicantAddress>());
        _beneficiaryRepo.GetByApplicationIdAsync(app.Id, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<Beneficiary>());
        _documentRepo.GetByApplicationIdAsync(app.Id, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<ApplicationDocument>());
        _observationRepo.GetByApplicationIdAsync(app.Id, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<ApplicationObservation>());
        _traceEventRepo.GetByApplicationIdAsync(app.Id, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<TraceEvent>());
    }

    [Fact(DisplayName = "TP_OPS_04_01: Returns detail for valid application")]
    public async Task TP_OPS_04_01_Returns_Detail_For_Valid_Application()
    {
        // Arrange
        var app = CreateApplication();
        var applicant = CreateApplicant();
        SetupHappyPath(app, applicant);

        var query = new GetApplicationDetailQuery(app.Id, TenantId);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(app.Id);
        result.Code.Should().Be("OPS-001");
        result.Applicant.FirstName.Should().Be("John");
        result.Applicant.LastName.Should().Be("Doe");
    }

    [Fact(DisplayName = "TP_OPS_04_02: Throws OPS_APPLICATION_NOT_FOUND")]
    public async Task TP_OPS_04_02_Throws_Application_Not_Found()
    {
        // Arrange
        _applicationRepo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((SA.Operations.Domain.Entities.Application?)null);

        var query = new GetApplicationDetailQuery(Guid.NewGuid(), TenantId);

        // Act
        Func<Task> act = async () => await _sut.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("OPS_APPLICATION_NOT_FOUND");
    }

    [Fact(DisplayName = "TP_OPS_04_03: Throws OPS_APPLICANT_NOT_FOUND")]
    public async Task TP_OPS_04_03_Throws_Applicant_Not_Found()
    {
        // Arrange
        var app = CreateApplication();
        _applicationRepo.GetByIdAsync(app.Id, Arg.Any<CancellationToken>()).Returns(app);
        _applicantRepo.GetByIdAsync(app.ApplicantId, Arg.Any<CancellationToken>())
            .Returns((Applicant?)null);

        var query = new GetApplicationDetailQuery(app.Id, TenantId);

        // Act
        Func<Task> act = async () => await _sut.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("OPS_APPLICANT_NOT_FOUND");
    }

    [Fact(DisplayName = "TP_OPS_04_04: Includes contacts in result")]
    public async Task TP_OPS_04_04_Includes_Contacts_In_Result()
    {
        // Arrange
        var app = CreateApplication();
        var applicant = CreateApplicant();
        SetupHappyPath(app, applicant);

        var contact = ApplicantContact.Create(
            applicant.Id, TenantId, ContactType.Personal, "john@test.com", "+54", "1155551234");

        _contactRepo.GetByApplicantIdAsync(applicant.Id, Arg.Any<CancellationToken>())
            .Returns(new[] { contact });

        var query = new GetApplicationDetailQuery(app.Id, TenantId);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Contacts.Should().HaveCount(1);
        result.Contacts[0].Email.Should().Be("john@test.com");
    }

    [Fact(DisplayName = "TP_OPS_04_05: Includes beneficiaries in result")]
    public async Task TP_OPS_04_05_Includes_Beneficiaries_In_Result()
    {
        // Arrange
        var app = CreateApplication();
        var applicant = CreateApplicant();
        SetupHappyPath(app, applicant);

        var beneficiary = Beneficiary.Create(app.Id, TenantId, "Jane", "Doe", "Spouse", 100m);

        _beneficiaryRepo.GetByApplicationIdAsync(app.Id, Arg.Any<CancellationToken>())
            .Returns(new[] { beneficiary });

        var query = new GetApplicationDetailQuery(app.Id, TenantId);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Beneficiaries.Should().HaveCount(1);
        result.Beneficiaries[0].FirstName.Should().Be("Jane");
        result.Beneficiaries[0].Percentage.Should().Be(100m);
    }
}
