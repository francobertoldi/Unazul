using FluentAssertions;
using NSubstitute;
using SA.Identity.Application.Commands.Users;
using SA.Identity.Application.Dtos.Users;
using SA.Identity.Application.Interfaces;
using SA.Identity.DataAccess.Interface.Repositories;
using SA.Identity.Domain.Entities;
using Shared.Contract.Exceptions;
using Xunit;

namespace SA.Identity.Tests.Unit.Commands.Users;

public sealed class CreateUserCommandHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IPasswordService _passwordService = Substitute.For<IPasswordService>();
    private readonly IIntegrationEventPublisher _eventPublisher = Substitute.For<IIntegrationEventPublisher>();
    private readonly CreateUserCommandHandler _sut;

    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid CreatedBy = Guid.NewGuid();
    private static readonly Guid RoleId = Guid.NewGuid();

    public CreateUserCommandHandlerTests()
    {
        _sut = new CreateUserCommandHandler(
            _userRepository,
            _passwordService,
            _eventPublisher);
    }

    private CreateUserCommand BuildCommand(
        string username = "john.doe",
        string password = "P@ssw0rd!",
        string email = "john@example.com",
        Guid[]? roleIds = null,
        CreateUserAssignmentInput[]? assignments = null)
    {
        return new CreateUserCommand(
            TenantId,
            username,
            password,
            email,
            "John",
            "Doe",
            null,
            null,
            roleIds ?? [RoleId],
            assignments ?? [],
            CreatedBy);
    }

    private User CreateFakeUser(Guid? id = null)
    {
        var user = User.Create(TenantId, "john.doe", "hashed", "john@example.com", "John", "Doe", null, null, CreatedBy);
        return user;
    }

    private User CreateFakeUserWithRoles()
    {
        var user = CreateFakeUser();
        // For the return from GetByIdWithRolesAsync we need UserRoles and Assignments populated
        // Since they are private set, we use reflection or just return the user as-is
        // The handler maps from user.UserRoles which will be empty by default
        return user;
    }

    /// <summary>
    /// TP-SEC-07-01: POST /users crea usuario con roles y asignaciones
    /// </summary>
    [Fact]
    public async Task Handle_ValidCommand_CreatesUserWithRolesAndAssignments()
    {
        // Arrange
        var command = BuildCommand(
            assignments: [new CreateUserAssignmentInput("branch", Guid.NewGuid(), "Sucursal 1")]);

        _passwordService.Hash(command.Password).Returns("bcrypt_hash");
        _userRepository.GetByUsernameAsync(TenantId, command.Username, Arg.Any<CancellationToken>())
            .Returns((User?)null);
        _userRepository.GetByEmailAsync(TenantId, command.Email, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var createdUser = CreateFakeUserWithRoles();
        _userRepository.GetByIdWithRolesAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(createdUser);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Username.Should().Be("john.doe");

        await _userRepository.Received(1).AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
        await _userRepository.Received(1).SetUserRolesAsync(Arg.Any<Guid>(), command.RoleIds, Arg.Any<CancellationToken>());
        await _userRepository.Received(1).SetUserAssignmentsAsync(
            Arg.Any<Guid>(), Arg.Any<IEnumerable<UserAssignment>>(), Arg.Any<CancellationToken>());
        await _userRepository.Received().SaveChangesAsync(Arg.Any<CancellationToken>());
        await _eventPublisher.Received(1).PublishAsync(Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// TP-SEC-07-03: Password se hashea con bcrypt
    /// </summary>
    [Fact]
    public async Task Handle_ValidCommand_HashesPasswordWithService()
    {
        // Arrange
        var command = BuildCommand();
        _passwordService.Hash(command.Password).Returns("bcrypt_hashed_value");
        _userRepository.GetByUsernameAsync(TenantId, command.Username, Arg.Any<CancellationToken>())
            .Returns((User?)null);
        _userRepository.GetByEmailAsync(TenantId, command.Email, Arg.Any<CancellationToken>())
            .Returns((User?)null);
        _userRepository.GetByIdWithRolesAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(CreateFakeUserWithRoles());

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        _passwordService.Received(1).Hash(command.Password);
        await _userRepository.Received(1).AddAsync(
            Arg.Is<User>(u => u.PasswordHash == "bcrypt_hashed_value"),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// TP-SEC-07-04: Username duplicado por tenant retorna 409
    /// </summary>
    [Fact]
    public async Task Handle_DuplicateUsername_ThrowsConflict()
    {
        // Arrange
        var command = BuildCommand();
        _userRepository.GetByUsernameAsync(TenantId, command.Username, Arg.Any<CancellationToken>())
            .Returns(CreateFakeUser());

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None).AsTask();

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*nombre de usuario ya está en uso*");
    }

    /// <summary>
    /// TP-SEC-07-05: Email duplicado por tenant retorna 409
    /// </summary>
    [Fact]
    public async Task Handle_DuplicateEmail_ThrowsConflict()
    {
        // Arrange
        var command = BuildCommand();
        _userRepository.GetByUsernameAsync(TenantId, command.Username, Arg.Any<CancellationToken>())
            .Returns((User?)null);
        _userRepository.GetByEmailAsync(TenantId, command.Email, Arg.Any<CancellationToken>())
            .Returns(CreateFakeUser());

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None).AsTask();

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*email ya está en uso*");
    }

    /// <summary>
    /// TP-SEC-07-06: Username no cumple mask retorna 422 (too short)
    /// </summary>
    [Fact]
    public async Task Handle_UsernameTooShort_ThrowsValidation()
    {
        // Arrange
        var command = BuildCommand(username: "ab");

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None).AsTask();

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*nombre de usuario*alfanuméricos*");
    }

    /// <summary>
    /// TP-SEC-07-06b: Username con caracteres invalidos retorna 422
    /// </summary>
    [Fact]
    public async Task Handle_UsernameWithInvalidChars_ThrowsValidation()
    {
        // Arrange
        var command = BuildCommand(username: "john doe!");

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None).AsTask();

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*nombre de usuario*alfanuméricos*");
    }

    /// <summary>
    /// TP-SEC-07-06c: Username demasiado largo retorna 422
    /// </summary>
    [Fact]
    public async Task Handle_UsernameTooLong_ThrowsValidation()
    {
        // Arrange
        var command = BuildCommand(username: new string('a', 31));

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None).AsTask();

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*nombre de usuario*alfanuméricos*");
    }

    /// <summary>
    /// TP-SEC-07-01b: Crear usuario sin asignaciones funciona correctamente
    /// </summary>
    [Fact]
    public async Task Handle_NoAssignments_CreatesUserWithoutAssignments()
    {
        // Arrange
        var command = BuildCommand(assignments: []);
        _passwordService.Hash(command.Password).Returns("hashed");
        _userRepository.GetByUsernameAsync(TenantId, command.Username, Arg.Any<CancellationToken>())
            .Returns((User?)null);
        _userRepository.GetByEmailAsync(TenantId, command.Email, Arg.Any<CancellationToken>())
            .Returns((User?)null);
        _userRepository.GetByIdWithRolesAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(CreateFakeUserWithRoles());

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        await _userRepository.DidNotReceive().SetUserAssignmentsAsync(
            Arg.Any<Guid>(), Arg.Any<IEnumerable<UserAssignment>>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// TP-SEC-07-01c: Crear usuario sin roles no invoca SetUserRolesAsync
    /// </summary>
    [Fact]
    public async Task Handle_EmptyRoleIds_DoesNotSetRoles()
    {
        // Arrange
        var command = BuildCommand(roleIds: []);
        _passwordService.Hash(command.Password).Returns("hashed");
        _userRepository.GetByUsernameAsync(TenantId, command.Username, Arg.Any<CancellationToken>())
            .Returns((User?)null);
        _userRepository.GetByEmailAsync(TenantId, command.Email, Arg.Any<CancellationToken>())
            .Returns((User?)null);
        _userRepository.GetByIdWithRolesAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(CreateFakeUserWithRoles());

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _userRepository.DidNotReceive().SetUserRolesAsync(
            Arg.Any<Guid>(), Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// TP-SEC-07-01d: Evento DomainEvent publicado al crear usuario
    /// </summary>
    [Fact]
    public async Task Handle_ValidCommand_PublishesDomainEvent()
    {
        // Arrange
        var command = BuildCommand();
        _passwordService.Hash(command.Password).Returns("hashed");
        _userRepository.GetByUsernameAsync(TenantId, command.Username, Arg.Any<CancellationToken>())
            .Returns((User?)null);
        _userRepository.GetByEmailAsync(TenantId, command.Email, Arg.Any<CancellationToken>())
            .Returns((User?)null);
        _userRepository.GetByIdWithRolesAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(CreateFakeUserWithRoles());

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _eventPublisher.Received(1).PublishAsync(
            Arg.Any<Shared.Contract.Events.DomainEvent>(),
            Arg.Any<CancellationToken>());
    }
}
