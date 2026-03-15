# 05 — Modelo de Datos

> **Proyecto:** Unazul Backoffice
> **Version:** 1.0.0
> **Fecha:** 2026-03-15
> **Prerequisitos:** `01_alcance_funcional.md`, `02_arquitectura.md`

---

## Principios

1. **Database-per-service:** cada microservicio tiene su propia base de datos PostgreSQL. No se comparten esquemas ni conexiones entre servicios.
2. **RLS obligatorio:** toda tabla con datos multi-tenant incluye columna `tenant_id` y una policy RLS `USING (tenant_id = current_setting('app.current_tenant')::uuid)`. El middleware setea `SET app.current_tenant` por request.
3. **Columnas de auditoria:** todas las tablas incluyen `created_at`, `updated_at`, `created_by`, `updated_by`.
4. **PKs:** `uuid` generado por la aplicacion (UUIDv7 recomendado para orden temporal).
5. **Soft delete:** no se usa. Las eliminaciones son fisicas con evento de auditoria via RabbitMQ.
6. **Naming:** snake_case para tablas y columnas. Nombres en ingles.

---

## 1. SA.identity (Identity Service)

Responsabilidad: autenticacion, usuarios, roles, permisos.

```mermaid
erDiagram
    users {
        uuid id PK
        uuid tenant_id FK
        string username UK
        string password_hash
        string email
        string first_name
        string last_name
        uuid entity_id "nullable - referencia logica a SA.organization"
        string entity_name "denormalizado para display"
        user_status status "active | inactive | locked"
        timestamp last_login "nullable"
        string avatar "nullable - URL"
        timestamp created_at
        timestamp updated_at
        uuid created_by
        uuid updated_by
    }

    user_assignments {
        uuid id PK
        uuid user_id FK
        string scope_type "organization | entity | branch"
        uuid scope_id "id de la org, entidad o sucursal asignada"
        string scope_name "denormalizado para display"
    }

    roles {
        uuid id PK
        uuid tenant_id FK
        string name UK
        string description
        timestamp created_at
        timestamp updated_at
        uuid created_by
        uuid updated_by
    }

    permissions {
        uuid id PK
        string module "ej: organizaciones, solicitudes"
        string action "ej: listar, crear, editar"
        string code UK "ej: p_org_list"
        string description
    }

    user_roles {
        uuid user_id FK
        uuid role_id FK
    }

    role_permissions {
        uuid role_id FK
        uuid permission_id FK
    }

    refresh_tokens {
        uuid id PK
        uuid user_id FK
        string token_hash UK
        timestamp expires_at
        boolean revoked
        timestamp created_at
    }

    users ||--o{ user_assignments : "asignado a"
    users ||--o{ user_roles : "tiene"
    roles ||--o{ user_roles : "asignado a"
    roles ||--o{ role_permissions : "tiene"
    permissions ||--o{ role_permissions : "otorgado en"
    users ||--o{ refresh_tokens : "tiene"
```

**RLS:** aplica a `users`, `user_assignments`, `roles`, `user_roles`, `role_permissions`, `refresh_tokens`.
**Nota:** `permissions` es tabla de referencia global, sin `tenant_id`.

**Indices clave:**
- `users(tenant_id, username)` UNIQUE
- `users(tenant_id, email)` UNIQUE
- `users(tenant_id, entity_id)`
- `users(tenant_id, status)`
- `user_assignments(user_id, scope_type, scope_id)` UNIQUE
- `refresh_tokens(token_hash)` UNIQUE

---

## 2. SA.organization (Organization Service)

Responsabilidad: tenants (organizaciones), entidades, sucursales.

```mermaid
erDiagram
    tenants {
        uuid id PK
        string name
        string identifier UK "CUIT"
        string description
        tenant_status status "active | inactive"
        string contact_name
        string contact_email
        string contact_phone_code
        string contact_phone
        string country
        timestamp created_at
        timestamp updated_at
        uuid created_by
        uuid updated_by
    }

    entities {
        uuid id PK
        uuid tenant_id FK
        string name
        string identifier "CUIT"
        entity_type type "bank | insurance | fintech | cooperative | sgr | regional_card"
        entity_status status "active | inactive | suspended"
        string email
        string phone_code
        string phone
        string address
        string city
        string province
        string country
        timestamp created_at
        timestamp updated_at
        uuid created_by
        uuid updated_by
    }

    entity_channels {
        uuid id PK
        uuid entity_id FK
        channel_type channel "web | mobile | api | presencial | ia_agent"
    }

    branches {
        uuid id PK
        uuid entity_id FK
        uuid tenant_id FK
        string name
        string code
        string address
        string city
        string province
        entity_status status "active | inactive | suspended"
        string manager
        string phone_code
        string phone
        timestamp created_at
        timestamp updated_at
        uuid created_by
        uuid updated_by
    }

    tenants ||--o{ entities : "agrupa"
    entities ||--o{ entity_channels : "habilita"
    entities ||--o{ branches : "tiene"
```

**RLS:** aplica a `entities`, `entity_channels`, `branches`. `tenants` no lleva RLS (el tenant_id ES el id de la fila; el acceso se controla por claim).

**Indices clave:**
- `tenants(identifier)` UNIQUE
- `entities(tenant_id, identifier)` UNIQUE
- `entities(tenant_id, status)`
- `entity_channels(entity_id, channel)` UNIQUE
- `branches(entity_id)`
- `branches(tenant_id, code)` UNIQUE

---

## 3. SA.catalog (Catalog Service)

Responsabilidad: familias, productos, sub-productos (planes), coberturas, requisitos, comisiones.

```mermaid
erDiagram
    product_families {
        uuid id PK
        uuid tenant_id FK
        string code UK "max 15 chars - prefijo determina categoria"
        string description "max 30 chars"
        timestamp created_at
        timestamp updated_at
        uuid created_by
        uuid updated_by
    }

    products {
        uuid id PK
        uuid tenant_id FK
        uuid entity_id "referencia logica a SA.organization"
        uuid family_id FK
        string name
        string code
        string description
        product_status status "draft | active | inactive | deprecated"
        date valid_from
        date valid_to "nullable"
        integer version
        timestamp created_at
        timestamp updated_at
        uuid created_by
        uuid updated_by
    }

    product_plans {
        uuid id PK
        uuid product_id FK
        uuid tenant_id FK
        string name
        string code
        decimal price
        string currency "ARS, USD, etc"
        integer installments "nullable"
        uuid commission_plan_id FK "nullable"
        timestamp created_at
        timestamp updated_at
        uuid created_by
        uuid updated_by
    }

    plan_loan_attributes {
        uuid id PK
        uuid plan_id FK "1:1"
        amortization_type amortization_type "french | german | american | bullet"
        decimal annual_effective_rate
        decimal cft_rate "nullable"
        decimal admin_fees "nullable"
    }

    plan_insurance_attributes {
        uuid id PK
        uuid plan_id FK "1:1"
        decimal premium
        decimal sum_insured
        integer grace_period_days "nullable"
        string coverage_type
    }

    plan_account_attributes {
        uuid id PK
        uuid plan_id FK "1:1"
        decimal maintenance_fee
        decimal minimum_balance "nullable"
        decimal interest_rate "nullable"
        string account_type
    }

    plan_card_attributes {
        uuid id PK
        uuid plan_id FK "1:1"
        decimal credit_limit
        decimal annual_fee
        decimal interest_rate "nullable"
        card_network network "visa | mastercard | amex | cabal | naranja"
        string level "gold | platinum | black | etc"
    }

    plan_investment_attributes {
        uuid id PK
        uuid plan_id FK "1:1"
        decimal minimum_amount
        decimal expected_return "nullable"
        integer term_days "nullable"
        risk_level risk_level "low | medium | high"
    }

    coverages {
        uuid id PK
        uuid plan_id FK
        uuid tenant_id FK
        string name
        string coverage_type
        decimal sum_insured
        decimal premium "nullable"
        integer grace_period_days "nullable"
        timestamp created_at
        timestamp updated_at
    }

    product_requirements {
        uuid id PK
        uuid product_id FK
        uuid tenant_id FK
        string name
        string type "document | data | validation"
        boolean is_mandatory
        string description "nullable"
        timestamp created_at
        timestamp updated_at
    }

    commission_plans {
        uuid id PK
        uuid tenant_id FK
        string code UK
        string description
        commission_value_type type "fixed_per_sale | percentage_capital | percentage_total_loan"
        decimal value
        decimal max_amount "nullable"
        timestamp created_at
        timestamp updated_at
        uuid created_by
        uuid updated_by
    }

    product_families ||--o{ products : "agrupa"
    products ||--o{ product_plans : "tiene"
    products ||--o{ product_requirements : "requiere"
    product_plans ||--o| plan_loan_attributes : "PREST"
    product_plans ||--o| plan_insurance_attributes : "SEG"
    product_plans ||--o| plan_account_attributes : "CTA"
    product_plans ||--o| plan_card_attributes : "TARJETA"
    product_plans ||--o| plan_investment_attributes : "INV"
    product_plans ||--o{ coverages : "incluye"
    commission_plans ||--o| product_plans : "aplica a"
```

**Atributos por categoria (tabla 1:1 segun prefijo de familia):**

| Prefijo familia | Tabla de atributos | Relacion |
|---|---|---|
| `PREST` | `plan_loan_attributes` | 1:1 con `product_plans` |
| `SEG` | `plan_insurance_attributes` | 1:1 con `product_plans` |
| `CTA` | `plan_account_attributes` | 1:1 con `product_plans` |
| `TARJETA` | `plan_card_attributes` | 1:1 con `product_plans` |
| `INV` | `plan_investment_attributes` | 1:1 con `product_plans` |

Cada plan tiene exactamente una tabla de atributos segun la categoria de su familia. La aplicacion determina cual tabla usar a partir del prefijo del codigo de familia.

**RLS:** aplica a todas las tablas.

**Indices clave:**
- `product_families(tenant_id, code)` UNIQUE
- `products(tenant_id, entity_id, status)`
- `products(tenant_id, family_id)`
- `product_plans(product_id)`
- `plan_loan_attributes(plan_id)` UNIQUE
- `plan_insurance_attributes(plan_id)` UNIQUE
- `plan_account_attributes(plan_id)` UNIQUE
- `plan_card_attributes(plan_id)` UNIQUE
- `plan_investment_attributes(plan_id)` UNIQUE
- `commission_plans(tenant_id, code)` UNIQUE

---

## 4. SA.operations (Operations Service)

Responsabilidad: solicitudes, liquidaciones, documentos, trazabilidad.

```mermaid
erDiagram
    applications {
        uuid id PK
        uuid tenant_id FK
        uuid entity_id "referencia logica a SA.organization"
        uuid applicant_id FK
        string code UK "generado"
        uuid product_id "referencia logica a SA.catalog"
        uuid plan_id "referencia logica a SA.catalog"
        string product_name "denormalizado"
        string plan_name "denormalizado"
        application_status status "draft | pending | in_review | approved | rejected | cancelled | settled"
        string workflow_stage "nullable - etapa actual del workflow"
        timestamp created_at
        timestamp updated_at
        uuid created_by
        uuid updated_by
    }

    applicants {
        uuid id PK
        uuid tenant_id FK
        string first_name
        string last_name
        document_type document_type "DNI | CUIT | passport"
        string document_number
        date birth_date "nullable"
        gender gender "male | female | other | not_specified"
        string occupation "nullable"
        timestamp created_at
        timestamp updated_at
    }

    applicant_contacts {
        uuid id PK
        uuid applicant_id FK
        contact_type type "personal | work | emergency | other"
        string email "nullable"
        string phone_code "nullable"
        string phone "nullable"
    }

    applicant_addresses {
        uuid id PK
        uuid applicant_id FK
        address_type type "home | work | legal | other"
        string street
        string number
        string floor "nullable"
        string apartment "nullable"
        string city
        string province
        string postal_code
        decimal latitude "nullable"
        decimal longitude "nullable"
    }

    beneficiaries {
        uuid id PK
        uuid application_id FK
        string first_name
        string last_name
        string relationship
        decimal percentage
    }

    application_documents {
        uuid id PK
        uuid application_id FK
        string name
        string document_type
        string file_url
        document_status status "pending | approved | rejected"
        timestamp uploaded_at
        uuid uploaded_by
    }

    application_observations {
        uuid id PK
        uuid application_id FK
        text content
        uuid user_id
        string user_name "denormalizado"
        timestamp created_at
    }

    trace_events {
        uuid id PK
        uuid application_id FK
        string state
        string action
        uuid user_id
        string user_name "denormalizado"
        timestamp occurred_at
    }

    trace_event_details {
        uuid id PK
        uuid trace_event_id FK
        string key "ej: reason, assigned_to, previous_state"
        string value
    }

    settlements {
        uuid id PK
        uuid tenant_id FK
        timestamp settled_at
        uuid settled_by
        string settled_by_name "denormalizado"
        integer operation_count
        string excel_url "nullable - URL del reporte generado"
    }

    settlement_totals {
        uuid id PK
        uuid settlement_id FK
        string currency "ARS, USD, etc"
        decimal total_amount
    }

    settlement_items {
        uuid id PK
        uuid settlement_id FK
        uuid application_id FK
        uuid plan_id "referencia logica"
        string commission_type
        decimal commission_value
        decimal calculated_amount
        string currency
        string formula_description "legible para Excel"
    }

    applicants ||--o{ applications : "solicita"
    applicants ||--o{ applicant_contacts : "contactos"
    applicants ||--o{ applicant_addresses : "direcciones"
    applications ||--o{ beneficiaries : "tiene"
    applications ||--o{ application_documents : "adjunta"
    applications ||--o{ application_observations : "registra"
    applications ||--o{ trace_events : "traza"
    trace_events ||--o{ trace_event_details : "detalla"
    settlements ||--o{ settlement_totals : "totaliza"
    settlements ||--o{ settlement_items : "contiene"
    applications ||--o{ settlement_items : "liquidada en"
```

**RLS:** aplica a `applications`, `applicants`, `settlements`, `settlement_items`. Las tablas hijas heredan filtro via JOIN con `applications.tenant_id`.

**Indices clave:**
- `applicants(tenant_id, document_type, document_number)` UNIQUE
- `applicant_contacts(applicant_id)`
- `applications(tenant_id, status)`
- `applications(tenant_id, entity_id)`
- `applications(tenant_id, applicant_id)`
- `applications(code)` UNIQUE
- `trace_events(application_id, occurred_at)`
- `trace_event_details(trace_event_id)`
- `settlements(tenant_id, settled_at DESC)`
- `settlement_totals(settlement_id)`
- `settlement_items(settlement_id)`
- `settlement_items(application_id)`

---

## 5. SA.config (Config Service)

Responsabilidad: parametros, servicios externos, workflows, plantillas.

```mermaid
erDiagram
    parameter_groups {
        uuid id PK
        string code UK
        string name
        string category "general | tecnico | notificaciones | datos"
        string icon
        integer sort_order
    }

    parameters {
        uuid id PK
        uuid tenant_id FK
        uuid group_id FK
        string key
        string value
        string description
        parameter_type type "text | number | boolean | select | list | html"
        string parent_key "nullable - jerarquia: province -> country"
        timestamp updated_at
        uuid updated_by
    }

    parameter_options {
        uuid id PK
        uuid parameter_id FK
        string option_value
        string option_label
        integer sort_order
    }

    external_services {
        uuid id PK
        uuid tenant_id FK
        string name
        string description
        service_type type "rest_api | mcp | graphql | soap | webhook"
        string base_url
        service_status status "active | inactive | error"
        integer timeout_ms
        integer max_retries
        auth_type auth_type "none | api_key | bearer_token | basic_auth | oauth2 | custom_header"
        timestamp last_tested_at "nullable"
        boolean last_test_success "nullable"
        timestamp created_at
        timestamp updated_at
        uuid created_by
        uuid updated_by
    }

    service_auth_configs {
        uuid id PK
        uuid service_id FK
        string key "ej: header_name, token, client_id, client_secret, token_url, scopes"
        string value_encrypted "valor encriptado"
    }

    workflow_definitions {
        uuid id PK
        uuid tenant_id FK
        string name
        string description
        workflow_status status "draft | active | inactive"
        integer version
        timestamp created_at
        timestamp updated_at
        uuid created_by
        uuid updated_by
    }

    workflow_states {
        uuid id PK
        uuid workflow_id FK
        string name
        string label
        flow_node_type type "start | end | service_call | decision | send_message | data_capture | timer"
        decimal position_x "coordenada X en editor visual"
        decimal position_y "coordenada Y en editor visual"
    }

    workflow_state_configs {
        uuid id PK
        uuid state_id FK
        string key "ej: service_id, endpoint, method, condition, channel, template_id, timer_minutes"
        string value
    }

    workflow_state_fields {
        uuid id PK
        uuid state_id FK "solo para nodos data_capture"
        string field_name
        string field_type "text | number | boolean | date | select"
        boolean is_required
        integer sort_order
    }

    workflow_transitions {
        uuid id PK
        uuid workflow_id FK
        uuid from_state_id FK
        uuid to_state_id FK
        string label "nullable"
        string condition "nullable"
        integer sla_hours "nullable"
    }

    notification_templates {
        uuid id PK
        uuid tenant_id FK
        string title
        string format "text | html"
        text content "con variables {{variable}}"
        string channel "email | sms | whatsapp"
        timestamp updated_at
        uuid updated_by
    }

    parameter_groups ||--o{ parameters : "agrupa"
    parameters ||--o{ parameter_options : "opciones"
    external_services ||--o{ service_auth_configs : "credenciales"
    workflow_definitions ||--o{ workflow_states : "contiene"
    workflow_states ||--o{ workflow_state_configs : "configurado por"
    workflow_states ||--o{ workflow_state_fields : "captura"
    workflow_definitions ||--o{ workflow_transitions : "conecta"
    workflow_states ||--o{ workflow_transitions : "origen"
    workflow_states ||--o{ workflow_transitions : "destino"
```

**RLS:** aplica a `parameters`, `parameter_options`, `external_services`, `service_auth_configs`, `workflow_definitions`, `workflow_states`, `workflow_state_configs`, `workflow_state_fields`, `workflow_transitions`, `notification_templates`. `parameter_groups` es tabla de referencia global.

**Indices clave:**
- `parameters(tenant_id, group_id, key)` UNIQUE
- `parameters(tenant_id, parent_key)` para filtro jerarquico
- `parameter_options(parameter_id, sort_order)`
- `external_services(tenant_id, name)` UNIQUE
- `service_auth_configs(service_id, key)` UNIQUE
- `workflow_definitions(tenant_id, status)`
- `workflow_states(workflow_id)`
- `workflow_state_configs(state_id, key)` UNIQUE
- `workflow_state_fields(state_id, sort_order)`
- `workflow_transitions(workflow_id)`
- `notification_templates(tenant_id, channel)`

---

## 6. SA.audit (Audit Service)

Responsabilidad: log inmutable de acciones del sistema. Solo INSERT, nunca UPDATE ni DELETE.

```mermaid
erDiagram
    audit_log {
        uuid id PK
        uuid tenant_id
        timestamp occurred_at "NOT NULL, particion por mes"
        uuid user_id
        string user_name
        audit_operation_type operation "Crear | Editar | Eliminar | Login | Logout | etc"
        string module "ej: solicitudes, productos"
        string action "descripcion legible"
        string detail "nullable"
        string ip_address
        string entity_type "nullable - tipo de entidad afectada"
        uuid entity_id "nullable - id de entidad afectada"
    }
```

**Particionamiento:** tabla particionada por rango en `occurred_at` (mensual). Las particiones antiguas se archivan.

**Sin RLS:** el filtro por `tenant_id` se aplica en la query de la API, no por RLS, porque el Audit Service es consumidor async y no recibe contexto de request HTTP.

**Dual write:** cada entrada se persiste en PostgreSQL (fuente de verdad) Y se indexa en Elasticsearch (`audit-{yyyy.MM.dd}`) para busqueda full-text.

**Indices clave:**
- `audit_log(tenant_id, occurred_at DESC)` — consulta principal
- `audit_log(tenant_id, module, occurred_at DESC)`
- `audit_log(tenant_id, user_id, occurred_at DESC)`
- `audit_log(entity_type, entity_id)` — buscar historial de una entidad

---

## 7. SA.notification (Notification Service)

Responsabilidad: registro de notificaciones enviadas. Solo INSERT.

```mermaid
erDiagram
    notification_log {
        uuid id PK
        uuid tenant_id
        uuid application_id "nullable"
        string channel "email | sms | whatsapp"
        string recipient "email o telefono destino"
        string template_title "nullable"
        string subject "nullable - solo email"
        text body_preview "primeros 500 chars"
        notification_status status "sent | failed | pending"
        string error_message "nullable"
        string provider "smtp | twilio | meta | etc"
        timestamp sent_at
        uuid sent_by
    }
```

**Sin RLS:** mismo caso que Audit — servicio async sin contexto HTTP.

**Indices clave:**
- `notification_log(tenant_id, sent_at DESC)`
- `notification_log(application_id)`

---

## 8. Tipos enumerados (compartidos)

Cada servicio define sus propios enums en su esquema. Los valores se sincronizan por convencion, no por tabla compartida.

| Enum | Valores | Usado en |
|---|---|---|
| `tenant_status` | active, inactive | SA.organization |
| `entity_status` | active, inactive, suspended | SA.organization |
| `entity_type` | bank, insurance, fintech, cooperative, sgr, regional_card | SA.organization |
| `user_status` | active, inactive, locked | SA.identity |
| `product_status` | draft, active, inactive, deprecated | SA.catalog |
| `application_status` | draft, pending, in_review, approved, rejected, cancelled, settled | SA.operations |
| `workflow_status` | draft, active, inactive | SA.config |
| `contact_type` | personal, work, emergency, other | SA.operations |
| `address_type` | home, work, legal, other | SA.operations |
| `document_type` | DNI, CUIT, passport | SA.operations |
| `gender` | male, female, other, not_specified | SA.operations |
| `document_status` | pending, approved, rejected | SA.operations |
| `commission_value_type` | fixed_per_sale, percentage_capital, percentage_total_loan | SA.catalog |
| `service_type` | rest_api, mcp, graphql, soap, webhook | SA.config |
| `auth_type` | none, api_key, bearer_token, basic_auth, oauth2, custom_header | SA.config |
| `service_status` | active, inactive, error | SA.config |
| `channel_type` | web, mobile, api, presencial, ia_agent | SA.organization |
| `amortization_type` | french, german, american, bullet | SA.catalog |
| `card_network` | visa, mastercard, amex, cabal, naranja | SA.catalog |
| `risk_level` | low, medium, high | SA.catalog |
| `flow_node_type` | start, end, service_call, decision, send_message, data_capture, timer | SA.config |
| `parameter_type` | text, number, boolean, select, list, html | SA.config |
| `audit_operation_type` | Crear, Editar, Eliminar, Login, Logout, Cambiar Contrasena, Cambiar Estado, Liquidar, Exportar, Consultar, Otro | SA.audit |
| `notification_status` | sent, failed, pending | SA.notification |

---

## 9. Referencias cruzadas entre servicios

Los microservicios no hacen JOINs entre bases de datos. Las referencias se manejan asi:

| Servicio origen | Campo | Referencia logica a | Estrategia |
|---|---|---|---|
| SA.identity | `users.entity_id` | SA.organization `entities.id` | UUID almacenado + `entity_name` denormalizado |
| SA.catalog | `products.entity_id` | SA.organization `entities.id` | UUID almacenado; validacion sync via HTTP al crear |
| SA.operations | `applications.product_id`, `plan_id` | SA.catalog | UUID almacenado + `product_name`, `plan_name` denormalizados |
| SA.operations | `applications.entity_id` | SA.organization `entities.id` | UUID almacenado |
| SA.operations | `trace_events.user_id` | SA.identity `users.id` | UUID almacenado + `user_name` denormalizado |
| SA.audit | `audit_log.user_id` | SA.identity `users.id` | UUID + `user_name` denormalizado (viene en el evento) |

**Regla:** cuando se necesita mostrar un nombre de otra base de datos, se denormaliza al momento de escribir. No se hacen lookups sync para lectura.

---

## 10. Politica RLS — Template

Cada tabla multi-tenant aplica esta policy generada por migracion EF Core:

```sql
-- Habilitar RLS
ALTER TABLE {table_name} ENABLE ROW LEVEL SECURITY;
ALTER TABLE {table_name} FORCE ROW LEVEL SECURITY;

-- Policy de tenant
CREATE POLICY tenant_isolation ON {table_name}
    USING (tenant_id = current_setting('app.current_tenant')::uuid);

-- El middleware .NET ejecuta antes de cada query:
-- SET LOCAL app.current_tenant = '{tenant_id_from_jwt}';
```

**Excepciones (sin RLS):**
- `permissions` (SA.identity) — catalogo global
- `parameter_groups` (SA.config) — catalogo global
- `audit_log` (SA.audit) — consumidor async, filtro en query
- `notification_log` (SA.notification) — consumidor async, filtro en query

---

## 11. Almacenamiento de archivos (File Storage)

Los archivos generados y subidos por el sistema se almacenan en el file system, nunca en la base de datos. Las tablas solo guardan la ruta relativa al archivo.

### Ruta base

```
{STORAGE_ROOT}/
```

`STORAGE_ROOT` se configura por variable de entorno (ej: `/data/unazul` en Linux, `D:\unazul-storage` en Windows). En desarrollo se usa una carpeta local; en produccion puede montarse como volumen de red o storage compartido.

### Estructura de carpetas

```
{STORAGE_ROOT}/
├── {tenant_id}/
│   ├── settlements/
│   │   └── {yyyy}/{MM}/
│   │       └── {settlement_id}.xlsx
│   ├── documents/
│   │   └── {application_id}/
│   │       └── {document_id}_{original_filename}
│   └── exports/
│       └── {yyyy}/{MM}/
│           └── {export_type}_{timestamp}.xlsx
```

### Convencion de nombres

| Tipo de archivo | Patron de nombre | Ejemplo |
|---|---|---|
| Reporte de liquidacion | `{settlement_id}.xlsx` | `a1b2c3d4-...-e5f6.xlsx` |
| Documento de solicitud | `{document_id}_{original_filename}` | `d7e8f9a0-...-b1c2_dni_frente.pdf` |
| Exportacion auditoria | `audit_{timestamp}.xlsx` | `audit_20260315_143022.xlsx` |
| Exportacion solicitudes | `applications_{timestamp}.xlsx` | `applications_20260315_150000.xlsx` |

### Reglas

1. **Aislamiento por tenant:** todo archivo se guarda bajo `{STORAGE_ROOT}/{tenant_id}/`. Ningun servicio accede a carpetas de otro tenant.
2. **Solo la ruta relativa en DB:** las columnas `file_url` y `excel_url` almacenan la ruta relativa desde `STORAGE_ROOT` (ej: `{tenant_id}/settlements/2026/03/{id}.xlsx`). La ruta absoluta se resuelve en runtime.
3. **Inmutabilidad de liquidaciones:** los archivos Excel de liquidacion no se sobreescriben ni eliminan. Son registros historicos.
4. **Limpieza de documentos:** cuando se elimina una solicitud, sus archivos en `documents/{application_id}/` se eliminan fisicamente junto con los registros en DB.
5. **Permisos del file system:** el proceso del Operations Service debe tener permisos de lectura/escritura en `{STORAGE_ROOT}`. Ningun otro servicio escribe archivos (Audit exporta via Operations).
