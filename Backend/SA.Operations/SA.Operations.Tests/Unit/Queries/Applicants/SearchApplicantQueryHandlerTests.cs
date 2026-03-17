using FluentAssertions;
using NSubstitute;
using SA.Operations.Application.Queries.Applicants;
using SA.Operations.DataAccess.Interface.Repositories;
using SA.Operations.Domain.Entities;
using SA.Operations.Domain.Enums;
using Xunit;

namespace SA.Operations.Tests.Unit.Queries.Applicants;

public sealed class SearchApplicantQueryHandlerTests
{
    private static readonly Guid TenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    private readonly IApplicantRepository _applicantRepo = Substitute.For<IApplicantRepository>();
    private readonly IContactRepository _contactRepo = Substitute.For<IContactRepository>();
    private readonly IAddressRepository _addressRepo = Substitute.For<IAddressRepository>();

    private readonly SearchApplicantQueryHandler _sut;

    public SearchApplicantQueryHandlerTests()
    {
        _sut = new SearchApplicantQueryHandler(_applicantRepo, _contactRepo, _addressRepo);
    }

    [Fact(DisplayName = "TP_OPS_06_01: Returns applicant when found")]
    public async Task TP_OPS_06_01_Returns_Applicant_When_Found()
    {
        // Arrange
        var applicant = Applicant.Create(
            TenantId, "Maria", "Garcia", DocumentType.DNI, "33445566",
            new DateOnly(1985, 6, 15), Gender.Female, "Teacher");

        _applicantRepo.GetByDocumentAsync(TenantId, DocumentType.DNI, "33445566", Arg.Any<CancellationToken>())
            .Returns(applicant);
        _applicantRepo.CountApplicationsAsync(applicant.Id, Arg.Any<CancellationToken>()).Returns(3);

        var query = new SearchApplicantQuery(TenantId, "DNI", "33445566");

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.FirstName.Should().Be("Maria");
        result.LastName.Should().Be("Garcia");
        result.DocumentNumber.Should().Be("33445566");
        result.ApplicationCount.Should().Be(3);
    }

    [Fact(DisplayName = "TP_OPS_06_02: Returns null when not found")]
    public async Task TP_OPS_06_02_Returns_Null_When_Not_Found()
    {
        // Arrange
        _applicantRepo.GetByDocumentAsync(TenantId, DocumentType.DNI, "99999999", Arg.Any<CancellationToken>())
            .Returns((Applicant?)null);

        var query = new SearchApplicantQuery(TenantId, "DNI", "99999999");

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }
}
