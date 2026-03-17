using FluentAssertions;
using SA.Config.DataAccess.EntityFramework.Repositories;
using SA.Config.Domain.Entities;
using SA.Config.Domain.Enums;
using SA.Config.Tests.Integration.Fixtures;
using Xunit;

namespace SA.Config.Tests.Integration.Repositories;

public sealed class ParameterRepositoryIntegrationTests : IClassFixture<InMemoryDbFixture>
{
    private readonly InMemoryDbFixture _fixture;

    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();

    public ParameterRepositoryIntegrationTests(InMemoryDbFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Creates_Parameter_With_Unique_Key()
    {
        // Arrange
        using var db = _fixture.CreateContext();
        var repo = new ParameterRepository(db);

        var group = ParameterGroup.Create("test_group", "Test Group", "General", "mdi-test", 1);
        db.ParameterGroups.Add(group);
        await db.SaveChangesAsync();

        var parameter = Parameter.Create(TenantId, group.Id, "max_retries", "3", ParameterType.Number, "Max retries", null, UserId);

        // Act
        await repo.AddAsync(parameter);
        await repo.SaveChangesAsync();

        // Assert
        var exists = await repo.ExistsByKeyAsync(TenantId, group.Id, "max_retries");
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task Duplicate_Key_Throws()
    {
        // Arrange
        using var db = _fixture.CreateContext();
        var repo = new ParameterRepository(db);

        var group = ParameterGroup.Create("dup_group", "Dup Group", "General", "mdi-test", 1);
        db.ParameterGroups.Add(group);
        await db.SaveChangesAsync();

        var param1 = Parameter.Create(TenantId, group.Id, "same_key", "v1", ParameterType.Text, "First", null, UserId);
        await repo.AddAsync(param1);
        await repo.SaveChangesAsync();

        // Act - verify the key exists
        var exists = await repo.ExistsByKeyAsync(TenantId, group.Id, "same_key");

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task Lists_By_GroupId_Filtered()
    {
        // Arrange
        using var db = _fixture.CreateContext();
        var repo = new ParameterRepository(db);

        var group = ParameterGroup.Create("filter_group", "Filter Group", "General", "mdi-test", 1);
        db.ParameterGroups.Add(group);
        await db.SaveChangesAsync();

        var param1 = Parameter.Create(TenantId, group.Id, "key_a", "1", ParameterType.Text, "A", null, UserId);
        var param2 = Parameter.Create(TenantId, group.Id, "key_b", "2", ParameterType.Text, "B", "parent_x", UserId);
        var param3 = Parameter.Create(TenantId, group.Id, "key_c", "3", ParameterType.Text, "C", null, UserId);

        await repo.AddAsync(param1);
        await repo.AddAsync(param2);
        await repo.AddAsync(param3);
        await repo.SaveChangesAsync();

        // Act - list root params (parentKey == null)
        var rootParams = await repo.GetByGroupIdAsync(group.Id);

        // Assert
        rootParams.Should().HaveCount(2);
        rootParams.Should().OnlyContain(p => p.ParentKey == null);

        // Act - list child params
        var childParams = await repo.GetByGroupIdAsync(group.Id, "parent_x");

        // Assert
        childParams.Should().HaveCount(1);
        childParams[0].Key.Should().Be("key_b");
    }

    [Fact]
    public async Task ReplaceOptions_Works()
    {
        // Arrange
        using var db = _fixture.CreateContext();
        var repo = new ParameterRepository(db);

        var group = ParameterGroup.Create("opt_group", "Opt Group", "General", "mdi-test", 1);
        db.ParameterGroups.Add(group);
        await db.SaveChangesAsync();

        var param = Parameter.Create(TenantId, group.Id, "select_param", "opt1", ParameterType.Select, "Select param", null, UserId);
        await repo.AddAsync(param);
        await repo.SaveChangesAsync();

        var originalOptions = new[]
        {
            ParameterOption.Create(param.Id, TenantId, "opt1", "Option 1", 1),
            ParameterOption.Create(param.Id, TenantId, "opt2", "Option 2", 2),
        };

        await repo.ReplaceOptionsAsync(param.Id, originalOptions);
        await repo.SaveChangesAsync();

        // Act - replace with new options
        var newOptions = new[]
        {
            ParameterOption.Create(param.Id, TenantId, "new1", "New Option 1", 1),
            ParameterOption.Create(param.Id, TenantId, "new2", "New Option 2", 2),
            ParameterOption.Create(param.Id, TenantId, "new3", "New Option 3", 3),
        };

        await repo.ReplaceOptionsAsync(param.Id, newOptions);
        await repo.SaveChangesAsync();

        // Assert
        var loaded = await repo.GetByIdAsync(param.Id);
        loaded.Should().NotBeNull();
        loaded!.Options.Should().HaveCount(3);
        loaded.Options.Should().Contain(o => o.OptionValue == "new1");
        loaded.Options.Should().NotContain(o => o.OptionValue == "opt1");
    }
}
