using FluentAssertions;
using NSubstitute;
using SA.Config.Application.Queries.NotificationTemplates;
using SA.Config.DataAccess.Interface.Repositories;
using SA.Config.Domain.Entities;
using Xunit;

namespace SA.Config.Tests.Unit.Queries.NotificationTemplates;

public sealed class ListNotificationTemplatesQueryHandlerTests
{
    private readonly INotificationTemplateRepository _repo = Substitute.For<INotificationTemplateRepository>();
    private readonly ListNotificationTemplatesQueryHandler _sut;

    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();

    public ListNotificationTemplatesQueryHandlerTests()
    {
        _sut = new ListNotificationTemplatesQueryHandler(_repo);
    }

    [Fact]
    public async Task TP_CFG_19_01_Returns_Paginated_List()
    {
        // Arrange
        var templates = new List<NotificationTemplate>
        {
            NotificationTemplate.Create(TenantId, "tpl_1", "Template 1", "email", "Subject 1", "Body 1", "active", UserId),
            NotificationTemplate.Create(TenantId, "tpl_2", "Template 2", "sms", null, "Body 2", "active", UserId),
            NotificationTemplate.Create(TenantId, "tpl_3", "Template 3", "whatsapp", null, "Body 3", "inactive", UserId),
        };

        _repo.ListAsync(0, 10, null, null, Arg.Any<CancellationToken>())
            .Returns((templates.AsReadOnly(), 3));

        var query = new ListNotificationTemplatesQuery(1, 10, null, null);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(3);
        result.Total.Should().Be(3);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.Items[0].Code.Should().Be("tpl_1");
        result.Items[1].Channel.Should().Be("sms");
    }

    [Fact]
    public async Task TP_CFG_19_02_Filters_By_Channel()
    {
        // Arrange
        var emailTemplates = new List<NotificationTemplate>
        {
            NotificationTemplate.Create(TenantId, "email_1", "Email 1", "email", "Subject", "Body", "active", UserId),
        };

        _repo.ListAsync(0, 10, "email", null, Arg.Any<CancellationToken>())
            .Returns((emailTemplates.AsReadOnly(), 1));

        var query = new ListNotificationTemplatesQuery(1, 10, "email", null);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Total.Should().Be(1);
        result.Items[0].Channel.Should().Be("email");
    }
}
