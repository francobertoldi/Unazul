using FluentAssertions;
using NSubstitute;
using SA.Organization.Application.Commands.Tenants;
using SA.Organization.DataAccess.Interface.Repositories;
using Xunit;

namespace SA.Organization.Tests.Unit.Commands.Tenants;

public sealed class CreateTenantValidationTests
{
    private readonly ITenantRepository _tenantRepository = Substitute.For<ITenantRepository>();
    private readonly CreateTenantCommandHandler _sut;

    public CreateTenantValidationTests()
    {
        _sut = new CreateTenantCommandHandler(_tenantRepository);
    }

    [Fact]
    public async Task TP_ORG_02_02_Create_With_Inactive_Status_Succeeds()
    {
        _tenantRepository.ExistsByIdentifierAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new CreateTenantCommand("Test Org", "20-12345678-1", "Inactive",
            null, null, null, null, null, null, null, null);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Status.Should().Be("Inactive");
    }

    [Fact]
    public async Task TP_ORG_02_02b_Create_With_Suspended_Status_Succeeds()
    {
        _tenantRepository.ExistsByIdentifierAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new CreateTenantCommand("Test Org", "20-12345678-1", "Suspended",
            null, null, null, null, null, null, null, null);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Status.Should().Be("Suspended");
    }

    [Theory]
    [InlineData("")]
    [InlineData("InvalidStatus")]
    [InlineData("ACTIVE_TYPO")]
    [InlineData("deleted")]
    public async Task TP_ORG_02_06b_Create_With_Various_Invalid_Statuses_Throws(string invalidStatus)
    {
        var command = new CreateTenantCommand("Test Org", "20-12345678-1", invalidStatus,
            null, null, null, null, null, null, null, null);

        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("ORG_INVALID_STATUS");
    }

    [Fact]
    public async Task TP_ORG_02_01b_Create_With_All_Optional_Fields_Returns_TenantDto()
    {
        _tenantRepository.ExistsByIdentifierAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new CreateTenantCommand("Full Org", "20-99887766-5", "Active",
            "123 Main St", "Buenos Aires", "CABA", "C1000", "Argentina",
            "+5411-1234-5678", "contact@org.com", "https://logo.url/img.png");

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Name.Should().Be("Full Org");
        result.Identifier.Should().Be("20-99887766-5");
        result.Address.Should().Be("123 Main St");
        result.City.Should().Be("Buenos Aires");
        result.Province.Should().Be("CABA");
        result.Phone.Should().Be("+5411-1234-5678");
        result.Email.Should().Be("contact@org.com");
    }

    [Fact]
    public async Task TP_ORG_03_03_Create_Tenant_With_Active_Status_Succeeds()
    {
        _tenantRepository.ExistsByIdentifierAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new CreateTenantCommand("Org to Deactivate", "20-11223344-5", "Active",
            null, null, null, null, null, null, null, null);

        var result = await _sut.Handle(command, CancellationToken.None);
        result.Status.Should().Be("Active");
    }
}
