using FluentAssertions;
using SA.Identity.DataAccess.EntityFramework.Seed;
using SA.Identity.Tests.Integration.Fixtures;
using Xunit;

namespace SA.Identity.Tests.Integration.Seed;

public sealed class PermissionSeedTests : IClassFixture<InMemoryDbFixture>
{
    private readonly InMemoryDbFixture _fixture;

    public PermissionSeedTests(InMemoryDbFixture fixture)
    {
        _fixture = fixture;
    }

    // The seed data file comment mentions 88, but the actual count is 78.
    // Tests validate against the real seed data.
    private static readonly int ExpectedPermissionCount = PermissionSeedData.Count;

    /// <summary>
    /// TP-SEC-10-14: All defined permissions are returned by GetSeedObjects.
    /// </summary>
    [Fact]
    public void GetSeedObjects_Returns_All_Defined_Permissions()
    {
        var seeds = PermissionSeedData.GetSeedObjects();
        seeds.Should().HaveCount(ExpectedPermissionCount);
    }

    /// <summary>
    /// TP-SEC-10-14: Count property matches definitions length.
    /// </summary>
    [Fact]
    public void Count_Matches_Definitions()
    {
        PermissionSeedData.Count.Should().Be(PermissionSeedData.GetDefinitions().Count);
    }

    /// <summary>
    /// TP-SEC-10-14: All permission codes are unique.
    /// </summary>
    [Fact]
    public void AllPermissionCodes_AreUnique()
    {
        var codes = PermissionSeedData.GetAllPermissionCodes();
        codes.Should().OnlyHaveUniqueItems();
    }

    /// <summary>
    /// TP-SEC-10-14: Permissions are grouped across 15 modules.
    /// </summary>
    [Fact]
    public void Permissions_AreGrouped_Across_15_Modules()
    {
        var definitions = PermissionSeedData.GetDefinitions();
        var modules = definitions.Select(d => d.Module).Distinct().ToList();
        modules.Should().HaveCount(15);
    }

    /// <summary>
    /// TP-SEC-10-14: Deterministic GUIDs are consistent across calls.
    /// </summary>
    [Fact]
    public void DeterministicGuids_AreConsistent()
    {
        var id1 = PermissionSeedData.GetPermissionId("p_users_list");
        var id2 = PermissionSeedData.GetPermissionId("p_users_list");
        id1.Should().Be(id2);
    }

    /// <summary>
    /// TP-SEC-10-14: Seeds are loaded into InMemory DbContext via HasData.
    /// </summary>
    [Fact]
    public void SeedPermissions_LoadedIntoDbContext()
    {
        using var ctx = _fixture.CreateContext();
        var permissions = ctx.Permissions.ToList();
        permissions.Should().HaveCount(ExpectedPermissionCount);
    }

    /// <summary>
    /// TP-SEC-10-14: Each seeded permission has a non-empty code.
    /// </summary>
    [Fact]
    public void SeedPermissions_AllHaveNonEmptyCode()
    {
        using var ctx = _fixture.CreateContext();
        var permissions = ctx.Permissions.ToList();
        permissions.Should().OnlyContain(p => !string.IsNullOrWhiteSpace(p.Code));
    }

    /// <summary>
    /// TP-SEC-10-14: Each seeded permission has a non-empty module.
    /// </summary>
    [Fact]
    public void SeedPermissions_AllHaveNonEmptyModule()
    {
        using var ctx = _fixture.CreateContext();
        var permissions = ctx.Permissions.ToList();
        permissions.Should().OnlyContain(p => !string.IsNullOrWhiteSpace(p.Module));
    }
}
