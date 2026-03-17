import { useState, useCallback, useSyncExternalStore } from "react";
import { mockParameters, defaultParameterGroups, Parameter, ParameterGroup } from "./parameters";
import { addAuditEntry, getAuditData, subscribeAudit } from "./auditLog";
import {
  Entity,
  User,
  Role,
  Branch,
  Product,
  ProductPlan,
  ProductRequirement,
  Application,
  WorkflowDefinition,
  Tenant,
  ProductFamily,
  CommissionPlan,
  CommissionValueType,
  SettlementHistoryEntry,
  SettlementHistoryItem,
} from "./types";
import {
  mockEntities,
  mockUsers,
  mockRoles,
  mockPermissions,
  mockProducts,
  mockApplications,
  mockWorkflows,
  mockTenants,
  mockProductFamilies,
  mockCommissionPlans,
} from "./mock";

let tenantData = [...mockTenants];
let entityData = [...mockEntities];
let userData = [...mockUsers];
let roleData = [...mockRoles];
let productData = [...mockProducts];
let productFamilyData = [...mockProductFamilies];
let commissionPlanData = [...mockCommissionPlans];
let applicationData = [...mockApplications];
let workflowData = [...mockWorkflows];

function generateId(prefix: string) {
  return `${prefix}_${Date.now()}_${Math.random().toString(36).slice(2, 7)}`;
}

// ── Input types ──

export interface TenantInput {
  name: string; identifier: string; description: string;
  status: "active" | "inactive";
  contactName: string; contactEmail: string; contactPhoneCode: string; contactPhone: string; country: string;
}

export interface EntityInput {
  tenantId: string; tenantName: string;
  name: string; identifier: string;
  type: "bank" | "insurance" | "fintech" | "cooperative";
  status: "active" | "inactive" | "suspended";
  email: string; phoneCode: string; phone: string; address: string; city: string; province: string; country: string;
  channels: ("web" | "mobile" | "api" | "presencial")[];
}

export interface BranchInput {
  name: string; code: string; address: string; city: string; province: string;
  status: "active" | "inactive" | "suspended"; manager: string; phoneCode: string; phone: string;
}

export interface UserInput {
  username: string; password: string; firstName: string; lastName: string; email: string;
  entityId: string; entityName: string; roleIds: string[]; roleNames: string[];
  status: "active" | "inactive" | "locked";
}

export interface RoleInput { name: string; description: string; permissions: string[]; }

export interface ProductInput {
  entityId: string; entityName: string; familyId: string; familyName: string;
  name: string; code: string;
  description: string; status: "draft" | "active" | "inactive" | "deprecated";
  validFrom: string; validTo?: string;
}

export interface ProductFamilyInput {
  code: string;       // max 15 chars
  description: string; // max 30 chars
}

export interface CommissionPlanInput {
  code: string;        // max 15 chars
  description: string; // max 50 chars
  valueType: CommissionValueType;
  value: number;
  maxAmount?: number;
}

export interface ApplicationInput {
  entityId: string; entityName: string; productId: string; productName: string;
  planId: string; planName: string;
  applicant: { firstName: string; lastName: string; documentType: "DNI" | "CUIT" | "passport"; documentNumber: string; email: string; phoneCode: string; phone: string; };
  status: "draft" | "pending" | "in_review" | "approved" | "rejected" | "cancelled";
  currentWorkflowStateId: string; currentWorkflowStateName: string;
  assignedUserId?: string; assignedUserName?: string;
}

// ── Tenant Store ──

export function useTenantStore() {
  const [tenants, setTenants] = useState<Tenant[]>(tenantData);
  const refresh = useCallback(() => setTenants([...tenantData]), []);
  const addTenant = useCallback((data: TenantInput) => {
    const tenant: Tenant = { ...data, id: generateId("t"), createdAt: new Date().toISOString().slice(0, 10) };
    tenantData = [tenant, ...tenantData]; refresh();
    addAuditEntry("Crear", "Crear Organización", "Organización", `Creó organización "${data.name}"`);
    return tenant;
  }, [refresh]);
  const updateTenant = useCallback((id: string, data: Partial<TenantInput>) => {
    const t = tenantData.find(t => t.id === id);
    tenantData = tenantData.map(t => t.id === id ? { ...t, ...data } : t); refresh();
    addAuditEntry("Editar", "Editar Organización", "Organización", `Editó organización "${t?.name || id}"`);
  }, [refresh]);
  const deleteTenant = useCallback((id: string) => {
    const t = tenantData.find(t => t.id === id);
    tenantData = tenantData.filter(t => t.id !== id); refresh();
    addAuditEntry("Eliminar", "Eliminar Organización", "Organización", `Eliminó organización "${t?.name || id}"`);
  }, [refresh]);
  return { tenants, addTenant, updateTenant, deleteTenant };
}

// ── Entity Store ──

export function useEntityStore() {
  const [entities, setEntities] = useState<Entity[]>(entityData);
  const refresh = useCallback(() => setEntities([...entityData]), []);

  const addEntity = useCallback((data: EntityInput) => {
    const entity: Entity = { ...data, id: generateId("e"), createdAt: new Date().toISOString().slice(0, 10), branches: [] };
    entityData = [entity, ...entityData]; refresh();
    addAuditEntry("Crear", "Crear Entidad", "Organización", `Creó entidad "${data.name}"`);
    return entity;
  }, [refresh]);
  const updateEntity = useCallback((id: string, data: Partial<EntityInput>) => {
    const e = entityData.find(e => e.id === id);
    entityData = entityData.map(e => e.id === id ? { ...e, ...data } : e); refresh();
    addAuditEntry("Editar", "Editar Entidad", "Organización", `Editó entidad "${e?.name || id}"`);
  }, [refresh]);
  const deleteEntity = useCallback((id: string) => {
    const e = entityData.find(e => e.id === id);
    entityData = entityData.filter(e => e.id !== id); refresh();
    addAuditEntry("Eliminar", "Eliminar Entidad", "Organización", `Eliminó entidad "${e?.name || id}"`);
  }, [refresh]);

  const addBranch = useCallback((entityId: string, data: BranchInput) => {
    const branch: Branch = { ...data, id: generateId("b"), entityId };
    entityData = entityData.map(e => e.id === entityId ? { ...e, branches: [...e.branches, branch] } : e); refresh();
    addAuditEntry("Crear", "Crear Sucursal", "Organización", `Creó sucursal "${data.name}"`);
    return branch;
  }, [refresh]);
  const updateBranch = useCallback((entityId: string, branchId: string, data: Partial<BranchInput>) => {
    entityData = entityData.map(e => e.id === entityId ? { ...e, branches: e.branches.map(b => b.id === branchId ? { ...b, ...data } : b) } : e); refresh();
    addAuditEntry("Editar", "Editar Sucursal", "Organización", `Editó sucursal "${data.name || branchId}"`);
  }, [refresh]);
  const deleteBranch = useCallback((entityId: string, branchId: string) => {
    const ent = entityData.find(e => e.id === entityId);
    const br = ent?.branches.find(b => b.id === branchId);
    entityData = entityData.map(e => e.id === entityId ? { ...e, branches: e.branches.filter(b => b.id !== branchId) } : e); refresh();
    addAuditEntry("Eliminar", "Eliminar Sucursal", "Organización", `Eliminó sucursal "${br?.name || branchId}"`);
  }, [refresh]);

  return { entities, addEntity, updateEntity, deleteEntity, addBranch, updateBranch, deleteBranch };
}

// ── User Store ──

export function useUserStore() {
  const [users, setUsers] = useState<User[]>(userData);
  const refresh = useCallback(() => setUsers([...userData]), []);
  const addUser = useCallback((data: UserInput) => {
    const user: User = { ...data, id: generateId("u"), createdAt: new Date().toISOString().slice(0, 10), lastLogin: new Date().toISOString() };
    userData = [user, ...userData]; refresh();
    addAuditEntry("Crear", "Crear Usuario", "Seguridad", `Creó usuario "${data.firstName} ${data.lastName}" (${data.email})`);
    return user;
  }, [refresh]);
  const updateUser = useCallback((id: string, data: Partial<UserInput>) => {
    const u = userData.find(u => u.id === id);
    userData = userData.map(u => u.id === id ? { ...u, ...data } : u); refresh();
    addAuditEntry("Editar", "Editar Usuario", "Seguridad", `Editó usuario "${u?.firstName} ${u?.lastName}"`);
  }, [refresh]);
  const deleteUser = useCallback((id: string) => {
    const u = userData.find(u => u.id === id);
    userData = userData.filter(u => u.id !== id); refresh();
    addAuditEntry("Eliminar", "Eliminar Usuario", "Seguridad", `Eliminó usuario "${u?.firstName} ${u?.lastName}"`);
  }, [refresh]);
  return { users, addUser, updateUser, deleteUser };
}

// ── Role Store ──

export function useRoleStore() {
  const [roles, setRoles] = useState<Role[]>(roleData);
  const refresh = useCallback(() => setRoles([...roleData]), []);
  const addRole = useCallback((data: RoleInput) => {
    const role: Role = { ...data, id: generateId("r"), userCount: 0 };
    roleData = [role, ...roleData]; refresh();
    addAuditEntry("Crear", "Crear Rol", "Seguridad", `Creó rol "${data.name}"`);
    return role;
  }, [refresh]);
  const updateRole = useCallback((id: string, data: Partial<RoleInput>) => {
    const r = roleData.find(r => r.id === id);
    roleData = roleData.map(r => r.id === id ? { ...r, ...data } : r); refresh();
    addAuditEntry("Editar", "Editar Rol", "Seguridad", `Editó rol "${r?.name || id}"`);
  }, [refresh]);
  const deleteRole = useCallback((id: string) => {
    const r = roleData.find(r => r.id === id);
    roleData = roleData.filter(r => r.id !== id); refresh();
    addAuditEntry("Eliminar", "Eliminar Rol", "Seguridad", `Eliminó rol "${r?.name || id}"`);
  }, [refresh]);
  return { roles, addRole, updateRole, deleteRole };
}

// ── Product Store ──

export function useProductStore() {
  const [products, setProducts] = useState<Product[]>(productData);
  const refresh = useCallback(() => setProducts([...productData]), []);
  const addProduct = useCallback((data: ProductInput) => {
    const product: Product = { ...data, id: generateId("prod"), version: 1, createdAt: new Date().toISOString().slice(0, 10), plans: [], requirements: [] };
    productData = [product, ...productData]; refresh();
    addAuditEntry("Crear", "Crear Producto", "Catálogo", `Creó producto "${data.name}" (${data.code})`);
    return product;
  }, [refresh]);
  const updateProduct = useCallback((id: string, data: Partial<ProductInput>) => {
    const p = productData.find(p => p.id === id);
    productData = productData.map(p => p.id === id ? { ...p, ...data } : p); refresh();
    addAuditEntry("Editar", "Editar Producto", "Catálogo", `Editó producto "${p?.name || id}"`);
  }, [refresh]);
  const deleteProduct = useCallback((id: string) => {
    const p = productData.find(p => p.id === id);
    productData = productData.filter(p => p.id !== id); refresh();
    addAuditEntry("Eliminar", "Eliminar Producto", "Catálogo", `Eliminó producto "${p?.name || id}"`);
  }, [refresh]);

  const addPlan = useCallback((productId: string, data: Omit<ProductPlan, 'id' | 'productId'>) => {
    const plan: ProductPlan = { ...data, id: generateId("plan"), productId };
    productData = productData.map(p => p.id === productId ? { ...p, plans: [...p.plans, plan] } : p); refresh();
    addAuditEntry("Crear", "Crear Subproducto", "Catálogo", `Creó subproducto "${data.name}"`);
    return plan;
  }, [refresh]);
  const updatePlan = useCallback((productId: string, planId: string, data: Partial<Omit<ProductPlan, 'id' | 'productId'>>) => {
    productData = productData.map(p => p.id === productId ? { ...p, plans: p.plans.map(pl => pl.id === planId ? { ...pl, ...data } : pl) } : p); refresh();
    addAuditEntry("Editar", "Editar Subproducto", "Catálogo", `Editó subproducto "${data.name || planId}"`);
  }, [refresh]);
  const deletePlan = useCallback((productId: string, planId: string) => {
    const prod = productData.find(p => p.id === productId);
    const pl = prod?.plans.find(p => p.id === planId);
    productData = productData.map(p => p.id === productId ? { ...p, plans: p.plans.filter(pl => pl.id !== planId) } : p); refresh();
    addAuditEntry("Eliminar", "Eliminar Subproducto", "Catálogo", `Eliminó subproducto "${pl?.name || planId}"`);
  }, [refresh]);

  const addRequirement = useCallback((productId: string, data: Omit<ProductRequirement, 'id'>) => {
    const req: ProductRequirement = { ...data, id: generateId("req") };
    productData = productData.map(p => p.id === productId ? { ...p, requirements: [...p.requirements, req] } : p); refresh();
    addAuditEntry("Crear", "Crear Requisito", "Catálogo", `Creó requisito "${data.name}"`);
    return req;
  }, [refresh]);
  const updateRequirement = useCallback((productId: string, reqId: string, data: Partial<Omit<ProductRequirement, 'id'>>) => {
    productData = productData.map(p => p.id === productId ? { ...p, requirements: p.requirements.map(r => r.id === reqId ? { ...r, ...data } : r) } : p); refresh();
    addAuditEntry("Editar", "Editar Requisito", "Catálogo", `Editó requisito "${data.name || reqId}"`);
  }, [refresh]);
  const deleteRequirement = useCallback((productId: string, reqId: string) => {
    let reqName = reqId;
    for (const p of productData) { const r = p.requirements.find(r => r.id === reqId); if (r) { reqName = r.name; break; } }
    productData = productData.map(p => p.id === productId ? { ...p, requirements: p.requirements.filter(r => r.id !== reqId) } : p); refresh();
    addAuditEntry("Eliminar", "Eliminar Requisito", "Catálogo", `Eliminó requisito "${reqName}"`);
  }, [refresh]);

  return { products, addProduct, updateProduct, deleteProduct, addPlan, updatePlan, deletePlan, addRequirement, updateRequirement, deleteRequirement };
}

// ── Product Family Store ──

export function useProductFamilyStore() {
  const [families, setFamilies] = useState<ProductFamily[]>(productFamilyData);
  const refresh = useCallback(() => setFamilies([...productFamilyData]), []);
  const addFamily = useCallback((data: ProductFamilyInput) => {
    const family: ProductFamily = { ...data, id: generateId("fam") };
    productFamilyData = [family, ...productFamilyData]; refresh();
    addAuditEntry("Crear", "Crear Familia", "Catálogo", `Creó familia de productos "${data.description}" (${data.code})`);
    return family;
  }, [refresh]);
  const updateFamily = useCallback((id: string, data: Partial<ProductFamilyInput>) => {
    const f = productFamilyData.find(f => f.id === id);
    productFamilyData = productFamilyData.map(f => f.id === id ? { ...f, ...data } : f); refresh();
    addAuditEntry("Editar", "Editar Familia", "Catálogo", `Editó familia "${f?.description || id}"`);
  }, [refresh]);
  const deleteFamily = useCallback((id: string) => {
    const f = productFamilyData.find(f => f.id === id);
    productFamilyData = productFamilyData.filter(f => f.id !== id); refresh();
    addAuditEntry("Eliminar", "Eliminar Familia", "Catálogo", `Eliminó familia "${f?.description || id}"`);
  }, [refresh]);
  return { families, addFamily, updateFamily, deleteFamily };
}

// ── Commission Plan Store ──

export function useCommissionPlanStore() {
  const [commissionPlans, setCommissionPlans] = useState<CommissionPlan[]>(commissionPlanData);
  const refresh = useCallback(() => setCommissionPlans([...commissionPlanData]), []);

  const addCommissionPlan = useCallback((data: CommissionPlanInput) => {
    const plan: CommissionPlan = { ...data, id: generateId("cp") };
    commissionPlanData = [plan, ...commissionPlanData]; refresh();
    addAuditEntry("Crear", "Crear Plan Comisión", "Configuración", `Creó plan de comisión "${data.description}" (${data.code})`);
    return plan;
  }, [refresh]);

  const updateCommissionPlan = useCallback((id: string, data: Partial<CommissionPlanInput>) => {
    const cp = commissionPlanData.find(p => p.id === id);
    commissionPlanData = commissionPlanData.map(p => p.id === id ? { ...p, ...data } : p); refresh();
    addAuditEntry("Editar", "Editar Plan Comisión", "Configuración", `Editó plan de comisión "${cp?.description || id}"`);
  }, [refresh]);

  const deleteCommissionPlan = useCallback((id: string) => {
    const cp = commissionPlanData.find(p => p.id === id);
    commissionPlanData = commissionPlanData.filter(p => p.id !== id); refresh();
    addAuditEntry("Eliminar", "Eliminar Plan Comisión", "Configuración", `Eliminó plan de comisión "${cp?.description || id}"`);
  }, [refresh]);

  return { commissionPlans, addCommissionPlan, updateCommissionPlan, deleteCommissionPlan };
}

// ── Application Store ──

export function useApplicationStore() {
  const [applications, setApplications] = useState<Application[]>(applicationData);
  const refresh = useCallback(() => setApplications([...applicationData]), []);
  const addApplication = useCallback((data: ApplicationInput) => {
    const app: Application = {
      ...data, id: generateId("app"), code: `SOL-${new Date().getFullYear()}-${String(applicationData.length + 1).padStart(3, '0')}`,
      beneficiaries: [], observations: [], documents: [], traceEvents: [],
      createdAt: new Date().toISOString(), updatedAt: new Date().toISOString(),
    };
    applicationData = [app, ...applicationData]; refresh();
    addAuditEntry("Crear", "Crear Solicitud", "Solicitudes", `Creó solicitud ${app.code} para ${data.productName}`);
    return app;
  }, [refresh]);
  const updateApplication = useCallback((id: string, data: Partial<Application>) => {
    const app = applicationData.find(a => a.id === id);
    const statusChanged = data.status && data.status !== app?.status;
    applicationData = applicationData.map(a => a.id === id ? { ...a, ...data, updatedAt: new Date().toISOString() } : a); refresh();
    if (statusChanged) {
      const statusLabels: Record<string, string> = { draft: "Borrador", pending: "Pendiente", in_review: "En Revisión", approved: "Aprobada", rejected: "Rechazada", cancelled: "Cancelada", settled: "Liquidada" };
      addAuditEntry("Cambiar Estado", "Cambiar Estado Solicitud", "Solicitudes", `Cambió estado de ${app?.code || id} a "${statusLabels[data.status!] || data.status}"`);
    } else {
      addAuditEntry("Editar", "Editar Solicitud", "Solicitudes", `Editó solicitud ${app?.code || id}`);
    }
  }, [refresh]);
  const deleteApplication = useCallback((id: string) => {
    const app = applicationData.find(a => a.id === id);
    applicationData = applicationData.filter(a => a.id !== id); refresh();
    addAuditEntry("Eliminar", "Eliminar Solicitud", "Solicitudes", `Eliminó solicitud ${app?.code || id}`);
  }, [refresh]);
  return { applications, addApplication, updateApplication, deleteApplication };
}

// ── Workflow Store ──

export function useWorkflowStore() {
  const [workflows, setWorkflows] = useState<WorkflowDefinition[]>(workflowData);
  const refresh = useCallback(() => setWorkflows([...workflowData]), []);
  const addWorkflow = useCallback((data: Omit<WorkflowDefinition, 'id' | 'createdAt'>) => {
    const wf: WorkflowDefinition = { ...data, id: generateId("wf"), createdAt: new Date().toISOString().slice(0, 10) };
    workflowData = [wf, ...workflowData]; refresh();
    addAuditEntry("Crear", "Crear Workflow", "Configuración", `Creó workflow "${data.name}"`);
    return wf;
  }, [refresh]);
  const updateWorkflow = useCallback((id: string, data: Partial<WorkflowDefinition>) => {
    const wf = workflowData.find(w => w.id === id);
    workflowData = workflowData.map(w => w.id === id ? { ...w, ...data } : w); refresh();
    addAuditEntry("Editar", "Editar Workflow", "Configuración", `Editó workflow "${wf?.name || id}"`);
  }, [refresh]);
  const deleteWorkflow = useCallback((id: string) => {
    const wf = workflowData.find(w => w.id === id);
    workflowData = workflowData.filter(w => w.id !== id); refresh();
    addAuditEntry("Eliminar", "Eliminar Workflow", "Configuración", `Eliminó workflow "${wf?.name || id}"`);
  }, [refresh]);
  return { workflows, addWorkflow, updateWorkflow, deleteWorkflow };
}

// ── Parameter Store ──

let parameterData = [...mockParameters];
let parameterGroupData = [...defaultParameterGroups];

export function useParameterStore() {
  const [parameters, setParameters] = useState<Parameter[]>(parameterData);
  const [groups, setGroups] = useState<ParameterGroup[]>(parameterGroupData);
  const refreshParams = useCallback(() => setParameters([...parameterData]), []);
  const refreshGroups = useCallback(() => setGroups([...parameterGroupData]), []);

  const updateParameter = useCallback((id: string, data: Partial<Parameter>) => {
    const p = parameterData.find(p => p.id === id);
    parameterData = parameterData.map(p => p.id === id ? { ...p, ...data } : p); refreshParams();
    addAuditEntry("Editar", "Editar Parámetro", "Configuración", `Editó parámetro "${p?.value || id}"`);
  }, [refreshParams]);

  const addParameter = useCallback((data: Omit<Parameter, 'id'>) => {
    const param: Parameter = { ...data, id: `param_${Date.now()}_${Math.random().toString(36).slice(2, 5)}` };
    parameterData = [...parameterData, param]; refreshParams();
    addAuditEntry("Crear", "Crear Parámetro", "Configuración", `Creó parámetro "${data.value}" en grupo "${data.group}"`);
    return param;
  }, [refreshParams]);

  const deleteParameter = useCallback((id: string) => {
    const p = parameterData.find(p => p.id === id);
    parameterData = parameterData.filter(p => p.id !== id); refreshParams();
    addAuditEntry("Eliminar", "Eliminar Parámetro", "Configuración", `Eliminó parámetro "${p?.value || id}"`);
  }, [refreshParams]);

  const addGroup = useCallback((data: ParameterGroup) => {
    parameterGroupData = [...parameterGroupData, data]; refreshGroups();
    addAuditEntry("Crear", "Crear Grupo Parámetros", "Configuración", `Creó grupo de parámetros "${data.label}"`);
  }, [refreshGroups]);

  const deleteGroup = useCallback((id: string) => {
    const g = parameterGroupData.find(g => g.id === id);
    parameterGroupData = parameterGroupData.filter(g => g.id !== id); refreshGroups();
    addAuditEntry("Eliminar", "Eliminar Grupo Parámetros", "Configuración", `Eliminó grupo "${g?.label || id}"`);
  }, [refreshGroups]);

  /** Returns the parameter values (descriptions) for a given group id */
  const getGroupValues = useCallback((groupId: string): { code: string; label: string }[] => {
    return parameterData
      .filter(p => p.group === groupId)
      .map(p => ({ code: p.value, label: p.value }));
  }, []);

  return { parameters, groups, updateParameter, addParameter, deleteParameter, addGroup, deleteGroup, getGroupValues };
}

// ── Settlement History Store ──

let settlementHistoryData: SettlementHistoryEntry[] = [];

export function useSettlementHistoryStore() {
  const [entries, setEntries] = useState<SettlementHistoryEntry[]>(settlementHistoryData);
  const refresh = useCallback(() => setEntries([...settlementHistoryData]), []);

  const addEntry = useCallback((data: Omit<SettlementHistoryEntry, 'id'>) => {
    const entry: SettlementHistoryEntry = { ...data, id: `sh_${Date.now()}_${Math.random().toString(36).slice(2, 7)}` };
    settlementHistoryData = [entry, ...settlementHistoryData]; refresh();
    const totalsStr = data.totalsByCurrency.map(t => `${t.currency === "USD" ? "US$" : "$"} ${t.total.toLocaleString("es-AR", { minimumFractionDigits: 2 })} ${t.currency}`).join(", ");
    addAuditEntry("Liquidar", "Liquidar Comisiones", "Operaciones", `Liquidó ${data.operationCount} operación(es) por ${totalsStr}`);
    return entry;
  }, [refresh]);

  return { entries, addEntry };
}

// ── Audit Store (reactive) ──

export function useAuditStore() {
  const data = useSyncExternalStore(subscribeAudit, getAuditData, getAuditData);
  return { auditLog: data };
}

export { mockPermissions, addAuditEntry };
