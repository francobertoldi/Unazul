namespace SA.Identity.DataAccess.EntityFramework.Seed;

/// <summary>
/// Seed data for the 7 predefined system roles (RN-SEC-13).
/// Roles are per-tenant, so they cannot be seeded globally via HasData.
/// This class provides role templates used by ITenantSeeder when provisioning a new tenant.
/// </summary>
public static class RoleSeedData
{
    public sealed record RoleTemplate(string Name, string Description, bool IsSystem, IReadOnlyList<string> PermissionCodes);

    /// <summary>
    /// Returns the 7 predefined system role templates with their default permission codes.
    /// </summary>
    public static IReadOnlyList<RoleTemplate> GetRoleTemplates() =>
    [
        SuperAdmin(),
        AdminEntidad(),
        Operador(),
        AdminProducto(),
        DisenadorDeProcesos(),
        Auditor(),
        Consulta(),
    ];

    // ────────────────────────────────────────────────────────────────────
    // 1. Super Admin — ALL 88 permissions
    // ────────────────────────────────────────────────────────────────────
    public static RoleTemplate SuperAdmin() => new(
        Name: "Super Admin",
        Description: "Acceso total al sistema. Gestiona organizaciones, configuracion global y seguridad.",
        IsSystem: true,
        PermissionCodes: PermissionSeedData.GetAllPermissionCodes()
    );

    // ────────────────────────────────────────────────────────────────────
    // 2. Admin Entidad — Entity management + users + applications + basic reads
    // ────────────────────────────────────────────────────────────────────
    public static RoleTemplate AdminEntidad() => new(
        Name: "Admin Entidad",
        Description: "Administra entidades, sucursales, usuarios y solicitudes de su entidad.",
        IsSystem: true,
        PermissionCodes:
        [
            // Entidades (full)
            "p_ent_list", "p_ent_create", "p_ent_edit", "p_ent_delete", "p_ent_detail", "p_ent_export",
            // Sucursales (full)
            "p_branch_list", "p_branch_create", "p_branch_edit", "p_branch_delete",
            // Usuarios (full)
            "p_users_list", "p_users_create", "p_users_edit", "p_users_detail",
            // Roles (read-only)
            "p_roles_list",
            // Solicitudes (full)
            "p_app_list", "p_app_create", "p_app_edit", "p_app_detail", "p_app_transition", "p_app_export",
            // Liquidaciones (read-only)
            "p_settle_list", "p_settle_detail", "p_settle_export",
            // Catalogo productos (read-only)
            "p_cat_list", "p_cat_detail",
            // Catalogo familias (read-only)
            "p_cat_family_list",
            // Comisiones (read-only)
            "p_com_list",
            // Organizaciones (read-only)
            "p_org_list", "p_org_detail",
        ]
    );

    // ────────────────────────────────────────────────────────────────────
    // 3. Operador — Applications CRUD + transitions + settlements + reads
    // ────────────────────────────────────────────────────────────────────
    public static RoleTemplate Operador() => new(
        Name: "Operador",
        Description: "Procesa solicitudes, gestiona documentos y observaciones.",
        IsSystem: true,
        PermissionCodes:
        [
            // Solicitudes (full)
            "p_app_list", "p_app_create", "p_app_edit", "p_app_detail", "p_app_transition", "p_app_export",
            // Liquidaciones (full)
            "p_settle_list", "p_settle_create", "p_settle_detail", "p_settle_export",
            // Catalogo productos (read-only)
            "p_cat_list", "p_cat_detail",
            // Catalogo familias (read-only)
            "p_cat_family_list",
            // Comisiones (read-only)
            "p_com_list",
            // Entidades (read-only)
            "p_ent_list", "p_ent_detail",
            // Sucursales (read-only)
            "p_branch_list",
            // Organizaciones (read-only)
            "p_org_list", "p_org_detail",
        ]
    );

    // ────────────────────────────────────────────────────────────────────
    // 4. Admin Producto — Full catalog management + basic reads
    // ────────────────────────────────────────────────────────────────────
    public static RoleTemplate AdminProducto() => new(
        Name: "Admin Producto",
        Description: "Mantiene catalogo de familias, productos, planes y comisiones.",
        IsSystem: true,
        PermissionCodes:
        [
            // Catalogo familias (full)
            "p_cat_family_list", "p_cat_family_create", "p_cat_family_edit", "p_cat_family_delete",
            // Catalogo productos (full)
            "p_cat_list", "p_cat_create", "p_cat_edit", "p_cat_delete", "p_cat_detail", "p_cat_export", "p_cat_deprecate",
            // Catalogo planes (full)
            "p_cat_plan_create", "p_cat_plan_edit", "p_cat_plan_delete",
            // Catalogo coberturas (full)
            "p_cat_coverage_create", "p_cat_coverage_edit", "p_cat_coverage_delete",
            // Catalogo requisitos (full)
            "p_cat_req_create", "p_cat_req_edit", "p_cat_req_delete",
            // Comisiones (full)
            "p_com_list", "p_com_create", "p_com_edit", "p_com_delete",
            // Entidades (read-only)
            "p_ent_list", "p_ent_detail",
            // Organizaciones (read-only)
            "p_org_list", "p_org_detail",
        ]
    );

    // ────────────────────────────────────────────────────────────────────
    // 5. Disenador de Procesos — Workflow + config + templates
    // ────────────────────────────────────────────────────────────────────
    public static RoleTemplate DisenadorDeProcesos() => new(
        Name: "Disenador de Procesos",
        Description: "Disena workflows en el editor visual, gestiona configuracion y plantillas de notificacion.",
        IsSystem: true,
        PermissionCodes:
        [
            // Configuracion - Parametros (full)
            "p_cfg_param_list", "p_cfg_param_create", "p_cfg_param_edit", "p_cfg_param_delete",
            // Configuracion - Servicios externos (full)
            "p_cfg_svc_list", "p_cfg_svc_create", "p_cfg_svc_edit", "p_cfg_svc_delete", "p_cfg_svc_test",
            // Configuracion - Workflows (full)
            "p_cfg_wf_list", "p_cfg_wf_create", "p_cfg_wf_edit", "p_cfg_wf_activate", "p_cfg_wf_deactivate",
            // Configuracion - Plantillas de notificacion (full)
            "p_cfg_tpl_list", "p_cfg_tpl_create", "p_cfg_tpl_edit", "p_cfg_tpl_delete",
            // Catalogo productos (read-only for workflow context)
            "p_cat_list", "p_cat_detail",
            // Catalogo familias (read-only)
            "p_cat_family_list",
        ]
    );

    // ────────────────────────────────────────────────────────────────────
    // 6. Auditor — Audit read/export + read-only across main modules
    // ────────────────────────────────────────────────────────────────────
    public static RoleTemplate Auditor() => new(
        Name: "Auditor",
        Description: "Consulta logs de auditoria y exporta reportes. Lectura en modulos principales.",
        IsSystem: true,
        PermissionCodes:
        [
            // Auditoria (full)
            "p_audit_list", "p_audit_export",
            // Organizaciones (read-only)
            "p_org_list", "p_org_detail", "p_org_export",
            // Entidades (read-only)
            "p_ent_list", "p_ent_detail", "p_ent_export",
            // Sucursales (read-only)
            "p_branch_list",
            // Usuarios (read-only)
            "p_users_list", "p_users_detail",
            // Roles (read-only)
            "p_roles_list",
            // Solicitudes (read-only)
            "p_app_list", "p_app_detail", "p_app_export",
            // Liquidaciones (read-only)
            "p_settle_list", "p_settle_detail", "p_settle_export",
            // Catalogo productos (read-only)
            "p_cat_list", "p_cat_detail", "p_cat_export",
            // Catalogo familias (read-only)
            "p_cat_family_list",
            // Comisiones (read-only)
            "p_com_list",
        ]
    );

    // ────────────────────────────────────────────────────────────────────
    // 7. Consulta — Read-only across all modules (only list/detail/export)
    // ────────────────────────────────────────────────────────────────────
    public static RoleTemplate Consulta() => new(
        Name: "Consulta",
        Description: "Lectura en todos los modulos. Solo permisos de listar, ver detalle y exportar.",
        IsSystem: true,
        PermissionCodes:
        [
            // Organizaciones
            "p_org_list", "p_org_detail", "p_org_export",
            // Entidades
            "p_ent_list", "p_ent_detail", "p_ent_export",
            // Sucursales
            "p_branch_list",
            // Usuarios
            "p_users_list", "p_users_detail",
            // Roles
            "p_roles_list",
            // Catalogo familias
            "p_cat_family_list",
            // Catalogo productos
            "p_cat_list", "p_cat_detail", "p_cat_export",
            // Comisiones
            "p_com_list",
            // Solicitudes
            "p_app_list", "p_app_detail", "p_app_export",
            // Liquidaciones
            "p_settle_list", "p_settle_detail", "p_settle_export",
            // Configuracion (read-only lists)
            "p_cfg_param_list", "p_cfg_svc_list", "p_cfg_wf_list", "p_cfg_tpl_list",
            // Auditoria
            "p_audit_list", "p_audit_export",
        ]
    );
}
