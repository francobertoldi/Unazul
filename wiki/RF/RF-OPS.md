# RF-OPS — Requerimientos Funcionales: Operaciones

> **Proyecto:** Unazul Backoffice
> **Modulo:** Operations Service (SA.operations)
> **Version:** 3.0.0
> **Fecha:** 2026-03-15
> **Prerequisitos:** `01_alcance_funcional.md`, `02_arquitectura.md`, `03_FL.md`, `05_modelo_datos.md`
> **Flujos origen:** FL-OPS-01 (Ciclo de Vida de Solicitud), FL-OPS-02 (Liquidar Comisiones), FL-OPS-03 (Enviar Mensaje desde Solicitud)
> **HUs origen:** HU009, HU010, HU011, HU012, HU013, HU033, HU035, HU036, HU037

---

## Resumen de Requerimientos

| ID | Titulo | Prioridad | Severidad | HU | Estado |
|----|--------|-----------|-----------|-----|--------|
| RF-OPS-01 | Listar solicitudes con filtros, busqueda y exportacion | Alta | P1 | HU009 | Documentado |
| RF-OPS-02 | Buscar solicitante por documento (reutilizacion 1:N) | Alta | P0 | HU010 | Documentado |
| RF-OPS-03 | Crear solicitud con selects en cascada y solicitante reutilizable | Alta | P0 | HU010 | Documentado |
| RF-OPS-04 | Editar solicitud en estado draft | Alta | P0 | HU010 | Documentado |
| RF-OPS-05 | Transicion de estado con state machine y trazabilidad | Alta | P0 | HU011 | Documentado |
| RF-OPS-06 | Ver detalle de solicitud con 8 solapas | Alta | P1 | HU009, HU033 | Documentado |
| RF-OPS-07 | Trazabilidad BPM (timeline visual) | Alta | P1 | HU012 | Documentado |
| RF-OPS-08 | CRUD contactos y direcciones del solicitante | Alta | P0 | HU013 | Documentado |
| RF-OPS-09 | CRUD beneficiarios de solicitud | Alta | P1 | HU013 | Documentado |
| RF-OPS-10 | Gestionar documentos de solicitud | Alta | P1 | HU013 | Documentado |
| RF-OPS-11 | Agregar observacion a solicitud | Media | P2 | HU013 | Documentado |
| RF-OPS-12 | Preliquidacion con filtros y calculo estimado | Alta | P0 | HU035 | Documentado |
| RF-OPS-13 | Confirmacion de liquidacion con lock optimista | Alta | P0 | HU035 | Documentado |
| RF-OPS-14 | Generacion de reporte Excel post-confirmacion | Alta | P1 | HU035 | Documentado |
| RF-OPS-15 | Enviar mensaje desde solicitud con resolucion de variables de plantilla | Alta | P1 | HU037 | Documentado |
| RF-OPS-16 | Listar historial de liquidaciones con filtros | Alta | P1 | HU036 | Documentado |
| RF-OPS-17 | Ver detalle de liquidacion con items | Alta | P1 | HU036 | Documentado |
| RF-OPS-18 | Descargar reporte Excel de liquidacion | Media | P2 | HU036 | Documentado |

---

## Reglas de Negocio

| ID | Regla | Detalle | Decision |
|----|-------|---------|----------|
| RN-OPS-01 | State machine fija | Las transiciones de estado son: draft→pending, pending→in_review, in_review→approved, in_review→rejected, approved→settled (via FL-OPS-02), draft/pending/in_review→cancelled. Toda transicion fuera de esta tabla se rechaza con 422. | D-OPS-01 |
| RN-OPS-02 | Hibrido status + workflow_stage | `status` es la maquina de estados fija (obligatorio). `workflow_stage` es informativo (nullable), gestionado por el motor de workflow. Ambos son independientes. | D-OPS-01 |
| RN-OPS-03 | Solicitante reutilizable 1:N | Un solicitante se identifica por `(tenant_id, document_type, document_number)` UNIQUE. Si ya existe, se reutiliza y se actualizan sus datos (upsert). Contactos y direcciones pertenecen al solicitante, no a la solicitud. | — |
| RN-OPS-04 | Codigo de solicitud autogenerado | El formato es `SOL-YYYY-NNN` donde YYYY es el ano y NNN es secuencia atomica por tenant. Generado por el backend, nunca por el frontend. La unicidad se garantiza con indice UNIQUE en `applications.code`. | — |
| RN-OPS-05 | Denormalizacion de nombres | Al crear/editar una solicitud, se copian `product_name` y `plan_name` desde Catalog Service. Al crear trace_events, se copia `user_name` desde el JWT. No se hacen lookups sync para lectura. | — |
| RN-OPS-06 | Advertencia datos compartidos | Cuando el solicitante tiene >1 solicitud activa, la UI muestra advertencia: "Estos datos pertenecen al solicitante y afectaran a todas sus solicitudes (N solicitudes activas)." No es bloqueo, solo informativo. | — |
| RN-OPS-07 | Edicion solo en draft | Una solicitud solo puede editarse (datos de producto, plan, solicitante) cuando `status = draft`. En cualquier otro estado, solo se permiten transiciones, documentos, observaciones y beneficiarios. | — |
| RN-OPS-08 | Validacion de producto via Catalog | Al crear/editar solicitud, Operations Service valida contra Catalog Service que el producto y plan existan y esten activos. Si Catalog no responde, se retorna 503. | — |
| RN-OPS-09 | Trace event en toda transicion | Cada transicion de estado genera un `trace_event` con `state`, `action`, `user_id`, `user_name`, `occurred_at`. Opcionalmente incluye `trace_event_details` (reason, assigned_to, previous_state). | — |
| RN-OPS-10 | Beneficiarios suman 100% | La suma de `percentage` de todos los beneficiarios de una solicitud debe ser exactamente 100. Se valida al guardar, no en tiempo real. | — |
| RN-OPS-11 | Documentos pertenecen a la solicitud | A diferencia de contactos/direcciones (que pertenecen al solicitante), los documentos y beneficiarios pertenecen a la solicitud especifica (`application_id`). | — |
| RN-OPS-12 | Observaciones inmutables | Las observaciones son append-only (solo INSERT). No se pueden editar ni eliminar una vez creadas. | — |
| RN-OPS-13 | Contactos y direcciones pertenecen al solicitante | `applicant_contacts` y `applicant_addresses` tienen FK a `applicant_id`, no a `application_id`. Se comparten entre todas las solicitudes del mismo solicitante. | — |
| RN-OPS-14 | Concurrencia en transicion de estado | Se usa optimistic locking: `UPDATE applications SET status = :new WHERE id = :id AND status = :old RETURNING id`. Si no retorna fila, otro proceso cambio el estado y se retorna 409. | D-OPS-02 |
| RN-OPS-15 | Operations resuelve variables antes de publicar | El MessageSentEvent contiene el cuerpo ya resuelto (sin `{{...}}`). Notification Service solo despacha al proveedor externo, no resuelve variables. | D-OPS-03 |
| RN-OPS-16 | Observacion automatica al enviar mensaje | Cada envio de mensaje genera una observacion con `observation_type='message'` y content descriptivo del envio (canal, destinatario, plantilla). Aplica RN-OPS-12 (inmutable). | — |
| RN-OPS-17 | Envio siempre asincrono (202 Accepted) | `POST /messages` retorna 202 Accepted inmediatamente. El resultado del despacho (sent/failed) queda en `notification_log` de SA.notification. Operations no consulta ni espera el resultado. | — |
| RN-OPS-18 | body_override anula resolucion de variables | Si el operador provee `body_override`, se usa como cuerpo final tal cual. Operations no resuelve variables de plantilla en ese caso. | — |
| RN-OPS-19 | Lock optimista en confirmacion de liquidacion | Al confirmar, `SELECT ... FOR UPDATE WHERE id IN (...) AND status = 'approved'`. Si alguna solicitud ya no esta en approved, se hace ROLLBACK y se retorna 409 con lista de conflictos. El operador debe volver a preliquidar. | D-OPS-02 |
| RN-OPS-20 | Settlement inmutable post-creacion | `settlements`, `settlement_totals` y `settlement_items` son solo INSERT. No se actualizan ni eliminan una vez creados. Unica excepcion: `settlements.excel_url` se actualiza una vez al generar el Excel. | — |
| RN-OPS-21 | Excel generacion best-effort | Si falla la escritura del archivo Excel (filesystem no disponible, error I/O), el settlement se crea igual con `excel_url = NULL`. La UI muestra "Reporte no disponible". No se hace rollback del settlement. | — |
| RN-OPS-22 | Transicion approved→settled solo via liquidacion masiva | No existe transicion manual de approved a settled. Solo el proceso de confirmacion de liquidacion (RF-OPS-13) puede cambiar solicitudes de approved a settled, en batch dentro de la transaccion. | — |
| RN-OPS-23 | Calculo de comision segun formula del commission_plan | Tres tipos: `fixed_per_sale` (monto fijo por operacion), `percentage_capital` (porcentaje sobre capital), `percentage_total_loan` (porcentaje sobre monto total del prestamo). La formula se almacena como `formula_description` legible en `settlement_items`. | — |
| RN-OPS-24 | Totales agrupados por moneda en settlement_totals | Al crear un settlement, se calculan totales sumando `calculated_amount` agrupados por `currency`. Se persisten en `settlement_totals` (una fila por moneda). | — |
| RN-OPS-25 | Excel inmutable y ruta deterministica | El archivo Excel no se sobreescribe ni elimina. Ruta: `{STORAGE_ROOT}/{tenant_id}/settlements/{yyyy}/{MM}/{settlement_id}.xlsx`. Si el archivo no existe, la descarga retorna 404. | — |

---

## Requerimientos Funcionales

---

### RF-OPS-01 — Listar solicitudes con filtros, busqueda y exportacion

#### Execution Sheet

| Campo | Valor |
|-------|-------|
| **ID** | RF-OPS-01 |
| **Titulo** | Listar solicitudes con filtros, busqueda y exportacion Excel |
| **Actor(es)** | Operador, Admin Entidad, Super Admin, Consulta, Auditor |
| **Prioridad** | Alta |
| **Severidad** | P1 |
| **Flujo origen** | FL-OPS-01 seccion 6 |
| **HU origen** | HU009 |

#### Precondiciones

| # | Condicion | Detalle |
|---|-----------|---------|
| 1 | Operations Service operativo | SA.operations accesible |
| 2 | Usuario autenticado | JWT con permiso `p_ops_app_list` |

#### Entradas

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| `tenant_id` | uuid | Si | JWT claim | UUID valido | — |
| `status` | enum | No | Query param | Valor valido de `application_status` | — |
| `entity_id` | uuid | No | Query param | UUID valido | — |
| `search` | string | No | Query param | Min 2 chars. Busca en code, applicant first_name, last_name, document_number | — |
| `page` | integer | No | Query param | Default: 1, min: 1 | — |
| `page_size` | integer | No | Query param | Default: 20, max: 100 | — |
| `sort_by` | string | No | Query param | Campos: created_at, code, status. Default: created_at | — |
| `sort_dir` | enum | No | Query param | asc, desc. Default: desc | — |
| `export` | boolean | No | Query param | Si true, retorna Excel en lugar de JSON | — |

#### Proceso (Happy Path)

| # | Paso | Responsable |
|---|------|-------------|
| 1 | Recibir `GET /applications` con query params | API Gateway |
| 2 | Verificar permiso `p_ops_app_list` en JWT | API Gateway |
| 3 | Ejecutar `SET LOCAL app.current_tenant = '{tenant_id}'` | Operations Service |
| 4 | Construir query con filtros: status, entity_id, search (ILIKE en code, first_name, last_name, document_number via JOIN applicants) | Operations Service |
| 5 | Si `export=true`: ejecutar query completa (sin paginacion), generar Excel, guardar en `{STORAGE_ROOT}/{tenant_id}/exports/applications_{timestamp}.xlsx`, retornar URL | Operations Service |
| 6 | Si `export=false`: aplicar paginacion, ejecutar COUNT + SELECT | Operations Service |
| 7 | Retornar `200 OK` con `{ items: [{ id, code, status, entity_id, product_name, plan_name, applicant_name, created_at }], total, page, page_size }` | Operations Service |

#### Salidas

| Campo | Tipo | Destino | Efecto observable |
|-------|------|---------|-------------------|
| `items[]` | array | Response body → SPA | Lista paginada de solicitudes |
| `items[].code` | string | Response body | Codigo SOL-YYYY-NNN |
| `items[].status` | enum | Response body | Estado actual con badge de color |
| `items[].applicant_name` | string | Response body | first_name + last_name del solicitante (JOIN) |
| `total` | integer | Response body | Total de registros para paginacion |
| `file_url` | string? | Response body (si export) | Ruta relativa al Excel generado |

#### Errores Tipados

| Codigo | Causa | Condicion de disparo | Respuesta esperada |
|--------|-------|---------------------|--------------------|
| `OPS_UNAUTHORIZED` | Sin permiso | JWT no contiene `p_ops_app_list` | HTTP 403 |
| `OPS_UNAUTHENTICATED` | Token invalido | JWT ausente o expirado | HTTP 401 |
| `OPS_VALIDATION_ERROR` | Filtro invalido | status no valido, page < 1 | HTTP 422 |

#### Casos Especiales y Variantes

- **Busqueda por texto:** ILIKE en `applications.code`, `applicants.first_name`, `applicants.last_name`, `applicants.document_number`. Requiere JOIN con `applicants`.
- **Exportacion Excel:** Retorna todos los registros (sin paginacion) con columnas: Codigo, Estado, Entidad, Producto, Plan, Solicitante, Documento, Fecha Creacion. El archivo se guarda en file system.
- **Sin resultados:** Retorna `200` con `items: []` y `total: 0`.
- **Filtro por entity_id:** Para Admin Entidad, el backend filtra automaticamente por la `entity_id` del JWT si el usuario no tiene scope global.

#### Impacto en Modelo de Datos

| Entidad | Operacion | Campo(s) afectado(s) | Evento |
|---------|-----------|---------------------|--------|
| `applications` | SELECT | `id`, `code`, `status`, `entity_id`, `product_name`, `plan_name`, `created_at` | — |
| `applicants` | SELECT (JOIN) | `first_name`, `last_name`, `document_number` | — |

#### Criterios de Aceptacion (Gherkin)

```gherkin
Scenario: Listar solicitudes paginadas
  Given existen 50 solicitudes para el tenant actual
  And el usuario tiene permiso p_ops_app_list
  When envio GET /applications?page=1&page_size=20
  Then recibo HTTP 200 con items[] de 20 elementos y total=50

Scenario: Filtrar por estado
  Given existen 10 solicitudes en status "draft" y 5 en "pending"
  When envio GET /applications?status=draft
  Then recibo HTTP 200 con items[] de 10 elementos todos con status="draft"

Scenario: Buscar por nombre de solicitante
  Given existe una solicitud cuyo solicitante se llama "Juan Perez"
  When envio GET /applications?search=perez
  Then recibo HTTP 200 con items[] conteniendo la solicitud de Juan Perez

Scenario: Exportar a Excel
  Given existen solicitudes para el tenant
  When envio GET /applications?export=true
  Then recibo HTTP 200 con file_url apuntando a un archivo .xlsx generado
```

#### Trazabilidad de Pruebas

| Test ID | Tipo | Descripcion |
|---------|------|-------------|
| TP-OPS-01-01 | Positivo | Listar solicitudes paginadas retorna items + total |
| TP-OPS-01-02 | Positivo | Filtro por status retorna solo solicitudes del estado |
| TP-OPS-01-03 | Positivo | Filtro por entity_id retorna solo solicitudes de la entidad |
| TP-OPS-01-04 | Positivo | Busqueda ILIKE por code, nombre, documento |
| TP-OPS-01-05 | Positivo | Exportacion Excel genera archivo y retorna URL |
| TP-OPS-01-06 | Negativo | 403 sin permiso p_ops_app_list |
| TP-OPS-01-07 | Negativo | 422 por status invalido |
| TP-OPS-01-08 | Integracion | RLS filtra por tenant correctamente |
| TP-OPS-01-09 | Integracion | JOIN con applicants funciona para search y display |
| TP-OPS-01-10 | E2E | Navegar a /solicitudes muestra tabla paginada con filtros |

#### Sin Ambiguedades

- **Supuestos prohibidos:** No se asume que el frontend tiene acceso directo a applicants para construir el nombre. El backend retorna `applicant_name` calculado.
- **Decisiones cerradas:** La busqueda es ILIKE (case-insensitive). La exportacion no aplica paginacion.
- **Fuera de alcance explicito:** Filtro por rango de fechas. Ordenamiento por multiples campos. Exportacion en CSV.
- **TODO explicitos = 0**

---

### RF-OPS-02 — Buscar solicitante por documento (reutilizacion 1:N)

#### Execution Sheet

| Campo | Valor |
|-------|-------|
| **ID** | RF-OPS-02 |
| **Titulo** | Buscar solicitante existente por tipo y numero de documento |
| **Actor(es)** | Operador, Admin Entidad, Super Admin |
| **Prioridad** | Alta |
| **Severidad** | P0 |
| **Flujo origen** | FL-OPS-01 seccion 6 (buscar solicitante) |
| **HU origen** | HU010 |

#### Precondiciones

| # | Condicion | Detalle |
|---|-----------|---------|
| 1 | Operations Service operativo | SA.operations accesible |
| 2 | Usuario autenticado | JWT con permiso `p_ops_app_create` |

#### Entradas

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| `doc_type` | enum | Si | Query param | Uno de: DNI, CUIT, passport | — |
| `doc_number` | string | Si | Query param | No vacio, max 20 chars | — |
| `tenant_id` | uuid | Si | JWT claim | UUID valido | — |

#### Proceso (Happy Path)

| # | Paso | Responsable |
|---|------|-------------|
| 1 | Recibir `GET /applicants?doc_type=DNI&doc_number=30555888` | API Gateway |
| 2 | Verificar permiso `p_ops_app_create` en JWT | API Gateway |
| 3 | Ejecutar `SET LOCAL app.current_tenant = '{tenant_id}'` | Operations Service |
| 4 | SELECT de `applicants` WHERE `document_type = :doc_type AND document_number = :doc_number` (indice UNIQUE por tenant) | Operations Service |
| 5 | Si encontrado: cargar `applicant_contacts[]` y `applicant_addresses[]` | Operations Service |
| 6 | Contar solicitudes del solicitante: `SELECT COUNT(*) FROM applications WHERE applicant_id = :id` | Operations Service |
| 7 | Retornar `200 OK` con `{ applicant: { id, first_name, last_name, ..., application_count }, contacts[], addresses[] }` | Operations Service |

#### Proceso (No encontrado)

| # | Paso | Responsable |
|---|------|-------------|
| 4a | Si no encontrado: retornar `404` | Operations Service |

#### Salidas

| Campo | Tipo | Destino | Efecto observable |
|-------|------|---------|-------------------|
| `applicant` | object | Response body → SPA | Datos del solicitante existente |
| `applicant.application_count` | integer | Response body → SPA | Numero de solicitudes existentes (para advertencia UI) |
| `contacts[]` | array | Response body → SPA | Contactos del solicitante para precarga |
| `addresses[]` | array | Response body → SPA | Direcciones del solicitante para precarga |

#### Errores Tipados

| Codigo | Causa | Condicion de disparo | Respuesta esperada |
|--------|-------|---------------------|--------------------|
| `OPS_APPLICANT_NOT_FOUND` | Solicitante no existe | No hay registro con doc_type+doc_number en el tenant | HTTP 404, mensaje: "Solicitante no encontrado" |
| `OPS_VALIDATION_ERROR` | Entrada invalida | doc_type invalido o doc_number vacio | HTTP 422 |
| `OPS_UNAUTHORIZED` | Sin permiso | JWT no contiene `p_ops_app_create` | HTTP 403 |

#### Casos Especiales y Variantes

- **Solicitante con multiples solicitudes:** Si `application_count > 1`, el SPA muestra la advertencia de datos compartidos (RN-OPS-06). Esto es responsabilidad del frontend, no del backend.
- **404 como flujo normal:** Retornar 404 es un resultado esperado y valido (solicitante nuevo). El SPA muestra formulario vacio.

#### Impacto en Modelo de Datos

| Entidad | Operacion | Campo(s) afectado(s) | Evento |
|---------|-----------|---------------------|--------|
| `applicants` | SELECT | todos los campos | — |
| `applicant_contacts` | SELECT | todos los campos | — |
| `applicant_addresses` | SELECT | todos los campos | — |
| `applications` | SELECT (COUNT) | `applicant_id` | — |

#### Criterios de Aceptacion (Gherkin)

```gherkin
Scenario: Buscar solicitante existente retorna datos con contactos y direcciones
  Given existe un solicitante con doc_type="DNI" y doc_number="30555888" con 2 contactos y 1 direccion
  And tiene 3 solicitudes
  When envio GET /applicants?doc_type=DNI&doc_number=30555888
  Then recibo HTTP 200 con applicant, contacts[2], addresses[1] y application_count=3

Scenario: Solicitante no encontrado retorna 404
  Given no existe solicitante con doc_type="DNI" y doc_number="99999999"
  When envio GET /applicants?doc_type=DNI&doc_number=99999999
  Then recibo HTTP 404 con codigo OPS_APPLICANT_NOT_FOUND

Scenario: Validacion de doc_type invalido
  When envio GET /applicants?doc_type=INVALID&doc_number=123
  Then recibo HTTP 422 con codigo OPS_VALIDATION_ERROR
```

#### Trazabilidad de Pruebas

| Test ID | Tipo | Descripcion |
|---------|------|-------------|
| TP-OPS-02-01 | Positivo | Buscar solicitante existente retorna datos + contactos + direcciones |
| TP-OPS-02-02 | Positivo | application_count refleja numero correcto de solicitudes |
| TP-OPS-02-03 | Negativo | 404 para solicitante no encontrado |
| TP-OPS-02-04 | Negativo | 422 para doc_type invalido |
| TP-OPS-02-05 | Negativo | 403 sin permiso |
| TP-OPS-02-06 | Integracion | RLS filtra por tenant correctamente |
| TP-OPS-02-07 | E2E | Escribir DNI en formulario → buscar → datos precargados en campos |

#### Sin Ambiguedades

- **Supuestos prohibidos:** No se asume que el frontend busca solicitantes por nombre. Solo por tipo+numero de documento. No se asume que 404 es un error del sistema.
- **Decisiones cerradas:** Busqueda por tipo+numero de documento UNIQUE por tenant. application_count se calcula en tiempo real.
- **Fuera de alcance explicito:** Busqueda por nombre de solicitante. Listado de todos los solicitantes.
- **TODO explicitos = 0**

---

### RF-OPS-03 — Crear solicitud con selects en cascada y solicitante reutilizable

#### Execution Sheet

| Campo | Valor |
|-------|-------|
| **ID** | RF-OPS-03 |
| **Titulo** | Crear solicitud con selects en cascada (entidad→producto→plan) y solicitante reutilizable |
| **Actor(es)** | Operador, Admin Entidad, Super Admin |
| **Prioridad** | Alta |
| **Severidad** | P0 |
| **Flujo origen** | FL-OPS-01 seccion 6 |
| **HU origen** | HU010 |

#### Precondiciones

| # | Condicion | Detalle |
|---|-----------|---------|
| 1 | Operations Service operativo | SA.operations accesible |
| 2 | Catalog Service operativo | Para validacion de producto/plan |
| 3 | RabbitMQ operativo | Para publicacion de ApplicationCreatedEvent |
| 4 | Al menos una entidad con productos activos | Datos de Catalog disponibles |
| 5 | Parametros provinces/cities cargados en Config | Para direcciones del solicitante |
| 6 | Usuario autenticado | JWT con permiso `p_ops_app_create` |

#### Entradas

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| `entity_id` | uuid | Si | Body JSON | UUID valido, entidad existente | — |
| `product_id` | uuid | Si | Body JSON | UUID valido, producto activo de la entidad | RN-OPS-08 |
| `plan_id` | uuid | Si | Body JSON | UUID valido, plan perteneciente al producto | RN-OPS-08 |
| `applicant.first_name` | string | Si | Body JSON | No vacio, max 100 chars | — |
| `applicant.last_name` | string | Si | Body JSON | No vacio, max 100 chars | — |
| `applicant.document_type` | enum | Si | Body JSON | DNI, CUIT, passport | — |
| `applicant.document_number` | string | Si | Body JSON | No vacio, max 20 chars | — |
| `applicant.birth_date` | date | No | Body JSON | Fecha valida, no futura | — |
| `applicant.gender` | enum | No | Body JSON | male, female, other, not_specified | — |
| `applicant.occupation` | string | No | Body JSON | Max 100 chars | — |
| `contacts[]` | array | No | Body JSON | Cada uno: { type, email?, phone_code?, phone? } | — |
| `addresses[]` | array | No | Body JSON | Cada uno: { type, street, number, floor?, apartment?, city, province, postal_code, latitude?, longitude? } | — |
| `beneficiaries[]` | array | No | Body JSON | Cada uno: { first_name, last_name, relationship, percentage } | RN-OPS-10 |
| `tenant_id` | uuid | Si | JWT claim | UUID valido | — |

#### Proceso (Happy Path)

| # | Paso | Responsable |
|---|------|-------------|
| 1 | Recibir `POST /applications` con body JSON | API Gateway |
| 2 | Verificar permiso `p_ops_app_create` en JWT | API Gateway |
| 3 | Ejecutar `SET LOCAL app.current_tenant = '{tenant_id}'` | Operations Service |
| 4 | Validar producto y plan contra Catalog Service: `GET /products/:id` y `GET /products/:id/plans/:plan_id`. Verificar existencia y status = active | Operations Service |
| 5 | Obtener `product_name` y `plan_name` de la respuesta de Catalog para denormalizacion | Operations Service |
| 6 | Si `beneficiaries[]` presente, validar que la suma de `percentage` = 100 | Operations Service |
| 7 | BEGIN TRANSACTION | Operations Service |
| 8 | UPSERT en `applicants`: buscar por `(tenant_id, document_type, document_number)`. Si existe, UPDATE datos personales. Si no existe, INSERT nuevo | Operations Service |
| 9 | SYNC `applicant_contacts`: DELETE existentes del solicitante + INSERT nuevos (replace all) | Operations Service |
| 10 | SYNC `applicant_addresses`: DELETE existentes del solicitante + INSERT nuevos (replace all) | Operations Service |
| 11 | Generar `code` = `SOL-{YYYY}-{NNN}` con secuencia atomica por tenant | Operations Service |
| 12 | INSERT en `applications` con `id` (UUIDv7), `tenant_id`, `entity_id`, `applicant_id`, `code`, `product_id`, `plan_id`, `product_name`, `plan_name`, `status = 'draft'`, `created_at`, `updated_at`, `created_by`, `updated_by` | Operations Service |
| 13 | Si `beneficiaries[]` presente: INSERT en `beneficiaries` para cada uno con `application_id`, `tenant_id` | Operations Service |
| 14 | INSERT en `trace_events` con `state = 'draft'`, `action = 'Solicitud creada'`, `user_id`, `user_name` | Operations Service |
| 15 | COMMIT | Operations Service |
| 16 | Publicar `ApplicationCreatedEvent` a RabbitMQ con `{ application_id, code, tenant_id, entity_id, product_id }` | Operations Service |
| 17 | Retornar `201 Created` con `{ id, code, status: 'draft' }` | Operations Service |

#### Salidas

| Campo | Tipo | Destino | Efecto observable |
|-------|------|---------|-------------------|
| `201 Created` | HTTP status | Response → SPA | Solicitud creada exitosamente |
| `id` | uuid | Response body | ID de la solicitud creada |
| `code` | string | Response body | Codigo SOL-YYYY-NNN generado |
| `status` | string | Response body | Siempre `"draft"` al crear |
| `ApplicationCreatedEvent` | evento async | RabbitMQ → Audit | Registro en audit_log |
| `trace_events` | INSERT | SA.operations | Primer evento de trazabilidad |

#### Errores Tipados

| Codigo | Causa | Condicion de disparo | Respuesta esperada |
|--------|-------|---------------------|--------------------|
| `OPS_PRODUCT_NOT_FOUND` | Producto no existe o inactivo | Catalog retorna 404 o producto con status != active | HTTP 422, mensaje: "Producto no encontrado o inactivo" |
| `OPS_PLAN_NOT_FOUND` | Plan no existe | Catalog retorna 404 para el plan | HTTP 422, mensaje: "Plan no encontrado" |
| `OPS_CATALOG_UNAVAILABLE` | Catalog Service caido | Timeout o error de conexion a Catalog | HTTP 503, mensaje: "Servicio de catalogo no disponible" |
| `OPS_BENEFICIARY_PERCENTAGE_INVALID` | Porcentajes no suman 100 | Suma de percentage de beneficiaries[] != 100 | HTTP 422, mensaje: "Los porcentajes de beneficiarios deben sumar 100" |
| `OPS_VALIDATION_ERROR` | Entrada invalida | Campos requeridos vacios, tipos invalidos | HTTP 422, detalle de campos |
| `OPS_UNAUTHORIZED` | Sin permiso | JWT no contiene `p_ops_app_create` | HTTP 403 |

#### Casos Especiales y Variantes

- **Solicitante existente (upsert):** Si ya existe un solicitante con mismo `(tenant_id, doc_type, doc_number)`, se actualizan sus datos personales y se reemplazan contactos/direcciones completos (DELETE + INSERT).
- **Solicitante nuevo:** Si no existe, se crea uno nuevo con todos los datos proporcionados.
- **Sin beneficiarios:** `beneficiaries[]` es opcional. Si no se envia, no se valida porcentaje.
- **Sin contactos/direcciones:** `contacts[]` y `addresses[]` son opcionales. Si no se envian, no se sincronizan.
- **Transaccion atomica:** Todo el proceso (upsert applicant + sync contacts/addresses + insert application + insert beneficiaries + insert trace_event) ocurre en una sola transaccion. Si falla cualquier paso, se hace rollback.

#### Impacto en Modelo de Datos

| Entidad | Operacion | Campo(s) afectado(s) | Evento |
|---------|-----------|---------------------|--------|
| `applicants` | INSERT o UPDATE (upsert) | todos los campos | — |
| `applicant_contacts` | DELETE + INSERT | todos los campos | — |
| `applicant_addresses` | DELETE + INSERT | todos los campos | — |
| `applications` | INSERT | todos los campos | `ApplicationCreatedEvent` |
| `beneficiaries` | INSERT | todos los campos | — |
| `trace_events` | INSERT | `state='draft'`, `action='Solicitud creada'` | — |
| `audit_log` (SA.audit) | INSERT (async) | operation = 'Crear', module = 'solicitudes' | consume `ApplicationCreatedEvent` |

#### Criterios de Aceptacion (Gherkin)

```gherkin
Scenario: Crear solicitud con solicitante nuevo
  Given entidad con productos activos en Catalog
  And no existe solicitante con DNI 30555888
  And el usuario tiene permiso p_ops_app_create
  When envio POST /applications con entity_id, product_id, plan_id, applicant{doc_type:"DNI", doc_number:"30555888", first_name:"Juan", last_name:"Perez"}
  Then recibo HTTP 201 con id, code tipo SOL-2026-001, status="draft"
  And se crea un nuevo registro en applicants
  And se crea trace_event con state="draft" y action="Solicitud creada"
  And se publica ApplicationCreatedEvent

Scenario: Crear solicitud reutilizando solicitante existente
  Given existe un solicitante con DNI 30555888 con 1 solicitud previa
  When envio POST /applications con applicant{doc_type:"DNI", doc_number:"30555888", first_name:"Juan Actualizado", ...}
  Then recibo HTTP 201
  And el solicitante existente se actualiza con first_name="Juan Actualizado"
  And la nueva solicitud se asocia al mismo applicant_id

Scenario: Producto inactivo retorna 422
  Given el producto referenciado tiene status="inactive"
  When envio POST /applications con product_id del producto inactivo
  Then recibo HTTP 422 con codigo OPS_PRODUCT_NOT_FOUND

Scenario: Catalog no disponible retorna 503
  Given Catalog Service no responde
  When envio POST /applications
  Then recibo HTTP 503 con codigo OPS_CATALOG_UNAVAILABLE

Scenario: Beneficiarios no suman 100 retorna 422
  When envio POST /applications con beneficiaries[{percentage:60}, {percentage:30}]
  Then recibo HTTP 422 con codigo OPS_BENEFICIARY_PERCENTAGE_INVALID
```

#### Trazabilidad de Pruebas

| Test ID | Tipo | Descripcion |
|---------|------|-------------|
| TP-OPS-03-01 | Positivo | Crear solicitud con solicitante nuevo retorna 201 con code |
| TP-OPS-03-02 | Positivo | Crear solicitud reutilizando solicitante existente (upsert) |
| TP-OPS-03-03 | Positivo | Crear con beneficiarios que suman 100% |
| TP-OPS-03-04 | Positivo | Crear con contactos y direcciones sincronizados |
| TP-OPS-03-05 | Negativo | 422 por producto inexistente o inactivo |
| TP-OPS-03-06 | Negativo | 422 por plan inexistente |
| TP-OPS-03-07 | Negativo | 503 si Catalog no disponible |
| TP-OPS-03-08 | Negativo | 422 por beneficiarios que no suman 100 |
| TP-OPS-03-09 | Negativo | 422 por campos requeridos vacios |
| TP-OPS-03-10 | Negativo | 403 sin permiso p_ops_app_create |
| TP-OPS-03-11 | Integracion | ApplicationCreatedEvent publicado y consumido por Audit |
| TP-OPS-03-12 | Integracion | trace_event creado con state=draft |
| TP-OPS-03-13 | Integracion | Transaccion atomica: fallo en beneficiarios revierte applicant |
| TP-OPS-03-14 | E2E | Formulario cascada entidad→producto→plan → submit → redireccion a listado |

#### Sin Ambiguedades

- **Supuestos prohibidos:** No se asume que el frontend valida la existencia del producto. No se asume que el codigo lo genera el frontend. No se asume que contactos/direcciones se sincronizan de forma incremental (es replace all).
- **Decisiones cerradas:** Upsert de solicitante por `(tenant_id, doc_type, doc_number)`. Codigo SOL-YYYY-NNN generado por backend. Replace all de contactos y direcciones al crear.
- **Fuera de alcance explicito:** Crear solicitud sin solicitante. Crear solicitud con multiples productos. Draft auto-save periodico.
- **Dependencias externas explicitas:** Catalog Service (sync HTTP para validacion de producto/plan). Config Service (parametros de provincias/ciudades, consumido por frontend, no por backend en este RF).
- **TODO explicitos = 0**

---

### RF-OPS-04 — Editar solicitud en estado draft

#### Execution Sheet

| Campo | Valor |
|-------|-------|
| **ID** | RF-OPS-04 |
| **Titulo** | Editar solicitud existente (solo en estado draft) |
| **Actor(es)** | Operador, Admin Entidad, Super Admin |
| **Prioridad** | Alta |
| **Severidad** | P0 |
| **Flujo origen** | FL-OPS-01 seccion 6 |
| **HU origen** | HU010 |

#### Precondiciones

| # | Condicion | Detalle |
|---|-----------|---------|
| 1 | Operations Service operativo | SA.operations accesible |
| 2 | Catalog Service operativo | Para validacion de producto/plan si cambian |
| 3 | Solicitud existente en estado draft | `applications.status = 'draft'` |
| 4 | Usuario autenticado | JWT con permiso `p_ops_app_edit` |

#### Entradas

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| `id` | uuid | Si | Path param `/applications/:id` | UUID valido, solicitud existente | — |
| `entity_id` | uuid | No | Body JSON | UUID valido si se modifica | — |
| `product_id` | uuid | No | Body JSON | UUID valido, producto activo | RN-OPS-08 |
| `plan_id` | uuid | No | Body JSON | UUID valido, plan del producto | RN-OPS-08 |
| `applicant.*` | object | No | Body JSON | Mismas validaciones que RF-OPS-03 | — |
| `contacts[]` | array | No | Body JSON | Replace all de contactos del solicitante | — |
| `addresses[]` | array | No | Body JSON | Replace all de direcciones del solicitante | — |
| `beneficiaries[]` | array | No | Body JSON | Replace all; si presente, suma = 100 | RN-OPS-10 |
| `tenant_id` | uuid | Si | JWT claim | UUID valido | — |

#### Proceso (Happy Path)

| # | Paso | Responsable |
|---|------|-------------|
| 1 | Recibir `PUT /applications/:id` con body JSON | API Gateway |
| 2 | Verificar permiso `p_ops_app_edit` en JWT | API Gateway |
| 3 | Ejecutar `SET LOCAL app.current_tenant = '{tenant_id}'` | Operations Service |
| 4 | SELECT solicitud por `id`. Verificar `status = 'draft'` | Operations Service |
| 5 | Si `product_id` o `plan_id` cambian: validar contra Catalog Service | Operations Service |
| 6 | Si `beneficiaries[]` presente: validar suma de percentage = 100 | Operations Service |
| 7 | BEGIN TRANSACTION | Operations Service |
| 8 | Si `applicant.*` presente: UPDATE `applicants` con datos modificados | Operations Service |
| 9 | Si `contacts[]` presente: DELETE + INSERT en `applicant_contacts` | Operations Service |
| 10 | Si `addresses[]` presente: DELETE + INSERT en `applicant_addresses` | Operations Service |
| 11 | UPDATE `applications` con campos modificados, `updated_at`, `updated_by`. Si producto/plan cambio, actualizar `product_name`, `plan_name` | Operations Service |
| 12 | Si `beneficiaries[]` presente: DELETE existentes + INSERT nuevos en `beneficiaries` | Operations Service |
| 13 | COMMIT | Operations Service |
| 14 | Retornar `200 OK` con `{ id, code, status }` | Operations Service |

#### Salidas

| Campo | Tipo | Destino | Efecto observable |
|-------|------|---------|-------------------|
| `200 OK` | HTTP status | Response → SPA | Solicitud actualizada |

#### Errores Tipados

| Codigo | Causa | Condicion de disparo | Respuesta esperada |
|--------|-------|---------------------|--------------------|
| `OPS_APPLICATION_NOT_FOUND` | Solicitud inexistente | `id` no existe o no pertenece al tenant | HTTP 404 |
| `OPS_NOT_EDITABLE` | Estado no es draft | `applications.status != 'draft'` | HTTP 422, mensaje: "Solo se pueden editar solicitudes en estado draft" |
| `OPS_PRODUCT_NOT_FOUND` | Producto invalido | Producto no existe o inactivo en Catalog | HTTP 422 |
| `OPS_PLAN_NOT_FOUND` | Plan invalido | Plan no existe en Catalog | HTTP 422 |
| `OPS_CATALOG_UNAVAILABLE` | Catalog Service caido | Si product_id/plan_id cambian y Catalog no responde | HTTP 503 |
| `OPS_BENEFICIARY_PERCENTAGE_INVALID` | Porcentajes no suman 100 | Suma != 100 | HTTP 422 |
| `OPS_UNAUTHORIZED` | Sin permiso | JWT no contiene `p_ops_app_edit` | HTTP 403 |

#### Casos Especiales y Variantes

- **Solo en draft:** Si la solicitud esta en cualquier estado distinto de `draft`, se retorna `OPS_NOT_EDITABLE`. No se permite volver a draft para editar.
- **Edicion parcial:** Solo se actualizan los campos enviados en el body. Si `product_id`/`plan_id` no se envian, se mantienen los actuales y no se valida contra Catalog.
- **Advertencia datos compartidos:** Si el solicitante tiene >1 solicitud, la advertencia es responsabilidad del frontend (RN-OPS-06). El backend edita sin distincion.

#### Impacto en Modelo de Datos

| Entidad | Operacion | Campo(s) afectado(s) | Evento |
|---------|-----------|---------------------|--------|
| `applications` | SELECT + UPDATE | campos modificados, `updated_at`, `updated_by` | — |
| `applicants` | UPDATE (condicional) | datos personales | — |
| `applicant_contacts` | DELETE + INSERT (condicional) | todos | — |
| `applicant_addresses` | DELETE + INSERT (condicional) | todos | — |
| `beneficiaries` | DELETE + INSERT (condicional) | todos | — |

#### Criterios de Aceptacion (Gherkin)

```gherkin
Scenario: Editar solicitud en draft exitosamente
  Given existe una solicitud en status="draft"
  And el usuario tiene permiso p_ops_app_edit
  When envio PUT /applications/:id con product_id nuevo
  Then recibo HTTP 200 con product_name actualizado

Scenario: Editar solicitud no-draft retorna 422
  Given existe una solicitud en status="pending"
  When envio PUT /applications/:id
  Then recibo HTTP 422 con codigo OPS_NOT_EDITABLE

Scenario: Editar solicitud inexistente retorna 404
  When envio PUT /applications/00000000-0000-0000-0000-000000000000
  Then recibo HTTP 404 con codigo OPS_APPLICATION_NOT_FOUND
```

#### Trazabilidad de Pruebas

| Test ID | Tipo | Descripcion |
|---------|------|-------------|
| TP-OPS-04-01 | Positivo | Editar solicitud draft con nuevo producto retorna 200 |
| TP-OPS-04-02 | Positivo | Editar datos del solicitante actualiza applicants |
| TP-OPS-04-03 | Positivo | Editar beneficiarios reemplaza correctamente |
| TP-OPS-04-04 | Negativo | 422 para solicitud en status != draft |
| TP-OPS-04-05 | Negativo | 404 para solicitud inexistente |
| TP-OPS-04-06 | Negativo | 422 por producto inactivo |
| TP-OPS-04-07 | Negativo | 403 sin permiso p_ops_app_edit |
| TP-OPS-04-08 | Integracion | RLS impide editar solicitud de otro tenant |
| TP-OPS-04-09 | E2E | Abrir solicitud draft → editar campos → guardar → datos actualizados |

#### Sin Ambiguedades

- **Supuestos prohibidos:** No se asume que el frontend valida el estado antes de enviar PUT. El backend siempre verifica. No se asume que se puede volver a draft desde otro estado.
- **Decisiones cerradas:** Edicion solo en draft (RN-OPS-07). Contactos y direcciones se reemplazan completamente (replace all).
- **Fuera de alcance explicito:** Edicion en estados posteriores a draft. Versionamiento de borradores.
- **TODO explicitos = 0**

---

### RF-OPS-05 — Transicion de estado con state machine y trazabilidad

#### Execution Sheet

| Campo | Valor |
|-------|-------|
| **ID** | RF-OPS-05 |
| **Titulo** | Ejecutar transicion de estado con validacion de state machine y registro de trazabilidad |
| **Actor(es)** | Operador, Admin Entidad, Super Admin |
| **Prioridad** | Alta |
| **Severidad** | P0 |
| **Flujo origen** | FL-OPS-01 seccion 7 |
| **HU origen** | HU011 |

#### Precondiciones

| # | Condicion | Detalle |
|---|-----------|---------|
| 1 | Operations Service operativo | SA.operations accesible |
| 2 | RabbitMQ operativo | Para publicacion de ApplicationStatusChangedEvent |
| 3 | Solicitud existente | `id` valido en `applications` para el tenant |
| 4 | Usuario autenticado | JWT con permiso `p_ops_app_transition` |

#### Entradas

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| `id` | uuid | Si | Path param `/applications/:id/status` | UUID valido, solicitud existente | — |
| `new_status` | enum | Si | Body JSON | Valor valido de `application_status` | RN-OPS-01 |
| `action` | string | Si | Body JSON | No vacio, max 200 chars. Descripcion de la accion (ej: "Enviar a revision") | — |
| `detail` | string | No | Body JSON | Max 1000 chars. Razon o nota adicional | — |
| `tenant_id` | uuid | Si | JWT claim | UUID valido | — |

#### Proceso (Happy Path)

| # | Paso | Responsable |
|---|------|-------------|
| 1 | Recibir `PUT /applications/:id/status` con body JSON | API Gateway |
| 2 | Verificar permiso `p_ops_app_transition` en JWT | API Gateway |
| 3 | Ejecutar `SET LOCAL app.current_tenant = '{tenant_id}'` | Operations Service |
| 4 | Validar transicion contra state machine: verificar que `(current_status → new_status)` esta en la tabla de transiciones permitidas | Operations Service |
| 4a | Si `new_status = pending` (transicion `draft → pending`): validar que la suma de `percentage` de todos los beneficiarios de la solicitud = 100 (si existen beneficiarios). Si no suma 100 → retornar 422 `OPS_BENEFICIARIES_PERCENTAGE_INVALID` | Operations Service |
| 5 | Ejecutar optimistic update: `UPDATE applications SET status = :new_status, updated_at = NOW(), updated_by = :user_id WHERE id = :id AND status = :current_status RETURNING id` | Operations Service |
| 6 | Si RETURNING vacio: otro proceso cambio el estado → retornar 409 | Operations Service |
| 7 | INSERT en `trace_events` con `application_id`, `state = :new_status`, `action`, `user_id`, `user_name`, `occurred_at = NOW()` | Operations Service |
| 8 | Si `detail` presente: INSERT en `trace_event_details` con `{ key: 'reason', value: detail }` y `{ key: 'previous_state', value: current_status }` | Operations Service |
| 9 | Publicar `ApplicationStatusChangedEvent` a RabbitMQ con `{ application_id, old_status, new_status, user_id, tenant_id }` | Operations Service |
| 10 | Retornar `200 OK` con `{ id, code, status: new_status }` | Operations Service |

#### Salidas

| Campo | Tipo | Destino | Efecto observable |
|-------|------|---------|-------------------|
| `200 OK` | HTTP status | Response → SPA | Transicion ejecutada |
| `status` | enum | Response body | Nuevo estado de la solicitud |
| `ApplicationStatusChangedEvent` | evento async | RabbitMQ → Audit, Notification | Registro auditoria + posible notificacion |
| `trace_events` | INSERT | SA.operations | Evento de trazabilidad registrado |

#### Errores Tipados

| Codigo | Causa | Condicion de disparo | Respuesta esperada |
|--------|-------|---------------------|--------------------|
| `OPS_APPLICATION_NOT_FOUND` | Solicitud inexistente | `id` no existe o no pertenece al tenant | HTTP 404 |
| `OPS_INVALID_TRANSITION` | Transicion no permitida | `(current_status → new_status)` no esta en la state machine | HTTP 422, mensaje: "Transicion de {current} a {new_status} no permitida" |
| `OPS_CONCURRENT_MODIFICATION` | Conflicto de concurrencia | Otro proceso cambio el estado entre SELECT y UPDATE | HTTP 409, mensaje: "La solicitud fue modificada por otro usuario" |
| `OPS_BENEFICIARIES_PERCENTAGE_INVALID` | Porcentajes no suman 100 | `new_status = pending` y la suma de porcentajes de beneficiarios != 100 (cuando hay beneficiarios) | HTTP 422, mensaje: "La suma de porcentajes de beneficiarios debe ser 100" |
| `OPS_VALIDATION_ERROR` | Entrada invalida | new_status vacio o action vacio | HTTP 422 |
| `OPS_UNAUTHORIZED` | Sin permiso | JWT no contiene `p_ops_app_transition` | HTTP 403 |

#### Casos Especiales y Variantes

- **Transiciones permitidas (RN-OPS-01):**
  - `draft → pending` (Enviar)
  - `pending → in_review` (Tomar/Asignar)
  - `in_review → approved` (Aprobar)
  - `in_review → rejected` (Rechazar)
  - `approved → settled` (Liquidar — via FL-OPS-02, no directamente)
  - `draft → cancelled`, `pending → cancelled`, `in_review → cancelled` (Cancelar)
- **Concurrencia (RN-OPS-14):** Se usa optimistic locking con WHERE clause. Si el status cambio entre la lectura y la escritura, el UPDATE no afecta filas y se retorna 409.
- **approved → settled:** Esta transicion NO se ejecuta via este RF. Es responsabilidad de FL-OPS-02 (liquidacion). Si se intenta, se retorna `OPS_INVALID_TRANSITION`.
- **trace_event_details:** Se registra siempre `previous_state`. Si hay `detail`, se registra como `reason`.

#### Impacto en Modelo de Datos

| Entidad | Operacion | Campo(s) afectado(s) | Evento |
|---------|-----------|---------------------|--------|
| `applications` | SELECT | `status` (verificar transicion) | — |
| `applications` | UPDATE | `status`, `updated_at`, `updated_by` | `ApplicationStatusChangedEvent` |
| `trace_events` | INSERT | `application_id`, `state`, `action`, `user_id`, `user_name`, `occurred_at` | — |
| `trace_event_details` | INSERT (condicional) | `trace_event_id`, `key`, `value` | — |
| `audit_log` (SA.audit) | INSERT (async) | operation = 'Cambiar Estado', module = 'solicitudes' | consume `ApplicationStatusChangedEvent` |

#### Criterios de Aceptacion (Gherkin)

```gherkin
Scenario: Transicion draft a pending exitosa
  Given existe una solicitud en status="draft"
  And el usuario tiene permiso p_ops_app_transition
  When envio PUT /applications/:id/status con new_status="pending", action="Enviar a revision"
  Then recibo HTTP 200 con status="pending"
  And se crea trace_event con state="pending" y action="Enviar a revision"
  And se publica ApplicationStatusChangedEvent

Scenario: Transicion con detalle registra reason
  Given existe una solicitud en status="in_review"
  When envio PUT /applications/:id/status con new_status="rejected", action="Rechazar", detail="Documentacion incompleta"
  Then recibo HTTP 200 con status="rejected"
  And trace_event_details contiene key="reason" value="Documentacion incompleta"
  And trace_event_details contiene key="previous_state" value="in_review"

Scenario: Transicion invalida retorna 422
  Given existe una solicitud en status="draft"
  When envio PUT /applications/:id/status con new_status="approved"
  Then recibo HTTP 422 con codigo OPS_INVALID_TRANSITION

Scenario: Conflicto de concurrencia retorna 409
  Given existe una solicitud en status="draft"
  And otro proceso cambia el status a "pending" entre SELECT y UPDATE
  When envio PUT /applications/:id/status con new_status="pending"
  Then recibo HTTP 409 con codigo OPS_CONCURRENT_MODIFICATION

Scenario: Transicion approved a settled rechazada (reservada para FL-OPS-02)
  Given existe una solicitud en status="approved"
  When envio PUT /applications/:id/status con new_status="settled"
  Then recibo HTTP 422 con codigo OPS_INVALID_TRANSITION

Scenario: Transicion draft a pending con beneficiarios que no suman 100
  Given existe una solicitud en status="draft" con 2 beneficiarios (60% + 30%)
  When envio PUT /applications/:id/status con new_status="pending"
  Then recibo HTTP 422 con codigo OPS_BENEFICIARIES_PERCENTAGE_INVALID
```

#### Trazabilidad de Pruebas

| Test ID | Tipo | Descripcion |
|---------|------|-------------|
| TP-OPS-05-01 | Positivo | Transicion draft→pending exitosa |
| TP-OPS-05-02 | Positivo | Transicion pending→in_review exitosa |
| TP-OPS-05-03 | Positivo | Transicion in_review→approved exitosa |
| TP-OPS-05-04 | Positivo | Transicion in_review→rejected con detail/reason |
| TP-OPS-05-05 | Positivo | Transicion draft/pending/in_review→cancelled |
| TP-OPS-05-06 | Negativo | 422 por transicion invalida (draft→approved) |
| TP-OPS-05-07 | Negativo | 422 por approved→settled (reservada FL-OPS-02) |
| TP-OPS-05-14 | Negativo | 422 draft→pending con beneficiarios que no suman 100% (OPS_BENEFICIARIES_PERCENTAGE_INVALID) |
| TP-OPS-05-08 | Negativo | 409 por conflicto de concurrencia |
| TP-OPS-05-09 | Negativo | 404 para solicitud inexistente |
| TP-OPS-05-10 | Negativo | 403 sin permiso p_ops_app_transition |
| TP-OPS-05-11 | Integracion | ApplicationStatusChangedEvent publicado y consumido por Audit |
| TP-OPS-05-12 | Integracion | trace_event + trace_event_details creados correctamente |
| TP-OPS-05-13 | E2E | Click "Enviar a revision" → badge cambia → trace visible en timeline |

#### Sin Ambiguedades

- **Supuestos prohibidos:** No se asume que el frontend conoce las transiciones permitidas (el backend valida). No se asume que approved→settled se ejecuta via este endpoint. No se asume que el lock es pesimista.
- **Decisiones cerradas:** State machine fija (D-OPS-01). Optimistic locking con WHERE (D-OPS-02). approved→settled solo via FL-OPS-02.
- **Fuera de alcance explicito:** Transiciones automaticas por timeout. Aprobacion con multiples firmantes. Rollback de transiciones.
- **TODO explicitos = 0**

---

### RF-OPS-06 — Ver detalle de solicitud con 8 solapas

#### Execution Sheet

| Campo | Valor |
|-------|-------|
| **ID** | RF-OPS-06 |
| **Titulo** | Ver detalle completo de solicitud con 8 solapas de informacion |
| **Actor(es)** | Operador, Admin Entidad, Super Admin, Consulta, Auditor |
| **Prioridad** | Alta |
| **Severidad** | P1 |
| **Flujo origen** | FL-OPS-01 seccion 8a |
| **HU origen** | HU009, HU033 |

#### Precondiciones

| # | Condicion | Detalle |
|---|-----------|---------|
| 1 | Operations Service operativo | SA.operations accesible |
| 2 | Solicitud existente | `id` valido en `applications` para el tenant |
| 3 | Usuario autenticado | JWT con permiso `p_ops_app_detail` |

#### Entradas

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| `id` | uuid | Si | Path param `/applications/:id` | UUID valido, solicitud existente | — |
| `tenant_id` | uuid | Si | JWT claim | UUID valido | — |

#### Proceso (Happy Path)

| # | Paso | Responsable |
|---|------|-------------|
| 1 | Recibir `GET /applications/:id` | API Gateway |
| 2 | Verificar permiso `p_ops_app_detail` en JWT | API Gateway |
| 3 | Ejecutar `SET LOCAL app.current_tenant = '{tenant_id}'` | Operations Service |
| 4 | SELECT de `applications` por `id` (RLS filtra por tenant) | Operations Service |
| 5 | SELECT de `applicants` por `applicant_id` + COUNT de solicitudes del solicitante | Operations Service |
| 6 | SELECT de `applicant_contacts` por `applicant_id` | Operations Service |
| 7 | SELECT de `applicant_addresses` por `applicant_id` | Operations Service |
| 8 | SELECT de `beneficiaries` por `application_id` | Operations Service |
| 9 | SELECT de `application_documents` por `application_id` | Operations Service |
| 10 | SELECT de `application_observations` por `application_id` ordenadas por `created_at DESC` | Operations Service |
| 11 | SELECT de `trace_events` + `trace_event_details` por `application_id` ordenados por `occurred_at ASC` | Operations Service |
| 12 | Retornar `200 OK` con objeto completo conteniendo todas las solapas | Operations Service |

#### Salidas

| Campo | Tipo | Destino | Efecto observable |
|-------|------|---------|-------------------|
| `application` | object | Response body | Datos generales: id, code, status, entity_id, product_name, plan_name, workflow_stage |
| `applicant` | object | Response body | Datos del solicitante + `application_count` |
| `product` | object | Response body | Datos denormalizados del producto (product_name, plan_name, product_id, plan_id) |
| `contacts[]` | array | Response body | Contactos del solicitante |
| `addresses[]` | array | Response body | Direcciones del solicitante |
| `beneficiaries[]` | array | Response body | Beneficiarios de la solicitud |
| `documents[]` | array | Response body | Documentos con status (pending/approved/rejected) |
| `observations[]` | array | Response body | Observaciones ordenadas DESC |
| `trace_events[]` | array | Response body | Datos crudos de trazabilidad con details (sin tipo visual). Para nodos con tipo visual calculado (`completed`, `current`, `pending`), usar endpoint `/applications/:id/timeline` (RF-OPS-07) |

#### Errores Tipados

| Codigo | Causa | Condicion de disparo | Respuesta esperada |
|--------|-------|---------------------|--------------------|
| `OPS_APPLICATION_NOT_FOUND` | Solicitud inexistente | `id` no existe o no pertenece al tenant | HTTP 404 |
| `OPS_UNAUTHORIZED` | Sin permiso | JWT no contiene `p_ops_app_detail` | HTTP 403 |

#### Casos Especiales y Variantes

- **Solapas vacias:** Si una solicitud no tiene beneficiarios, documentos u observaciones, esos arrays se retornan vacios. No es un error.
- **application_count del solicitante:** Se incluye para que el frontend muestre la nota "N solicitudes" en la solapa Solicitante.
- **Producto denormalizado:** Los datos de producto vienen de campos denormalizados en `applications` (product_name, plan_name). No se consulta Catalog Service en lectura.
- **Trazabilidad incluye details:** Cada `trace_event` incluye su array de `trace_event_details[]` anidado.

#### Impacto en Modelo de Datos

| Entidad | Operacion | Campo(s) afectado(s) | Evento |
|---------|-----------|---------------------|--------|
| `applications` | SELECT | todos los campos | — |
| `applicants` | SELECT | todos los campos | — |
| `applicant_contacts` | SELECT | todos | — |
| `applicant_addresses` | SELECT | todos | — |
| `beneficiaries` | SELECT | todos | — |
| `application_documents` | SELECT | todos | — |
| `application_observations` | SELECT | todos | — |
| `trace_events` + `trace_event_details` | SELECT | todos | — |

#### Criterios de Aceptacion (Gherkin)

```gherkin
Scenario: Ver detalle completo de solicitud
  Given existe una solicitud con solicitante, 2 contactos, 1 direccion, 3 beneficiarios, 1 documento y 2 observaciones
  And el usuario tiene permiso p_ops_app_detail
  When envio GET /applications/:id
  Then recibo HTTP 200 con application, applicant, contacts[2], addresses[1], beneficiaries[3], documents[1], observations[2], trace_events[]

Scenario: Solicitud inexistente retorna 404
  When envio GET /applications/00000000-0000-0000-0000-000000000000
  Then recibo HTTP 404 con codigo OPS_APPLICATION_NOT_FOUND

Scenario: Solicitante con multiples solicitudes incluye count
  Given el solicitante tiene 5 solicitudes
  When envio GET /applications/:id
  Then applicant.application_count = 5
```

#### Trazabilidad de Pruebas

| Test ID | Tipo | Descripcion |
|---------|------|-------------|
| TP-OPS-06-01 | Positivo | Detalle completo retorna 8 secciones de datos |
| TP-OPS-06-02 | Positivo | Solicitante incluye application_count correcto |
| TP-OPS-06-03 | Positivo | Solicitud sin beneficiarios retorna array vacio |
| TP-OPS-06-04 | Positivo | Trace events incluyen details anidados |
| TP-OPS-06-05 | Negativo | 404 para solicitud inexistente |
| TP-OPS-06-06 | Negativo | 403 sin permiso p_ops_app_detail |
| TP-OPS-06-07 | Integracion | RLS filtra por tenant |
| TP-OPS-06-08 | Integracion | Observaciones ordenadas por created_at DESC |
| TP-OPS-06-09 | E2E | Click en solicitud del listado → pantalla detalle con 8 solapas navegables |

#### Sin Ambiguedades

- **Supuestos prohibidos:** No se asume que el frontend hace queries separadas por solapa. El backend retorna todo en un solo GET. No se asume que se consulta Catalog para producto.
- **Decisiones cerradas:** Todo se retorna en un solo endpoint. Producto viene de campos denormalizados. Trazabilidad ordenada ASC (cronologica).
- **Fuera de alcance explicito:** Lazy loading de solapas. Cache de detalle. Edicion inline desde detalle (excepto via RF-OPS-04, RF-OPS-08, RF-OPS-09, RF-OPS-10, RF-OPS-11).
- **TODO explicitos = 0**

---

### RF-OPS-07 — Trazabilidad BPM (timeline visual)

#### Execution Sheet

| Campo | Valor |
|-------|-------|
| **ID** | RF-OPS-07 |
| **Titulo** | Construir timeline BPM con estados completados, actual y pendientes |
| **Actor(es)** | Operador, Admin Entidad, Super Admin, Consulta, Auditor |
| **Prioridad** | Alta |
| **Severidad** | P1 |
| **Flujo origen** | FL-OPS-01 seccion 8b |
| **HU origen** | HU012 |

#### Precondiciones

| # | Condicion | Detalle |
|---|-----------|---------|
| 1 | Operations Service operativo | SA.operations accesible |
| 2 | Solicitud existente | Con al menos un trace_event (creacion) |
| 3 | Usuario autenticado | JWT con permiso `p_ops_app_detail` |

#### Entradas

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| `id` | uuid | Si | Path param `/applications/:id/timeline` | UUID valido, solicitud existente | — |
| `tenant_id` | uuid | Si | JWT claim | UUID valido | — |

#### Proceso (Happy Path)

| # | Paso | Responsable |
|---|------|-------------|
| 1 | Recibir `GET /applications/:id/timeline` | API Gateway |
| 2 | Verificar permiso `p_ops_app_detail` en JWT | API Gateway |
| 3 | Ejecutar `SET LOCAL app.current_tenant = '{tenant_id}'` | Operations Service |
| 4 | SELECT de `applications` por `id`: obtener `status` actual y `workflow_stage` | Operations Service |
| 5 | SELECT de `trace_events` + `trace_event_details` por `application_id` ordenados por `occurred_at ASC` | Operations Service |
| 6 | Construir array de nodos del timeline basado en state machine + trace_events existentes | Operations Service |
| 7 | Para cada nodo: determinar tipo visual (completed, current, pending, final_approved, final_rejected) | Operations Service |
| 8 | Retornar `200 OK` con `{ current_status, workflow_stage, nodes: [{ state, type, action?, user_name?, occurred_at?, details[]? }] }` | Operations Service |

#### Salidas

| Campo | Tipo | Destino | Efecto observable |
|-------|------|---------|-------------------|
| `current_status` | enum | Response body | Estado actual de la solicitud |
| `workflow_stage` | string? | Response body | Etapa informativa del workflow (nullable) |
| `nodes[]` | array | Response body → SPA | Nodos del timeline para renderizado visual |
| `nodes[].state` | string | Response body | Estado representado (draft, pending, etc) |
| `nodes[].type` | enum | Response body | `completed`, `current`, `pending`, `final_approved`, `final_rejected` |
| `nodes[].action` | string? | Response body | Accion ejecutada (solo si completed/current) |
| `nodes[].user_name` | string? | Response body | Quien ejecuto la accion |
| `nodes[].occurred_at` | timestamp? | Response body | Cuando ocurrio |
| `nodes[].details[]` | array? | Response body | trace_event_details del nodo |

#### Errores Tipados

| Codigo | Causa | Condicion de disparo | Respuesta esperada |
|--------|-------|---------------------|--------------------|
| `OPS_APPLICATION_NOT_FOUND` | Solicitud inexistente | `id` no existe o no pertenece al tenant | HTTP 404 |
| `OPS_UNAUTHORIZED` | Sin permiso | JWT no contiene `p_ops_app_detail` | HTTP 403 |

#### Casos Especiales y Variantes

- **Relacion con RF-OPS-06 (detalle):** RF-OPS-06 retorna `trace_events[]` como datos crudos (sin tipo visual). Este endpoint (RF-OPS-07) retorna nodos ya calculados con tipo visual (`completed`, `current`, `pending`, etc.). El frontend debe usar este endpoint para construir la solapa de trazabilidad BPM visual. No es necesario llamar a ambos endpoints para la solapa de trazabilidad.
- **Sin workflow asociado:** Si `workflow_stage` es null, el timeline muestra solo los nodos de la state machine. No es un error.
- **Tipos de nodo visual:**
  - `completed`: trace_event existe para ese estado. Circulo primario + CheckCircle2.
  - `current`: es el estado actual y no es estado final. Anillo animado + Clock + badge "En curso".
  - `pending`: no hay trace_event y no es estado actual. Circulo muted + Circle vacio.
  - `final_approved`: status = approved o settled. Circulo verde + badge "Finalizado".
  - `final_rejected`: status = rejected. Circulo rojo + badge "Rechazada".
- **Nodos de la state machine:** draft → pending → in_review → (approved/rejected). cancelled no aparece como nodo en el timeline (es un estado terminal lateral).

#### Impacto en Modelo de Datos

| Entidad | Operacion | Campo(s) afectado(s) | Evento |
|---------|-----------|---------------------|--------|
| `applications` | SELECT | `status`, `workflow_stage` | — |
| `trace_events` | SELECT | todos | — |
| `trace_event_details` | SELECT | todos | — |

#### Criterios de Aceptacion (Gherkin)

```gherkin
Scenario: Timeline de solicitud en in_review
  Given una solicitud con status="in_review" y trace_events para draft, pending, in_review
  When envio GET /applications/:id/timeline
  Then recibo nodes con: draft(completed), pending(completed), in_review(current), approved(pending), rejected(pending)

Scenario: Timeline de solicitud aprobada
  Given una solicitud con status="approved" y trace_events completos
  When envio GET /applications/:id/timeline
  Then recibo nodes con: draft(completed), pending(completed), in_review(completed), approved(final_approved)

Scenario: Timeline sin workflow muestra solo state machine
  Given una solicitud sin workflow_stage
  When envio GET /applications/:id/timeline
  Then recibo workflow_stage=null y nodes basados en state machine
```

#### Trazabilidad de Pruebas

| Test ID | Tipo | Descripcion |
|---------|------|-------------|
| TP-OPS-07-01 | Positivo | Timeline con nodos completed, current y pending |
| TP-OPS-07-02 | Positivo | Timeline de solicitud aprobada muestra final_approved |
| TP-OPS-07-03 | Positivo | Timeline de solicitud rechazada muestra final_rejected |
| TP-OPS-07-04 | Positivo | Timeline sin workflow muestra solo state machine |
| TP-OPS-07-05 | Negativo | 404 para solicitud inexistente |
| TP-OPS-07-06 | Negativo | 403 sin permiso |
| TP-OPS-07-07 | Integracion | trace_events ordenados cronologicamente |
| TP-OPS-07-08 | E2E | Solapa trazabilidad muestra timeline visual con iconos |

#### Sin Ambiguedades

- **Supuestos prohibidos:** No se asume que el frontend construye el timeline. El backend retorna los nodos con tipo visual calculado. No se asume que cancelled aparece en el timeline.
- **Decisiones cerradas:** Status + workflow_stage hibrido (D-OPS-01). Cancelled es terminal lateral, no aparece en timeline lineal.
- **Fuera de alcance explicito:** Timeline interactivo (no se puede hacer click para cambiar estado). Historial de workflow_stage.
- **TODO explicitos = 0**

---

### RF-OPS-08 — CRUD contactos y direcciones del solicitante

#### Execution Sheet

| Campo | Valor |
|-------|-------|
| **ID** | RF-OPS-08 |
| **Titulo** | CRUD de contactos y direcciones del solicitante (datos compartidos) |
| **Actor(es)** | Operador, Admin Entidad, Super Admin |
| **Prioridad** | Alta |
| **Severidad** | P0 |
| **Flujo origen** | FL-OPS-01 seccion 6, 8c |
| **HU origen** | HU013 |

#### Precondiciones

| # | Condicion | Detalle |
|---|-----------|---------|
| 1 | Operations Service operativo | SA.operations accesible |
| 2 | Solicitante existente | `applicant_id` valido en `applicants` |
| 3 | Usuario autenticado | JWT con permiso `p_ops_app_edit` |

#### Entradas — Crear/Editar Contacto

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| `applicant_id` | uuid | Si | Path param | UUID valido, solicitante existente | — |
| `id` | uuid | Condicional | Path param (editar) | UUID valido para editar; ausente para crear | — |
| `type` | enum | Si | Body JSON | personal, work, emergency, other | — |
| `email` | string | No | Body JSON | Formato email valido si presente | — |
| `phone_code` | string | No | Body JSON | Max 5 chars | — |
| `phone` | string | No | Body JSON | Max 20 chars | — |

#### Entradas — Crear/Editar Direccion

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| `applicant_id` | uuid | Si | Path param | UUID valido, solicitante existente | — |
| `id` | uuid | Condicional | Path param (editar) | UUID valido para editar; ausente para crear | — |
| `type` | enum | Si | Body JSON | home, work, legal, other | — |
| `street` | string | Si | Body JSON | No vacio, max 200 chars | — |
| `number` | string | Si | Body JSON | No vacio, max 20 chars | — |
| `floor` | string | No | Body JSON | Max 10 chars | — |
| `apartment` | string | No | Body JSON | Max 10 chars | — |
| `city` | string | Si | Body JSON | No vacio, max 100 chars | — |
| `province` | string | Si | Body JSON | No vacio, max 100 chars | — |
| `postal_code` | string | Si | Body JSON | No vacio, max 10 chars | — |
| `latitude` | decimal | No | Body JSON | -90 a 90 | — |
| `longitude` | decimal | No | Body JSON | -180 a 180 | — |

#### Proceso (Happy Path) — Crear Contacto

| # | Paso | Responsable |
|---|------|-------------|
| 1 | Recibir `POST /applicants/:applicant_id/contacts` | API Gateway |
| 2 | Verificar permiso `p_ops_app_edit` en JWT | API Gateway |
| 3 | Ejecutar `SET LOCAL app.current_tenant = '{tenant_id}'` | Operations Service |
| 4 | Validar que `applicant_id` existe (RLS) | Operations Service |
| 5 | INSERT en `applicant_contacts` con `id` (UUIDv7), `applicant_id`, `tenant_id`, campos del body | Operations Service |
| 6 | Retornar `201 Created` con contacto creado | Operations Service |

#### Proceso — Editar/Eliminar Contacto

| # | Paso | Responsable |
|---|------|-------------|
| 1 | `PUT /applicants/:applicant_id/contacts/:id` o `DELETE /applicants/:applicant_id/contacts/:id` | API Gateway |
| 2 | Verificar permiso y RLS | Operations Service |
| 3 | UPDATE o DELETE del contacto | Operations Service |
| 4 | Retornar `200 OK` o `204 No Content` | Operations Service |

*(Misma logica para direcciones con endpoints `/applicants/:applicant_id/addresses`)*

#### Salidas

| Campo | Tipo | Destino | Efecto observable |
|-------|------|---------|-------------------|
| `201 Created` / `200 OK` / `204 No Content` | HTTP status | Response → SPA | Operacion CRUD exitosa |
| Contacto o direccion | object | Response body (crear/editar) | Datos del recurso |

#### Errores Tipados

| Codigo | Causa | Condicion de disparo | Respuesta esperada |
|--------|-------|---------------------|--------------------|
| `OPS_APPLICANT_NOT_FOUND` | Solicitante inexistente | `applicant_id` no existe o no pertenece al tenant | HTTP 404 |
| `OPS_CONTACT_NOT_FOUND` | Contacto inexistente | `id` no existe para ese solicitante | HTTP 404 |
| `OPS_ADDRESS_NOT_FOUND` | Direccion inexistente | `id` no existe para ese solicitante | HTTP 404 |
| `OPS_VALIDATION_ERROR` | Entrada invalida | Campos requeridos vacios, tipo invalido | HTTP 422 |
| `OPS_UNAUTHORIZED` | Sin permiso | JWT no contiene `p_ops_app_edit` | HTTP 403 |

#### Casos Especiales y Variantes

- **Datos compartidos (RN-OPS-13):** Contactos y direcciones pertenecen al solicitante, no a la solicitud. Editar un contacto afecta a todas las solicitudes del mismo solicitante.
- **Advertencia UI (RN-OPS-06):** El frontend consulta `application_count` y muestra advertencia si >1. Esto NO es responsabilidad del backend.
- **Al menos un contacto requerido:** No se fuerza desde backend. Es una regla de UI. El backend permite 0 contactos.

#### Impacto en Modelo de Datos

| Entidad | Operacion | Campo(s) afectado(s) | Evento |
|---------|-----------|---------------------|--------|
| `applicants` | SELECT | `id` (validacion) | — |
| `applicant_contacts` | INSERT, UPDATE, DELETE | todos los campos | — |
| `applicant_addresses` | INSERT, UPDATE, DELETE | todos los campos | — |

#### Criterios de Aceptacion (Gherkin)

```gherkin
Scenario: Crear contacto del solicitante
  Given existe un solicitante con id "app-001"
  When envio POST /applicants/app-001/contacts con type="personal", email="juan@test.com"
  Then recibo HTTP 201 con el contacto creado

Scenario: Editar direccion del solicitante
  Given existe una direccion con id "addr-001" del solicitante "app-001"
  When envio PUT /applicants/app-001/addresses/addr-001 con city="Cordoba"
  Then recibo HTTP 200 con city="Cordoba"

Scenario: Eliminar contacto
  Given existe un contacto con id "cont-001"
  When envio DELETE /applicants/app-001/contacts/cont-001
  Then recibo HTTP 204

Scenario: Solicitante inexistente retorna 404
  When envio POST /applicants/00000000-0000-0000-0000-000000000000/contacts
  Then recibo HTTP 404 con codigo OPS_APPLICANT_NOT_FOUND
```

#### Trazabilidad de Pruebas

| Test ID | Tipo | Descripcion |
|---------|------|-------------|
| TP-OPS-08-01 | Positivo | Crear contacto retorna 201 |
| TP-OPS-08-02 | Positivo | Editar contacto retorna 200 |
| TP-OPS-08-03 | Positivo | Eliminar contacto retorna 204 |
| TP-OPS-08-04 | Positivo | Crear direccion con coordenadas retorna 201 |
| TP-OPS-08-05 | Positivo | Editar direccion retorna 200 |
| TP-OPS-08-06 | Positivo | Eliminar direccion retorna 204 |
| TP-OPS-08-07 | Negativo | 404 para solicitante inexistente |
| TP-OPS-08-08 | Negativo | 404 para contacto/direccion inexistente |
| TP-OPS-08-09 | Negativo | 422 por campos requeridos vacios |
| TP-OPS-08-10 | Negativo | 403 sin permiso p_ops_app_edit |
| TP-OPS-08-11 | Integracion | RLS filtra por tenant |
| TP-OPS-08-12 | Integracion | Contacto editado visible desde otra solicitud del mismo solicitante |
| TP-OPS-08-13 | E2E | Solapa contactos → agregar → editar → eliminar contacto |

#### Sin Ambiguedades

- **Supuestos prohibidos:** No se asume que contactos son por solicitud. Son por solicitante (RN-OPS-13). No se asume que el backend valida minimo de contactos.
- **Decisiones cerradas:** Contactos y direcciones pertenecen a `applicant_id`, compartidos entre solicitudes. Eliminacion fisica (no soft delete).
- **Fuera de alcance explicito:** Validacion de direccion contra servicio de geocodificacion. Import de contactos. Deduplicacion de contactos.
- **TODO explicitos = 0**

---

### RF-OPS-09 — CRUD beneficiarios de solicitud

#### Execution Sheet

| Campo | Valor |
|-------|-------|
| **ID** | RF-OPS-09 |
| **Titulo** | CRUD de beneficiarios de una solicitud especifica |
| **Actor(es)** | Operador, Admin Entidad, Super Admin |
| **Prioridad** | Alta |
| **Severidad** | P1 |
| **Flujo origen** | FL-OPS-01 seccion 8a |
| **HU origen** | HU013 |

#### Precondiciones

| # | Condicion | Detalle |
|---|-----------|---------|
| 1 | Operations Service operativo | SA.operations accesible |
| 2 | Solicitud existente | `application_id` valido en `applications` |
| 3 | Usuario autenticado | JWT con permiso `p_ops_app_edit` |

#### Entradas

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| `application_id` | uuid | Si | Path param | UUID valido, solicitud existente | — |
| `first_name` | string | Si | Body JSON | No vacio, max 100 chars | — |
| `last_name` | string | Si | Body JSON | No vacio, max 100 chars | — |
| `relationship` | string | Si | Body JSON | No vacio, max 100 chars | — |
| `percentage` | decimal | Si | Body JSON | > 0, <= 100 | RN-OPS-10 |

#### Proceso (Happy Path) — Crear Beneficiario

| # | Paso | Responsable |
|---|------|-------------|
| 1 | Recibir `POST /applications/:application_id/beneficiaries` | API Gateway |
| 2 | Verificar permiso `p_ops_app_edit` en JWT | API Gateway |
| 3 | Validar que la solicitud existe (RLS) | Operations Service |
| 4 | INSERT en `beneficiaries` con `id` (UUIDv7), `application_id`, `tenant_id` | Operations Service |
| 5 | Retornar `201 Created` | Operations Service |

*(Editar: `PUT /applications/:application_id/beneficiaries/:id`, Eliminar: `DELETE /applications/:application_id/beneficiaries/:id`)*

#### Salidas

| Campo | Tipo | Destino | Efecto observable |
|-------|------|---------|-------------------|
| `201 Created` / `200 OK` / `204 No Content` | HTTP status | Response → SPA | Operacion CRUD exitosa |

#### Errores Tipados

| Codigo | Causa | Condicion de disparo | Respuesta esperada |
|--------|-------|---------------------|--------------------|
| `OPS_APPLICATION_NOT_FOUND` | Solicitud inexistente | `application_id` no existe | HTTP 404 |
| `OPS_BENEFICIARY_NOT_FOUND` | Beneficiario inexistente | `id` no existe para esa solicitud | HTTP 404 |
| `OPS_VALIDATION_ERROR` | Entrada invalida | Campos requeridos vacios, percentage fuera de rango | HTTP 422 |
| `OPS_UNAUTHORIZED` | Sin permiso | JWT no contiene `p_ops_app_edit` | HTTP 403 |

#### Casos Especiales y Variantes

- **Validacion de porcentaje:** RN-OPS-10 indica que la suma debe ser 100. Sin embargo, la validacion de suma total NO se ejecuta en cada operacion CRUD individual. Se valida al enviar la solicitud (transicion draft→pending). Esto permite agregar beneficiarios incrementalmente.
- **Beneficiarios pertenecen a la solicitud (RN-OPS-11):** A diferencia de contactos, los beneficiarios son por `application_id`, no por `applicant_id`.

#### Impacto en Modelo de Datos

| Entidad | Operacion | Campo(s) afectado(s) | Evento |
|---------|-----------|---------------------|--------|
| `applications` | SELECT | `id` (validacion) | — |
| `beneficiaries` | INSERT, UPDATE, DELETE | todos | — |

#### Criterios de Aceptacion (Gherkin)

```gherkin
Scenario: Crear beneficiario de solicitud
  Given existe una solicitud con id "sol-001"
  When envio POST /applications/sol-001/beneficiaries con first_name="Maria", last_name="Perez", relationship="Hija", percentage=50
  Then recibo HTTP 201 con beneficiario creado

Scenario: Eliminar beneficiario
  When envio DELETE /applications/sol-001/beneficiaries/ben-001
  Then recibo HTTP 204

Scenario: Solicitud inexistente retorna 404
  When envio POST /applications/00000000-0000-0000-0000-000000000000/beneficiaries
  Then recibo HTTP 404
```

#### Trazabilidad de Pruebas

| Test ID | Tipo | Descripcion |
|---------|------|-------------|
| TP-OPS-09-01 | Positivo | Crear beneficiario retorna 201 |
| TP-OPS-09-02 | Positivo | Editar beneficiario retorna 200 |
| TP-OPS-09-03 | Positivo | Eliminar beneficiario retorna 204 |
| TP-OPS-09-04 | Negativo | 404 para solicitud inexistente |
| TP-OPS-09-05 | Negativo | 404 para beneficiario inexistente |
| TP-OPS-09-06 | Negativo | 422 por percentage fuera de rango |
| TP-OPS-09-07 | Negativo | 403 sin permiso |
| TP-OPS-09-08 | Integracion | RLS filtra por tenant |
| TP-OPS-09-09 | E2E | Solapa beneficiarios → agregar → editar → eliminar |

#### Sin Ambiguedades

- **Supuestos prohibidos:** No se asume que la suma de porcentajes se valida en cada operacion individual. Se valida en transicion.
- **Decisiones cerradas:** Beneficiarios pertenecen a solicitud (RN-OPS-11). Validacion de suma 100% se difiere a transicion draft→pending.
- **Fuera de alcance explicito:** Beneficiarios compartidos entre solicitudes. Porcentaje auto-distribuido.
- **TODO explicitos = 0**

---

### RF-OPS-10 — Gestionar documentos de solicitud

#### Execution Sheet

| Campo | Valor |
|-------|-------|
| **ID** | RF-OPS-10 |
| **Titulo** | Subir, aprobar, rechazar y eliminar documentos de una solicitud |
| **Actor(es)** | Operador, Admin Entidad, Super Admin |
| **Prioridad** | Alta |
| **Severidad** | P1 |
| **Flujo origen** | FL-OPS-01 seccion 8a |
| **HU origen** | HU013 |

#### Precondiciones

| # | Condicion | Detalle |
|---|-----------|---------|
| 1 | Operations Service operativo | SA.operations accesible |
| 2 | File system disponible | `{STORAGE_ROOT}` accesible con permisos de escritura |
| 3 | Solicitud existente | `application_id` valido en `applications` |
| 4 | Usuario autenticado | JWT con permiso `p_ops_doc_manage` |

#### Entradas — Subir Documento

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| `application_id` | uuid | Si | Path param | UUID valido, solicitud existente | — |
| `file` | binary | Si | Multipart form | Max 10MB, tipos: pdf, jpg, png, docx | — |
| `name` | string | Si | Form field | No vacio, max 200 chars | — |
| `document_type` | string | Si | Form field | No vacio, max 100 chars (ej: "DNI frente", "Recibo de sueldo") | — |

#### Proceso (Happy Path) — Subir

| # | Paso | Responsable |
|---|------|-------------|
| 1 | Recibir `POST /applications/:application_id/documents` (multipart) | API Gateway |
| 2 | Verificar permiso `p_ops_doc_manage` en JWT | API Gateway |
| 3 | Validar solicitud existe (RLS) | Operations Service |
| 4 | Guardar archivo en `{STORAGE_ROOT}/{tenant_id}/documents/{application_id}/{document_id}_{original_filename}` | Operations Service |
| 5 | INSERT en `application_documents` con `id` (UUIDv7), `application_id`, `tenant_id`, `name`, `document_type`, `file_url` (ruta relativa), `status = 'pending'` | Operations Service |
| 6 | Retornar `201 Created` con documento creado | Operations Service |

#### Proceso — Cambiar estado documento

| # | Paso | Responsable |
|---|------|-------------|
| 1 | Recibir `PUT /applications/:application_id/documents/:id/status` con `{ status: 'approved' | 'rejected' }` | API Gateway |
| 2 | UPDATE `application_documents` SET `status`, `updated_at`, `updated_by` | Operations Service |
| 3 | Retornar `200 OK` | Operations Service |

#### Proceso — Eliminar documento

| # | Paso | Responsable |
|---|------|-------------|
| 1 | Recibir `DELETE /applications/:application_id/documents/:id` | API Gateway |
| 2 | DELETE de `application_documents` + eliminar archivo fisico del file system | Operations Service |
| 3 | Retornar `204 No Content` | Operations Service |

#### Salidas

| Campo | Tipo | Destino | Efecto observable |
|-------|------|---------|-------------------|
| `201 Created` / `200 OK` / `204 No Content` | HTTP status | Response → SPA | Operacion exitosa |
| `file_url` | string | Response body (subir) | Ruta relativa al archivo almacenado |
| `status` | enum | Response body | pending, approved, rejected |

#### Errores Tipados

| Codigo | Causa | Condicion de disparo | Respuesta esperada |
|--------|-------|---------------------|--------------------|
| `OPS_APPLICATION_NOT_FOUND` | Solicitud inexistente | `application_id` no existe | HTTP 404 |
| `OPS_DOCUMENT_NOT_FOUND` | Documento inexistente | `id` no existe | HTTP 404 |
| `OPS_FILE_TOO_LARGE` | Archivo excede limite | Tamano > 10MB | HTTP 413 |
| `OPS_FILE_TYPE_NOT_ALLOWED` | Tipo no permitido | Extension no es pdf/jpg/png/docx | HTTP 422 |
| `OPS_STORAGE_ERROR` | Error de file system | No se puede escribir en STORAGE_ROOT | HTTP 500 |
| `OPS_UNAUTHORIZED` | Sin permiso | JWT no contiene `p_ops_doc_manage` | HTTP 403 |

#### Casos Especiales y Variantes

- **Documentos pertenecen a la solicitud (RN-OPS-11):** No se comparten entre solicitudes.
- **Eliminacion fisica:** Al eliminar un documento, se borra el registro en DB Y el archivo del file system.
- **Estado inicial:** Todo documento se crea con `status = 'pending'`.

#### Impacto en Modelo de Datos

| Entidad | Operacion | Campo(s) afectado(s) | Evento |
|---------|-----------|---------------------|--------|
| `application_documents` | INSERT, UPDATE (status), DELETE | todos | — |
| File system | WRITE (subir), DELETE (eliminar) | `{STORAGE_ROOT}/{tenant_id}/documents/...` | — |

#### Criterios de Aceptacion (Gherkin)

```gherkin
Scenario: Subir documento a solicitud
  Given existe una solicitud con id "sol-001"
  When envio POST /applications/sol-001/documents con archivo PDF y name="DNI frente"
  Then recibo HTTP 201 con file_url y status="pending"
  And el archivo se guarda en el file system

Scenario: Aprobar documento
  Given existe un documento con status="pending"
  When envio PUT /applications/:id/documents/:doc_id/status con status="approved"
  Then recibo HTTP 200 con status="approved"

Scenario: Archivo excede 10MB
  When envio un archivo de 15MB
  Then recibo HTTP 413 con codigo OPS_FILE_TOO_LARGE
```

#### Trazabilidad de Pruebas

| Test ID | Tipo | Descripcion |
|---------|------|-------------|
| TP-OPS-10-01 | Positivo | Subir documento PDF retorna 201 con file_url |
| TP-OPS-10-02 | Positivo | Aprobar documento cambia status |
| TP-OPS-10-03 | Positivo | Rechazar documento cambia status |
| TP-OPS-10-04 | Positivo | Eliminar documento borra registro y archivo |
| TP-OPS-10-05 | Negativo | 413 archivo excede 10MB |
| TP-OPS-10-06 | Negativo | 422 tipo de archivo no permitido |
| TP-OPS-10-07 | Negativo | 404 solicitud inexistente |
| TP-OPS-10-08 | Negativo | 403 sin permiso p_ops_doc_manage |
| TP-OPS-10-09 | Integracion | Archivo almacenado en ruta correcta por tenant |
| TP-OPS-10-10 | E2E | Solapa documentos → subir → badge pending → aprobar → badge approved |

#### Sin Ambiguedades

- **Supuestos prohibidos:** No se asume que los documentos se almacenan en DB. Se usa file system. No se asume que documentos se comparten entre solicitudes.
- **Decisiones cerradas:** Limite 10MB. Tipos: pdf, jpg, png, docx. Status inicial: pending.
- **Fuera de alcance explicito:** Preview de documentos. OCR. Versionamiento de documentos. Firma digital.
- **TODO explicitos = 0**

---

### RF-OPS-11 — Agregar observacion a solicitud

#### Execution Sheet

| Campo | Valor |
|-------|-------|
| **ID** | RF-OPS-11 |
| **Titulo** | Agregar observacion de texto a una solicitud (append-only) |
| **Actor(es)** | Operador, Admin Entidad, Super Admin |
| **Prioridad** | Media |
| **Severidad** | P2 |
| **Flujo origen** | FL-OPS-01 seccion 8a |
| **HU origen** | HU013 |

#### Precondiciones

| # | Condicion | Detalle |
|---|-----------|---------|
| 1 | Operations Service operativo | SA.operations accesible |
| 2 | Solicitud existente | `application_id` valido en `applications` |
| 3 | Usuario autenticado | JWT con permiso `p_ops_obs_create` |

#### Entradas

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| `application_id` | uuid | Si | Path param | UUID valido, solicitud existente | — |
| `content` | text | Si | Body JSON | No vacio, max 5000 chars | — |
| `observation_type` | enum | No | Servidor (hardcoded) | Valor fijo `manual` para observaciones creadas por usuario. Valores validos: `manual` (default), `message` (solo via RF-OPS-15). No enviado por el cliente. | RN-OPS-16 |
| `tenant_id` | uuid | Si | JWT claim | UUID valido | — |
| `user_id` | uuid | Si | JWT claim `sub` | Extraido del token | — |
| `user_name` | string | Si | JWT claims | Construido de first_name + last_name del token o denormalizado | RN-OPS-05 |

#### Proceso (Happy Path)

| # | Paso | Responsable |
|---|------|-------------|
| 1 | Recibir `POST /applications/:application_id/observations` con `{ content }` | API Gateway |
| 2 | Verificar permiso `p_ops_obs_create` en JWT | API Gateway |
| 3 | Validar solicitud existe (RLS) | Operations Service |
| 4 | INSERT en `application_observations` con `id` (UUIDv7), `application_id`, `observation_type='manual'`, `content`, `user_id` (del JWT), `user_name` (del JWT), `created_at = NOW()` | Operations Service |
| 5 | Retornar `201 Created` con `{ id, content, user_name, created_at }` | Operations Service |

#### Salidas

| Campo | Tipo | Destino | Efecto observable |
|-------|------|---------|-------------------|
| `201 Created` | HTTP status | Response → SPA | Observacion creada |
| `id` | uuid | Response body | ID de la observacion |
| `user_name` | string | Response body | Nombre del usuario que creo la observacion |
| `created_at` | timestamp | Response body | Timestamp de creacion |

#### Errores Tipados

| Codigo | Causa | Condicion de disparo | Respuesta esperada |
|--------|-------|---------------------|--------------------|
| `OPS_APPLICATION_NOT_FOUND` | Solicitud inexistente | `application_id` no existe | HTTP 404 |
| `OPS_VALIDATION_ERROR` | Contenido vacio | `content` vacio o > 5000 chars | HTTP 422 |
| `OPS_UNAUTHORIZED` | Sin permiso | JWT no contiene `p_ops_obs_create` | HTTP 403 |

#### Casos Especiales y Variantes

- **Append-only (RN-OPS-12):** No se pueden editar ni eliminar observaciones. Solo INSERT.
- **Cualquier estado:** Las observaciones se pueden agregar en cualquier estado de la solicitud, incluyendo rejected y cancelled.
- **user_name denormalizado:** Se copia del JWT al crear. No se actualiza si el usuario cambia de nombre.

#### Impacto en Modelo de Datos

| Entidad | Operacion | Campo(s) afectado(s) | Evento |
|---------|-----------|---------------------|--------|
| `application_observations` | INSERT | `id`, `application_id`, `observation_type` (`manual`), `content`, `user_id`, `user_name`, `created_at` | — |

#### Criterios de Aceptacion (Gherkin)

```gherkin
Scenario: Agregar observacion a solicitud
  Given existe una solicitud con id "sol-001"
  And el usuario tiene permiso p_ops_obs_create
  When envio POST /applications/sol-001/observations con content="Falta documentacion"
  Then recibo HTTP 201 con id, content, user_name y created_at

Scenario: Observacion en solicitud rechazada permitida
  Given existe una solicitud en status="rejected"
  When envio POST /applications/:id/observations con content="Nota post-rechazo"
  Then recibo HTTP 201

Scenario: Contenido vacio retorna 422
  When envio POST /applications/:id/observations con content=""
  Then recibo HTTP 422 con codigo OPS_VALIDATION_ERROR
```

#### Trazabilidad de Pruebas

| Test ID | Tipo | Descripcion |
|---------|------|-------------|
| TP-OPS-11-01 | Positivo | Agregar observacion retorna 201 con user_name |
| TP-OPS-11-02 | Positivo | Observacion permitida en cualquier estado |
| TP-OPS-11-03 | Negativo | 422 por contenido vacio |
| TP-OPS-11-04 | Negativo | 404 para solicitud inexistente |
| TP-OPS-11-05 | Negativo | 403 sin permiso p_ops_obs_create |
| TP-OPS-11-06 | Integracion | Observacion visible en detalle (RF-OPS-06) |
| TP-OPS-11-07 | E2E | Solapa observaciones → escribir texto → submit → aparece en lista con avatar |

#### Sin Ambiguedades

- **Supuestos prohibidos:** No se asume que las observaciones se pueden editar o eliminar (RN-OPS-12). No se asume que se valida estado de solicitud para agregar.
- **Decisiones cerradas:** Append-only. Se permiten en cualquier estado. user_name denormalizado.
- **Fuera de alcance explicito:** Edicion de observaciones. Eliminacion de observaciones. Observaciones con archivos adjuntos. Mencion de usuarios (@usuario).
- **TODO explicitos = 0**

---

### RF-OPS-15 — Enviar mensaje desde solicitud con resolucion de variables de plantilla

#### Execution Sheet

| Campo | Valor |
|-------|-------|
| **ID** | RF-OPS-15 |
| **Titulo** | Enviar mensaje desde solicitud con resolucion de variables de plantilla |
| **Actor(es)** | Operador, Admin Entidad, Super Admin |
| **Prioridad** | Alta |
| **Severidad** | P1 |
| **Flujo origen** | FL-OPS-03 seccion 6 |
| **HU origen** | HU037 |

> **Nota de consolidacion:** FL-OPS-03 propuso 4 RF candidatos (RF-OPS-15 a RF-OPS-18). Se consolidaron: RF-OPS-16 (resolucion de variables) y RF-OPS-17 (auto-observacion) son pasos internos de esta operacion, no comportamientos separados. RF-OPS-18 (despacho asincrono) es responsabilidad de Notification Service, fuera de alcance de Operations.

#### Precondiciones

| # | Condicion | Detalle |
|---|-----------|---------|
| 1 | Operations Service operativo | SA.operations accesible |
| 2 | Config Service operativo | Para lookup de plantilla (sync HTTP) |
| 3 | RabbitMQ operativo | Para publicar MessageSentEvent |
| 4 | Solicitud existente | `application_id` valido en `applications` para el tenant |
| 5 | Usuario autenticado | JWT con permiso `p_ops_msg_send` |

#### Entradas

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| `application_id` | uuid | Si | Path param `/applications/:id/messages` | UUID valido, solicitud existente para tenant | — |
| `tenant_id` | uuid | Si | JWT claim | UUID valido | — |
| `channel` | enum | Si | Request body | Valores: `email`, `sms`, `whatsapp` | — |
| `template_id` | uuid | Si | Request body | UUID valido; plantilla existente en Config Service para ese canal | RN-OPS-15 |
| `recipient` | string | Si | Request body | Email RFC 5322 si channel=email; telefono E.164 si sms/whatsapp. Max 320 chars. | — |
| `subject` | string | Condicional | Request body | Requerido si channel=email y template.subject es null. Max 200 chars. | — |
| `body_override` | string | No | Request body | Si se provee, reemplaza el cuerpo resuelto de la plantilla. Max 5000 chars. | RN-OPS-18 |

#### Proceso (Happy Path)

| # | Paso | Responsable |
|---|------|-------------|
| 1 | Recibir `POST /applications/:id/messages` | API Gateway |
| 2 | Verificar permiso `p_ops_msg_send` en JWT | API Gateway |
| 3 | Ejecutar `SET LOCAL app.current_tenant = '{tenant_id}'` | Operations Service |
| 4 | SELECT `applications` JOIN `applicants` por `application_id` para obtener datos de variables | Operations Service |
| 5 | Si solicitud no existe → retornar 404 `OPS_APPLICATION_NOT_FOUND` | Operations Service |
| 6 | GET `/notification-templates/:template_id` a Config Service (sync HTTP, timeout 5s) | Operations Service |
| 7 | Si Config retorna 404 → retornar 422 `OPS_TEMPLATE_NOT_FOUND` | Operations Service |
| 8 | Validar `template.channel == request.channel`; si no coincide → 422 `OPS_TEMPLATE_CHANNEL_MISMATCH` | Operations Service |
| 9 | Si `body_override` NO se provee: resolver variables en `template.content` usando datos de application + applicant. Variables disponibles: `{{nombre}}` → `applicant.first_name`, `{{apellido}}` → `applicant.last_name`, `{{codigo_solicitud}}` → `application.code`, `{{producto}}` → `application.product_name`, `{{plan}}` → `application.plan_name`, `{{estado}}` → `application.status`. Variable sin dato → cadena vacia. | Operations Service |
| 10 | Si `body_override` se provee: usar `body_override` como cuerpo final (sin resolver variables) | Operations Service |
| 11 | Determinar subject: si channel=email → usar `request.subject` ?? resolver `template.subject` ?? error 422 `OPS_EMAIL_SUBJECT_REQUIRED`. Si channel!=email → subject = null. | Operations Service |
| 12 | INSERT `application_observations` con `observation_type='message'`, `content='[{channel}] Mensaje enviado a {recipient} usando plantilla "{template.title}"'`, `user_id`, `user_name` (del JWT) | Operations Service |
| 13 | Publicar `MessageSentEvent` a RabbitMQ: `{ application_id, tenant_id, channel, recipient, subject, resolved_body, template_id, template_title, user_id, sent_at }` | Operations Service |
| 14 | Retornar `202 Accepted` con `{ message_id, channel, recipient, status: "queued" }` | Operations Service |

#### Salidas

| Campo | Tipo | Destino | Efecto observable |
|-------|------|---------|-------------------|
| `message_id` | uuid | Response body | ID de la observacion creada (referencia para tracking) |
| `channel` | string | Response body | Canal utilizado: email, sms, whatsapp |
| `recipient` | string | Response body | Destinatario del mensaje |
| `status` | string | Response body | Siempre `"queued"` (despacho es asincrono, RN-OPS-17) |
| observacion | — | `application_observations` | Registro con `observation_type='message'` (RN-OPS-16) |
| `MessageSentEvent` | — | RabbitMQ exchange | Evento con cuerpo ya resuelto para Notification Service (RN-OPS-15) |

#### Errores Tipados

| Codigo | Causa | Condicion de disparo | Respuesta esperada |
|--------|-------|---------------------|--------------------|
| `OPS_APPLICATION_NOT_FOUND` | Solicitud inexistente | `application_id` no existe o no pertenece al tenant (RLS) | HTTP 404 `{ code, message }` |
| `OPS_UNAUTHORIZED` | Sin permiso | JWT no contiene `p_ops_msg_send` | HTTP 403 `{ code, message }` |
| `OPS_TEMPLATE_NOT_FOUND` | Plantilla inexistente | Config Service retorna 404 para `template_id` | HTTP 422 `{ code, message }` |
| `OPS_TEMPLATE_CHANNEL_MISMATCH` | Canal no coincide con plantilla | `template.channel` != `request.channel` | HTTP 422 `{ code, message, detail: { expected, received } }` |
| `OPS_VALIDATION_ERROR` | Datos de entrada invalidos | `recipient` formato invalido, `channel` no es enum valido, `body_override` excede 5000 chars | HTTP 422 `{ code, message, errors[] }` |
| `OPS_EMAIL_SUBJECT_REQUIRED` | Subject obligatorio para email | `channel=email` y no hay subject en request ni en template | HTTP 422 `{ code, message }` |
| `OPS_CONFIG_UNAVAILABLE` | Config Service no responde | Timeout (>5s) o error HTTP en llamada sync a Config | HTTP 503 `{ code, message }` |

#### Casos Especiales y Variantes

- **body_override:** Si el operador edita el cuerpo antes de enviar, se envia `body_override`. Operations no resuelve variables en este caso; el cuerpo se usa tal cual (RN-OPS-18).
- **Plantilla sin variables:** Si la plantilla no contiene `{{...}}`, el cuerpo se usa como esta despues de la "resolucion" (que no encuentra nada que reemplazar). No es un error.
- **Variable sin dato:** Si una variable de plantilla referencia un dato null (ej: `{{plan}}` cuando `plan_name` es null), se reemplaza por cadena vacia sin error.
- **Solicitante sin contacto del canal:** El `recipient` se envia desde el frontend. Si el solicitante no tiene email/phone registrado, el operador puede ingresar manualmente. No se valida contra contactos del solicitante.
- **Multiples envios:** Cada envio es independiente (un mensaje = un destinatario = un canal). Para enviar por multiples canales, el operador repite la accion.
- **Estado de solicitud:** Se puede enviar mensaje en cualquier estado de la solicitud. No se valida `status`.
- **Despacho asincrono:** Operations NO espera confirmacion del proveedor externo. El status retornado es siempre `"queued"`. El resultado final (sent/failed) queda en `notification_log` de SA.notification (RN-OPS-17).
- **Subject para email:** Se resuelve con precedencia: `request.subject` > `template.subject` resuelto > error 422. Para SMS/WhatsApp, subject es null.

#### Impacto en Modelo de Datos

| Entidad | Operacion | Campo(s) afectado(s) | Evento |
|---------|-----------|---------------------|--------|
| `applications` | SELECT | id, code, status, product_name, plan_name, applicant_id | — |
| `applicants` | SELECT | first_name, last_name | — |
| `application_observations` | INSERT | id, application_id, observation_type, content, user_id, user_name, created_at | — |
| `notification_templates` (SA.config) | SELECT (sync HTTP) | id, title, subject, content, channel | — |
| — | — | — | MessageSentEvent (RabbitMQ) |

**Cambio en modelo de datos:** Se agrega campo `observation_type` (enum: `manual | message`, default `manual`) a `application_observations`. Ver actualizacion en `05_modelo_datos.md`.

#### Criterios de Aceptacion (Gherkin)

```gherkin
Scenario: Enviar email con plantilla y variables resueltas
  Given existe una solicitud "SOL-2026-001" con solicitante first_name="Juan", last_name="Perez"
  And existe una plantilla email id=:tid con content="Hola {{nombre}} {{apellido}}, su solicitud {{codigo_solicitud}} fue recibida."
  And el usuario tiene permiso p_ops_msg_send
  When envio POST /applications/:id/messages con channel="email", template_id=:tid, recipient="juan@mail.com", subject="Bienvenida"
  Then recibo HTTP 202 con status="queued" y message_id
  And se crea application_observation con observation_type="message" y content contiene "[email]"
  And se publica MessageSentEvent con resolved_body="Hola Juan Perez, su solicitud SOL-2026-001 fue recibida."

Scenario: Enviar SMS con body_override (operador edita cuerpo)
  Given existe una solicitud con solicitante
  And el usuario tiene permiso p_ops_msg_send
  When envio POST /applications/:id/messages con channel="sms", template_id=:tid, recipient="+5491155001234", body_override="Texto personalizado por el operador"
  Then recibo HTTP 202
  And MessageSentEvent.resolved_body = "Texto personalizado por el operador"

Scenario: Plantilla inexistente retorna 422
  When envio POST /applications/:id/messages con template_id=uuid_inexistente
  Then recibo HTTP 422 con codigo OPS_TEMPLATE_NOT_FOUND

Scenario: Canal no coincide con plantilla retorna 422
  Given existe una plantilla con channel="email"
  When envio POST /applications/:id/messages con channel="sms" y esa template_id
  Then recibo HTTP 422 con codigo OPS_TEMPLATE_CHANNEL_MISMATCH

Scenario: Config Service no disponible retorna 503
  Given Config Service no responde (timeout)
  When envio POST /applications/:id/messages
  Then recibo HTTP 503 con codigo OPS_CONFIG_UNAVAILABLE

Scenario: Email sin subject en request ni en template retorna 422
  Given existe una plantilla email con subject=null
  When envio POST /applications/:id/messages con channel="email" sin subject
  Then recibo HTTP 422 con codigo OPS_EMAIL_SUBJECT_REQUIRED

Scenario: Variable sin dato se reemplaza por cadena vacia
  Given solicitud con plan_name=null
  And plantilla con content="Plan: {{plan}}"
  When envio POST /applications/:id/messages
  Then MessageSentEvent.resolved_body = "Plan: "
```

#### Trazabilidad de Pruebas

| Test ID | Tipo | Descripcion |
|---------|------|-------------|
| TP-OPS-15-01 | Positivo | Enviar email con variables resueltas retorna 202 |
| TP-OPS-15-02 | Positivo | Enviar SMS con body_override retorna 202 |
| TP-OPS-15-03 | Positivo | Enviar WhatsApp con recipient manual retorna 202 |
| TP-OPS-15-04 | Positivo | Observacion auto-creada con observation_type=message |
| TP-OPS-15-05 | Positivo | Variable sin dato se reemplaza por cadena vacia |
| TP-OPS-15-06 | Negativo | 404 para solicitud inexistente |
| TP-OPS-15-07 | Negativo | 422 para plantilla inexistente (OPS_TEMPLATE_NOT_FOUND) |
| TP-OPS-15-08 | Negativo | 422 para canal no coincide (OPS_TEMPLATE_CHANNEL_MISMATCH) |
| TP-OPS-15-09 | Negativo | 422 para recipient formato invalido |
| TP-OPS-15-10 | Negativo | 422 email sin subject (OPS_EMAIL_SUBJECT_REQUIRED) |
| TP-OPS-15-11 | Negativo | 503 cuando Config Service no disponible |
| TP-OPS-15-12 | Negativo | 403 sin permiso p_ops_msg_send |
| TP-OPS-15-13 | Integracion | MessageSentEvent publicado con cuerpo resuelto |
| TP-OPS-15-14 | Integracion | Observacion visible en detalle solicitud (RF-OPS-06) |
| TP-OPS-15-15 | E2E | Dialogo enviar mensaje → seleccionar canal y plantilla → enviar → confirmacion + observacion visible |

#### Sin Ambiguedades

- **Supuestos prohibidos:** No se asume que Operations espera confirmacion del proveedor externo (siempre 202). No se asume que Operations despacha el mensaje (Notification Service lo hace). No se asume que las plantillas se cachean en Operations. No se asume que `recipient` debe coincidir con un contacto existente del solicitante. No se asume que el frontend resuelve variables (Operations las resuelve server-side).
- **Decisiones cerradas:** Operations resuelve variables antes de publicar (D-OPS-03). Observacion siempre se crea al enviar (RN-OPS-16). body_override anula resolucion (RN-OPS-18). Un envio = un destinatario = un canal. Subject: request > template > error.
- **Consolidacion de RF candidatos:** FL-OPS-03 propuso RF-OPS-15 a RF-OPS-18. Consolidados: RF-OPS-16 (resolucion) y RF-OPS-17 (auto-observacion) son pasos internos, no comportamientos separados. RF-OPS-18 (despacho) es responsabilidad de Notification Service, fuera de alcance.
- **Fuera de alcance explicito:** CRUD de plantillas de notificacion (Config Service). Despacho al proveedor externo (Notification Service). Retry de envios fallidos (MassTransit en Notification Service). Consulta de estado de notificacion (SA.notification). Envio masivo/batch. Adjuntos en mensajes. Preview con variables resueltas (preview es client-side con variables sin resolver).
- **Dependencias externas explicitas:** Config Service sync HTTP para lookup de plantilla (timeout 5s). RabbitMQ para MessageSentEvent. No hay dependencia sync con Notification Service.
- **TODO explicitos = 0**

---

### RF-OPS-12 — Preliquidacion con filtros y calculo estimado

#### Execution Sheet

| Campo | Valor |
|-------|-------|
| **ID** | RF-OPS-12 |
| **Titulo** | Preliquidacion con filtros y calculo estimado |
| **Actor(es)** | Operador, Admin Entidad, Super Admin |
| **Prioridad** | Alta |
| **Severidad** | P0 |
| **Flujo origen** | FL-OPS-02 seccion 6 (Preliquidacion) |
| **HU origen** | HU035 |

#### Precondiciones

| # | Condicion | Detalle |
|---|-----------|---------|
| 1 | Operations Service operativo | SA.operations accesible |
| 2 | Usuario autenticado | JWT con permiso `p_ops_settlement_create` |
| 3 | Catalog Service disponible | Necesario para lookup de commission_plans |
| 4 | Solicitudes aprobadas existen | Al menos una application con `status = approved` en el rango |

#### Entradas

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| `tenant_id` | uuid | Si | JWT claim | UUID valido | — |
| `entity_id` | uuid | No | Body JSON | UUID valido, debe existir en tenant | — |
| `date_from` | date | Si | Body JSON | Formato ISO 8601, no futuro | — |
| `date_to` | date | Si | Body JSON | Formato ISO 8601, >= date_from | — |

#### Proceso (Happy Path)

| # | Paso | Responsable |
|---|------|-------------|
| 1 | Recibir `POST /settlements/preview` con body `{ entity_id?, date_from, date_to }` | API Gateway |
| 2 | Validar JWT y permiso `p_ops_settlement_create` | Operations Service |
| 3 | Ejecutar `SET LOCAL app.current_tenant = :tenant_id` | Operations Service (RLS) |
| 4 | `SELECT applications WHERE status = 'approved' AND created_at BETWEEN :date_from AND :date_to` (+ filtro entity_id si presente) | SA.operations |
| 5 | Si lista vacia → retornar `200 { items: [], totals_by_currency: [] }` | Operations Service |
| 6 | Extraer `product_id` y `plan_id` unicos de las solicitudes | Operations Service |
| 7 | `GET /commission-plans?product_ids=[...]&plan_ids=[...]` al Catalog Service (batch lookup, timeout 5s) | Operations Service → Catalog |
| 8 | Para cada solicitud, calcular monto segun formula del plan (RN-OPS-23) | Operations Service |
| 9 | Agrupar totales por moneda (RN-OPS-24) | Operations Service |
| 10 | Retornar `200 { items[], totals_by_currency[] }` | Operations Service |

#### Salidas

| Campo | Tipo | Destino | Efecto observable |
|-------|------|---------|-------------------|
| `items[]` | array | Response body | Lista de solicitudes con desglose: `application_id`, `app_code`, `applicant_name`, `product_name`, `plan_name`, `commission_type`, `commission_value`, `calculated_amount`, `currency`, `formula_description` |
| `totals_by_currency[]` | array | Response body | `[{ currency, total_amount, item_count }]` |

#### Errores Tipados

| Codigo | Causa | Condicion | Respuesta |
|--------|-------|-----------|-----------|
| 401 | No autenticado | JWT ausente o expirado | `401 { code: "OPS_UNAUTHENTICATED" }` |
| 403 | Sin permiso | JWT sin `p_ops_settlement_create` | `403 { code: "OPS_UNAUTHORIZED" }` |
| 422 | date_from > date_to | Rango invertido | `422 { code: "OPS_INVALID_DATE_RANGE", field: "date_from" }` |
| 422 | date_from en futuro | Fecha futura | `422 { code: "OPS_FUTURE_DATE", field: "date_from" }` |
| 503 | Catalog no disponible | Timeout o error en lookup de commission_plans | `503 { code: "OPS_CATALOG_UNAVAILABLE" }` |

#### Casos Especiales y Variantes

- **Sin solicitudes candidatas:** Retorna 200 con `items: []` y `totals_by_currency: []`. No es error.
- **Solicitudes sin commission_plan:** Si un producto/plan no tiene commission_plan configurado en Catalog, la solicitud se incluye con `commission_type: null`, `calculated_amount: 0`, `formula_description: "Sin plan de comision configurado"`. No bloquea la preliquidacion.
- **Multiples monedas:** Si hay solicitudes en ARS y USD, se calculan totales separados por moneda.
- **Filtro por entidad:** Si `entity_id` se provee, solo se incluyen solicitudes de esa entidad. Si no, se incluyen todas las del tenant.

#### Impacto en Modelo de Datos

- **Lectura:** `applications` (filtro por status, fechas, entity_id), Catalog Service (commission_plans lookup).
- **Escritura:** Ninguna. Preliquidacion es solo lectura.

#### Criterios de Aceptacion (Gherkin)

```gherkin
Feature: Preliquidacion con filtros y calculo estimado

  Scenario: Preliquidacion exitosa con solicitudes aprobadas
    Given existen 5 solicitudes en status "approved" creadas en el rango 2026-01-01 a 2026-01-31
    And cada solicitud tiene un commission_plan configurado en Catalog
    When el operador envia POST /settlements/preview con date_from="2026-01-01" y date_to="2026-01-31"
    Then el sistema retorna 200
    And items contiene 5 elementos con desglose de comision
    And totals_by_currency contiene al menos un elemento con total_amount > 0

  Scenario: Preliquidacion sin solicitudes candidatas
    Given no existen solicitudes en status "approved" en el rango seleccionado
    When el operador envia POST /settlements/preview
    Then el sistema retorna 200 con items=[] y totals_by_currency=[]

  Scenario: Catalog no disponible
    Given Catalog Service no responde dentro de 5 segundos
    When el operador envia POST /settlements/preview
    Then el sistema retorna 503 con code="OPS_CATALOG_UNAVAILABLE"

  Scenario Outline: Validacion de rango de fechas
    When el operador envia POST /settlements/preview con date_from=<from> y date_to=<to>
    Then el sistema retorna <status> con code=<code>

    Examples:
      | from       | to         | status | code               |
      | 2026-02-01 | 2026-01-01 | 422    | OPS_INVALID_DATE_RANGE |
      | 2027-06-01 | 2027-06-30 | 422    | OPS_FUTURE_DATE        |
```

#### Trazabilidad de Tests

| ID Test | Tipo | Descripcion |
|---------|------|-------------|
| TP-OPS-12-01 | Positivo | Preview exitoso con solicitudes aprobadas y comisiones calculadas |
| TP-OPS-12-02 | Positivo | Preview con filtro entity_id retorna solo solicitudes de esa entidad |
| TP-OPS-12-03 | Positivo | Preview sin resultados retorna 200 con arrays vacios |
| TP-OPS-12-04 | Positivo | Preview con multiples monedas genera totals separados |
| TP-OPS-12-05 | Negativo | 401 sin JWT |
| TP-OPS-12-06 | Negativo | 403 sin permiso p_ops_settlement_create |
| TP-OPS-12-07 | Negativo | 422 date_from > date_to |
| TP-OPS-12-08 | Negativo | 422 date_from en futuro |
| TP-OPS-12-09 | Negativo | 503 Catalog no disponible (timeout) |
| TP-OPS-12-10 | Negativo | Solicitud sin commission_plan incluida con amount=0 |
| TP-OPS-12-11 | Integracion | Lookup batch a Catalog con product_ids unicos |
| TP-OPS-12-12 | Integracion | RLS filtra por tenant_id |
| TP-OPS-12-13 | Integracion | Calculo correcto para cada tipo de formula (fixed, %, % total) |
| TP-OPS-12-14 | E2E | Filtros → preview → grilla con desglose y totales por moneda |

#### Sin Ambiguedades

- **Supuestos prohibidos:** No se asume que la preliquidacion persiste datos (es solo lectura). No se asume que el frontend cachea el resultado del preview (debe re-consultar al confirmar). No se asume que Catalog retorna comisiones pre-calculadas (Operations calcula). No se asume que solicitudes sin plan de comision se excluyen (se incluyen con amount=0).
- **Decisiones cerradas:** Preview es stateless, no crea ningun registro. Solicitudes sin commission_plan se incluyen con amount=0 y formula descriptiva. Catalog lookup es batch (un request con multiples product_ids). Timeout de Catalog: 5 segundos.
- **Fuera de alcance explicito:** Persistencia del resultado de preview. Paginacion en preview (se retornan todas las candidatas). Filtro por producto o plan en preview.
- **Dependencias externas explicitas:** Catalog Service sync HTTP para commission_plans lookup (timeout 5s, retry 1).
- **TODO explicitos = 0**

---

### RF-OPS-13 — Confirmacion de liquidacion con lock optimista

#### Execution Sheet

| Campo | Valor |
|-------|-------|
| **ID** | RF-OPS-13 |
| **Titulo** | Confirmacion de liquidacion con lock optimista |
| **Actor(es)** | Operador, Admin Entidad, Super Admin |
| **Prioridad** | Alta |
| **Severidad** | P0 |
| **Flujo origen** | FL-OPS-02 seccion 6 (Confirmacion) |
| **HU origen** | HU035 |

#### Precondiciones

| # | Condicion | Detalle |
|---|-----------|---------|
| 1 | Operations Service operativo | SA.operations accesible |
| 2 | Usuario autenticado | JWT con permiso `p_ops_settlement_create` |
| 3 | Preliquidacion ejecutada | El operador ya vio el preview y decide confirmar |
| 4 | application_ids no vacio | Al menos una solicitud a liquidar |

#### Entradas

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| `tenant_id` | uuid | Si | JWT claim | UUID valido | — |
| `entity_id` | uuid | No | Body JSON | UUID valido | — |
| `date_from` | date | Si | Body JSON | Formato ISO 8601 | — |
| `date_to` | date | Si | Body JSON | Formato ISO 8601, >= date_from | — |
| `application_ids` | uuid[] | Si | Body JSON | Array no vacio, cada UUID valido | RN-OPS-19 |

#### Proceso (Happy Path)

| # | Paso | Responsable |
|---|------|-------------|
| 1 | Recibir `POST /settlements` con body `{ entity_id?, date_from, date_to, application_ids[] }` | API Gateway |
| 2 | Validar JWT y permiso `p_ops_settlement_create` | Operations Service |
| 3 | Ejecutar `SET LOCAL app.current_tenant = :tenant_id` | Operations Service (RLS) |
| 4 | `BEGIN TRANSACTION` | SA.operations |
| 5 | `SELECT * FROM applications WHERE id IN (:ids) AND status = 'approved' FOR UPDATE` (RN-OPS-19) | SA.operations |
| 6 | Verificar que el count de rows retornadas == count de application_ids. Si no coincide → ROLLBACK, 409 con lista de IDs en conflicto | Operations Service |
| 7 | Lookup commission_plans desde Catalog (batch, mismo que preview) | Operations Service → Catalog |
| 8 | Calcular monto por solicitud segun formula (RN-OPS-23) | Operations Service |
| 9 | `INSERT INTO settlements (id, tenant_id, settled_at, settled_by, settled_by_name, operation_count)` | SA.operations |
| 10 | Calcular totales por moneda (RN-OPS-24). `INSERT INTO settlement_totals` por cada moneda | SA.operations |
| 11 | `INSERT INTO settlement_items` por cada solicitud con desglose de comision | SA.operations |
| 12 | `UPDATE applications SET status = 'settled', updated_at = NOW(), updated_by = :user_id WHERE id IN (:ids)` (RN-OPS-22) | SA.operations |
| 13 | `INSERT INTO trace_events` por cada solicitud (action='settle', state='settled') | SA.operations |
| 14 | `COMMIT` | SA.operations |
| 15 | Generar archivo Excel asincrono (RF-OPS-14) | Operations Service |
| 16 | Publicar `ApplicationStatusChangedEvent` x N (approved→settled) | Operations → RabbitMQ |
| 17 | Publicar `CommissionsSettledEvent { settlement_id, items_count, totals[] }` | Operations → RabbitMQ |
| 18 | Retornar `201 { settlement_id, operation_count, totals_by_currency[], excel_url }` | Operations Service |

#### Salidas

| Campo | Tipo | Destino | Efecto observable |
|-------|------|---------|-------------------|
| `settlement_id` | uuid | Response body | ID del settlement creado |
| `operation_count` | integer | Response body | Cantidad de solicitudes liquidadas |
| `totals_by_currency[]` | array | Response body | Totales por moneda |
| `excel_url` | string? | Response body | URL relativa del Excel o null si fallo generacion |
| `ApplicationStatusChangedEvent` x N | evento | RabbitMQ | Un evento por cada solicitud transicionada |
| `CommissionsSettledEvent` | evento | RabbitMQ | Evento resumen de la liquidacion |

#### Errores Tipados

| Codigo | Causa | Condicion | Respuesta |
|--------|-------|-----------|-----------|
| 401 | No autenticado | JWT ausente o expirado | `401 { code: "OPS_UNAUTHENTICATED" }` |
| 403 | Sin permiso | JWT sin `p_ops_settlement_create` | `403 { code: "OPS_UNAUTHORIZED" }` |
| 409 | Conflicto de concurrencia | Alguna solicitud ya no esta en approved | `409 { code: "OPS_SETTLEMENT_CONFLICT", conflicts: [{ application_id, current_status }] }` |
| 422 | application_ids vacio | Array vacio o ausente | `422 { code: "OPS_EMPTY_APPLICATION_IDS" }` |
| 422 | application_id no existe | UUID no encontrado en tenant | `422 { code: "OPS_APPLICATION_NOT_FOUND", application_id }` |
| 422 | date_from > date_to | Rango invertido | `422 { code: "OPS_INVALID_DATE_RANGE" }` |
| 503 | Catalog no disponible | Timeout en lookup de commission_plans | `503 { code: "OPS_CATALOG_UNAVAILABLE" }` |

#### Casos Especiales y Variantes

- **Conflicto parcial:** Si de 10 solicitudes 2 cambiaron de estado, se hace ROLLBACK completo. No se liquida parcialmente. El 409 lista los IDs en conflicto para que el operador vuelva a preliquidar.
- **Excel falla:** Si la generacion de Excel falla, el settlement ya fue commiteado. `excel_url` queda NULL (RN-OPS-21). El 201 se retorna igual con `excel_url: null`.
- **Solicitudes de multiples entidades:** Si no se filtra por entity_id, el settlement puede incluir solicitudes de distintas entidades del mismo tenant.
- **Trace events:** Cada solicitud recibe un trace_event con `action='settle'`, `state='settled'`, `user_name` del JWT. El `trace_event_details` incluye `settlement_id` como referencia.

#### Impacto en Modelo de Datos

- **Lectura:** `applications` (FOR UPDATE), Catalog (commission_plans).
- **Escritura:** `settlements` (INSERT), `settlement_totals` (INSERT), `settlement_items` (INSERT), `applications` (UPDATE status→settled), `trace_events` (INSERT), `trace_event_details` (INSERT).
- **Eventos:** `ApplicationStatusChangedEvent` x N, `CommissionsSettledEvent` x 1.

#### Criterios de Aceptacion (Gherkin)

```gherkin
Feature: Confirmacion de liquidacion con lock optimista

  Scenario: Confirmacion exitosa
    Given existen 5 solicitudes en status "approved" con commission_plans configurados
    And el operador ya ejecuto preliquidacion
    When el operador envia POST /settlements con application_ids de las 5 solicitudes
    Then el sistema retorna 201
    And se crea un settlement con operation_count=5
    And las 5 solicitudes pasan a status "settled"
    And se crean 5 settlement_items con desglose de comision
    And se crean settlement_totals agrupados por moneda
    And se crean 5 trace_events (approved→settled)
    And se publican 5 ApplicationStatusChangedEvent
    And se publica 1 CommissionsSettledEvent

  Scenario: Conflicto de concurrencia — solicitud cambio de estado
    Given existen 3 solicitudes en status "approved"
    And otra sesion transiciona la solicitud #2 a "cancelled"
    When el operador envia POST /settlements con los 3 application_ids
    Then el sistema retorna 409 con code="OPS_SETTLEMENT_CONFLICT"
    And conflicts contiene application_id de solicitud #2 con current_status="cancelled" (code="OPS_SETTLEMENT_CONFLICT")
    And no se crea ningun settlement
    And ninguna solicitud cambia de estado

  Scenario: application_ids vacio
    When el operador envia POST /settlements con application_ids=[]
    Then el sistema retorna 422 con code="OPS_EMPTY_APPLICATION_IDS"

  Scenario: Excel falla pero settlement se crea
    Given existen solicitudes aprobadas y filesystem no disponible
    When el operador confirma la liquidacion
    Then el sistema retorna 201 con excel_url=null
    And el settlement existe con todos sus items y totales
```

#### Trazabilidad de Tests

| ID Test | Tipo | Descripcion |
|---------|------|-------------|
| TP-OPS-13-01 | Positivo | Confirmacion exitosa con N solicitudes aprobadas |
| TP-OPS-13-02 | Positivo | Settlement con multiples monedas genera totals separados |
| TP-OPS-13-03 | Positivo | Excel generado y excel_url presente en respuesta |
| TP-OPS-13-04 | Negativo | 409 conflicto: solicitud cambio de estado entre preview y confirm |
| TP-OPS-13-05 | Negativo | 409 conflicto parcial: rollback completo, ninguna liquidada |
| TP-OPS-13-06 | Negativo | 422 application_ids vacio |
| TP-OPS-13-07 | Negativo | 422 application_id no existe en tenant |
| TP-OPS-13-08 | Negativo | 401 sin JWT |
| TP-OPS-13-09 | Negativo | 403 sin permiso |
| TP-OPS-13-10 | Negativo | 503 Catalog no disponible |
| TP-OPS-13-11 | Integracion | FOR UPDATE bloquea filas concurrentes |
| TP-OPS-13-12 | Integracion | Trace events creados por cada solicitud con settlement_id en details |
| TP-OPS-13-13 | Integracion | ApplicationStatusChangedEvent publicado por cada solicitud |
| TP-OPS-13-14 | Integracion | CommissionsSettledEvent publicado con totales |
| TP-OPS-13-15 | E2E | Preview → confirmar → settlement creado → solicitudes en settled → redirect a historial |

#### Sin Ambiguedades

- **Supuestos prohibidos:** No se asume que el confirm reutiliza datos del preview (re-calcula comisiones). No se asume liquidacion parcial si hay conflicto (es todo o nada). No se asume que el Excel se genera dentro de la transaccion (se genera despues del COMMIT). No se asume que el frontend envia montos calculados (el backend calcula).
- **Decisiones cerradas:** Lock optimista con SELECT FOR UPDATE (D-OPS-02). Rollback completo ante cualquier conflicto. Excel generacion best-effort post-commit (RN-OPS-21). Settlement inmutable (RN-OPS-20). Transicion approved→settled solo via este RF (RN-OPS-22).
- **Fuera de alcance explicito:** Liquidacion parcial. Reverso de liquidacion. Edicion de settlement post-creacion. Re-generacion de Excel.
- **Dependencias externas explicitas:** Catalog Service sync HTTP (timeout 5s). RabbitMQ para eventos async. Filesystem para Excel (best-effort).
- **TODO explicitos = 0**

---

### RF-OPS-14 — Generacion de reporte Excel post-confirmacion

#### Execution Sheet

| Campo | Valor |
|-------|-------|
| **ID** | RF-OPS-14 |
| **Titulo** | Generacion de reporte Excel post-confirmacion |
| **Actor(es)** | Operations Service (automatico, disparado por RF-OPS-13) |
| **Prioridad** | Alta |
| **Severidad** | P1 |
| **Flujo origen** | FL-OPS-02 seccion 6 (Excel), seccion 8c |
| **HU origen** | HU035 |

#### Precondiciones

| # | Condicion | Detalle |
|---|-----------|---------|
| 1 | Settlement commiteado | RF-OPS-13 completo con COMMIT exitoso |
| 2 | Filesystem accesible | `{STORAGE_ROOT}/{tenant_id}/settlements/` con permisos de escritura |

#### Entradas

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| `settlement_id` | uuid | Si | Interno (post-commit) | Settlement debe existir | RN-OPS-20 |
| `tenant_id` | uuid | Si | Contexto de la transaccion | UUID valido | — |
| `settlement_items[]` | array | Si | Datos en memoria del confirm | Items con desglose de comision | — |
| `settlement_totals[]` | array | Si | Datos en memoria del confirm | Totales por moneda | — |

#### Proceso (Happy Path)

| # | Paso | Responsable |
|---|------|-------------|
| 1 | Recibir datos del settlement recien creado (post-commit de RF-OPS-13) | Operations Service |
| 2 | Construir ruta: `{STORAGE_ROOT}/{tenant_id}/settlements/{yyyy}/{MM}/{settlement_id}.xlsx` (RN-OPS-25) | Operations Service |
| 3 | Crear directorio si no existe (`{yyyy}/{MM}/`) | Operations Service |
| 4 | Generar Excel con hojas: "Resumen" (totales por moneda, metadata) y "Detalle" (items con desglose) | Operations Service |
| 5 | Escribir archivo en filesystem | Operations Service |
| 6 | `UPDATE settlements SET excel_url = :ruta_relativa WHERE id = :settlement_id` | SA.operations |

#### Salidas

| Campo | Tipo | Destino | Efecto observable |
|-------|------|---------|-------------------|
| `excel_url` | string | `settlements.excel_url` | Ruta relativa del archivo generado |
| Archivo .xlsx | binary | Filesystem | Archivo Excel disponible para descarga |

#### Errores Tipados

| Codigo | Causa | Condicion | Respuesta |
|--------|-------|-----------|-----------|
| — | Filesystem no disponible | Error I/O al escribir | Log error, `excel_url` queda NULL (RN-OPS-21). No retorna error al usuario |
| — | Directorio no creado | Permisos insuficientes | Log error, `excel_url` queda NULL |

#### Casos Especiales y Variantes

- **Fallo de escritura:** Settlement ya fue commiteado. Se loguea el error y `excel_url` queda NULL. La respuesta del confirm (RF-OPS-13) incluye `excel_url: null`. No hay retry automatico.
- **Contenido del Excel:** Hoja "Resumen": settlement_id, fecha, ejecutor, operation_count, totales por moneda. Hoja "Detalle": una fila por item con app_code, applicant_name, product_name, plan_name, commission_type, value, amount, currency, formula.
- **Archivo inmutable:** Una vez generado, el Excel no se sobreescribe ni elimina (RN-OPS-25).

#### Impacto en Modelo de Datos

- **Lectura:** Datos en memoria del settlement recien creado.
- **Escritura:** `settlements.excel_url` (UPDATE unico). Filesystem (INSERT archivo).

#### Criterios de Aceptacion (Gherkin)

```gherkin
Feature: Generacion de reporte Excel post-confirmacion

  Scenario: Excel generado exitosamente
    Given se confirmo un settlement con 5 items en ARS
    When se ejecuta la generacion de Excel
    Then se crea un archivo en {STORAGE_ROOT}/{tenant_id}/settlements/{yyyy}/{MM}/{settlement_id}.xlsx
    And settlements.excel_url se actualiza con la ruta relativa
    And el Excel contiene hoja "Resumen" con totales
    And el Excel contiene hoja "Detalle" con 5 filas

  Scenario: Filesystem no disponible
    Given se confirmo un settlement pero el filesystem no esta accesible
    When se intenta generar el Excel
    Then se loguea el error
    And settlements.excel_url permanece NULL
    And el settlement existe con todos sus datos intactos

  Scenario: Directorio no existe
    Given la ruta {yyyy}/{MM}/ no existe
    When se genera el Excel
    Then se crea el directorio automaticamente
    And se escribe el archivo correctamente
```

#### Trazabilidad de Tests

| ID Test | Tipo | Descripcion |
|---------|------|-------------|
| TP-OPS-14-01 | Positivo | Excel generado con estructura correcta (2 hojas) |
| TP-OPS-14-02 | Positivo | Ruta deterministica segun tenant, ano, mes, settlement_id |
| TP-OPS-14-03 | Positivo | Directorio creado automaticamente si no existe |
| TP-OPS-14-04 | Negativo | Filesystem no disponible: excel_url queda NULL, settlement intacto |
| TP-OPS-14-05 | Negativo | Permisos insuficientes: log error, excel_url NULL |
| TP-OPS-14-06 | Negativo | Settlement no encontrado (edge case): log error, no crash |
| TP-OPS-14-07 | Integracion | excel_url actualizado en settlements despues de generacion |
| TP-OPS-14-08 | Integracion | Contenido Excel coincide con settlement_items y totals en DB |
| TP-OPS-14-09 | Integracion | Archivo no se sobreescribe si ya existe (RN-OPS-25) |
| TP-OPS-14-10 | E2E | Confirm → Excel generado → descarga exitosa del archivo |

#### Sin Ambiguedades

- **Supuestos prohibidos:** No se asume que la generacion ocurre dentro de la transaccion DB (es post-commit). No se asume retry automatico si falla. No se asume que el Excel se almacena en blob storage (es filesystem local). No se asume que el Excel puede ser regenerado (inmutable, RN-OPS-25).
- **Decisiones cerradas:** Best-effort (RN-OPS-21). Ruta deterministica (RN-OPS-25). Dos hojas: Resumen y Detalle. Post-commit (fuera de la transaccion principal).
- **Fuera de alcance explicito:** Re-generacion de Excel. Almacenamiento en cloud/blob. Compresion/zip. Envio por email del Excel.
- **Dependencias externas explicitas:** Filesystem local con permisos de escritura.
- **TODO explicitos = 0**

---

### RF-OPS-16 — Listar historial de liquidaciones con filtros

#### Execution Sheet

| Campo | Valor |
|-------|-------|
| **ID** | RF-OPS-16 |
| **Titulo** | Listar historial de liquidaciones con filtros |
| **Actor(es)** | Operador, Admin Entidad, Super Admin, Consulta, Auditor |
| **Prioridad** | Alta |
| **Severidad** | P1 |
| **Flujo origen** | FL-OPS-02 seccion 7 (Historial) |
| **HU origen** | HU036 |

#### Precondiciones

| # | Condicion | Detalle |
|---|-----------|---------|
| 1 | Operations Service operativo | SA.operations accesible |
| 2 | Usuario autenticado | JWT con permiso `p_ops_settlement_list` |

#### Entradas

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| `tenant_id` | uuid | Si | JWT claim | UUID valido | — |
| `date_from` | date | No | Query param | Formato ISO 8601 | — |
| `date_to` | date | No | Query param | >= date_from si ambos presentes | — |
| `settled_by` | uuid | No | Query param | UUID valido | — |
| `page` | integer | No | Query param | Default: 1, min: 1 | — |
| `page_size` | integer | No | Query param | Default: 20, max: 100 | — |
| `sort_by` | string | No | Query param | Campos: settled_at, operation_count. Default: settled_at | — |
| `sort_dir` | enum | No | Query param | asc, desc. Default: desc | — |

#### Proceso (Happy Path)

| # | Paso | Responsable |
|---|------|-------------|
| 1 | Recibir `GET /settlements` con query params | API Gateway |
| 2 | Validar JWT y permiso `p_ops_settlement_list` | Operations Service |
| 3 | Ejecutar `SET LOCAL app.current_tenant = :tenant_id` | Operations Service (RLS) |
| 4 | `SELECT settlements` con filtros + `JOIN settlement_totals` para incluir totales inline | SA.operations |
| 5 | Aplicar paginacion y ordenamiento | SA.operations |
| 6 | Retornar `200 { items[], total, page, page_size }` | Operations Service |

#### Salidas

| Campo | Tipo | Destino | Efecto observable |
|-------|------|---------|-------------------|
| `items[]` | array | Response body | `[{ id, settled_at, settled_by_name, operation_count, excel_url, totals: [{ currency, total_amount }] }]` |
| `total` | integer | Response body | Total de registros sin paginacion |
| `page` | integer | Response body | Pagina actual |
| `page_size` | integer | Response body | Tamano de pagina |

#### Errores Tipados

| Codigo | Causa | Condicion | Respuesta |
|--------|-------|-----------|-----------|
| 401 | No autenticado | JWT ausente o expirado | `401 { code: "OPS_UNAUTHENTICATED" }` |
| 403 | Sin permiso | JWT sin `p_ops_settlement_list` | `403 { code: "OPS_UNAUTHORIZED" }` |
| 422 | date_from > date_to | Rango invertido | `422 { code: "OPS_INVALID_DATE_RANGE" }` |

#### Casos Especiales y Variantes

- **Sin resultados:** Retorna 200 con `items: []`, `total: 0`.
- **Filtro por ejecutor:** `settled_by` filtra por el UUID del usuario que confirmo la liquidacion.
- **Excel no disponible:** Si `excel_url` es NULL, la UI muestra icono de descarga deshabilitado.

#### Impacto en Modelo de Datos

- **Lectura:** `settlements` (paginado, filtrado), `settlement_totals` (JOIN).
- **Escritura:** Ninguna.

#### Criterios de Aceptacion (Gherkin)

```gherkin
Feature: Listar historial de liquidaciones con filtros

  Scenario: Listar liquidaciones exitoso
    Given existen 3 settlements en el tenant
    When el usuario envia GET /settlements
    Then el sistema retorna 200 con items de 3 liquidaciones
    And cada item incluye totals por moneda

  Scenario: Filtrar por rango de fechas
    Given existen settlements de enero y febrero
    When el usuario filtra date_from="2026-02-01" y date_to="2026-02-28"
    Then solo se retornan settlements de febrero

  Scenario: Filtrar por ejecutor
    Given existen settlements creados por usuario A y usuario B
    When el usuario filtra settled_by=usuario_A_id
    Then solo se retornan settlements de usuario A

  Scenario: Sin resultados
    Given no existen settlements en el rango seleccionado
    When el usuario envia GET /settlements con filtros
    Then el sistema retorna 200 con items=[] y total=0
```

#### Trazabilidad de Tests

| ID Test | Tipo | Descripcion |
|---------|------|-------------|
| TP-OPS-16-01 | Positivo | Listar settlements exitoso con totales por moneda inline |
| TP-OPS-16-02 | Positivo | Filtro por rango de fechas retorna solo settlements en rango |
| TP-OPS-16-03 | Positivo | Filtro por settled_by retorna solo settlements del ejecutor |
| TP-OPS-16-04 | Positivo | Paginacion y ordenamiento por settled_at desc |
| TP-OPS-16-05 | Negativo | 401 sin JWT |
| TP-OPS-16-06 | Negativo | 403 sin permiso p_ops_settlement_list |
| TP-OPS-16-07 | Negativo | 422 date_from > date_to |
| TP-OPS-16-08 | Negativo | Sin resultados retorna 200 con items vacio |
| TP-OPS-16-09 | Integracion | RLS filtra por tenant_id |
| TP-OPS-16-10 | Integracion | JOIN settlement_totals retorna totales correctos |
| TP-OPS-16-11 | E2E | Navegar a /liquidaciones → ver grilla con fechas, ejecutor, totales, icono Excel |

#### Sin Ambiguedades

- **Supuestos prohibidos:** No se asume que los totales se calculan on-the-fly (estan pre-calculados en settlement_totals). No se asume que el listado incluye items individuales (solo resumen + totales).
- **Decisiones cerradas:** Totales inline via JOIN (no lazy load). Paginacion server-side. Ordenamiento default: settled_at DESC.
- **Fuera de alcance explicito:** Exportacion Excel del listado. Busqueda full-text. Filtro por entidad en historial.
- **Dependencias externas explicitas:** Ninguna (solo SA.operations).
- **TODO explicitos = 0**

---

### RF-OPS-17 — Ver detalle de liquidacion con items

#### Execution Sheet

| Campo | Valor |
|-------|-------|
| **ID** | RF-OPS-17 |
| **Titulo** | Ver detalle de liquidacion con items |
| **Actor(es)** | Operador, Admin Entidad, Super Admin, Consulta, Auditor |
| **Prioridad** | Alta |
| **Severidad** | P1 |
| **Flujo origen** | FL-OPS-02 seccion 7 (Detalle) |
| **HU origen** | HU036 |

#### Precondiciones

| # | Condicion | Detalle |
|---|-----------|---------|
| 1 | Operations Service operativo | SA.operations accesible |
| 2 | Usuario autenticado | JWT con permiso `p_ops_settlement_list` |
| 3 | Settlement existe | El ID corresponde a un settlement del tenant |

#### Entradas

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| `tenant_id` | uuid | Si | JWT claim | UUID valido | — |
| `settlement_id` | uuid | Si | Path param | UUID valido, debe existir | — |

#### Proceso (Happy Path)

| # | Paso | Responsable |
|---|------|-------------|
| 1 | Recibir `GET /settlements/:id` | API Gateway |
| 2 | Validar JWT y permiso `p_ops_settlement_list` | Operations Service |
| 3 | Ejecutar `SET LOCAL app.current_tenant = :tenant_id` | Operations Service (RLS) |
| 4 | `SELECT settlement` + `JOIN settlement_totals` + `JOIN settlement_items` (+ JOIN applications para datos denormalizados) | SA.operations |
| 5 | Si no encontrado → 404 | Operations Service |
| 6 | Retornar `200 { settlement, totals[], items[] }` | Operations Service |

#### Salidas

| Campo | Tipo | Destino | Efecto observable |
|-------|------|---------|-------------------|
| `settlement` | object | Response body | `{ id, settled_at, settled_by_name, operation_count, excel_url }` |
| `totals[]` | array | Response body | `[{ currency, total_amount }]` |
| `items[]` | array | Response body | `[{ application_id, app_code, applicant_name, product_name, plan_name, commission_type, commission_value, calculated_amount, currency, formula_description }]` |

#### Errores Tipados

| Codigo | Causa | Condicion | Respuesta |
|--------|-------|-----------|-----------|
| 401 | No autenticado | JWT ausente o expirado | `401 { code: "OPS_UNAUTHENTICATED" }` |
| 403 | Sin permiso | JWT sin `p_ops_settlement_list` | `403 { code: "OPS_UNAUTHORIZED" }` |
| 404 | No encontrado | Settlement no existe o no pertenece al tenant | `404 { code: "OPS_SETTLEMENT_NOT_FOUND" }` |

#### Casos Especiales y Variantes

- **Items con datos de solicitud:** Cada item incluye datos denormalizados de la solicitud (app_code, applicant_name, product_name, plan_name) obtenidos via JOIN.
- **Excel no disponible:** Si `excel_url` es NULL, la UI deshabilita el boton de descarga.

#### Impacto en Modelo de Datos

- **Lectura:** `settlements`, `settlement_totals`, `settlement_items`, `applications` (JOIN para datos denormalizados).
- **Escritura:** Ninguna.

#### Criterios de Aceptacion (Gherkin)

```gherkin
Feature: Ver detalle de liquidacion con items

  Scenario: Detalle exitoso
    Given existe un settlement con 5 items y totales en ARS y USD
    When el usuario envia GET /settlements/:id
    Then el sistema retorna 200
    And settlement contiene metadata (settled_at, settled_by_name, operation_count)
    And totals contiene 2 elementos (ARS y USD)
    And items contiene 5 elementos con desglose de comision

  Scenario: Settlement no encontrado
    When el usuario envia GET /settlements/:id con un UUID inexistente
    Then el sistema retorna 404 con code="OPS_SETTLEMENT_NOT_FOUND"

  Scenario: Settlement de otro tenant
    Given existe un settlement del tenant A
    When un usuario del tenant B envia GET /settlements/:id
    Then el sistema retorna 404 (RLS lo oculta)
```

#### Trazabilidad de Tests

| ID Test | Tipo | Descripcion |
|---------|------|-------------|
| TP-OPS-17-01 | Positivo | Detalle exitoso con settlement, totals e items completos |
| TP-OPS-17-02 | Positivo | Items incluyen datos denormalizados de solicitud via JOIN |
| TP-OPS-17-03 | Positivo | Settlement con multiples monedas muestra totals separados |
| TP-OPS-17-04 | Negativo | 404 settlement no encontrado |
| TP-OPS-17-05 | Negativo | 404 settlement de otro tenant (RLS) |
| TP-OPS-17-06 | Negativo | 401 sin JWT |
| TP-OPS-17-07 | Negativo | 403 sin permiso |
| TP-OPS-17-08 | Integracion | JOIN correcto entre settlements, totals, items y applications |
| TP-OPS-17-09 | Integracion | RLS filtra por tenant_id |
| TP-OPS-17-10 | E2E | Click en fila de historial → detalle con resumen y grilla de items |

#### Sin Ambiguedades

- **Supuestos prohibidos:** No se asume que los items se cargan lazy (se cargan todos con el detalle). No se asume que el detalle incluye trace_events de las solicitudes (solo items de la liquidacion).
- **Decisiones cerradas:** Carga eager de totals e items. Datos de solicitud via JOIN (no lookup a otra tabla). Permiso de lectura reutiliza `p_ops_settlement_list`.
- **Fuera de alcance explicito:** Paginacion de items dentro del detalle. Edicion de settlement. Historial de trace_events de las solicitudes liquidadas.
- **Dependencias externas explicitas:** Ninguna (solo SA.operations).
- **TODO explicitos = 0**

---

### RF-OPS-18 — Descargar reporte Excel de liquidacion

#### Execution Sheet

| Campo | Valor |
|-------|-------|
| **ID** | RF-OPS-18 |
| **Titulo** | Descargar reporte Excel de liquidacion |
| **Actor(es)** | Operador, Admin Entidad, Super Admin, Consulta, Auditor |
| **Prioridad** | Media |
| **Severidad** | P2 |
| **Flujo origen** | FL-OPS-02 seccion 7 (Descarga) |
| **HU origen** | HU036 |

#### Precondiciones

| # | Condicion | Detalle |
|---|-----------|---------|
| 1 | Operations Service operativo | SA.operations accesible |
| 2 | Usuario autenticado | JWT con permiso `p_ops_settlement_list` |
| 3 | Settlement existe | El ID corresponde a un settlement del tenant |
| 4 | Excel generado | `settlements.excel_url` no es NULL |

#### Entradas

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| `tenant_id` | uuid | Si | JWT claim | UUID valido | — |
| `settlement_id` | uuid | Si | Path param | UUID valido, debe existir | — |

#### Proceso (Happy Path)

| # | Paso | Responsable |
|---|------|-------------|
| 1 | Recibir `GET /settlements/:id/download` | API Gateway |
| 2 | Validar JWT y permiso `p_ops_settlement_list` | Operations Service |
| 3 | Ejecutar `SET LOCAL app.current_tenant = :tenant_id` | Operations Service (RLS) |
| 4 | `SELECT excel_url FROM settlements WHERE id = :id` | SA.operations |
| 5 | Si settlement no encontrado → 404 | Operations Service |
| 6 | Si `excel_url` es NULL → 404 con codigo especifico | Operations Service |
| 7 | Leer archivo desde `{STORAGE_ROOT}/{excel_url}` | Operations Service → Filesystem |
| 8 | Si archivo no existe en filesystem → 404 | Operations Service |
| 9 | Retornar `200` con Content-Type `application/vnd.openxmlformats-officedocument.spreadsheetml.sheet` y Content-Disposition `attachment; filename="settlement-{id}.xlsx"` | Operations Service |

#### Salidas

| Campo | Tipo | Destino | Efecto observable |
|-------|------|---------|-------------------|
| Archivo .xlsx | binary stream | Response body | Descarga del archivo Excel |
| Content-Type | header | Response header | `application/vnd.openxmlformats-officedocument.spreadsheetml.sheet` |
| Content-Disposition | header | Response header | `attachment; filename="settlement-{id}.xlsx"` |

#### Errores Tipados

| Codigo | Causa | Condicion | Respuesta |
|--------|-------|-----------|-----------|
| 401 | No autenticado | JWT ausente o expirado | `401 { code: "OPS_UNAUTHENTICATED" }` |
| 403 | Sin permiso | JWT sin `p_ops_settlement_list` | `403 { code: "OPS_UNAUTHORIZED" }` |
| 404 | Settlement no encontrado | UUID no existe o no pertenece al tenant | `404 { code: "OPS_SETTLEMENT_NOT_FOUND" }` |
| 404 | Excel no generado | `excel_url` es NULL | `404 { code: "OPS_EXCEL_NOT_AVAILABLE" }` |
| 404 | Archivo no existe | `excel_url` apunta a ruta inexistente en filesystem | `404 { code: "OPS_EXCEL_FILE_MISSING" }` |

#### Casos Especiales y Variantes

- **Excel no generado (NULL):** Retorna 404 con `EXCEL_NOT_AVAILABLE`. La UI ya deberia deshabilitar el boton si vio `excel_url: null` en el listado/detalle.
- **Archivo eliminado del filesystem:** Retorna 404 con `EXCEL_FILE_MISSING`. Caso raro (los archivos son inmutables, RN-OPS-25), pero se maneja defensivamente.
- **Content-Disposition:** Fuerza descarga con nombre legible `settlement-{id}.xlsx`.

#### Impacto en Modelo de Datos

- **Lectura:** `settlements.excel_url`. Filesystem (lectura de archivo).
- **Escritura:** Ninguna.

#### Criterios de Aceptacion (Gherkin)

```gherkin
Feature: Descargar reporte Excel de liquidacion

  Scenario: Descarga exitosa
    Given existe un settlement con excel_url apuntando a un archivo valido
    When el usuario envia GET /settlements/:id/download
    Then el sistema retorna 200 con Content-Type xlsx
    And Content-Disposition indica attachment con filename
    And el cuerpo es el archivo Excel binario

  Scenario: Excel no generado
    Given existe un settlement con excel_url=NULL
    When el usuario envia GET /settlements/:id/download
    Then el sistema retorna 404 con code="OPS_EXCEL_NOT_AVAILABLE"

  Scenario: Archivo eliminado del filesystem
    Given existe un settlement con excel_url apuntando a un archivo que ya no existe
    When el usuario envia GET /settlements/:id/download
    Then el sistema retorna 404 con code="OPS_EXCEL_FILE_MISSING"

  Scenario: Settlement de otro tenant
    Given existe un settlement del tenant A
    When un usuario del tenant B intenta descargar
    Then el sistema retorna 404 (RLS lo oculta)
```

#### Trazabilidad de Tests

| ID Test | Tipo | Descripcion |
|---------|------|-------------|
| TP-OPS-18-01 | Positivo | Descarga exitosa con Content-Type y Content-Disposition correctos |
| TP-OPS-18-02 | Positivo | Archivo descargado coincide con el generado originalmente |
| TP-OPS-18-03 | Negativo | 404 settlement no encontrado |
| TP-OPS-18-04 | Negativo | 404 excel_url es NULL (EXCEL_NOT_AVAILABLE) |
| TP-OPS-18-05 | Negativo | 404 archivo no existe en filesystem (EXCEL_FILE_MISSING) |
| TP-OPS-18-06 | Negativo | 404 settlement de otro tenant (RLS) |
| TP-OPS-18-07 | Negativo | 401 sin JWT |
| TP-OPS-18-08 | Negativo | 403 sin permiso |
| TP-OPS-18-09 | Integracion | Lectura de archivo desde ruta deterministica en filesystem |
| TP-OPS-18-10 | Integracion | RLS filtra por tenant_id en settlements |
| TP-OPS-18-11 | E2E | Click icono descarga en historial → archivo .xlsx descargado en navegador |

#### Sin Ambiguedades

- **Supuestos prohibidos:** No se asume que el archivo se sirve desde CDN o blob storage (es filesystem local). No se asume que se genera on-demand (es pre-generado, RN-OPS-25). No se asume que el endpoint retorna JSON (retorna binary stream).
- **Decisiones cerradas:** Streaming directo desde filesystem. Content-Disposition: attachment. Tres tipos de 404 diferenciados (not_found, not_available, file_missing). Permiso reutiliza `p_ops_settlement_list`.
- **Fuera de alcance explicito:** Generacion on-demand. Preview del Excel en navegador. Conversion a PDF.
- **Dependencias externas explicitas:** Filesystem local con permisos de lectura.
- **TODO explicitos = 0**

---

## Changelog

### v3.0.0 (2026-03-15)
- RF-OPS-12 a RF-OPS-14, RF-OPS-16 a RF-OPS-18 documentados (FL-OPS-02: Liquidar Comisiones)
- 7 reglas de negocio nuevas (RN-OPS-19 a RN-OPS-25)
- RF-OPS-15 reservado por FL-OPS-03; numeracion FL-OPS-02 salta de 14 a 16
- Total: 18 RF, 25 RN

### v2.0.0 (2026-03-15)
- RF-OPS-15 documentado (FL-OPS-03: Enviar Mensaje desde Solicitud)
- 4 reglas de negocio nuevas (RN-OPS-15 a RN-OPS-18)
- Cambio en modelo: `observation_type` agregado a `application_observations`
- Consolidacion: 4 candidatos FL → 1 RF atomico (RF-OPS-16/17 merged, RF-OPS-18 out of scope)

### v1.0.0 (2026-03-15)
- RF-OPS-01 a RF-OPS-11 documentados (FL-OPS-01: Ciclo de Vida de Solicitud)
- 14 reglas de negocio (RN-OPS-01 a RN-OPS-14)
