import { Entity, User, Role, Permission, AuditEntry, Branch, Product, ProductPlan, Coverage, ProductRequirement, Application, Applicant, Beneficiary, ApplicationObservation, ApplicationDocument, WorkflowDefinition, WorkflowState, WorkflowTransition, Tenant, ProductFamily, CommissionPlan } from './types';

export const mockPermissions: Permission[] = [
  // ── Organizaciones ──
  { id: 'p_org_list', module: 'Organizaciones', action: 'Listar', description: 'Ver listado de organizaciones' },
  { id: 'p_org_view', module: 'Organizaciones', action: 'Ver Detalle', description: 'Ver detalle de una organización' },
  { id: 'p_org_create', module: 'Organizaciones', action: 'Crear', description: 'Crear nuevas organizaciones' },
  { id: 'p_org_edit', module: 'Organizaciones', action: 'Editar', description: 'Modificar organizaciones existentes' },
  { id: 'p_org_delete', module: 'Organizaciones', action: 'Eliminar', description: 'Eliminar organizaciones' },
  { id: 'p_org_export', module: 'Organizaciones', action: 'Exportar', description: 'Exportar listado a Excel/CSV' },

  // ── Entidades ──
  { id: 'p_ent_list', module: 'Entidades', action: 'Listar', description: 'Ver listado de entidades' },
  { id: 'p_ent_view', module: 'Entidades', action: 'Ver Detalle', description: 'Ver detalle de una entidad' },
  { id: 'p_ent_create', module: 'Entidades', action: 'Crear', description: 'Crear nuevas entidades' },
  { id: 'p_ent_edit', module: 'Entidades', action: 'Editar', description: 'Modificar entidades existentes' },
  { id: 'p_ent_delete', module: 'Entidades', action: 'Eliminar', description: 'Eliminar entidades' },
  { id: 'p_ent_export', module: 'Entidades', action: 'Exportar', description: 'Exportar listado a Excel/CSV' },

  // ── Sucursales ──
  { id: 'p_branch_list', module: 'Sucursales', action: 'Listar', description: 'Ver sucursales de una entidad' },
  { id: 'p_branch_create', module: 'Sucursales', action: 'Crear', description: 'Crear nuevas sucursales' },
  { id: 'p_branch_edit', module: 'Sucursales', action: 'Editar', description: 'Modificar sucursales existentes' },
  { id: 'p_branch_delete', module: 'Sucursales', action: 'Eliminar', description: 'Eliminar sucursales' },

  // ── Solicitudes ──
  { id: 'p_app_list', module: 'Solicitudes', action: 'Listar', description: 'Ver listado de solicitudes' },
  { id: 'p_app_view', module: 'Solicitudes', action: 'Ver Detalle', description: 'Ver detalle de una solicitud' },
  { id: 'p_app_create', module: 'Solicitudes', action: 'Crear', description: 'Crear nuevas solicitudes' },
  { id: 'p_app_edit', module: 'Solicitudes', action: 'Editar', description: 'Modificar solicitudes existentes' },
  { id: 'p_app_delete', module: 'Solicitudes', action: 'Eliminar', description: 'Eliminar solicitudes' },
  { id: 'p_app_approve', module: 'Solicitudes', action: 'Aprobar', description: 'Aprobar solicitudes' },
  { id: 'p_app_reject', module: 'Solicitudes', action: 'Rechazar', description: 'Rechazar solicitudes' },
  { id: 'p_app_assign', module: 'Solicitudes', action: 'Asignar', description: 'Asignar solicitudes a usuarios' },
  { id: 'p_app_observe', module: 'Solicitudes', action: 'Agregar Observaciones', description: 'Agregar observaciones a solicitudes' },
  { id: 'p_app_docs', module: 'Solicitudes', action: 'Gestionar Documentos', description: 'Aprobar/rechazar documentos adjuntos' },
  { id: 'p_app_trace', module: 'Solicitudes', action: 'Ver Trazabilidad', description: 'Ver timeline de trazabilidad del workflow' },
  { id: 'p_app_export', module: 'Solicitudes', action: 'Exportar', description: 'Exportar listado a Excel/CSV' },

  // ── Familias de Productos ──
  { id: 'p_fam_list', module: 'Familias de Productos', action: 'Listar', description: 'Ver listado de familias' },
  { id: 'p_fam_create', module: 'Familias de Productos', action: 'Crear', description: 'Crear nuevas familias' },
  { id: 'p_fam_edit', module: 'Familias de Productos', action: 'Editar', description: 'Modificar familias existentes' },
  { id: 'p_fam_delete', module: 'Familias de Productos', action: 'Eliminar', description: 'Eliminar familias' },

  // ── Productos ──
  { id: 'p_prod_list', module: 'Productos', action: 'Listar', description: 'Ver listado de productos' },
  { id: 'p_prod_view', module: 'Productos', action: 'Ver Detalle', description: 'Ver detalle de un producto' },
  { id: 'p_prod_create', module: 'Productos', action: 'Crear', description: 'Crear nuevos productos' },
  { id: 'p_prod_edit', module: 'Productos', action: 'Editar', description: 'Modificar productos existentes' },
  { id: 'p_prod_delete', module: 'Productos', action: 'Eliminar', description: 'Eliminar productos' },
  { id: 'p_prod_export', module: 'Productos', action: 'Exportar', description: 'Exportar listado a Excel/CSV' },

  // ── Sub Productos / Planes ──
  { id: 'p_plan_create', module: 'Sub Productos', action: 'Crear', description: 'Crear sub productos / planes' },
  { id: 'p_plan_edit', module: 'Sub Productos', action: 'Editar', description: 'Modificar sub productos / planes' },
  { id: 'p_plan_delete', module: 'Sub Productos', action: 'Eliminar', description: 'Eliminar sub productos / planes' },

  // ── Requisitos de Producto ──
  { id: 'p_req_create', module: 'Requisitos', action: 'Crear', description: 'Crear requisitos documentales' },
  { id: 'p_req_edit', module: 'Requisitos', action: 'Editar', description: 'Modificar requisitos documentales' },
  { id: 'p_req_delete', module: 'Requisitos', action: 'Eliminar', description: 'Eliminar requisitos documentales' },

  // ── Planes de Comisiones ──
  { id: 'p_comm_list', module: 'Planes de Comisiones', action: 'Listar', description: 'Ver listado de planes de comisiones' },
  { id: 'p_comm_create', module: 'Planes de Comisiones', action: 'Crear', description: 'Crear planes de comisiones' },
  { id: 'p_comm_edit', module: 'Planes de Comisiones', action: 'Editar', description: 'Modificar planes de comisiones' },
  { id: 'p_comm_delete', module: 'Planes de Comisiones', action: 'Eliminar', description: 'Eliminar planes de comisiones' },

  // ── Workflows ──
  { id: 'p_wf_list', module: 'Workflows', action: 'Listar', description: 'Ver listado de workflows' },
  { id: 'p_wf_view', module: 'Workflows', action: 'Ver Detalle', description: 'Ver detalle de un workflow' },
  { id: 'p_wf_create', module: 'Workflows', action: 'Crear', description: 'Crear nuevos workflows' },
  { id: 'p_wf_edit', module: 'Workflows', action: 'Editar', description: 'Modificar workflows existentes' },
  { id: 'p_wf_delete', module: 'Workflows', action: 'Eliminar', description: 'Eliminar workflows' },
  { id: 'p_wf_design', module: 'Workflows', action: 'Diseñar Flujos', description: 'Usar el editor visual de flujos' },

  // ── Usuarios ──
  { id: 'p_user_list', module: 'Usuarios', action: 'Listar', description: 'Ver listado de usuarios' },
  { id: 'p_user_view', module: 'Usuarios', action: 'Ver Detalle', description: 'Ver detalle de un usuario' },
  { id: 'p_user_create', module: 'Usuarios', action: 'Crear', description: 'Crear nuevos usuarios' },
  { id: 'p_user_edit', module: 'Usuarios', action: 'Editar', description: 'Modificar usuarios existentes' },
  { id: 'p_user_delete', module: 'Usuarios', action: 'Eliminar', description: 'Eliminar usuarios' },
  { id: 'p_user_lock', module: 'Usuarios', action: 'Bloquear / Desbloquear', description: 'Bloquear o desbloquear cuentas de usuario' },
  { id: 'p_user_export', module: 'Usuarios', action: 'Exportar', description: 'Exportar listado a Excel/CSV' },

  // ── Roles ──
  { id: 'p_role_list', module: 'Roles', action: 'Listar', description: 'Ver listado de roles' },
  { id: 'p_role_create', module: 'Roles', action: 'Crear', description: 'Crear nuevos roles' },
  { id: 'p_role_edit', module: 'Roles', action: 'Editar', description: 'Modificar roles existentes' },
  { id: 'p_role_delete', module: 'Roles', action: 'Eliminar', description: 'Eliminar roles' },

  // ── Auditoría ──
  { id: 'p_audit_list', module: 'Auditoría', action: 'Consultar', description: 'Consultar el log de auditoría' },
  { id: 'p_audit_export', module: 'Auditoría', action: 'Exportar', description: 'Exportar log de auditoría a Excel/CSV' },

  // ── Parámetros ──
  { id: 'p_param_list', module: 'Parámetros', action: 'Listar', description: 'Ver grupos y parámetros del sistema' },
  { id: 'p_param_create', module: 'Parámetros', action: 'Crear', description: 'Crear parámetros y grupos' },
  { id: 'p_param_edit', module: 'Parámetros', action: 'Editar', description: 'Modificar parámetros existentes' },
  { id: 'p_param_delete', module: 'Parámetros', action: 'Eliminar', description: 'Eliminar parámetros y grupos' },

  // ── Servicios Externos ──
  { id: 'p_svc_list', module: 'Servicios Externos', action: 'Listar', description: 'Ver servicios externos configurados' },
  { id: 'p_svc_create', module: 'Servicios Externos', action: 'Crear', description: 'Crear nuevos servicios externos' },
  { id: 'p_svc_edit', module: 'Servicios Externos', action: 'Editar', description: 'Modificar servicios externos' },
  { id: 'p_svc_delete', module: 'Servicios Externos', action: 'Eliminar', description: 'Eliminar servicios externos' },
  { id: 'p_svc_test', module: 'Servicios Externos', action: 'Probar Conexión', description: 'Ejecutar pruebas de conexión' },
];

const allPermIds = mockPermissions.map(p => p.id);

export const mockRoles: Role[] = [
  { id: 'r1', name: 'Super Admin', description: 'Acceso total al sistema', permissions: allPermIds, userCount: 2 },
  { id: 'r2', name: 'Admin Entidad', description: 'Administración de una entidad específica', permissions: [
    'p_org_list', 'p_org_view',
    'p_ent_list', 'p_ent_view', 'p_ent_edit', 'p_ent_export',
    'p_branch_list', 'p_branch_create', 'p_branch_edit', 'p_branch_delete',
    'p_app_list', 'p_app_view', 'p_app_create', 'p_app_edit', 'p_app_approve', 'p_app_reject', 'p_app_assign', 'p_app_observe', 'p_app_docs', 'p_app_trace', 'p_app_export',
    'p_prod_list', 'p_prod_view', 'p_prod_edit', 'p_prod_export',
    'p_plan_create', 'p_plan_edit', 'p_plan_delete',
    'p_fam_list',
    'p_comm_list',
    'p_user_list', 'p_user_view', 'p_user_create', 'p_user_edit', 'p_user_lock', 'p_user_export',
    'p_param_list',
    'p_svc_list',
  ], userCount: 5 },
  { id: 'r3', name: 'Operador', description: 'Operaciones diarias y gestión de solicitudes', permissions: [
    'p_ent_list', 'p_ent_view',
    'p_branch_list',
    'p_app_list', 'p_app_view', 'p_app_create', 'p_app_edit', 'p_app_observe', 'p_app_docs', 'p_app_trace', 'p_app_export',
    'p_prod_list', 'p_prod_view',
    'p_fam_list',
  ], userCount: 12 },
  { id: 'r4', name: 'Auditor', description: 'Solo lectura y auditoría', permissions: [
    'p_org_list', 'p_org_view',
    'p_ent_list', 'p_ent_view',
    'p_branch_list',
    'p_app_list', 'p_app_view', 'p_app_trace', 'p_app_export',
    'p_prod_list', 'p_prod_view',
    'p_fam_list',
    'p_comm_list',
    'p_wf_list', 'p_wf_view',
    'p_user_list', 'p_user_view',
    'p_role_list',
    'p_audit_list', 'p_audit_export',
    'p_param_list',
    'p_svc_list',
  ], userCount: 3 },
  { id: 'r5', name: 'Consulta', description: 'Solo consulta de información', permissions: [
    'p_org_list', 'p_org_view',
    'p_ent_list', 'p_ent_view',
    'p_branch_list',
    'p_app_list', 'p_app_view', 'p_app_trace',
    'p_prod_list', 'p_prod_view',
    'p_fam_list',
    'p_comm_list',
    'p_wf_list', 'p_wf_view',
    'p_user_list', 'p_user_view',
    'p_param_list',
  ], userCount: 8 },
  { id: 'r6', name: 'Diseñador de Procesos', description: 'Diseño de workflows y reglas', permissions: [
    'p_ent_list', 'p_ent_view',
    'p_prod_list', 'p_prod_view',
    'p_fam_list',
    'p_wf_list', 'p_wf_view', 'p_wf_create', 'p_wf_edit', 'p_wf_delete', 'p_wf_design',
    'p_svc_list',
  ], userCount: 2 },
  { id: 'r7', name: 'Admin Producto', description: 'Gestión del catálogo de productos', permissions: [
    'p_ent_list', 'p_ent_view',
    'p_prod_list', 'p_prod_view', 'p_prod_create', 'p_prod_edit', 'p_prod_delete', 'p_prod_export',
    'p_plan_create', 'p_plan_edit', 'p_plan_delete',
    'p_req_create', 'p_req_edit', 'p_req_delete',
    'p_fam_list', 'p_fam_create', 'p_fam_edit', 'p_fam_delete',
    'p_comm_list', 'p_comm_create', 'p_comm_edit', 'p_comm_delete',
  ], userCount: 3 },
];

const branches1: Branch[] = [
  { id: 'b1', entityId: 'e1', name: 'Casa Central', code: '0001', address: 'Av. Corrientes 1234', city: 'Ciudad Autónoma de Buenos Aires', province: 'CABA', status: 'active', manager: 'Carlos Pérez', phoneCode: '+54', phone: '11 4321-5678' },
  { id: 'b2', entityId: 'e1', name: 'Sucursal Córdoba', code: '0015', address: 'Av. Colón 456', city: 'Córdoba', province: 'Córdoba', status: 'active', manager: 'María García', phoneCode: '+54', phone: '351 421-9876' },
  { id: 'b3', entityId: 'e1', name: 'Sucursal Rosario', code: '0023', address: 'Bv. Oroño 789', city: 'Rosario', province: 'Santa Fe', status: 'inactive', manager: 'Jorge López', phoneCode: '+54', phone: '341 456-7890' },
];

const branches2: Branch[] = [
  { id: 'b4', entityId: 'e2', name: 'Oficina Principal', code: '0001', address: 'Av. del Libertador 5678', city: 'Ciudad Autónoma de Buenos Aires', province: 'CABA', status: 'active', manager: 'Ana Martínez', phoneCode: '+54', phone: '11 5678-1234' },
  { id: 'b5', entityId: 'e2', name: 'Centro de Atención Mendoza', code: '0008', address: 'San Martín 321', city: 'Mendoza', province: 'Mendoza', status: 'active', manager: 'Roberto Sánchez', phoneCode: '+54', phone: '261 432-1098' },
];

export const mockTenants: Tenant[] = [
  { id: 't1', name: 'Grupo Financiero del Plata', identifier: '30-70000001-0', description: 'Holding financiero con operaciones bancarias y de seguros', status: 'active', contactName: 'Ricardo Méndez', contactEmail: 'rmendez@gfplata.com.ar', contactPhoneCode: '+54', contactPhone: '11 4000-0001', country: 'Argentina', createdAt: '2023-06-01' },
  { id: 't2', name: 'FinTech Latam Group', identifier: '30-70000002-8', description: 'Grupo de empresas fintech con presencia regional', status: 'active', contactName: 'Valentina Rossi', contactEmail: 'vrossi@fintechlatam.com', contactPhoneCode: '+54', contactPhone: '11 4000-0002', country: 'Argentina', createdAt: '2023-09-15' },
  { id: 't3', name: 'Cooperativas Unidas', identifier: '30-70000003-6', description: 'Red de cooperativas de crédito', status: 'inactive', contactName: 'Marcelo Díaz', contactEmail: 'mdiaz@coopunidas.com.ar', contactPhoneCode: '+54', contactPhone: '261 400-0003', country: 'Argentina', createdAt: '2023-04-20' },
];

export const mockEntities: Entity[] = [
  { id: 'e1', tenantId: 't1', tenantName: 'Grupo Financiero del Plata', name: 'Banco del Plata', identifier: '30-71234567-9', type: 'bank', status: 'active', email: 'contacto@bancodelplata.com.ar', phoneCode: '+54', phone: '11 4321-0000', address: 'Av. Corrientes 1234', city: 'Ciudad Autónoma de Buenos Aires', province: 'CABA', country: 'Argentina', createdAt: '2024-01-15', branches: branches1, channels: ['web', 'mobile', 'presencial'] },
  { id: 'e2', tenantId: 't1', tenantName: 'Grupo Financiero del Plata', name: 'Seguros Patagonia', identifier: '30-65432198-7', type: 'insurance', status: 'active', email: 'info@segurospatagonia.com.ar', phoneCode: '+54', phone: '11 5678-0000', address: 'Av. del Libertador 5678', city: 'Ciudad Autónoma de Buenos Aires', province: 'CABA', country: 'Argentina', createdAt: '2024-02-20', branches: branches2, channels: ['web', 'api'] },
  { id: 'e3', tenantId: 't2', tenantName: 'FinTech Latam Group', name: 'FinPay Digital', identifier: '30-98765432-1', type: 'fintech', status: 'active', email: 'soporte@finpay.io', phoneCode: '+54', phone: '11 9012-0000', address: 'Av. Madero 900', city: 'Ciudad Autónoma de Buenos Aires', province: 'CABA', country: 'Argentina', createdAt: '2024-03-10', branches: [], channels: ['web', 'mobile', 'api'] },
  { id: 'e4', tenantId: 't3', tenantName: 'Cooperativas Unidas', name: 'Cooperativa Andina', identifier: '30-45678901-5', type: 'cooperative', status: 'inactive', email: 'admin@coopandina.com.ar', phoneCode: '+54', phone: '261 300-0000', address: 'Av. San Martín 456', city: 'Mendoza', province: 'Mendoza', country: 'Argentina', createdAt: '2023-11-05', branches: [{ id: 'b6', entityId: 'e4', name: 'Sede Central', code: '0001', address: 'Av. San Martín 456', city: 'Mendoza', province: 'Mendoza', status: 'inactive', manager: 'Luis Fernández', phoneCode: '+54', phone: '261 300-0001' }], channels: ['presencial'] },
  { id: 'e5', tenantId: 't1', tenantName: 'Grupo Financiero del Plata', name: 'Banco Austral S.A.', identifier: '30-11223344-2', type: 'bank', status: 'suspended', email: 'mesa@bancoaustral.com.ar', phoneCode: '+54', phone: '11 7777-0000', address: 'Av. Rivadavia 2000', city: 'Ciudad Autónoma de Buenos Aires', province: 'CABA', country: 'Argentina', createdAt: '2023-08-18', branches: [{ id: 'b7', entityId: 'e5', name: 'Casa Matriz', code: '0001', address: 'Av. Rivadavia 2000', city: 'Ciudad Autónoma de Buenos Aires', province: 'CABA', status: 'suspended', manager: 'Patricia Ruiz', phoneCode: '+54', phone: '11 7777-0001' }], channels: ['web', 'presencial'] },
];

export const mockUsers: User[] = [
  { id: 'u1', username: 'admin', password: 'Admin2024!', email: 'admin@backoffice.com', firstName: 'Administrador', lastName: 'Sistema', entityId: '', entityName: 'Plataforma', roleIds: ['r1'], roleNames: ['Super Admin'], status: 'active', lastLogin: '2026-03-07T10:30:00', createdAt: '2024-01-01', assignments: { organizationIds: ['t1', 't2', 't3'], entityIds: [], branchIds: [] } },
  { id: 'u2', username: 'cperez', password: 'Cperez2024!', email: 'cperez@bancodelplata.com.ar', firstName: 'Carlos', lastName: 'Pérez', entityId: 'e1', entityName: 'Banco del Plata', roleIds: ['r2', 'r3'], roleNames: ['Admin Entidad', 'Operador'], status: 'active', lastLogin: '2026-03-07T09:15:00', createdAt: '2024-01-20', assignments: { organizationIds: ['t1'], entityIds: ['e1'], branchIds: ['b1', 'b2'] } },
  { id: 'u3', username: 'mgarcia', password: 'Mgarcia2024!', email: 'mgarcia@bancodelplata.com.ar', firstName: 'María', lastName: 'García', entityId: 'e1', entityName: 'Banco del Plata', roleIds: ['r3'], roleNames: ['Operador'], status: 'active', lastLogin: '2026-03-06T16:45:00', createdAt: '2024-02-01', assignments: { organizationIds: [], entityIds: ['e1'], branchIds: ['b2'] } },
  { id: 'u4', username: 'amartinez', password: 'Amartinez2024!', email: 'amartinez@segurospatagonia.com.ar', firstName: 'Ana', lastName: 'Martínez', entityId: 'e2', entityName: 'Seguros Patagonia', roleIds: ['r2'], roleNames: ['Admin Entidad'], status: 'active', lastLogin: '2026-03-07T08:00:00', createdAt: '2024-02-25', assignments: { organizationIds: ['t1'], entityIds: ['e2'], branchIds: ['b4', 'b5'] } },
  { id: 'u5', username: 'rsanchez', password: 'Rsanchez2024!', email: 'rsanchez@segurospatagonia.com.ar', firstName: 'Roberto', lastName: 'Sánchez', entityId: 'e2', entityName: 'Seguros Patagonia', roleIds: ['r3', 'r4'], roleNames: ['Operador', 'Auditor'], status: 'inactive', lastLogin: '2026-01-15T14:20:00', createdAt: '2024-03-05', assignments: { organizationIds: [], entityIds: ['e2'], branchIds: [] } },
  { id: 'u6', username: 'jlopez', password: 'Jlopez2024!', email: 'jlopez@finpay.io', firstName: 'Jorge', lastName: 'López', entityId: 'e3', entityName: 'FinPay Digital', roleIds: ['r2'], roleNames: ['Admin Entidad'], status: 'active', lastLogin: '2026-03-07T11:00:00', createdAt: '2024-03-15', assignments: { organizationIds: ['t2'], entityIds: ['e3'], branchIds: [] } },
  { id: 'u7', username: 'ldiaz', password: 'Ldiaz2024!', email: 'auditor@backoffice.com', firstName: 'Laura', lastName: 'Díaz', entityId: '', entityName: 'Plataforma', roleIds: ['r4'], roleNames: ['Auditor'], status: 'active', lastLogin: '2026-03-06T10:30:00', createdAt: '2024-04-01', assignments: { organizationIds: ['t1', 't2'], entityIds: [], branchIds: [] } },
  { id: 'u8', username: 'lfernandez', password: 'Lfernandez2024!', email: 'lfernandez@coopandina.com.ar', firstName: 'Luis', lastName: 'Fernández', entityId: 'e4', entityName: 'Cooperativa Andina', roleIds: ['r5'], roleNames: ['Consulta'], status: 'locked', lastLogin: '2025-12-10T09:00:00', createdAt: '2024-01-10', assignments: { organizationIds: ['t3'], entityIds: ['e4'], branchIds: ['b6'] } },
];

export const mockAuditLog: AuditEntry[] = [
  { id: 'a1', userId: 'u1', userName: 'Administrador Sistema', operationType: 'Crear', action: 'Crear Entidad', module: 'Organización', detail: 'Creó entidad "FinPay Digital"', timestamp: '2026-03-07T10:30:00', ip: '192.168.1.100' },
  { id: 'a2', userId: 'u2', userName: 'Carlos Pérez', operationType: 'Editar', action: 'Editar Sucursal', module: 'Organización', detail: 'Actualizó datos de Sucursal Córdoba', timestamp: '2026-03-07T09:15:00', ip: '10.0.0.50' },
  { id: 'a3', userId: 'u4', userName: 'Ana Martínez', operationType: 'Crear', action: 'Crear Usuario', module: 'Seguridad', detail: 'Creó usuario rsanchez@segurospatagonia.com.ar', timestamp: '2026-03-06T16:00:00', ip: '172.16.0.25' },
  { id: 'a4', userId: 'u1', userName: 'Administrador Sistema', operationType: 'Editar', action: 'Modificar Rol', module: 'Seguridad', detail: 'Agregó permisos al rol "Operador"', timestamp: '2026-03-06T14:20:00', ip: '192.168.1.100' },
  { id: 'a5', userId: 'u7', userName: 'Laura Díaz', operationType: 'Exportar', action: 'Consultar Auditoría', module: 'Auditoría', detail: 'Exportó log de auditoría del mes', timestamp: '2026-03-06T10:30:00', ip: '192.168.1.105' },
  { id: 'a6', userId: 'u2', userName: 'Carlos Pérez', operationType: 'Crear', action: 'Crear Solicitud', module: 'Solicitudes', detail: 'Solicitud SOL-2026-001 para Préstamo Personal', timestamp: '2026-03-07T11:00:00', ip: '10.0.0.50' },
  { id: 'a7', userId: 'u4', userName: 'Ana Martínez', operationType: 'Cambiar Estado', action: 'Aprobar Solicitud', module: 'Solicitudes', detail: 'Aprobó solicitud SOL-2026-003', timestamp: '2026-03-07T14:30:00', ip: '172.16.0.25' },
];

// ── Product Families ──

export const mockProductFamilies: ProductFamily[] = [
  { id: 'fam1', code: 'PREST', description: 'Préstamos' },
  { id: 'fam2', code: 'SEG-VIDA', description: 'Seguros de Vida' },
  { id: 'fam3', code: 'SEG-PATR', description: 'Seguros Patrimoniales' },
  { id: 'fam4', code: 'CTA-BANCARIA', description: 'Cuentas Bancarias' },
  { id: 'fam5', code: 'TARJETAS', description: 'Tarjetas de Crédito' },
  { id: 'fam6', code: 'INVERSIONES', description: 'Inversiones' },
];

// ── Planes de Comisiones ──

export const mockCommissionPlans: CommissionPlan[] = [
  { id: 'cp1', code: 'COM-FIJA-001', description: 'Valor fijo por venta (estándar)', valueType: 'fixed_per_sale', value: 15000, maxAmount: 150000 },
  { id: 'cp2', code: 'COM-CAP-001', description: 'Porcentaje del capital (tope alto)', valueType: 'percentage_capital', value: 2.5, maxAmount: 250000 },
  { id: 'cp3', code: 'COM-TOT-001', description: 'Porcentaje del total del préstamo (sin tope)', valueType: 'percentage_total_loan', value: 1.2 },
  { id: 'cp4', code: 'COM-FIJA-002', description: 'Valor fijo por venta (promo)', valueType: 'fixed_per_sale', value: 10000, maxAmount: 80000 },
  { id: 'cp5', code: 'COM-CAP-002', description: 'Porcentaje del capital (promo)', valueType: 'percentage_capital', value: 1.8, maxAmount: 120000 },
  { id: 'cp6', code: 'COM-TOT-002', description: 'Porcentaje del total del préstamo (tope)', valueType: 'percentage_total_loan', value: 0.9, maxAmount: 200000 },
  { id: 'cp7', code: 'COM-FIJA-003', description: 'Valor fijo por venta (premium)', valueType: 'fixed_per_sale', value: 25000 },
  { id: 'cp8', code: 'COM-CAP-003', description: 'Porcentaje del capital (premium)', valueType: 'percentage_capital', value: 3.1 },
  { id: 'cp9', code: 'COM-TOT-003', description: 'Porcentaje del total del préstamo (premium)', valueType: 'percentage_total_loan', value: 1.6, maxAmount: 300000 },
];

// ── Products ──

export const mockProducts: Product[] = [
  {
    id: 'prod1', entityId: 'e1', entityName: 'Banco del Plata', familyId: 'fam1', familyName: 'Préstamos',
    name: 'Préstamo Personal', code: 'PP-001',
    description: 'Préstamo personal con tasa fija a 12, 24 o 36 meses',
    version: 2, status: 'active', validFrom: '2025-01-01', createdAt: '2024-06-15',
    requirements: [
      { id: 'req1', name: 'DNI frente y dorso', type: 'document', mandatory: true, description: 'Documento de identidad escaneado' },
      { id: 'req2', name: 'Recibo de sueldo', type: 'document', mandatory: true, description: 'Últimos 3 recibos de sueldo' },
      { id: 'req3', name: 'Scoring crediticio', type: 'validation', mandatory: true, description: 'Score mínimo 600 puntos' },
    ],
    plans: [
      { id: 'plan1', productId: 'prod1', name: 'Plan 12 meses', description: 'Cuotas fijas a 12 meses', coverages: [], price: 0, currency: 'ARS', commissionPlanId: 'cp1', status: 'active' },
      { id: 'plan2', productId: 'prod1', name: 'Plan 24 meses', description: 'Cuotas fijas a 24 meses', coverages: [], price: 0, currency: 'ARS', commissionPlanId: 'cp2', status: 'active' },
      { id: 'plan3', productId: 'prod1', name: 'Plan 36 meses', description: 'Cuotas fijas a 36 meses', coverages: [], price: 0, currency: 'ARS', commissionPlanId: 'cp3', status: 'active' },
    ],
  },
  {
    id: 'prod2', entityId: 'e2', entityName: 'Seguros Patagonia', familyId: 'fam2', familyName: 'Seguros de Vida',
    name: 'Seguro de Vida Individual', code: 'SV-001',
    description: 'Seguro de vida con cobertura por fallecimiento e invalidez',
    version: 1, status: 'active', validFrom: '2025-03-01', createdAt: '2025-01-10',
    requirements: [
      { id: 'req4', name: 'DNI', type: 'document', mandatory: true, description: 'Documento de identidad' },
      { id: 'req5', name: 'Declaración jurada de salud', type: 'document', mandatory: true, description: 'Formulario de salud completado' },
      { id: 'req6', name: 'Edad mínima 18 años', type: 'validation', mandatory: true, description: 'El solicitante debe ser mayor de edad' },
    ],
    plans: [
      { id: 'plan4', productId: 'prod2', name: 'Plan Básico', description: 'Cobertura por fallecimiento', coverages: [
        { id: 'cov1', name: 'Fallecimiento', description: 'Cobertura por fallecimiento cualquier causa', sumInsured: 5000000, premium: 3500 },
      ], price: 3500, currency: 'ARS', commissionPlanId: 'cp4', status: 'active' },
      { id: 'plan5', productId: 'prod2', name: 'Plan Premium', description: 'Cobertura por fallecimiento e invalidez', coverages: [
        { id: 'cov2', name: 'Fallecimiento', description: 'Cobertura por fallecimiento cualquier causa', sumInsured: 10000000, premium: 5500 },
        { id: 'cov3', name: 'Invalidez Total', description: 'Invalidez total y permanente', sumInsured: 10000000, premium: 2000 },
      ], price: 7500, currency: 'ARS', commissionPlanId: 'cp5', status: 'active' },
    ],
  },
  {
    id: 'prod3', entityId: 'e1', entityName: 'Banco del Plata', familyId: 'fam4', familyName: 'Cuentas Bancarias',
    name: 'Cuenta Corriente', code: 'CC-001',
    description: 'Cuenta corriente en pesos con tarjeta de débito',
    version: 1, status: 'active', validFrom: '2024-01-01', createdAt: '2024-01-01',
    requirements: [
      { id: 'req7', name: 'DNI', type: 'document', mandatory: true, description: 'Documento de identidad' },
      { id: 'req8', name: 'Comprobante de domicilio', type: 'document', mandatory: false, description: 'Servicio a nombre del titular' },
    ],
    plans: [
      { id: 'plan6', productId: 'prod3', name: 'Cuenta Estándar', description: 'Sin costo de mantenimiento', coverages: [], price: 0, currency: 'ARS', commissionPlanId: 'cp6', status: 'active' },
    ],
  },
  {
    id: 'prod4', entityId: 'e3', entityName: 'FinPay Digital', familyId: 'fam5', familyName: 'Tarjetas de Crédito',
    name: 'Tarjeta de Crédito Virtual', code: 'TCV-001',
    description: 'Tarjeta de crédito 100% digital con aprobación inmediata',
    version: 1, status: 'active', validFrom: '2025-06-01', createdAt: '2025-05-01',
    requirements: [
      { id: 'req9', name: 'Selfie con DNI', type: 'document', mandatory: true, description: 'Foto del titular con documento' },
      { id: 'req10', name: 'Validación biométrica', type: 'validation', mandatory: true, description: 'Verificación facial automática' },
    ],
    plans: [
      { id: 'plan7', productId: 'prod4', name: 'Classic', description: 'Límite hasta $500.000', coverages: [], price: 0, currency: 'USD', commissionPlanId: 'cp7', status: 'active' },
      { id: 'plan8', productId: 'prod4', name: 'Gold', description: 'Límite hasta $2.000.000', coverages: [], price: 2500, currency: 'USD', commissionPlanId: 'cp8', status: 'active' },
    ],
  },
  {
    id: 'prod5', entityId: 'e2', entityName: 'Seguros Patagonia', familyId: 'fam3', familyName: 'Seguros Patrimoniales',
    name: 'Seguro de Hogar', code: 'SH-001',
    description: 'Seguro integral para el hogar con coberturas combinables',
    version: 1, status: 'draft', validFrom: '2026-01-01', createdAt: '2025-11-01',
    requirements: [
      { id: 'req11', name: 'Escritura o contrato', type: 'document', mandatory: true, description: 'Documento que acredite la propiedad o alquiler' },
    ],
    plans: [
      { id: 'plan9', productId: 'prod5', name: 'Hogar Básico', description: 'Incendio y robo', coverages: [
        { id: 'cov4', name: 'Incendio', description: 'Daños por incendio', sumInsured: 20000000, premium: 4000 },
        { id: 'cov5', name: 'Robo', description: 'Robo de contenido', sumInsured: 5000000, premium: 2500 },
      ], price: 6500, currency: 'ARS', commissionPlanId: 'cp9', status: 'draft' },
    ],
  },
];

// ── Workflows ──

const wfStates1: WorkflowState[] = [
  { id: 'ws1', name: 'Borrador', type: 'initial', color: '#6b7280' },
  { id: 'ws2', name: 'Pendiente de Revisión', type: 'intermediate', slaHours: 24, color: '#f59e0b' },
  { id: 'ws3', name: 'En Análisis', type: 'intermediate', slaHours: 48, color: '#3b82f6' },
  { id: 'ws4', name: 'Aprobada', type: 'final', color: '#22c55e' },
  { id: 'ws5', name: 'Rechazada', type: 'final', color: '#ef4444' },
];

const wfTransitions1: WorkflowTransition[] = [
  { id: 'wt1', fromStateId: 'ws1', toStateId: 'ws2', name: 'Enviar a revisión', autoTransition: false },
  { id: 'wt2', fromStateId: 'ws2', toStateId: 'ws3', name: 'Iniciar análisis', requiredRole: 'Operador', autoTransition: false },
  { id: 'wt3', fromStateId: 'ws3', toStateId: 'ws4', name: 'Aprobar', requiredRole: 'Admin Entidad', autoTransition: false },
  { id: 'wt4', fromStateId: 'ws3', toStateId: 'ws5', name: 'Rechazar', requiredRole: 'Admin Entidad', autoTransition: false },
  { id: 'wt5', fromStateId: 'ws2', toStateId: 'ws5', name: 'Rechazar directo', requiredRole: 'Admin Entidad', autoTransition: false },
];

const wfStates2: WorkflowState[] = [
  { id: 'ws6', name: 'Ingresada', type: 'initial', color: '#6b7280' },
  { id: 'ws7', name: 'Validación Documental', type: 'intermediate', slaHours: 12, color: '#8b5cf6' },
  { id: 'ws8', name: 'Suscripción', type: 'intermediate', slaHours: 72, color: '#3b82f6' },
  { id: 'ws9', name: 'Emitida', type: 'final', color: '#22c55e' },
  { id: 'ws10', name: 'Anulada', type: 'final', color: '#ef4444' },
];

const wfTransitions2: WorkflowTransition[] = [
  { id: 'wt6', fromStateId: 'ws6', toStateId: 'ws7', name: 'Validar documentos', autoTransition: false },
  { id: 'wt7', fromStateId: 'ws7', toStateId: 'ws8', name: 'Enviar a suscripción', requiredRole: 'Operador', autoTransition: false },
  { id: 'wt8', fromStateId: 'ws8', toStateId: 'ws9', name: 'Emitir póliza', requiredRole: 'Admin Entidad', autoTransition: false },
  { id: 'wt9', fromStateId: 'ws7', toStateId: 'ws10', name: 'Anular', autoTransition: false },
  { id: 'wt10', fromStateId: 'ws8', toStateId: 'ws10', name: 'Anular', autoTransition: false },
];

export const mockWorkflows: WorkflowDefinition[] = [
  { id: 'wf1', name: 'Solicitud Préstamo', description: 'Workflow estándar para solicitudes de préstamos', version: 2, status: 'active', states: wfStates1, transitions: wfTransitions1, productCategory: 'loan', createdAt: '2024-06-01' },
  { id: 'wf2', name: 'Solicitud Seguro', description: 'Workflow para emisión de pólizas de seguros', version: 1, status: 'active', states: wfStates2, transitions: wfTransitions2, productCategory: 'insurance', createdAt: '2025-01-15' },
];

// ── Applications ──

export const mockApplications: Application[] = [
  {
    id: 'app1', code: 'SOL-2026-001', entityId: 'e1', entityName: 'Banco del Plata',
    productId: 'prod1', productName: 'Préstamo Personal', planId: 'plan2', planName: 'Plan 24 meses',
    applicant: { firstName: 'Juan', lastName: 'Rodríguez', documentType: 'DNI', documentNumber: '35456789', email: 'jrodriguez@email.com', phoneCode: '+54', phone: '11 5555-1234', birthDate: '1988-03-15', gender: 'male', occupation: 'Ingeniero Civil' },
    beneficiaries: [],
    status: 'in_review', currentWorkflowStateId: 'ws3', currentWorkflowStateName: 'En Análisis',
    observations: [
      { id: 'obs1', userId: 'u3', userName: 'María García', text: 'Documentación completa, se envía a análisis crediticio', timestamp: '2026-03-07T11:30:00' },
    ],
    documents: [
      { id: 'doc1', name: 'DNI_frente.pdf', type: 'DNI frente y dorso', status: 'approved', uploadedAt: '2026-03-07T11:00:00' },
      { id: 'doc2', name: 'recibo_sueldo_01.pdf', type: 'Recibo de sueldo', status: 'approved', uploadedAt: '2026-03-07T11:05:00' },
    ],
    traceEvents: [
      { id: 'te1', workflowStateId: 'ws1', workflowStateName: 'Borrador', action: 'Solicitud creada', userName: 'Juan Rodríguez', timestamp: '2026-03-07T10:45:00', detail: 'Se creó la solicitud SOL-2026-001' },
      { id: 'te2', workflowStateId: 'ws2', workflowStateName: 'Pendiente de Revisión', action: 'Enviar a revisión', userName: 'Juan Rodríguez', timestamp: '2026-03-07T11:00:00', detail: 'Documentación adjuntada y enviada a revisión' },
      { id: 'te3', workflowStateId: 'ws3', workflowStateName: 'En Análisis', action: 'Iniciar análisis', userName: 'María García', timestamp: '2026-03-07T11:30:00', detail: 'Documentación completa, se inicia análisis crediticio' },
    ],
    address: {
      street: 'Av. Corrientes', number: '1234', floor: '5', apartment: 'B',
      city: 'Ciudad Autónoma de Buenos Aires', province: 'CABA', postalCode: 'C1043AAZ',
      latitude: -34.6037, longitude: -58.3816,
    },
    assignedUserId: 'u2', assignedUserName: 'Carlos Pérez',
    createdAt: '2026-03-07T10:45:00', updatedAt: '2026-03-07T11:30:00',
  },
  {
    id: 'app2', code: 'SOL-2026-002', entityId: 'e2', entityName: 'Seguros Patagonia',
    productId: 'prod2', productName: 'Seguro de Vida Individual', planId: 'plan5', planName: 'Plan Premium',
    applicant: { firstName: 'Laura', lastName: 'Gómez', documentType: 'DNI', documentNumber: '28901234', email: 'lgomez@email.com', phoneCode: '+54', phone: '11 6666-5678', birthDate: '1980-07-22', gender: 'female', occupation: 'Contadora Pública' },
    beneficiaries: [
      { firstName: 'Pedro', lastName: 'Gómez', relationship: 'Cónyuge', percentage: 60 },
      { firstName: 'Sofía', lastName: 'Gómez', relationship: 'Hija', percentage: 40 },
    ],
    status: 'pending', currentWorkflowStateId: 'ws7', currentWorkflowStateName: 'Validación Documental',
    observations: [],
    documents: [
      { id: 'doc3', name: 'DNI_laura.pdf', type: 'DNI', status: 'approved', uploadedAt: '2026-03-06T15:00:00' },
      { id: 'doc4', name: 'ddjj_salud.pdf', type: 'Declaración jurada de salud', status: 'pending', uploadedAt: '2026-03-06T15:05:00' },
    ],
    traceEvents: [
      { id: 'te4', workflowStateId: 'ws6', workflowStateName: 'Ingresada', action: 'Solicitud ingresada', userName: 'Laura Gómez', timestamp: '2026-03-06T14:50:00', detail: 'Solicitud de seguro de vida ingresada al sistema' },
      { id: 'te5', workflowStateId: 'ws7', workflowStateName: 'Validación Documental', action: 'Validar documentos', userName: 'Ana Martínez', timestamp: '2026-03-06T15:05:00', detail: 'Se inicia validación de documentación presentada' },
    ],
    address: {
      street: 'Av. del Libertador', number: '5678', floor: '12', apartment: 'A',
      city: 'Ciudad Autónoma de Buenos Aires', province: 'CABA', postalCode: 'C1428BCO',
      latitude: -34.5558, longitude: -58.4173,
    },
    assignedUserId: 'u4', assignedUserName: 'Ana Martínez',
    createdAt: '2026-03-06T14:50:00', updatedAt: '2026-03-06T15:05:00',
  },
  {
    id: 'app3', code: 'SOL-2026-003', entityId: 'e1', entityName: 'Banco del Plata',
    productId: 'prod3', productName: 'Cuenta Corriente', planId: 'plan6', planName: 'Cuenta Estándar',
    applicant: { firstName: 'Martín', lastName: 'Silva', documentType: 'DNI', documentNumber: '40123456', email: 'msilva@email.com', phoneCode: '+54', phone: '11 7777-9012', birthDate: '1995-11-08', gender: 'male', occupation: 'Desarrollador de Software' },
    beneficiaries: [],
    status: 'approved', currentWorkflowStateId: 'ws4', currentWorkflowStateName: 'Aprobada',
    observations: [
      { id: 'obs2', userId: 'u2', userName: 'Carlos Pérez', text: 'Solicitud aprobada. Cuenta lista para activación.', timestamp: '2026-03-07T14:30:00' },
    ],
    documents: [
      { id: 'doc5', name: 'DNI_martin.pdf', type: 'DNI', status: 'approved', uploadedAt: '2026-03-05T09:00:00' },
    ],
    traceEvents: [
      { id: 'te6', workflowStateId: 'ws1', workflowStateName: 'Borrador', action: 'Solicitud creada', userName: 'Martín Silva', timestamp: '2026-03-05T08:30:00', detail: 'Se creó la solicitud de cuenta corriente' },
      { id: 'te7', workflowStateId: 'ws2', workflowStateName: 'Pendiente de Revisión', action: 'Enviar a revisión', userName: 'Martín Silva', timestamp: '2026-03-05T09:00:00', detail: 'DNI adjuntado y enviado a revisión' },
      { id: 'te8', workflowStateId: 'ws3', workflowStateName: 'En Análisis', action: 'Iniciar análisis', userName: 'Carlos Pérez', timestamp: '2026-03-06T10:00:00', detail: 'Se inicia la verificación de datos del solicitante' },
      { id: 'te9', workflowStateId: 'ws4', workflowStateName: 'Aprobada', action: 'Aprobar', userName: 'Carlos Pérez', timestamp: '2026-03-07T14:30:00', detail: 'Solicitud aprobada. Cuenta lista para activación.' },
    ],
    address: {
      street: 'Bv. Oroño', number: '1500',
      city: 'Rosario', province: 'Santa Fe', postalCode: 'S2000BHF',
      latitude: -32.9468, longitude: -60.6393,
    },
    assignedUserId: 'u2', assignedUserName: 'Carlos Pérez',
    createdAt: '2026-03-05T08:30:00', updatedAt: '2026-03-07T14:30:00',
  },
  {
    id: 'app4', code: 'SOL-2026-004', entityId: 'e3', entityName: 'FinPay Digital',
    productId: 'prod4', productName: 'Tarjeta de Crédito Virtual', planId: 'plan8', planName: 'Gold',
    applicant: { firstName: 'Carolina', lastName: 'Torres', documentType: 'DNI', documentNumber: '33567890', email: 'ctorres@email.com', phoneCode: '+54', phone: '11 8888-3456', birthDate: '1990-01-30', gender: 'female', occupation: 'Diseñadora Gráfica' },
    beneficiaries: [],
    status: 'rejected', currentWorkflowStateId: 'ws5', currentWorkflowStateName: 'Rechazada',
    observations: [
      { id: 'obs3', userId: 'u6', userName: 'Jorge López', text: 'Scoring crediticio insuficiente (450 pts). Se requiere mínimo 600.', timestamp: '2026-03-04T16:00:00' },
    ],
    documents: [
      { id: 'doc6', name: 'selfie_dni.jpg', type: 'Selfie con DNI', status: 'approved', uploadedAt: '2026-03-04T10:00:00' },
    ],
    traceEvents: [
      { id: 'te10', workflowStateId: 'ws1', workflowStateName: 'Borrador', action: 'Solicitud creada', userName: 'Carolina Torres', timestamp: '2026-03-04T09:45:00', detail: 'Se creó la solicitud de tarjeta virtual' },
      { id: 'te11', workflowStateId: 'ws2', workflowStateName: 'Pendiente de Revisión', action: 'Enviar a revisión', userName: 'Carolina Torres', timestamp: '2026-03-04T10:00:00', detail: 'Selfie con DNI adjuntada' },
      { id: 'te12', workflowStateId: 'ws3', workflowStateName: 'En Análisis', action: 'Iniciar análisis', userName: 'Jorge López', timestamp: '2026-03-04T14:00:00', detail: 'Se inicia análisis crediticio automático' },
      { id: 'te13', workflowStateId: 'ws5', workflowStateName: 'Rechazada', action: 'Rechazar', userName: 'Jorge López', timestamp: '2026-03-04T16:00:00', detail: 'Scoring crediticio insuficiente (450 pts). Se requiere mínimo 600.' },
    ],
    assignedUserId: 'u6', assignedUserName: 'Jorge López',
    createdAt: '2026-03-04T09:45:00', updatedAt: '2026-03-04T16:00:00',
  },
  {
    id: 'app5', code: 'SOL-2026-005', entityId: 'e1', entityName: 'Banco del Plata',
    productId: 'prod1', productName: 'Préstamo Personal', planId: 'plan1', planName: 'Plan 12 meses',
    applicant: { firstName: 'Diego', lastName: 'Fernández', documentType: 'DNI', documentNumber: '37890123', email: 'dfernandez@email.com', phoneCode: '+54', phone: '11 9999-6789', birthDate: '1992-05-14', gender: 'male', occupation: 'Comerciante' },
    beneficiaries: [],
    status: 'draft', currentWorkflowStateId: 'ws1', currentWorkflowStateName: 'Borrador',
    observations: [],
    documents: [],
    traceEvents: [
      { id: 'te14', workflowStateId: 'ws1', workflowStateName: 'Borrador', action: 'Solicitud creada', userName: 'Diego Fernández', timestamp: '2026-03-08T08:00:00', detail: 'Se creó la solicitud de préstamo personal' },
    ],
    createdAt: '2026-03-08T08:00:00', updatedAt: '2026-03-08T08:00:00',
  },
  // ── 30 additional applications ──
  {
    id: 'app6', code: 'SOL-2026-006', entityId: 'e1', entityName: 'Banco del Plata',
    productId: 'prod1', productName: 'Préstamo Personal', planId: 'plan3', planName: 'Plan 36 meses',
    applicant: { firstName: 'Lucía', lastName: 'Méndez', documentType: 'DNI', documentNumber: '31234567', email: 'lmendez@email.com', phoneCode: '+54', phone: '11 4321-1111', birthDate: '1985-02-10', gender: 'female', occupation: 'Abogada' },
    beneficiaries: [], status: 'approved', currentWorkflowStateId: 'ws4', currentWorkflowStateName: 'Aprobada',
    observations: [], documents: [], traceEvents: [
      { id: 'te15', workflowStateId: 'ws4', workflowStateName: 'Aprobada', action: 'Aprobar', userName: 'Carlos Pérez', timestamp: '2026-03-08T10:00:00', detail: 'Aprobada tras análisis crediticio' },
    ],
    address: { street: 'Av. Santa Fe', number: '2200', city: 'CABA', province: 'CABA', postalCode: 'C1123AAZ' },
    createdAt: '2026-03-01T09:00:00', updatedAt: '2026-03-08T10:00:00',
  },
  {
    id: 'app7', code: 'SOL-2026-007', entityId: 'e2', entityName: 'Seguros Patagonia',
    productId: 'prod2', productName: 'Seguro de Vida Individual', planId: 'plan4', planName: 'Plan Básico',
    applicant: { firstName: 'Roberto', lastName: 'Acosta', documentType: 'DNI', documentNumber: '27654321', email: 'racosta@email.com', phoneCode: '+54', phone: '351 555-2222', birthDate: '1978-09-05', gender: 'male', occupation: 'Médico' },
    beneficiaries: [{ firstName: 'Elena', lastName: 'Acosta', relationship: 'Cónyuge', percentage: 100 }],
    status: 'settled', currentWorkflowStateId: 'ws9', currentWorkflowStateName: 'Emitida',
    observations: [], documents: [], traceEvents: [],
    address: { street: 'Av. Colón', number: '500', city: 'Córdoba', province: 'Córdoba', postalCode: 'X5000' },
    createdAt: '2026-01-15T14:00:00', updatedAt: '2026-02-20T16:00:00',
  },
  {
    id: 'app8', code: 'SOL-2026-008', entityId: 'e3', entityName: 'FinPay Digital',
    productId: 'prod4', productName: 'Tarjeta de Crédito Virtual', planId: 'plan7', planName: 'Classic',
    applicant: { firstName: 'Valentina', lastName: 'Ríos', documentType: 'DNI', documentNumber: '42345678', email: 'vrios@email.com', phoneCode: '+54', phone: '11 3333-4444', birthDate: '1998-12-01', gender: 'female', occupation: 'Estudiante' },
    beneficiaries: [], status: 'cancelled', currentWorkflowStateId: 'ws5', currentWorkflowStateName: 'Rechazada',
    observations: [{ id: 'obs4', userId: 'u6', userName: 'Jorge López', text: 'Cancelada por el solicitante', timestamp: '2026-03-02T10:00:00' }],
    documents: [], traceEvents: [],
    createdAt: '2026-03-01T08:00:00', updatedAt: '2026-03-02T10:00:00',
  },
  {
    id: 'app9', code: 'SOL-2026-009', entityId: 'e1', entityName: 'Banco del Plata',
    productId: 'prod1', productName: 'Préstamo Personal', planId: 'plan2', planName: 'Plan 24 meses',
    applicant: { firstName: 'Federico', lastName: 'Paz', documentType: 'CUIT', documentNumber: '20-38901234-5', email: 'fpaz@email.com', phoneCode: '+54', phone: '261 555-3333', birthDate: '1991-06-20', gender: 'male', occupation: 'Contador' },
    beneficiaries: [], status: 'pending', currentWorkflowStateId: 'ws2', currentWorkflowStateName: 'Pendiente de Revisión',
    observations: [], documents: [], traceEvents: [],
    address: { street: 'Calle San Martín', number: '890', city: 'Mendoza', province: 'Mendoza', postalCode: 'M5500' },
    createdAt: '2026-03-09T11:00:00', updatedAt: '2026-03-09T11:00:00',
  },
  {
    id: 'app10', code: 'SOL-2026-010', entityId: 'e2', entityName: 'Seguros Patagonia',
    productId: 'prod2', productName: 'Seguro de Vida Individual', planId: 'plan5', planName: 'Plan Premium',
    applicant: { firstName: 'Mariana', lastName: 'Vega', documentType: 'DNI', documentNumber: '30567890', email: 'mvega@email.com', phoneCode: '+54', phone: '11 2222-5555', birthDate: '1982-04-18', gender: 'female', occupation: 'Empresaria' },
    beneficiaries: [{ firstName: 'Tomás', lastName: 'Vega', relationship: 'Hijo', percentage: 50 }, { firstName: 'Ana', lastName: 'Vega', relationship: 'Hija', percentage: 50 }],
    status: 'in_review', currentWorkflowStateId: 'ws8', currentWorkflowStateName: 'Suscripción',
    observations: [], documents: [], traceEvents: [],
    address: { street: 'Av. Cabildo', number: '1500', city: 'CABA', province: 'CABA', postalCode: 'C1426AAZ' },
    createdAt: '2026-03-05T09:00:00', updatedAt: '2026-03-09T14:00:00',
  },
  {
    id: 'app11', code: 'SOL-2026-011', entityId: 'e1', entityName: 'Banco del Plata',
    productId: 'prod3', productName: 'Cuenta Corriente', planId: 'plan6', planName: 'Cuenta Estándar',
    applicant: { firstName: 'Andrés', lastName: 'Navarro', documentType: 'DNI', documentNumber: '36789012', email: 'anavarro@email.com', phoneCode: '+54', phone: '11 7777-8888', birthDate: '1993-08-25', gender: 'male', occupation: 'Arquitecto' },
    beneficiaries: [], status: 'approved', currentWorkflowStateId: 'ws4', currentWorkflowStateName: 'Aprobada',
    observations: [], documents: [], traceEvents: [],
    address: { street: 'Av. Rivadavia', number: '4500', city: 'CABA', province: 'CABA', postalCode: 'C1205AAZ' },
    createdAt: '2026-02-28T10:00:00', updatedAt: '2026-03-05T15:00:00',
  },
  {
    id: 'app12', code: 'SOL-2026-012', entityId: 'e3', entityName: 'FinPay Digital',
    productId: 'prod4', productName: 'Tarjeta de Crédito Virtual', planId: 'plan8', planName: 'Gold',
    applicant: { firstName: 'Camila', lastName: 'Herrera', documentType: 'DNI', documentNumber: '39012345', email: 'cherrera@email.com', phoneCode: '+54', phone: '11 6666-9999', birthDate: '1994-03-12', gender: 'female', occupation: 'Periodista' },
    beneficiaries: [], status: 'in_review', currentWorkflowStateId: 'ws3', currentWorkflowStateName: 'En Análisis',
    observations: [], documents: [], traceEvents: [],
    createdAt: '2026-03-10T08:30:00', updatedAt: '2026-03-10T14:00:00',
  },
  {
    id: 'app13', code: 'SOL-2026-013', entityId: 'e1', entityName: 'Banco del Plata',
    productId: 'prod1', productName: 'Préstamo Personal', planId: 'plan1', planName: 'Plan 12 meses',
    applicant: { firstName: 'Gabriel', lastName: 'Molina', documentType: 'DNI', documentNumber: '34567890', email: 'gmolina@email.com', phoneCode: '+54', phone: '341 555-1111', birthDate: '1987-11-30', gender: 'male', occupation: 'Profesor' },
    beneficiaries: [], status: 'settled', currentWorkflowStateId: 'ws4', currentWorkflowStateName: 'Aprobada',
    observations: [], documents: [], traceEvents: [],
    address: { street: 'Pellegrini', number: '1200', city: 'Rosario', province: 'Santa Fe', postalCode: 'S2000' },
    createdAt: '2026-01-10T10:00:00', updatedAt: '2026-02-15T12:00:00',
  },
  {
    id: 'app14', code: 'SOL-2026-014', entityId: 'e2', entityName: 'Seguros Patagonia',
    productId: 'prod2', productName: 'Seguro de Vida Individual', planId: 'plan4', planName: 'Plan Básico',
    applicant: { firstName: 'Patricia', lastName: 'Castro', documentType: 'DNI', documentNumber: '29876543', email: 'pcastro@email.com', phoneCode: '+54', phone: '11 1111-2222', birthDate: '1979-01-14', gender: 'female', occupation: 'Farmacéutica' },
    beneficiaries: [{ firstName: 'Luis', lastName: 'Castro', relationship: 'Cónyuge', percentage: 100 }],
    status: 'rejected', currentWorkflowStateId: 'ws10', currentWorkflowStateName: 'Anulada',
    observations: [{ id: 'obs5', userId: 'u4', userName: 'Ana Martínez', text: 'DDJJ de salud con inconsistencias', timestamp: '2026-03-03T11:00:00' }],
    documents: [], traceEvents: [],
    createdAt: '2026-02-25T09:00:00', updatedAt: '2026-03-03T11:00:00',
  },
  {
    id: 'app15', code: 'SOL-2026-015', entityId: 'e1', entityName: 'Banco del Plata',
    productId: 'prod1', productName: 'Préstamo Personal', planId: 'plan3', planName: 'Plan 36 meses',
    applicant: { firstName: 'Ricardo', lastName: 'Domínguez', documentType: 'CUIT', documentNumber: '20-32456789-0', email: 'rdominguez@email.com', phoneCode: '+54', phone: '381 555-4444', birthDate: '1986-07-08', gender: 'male', occupation: 'Comerciante' },
    beneficiaries: [], status: 'draft', currentWorkflowStateId: 'ws1', currentWorkflowStateName: 'Borrador',
    observations: [], documents: [], traceEvents: [],
    address: { street: 'Av. Aconquija', number: '1800', city: 'San Miguel de Tucumán', province: 'Tucumán', postalCode: 'T4000' },
    createdAt: '2026-03-11T09:00:00', updatedAt: '2026-03-11T09:00:00',
  },
  {
    id: 'app16', code: 'SOL-2026-016', entityId: 'e3', entityName: 'FinPay Digital',
    productId: 'prod4', productName: 'Tarjeta de Crédito Virtual', planId: 'plan7', planName: 'Classic',
    applicant: { firstName: 'Sofía', lastName: 'Aguirre', documentType: 'DNI', documentNumber: '41234567', email: 'saguirre@email.com', phoneCode: '+54', phone: '11 5555-6666', birthDate: '1997-10-22', gender: 'female', occupation: 'Community Manager' },
    beneficiaries: [], status: 'approved', currentWorkflowStateId: 'ws4', currentWorkflowStateName: 'Aprobada',
    observations: [], documents: [], traceEvents: [],
    createdAt: '2026-03-06T11:00:00', updatedAt: '2026-03-09T16:00:00',
  },
  {
    id: 'app17', code: 'SOL-2026-017', entityId: 'e1', entityName: 'Banco del Plata',
    productId: 'prod3', productName: 'Cuenta Corriente', planId: 'plan6', planName: 'Cuenta Estándar',
    applicant: { firstName: 'Hernán', lastName: 'Peralta', documentType: 'DNI', documentNumber: '33890123', email: 'hperalta@email.com', phoneCode: '+54', phone: '11 4444-7777', birthDate: '1989-05-03', gender: 'male', occupation: 'Ingeniero Industrial' },
    beneficiaries: [], status: 'pending', currentWorkflowStateId: 'ws2', currentWorkflowStateName: 'Pendiente de Revisión',
    observations: [], documents: [], traceEvents: [],
    address: { street: 'Mitre', number: '600', city: 'La Plata', province: 'Buenos Aires', postalCode: 'B1900' },
    createdAt: '2026-03-10T14:00:00', updatedAt: '2026-03-10T14:00:00',
  },
  {
    id: 'app18', code: 'SOL-2026-018', entityId: 'e2', entityName: 'Seguros Patagonia',
    productId: 'prod2', productName: 'Seguro de Vida Individual', planId: 'plan5', planName: 'Plan Premium',
    applicant: { firstName: 'Claudia', lastName: 'Romero', documentType: 'DNI', documentNumber: '28345678', email: 'cromero@email.com', phoneCode: '+54', phone: '11 8888-1111', birthDate: '1977-12-19', gender: 'female', occupation: 'Directora de RRHH' },
    beneficiaries: [{ firstName: 'Marcos', lastName: 'Romero', relationship: 'Hijo', percentage: 100 }],
    status: 'settled', currentWorkflowStateId: 'ws9', currentWorkflowStateName: 'Emitida',
    observations: [], documents: [], traceEvents: [],
    address: { street: 'Av. Belgrano', number: '3200', city: 'CABA', province: 'CABA', postalCode: 'C1210AAZ' },
    createdAt: '2026-01-05T10:00:00', updatedAt: '2026-02-10T11:00:00',
  },
  {
    id: 'app19', code: 'SOL-2026-019', entityId: 'e1', entityName: 'Banco del Plata',
    productId: 'prod1', productName: 'Préstamo Personal', planId: 'plan2', planName: 'Plan 24 meses',
    applicant: { firstName: 'Pablo', lastName: 'Giménez', documentType: 'DNI', documentNumber: '36123456', email: 'pgimenez@email.com', phoneCode: '+54', phone: '343 555-2222', birthDate: '1990-09-11', gender: 'male', occupation: 'Veterinario' },
    beneficiaries: [], status: 'in_review', currentWorkflowStateId: 'ws3', currentWorkflowStateName: 'En Análisis',
    observations: [], documents: [], traceEvents: [],
    address: { street: 'Urquiza', number: '700', city: 'Paraná', province: 'Entre Ríos', postalCode: 'E3100' },
    createdAt: '2026-03-08T15:00:00', updatedAt: '2026-03-10T09:00:00',
  },
  {
    id: 'app20', code: 'SOL-2026-020', entityId: 'e3', entityName: 'FinPay Digital',
    productId: 'prod4', productName: 'Tarjeta de Crédito Virtual', planId: 'plan8', planName: 'Gold',
    applicant: { firstName: 'Natalia', lastName: 'Sosa', documentType: 'DNI', documentNumber: '38567890', email: 'nsosa@email.com', phoneCode: '+54', phone: '11 9999-3333', birthDate: '1993-04-07', gender: 'female', occupation: 'Psicóloga' },
    beneficiaries: [], status: 'rejected', currentWorkflowStateId: 'ws5', currentWorkflowStateName: 'Rechazada',
    observations: [{ id: 'obs6', userId: 'u6', userName: 'Jorge López', text: 'Documentación incompleta', timestamp: '2026-03-07T16:00:00' }],
    documents: [], traceEvents: [],
    createdAt: '2026-03-06T08:00:00', updatedAt: '2026-03-07T16:00:00',
  },
  {
    id: 'app21', code: 'SOL-2026-021', entityId: 'e1', entityName: 'Banco del Plata',
    productId: 'prod1', productName: 'Préstamo Personal', planId: 'plan1', planName: 'Plan 12 meses',
    applicant: { firstName: 'Esteban', lastName: 'Luna', documentType: 'DNI', documentNumber: '35678901', email: 'eluna@email.com', phoneCode: '+54', phone: '11 3333-8888', birthDate: '1988-10-30', gender: 'male', occupation: 'Electricista' },
    beneficiaries: [], status: 'approved', currentWorkflowStateId: 'ws4', currentWorkflowStateName: 'Aprobada',
    observations: [], documents: [], traceEvents: [],
    address: { street: 'San Juan', number: '1100', city: 'CABA', province: 'CABA', postalCode: 'C1148AAZ' },
    createdAt: '2026-02-20T10:00:00', updatedAt: '2026-03-01T12:00:00',
  },
  {
    id: 'app22', code: 'SOL-2026-022', entityId: 'e2', entityName: 'Seguros Patagonia',
    productId: 'prod2', productName: 'Seguro de Vida Individual', planId: 'plan4', planName: 'Plan Básico',
    applicant: { firstName: 'Daniela', lastName: 'Morales', documentType: 'DNI', documentNumber: '32456789', email: 'dmorales@email.com', phoneCode: '+54', phone: '11 2222-4444', birthDate: '1984-06-15', gender: 'female', occupation: 'Docente' },
    beneficiaries: [{ firstName: 'Ramiro', lastName: 'Morales', relationship: 'Hijo', percentage: 100 }],
    status: 'pending', currentWorkflowStateId: 'ws7', currentWorkflowStateName: 'Validación Documental',
    observations: [], documents: [], traceEvents: [],
    address: { street: 'Maipú', number: '350', city: 'Salta', province: 'Salta', postalCode: 'A4400' },
    createdAt: '2026-03-11T08:00:00', updatedAt: '2026-03-11T10:00:00',
  },
  {
    id: 'app23', code: 'SOL-2026-023', entityId: 'e1', entityName: 'Banco del Plata',
    productId: 'prod1', productName: 'Préstamo Personal', planId: 'plan3', planName: 'Plan 36 meses',
    applicant: { firstName: 'Maximiliano', lastName: 'Ruiz', documentType: 'CUIT', documentNumber: '20-37890123-4', email: 'mruiz@email.com', phoneCode: '+54', phone: '11 1111-5555', birthDate: '1992-02-28', gender: 'male', occupation: 'Analista de Sistemas' },
    beneficiaries: [], status: 'settled', currentWorkflowStateId: 'ws4', currentWorkflowStateName: 'Aprobada',
    observations: [], documents: [], traceEvents: [],
    address: { street: 'Av. Callao', number: '800', city: 'CABA', province: 'CABA', postalCode: 'C1023AAZ' },
    createdAt: '2026-01-20T09:00:00', updatedAt: '2026-02-25T14:00:00',
  },
  {
    id: 'app24', code: 'SOL-2026-024', entityId: 'e3', entityName: 'FinPay Digital',
    productId: 'prod4', productName: 'Tarjeta de Crédito Virtual', planId: 'plan7', planName: 'Classic',
    applicant: { firstName: 'Agustina', lastName: 'Flores', documentType: 'DNI', documentNumber: '40567890', email: 'aflores@email.com', phoneCode: '+54', phone: '11 7777-2222', birthDate: '1996-08-14', gender: 'female', occupation: 'Diseñadora UX' },
    beneficiaries: [], status: 'draft', currentWorkflowStateId: 'ws1', currentWorkflowStateName: 'Borrador',
    observations: [], documents: [], traceEvents: [],
    createdAt: '2026-03-12T10:00:00', updatedAt: '2026-03-12T10:00:00',
  },
  {
    id: 'app25', code: 'SOL-2026-025', entityId: 'e1', entityName: 'Banco del Plata',
    productId: 'prod3', productName: 'Cuenta Corriente', planId: 'plan6', planName: 'Cuenta Estándar',
    applicant: { firstName: 'Ignacio', lastName: 'Bustos', documentType: 'DNI', documentNumber: '34890123', email: 'ibustos@email.com', phoneCode: '+54', phone: '351 555-6666', birthDate: '1987-03-21', gender: 'male', occupation: 'Kinesiólogo' },
    beneficiaries: [], status: 'cancelled', currentWorkflowStateId: 'ws5', currentWorkflowStateName: 'Rechazada',
    observations: [{ id: 'obs7', userId: 'u2', userName: 'Carlos Pérez', text: 'Cancelada a pedido del cliente', timestamp: '2026-03-09T15:00:00' }],
    documents: [], traceEvents: [],
    address: { street: 'Vélez Sarsfield', number: '300', city: 'Córdoba', province: 'Córdoba', postalCode: 'X5000' },
    createdAt: '2026-03-07T11:00:00', updatedAt: '2026-03-09T15:00:00',
  },
  {
    id: 'app26', code: 'SOL-2026-026', entityId: 'e2', entityName: 'Seguros Patagonia',
    productId: 'prod2', productName: 'Seguro de Vida Individual', planId: 'plan5', planName: 'Plan Premium',
    applicant: { firstName: 'Florencia', lastName: 'Ibáñez', documentType: 'DNI', documentNumber: '31890123', email: 'fibanez@email.com', phoneCode: '+54', phone: '11 6666-1111', birthDate: '1983-11-05', gender: 'female', occupation: 'Gerente Comercial' },
    beneficiaries: [{ firstName: 'Martín', lastName: 'Ibáñez', relationship: 'Cónyuge', percentage: 60 }, { firstName: 'Julia', lastName: 'Ibáñez', relationship: 'Hija', percentage: 40 }],
    status: 'approved', currentWorkflowStateId: 'ws9', currentWorkflowStateName: 'Emitida',
    observations: [], documents: [], traceEvents: [],
    address: { street: 'Av. Pueyrredón', number: '1700', city: 'CABA', province: 'CABA', postalCode: 'C1119AAZ' },
    createdAt: '2026-02-15T10:00:00', updatedAt: '2026-03-08T16:00:00',
  },
  {
    id: 'app27', code: 'SOL-2026-027', entityId: 'e1', entityName: 'Banco del Plata',
    productId: 'prod1', productName: 'Préstamo Personal', planId: 'plan2', planName: 'Plan 24 meses',
    applicant: { firstName: 'Sebastián', lastName: 'Cabrera', documentType: 'DNI', documentNumber: '37234567', email: 'scabrera@email.com', phoneCode: '+54', phone: '11 8888-5555', birthDate: '1991-01-18', gender: 'male', occupation: 'Chef' },
    beneficiaries: [], status: 'in_review', currentWorkflowStateId: 'ws3', currentWorkflowStateName: 'En Análisis',
    observations: [], documents: [], traceEvents: [],
    address: { street: 'Av. Scalabrini Ortiz', number: '2300', city: 'CABA', province: 'CABA', postalCode: 'C1425AAZ' },
    createdAt: '2026-03-09T16:00:00', updatedAt: '2026-03-11T09:00:00',
  },
  {
    id: 'app28', code: 'SOL-2026-028', entityId: 'e3', entityName: 'FinPay Digital',
    productId: 'prod4', productName: 'Tarjeta de Crédito Virtual', planId: 'plan8', planName: 'Gold',
    applicant: { firstName: 'Milagros', lastName: 'Ortega', documentType: 'DNI', documentNumber: '39890123', email: 'mortega@email.com', phoneCode: '+54', phone: '11 4444-9999', birthDate: '1995-07-25', gender: 'female', occupation: 'Abogada' },
    beneficiaries: [], status: 'settled', currentWorkflowStateId: 'ws4', currentWorkflowStateName: 'Aprobada',
    observations: [], documents: [], traceEvents: [],
    createdAt: '2026-01-25T08:00:00', updatedAt: '2026-02-28T10:00:00',
  },
  {
    id: 'app29', code: 'SOL-2026-029', entityId: 'e1', entityName: 'Banco del Plata',
    productId: 'prod1', productName: 'Préstamo Personal', planId: 'plan1', planName: 'Plan 12 meses',
    applicant: { firstName: 'Gonzalo', lastName: 'Suárez', documentType: 'DNI', documentNumber: '36456789', email: 'gsuarez@email.com', phoneCode: '+54', phone: '11 5555-8888', birthDate: '1990-04-12', gender: 'male', occupation: 'Mecánico' },
    beneficiaries: [], status: 'pending', currentWorkflowStateId: 'ws2', currentWorkflowStateName: 'Pendiente de Revisión',
    observations: [], documents: [], traceEvents: [],
    address: { street: 'Av. Juan B. Justo', number: '4200', city: 'CABA', province: 'CABA', postalCode: 'C1414AAZ' },
    createdAt: '2026-03-12T11:00:00', updatedAt: '2026-03-12T11:00:00',
  },
  {
    id: 'app30', code: 'SOL-2026-030', entityId: 'e2', entityName: 'Seguros Patagonia',
    productId: 'prod2', productName: 'Seguro de Vida Individual', planId: 'plan5', planName: 'Plan Premium',
    applicant: { firstName: 'Julieta', lastName: 'Figueroa', documentType: 'DNI', documentNumber: '33234567', email: 'jfigueroa@email.com', phoneCode: '+54', phone: '11 3333-6666', birthDate: '1986-09-30', gender: 'female', occupation: 'Arquitecta' },
    beneficiaries: [{ firstName: 'Carlos', lastName: 'Figueroa', relationship: 'Cónyuge', percentage: 100 }],
    status: 'in_review', currentWorkflowStateId: 'ws8', currentWorkflowStateName: 'Suscripción',
    observations: [], documents: [], traceEvents: [],
    address: { street: 'Av. Las Heras', number: '2800', city: 'CABA', province: 'CABA', postalCode: 'C1425AAZ' },
    createdAt: '2026-03-07T14:00:00', updatedAt: '2026-03-10T11:00:00',
  },
  {
    id: 'app31', code: 'SOL-2026-031', entityId: 'e1', entityName: 'Banco del Plata',
    productId: 'prod1', productName: 'Préstamo Personal', planId: 'plan3', planName: 'Plan 36 meses',
    applicant: { firstName: 'Ramiro', lastName: 'Medina', documentType: 'DNI', documentNumber: '35012345', email: 'rmedina@email.com', phoneCode: '+54', phone: '387 555-7777', birthDate: '1988-12-03', gender: 'male', occupation: 'Odontólogo' },
    beneficiaries: [], status: 'approved', currentWorkflowStateId: 'ws4', currentWorkflowStateName: 'Aprobada',
    observations: [], documents: [], traceEvents: [],
    address: { street: 'Caseros', number: '500', city: 'Salta', province: 'Salta', postalCode: 'A4400' },
    createdAt: '2026-02-10T09:00:00', updatedAt: '2026-03-02T14:00:00',
  },
  {
    id: 'app32', code: 'SOL-2026-032', entityId: 'e3', entityName: 'FinPay Digital',
    productId: 'prod4', productName: 'Tarjeta de Crédito Virtual', planId: 'plan7', planName: 'Classic',
    applicant: { firstName: 'Rocío', lastName: 'Escobar', documentType: 'DNI', documentNumber: '41567890', email: 'rescobar@email.com', phoneCode: '+54', phone: '11 2222-7777', birthDate: '1997-02-08', gender: 'female', occupation: 'Fotógrafa' },
    beneficiaries: [], status: 'rejected', currentWorkflowStateId: 'ws5', currentWorkflowStateName: 'Rechazada',
    observations: [{ id: 'obs8', userId: 'u6', userName: 'Jorge López', text: 'Validación biométrica fallida', timestamp: '2026-03-11T16:00:00' }],
    documents: [], traceEvents: [],
    createdAt: '2026-03-11T08:00:00', updatedAt: '2026-03-11T16:00:00',
  },
  {
    id: 'app33', code: 'SOL-2026-033', entityId: 'e1', entityName: 'Banco del Plata',
    productId: 'prod3', productName: 'Cuenta Corriente', planId: 'plan6', planName: 'Cuenta Estándar',
    applicant: { firstName: 'Nicolás', lastName: 'Vargas', documentType: 'DNI', documentNumber: '38234567', email: 'nvargas@email.com', phoneCode: '+54', phone: '11 9999-1111', birthDate: '1993-10-17', gender: 'male', occupation: 'Programador' },
    beneficiaries: [], status: 'draft', currentWorkflowStateId: 'ws1', currentWorkflowStateName: 'Borrador',
    observations: [], documents: [], traceEvents: [],
    createdAt: '2026-03-13T09:00:00', updatedAt: '2026-03-13T09:00:00',
  },
  {
    id: 'app34', code: 'SOL-2026-034', entityId: 'e2', entityName: 'Seguros Patagonia',
    productId: 'prod2', productName: 'Seguro de Vida Individual', planId: 'plan4', planName: 'Plan Básico',
    applicant: { firstName: 'Verónica', lastName: 'Campos', documentType: 'DNI', documentNumber: '30123456', email: 'vcampos@email.com', phoneCode: '+54', phone: '11 6666-3333', birthDate: '1981-05-22', gender: 'female', occupation: 'Bióloga' },
    beneficiaries: [{ firstName: 'Alberto', lastName: 'Campos', relationship: 'Cónyuge', percentage: 100 }],
    status: 'settled', currentWorkflowStateId: 'ws9', currentWorkflowStateName: 'Emitida',
    observations: [], documents: [], traceEvents: [],
    address: { street: 'Av. Córdoba', number: '3600', city: 'CABA', province: 'CABA', postalCode: 'C1188AAZ' },
    createdAt: '2025-12-15T10:00:00', updatedAt: '2026-01-30T14:00:00',
  },
  {
    id: 'app35', code: 'SOL-2026-035', entityId: 'e1', entityName: 'Banco del Plata',
    productId: 'prod1', productName: 'Préstamo Personal', planId: 'plan2', planName: 'Plan 24 meses',
    applicant: { firstName: 'Tomás', lastName: 'Delgado', documentType: 'DNI', documentNumber: '37567890', email: 'tdelgado@email.com', phoneCode: '+54', phone: '11 4444-2222', birthDate: '1991-08-09', gender: 'male', occupation: 'Martillero' },
    beneficiaries: [], status: 'cancelled', currentWorkflowStateId: 'ws5', currentWorkflowStateName: 'Rechazada',
    observations: [{ id: 'obs9', userId: 'u2', userName: 'Carlos Pérez', text: 'El solicitante desistió de la operación', timestamp: '2026-03-10T17:00:00' }],
    documents: [], traceEvents: [],
    address: { street: 'Av. Independencia', number: '2100', city: 'CABA', province: 'CABA', postalCode: 'C1225AAZ' },
    createdAt: '2026-03-08T08:00:00', updatedAt: '2026-03-10T17:00:00',
  },
];
