# RF-ORG — Requerimientos Funcionales: Organizacion

> **Proyecto:** Unazul Backoffice
> **Modulo:** Organization (SA.organization)
> **Version:** 1.1.0
> **Fecha:** 2026-03-15
> **Prerequisitos:** `01_alcance_funcional.md`, `02_arquitectura.md`, `03_FL.md`, `05_modelo_datos.md`
> **Flujo origen:** FL-ORG-01 (Gestionar Organizaciones), FL-ORG-02 (Gestionar Entidades y Sucursales)
> **HUs origen:** HU002, HU003, HU004, HU005, HU006, HU007, HU008

---

## Resumen de Requerimientos

| ID | Titulo | Prioridad | Severidad | HU | Estado |
|----|--------|-----------|-----------|-----|--------|
| RF-ORG-01 | Listar organizaciones con busqueda, filtros y exportacion | Alta | P1 | HU002 | Documentado |
| RF-ORG-02 | Crear organizacion con datos de contacto | Alta | P0 | HU003 | Documentado |
| RF-ORG-03 | Editar organizacion con entidades dependientes | Alta | P0 | HU003 | Documentado |
| RF-ORG-04 | Ver detalle de organizacion con entidades dependientes | Media | P2 | HU004 | Documentado |
| RF-ORG-05 | Eliminar organizacion con validacion de dependencias | Alta | P0 | HU002 (*) | Documentado |
| RF-ORG-06 | Listar entidades con filtros, busqueda y exportacion | Alta | P1 | HU005 | Documentado |
| RF-ORG-07 | Crear/editar entidad con tipo, canales y selects en cascada | Alta | P0 | HU006 | Documentado |
| RF-ORG-08 | Ver detalle de entidad con sucursales y usuarios | Media | P2 | HU007 | Documentado |
| RF-ORG-09 | Eliminar/desactivar entidad con validacion de dependencias | Alta | P0 | HU005 (*) | Documentado |
| RF-ORG-10 | CRUD de sucursales con selects en cascada | Alta | P0 | HU008 | Documentado |

> (*) La accion de eliminar se dispara desde la vista de listado (grilla), por lo que se traza a la HU del listado. Si se crean HUs especificas para eliminacion, actualizar esta referencia.

---

## Reglas de Negocio

| ID | Regla | Detalle | Decision |
|----|-------|---------|----------|
| RN-ORG-01 | Unicidad de identificador (CUIT) | `tenants.identifier` es unico a nivel global (indice UNIQUE en `tenants(identifier)`). No se repite entre organizaciones. | — |
| RN-ORG-02 | Eliminacion solo sin entidades | Una organizacion solo puede eliminarse fisicamente si no tiene entidades asociadas (`entities` con ese `tenant_id`). Si tiene entidades, se retorna 409. | D-ORG-01 |
| RN-ORG-03 | Estados de organizacion | `tenant_status`: `active`, `inactive`. No hay estado `locked` ni `suspended`. Una organizacion inactiva puede tener entidades activas (el admin decide). | — |
| RN-ORG-04 | Tenants sin RLS | La tabla `tenants` no lleva Row-Level Security porque `tenant_id` ES el `id` de la fila. El acceso se controla por claim `tenant_id` del JWT y permisos del actor. | — |
| RN-ORG-05 | Super Admin ve todas las organizaciones | El Super Admin puede ver y gestionar todas las organizaciones. Los demas roles solo ven la organizacion a la que pertenecen (filtro por claim del JWT). | — |
| RN-ORG-06 | Eliminacion fisica con evento | La eliminacion de organizacion es fisica (DELETE), no soft delete. Se publica `TenantDeletedEvent` para auditoria. | — |
| RN-ORG-07 | Exportacion limitada | La exportacion (Excel/CSV) aplica los mismos filtros activos en la grilla. Limite maximo de 10,000 registros. | — |
| RN-ORG-08 | Unicidad de CUIT de entidad por tenant | `entities(tenant_id, identifier)` es UNIQUE. Un mismo CUIT puede existir en organizaciones distintas pero no repetirse dentro de la misma. | — |
| RN-ORG-16 | Politica de identifier diferenciada | En `tenants`, el `identifier` es texto libre sin formato (max 20 chars, UNIQUE global). En `entities`, el `identifier` debe cumplir formato CUIT (XX-XXXXXXXX-X, UNIQUE por tenant). La diferencia es intencional: organizaciones pueden tener identificadores de distintos paises, mientras que entidades financieras argentinas usan CUIT. | D-ORG-03 |
| RN-ORG-09 | Eliminacion de entidad condicionada a sucursales | Entidad sin sucursales: eliminacion fisica. Entidad con sucursales: solo desactivar (status → inactive/suspended). | D-ORG-02 |
| RN-ORG-10 | Canales como diff | Al editar entidad, los canales se sincronizan via diff: INSERT los nuevos, DELETE los desmarcados en `entity_channels`. | — |
| RN-ORG-11 | Selects en cascada provincia→ciudad | Las provincias y ciudades se obtienen de parametros del Config Service (cache Redis). Al seleccionar provincia, las ciudades se filtran por `parent_key`. Aplica a entidades y sucursales. | — |
| RN-ORG-12 | Unicidad de codigo de sucursal por tenant | `branches(tenant_id, code)` es UNIQUE. El codigo de sucursal no se repite dentro del mismo tenant. | — |
| RN-ORG-13 | Tipos de entidad parametrizados | Los tipos validos se obtienen del parametro `entity_types` en Config Service: bank, insurance, fintech, cooperative, sgr, regional_card. | — |
| RN-ORG-14 | Entidad eliminada referenciada por otros servicios | Si se elimina una entidad, los datos denormalizados (`entity_name`) en SA.identity y SA.catalog permanecen intactos. El `EntityDeletedEvent` permite limpieza eventual a futuro. | — |
| RN-ORG-15 | Eliminacion fisica de sucursal | Las sucursales se eliminan fisicamente (DELETE) con evento `BranchDeletedEvent`. No hay validacion de uso cross-service (sucursal es hoja del arbol). | — |

---

## RF-ORG-01 — Listar organizaciones con busqueda, filtros y exportacion

### Execution Sheet

| Campo | Valor |
|-------|-------|
| **ID** | RF-ORG-01 |
| **Titulo** | Listar organizaciones con busqueda, filtros y exportacion |
| **Actor(es)** | Super Admin, Admin Entidad (solo su org), Auditor (lectura), Consulta (lectura) |
| **Prioridad** | Alta |
| **Severidad** | P1 |
| **Flujo origen** | FL-ORG-01 seccion 6 (listar) |
| **HU origen** | HU002 |

### Precondiciones

| # | Condicion | Detalle |
|---|-----------|---------|
| 1 | Organization Service operativo | SA.organization accesible |
| 2 | Usuario autenticado | JWT valido con claim `tenant_id` |
| 3 | Permiso requerido | `p_org_list` presente en claims del JWT |

### Entradas

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| page | integer | No (default 1) | Query param | >= 1 | — |
| page_size | integer | No (default 20) | Query param | 1-100 | — |
| search | string | No | Query param | ILIKE sobre `name` e `identifier` | — |
| status | string | No | Query param | Enum: `active`, `inactive` | RN-ORG-03 |
| sort_by | string | No (default `created_at`) | Query param | Enum: `name`, `identifier`, `status`, `country`, `created_at` | — |
| sort_dir | string | No (default `desc`) | Query param | Enum: `asc`, `desc` | — |
| format | string | No | Query param (solo /export) | Enum: `xlsx`, `csv` | RN-ORG-07 |

### Proceso (Happy Path)

| # | Paso | Responsable |
|---|------|-------------|
| 1 | SPA envia `GET /tenants?page&page_size&search&status&sort_by&sort_dir` | Frontend SPA |
| 2 | API Gateway valida JWT y enruta a Organization Service | API Gateway |
| 3 | Organization Service verifica permiso `p_org_list` | Organization Service |
| 4 | Si actor es Super Admin: query sin filtro de tenant. Si otro rol: filtro `WHERE id = {tenant_id del JWT}` | Organization Service |
| 5 | Aplicar filtros de busqueda (ILIKE en `name`, `identifier`) | Organization Service |
| 6 | Aplicar filtro de status si presente | Organization Service |
| 7 | Aplicar ordenamiento y paginacion | Organization Service |
| 8 | Retornar `200 { items[], total, page, page_size }` | Organization Service |

### Salidas

| Campo | Tipo | Destino | Efecto observable |
|-------|------|---------|-------------------|
| items[] | Array de objetos | SPA | Cada item: `{ id, name, identifier, description, status, contact_name, contact_email, country, created_at }` |
| total | integer | SPA | Total de registros que coinciden con filtros |
| page | integer | SPA | Pagina actual |
| page_size | integer | SPA | Tamano de pagina |

### Errores Tipados

| Codigo | Causa | Condicion de disparo | Respuesta esperada |
|--------|-------|---------------------|--------------------|
| `AUTH_UNAUTHORIZED` | JWT ausente o invalido | Header Authorization faltante o token expirado | HTTP 401 |
| `AUTH_FORBIDDEN` | Sin permiso | `p_org_list` no presente en claims | HTTP 403, "Permiso p_org_list requerido" |
| `VALIDATION_ERROR` | Parametros invalidos | `page < 1`, `page_size > 100`, `status` no es enum valido | HTTP 422, detalle de campos invalidos |

### Casos Especiales y Variantes

- **Busqueda vacia:** Si `search` es vacio o no se envia, no se aplica filtro de busqueda.
- **Sin resultados:** Si no hay organizaciones que coincidan, retorna `{ items: [], total: 0 }` con HTTP 200.
- **Exportacion:** `GET /tenants/export?format=xlsx&search&status` aplica los mismos filtros sin paginacion. Limite de 10,000 filas (RN-ORG-07). Si excede, retorna 422 "Demasiados registros para exportar. Aplique filtros mas restrictivos".
- **Roles no-Super Admin:** Solo ven su propia organizacion en el listado (maximo 1 resultado).

### Impacto en Modelo de Datos

| Entidad | Operacion | Campo(s) afectado(s) | Evento |
|---------|-----------|---------------------|--------|
| `tenants` | SELECT | Todos los campos de lectura | — (sin escritura) |

### Criterios de Aceptacion (Gherkin)

```gherkin
Scenario: Super Admin lista todas las organizaciones
  Given un Super Admin autenticado con permiso p_org_list
  And existen 3 organizaciones en el sistema
  When envia GET /tenants?page=1&page_size=20
  Then recibe 200 con items[] de 3 elementos
  And cada item contiene id, name, identifier, status, country, created_at

Scenario: Busqueda por nombre filtra resultados
  Given un Super Admin autenticado
  And existen organizaciones "Banco Norte" y "Fintech Sur"
  When envia GET /tenants?search=Norte
  Then recibe 200 con items[] de 1 elemento
  And el item tiene name = "Banco Norte"

Scenario: Admin Entidad solo ve su organizacion
  Given un Admin Entidad autenticado con tenant_id = "abc-123"
  When envia GET /tenants
  Then recibe 200 con items[] de 1 elemento
  And el item tiene id = "abc-123"

Scenario: Sin permiso retorna 403
  Given un usuario autenticado sin permiso p_org_list
  When envia GET /tenants
  Then recibe 403 con error FORBIDDEN

Scenario: Exportacion Excel con filtros
  Given un Super Admin autenticado
  And existen 5 organizaciones, 3 con status=active
  When envia GET /tenants/export?format=xlsx&status=active
  Then recibe 200 con Content-Type application/vnd.openxmlformats-officedocument.spreadsheetml.sheet
  And el archivo contiene 3 filas de datos
```

### Trazabilidad de Pruebas

| Test ID | Tipo | Descripcion |
|---------|------|-------------|
| TP-ORG-01-01 | Positivo | Lista paginada con campos esperados |
| TP-ORG-01-02 | Positivo | Busqueda ILIKE filtra por nombre e identifier |
| TP-ORG-01-03 | Positivo | Filtro por status retorna solo organizaciones del estado solicitado |
| TP-ORG-01-04 | Positivo | Exportacion Excel genera archivo correcto |
| TP-ORG-01-05 | Positivo | Exportacion CSV genera archivo correcto |
| TP-ORG-01-06 | Negativo | Sin permiso p_org_list retorna 403 |
| TP-ORG-01-07 | Negativo | Sin JWT retorna 401 |
| TP-ORG-01-08 | Negativo | page_size > 100 retorna 400 |
| TP-ORG-01-09 | Negativo | status invalido retorna 400 |
| TP-ORG-01-10 | Integracion | Admin Entidad solo ve su organizacion |
| TP-ORG-01-11 | Integracion | Ordenamiento por name/identifier/created_at funciona |
| TP-ORG-01-12 | E2E | Navegar a /tenants, ver grilla, buscar, filtrar, exportar |

### Sin Ambiguedades

- **Supuestos prohibidos:** No se asume que todos los roles ven todas las organizaciones. No se asume paginacion del lado del cliente. No se asume que el campo `description` aparece en el listado (solo en detalle/edicion).
- **Decisiones cerradas:** Busqueda aplica ILIKE simultaneo sobre `name` e `identifier` (OR). Exportacion tiene limite de 10,000 filas. `contact_phone_code` y `contact_phone` no se incluyen en el listado (solo en detalle).
- **Fuera de alcance explicito:** Filtro por pais (no implementado en MVP). Ordenamiento multi-columna. Busqueda full-text.
- **TODO explicitos = 0**

---

## RF-ORG-02 — Crear organizacion con datos de contacto

### Execution Sheet

| Campo | Valor |
|-------|-------|
| **ID** | RF-ORG-02 |
| **Titulo** | Crear organizacion con datos de contacto |
| **Actor(es)** | Super Admin |
| **Prioridad** | Alta |
| **Severidad** | P0 |
| **Flujo origen** | FL-ORG-01 seccion 6 (crear) |
| **HU origen** | HU003 |

### Precondiciones

| # | Condicion | Detalle |
|---|-----------|---------|
| 1 | Organization Service operativo | SA.organization accesible |
| 2 | Usuario autenticado como Super Admin | JWT valido |
| 3 | Permiso requerido | `p_org_create` presente en claims del JWT |

### Entradas

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| name | string | Si | Body JSON | No vacio, max 200 chars | — |
| identifier | string | Si | Body JSON | No vacio, max 20 chars, UNIQUE global | RN-ORG-01 |
| description | string | Si | Body JSON | No vacio, max 500 chars | — |
| status | string | Si | Body JSON | Enum: `active`, `inactive` | RN-ORG-03 |
| contact_name | string | Si | Body JSON | No vacio, max 200 chars | — |
| contact_email | string | Si | Body JSON | Formato email valido, max 255 chars | — |
| contact_phone_code | string | Si | Body JSON | No vacio, max 10 chars | — |
| contact_phone | string | Si | Body JSON | No vacio, max 30 chars | — |
| country | string | Si | Body JSON | No vacio, max 100 chars | — |

### Proceso (Happy Path)

| # | Paso | Responsable |
|---|------|-------------|
| 1 | SPA envia `POST /tenants { name, identifier, description, status, contact_name, contact_email, contact_phone_code, contact_phone, country }` | Frontend SPA |
| 2 | API Gateway valida JWT y enruta | API Gateway |
| 3 | Organization Service verifica permiso `p_org_create` | Organization Service |
| 4 | Validar formato de todos los campos | Organization Service |
| 5 | Validar unicidad de `identifier` (`SELECT COUNT(*) FROM tenants WHERE identifier = ?`) | Organization Service |
| 6 | Generar UUID (UUIDv7) para `id` | Organization Service |
| 7 | `INSERT INTO tenants (id, name, identifier, description, status, contact_name, contact_email, contact_phone_code, contact_phone, country, created_at, updated_at, created_by, updated_by)` | Organization Service |
| 8 | Publicar `TenantCreatedEvent { tenant_id, name, identifier, user_id, user_name }` a RabbitMQ | Organization Service |
| 9 | Retornar `201 Created { id }` | Organization Service |

### Salidas

| Campo | Tipo | Destino | Efecto observable |
|-------|------|---------|-------------------|
| id | uuid | SPA | ID de la organizacion creada |
| HTTP 201 | status code | SPA | Redireccion a /tenants con notificacion de exito |
| TenantCreatedEvent | evento async | RabbitMQ → Audit Service | Registro en `audit_log` |

### Errores Tipados

| Codigo | Causa | Condicion de disparo | Respuesta esperada |
|--------|-------|---------------------|--------------------|
| `AUTH_UNAUTHORIZED` | JWT ausente o invalido | Header Authorization faltante o token expirado | HTTP 401 |
| `AUTH_FORBIDDEN` | Sin permiso | `p_org_create` no presente en claims | HTTP 403, "Permiso p_org_create requerido" |
| `TENANT_IDENTIFIER_DUPLICATE` | Identifier duplicado | Ya existe un tenant con el mismo `identifier` | HTTP 409, "Ya existe una organizacion con el identificador {identifier}" |
| `VALIDATION_ERROR` | Validacion fallida | Campo requerido vacio, formato email invalido, status no es enum | HTTP 422, detalle de campos invalidos |

### Casos Especiales y Variantes

- **CUIT duplicado:** La validacion de unicidad se realiza antes del INSERT. Si hay una race condition, el indice UNIQUE en `tenants(identifier)` lo atrapa y se retorna 409.
- **Solo Super Admin:** Ningun otro rol puede crear organizaciones. El permiso `p_org_create` es exclusivo del rol Super Admin.
- **Status al crear:** Se permite crear organizaciones directamente en estado `inactive` (para preparacion previa a la activacion).
- **Campos de auditoria:** `created_by` y `updated_by` se setean con el `user_id` del JWT. `created_at` y `updated_at` se setean con `now()`.

### Impacto en Modelo de Datos

| Entidad | Operacion | Campo(s) afectado(s) | Evento |
|---------|-----------|---------------------|--------|
| `tenants` | INSERT | Todos los campos | TenantCreatedEvent |
| `audit_log` (SA.audit) | INSERT (async) | Consume TenantCreatedEvent | — |

### Criterios de Aceptacion (Gherkin)

```gherkin
Scenario: Super Admin crea organizacion exitosamente
  Given un Super Admin autenticado con permiso p_org_create
  When envia POST /tenants con todos los campos requeridos validos
  Then recibe 201 con { id }
  And el tenant se persiste en tenants con status y datos de contacto
  And se publica TenantCreatedEvent

Scenario: Identifier duplicado retorna 409
  Given un Super Admin autenticado
  And existe una organizacion con identifier = "20-30555888-9"
  When envia POST /tenants con identifier = "20-30555888-9"
  Then recibe 409 con error CONFLICT

Scenario: Campo requerido vacio retorna 422
  Given un Super Admin autenticado
  When envia POST /tenants con name = ""
  Then recibe 422 con error VALIDATION_ERROR
  And details contiene { field: "name", message: "Nombre es requerido" }

Scenario: Email con formato invalido retorna 422
  Given un Super Admin autenticado
  When envia POST /tenants con contact_email = "no-es-email"
  Then recibe 422 con error VALIDATION_ERROR
  And details contiene { field: "contact_email", message: "Formato de email invalido" }

Scenario: Actor sin permiso retorna 403
  Given un Admin Entidad autenticado sin permiso p_org_create
  When envia POST /tenants
  Then recibe 403 con error FORBIDDEN
```

### Trazabilidad de Pruebas

| Test ID | Tipo | Descripcion |
|---------|------|-------------|
| TP-ORG-02-01 | Positivo | Creacion exitosa con todos los campos validos |
| TP-ORG-02-02 | Positivo | Creacion con status = inactive funciona |
| TP-ORG-02-03 | Negativo | Identifier duplicado retorna 409 |
| TP-ORG-02-04 | Negativo | Campo name vacio retorna 422 |
| TP-ORG-02-05 | Negativo | Email invalido retorna 422 |
| TP-ORG-02-06 | Negativo | Status invalido retorna 422 |
| TP-ORG-02-07 | Negativo | Sin permiso p_org_create retorna 403 |
| TP-ORG-02-08 | Negativo | Sin JWT retorna 401 |
| TP-ORG-02-09 | Integracion | TenantCreatedEvent se publica y Audit lo registra |
| TP-ORG-02-10 | Integracion | created_by y updated_by se setean del JWT |
| TP-ORG-02-11 | E2E | Navegar a /tenants/nuevo, completar formulario, guardar, ver en listado |

### Sin Ambiguedades

- **Supuestos prohibidos:** No se asume que el CUIT tiene formato fijo (es un string libre). No se asume validacion de CUIT contra AFIP. No se asume que el pais viene de un catalogo parametrizado (es texto libre en MVP).
- **Decisiones cerradas:** `identifier` es UNIQUE global (no por tenant). Se permite crear con status `inactive`. El `id` se genera como UUIDv7 en la aplicacion (no autoincremental en DB).
- **Fuera de alcance explicito:** Validacion de formato CUIT contra AFIP. Logo/branding de organizacion. Configuracion de tenant post-creacion.
- **TODO explicitos = 0**

---

## RF-ORG-03 — Editar organizacion con entidades dependientes

### Execution Sheet

| Campo | Valor |
|-------|-------|
| **ID** | RF-ORG-03 |
| **Titulo** | Editar organizacion con entidades dependientes |
| **Actor(es)** | Super Admin |
| **Prioridad** | Alta |
| **Severidad** | P0 |
| **Flujo origen** | FL-ORG-01 seccion 6 (editar) |
| **HU origen** | HU003 |

### Precondiciones

| # | Condicion | Detalle |
|---|-----------|---------|
| 1 | Organization Service operativo | SA.organization accesible |
| 2 | Usuario autenticado como Super Admin | JWT valido |
| 3 | Permiso requerido | `p_org_edit` presente en claims del JWT |
| 4 | Organizacion existe | `tenants.id` valido |

### Entradas

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| id | uuid | Si | Path param | Existe en `tenants` | — |
| name | string | Si | Body JSON | No vacio, max 200 chars | — |
| identifier | string | Si | Body JSON | No vacio, max 20 chars, UNIQUE global (excluyendo self) | RN-ORG-01 |
| description | string | Si | Body JSON | No vacio, max 500 chars | — |
| status | string | Si | Body JSON | Enum: `active`, `inactive` | RN-ORG-03 |
| contact_name | string | Si | Body JSON | No vacio, max 200 chars | — |
| contact_email | string | Si | Body JSON | Formato email valido, max 255 chars | — |
| contact_phone_code | string | Si | Body JSON | No vacio, max 10 chars | — |
| contact_phone | string | Si | Body JSON | No vacio, max 30 chars | — |
| country | string | Si | Body JSON | No vacio, max 100 chars | — |

### Proceso (Happy Path)

| # | Paso | Responsable |
|---|------|-------------|
| 1 | SPA solicita datos actuales: `GET /tenants/:id` | Frontend SPA |
| 2 | Organization Service retorna tenant + entidades dependientes | Organization Service |
| 3 | SPA renderiza formulario precargado con grilla de entidades | Frontend SPA |
| 4 | Usuario modifica campos y envia `PUT /tenants/:id { ... }` | Frontend SPA |
| 5 | API Gateway valida JWT y enruta | API Gateway |
| 6 | Organization Service verifica permiso `p_org_edit` | Organization Service |
| 7 | Validar que el tenant existe (404 si no) | Organization Service |
| 8 | Validar formato de todos los campos | Organization Service |
| 9 | Validar unicidad de `identifier` excluyendo el registro actual (`WHERE identifier = ? AND id != ?`) | Organization Service |
| 10 | `UPDATE tenants SET name=?, identifier=?, description=?, status=?, contact_name=?, contact_email=?, contact_phone_code=?, contact_phone=?, country=?, updated_at=now(), updated_by=? WHERE id = ?` | Organization Service |
| 11 | Publicar `TenantUpdatedEvent { tenant_id, name, identifier, changes, user_id, user_name }` a RabbitMQ | Organization Service |
| 12 | Retornar `200 OK` | Organization Service |

### Salidas

| Campo | Tipo | Destino | Efecto observable |
|-------|------|---------|-------------------|
| HTTP 200 | status code | SPA | Confirmacion de actualizacion |
| TenantUpdatedEvent | evento async | RabbitMQ → Audit Service | Registro en `audit_log` con detalle de cambios |

**Salida del GET previo (carga del formulario):**

| Campo | Tipo | Destino | Efecto observable |
|-------|------|---------|-------------------|
| tenant | objeto | SPA | Datos completos de la organizacion |
| entities[] | array | SPA | Grilla de entidades dependientes con `{ id, name, identifier, type, status }` |

### Errores Tipados

| Codigo | Causa | Condicion de disparo | Respuesta esperada |
|--------|-------|---------------------|--------------------|
| `AUTH_UNAUTHORIZED` | JWT ausente o invalido | Header Authorization faltante o token expirado | HTTP 401 |
| `AUTH_FORBIDDEN` | Sin permiso | `p_org_edit` no presente en claims | HTTP 403, "Permiso p_org_edit requerido" |
| `TENANT_NOT_FOUND` | Organizacion no encontrada | `id` no existe en `tenants` | HTTP 404, "Organizacion no encontrada" |
| `TENANT_IDENTIFIER_DUPLICATE` | Identifier duplicado | Ya existe otro tenant con el mismo `identifier` | HTTP 409, "Ya existe una organizacion con el identificador {identifier}" |
| `VALIDATION_ERROR` | Validacion fallida | Campo requerido vacio, formato email invalido, status no es enum | HTTP 422, detalle de campos invalidos |

### Casos Especiales y Variantes

- **Organizacion inactiva con entidades activas:** Se permite cambiar una organizacion a `inactive` aunque tenga entidades activas. El UI puede mostrar una advertencia, pero el backend no bloquea la accion (RN-ORG-03).
- **Grilla de entidades:** La grilla de entidades en el formulario de edicion es de solo lectura. Click en una entidad navega a `/entidades/:id/editar` (FL-ORG-02, fuera del alcance de este RF).
- **Concurrencia:** No se implementa lock optimista para edicion de tenants (baja probabilidad de edicion concurrente). El ultimo en guardar prevalece.

### Impacto en Modelo de Datos

| Entidad | Operacion | Campo(s) afectado(s) | Evento |
|---------|-----------|---------------------|--------|
| `tenants` | UPDATE | Todos excepto `id`, `created_at`, `created_by` | TenantUpdatedEvent |
| `entities` | SELECT | Lectura para grilla de dependientes | — |
| `audit_log` (SA.audit) | INSERT (async) | Consume TenantUpdatedEvent | — |

### Criterios de Aceptacion (Gherkin)

```gherkin
Scenario: Super Admin edita organizacion exitosamente
  Given un Super Admin autenticado con permiso p_org_edit
  And existe una organizacion con id = "abc-123"
  When envia PUT /tenants/abc-123 con name = "Nuevo Nombre"
  Then recibe 200
  And el tenant tiene name = "Nuevo Nombre" en base de datos
  And se publica TenantUpdatedEvent

Scenario: Carga de formulario incluye entidades dependientes
  Given un Super Admin autenticado
  And la organizacion "abc-123" tiene 2 entidades
  When envia GET /tenants/abc-123
  Then recibe 200 con tenant y entities[] de 2 elementos

Scenario: Identifier duplicado al editar retorna 409
  Given un Super Admin autenticado
  And existe organizacion A con identifier "20-111" y organizacion B con identifier "20-222"
  When edita organizacion A cambiando identifier a "20-222"
  Then recibe 409 con error CONFLICT

Scenario: Organizacion inexistente retorna 404
  Given un Super Admin autenticado
  When envia PUT /tenants/id-inexistente
  Then recibe 404 con error NOT_FOUND

Scenario: Desactivar organizacion con entidades activas se permite
  Given un Super Admin autenticado
  And la organizacion tiene 3 entidades activas
  When envia PUT /tenants/abc-123 con status = "inactive"
  Then recibe 200
  And el tenant tiene status = "inactive"
```

### Trazabilidad de Pruebas

| Test ID | Tipo | Descripcion |
|---------|------|-------------|
| TP-ORG-03-01 | Positivo | Edicion exitosa actualiza todos los campos |
| TP-ORG-03-02 | Positivo | GET /tenants/:id retorna tenant + entidades dependientes |
| TP-ORG-03-03 | Positivo | Desactivar organizacion con entidades activas se permite |
| TP-ORG-03-04 | Negativo | Identifier duplicado al editar retorna 409 |
| TP-ORG-03-05 | Negativo | Organizacion inexistente retorna 404 |
| TP-ORG-03-06 | Negativo | Campo name vacio retorna 422 |
| TP-ORG-03-07 | Negativo | Sin permiso p_org_edit retorna 403 |
| TP-ORG-03-08 | Negativo | Sin JWT retorna 401 |
| TP-ORG-03-09 | Integracion | TenantUpdatedEvent se publica y Audit lo registra |
| TP-ORG-03-10 | Integracion | updated_by y updated_at se actualizan correctamente |
| TP-ORG-03-11 | E2E | Navegar a /tenants/:id/editar, modificar, guardar, ver cambios |

### Sin Ambiguedades

- **Supuestos prohibidos:** No se asume lock optimista (ultimo en guardar gana). No se asume que la grilla de entidades es editable desde el formulario del tenant. No se asume que desactivar una organizacion desactiva sus entidades en cascada.
- **Decisiones cerradas:** Unicidad de `identifier` se valida excluyendo el registro actual. Se permite status `inactive` con entidades activas. La grilla de entidades es de solo lectura con navegacion.
- **Fuera de alcance explicito:** Lock optimista, edicion inline de entidades desde el formulario de organizacion, cascada de status a entidades.
- **TODO explicitos = 0**

---

## RF-ORG-04 — Ver detalle de organizacion con entidades dependientes

### Execution Sheet

| Campo | Valor |
|-------|-------|
| **ID** | RF-ORG-04 |
| **Titulo** | Ver detalle de organizacion con entidades dependientes |
| **Actor(es)** | Super Admin, Admin Entidad (su org), Auditor (lectura), Consulta (lectura) |
| **Prioridad** | Media |
| **Severidad** | P2 |
| **Flujo origen** | FL-ORG-01 seccion 6 (detalle) |
| **HU origen** | HU004 |

### Precondiciones

| # | Condicion | Detalle |
|---|-----------|---------|
| 1 | Organization Service operativo | SA.organization accesible |
| 2 | Usuario autenticado | JWT valido con claim `tenant_id` |
| 3 | Permiso requerido | `p_org_detail` presente en claims del JWT |
| 4 | Organizacion existe | `tenants.id` valido |

### Entradas

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| id | uuid | Si | Path param | Formato UUID valido | — |

### Proceso (Happy Path)

| # | Paso | Responsable |
|---|------|-------------|
| 1 | SPA envia `GET /tenants/:id` | Frontend SPA |
| 2 | API Gateway valida JWT y enruta | API Gateway |
| 3 | Organization Service verifica permiso `p_org_detail` | Organization Service |
| 4 | Si actor no es Super Admin: verificar que `id == tenant_id del JWT` | Organization Service |
| 5 | `SELECT tenant + COUNT entities` | Organization Service |
| 6 | `SELECT entities WHERE tenant_id = :id` (listado de entidades dependientes) | Organization Service |
| 7 | Retornar `200 { tenant, entities[] }` | Organization Service |

### Salidas

| Campo | Tipo | Destino | Efecto observable |
|-------|------|---------|-------------------|
| tenant | objeto | SPA | `{ id, name, identifier, description, status, contact_name, contact_email, contact_phone_code, contact_phone, country, created_at, updated_at }` |
| entities[] | array | SPA | Cada entidad: `{ id, name, identifier, type, status }` |

### Errores Tipados

| Codigo | Causa | Condicion de disparo | Respuesta esperada |
|--------|-------|---------------------|--------------------|
| `AUTH_UNAUTHORIZED` | JWT ausente o invalido | Header Authorization faltante o token expirado | HTTP 401 |
| `AUTH_FORBIDDEN` | Sin permiso o acceso a otro tenant | `p_org_detail` no presente, o actor no-Super Admin intenta ver organizacion ajena | HTTP 403 |
| `TENANT_NOT_FOUND` | Organizacion no encontrada | `id` no existe en `tenants` | HTTP 404, "Organizacion no encontrada" |

### Casos Especiales y Variantes

- **Organizacion sin entidades:** `entities[]` viene vacio. No es error.
- **Admin Entidad:** Solo puede ver el detalle de su propia organizacion (`id == tenant_id` del JWT). Si intenta otra, recibe 403.
- **Navegacion:** El SPA muestra boton "Editar" solo si el actor tiene permiso `p_org_edit`. El boton navega a `/tenants/:id/editar`.
- **Endpoint compartido:** Este GET es el mismo endpoint que usa RF-ORG-03 para cargar el formulario de edicion. La diferencia es solo en la vista del SPA (detalle vs formulario).

### Impacto en Modelo de Datos

| Entidad | Operacion | Campo(s) afectado(s) | Evento |
|---------|-----------|---------------------|--------|
| `tenants` | SELECT | Todos los campos | — (sin escritura) |
| `entities` | SELECT | id, name, identifier, type, status | — (sin escritura) |

### Criterios de Aceptacion (Gherkin)

```gherkin
Scenario: Super Admin ve detalle de organizacion
  Given un Super Admin autenticado con permiso p_org_detail
  And existe organizacion "abc-123" con 2 entidades
  When envia GET /tenants/abc-123
  Then recibe 200 con tenant completo y entities[] de 2 elementos

Scenario: Admin Entidad ve su propia organizacion
  Given un Admin Entidad autenticado con tenant_id = "abc-123"
  And tiene permiso p_org_detail
  When envia GET /tenants/abc-123
  Then recibe 200 con tenant y entities[]

Scenario: Admin Entidad intenta ver otra organizacion
  Given un Admin Entidad autenticado con tenant_id = "abc-123"
  When envia GET /tenants/xyz-789
  Then recibe 403 con error FORBIDDEN

Scenario: Organizacion inexistente retorna 404
  Given un Super Admin autenticado
  When envia GET /tenants/id-inexistente
  Then recibe 404 con error NOT_FOUND

Scenario: Organizacion sin entidades muestra lista vacia
  Given un Super Admin autenticado
  And la organizacion "abc-123" no tiene entidades
  When envia GET /tenants/abc-123
  Then recibe 200 con entities[] vacio
```

### Trazabilidad de Pruebas

| Test ID | Tipo | Descripcion |
|---------|------|-------------|
| TP-ORG-04-01 | Positivo | GET /tenants/:id retorna datos completos + entidades |
| TP-ORG-04-02 | Positivo | Organizacion sin entidades retorna entities[] vacio |
| TP-ORG-04-03 | Negativo | Organizacion inexistente retorna 404 |
| TP-ORG-04-04 | Negativo | Sin permiso p_org_detail retorna 403 |
| TP-ORG-04-05 | Negativo | Admin Entidad accediendo a otro tenant retorna 403 |
| TP-ORG-04-06 | Negativo | Sin JWT retorna 401 |
| TP-ORG-04-07 | Integracion | Entidades se cargan con tipo y estado correcto |
| TP-ORG-04-08 | E2E | Navegar a /tenants/:id, ver detalle, click editar |

### Sin Ambiguedades

- **Supuestos prohibidos:** No se asume que el detalle muestra entidades paginadas (se cargan todas las entidades de la organizacion). No se asume que el detalle incluye contadores de sucursales por entidad.
- **Decisiones cerradas:** El endpoint GET /tenants/:id es compartido entre detalle y carga de formulario de edicion. Admin Entidad solo accede a su propio tenant. Las entidades en la respuesta incluyen solo campos de resumen (no datos completos).
- **Fuera de alcance explicito:** Contadores de sucursales/usuarios por entidad en la vista detalle. Arbol organizacional visual. Historial de cambios de la organizacion (cubierto por auditoria).
- **TODO explicitos = 0**

---

## RF-ORG-05 — Eliminar organizacion con validacion de dependencias

### Execution Sheet

| Campo | Valor |
|-------|-------|
| **ID** | RF-ORG-05 |
| **Titulo** | Eliminar organizacion con validacion de dependencias |
| **Actor(es)** | Super Admin |
| **Prioridad** | Alta |
| **Severidad** | P0 |
| **Flujo origen** | FL-ORG-01 seccion 7a (eliminar) |
| **HU origen** | HU002 |

### Precondiciones

| # | Condicion | Detalle |
|---|-----------|---------|
| 1 | Organization Service operativo | SA.organization accesible |
| 2 | Usuario autenticado como Super Admin | JWT valido |
| 3 | Permiso requerido | `p_org_delete` presente en claims del JWT |
| 4 | Organizacion existe | `tenants.id` valido |

### Entradas

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| id | uuid | Si | Path param | Formato UUID valido, existe en `tenants` | — |

### Proceso (Happy Path)

| # | Paso | Responsable |
|---|------|-------------|
| 1 | SPA muestra dialogo de confirmacion: "Esta organizacion sera eliminada permanentemente" | Frontend SPA |
| 2 | Usuario confirma y SPA envia `DELETE /tenants/:id` | Frontend SPA |
| 3 | API Gateway valida JWT y enruta | API Gateway |
| 4 | Organization Service verifica permiso `p_org_delete` | Organization Service |
| 5 | Validar que el tenant existe (404 si no) | Organization Service |
| 6 | Verificar que no tiene entidades asociadas: `SELECT COUNT(*) FROM entities WHERE tenant_id = :id` | Organization Service |
| 7 | Si tiene entidades: retornar 409 | Organization Service |
| 8 | Si no tiene entidades: `DELETE FROM tenants WHERE id = :id` | Organization Service |
| 9 | Publicar `TenantDeletedEvent { tenant_id, name, identifier, user_id, user_name }` a RabbitMQ | Organization Service |
| 10 | Retornar `204 No Content` | Organization Service |

### Salidas

| Campo | Tipo | Destino | Efecto observable |
|-------|------|---------|-------------------|
| HTTP 204 | — | SPA | Confirmacion de eliminacion, SPA refresca listado |
| TenantDeletedEvent | evento async | RabbitMQ → Audit Service | Registro en `audit_log` |

### Errores Tipados

| Codigo | Causa | Condicion de disparo | Respuesta esperada |
|--------|-------|---------------------|--------------------|
| `AUTH_UNAUTHORIZED` | JWT ausente o invalido | Header Authorization faltante o token expirado | HTTP 401 |
| `AUTH_FORBIDDEN` | Sin permiso | `p_org_delete` no presente en claims | HTTP 403, "Permiso p_org_delete requerido" |
| `TENANT_NOT_FOUND` | Organizacion no encontrada | `id` no existe en `tenants` | HTTP 404, "Organizacion no encontrada" |
| `TENANT_HAS_ENTITIES` | Tiene entidades | `entities` con `tenant_id = :id` existen | HTTP 409, "No se puede eliminar: tiene {N} entidades asociadas" |

### Casos Especiales y Variantes

- **Eliminacion fisica:** La eliminacion es un DELETE real, no soft delete (RN-ORG-06). El evento de auditoria preserva el registro historico.
- **Cascada bloqueada:** Las entidades son el gateway a datos en otros servicios (Catalog, Operations, Identity). Por eso la eliminacion se bloquea si hay entidades (D-ORG-01).
- **Confirmacion en UI:** El SPA muestra un dialogo modal de confirmacion antes de enviar el DELETE. El backend no depende de esta confirmacion (la validacion es server-side).
- **Datos en otros servicios:** Si la organizacion se elimina, los datos referenciados en otros servicios (usuarios en SA.identity, productos en SA.catalog) quedan huerfanos. Por eso se requiere que no tenga entidades como primera linea de defensa.

### Impacto en Modelo de Datos

| Entidad | Operacion | Campo(s) afectado(s) | Evento |
|---------|-----------|---------------------|--------|
| `tenants` | DELETE | Registro completo eliminado | TenantDeletedEvent |
| `entities` | SELECT (verificacion) | COUNT para validacion de dependencias | — |
| `audit_log` (SA.audit) | INSERT (async) | Consume TenantDeletedEvent | — |

### Criterios de Aceptacion (Gherkin)

```gherkin
Scenario: Super Admin elimina organizacion sin entidades
  Given un Super Admin autenticado con permiso p_org_delete
  And existe organizacion "abc-123" sin entidades
  When envia DELETE /tenants/abc-123
  Then recibe 204
  And la organizacion ya no existe en tenants
  And se publica TenantDeletedEvent

Scenario: Organizacion con entidades retorna 409
  Given un Super Admin autenticado
  And la organizacion "abc-123" tiene 3 entidades
  When envia DELETE /tenants/abc-123
  Then recibe 409 con error CONFLICT
  And el mensaje contiene "tiene 3 entidades asociadas"

Scenario: Organizacion inexistente retorna 404
  Given un Super Admin autenticado
  When envia DELETE /tenants/id-inexistente
  Then recibe 404 con error NOT_FOUND

Scenario: Actor sin permiso retorna 403
  Given un Admin Entidad autenticado sin permiso p_org_delete
  When envia DELETE /tenants/abc-123
  Then recibe 403 con error FORBIDDEN
```

### Trazabilidad de Pruebas

| Test ID | Tipo | Descripcion |
|---------|------|-------------|
| TP-ORG-05-01 | Positivo | Eliminacion exitosa de organizacion sin entidades |
| TP-ORG-05-02 | Negativo | Organizacion con entidades retorna 409 con conteo |
| TP-ORG-05-03 | Negativo | Organizacion inexistente retorna 404 |
| TP-ORG-05-04 | Negativo | Sin permiso p_org_delete retorna 403 |
| TP-ORG-05-05 | Negativo | Sin JWT retorna 401 |
| TP-ORG-05-06 | Integracion | TenantDeletedEvent se publica y Audit lo registra |
| TP-ORG-05-07 | Integracion | Eliminacion fisica verificada (SELECT post-DELETE retorna 0) |
| TP-ORG-05-08 | E2E | Click eliminar en grilla, confirmar, organizacion desaparece del listado |

### Sin Ambiguedades

- **Supuestos prohibidos:** No se asume soft delete. No se asume eliminacion en cascada de entidades. No se asume que la eliminacion notifica a otros servicios para limpiar datos huerfanos (solo auditoria).
- **Decisiones cerradas:** Solo se elimina si no tiene entidades (D-ORG-01). La eliminacion es fisica. El mensaje de error incluye el conteo de entidades. Solo Super Admin puede eliminar.
- **Fuera de alcance explicito:** Eliminacion en cascada a entidades/sucursales/usuarios. Limpieza de datos en otros servicios. Papelera de reciclaje (undo).
- **TODO explicitos = 0**

---

## RF-ORG-06 — Listar entidades con filtros, busqueda y exportacion

### Execution Sheet

| Campo | Valor |
|-------|-------|
| **ID** | RF-ORG-06 |
| **Titulo** | Listar entidades con filtros, busqueda, paginacion y exportacion |
| **Actor(es)** | Super Admin, Admin Entidad (solo su entidad), Consulta, Auditor, Operador (lectura) |
| **Prioridad** | Alta |
| **Severidad** | P1 |
| **Flujo origen** | FL-ORG-02 seccion 6 (listar) |
| **HU origen** | HU005 |

### Precondiciones

| # | Condicion | Detalle |
|---|-----------|---------|
| 1 | Organization Service operativo | SA.organization accesible |
| 2 | Usuario autenticado | JWT valido con claim `tenant_id` |
| 3 | Permiso requerido | `p_entities_list` presente en claims del JWT |
| 4 | RLS activo | `tenant_id` resuelto desde JWT |

### Entradas

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| `Authorization` | string (Bearer JWT) | Si | Header | JWT valido con `p_entities_list` | — |
| `page` | integer | No (default 1) | Query param | >= 1 | — |
| `page_size` | integer | No (default 20) | Query param | 1-100 | — |
| `search` | string | No | Query param | ILIKE sobre `name` e `identifier`. Trim, max 100 chars. | — |
| `type` | string | No | Query param | Enum: `bank`, `insurance`, `fintech`, `cooperative`, `sgr`, `regional_card` | RN-ORG-13 |
| `status` | string | No | Query param | Enum: `active`, `inactive`, `suspended` | — |
| `sort_by` | string | No (default `name`) | Query param | Enum: `name`, `identifier`, `type`, `status`, `created_at` | — |
| `sort_dir` | string | No (default `asc`) | Query param | Enum: `asc`, `desc` | — |
| `format` | string | No | Query param (solo /export) | Enum: `xlsx`, `csv` | RN-ORG-07 |

### Proceso (Happy Path)

| # | Paso | Responsable |
|---|------|-------------|
| 1 | SPA envia `GET /entities?page&page_size&search&type&status&sort_by&sort_dir` | Frontend SPA |
| 2 | API Gateway valida JWT y enruta a Organization Service | API Gateway |
| 3 | Organization Service verifica permiso `p_entities_list` | Organization Service |
| 4 | Ejecutar `SET LOCAL app.current_tenant = '{tenant_id}'` para activar RLS | Organization Service |
| 5 | Si actor es Admin Entidad: filtro adicional `WHERE id = {entity_id del JWT}` | Organization Service |
| 6 | Aplicar filtros: `search` (ILIKE en `name`, `identifier`), `type`, `status` | Organization Service |
| 7 | Aplicar ordenamiento y paginacion | Organization Service |
| 8 | Retornar `200 { items[], total, page, page_size }` | Organization Service |

### Salidas

| Campo | Tipo | Destino | Efecto observable |
|-------|------|---------|-------------------|
| `items[]` | array | SPA | Cada item: `{ id, name, identifier, type, status, email, province, city, country, created_at }` |
| `total` | integer | SPA | Total de registros que coinciden |
| `page` | integer | SPA | Pagina actual |
| `page_size` | integer | SPA | Tamano de pagina |

### Errores Tipados

| Codigo | Causa | Condicion de disparo | Respuesta esperada |
|--------|-------|---------------------|--------------------|
| `AUTH_UNAUTHORIZED` | JWT ausente o invalido | Header Authorization faltante o token expirado | HTTP 401 |
| `AUTH_FORBIDDEN` | Sin permiso | `p_entities_list` no presente en claims | HTTP 403, "Permiso p_entities_list requerido" |
| `VALIDATION_ERROR` | Parametros invalidos | `type` no es enum valido, `page < 1`, `page_size > 100` | HTTP 422, detalle de campos invalidos |

### Casos Especiales y Variantes

- **Admin Entidad:** Solo ve la(s) entidad(es) de su propia organizacion. Si ademas tiene `entity_id` en JWT, solo ve esa entidad.
- **Exportacion:** `GET /entities/export?format=xlsx&search&type&status` aplica mismos filtros sin paginacion. Limite 10,000 filas (RN-ORG-07).
- **Sin resultados:** HTTP 200 con `{ items: [], total: 0 }`.
- **Busqueda vacia:** Si `search` vacio, no se aplica filtro ILIKE.

### Impacto en Modelo de Datos

| Entidad | Operacion | Campo(s) afectado(s) | Evento |
|---------|-----------|---------------------|--------|
| `entities` | SELECT | Todos los campos de lectura | — (sin escritura) |

### Criterios de Aceptacion (Gherkin)

```gherkin
Scenario: Listar entidades exitosamente con filtros
  Given un Super Admin autenticado con permiso "p_entities_list" en tenant "tenant-001"
  And existen 5 entidades: 2 bank, 2 insurance, 1 fintech
  When envio GET /entities?type=bank
  Then recibo HTTP 200 con items[] de 2 elementos
  And cada item tiene type = "bank"

Scenario: Busqueda por nombre parcial
  Given entidades "Banco Norte" y "Banco Sur" en el tenant
  When envio GET /entities?search=Norte
  Then recibo HTTP 200 con items[] de 1 elemento con name "Banco Norte"

Scenario: Admin Entidad solo ve su entidad
  Given un Admin Entidad con entity_id "ent-001"
  When envio GET /entities
  Then recibo HTTP 200 con items[] de 1 elemento con id "ent-001"

Scenario: Exportacion Excel con filtros
  Given 3 entidades activas y 2 inactivas
  When envio GET /entities/export?format=xlsx&status=active
  Then recibo 200 con Content-Type spreadsheetml.sheet con 3 filas

Scenario: Sin permiso retorna 403
  Given usuario sin permiso "p_entities_list"
  When envio GET /entities
  Then recibo HTTP 403
```

### Trazabilidad de Pruebas

| Test ID | Tipo | Descripcion |
|---------|------|-------------|
| TP-ORG-06-01 | Positivo | Lista paginada con campos esperados |
| TP-ORG-06-02 | Positivo | Busqueda ILIKE filtra por name e identifier |
| TP-ORG-06-03 | Positivo | Filtro por type retorna solo entidades del tipo |
| TP-ORG-06-04 | Positivo | Filtro por status funciona correctamente |
| TP-ORG-06-05 | Positivo | Exportacion Excel genera archivo correcto |
| TP-ORG-06-06 | Positivo | Exportacion CSV genera archivo correcto |
| TP-ORG-06-07 | Negativo | Sin permiso p_entities_list retorna 403 |
| TP-ORG-06-08 | Negativo | Sin JWT retorna 401 |
| TP-ORG-06-09 | Negativo | type invalido retorna 422 |
| TP-ORG-06-10 | Negativo | page_size > 100 retorna 422 |
| TP-ORG-06-11 | Integracion | RLS filtra por tenant |
| TP-ORG-06-12 | Integracion | Admin Entidad solo ve su entidad |
| TP-ORG-06-13 | Integracion | Ordenamiento por name/type/created_at funciona |
| TP-ORG-06-14 | E2E | Navegar a /entidades, ver grilla, buscar, filtrar, exportar |
| TP-ORG-06-15 | E2E | Click en fila navega a detalle |

### Sin Ambiguedades

- **Supuestos prohibidos:** No se asume que Admin Entidad ve todas las entidades del tenant. No se asume que `phone_code`/`phone` aparecen en el listado (solo en detalle). No se asume que canales aparecen en la grilla.
- **Decisiones cerradas:** Busqueda ILIKE sobre name e identifier (OR). Exportacion con limite 10,000. Filtros por type y status independientes. Admin Entidad filtrado por entity_id del JWT.
- **Fuera de alcance explicito:** Filtro por provincia/ciudad. Filtro combinado de multiples types. Vista de mapa.
- **TODO explicitos = 0**

---

## RF-ORG-07 — Crear/editar entidad con tipo, canales y selects en cascada

### Execution Sheet

| Campo | Valor |
|-------|-------|
| **ID** | RF-ORG-07 |
| **Titulo** | Crear y editar entidad financiera con tipo, canales habilitados y selects en cascada |
| **Actor(es)** | Super Admin, Admin Entidad (solo su organizacion) |
| **Prioridad** | Alta |
| **Severidad** | P0 |
| **Flujo origen** | FL-ORG-02 seccion 6 (crear, editar) |
| **HU origen** | HU006 |

### Precondiciones

| # | Condicion | Detalle |
|---|-----------|---------|
| 1 | Organization Service operativo | SA.organization accesible |
| 2 | Al menos una organizacion activa | Para asociar la entidad (tenant_id del JWT) |
| 3 | Super Admin o Admin Entidad autenticado | JWT valido con `p_entities_create` (crear) o `p_entities_edit` (editar) |
| 4 | Parametros cargados en Config Service | `provinces`, `cities` (con parent_key), `entity_types`, `channels` en cache Redis |
| 5 | RLS activo | `tenant_id` resuelto desde JWT |
| 6 | RabbitMQ operativo | Para publicacion de eventos |

### Entradas

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| `Authorization` | string (Bearer JWT) | Si | Header | JWT con `p_entities_create` o `p_entities_edit` | — |
| `id` | uuid | Solo en PUT | URL path `/entities/:id` | UUID valido, entidad existente en el tenant | — |
| `name` | string | Si | Body JSON | No vacio, trim, 3-200 chars | — |
| `identifier` | string | Si | Body JSON | No vacio, trim, formato CUIT (XX-XXXXXXXX-X), unico por tenant | RN-ORG-08 |
| `type` | string | Si | Body JSON | Enum valido de `entity_types`: bank, insurance, fintech, cooperative, sgr, regional_card | RN-ORG-13 |
| `status` | string | Si | Body JSON | Enum: `active`, `inactive`, `suspended` | — |
| `email` | string | Si | Body JSON | Formato email valido, max 255 chars | — |
| `phone_code` | string | No | Body JSON | Codigo de area, max 10 chars | — |
| `phone` | string | No | Body JSON | Numero de telefono, max 20 chars | — |
| `address` | string | No | Body JSON | Trim, max 500 chars | — |
| `province` | string | Si | Body JSON | Debe existir en parametro `provinces` | RN-ORG-11 |
| `city` | string | Si | Body JSON | Debe existir en parametro `cities` con `parent_key = province` | RN-ORG-11 |
| `country` | string | No (default "Argentina") | Body JSON | Max 100 chars | — |
| `channels` | string[] | No | Body JSON | Cada valor debe ser enum valido de `channels`: web, mobile, api, presencial, ia_agent. Sin duplicados. | — |

### Proceso (Happy Path) — Crear

| # | Paso | Responsable |
|---|------|-------------|
| 1 | Recibir `POST /entities` con body completo | API Gateway |
| 2 | Validar JWT y verificar permiso `p_entities_create` | Organization Service |
| 3 | Activar RLS: `SET LOCAL app.current_tenant = '{tenant_id}'` | Organization Service |
| 4 | Validar formato de entradas: name, identifier (CUIT), type, email, province, city | Organization Service |
| 5 | Verificar unicidad de `identifier` por tenant: `SELECT COUNT(*) FROM entities WHERE identifier = :identifier` (RLS filtra) | Organization Service |
| 6 | Generar UUIDv7 para la entidad | Organization Service |
| 7 | En transaccion: `INSERT INTO entities (...)` con `tenant_id` del JWT | Organization Service |
| 8 | `INSERT INTO entity_channels (id, entity_id, tenant_id, channel)` batch para cada canal | Organization Service |
| 9 | Commit | Organization Service |
| 10 | Publicar `EntityCreatedEvent { entity_id, tenant_id, name, type, channels[], created_by, timestamp }` | Organization Service |
| 11 | Retornar `201 Created` con `{ id, name, identifier, type, status, channels[], created_at }` | Organization Service |

### Proceso (Happy Path) — Editar

| # | Paso | Responsable |
|---|------|-------------|
| 1 | Recibir `PUT /entities/:id` con body completo | API Gateway |
| 2 | Validar JWT y verificar permiso `p_entities_edit` | Organization Service |
| 3 | Activar RLS | Organization Service |
| 4 | Buscar entidad por `id` (404 si no existe) | Organization Service |
| 5 | Validar formato de entradas | Organization Service |
| 6 | Verificar unicidad de `identifier` excluyendo la propia: `WHERE identifier = :identifier AND id != :id` | Organization Service |
| 7 | Obtener canales actuales: `SELECT channel FROM entity_channels WHERE entity_id = :id` | Organization Service |
| 8 | Calcular diff: `added = channels - current`, `removed = current - channels` | Organization Service |
| 9 | En transaccion: `UPDATE entities SET ...` | Organization Service |
| 10 | `DELETE FROM entity_channels WHERE entity_id = :id AND channel IN (:removed)` | Organization Service |
| 11 | `INSERT INTO entity_channels (id, entity_id, tenant_id, channel)` batch para `added` | Organization Service |
| 12 | Commit | Organization Service |
| 13 | Publicar `EntityUpdatedEvent { entity_id, tenant_id, name, channels_added[], channels_removed[], updated_by, timestamp }` | Organization Service |
| 14 | Retornar `200 OK` con `{ id, name, identifier, type, status, channels[], updated_at }` | Organization Service |

### Salidas

| Campo | Tipo | Destino | Efecto observable |
|-------|------|---------|-------------------|
| `id` | uuid | Response body | ID de la entidad |
| `name` | string | Response body | Nombre |
| `identifier` | string | Response body | CUIT |
| `type` | string | Response body | Tipo de entidad |
| `status` | string | Response body | Estado |
| `channels[]` | string[] | Response body | Canales habilitados |
| `created_at` / `updated_at` | timestamp | Response body | Timestamp |
| `EntityCreatedEvent` / `EntityUpdatedEvent` | evento async | RabbitMQ → Audit | Registro en `audit_log` |
| `entities` (row) | INSERT / UPDATE | SA.organization | Nuevo registro o actualizacion |
| `entity_channels` (rows) | INSERT / DELETE | SA.organization | Sincronizacion de canales |

### Errores Tipados

| Codigo | Causa | Condicion de disparo | Respuesta esperada |
|--------|-------|---------------------|--------------------|
| `AUTH_UNAUTHORIZED` | JWT ausente/invalido | Token faltante o expirado | HTTP 401 |
| `AUTH_FORBIDDEN` | Sin permiso | Sin `p_entities_create` o `p_entities_edit` | HTTP 403 |
| `ENTITY_NOT_FOUND` | Entidad inexistente | `id` no existe en el tenant (solo PUT) | HTTP 404, "Entidad no encontrada" |
| `ENTITY_IDENTIFIER_DUPLICATE` | CUIT duplicado | Ya existe otra entidad con el mismo `identifier` en el tenant | HTTP 409, "Ya existe una entidad con este identificador (CUIT)" |
| `VALIDATION_ERROR` | Entrada malformada | Campos vacios, `type` invalido, `province` no en parametros, `city` no pertenece a `province`, email invalido | HTTP 422, detalle de campos |
| `ENTITY_CHANNEL_INVALID` | Canal no valido | Un valor en `channels` no existe en parametro `channels` | HTTP 422, "Canal no valido: {channel}" |

### Casos Especiales y Variantes

- **Selects en cascada (SPA):** El SPA carga provincias del cache de Config Service. Al seleccionar provincia, filtra ciudades por `parent_key`. Si Config no responde, fallback a input de texto libre (RN-ORG-11).
- **Canales vacios:** Si `channels` es array vacio o no se envia, la entidad se crea sin canales. Es valido.
- **Deduplicacion de canales:** Si `channels` contiene duplicados, se deduplicar silenciosamente.
- **Edicion de canales como diff:** No se borran y recrean todos; solo se aplica el diff (RN-ORG-10).
- **Admin Entidad:** Solo puede crear/editar entidades en su propia organizacion (RLS lo garantiza).
- **GET previo para precarga:** El SPA obtiene la entidad con canales actuales via `GET /entities/:id`. Este GET requiere `p_entities_edit`.

### Impacto en Modelo de Datos

| Entidad | Operacion | Campo(s) afectado(s) | Evento |
|---------|-----------|---------------------|--------|
| `entities` | INSERT (crear) | Todos los campos | `EntityCreatedEvent` |
| `entities` | UPDATE (editar) | name, identifier, type, status, email, phone_code, phone, address, province, city, country, updated_at, updated_by | `EntityUpdatedEvent` |
| `entity_channels` | INSERT (batch) | entity_id, tenant_id, channel | — (incluido en EntityEvent) |
| `entity_channels` | DELETE (diff, solo editar) | entity_id, channel | — (incluido en EntityEvent) |
| `audit_log` (SA.audit) | INSERT (async) | operation, module, entity_type, entity_id | Consume eventos |

### Criterios de Aceptacion (Gherkin)

```gherkin
Scenario: Crear entidad exitosamente con canales
  Given un Super Admin autenticado con permiso "p_entities_create"
  And parametros provinces, cities, entity_types cargados
  When envio POST /entities con name "Banco Norte", identifier "30-12345678-9", type "bank", province "Buenos Aires", city "La Plata", channels ["web", "mobile"]
  Then recibo HTTP 201 con id, name, type "bank", channels ["web", "mobile"]
  And se inserta 1 fila en entities y 2 filas en entity_channels
  And se publica EntityCreatedEvent

Scenario: Editar entidad con cambio de canales
  Given entidad "Banco Norte" con canales ["web", "mobile"]
  When envio PUT /entities/:id con channels ["web", "api"]
  Then recibo HTTP 200
  And entity_channels tiene ["web", "api"] (mobile removido, api agregado)
  And se publica EntityUpdatedEvent con channels_added ["api"] y channels_removed ["mobile"]

Scenario: CUIT duplicado retorna 409
  Given entidad con identifier "30-12345678-9" en el tenant
  When envio POST /entities con identifier "30-12345678-9"
  Then recibo HTTP 409 con ENTITY_IDENTIFIER_DUPLICATE

Scenario: Ciudad no pertenece a provincia retorna 422
  When envio POST /entities con province "Buenos Aires" y city "Rosario"
  Then recibo HTTP 422 con detalle "city no pertenece a province seleccionada"

Scenario: Entidad inexistente al editar retorna 404
  When envio PUT /entities/:uuid_invalido
  Then recibo HTTP 404 con ENTITY_NOT_FOUND
```

### Trazabilidad de Pruebas

| Test ID | Tipo | Descripcion |
|---------|------|-------------|
| TP-ORG-07-01 | Positivo | Crear entidad retorna 201 con todos los campos |
| TP-ORG-07-02 | Positivo | Entidad se crea con canales correctamente (entity_channels) |
| TP-ORG-07-03 | Positivo | Editar entidad retorna 200 con campos actualizados |
| TP-ORG-07-04 | Positivo | Diff de canales: removidos se eliminan, agregados se insertan |
| TP-ORG-07-05 | Positivo | EntityCreatedEvent y EntityUpdatedEvent publicados |
| TP-ORG-07-06 | Positivo | Canales duplicados en el array se deduplicar |
| TP-ORG-07-07 | Positivo | Crear entidad sin canales es valido |
| TP-ORG-07-08 | Negativo | CUIT duplicado retorna 409 |
| TP-ORG-07-09 | Negativo | type invalido retorna 422 |
| TP-ORG-07-10 | Negativo | city no pertenece a province retorna 422 |
| TP-ORG-07-11 | Negativo | email invalido retorna 422 |
| TP-ORG-07-12 | Negativo | name vacio retorna 422 |
| TP-ORG-07-13 | Negativo | Canal invalido retorna 422 |
| TP-ORG-07-14 | Negativo | Entidad inexistente al editar retorna 404 |
| TP-ORG-07-15 | Negativo | Sin JWT retorna 401 |
| TP-ORG-07-16 | Negativo | Sin permiso retorna 403 |
| TP-ORG-07-17 | Seguridad | RLS garantiza tenant_id del JWT al crear |
| TP-ORG-07-18 | Transaccion | Fallo en entity_channels revierte entities (rollback) |
| TP-ORG-07-19 | Integracion | Audit log registra creacion y edicion |
| TP-ORG-07-20 | E2E | Click "Nueva Entidad" → formulario con selects en cascada → guardar |
| TP-ORG-07-21 | E2E | Seleccionar provincia → ciudades se filtran automaticamente |
| TP-ORG-07-22 | E2E | Editar entidad → checkboxes de canales precargados → modificar → guardar |

### Sin Ambiguedades

- **Supuestos prohibidos:** No se asume que canales se borran y recrean al editar (se usa diff). No se asume que province/city se validan contra DB (se validan contra parametros de Config). No se asume formato libre para CUIT (se valida formato XX-XXXXXXXX-X).
- **Decisiones cerradas:** Diff de canales (RN-ORG-10). Cascada provincia→ciudad via parametros con parent_key. Canales son opcionales (entidad sin canales es valida). CUIT unico por tenant. Tipos cargados de parametro entity_types.
- **Fuera de alcance explicito:** Validacion de CUIT contra AFIP. Logo/imagen de entidad. Geolocalizacion automatica. Configuracion avanzada de canales.
- **TODO explicitos = 0**

---

## RF-ORG-08 — Ver detalle de entidad con sucursales y usuarios

### Execution Sheet

| Campo | Valor |
|-------|-------|
| **ID** | RF-ORG-08 |
| **Titulo** | Ver detalle de entidad con sucursales asociadas y conteo de usuarios |
| **Actor(es)** | Super Admin, Admin Entidad, Consulta, Auditor, Operador |
| **Prioridad** | Media |
| **Severidad** | P2 |
| **Flujo origen** | FL-ORG-02 seccion 6 (ver detalle) |
| **HU origen** | HU007 |

### Precondiciones

| # | Condicion | Detalle |
|---|-----------|---------|
| 1 | Organization Service operativo | SA.organization accesible |
| 2 | Usuario autenticado | JWT valido con `p_entities_detail` |
| 3 | Entidad existente | `id` valido en el tenant |
| 4 | RLS activo | `tenant_id` resuelto desde JWT |

### Entradas

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| `Authorization` | string (Bearer JWT) | Si | Header | JWT valido con `p_entities_detail` | — |
| `id` | uuid | Si | URL path `/entities/:id` | UUID valido | — |

### Proceso (Happy Path)

| # | Paso | Responsable |
|---|------|-------------|
| 1 | Recibir `GET /entities/:id` | API Gateway |
| 2 | Validar JWT y verificar permiso `p_entities_detail` | Organization Service |
| 3 | Activar RLS | Organization Service |
| 4 | Buscar entidad: `SELECT * FROM entities WHERE id = :id` | Organization Service |
| 5 | Verificar existencia (404 si no) | Organization Service |
| 6 | Cargar canales: `SELECT channel FROM entity_channels WHERE entity_id = :id` | Organization Service |
| 7 | Cargar sucursales: `SELECT * FROM branches WHERE entity_id = :id ORDER BY name` | Organization Service |
| 8 | Contar usuarios asignados: contar `users` con `entity_id = :id` via referencia logica. Como SA.identity es otro servicio, se usa dato denormalizado o se consulta via HTTP sync. | Organization Service |
| 9 | Retornar `200 OK` con `{ entity, channels[], branches[], user_count }` | Organization Service |

### Salidas

| Campo | Tipo | Destino | Efecto observable |
|-------|------|---------|-------------------|
| `entity` | object | Response body | Todos los campos: id, name, identifier, type, status, email, phone_code, phone, address, province, city, country, created_at, updated_at |
| `channels[]` | string[] | Response body | Lista de canales habilitados (ej: ["web", "mobile"]) |
| `branches[]` | array of branch summary | Response body | Cada branch: { id, name, code, address, city, province, status, manager, phone } |
| `user_count` | integer | Response body | Cantidad de usuarios asignados a esta entidad |

### Errores Tipados

| Codigo | Causa | Condicion de disparo | Respuesta esperada |
|--------|-------|---------------------|--------------------|
| `AUTH_UNAUTHORIZED` | JWT ausente/invalido | Token faltante o expirado | HTTP 401 |
| `AUTH_FORBIDDEN` | Sin permiso | Sin `p_entities_detail` | HTTP 403 |
| `ENTITY_NOT_FOUND` | Entidad inexistente | `id` no existe en el tenant (RLS filtra) | HTTP 404, "Entidad no encontrada" |
| `VALIDATION_ERROR` | ID invalido | `id` no es UUID valido | HTTP 422 |

### Casos Especiales y Variantes

- **User count cross-service:** Como `users.entity_id` esta en SA.identity y este endpoint es de SA.organization, el conteo de usuarios puede resolverse de dos formas: (1) HTTP sync `GET /internal/users/count?entity_id=:id` al Identity Service, o (2) mantener un contador denormalizado actualizado via eventos. Decision de implementacion; ambas opciones son validas.
- **Entidad sin sucursales:** `branches[]` retorna array vacio. Es valido.
- **Entidad sin canales:** `channels[]` retorna array vacio.
- **Admin Entidad:** Solo puede ver detalle de entidades de su organizacion (RLS lo garantiza). Si tiene entity_id en JWT, solo ve su propia entidad.

### Impacto en Modelo de Datos

| Entidad | Operacion | Campo(s) afectado(s) | Evento |
|---------|-----------|---------------------|--------|
| `entities` | SELECT | Todos los campos | — |
| `entity_channels` | SELECT | entity_id, channel | — |
| `branches` | SELECT | Todos los campos | — |

### Criterios de Aceptacion (Gherkin)

```gherkin
Scenario: Ver detalle de entidad con sucursales y canales
  Given una entidad "Banco Norte" con canales ["web", "mobile"] y 3 sucursales
  And un Super Admin autenticado con permiso "p_entities_detail"
  When envio GET /entities/:id
  Then recibo HTTP 200 con entity.name "Banco Norte"
  And channels tiene 2 elementos
  And branches tiene 3 elementos con id, name, code, status
  And user_count es un entero >= 0

Scenario: Entidad sin sucursales retorna branches vacio
  Given una entidad sin sucursales
  When envio GET /entities/:id
  Then recibo HTTP 200 con branches = []

Scenario: Entidad inexistente retorna 404
  When envio GET /entities/:uuid_inexistente
  Then recibo HTTP 404 con ENTITY_NOT_FOUND

Scenario: Sin permiso retorna 403
  Given usuario sin permiso "p_entities_detail"
  When envio GET /entities/:id
  Then recibo HTTP 403
```

### Trazabilidad de Pruebas

| Test ID | Tipo | Descripcion |
|---------|------|-------------|
| TP-ORG-08-01 | Positivo | Detalle retorna todos los campos de la entidad |
| TP-ORG-08-02 | Positivo | Channels incluidos correctamente |
| TP-ORG-08-03 | Positivo | Branches incluidas con todos sus campos |
| TP-ORG-08-04 | Positivo | user_count retorna conteo correcto |
| TP-ORG-08-05 | Positivo | Entidad sin sucursales retorna branches vacio |
| TP-ORG-08-06 | Negativo | Entidad inexistente retorna 404 |
| TP-ORG-08-07 | Negativo | Sin permiso retorna 403 |
| TP-ORG-08-08 | Negativo | Sin JWT retorna 401 |
| TP-ORG-08-09 | Negativo | ID no UUID retorna 422 |
| TP-ORG-08-10 | Seguridad | RLS impide ver entidad de otro tenant |
| TP-ORG-08-11 | Integracion | Admin Entidad solo ve detalle de su entidad |
| TP-ORG-08-12 | E2E | Click en fila de grilla → ver detalle con sucursales |
| TP-ORG-08-13 | E2E | Detalle muestra boton editar/eliminar segun permisos |

### Sin Ambiguedades

- **Supuestos prohibidos:** No se asume que user_count se obtiene con JOIN cross-database (es cross-service). No se asume que branches se paginan en detalle (se listan todas). No se asume que el detalle incluye historial de auditoria.
- **Decisiones cerradas:** Branches se cargan todas (sin paginacion) porque la cantidad por entidad es baja (<50 tipico). User count via HTTP sync o contador denormalizado (decision de implementacion). Canales como array de strings. Endpoint de solo lectura sin eventos.
- **Fuera de alcance explicito:** Historial de cambios de la entidad. Estadisticas de solicitudes por entidad. Mapa de sucursales. Paginacion de sucursales en detalle.
- **TODO explicitos = 0**

---

## RF-ORG-09 — Eliminar/desactivar entidad con validacion de dependencias

### Execution Sheet

| Campo | Valor |
|-------|-------|
| **ID** | RF-ORG-09 |
| **Titulo** | Eliminar entidad sin sucursales o desactivar entidad con sucursales |
| **Actor(es)** | Super Admin, Admin Entidad (solo su organizacion) |
| **Prioridad** | Alta |
| **Severidad** | P0 |
| **Flujo origen** | FL-ORG-02 seccion 8a |
| **HU origen** | HU005 |

### Precondiciones

| # | Condicion | Detalle |
|---|-----------|---------|
| 1 | Organization Service operativo | SA.organization accesible |
| 2 | Entidad existente en el tenant | `id` valido |
| 3 | Super Admin o Admin Entidad autenticado | JWT con `p_entities_delete` |
| 4 | RLS activo | `tenant_id` resuelto desde JWT |
| 5 | RabbitMQ operativo | Para publicacion de eventos |

### Entradas

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| `Authorization` | string (Bearer JWT) | Si | Header | JWT con `p_entities_delete` | — |
| `id` | uuid | Si | URL path `/entities/:id` | UUID valido, entidad existente en el tenant | — |

### Proceso (Happy Path) — Eliminar (sin sucursales)

| # | Paso | Responsable |
|---|------|-------------|
| 1 | Recibir `DELETE /entities/:id` | API Gateway |
| 2 | Validar JWT y verificar permiso `p_entities_delete` | Organization Service |
| 3 | Activar RLS | Organization Service |
| 4 | Buscar entidad: `SELECT id, name FROM entities WHERE id = :id` (404 si no) | Organization Service |
| 5 | Contar sucursales: `SELECT COUNT(*) FROM branches WHERE entity_id = :id` | Organization Service |
| 6 | Si count = 0: En transaccion: `DELETE FROM entity_channels WHERE entity_id = :id` + `DELETE FROM entities WHERE id = :id` | Organization Service |
| 7 | Commit | Organization Service |
| 8 | Publicar `EntityDeletedEvent { entity_id, entity_name, tenant_id, deleted_by, timestamp }` | Organization Service |
| 9 | Retornar `204 No Content` | Organization Service |

### Proceso — Desactivar (con sucursales)

| # | Paso | Responsable |
|---|------|-------------|
| 1-5 | Igual que eliminar | Organization Service |
| 6 | Si count > 0: retornar `409` con `{ error: "ENTITY_HAS_BRANCHES", message: "Entidad tiene {N} sucursal(es). Solo se puede desactivar.", branch_count: N }` | Organization Service |

**Nota:** La desactivacion (cambio de status) se realiza via `PUT /entities/:id` (RF-ORG-07), no a traves de este endpoint. El DELETE solo ejecuta eliminacion fisica y retorna 409 si tiene sucursales, informando que debe desactivarse via edicion.

### Salidas

| Campo | Tipo | Destino | Efecto observable |
|-------|------|---------|-------------------|
| HTTP 204 | — | Response → SPA | Confirmacion de eliminacion exitosa |
| `EntityDeletedEvent` | evento async | RabbitMQ → Audit | Registro en audit_log |
| `entity_channels` | DELETE | SA.organization | Canales eliminados |
| `entities` | DELETE | SA.organization | Entidad eliminada fisicamente |

### Errores Tipados

| Codigo | Causa | Condicion de disparo | Respuesta esperada |
|--------|-------|---------------------|--------------------|
| `AUTH_UNAUTHORIZED` | JWT ausente/invalido | Token faltante o expirado | HTTP 401 |
| `AUTH_FORBIDDEN` | Sin permiso | Sin `p_entities_delete` | HTTP 403 |
| `ENTITY_NOT_FOUND` | Entidad inexistente | `id` no existe en el tenant | HTTP 404, "Entidad no encontrada" |
| `ENTITY_HAS_BRANCHES` | Entidad con sucursales | `branches` count > 0 | HTTP 409, "Entidad tiene {N} sucursal(es). Solo se puede desactivar." |
| `VALIDATION_ERROR` | ID invalido | `id` no es UUID valido | HTTP 422 |

### Casos Especiales y Variantes

- **Eliminacion fisica:** Consistente con la politica del proyecto (sin soft delete). Se eliminan `entity_channels` y `entities` en la misma transaccion.
- **Confirmacion en SPA:** El SPA muestra dialogo: "Esta entidad sera eliminada permanentemente. ¿Continuar?". Si tiene sucursales, el SPA muestra: "Esta entidad tiene N sucursales. Puede desactivarla desde la edicion." y deshabilita el boton de eliminar.
- **Entidad referenciada en otros servicios:** Los datos denormalizados (`entity_name`) en SA.identity y SA.catalog permanecen (RN-ORG-14). El `EntityDeletedEvent` permite limpieza eventual.
- **Orden de validacion:** 1) Existencia, 2) Sucursales check.

### Impacto en Modelo de Datos

| Entidad | Operacion | Campo(s) afectado(s) | Evento |
|---------|-----------|---------------------|--------|
| `entities` | SELECT | id, name | — |
| `entities` | DELETE | Fila completa | `EntityDeletedEvent` |
| `entity_channels` | DELETE (batch) | Todas las filas de la entidad | — (incluido en EntityDeletedEvent) |
| `branches` | SELECT (COUNT) | entity_id | — (verificacion) |
| `audit_log` (SA.audit) | INSERT (async) | operation='Eliminar', module='entidades' | Consume evento |

### Criterios de Aceptacion (Gherkin)

```gherkin
Scenario: Eliminar entidad sin sucursales exitosamente
  Given una entidad "Fintech Test" sin sucursales
  And un Super Admin con permiso "p_entities_delete"
  When envio DELETE /entities/:id
  Then recibo HTTP 204
  And la entidad y sus canales son eliminados fisicamente
  And se publica EntityDeletedEvent

Scenario: Entidad con sucursales retorna 409
  Given una entidad "Banco Norte" con 3 sucursales
  When envio DELETE /entities/:id
  Then recibo HTTP 409 con ENTITY_HAS_BRANCHES y branch_count = 3

Scenario: Entidad inexistente retorna 404
  When envio DELETE /entities/:uuid_inexistente
  Then recibo HTTP 404 con ENTITY_NOT_FOUND

Scenario: Sin permiso retorna 403
  Given usuario sin permiso "p_entities_delete"
  When envio DELETE /entities/:id
  Then recibo HTTP 403
```

### Trazabilidad de Pruebas

| Test ID | Tipo | Descripcion |
|---------|------|-------------|
| TP-ORG-09-01 | Positivo | Eliminar entidad sin sucursales retorna 204 |
| TP-ORG-09-02 | Positivo | entity_channels eliminados junto con la entidad |
| TP-ORG-09-03 | Positivo | EntityDeletedEvent publicado |
| TP-ORG-09-04 | Negativo | Entidad con sucursales retorna 409 con branch_count |
| TP-ORG-09-05 | Negativo | Entidad inexistente retorna 404 |
| TP-ORG-09-06 | Negativo | Sin JWT retorna 401 |
| TP-ORG-09-07 | Negativo | Sin permiso retorna 403 |
| TP-ORG-09-08 | Negativo | ID no UUID retorna 422 |
| TP-ORG-09-09 | Seguridad | RLS impide eliminar entidad de otro tenant |
| TP-ORG-09-10 | Transaccion | DELETE entity_channels y DELETE entities atomico |
| TP-ORG-09-11 | Integracion | Audit log registra eliminacion |
| TP-ORG-09-12 | E2E | Click eliminar → confirmacion → entidad desaparece de grilla |
| TP-ORG-09-13 | E2E | Entidad con sucursales → boton eliminar deshabilitado o dialogo informativo |

### Sin Ambiguedades

- **Supuestos prohibidos:** No se asume eliminacion en cascada de sucursales. No se asume desactivacion automatica (el admin debe hacerlo via edicion). No se asume que otros servicios limpian datos al recibir EntityDeletedEvent (es eventual).
- **Decisiones cerradas:** Sin sucursales = eliminacion fisica (D-ORG-02). Con sucursales = 409 (no se desactiva automaticamente, se informa). entity_channels se eliminan en la misma transaccion. Evento post-commit.
- **Fuera de alcance explicito:** Desactivacion automatica de entidades con sucursales. Eliminacion forzada ignorando sucursales. Validacion de referencias cross-service antes de eliminar.
- **TODO explicitos = 0**

---

## RF-ORG-10 — CRUD de sucursales con selects en cascada

### Execution Sheet

| Campo | Valor |
|-------|-------|
| **ID** | RF-ORG-10 |
| **Titulo** | Crear, editar y eliminar sucursales dentro de una entidad |
| **Actor(es)** | Super Admin, Admin Entidad (solo su organizacion) |
| **Prioridad** | Alta |
| **Severidad** | P0 |
| **Flujo origen** | FL-ORG-02 seccion 7 |
| **HU origen** | HU008 |

### Precondiciones

| # | Condicion | Detalle |
|---|-----------|---------|
| 1 | Organization Service operativo | SA.organization accesible |
| 2 | Entidad existente y activa | `entity_id` valido en el tenant |
| 3 | Super Admin o Admin Entidad autenticado | JWT con `p_branches_create`, `p_branches_edit` o `p_branches_delete` segun operacion |
| 4 | Parametros cargados | `provinces`, `cities` (con parent_key) en cache Redis |
| 5 | RLS activo | `tenant_id` resuelto desde JWT |
| 6 | RabbitMQ operativo | Para publicacion de eventos |

### Entradas — Crear/Editar

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| `Authorization` | string (Bearer JWT) | Si | Header | JWT con `p_branches_create` (POST) o `p_branches_edit` (PUT) | — |
| `entity_id` | uuid | Si | URL path `/entities/:entityId/branches` | UUID valido, entidad existente | — |
| `id` | uuid | Solo en PUT | URL path `/entities/:entityId/branches/:id` | UUID valido, sucursal existente | — |
| `name` | string | Si | Body JSON | No vacio, trim, 3-200 chars | — |
| `code` | string | Si | Body JSON | No vacio, trim, 3-50 chars, unico por tenant | RN-ORG-12 |
| `address` | string | No | Body JSON | Trim, max 500 chars | — |
| `province` | string | Si | Body JSON | Debe existir en parametro `provinces` | RN-ORG-11 |
| `city` | string | Si | Body JSON | Debe existir en `cities` con parent_key = province | RN-ORG-11 |
| `status` | string | Si | Body JSON | Enum: `active`, `inactive`, `suspended` | — |
| `manager` | string | No | Body JSON | Trim, max 200 chars | — |
| `phone_code` | string | No | Body JSON | Max 10 chars | — |
| `phone` | string | No | Body JSON | Max 20 chars | — |

### Entradas — Eliminar

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| `Authorization` | string (Bearer JWT) | Si | Header | JWT con `p_branches_delete` | — |
| `entity_id` | uuid | Si | URL path | UUID valido | — |
| `id` | uuid | Si | URL path `/entities/:entityId/branches/:id` | UUID valido, sucursal existente | — |

### Proceso (Happy Path) — Crear Sucursal

| # | Paso | Responsable |
|---|------|-------------|
| 1 | Recibir `POST /entities/:entityId/branches` con body | API Gateway |
| 2 | Validar JWT y verificar permiso `p_branches_create` | Organization Service |
| 3 | Activar RLS | Organization Service |
| 4 | Verificar que la entidad `:entityId` existe | Organization Service |
| 5 | Validar formato de entradas: name, code, province, city, status | Organization Service |
| 6 | Verificar unicidad de `code` por tenant: `SELECT COUNT(*) FROM branches WHERE code = :code` (RLS filtra por tenant) | Organization Service |
| 7 | Generar UUIDv7 | Organization Service |
| 8 | `INSERT INTO branches (id, entity_id, tenant_id, name, code, address, city, province, status, manager, phone_code, phone, created_at, updated_at, created_by, updated_by)` | Organization Service |
| 9 | Publicar `BranchCreatedEvent { branch_id, entity_id, tenant_id, name, code, created_by, timestamp }` | Organization Service |
| 10 | Retornar `201 Created` con `{ id, name, code, address, province, city, status, manager, phone, created_at }` | Organization Service |

### Proceso (Happy Path) — Editar Sucursal

| # | Paso | Responsable |
|---|------|-------------|
| 1 | Recibir `PUT /entities/:entityId/branches/:id` con body | API Gateway |
| 2 | Validar JWT y verificar permiso `p_branches_edit` | Organization Service |
| 3 | Activar RLS | Organization Service |
| 4 | Verificar que la sucursal `:id` existe y pertenece a la entidad `:entityId` | Organization Service |
| 5 | Validar formato de entradas | Organization Service |
| 6 | Verificar unicidad de `code` excluyendo la propia: `WHERE code = :code AND id != :id` | Organization Service |
| 7 | `UPDATE branches SET name = :name, code = :code, ...` | Organization Service |
| 8 | Publicar `BranchUpdatedEvent { branch_id, entity_id, tenant_id, updated_by, timestamp }` | Organization Service |
| 9 | Retornar `200 OK` con `{ id, name, code, address, province, city, status, manager, phone, updated_at }` | Organization Service |

### Proceso (Happy Path) — Eliminar Sucursal

| # | Paso | Responsable |
|---|------|-------------|
| 1 | Recibir `DELETE /entities/:entityId/branches/:id` | API Gateway |
| 2 | Validar JWT y verificar permiso `p_branches_delete` | Organization Service |
| 3 | Activar RLS | Organization Service |
| 4 | Verificar que la sucursal `:id` existe y pertenece a la entidad `:entityId` | Organization Service |
| 5 | `DELETE FROM branches WHERE id = :id` | Organization Service |
| 6 | Publicar `BranchDeletedEvent { branch_id, entity_id, branch_name, tenant_id, deleted_by, timestamp }` | Organization Service |
| 7 | Retornar `204 No Content` | Organization Service |

### Salidas

| Campo | Tipo | Destino | Efecto observable |
|-------|------|---------|-------------------|
| Crear: `201 Created` | object | Response → SPA | Sucursal creada con todos los campos |
| Editar: `200 OK` | object | Response → SPA | Sucursal actualizada |
| Eliminar: `204 No Content` | — | Response → SPA | Sucursal eliminada |
| `BranchCreatedEvent` / `BranchUpdatedEvent` / `BranchDeletedEvent` | evento async | RabbitMQ → Audit | Registro en audit_log |
| `branches` (row) | INSERT / UPDATE / DELETE | SA.organization | Operacion sobre registro |

### Errores Tipados

| Codigo | Causa | Condicion de disparo | Respuesta esperada |
|--------|-------|---------------------|--------------------|
| `AUTH_UNAUTHORIZED` | JWT ausente/invalido | Token faltante o expirado | HTTP 401 |
| `AUTH_FORBIDDEN` | Sin permiso | Sin `p_branches_create/edit/delete` segun operacion | HTTP 403 |
| `ENTITY_NOT_FOUND` | Entidad inexistente | `entityId` no existe en el tenant | HTTP 404, "Entidad no encontrada" |
| `BRANCH_NOT_FOUND` | Sucursal inexistente | `id` no existe o no pertenece a `entityId` | HTTP 404, "Sucursal no encontrada" |
| `BRANCH_CODE_DUPLICATE` | Codigo duplicado | Ya existe otra sucursal con el mismo `code` en el tenant | HTTP 409, "Ya existe una sucursal con este codigo" |
| `VALIDATION_ERROR` | Entrada malformada | Campos vacios, `status` no enum, city no pertenece a province, code fuera de rango | HTTP 422, detalle de campos |

### Casos Especiales y Variantes

- **Selects en cascada:** Mismo patron que entidades (RN-ORG-11). Al seleccionar provincia, las ciudades se filtran por parent_key.
- **Codigo unico por tenant (no por entidad):** El codigo de sucursal es unico a nivel tenant, no solo a nivel entidad. Dos entidades del mismo tenant no pueden tener sucursales con el mismo codigo (RN-ORG-12).
- **Eliminacion fisica:** Las sucursales se eliminan fisicamente sin validacion de dependencias cross-service (RN-ORG-15). Son hojas del arbol organizacional.
- **Confirmacion en SPA:** El SPA muestra dialogo de confirmacion antes de eliminar.
- **Sucursal y entidad padre:** El endpoint anida bajo `/entities/:entityId/`. Si la sucursal existe pero no pertenece a esa entidad, retorna 404.
- **Formulario inline:** Las sucursales se gestionan desde la pantalla de detalle/edicion de la entidad, no como pagina independiente.

### Impacto en Modelo de Datos

| Entidad | Operacion | Campo(s) afectado(s) | Evento |
|---------|-----------|---------------------|--------|
| `entities` | SELECT | id (verificacion existencia) | — |
| `branches` | INSERT (crear) | Todos los campos | `BranchCreatedEvent` |
| `branches` | UPDATE (editar) | name, code, address, city, province, status, manager, phone_code, phone, updated_at, updated_by | `BranchUpdatedEvent` |
| `branches` | DELETE (eliminar) | Fila completa | `BranchDeletedEvent` |
| `audit_log` (SA.audit) | INSERT (async) | operation, module='sucursales' | Consume eventos |

### Criterios de Aceptacion (Gherkin)

```gherkin
Scenario: Crear sucursal exitosamente
  Given una entidad "Banco Norte" existente
  And un Super Admin con permiso "p_branches_create"
  When envio POST /entities/:entityId/branches con name "Sucursal Centro", code "SUC-001", province "Buenos Aires", city "La Plata", status "active"
  Then recibo HTTP 201 con id, name "Sucursal Centro", code "SUC-001"
  And se publica BranchCreatedEvent

Scenario: Editar sucursal exitosamente
  Given una sucursal "SUC-001" en entidad "Banco Norte"
  And un Super Admin con permiso "p_branches_edit"
  When envio PUT /entities/:entityId/branches/:id con manager "Juan Perez"
  Then recibo HTTP 200 con manager "Juan Perez"
  And se publica BranchUpdatedEvent

Scenario: Eliminar sucursal exitosamente
  Given una sucursal "SUC-001" en entidad "Banco Norte"
  And un Super Admin con permiso "p_branches_delete"
  When envio DELETE /entities/:entityId/branches/:id
  Then recibo HTTP 204
  And la sucursal se elimina fisicamente
  And se publica BranchDeletedEvent

Scenario: Codigo duplicado retorna 409
  Given una sucursal con code "SUC-001" en el tenant
  When envio POST /entities/:entityId/branches con code "SUC-001"
  Then recibo HTTP 409 con BRANCH_CODE_DUPLICATE

Scenario: Sucursal no pertenece a la entidad retorna 404
  Given sucursal "SUC-001" pertenece a entidad "ent-A"
  When envio DELETE /entities/ent-B/branches/SUC-001
  Then recibo HTTP 404 con BRANCH_NOT_FOUND

Scenario: Ciudad no pertenece a provincia retorna 422
  When envio POST /entities/:entityId/branches con province "Buenos Aires" y city "Rosario"
  Then recibo HTTP 422
```

### Trazabilidad de Pruebas

| Test ID | Tipo | Descripcion |
|---------|------|-------------|
| TP-ORG-10-01 | Positivo | Crear sucursal retorna 201 con todos los campos |
| TP-ORG-10-02 | Positivo | Editar sucursal retorna 200 con campos actualizados |
| TP-ORG-10-03 | Positivo | Eliminar sucursal retorna 204 |
| TP-ORG-10-04 | Positivo | BranchCreatedEvent publicado |
| TP-ORG-10-05 | Positivo | BranchUpdatedEvent publicado |
| TP-ORG-10-06 | Positivo | BranchDeletedEvent publicado |
| TP-ORG-10-07 | Negativo | Codigo duplicado en tenant retorna 409 |
| TP-ORG-10-08 | Negativo | Entidad inexistente retorna 404 |
| TP-ORG-10-09 | Negativo | Sucursal inexistente retorna 404 |
| TP-ORG-10-10 | Negativo | Sucursal no pertenece a la entidad retorna 404 |
| TP-ORG-10-11 | Negativo | city no pertenece a province retorna 422 |
| TP-ORG-10-12 | Negativo | name vacio retorna 422 |
| TP-ORG-10-13 | Negativo | Sin JWT retorna 401 |
| TP-ORG-10-14 | Negativo | Sin permiso retorna 403 |
| TP-ORG-10-15 | Seguridad | RLS impide operar sucursales de otro tenant |
| TP-ORG-10-16 | Seguridad | Sucursal hereda tenant_id de la entidad al crear |
| TP-ORG-10-17 | Integracion | Audit log registra creacion, edicion y eliminacion |
| TP-ORG-10-18 | Integracion | Eliminacion fisica verificada (SELECT post-DELETE retorna 0) |
| TP-ORG-10-19 | E2E | En detalle de entidad → click "Agregar Sucursal" → formulario → guardar → aparece en grilla |
| TP-ORG-10-20 | E2E | Seleccionar provincia → ciudades se filtran |
| TP-ORG-10-21 | E2E | Editar sucursal → formulario precargado → modificar → guardar |
| TP-ORG-10-22 | E2E | Eliminar sucursal → confirmacion → desaparece de grilla |

### Sin Ambiguedades

- **Supuestos prohibidos:** No se asume que el codigo de sucursal es unico solo dentro de la entidad (es unico por tenant). No se asume que sucursales tienen dependencias que impidan eliminacion. No se asume que la sucursal tiene su propia pagina (se gestiona desde detalle de entidad).
- **Decisiones cerradas:** Codigo unico por tenant (RN-ORG-12). Eliminacion fisica sin validacion cross-service (RN-ORG-15). Cascada provincia→ciudad via parametros. Formulario inline en detalle de entidad. Endpoints anidados bajo /entities/:entityId/branches. Sucursal debe pertenecer a la entidad del path.
- **Fuera de alcance explicito:** Transferencia de sucursal entre entidades. Geolocalizacion automatica. Horarios de atencion. Limite maximo de sucursales por entidad.
- **TODO explicitos = 0**

---

## Changelog

### v1.1.0 (2026-03-15)
- RF-ORG-06 a RF-ORG-10 documentados (FL-ORG-02: Gestionar Entidades y Sucursales)
- 8 reglas de negocio agregadas (RN-ORG-08 a RN-ORG-15)
- Trazabilidad completa con FL-ORG-02 y HU005, HU006, HU007, HU008

### v1.0.0 (2026-03-15)
- RF-ORG-01 a RF-ORG-05 documentados (modulo Organization, flujo FL-ORG-01)
- 7 reglas de negocio definidas (RN-ORG-01 a RN-ORG-07)
- Trazabilidad completa con FL-ORG-01 y HU002, HU003, HU004
