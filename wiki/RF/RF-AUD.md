# RF-AUD — Requerimientos Funcionales del Modulo Auditoria

> **Proyecto:** Unazul Backoffice
> **Modulo:** Auditoria (SA.audit)
> **Version:** 1.2.0
> **Fecha:** 2026-03-15
> **Flujos:** FL-AUD-01
> **HUs:** HU027

---

## 1. Resumen

| ID | Titulo | Actor | Flujo | HU | Prioridad | Estado |
|----|--------|-------|-------|----|-----------|--------|
| RF-AUD-01 | Consultar log de auditoria con filtros y paginacion | Auditor, Super Admin | FL-AUD-01 s7 | HU027 | Alta | Documentado |
| RF-AUD-02 | Exportar log de auditoria a Excel/CSV | Auditor, Super Admin | FL-AUD-01 s7 | HU027 | Alta | Documentado |
| RF-AUD-03 | Ingesta asincrona de eventos de dominio | Audit Service (consumer) | FL-AUD-01 s6 | HU027 | Critica | Documentado |

---

## 2. Reglas de Negocio

| ID | Regla | Origen |
|----|-------|--------|
| RN-AUD-01 | `audit_log` es INSERT-only. No se permite UPDATE ni DELETE desde la API ni desde la base de datos (protegido por falta de grants) | FL-AUD-01, modelo datos |
| RN-AUD-02 | El filtro por tenant es en la query (WHERE `tenant_id` = ?), NO por RLS, porque la ingesta async no tiene contexto HTTP | FL-AUD-01 s9 |
| RN-AUD-03 | Consulta sync: `tenant_id` se obtiene del JWT del usuario autenticado | FL-AUD-01 s7 |
| RN-AUD-04 | Ingesta async: `tenant_id` se obtiene del payload del evento de dominio | FL-AUD-01 s6 |
| RN-AUD-05 | Tabla `audit_log` particionada por mes en `occurred_at` | modelo datos |
| RN-AUD-06 | Dual write: PG es fuente de verdad; ES (`audit-{yyyy.MM.dd}`) es best-effort async | FL-AUD-01 s6 |
| RN-AUD-07 | MassTransit retry: 3 intentos con backoff exponencial (5s, 15s, 45s); si agota reintentos va a DLQ | FL-AUD-01 s8a |
| RN-AUD-08 | Exportacion limitada a 10,000 registros. Si excede, retorna mensaje indicando reducir rango de fechas | FL-AUD-01 s8b |
| RN-AUD-09 | `user_name` se denormaliza al momento de escribir (viene en el evento). No se hace lookup sync a SA.identity | modelo datos |
| RN-AUD-10 | `operation` mapea al enum `audit_operation_type`: Crear, Editar, Eliminar, Login, Logout, Cambiar Contrasena, Cambiar Estado, Liquidar, Exportar, Consultar, Otro | modelo datos |

---

## 3. RF-AUD-01 — Consultar log de auditoria con filtros y paginacion

### 3.1 Execution Sheet

| Campo | Valor |
|-------|-------|
| **ID** | RF-AUD-01 |
| **Titulo** | Consultar log de auditoria con filtros y paginacion |
| **Actor(es)** | Auditor, Super Admin |
| **Prioridad** | Alta |
| **Severidad** | Alta |
| **Flujo origen** | FL-AUD-01 seccion 7 |
| **HU** | HU027 CA-1, CA-2, CA-4 |

### 3.2 Precondiciones

| # | Condicion | Detalle |
|---|-----------|---------|
| P1 | Usuario autenticado | JWT valido con `tenant_id` en claims |
| P2 | Permiso requerido | `audit:read` |
| P3 | Audit Service operativo | SA.audit accesible |
| P4 | Tabla `audit_log` existe | Con particiones mensuales activas |

### 3.3 Entradas

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| `tenant_id` | UUID | Si | JWT claim | — | RN-AUD-03 |
| `page` | int | No (default 1) | Query param | >= 1 | — |
| `size` | int | No (default 20) | Query param | 1..100 | — |
| `user_id` | UUID | No | Query param | UUID v4 valido | — |
| `operation` | string | No | Query param | Valor del enum `audit_operation_type` | RN-AUD-10 |
| `module` | string | No | Query param | String no vacio, max 100 chars | — |
| `from` | ISO 8601 datetime | No | Query param | Fecha valida, <= `to` si ambos presentes | — |
| `to` | ISO 8601 datetime | No | Query param | Fecha valida, >= `from` si ambos presentes | — |
| `sort` | string | No (default `occurred_at`) | Query param | Valores permitidos: `occurred_at`, `operation`, `module`, `user_name` | — |
| `order` | string | No (default `desc`) | Query param | `asc` o `desc` | — |

### 3.4 Pasos del Proceso (Happy Path)

| # | Responsable | Accion |
|---|-------------|--------|
| 1 | SPA | GET `/audit-log?page={page}&size={size}&...filtros` |
| 2 | API Gateway | Valida JWT, extrae `tenant_id`, forward a Audit Service |
| 3 | Audit Service | Valida permiso `audit:read` |
| 4 | Audit Service | Construye query: `SELECT * FROM audit_log WHERE tenant_id = :tenant_id` + filtros opcionales + `ORDER BY :sort :order` + `LIMIT :size OFFSET (:page - 1) * :size` |
| 5 | Audit Service | Ejecuta COUNT para total (con mismos filtros) |
| 6 | SA.audit (PG) | Retorna resultados usando indices compuestos y partition pruning |
| 7 | Audit Service | Mapea resultados a DTOs |
| 8 | SPA | Renderiza DataTable con paginacion |

### 3.5 Salidas

| Campo | Tipo | Destino | Efecto observable |
|-------|------|---------|-------------------|
| `items` | AuditLogDto[] | Response body | Array de registros |
| `items[].id` | UUID | — | ID del registro |
| `items[].user_name` | string | — | Nombre del usuario que realizo la accion |
| `items[].operation` | string | — | Tipo de operacion (badge en UI) |
| `items[].action` | string | — | Descripcion legible de la accion |
| `items[].module` | string | — | Modulo afectado |
| `items[].detail` | string? | — | Detalle adicional (nullable) |
| `items[].occurred_at` | ISO 8601 | — | Fecha y hora de la accion |
| `items[].ip_address` | string | — | IP de origen |
| `items[].entity_type` | string? | — | Tipo de entidad afectada (nullable) |
| `items[].entity_id` | UUID? | — | ID de entidad afectada (nullable) |
| `total` | int | — | Total de registros que cumplen filtros |
| `page` | int | — | Pagina actual |
| `page_size` | int | — | Tamanio de pagina solicitado |

**HTTP:** `200 OK` con body JSON `{ items[], total, page, page_size }`.

> **Formato estandar de paginacion:** Todas las respuestas paginadas del sistema siguen el contrato `{ items: T[], total: number, page: number, page_size: number }`. El campo `page_size` refleja el tamanio solicitado (query param `size`), no la cantidad de items retornados (que puede ser menor en la ultima pagina).

### 3.6 Errores Tipados

| Codigo | HTTP | Causa | Condicion de disparo | Respuesta esperada |
|--------|------|-------|----------------------|-------------------|
| AUD-01-E01 | 401 | JWT ausente o invalido | No Authorization header o token expirado | `{ error: "Unauthorized" }` |
| AUD-01-E02 | 403 | Sin permiso `audit:read` | Usuario sin rol Auditor ni Super Admin | `{ error: "Forbidden", detail: "audit:read required" }` |
| AUD-01-E03 | 422 | `from` > `to` | Rango de fechas invertido | `{ error: "Unprocessable Entity", detail: "from must be <= to" }` |
| AUD-01-E04 | 422 | `operation` invalida | Valor no pertenece a `audit_operation_type` | `{ error: "Unprocessable Entity", detail: "Invalid operation value" }` |
| AUD-01-E05 | 422 | `page` o `size` fuera de rango | page < 1 o size < 1 o size > 100 | `{ error: "Unprocessable Entity", detail: "page >= 1, 1 <= size <= 100" }` |
| AUD-01-E06 | 500 | Error de base de datos | SA.audit no disponible | `{ error: "Internal Server Error" }` + log structured |
| AUD-01-E07 | 504 | Timeout en COUNT | COUNT query excede timeout configurado (3s) | `504 { code: "QUERY_TIMEOUT" }` |

### 3.7 Casos Especiales y Variantes

| Variante | Comportamiento |
|----------|---------------|
| Sin filtros | Retorna todas las entradas del tenant, orden `occurred_at DESC` |
| Filtro por `user_id` | Usa indice `(tenant_id, user_id, occurred_at DESC)` |
| Filtro por `module` | Usa indice `(tenant_id, module, occurred_at DESC)` |
| Filtro por rango de fechas | Partition pruning en `occurred_at` — solo escanea particiones relevantes |
| Combinacion de filtros | AND logico entre todos los filtros presentes |
| Resultado vacio | 200 con `{ items: [], total: 0, page: 1, page_size: 20 }` |
| Sin edicion ni eliminacion | La UI NO muestra botones de editar/eliminar (RN-AUD-01). La API NO expone endpoints PUT/PATCH/DELETE |
| Timeout en COUNT | La consulta paginada ejecuta dos queries: COUNT (total con filtros) + SELECT (datos de la pagina). Timeout recomendado para COUNT: 3 segundos (configurable). Para tenants con >1M registros sin filtro de fecha, considerar retornar count estimado (reltuples) con flag `exact: false` en la respuesta |

### 3.8 Impacto en Modelo de Datos

| Entidad | Operacion | Campos |
|---------|-----------|--------|
| `audit_log` | SELECT | Todos los campos |

No se crean, modifican ni eliminan registros en esta operacion.

### 3.9 Criterios de Aceptacion Expandidos (Gherkin)

```gherkin
Feature: RF-AUD-01 Consultar log de auditoria

  Background:
    Given usuario autenticado con permiso "audit:read"
    And tenant_id "T1" en JWT

  Scenario: Consulta exitosa sin filtros
    When GET /audit-log?page=1&size=20
    Then status 200
    And response contiene "items" array
    And todos los items tienen tenant_id = "T1"
    And items ordenados por occurred_at DESC
    And response contiene "total", "page", "page_size"

  Scenario: Filtro por usuario
    When GET /audit-log?user_id=<user_id>
    Then status 200
    And todos los items tienen user_id = <user_id>

  Scenario: Filtro por tipo de operacion
    When GET /audit-log?operation=Crear
    Then status 200
    And todos los items tienen operation = "Crear"

  Scenario: Filtro por modulo
    When GET /audit-log?module=solicitudes
    Then status 200
    And todos los items tienen module = "solicitudes"

  Scenario: Filtro por rango de fechas
    When GET /audit-log?from=2026-01-01T00:00:00Z&to=2026-01-31T23:59:59Z
    Then status 200
    And todos los items tienen occurred_at entre from y to

  Scenario: Combinacion de filtros (AND)
    When GET /audit-log?operation=Editar&module=productos&from=2026-01-01T00:00:00Z&to=2026-01-31T23:59:59Z
    Then status 200
    And todos los items cumplen todos los filtros simultaneamente

  Scenario: Resultado vacio
    When GET /audit-log?module=modulo_inexistente
    Then status 200
    And items = []
    And total = 0

  Scenario: 401 sin JWT
    Given usuario NO autenticado
    When GET /audit-log
    Then status 401

  Scenario: 403 sin permiso
    Given usuario autenticado SIN permiso "audit:read"
    When GET /audit-log
    Then status 403

  Scenario: 422 rango invertido
    When GET /audit-log?from=2026-12-31T00:00:00Z&to=2026-01-01T00:00:00Z
    Then status 422
    And error contiene "from must be <= to"

  Scenario: 422 operacion invalida
    When GET /audit-log?operation=INVALIDA
    Then status 422
    And error contiene "Invalid operation value"

  Scenario: Paginacion pagina 2
    Given existen 25 registros
    When GET /audit-log?page=2&size=20
    Then status 200
    And items.length = 5
    And total = 25
    And page = 2

  Scenario: Sort por module ASC
    When GET /audit-log?sort=module&order=asc
    Then status 200
    And items ordenados por module ASC
```

### 3.10 Trazabilidad de Pruebas

| TP | Escenario | Tipo |
|----|-----------|------|
| TP-AUD-01 | Consulta exitosa sin filtros | Positivo |
| TP-AUD-02 | Filtro por user_id | Positivo |
| TP-AUD-03 | Filtro por operation | Positivo |
| TP-AUD-04 | Filtro por module | Positivo |
| TP-AUD-05 | Filtro por rango de fechas | Positivo |
| TP-AUD-06 | Combinacion de filtros (AND) | Positivo |
| TP-AUD-07 | Resultado vacio devuelve 200 con items=[] | Positivo |
| TP-AUD-08 | Sort por module ASC | Positivo |
| TP-AUD-09 | 401 sin JWT | Negativo |
| TP-AUD-10 | 403 sin permiso audit:read | Negativo |
| TP-AUD-11 | 422 rango de fechas invertido | Negativo |
| TP-AUD-12 | 422 operation invalida | Negativo |
| TP-AUD-13 | 422 page < 1 | Negativo |
| TP-AUD-14 | 422 size > 100 | Negativo |
| TP-AUD-15 | 422 size < 1 | Negativo |
| TP-AUD-16 | Paginacion pagina 2 con offset correcto | Integracion |
| TP-AUD-17 | Partition pruning por rango de fechas (EXPLAIN) | Integracion |
| TP-AUD-18 | Indice tenant_id+module+occurred_at usado (EXPLAIN) | Integracion |
| TP-AUD-19 | Aislamiento multi-tenant: tenant A no ve datos de tenant B | Integracion |
| TP-AUD-20 | E2E: login → navegar /auditoria → ver DataTable con registros | E2E |
| TP-AUD-21 | E2E: aplicar filtros → verificar resultados filtrados | E2E |

**Total RF-AUD-01:** 8 positivos + 7 negativos + 4 integracion + 2 E2E = **21 tests**

### 3.11 No Ambiguities Left

| Categoria | Decision |
|-----------|----------|
| Supuestos prohibidos | Ningun campo se infiere; `tenant_id` siempre del JWT, nunca del query param |
| Decisiones cerradas | Sort default: `occurred_at DESC`. Tamanio pagina default: 20. Maximo por pagina: 100. No existe endpoint para busqueda full-text en ES (fuera de scope FL-AUD-01) |
| TODO explicitos | 0 |
| Fuera de scope | Busqueda full-text via Elasticsearch. Alertas automaticas. Dashboards en tiempo real. Purge/retencion automatica |
| Dependencias externas | SA.audit (PG) debe estar operativo. Particiones mensuales creadas por DBA o job automatico. No depende de ES para consulta (PG es fuente de verdad) |
| Indice entity_type/entity_id | El indice (entity_type, entity_id) existe en el modelo pero no esta expuesto como filtro en la API REST en esta version. Reservado para consultas directas a BD y futuro endpoint de historial de entidad |
| Indice covering recomendado | Para combinaciones frecuentes de filtros multi-campo, evaluar indice covering `(tenant_id, operation, occurred_at DESC)`. Los 4 indices actuales pueden requerir index intersection para queries complejas |

---

## 4. RF-AUD-02 — Exportar log de auditoria a Excel/CSV

### 4.1 Execution Sheet

| Campo | Valor |
|-------|-------|
| **ID** | RF-AUD-02 |
| **Titulo** | Exportar log de auditoria a Excel/CSV |
| **Actor(es)** | Auditor, Super Admin |
| **Prioridad** | Alta |
| **Severidad** | Media |
| **Flujo origen** | FL-AUD-01 seccion 7, 8b |
| **HU** | HU027 CA-3 |

### 4.2 Precondiciones

| # | Condicion | Detalle |
|---|-----------|---------|
| P1 | Usuario autenticado | JWT valido con `tenant_id` en claims |
| P2 | Permiso requerido | `audit:export` |
| P3 | Audit Service operativo | SA.audit accesible |

### 4.3 Entradas

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| `tenant_id` | UUID | Si | JWT claim | — | RN-AUD-03 |
| `format` | string | Si | Query param | `xlsx` o `csv` | — |
| `user_id` | UUID | No | Query param | UUID v4 valido | — |
| `operation` | string | No | Query param | Valor del enum `audit_operation_type` | RN-AUD-10 |
| `module` | string | No | Query param | String no vacio, max 100 chars | — |
| `from` | ISO 8601 datetime | No | Query param | Fecha valida, <= `to` si ambos | — |
| `to` | ISO 8601 datetime | No | Query param | Fecha valida, >= `from` si ambos | — |

### 4.4 Pasos del Proceso (Happy Path)

| # | Responsable | Accion |
|---|-------------|--------|
| 1 | SPA | GET `/audit-log/export?format=xlsx&...filtros` |
| 2 | API Gateway | Valida JWT, extrae `tenant_id`, forward a Audit Service |
| 3 | Audit Service | Valida permiso `audit:export` |
| 4 | Audit Service | Ejecuta COUNT con filtros para verificar limite |
| 5 | Audit Service | Si count <= 10,000: ejecuta SELECT con cursor server-side (`DECLARE CURSOR ... FETCH 1000`) para no cargar todos los registros en memoria |
| 6 | Audit Service | Genera archivo en formato solicitado con streaming: para **CSV** escribe directamente al response stream (fila por fila desde cursor); para **xlsx** usa EPPlus/ClosedXML con streaming mode (row-by-row flush) para mantener consumo de memoria acotado |
| 7 | Audit Service | Retorna archivo como chunked transfer encoding (stream binario) |
| 8 | SPA | Descarga archivo via browser download |

### 4.5 Salidas

| Campo | Tipo | Destino | Efecto observable |
|-------|------|---------|-------------------|
| Binary stream | application/vnd.openxmlformats-officedocument.spreadsheetml.sheet (xlsx) o text/csv | Response body | Archivo descargable |
| Content-Disposition | header | Response header | `attachment; filename="audit_log_{timestamp}.{ext}"` |

**HTTP:** `200 OK` con body binario y headers de descarga.

**Columnas del archivo exportado:**
- Fecha y hora (`occurred_at`)
- Usuario (`user_name`)
- Operacion (`operation`)
- Accion (`action`)
- Modulo (`module`)
- Detalle (`detail`)
- IP (`ip_address`)
- Entidad (`entity_type`)
- ID Entidad (`entity_id`)

### 4.6 Errores Tipados

| Codigo | HTTP | Causa | Condicion de disparo | Respuesta esperada |
|--------|------|-------|----------------------|-------------------|
| AUD-02-E01 | 401 | JWT ausente o invalido | No Authorization header o token expirado | `{ error: "Unauthorized" }` |
| AUD-02-E02 | 403 | Sin permiso `audit:export` | Usuario sin permiso de exportacion | `{ error: "Forbidden", detail: "audit:export required" }` |
| AUD-02-E03 | 422 | `format` invalido | Valor distinto a `xlsx` o `csv` | `{ error: "Unprocessable Entity", detail: "format must be xlsx or csv" }` |
| AUD-02-E04 | 422 | `from` > `to` | Rango de fechas invertido | `{ error: "Unprocessable Entity", detail: "from must be <= to" }` |
| AUD-02-E05 | 422 | Excede limite de 10,000 registros | Count con filtros > 10,000 | `{ error: "Unprocessable Entity", detail: "Result exceeds 10000 records. Narrow date range." }` |
| AUD-02-E06 | 500 | Error de base de datos o generacion | SA.audit no disponible o error en generacion de archivo | `{ error: "Internal Server Error" }` + log structured |
| AUD-02-E07 | 504 | Timeout en COUNT | COUNT query de verificacion excede timeout | `504 { code: "QUERY_TIMEOUT" }` |

### 4.7 Casos Especiales y Variantes

| Variante | Comportamiento |
|----------|---------------|
| Formato xlsx | Genera archivo Excel con encabezados en fila 1, datos desde fila 2. Columnas auto-ajustadas |
| Formato csv | Genera CSV con separador `,` y encoding UTF-8 BOM. Primera fila encabezados |
| Resultado vacio | Genera archivo con solo encabezados (0 filas de datos) |
| Exactamente 10,000 registros | Exporta OK (limite es estricto: > 10,000 es error) |
| Sin filtros | Exporta todos los registros del tenant (sujeto a limite 10,000) |
| Registro de exportacion | La propia accion de exportar genera un evento de auditoria con operation=Exportar |
| Estrategia de query y streaming | La query de datos usa cursor server-side con `DECLARE CURSOR` y `FETCH 1000` para no cargar los 10,000 registros en memoria simultaneamente. Para CSV, cada batch del cursor se escribe directo al response stream. Para xlsx, se usa streaming mode de EPPlus/ClosedXML (row-by-row flush). El response usa chunked transfer encoding |
| Timeout en COUNT de verificacion | El COUNT de verificacion de limite debe ejecutarse con los mismos filtros que el SELECT posterior (mismo partition pruning). Timeout recomendado para COUNT: 5 segundos (configurable). Si el COUNT excede el timeout, retornar 504 |

### 4.8 Impacto en Modelo de Datos

| Entidad | Operacion | Campos |
|---------|-----------|--------|
| `audit_log` | SELECT | Todos los campos (lectura para exportar) |
| `audit_log` | INSERT (indirecto) | La accion de exportar genera un evento que se ingesta como nuevo registro via RF-AUD-03 |

La propia accion de exportar genera un DomainEvent con operation=Exportar que es consumido por RF-AUD-03, creando una entrada en audit_log. Esto constituye un feedback loop de auto-auditoria.

### 4.9 Criterios de Aceptacion Expandidos (Gherkin)

```gherkin
Feature: RF-AUD-02 Exportar log de auditoria

  Background:
    Given usuario autenticado con permiso "audit:export"
    And tenant_id "T1" en JWT

  Scenario: Exportar a Excel sin filtros
    Given existen 50 registros de auditoria
    When GET /audit-log/export?format=xlsx
    Then status 200
    And Content-Type = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
    And Content-Disposition contiene "audit_log_"
    And archivo tiene 50 filas de datos + 1 fila de encabezados

  Scenario: Exportar a CSV con filtros
    When GET /audit-log/export?format=csv&module=solicitudes
    Then status 200
    And Content-Type = "text/csv"
    And todas las filas tienen modulo "solicitudes"

  Scenario: Exportar resultado vacio
    When GET /audit-log/export?format=xlsx&module=inexistente
    Then status 200
    And archivo tiene solo fila de encabezados

  Scenario: Limite exacto 10,000 registros
    Given existen exactamente 10000 registros con filtros aplicados
    When GET /audit-log/export?format=xlsx
    Then status 200
    And archivo tiene 10000 filas de datos

  Scenario: 422 excede limite
    Given existen 10001 registros con filtros aplicados
    When GET /audit-log/export?format=xlsx
    Then status 422
    And error contiene "Result exceeds 10000 records"

  Scenario: 422 formato invalido
    When GET /audit-log/export?format=pdf
    Then status 422
    And error contiene "format must be xlsx or csv"

  Scenario: 403 sin permiso
    Given usuario autenticado SIN permiso "audit:export"
    When GET /audit-log/export?format=xlsx
    Then status 403

  Scenario: 401 sin JWT
    Given usuario NO autenticado
    When GET /audit-log/export?format=xlsx
    Then status 401

  Scenario: Se genera evento de auditoria por la exportacion
    When GET /audit-log/export?format=xlsx
    Then status 200
    And se publica evento DomainEvent con operation = "Exportar" y module = "auditoria"
```

### 4.10 Trazabilidad de Pruebas

| TP | Escenario | Tipo |
|----|-----------|------|
| TP-AUD-22 | Exportar a Excel sin filtros — archivo valido | Positivo |
| TP-AUD-23 | Exportar a CSV con filtro por module | Positivo |
| TP-AUD-24 | Exportar resultado vacio — archivo con solo encabezados | Positivo |
| TP-AUD-25 | Exportar exactamente 10,000 registros OK | Positivo |
| TP-AUD-26 | 401 sin JWT | Negativo |
| TP-AUD-27 | 403 sin permiso audit:export | Negativo |
| TP-AUD-28 | 422 formato invalido (pdf) | Negativo |
| TP-AUD-29 | 422 rango de fechas invertido | Negativo |
| TP-AUD-30 | 422 excede limite 10,000 registros | Negativo |
| TP-AUD-31 | Archivo xlsx tiene columnas correctas y datos aislados por tenant | Integracion |
| TP-AUD-32 | Archivo csv encoding UTF-8 BOM con separador coma | Integracion |
| TP-AUD-33 | Exportacion genera evento de auditoria con operation=Exportar | Integracion |
| TP-AUD-34 | E2E: login → /auditoria → click Exportar Excel → descarga archivo | E2E |
| TP-AUD-35 | E2E: aplicar filtros → exportar CSV → verificar contenido filtrado | E2E |

**Total RF-AUD-02:** 4 positivos + 5 negativos + 3 integracion + 2 E2E = **14 tests**

### 4.11 No Ambiguities Left

| Categoria | Decision |
|-----------|----------|
| Supuestos prohibidos | No se asume que el usuario puede exportar sin permiso `audit:export` (distinto de `audit:read`) |
| Decisiones cerradas | Limite: 10,000 registros (inclusive). Formatos: solo xlsx y csv. CSV con UTF-8 BOM y separador coma. Nombre archivo: `audit_log_{yyyyMMdd_HHmmss}.{ext}`. La propia exportacion genera evento de auditoria |
| TODO explicitos | 0 |
| SLA de exportacion | SLA sugerido para exportacion sincrona: max 10 segundos para 10,000 filas. Exportacion asincrona para volumenes mayores diferida a version futura |
| Fuera de scope | Exportacion asincrona para volumenes mayores. Formato PDF. Exportacion programada/scheduled |
| Dependencias externas | SA.audit (PG) debe estar operativo. Libreria de generacion Excel (EPPlus/ClosedXML) disponible |

---

## 5. RF-AUD-03 — Ingesta asincrona de eventos de dominio

### 5.1 Execution Sheet

| Campo | Valor |
|-------|-------|
| **ID** | RF-AUD-03 |
| **Titulo** | Ingesta asincrona de eventos de dominio |
| **Actor(es)** | Audit Service (MassTransit consumer), Microservicios origen (publisher — secundario), RabbitMQ (transport — secundario) |
| **Prioridad** | Critica |
| **Severidad** | Critica |
| **Flujo origen** | FL-AUD-01 seccion 6, 8a |
| **HU** | HU027 (precondicion implicita: registros existen para consulta) |

### 5.2 Precondiciones

| # | Condicion | Detalle |
|---|-----------|---------|
| P1 | Audit Service levantado | MassTransit bus started, consumers registrados |
| P2 | RabbitMQ operativo | Exchange y queues configuradas |
| P3 | SA.audit accesible | Tabla `audit_log` con particiones mensuales |
| P4 | Microservicio origen publica evento | Evento de dominio con schema valido |

### 5.3 Entradas

**Schema esperado del mensaje RabbitMQ (DomainEvent):**

```json
{
  "tenant_id": "UUID (requerido)",
  "user_id": "UUID (requerido)",
  "user_name": "string (requerido)",
  "operation": "string enum audit_operation_type (requerido)",
  "module": "string (requerido)",
  "action": "string (requerido)",
  "detail": "string | null (opcional)",
  "ip_address": "string IPv4/IPv6 (requerido)",
  "entity_type": "string | null (opcional)",
  "entity_id": "UUID | null (opcional)",
  "changes_json": "string JSON | null (opcional)",
  "occurred_at": "ISO 8601 datetime (requerido)"
}
```

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| `tenant_id` | UUID | Si | Evento payload | UUID v4 valido, no nulo | RN-AUD-04 |
| `user_id` | UUID | Si | Evento payload | UUID v4 valido | — |
| `user_name` | string | Si | Evento payload | No vacio, max 200 chars | RN-AUD-09 |
| `operation` | string | Si | Evento payload | Valor del enum `audit_operation_type` | RN-AUD-10 |
| `module` | string | Si | Evento payload | No vacio, max 100 chars | — |
| `action` | string | Si | Evento payload | No vacio, max 500 chars | — |
| `detail` | string | No | Evento payload | Max 4000 chars | — |
| `ip_address` | string | Si | Evento payload | IP v4 o v6 valida | — |
| `entity_type` | string | No | Evento payload | Max 100 chars | — |
| `entity_id` | UUID | No | Evento payload | UUID v4 valido si presente | — |
| `changes_json` | string (JSON) | No | Evento payload | JSON valido si presente, max 8000 chars. Contiene snapshot de cambios (before/after) para operaciones de edicion | — |
| `occurred_at` | ISO 8601 datetime | Si | Evento payload | Fecha valida, no futura (tolerancia 5 min) | — |

### 5.4 Pasos del Proceso (Happy Path)

| # | Responsable | Accion |
|---|-------------|--------|
| 1 | Microservicio origen | Publica `DomainEvent` en RabbitMQ (exchange fanout o topic) |
| 2 | RabbitMQ | Enruta evento a queue de Audit Service |
| 3 | MassTransit consumer | Deserializa evento |
| 4 | Audit Service | Valida campos requeridos y tipos |
| 5 | Audit Service | Mapea evento a entidad `audit_log` (genera `id` como UUID v4) |
| 6 | Audit Service | INSERT en `audit_log` (particion automatica por `occurred_at`) |
| 7 | Audit Service | Async best-effort: indexa en ES `audit-{yyyy.MM.dd}` |
| 8 | MassTransit | Ack del mensaje |

### 5.5 Salidas

| Campo | Tipo | Destino | Efecto observable |
|-------|------|---------|-------------------|
| `audit_log` row | DB record | SA.audit PostgreSQL | Registro inmutable insertado en particion correspondiente |
| ES document | JSON | Elasticsearch `audit-{yyyy.MM.dd}` | Documento indexado (best-effort) |
| Ack | protocol | RabbitMQ | Mensaje consumido y confirmado |

No hay respuesta HTTP — esta es una operacion asincrona.

### 5.6 Errores Tipados

| Codigo | HTTP | Causa | Condicion de disparo | Respuesta esperada |
|--------|------|-------|----------------------|-------------------|
| AUD-03-E01 | — | tenant_id nulo o invalido | Evento sin tenant_id o UUID malformado | Log error + Nack + retry (RN-AUD-07) |
| AUD-03-E02 | — | Campos requeridos faltantes | user_id, user_name, operation, module, action, ip_address u occurred_at nulos | Log error + Nack + retry |
| AUD-03-E03 | — | operation invalida | Valor no pertenece a `audit_operation_type` | Log error + Nack + retry |
| AUD-03-E04 | — | Error de conexion a PG | SA.audit no disponible | MassTransit retry: 3 intentos (5s/15s/45s) |
| AUD-03-E05 | — | Agotados reintentos | 3 fallos consecutivos | Mensaje a DLQ + alerta operativa |
| AUD-03-E06 | — | Error al indexar en ES | Elasticsearch no disponible | Log warning. PG ya tiene el registro (fuente de verdad). No retry para ES |
| AUD-03-E07 | — | Evento duplicado | Mismo evento procesado dos veces (redelivery) | INSERT con id generado — no produce conflicto (cada delivery genera nuevo UUID) |
| AUD-03-E08 | — | Fallo de deserializacion del mensaje | MassTransit no puede deserializar el payload (JSON malformado, tipo incompatible, encoding invalido) | Log error + DLQ directo (sin reintentos) |

> **Clasificacion de errores para retry:**
> - **No retriable → DLQ directo (sin consumir reintentos):** AUD-03-E01 (tenant_id invalido), AUD-03-E02 (campos requeridos ausentes), AUD-03-E03 (operation invalido), AUD-03-E08 (fallo de deserializacion). Son errores de validacion o de formato que no se resolvaran con reintento.
> - **Retriable → retry con backoff exponencial (max 3):** AUD-03-E04 (SA.audit no disponible), error de particion inexistente. Son errores de infraestructura transitorios.

> **Criterios completos de enrutamiento a DLQ:**
> 1. `tenant_id` es null o UUID malformado (AUD-03-E01)
> 2. Fallo de deserializacion del mensaje — JSON malformado o tipo incompatible (AUD-03-E08)
> 3. Campos requeridos ausentes: `user_id`, `operation`, `module` (AUD-03-E02)
> 4. Valor de `operation` no pertenece al enum `audit_operation_type` (AUD-03-E03)
> 5. Agotamiento de 3 reintentos por error de infraestructura transitorio (AUD-03-E05)

### 5.7 Casos Especiales y Variantes

| Variante | Comportamiento |
|----------|---------------|
| ES no disponible | INSERT en PG procede OK (PG es fuente de verdad — RN-AUD-06). El fallo de indexacion en ES se registra como log warning pero **no bloquea** la creacion del registro de auditoria en PostgreSQL. No se envian a DLQ por fallo ES. El documento ES se reconciliara en una futura reindexacion si es necesario |
| Redelivery por crash del consumer | MassTransit redelivers desde RabbitMQ. Se genera nuevo UUID para `id`, lo que puede producir un registro duplicado. Aceptable: auditoria es append-only y un duplicado no es peor que una perdida |
| Evento con entity_type + entity_id | Se almacenan para trazabilidad de entidad (ej: buscar historial de producto X) |
| Evento sin entity_type/entity_id | Campos null — valido para operaciones globales (Login, Logout) |
| Particion mensual no existe | PG con particionamiento automatico (pg_partman) o creacion manual por DBA. Si la particion no existe, INSERT falla y va a retry/DLQ |
| Multiples consumers (scale-out) | MassTransit prefetch con competing consumers. Cada mensaje procesado por exactamente un consumer |
| Evento con occurred_at futuro (>5min) | Rechazado en validacion. Log error + Nack + retry (posible clock skew del microservicio origen) |
| Denormalizacion de user_name | `user_name` se denormaliza al momento de la ingesta — viene en el DomainEvent. No se hace lookup sync a SA.identity. Si `user_name` es null en el evento, se almacena null |
| Particion mensual inexistente | Si la particion mensual no existe al momento del INSERT, el insert falla y el mensaje va a retry. Mitigacion: usar pg_partman para creacion automatica de particiones, o job preventivo que crea particion N+1 al inicio de cada mes |

### 5.8 Impacto en Modelo de Datos

| Entidad | Operacion | Campos |
|---------|-----------|--------|
| `audit_log` | INSERT | id, tenant_id, occurred_at, user_id, user_name, operation, module, action, detail, ip_address, entity_type, entity_id, changes_json |

**Particionamiento:** el INSERT se dirige automaticamente a la particion mensual correspondiente segun `occurred_at`.

### 5.9 Criterios de Aceptacion Expandidos (Gherkin)

```gherkin
Feature: RF-AUD-03 Ingesta asincrona de eventos de dominio

  Scenario: Ingesta exitosa de evento completo
    Given Audit Service consumer activo
    When microservicio publica DomainEvent con todos los campos requeridos
    Then se inserta un registro en audit_log
    And el registro tiene todos los campos del evento
    And el registro esta en la particion mensual de occurred_at
    And se indexa documento en ES audit-{yyyy.MM.dd}
    And mensaje es ack en RabbitMQ

  Scenario: Ingesta con campos opcionales nulos
    When microservicio publica DomainEvent sin entity_type ni entity_id ni detail
    Then se inserta registro con entity_type=null, entity_id=null, detail=null
    And operacion exitosa

  Scenario: Ingesta con ES no disponible
    Given Elasticsearch no accesible
    When microservicio publica DomainEvent valido
    Then se inserta registro en PG exitosamente
    And se genera log warning sobre fallo ES
    And mensaje es ack (no va a DLQ)

  Scenario: Retry por fallo de PG
    Given SA.audit no disponible temporalmente
    When microservicio publica DomainEvent valido
    Then MassTransit intenta 3 veces con backoff 5s/15s/45s
    And si PG se recupera dentro de los reintentos, inserta OK

  Scenario: DLQ por fallo persistente
    Given SA.audit no disponible de forma prolongada
    When microservicio publica DomainEvent valido
    And 3 reintentos fallan
    Then mensaje va a Dead Letter Queue
    And se genera alerta operativa

  Scenario: Evento con tenant_id nulo
    When microservicio publica evento sin tenant_id
    Then consumer rechaza evento
    And mensaje va a DLQ directo (sin reintentos — error de validacion)

  Scenario: Fallo de deserializacion del mensaje
    When RabbitMQ entrega mensaje con JSON malformado o tipo incompatible
    Then MassTransit no puede deserializar el payload
    And mensaje va a DLQ directo (sin reintentos)
    And se registra log error con detalle del fallo de deserializacion

  Scenario: Evento con operation invalida
    When microservicio publica evento con operation = "OPERACION_FALSA"
    Then consumer rechaza evento
    And tras reintentos va a DLQ

  Scenario: Evento con occurred_at futuro
    When microservicio publica evento con occurred_at = now + 10 minutos
    Then consumer rechaza evento por clock skew

  Scenario: Redelivery genera registro separado
    Given evento fue procesado exitosamente
    When RabbitMQ redelivers el mismo evento (ej: ack perdido)
    Then se genera un nuevo registro con id diferente
    And ambos registros existen en audit_log (duplicado aceptable)
```

### 5.10 Trazabilidad de Pruebas

| TP | Escenario | Tipo |
|----|-----------|------|
| TP-AUD-36 | Ingesta exitosa con todos los campos | Positivo |
| TP-AUD-37 | Ingesta con campos opcionales nulos | Positivo |
| TP-AUD-38 | Ingesta con ES no disponible — PG OK | Positivo |
| TP-AUD-39 | Redelivery genera registro separado (duplicado aceptable) | Positivo |
| TP-AUD-40 | Evento con tenant_id nulo rechazado | Negativo |
| TP-AUD-41 | Evento con campos requeridos faltantes rechazado | Negativo |
| TP-AUD-42 | Evento con operation invalida rechazado | Negativo |
| TP-AUD-43 | Evento con occurred_at futuro (>5 min) rechazado | Negativo |
| TP-AUD-51 | Fallo de deserializacion — mensaje va a DLQ directo | Negativo |
| TP-AUD-44 | Retry 3 veces con backoff exponencial ante fallo PG | Integracion |
| TP-AUD-45 | DLQ tras 3 reintentos fallidos | Integracion |
| TP-AUD-46 | Registro insertado en particion mensual correcta | Integracion |
| TP-AUD-47 | Documento indexado en ES audit-{yyyy.MM.dd} | Integracion |
| TP-AUD-48 | Competing consumers procesan mensajes sin duplicados | Integracion |
| TP-AUD-49 | E2E: crear usuario en Identity → evento publicado → registro en audit_log | E2E |
| TP-AUD-50 | E2E: editar producto en Catalog → evento publicado → registro en audit_log con entity_type+entity_id | E2E |

**Total RF-AUD-03:** 4 positivos + 5 negativos + 5 integracion + 2 E2E = **16 tests**

### 5.11 No Ambiguities Left

| Categoria | Decision |
|-----------|----------|
| Supuestos prohibidos | No se asume que ES esta siempre disponible (RN-AUD-06). No se asume que eventos llegan exactamente una vez (redelivery posible). No se asume que particiones se crean automaticamente (depende de config DBA/pg_partman) |
| Decisiones cerradas | Retry: 3 intentos con backoff 5s/15s/45s. DLQ como destino final. Cada delivery genera nuevo UUID (no idempotencia por event ID). ES fallo no bloquea ingesta. Tolerancia clock skew: 5 minutos hacia futuro |
| TODO explicitos | 0 |
| Fuera de scope | Reproceso automatico desde DLQ (manual por operaciones). Compactacion de duplicados. Busqueda full-text via ES. Alertas automaticas sobre patrones anomalos. Reproceso desde DLQ: proceso manual operativo (via RabbitMQ Management UI), sin RF en esta version. Especificacion de alertas operativas sobre profundidad de DLQ: diferido a configuracion de monitoreo |
| Dependencias externas | RabbitMQ operativo. SA.audit (PG) con particiones. Elasticsearch para indexacion best-effort. pg_partman o proceso manual DBA para crear particiones futuras |

---

## Changelog

### v1.2.0 (2026-03-15)
- RF-AUD-03: fallback ES explicito — PG es fuente de verdad, fallo ES no bloquea creacion del registro
- RF-AUD-03: error AUD-03-E08 agregado (fallo de deserializacion → DLQ directo)
- RF-AUD-03: criterios completos de enrutamiento a DLQ documentados (5 condiciones)
- RF-AUD-03: schema esperado del mensaje RabbitMQ (DomainEvent) agregado en Entradas
- RF-AUD-03: campo `changes_json` agregado al schema y modelo de datos
- RF-AUD-03: escenario Gherkin y test TP-AUD-51 para fallo de deserializacion
- RF-AUD-03: escenario tenant_id nulo corregido — DLQ directo sin reintentos
- RF-AUD-02: estrategia de query con cursor server-side y streaming para exportacion
- RF-AUD-01: campo de respuesta renombrado `size` → `page_size` para formato estandar de paginacion
- RF-AUD-01: nota de formato estandar de paginacion `{ items[], total, page, page_size }`

### v1.1.0 (2026-03-15)
- RF-AUD-03: actores secundarios agregados (Microservicios origen, RabbitMQ)
- RF-AUD-03: clasificacion de errores retriable vs no-retriable para politica de retry/DLQ
- RF-AUD-03: nota de denormalizacion de user_name en ingesta
- RF-AUD-03: nota de mitigacion para particion mensual inexistente
- RF-AUD-03: alcance explicito de reproceso DLQ y alertas operativas
- RF-AUD-02: nota de feedback loop de auto-auditoria en exportacion
- RF-AUD-02: timeout de COUNT para verificacion de limite (5s configurable) + error AUD-02-E07 (504)
- RF-AUD-02: SLA sugerido para exportacion sincrona
- RF-AUD-01: nota de timeout en COUNT para paginacion (3s configurable) + error AUD-01-E07 (504)
- RF-AUD-01: nota sobre indice (entity_type, entity_id) reservado para futuro endpoint
- RF-AUD-01: recomendacion de indice covering (tenant_id, operation, occurred_at DESC)

### v1.0.0 (2026-03-15)
- RF-AUD-01 a RF-AUD-03 documentados (FL-AUD-01: Consultar Log de Auditoria)
- 10 reglas de negocio (RN-AUD-01 a RN-AUD-10)
- 51 tests totales: 16 positivos + 17 negativos + 12 integracion + 6 E2E
