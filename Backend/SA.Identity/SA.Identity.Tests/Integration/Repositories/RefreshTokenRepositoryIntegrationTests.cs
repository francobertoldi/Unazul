using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SA.Identity.DataAccess.EntityFramework.Repositories;
using SA.Identity.Domain.Entities;
using SA.Identity.Tests.Integration.Fixtures;
using Xunit;

namespace SA.Identity.Tests.Integration.Repositories;

public sealed class RefreshTokenRepositoryIntegrationTests : IClassFixture<InMemoryDbFixture>
{
    private readonly InMemoryDbFixture _fixture;

    public RefreshTokenRepositoryIntegrationTests(InMemoryDbFixture fixture)
    {
        _fixture = fixture;
    }

    private static User CreateUser(Guid tenantId)
    {
        return User.Create(tenantId, $"user_{Guid.NewGuid():N}"[..20], "h",
            $"{Guid.NewGuid():N}@test.com", "Test", "User", null, null, Guid.NewGuid());
    }

    /// <summary>
    /// TP-SEC-03-07: Token creation with hash persists correctly.
    /// </summary>
    [Fact]
    public async Task AddAsync_TokenPersistsWithHash()
    {
        // Arrange
        using var ctx = _fixture.CreateContext();
        var repo = new RefreshTokenRepository(ctx);
        var tenantId = Guid.NewGuid();
        var user = CreateUser(tenantId);
        ctx.Users.Add(user);
        await ctx.SaveChangesAsync();

        var token = RefreshToken.Create(user.Id, "hashed_token_value", DateTime.UtcNow.AddDays(7));

        // Act
        await repo.AddAsync(token);
        await repo.SaveChangesAsync();

        // Assert
        var found = await repo.GetByTokenHashAsync("hashed_token_value");
        found.Should().NotBeNull();
        found!.UserId.Should().Be(user.Id);
        found.TokenHash.Should().Be("hashed_token_value");
        found.Revoked.Should().BeFalse();
    }

    /// <summary>
    /// TP-SEC-03-08: Finding by hash returns the correct token.
    /// </summary>
    [Fact]
    public async Task GetByTokenHashAsync_ReturnsCorrectToken()
    {
        // Arrange
        using var ctx = _fixture.CreateContext();
        var repo = new RefreshTokenRepository(ctx);
        var tenantId = Guid.NewGuid();
        var user = CreateUser(tenantId);
        ctx.Users.Add(user);
        await ctx.SaveChangesAsync();

        var token1 = RefreshToken.Create(user.Id, "hash_1", DateTime.UtcNow.AddDays(7));
        var token2 = RefreshToken.Create(user.Id, "hash_2", DateTime.UtcNow.AddDays(7));
        await repo.AddAsync(token1);
        await repo.AddAsync(token2);
        await repo.SaveChangesAsync();

        // Act
        var found = await repo.GetByTokenHashAsync("hash_2");

        // Assert
        found.Should().NotBeNull();
        found!.Id.Should().Be(token2.Id);
    }

    /// <summary>
    /// TP-SEC-03-08: Finding by nonexistent hash returns null.
    /// </summary>
    [Fact]
    public async Task GetByTokenHashAsync_NonExistentHash_ReturnsNull()
    {
        // Arrange
        using var ctx = _fixture.CreateContext();
        var repo = new RefreshTokenRepository(ctx);

        // Act
        var found = await repo.GetByTokenHashAsync("nonexistent_hash");

        // Assert
        found.Should().BeNull();
    }

    /// <summary>
    /// TP-SEC-04-10: Token revocation sets Revoked flag.
    /// </summary>
    [Fact]
    public async Task RevokeToken_SetsRevokedFlag()
    {
        // Arrange
        using var ctx = _fixture.CreateContext();
        var repo = new RefreshTokenRepository(ctx);
        var tenantId = Guid.NewGuid();
        var user = CreateUser(tenantId);
        ctx.Users.Add(user);
        await ctx.SaveChangesAsync();

        var token = RefreshToken.Create(user.Id, "to_revoke_hash", DateTime.UtcNow.AddDays(7));
        await repo.AddAsync(token);
        await repo.SaveChangesAsync();

        // Act
        var found = await repo.GetByTokenHashAsync("to_revoke_hash");
        found!.Revoke();
        await repo.SaveChangesAsync();

        // Assert
        var afterRevoke = await repo.GetByTokenHashAsync("to_revoke_hash");
        afterRevoke.Should().NotBeNull();
        afterRevoke!.Revoked.Should().BeTrue();
        afterRevoke.IsValid.Should().BeFalse();
    }

    /// <summary>
    /// TP-SEC-04-10: RevokeAllByUserIdAsync revokes all active tokens for a user.
    /// </summary>
    [Fact]
    public async Task RevokeAllByUserIdAsync_RevokesAllActiveTokensForUser()
    {
        // Arrange
        using var ctx = _fixture.CreateContext();
        var repo = new RefreshTokenRepository(ctx);
        var tenantId = Guid.NewGuid();
        var user = CreateUser(tenantId);
        ctx.Users.Add(user);
        await ctx.SaveChangesAsync();

        var token1 = RefreshToken.Create(user.Id, "user_token_1", DateTime.UtcNow.AddDays(7));
        var token2 = RefreshToken.Create(user.Id, "user_token_2", DateTime.UtcNow.AddDays(7));
        var token3 = RefreshToken.Create(user.Id, "user_token_3", DateTime.UtcNow.AddDays(7));
        // Pre-revoke token3 to verify it is not re-processed
        token3.Revoke();

        await repo.AddAsync(token1);
        await repo.AddAsync(token2);
        await repo.AddAsync(token3);
        await repo.SaveChangesAsync();

        // Act
        await repo.RevokeAllByUserIdAsync(user.Id);
        await repo.SaveChangesAsync();

        // Assert
        var allTokens = await ctx.RefreshTokens
            .Where(t => t.UserId == user.Id)
            .ToListAsync();

        allTokens.Should().HaveCount(3);
        allTokens.Should().OnlyContain(t => t.Revoked);
    }

    /// <summary>
    /// TP-SEC-04-10: RevokeAllByUserIdAsync does not affect other users.
    /// </summary>
    [Fact]
    public async Task RevokeAllByUserIdAsync_DoesNotAffectOtherUsers()
    {
        // Arrange
        using var ctx = _fixture.CreateContext();
        var repo = new RefreshTokenRepository(ctx);
        var tenantId = Guid.NewGuid();
        var user1 = CreateUser(tenantId);
        var user2 = CreateUser(tenantId);
        ctx.Users.AddRange(user1, user2);
        await ctx.SaveChangesAsync();

        var token1 = RefreshToken.Create(user1.Id, "u1_token", DateTime.UtcNow.AddDays(7));
        var token2 = RefreshToken.Create(user2.Id, "u2_token", DateTime.UtcNow.AddDays(7));
        await repo.AddAsync(token1);
        await repo.AddAsync(token2);
        await repo.SaveChangesAsync();

        // Act
        await repo.RevokeAllByUserIdAsync(user1.Id);
        await repo.SaveChangesAsync();

        // Assert
        var u2Token = await repo.GetByTokenHashAsync("u2_token");
        u2Token.Should().NotBeNull();
        u2Token!.Revoked.Should().BeFalse();
    }
}
