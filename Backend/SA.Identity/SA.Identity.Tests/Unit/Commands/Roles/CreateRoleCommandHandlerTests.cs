using FluentAssertions;
using NSubstitute;
using SA.Identity.Application.Commands.Roles;
using SA.Identity.Application.Interfaces;
using SA.Identity.DataAccess.Interface.Repositories;
using SA.Identity.Domain.Entities;
using Xunit;

namespace SA.Identity.Tests.Unit.Commands.Roles;

public sealed class CreateRoleCommandHandlerTests
{
    private readonly IRoleRepository _roleRepository = Substitute.For<IRoleRepository>();
    private readonly IPermissionRepository _permissionRepository = Substitute.For<IPermissionRepository>();
    private readonly IIntegrationEventPublisher _eventPublisher = Substitute.For<IIntegrationEventPublisher>();
    private readonly CreateRoleCommandHandler _sut;

    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid CreatedBy = Guid.NewGuid();

    public CreateRoleCommandHandlerTests()
    {
        _sut = new CreateRoleCommandHandler(_roleRepository, _permissionRepository, _eventPublisher);
    }

    private static void SetPrivateProperty<T>(T obj, string propertyName, object value) where T : class
    {
        var prop = typeof(T).GetProperty(propertyName);
        prop!.SetValue(obj, value);
    }

    private CreateRoleCommand BuildCommand(
        string name = "Operator",
        string? description = "Can operate",
        Guid[]? permissionIds = null)
    {
        return new CreateRoleCommand(
            TenantId,
            name,
            description,
            permissionIds ?? [Guid.NewGuid(), Guid.NewGuid()],
            CreatedBy);
    }

    private static Role CreateRoleWithPermissions(string name = "Operator", int permissionCount = 2)
    {
        var role = Role.Create(TenantId, name, "desc", CreatedBy);
        for (int i = 0; i < permissionCount; i++)
        {
            var perm = Permission.Create("mod", $"act_{i}", $"code_{i}", null);
            var rp = RolePermission.Create(role.Id, perm.Id);
            SetPrivateProperty(rp, nameof(RolePermission.Permission), perm);
            ((ICollection<RolePermission>)role.RolePermissions).Add(rp);
        }
        return role;
    }

    /// <summary>
    /// TP-SEC-10-01: POST /roles crea rol con permisos y retorna 201
    /// </summary>
    [Fact]
    public async Task Handle_ValidCommand_CreatesRoleWithPermissions()
    {
        // Arrange
        var permId1 = Guid.NewGuid();
        var permId2 = Guid.NewGuid();
        var command = BuildCommand(permissionIds: [permId1, permId2]);
        var perms = new List<Permission>
        {
            Permission.Create("mod", "act1", "code1", null),
            Permission.Create("mod", "act2", "code2", null)
        };
        // Override Ids to match command
        SetPrivateProperty(perms[0], nameof(Permission.Id), permId1);
        SetPrivateProperty(perms[1], nameof(Permission.Id), permId2);

        _permissionRepository.GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(perms.AsReadOnly() as IReadOnlyList<Permission>);
        _roleRepository.GetByNameAsync(TenantId, command.Name, Arg.Any<CancellationToken>())
            .Returns((Role?)null);

        var fullRole = CreateRoleWithPermissions();
        _roleRepository.GetByIdWithPermissionsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(fullRole);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Operator");
        await _roleRepository.Received(1).AddAsync(Arg.Any<Role>(), Arg.Any<CancellationToken>());
        await _roleRepository.Received(1).SetRolePermissionsAsync(
            Arg.Any<Guid>(), Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>());
        await _roleRepository.Received(2).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// TP-SEC-10-02: Rol creado tiene is_system = false
    /// </summary>
    [Fact]
    public async Task Handle_ValidCommand_CreatedRoleIsNotSystem()
    {
        // Arrange
        var command = BuildCommand();
        var perms = new List<Permission>
        {
            Permission.Create("mod", "act1", "code1", null),
            Permission.Create("mod", "act2", "code2", null)
        };

        _permissionRepository.GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(perms.AsReadOnly() as IReadOnlyList<Permission>);
        _roleRepository.GetByNameAsync(TenantId, command.Name, Arg.Any<CancellationToken>())
            .Returns((Role?)null);

        var fullRole = CreateRoleWithPermissions();
        _roleRepository.GetByIdWithPermissionsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(fullRole);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSystem.Should().BeFalse();
        await _roleRepository.Received(1).AddAsync(
            Arg.Is<Role>(r => !r.IsSystem),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// TP-SEC-10-03: Permisos duplicados se deduplican
    /// </summary>
    [Fact]
    public async Task Handle_DuplicatePermissionIds_AreDeduplicated()
    {
        // Arrange
        var permId = Guid.NewGuid();
        var command = BuildCommand(permissionIds: [permId, permId, permId]);
        var perms = new List<Permission>
        {
            Permission.Create("mod", "act1", "code1", null)
        };
        SetPrivateProperty(perms[0], nameof(Permission.Id), permId);

        _permissionRepository.GetByIdsAsync(
            Arg.Is<IEnumerable<Guid>>(ids => ids.Count() == 1),
            Arg.Any<CancellationToken>())
            .Returns(perms.AsReadOnly() as IReadOnlyList<Permission>);
        _roleRepository.GetByNameAsync(TenantId, command.Name, Arg.Any<CancellationToken>())
            .Returns((Role?)null);

        var fullRole = CreateRoleWithPermissions(permissionCount: 1);
        _roleRepository.GetByIdWithPermissionsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(fullRole);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        await _permissionRepository.Received(1).GetByIdsAsync(
            Arg.Is<IEnumerable<Guid>>(ids => ids.Count() == 1),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// TP-SEC-10-04: Nombre duplicado retorna 409
    /// </summary>
    [Fact]
    public async Task Handle_DuplicateName_ThrowsConflict()
    {
        // Arrange
        var command = BuildCommand(name: "ExistingRole");
        var perms = new List<Permission>
        {
            Permission.Create("mod", "act1", "code1", null),
            Permission.Create("mod", "act2", "code2", null)
        };

        _permissionRepository.GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(perms.AsReadOnly() as IReadOnlyList<Permission>);
        _roleRepository.GetByNameAsync(TenantId, "ExistingRole", Arg.Any<CancellationToken>())
            .Returns(CreateRoleWithPermissions("ExistingRole"));

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None).AsTask();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*ya existe un rol con ese nombre*");
    }

    /// <summary>
    /// TP-SEC-10-05: permission_ids vacio retorna 422
    /// </summary>
    [Fact]
    public async Task Handle_EmptyPermissionIds_ThrowsValidation()
    {
        // Arrange
        var command = BuildCommand(permissionIds: []);

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None).AsTask();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*ROLE_PERMISSIONS_EMPTY*");
    }

    /// <summary>
    /// TP-SEC-10-06: Permiso UUID inexistente retorna 422
    /// </summary>
    [Fact]
    public async Task Handle_NonExistentPermissionId_ThrowsValidation()
    {
        // Arrange
        var command = BuildCommand(permissionIds: [Guid.NewGuid(), Guid.NewGuid()]);

        // Return only 1 permission when 2 were requested
        var perms = new List<Permission>
        {
            Permission.Create("mod", "act1", "code1", null)
        };
        _permissionRepository.GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(perms.AsReadOnly() as IReadOnlyList<Permission>);

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None).AsTask();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*ROLE_PERMISSION_NOT_FOUND*");
    }

    /// <summary>
    /// TP-SEC-10-01b: DomainEvent publicado al crear rol
    /// </summary>
    [Fact]
    public async Task Handle_ValidCommand_PublishesDomainEvent()
    {
        // Arrange
        var command = BuildCommand();
        var perms = new List<Permission>
        {
            Permission.Create("mod", "act1", "code1", null),
            Permission.Create("mod", "act2", "code2", null)
        };

        _permissionRepository.GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(perms.AsReadOnly() as IReadOnlyList<Permission>);
        _roleRepository.GetByNameAsync(TenantId, command.Name, Arg.Any<CancellationToken>())
            .Returns((Role?)null);
        _roleRepository.GetByIdWithPermissionsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(CreateRoleWithPermissions());

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _eventPublisher.Received(1).PublishAsync(
            Arg.Any<Shared.Contract.Events.DomainEvent>(),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// TP-SEC-10-01c: Result includes permissions list
    /// </summary>
    [Fact]
    public async Task Handle_ValidCommand_ResultIncludesPermissions()
    {
        // Arrange
        var command = BuildCommand();
        var perms = new List<Permission>
        {
            Permission.Create("mod", "act1", "code1", null),
            Permission.Create("mod", "act2", "code2", null)
        };

        _permissionRepository.GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(perms.AsReadOnly() as IReadOnlyList<Permission>);
        _roleRepository.GetByNameAsync(TenantId, command.Name, Arg.Any<CancellationToken>())
            .Returns((Role?)null);

        var fullRole = CreateRoleWithPermissions(permissionCount: 3);
        _roleRepository.GetByIdWithPermissionsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(fullRole);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Permissions.Should().HaveCount(3);
    }

    /// <summary>
    /// TP-SEC-10-04b: All permission IDs must exist, partial match fails
    /// </summary>
    [Fact]
    public async Task Handle_PartialPermissionMatch_ThrowsNotFound()
    {
        // Arrange
        var perm1 = Guid.NewGuid();
        var perm2 = Guid.NewGuid();
        var perm3 = Guid.NewGuid();
        var command = BuildCommand(permissionIds: [perm1, perm2, perm3]);

        // Only 2 of 3 exist
        var perms = new List<Permission>
        {
            Permission.Create("mod", "act1", "code1", null),
            Permission.Create("mod", "act2", "code2", null)
        };
        SetPrivateProperty(perms[0], nameof(Permission.Id), perm1);
        SetPrivateProperty(perms[1], nameof(Permission.Id), perm2);

        _permissionRepository.GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(perms.AsReadOnly() as IReadOnlyList<Permission>);

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None).AsTask();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*ROLE_PERMISSION_NOT_FOUND*");
    }
}
