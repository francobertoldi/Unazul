# Ingeniería Reversa — Frontend Unazul Backoffice

> Documento generado por análisis estático del código fuente en `Frontend/src`.
> Fecha: 2026-03-15

---

## 1. Stack Tecnológico

| Capa | Tecnología | Versión |
|------|-----------|---------|
| Framework UI | React | 18.3 |
| Lenguaje | TypeScript | 5.8 |
| Build tool | Vite + SWC | 5.4 |
| Routing | React Router DOM | 6.30 |
| Estado | Custom hooks + módulo-nivel (no Redux, no Zustand) | — |
| Server state | TanStack React Query | 5.83 (instalado pero no usado actualmente) |
| Formularios | React Hook Form + Zod | 7.61 / 3.25 |
| UI Components | shadcn/ui (Radix UI) + Tailwind CSS | — |
| Graficos | Recharts | 2.15 |
| Workflow editor | @xyflow/react (React Flow) | 12.10 |
| Notificaciones | Sonner | 1.7 |
| Exportacion Excel | xlsx (SheetJS) | 0.18 |
| Testing | Vitest + Testing Library | — |
| Generado con | Lovable (IA-assisted) | — |

### Patron de estado actual (mock)
El frontend **no hace llamadas HTTP reales**. Todo el estado vive en variables de módulo (arrays JS mutables) que se inicializan desde `data/mock.ts`. Cada store es un custom hook que lee/escribe esas variables y fuerza re-render via `useState`.

**Implicacion para el backend**: el backend debe implementar exactamente las operaciones CRUD que cada store expone, con los mismos contratos de datos.

---

## 2. Estructura de Directorios

```
src/
├── App.tsx                  # Router raíz + providers
├── main.tsx                 # Entry point
├── data/
│   ├── types.ts             # TODOS los tipos/interfaces del dominio
│   ├── mock.ts              # Datos de prueba iniciales
│   ├── store.ts             # Custom hooks de estado (un hook por entidad)
│   ├── authStore.ts         # Store de autenticación
│   ├── auditLog.ts          # Store reactivo de auditoría
│   ├── parameters.ts        # Tipos y mock de parámetros del sistema
│   ├── services.ts          # Tipos y mock de servicios externos
│   └── workflowNodes.ts     # Tipos de nodos del editor visual
├── pages/
│   ├── auth/                # Login, recuperar clave, cambiar contraseña
│   ├── tenants/             # CRUD Organizaciones
│   ├── entities/            # CRUD Entidades + Sucursales
│   ├── security/            # CRUD Usuarios, Roles, Permisos
│   ├── products/            # CRUD Familias + Productos + SubProductos
│   ├── applications/        # CRUD Solicitudes
│   ├── workflows/           # Lista + Editor visual de Workflows
│   ├── commissions/         # Liquidación + Historial
│   ├── config/              # Parámetros + Servicios + Planes Comisiones
│   ├── organization/        # Árbol jerárquico
│   ├── Dashboard.tsx
│   └── AuditPage.tsx
├── components/
│   ├── layout/              # AppLayout, AppSidebar
│   ├── shared/              # DataTable, MetricCard, StatusBadge, PhoneCodeSelect
│   ├── applications/        # SendMessageDialog
│   ├── entities/            # EntityProductsTab
│   ├── parameters/          # HtmlParamEditor, ListParamEditor
│   ├── workflow/            # FlowNode, NodeConfigDialog
│   └── ui/                  # shadcn/ui (accordion, button, card, etc.)
└── hooks/
    ├── use-mobile.tsx
    └── use-toast.ts
```

---

## 3. Rutas de Navegación

### Rutas públicas
| Ruta | Componente | Descripcion |
|------|-----------|-------------|
| `/login` | `LoginPage` | Autenticacion con usuario/contraseña |
| `/recuperar-clave` | `ForgotPasswordPage` | Solicitud de reset por email |

### Rutas protegidas (requieren `isAuthenticated`)

#### General
| Ruta | Componente | Descripcion |
|------|-----------|-------------|
| `/` | `Dashboard` | Métricas generales y gráficos |
| `/cambiar-clave` | `ChangePasswordPage` | Cambio de contraseña con OTP SMS |

#### Organización
| Ruta | Componente | Descripcion |
|------|-----------|-------------|
| `/tenants` | `TenantList` | Listado de organizaciones |
| `/tenants/nuevo` | `TenantForm` | Alta de organización |
| `/tenants/:id` | `TenantDetail` | Detalle de organización |
| `/tenants/:id/editar` | `TenantForm` | Edición de organización |
| `/estructura` | `OrgTreePage` | Árbol jerárquico Org > Entidad > Sucursal |
| `/entidades` | `EntityList` | Listado de entidades |
| `/entidades/nuevo` | `EntityForm` | Alta de entidad |
| `/entidades/:id` | `EntityDetail` | Detalle de entidad |
| `/entidades/:id/editar` | `EntityForm` | Edición de entidad |
| `/entidades/:entityId/sucursales/nuevo` | `BranchForm` | Alta de sucursal |
| `/entidades/:entityId/sucursales/:branchId/editar` | `BranchForm` | Edición de sucursal |

#### Seguridad
| Ruta | Componente | Descripcion |
|------|-----------|-------------|
| `/usuarios` | `UserList` | Listado de usuarios |
| `/usuarios/nuevo` | `UserForm` | Alta de usuario |
| `/usuarios/:id` | `UserDetail` | Detalle de usuario |
| `/usuarios/:id/editar` | `UserForm` | Edición de usuario |
| `/roles` | `RolesPage` | Listado de roles |
| `/roles/nuevo` | `RoleForm` | Alta de rol |
| `/roles/:id/editar` | `RoleForm` | Edición de rol |

#### Catálogo
| Ruta | Componente | Descripcion |
|------|-----------|-------------|
| `/familias-productos` | `ProductFamilyList` | Listado de familias de productos |
| `/productos` | `ProductList` | Listado de productos |
| `/productos/nuevo` | `ProductForm` | Alta de producto (con subproductos y requisitos inline) |
| `/productos/:id` | `ProductDetail` | Detalle de producto |
| `/productos/:id/editar` | `ProductForm` | Edición de producto |

#### Solicitudes
| Ruta | Componente | Descripcion |
|------|-----------|-------------|
| `/solicitudes` | `ApplicationList` | Listado de solicitudes |
| `/solicitudes/nuevo` | `ApplicationForm` | Alta de solicitud |
| `/solicitudes/:id` | `ApplicationDetail` | Detalle con tabs (solicitante, producto, domicilio, beneficiarios, documentos, observaciones, trazabilidad) |
| `/solicitudes/:id/editar` | `ApplicationForm` | Edición de solicitud |

#### Liquidación de Comisiones
| Ruta | Componente | Descripcion |
|------|-----------|-------------|
| `/liquidacion-comisiones` | `CommissionSettlementList` | Lista de solicitudes con cálculo de comisiones; permite seleccionar y liquidar (genera Excel) |
| `/liquidacion-comisiones/:id` | `CommissionSettlementDetail` | Detalle del cálculo de comisión de una solicitud |
| `/historial-liquidaciones` | `SettlementHistoryList` | Historial de liquidaciones con descarga de Excel |

#### Configuración
| Ruta | Componente | Descripcion |
|------|-----------|-------------|
| `/workflows` | `WorkflowList` | Listado de definiciones de workflow |
| `/workflows/nuevo` | `WorkflowEditor` | Editor visual (React Flow) de workflow |
| `/workflows/:id/editar` | `WorkflowEditor` | Edición de workflow existente |
| `/parametros` | `ParametersPage` | ABM de parámetros del sistema por grupos |
| `/servicios` | `ServicesPage` | ABM de servicios externos (APIs) |
| `/planes-comisiones` | `CommissionPlanList` | ABM de planes de comisiones |

#### Auditoría
| Ruta | Componente | Descripcion |
|------|-----------|-------------|
| `/auditoria` | `AuditPage` | Log de auditoría filtrable por tipo, módulo, usuario, fecha |

---

## 4. Modelos de Datos (Interfaces TypeScript)

> Archivo fuente: `src/data/types.ts`

### 4.1 Tipos base (enums)

```typescript
EntityStatus    = 'active' | 'inactive' | 'suspended'
UserStatus      = 'active' | 'inactive' | 'locked'
ChannelType     = 'web' | 'mobile' | 'api' | 'presencial'
ProductStatus   = 'draft' | 'active' | 'inactive' | 'deprecated'
ApplicationStatus = 'draft' | 'pending' | 'in_review' | 'approved' | 'rejected' | 'cancelled' | 'settled'
WorkflowStatus  = 'draft' | 'active' | 'inactive'
TenantStatus    = 'active' | 'inactive'
AmortizationType = 'french' | 'german' | 'american' | 'bullet'
CommissionValueType = 'fixed_per_sale' | 'percentage_capital' | 'percentage_total_loan'
Gender          = 'male' | 'female' | 'other' | 'not_specified'
RiskLevel       = 'low' | 'medium' | 'high'
InstrumentType  = 'fixed_term' | 'bond' | 'mutual_fund' | 'stock'
CardNetwork     = 'visa' | 'mastercard' | 'amex' | 'cabal' | 'naranja'
AuditOperationType = 'Crear' | 'Editar' | 'Eliminar' | 'Login' | 'Logout' |
                     'Cambiar Contraseña' | 'Cambiar Estado' | 'Liquidar' |
                     'Exportar' | 'Consultar' | 'Otro'
```

### 4.2 Tenant (Organización)

```typescript
interface Tenant {
  id: string;
  name: string;
  identifier: string;       // CUIT, mín 5 máx 20 chars
  description: string;      // máx 500 chars
  status: TenantStatus;     // 'active' | 'inactive'
  contactName: string;
  contactEmail: string;
  contactPhoneCode: string; // ej: "+54"
  contactPhone: string;
  country: string;
  createdAt: string;        // ISO date string YYYY-MM-DD
}
```

**Input para crear/editar** (sin `id` ni `createdAt`):
```typescript
interface TenantInput {
  name: string;             // mín 2, máx 100
  identifier: string;       // mín 5, máx 20
  description: string;      // máx 500
  status: 'active' | 'inactive';
  contactName: string;      // mín 2, máx 100
  contactEmail: string;     // email válido, máx 255
  contactPhoneCode: string;
  contactPhone: string;     // mín 5, máx 30
  country: string;          // mín 2, máx 100
}
```

### 4.3 Entity (Entidad financiera)

```typescript
interface Entity {
  id: string;
  tenantId: string;
  tenantName: string;
  name: string;
  identifier: string;       // CUIT
  type: 'bank' | 'insurance' | 'fintech' | 'cooperative';
  status: EntityStatus;
  email: string;
  phoneCode: string;
  phone: string;
  address: string;
  city: string;
  province: string;
  country: string;
  createdAt: string;
  branches: Branch[];       // embedded
  channels: ChannelType[];  // canales habilitados
}
```

**Input** (sin `id`, `createdAt`, `branches`):
```typescript
interface EntityInput {
  tenantId: string;
  tenantName: string;
  name: string;
  identifier: string;
  type: 'bank' | 'insurance' | 'fintech' | 'cooperative';
  status: EntityStatus;
  email: string;
  phoneCode: string;
  phone: string;
  address: string;
  city: string;
  province: string;
  country: string;
  channels: ChannelType[];
}
```

### 4.4 Branch (Sucursal)

```typescript
interface Branch {
  id: string;
  entityId: string;
  name: string;
  code: string;
  address: string;
  city: string;
  province: string;
  status: EntityStatus;
  manager: string;
  phoneCode: string;
  phone: string;
}
```

**Input** (sin `id`, `entityId`):
```typescript
interface BranchInput {
  name: string;
  code: string;
  address: string;
  city: string;
  province: string;
  status: EntityStatus;
  manager: string;
  phoneCode: string;
  phone: string;
}
```

### 4.5 User (Usuario)

```typescript
interface User {
  id: string;
  username: string;
  password: string;          // hash en backend
  email: string;
  firstName: string;
  lastName: string;
  entityId: string;          // "" = usuario de plataforma
  entityName: string;
  roleIds: string[];
  roleNames: string[];
  status: UserStatus;
  lastLogin: string;         // ISO datetime
  createdAt: string;         // YYYY-MM-DD
  avatar?: string;
  assignments?: UserAssignment;
}

interface UserAssignment {
  organizationIds: string[];
  entityIds: string[];
  branchIds: string[];
}
```

**Input**:
```typescript
interface UserInput {
  username: string;          // mín 3, máx 30, regex configurable
  password: string;          // mín 8, regex configurable
  firstName: string;         // mín 2, máx 50
  lastName: string;          // mín 2, máx 50
  email: string;             // máx 255
  entityId: string;          // "" para plataforma
  entityName: string;
  roleIds: string[];         // mínimo 1
  roleNames: string[];
  status: UserStatus;
}
```

### 4.6 Role y Permission

```typescript
interface Role {
  id: string;
  name: string;              // mín 2, máx 50
  description: string;       // mín 3, máx 200
  permissions: string[];     // array de Permission.id
  userCount: number;
}

interface Permission {
  id: string;
  module: string;
  action: string;
  description: string;
}
```

**Módulos de permisos definidos** (hardcoded en mock):
Organizaciones, Entidades, Sucursales, Solicitudes, Familias de Productos, Productos,
Sub Productos, Requisitos, Planes de Comisiones, Workflows, Usuarios, Roles,
Auditoría, Parámetros, Servicios, Liquidación de Comisiones.

### 4.7 ProductFamily (Familia de Producto)

```typescript
interface ProductFamily {
  id: string;
  code: string;              // máx 15 chars
  description: string;       // máx 30 chars
}
```

### 4.8 CommissionPlan (Plan de Comisiones)

```typescript
type CommissionPlan =
  | { id: string; code: string; description: string; valueType: 'fixed_per_sale'; value: number; maxAmount?: number; }
  | { id: string; code: string; description: string; valueType: 'percentage_capital' | 'percentage_total_loan'; value: number; maxAmount?: number; };
```

**Fórmulas de cálculo** (implementadas en `CommissionSettlementList.tsx`):
- `fixed_per_sale`: `commission = value`
- `percentage_capital`: `commission = (value / 100) * plan.price`
- `percentage_total_loan`: `commission = (value / 100) * (plan.price * plan.installments)`
- En todos los casos: si `commission > maxAmount`, se aplica `commission = maxAmount`

### 4.9 Product (Producto)

```typescript
interface Product {
  id: string;
  entityId: string;
  entityName: string;
  familyId: string;
  familyName: string;
  name: string;              // mín 2, máx 100
  code: string;              // mín 2, máx 20
  description: string;
  version: number;           // entero, inicia en 1
  status: ProductStatus;
  plans: ProductPlan[];      // embedded (subproductos)
  requirements: ProductRequirement[];
  validFrom: string;         // YYYY-MM-DD
  validTo?: string;          // YYYY-MM-DD, opcional
  createdAt: string;
}
```

### 4.10 ProductPlan (SubProducto / Plan)

```typescript
interface ProductPlan {
  id: string;
  productId: string;
  name: string;
  description: string;
  coverages: Coverage[];
  price: number;
  currency: string;          // ej: "ARS", "USD"
  installments?: number;
  commissionPlanId: string;  // FK → CommissionPlan
  status: ProductStatus;
  otherCosts?: number;
  // Atributos específicos por tipo (mutuamente excluyentes en la práctica):
  loanAttributes?: LoanAttributes;
  insuranceAttributes?: InsuranceAttributes;
  accountAttributes?: AccountAttributes;
  cardAttributes?: CardAttributes;
  investmentAttributes?: InvestmentAttributes;
}

interface LoanAttributes {
  amortizationType: AmortizationType; // 'french'|'german'|'american'|'bullet'
  annualEffectiveRate: number;         // TEA %
  cftRate: number;                     // CFT %
  adminFees: number;                   // gastos administrativos
}

interface InsuranceAttributes {
  premium: number;
  sumInsured: number;
  gracePeriodDays: number;
  coverageType: 'individual' | 'group' | 'collective';
}

interface AccountAttributes {
  maintenanceFee: number;
  minimumBalance: number;
  interestRate: number;
  accountType: 'savings' | 'checking' | 'money_market';
}

interface CardAttributes {
  creditLimit: number;
  annualFee: number;
  interestRate: number;
  network: CardNetwork;      // 'visa'|'mastercard'|'amex'|'cabal'|'naranja'
  gracePeriodDays: number;
  level?: string;
}

interface InvestmentAttributes {
  minimumAmount: number;
  expectedReturn: number;
  termDays: number;
  riskLevel: RiskLevel;       // 'low'|'medium'|'high'
  instrumentType: InstrumentType; // 'fixed_term'|'bond'|'mutual_fund'|'stock'
}

interface Coverage {
  id: string;
  name: string;
  description: string;
  sumInsured?: number;
  premium?: number;
}
```

### 4.11 ProductRequirement (Requisito)

```typescript
interface ProductRequirement {
  id: string;
  name: string;
  type: 'document' | 'data' | 'validation';
  mandatory: boolean;
  description: string;
}
```

### 4.12 Application (Solicitud)

```typescript
interface Application {
  id: string;
  code: string;              // formato "SOL-{año}-{nnn}" ej: "SOL-2025-001"
  entityId: string;
  entityName: string;
  productId: string;
  productName: string;
  planId: string;
  planName: string;
  applicant: Applicant;
  beneficiaries: Beneficiary[];
  status: ApplicationStatus;
  currentWorkflowStateId: string;
  currentWorkflowStateName: string;
  observations: ApplicationObservation[];
  documents: ApplicationDocument[];
  traceEvents: TraceEvent[];
  address?: ApplicationAddress;
  assignedUserId?: string;
  assignedUserName?: string;
  createdAt: string;         // ISO datetime
  updatedAt: string;         // ISO datetime
}

interface Applicant {
  firstName: string;         // mín 2
  lastName: string;          // mín 2
  documentType: 'DNI' | 'CUIT' | 'passport';
  documentNumber: string;    // mín 3
  email: string;
  phoneCode: string;
  phone: string;             // mín 5
  birthDate?: string;
  gender?: Gender;
  occupation?: string;
}

interface Beneficiary {
  firstName: string;
  lastName: string;
  relationship: string;
  percentage: number;        // suma total debe ser 100
}

interface ApplicationObservation {
  id: string;
  userId: string;
  userName: string;
  text: string;
  timestamp: string;
}

interface ApplicationDocument {
  id: string;
  name: string;
  type: string;
  status: 'pending' | 'approved' | 'rejected';
  uploadedAt: string;
}

interface ApplicationAddress {
  street: string;
  number: string;
  floor?: string;
  apartment?: string;
  city: string;
  province: string;
  postalCode: string;
  latitude?: number;
  longitude?: number;
}

interface TraceEvent {
  id: string;
  workflowStateId: string;
  workflowStateName: string;
  action: string;
  userName: string;
  timestamp: string;
  detail?: string;
}
```

### 4.13 WorkflowDefinition

```typescript
interface WorkflowDefinition {
  id: string;
  name: string;
  description: string;
  version: number;
  status: WorkflowStatus;    // 'draft'|'active'|'inactive'
  states: WorkflowState[];
  transitions: WorkflowTransition[];
  productCategory: string;   // 'loan'|'insurance'|'investment'|'credit_card'|'account'
  createdAt: string;
}

interface WorkflowState {
  id: string;
  name: string;
  type: 'initial' | 'intermediate' | 'final';
  slaHours?: number;
  color: string;
}

interface WorkflowTransition {
  id: string;
  fromStateId: string;
  toStateId: string;
  name: string;
  requiredRole?: string;
  autoTransition: boolean;
}
```

### 4.14 AuditEntry

```typescript
interface AuditEntry {
  id: string;
  userId: string;
  userName: string;
  operationType: AuditOperationType;
  action: string;            // descripción larga de la acción
  module: string;            // módulo del sistema
  detail: string;            // texto libre descriptivo
  timestamp: string;         // ISO datetime
  ip: string;
}
```

### 4.15 Parameter (Parámetro del sistema)

```typescript
interface Parameter {
  id: string;
  group: string;             // ID del grupo (ej: 'general', 'security', 'provinces')
  key: string;               // clave única dentro del grupo (ej: 'notifications.email_enabled')
  value: string;             // siempre string (booleanos como 'true'/'false')
  description: string;
  type: 'text' | 'number' | 'boolean' | 'select' | 'list' | 'html';
  options?: string[];        // para type='select'
  listItems?: { code: string; description: string }[];  // para type='list'
  parentKey?: string;        // para jerarquía (ej: ciudad tiene parentKey=provincia)
  icon?: string;             // nombre de icono Lucide
  updatedAt: string;         // YYYY-MM-DD
}

interface ParameterGroup {
  id: string;
  label: string;
  icon: string;
}
```

**Grupos de parámetros predefinidos**:

| Sección | Grupos |
|---------|--------|
| General | general, channels, currencies, masks, security, entity_types |
| Técnicos | integrations, workflow |
| Notificaciones | notifications, whatsapp, sms, templates |
| Posicionamiento | countries, provinces, cities |
| Seguros | insurance_coverages |
| Tarjetas | card_networks, card_levels |

**Claves de parámetros utilizadas en código** (consumidas por el frontend):
- `mask.username` — regex para validar usernames
- `mask.password` — regex para validar contraseñas
- `notifications.email_enabled` — habilita botón Email en detalle de solicitud
- `sms.enabled` — habilita botón SMS en detalle de solicitud
- `whatsapp.enabled` — habilita botón WhatsApp en detalle de solicitud
- Grupo `countries` — lista de países para selects
- Grupo `provinces` — lista de provincias (con parentKey=país)
- Grupo `cities` — lista de ciudades (con parentKey=provincia)

### 4.16 ExternalService (Servicio Externo)

```typescript
type AuthType = 'none' | 'api_key' | 'bearer_token' | 'basic_auth' | 'oauth2' | 'custom_header'
type ServiceType = 'rest_api' | 'mcp' | 'graphql' | 'soap' | 'webhook'
type ServiceStatus = 'active' | 'inactive' | 'error'

interface ExternalService {
  id: string;
  name: string;
  description: string;
  type: ServiceType;
  baseUrl: string;
  status: ServiceStatus;
  auth: ServiceAuthConfig;
  timeout: number;           // segundos
  retries: number;
  createdAt: string;
  updatedAt: string;
  lastTestedAt?: string;
  lastTestResult?: 'success' | 'failure';
}

interface ServiceAuthConfig {
  type: AuthType;
  apiKeyHeader?: string;
  apiKeyValue?: string;
  apiKeyLocation?: 'header' | 'query';
  bearerToken?: string;
  basicUser?: string;
  basicPassword?: string;
  oauth2ClientId?: string;
  oauth2ClientSecret?: string;
  oauth2TokenUrl?: string;
  oauth2Scopes?: string;
  oauth2GrantType?: 'client_credentials' | 'authorization_code';
  customHeaders?: { key: string; value: string }[];
}
```

### 4.17 SettlementHistoryEntry (Historial de Liquidaciones)

```typescript
interface SettlementHistoryEntry {
  id: string;
  settledAt: string;         // ISO datetime
  userId: string;
  userName: string;
  operationCount: number;
  totalsByCurrency: { currency: string; total: number }[];
  items: SettlementHistoryItem[];
  excelDataUrl?: string;     // Base64 data URL del Excel generado
}

interface SettlementHistoryItem {
  applicationId: string;
  code: string;
  entityName: string;
  applicantName: string;
  applicantDoc: string;
  commissionPlanName: string;
  commissionFormula: string;
  commission: number;
  currency: string;
}
```

---

## 5. Estado Global (Stores)

> Archivo fuente: `src/data/store.ts`, `src/data/authStore.ts`, `src/data/auditLog.ts`

El frontend usa un patrón de "module-level mutable state" + custom hooks React. No usa Redux ni Zustand. **TanStack Query está instalado pero no conectado a endpoints reales**.

### 5.1 Resumen de stores

| Hook | Entidad | Operaciones |
|------|---------|-------------|
| `useTenantStore()` | Tenant | tenants, addTenant, updateTenant, deleteTenant |
| `useEntityStore()` | Entity + Branch | entities, addEntity, updateEntity, deleteEntity, addBranch, updateBranch, deleteBranch |
| `useUserStore()` | User | users, addUser, updateUser, deleteUser |
| `useRoleStore()` | Role | roles, addRole, updateRole, deleteRole |
| `useProductStore()` | Product + Plan + Requirement | products, add/update/deleteProduct, add/update/deletePlan, add/update/deleteRequirement |
| `useProductFamilyStore()` | ProductFamily | families, addFamily, updateFamily, deleteFamily |
| `useCommissionPlanStore()` | CommissionPlan | commissionPlans, add/update/deleteCommissionPlan |
| `useApplicationStore()` | Application | applications, addApplication, updateApplication, deleteApplication |
| `useWorkflowStore()` | WorkflowDefinition | workflows, addWorkflow, updateWorkflow, deleteWorkflow |
| `useParameterStore()` | Parameter + ParameterGroup | parameters, groups, add/update/deleteParameter, addGroup, deleteGroup, getGroupValues |
| `useSettlementHistoryStore()` | SettlementHistoryEntry | entries, addEntry |
| `useAuditStore()` | AuditEntry | auditLog (read-only, reactivo) |
| `useAuthStore()` | AuthState | user, isAuthenticated, login, logout, sendOtp, verifyOtp, resetOtp, changePassword, requestPasswordReset |

### 5.2 AuthStore — flujo de autenticación

```
Login:   POST credentials → validar usuario/contraseña/estado
         → setear isAuthenticated=true + user
Logout:  → limpiar estado
Cambio de contraseña (flujo 3 pasos):
  1. Ingresar contraseña actual + nueva + confirmación
  2. Enviar OTP por SMS → verifyOtp(code)
  3. changePassword(userId, newPassword)
Recuperar clave: requestPasswordReset(email) → envía link
```

### 5.3 Auditoría — eventos registrados

Cada operación en los stores llama a `addAuditEntry(operationType, action, module, detail)`. Los módulos registrados son:
- `"Organización"` — Tenants, Entidades, Sucursales
- `"Seguridad"` — Usuarios, Roles, Login/Logout/Cambio Contraseña
- `"Catálogo"` — Productos, Familias, SubProductos, Requisitos
- `"Solicitudes"` — Applications
- `"Operaciones"` — Liquidaciones
- `"Configuración"` — Parámetros, Workflows, Planes Comisiones

---

## 6. Endpoints API Necesarios (Contrato esperado)

> El frontend actualmente usa datos mock. Esta sección describe los endpoints que el backend **debe** implementar para reemplazar los stores con llamadas HTTP reales.

### Convenciones
- Base URL: `https://api.unazul.com/v1` (a definir)
- Autenticación: Bearer JWT (implícito por el flujo de login)
- Respuestas paginadas donde se lista: `{ data: T[], total: number, page: number, pageSize: number }`
- Errores: `{ error: string, details?: object }`
- Ids generados por el servidor (UUID o similar)

---

### 6.1 Auth

| Método | Ruta | Descripcion | Body Request | Response |
|--------|------|-------------|-------------|----------|
| `POST` | `/auth/login` | Login con credenciales | `{ username, password }` | `{ token, user: User }` |
| `POST` | `/auth/logout` | Cerrar sesión | — | `{ ok }` |
| `POST` | `/auth/forgot-password` | Solicitar reset de contraseña | `{ email }` | `{ ok }` |
| `POST` | `/auth/send-otp` | Enviar OTP por SMS para cambio de contraseña | `{ userId }` | `{ ok }` |
| `POST` | `/auth/verify-otp` | Verificar OTP | `{ userId, code }` | `{ verified: bool }` |
| `POST` | `/auth/change-password` | Cambiar contraseña (post OTP) | `{ userId, currentPassword, newPassword }` | `{ ok }` |

---

### 6.2 Tenants (Organizaciones)

| Método | Ruta | Descripcion | Body / Query | Response |
|--------|------|-------------|-------------|----------|
| `GET` | `/tenants` | Listar organizaciones | `?search&status&country&page&pageSize` | `Tenant[]` paginado |
| `GET` | `/tenants/:id` | Obtener organización | — | `Tenant` |
| `POST` | `/tenants` | Crear organización | `TenantInput` | `Tenant` |
| `PUT` | `/tenants/:id` | Actualizar organización | `Partial<TenantInput>` | `Tenant` |
| `DELETE` | `/tenants/:id` | Eliminar organización | — | `{ ok }` |

---

### 6.3 Entities (Entidades)

| Método | Ruta | Descripcion | Body / Query | Response |
|--------|------|-------------|-------------|----------|
| `GET` | `/entities` | Listar entidades | `?tenantId&status&type&page&pageSize` | `Entity[]` paginado |
| `GET` | `/entities/:id` | Obtener entidad | — | `Entity` (con branches embedded) |
| `POST` | `/entities` | Crear entidad | `EntityInput` | `Entity` |
| `PUT` | `/entities/:id` | Actualizar entidad | `Partial<EntityInput>` | `Entity` |
| `DELETE` | `/entities/:id` | Eliminar entidad | — | `{ ok }` |

---

### 6.4 Branches (Sucursales) — sub-recurso de Entity

| Método | Ruta | Descripcion | Body | Response |
|--------|------|-------------|------|----------|
| `POST` | `/entities/:entityId/branches` | Crear sucursal | `BranchInput` | `Branch` |
| `PUT` | `/entities/:entityId/branches/:branchId` | Actualizar sucursal | `Partial<BranchInput>` | `Branch` |
| `DELETE` | `/entities/:entityId/branches/:branchId` | Eliminar sucursal | — | `{ ok }` |

---

### 6.5 Users (Usuarios)

| Método | Ruta | Descripcion | Body / Query | Response |
|--------|------|-------------|-------------|----------|
| `GET` | `/users` | Listar usuarios | `?entityId&status&roleId&page&pageSize` | `User[]` paginado (sin password) |
| `GET` | `/users/:id` | Obtener usuario | — | `User` (sin password) |
| `POST` | `/users` | Crear usuario | `UserInput` | `User` |
| `PUT` | `/users/:id` | Actualizar usuario | `Partial<UserInput>` | `User` |
| `DELETE` | `/users/:id` | Eliminar usuario | — | `{ ok }` |

**Nota de seguridad**: el campo `password` debe enviarse en el body solo al crear/actualizar y nunca retornarse en GETs.

---

### 6.6 Roles

| Método | Ruta | Descripcion | Body | Response |
|--------|------|-------------|------|----------|
| `GET` | `/roles` | Listar roles | — | `Role[]` |
| `POST` | `/roles` | Crear rol | `RoleInput` | `Role` |
| `PUT` | `/roles/:id` | Actualizar rol | `Partial<RoleInput>` | `Role` |
| `DELETE` | `/roles/:id` | Eliminar rol | — | `{ ok }` |

---

### 6.7 Permissions (Permisos) — catálogo estático

| Método | Ruta | Descripcion | Response |
|--------|------|-------------|----------|
| `GET` | `/permissions` | Catálogo completo de permisos | `Permission[]` |

---

### 6.8 Product Families

| Método | Ruta | Descripcion | Body | Response |
|--------|------|-------------|------|----------|
| `GET` | `/product-families` | Listar familias | — | `ProductFamily[]` |
| `POST` | `/product-families` | Crear familia | `{ code, description }` | `ProductFamily` |
| `PUT` | `/product-families/:id` | Actualizar familia | `Partial<{ code, description }>` | `ProductFamily` |
| `DELETE` | `/product-families/:id` | Eliminar familia | — | `{ ok }` |

---

### 6.9 Products (Productos)

| Método | Ruta | Descripcion | Body / Query | Response |
|--------|------|-------------|-------------|----------|
| `GET` | `/products` | Listar productos | `?entityId&familyId&status&page&pageSize` | `Product[]` paginado |
| `GET` | `/products/:id` | Obtener producto | — | `Product` (con plans y requirements embedded) |
| `POST` | `/products` | Crear producto | `ProductInput` | `Product` |
| `PUT` | `/products/:id` | Actualizar producto | `Partial<ProductInput>` | `Product` |
| `DELETE` | `/products/:id` | Eliminar producto | — | `{ ok }` |

**Sub-recursos de Product**:

| Método | Ruta | Descripcion | Body |
|--------|------|-------------|------|
| `POST` | `/products/:id/plans` | Agregar subproducto/plan | `Omit<ProductPlan, 'id' | 'productId'>` |
| `PUT` | `/products/:id/plans/:planId` | Actualizar plan | `Partial<ProductPlan>` |
| `DELETE` | `/products/:id/plans/:planId` | Eliminar plan | — |
| `POST` | `/products/:id/requirements` | Agregar requisito | `Omit<ProductRequirement, 'id'>` |
| `PUT` | `/products/:id/requirements/:reqId` | Actualizar requisito | `Partial<ProductRequirement>` |
| `DELETE` | `/products/:id/requirements/:reqId` | Eliminar requisito | — |

---

### 6.10 Applications (Solicitudes)

| Método | Ruta | Descripcion | Body / Query | Response |
|--------|------|-------------|-------------|----------|
| `GET` | `/applications` | Listar solicitudes | `?entityId&status&productId&assignedUserId&page&pageSize` | `Application[]` paginado |
| `GET` | `/applications/:id` | Obtener solicitud | — | `Application` completa |
| `POST` | `/applications` | Crear solicitud | `ApplicationInput` | `Application` |
| `PUT` | `/applications/:id` | Actualizar solicitud | `Partial<Application>` | `Application` |
| `DELETE` | `/applications/:id` | Eliminar solicitud | — | `{ ok }` |

**Operaciones especiales**:

| Método | Ruta | Descripcion | Body |
|--------|------|-------------|------|
| `POST` | `/applications/:id/observations` | Agregar observación | `{ text }` |
| `POST` | `/applications/:id/documents` | Adjuntar documento | multipart/form-data |
| `PUT` | `/applications/:id/documents/:docId` | Actualizar estado de documento | `{ status: 'pending' | 'approved' | 'rejected' }` |
| `POST` | `/applications/:id/workflow-advance` | Avanzar estado workflow | `{ transitionId, action, detail? }` |
| `PATCH` | `/applications/:id/status` | Cambiar estado | `{ status: ApplicationStatus }` |
| `POST` | `/applications/:id/messages` | Enviar mensaje (email/SMS/WhatsApp) | `{ type: 'email' | 'sms' | 'whatsapp', templateId?, body, recipient }` |

---

### 6.11 Commission Plans

| Método | Ruta | Descripcion | Body | Response |
|--------|------|-------------|------|----------|
| `GET` | `/commission-plans` | Listar planes | — | `CommissionPlan[]` |
| `POST` | `/commission-plans` | Crear plan | `CommissionPlanInput` | `CommissionPlan` |
| `PUT` | `/commission-plans/:id` | Actualizar plan | `Partial<CommissionPlanInput>` | `CommissionPlan` |
| `DELETE` | `/commission-plans/:id` | Eliminar plan | — | `{ ok }` |

---

### 6.12 Commission Settlement (Liquidación)

| Método | Ruta | Descripcion | Body / Query | Response |
|--------|------|-------------|-------------|----------|
| `GET` | `/settlements/pending` | Solicitudes aprobadas pendientes de liquidar | `?entityId&currency` | `CommissionRow[]` con comisión calculada |
| `POST` | `/settlements` | Ejecutar liquidación | `{ applicationIds: string[] }` | `{ entry: SettlementHistoryEntry, excelUrl: string }` |
| `GET` | `/settlements/history` | Historial de liquidaciones | `?userId&dateFrom&dateTo&page&pageSize` | `SettlementHistoryEntry[]` paginado |
| `GET` | `/settlements/history/:id/excel` | Descargar Excel de liquidación | — | File download |

**Nota**: el cálculo de comisión se realiza actualmente en el frontend (`calculateCommission`). El backend debe replicar exactamente la misma lógica.

---

### 6.13 Workflows

| Método | Ruta | Descripcion | Body | Response |
|--------|------|-------------|------|----------|
| `GET` | `/workflows` | Listar workflows | `?status&productCategory` | `WorkflowDefinition[]` |
| `GET` | `/workflows/:id` | Obtener workflow | — | `WorkflowDefinition` |
| `POST` | `/workflows` | Crear workflow | `Omit<WorkflowDefinition, 'id' | 'createdAt'>` | `WorkflowDefinition` |
| `PUT` | `/workflows/:id` | Actualizar workflow | `Partial<WorkflowDefinition>` | `WorkflowDefinition` |
| `DELETE` | `/workflows/:id` | Eliminar workflow | — | `{ ok }` |

---

### 6.14 Parameters (Parámetros del Sistema)

| Método | Ruta | Descripcion | Body | Response |
|--------|------|-------------|------|----------|
| `GET` | `/parameters` | Todos los parámetros | `?group` | `Parameter[]` |
| `GET` | `/parameters/groups` | Grupos de parámetros | — | `ParameterGroup[]` |
| `POST` | `/parameters` | Crear parámetro | `Omit<Parameter, 'id' | 'updatedAt'>` | `Parameter` |
| `PUT` | `/parameters/:id` | Actualizar parámetro | `Partial<Parameter>` | `Parameter` |
| `DELETE` | `/parameters/:id` | Eliminar parámetro | — | `{ ok }` |
| `POST` | `/parameters/groups` | Crear grupo | `Omit<ParameterGroup, 'id'>` | `ParameterGroup` |
| `DELETE` | `/parameters/groups/:id` | Eliminar grupo | — | `{ ok }` |
| `POST` | `/parameters/save-bulk` | Guardar múltiples parámetros (batch) | `{ updates: Array<{ id, value?, description?, listItems? }> }` | `Parameter[]` |

---

### 6.15 External Services (Servicios Externos)

| Método | Ruta | Descripcion | Body | Response |
|--------|------|-------------|------|----------|
| `GET` | `/services` | Listar servicios | — | `ExternalService[]` |
| `POST` | `/services` | Crear servicio | `Omit<ExternalService, 'id' | 'createdAt' | 'updatedAt'>` | `ExternalService` |
| `PUT` | `/services/:id` | Actualizar servicio | `Partial<ExternalService>` | `ExternalService` |
| `DELETE` | `/services/:id` | Eliminar servicio | — | `{ ok }` |
| `POST` | `/services/:id/test` | Probar conectividad del servicio | — | `{ success: bool, latencyMs?: number, error?: string }` |

---

### 6.16 Audit Log

| Método | Ruta | Descripcion | Query | Response |
|--------|------|-------------|-------|----------|
| `GET` | `/audit` | Consultar log | `?userId&operationType&module&dateFrom&dateTo&search&page&pageSize` | `AuditEntry[]` paginado |
| `GET` | `/audit/export` | Exportar a Excel | mismos filtros | File download (xlsx) |

---

### 6.17 Organization Tree

| Método | Ruta | Descripcion | Response |
|--------|------|-------------|----------|
| `GET` | `/org-tree` | Árbol jerárquico completo Tenant > Entity > Branch | `{ tenants: Array<Tenant & { entities: Array<Entity & { branches: Branch[] }> }> }` |

---

## 7. Módulos/Páginas — Resumen Funcional

### Dashboard (`/`)
- Métricas: entidades activas, usuarios activos, productos activos, solicitudes pendientes
- Gráficos de barras doble-eje por organización (solicitudes por estado, monto)
- Gráficos de barras por entidad (productos: cantidad y valor representativo)
- Feed de últimas 5 entradas de auditoría
- Filtro de rango de fechas

### Auth
- **LoginPage**: form usuario+contraseña, link a recuperación
- **ForgotPasswordPage**: solicitud por email, confirmación UI
- **ChangePasswordPage**: flujo 3 pasos — (1) contraseña actual + nueva, (2) OTP 6 dígitos por SMS, (3) confirmación

### Organizaciones
- **TenantList**: DataTable filtrable (nombre, país, estado), exporta Excel, click-to-detail, acciones inline editar/eliminar
- **TenantForm**: form alta/edición con validación Zod, combobox buscable de países (desde parámetros), sección de entidades asociadas en modo edición
- **TenantDetail**: info + entidades de la org

### Entidades
- **EntityList**: DataTable con filtros de tipo, estado, canales
- **EntityForm**: datos generales + contacto + canales multi-select + selección de tenant
- **EntityDetail**: tabs — Info general, Sucursales (tabla inline), Productos (EntityProductsTab)
- **BranchForm**: alta/edición de sucursal (anidado en ruta de entidad)

### Seguridad
- **UserList**: DataTable, filtros por entidad/rol/estado
- **UserForm**: datos personales + autenticación + asignación jerárquica (org > entidad > sucursal) + roles multi-select + vista de permisos efectivos
- **UserDetail**: info + permisos + historial
- **RolesPage**: tabla con permisos por módulo
- **RoleForm**: datos + checklist de permisos agrupados por módulo

### Catálogo
- **ProductFamilyList**: ABM inline con modal de edición
- **ProductList**: DataTable filtrable por entidad/familia/estado
- **ProductForm**: datos generales + sección de SubProductos (modal inline con atributos específicos por tipo) + sección de Requisitos
- **ProductDetail**: tabs — SubProductos (cards) + Requisitos

### Solicitudes
- **ApplicationList**: DataTable con filtros multi, exporta Excel
- **ApplicationForm**: selección entidad > producto > plan en cascada + datos del solicitante + domicilio (provincia > ciudad desde parámetros) + asignación de usuario
- **ApplicationDetail**: tabs — Solicitante, Producto (con atributos por tipo), Domicilio (con Google Maps embed), Beneficiarios, Documentos, Observaciones, Trazabilidad (timeline de workflow)
  - Botones condicionales: Email/SMS/WhatsApp (según parámetros habilitados)

### Liquidación de Comisiones
- **CommissionSettlementList**: tabla de todas las solicitudes con comisión calculada, selección múltiple, preliquidación (Excel con marca de agua), liquidación definitiva (Excel + cambio de estado a `settled`)
- **CommissionSettlementDetail**: desglose completo: solicitud + solicitante + entidad + producto/plan + plan de comisión + cálculo detallado
- **SettlementHistoryList**: registro de liquidaciones pasadas con descarga del Excel

### Configuración
- **WorkflowList/WorkflowEditor**: lista + editor visual drag-and-drop con React Flow. Nodos: start, end, service_call, decision, send_message, data_capture, timer. Panel de atributos del dominio para formulas.
- **ParametersPage**: sidebar de grupos + edición inline de valores (text, number, boolean, select, list, html), batch save
- **ServicesPage**: ABM de servicios externos con soporte para REST API, GraphQL, SOAP, Webhook, MCP; múltiples métodos de autenticación
- **CommissionPlanList**: ABM de planes de comisiones

### Auditoría
- **AuditPage**: log filtrable por fecha, usuario, tipo de operación, módulo, acción; exporta Excel

---

## 8. Componentes Reutilizables Clave

| Componente | Ubicación | Descripcion |
|-----------|----------|-------------|
| `DataTable` | `components/shared/DataTable` | Tabla con búsqueda, filtros por columna (text, select, multiselect, date), ordenamiento, exportación Excel via xlsx |
| `MetricCard` | `components/shared/MetricCard` | Card de KPI con título, valor, subtítulo, icono y tendencia |
| `StatusBadge` | `components/shared/StatusBadge` | Badge con color según status (active/inactive/suspended/etc) |
| `PhoneCodeSelect` | `components/shared/PhoneCodeSelect` | Select de código de país telefónico |
| `DeleteConfirmDialog` | `components/crud/DeleteConfirmDialog` | Dialog de confirmación de eliminación reutilizable |
| `SendMessageDialog` | `components/applications/SendMessageDialog` | Dialog para enviar email/SMS/WhatsApp a solicitante |
| `FlowNode` | `components/workflow/FlowNode` | Nodo custom de React Flow con ícono y configuración |
| `NodeConfigDialog` | `components/workflow/NodeConfigDialog` | Dialog de configuración de atributos de un nodo |
| `HtmlParamEditor` | `components/parameters/HtmlParamEditor` | Editor HTML/RichText para parámetros de tipo html |
| `ListParamEditor` | `components/parameters/ListParamEditor` | Editor de listas code/description para parámetros tipo list |
| `AppLayout` | `components/layout/AppLayout` | Layout principal con sidebar colapsable |
| `AppSidebar` | `components/layout/AppSidebar` | Sidebar con secciones: General, Organización, Operaciones, Configuración, Seguridad |

---

## 9. Lógica de Negocio Relevante para el Backend

### 9.1 Código de solicitud
```
formato: "SOL-{YYYY}-{NNN}"
NNN = número secuencial zero-padded 3 dígitos
```

### 9.2 Cálculo de comisión
```
fixed_per_sale:         commission = plan.commissionPlan.value
percentage_capital:     commission = (commissionPlan.value / 100) * plan.price
percentage_total_loan:  commission = (commissionPlan.value / 100) * (plan.price * plan.installments)

En todos los casos:     if (maxAmount && commission > maxAmount) commission = maxAmount
```

### 9.3 Permisos efectivos de usuario
```
Un usuario puede tener múltiples roles.
Permisos efectivos = UNION de los permissions[] de todos los roles asignados (sin duplicados).
```

### 9.4 Jerarquía organizacional
```
Tenant (Organización)
  └── Entity (Entidad: bank | insurance | fintech | cooperative)
        └── Branch (Sucursal)

Un usuario puede tener assignments en múltiples niveles:
  assignments.organizationIds[] → acceso a esas organizaciones
  assignments.entityIds[]       → acceso a esas entidades (independiente de org)
  assignments.branchIds[]       → acceso a esas sucursales
```

### 9.5 Workflow de solicitudes
```
Estados:  initial → intermediate(s) → final
Tipos:    initial (1 por workflow), intermediate (N), final (N)
Cada estado tiene: name, slaHours (opcional), color
Transiciones: fromStateId → toStateId, con nombre, rol requerido (opcional), y flag autoTransition

El frontend muestra un timeline visual (happy path) para trazabilidad.
```

### 9.6 Categorías de producto para workflows
```
loan | insurance | investment | credit_card | account
```

### 9.7 Parámetros de validación de contraseña/username
```
Las máscaras (regex) se leen del parámetro:
  key = 'mask.password'  → regex para validar contraseñas
  key = 'mask.username'  → regex para validar nombres de usuario

Si no existen los parámetros, se usan defaults:
  username: /^[a-zA-Z0-9._-]{3,30}$/
  password: /^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,50}$/
```

### 9.8 Export Excel de liquidaciones
El frontend genera el Excel en el navegador (SheetJS). Si el backend necesita generarlo:
- Una hoja por moneda
- Filas agrupadas por entidad con subtotales
- Fila de total general al final
- Columnas: Entidad, Nro. Solicitud, Fecha Solicitud, Solicitante, Documento, Plan Comisión, Fórmula Comisión, Comisión ({moneda})

---

## 10. Integraciones Externas Identificadas

| Integracion | Tipo | Descripcion |
|-------------|------|-------------|
| Google Maps | Embed iframe | Visualización de domicilio en detalle de solicitud (sin API key visible, usa URL pública) |
| SMS Provider | REST API configurable | Envío de OTP para cambio de contraseña; envío de mensajes a solicitantes |
| Email Provider | REST API configurable | Envío de emails a solicitantes desde detalle de solicitud |
| WhatsApp | REST API configurable | Mensajes a solicitantes (condicional según parámetro `whatsapp.enabled`) |
| Servicios de scoring/validación/IA | REST API / SOAP / MCP configurable | Gestión de servicios externos vía `ServicesPage` |

---

## 11. Notas para Migración a Backend Real

1. **Reemplazar custom hooks con React Query**: cada `useXxxStore()` debería migrarse a `useQuery`/`useMutation` de TanStack Query (ya instalado).

2. **Autenticación JWT**: el `useAuthStore` debe persistir el token en `localStorage` o `httpOnly cookie`, y el `AppLayout` debe validarlo.

3. **Parámetros del sistema**: el `ParametersPage` hace un "save-bulk" — el backend debe soportar actualizar múltiples parámetros en una sola llamada.

4. **Excel de liquidaciones**: actualmente se genera 100% en el cliente. Para auditoría completa, el backend debería guardar el binario y servir un endpoint de descarga.

5. **Paginación**: el `DataTable` actual carga todos los datos en memoria. Con datos reales se requiere paginación server-side o cursor-based.

6. **Permisos**: el frontend tiene el catálogo de permisos hardcoded en `mock.ts`. El backend debe persistirlos y exponerlos vía `GET /permissions`.

7. **OTP**: el `ChangePasswordPage` muestra el OTP en pantalla (modo demo). En producción, el código debe enviarse sólo por SMS y nunca retornarse al frontend.

8. **Cascada País > Provincia > Ciudad**: actualmente viene del store de parámetros. En un backend real esto puede ser un endpoint dedicado de geolocalización o seguir siendo parte de `/parameters?group=provinces` y `/parameters?group=cities`.
