# RF-CAT — Requerimientos Funcionales: Catalogo de Productos

> **Proyecto:** Unazul Backoffice
> **Modulo:** Catalog (SA.catalog)
> **Version:** 1.0.0
> **Fecha:** 2026-03-15
> **Prerequisitos:** `01_alcance_funcional.md`, `02_arquitectura.md`, `03_FL.md`, `05_modelo_datos.md`
> **Flujo origen:** FL-CAT-01 (Gestionar Catalogo de Productos)
> **HUs origen:** HU014, HU015, HU016, HU017, HU018, HU019, HU030, HU031

---

## Resumen de Requerimientos

| ID | Titulo | Prioridad | Severidad | HU | Estado |
|----|--------|-----------|-----------|-----|--------|
| RF-CAT-01 | CRUD de familias de productos con validacion de prefijo | Alta | P0 | HU014 | Documentado |
| RF-CAT-02 | Listar productos con filtros, busqueda y exportacion | Alta | P1 | HU015 | Documentado |
| RF-CAT-03 | Crear/editar producto con entidad y familia | Alta | P0 | HU016 | Documentado |
| RF-CAT-04 | Ver detalle de producto con planes y requisitos | Media | P2 | HU018 | Documentado |
| RF-CAT-05 | CRUD de planes con atributos especificos por categoria | Alta | P0 | HU017 | Documentado |
| RF-CAT-06 | CRUD de coberturas de seguros inline en plan | Alta | P0 | HU030 | Documentado |
| RF-CAT-07 | CRUD de requisitos documentales por producto | Alta | P1 | HU031 | Documentado |
| RF-CAT-08 | CRUD de planes de comisiones con validacion de uso | Alta | P0 | HU019 | Documentado |
| RF-CAT-09 | Deprecar o eliminar producto segun dependencias | Alta | P0 | HU016 | Documentado |

---

## Reglas de Negocio

| ID | Regla | Detalle | Decision |
|----|-------|---------|----------|
| RN-CAT-01 | Prefijo de familia determina categoria | El prefijo del `code` de familia determina la categoria del producto: `PREST`→loan, `SEG`→insurance, `CTA`→account, `TARJETA`→card, `INV`→investment. Funcion pura determinista. | D-CAT-01 |
| RN-CAT-02 | Unicidad de codigo de familia por tenant | `product_families(tenant_id, code)` es UNIQUE. No se repite dentro del mismo tenant. | — |
| RN-CAT-03 | Eliminacion de familia condicionada a productos | Familia sin productos: eliminacion fisica + ProductFamilyDeletedEvent. Familia con productos: 409 Conflict. | — |
| RN-CAT-04 | Estados de producto | `product_status`: draft → active → inactive / deprecated. `deprecated` es estado terminal para productos con planes. | — |
| RN-CAT-05 | Eliminacion vs deprecacion de producto | Producto sin planes: eliminacion fisica + ProductDeletedEvent. Producto con planes: solo deprecated (no se elimina). | D-CAT-02 |
| RN-CAT-06 | Tabla de atributos 1:1 por categoria | Cada plan tiene exactamente una tabla de atributos segun la categoria de su familia: `plan_loan_attributes`, `plan_insurance_attributes`, `plan_account_attributes`, `plan_card_attributes`, `plan_investment_attributes`. | — |
| RN-CAT-07 | Coberturas solo para seguros | Solo los planes de categoria `insurance` (SEG) pueden tener coberturas en tabla `coverages`. Otras categorias: campo ignorado. | — |
| RN-CAT-08 | Unicidad de codigo de comision por tenant | `commission_plans(tenant_id, code)` es UNIQUE. | — |
| RN-CAT-09 | Eliminacion de comision condicionada a uso | Plan de comision sin asignaciones a `product_plans`: eliminacion fisica. Con asignaciones: 409 Conflict. | — |
| RN-CAT-10 | Validacion entity_id cross-service | Al crear producto, se valida que `entity_id` exista en Organization Service (HTTP sync, cacheable). Si no existe o esta inactiva: 422. | — |
| RN-CAT-11 | Doble validacion prefijo-categoria | Frontend resuelve categoria para UX (selects dinamicos). Backend valida que atributos enviados correspondan a la categoria de la familia. | D-CAT-01 |
| RN-CAT-12 | Deprecated es terminal | Un producto en estado `deprecated` no puede volver a `active` ni a `draft`. Es irreversible. | D-CAT-02 |
| RN-CAT-13 | Ownership de comisiones | Catalog Service es owner de `commission_plans`. La UI puede ubicarlos bajo Configuracion pero la API es de Catalog. | D-CAT-03 |
| RN-CAT-14 | Exportacion limitada | La exportacion (Excel/CSV) aplica los mismos filtros activos. Limite maximo de 10,000 registros. | — |
| RN-CAT-15 | Deprecated excluido por defecto | El listado de productos excluye `status = deprecated` a menos que se envie filtro explicito `status=deprecated`. | — |
| RN-CAT-16 | Coberturas parametrizadas | Las coberturas disponibles se obtienen del parametro `insurance_coverages` en Config Service. Las ya agregadas al plan no aparecen en el select. | — |
| RN-CAT-17 | RLS en todas las tablas | Todas las tablas de SA.catalog aplican RLS con `tenant_id = current_setting('app.current_tenant')::uuid`. | — |

---

## RF-CAT-01 — CRUD de familias de productos con validacion de prefijo

### Execution Sheet

| Campo | Valor |
|-------|-------|
| **ID** | RF-CAT-01 |
| **Titulo** | CRUD de familias de productos con validacion de prefijo |
| **Actor(es)** | Admin Producto, Super Admin (CRUD completo); Admin Entidad, Consulta, Auditor, Operador (solo lectura: GET /product-families) |
| **Prioridad** | Alta |
| **Severidad** | P0 |
| **Flujo origen** | FL-CAT-01 seccion 6 (familias), seccion 8a (eliminar) |
| **HU origen** | HU014 |

### Precondiciones

| # | Condicion | Detalle |
|---|-----------|---------|
| 1 | Catalog Service operativo | SA.catalog accesible |
| 2 | Usuario autenticado | JWT valido con claim `tenant_id` |
| 3 | Permiso requerido | `p_cat_create` para crear/editar, `p_cat_delete` para eliminar, `p_cat_list` para listar |

### Entradas

**Listar:** `GET /product-families`

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| page | integer | No (default 1) | Query param | >= 1 | — |
| page_size | integer | No (default 20) | Query param | 1-100 | — |
| search | string | No | Query param | ILIKE sobre `code` y `description` | — |

**Crear:** `POST /product-families`

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| code | string | Si | Body JSON | No vacio, max 15 chars, debe iniciar con prefijo valido (PREST, SEG, CTA, TARJETA, INV) | RN-CAT-01, RN-CAT-02 |
| description | string | Si | Body JSON | No vacio, max 30 chars | — |

**Editar:** `PUT /product-families/:id`

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| id | uuid | Si | Path param | Debe existir en `product_families` del tenant | — |
| description | string | Si | Body JSON | No vacio, max 30 chars | — |

> **Nota:** `code` no es editable despues de la creacion (determina categoria).

**Eliminar:** `DELETE /product-families/:id`

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| id | uuid | Si | Path param | Debe existir, sin productos asociados | RN-CAT-03 |

### Proceso (Happy Path)

**Variante Crear:**

| # | Paso | Responsable |
|---|------|-------------|
| 1 | SPA envia `POST /product-families { code, description }` | Frontend SPA |
| 2 | API Gateway valida JWT y enruta a Catalog Service | API Gateway |
| 3 | Catalog Service verifica permiso `p_cat_create` | Catalog Service |
| 4 | Validar formato: `code` no vacio, max 15 chars, prefijo valido | Catalog Service |
| 5 | Extraer categoria del prefijo (`PREST`→loan, `SEG`→insurance, etc.) y validar que sea reconocido | Catalog Service |
| 6 | Validar unicidad: `SELECT COUNT(*) FROM product_families WHERE tenant_id = ? AND code = ?` | Catalog Service |
| 7 | `INSERT INTO product_families (id, tenant_id, code, description, created_at, updated_at, created_by, updated_by)` | Catalog Service |
| 8 | Publicar `ProductFamilyCreatedEvent { family_id, tenant_id, code, description, user_id, user_name }` a RabbitMQ | Catalog Service |
| 9 | Retornar `201 Created { id }` | Catalog Service |

**Variante Editar:**

| # | Paso | Responsable |
|---|------|-------------|
| 1 | SPA envia `PUT /product-families/:id { description }` | Frontend SPA |
| 2 | API Gateway valida JWT y enruta | API Gateway |
| 3 | Catalog Service verifica permiso `p_cat_edit` | Catalog Service |
| 4 | Validar que familia exista: `SELECT * FROM product_families WHERE id = ? AND tenant_id = ?` | Catalog Service |
| 5 | Actualizar `description`, `updated_at`, `updated_by` | Catalog Service |
| 6 | Publicar `ProductFamilyUpdatedEvent` a RabbitMQ | Catalog Service |
| 7 | Retornar `200 OK { id }` | Catalog Service |

**Variante Eliminar:**

| # | Paso | Responsable |
|---|------|-------------|
| 1 | SPA envia `DELETE /product-families/:id` | Frontend SPA |
| 2 | API Gateway valida JWT y enruta | API Gateway |
| 3 | Catalog Service verifica permiso `p_cat_delete` | Catalog Service |
| 4 | Validar que familia exista | Catalog Service |
| 5 | Verificar que no tiene productos: `SELECT COUNT(*) FROM products WHERE family_id = ? AND tenant_id = ?` | Catalog Service |
| 6 | Si tiene productos → retornar 409 | Catalog Service |
| 7 | `DELETE FROM product_families WHERE id = ? AND tenant_id = ?` | Catalog Service |
| 8 | Publicar `ProductFamilyDeletedEvent { family_id, tenant_id, code, user_id, user_name }` a RabbitMQ | Catalog Service |
| 9 | Retornar `204 No Content` | Catalog Service |

### Salidas

| Campo | Tipo | Destino | Efecto observable |
|-------|------|---------|-------------------|
| items[] | Array | SPA (listar) | Cada item: `{ id, code, description, category, product_count, created_at }` |
| id | uuid | SPA (crear) | ID de la familia creada |
| HTTP 201/200/204 | status code | SPA | Confirmacion de operacion |
| ProductFamily*Event | evento async | RabbitMQ → Audit Service | Registro en `audit_log` |

### Errores Tipados

| Codigo | Causa | Condicion de disparo | Respuesta esperada |
|--------|-------|---------------------|--------------------|
| 401 | JWT ausente o invalido | Header Authorization faltante o token expirado | `{ error: "UNAUTHORIZED" }` |
| 403 | Sin permiso | Permiso requerido no presente en claims | `{ error: "FORBIDDEN", message: "Permiso {perm} requerido" }` |
| 404 | Familia no encontrada | `id` no existe en `product_families` del tenant (editar/eliminar) | `{ error: "NOT_FOUND", message: "Familia no encontrada" }` |
| 409 | Codigo duplicado | Ya existe familia con mismo `code` en el tenant | `{ error: "CONFLICT", message: "Ya existe una familia con el codigo {code}" }` |
| 409 | Familia con productos | Intento de eliminar familia que tiene productos asociados | `{ error: "CONFLICT", message: "Familia con {N} productos asociados, no se puede eliminar" }` |
| 422 | Validacion fallida | Codigo vacio, prefijo no reconocido, description vacio, max chars excedido | `{ error: "VALIDATION_ERROR", details: [{ field, message }] }` |

### Casos Especiales y Variantes

- **Prefijo no reconocido:** Si el `code` no inicia con ninguno de los 5 prefijos validos, retorna 422 con mensaje "Prefijo de codigo no reconocido. Prefijos validos: PREST, SEG, CTA, TARJETA, INV".
- **Code inmutable:** El `code` no se puede modificar en edicion porque determina la categoria de todos los productos/planes asociados.
- **product_count en listado:** Se calcula con `COUNT(products.id)` por familia para mostrar en la grilla.
- **category en respuesta:** Se calcula del prefijo del code al momento de retornar (campo derivado, no almacenado).
- **Race condition en unicidad:** Si dos requests crean el mismo `code` simultaneamente, el indice UNIQUE `product_families(tenant_id, code)` atrapa la segunda y se retorna 409.
- **Acceso de solo lectura (Consulta / Auditor / Operador):** Los roles de solo lectura tienen acceso al endpoint `GET /product-families` (listar) con permiso `p_cat_list`. No pueden crear, editar ni eliminar familias.

### Impacto en Modelo de Datos

| Entidad | Operacion | Campo(s) afectado(s) | Evento |
|---------|-----------|---------------------|--------|
| `product_families` | INSERT, UPDATE, DELETE | Todos los campos | ProductFamilyCreatedEvent, ProductFamilyUpdatedEvent, ProductFamilyDeletedEvent |
| `audit_log` (SA.audit) | INSERT (async) | Consume eventos de familia | — |

### Criterios de Aceptacion (Gherkin)

```gherkin
Scenario: Crear familia con prefijo valido
  Given un Admin Producto autenticado con permiso p_cat_create
  When envia POST /product-families con code = "PREST001" y description = "Prestamos Personales"
  Then recibe 201 con { id }
  And la familia se persiste con code = "PREST001"
  And se publica ProductFamilyCreatedEvent

Scenario: Crear familia con prefijo invalido retorna 422
  Given un Admin Producto autenticado
  When envia POST /product-families con code = "INVALID01"
  Then recibe 422 con error VALIDATION_ERROR
  And details contiene mensaje sobre prefijo no reconocido

Scenario: Codigo duplicado retorna 409
  Given existe familia con code = "SEG001" en el tenant
  When envia POST /product-families con code = "SEG001"
  Then recibe 409 con error CONFLICT

Scenario: Eliminar familia sin productos exitosamente
  Given existe familia "PREST002" sin productos asociados
  When envia DELETE /product-families/:id
  Then recibe 204
  And la familia se elimina fisicamente
  And se publica ProductFamilyDeletedEvent

Scenario: Eliminar familia con productos retorna 409
  Given existe familia "CTA001" con 3 productos asociados
  When envia DELETE /product-families/:id
  Then recibe 409 con mensaje "Familia con 3 productos asociados, no se puede eliminar"
```

### Trazabilidad de Pruebas

| Test ID | Tipo | Descripcion |
|---------|------|-------------|
| TP-CAT-01-01 | Positivo | Listar familias paginado con product_count |
| TP-CAT-01-02 | Positivo | Crear familia con prefijo PREST valido |
| TP-CAT-01-03 | Positivo | Crear familia con prefijo SEG valido |
| TP-CAT-01-04 | Positivo | Editar description de familia existente |
| TP-CAT-01-05 | Positivo | Eliminar familia sin productos retorna 204 |
| TP-CAT-01-06 | Negativo | Prefijo no reconocido retorna 422 |
| TP-CAT-01-07 | Negativo | Codigo duplicado retorna 409 |
| TP-CAT-01-08 | Negativo | Eliminar familia con productos retorna 409 |
| TP-CAT-01-09 | Negativo | Code vacio retorna 422 |
| TP-CAT-01-10 | Negativo | 403 sin permiso p_cat_create |
| TP-CAT-01-11 | Negativo | 401 sin JWT |
| TP-CAT-01-12 | Negativo | 404 familia no encontrada en edicion |
| TP-CAT-01-13 | Integracion | ProductFamilyCreatedEvent publicado y registrado en audit |
| TP-CAT-01-14 | Integracion | ProductFamilyDeletedEvent publicado y registrado en audit |
| TP-CAT-01-15 | Integracion | RLS: familia de otro tenant no visible |
| TP-CAT-01-16 | E2E | Navegar a /familias-productos, crear, editar, eliminar |

### Sin Ambiguedades

- **Supuestos prohibidos:** No se asume que el code se puede editar despues de crear. No se asume eliminacion logica. No se asume que la categoria se almacena en la tabla (se deriva del prefijo).
- **Decisiones cerradas:** Code inmutable post-creacion. Categoria derivada de prefijo (no campo almacenado). Eliminacion fisica con validacion de productos (D-CAT-01).
- **Fuera de alcance explicito:** Reordenamiento de familias. Familias sin prefijo valido (siempre requieren prefijo). Importacion masiva de familias.
- **Dependencias externas:** Ninguna (operacion local en SA.catalog).
- **TODO explicitos = 0**

---

## RF-CAT-02 — Listar productos con filtros, busqueda y exportacion

### Execution Sheet

| Campo | Valor |
|-------|-------|
| **ID** | RF-CAT-02 |
| **Titulo** | Listar productos con filtros, busqueda y exportacion |
| **Actor(es)** | Admin Producto, Super Admin, Admin Entidad (su entidad), Consulta, Auditor, Operador |
| **Prioridad** | Alta |
| **Severidad** | P1 |
| **Flujo origen** | FL-CAT-01 seccion 6 (listar productos) |
| **HU origen** | HU015 |

### Precondiciones

| # | Condicion | Detalle |
|---|-----------|---------|
| 1 | Catalog Service operativo | SA.catalog accesible |
| 2 | Usuario autenticado | JWT valido con claim `tenant_id` |
| 3 | Permiso requerido | `p_cat_list` presente en claims del JWT |

### Entradas

`GET /products`

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| page | integer | No (default 1) | Query param | >= 1 | — |
| page_size | integer | No (default 20) | Query param | 1-100 | — |
| search | string | No | Query param | ILIKE sobre `name` y `code` | — |
| status | string | No | Query param | Enum: `draft`, `active`, `inactive`, `deprecated` | RN-CAT-04, RN-CAT-15 |
| family_id | uuid | No | Query param | Debe existir en `product_families` | — |
| entity_id | uuid | No | Query param | UUID valido | — |
| sort_by | string | No (default `created_at`) | Query param | Enum: `name`, `code`, `status`, `family_code`, `created_at` | — |
| sort_dir | string | No (default `desc`) | Query param | Enum: `asc`, `desc` | — |
| format | string | No | Query param (solo /export) | Enum: `xlsx`, `csv` | RN-CAT-14 |

### Proceso (Happy Path)

| # | Paso | Responsable |
|---|------|-------------|
| 1 | SPA envia `GET /products?page&page_size&search&status&family_id&entity_id&sort_by&sort_dir` | Frontend SPA |
| 2 | API Gateway valida JWT y enruta a Catalog Service | API Gateway |
| 3 | Catalog Service verifica permiso `p_cat_list` | Catalog Service |
| 4 | SET LOCAL app.current_tenant (RLS) | Catalog Service |
| 5 | Si actor es Admin Entidad: agregar filtro `WHERE entity_id = {entity_id del JWT}` | Catalog Service |
| 6 | Si `status` no se envia: agregar filtro `WHERE status != 'deprecated'` (RN-CAT-15) | Catalog Service |
| 7 | Aplicar busqueda ILIKE sobre `name` y `code` si `search` presente | Catalog Service |
| 8 | Aplicar filtros de `family_id`, `entity_id`, `status` si presentes | Catalog Service |
| 9 | JOIN con `product_families` para obtener `family_code` y `family_description` | Catalog Service |
| 10 | Aplicar ordenamiento y paginacion | Catalog Service |
| 11 | Retornar `200 { items[], total, page, page_size }` | Catalog Service |

### Salidas

| Campo | Tipo | Destino | Efecto observable |
|-------|------|---------|-------------------|
| items[] | Array | SPA | Cada item: `{ id, name, code, description, status, family_id, family_code, family_description, entity_id, valid_from, valid_to, plan_count, created_at }` |
| total | integer | SPA | Total de registros que coinciden con filtros |
| page | integer | SPA | Pagina actual |
| page_size | integer | SPA | Tamano de pagina |

### Errores Tipados

| Codigo | Causa | Condicion de disparo | Respuesta esperada |
|--------|-------|---------------------|--------------------|
| 401 | JWT ausente o invalido | Header Authorization faltante o token expirado | `{ error: "UNAUTHORIZED" }` |
| 403 | Sin permiso | `p_cat_list` no presente en claims | `{ error: "FORBIDDEN", message: "Permiso p_cat_list requerido" }` |
| 400 | Parametros invalidos | `page < 1`, `page_size > 100`, `status` no es enum valido, `sort_by` no reconocido | `{ error: "VALIDATION_ERROR", details[] }` |
| 422 | Exportacion excede limite | Mas de 10,000 registros para exportar | `{ error: "EXPORT_LIMIT_EXCEEDED", message: "Demasiados registros para exportar. Aplique filtros mas restrictivos" }` |

### Casos Especiales y Variantes

- **Deprecated excluido por defecto:** Si no se envia `status`, los productos con `status = deprecated` no aparecen. Para verlos, enviar `status=deprecated` explicitamente (RN-CAT-15).
- **plan_count:** Se calcula con `COUNT(product_plans.id)` por producto para mostrar en la grilla.
- **Admin Entidad:** Solo ve productos de su entidad (filtro automatico por `entity_id` del JWT).
- **Exportacion:** `GET /products/export?format=xlsx&...` aplica mismos filtros sin paginacion. Limite 10,000 filas.
- **Sin resultados:** Retorna `{ items: [], total: 0 }` con HTTP 200.

### Impacto en Modelo de Datos

| Entidad | Operacion | Campo(s) afectado(s) | Evento |
|---------|-----------|---------------------|--------|
| `products` | SELECT | Todos los campos de lectura | — |
| `product_families` | SELECT (JOIN) | `code`, `description` | — |

### Criterios de Aceptacion (Gherkin)

```gherkin
Scenario: Admin Producto lista productos excluyendo deprecated
  Given un Admin Producto autenticado con permiso p_cat_list
  And existen 5 productos: 3 active, 1 draft, 1 deprecated
  When envia GET /products?page=1&page_size=20
  Then recibe 200 con items[] de 4 elementos
  And ningun item tiene status = "deprecated"

Scenario: Filtro por familia retorna solo productos de esa familia
  Given existen 3 productos de familia "PREST001" y 2 de "SEG001"
  When envia GET /products?family_id={id_prest001}
  Then recibe 200 con items[] de 3 elementos

Scenario: Busqueda ILIKE filtra por nombre y codigo
  Given existen productos "Prestamo Personal" y "Seguro de Vida"
  When envia GET /products?search=Prestamo
  Then recibe 200 con items[] de 1 elemento

Scenario: Admin Entidad solo ve productos de su entidad
  Given un Admin Entidad con entity_id = "ent-123"
  And existen 2 productos de "ent-123" y 3 de "ent-456"
  When envia GET /products
  Then recibe 200 con items[] de 2 elementos

Scenario: Status invalido retorna 400
  Given un Admin Producto autenticado
  When envia GET /products?status=nonexistent
  Then recibe 400 con error VALIDATION_ERROR
```

### Trazabilidad de Pruebas

| Test ID | Tipo | Descripcion |
|---------|------|-------------|
| TP-CAT-02-01 | Positivo | Lista paginada con campos esperados y plan_count |
| TP-CAT-02-02 | Positivo | Busqueda ILIKE filtra por name y code |
| TP-CAT-02-03 | Positivo | Filtro por status retorna solo productos del estado |
| TP-CAT-02-04 | Positivo | Filtro por family_id retorna solo productos de esa familia |
| TP-CAT-02-05 | Positivo | Filtro por entity_id retorna productos de esa entidad |
| TP-CAT-02-06 | Positivo | Exportacion Excel genera archivo correcto |
| TP-CAT-02-07 | Positivo | Deprecated excluido por defecto sin filtro status |
| TP-CAT-02-08 | Negativo | Sin permiso p_cat_list retorna 403 |
| TP-CAT-02-09 | Negativo | Sin JWT retorna 401 |
| TP-CAT-02-10 | Negativo | page_size > 100 retorna 400 |
| TP-CAT-02-11 | Negativo | status invalido retorna 400 |
| TP-CAT-02-12 | Integracion | Admin Entidad solo ve productos de su entidad |
| TP-CAT-02-13 | Integracion | Ordenamiento por name/code/status/family_code/created_at funciona |
| TP-CAT-02-14 | E2E | Navegar a /productos, buscar, filtrar por familia y status, exportar |

### Sin Ambiguedades

- **Supuestos prohibidos:** No se asume que deprecated aparece en listado sin filtro explicito. No se asume paginacion del lado del cliente. No se asume que `entity_name` se muestra en listado (no se almacena en `products`; el frontend puede resolver via lookup si necesario).
- **Decisiones cerradas:** Deprecated excluido por defecto (RN-CAT-15). Busqueda ILIKE simultaneo sobre `name` y `code` (OR). Exportacion con limite de 10,000 filas.
- **Fuera de alcance explicito:** Filtro por fecha de vigencia. Busqueda full-text en description. Filtro multi-familia.
- **Dependencias externas:** Ninguna para lectura (JOIN local con `product_families`).
- **TODO explicitos = 0**

---

## RF-CAT-03 — Crear/editar producto con entidad y familia

### Execution Sheet

| Campo | Valor |
|-------|-------|
| **ID** | RF-CAT-03 |
| **Titulo** | Crear/editar producto con entidad y familia |
| **Actor(es)** | Admin Producto, Super Admin, Admin Entidad (su entidad) |
| **Prioridad** | Alta |
| **Severidad** | P0 |
| **Flujo origen** | FL-CAT-01 seccion 6 (crear/editar producto) |
| **HU origen** | HU016 |

### Precondiciones

| # | Condicion | Detalle |
|---|-----------|---------|
| 1 | Catalog Service operativo | SA.catalog accesible |
| 2 | Usuario autenticado | JWT valido con claim `tenant_id` |
| 3 | Permiso requerido | `p_cat_create` para crear, `p_cat_edit` para editar |
| 4 | Al menos una familia existente | Para seleccionar en el formulario |
| 5 | Al menos una entidad activa | Organization Service accesible para lookup |

### Entradas

**Crear:** `POST /products`

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| entity_id | uuid | Si | Body JSON | Debe existir y estar activa en Organization Service | RN-CAT-10 |
| family_id | uuid | Si | Body JSON | Debe existir en `product_families` del tenant | — |
| name | string | Si | Body JSON | No vacio, max 200 chars | — |
| code | string | Si | Body JSON | No vacio, max 30 chars | — |
| description | string | No | Body JSON | Max 500 chars | — |
| status | string | Si | Body JSON | Enum: `draft`, `active`, `inactive` | RN-CAT-04 |
| valid_from | date | Si | Body JSON | Formato ISO 8601 (yyyy-MM-dd) | — |
| valid_to | date | No | Body JSON | Formato ISO 8601, >= valid_from si presente | — |

**Editar:** `PUT /products/:id`

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| id | uuid | Si | Path param | Debe existir en `products` del tenant | — |
| name | string | Si | Body JSON | No vacio, max 200 chars | — |
| code | string | Si | Body JSON | No vacio, max 30 chars | — |
| description | string | No | Body JSON | Max 500 chars | — |
| status | string | Si | Body JSON | Enum: `draft`, `active`, `inactive` (no `deprecated`, ver RF-CAT-09) | RN-CAT-04 |
| valid_from | date | Si | Body JSON | Formato ISO 8601 | — |
| valid_to | date | No | Body JSON | Formato ISO 8601, >= valid_from si presente | — |

> **Nota:** `entity_id` y `family_id` no son editables post-creacion (determinan entidad y categoria).

### Proceso (Happy Path)

**Variante Crear:**

| # | Paso | Responsable |
|---|------|-------------|
| 1 | SPA obtiene lookups: `GET /entities` (via Organization Service) y `GET /product-families` | Frontend SPA |
| 2 | SPA resuelve categoria del prefijo de familia seleccionada (para UX) | Frontend SPA |
| 3 | SPA envia `POST /products { entity_id, family_id, name, code, description, status, valid_from, valid_to }` | Frontend SPA |
| 4 | API Gateway valida JWT y enruta a Catalog Service | API Gateway |
| 5 | Catalog Service verifica permiso `p_cat_create` | Catalog Service |
| 6 | Si actor es Admin Entidad: validar que `entity_id` coincida con su entidad del JWT | Catalog Service |
| 7 | Validar que `family_id` exista en `product_families` del tenant | Catalog Service |
| 8 | Validar que `entity_id` exista y este activa en Organization Service (HTTP sync GET /entities/:id, cacheable) | Catalog Service |
| 9 | Validar formato de todos los campos | Catalog Service |
| 10 | Generar UUID (UUIDv7) para `id`, setear `version = 1` | Catalog Service |
| 11 | `INSERT INTO products (id, tenant_id, entity_id, family_id, name, code, description, status, valid_from, valid_to, version, created_at, updated_at, created_by, updated_by)` | Catalog Service |
| 12 | Publicar `ProductCreatedEvent { product_id, tenant_id, entity_id, family_id, name, code, user_id, user_name }` a RabbitMQ | Catalog Service |
| 13 | Retornar `201 Created { id }` | Catalog Service |
| 14 | SPA redirige a `/productos/:id/editar` | Frontend SPA |

**Variante Editar:**

| # | Paso | Responsable |
|---|------|-------------|
| 1 | SPA envia `PUT /products/:id { name, code, description, status, valid_from, valid_to }` | Frontend SPA |
| 2 | API Gateway valida JWT y enruta | API Gateway |
| 3 | Catalog Service verifica permiso `p_cat_edit` | Catalog Service |
| 4 | Validar que producto exista: `SELECT * FROM products WHERE id = ? AND tenant_id = ?` | Catalog Service |
| 5 | Si actor es Admin Entidad: validar que producto pertenezca a su entidad | Catalog Service |
| 6 | Validar que `status` no sea `deprecated` (usar RF-CAT-09 para eso) | Catalog Service |
| 7 | Si el producto actual tiene `status = deprecated`: rechazar edicion (422 "Producto deprecado no se puede editar") | Catalog Service |
| 8 | Actualizar campos, incrementar `version`, setear `updated_at`, `updated_by` | Catalog Service |
| 9 | Publicar `ProductUpdatedEvent { product_id, tenant_id, changes, user_id, user_name }` a RabbitMQ | Catalog Service |
| 10 | Retornar `200 OK { id }` | Catalog Service |

### Salidas

| Campo | Tipo | Destino | Efecto observable |
|-------|------|---------|-------------------|
| id | uuid | SPA | ID del producto creado/editado |
| HTTP 201/200 | status code | SPA | Confirmacion de operacion |
| ProductCreatedEvent / ProductUpdatedEvent | evento async | RabbitMQ → Audit Service | Registro en `audit_log` |

### Errores Tipados

| Codigo | Causa | Condicion de disparo | Respuesta esperada |
|--------|-------|---------------------|--------------------|
| 401 | JWT ausente o invalido | Header Authorization faltante o token expirado | `{ error: "UNAUTHORIZED" }` |
| 403 | Sin permiso | Permiso requerido no presente en claims | `{ error: "FORBIDDEN", message: "Permiso {perm} requerido" }` |
| 403 | Entidad ajena | Admin Entidad intenta crear/editar producto de otra entidad | `{ error: "FORBIDDEN", message: "No tiene acceso a esta entidad" }` |
| 404 | Producto no encontrado | `id` no existe en `products` del tenant (editar) | `{ error: "NOT_FOUND", message: "Producto no encontrado" }` |
| 422 | Family no encontrada | `family_id` no existe en `product_families` del tenant | `{ error: "VALIDATION_ERROR", details: [{ field: "family_id", message: "Familia no encontrada" }] }` |
| 422 | Entity no encontrada o inactiva | `entity_id` no existe o esta inactiva en Organization Service | `{ error: "VALIDATION_ERROR", details: [{ field: "entity_id", message: "Entidad no encontrada o inactiva" }] }` |
| 422 | Producto deprecado | Intento de editar producto con `status = deprecated` | `{ error: "VALIDATION_ERROR", message: "Producto deprecado no se puede editar" }` |
| 422 | Validacion fallida | Campos requeridos vacios, formatos invalidos, valid_to < valid_from | `{ error: "VALIDATION_ERROR", details: [{ field, message }] }` |

### Casos Especiales y Variantes

- **entity_id y family_id inmutables:** No se pueden cambiar en edicion. Determinan la entidad propietaria y la categoria del producto.
- **Lookup de entidades:** SPA obtiene entidades via `GET /entities` (Organization Service a traves de API Gateway). El Catalog Service valida `entity_id` con HTTP sync (cacheable con TTL corto).
- **Admin Entidad:** Solo puede crear productos para su propia entidad. El backend valida que `entity_id` coincida con la entidad del JWT.
- **version:** Se incrementa en cada edicion. Util para concurrencia optimista futura.
- **Organization Service caido:** Si la validacion sync de `entity_id` falla por timeout/error, retornar 503 "Servicio de organizaciones no disponible".

### Impacto en Modelo de Datos

| Entidad | Operacion | Campo(s) afectado(s) | Evento |
|---------|-----------|---------------------|--------|
| `products` | INSERT, UPDATE | Todos los campos | ProductCreatedEvent, ProductUpdatedEvent |
| `audit_log` (SA.audit) | INSERT (async) | Consume eventos de producto | — |

### Criterios de Aceptacion (Gherkin)

```gherkin
Scenario: Crear producto con entidad y familia validas
  Given un Admin Producto autenticado con permiso p_cat_create
  And existe familia "PREST001" y entidad activa "ent-123"
  When envia POST /products con entity_id, family_id, name, code, status=draft, valid_from
  Then recibe 201 con { id }
  And el producto se persiste con version = 1
  And se publica ProductCreatedEvent

Scenario: Entity no encontrada retorna 422
  Given un Admin Producto autenticado
  When envia POST /products con entity_id = UUID inexistente
  Then recibe 422 con details[].field = "entity_id"

Scenario: Admin Entidad no puede crear producto de otra entidad
  Given un Admin Entidad con entity_id = "ent-123"
  When envia POST /products con entity_id = "ent-456"
  Then recibe 403 con mensaje "No tiene acceso a esta entidad"

Scenario: Editar producto deprecado retorna 422
  Given existe producto con status = "deprecated"
  When envia PUT /products/:id con name = "Nuevo nombre"
  Then recibe 422 con mensaje "Producto deprecado no se puede editar"

Scenario: Editar producto actualiza version
  Given existe producto con version = 1
  When envia PUT /products/:id con name actualizado
  Then recibe 200
  And el producto tiene version = 2
```

### Trazabilidad de Pruebas

| Test ID | Tipo | Descripcion |
|---------|------|-------------|
| TP-CAT-03-01 | Positivo | Creacion exitosa con todos los campos validos |
| TP-CAT-03-02 | Positivo | Creacion con status = draft |
| TP-CAT-03-03 | Positivo | Edicion actualiza nombre, status y version |
| TP-CAT-03-04 | Negativo | entity_id inexistente retorna 422 |
| TP-CAT-03-05 | Negativo | family_id inexistente retorna 422 |
| TP-CAT-03-06 | Negativo | Campo name vacio retorna 422 |
| TP-CAT-03-07 | Negativo | valid_to < valid_from retorna 422 |
| TP-CAT-03-08 | Negativo | Admin Entidad con entidad ajena retorna 403 |
| TP-CAT-03-09 | Negativo | Editar producto deprecado retorna 422 |
| TP-CAT-03-10 | Negativo | Sin permiso p_cat_create retorna 403 |
| TP-CAT-03-11 | Negativo | Sin JWT retorna 401 |
| TP-CAT-03-12 | Integracion | ProductCreatedEvent publicado y registrado en audit |
| TP-CAT-03-13 | Integracion | entity_id validado sync contra Organization Service |
| TP-CAT-03-14 | Integracion | Admin Entidad solo crea para su entidad |
| TP-CAT-03-15 | Integracion | Organization Service caido retorna 503 |
| TP-CAT-03-16 | E2E | Crear producto: seleccionar entidad, familia, completar form, guardar, ver redireccion |

### Sin Ambiguedades

- **Supuestos prohibidos:** No se asume que entity_id se valida solo en frontend. No se asume que family_id y entity_id son editables. No se asume status `deprecated` como opcion en edicion (eso es RF-CAT-09).
- **Decisiones cerradas:** entity_id validado sync contra Organization Service al crear (cacheable). family_id y entity_id inmutables post-creacion. Version incrementada en cada edicion.
- **Fuera de alcance explicito:** Clonado de productos entre entidades. Importacion masiva. Concurrencia optimista basada en version (preparado pero no impuesto en MVP).
- **Dependencias externas:** Organization Service para validar entity_id (HTTP sync, cacheable).
- **TODO explicitos = 0**

---

## RF-CAT-04 — Ver detalle de producto con planes y requisitos

### Execution Sheet

| Campo | Valor |
|-------|-------|
| **ID** | RF-CAT-04 |
| **Titulo** | Ver detalle de producto con planes y requisitos |
| **Actor(es)** | Admin Producto, Super Admin, Admin Entidad (su entidad), Consulta, Auditor, Operador |
| **Prioridad** | Media |
| **Severidad** | P2 |
| **Flujo origen** | FL-CAT-01 seccion 6 (detalle) |
| **HU origen** | HU018 |

### Precondiciones

| # | Condicion | Detalle |
|---|-----------|---------|
| 1 | Catalog Service operativo | SA.catalog accesible |
| 2 | Usuario autenticado | JWT valido con claim `tenant_id` |
| 3 | Permiso requerido | `p_cat_detail` presente en claims del JWT |

### Entradas

`GET /products/:id`

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| id | uuid | Si | Path param | Debe existir en `products` del tenant | — |

### Proceso (Happy Path)

| # | Paso | Responsable |
|---|------|-------------|
| 1 | SPA envia `GET /products/:id` | Frontend SPA |
| 2 | API Gateway valida JWT y enruta a Catalog Service | API Gateway |
| 3 | Catalog Service verifica permiso `p_cat_detail` | Catalog Service |
| 4 | SET LOCAL app.current_tenant (RLS) | Catalog Service |
| 5 | `SELECT * FROM products WHERE id = ? AND tenant_id = ?` | Catalog Service |
| 6 | Si no existe → 404 | Catalog Service |
| 7 | Si actor es Admin Entidad: validar que `entity_id` coincida con su entidad del JWT | Catalog Service |
| 8 | JOIN con `product_families` para obtener family_code, family_description, categoria derivada | Catalog Service |
| 9 | `SELECT * FROM product_plans WHERE product_id = ?` + LEFT JOIN con tabla de atributos segun categoria | Catalog Service |
| 10 | Para cada plan: JOIN con `commission_plans` para obtener commission_plan_code y commission_plan_description | Catalog Service |
| 11 | Si categoria = insurance: `SELECT * FROM coverages WHERE plan_id = ?` para cada plan | Catalog Service |
| 12 | `SELECT * FROM product_requirements WHERE product_id = ?` | Catalog Service |
| 13 | Retornar `200 { product, family, plans[], requirements[] }` | Catalog Service |

### Salidas

| Campo | Tipo | Destino | Efecto observable |
|-------|------|---------|-------------------|
| product | object | SPA | `{ id, name, code, description, status, entity_id, family_id, valid_from, valid_to, version, created_at, updated_at }` |
| family | object | SPA | `{ id, code, description, category }` |
| plans[] | Array | SPA | Cada plan: `{ id, name, code, price, currency, installments, commission_plan_id, commission_plan_code, commission_plan_description, category_attributes: {...}, coverages[]? }` |
| requirements[] | Array | SPA | Cada req: `{ id, name, type, is_mandatory, description }` |

### Errores Tipados

| Codigo | Causa | Condicion de disparo | Respuesta esperada |
|--------|-------|---------------------|--------------------|
| 401 | JWT ausente o invalido | Header Authorization faltante o token expirado | `{ error: "UNAUTHORIZED" }` |
| 403 | Sin permiso | `p_cat_detail` no presente en claims | `{ error: "FORBIDDEN", message: "Permiso p_cat_detail requerido" }` |
| 403 | Entidad ajena | Admin Entidad intenta ver producto de otra entidad | `{ error: "FORBIDDEN", message: "No tiene acceso a esta entidad" }` |
| 404 | Producto no encontrado | `id` no existe en `products` del tenant | `{ error: "NOT_FOUND", message: "Producto no encontrado" }` |

### Casos Especiales y Variantes

- **Producto sin planes:** `plans[]` retorna array vacio.
- **Producto sin requisitos:** `requirements[]` retorna array vacio.
- **category_attributes segun categoria:** El JSON de atributos varia por categoria: loan_attributes, insurance_attributes, account_attributes, card_attributes, investment_attributes.
- **Coberturas solo en insurance:** Solo los planes de tipo insurance incluyen `coverages[]`.
- **commission_plan_id null:** Si un plan no tiene comision asignada, `commission_plan_code` y `commission_plan_description` son null.
- **Producto deprecated:** Se puede ver el detalle incluso si esta deprecated (lectura permitida).

### Impacto en Modelo de Datos

| Entidad | Operacion | Campo(s) afectado(s) | Evento |
|---------|-----------|---------------------|--------|
| `products` | SELECT | Todos | — |
| `product_families` | SELECT (JOIN) | code, description | — |
| `product_plans` | SELECT | Todos | — |
| `plan_*_attributes` | SELECT (JOIN segun categoria) | Todos | — |
| `coverages` | SELECT (solo insurance) | Todos | — |
| `product_requirements` | SELECT | Todos | — |
| `commission_plans` | SELECT (JOIN) | code, description | — |

### Criterios de Aceptacion (Gherkin)

```gherkin
Scenario: Ver detalle con planes y requisitos
  Given un Admin Producto autenticado con permiso p_cat_detail
  And existe producto "PREST" con 2 planes (loan) y 3 requisitos
  When envia GET /products/:id
  Then recibe 200 con product, family, plans[] de 2 elementos, requirements[] de 3 elementos
  And cada plan incluye loan_attributes (amortization_type, annual_effective_rate, cft_rate, admin_fees)

Scenario: Detalle de producto de seguros incluye coberturas
  Given existe producto "SEG" con 1 plan insurance y 2 coberturas
  When envia GET /products/:id
  Then recibe 200
  And plans[0].coverages tiene 2 elementos con name, sum_insured, premium

Scenario: Producto sin planes retorna arrays vacios
  Given existe producto recien creado sin planes ni requisitos
  When envia GET /products/:id
  Then recibe 200 con plans = [] y requirements = []

Scenario: Producto no encontrado retorna 404
  Given id inexistente
  When envia GET /products/:id
  Then recibe 404 con error NOT_FOUND
```

### Trazabilidad de Pruebas

| Test ID | Tipo | Descripcion |
|---------|------|-------------|
| TP-CAT-04-01 | Positivo | Detalle con planes loan y requisitos |
| TP-CAT-04-02 | Positivo | Detalle con plan insurance incluye coberturas |
| TP-CAT-04-03 | Positivo | Detalle con plan card incluye card_attributes |
| TP-CAT-04-04 | Positivo | Producto sin planes ni requisitos retorna arrays vacios |
| TP-CAT-04-05 | Negativo | 404 producto no encontrado |
| TP-CAT-04-06 | Negativo | 403 sin permiso p_cat_detail |
| TP-CAT-04-07 | Negativo | 401 sin JWT |
| TP-CAT-04-08 | Negativo | Admin Entidad con producto de otra entidad retorna 403 |
| TP-CAT-04-09 | Integracion | Plans cargados con atributos por categoria correctos |
| TP-CAT-04-10 | Integracion | Commission plan name resuelto via JOIN |
| TP-CAT-04-11 | E2E | Navegar a detalle de producto, ver planes, requisitos, coberturas |

### Sin Ambiguedades

- **Supuestos prohibidos:** No se asume que todos los planes tienen coberturas (solo insurance). No se asume que commission_plan siempre esta asignado. No se asume que product deprecated no se puede ver.
- **Decisiones cerradas:** category_attributes se serializa como objeto JSON dentro del plan. Coberturas solo en planes insurance. Detalle de producto deprecated es lectura valida.
- **Fuera de alcance explicito:** Historial de versiones del producto. Comparacion entre planes.
- **Dependencias externas:** Ninguna (todo en SA.catalog).
- **TODO explicitos = 0**

---

## RF-CAT-05 — CRUD de planes con atributos especificos por categoria

### Execution Sheet

| Campo | Valor |
|-------|-------|
| **ID** | RF-CAT-05 |
| **Titulo** | CRUD de planes con atributos especificos por categoria |
| **Actor(es)** | Admin Producto, Super Admin, Admin Entidad (su entidad) |
| **Prioridad** | Alta |
| **Severidad** | P0 |
| **Flujo origen** | FL-CAT-01 seccion 7 (planes con atributos) |
| **HU origen** | HU017 |

### Precondiciones

| # | Condicion | Detalle |
|---|-----------|---------|
| 1 | Catalog Service operativo | SA.catalog accesible |
| 2 | Usuario autenticado | JWT valido con claim `tenant_id` |
| 3 | Permiso requerido | `p_cat_edit` para crear/editar/eliminar planes |
| 4 | Producto existente | El producto padre debe existir y no estar deprecated |
| 5 | Parametros cargados (solo card) | `card_networks` y `card_levels` en Config Service para selects |

### Entradas

**Crear:** `POST /products/:productId/plans`

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| productId | uuid | Si | Path param | Producto debe existir, no deprecated | — |
| name | string | Si | Body JSON | No vacio, max 200 chars | — |
| code | string | Si | Body JSON | No vacio, max 30 chars | — |
| price | decimal | Si | Body JSON | >= 0 | — |
| currency | string | Si | Body JSON | No vacio, max 10 chars (ARS, USD, etc.) | — |
| installments | integer | No | Body JSON | >= 1 si presente | — |
| commission_plan_id | uuid | No | Body JSON | Debe existir en `commission_plans` del tenant si presente | — |
| category_attributes | object | Si | Body JSON | Campos segun categoria de la familia del producto | RN-CAT-06, RN-CAT-11 |
| coverages[] | array | No | Body JSON | Solo si categoria = insurance | RN-CAT-07 |

**Atributos por categoria (dentro de `category_attributes`):**

| Categoria | Campo | Tipo | Requerido |
|-----------|-------|------|-----------|
| loan | amortization_type | string | Si (enum: french, german, american, bullet) |
| loan | annual_effective_rate | decimal | Si (>= 0) |
| loan | cft_rate | decimal | No |
| loan | admin_fees | decimal | No |
| insurance | premium | decimal | Si (>= 0) |
| insurance | sum_insured | decimal | Si (> 0) |
| insurance | grace_period_days | integer | No (>= 0) |
| insurance | coverage_type | string | Si, no vacio |
| account | maintenance_fee | decimal | Si (>= 0) |
| account | minimum_balance | decimal | No |
| account | interest_rate | decimal | No |
| account | account_type | string | Si, no vacio |
| card | credit_limit | decimal | Si (> 0) |
| card | annual_fee | decimal | Si (>= 0) |
| card | interest_rate | decimal | No |
| card | network | string | Si (enum: visa, mastercard, amex, cabal, naranja) |
| card | level | string | Si (validado contra parametro `card_levels`) |
| investment | minimum_amount | decimal | Si (> 0) |
| investment | expected_return | decimal | No |
| investment | term_days | integer | No (>= 1) |
| investment | risk_level | string | Si (enum: low, medium, high) |

**Editar:** `PUT /products/:productId/plans/:planId` — mismos campos que crear.

**Eliminar:** `DELETE /products/:productId/plans/:planId`

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| productId | uuid | Si | Path param | Producto debe existir | — |
| planId | uuid | Si | Path param | Plan debe existir y pertenecer al producto | — |

### Proceso (Happy Path)

**Variante Crear:**

| # | Paso | Responsable |
|---|------|-------------|
| 1 | SPA envia `POST /products/:productId/plans { name, code, price, currency, installments, commission_plan_id, category_attributes, coverages? }` | Frontend SPA |
| 2 | API Gateway valida JWT y enruta a Catalog Service | API Gateway |
| 3 | Catalog Service verifica permiso `p_cat_edit` | Catalog Service |
| 4 | Validar que producto exista y no este deprecated | Catalog Service |
| 5 | Si actor es Admin Entidad: validar que producto pertenezca a su entidad | Catalog Service |
| 6 | Determinar categoria del producto via JOIN con `product_families` (derivar de prefijo) | Catalog Service |
| 7 | Validar `category_attributes` contra la categoria determinada (RN-CAT-11) | Catalog Service |
| 8 | Si `commission_plan_id` presente: validar que exista en `commission_plans` del tenant | Catalog Service |
| 9 | Generar UUID para plan | Catalog Service |
| 10 | `INSERT INTO product_plans (id, product_id, tenant_id, name, code, price, currency, installments, commission_plan_id, ...)` | Catalog Service |
| 11 | `INSERT INTO plan_{category}_attributes (id, plan_id, ...atributos)` (tabla 1:1 segun categoria) | Catalog Service |
| 12 | Si categoria = insurance y `coverages[]` presente: `INSERT INTO coverages` (batch) | Catalog Service |
| 13 | Retornar `201 Created { id }` | Catalog Service |

**Variante Editar:**

| # | Paso | Responsable |
|---|------|-------------|
| 1 | SPA envia `PUT /products/:productId/plans/:planId { ... }` | Frontend SPA |
| 2-5 | (Igual que crear: JWT, permisos, producto existe, entidad validada) | — |
| 6 | Validar que plan exista y pertenezca al producto | Catalog Service |
| 7 | Determinar categoria y validar `category_attributes` | Catalog Service |
| 8 | `UPDATE product_plans SET ... WHERE id = ?` | Catalog Service |
| 9 | `UPDATE plan_{category}_attributes SET ... WHERE plan_id = ?` | Catalog Service |
| 10 | Si categoria = insurance: sincronizar coberturas via diff (INSERT nuevas, DELETE removidas, UPDATE modificadas) | Catalog Service |
| 11 | Retornar `200 OK { id }` | Catalog Service |

**Variante Eliminar:**

| # | Paso | Responsable |
|---|------|-------------|
| 1 | SPA envia `DELETE /products/:productId/plans/:planId` | Frontend SPA |
| 2-5 | (JWT, permisos, producto existe, entidad validada) | — |
| 6 | Validar que plan exista y pertenezca al producto | Catalog Service |
| 7 | `DELETE FROM coverages WHERE plan_id = ?` (si insurance) | Catalog Service |
| 8 | `DELETE FROM plan_{category}_attributes WHERE plan_id = ?` | Catalog Service |
| 9 | `DELETE FROM product_plans WHERE id = ?` | Catalog Service |
| 10 | Retornar `204 No Content` | Catalog Service |

### Salidas

| Campo | Tipo | Destino | Efecto observable |
|-------|------|---------|-------------------|
| id | uuid | SPA (crear/editar) | ID del plan |
| HTTP 201/200/204 | status code | SPA | Confirmacion |

### Errores Tipados

| Codigo | Causa | Condicion de disparo | Respuesta esperada |
|--------|-------|---------------------|--------------------|
| 401 | JWT ausente o invalido | Header Authorization faltante | `{ error: "UNAUTHORIZED" }` |
| 403 | Sin permiso | `p_cat_edit` no presente | `{ error: "FORBIDDEN" }` |
| 403 | Entidad ajena | Admin Entidad accede a producto de otra entidad | `{ error: "FORBIDDEN", message: "No tiene acceso a esta entidad" }` |
| 404 | Producto no encontrado | `productId` no existe | `{ error: "NOT_FOUND", message: "Producto no encontrado" }` |
| 404 | Plan no encontrado | `planId` no existe o no pertenece al producto | `{ error: "NOT_FOUND", message: "Plan no encontrado" }` |
| 422 | Producto deprecado | Producto tiene `status = deprecated` | `{ error: "VALIDATION_ERROR", message: "No se pueden gestionar planes de un producto deprecado" }` |
| 422 | Atributos incorrectos | `category_attributes` no coincide con la categoria del producto | `{ error: "VALIDATION_ERROR", message: "Atributos no corresponden a la categoria {category}" }` |
| 422 | Commission plan no encontrada | `commission_plan_id` no existe en tenant | `{ error: "VALIDATION_ERROR", details: [{ field: "commission_plan_id" }] }` |
| 422 | Campos requeridos faltantes | name, code, price, currency vacios; atributos obligatorios de categoria faltantes | `{ error: "VALIDATION_ERROR", details[] }` |

### Casos Especiales y Variantes

- **Tabla 1:1 estricta:** Al crear un plan, se crea exactamente UNA fila en la tabla de atributos de la categoria correspondiente. Al eliminar, se elimina en cascada.
- **Coberturas batch:** Para planes insurance, las coberturas se crean/actualizan en batch con el plan. No hay endpoint separado para coberturas individuales dentro de un plan (ver RF-CAT-06 para CRUD inline).
- **Parametros card:** `network` se valida contra enum del modelo. `level` se valida contra parametro `card_levels` de Config Service.
- **Producto deprecated bloquea CRUD:** No se pueden crear, editar ni eliminar planes si el producto esta deprecated.
- **Fallback Config Service no disponible:** Si Config Service no esta disponible o los parametros (`card_networks`, `card_levels`, `insurance_coverages`) no estan cargados, los campos select correspondientes muestran una lista vacia con un mensaje de advertencia ("Parametros no disponibles, intente mas tarde"). El plan puede guardarse sin completar esos valores opcionales de select; al volver a editar con Config Service disponible, el usuario podra seleccionarlos.

### Impacto en Modelo de Datos

| Entidad | Operacion | Campo(s) afectado(s) | Evento |
|---------|-----------|---------------------|--------|
| `product_plans` | INSERT, UPDATE, DELETE | Todos | — (incluido en Product events) |
| `plan_loan_attributes` | INSERT, UPDATE, DELETE (1:1) | Todos (si loan) | — |
| `plan_insurance_attributes` | INSERT, UPDATE, DELETE (1:1) | Todos (si insurance) | — |
| `plan_account_attributes` | INSERT, UPDATE, DELETE (1:1) | Todos (si account) | — |
| `plan_card_attributes` | INSERT, UPDATE, DELETE (1:1) | Todos (si card) | — |
| `plan_investment_attributes` | INSERT, UPDATE, DELETE (1:1) | Todos (si investment) | — |
| `coverages` | INSERT, UPDATE, DELETE (batch, si insurance) | Todos | — |

### Criterios de Aceptacion (Gherkin)

```gherkin
Scenario: Crear plan de prestamo con atributos loan
  Given un Admin Producto autenticado
  And existe producto de familia "PREST001" (categoria = loan)
  When envia POST /products/:id/plans con category_attributes = { amortization_type: "french", annual_effective_rate: 45.5 }
  Then recibe 201 con { id }
  And se crea fila en product_plans y plan_loan_attributes

Scenario: Crear plan de seguro con coberturas
  Given existe producto de familia "SEG001" (categoria = insurance)
  When envia POST /products/:id/plans con category_attributes de insurance y coverages[] de 2 items
  Then recibe 201
  And se crean 2 filas en coverages

Scenario: Atributos no corresponden a la categoria retorna 422
  Given existe producto de familia "PREST001" (categoria = loan)
  When envia POST con category_attributes = { premium: 100, sum_insured: 5000 } (atributos de insurance)
  Then recibe 422 con mensaje "Atributos no corresponden a la categoria loan"

Scenario: Eliminar plan elimina atributos y coberturas en cascada
  Given existe plan de insurance con 3 coberturas
  When envia DELETE /products/:id/plans/:planId
  Then recibe 204
  And plan, plan_insurance_attributes y coverages eliminados

Scenario: Producto deprecado bloquea creacion de plan
  Given producto con status = "deprecated"
  When envia POST /products/:id/plans
  Then recibe 422 con mensaje "No se pueden gestionar planes de un producto deprecado"
```

### Trazabilidad de Pruebas

| Test ID | Tipo | Descripcion |
|---------|------|-------------|
| TP-CAT-05-01 | Positivo | Crear plan loan con atributos correctos |
| TP-CAT-05-02 | Positivo | Crear plan insurance con atributos y coberturas |
| TP-CAT-05-03 | Positivo | Crear plan account con atributos correctos |
| TP-CAT-05-04 | Positivo | Crear plan card con network y level validos |
| TP-CAT-05-05 | Positivo | Crear plan investment con risk_level valido |
| TP-CAT-05-06 | Positivo | Editar plan actualiza atributos 1:1 |
| TP-CAT-05-07 | Positivo | Eliminar plan elimina atributos y coberturas en cascada |
| TP-CAT-05-08 | Negativo | Atributos no corresponden a categoria retorna 422 |
| TP-CAT-05-09 | Negativo | Producto deprecated bloquea creacion 422 |
| TP-CAT-05-10 | Negativo | commission_plan_id inexistente retorna 422 |
| TP-CAT-05-11 | Negativo | Campos requeridos faltantes retorna 422 |
| TP-CAT-05-12 | Negativo | Plan no encontrado retorna 404 |
| TP-CAT-05-13 | Negativo | 403 sin permiso p_cat_edit |
| TP-CAT-05-14 | Negativo | Admin Entidad producto ajeno retorna 403 |
| TP-CAT-05-15 | Integracion | Tabla 1:1 creada correctamente segun categoria |
| TP-CAT-05-16 | Integracion | card_levels validado contra Config Service |
| TP-CAT-05-17 | Integracion | Coberturas batch creadas con plan insurance |
| TP-CAT-05-18 | E2E | Abrir PlanDialog, completar campos por categoria, guardar, ver en grilla |
| TP-CAT-05-19 | E2E | Editar plan existente, eliminar plan |

### Sin Ambiguedades

- **Supuestos prohibidos:** No se asume que la categoria se envia explicitamente (se deriva de la familia del producto). No se asume que todas las categorias tienen coberturas. No se asume que los atributos de categoria son opcionales (varian por categoria).
- **Decisiones cerradas:** Categoria derivada de prefijo de familia (D-CAT-01). Coberturas batch con plan (no endpoint separado para crear coberturas sueltas — RF-CAT-06 cubre el CRUD inline). Eliminacion en cascada: plan → atributos 1:1 → coberturas.
- **Fuera de alcance explicito:** Versionado de planes. Copiar plan de un producto a otro. Reordenamiento de planes.
- **Dependencias externas:** Config Service para `card_networks`, `card_levels` (parametros, cacheable).
- **TODO explicitos = 0**

---

## RF-CAT-06 — CRUD de coberturas de seguros inline en plan

### Execution Sheet

| Campo | Valor |
|-------|-------|
| **ID** | RF-CAT-06 |
| **Titulo** | CRUD de coberturas de seguros inline en plan |
| **Actor(es)** | Admin Producto, Super Admin, Admin Entidad (su entidad) |
| **Prioridad** | Alta |
| **Severidad** | P0 |
| **Flujo origen** | FL-CAT-01 seccion 8e (coberturas) |
| **HU origen** | HU030 |

### Precondiciones

| # | Condicion | Detalle |
|---|-----------|---------|
| 1 | Catalog Service operativo | SA.catalog accesible |
| 2 | Usuario autenticado | JWT valido con claim `tenant_id` |
| 3 | Permiso requerido | `p_cat_edit` |
| 4 | Plan de tipo insurance | La categoria del producto debe ser `insurance` (SEG) |
| 5 | Parametros cargados | `insurance_coverages` en Config Service |

### Entradas

Las coberturas se gestionan como parte del plan (RF-CAT-05) via `coverages[]` en el body de crear/editar plan. Adicionalmente, se expone endpoint dedicado para gestion inline:

**Agregar:** `POST /products/:productId/plans/:planId/coverages`

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| productId | uuid | Si | Path param | Producto debe existir, categoria = insurance | RN-CAT-07 |
| planId | uuid | Si | Path param | Plan debe existir y pertenecer al producto | — |
| name | string | Si | Body JSON | Debe ser cobertura valida de `insurance_coverages` parametro | RN-CAT-16 |
| coverage_type | string | Si | Body JSON | No vacio | — |
| sum_insured | decimal | Si | Body JSON | > 0 | — |
| premium | decimal | No | Body JSON | >= 0 si presente | — |
| grace_period_days | integer | No | Body JSON | >= 0 si presente | — |

**Editar:** `PUT /products/:productId/plans/:planId/coverages/:coverageId`

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| coverageId | uuid | Si | Path param | Debe existir y pertenecer al plan | — |
| sum_insured | decimal | Si | Body JSON | > 0 | — |
| premium | decimal | No | Body JSON | >= 0 si presente | — |
| grace_period_days | integer | No | Body JSON | >= 0 si presente | — |

> **Nota:** `name` y `coverage_type` no son editables post-creacion.

**Eliminar:** `DELETE /products/:productId/plans/:planId/coverages/:coverageId`

### Proceso (Happy Path)

**Variante Agregar:**

| # | Paso | Responsable |
|---|------|-------------|
| 1 | SPA muestra PlanDialog: select de coberturas disponibles (filtradas: ya agregadas no aparecen) | Frontend SPA |
| 2 | SPA obtiene lista de coberturas disponibles: `insurance_coverages` de Config Service | Frontend SPA |
| 3 | SPA envia `POST /products/:productId/plans/:planId/coverages { name, coverage_type, sum_insured, premium, grace_period_days }` | Frontend SPA |
| 4 | Catalog Service verifica permiso `p_cat_edit` | Catalog Service |
| 5 | Validar que producto sea de categoria insurance | Catalog Service |
| 6 | Validar que `name` sea cobertura valida y no exista ya en el plan | Catalog Service |
| 7 | `INSERT INTO coverages (id, plan_id, tenant_id, name, coverage_type, sum_insured, premium, grace_period_days, ...)` | Catalog Service |
| 8 | Retornar `201 Created { id }` | Catalog Service |

**Variante Eliminar:**

| # | Paso | Responsable |
|---|------|-------------|
| 1 | SPA envia `DELETE /products/:productId/plans/:planId/coverages/:coverageId` | Frontend SPA |
| 2 | Catalog Service verifica permisos y existencia | Catalog Service |
| 3 | `DELETE FROM coverages WHERE id = ?` | Catalog Service |
| 4 | Retornar `204 No Content` | Catalog Service |
| 5 | SPA: la cobertura eliminada vuelve a aparecer en el select de disponibles | Frontend SPA |

### Salidas

| Campo | Tipo | Destino | Efecto observable |
|-------|------|---------|-------------------|
| id | uuid | SPA (agregar) | ID de la cobertura creada |
| HTTP 201/200/204 | status code | SPA | Confirmacion |

### Errores Tipados

| Codigo | Causa | Condicion de disparo | Respuesta esperada |
|--------|-------|---------------------|--------------------|
| 401 | JWT ausente | Token faltante | `{ error: "UNAUTHORIZED" }` |
| 403 | Sin permiso | `p_cat_edit` no presente | `{ error: "FORBIDDEN" }` |
| 404 | Producto/plan/cobertura no encontrado | ID inexistente | `{ error: "NOT_FOUND" }` |
| 422 | Categoria no es insurance | Producto no es de familia SEG | `{ error: "VALIDATION_ERROR", message: "Coberturas solo aplican a productos de seguros" }` |
| 422 | Cobertura duplicada | `name` ya existe en el plan | `{ error: "VALIDATION_ERROR", message: "Cobertura {name} ya agregada a este plan" }` |
| 422 | sum_insured invalido | <= 0 | `{ error: "VALIDATION_ERROR", details[] }` |

### Casos Especiales y Variantes

- **Select filtrado:** El frontend filtra del select las coberturas ya agregadas al plan para evitar duplicados.
- **Cobertura eliminada vuelve al select:** Al eliminar una cobertura del plan, su `name` vuelve a estar disponible en el select.
- **Batch en creacion de plan:** Al crear plan con `coverages[]` (RF-CAT-05), las coberturas se crean en batch. Este RF cubre la gestion individual inline despues de la creacion.
- **name inmutable:** El nombre de la cobertura (seleccionado del parametro) no se edita. Solo se editan valores monetarios y dias de gracia.

### Impacto en Modelo de Datos

| Entidad | Operacion | Campo(s) afectado(s) | Evento |
|---------|-----------|---------------------|--------|
| `coverages` | INSERT, UPDATE, DELETE | Todos | — (parte de operacion de plan) |

### Criterios de Aceptacion (Gherkin)

```gherkin
Scenario: Agregar cobertura a plan de seguro
  Given un Admin Producto autenticado
  And existe plan de producto SEG sin coberturas
  And insurance_coverages parametrizada con ["Incendio", "Robo", "RC"]
  When envia POST coverages con name = "Incendio", sum_insured = 500000
  Then recibe 201
  And cobertura "Incendio" ya no aparece en selects disponibles

Scenario: Cobertura duplicada retorna 422
  Given plan ya tiene cobertura "Incendio"
  When envia POST coverages con name = "Incendio"
  Then recibe 422 con mensaje "Cobertura Incendio ya agregada"

Scenario: Coberturas en producto no-insurance retorna 422
  Given producto de familia "PREST001" (loan)
  When envia POST coverages
  Then recibe 422 con mensaje "Coberturas solo aplican a productos de seguros"

Scenario: Eliminar cobertura libera el select
  Given plan tiene cobertura "Robo"
  When envia DELETE coverages/:id
  Then recibe 204
  And "Robo" vuelve a estar disponible en el select
```

### Trazabilidad de Pruebas

| Test ID | Tipo | Descripcion |
|---------|------|-------------|
| TP-CAT-06-01 | Positivo | Agregar cobertura a plan insurance exitosamente |
| TP-CAT-06-02 | Positivo | Editar sum_insured y premium de cobertura |
| TP-CAT-06-03 | Positivo | Eliminar cobertura retorna 204 |
| TP-CAT-06-04 | Positivo | Select filtra coberturas ya agregadas |
| TP-CAT-06-05 | Negativo | Cobertura duplicada retorna 422 |
| TP-CAT-06-06 | Negativo | Producto no-insurance retorna 422 |
| TP-CAT-06-07 | Negativo | sum_insured <= 0 retorna 422 |
| TP-CAT-06-08 | Negativo | Plan no encontrado retorna 404 |
| TP-CAT-06-09 | Negativo | 403 sin permiso |
| TP-CAT-06-10 | Integracion | insurance_coverages cargadas desde Config Service |
| TP-CAT-06-11 | Integracion | Cobertura eliminada vuelve al select |
| TP-CAT-06-12 | E2E | Flujo completo: agregar, editar montos, eliminar cobertura en PlanDialog |

### Sin Ambiguedades

- **Supuestos prohibidos:** No se asume que coberturas aplican a todas las categorias. No se asume que el nombre es texto libre (viene del parametro). No se asume que al eliminar cobertura se elimina el plan.
- **Decisiones cerradas:** Coberturas solo para insurance (RN-CAT-07). Nombre viene de parametro `insurance_coverages` (RN-CAT-16). Duplicados verificados por `name` dentro del plan.
- **Fuera de alcance explicito:** Coberturas con logica de exclusion entre si. Coberturas obligatorias por tipo de seguro.
- **Dependencias externas:** Config Service para parametro `insurance_coverages` (cache).
- **TODO explicitos = 0**

---

## RF-CAT-07 — CRUD de requisitos documentales por producto

### Execution Sheet

| Campo | Valor |
|-------|-------|
| **ID** | RF-CAT-07 |
| **Titulo** | CRUD de requisitos documentales por producto |
| **Actor(es)** | Admin Producto, Super Admin, Admin Entidad (su entidad) |
| **Prioridad** | Alta |
| **Severidad** | P1 |
| **Flujo origen** | FL-CAT-01 seccion 8d (requisitos) |
| **HU origen** | HU031 |

### Precondiciones

| # | Condicion | Detalle |
|---|-----------|---------|
| 1 | Catalog Service operativo | SA.catalog accesible |
| 2 | Usuario autenticado | JWT valido con claim `tenant_id` |
| 3 | Permiso requerido | `p_cat_edit` |
| 4 | Producto existente | El producto debe existir y no estar deprecated |

### Entradas

**Agregar:** `POST /products/:productId/requirements`

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| productId | uuid | Si | Path param | Producto debe existir, no deprecated | — |
| name | string | Si | Body JSON | No vacio, max 200 chars | — |
| type | string | Si | Body JSON | Enum: `document`, `data`, `validation` | — |
| is_mandatory | boolean | Si | Body JSON | true o false | — |
| description | string | No | Body JSON | Max 500 chars | — |

**Editar:** `PUT /products/:productId/requirements/:requirementId` — mismos campos.

**Eliminar:** `DELETE /products/:productId/requirements/:requirementId`

### Proceso (Happy Path)

**Variante Agregar:**

| # | Paso | Responsable |
|---|------|-------------|
| 1 | SPA envia `POST /products/:productId/requirements { name, type, is_mandatory, description }` | Frontend SPA |
| 2 | API Gateway valida JWT y enruta | API Gateway |
| 3 | Catalog Service verifica permiso `p_cat_edit` | Catalog Service |
| 4 | Validar que producto exista y no este deprecated | Catalog Service |
| 5 | Si actor es Admin Entidad: validar que producto pertenezca a su entidad | Catalog Service |
| 6 | Validar campos de entrada | Catalog Service |
| 7 | `INSERT INTO product_requirements (id, product_id, tenant_id, name, type, is_mandatory, description, created_at, updated_at)` | Catalog Service |
| 8 | Retornar `201 Created { id }` | Catalog Service |

**Variante Editar:**

| # | Paso | Responsable |
|---|------|-------------|
| 1 | SPA envia `PUT /products/:productId/requirements/:requirementId { name, type, is_mandatory, description }` | Frontend SPA |
| 2-5 | (JWT, permisos, producto existe, entidad validada) | — |
| 6 | Validar que requisito exista y pertenezca al producto | Catalog Service |
| 7 | `UPDATE product_requirements SET ... WHERE id = ?` | Catalog Service |
| 8 | Retornar `200 OK { id }` | Catalog Service |

**Variante Eliminar:**

| # | Paso | Responsable |
|---|------|-------------|
| 1 | SPA envia `DELETE /products/:productId/requirements/:requirementId` | Frontend SPA |
| 2-5 | (JWT, permisos, producto existe, entidad validada) | — |
| 6 | Validar que requisito exista y pertenezca al producto | Catalog Service |
| 7 | `DELETE FROM product_requirements WHERE id = ?` | Catalog Service |
| 8 | Retornar `204 No Content` | Catalog Service |

### Salidas

| Campo | Tipo | Destino | Efecto observable |
|-------|------|---------|-------------------|
| id | uuid | SPA (agregar/editar) | ID del requisito |
| HTTP 201/200/204 | status code | SPA | Confirmacion |

### Errores Tipados

| Codigo | Causa | Condicion de disparo | Respuesta esperada |
|--------|-------|---------------------|--------------------|
| 401 | JWT ausente | Token faltante | `{ error: "UNAUTHORIZED" }` |
| 403 | Sin permiso | `p_cat_edit` no presente | `{ error: "FORBIDDEN" }` |
| 403 | Entidad ajena | Admin Entidad accede a producto de otra entidad | `{ error: "FORBIDDEN" }` |
| 404 | Producto no encontrado | productId inexistente | `{ error: "NOT_FOUND", message: "Producto no encontrado" }` |
| 404 | Requisito no encontrado | requirementId inexistente o no pertenece al producto | `{ error: "NOT_FOUND", message: "Requisito no encontrado" }` |
| 422 | Producto deprecado | Producto tiene `status = deprecated` | `{ error: "VALIDATION_ERROR", message: "No se pueden gestionar requisitos de un producto deprecado" }` |
| 422 | Validacion fallida | name vacio, type no es enum valido | `{ error: "VALIDATION_ERROR", details[] }` |

### Casos Especiales y Variantes

- **Inline en formulario:** Los requisitos se gestionan en una grilla inline dentro del formulario de edicion de producto.
- **Sin limite de requisitos:** No hay limite maximo de requisitos por producto.
- **Producto deprecated bloquea CRUD:** No se pueden crear, editar ni eliminar requisitos si el producto esta deprecated.
- **Eliminacion sin validacion de uso:** Los requisitos se eliminan fisicamente sin verificar si hay solicitudes con documentos que referencian ese requisito (los documentos de solicitud son independientes en SA.operations).

### Impacto en Modelo de Datos

| Entidad | Operacion | Campo(s) afectado(s) | Evento |
|---------|-----------|---------------------|--------|
| `product_requirements` | INSERT, UPDATE, DELETE | Todos | — (incluido en Product events) |

### Criterios de Aceptacion (Gherkin)

```gherkin
Scenario: Agregar requisito documental a producto
  Given un Admin Producto autenticado
  And existe producto activo sin requisitos
  When envia POST requirements con name = "DNI Frente", type = "document", is_mandatory = true
  Then recibe 201 con { id }
  And requisito aparece en la grilla del producto

Scenario: Editar requisito existente
  Given existe requisito "DNI Frente" con is_mandatory = true
  When envia PUT requirements/:id con is_mandatory = false
  Then recibe 200
  And requisito actualizado

Scenario: Eliminar requisito
  Given existe requisito "Comprobante de domicilio"
  When envia DELETE requirements/:id
  Then recibe 204

Scenario: Type invalido retorna 422
  Given un Admin Producto autenticado
  When envia POST requirements con type = "invalid"
  Then recibe 422 con error VALIDATION_ERROR
```

### Trazabilidad de Pruebas

| Test ID | Tipo | Descripcion |
|---------|------|-------------|
| TP-CAT-07-01 | Positivo | Agregar requisito tipo document exitosamente |
| TP-CAT-07-02 | Positivo | Agregar requisito tipo data exitosamente |
| TP-CAT-07-03 | Positivo | Editar requisito actualiza campos |
| TP-CAT-07-04 | Positivo | Eliminar requisito retorna 204 |
| TP-CAT-07-05 | Negativo | name vacio retorna 422 |
| TP-CAT-07-06 | Negativo | type invalido retorna 422 |
| TP-CAT-07-07 | Negativo | Producto deprecado bloquea CRUD 422 |
| TP-CAT-07-08 | Negativo | Requisito no encontrado retorna 404 |
| TP-CAT-07-09 | Negativo | 403 sin permiso |
| TP-CAT-07-10 | Integracion | Requisitos persisten con product_id y tenant_id correctos |
| TP-CAT-07-11 | E2E | Gestionar requisitos en grilla inline del formulario de producto |

### Sin Ambiguedades

- **Supuestos prohibidos:** No se asume que los requisitos estan vinculados a planes (estan vinculados a producto). No se asume validacion de uso en solicitudes al eliminar.
- **Decisiones cerradas:** Requisitos son por producto (no por plan). Eliminacion fisica sin validacion cross-service. Tres tipos: document, data, validation.
- **Fuera de alcance explicito:** Templates de requisitos predefinidos. Requisitos condicionales segun plan. Ordenamiento de requisitos.
- **Dependencias externas:** Ninguna.
- **TODO explicitos = 0**

---

## RF-CAT-08 — CRUD de planes de comisiones con validacion de uso

### Execution Sheet

| Campo | Valor |
|-------|-------|
| **ID** | RF-CAT-08 |
| **Titulo** | CRUD de planes de comisiones con validacion de uso |
| **Actor(es)** | Admin Producto, Super Admin |
| **Prioridad** | Alta |
| **Severidad** | P0 |
| **Flujo origen** | FL-CAT-01 seccion 8c (comisiones) |
| **HU origen** | HU019 |

### Precondiciones

| # | Condicion | Detalle |
|---|-----------|---------|
| 1 | Catalog Service operativo | SA.catalog accesible |
| 2 | Usuario autenticado | JWT valido con claim `tenant_id` |
| 3 | Permiso requerido | `p_cat_commissions_list` para listar, `p_cat_commissions_create` para crear, `p_cat_commissions_edit` para editar, `p_cat_commissions_delete` para eliminar |

### Entradas

**Listar:** `GET /commission-plans`

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| page | integer | No (default 1) | Query param | >= 1 | — |
| page_size | integer | No (default 20) | Query param | 1-100 | — |
| search | string | No | Query param | ILIKE sobre `code` y `description` | — |

**Crear:** `POST /commission-plans`

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| code | string | Si | Body JSON | No vacio, max 30 chars, UNIQUE por tenant | RN-CAT-08 |
| description | string | Si | Body JSON | No vacio, max 200 chars | — |
| type | string | Si | Body JSON | Enum: `fixed_per_sale`, `percentage_capital`, `percentage_total_loan` | — |
| value | decimal | Si | Body JSON | > 0 | — |
| max_amount | decimal | No | Body JSON | > 0 si presente | — |

**Editar:** `PUT /commission-plans/:id` — mismos campos que crear.

**Eliminar:** `DELETE /commission-plans/:id`

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| id | uuid | Si | Path param | Debe existir, sin planes asignados | RN-CAT-09 |

### Proceso (Happy Path)

**Variante Listar:**

| # | Paso | Responsable |
|---|------|-------------|
| 1 | SPA envia `GET /commission-plans?page&page_size&search` | Frontend SPA |
| 2 | API Gateway valida JWT y enruta | API Gateway |
| 3 | Catalog Service verifica permiso `p_cat_commissions_list` | Catalog Service |
| 4 | SET LOCAL app.current_tenant (RLS) | Catalog Service |
| 5 | Query con busqueda, paginacion. LEFT JOIN para calcular `assigned_plan_count` (COUNT de product_plans con commission_plan_id = ?) | Catalog Service |
| 6 | Retornar `200 { items[], total, page, page_size }` | Catalog Service |

**Variante Crear:**

| # | Paso | Responsable |
|---|------|-------------|
| 1 | SPA envia `POST /commission-plans { code, description, type, value, max_amount }` | Frontend SPA |
| 2 | API Gateway valida JWT y enruta | API Gateway |
| 3 | Catalog Service verifica permiso `p_cat_commissions_create` | Catalog Service |
| 4 | Validar unicidad de `code`: `SELECT COUNT(*) FROM commission_plans WHERE tenant_id = ? AND code = ?` | Catalog Service |
| 5 | `INSERT INTO commission_plans (id, tenant_id, code, description, type, value, max_amount, ...)` | Catalog Service |
| 6 | Publicar `CommissionPlanCreatedEvent { commission_plan_id, tenant_id, code, type, user_id, user_name }` a RabbitMQ | Catalog Service |
| 7 | Retornar `201 Created { id }` | Catalog Service |

**Variante Editar:**

| # | Paso | Responsable |
|---|------|-------------|
| 1 | SPA envia `PUT /commission-plans/:id { code, description, type, value, max_amount }` | Frontend SPA |
| 2 | API Gateway valida JWT y enruta | API Gateway |
| 3 | Catalog Service verifica permiso `p_cat_commissions_edit` | Catalog Service |
| 4 | Validar que exista el plan de comision | Catalog Service |
| 5 | Validar unicidad de `code` (excluyendo registro actual) | Catalog Service |
| 6 | `UPDATE commission_plans SET ... WHERE id = ?` | Catalog Service |
| 7 | Publicar `CommissionPlanUpdatedEvent { commission_plan_id, tenant_id, code, type, user_id, user_name }` a RabbitMQ | Catalog Service |
| 8 | Retornar `200 OK { id }` | Catalog Service |

**Variante Eliminar:**

| # | Paso | Responsable |
|---|------|-------------|
| 1 | SPA envia `DELETE /commission-plans/:id` | Frontend SPA |
| 2 | API Gateway valida JWT y enruta | API Gateway |
| 3 | Catalog Service verifica permiso `p_cat_commissions_delete` | Catalog Service |
| 4 | Validar que exista el plan de comision | Catalog Service |
| 5 | Verificar que no esta asignado: `SELECT COUNT(*) FROM product_plans WHERE commission_plan_id = ?` | Catalog Service |
| 6 | Si esta asignado → 409 | Catalog Service |
| 7 | `DELETE FROM commission_plans WHERE id = ?` | Catalog Service |
| 8 | Publicar `CommissionPlanDeletedEvent { commission_plan_id, tenant_id, code, user_id, user_name }` a RabbitMQ | Catalog Service |
| 9 | Retornar `204 No Content` | Catalog Service |

### Salidas

| Campo | Tipo | Destino | Efecto observable |
|-------|------|---------|-------------------|
| items[] | Array | SPA (listar) | Cada item: `{ id, code, description, type, value, max_amount, assigned_plan_count, created_at }` |
| id | uuid | SPA (crear) | ID del plan de comision |
| HTTP 201/200/204 | status code | SPA | Confirmacion |
| CommissionPlanCreatedEvent | evento async | RabbitMQ → Audit | Registro en audit (crear) |
| CommissionPlanUpdatedEvent | evento async | RabbitMQ → Audit | Registro en audit (editar) |
| CommissionPlanDeletedEvent | evento async | RabbitMQ → Audit | Registro en audit (eliminar) |

### Errores Tipados

| Codigo | Causa | Condicion de disparo | Respuesta esperada |
|--------|-------|---------------------|--------------------|
| 401 | JWT ausente | Token faltante | `{ error: "UNAUTHORIZED" }` |
| 403 | Sin permiso | Permiso requerido no presente | `{ error: "FORBIDDEN" }` |
| 404 | No encontrado | `id` no existe en `commission_plans` del tenant | `{ error: "NOT_FOUND", message: "Plan de comision no encontrado" }` |
| 409 | Codigo duplicado | Ya existe commission plan con mismo `code` en tenant | `{ error: "CONFLICT", message: "Ya existe un plan de comision con el codigo {code}" }` |
| 409 | En uso | Intento de eliminar plan asignado a planes de producto | `{ error: "CONFLICT", message: "Plan de comision asignado a {N} planes, no se puede eliminar" }` |
| 422 | Validacion fallida | code vacio, type invalido, value <= 0 | `{ error: "VALIDATION_ERROR", details[] }` |

### Casos Especiales y Variantes

- **assigned_plan_count en listado:** Muestra cuantos `product_plans` tienen asignado este commission plan. Util para saber si se puede eliminar.
- **max_amount opcional:** Solo aplica a tipos porcentuales. Para `fixed_per_sale` no tiene sentido pero se permite (backend no valida correlacion tipo-max_amount).
- **Edicion con asignaciones:** Se permite editar un plan de comision incluso si esta asignado a planes de producto. Los cambios afectan liquidaciones futuras.
- **Race condition en unicidad:** El indice UNIQUE `commission_plans(tenant_id, code)` atrapa duplicados concurrentes.

### Impacto en Modelo de Datos

| Entidad | Operacion | Campo(s) afectado(s) | Evento |
|---------|-----------|---------------------|--------|
| `commission_plans` | INSERT, UPDATE, DELETE | Todos | CommissionPlanCreatedEvent, CommissionPlanUpdatedEvent, CommissionPlanDeletedEvent |
| `audit_log` (SA.audit) | INSERT (async) | Consume eventos de comision | — |

### Criterios de Aceptacion (Gherkin)

```gherkin
Scenario: Listar planes de comision con assigned_plan_count
  Given existen 3 planes de comision, 1 asignado a 2 product_plans
  When envia GET /commission-plans
  Then recibe 200 con items[] de 3 elementos
  And el plan asignado tiene assigned_plan_count = 2

Scenario: Crear plan de comision con tipo fixed_per_sale
  Given un Admin Producto autenticado
  When envia POST /commission-plans con code = "COM001", type = "fixed_per_sale", value = 1500
  Then recibe 201 con { id }
  And se publica CommissionPlanCreatedEvent

Scenario: Eliminar plan de comision no asignado
  Given plan de comision sin asignaciones
  When envia DELETE /commission-plans/:id
  Then recibe 204

Scenario: Eliminar plan asignado retorna 409
  Given plan de comision asignado a 3 product_plans
  When envia DELETE /commission-plans/:id
  Then recibe 409 con mensaje "Plan de comision asignado a 3 planes"

Scenario: Codigo duplicado retorna 409
  Given existe plan con code = "COM001"
  When envia POST /commission-plans con code = "COM001"
  Then recibe 409 con error CONFLICT
```

### Trazabilidad de Pruebas

| Test ID | Tipo | Descripcion |
|---------|------|-------------|
| TP-CAT-08-01 | Positivo | Listar planes de comision con assigned_plan_count |
| TP-CAT-08-02 | Positivo | Crear plan tipo fixed_per_sale |
| TP-CAT-08-03 | Positivo | Crear plan tipo percentage_capital con max_amount |
| TP-CAT-08-04 | Positivo | Editar plan de comision existente |
| TP-CAT-08-05 | Positivo | Eliminar plan sin asignaciones retorna 204 |
| TP-CAT-08-06 | Negativo | Codigo duplicado retorna 409 |
| TP-CAT-08-07 | Negativo | Eliminar plan asignado retorna 409 |
| TP-CAT-08-08 | Negativo | code vacio retorna 422 |
| TP-CAT-08-09 | Negativo | type invalido retorna 422 |
| TP-CAT-08-10 | Negativo | value <= 0 retorna 422 |
| TP-CAT-08-11 | Negativo | 403 sin permiso |
| TP-CAT-08-12 | Negativo | 401 sin JWT |
| TP-CAT-08-13 | Integracion | CommissionPlanCreatedEvent publicado y registrado en audit |
| TP-CAT-08-14 | Integracion | assigned_plan_count calculado correctamente via LEFT JOIN |
| TP-CAT-08-15 | E2E | Flujo completo: listar, crear, editar, eliminar comision |

### Sin Ambiguedades

- **Supuestos prohibidos:** No se asume que editar un plan de comision recalcula liquidaciones pasadas. No se asume que max_amount es obligatorio para tipos porcentuales. No se asume que Admin Entidad puede gestionar comisiones (solo Admin Producto y Super Admin).
- **Decisiones cerradas:** Ownership en Catalog Service, UI puede estar bajo Configuracion (D-CAT-03). Eliminacion solo si sin asignaciones. Edicion permitida con asignaciones (afecta futuro).
- **Fuera de alcance explicito:** Historial de cambios en comisiones. Comisiones por rango de monto. Comisiones escalonadas.
- **Dependencias externas:** Ninguna (tabla local en SA.catalog).
- **TODO explicitos = 0**

---

## RF-CAT-09 — Deprecar o eliminar producto segun dependencias

### Execution Sheet

| Campo | Valor |
|-------|-------|
| **ID** | RF-CAT-09 |
| **Titulo** | Deprecar o eliminar producto segun dependencias |
| **Actor(es)** | Admin Producto, Super Admin, Admin Entidad (su entidad) |
| **Prioridad** | Alta |
| **Severidad** | P0 |
| **Flujo origen** | FL-CAT-01 seccion 8b (eliminar/deprecar producto) |
| **HU origen** | HU016 |

### Precondiciones

| # | Condicion | Detalle |
|---|-----------|---------|
| 1 | Catalog Service operativo | SA.catalog accesible |
| 2 | Usuario autenticado | JWT valido con claim `tenant_id` |
| 3 | Permiso requerido | `p_cat_delete` |
| 4 | Producto existente | Debe existir en `products` del tenant |

### Entradas

**Eliminar producto sin planes:** `DELETE /products/:id`

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| id | uuid | Si | Path param | Producto debe existir, sin planes | RN-CAT-05 |

**Deprecar producto con planes:** `PUT /products/:id/deprecate`

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| id | uuid | Si | Path param | Producto debe existir, no estar ya deprecated | RN-CAT-05, RN-CAT-12 |

### Proceso (Happy Path)

**Variante Eliminar (sin planes):**

| # | Paso | Responsable |
|---|------|-------------|
| 1 | SPA envia `DELETE /products/:id` | Frontend SPA |
| 2 | API Gateway valida JWT y enruta | API Gateway |
| 3 | Catalog Service verifica permiso `p_cat_delete` | Catalog Service |
| 4 | Validar que producto exista | Catalog Service |
| 5 | Si actor es Admin Entidad: validar que producto pertenezca a su entidad | Catalog Service |
| 6 | Verificar que no tiene planes: `SELECT COUNT(*) FROM product_plans WHERE product_id = ?` | Catalog Service |
| 7 | Si tiene planes → retornar 409 con mensaje indicando usar deprecacion | Catalog Service |
| 8 | `DELETE FROM product_requirements WHERE product_id = ?` | Catalog Service |
| 9 | `DELETE FROM products WHERE id = ?` | Catalog Service |
| 10 | Publicar `ProductDeletedEvent { product_id, tenant_id, name, code, user_id, user_name }` a RabbitMQ | Catalog Service |
| 11 | Retornar `204 No Content` | Catalog Service |

**Variante Deprecar (con planes):**

| # | Paso | Responsable |
|---|------|-------------|
| 1 | SPA envia `PUT /products/:id/deprecate` | Frontend SPA |
| 2 | API Gateway valida JWT y enruta | API Gateway |
| 3 | Catalog Service verifica permiso `p_cat_delete` | Catalog Service |
| 4 | Validar que producto exista | Catalog Service |
| 5 | Si actor es Admin Entidad: validar que producto pertenezca a su entidad | Catalog Service |
| 6 | Validar que `status` no sea ya `deprecated` | Catalog Service |
| 7 | `UPDATE products SET status = 'deprecated', updated_at = now(), updated_by = ? WHERE id = ?` | Catalog Service |
| 8 | Publicar `ProductDeprecatedEvent { product_id, tenant_id, name, code, previous_status, user_id, user_name }` a RabbitMQ | Catalog Service |
| 9 | Retornar `200 OK { id, status: "deprecated" }` | Catalog Service |

### Salidas

| Campo | Tipo | Destino | Efecto observable |
|-------|------|---------|-------------------|
| HTTP 204 | status code | SPA (eliminar) | Producto eliminado fisicamente |
| id, status | object | SPA (deprecar) | Producto marcado como deprecated |
| ProductDeletedEvent / ProductDeprecatedEvent | evento async | RabbitMQ → Audit | Registro en audit_log |

### Errores Tipados

| Codigo | Causa | Condicion de disparo | Respuesta esperada |
|--------|-------|---------------------|--------------------|
| 401 | JWT ausente | Token faltante | `{ error: "UNAUTHORIZED" }` |
| 403 | Sin permiso | `p_cat_delete` no presente | `{ error: "FORBIDDEN" }` |
| 403 | Entidad ajena | Admin Entidad accede a producto de otra entidad | `{ error: "FORBIDDEN" }` |
| 404 | Producto no encontrado | `id` no existe | `{ error: "NOT_FOUND", message: "Producto no encontrado" }` |
| 409 | Tiene planes (en DELETE) | Intento de eliminar producto con planes existentes | `{ error: "CONFLICT", message: "Producto con {N} planes asociados. Use deprecacion en lugar de eliminacion" }` |
| 422 | Ya deprecado | Intento de deprecar producto que ya esta deprecated | `{ error: "VALIDATION_ERROR", message: "El producto ya esta deprecado" }` |

### Casos Especiales y Variantes

- **DELETE vs deprecar:** El frontend presenta un boton "Eliminar" que llama a `DELETE /products/:id`. Si el backend retorna 409 (tiene planes), el frontend muestra dialogo de confirmacion para deprecar (`PUT /products/:id/deprecate`).
- **Deprecated es terminal:** Un producto deprecated no puede volver a otro estado (RN-CAT-12).
- **Datos en Operations:** Los datos denormalizados (`product_name`, `plan_name`) en SA.operations permanecen intactos tras deprecacion o eliminacion. No se afectan solicitudes existentes.
- **Requisitos al eliminar:** Si el producto se elimina fisicamente (sin planes), sus requisitos se eliminan en cascada.
- **Planes no se eliminan en deprecacion:** Al deprecar, los planes y coberturas permanecen intactos (lectura historica). Solo se bloquea edicion/creacion futura (RF-CAT-05).

### Impacto en Modelo de Datos

| Entidad | Operacion | Campo(s) afectado(s) | Evento |
|---------|-----------|---------------------|--------|
| `products` | UPDATE (deprecar) o DELETE (eliminar) | status / todos | ProductDeprecatedEvent / ProductDeletedEvent |
| `product_requirements` | DELETE (cascada, solo en eliminacion) | Todos | — |
| `audit_log` (SA.audit) | INSERT (async) | Consume eventos | — |

### Criterios de Aceptacion (Gherkin)

```gherkin
Scenario: Eliminar producto sin planes
  Given un Admin Producto autenticado
  And existe producto sin planes ni requisitos
  When envia DELETE /products/:id
  Then recibe 204
  And producto eliminado fisicamente
  And se publica ProductDeletedEvent

Scenario: Eliminar producto con planes retorna 409
  Given producto con 2 planes asociados
  When envia DELETE /products/:id
  Then recibe 409 con mensaje "Producto con 2 planes asociados. Use deprecacion"

Scenario: Deprecar producto con planes exitosamente
  Given producto con status = "active" y 3 planes
  When envia PUT /products/:id/deprecate
  Then recibe 200 con status = "deprecated"
  And se publica ProductDeprecatedEvent
  And el producto no aparece en listados sin filtro status

Scenario: Deprecar producto ya deprecated retorna 422
  Given producto con status = "deprecated"
  When envia PUT /products/:id/deprecate
  Then recibe 422 con mensaje "El producto ya esta deprecado"

Scenario: Eliminar producto con requisitos los elimina en cascada
  Given producto sin planes pero con 3 requisitos
  When envia DELETE /products/:id
  Then recibe 204
  And los 3 requisitos se eliminan fisicamente
```

### Trazabilidad de Pruebas

| Test ID | Tipo | Descripcion |
|---------|------|-------------|
| TP-CAT-09-01 | Positivo | Eliminar producto sin planes retorna 204 |
| TP-CAT-09-02 | Positivo | Deprecar producto con planes retorna 200 |
| TP-CAT-09-03 | Negativo | DELETE producto con planes retorna 409 |
| TP-CAT-09-04 | Negativo | Deprecar producto ya deprecated retorna 422 |
| TP-CAT-09-05 | Negativo | 404 producto no encontrado |
| TP-CAT-09-06 | Negativo | 403 sin permiso p_cat_delete |
| TP-CAT-09-07 | Negativo | Admin Entidad producto ajeno retorna 403 |
| TP-CAT-09-08 | Integracion | ProductDeletedEvent publicado y registrado en audit |
| TP-CAT-09-09 | Integracion | ProductDeprecatedEvent publicado y registrado en audit |
| TP-CAT-09-10 | Integracion | Requisitos eliminados en cascada con producto |
| TP-CAT-09-11 | Integracion | Deprecated excluido de listado por defecto |
| TP-CAT-09-12 | E2E | Flujo eliminar: click eliminar, 409, deprecar, verificar no aparece en listado |

### Sin Ambiguedades

- **Supuestos prohibidos:** No se asume que deprecated es reversible. No se asume que al deprecar se eliminan planes o coberturas. No se asume que solicitudes existentes se afectan por deprecacion.
- **Decisiones cerradas:** Producto con planes solo se depreca, nunca se elimina (D-CAT-02). Deprecated es estado terminal irreversible (RN-CAT-12). Eliminacion fisica de requisitos en cascada con producto.
- **Fuera de alcance explicito:** Programar deprecacion futura (fecha automatica). Notificar entidades afectadas al deprecar. Reactivar producto deprecated.
- **Dependencias externas:** Ninguna directa. Datos denormalizados en SA.operations permanecen intactos.
- **TODO explicitos = 0**

---

## Changelog

### v1.0.0 (2026-03-15)
- RF-CAT-01 a RF-CAT-09 documentados (FL-CAT-01: Gestionar Catalogo de Productos)
- Modulo CAT completo (9 RF)
- 17 reglas de negocio (RN-CAT-01 a RN-CAT-17)
- 119 tests referenciados (TP-CAT-01 a TP-CAT-09)
