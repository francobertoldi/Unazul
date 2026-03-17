using FluentAssertions;
using NSubstitute;
using SA.Identity.Application.Commands.Roles;
using SA.Identity.Application.Interfaces;
using SA.Identity.DataAccess.Interface.Repositories;
using SA.Identity.Domain.Entities;
using Shared.Contract.Events;
using Xunit;

namespace SA.Identity.Tests.Unit.Commands.Roles;

public sealed class UpdateRoleCommandHandlerTests
{
    private readonly IRoleRepository _roleRepository = Substitute.For<IRoleRepository>();
    private readonly IPermissionRepository _permissionRepository = Substitute.For<IPermissionRepository>();
    private readonly IIntegrationEventPublisher _eventPublisher = Substitute.For<IIntegrationEventPublisher>();
    private readonly UpdateRoleCommandHandler _sut;

    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid UpdatedBy = Guid.NewGuid();

    public UpdateRoleCommandHandlerTests()
    {
        _sut = new UpdateRoleCommandHandler(_roleRepository, _permissionRepository, _eventPublisher);
    }

    private static void SetPrivateProperty<T>(T obj, string propertyName, object value) where T : class
    {
        var prop = typeof(T).GetProperty(propertyName);
        prop!.SetValue(obj, value);
    }

    private UpdateRoleCommand BuildCommand(
        Guid? roleId = null,
        string name = "UpdatedRole",
        string? description = "Updated description",
        Guid[]? permissionIds = null)
    {
        return new UpdateRoleCommand(
            roleId ?? Guid.NewGuid(),
            TenantId,
            name,
            description,
            permissionIds ?? [Guid.NewGuid()],
            UpdatedBy);
    }

    private static Role CreateRole(
        string name = "TestRole",
        bool isSystem = false,
        Guid? id = null,
        int permissionCount = 2)
    {
        var role = Role.Create(TenantId, name, "desc", Guid.NewGuid());
        if (id.HasValue)
            SetPrivateProperty(role, nameof(Role.Id), id.Value);
        if (isSystem)
            SetPrivateProperty(role, nameof(Role.IsSystem), true);

        for (int i = 0; i < permissionCount; i++)
        {
            var perm = Permission.Create("mod", $"act_{i}", $"code_{i}", null);
            var rp = RolePermission.Create(role.Id, perm.Id);
            SetPrivateProperty(rp, nameof(RolePermission.Permission), perm);
            ((ICollection<RolePermission>)role.RolePermissions).Add(rp);
        }
        return role;
    }

    private static Permission CreatePermissionWithCode(string code)
    {
        var p = Permission.Create("identity", "action", code, null);
        return p;
    }

    private void SetupValidUpdateFlow(Role role, Guid[] newPermissionIds, Guid[]? currentPermissionIds = null)
    {
        _roleRepository.GetByIdWithPermissionsAsync(role.Id, Arg.Any<CancellationToken>())
            .Returns(role);
        _roleRepository.GetByNameAsync(TenantId, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((Role?)null);
        _roleRepository.GetCurrentPermissionIdsAsync(role.Id, Arg.Any<CancellationToken>())
            .Returns(currentPermissionIds ?? newPermissionIds);

        var updatedRole = CreateRole(name: "UpdatedRole", id: role.Id);
        // Return the updated role on second fetch
        _roleRepository.GetByIdWithPermissionsAsync(role.Id, Arg.Any<CancellationToken>())
            .Returns(role, updatedRole);
    }

    /// <summary>
    /// TP-SEC-11-01: PUT /roles/:id actualiza nombre, descripcion y permisos
    /// </summary>
    [Fact]
    public async Task Handle_ValidCommand_UpdatesNameDescriptionAndPermissions()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var role = CreateRole(name: "OldName", id: roleId);
        var permId = Guid.NewGuid();
        var command = BuildCommand(roleId: roleId, name: "NewName", permissionIds: [permId]);

        SetupValidUpdateFlow(role, [permId], currentPermissionIds: []);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _roleRepository.Received(1).Update(role);
        await _roleRepository.Received(1).SetRolePermissionsAsync(
            roleId, Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>());
        await _roleRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// TP-SEC-11-02: Diff de permisos: removidos se eliminan, agregados se insertan
    /// </summary>
    [Fact]
    public async Task Handle_PermissionDiff_PublishesRoleUpdatedEventWithDiff()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var role = CreateRole(name: "TestRole", id: roleId);
        var existingPermId = Guid.NewGuid();
        var newPermId = Guid.NewGuid();
        var command = BuildCommand(roleId: roleId, name: "TestRole", permissionIds: [newPermId]);

        _roleRepository.GetByIdWithPermissionsAsync(roleId, Arg.Any<CancellationToken>())
            .Returns(role);
        _roleRepository.GetByNameAsync(TenantId, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((Role?)null);
        _roleRepository.GetCurrentPermissionIdsAsync(roleId, Arg.Any<CancellationToken>())
            .Returns(new List<Guid> { existingPermId } as IReadOnlyList<Guid>);

        var updatedRole = CreateRole(name: "TestRole", id: roleId);
        _roleRepository.GetByIdWithPermissionsAsync(roleId, Arg.Any<CancellationToken>())
            .Returns(role, updatedRole);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _eventPublisher.Received(1).PublishAsync(
            Arg.Is<RoleUpdatedEvent>(e =>
                e.AddedPermissionIds.Contains(newPermId) &&
                e.RemovedPermissionIds.Contains(existingPermId)),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// TP-SEC-11-03: Editar solo metadata sin cambiar permisos funciona
    /// </summary>
    [Fact]
    public async Task Handle_SamePermissions_DoesNotPublishRoleUpdatedEvent()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var permId = Guid.NewGuid();
        var role = CreateRole(name: "TestRole", id: roleId);
        var command = BuildCommand(roleId: roleId, name: "RenamedRole", permissionIds: [permId]);

        _roleRepository.GetByIdWithPermissionsAsync(roleId, Arg.Any<CancellationToken>())
            .Returns(role);
        _roleRepository.GetByNameAsync(TenantId, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((Role?)null);
        _roleRepository.GetCurrentPermissionIdsAsync(roleId, Arg.Any<CancellationToken>())
            .Returns(new List<Guid> { permId } as IReadOnlyList<Guid>);

        var updatedRole = CreateRole(name: "RenamedRole", id: roleId);
        _roleRepository.GetByIdWithPermissionsAsync(roleId, Arg.Any<CancellationToken>())
            .Returns(role, updatedRole);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert - No RoleUpdatedEvent when permissions unchanged
        await _eventPublisher.DidNotReceive().PublishAsync(
            Arg.Any<RoleUpdatedEvent>(),
            Arg.Any<CancellationToken>());
        // But DomainEvent is still published
        await _eventPublisher.Received(1).PublishAsync(
            Arg.Any<Shared.Contract.Events.DomainEvent>(),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// TP-SEC-11-04: Rol de sistema (Super Admin) permite cambiar permisos y descripcion
    /// </summary>
    [Fact]
    public async Task Handle_SuperAdminRole_AllowsPermissionAndDescriptionChange()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var role = CreateRole(name: "Super Admin", isSystem: true, id: roleId);

        // Create critical permissions
        var criticalCodes = new[] { "p_roles_list", "p_roles_create", "p_roles_edit", "p_roles_delete", "p_users_list", "p_users_create", "p_users_edit" };
        var criticalPerms = criticalCodes.Select(c => CreatePermissionWithCode(c)).ToList();
        var permIds = criticalPerms.Select(p => p.Id).ToArray();

        var command = BuildCommand(roleId: roleId, name: "Super Admin", description: "Updated desc", permissionIds: permIds);

        _roleRepository.GetByIdWithPermissionsAsync(roleId, Arg.Any<CancellationToken>())
            .Returns(role);
        _permissionRepository.GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(criticalPerms.AsReadOnly() as IReadOnlyList<Permission>);
        _roleRepository.GetByNameAsync(TenantId, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((Role?)null);
        _roleRepository.GetCurrentPermissionIdsAsync(roleId, Arg.Any<CancellationToken>())
            .Returns(permIds.ToList() as IReadOnlyList<Guid>);

        var updatedRole = CreateRole(name: "Super Admin", isSystem: true, id: roleId);
        _roleRepository.GetByIdWithPermissionsAsync(roleId, Arg.Any<CancellationToken>())
            .Returns(role, updatedRole);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _roleRepository.Received(1).Update(role);
    }

    /// <summary>
    /// TP-SEC-11-04b: Super Admin missing critical permissions throws
    /// </summary>
    [Fact]
    public async Task Handle_SuperAdminMissingCriticalPermissions_Throws()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var role = CreateRole(name: "Super Admin", isSystem: true, id: roleId);

        // Only provide some critical permissions, missing others
        var partialPerms = new List<Permission>
        {
            CreatePermissionWithCode("p_roles_list"),
            CreatePermissionWithCode("p_users_list")
        };
        var permIds = partialPerms.Select(p => p.Id).ToArray();
        var command = BuildCommand(roleId: roleId, name: "Super Admin", permissionIds: permIds);

        _roleRepository.GetByIdWithPermissionsAsync(roleId, Arg.Any<CancellationToken>())
            .Returns(role);
        _permissionRepository.GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(partialPerms.AsReadOnly() as IReadOnlyList<Permission>);

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None).AsTask();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*ROLE_SUPER_ADMIN_MISSING_CRITICAL_PERMISSIONS*");
    }

    /// <summary>
    /// TP-SEC-11-05: Non-Super-Admin system role cannot be modified
    /// </summary>
    [Fact]
    public async Task Handle_NonSuperAdminSystemRole_ThrowsForbidden()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var role = CreateRole(name: "Auditor", isSystem: true, id: roleId);
        var command = BuildCommand(roleId: roleId, name: "Auditor Updated");

        _roleRepository.GetByIdWithPermissionsAsync(roleId, Arg.Any<CancellationToken>())
            .Returns(role);

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None).AsTask();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No se puede modificar un rol de sistema*");
    }

    /// <summary>
    /// TP-SEC-11-06: Nombre duplicado con otro rol retorna 409
    /// </summary>
    [Fact]
    public async Task Handle_DuplicateNameDifferentRole_ThrowsConflict()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var otherRoleId = Guid.NewGuid();
        var role = CreateRole(name: "OldName", id: roleId);
        var otherRole = CreateRole(name: "TakenName", id: otherRoleId);
        var command = BuildCommand(roleId: roleId, name: "TakenName");

        _roleRepository.GetByIdWithPermissionsAsync(roleId, Arg.Any<CancellationToken>())
            .Returns(role);
        _roleRepository.GetByNameAsync(TenantId, "TakenName", Arg.Any<CancellationToken>())
            .Returns(otherRole);

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None).AsTask();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*ya existe un rol con ese nombre*");
    }

    /// <summary>
    /// TP-SEC-11-07: permission_ids vacio retorna 422
    /// </summary>
    [Fact]
    public async Task Handle_EmptyPermissionIds_ThrowsValidation()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var role = CreateRole(name: "TestRole", id: roleId);
        var command = BuildCommand(roleId: roleId, permissionIds: []);

        _roleRepository.GetByIdWithPermissionsAsync(roleId, Arg.Any<CancellationToken>())
            .Returns(role);

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None).AsTask();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*ROLE_PERMISSIONS_EMPTY*");
    }

    /// <summary>
    /// TP-SEC-11-08: Rol inexistente retorna 404
    /// </summary>
    [Fact]
    public async Task Handle_RoleNotFound_ThrowsNotFound()
    {
        // Arrange
        var command = BuildCommand();
        _roleRepository.GetByIdWithPermissionsAsync(command.RoleId, Arg.Any<CancellationToken>())
            .Returns((Role?)null);

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None).AsTask();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Rol no encontrado*");
    }

    /// <summary>
    /// TP-SEC-11-06b: Same name same role is allowed
    /// </summary>
    [Fact]
    public async Task Handle_SameNameSameRole_DoesNotThrow()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var role = CreateRole(name: "MyRole", id: roleId);
        var permId = Guid.NewGuid();
        var command = BuildCommand(roleId: roleId, name: "MyRole", permissionIds: [permId]);

        _roleRepository.GetByIdWithPermissionsAsync(roleId, Arg.Any<CancellationToken>())
            .Returns(role);
        // Same role returned by name lookup
        _roleRepository.GetByNameAsync(TenantId, "MyRole", Arg.Any<CancellationToken>())
            .Returns(role);
        _roleRepository.GetCurrentPermissionIdsAsync(roleId, Arg.Any<CancellationToken>())
            .Returns(new List<Guid> { permId } as IReadOnlyList<Guid>);

        var updatedRole = CreateRole(name: "MyRole", id: roleId);
        _roleRepository.GetByIdWithPermissionsAsync(roleId, Arg.Any<CancellationToken>())
            .Returns(role, updatedRole);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    /// <summary>
    /// TP-SEC-11-01b: DomainEvent always published on update
    /// </summary>
    [Fact]
    public async Task Handle_ValidUpdate_AlwaysPublishesDomainEvent()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var permId = Guid.NewGuid();
        var role = CreateRole(name: "TestRole", id: roleId);
        var command = BuildCommand(roleId: roleId, permissionIds: [permId]);

        _roleRepository.GetByIdWithPermissionsAsync(roleId, Arg.Any<CancellationToken>())
            .Returns(role);
        _roleRepository.GetByNameAsync(TenantId, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((Role?)null);
        _roleRepository.GetCurrentPermissionIdsAsync(roleId, Arg.Any<CancellationToken>())
            .Returns(new List<Guid> { permId } as IReadOnlyList<Guid>);

        var updatedRole = CreateRole(name: "UpdatedRole", id: roleId);
        _roleRepository.GetByIdWithPermissionsAsync(roleId, Arg.Any<CancellationToken>())
            .Returns(role, updatedRole);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _eventPublisher.Received(1).PublishAsync(
            Arg.Any<Shared.Contract.Events.DomainEvent>(),
            Arg.Any<CancellationToken>());
    }
}
