# Unazul Backoffice — Especificaciones Técnicas y Funcionales

> **Versión:** 2.0.0  
> **Fecha:** 2026-03-15  
> **Frontend:** React 18 · TypeScript · Vite · Tailwind CSS · shadcn/ui  
> **Backend:** .NET Core 10 · Arquitectura de Microservicios · PostgreSQL · Redis · RabbitMQ

---

## Índice

1. [Visión General](#1-visión-general)
2. [Arquitectura del Sistema](#2-arquitectura-del-sistema)
3. [Microservicios Backend](#3-microservicios-backend)
4. [Estructura de Navegación](#4-estructura-de-navegación)
5. [Módulos Funcionales](#5-módulos-funcionales)
   - 5.1 [Dashboard](#51-dashboard)
   - 5.2 [Organización](#52-organización)
   - 5.3 [Operaciones](#53-operaciones)
   - 5.4 [Catálogo de Productos](#54-catálogo-de-productos)
   - 5.5 [Configuración](#55-configuración)
   - 5.6 [Seguridad](#56-seguridad)
6. [Modelo de Datos](#6-modelo-de-datos)
7. [Sistema de Parametrización](#7-sistema-de-parametrización)
8. [Sistema de Notificaciones](#8-sistema-de-notificaciones)
9. [Validaciones y Máscaras](#9-validaciones-y-máscaras)
10. [Componentes Compartidos Frontend](#10-componentes-compartidos-frontend)
11. [Permisos y Roles](#11-permisos-y-roles)
12. [API Gateway y Contratos](#12-api-gateway-y-contratos)
13. [Infraestructura y Despliegue](#13-infraestructura-y-despliegue)
14. [Seguridad y Cumplimiento](#14-seguridad-y-cumplimiento)

---

## 1. Visión General

**Unazul Backoffice** es una plataforma de gestión administrativa multi-organización y multi-entidad diseñada para organizaciones financieras argentinas (bancos, aseguradoras, fintechs, cooperativas, mutuales y SGRs).

### 1.1 Características Principales

- **Multi-organización (Multi-Tenant):** Agrupación jerárquica de entidades bajo organizaciones (holdings o grupos empresarios). La terminología visible en la UI es "Organización" (internamente `Tenant`).
- **Multi-entidad:** Cada entidad opera de forma independiente con sus propias sucursales, usuarios, productos y solicitudes.
- **Parametrización total:** Tipos de entidades, canales, provincias, ciudades, redes de tarjetas, niveles de tarjetas, coberturas de seguros, máscaras de validación y configuración general se administran desde un módulo centralizado.
- **Gestión de productos financieros:** Catálogo con familias de productos, sub productos (planes), coberturas, requisitos y atributos específicos por categoría (préstamos, seguros, inversiones, tarjetas, cuentas).
- **Motor de workflows:** Editor visual de flujos de proceso con estados, transiciones, SLAs, nodos de decisión y asignación automática.
- **Liquidación de comisiones:** Cálculo automático de comisiones por venta con generación de reportes Excel y gestión de historial.
- **Sistema de notificaciones multicanal:** Envío de Email, SMS y WhatsApp con sistema de plantillas configurable y reemplazo de variables dinámicas.
- **Auditoría completa:** Registro inmutable de todas las acciones del sistema con tipo de operación categorizado.

### 1.2 Usuarios Objetivo

| Rol | Descripción | Permisos Principales |
|-----|-------------|---------------------|
| Super Admin | Acceso total al sistema | Todas las funcionalidades |
| Admin Entidad | Administración de una entidad específica | CRUD entidades, sucursales, solicitudes, usuarios de su entidad |
| Operador | Operaciones diarias | Gestión de solicitudes, documentos, observaciones |
| Auditor | Solo lectura y auditoría | Consulta de logs, exportación |
| Consulta | Solo consulta de información | Lectura en todos los módulos |
| Diseñador de Procesos | Diseño de workflows | Editor visual de flujos |
| Admin Producto | Gestión del catálogo | CRUD de productos, planes, requisitos, familias |

---

## 2. Arquitectura del Sistema

### 2.1 Arquitectura General

```
                    ┌─────────────────────┐
                    │   CDN / Load Balancer│
                    └──────────┬──────────┘
                               │
              ┌────────────────┼────────────────┐
              │                │                │
    ┌─────────▼─────────┐     │     ┌──────────▼──────────┐
    │   Frontend SPA    │     │     │   Admin Portal       │
    │   React 18 + Vite │     │     │   (futuro)           │
    └─────────┬─────────┘     │     └──────────┬──────────┘
              │               │                │
              └───────────────┼────────────────┘
                              │
                    ┌─────────▼─────────┐
                    │   API Gateway      │
                    │   (.NET 10 YARP)  │
                    └─────────┬─────────┘
                              │
        ┌──────────┬──────────┼──────────┬──────────┬──────────┐
        │          │          │          │          │          │
   ┌────▼───┐ ┌───▼────┐ ┌───▼────┐ ┌───▼────┐ ┌───▼────┐ ┌───▼────┐
   │Identity│ │Org Svc │ │Catalog │ │Ops Svc │ │Config  │ │Audit   │
   │Service │ │        │ │Service │ │        │ │Service │ │Service │
   └───┬────┘ └───┬────┘ └───┬────┘ └───┬────┘ └───┬────┘ └───┬────┘
       │          │          │          │          │          │
       ▼          ▼          ▼          ▼          ▼          ▼
   ┌────────────────────────────────────────────────────────────────┐
   │                     PostgreSQL Cluster                         │
   │  identity_db │ org_db │ catalog_db │ ops_db │ config_db │ audit_db │
   └────────────────────────────────────────────────────────────────┘
                              │
                    ┌─────────▼─────────┐
                    │  RabbitMQ / Redis  │
                    │  (Events + Cache)  │
                    └───────────────────┘
```

### 2.2 Stack Frontend

| Capa | Tecnología |
|------|-----------|
| Framework | React 18.3 + TypeScript 5.x |
| Build | Vite 5.x |
| Estilos | Tailwind CSS 3.x + tailwindcss-animate |
| Componentes UI | shadcn/ui (Radix primitives) |
| Ruteo | React Router DOM v6 (lazy loading) |
| Formularios | React Hook Form + Zod |
| Gráficos | Recharts |
| Editor visual | @xyflow/react (React Flow) |
| Estado | Custom hooks con useState (patrón store) |
| Exportación | xlsx (Excel/CSV) |
| HTTP Client | Axios / TanStack Query |

### 2.3 Stack Backend (.NET Core 10)

| Capa | Tecnología |
|------|-----------|
| Framework | .NET 10 (ASP.NET Core Minimal APIs + Controllers) |
| ORM | Entity Framework Core 10 |
| Base de datos | PostgreSQL 16 |
| Cache | Redis 7.x |
| Mensajería | RabbitMQ 3.13 / MassTransit |
| Autenticación | ASP.NET Core Identity + JWT Bearer |
| Autorización | Policy-based authorization |
| API Gateway | YARP (Yet Another Reverse Proxy) |
| Observabilidad | OpenTelemetry + Serilog + Seq |
| Contenedores | Docker + Kubernetes |
| CI/CD | GitHub Actions |
| Testing | xUnit + NSubstitute + TestContainers |

### 2.4 Estructura de Archivos Frontend

```
src/
├── components/
│   ├── layout/          # AppLayout, AppSidebar
│   ├── shared/          # DataTable, MetricCard, StatusBadge, PhoneCodeSelect
│   ├── crud/            # DeleteConfirmDialog
│   ├── applications/    # SendMessageDialog
│   ├── entities/        # EntityProductsTab
│   ├── parameters/      # ListParamEditor, HtmlParamEditor
│   ├── workflow/        # FlowNode, NodeConfigDialog
│   └── ui/              # shadcn/ui components (~50 componentes)
├── data/
│   ├── types.ts         # Interfaces y tipos del dominio (~400 líneas)
│   ├── store.ts         # Stores de estado reactivos (~470 líneas)
│   ├── mock.ts          # Datos iniciales de ejemplo (~765 líneas)
│   ├── parameters.ts    # Parámetros del sistema y grupos (~409 líneas)
│   ├── services.ts      # Servicios externos
│   ├── authStore.ts     # Autenticación y sesión
│   ├── auditLog.ts      # Sistema de auditoría centralizada
│   └── workflowNodes.ts # Nodos del editor de workflow
├── pages/
│   ├── Dashboard.tsx
│   ├── AuditPage.tsx
│   ├── Index.tsx / NotFound.tsx
│   ├── auth/            # LoginPage, ForgotPasswordPage, ChangePasswordPage
│   ├── tenants/         # TenantList, TenantDetail, TenantForm
│   ├── entities/        # EntityList, EntityDetail, EntityForm, BranchForm
│   ├── security/        # UserList, UserDetail, UserForm, RolesPage, RoleForm
│   ├── products/        # ProductList, ProductDetail, ProductForm, ProductFamilyList
│   ├── applications/    # ApplicationList, ApplicationDetail, ApplicationForm
│   ├── commissions/     # CommissionSettlementList, CommissionSettlementDetail, SettlementHistoryList
│   ├── workflows/       # WorkflowList, WorkflowEditor
│   ├── organization/    # OrgTreePage
│   └── config/          # ParametersPage, ServicesPage, CommissionPlanList
└── hooks/               # use-mobile, use-toast
```

---

## 3. Microservicios Backend

### 3.1 Identity Service (`identity-service`)

**Responsabilidad:** Autenticación, autorización, gestión de usuarios, roles y permisos.

**Endpoints principales:**

| Método | Ruta | Descripción |
|--------|------|-------------|
| POST | `/api/v1/auth/login` | Inicio de sesión (retorna JWT) |
| POST | `/api/v1/auth/logout` | Cierre de sesión (invalida token) |
| POST | `/api/v1/auth/forgot-password` | Solicita reseteo de contraseña |
| POST | `/api/v1/auth/change-password` | Cambia contraseña (requiere OTP) |
| POST | `/api/v1/auth/send-otp` | Envía código OTP al email del usuario |
| POST | `/api/v1/auth/verify-otp` | Verifica código OTP |
| GET | `/api/v1/users` | Lista usuarios (paginado, filtros) |
| GET | `/api/v1/users/{id}` | Detalle de usuario |
| POST | `/api/v1/users` | Crear usuario |
| PUT | `/api/v1/users/{id}` | Editar usuario |
| DELETE | `/api/v1/users/{id}` | Eliminar usuario |
| PATCH | `/api/v1/users/{id}/status` | Cambiar estado (active/inactive/locked) |
| GET | `/api/v1/roles` | Lista roles |
| POST | `/api/v1/roles` | Crear rol |
| PUT | `/api/v1/roles/{id}` | Editar rol y permisos |
| DELETE | `/api/v1/roles/{id}` | Eliminar rol |
| GET | `/api/v1/permissions` | Lista permisos atómicos |

**Modelo de datos:**

```csharp
public class User
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public Guid? EntityId { get; set; }
    public string EntityName { get; set; }
    public UserStatus Status { get; set; }
    public DateTime? LastLogin { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Avatar { get; set; }
    public UserAssignment? Assignments { get; set; }
    public ICollection<UserRole> UserRoles { get; set; }
}

public class Role
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public ICollection<RolePermission> RolePermissions { get; set; }
}

public class Permission
{
    public Guid Id { get; set; }
    public string Module { get; set; }
    public string Action { get; set; }
    public string Description { get; set; }
}

public enum UserStatus { Active, Inactive, Locked }
```

### 3.2 Organization Service (`organization-service`)

**Responsabilidad:** Gestión de organizaciones (tenants), entidades y sucursales.

**Endpoints principales:**

| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/v1/tenants` | Lista organizaciones |
| GET | `/api/v1/tenants/{id}` | Detalle de organización |
| POST | `/api/v1/tenants` | Crear organización |
| PUT | `/api/v1/tenants/{id}` | Editar organización |
| DELETE | `/api/v1/tenants/{id}` | Eliminar organización |
| GET | `/api/v1/entities` | Lista entidades (filtro por tenant) |
| GET | `/api/v1/entities/{id}` | Detalle de entidad con sucursales |
| POST | `/api/v1/entities` | Crear entidad |
| PUT | `/api/v1/entities/{id}` | Editar entidad |
| DELETE | `/api/v1/entities/{id}` | Eliminar entidad |
| GET | `/api/v1/entities/{entityId}/branches` | Lista sucursales |
| POST | `/api/v1/entities/{entityId}/branches` | Crear sucursal |
| PUT | `/api/v1/entities/{entityId}/branches/{branchId}` | Editar sucursal |
| DELETE | `/api/v1/entities/{entityId}/branches/{branchId}` | Eliminar sucursal |
| GET | `/api/v1/org-tree` | Árbol jerárquico completo |

**Modelo de datos:**

```csharp
public class Tenant
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Identifier { get; set; }  // CUIT
    public string Description { get; set; }
    public TenantStatus Status { get; set; }
    public string ContactName { get; set; }
    public string ContactEmail { get; set; }
    public string ContactPhoneCode { get; set; }
    public string ContactPhone { get; set; }
    public string Country { get; set; }
    public DateTime CreatedAt { get; set; }
    public ICollection<Entity> Entities { get; set; }
}

public class Entity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; }
    public string Identifier { get; set; }  // CUIT
    public EntityType Type { get; set; }    // bank, insurance, fintech, cooperative
    public EntityStatus Status { get; set; }
    public string Email { get; set; }
    public string PhoneCode { get; set; }
    public string Phone { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public string Province { get; set; }
    public string Country { get; set; }
    public DateTime CreatedAt { get; set; }
    public ICollection<Branch> Branches { get; set; }
    public List<ChannelType> Channels { get; set; }
}

public class Branch
{
    public Guid Id { get; set; }
    public Guid EntityId { get; set; }
    public string Name { get; set; }
    public string Code { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public string Province { get; set; }
    public EntityStatus Status { get; set; }
    public string Manager { get; set; }
    public string PhoneCode { get; set; }
    public string Phone { get; set; }
}
```

### 3.3 Catalog Service (`catalog-service`)

**Responsabilidad:** Familias de productos, productos, sub productos (planes), coberturas, requisitos y planes de comisiones.

**Endpoints principales:**

| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/v1/product-families` | Lista familias |
| POST | `/api/v1/product-families` | Crear familia |
| PUT | `/api/v1/product-families/{id}` | Editar familia |
| DELETE | `/api/v1/product-families/{id}` | Eliminar familia |
| GET | `/api/v1/products` | Lista productos (filtros por entidad, familia, estado) |
| GET | `/api/v1/products/{id}` | Detalle con planes, requisitos y coberturas |
| POST | `/api/v1/products` | Crear producto |
| PUT | `/api/v1/products/{id}` | Editar producto |
| DELETE | `/api/v1/products/{id}` | Eliminar producto |
| POST | `/api/v1/products/{id}/plans` | Crear sub producto/plan |
| PUT | `/api/v1/products/{id}/plans/{planId}` | Editar plan |
| DELETE | `/api/v1/products/{id}/plans/{planId}` | Eliminar plan |
| POST | `/api/v1/products/{id}/requirements` | Crear requisito |
| PUT | `/api/v1/products/{id}/requirements/{reqId}` | Editar requisito |
| DELETE | `/api/v1/products/{id}/requirements/{reqId}` | Eliminar requisito |
| GET | `/api/v1/commission-plans` | Lista planes de comisiones |
| POST | `/api/v1/commission-plans` | Crear plan de comisión |
| PUT | `/api/v1/commission-plans/{id}` | Editar plan |
| DELETE | `/api/v1/commission-plans/{id}` | Eliminar plan |

**Categorías de productos (determinadas por prefijo de familia):**

| Prefijo | Categoría | Atributos específicos |
|---------|-----------|----------------------|
| `PREST` | Préstamos (`loan`) | `amortizationType`, `annualEffectiveRate`, `cftRate`, `adminFees` |
| `SEG` | Seguros (`insurance`) | `premium`, `sumInsured`, `gracePeriodDays`, `coverageType` |
| `CTA` | Cuentas (`account`) | `maintenanceFee`, `minimumBalance`, `interestRate`, `accountType` |
| `TARJETA` | Tarjetas (`card`) | `creditLimit`, `annualFee`, `interestRate`, `network`, `level` |
| `INV` | Inversiones (`investment`) | `minimumAmount`, `expectedReturn`, `termDays`, `riskLevel` |

**Tipos de comisión:**

| Tipo | Descripción | Fórmula |
|------|-------------|---------|
| `fixed_per_sale` | Monto fijo por venta | `commission = value` |
| `percentage_capital` | Porcentaje del capital | `commission = (value/100) × price` |
| `percentage_total_loan` | Porcentaje del total del préstamo | `commission = (value/100) × price × installments` |

### 3.4 Operations Service (`operations-service`)

**Responsabilidad:** Solicitudes, liquidación de comisiones, trazabilidad y documentos.

**Endpoints principales:**

| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/v1/applications` | Lista solicitudes (paginado, filtros avanzados) |
| GET | `/api/v1/applications/{id}` | Detalle completo con todos los datos relacionados |
| POST | `/api/v1/applications` | Crear solicitud |
| PUT | `/api/v1/applications/{id}` | Editar solicitud |
| DELETE | `/api/v1/applications/{id}` | Eliminar solicitud |
| PATCH | `/api/v1/applications/{id}/status` | Cambiar estado |
| POST | `/api/v1/applications/{id}/observations` | Agregar observación |
| POST | `/api/v1/applications/{id}/documents` | Subir documento |
| PATCH | `/api/v1/applications/{id}/documents/{docId}/status` | Aprobar/rechazar documento |
| POST | `/api/v1/applications/{id}/send-message` | Enviar Email/SMS/WhatsApp |
| POST | `/api/v1/settlements` | Liquidar comisiones (bulk) |
| GET | `/api/v1/settlements/preview` | Preliquidación (preview) |
| GET | `/api/v1/settlements/history` | Historial de liquidaciones |
| GET | `/api/v1/settlements/history/{id}` | Detalle de liquidación |
| GET | `/api/v1/settlements/history/{id}/export` | Descargar Excel de liquidación |

**Estados del ciclo de vida de solicitudes:**

```
draft → pending → in_review → approved → settled
                            → rejected
                → cancelled
```

| Estado | Etiqueta | Descripción |
|--------|----------|-------------|
| `draft` | Borrador | Solicitud creada, sin enviar |
| `pending` | Pendiente | Enviada a revisión |
| `in_review` | En Revisión | En análisis por operador |
| `approved` | Aprobada | Aprobada, lista para liquidación |
| `rejected` | Rechazada | Rechazada |
| `cancelled` | Cancelada | Cancelada por solicitante o sistema |
| `settled` | Liquidado | Comisión liquidada |

### 3.5 Configuration Service (`config-service`)

**Responsabilidad:** Parámetros del sistema, servicios externos, workflows y plantillas de notificación.

**Endpoints principales:**

| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/v1/parameters` | Lista parámetros (filtro por grupo) |
| GET | `/api/v1/parameters/groups` | Lista grupos de parámetros |
| POST | `/api/v1/parameters` | Crear parámetro |
| PUT | `/api/v1/parameters/{id}` | Editar parámetro |
| DELETE | `/api/v1/parameters/{id}` | Eliminar parámetro |
| POST | `/api/v1/parameters/groups` | Crear grupo |
| DELETE | `/api/v1/parameters/groups/{id}` | Eliminar grupo |
| GET | `/api/v1/services` | Lista servicios externos |
| POST | `/api/v1/services` | Crear servicio |
| PUT | `/api/v1/services/{id}` | Editar servicio |
| DELETE | `/api/v1/services/{id}` | Eliminar servicio |
| POST | `/api/v1/services/{id}/test` | Probar conexión |
| GET | `/api/v1/workflows` | Lista workflows |
| GET | `/api/v1/workflows/{id}` | Detalle con estados y transiciones |
| POST | `/api/v1/workflows` | Crear workflow |
| PUT | `/api/v1/workflows/{id}` | Editar workflow |
| DELETE | `/api/v1/workflows/{id}` | Eliminar workflow |

### 3.6 Audit Service (`audit-service`)

**Responsabilidad:** Registro inmutable de todas las acciones del sistema.

**Endpoints principales:**

| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/v1/audit` | Consultar log (paginado, filtros) |
| GET | `/api/v1/audit/export` | Exportar a Excel/CSV |

**Tipos de operación:**

`Crear`, `Editar`, `Eliminar`, `Login`, `Logout`, `Cambiar Contraseña`, `Cambiar Estado`, `Liquidar`, `Exportar`, `Consultar`, `Otro`

### 3.7 Notification Service (`notification-service`)

**Responsabilidad:** Envío de mensajes por Email, SMS y WhatsApp con sistema de plantillas.

**Endpoints principales:**

| Método | Ruta | Descripción |
|--------|------|-------------|
| POST | `/api/v1/notifications/email` | Enviar email |
| POST | `/api/v1/notifications/sms` | Enviar SMS |
| POST | `/api/v1/notifications/whatsapp` | Enviar WhatsApp |
| GET | `/api/v1/notifications/templates` | Lista plantillas |
| POST | `/api/v1/notifications/templates/preview` | Vista previa con variables resueltas |

**Proveedores soportados:**

| Canal | Proveedores |
|-------|------------|
| Email | SMTP (configurable) |
| SMS | Twilio, SMS Masivos, Vonage, Infobip |
| WhatsApp | Meta WhatsApp Business API |

---

## 4. Estructura de Navegación

### 4.1 Sidebar

```
📊 Dashboard                    /

── ORGANIZACIÓN ──
🏢 Organizaciones               /tenants
🏛 Entidades                    /entidades
🌐 Estructura                   /estructura

── OPERACIONES ──
📋 Solicitudes                  /solicitudes
💰 Liquidación                  /liquidacion-comisiones
📜 Historial de liquidaciones   /historial-liquidaciones

── CONFIGURACIÓN ──
📁 Familia de productos         /familias-productos
📦 Productos                    /productos
⚙️ Parámetros                   /parametros
🔌 Servicios                    /servicios
💲 Planes de comisiones         /planes-comisiones
🔀 Workflows                    /workflows

── SEGURIDAD ──
👥 Usuarios                     /usuarios
🛡 Roles y Permisos             /roles
📑 Auditoría                    /auditoria
```

### 4.2 Rutas del Sistema

| Ruta | Componente | Acceso | Descripción |
|------|-----------|--------|-------------|
| `/login` | LoginPage | Público | Inicio de sesión |
| `/recuperar-clave` | ForgotPasswordPage | Público | Recuperación de contraseña |
| `/` | Dashboard | Autenticado | Panel principal con métricas y gráficos |
| `/cambiar-clave` | ChangePasswordPage | Autenticado | Cambio de contraseña con OTP |
| `/tenants` | TenantList | Autenticado | Listado de organizaciones |
| `/tenants/nuevo` | TenantForm | Autenticado | Crear organización |
| `/tenants/:id` | TenantDetail | Autenticado | Detalle de organización |
| `/tenants/:id/editar` | TenantForm | Autenticado | Editar organización |
| `/estructura` | OrgTreePage | Autenticado | Árbol jerárquico organizacional |
| `/entidades` | EntityList | Autenticado | Listado de entidades |
| `/entidades/nuevo` | EntityForm | Autenticado | Crear entidad |
| `/entidades/:id` | EntityDetail | Autenticado | Detalle de entidad (con solapas) |
| `/entidades/:id/editar` | EntityForm | Autenticado | Editar entidad |
| `/entidades/:entityId/sucursales/nuevo` | BranchForm | Autenticado | Crear sucursal |
| `/entidades/:entityId/sucursales/:branchId/editar` | BranchForm | Autenticado | Editar sucursal |
| `/solicitudes` | ApplicationList | Autenticado | Listado de solicitudes |
| `/solicitudes/nuevo` | ApplicationForm | Autenticado | Crear solicitud |
| `/solicitudes/:id` | ApplicationDetail | Autenticado | Detalle de solicitud (7 solapas) |
| `/solicitudes/:id/editar` | ApplicationForm | Autenticado | Editar solicitud |
| `/liquidacion-comisiones` | CommissionSettlementList | Autenticado | Grilla de liquidación |
| `/liquidacion-comisiones/:id` | CommissionSettlementDetail | Autenticado | Detalle de liquidación |
| `/historial-liquidaciones` | SettlementHistoryList | Autenticado | Historial de liquidaciones |
| `/familias-productos` | ProductFamilyList | Autenticado | Gestión de familias de productos |
| `/productos` | ProductList | Autenticado | Catálogo de productos |
| `/productos/nuevo` | ProductForm | Autenticado | Crear producto |
| `/productos/:id` | ProductDetail | Autenticado | Detalle de producto |
| `/productos/:id/editar` | ProductForm | Autenticado | Editar producto |
| `/parametros` | ParametersPage | Autenticado | Configuración por grupos |
| `/servicios` | ServicesPage | Autenticado | Servicios externos |
| `/planes-comisiones` | CommissionPlanList | Autenticado | Planes de comisiones |
| `/workflows` | WorkflowList | Autenticado | Definiciones de workflow |
| `/workflows/nuevo` | WorkflowEditor | Autenticado | Crear workflow (editor visual) |
| `/workflows/:id/editar` | WorkflowEditor | Autenticado | Editar workflow (editor visual) |
| `/usuarios` | UserList | Autenticado | Listado de usuarios |
| `/usuarios/nuevo` | UserForm | Autenticado | Crear usuario |
| `/usuarios/:id` | UserDetail | Autenticado | Detalle de usuario |
| `/usuarios/:id/editar` | UserForm | Autenticado | Editar usuario |
| `/roles` | RolesPage | Autenticado | Gestión de roles |
| `/roles/nuevo` | RoleForm | Autenticado | Crear rol |
| `/roles/:id/editar` | RoleForm | Autenticado | Editar rol y permisos |
| `/auditoria` | AuditPage | Autenticado | Log de auditoría |

---

## 5. Módulos Funcionales

### 5.1 Dashboard

**Componentes:** `Dashboard.tsx`, `MetricCard.tsx`

**Secciones:**

1. **Análisis por Organización:** Gráficos de barras agrupados por organización mostrando cantidad de solicitudes y monto total por entidad. Filtro de rango de fechas con calendario.

2. **Análisis por Entidad:** Gráficos de barras por entidad mostrando productos con cantidad y valor representativo. El "valor representativo" se calcula según la categoría del producto:
   - Préstamos: `price`
   - Tarjetas: `creditLimit`
   - Cuentas: `maintenanceFee`
   - Inversiones: `minimumAmount`
   - Seguros: suma de `sumInsured` de coberturas

3. **Métricas resumen:** 4 tarjetas con:
   - Entidades activas (total y activas)
   - Usuarios activos (registrados y activos)
   - Productos activos (catálogo y activos)
   - Solicitudes pendientes (totales y en proceso)

4. **Actividad reciente:** Últimas 5 entradas del log de auditoría.

### 5.2 Organización

#### 5.2.1 Organizaciones (Tenants)

**CRUD completo** con campos: Nombre, Identificador (CUIT), Descripción, Estado, Contacto (nombre, email, código telefónico, teléfono), País.

#### 5.2.2 Entidades

**CRUD completo** con campos: Organización (select), Nombre, Identificador (CUIT), Tipo (bank/insurance/fintech/cooperative), Estado, Email, Teléfono, Dirección completa, Canales habilitados (multiselect).

**Detalle de entidad:** Información general, sucursales (con CRUD inline) y solapa de productos asociados con gestión completa.

#### 5.2.3 Sucursales

**CRUD** dentro del contexto de una entidad. Campos: Nombre, Código, Dirección, Ciudad, Provincia, Estado, Gerente, Teléfono.

#### 5.2.4 Estructura Organizacional

Vista de árbol jerárquico: Organización → Entidades → Sucursales.

### 5.3 Operaciones

#### 5.3.1 Solicitudes

**Listado:** Columnas Código, Solicitante (nombre + tipo/nro documento), Entidad, Plan, Producto (familia + precio), Estado (badge), Etapa workflow, Fechas creación/actualización.

**Detalle:** 7 solapas:
1. **Solicitante:** Nombre, documento, email, teléfono, fecha nacimiento, edad (calculada), género, ocupación.
2. **Producto:** Datos del producto y plan asignado con atributos técnicos según categoría.
3. **Domicilio:** Calle, número, piso, departamento, ciudad, provincia, código postal, coordenadas.
4. **Beneficiarios:** Nombre, apellido, relación, porcentaje.
5. **Documentos:** Nombre, tipo, estado (pending/approved/rejected), fecha.
6. **Observaciones:** Texto, usuario, timestamp.
7. **Trazabilidad:** Timeline visual del workflow con estados completados, actual y pendientes. Detalle de cada evento con fecha, usuario y acción.

**Acciones de cabecera:** Botones de envío de Email/SMS/WhatsApp (visibilidad condicional según parámetros habilitados), Editar.

#### 5.3.2 Liquidación de Comisiones

Grilla filtrada por defecto a solicitudes "Aprobadas". Selección múltiple con checkboxes. Cálculo automático de comisiones según fórmula del plan. Flujo de confirmación con resumen de totales por moneda. Generación de reporte Excel con fórmula de cálculo legible.

#### 5.3.3 Historial de Liquidaciones

Registro de liquidaciones realizadas con fecha, usuario, cantidad de operaciones, totales por moneda, descarga de Excel almacenado.

### 5.4 Catálogo de Productos

#### 5.4.1 Familias de Productos

CRUD con campos Código (máx 15 chars) y Descripción (máx 30 chars). El prefijo del código determina la categoría.

#### 5.4.2 Productos

CRUD con: Entidad, Familia, Nombre, Código, Descripción, Estado (draft/active/inactive/deprecated), Vigencia desde/hasta, Versión.

En modo edición incluye:
- Grilla de **Sub Productos / Planes** con coberturas, precio, moneda, cuotas, plan de comisión y atributos técnicos por categoría.
- Grilla de **Requisitos** (tipo document/data/validation, obligatorio sí/no).

#### 5.4.3 Planes de Comisiones

CRUD con campos: Código, Descripción, Tipo de comisión, Valor, Monto máximo. Relación 1:1 con sub productos.

### 5.5 Configuración

#### 5.5.1 Parámetros del Sistema

Panel con barra lateral de grupos organizados por categorías:

**General:** Canales, Monedas, Caract. telefónica, Generales, Máscaras, Seguridad, Tipos de Entidades  
**Técnicos:** Integraciones, Workflow  
**Notificaciones:** Email (SMTP), WhatsApp (Meta API), SMS (Twilio/etc.), Plantillas  
**Datos:** Países, Provincias (jerárquico → País), Ciudades (jerárquico → Provincia), Red de tarjetas, Niveles de tarjetas, Coberturas de seguros

Cada parámetro tiene: clave, valor, descripción, tipo (text/number/boolean/select/list/html), opciones y fecha de actualización.

Las **plantillas** se agrupan visualmente en tarjetas con 3 campos: título, formato (texto/html) y contenido.

#### 5.5.2 Servicios Externos

CRUD con: Nombre, Descripción, Tipo (REST API/MCP/GraphQL/SOAP/Webhook), URL base, Estado, Timeout, Reintentos.

Configuración de autenticación según tipo:
- **none:** Sin autenticación
- **api_key:** Header, valor, ubicación (header/query)
- **bearer_token:** Token
- **basic_auth:** Usuario y contraseña
- **oauth2:** Client ID, Secret, Token URL, Scopes, Grant Type
- **custom_header:** Headers personalizados (clave/valor)

Prueba de conexión con resultado (success/failure) y timestamp.

#### 5.5.3 Workflows

Listado con vista previa de estados y transiciones. Editor visual con:
- Nodos: Inicio, Fin, Consulta Servicio, Decisión, Envío Mensaje, Captura Datos, Temporizador
- Configuración por nodo: servicio, endpoint, método, condición, canal, template, campos, timer
- Panel de atributos arrastrables para nodos de Decisión

### 5.6 Seguridad

#### 5.6.1 Autenticación

- Login con usuario/contraseña
- Validación de estado de cuenta (bloqueada/inactiva)
- Recuperación de contraseña por email
- Cambio de contraseña con OTP de 6 dígitos

#### 5.6.2 Usuarios

CRUD con: Username, Contraseña, Email, Nombre, Apellido, Entidad, Roles (multiselect), Estado.

Asignaciones jerárquicas: organizaciones, entidades y sucursales asignadas.

#### 5.6.3 Roles y Permisos

88 permisos atómicos en 15 módulos. Los permisos son aditivos entre roles.

7 roles predefinidos: Super Admin, Admin Entidad, Operador, Auditor, Consulta, Diseñador de Procesos, Admin Producto.

#### 5.6.4 Auditoría

Log inmutable con: Fecha/hora, Usuario, Tipo de operación, Acción, Módulo, Detalle, IP. Sin opciones de editar o eliminar. Exportación a Excel/CSV.

---

## 6. Modelo de Datos

### 6.1 Diagrama de Relaciones

```
Tenant (1) ──── (N) Entity (1) ──── (N) Branch
                         │
                         ├──── (N) User
                         │
                         └──── (N) Product (1) ──── (N) ProductPlan (1) ──── (1) CommissionPlan
                                     │                      │
                                     │                      └──── (N) Coverage
                                     │
                                     └──── (N) ProductRequirement
                                     
Product (1) ──── (N) Application (1) ──── (1) Applicant
                         │                     └──── (1) ApplicationAddress
                         │
                         ├──── (N) Beneficiary
                         ├──── (N) ApplicationDocument
                         ├──── (N) ApplicationObservation
                         └──── (N) TraceEvent

WorkflowDefinition (1) ──── (N) WorkflowState
                    │
                    └──── (N) WorkflowTransition

ProductFamily ←──── Product.familyId
```

### 6.2 Tipos Enumerados

```typescript
EntityStatus: 'active' | 'inactive' | 'suspended'
UserStatus: 'active' | 'inactive' | 'locked'
ProductStatus: 'draft' | 'active' | 'inactive' | 'deprecated'
ApplicationStatus: 'draft' | 'pending' | 'in_review' | 'approved' | 'rejected' | 'cancelled' | 'settled'
WorkflowStatus: 'draft' | 'active' | 'inactive'
TenantStatus: 'active' | 'inactive'
ChannelType: 'web' | 'mobile' | 'api' | 'presencial'
EntityType: 'bank' | 'insurance' | 'fintech' | 'cooperative'
DocumentType: 'DNI' | 'CUIT' | 'passport'
Gender: 'male' | 'female' | 'other' | 'not_specified'
AmortizationType: 'french' | 'german' | 'american' | 'bullet'
CardNetwork: 'visa' | 'mastercard' | 'amex' | 'cabal' | 'naranja'
RiskLevel: 'low' | 'medium' | 'high'
InstrumentType: 'fixed_term' | 'bond' | 'mutual_fund' | 'stock'
CommissionValueType: 'fixed_per_sale' | 'percentage_capital' | 'percentage_total_loan'
AuditOperationType: 'Crear' | 'Editar' | 'Eliminar' | 'Login' | 'Logout' | 'Cambiar Contraseña' | 'Cambiar Estado' | 'Liquidar' | 'Exportar' | 'Consultar' | 'Otro'
ServiceType: 'rest_api' | 'mcp' | 'graphql' | 'soap' | 'webhook'
AuthType: 'none' | 'api_key' | 'bearer_token' | 'basic_auth' | 'oauth2' | 'custom_header'
ServiceStatus: 'active' | 'inactive' | 'error'
FlowNodeType: 'start' | 'end' | 'service_call' | 'decision' | 'send_message' | 'data_capture' | 'timer'
```

---

## 7. Sistema de Parametrización

### 7.1 Grupos de Parámetros

| Grupo | Ícono | Categoría | Cantidad |
|-------|-------|-----------|----------|
| `general` | Settings | General | 5 |
| `security` | Shield | General | 5 |
| `notifications` | Mail | Notificaciones | 12 |
| `whatsapp` | MessageSquare | Notificaciones | 12 |
| `sms` | Smartphone | Notificaciones | 12 |
| `templates` | FileText | Notificaciones | ~16 |
| `workflow` | GitBranch | Técnicos | 3 |
| `integrations` | Plug | Técnicos | 4 |
| `masks` | Regex | General | 14 |
| `entity_types` | Building2 | General | 6 |
| `channels` | Globe | General | 6 |
| `provinces` | MapPin | Datos | 24 |
| `cities` | MapPinned | Datos | ~70 |
| `card_networks` | CreditCard | Datos | 6 |
| `card_levels` | CreditCard | Datos | 6 |
| `insurance_coverages` | ShieldCheck | Datos | ~14 |
| `countries` | Globe2 | Datos | 5 |
| `currencies` | Banknote | Datos | 18 |
| `phone_codes` | Phone | Datos | ~70 |

### 7.2 Jerarquía de Parámetros

Los parámetros soportan relaciones jerárquicas mediante `parentKey`:
- Provincias → País (`parentKey: 'country.argentina'`)
- Ciudades → Provincia (`parentKey: 'province.buenos_aires'`)

Esto permite filtrar en la UI por el padre correspondiente.

### 7.3 Tipos de Parámetros

| Tipo | Descripción | Control UI |
|------|-------------|-----------|
| `text` | Texto libre | Input |
| `number` | Numérico | Input number |
| `boolean` | Verdadero/Falso | Switch |
| `select` | Selección de opciones | Select/Dropdown |
| `list` | Lista de ítems (code/description) | Editor de lista CRUD |
| `html` | Contenido HTML | Editor HTML visual |

---

## 8. Sistema de Notificaciones

### 8.1 Canales

| Canal | Formato Plantilla | Parámetro habilitación | Proveedor |
|-------|------------------|----------------------|-----------|
| Email | HTML | `notifications.email_enabled` | SMTP configurable |
| SMS | Texto | `sms.enabled` | Twilio / SMS Masivos / Vonage / Infobip |
| WhatsApp | Texto | `whatsapp.enabled` | Meta WhatsApp Business API |

### 8.2 Plantillas

Cada plantilla tiene 3 campos: `titulo`, `formato` (texto/html), `contenido`.

**Plantillas predefinidas:**
- Bienvenida (html)
- Aprobación (html)
- Rechazo (html)
- Reset de contraseña (html)

### 8.3 Variables Disponibles

| Variable | Descripción |
|----------|-------------|
| `{{solicitante.nombre}}` | Nombre del solicitante |
| `{{solicitante.apellido}}` | Apellido del solicitante |
| `{{solicitante.email}}` | Email del solicitante |
| `{{solicitante.telefono}}` | Teléfono del solicitante |
| `{{solicitante.documento}}` | Tipo + número de documento |
| `{{solicitud.codigo}}` | Código de la solicitud |
| `{{solicitud.estado}}` | Estado actual |
| `{{producto.nombre}}` | Nombre del producto |
| `{{plan.nombre}}` | Nombre del plan |
| `{{plan.precio}}` | Precio del plan |
| `{{plan.moneda}}` | Moneda del plan |
| `{{entidad.nombre}}` | Nombre de la entidad |
| `{{organizacion.nombre}}` | Nombre de la organización |
| `{{usuario.nombre}}` | Nombre del usuario asignado |
| `{{usuario.email}}` | Email del usuario |
| `{{sistema.fecha}}` | Fecha actual del sistema |
| `{{sucursal.nombre}}` | Nombre de la sucursal |

---

## 9. Validaciones y Máscaras

| Máscara | Patrón | Ejemplo |
|---------|--------|---------|
| CUIT | `XX-XXXXXXXX-X` | 30-71234567-9 |
| DNI | `7-8 dígitos` | 35456789 |
| Teléfono AR | `+54 XX XXXX-XXXX` | +54 11 4321-5678 |
| Email | RFC 5322 | user@domain.com |
| CBU | `22 dígitos` | 0110000000000000000001 |
| Alias CBU | `6-20 chars alfanum` | mi.alias.cbu |
| CPA | `X0000XXX` | C1043AAZ |
| Monto | `12+2 decimales` | 1500000.50 |
| Fecha | `DD/MM/YYYY` | 15/03/2026 |
| Pasaporte AR | `3 letras + 6 dígitos` | AAA123456 |
| IPv4 | `X.X.X.X` | 192.168.1.100 |
| Patente AR | `AA000AA` o `AAA000` | AB123CD |
| Username | `3-30 chars` | admin.sistema |
| Password | `8-50 chars, mayúscula, minúscula, número, especial` | Admin2024! |

---

## 10. Componentes Compartidos Frontend

### 10.1 DataTable

Componente genérico reutilizable para todas las grillas del sistema.

**Props:**
- `data: T[]` — Datos a renderizar
- `columns: Column<T>[]` — Definición de columnas
- `searchPlaceholder?: string` — Placeholder de búsqueda
- `pageSize?: number` — Registros por página
- `onRowClick?: (item: T) => void` — Click en fila
- `actions?: React.ReactNode` — Acciones en cabecera
- `exportFileName?: string` — Nombre del archivo de exportación
- `defaultFilters?: Record<string, ...>` — Filtros por defecto
- `onFilteredDataChange?: (data: T[]) => void` — Callback de datos filtrados

**Funcionalidades:**
- Búsqueda global por columnas marcadas como `searchable`
- Filtros por columna (text, date, select, multiselect)
- Ordenamiento por columna (text, number)
- Paginación
- Exportación a Excel/CSV (vía `xlsx`)
- Selección múltiple con checkboxes

### 10.2 MetricCard

Tarjeta de métricas con ícono, título, valor, subtítulo y tendencia.

### 10.3 StatusBadge

Badge semántico con colores según estado.

### 10.4 PhoneCodeSelect

Selector de código telefónico internacional con banderas.

### 10.5 SendMessageDialog

Diálogo de envío de Email/SMS/WhatsApp con:
- Selector de plantilla filtrado por formato (html→email, texto→SMS/WhatsApp)
- Vista previa con variables reemplazadas
- Botón de envío

### 10.6 DeleteConfirmDialog

Diálogo de confirmación de eliminación reutilizable.

---

## 11. Permisos y Roles

### 11.1 Módulos y Permisos (88 permisos atómicos)

| Módulo | Permisos | IDs |
|--------|----------|-----|
| Organizaciones | Listar, Ver Detalle, Crear, Editar, Eliminar, Exportar | `p_org_*` |
| Entidades | Listar, Ver Detalle, Crear, Editar, Eliminar, Exportar | `p_ent_*` |
| Sucursales | Listar, Crear, Editar, Eliminar | `p_branch_*` |
| Solicitudes | Listar, Ver Detalle, Crear, Editar, Eliminar, Aprobar, Rechazar, Asignar, Agregar Observaciones, Gestionar Documentos, Ver Trazabilidad, Exportar | `p_app_*` |
| Familias de Productos | Listar, Crear, Editar, Eliminar | `p_fam_*` |
| Productos | Listar, Ver Detalle, Crear, Editar, Eliminar, Exportar | `p_prod_*` |
| Sub Productos | Crear, Editar, Eliminar | `p_plan_*` |
| Requisitos | Crear, Editar, Eliminar | `p_req_*` |
| Planes de Comisiones | Listar, Crear, Editar, Eliminar | `p_comm_*` |
| Workflows | Listar, Ver Detalle, Crear, Editar, Eliminar, Diseñar Flujos | `p_wf_*` |
| Usuarios | Listar, Ver Detalle, Crear, Editar, Eliminar, Bloquear/Desbloquear, Exportar | `p_user_*` |
| Roles | Listar, Crear, Editar, Eliminar | `p_role_*` |
| Auditoría | Consultar, Exportar | `p_audit_*` |
| Parámetros | Listar, Crear, Editar, Eliminar | `p_param_*` |
| Servicios Externos | Listar, Crear, Editar, Eliminar, Probar Conexión | `p_svc_*` |

### 11.2 Roles Predefinidos

| Rol | Permisos | Usuarios |
|-----|----------|----------|
| Super Admin | Todos (88) | 2 |
| Admin Entidad | 36 permisos (org lectura + entidades + solicitudes + usuarios) | 5 |
| Operador | 14 permisos (solicitudes + entidades lectura) | 12 |
| Auditor | 22 permisos (lectura global + auditoría) | 3 |
| Consulta | 18 permisos (lectura global sin auditoría) | 8 |
| Diseñador de Procesos | 12 permisos (workflows + lectura) | 2 |
| Admin Producto | 16 permisos (catálogo completo) | 3 |

---

## 12. API Gateway y Contratos

### 12.1 Convenciones de API

- **Base URL:** `https://api.unazul.com/api/v1`
- **Autenticación:** JWT Bearer Token en header `Authorization`
- **Content-Type:** `application/json`
- **Paginación:** `?page=1&pageSize=10`
- **Filtros:** `?status=active&entityId=xxx`
- **Ordenamiento:** `?sortBy=createdAt&sortDir=desc`
- **Búsqueda:** `?search=texto`

### 12.2 Respuestas Estándar

```json
// Lista paginada
{
  "data": [...],
  "totalCount": 100,
  "page": 1,
  "pageSize": 10,
  "totalPages": 10
}

// Detalle
{
  "data": { ... }
}

// Error
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "El campo nombre es requerido",
    "details": [...]
  }
}
```

### 12.3 Eventos de Dominio (RabbitMQ)

| Evento | Productor | Consumidores |
|--------|-----------|-------------|
| `ApplicationCreated` | Operations | Audit, Notification |
| `ApplicationStatusChanged` | Operations | Audit, Notification |
| `UserCreated` | Identity | Audit |
| `UserLocked` | Identity | Audit, Notification |
| `SettlementCompleted` | Operations | Audit |
| `PasswordChanged` | Identity | Audit |
| `EntityCreated` | Organization | Audit |
| `DocumentApproved` | Operations | Notification |

---

## 13. Infraestructura y Despliegue

### 13.1 Contenedores Docker

```yaml
services:
  api-gateway:
    image: unazul/api-gateway:latest
    ports: ["8080:8080"]
  
  identity-service:
    image: unazul/identity-service:latest
    environment:
      - ConnectionStrings__Default=Host=postgres;Database=identity_db
  
  organization-service:
    image: unazul/organization-service:latest
  
  catalog-service:
    image: unazul/catalog-service:latest
  
  operations-service:
    image: unazul/operations-service:latest
  
  config-service:
    image: unazul/config-service:latest
  
  audit-service:
    image: unazul/audit-service:latest
  
  notification-service:
    image: unazul/notification-service:latest
  
  postgres:
    image: postgres:16
  
  redis:
    image: redis:7
  
  rabbitmq:
    image: rabbitmq:3.13-management
```

### 13.2 Kubernetes

- Cada microservicio como Deployment con HPA (Horizontal Pod Autoscaler)
- Ingress Controller con TLS
- ConfigMaps y Secrets para configuración
- PersistentVolumeClaim para PostgreSQL

---

## 14. Seguridad y Cumplimiento

### 14.1 Autenticación y Autorización

- JWT con refresh tokens y rotación
- Expiración de sesión configurable (default: 30 min)
- Bloqueo de cuenta tras N intentos fallidos (default: 5)
- Expiración de contraseña configurable (default: 90 días)
- Segundo factor de autenticación (OTP vía email)
- Policy-based authorization en .NET Core

### 14.2 Protección de Datos

- Contraseñas hasheadas con bcrypt/Argon2
- Tokens de API enmascarados en UI (`••••••••`)
- Comunicación HTTPS/TLS obligatoria
- Headers de seguridad (HSTS, CSP, X-Frame-Options)

### 14.3 Auditoría y Trazabilidad

- Log inmutable de todas las operaciones
- Dirección IP del cliente en cada registro
- Sin opciones de edición o eliminación de entradas
- Exportación para cumplimiento regulatorio

### 14.4 Multi-Tenancy

- Aislamiento de datos por tenant a nivel de base de datos (schema por tenant o filtro por TenantId)
- Row-Level Security en PostgreSQL
- Validación de pertenencia en cada operación

---

> **Documento generado:** 2026-03-15  
> **Próxima revisión:** Al implementar backend .NET Core 10
