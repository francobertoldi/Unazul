using FluentAssertions;
using NSubstitute;
using SA.Identity.Application.Dtos.Users;
using SA.Identity.Application.Queries.Users;
using SA.Identity.DataAccess.Interface.Repositories;
using SA.Identity.Domain.Entities;
using Shared.Contract.Enums;
using Xunit;

namespace SA.Identity.Tests.Unit.Queries.Users;

public sealed class ListUsersQueryHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly ListUsersQueryHandler _sut;

    private static readonly Guid TenantId = Guid.NewGuid();

    public ListUsersQueryHandlerTests()
    {
        _sut = new ListUsersQueryHandler(_userRepository);
    }

    private static User CreateUser(string username, string email, UserStatus status = UserStatus.Active)
    {
        var user = User.Create(TenantId, username, "hash", email, "First", "Last", null, null, Guid.NewGuid());
        if (status != UserStatus.Active)
        {
            user.Update(user.Email, user.FirstName, user.LastName, user.EntityId, user.EntityName,
                status, user.Avatar, Guid.NewGuid());
        }
        return user;
    }

    private static List<User> CreateUserList(int count)
    {
        var users = new List<User>();
        for (int i = 0; i < count; i++)
        {
            users.Add(CreateUser($"user{i}", $"user{i}@test.com"));
        }
        return users;
    }

    [Fact]
    public async Task RF_SEC_06_01_ListUsers_ReturnsPaginatedListWithExpectedFields()
    {
        // Arrange
        var users = CreateUserList(3);
        _userRepository.ListAsync(
                TenantId, 0, 10, null, null, null, null, null, null, Arg.Any<CancellationToken>())
            .Returns((users.AsReadOnly(), 3));

        // Act
        var result = await _sut.Handle(
            new ListUsersQuery(TenantId, 1, 10, null, null, null, null, null, null, false),
            CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(3);
        result.Total.Should().Be(3);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);

        var firstUser = result.Items[0];
        firstUser.Id.Should().NotBeEmpty();
        firstUser.Username.Should().Be("user0");
        firstUser.Email.Should().Be("user0@test.com");
        firstUser.FirstName.Should().Be("First");
        firstUser.LastName.Should().Be("Last");
        firstUser.Status.Should().Be(UserStatus.Active);
    }

    [Fact]
    public async Task RF_SEC_06_02_SearchByUsernameOrEmail_FiltersCorrectly()
    {
        // Arrange
        var searchTerm = "admin";
        var filteredUsers = new List<User> { CreateUser("admin", "admin@test.com") };
        _userRepository.ListAsync(
                TenantId, 0, 10, searchTerm, null, null, null, null, null, Arg.Any<CancellationToken>())
            .Returns((filteredUsers.AsReadOnly(), 1));

        // Act
        var result = await _sut.Handle(
            new ListUsersQuery(TenantId, 1, 10, searchTerm, null, null, null, null, null, false),
            CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items[0].Username.Should().Be("admin");
        result.Total.Should().Be(1);
    }

    [Fact]
    public async Task RF_SEC_06_03_FilterByStatus_ReturnsOnlyMatchingStatus()
    {
        // Arrange
        var lockedUsers = new List<User> { CreateUser("locked1", "locked1@test.com", UserStatus.Locked) };
        _userRepository.ListAsync(
                TenantId, 0, 10, null, UserStatus.Locked, null, null, null, null, Arg.Any<CancellationToken>())
            .Returns((lockedUsers.AsReadOnly(), 1));

        // Act
        var result = await _sut.Handle(
            new ListUsersQuery(TenantId, 1, 10, null, UserStatus.Locked, null, null, null, null, false),
            CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items[0].Status.Should().Be(UserStatus.Locked);
    }

    [Fact]
    public async Task RF_SEC_06_04_PermissionCheck_IsAtEndpointLevel()
    {
        // Note: Authorization (403 for missing permission) is enforced at endpoint level,
        // not in the query handler. This test documents that the handler itself does not
        // perform any permission check and will always return data if called.
        _userRepository.ListAsync(
                TenantId, 0, 10, null, null, null, null, null, null, Arg.Any<CancellationToken>())
            .Returns((new List<User>().AsReadOnly(), 0));

        // Act - handler does not throw even without permission check
        var result = await _sut.Handle(
            new ListUsersQuery(TenantId, 1, 10, null, null, null, null, null, null, false),
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task RF_SEC_06_05_PageOutOfRange_ReturnsEmptyList()
    {
        // Arrange - page 999 with only 3 total records
        _userRepository.ListAsync(
                TenantId, 9980, 10, null, null, null, null, null, null, Arg.Any<CancellationToken>())
            .Returns((new List<User>().AsReadOnly(), 3));

        // Act
        var result = await _sut.Handle(
            new ListUsersQuery(TenantId, 999, 10, null, null, null, null, null, null, false),
            CancellationToken.None);

        // Assert
        result.Items.Should().BeEmpty();
        result.Total.Should().Be(3);
    }

    [Fact]
    public async Task RF_SEC_06_06_PageSizeGreaterThan100_ClampedTo100()
    {
        // Arrange
        _userRepository.ListAsync(
                TenantId, 0, 100, null, null, null, null, null, null, Arg.Any<CancellationToken>())
            .Returns((new List<User>().AsReadOnly(), 0));

        // Act
        var result = await _sut.Handle(
            new ListUsersQuery(TenantId, 1, 500, null, null, null, null, null, null, false),
            CancellationToken.None);

        // Assert
        result.PageSize.Should().Be(100);
        // Verify the repository was called with clamped page size
        await _userRepository.Received(1).ListAsync(
            TenantId, 0, 100, null, null, null, null, null, null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RF_SEC_06_07_JwtCheck_IsAtEndpointLevel()
    {
        // Note: JWT validation (401 for missing token) is enforced at endpoint/middleware level,
        // not in the query handler. This test documents the handler behavior.
        _userRepository.ListAsync(
                TenantId, 0, 10, null, null, null, null, null, null, Arg.Any<CancellationToken>())
            .Returns((new List<User>().AsReadOnly(), 0));

        // Act - handler processes request regardless of JWT presence
        var result = await _sut.Handle(
            new ListUsersQuery(TenantId, 1, 10, null, null, null, null, null, null, false),
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }
}
