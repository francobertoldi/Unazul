# RF-CFG — Requerimientos Funcionales: Configuracion

> **Proyecto:** Unazul Backoffice
> **Modulo:** Config Service (SA.config)
> **Version:** 2.0.0
> **Fecha:** 2026-03-15
> **Prerequisitos:** `01_alcance_funcional.md`, `02_arquitectura.md`, `03_FL.md`, `05_modelo_datos.md`
> **Flujos origen:** FL-CFG-01 (Parametros y Servicios Externos), FL-CFG-02 (Disenar Workflow)
> **HUs origen:** HU020, HU021, HU022, HU034

---

## Resumen de Requerimientos

| ID | Titulo | Prioridad | Severidad | HU | Estado |
|----|--------|-----------|-----------|-----|--------|
| RF-CFG-01 | Listar grupos de parametros por categoria | Alta | P1 | HU020 | Documentado |
| RF-CFG-02 | Listar parametros de un grupo | Alta | P1 | HU020 | Documentado |
| RF-CFG-03 | Crear parametro | Alta | P0 | HU020 | Documentado |
| RF-CFG-04 | Editar parametro inline | Alta | P0 | HU020 | Documentado |
| RF-CFG-05 | Eliminar parametro | Alta | P1 | HU020 | Documentado |
| RF-CFG-06 | Gestionar grupos de parametros | Media | P2 | HU020 | Documentado |
| RF-CFG-07 | Filtrar parametros por relacion jerarquica | Media | P2 | HU020 | Documentado |
| RF-CFG-08 | Listar servicios externos | Alta | P1 | HU021 | Documentado |
| RF-CFG-09 | Crear servicio externo con credenciales encriptadas | Alta | P0 | HU021 | Documentado |
| RF-CFG-10 | Editar servicio externo | Alta | P0 | HU021 | Documentado |
| RF-CFG-11 | Probar conexion de servicio externo | Alta | P1 | HU021 | Documentado |
| RF-CFG-12 | Listar workflows con estado y version | Alta | P1 | HU022 | Documentado |
| RF-CFG-13 | Crear/editar workflow con estados y transiciones | Alta | P0 | HU022 | Documentado |
| RF-CFG-14 | Configurar nodo por tipo con persistencia key-value | Alta | P0 | HU022 | Documentado |
| RF-CFG-15 | Panel de atributos del dominio con insert en condiciones | Alta | P1 | HU034 | Documentado |
| RF-CFG-16 | Gestionar transiciones con label, condicion y SLA | Alta | P1 | HU022 | Documentado |
| RF-CFG-17 | Activar workflow con validacion de grafo | Alta | P0 | HU022 | Documentado |
| RF-CFG-18 | Desactivar workflow | Media | P2 | HU022 | Documentado |
| RF-CFG-19 | CRUD de plantillas de notificacion | Alta | P0 | HU022, HU037 | Documentado |

---

## Reglas de Negocio

| ID | Regla | Detalle | Decision |
|----|-------|---------|----------|
| RN-CFG-01 | Invalidacion de cache via evento async | Al crear, editar o eliminar un parametro, Config Service publica `ParameterUpdatedEvent` a RabbitMQ. Cada servicio consumidor invalida la key afectada en su cache local. TTL backup de 5 minutos como fallback. | D-CFG-01 |
| RN-CFG-02 | Encriptacion de credenciales AES-256 | Los valores en `service_auth_configs.value_encrypted` se encriptan con AES-256. La clave de encriptacion se obtiene de variable de entorno. Nunca se exponen valores desencriptados en respuestas GET. | D-CFG-03 |
| RN-CFG-03 | parameter_groups es tabla global | `parameter_groups` no tiene `tenant_id` ni RLS. Es seed data compartido entre todos los tenants. Los parametros dentro de cada grupo SI son por tenant. | — |
| RN-CFG-04 | Unicidad de key por grupo y tenant | `parameters(tenant_id, group_id, key)` es UNIQUE. Un parametro no puede duplicar su key dentro del mismo grupo y tenant. | — |
| RN-CFG-05 | Unicidad de nombre de servicio por tenant | `external_services(tenant_id, name)` es UNIQUE. No pueden existir dos servicios con el mismo nombre en un tenant. | — |
| RN-CFG-06 | Grupo no eliminable con parametros | Un grupo de parametros solo puede eliminarse si no tiene parametros asociados. Si tiene parametros, se retorna 409. | — |
| RN-CFG-07 | Credenciales por esquema de autenticacion | Cada `auth_type` requiere un conjunto especifico de keys en `service_auth_configs`: `api_key` → {header_name, api_key}; `bearer_token` → {token}; `basic_auth` → {username, password}; `oauth2` → {client_id, client_secret, token_url, scopes}; `custom_header` → {header_name, header_value}; `none` → sin registros. | — |
| RN-CFG-08 | Prueba de conexion no bloquea operaciones | El resultado de una prueba de conexion (exito o fallo) actualiza `last_tested_at` y `last_test_success` pero no cambia el `status` del servicio a menos que falle, en cuyo caso `status = 'error'`. Un servicio en `status = 'error'` sigue siendo utilizable. | — |
| RN-CFG-09 | Parametros con relacion jerarquica | Parametros con `parent_key` no nulo representan relaciones jerarquicas (ej: ciudades filtradas por provincia). El frontend filtra parametros hijos usando `GET /parameters?group_id=xxx&parent_key=yyy`. | — |
| RN-CFG-10 | Tipos de parametro soportados | Los tipos validos son: `text`, `number`, `boolean`, `select`, `list`, `html`. Los parametros de tipo `select` y `list` requieren opciones en `parameter_options`. | — |
| RN-CFG-11 | Logging excluye credenciales | El sistema de logging nunca registra valores de `service_auth_configs`. Los eventos de auditoria registran la accion pero no los valores de credenciales. | — |
| RN-CFG-12 | Ciclo de estados de workflow | `workflow_status`: draft → active → inactive. Edicion de workflow activo revierte automaticamente a draft. | — |
| RN-CFG-13 | Guardado draft sin validacion de grafo | Al guardar como draft, no se aplican reglas de validacion de grafo (nodos, conexiones, ciclos). Solo se valida al activar (RF-CFG-17). | — |
| RN-CFG-14 | Reglas de validacion de grafo | Exactamente 1 nodo `start`, al menos 1 nodo `end`, todos los nodos conectados (sin huerfanos), sin ciclos que no pasen por un nodo `end`. | — |
| RN-CFG-15 | Version se incrementa al activar | `workflow_definitions.version` se incrementa al activar (PUT /activate), no al guardar como draft. | — |
| RN-CFG-16 | Validacion de referencias al activar | Al activar, se valida que los `external_services` (nodos service_call) y `notification_templates` (nodos send_message) referenciados existan y esten activos. | — |
| RN-CFG-17 | Solicitudes en curso mantienen version vigente | Al desactivar o editar un workflow activo, las solicitudes en curso continuan con la version anterior. Nueva version solo aplica a nuevas solicitudes. | — |
| RN-CFG-18 | Catalogo de atributos estatico | El catalogo de atributos del dominio es estatico en frontend (11 objetos, ~68 atributos). Formato de referencia: `{{Objeto.atributo}}`. | — |
| RN-CFG-19 | Persistencia de configuracion por tipo de nodo | Configuracion de nodos se persiste en `workflow_state_configs` (key-value) excepto `data_capture` que usa `workflow_state_fields`. | — |
| RN-CFG-20 | Unicidad de codigo de plantilla por tenant | `notification_templates(tenant_id, code)` es UNIQUE. No pueden existir dos plantillas con el mismo codigo en un tenant. | — |
| RN-CFG-21 | Plantilla requiere al menos un canal | Toda plantilla debe tener al menos un canal habilitado (email, sms, whatsapp). El body puede contener variables con formato `{{variable}}`. | — |

---

## Requerimientos Funcionales

---

### RF-CFG-01 — Listar grupos de parametros por categoria

#### Execution Sheet

| Campo | Valor |
|-------|-------|
| **ID** | RF-CFG-01 |
| **Titulo** | Listar grupos de parametros organizados por categoria |
| **Actor(es)** | Super Admin |
| **Prioridad** | Alta |
| **Severidad** | P1 |
| **Flujo origen** | FL-CFG-01 seccion 6 |
| **HU origen** | HU020 |

#### Precondiciones

| # | Condicion | Detalle |
|---|-----------|---------|
| 1 | Config Service operativo | SA.config accesible y saludable |
| 2 | `parameter_groups` inicializado | Seed data con 14+ grupos en 4 categorias (general, tecnico, notificaciones, datos) |
| 3 | Usuario autenticado como Super Admin | JWT valido con permiso `p_cfg_param_list` |

#### Entradas

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| `tenant_id` | uuid | Si | JWT claim `tenant_id` | UUID valido, tenant activo | — |

#### Proceso (Happy Path)

| # | Paso | Responsable |
|---|------|-------------|
| 1 | Recibir `GET /parameter-groups` | API Gateway |
| 2 | Verificar permiso `p_cfg_param_list` en JWT claims | API Gateway |
| 3 | Ejecutar `SET LOCAL app.current_tenant = '{tenant_id}'` | Config Service |
| 4 | SELECT de `parameter_groups` ordenado por `category`, `sort_order` | Config Service |
| 5 | Agrupar resultados por `category` en el response | Config Service |
| 6 | Retornar `200 OK` con `{ categories: [{ name, groups: [{ id, code, name, icon, sort_order }] }] }` | Config Service |

#### Salidas

| Campo | Tipo | Destino | Efecto observable |
|-------|------|---------|-------------------|
| `categories[]` | array | Response body → SPA | Lista de categorias con sus grupos anidados |
| `categories[].name` | string | Response body → SPA | Nombre de la categoria (general, tecnico, notificaciones, datos) |
| `categories[].groups[]` | array | Response body → SPA | Grupos de parametros dentro de la categoria |

#### Errores Tipados

| Codigo | Causa | Condicion de disparo | Respuesta esperada |
|--------|-------|---------------------|--------------------|
| `CFG_UNAUTHORIZED` | Sin permiso | JWT no contiene `p_cfg_param_list` | HTTP 403, mensaje: "No tiene permisos para acceder a parametros" |
| `CFG_UNAUTHENTICATED` | Token invalido | JWT ausente, expirado o malformado | HTTP 401, mensaje: "Autenticacion requerida" |

#### Casos Especiales y Variantes

- **parameter_groups es tabla global:** No aplica RLS. Todos los tenants ven los mismos grupos. El contenido de cada grupo (parametros) SI es por tenant.
- **Sin grupos:** Si la seed data no se ejecuto, se retorna `200` con `categories: []`. No es un error.

#### Impacto en Modelo de Datos

| Entidad | Operacion | Campo(s) afectado(s) | Evento |
|---------|-----------|---------------------|--------|
| `parameter_groups` | SELECT | `id`, `code`, `name`, `category`, `icon`, `sort_order` | — |

#### Criterios de Aceptacion (Gherkin)

```gherkin
Scenario: Listar grupos de parametros exitosamente
  Given el seed data de parameter_groups esta cargado con 14 grupos en 4 categorias
  And el usuario tiene permiso p_cfg_param_list
  When envio GET /parameter-groups
  Then recibo HTTP 200 con categories[] agrupados por nombre de categoria
  And cada grupo incluye id, code, name, icon y sort_order
  And los grupos estan ordenados por sort_order dentro de cada categoria

Scenario: Acceso denegado sin permiso
  Given el usuario NO tiene permiso p_cfg_param_list
  When envio GET /parameter-groups
  Then recibo HTTP 403 con codigo CFG_UNAUTHORIZED
```

#### Trazabilidad de Pruebas

| Test ID | Tipo | Descripcion |
|---------|------|-------------|
| TP-CFG-01-01 | Positivo | Listar grupos retorna 4 categorias con 14+ grupos ordenados |
| TP-CFG-01-02 | Positivo | Grupos ordenados por sort_order dentro de cada categoria |
| TP-CFG-01-03 | Negativo | 403 sin permiso p_cfg_param_list |
| TP-CFG-01-04 | Negativo | 401 sin JWT |
| TP-CFG-01-05 | Integracion | Seed data de parameter_groups presente tras migracion |
| TP-CFG-01-06 | E2E | Navegacion a /parametros muestra sidebar con grupos colapsados por categoria |

#### Sin Ambiguedades

- **Supuestos prohibidos:** No se asume que los grupos son dinamicos por tenant. No se asume que el frontend crea las categorias.
- **Decisiones cerradas:** `parameter_groups` es tabla global sin RLS (RN-CFG-03). Las categorias son 4 fijas: general, tecnico, notificaciones, datos.
- **Fuera de alcance explicito:** CRUD de categorias (las categorias son fijas por seed). Busqueda de grupos por texto.
- **TODO explicitos = 0**

---

### RF-CFG-02 — Listar parametros de un grupo

#### Execution Sheet

| Campo | Valor |
|-------|-------|
| **ID** | RF-CFG-02 |
| **Titulo** | Listar parametros de un grupo especifico |
| **Actor(es)** | Super Admin |
| **Prioridad** | Alta |
| **Severidad** | P1 |
| **Flujo origen** | FL-CFG-01 seccion 6 |
| **HU origen** | HU020 |

#### Precondiciones

| # | Condicion | Detalle |
|---|-----------|---------|
| 1 | Config Service operativo | SA.config accesible |
| 2 | Grupo de parametros existente | `group_id` valido en `parameter_groups` |
| 3 | Usuario autenticado como Super Admin | JWT con permiso `p_cfg_param_list` |

#### Entradas

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| `group_id` | uuid | Si | Query param | UUID valido, grupo existente en `parameter_groups` | — |
| `parent_key` | string | No | Query param | Si se provee, filtra parametros con `parent_key` coincidente | RN-CFG-09 |
| `tenant_id` | uuid | Si | JWT claim | UUID valido, tenant activo | — |

#### Proceso (Happy Path)

| # | Paso | Responsable |
|---|------|-------------|
| 1 | Recibir `GET /parameters?group_id=xxx` | API Gateway |
| 2 | Verificar permiso `p_cfg_param_list` en JWT | API Gateway |
| 3 | Ejecutar `SET LOCAL app.current_tenant = '{tenant_id}'` | Config Service |
| 4 | Validar que `group_id` existe en `parameter_groups` | Config Service |
| 5 | SELECT de `parameters` WHERE `group_id = :group_id` (RLS filtra por tenant) | Config Service |
| 6 | Si `parent_key` presente, agregar WHERE `parent_key = :parent_key` | Config Service |
| 7 | Para cada parametro tipo `select` o `list`, cargar `parameter_options` ordenadas por `sort_order` | Config Service |
| 8 | Retornar `200 OK` con `{ items: [{ id, key, value, type, description, parent_key, options[]? }] }` | Config Service |

#### Salidas

| Campo | Tipo | Destino | Efecto observable |
|-------|------|---------|-------------------|
| `items[]` | array | Response body → SPA | Lista de parametros del grupo |
| `items[].id` | uuid | Response body | Identificador del parametro |
| `items[].key` | string | Response body | Clave unica del parametro |
| `items[].value` | string | Response body | Valor actual del parametro |
| `items[].type` | enum | Response body | Tipo: text, number, boolean, select, list, html |
| `items[].description` | string | Response body | Descripcion del parametro |
| `items[].parent_key` | string? | Response body | Clave del padre jerarquico (nullable) |
| `items[].options[]` | array? | Response body | Opciones disponibles (solo para select y list) |

#### Errores Tipados

| Codigo | Causa | Condicion de disparo | Respuesta esperada |
|--------|-------|---------------------|--------------------|
| `CFG_GROUP_NOT_FOUND` | Grupo inexistente | `group_id` no existe en `parameter_groups` | HTTP 404, mensaje: "Grupo de parametros no encontrado" |
| `CFG_UNAUTHORIZED` | Sin permiso | JWT no contiene `p_cfg_param_list` | HTTP 403 |
| `CFG_VALIDATION_ERROR` | Entrada malformada | `group_id` no es UUID valido | HTTP 422, detalle de campos invalidos |

#### Casos Especiales y Variantes

- **Grupo vacio:** Si el grupo no tiene parametros para este tenant, retorna `200` con `items: []`.
- **Filtro jerarquico:** Cuando `parent_key` esta presente, solo se retornan parametros cuyo `parent_key` coincida. Ejemplo: `GET /parameters?group_id=ciudades&parent_key=buenos_aires` retorna solo ciudades de Buenos Aires.
- **Opciones de parametro:** Los parametros de tipo `select` y `list` incluyen el array `options[]` con `{ option_value, option_label, sort_order }`. Los demas tipos no incluyen este campo.

#### Impacto en Modelo de Datos

| Entidad | Operacion | Campo(s) afectado(s) | Evento |
|---------|-----------|---------------------|--------|
| `parameter_groups` | SELECT | `id` (validacion de existencia) | — |
| `parameters` | SELECT | `id`, `key`, `value`, `type`, `description`, `parent_key` | — |
| `parameter_options` | SELECT | `option_value`, `option_label`, `sort_order` | — |

#### Criterios de Aceptacion (Gherkin)

```gherkin
Scenario: Listar parametros de un grupo existente
  Given el grupo "provincias" existe con 24 parametros para el tenant actual
  And el usuario tiene permiso p_cfg_param_list
  When envio GET /parameters?group_id={id_provincias}
  Then recibo HTTP 200 con items[] conteniendo 24 parametros
  And cada item incluye id, key, value, type y description

Scenario: Listar parametros con filtro jerarquico
  Given el grupo "ciudades" tiene parametros con parent_key "buenos_aires" y "cordoba"
  When envio GET /parameters?group_id={id_ciudades}&parent_key=buenos_aires
  Then recibo HTTP 200 con items[] conteniendo solo ciudades de Buenos Aires

Scenario: Grupo inexistente retorna 404
  Given no existe un grupo con id "00000000-0000-0000-0000-000000000000"
  When envio GET /parameters?group_id=00000000-0000-0000-0000-000000000000
  Then recibo HTTP 404 con codigo CFG_GROUP_NOT_FOUND

Scenario: Parametro tipo select incluye opciones
  Given existe un parametro tipo "select" con 3 opciones
  When envio GET /parameters?group_id={grupo_con_select}
  Then el parametro tipo select incluye options[] con 3 elementos ordenados por sort_order
```

#### Trazabilidad de Pruebas

| Test ID | Tipo | Descripcion |
|---------|------|-------------|
| TP-CFG-02-01 | Positivo | Listar parametros de grupo existente retorna items con key, value, type |
| TP-CFG-02-02 | Positivo | Parametros tipo select incluyen options[] ordenadas |
| TP-CFG-02-03 | Positivo | Filtro parent_key retorna solo parametros hijos |
| TP-CFG-02-04 | Negativo | 404 para group_id inexistente |
| TP-CFG-02-05 | Negativo | 422 para group_id no UUID |
| TP-CFG-02-06 | Negativo | 403 sin permiso p_cfg_param_list |
| TP-CFG-02-07 | Integracion | RLS filtra parametros por tenant correctamente |
| TP-CFG-02-08 | E2E | Click en grupo "Provincias" en sidebar muestra tabla de parametros |

#### Sin Ambiguedades

- **Supuestos prohibidos:** No se asume que los parametros se cargan paginados (se cargan todos los del grupo). No se asume que el frontend conoce los IDs de grupo sin consultar RF-CFG-01 primero.
- **Decisiones cerradas:** Parametros se cargan completos por grupo (sin paginacion). Las opciones se incluyen inline en la respuesta.
- **Fuera de alcance explicito:** Busqueda full-text de parametros. Paginacion de parametros dentro de un grupo.
- **TODO explicitos = 0**

---

### RF-CFG-03 — Crear parametro

#### Execution Sheet

| Campo | Valor |
|-------|-------|
| **ID** | RF-CFG-03 |
| **Titulo** | Crear un nuevo parametro dentro de un grupo |
| **Actor(es)** | Super Admin |
| **Prioridad** | Alta |
| **Severidad** | P0 |
| **Flujo origen** | FL-CFG-01 seccion 6 (Agregar parametro) |
| **HU origen** | HU020 |

#### Precondiciones

| # | Condicion | Detalle |
|---|-----------|---------|
| 1 | Config Service operativo | SA.config accesible |
| 2 | RabbitMQ operativo | Para publicacion de ParameterUpdatedEvent |
| 3 | Grupo de parametros existente | `group_id` valido en `parameter_groups` |
| 4 | Usuario autenticado como Super Admin | JWT con permiso `p_cfg_param_create` |

#### Entradas

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| `group_id` | uuid | Si | Body JSON | UUID valido, grupo existente en `parameter_groups` | — |
| `key` | string | Si | Body JSON | No vacia, max 100 chars, unica por grupo y tenant | RN-CFG-04 |
| `value` | string | Si | Body JSON | No vacia, max 4000 chars | — |
| `type` | enum | Si | Body JSON | Uno de: text, number, boolean, select, list, html | RN-CFG-10 |
| `description` | string | Si | Body JSON | No vacia, max 500 chars | — |
| `parent_key` | string | No | Body JSON | Si se provee, max 100 chars | RN-CFG-09 |
| `options[]` | array | Condicional | Body JSON | Requerido si type = select o list. Cada opcion: { option_value: string, option_label: string } | RN-CFG-10 |
| `tenant_id` | uuid | Si | JWT claim | UUID valido | — |

#### Proceso (Happy Path)

| # | Paso | Responsable |
|---|------|-------------|
| 1 | Recibir `POST /parameters` con body JSON | API Gateway |
| 2 | Verificar permiso `p_cfg_param_create` en JWT | API Gateway |
| 3 | Ejecutar `SET LOCAL app.current_tenant = '{tenant_id}'` | Config Service |
| 4 | Validar que `group_id` existe en `parameter_groups` | Config Service |
| 5 | Validar unicidad de `key` en el grupo para el tenant (indice UNIQUE) | Config Service |
| 6 | Validar que `type` es un valor valido del enum `parameter_type` | Config Service |
| 7 | Si `type` es `select` o `list`, validar que `options[]` no esta vacio | Config Service |
| 8 | INSERT en `parameters` con `id` (UUIDv7), `tenant_id`, `group_id`, `key`, `value`, `type`, `description`, `parent_key`, `updated_at = NOW()`, `updated_by = user_id` | Config Service |
| 9 | Si `options[]` presente, INSERT en `parameter_options` para cada opcion con `sort_order` incremental | Config Service |
| 10 | Publicar `ParameterUpdatedEvent` a RabbitMQ con `{ group_code, key, tenant_id, action: 'created' }` | Config Service |
| 11 | Retornar `201 Created` con `{ id, key, value, type, description, parent_key, options[]? }` | Config Service |

#### Salidas

| Campo | Tipo | Destino | Efecto observable |
|-------|------|---------|-------------------|
| `201 Created` | HTTP status | Response → SPA | Parametro creado exitosamente |
| `id` | uuid | Response body | ID del parametro creado |
| `ParameterUpdatedEvent` | evento async | RabbitMQ → Audit Service, Servicios consumidores | Registra creacion en audit_log; servicios invalidan cache |

#### Errores Tipados

| Codigo | Causa | Condicion de disparo | Respuesta esperada |
|--------|-------|---------------------|--------------------|
| `CFG_GROUP_NOT_FOUND` | Grupo inexistente | `group_id` no existe en `parameter_groups` | HTTP 404 |
| `CFG_DUPLICATE_KEY` | Key duplicada | `parameters(tenant_id, group_id, key)` ya existe | HTTP 409, mensaje: "Ya existe un parametro con la key '{key}' en este grupo" |
| `CFG_VALIDATION_ERROR` | Entrada invalida | Campo requerido vacio, type invalido, options vacio para select/list | HTTP 422, detalle de campos invalidos |
| `CFG_UNAUTHORIZED` | Sin permiso | JWT no contiene `p_cfg_param_create` | HTTP 403 |

#### Casos Especiales y Variantes

- **Tipo select/list sin opciones:** Si `type` es `select` o `list` y `options[]` esta vacio o ausente, se retorna `CFG_VALIDATION_ERROR` con mensaje indicando que las opciones son requeridas para ese tipo.
- **Tipo number:** El `value` debe ser parseable a numero. Si no lo es, se retorna `CFG_VALIDATION_ERROR`.
- **Tipo boolean:** El `value` debe ser `"true"` o `"false"`. Si no lo es, se retorna `CFG_VALIDATION_ERROR`.
- **parent_key inexistente:** No se valida que `parent_key` referencie un parametro existente. Es una clave logica usada para filtrado.

#### Impacto en Modelo de Datos

| Entidad | Operacion | Campo(s) afectado(s) | Evento |
|---------|-----------|---------------------|--------|
| `parameter_groups` | SELECT | `id` (validacion) | — |
| `parameters` | INSERT | todos los campos | `ParameterUpdatedEvent` |
| `parameter_options` | INSERT (condicional) | `parameter_id`, `option_value`, `option_label`, `sort_order` | — |
| `audit_log` (SA.audit) | INSERT (async) | operation = 'Crear', module = 'parametros' | consume `ParameterUpdatedEvent` |

#### Criterios de Aceptacion (Gherkin)

```gherkin
Scenario: Crear parametro tipo text exitosamente
  Given el grupo "general" existe y no tiene parametro con key "nuevo_param"
  And el usuario tiene permiso p_cfg_param_create
  When envio POST /parameters con group_id, key="nuevo_param", value="valor", type="text", description="desc"
  Then recibo HTTP 201 con id del parametro creado
  And el parametro se persiste en DB con tenant_id del JWT
  And se publica ParameterUpdatedEvent con action "created"

Scenario: Crear parametro tipo select con opciones
  Given el grupo "datos" existe
  When envio POST /parameters con type="select" y options=[{option_value:"a", option_label:"Opcion A"}]
  Then recibo HTTP 201 y el parametro se crea con sus opciones

Scenario: Key duplicada retorna 409
  Given ya existe un parametro con key "existente" en el grupo "general" para el tenant actual
  When envio POST /parameters con key="existente" en el mismo grupo
  Then recibo HTTP 409 con codigo CFG_DUPLICATE_KEY

Scenario: Tipo select sin opciones retorna 422
  When envio POST /parameters con type="select" sin campo options
  Then recibo HTTP 422 con codigo CFG_VALIDATION_ERROR
```

#### Trazabilidad de Pruebas

| Test ID | Tipo | Descripcion |
|---------|------|-------------|
| TP-CFG-03-01 | Positivo | Crear parametro tipo text retorna 201 |
| TP-CFG-03-02 | Positivo | Crear parametro tipo select con opciones persiste opciones |
| TP-CFG-03-03 | Positivo | Crear parametro con parent_key persiste relacion jerarquica |
| TP-CFG-03-04 | Negativo | 409 por key duplicada en mismo grupo y tenant |
| TP-CFG-03-05 | Negativo | 404 por group_id inexistente |
| TP-CFG-03-06 | Negativo | 422 por type select sin opciones |
| TP-CFG-03-07 | Negativo | 422 por campos requeridos vacios |
| TP-CFG-03-08 | Negativo | 403 sin permiso p_cfg_param_create |
| TP-CFG-03-09 | Integracion | ParameterUpdatedEvent publicado y consumido por Audit |
| TP-CFG-03-10 | Integracion | RLS asegura que el parametro se crea con tenant_id del JWT |
| TP-CFG-03-11 | E2E | Click "Agregar Parametro" → formulario → submit → parametro visible en tabla |

#### Sin Ambiguedades

- **Supuestos prohibidos:** No se asume que el frontend valida unicidad de key antes de enviar. No se asume que `parent_key` referencia un parametro existente.
- **Decisiones cerradas:** El `value` se almacena siempre como string. La validacion de tipo (number, boolean) es responsabilidad del backend al crear/editar.
- **Fuera de alcance explicito:** Valores por defecto automaticos. Parametros de solo lectura (read-only).
- **TODO explicitos = 0**

---

### RF-CFG-04 — Editar parametro inline

#### Execution Sheet

| Campo | Valor |
|-------|-------|
| **ID** | RF-CFG-04 |
| **Titulo** | Editar valor de un parametro existente (edicion inline) |
| **Actor(es)** | Super Admin |
| **Prioridad** | Alta |
| **Severidad** | P0 |
| **Flujo origen** | FL-CFG-01 seccion 6 (Editar valor inline) |
| **HU origen** | HU020 |

#### Precondiciones

| # | Condicion | Detalle |
|---|-----------|---------|
| 1 | Config Service operativo | SA.config accesible |
| 2 | RabbitMQ operativo | Para publicacion de ParameterUpdatedEvent |
| 3 | Parametro existente | `id` valido en `parameters` para el tenant actual |
| 4 | Usuario autenticado como Super Admin | JWT con permiso `p_cfg_param_edit` |

#### Entradas

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| `id` | uuid | Si | Path param `/parameters/:id` | UUID valido, parametro existente y perteneciente al tenant | — |
| `value` | string | Si | Body JSON | No vacia, max 4000 chars, coherente con `type` del parametro | — |
| `options[]` | array | Condicional | Body JSON | Si el parametro es tipo select/list, se pueden actualizar opciones | RN-CFG-10 |
| `tenant_id` | uuid | Si | JWT claim | UUID valido | — |

#### Proceso (Happy Path)

| # | Paso | Responsable |
|---|------|-------------|
| 1 | Recibir `PUT /parameters/:id` con body JSON | API Gateway |
| 2 | Verificar permiso `p_cfg_param_edit` en JWT | API Gateway |
| 3 | Ejecutar `SET LOCAL app.current_tenant = '{tenant_id}'` | Config Service |
| 4 | SELECT parametro por `id` (RLS filtra por tenant) | Config Service |
| 5 | Validar que `value` es coherente con el `type` del parametro | Config Service |
| 6 | UPDATE `parameters` SET `value = :value`, `updated_at = NOW()`, `updated_by = :user_id` | Config Service |
| 7 | Si `options[]` presente y tipo es select/list: DELETE existentes + INSERT nuevas | Config Service |
| 8 | Publicar `ParameterUpdatedEvent` a RabbitMQ con `{ group_code, key, tenant_id, action: 'updated' }` | Config Service |
| 9 | Retornar `200 OK` con `{ id, key, value, type, description, parent_key, options[]? }` | Config Service |

#### Salidas

| Campo | Tipo | Destino | Efecto observable |
|-------|------|---------|-------------------|
| `200 OK` | HTTP status | Response → SPA | Parametro actualizado exitosamente |
| `ParameterUpdatedEvent` | evento async | RabbitMQ → Audit, Servicios consumidores | Cache invalidado en servicios consumidores |

#### Errores Tipados

| Codigo | Causa | Condicion de disparo | Respuesta esperada |
|--------|-------|---------------------|--------------------|
| `CFG_PARAMETER_NOT_FOUND` | Parametro inexistente | `id` no existe o no pertenece al tenant (RLS) | HTTP 404, mensaje: "Parametro no encontrado" |
| `CFG_VALIDATION_ERROR` | Valor incoherente con tipo | `value` no es parseable al tipo del parametro | HTTP 422, detalle |
| `CFG_UNAUTHORIZED` | Sin permiso | JWT no contiene `p_cfg_param_edit` | HTTP 403 |

#### Casos Especiales y Variantes

- **Edicion inline:** El frontend envia solo el campo `value` modificado. No se permite cambiar `key`, `type`, `group_id` ni `parent_key` via este endpoint.
- **Actualizacion de opciones:** Si se envian `options[]` para un parametro select/list, se hace DELETE + INSERT (replace all). Si no se envian, las opciones existentes se mantienen intactas.

#### Impacto en Modelo de Datos

| Entidad | Operacion | Campo(s) afectado(s) | Evento |
|---------|-----------|---------------------|--------|
| `parameters` | SELECT | `id`, `type` (validacion) | — |
| `parameters` | UPDATE | `value`, `updated_at`, `updated_by` | `ParameterUpdatedEvent` |
| `parameter_options` | DELETE + INSERT (condicional) | todas las columnas | — |
| `audit_log` (SA.audit) | INSERT (async) | operation = 'Editar', module = 'parametros' | consume `ParameterUpdatedEvent` |

#### Criterios de Aceptacion (Gherkin)

```gherkin
Scenario: Editar valor de parametro exitosamente
  Given existe un parametro con id "param-001" tipo text y value "antiguo"
  And el usuario tiene permiso p_cfg_param_edit
  When envio PUT /parameters/param-001 con value="nuevo"
  Then recibo HTTP 200 con value="nuevo"
  And updated_at se actualiza
  And se publica ParameterUpdatedEvent con action "updated"

Scenario: Editar parametro inexistente retorna 404
  When envio PUT /parameters/00000000-0000-0000-0000-000000000000 con value="x"
  Then recibo HTTP 404 con codigo CFG_PARAMETER_NOT_FOUND

Scenario: Valor incoherente con tipo number retorna 422
  Given existe un parametro tipo "number"
  When envio PUT /parameters/:id con value="no_es_numero"
  Then recibo HTTP 422 con codigo CFG_VALIDATION_ERROR

Scenario: Editar opciones de parametro select
  Given existe un parametro tipo "select" con 2 opciones
  When envio PUT /parameters/:id con options=[3 nuevas opciones]
  Then las 2 opciones anteriores se eliminan y se insertan las 3 nuevas
```

#### Trazabilidad de Pruebas

| Test ID | Tipo | Descripcion |
|---------|------|-------------|
| TP-CFG-04-01 | Positivo | Editar value de parametro text retorna 200 |
| TP-CFG-04-02 | Positivo | Editar value actualiza updated_at y updated_by |
| TP-CFG-04-03 | Positivo | Editar opciones de parametro select reemplaza correctamente |
| TP-CFG-04-04 | Negativo | 404 para parametro inexistente |
| TP-CFG-04-05 | Negativo | 422 para value incoherente con tipo |
| TP-CFG-04-06 | Negativo | 403 sin permiso p_cfg_param_edit |
| TP-CFG-04-07 | Integracion | ParameterUpdatedEvent publicado y cache invalidado |
| TP-CFG-04-08 | Integracion | RLS impide editar parametro de otro tenant |
| TP-CFG-04-09 | E2E | Click en valor de parametro → edicion inline → guardar → valor actualizado en tabla |

#### Sin Ambiguedades

- **Supuestos prohibidos:** No se asume que el frontend envia todos los campos del parametro. Solo `value` y opcionalmente `options[]`. No se permite cambiar `key` ni `type` via PUT.
- **Decisiones cerradas:** La edicion inline solo modifica `value`. Para cambios de `key`, `type` o `group_id` se requiere eliminar y recrear.
- **Fuera de alcance explicito:** Edicion de key. Edicion de type. Edicion de group_id. Edicion de parent_key.
- **TODO explicitos = 0**

---

### RF-CFG-05 — Eliminar parametro

#### Execution Sheet

| Campo | Valor |
|-------|-------|
| **ID** | RF-CFG-05 |
| **Titulo** | Eliminar un parametro existente |
| **Actor(es)** | Super Admin |
| **Prioridad** | Alta |
| **Severidad** | P1 |
| **Flujo origen** | FL-CFG-01 seccion 8a |
| **HU origen** | HU020 |

#### Precondiciones

| # | Condicion | Detalle |
|---|-----------|---------|
| 1 | Config Service operativo | SA.config accesible |
| 2 | RabbitMQ operativo | Para publicacion de ParameterUpdatedEvent |
| 3 | Parametro existente | `id` valido en `parameters` para el tenant actual |
| 4 | Usuario autenticado como Super Admin | JWT con permiso `p_cfg_param_delete` |

#### Entradas

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| `id` | uuid | Si | Path param `/parameters/:id` | UUID valido, parametro existente y perteneciente al tenant | — |
| `tenant_id` | uuid | Si | JWT claim | UUID valido | — |

#### Proceso (Happy Path)

| # | Paso | Responsable |
|---|------|-------------|
| 1 | Recibir `DELETE /parameters/:id` | API Gateway |
| 2 | Verificar permiso `p_cfg_param_delete` en JWT | API Gateway |
| 3 | Ejecutar `SET LOCAL app.current_tenant = '{tenant_id}'` | Config Service |
| 4 | SELECT parametro por `id` (RLS filtra por tenant) | Config Service |
| 5 | DELETE de `parameter_options` WHERE `parameter_id = :id` (si existen) | Config Service |
| 6 | DELETE de `parameters` WHERE `id = :id` | Config Service |
| 7 | Publicar `ParameterUpdatedEvent` a RabbitMQ con `{ group_code, key, tenant_id, action: 'deleted' }` | Config Service |
| 8 | Retornar `204 No Content` | Config Service |

#### Salidas

| Campo | Tipo | Destino | Efecto observable |
|-------|------|---------|-------------------|
| `204 No Content` | HTTP status | Response → SPA | Parametro eliminado exitosamente |
| `ParameterUpdatedEvent` | evento async | RabbitMQ → Audit, Servicios consumidores | Registro en audit_log; cache invalidado |

#### Errores Tipados

| Codigo | Causa | Condicion de disparo | Respuesta esperada |
|--------|-------|---------------------|--------------------|
| `CFG_PARAMETER_NOT_FOUND` | Parametro inexistente | `id` no existe o no pertenece al tenant | HTTP 404 |
| `CFG_UNAUTHORIZED` | Sin permiso | JWT no contiene `p_cfg_param_delete` | HTTP 403 |

#### Casos Especiales y Variantes

- **Eliminacion fisica:** No se usa soft delete. El parametro y sus opciones se eliminan fisicamente de la base de datos. El evento de auditoria queda como registro historico.
- **Parametros hijos:** Si otros parametros tienen `parent_key` apuntando al parametro eliminado, NO se eliminan en cascada. Quedan huerfanos. El frontend debe manejar este caso.
- **Confirmacion en UI:** El frontend muestra un dialogo de confirmacion antes de enviar el DELETE. Esto no es responsabilidad del backend.

#### Impacto en Modelo de Datos

| Entidad | Operacion | Campo(s) afectado(s) | Evento |
|---------|-----------|---------------------|--------|
| `parameters` | SELECT | `id` (validacion) | — |
| `parameter_options` | DELETE | todas las filas del parametro | — |
| `parameters` | DELETE | fila completa | `ParameterUpdatedEvent` |
| `audit_log` (SA.audit) | INSERT (async) | operation = 'Eliminar', module = 'parametros' | consume `ParameterUpdatedEvent` |

#### Criterios de Aceptacion (Gherkin)

```gherkin
Scenario: Eliminar parametro exitosamente
  Given existe un parametro con id "param-001" para el tenant actual
  And el usuario tiene permiso p_cfg_param_delete
  When envio DELETE /parameters/param-001
  Then recibo HTTP 204
  And el parametro y sus opciones se eliminan de la DB
  And se publica ParameterUpdatedEvent con action "deleted"

Scenario: Eliminar parametro con opciones elimina opciones tambien
  Given existe un parametro tipo "select" con 3 opciones
  When envio DELETE /parameters/:id
  Then recibo HTTP 204
  And las 3 opciones en parameter_options tambien se eliminan

Scenario: Eliminar parametro inexistente retorna 404
  When envio DELETE /parameters/00000000-0000-0000-0000-000000000000
  Then recibo HTTP 404 con codigo CFG_PARAMETER_NOT_FOUND
```

#### Trazabilidad de Pruebas

| Test ID | Tipo | Descripcion |
|---------|------|-------------|
| TP-CFG-05-01 | Positivo | Eliminar parametro retorna 204 |
| TP-CFG-05-02 | Positivo | Eliminar parametro tipo select elimina opciones en cascada |
| TP-CFG-05-03 | Negativo | 404 para parametro inexistente |
| TP-CFG-05-04 | Negativo | 403 sin permiso p_cfg_param_delete |
| TP-CFG-05-05 | Integracion | ParameterUpdatedEvent publicado y consumido por Audit |
| TP-CFG-05-06 | Integracion | Servicios consumidores invalidan cache al recibir evento de eliminacion |
| TP-CFG-05-07 | E2E | Hover parametro → click eliminar → confirmar → parametro desaparece de tabla |

#### Sin Ambiguedades

- **Supuestos prohibidos:** No se asume que la eliminacion es logica (soft delete). No se asume que los parametros hijos se eliminan en cascada.
- **Decisiones cerradas:** Eliminacion fisica (modelo de datos seccion "Soft delete: no se usa"). Opciones se eliminan por FK cascade o por delete explicito previo.
- **Fuera de alcance explicito:** Eliminacion masiva de parametros. Undo/restaurar parametro eliminado.
- **TODO explicitos = 0**

---

### RF-CFG-06 — Gestionar grupos de parametros

#### Execution Sheet

| Campo | Valor |
|-------|-------|
| **ID** | RF-CFG-06 |
| **Titulo** | Crear y eliminar grupos de parametros |
| **Actor(es)** | Super Admin |
| **Prioridad** | Media |
| **Severidad** | P2 |
| **Flujo origen** | FL-CFG-01 seccion 8b |
| **HU origen** | HU020 |

#### Precondiciones

| # | Condicion | Detalle |
|---|-----------|---------|
| 1 | Config Service operativo | SA.config accesible |
| 2 | Usuario autenticado como Super Admin | JWT con permiso `p_cfg_group_manage` |

#### Entradas — Crear grupo

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| `code` | string | Si | Body JSON | No vacio, max 50 chars, unico en `parameter_groups` | — |
| `name` | string | Si | Body JSON | No vacio, max 100 chars | — |
| `category` | enum | Si | Body JSON | Uno de: general, tecnico, notificaciones, datos | — |
| `icon` | string | Si | Body JSON | No vacio, nombre de icono valido | — |
| `sort_order` | integer | Si | Body JSON | >= 0 | — |

#### Entradas — Eliminar grupo

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| `id` | uuid | Si | Path param `/parameter-groups/:id` | UUID valido, grupo existente, sin parametros asociados | RN-CFG-06 |

#### Proceso (Happy Path) — Crear grupo

| # | Paso | Responsable |
|---|------|-------------|
| 1 | Recibir `POST /parameter-groups` con body JSON | API Gateway |
| 2 | Verificar permiso `p_cfg_group_manage` en JWT | API Gateway |
| 3 | Validar unicidad de `code` en `parameter_groups` (indice UNIQUE) | Config Service |
| 4 | INSERT en `parameter_groups` con `id` (UUIDv7), `code`, `name`, `category`, `icon`, `sort_order` | Config Service |
| 5 | Retornar `201 Created` con `{ id, code, name, category, icon, sort_order }` | Config Service |

#### Proceso (Happy Path) — Eliminar grupo

| # | Paso | Responsable |
|---|------|-------------|
| 1 | Recibir `DELETE /parameter-groups/:id` | API Gateway |
| 2 | Verificar permiso `p_cfg_group_manage` en JWT | API Gateway |
| 3 | SELECT de `parameters` WHERE `group_id = :id` (para cualquier tenant) | Config Service |
| 4 | Si existen parametros, retornar 409 | Config Service |
| 5 | DELETE de `parameter_groups` WHERE `id = :id` | Config Service |
| 6 | Retornar `204 No Content` | Config Service |

#### Salidas

| Campo | Tipo | Destino | Efecto observable |
|-------|------|---------|-------------------|
| `201 Created` | HTTP status | Response → SPA | Grupo creado (para crear) |
| `204 No Content` | HTTP status | Response → SPA | Grupo eliminado (para eliminar) |

#### Errores Tipados

| Codigo | Causa | Condicion de disparo | Respuesta esperada |
|--------|-------|---------------------|--------------------|
| `CFG_DUPLICATE_GROUP_CODE` | Code duplicado | `parameter_groups.code` ya existe | HTTP 409, mensaje: "Ya existe un grupo con el codigo '{code}'" |
| `CFG_GROUP_HAS_PARAMETERS` | Grupo con parametros | Existen registros en `parameters` para ese `group_id` | HTTP 409, mensaje: "No se puede eliminar un grupo con parametros" |
| `CFG_GROUP_NOT_FOUND` | Grupo inexistente | `id` no existe en `parameter_groups` | HTTP 404 |
| `CFG_VALIDATION_ERROR` | Entrada invalida | Campos requeridos vacios o category invalida | HTTP 422 |
| `CFG_UNAUTHORIZED` | Sin permiso | JWT no contiene `p_cfg_group_manage` | HTTP 403 |

#### Casos Especiales y Variantes

- **Tabla global:** `parameter_groups` no tiene `tenant_id` ni RLS. Un grupo creado es visible para todos los tenants. Crear y eliminar grupos es una operacion de administracion global.
- **Verificacion cross-tenant en eliminacion:** Al eliminar un grupo, se verifica que no tenga parametros en NINGUN tenant (query sin RLS context). Esto requiere que la verificacion se ejecute sin `SET LOCAL app.current_tenant`.
- **Edicion de grupo:** No se contempla edicion. Si se necesita cambiar un grupo, se elimina (si no tiene parametros) y se recrea.

#### Impacto en Modelo de Datos

| Entidad | Operacion | Campo(s) afectado(s) | Evento |
|---------|-----------|---------------------|--------|
| `parameter_groups` | INSERT (crear) | todos los campos | — |
| `parameter_groups` | DELETE (eliminar) | fila completa | — |
| `parameters` | SELECT (validacion eliminar) | `group_id` | — |

#### Criterios de Aceptacion (Gherkin)

```gherkin
Scenario: Crear grupo de parametros exitosamente
  Given no existe un grupo con code "nuevo_grupo"
  And el usuario tiene permiso p_cfg_group_manage
  When envio POST /parameter-groups con code="nuevo_grupo", name="Nuevo Grupo", category="general"
  Then recibo HTTP 201 con id del grupo creado

Scenario: Crear grupo con code duplicado retorna 409
  Given ya existe un grupo con code "provincias"
  When envio POST /parameter-groups con code="provincias"
  Then recibo HTTP 409 con codigo CFG_DUPLICATE_GROUP_CODE

Scenario: Eliminar grupo sin parametros exitosamente
  Given existe un grupo sin parametros en ningun tenant
  When envio DELETE /parameter-groups/:id
  Then recibo HTTP 204

Scenario: Eliminar grupo con parametros retorna 409
  Given existe un grupo con parametros asociados
  When envio DELETE /parameter-groups/:id
  Then recibo HTTP 409 con codigo CFG_GROUP_HAS_PARAMETERS
```

#### Trazabilidad de Pruebas

| Test ID | Tipo | Descripcion |
|---------|------|-------------|
| TP-CFG-06-01 | Positivo | Crear grupo retorna 201 |
| TP-CFG-06-02 | Positivo | Eliminar grupo sin parametros retorna 204 |
| TP-CFG-06-03 | Negativo | 409 por code duplicado |
| TP-CFG-06-04 | Negativo | 409 al eliminar grupo con parametros |
| TP-CFG-06-05 | Negativo | 404 al eliminar grupo inexistente |
| TP-CFG-06-06 | Negativo | 403 sin permiso p_cfg_group_manage |
| TP-CFG-06-07 | Integracion | Grupo creado es visible para todos los tenants |
| TP-CFG-06-08 | E2E | Crear grupo → aparece en sidebar bajo la categoria correspondiente |

#### Sin Ambiguedades

- **Supuestos prohibidos:** No se asume que los grupos son por tenant. Son globales. No se asume que la edicion de grupos existe.
- **Decisiones cerradas:** `parameter_groups` es tabla global sin `tenant_id` (05_modelo_datos.md). Eliminacion bloqueada si tiene parametros en cualquier tenant (RN-CFG-06).
- **Fuera de alcance explicito:** Edicion de grupos existentes. Reordenamiento drag-and-drop de grupos.
- **TODO explicitos = 0**

---

### RF-CFG-07 — Filtrar parametros por relacion jerarquica

#### Execution Sheet

| Campo | Valor |
|-------|-------|
| **ID** | RF-CFG-07 |
| **Titulo** | Filtrar parametros por relacion jerarquica (parent_key) |
| **Actor(es)** | Super Admin, Servicios consumidores (via API interna) |
| **Prioridad** | Media |
| **Severidad** | P2 |
| **Flujo origen** | FL-CFG-01 seccion 8c |
| **HU origen** | HU020 |

#### Precondiciones

| # | Condicion | Detalle |
|---|-----------|---------|
| 1 | Config Service operativo | SA.config accesible |
| 2 | Parametros con relacion jerarquica configurados | Existen parametros con `parent_key` no nulo |
| 3 | Usuario autenticado | JWT con permiso `p_cfg_param_list` |

#### Entradas

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| `group_id` | uuid | Si | Query param | UUID valido, grupo existente | — |
| `parent_key` | string | Si | Query param | No vacio, max 100 chars | RN-CFG-09 |
| `tenant_id` | uuid | Si | JWT claim | UUID valido | — |

#### Proceso (Happy Path)

| # | Paso | Responsable |
|---|------|-------------|
| 1 | Recibir `GET /parameters?group_id=xxx&parent_key=yyy` | API Gateway |
| 2 | Verificar permiso `p_cfg_param_list` en JWT | API Gateway |
| 3 | Ejecutar `SET LOCAL app.current_tenant = '{tenant_id}'` | Config Service |
| 4 | SELECT de `parameters` WHERE `group_id = :group_id AND parent_key = :parent_key` (indice `parameters(tenant_id, parent_key)`) | Config Service |
| 5 | Retornar `200 OK` con `{ items: [{ id, key, value, type, description, parent_key }] }` | Config Service |

#### Salidas

| Campo | Tipo | Destino | Efecto observable |
|-------|------|---------|-------------------|
| `items[]` | array | Response body → SPA o Servicio consumidor | Solo parametros hijos del parent_key indicado |

#### Errores Tipados

| Codigo | Causa | Condicion de disparo | Respuesta esperada |
|--------|-------|---------------------|--------------------|
| `CFG_GROUP_NOT_FOUND` | Grupo inexistente | `group_id` no existe | HTTP 404 |
| `CFG_VALIDATION_ERROR` | parent_key vacio | `parent_key` presente pero vacio | HTTP 422 |
| `CFG_UNAUTHORIZED` | Sin permiso | JWT no contiene `p_cfg_param_list` | HTTP 403 |

#### Casos Especiales y Variantes

- **Sin resultados:** Si no existen parametros con el `parent_key` indicado, se retorna `200` con `items: []`.
- **Reutiliza RF-CFG-02:** Este RF es un caso especifico de RF-CFG-02 con `parent_key` obligatorio. La implementacion es la misma query con filtro adicional.
- **Uso por servicios consumidores:** Otros microservicios pueden consumir este endpoint para obtener datos maestros filtrados (ej: Operations Service necesita ciudades de una provincia para formularios de solicitud).

#### Impacto en Modelo de Datos

| Entidad | Operacion | Campo(s) afectado(s) | Evento |
|---------|-----------|---------------------|--------|
| `parameters` | SELECT | `group_id`, `parent_key`, `key`, `value`, `type`, `description` | — |

#### Criterios de Aceptacion (Gherkin)

```gherkin
Scenario: Filtrar ciudades por provincia exitosamente
  Given el grupo "ciudades" tiene parametros con parent_key "buenos_aires" (5 ciudades) y "cordoba" (3 ciudades)
  And el usuario tiene permiso p_cfg_param_list
  When envio GET /parameters?group_id={id_ciudades}&parent_key=buenos_aires
  Then recibo HTTP 200 con items[] conteniendo exactamente 5 ciudades

Scenario: Filtro sin resultados retorna array vacio
  Given el grupo "ciudades" no tiene parametros con parent_key "inexistente"
  When envio GET /parameters?group_id={id_ciudades}&parent_key=inexistente
  Then recibo HTTP 200 con items: []
```

#### Trazabilidad de Pruebas

| Test ID | Tipo | Descripcion |
|---------|------|-------------|
| TP-CFG-07-01 | Positivo | Filtro jerarquico retorna solo parametros hijos |
| TP-CFG-07-02 | Positivo | parent_key sin hijos retorna array vacio |
| TP-CFG-07-03 | Negativo | 404 para group_id inexistente |
| TP-CFG-07-04 | Negativo | 403 sin permiso |
| TP-CFG-07-05 | Integracion | RLS filtra por tenant correctamente en consulta jerarquica |
| TP-CFG-07-06 | E2E | Seleccionar provincia en formulario filtra ciudades automaticamente |

#### Sin Ambiguedades

- **Supuestos prohibidos:** No se asume que las relaciones jerarquicas son multinivel (solo padre-hijo directo). No se asume que `parent_key` referencia un parametro existente en el mismo grupo.
- **Decisiones cerradas:** La jerarquia es de un solo nivel (padre → hijo). El indice `parameters(tenant_id, parent_key)` soporta el filtro.
- **Fuera de alcance explicito:** Jerarquias multinivel (abuelo → padre → hijo). Validacion de integridad referencial de parent_key.
- **TODO explicitos = 0**

---

### RF-CFG-08 — Listar servicios externos

#### Execution Sheet

| Campo | Valor |
|-------|-------|
| **ID** | RF-CFG-08 |
| **Titulo** | Listar servicios externos configurados |
| **Actor(es)** | Super Admin |
| **Prioridad** | Alta |
| **Severidad** | P1 |
| **Flujo origen** | FL-CFG-01 seccion 7 |
| **HU origen** | HU021 |

#### Precondiciones

| # | Condicion | Detalle |
|---|-----------|---------|
| 1 | Config Service operativo | SA.config accesible |
| 2 | Usuario autenticado como Super Admin | JWT con permiso `p_cfg_service_list` |

#### Entradas

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| `tenant_id` | uuid | Si | JWT claim | UUID valido, tenant activo | — |

#### Proceso (Happy Path)

| # | Paso | Responsable |
|---|------|-------------|
| 1 | Recibir `GET /external-services` | API Gateway |
| 2 | Verificar permiso `p_cfg_service_list` en JWT | API Gateway |
| 3 | Ejecutar `SET LOCAL app.current_tenant = '{tenant_id}'` | Config Service |
| 4 | SELECT de `external_services` (RLS filtra por tenant). **No se incluyen datos de `service_auth_configs`** | Config Service |
| 5 | Retornar `200 OK` con `{ items: [{ id, name, description, type, base_url, status, auth_type, timeout_ms, max_retries, last_tested_at, last_test_success }] }` | Config Service |

#### Salidas

| Campo | Tipo | Destino | Efecto observable |
|-------|------|---------|-------------------|
| `items[]` | array | Response body → SPA | Lista de servicios sin credenciales |
| `items[].status` | enum | Response body | active, inactive o error |
| `items[].last_tested_at` | timestamp? | Response body | Fecha de ultima prueba (nullable) |
| `items[].last_test_success` | boolean? | Response body | Resultado de ultima prueba (nullable) |

#### Errores Tipados

| Codigo | Causa | Condicion de disparo | Respuesta esperada |
|--------|-------|---------------------|--------------------|
| `CFG_UNAUTHORIZED` | Sin permiso | JWT no contiene `p_cfg_service_list` | HTTP 403 |
| `CFG_UNAUTHENTICATED` | Token invalido | JWT ausente o expirado | HTTP 401 |

#### Casos Especiales y Variantes

- **Credenciales nunca expuestas:** El GET de servicios externos NUNCA incluye datos de `service_auth_configs`. Las credenciales son write-only desde la perspectiva de la API REST (RN-CFG-02, RN-CFG-11).
- **Sin servicios:** Si el tenant no tiene servicios configurados, retorna `200` con `items: []`.

#### Impacto en Modelo de Datos

| Entidad | Operacion | Campo(s) afectado(s) | Evento |
|---------|-----------|---------------------|--------|
| `external_services` | SELECT | todos los campos excepto `service_auth_configs` | — |

#### Criterios de Aceptacion (Gherkin)

```gherkin
Scenario: Listar servicios externos exitosamente
  Given el tenant tiene 3 servicios externos configurados
  And el usuario tiene permiso p_cfg_service_list
  When envio GET /external-services
  Then recibo HTTP 200 con items[] conteniendo 3 servicios
  And ningun servicio incluye datos de credenciales

Scenario: Listar servicios muestra estado de prueba
  Given existe un servicio con last_tested_at y last_test_success = true
  When envio GET /external-services
  Then el servicio incluye last_tested_at y last_test_success en la respuesta

Scenario: Sin permiso retorna 403
  Given el usuario NO tiene permiso p_cfg_service_list
  When envio GET /external-services
  Then recibo HTTP 403
```

#### Trazabilidad de Pruebas

| Test ID | Tipo | Descripcion |
|---------|------|-------------|
| TP-CFG-08-01 | Positivo | Listar servicios retorna items sin credenciales |
| TP-CFG-08-02 | Positivo | Servicios incluyen last_tested_at y last_test_success |
| TP-CFG-08-03 | Positivo | Sin servicios retorna items: [] |
| TP-CFG-08-04 | Negativo | 403 sin permiso p_cfg_service_list |
| TP-CFG-08-05 | Negativo | 401 sin JWT |
| TP-CFG-08-06 | Integracion | RLS filtra servicios por tenant |
| TP-CFG-08-07 | Seguridad | Respuesta no contiene valores de service_auth_configs |
| TP-CFG-08-08 | E2E | Navegar a /servicios muestra tabla con servicios del tenant |

#### Sin Ambiguedades

- **Supuestos prohibidos:** No se asume que las credenciales se muestran enmascaradas (****). Simplemente no se incluyen en la respuesta.
- **Decisiones cerradas:** Credenciales son write-only (RN-CFG-02). El listado no soporta paginacion para MVP.
- **Fuera de alcance explicito:** Busqueda de servicios por texto. Filtro por tipo o status. Paginacion.
- **TODO explicitos = 0**

---

### RF-CFG-09 — Crear servicio externo con credenciales encriptadas

#### Execution Sheet

| Campo | Valor |
|-------|-------|
| **ID** | RF-CFG-09 |
| **Titulo** | Crear servicio externo con credenciales encriptadas |
| **Actor(es)** | Super Admin |
| **Prioridad** | Alta |
| **Severidad** | P0 |
| **Flujo origen** | FL-CFG-01 seccion 7 (Crear servicio) |
| **HU origen** | HU021 |

#### Precondiciones

| # | Condicion | Detalle |
|---|-----------|---------|
| 1 | Config Service operativo | SA.config accesible |
| 2 | RabbitMQ operativo | Para publicacion de ServiceCreatedEvent |
| 3 | Clave AES-256 disponible | Variable de entorno con clave de encriptacion configurada |
| 4 | Usuario autenticado como Super Admin | JWT con permiso `p_cfg_service_create` |

#### Entradas

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| `name` | string | Si | Body JSON | No vacio, max 200 chars, unico por tenant | RN-CFG-05 |
| `description` | string | No | Body JSON | Max 500 chars | — |
| `type` | enum | Si | Body JSON | Uno de: rest_api, mcp, graphql, soap, webhook | — |
| `base_url` | string | Si | Body JSON | URL valida (https:// o http://) | — |
| `status` | enum | No | Body JSON | Default: active. Valores: active, inactive | — |
| `timeout_ms` | integer | No | Body JSON | Default: 30000. Min 1000, max 120000 | — |
| `max_retries` | integer | No | Body JSON | Default: 3. Min 0, max 10 | — |
| `auth_type` | enum | Si | Body JSON | Uno de: none, api_key, bearer_token, basic_auth, oauth2, custom_header | — |
| `auth_configs[]` | array | Condicional | Body JSON | Requerido si `auth_type != none`. Keys segun RN-CFG-07 | RN-CFG-07 |
| `tenant_id` | uuid | Si | JWT claim | UUID valido | — |

#### Proceso (Happy Path)

| # | Paso | Responsable |
|---|------|-------------|
| 1 | Recibir `POST /external-services` con body JSON | API Gateway |
| 2 | Verificar permiso `p_cfg_service_create` en JWT | API Gateway |
| 3 | Ejecutar `SET LOCAL app.current_tenant = '{tenant_id}'` | Config Service |
| 4 | Validar unicidad de `name` por tenant (indice UNIQUE) | Config Service |
| 5 | Validar que `auth_configs[]` contiene las keys requeridas segun `auth_type` (RN-CFG-07) | Config Service |
| 6 | INSERT en `external_services` con `id` (UUIDv7), `tenant_id`, `name`, `description`, `type`, `base_url`, `status`, `timeout_ms`, `max_retries`, `auth_type`, `created_at`, `updated_at`, `created_by`, `updated_by` | Config Service |
| 7 | Para cada `auth_configs[]` entry: encriptar `value` con AES-256, INSERT en `service_auth_configs` con `service_id`, `key`, `value_encrypted` | Config Service |
| 8 | Publicar `ServiceCreatedEvent` a RabbitMQ con `{ service_id, name, type, tenant_id }` | Config Service |
| 9 | Retornar `201 Created` con `{ id, name, description, type, base_url, status, auth_type, timeout_ms, max_retries }` (sin credenciales) | Config Service |

#### Salidas

| Campo | Tipo | Destino | Efecto observable |
|-------|------|---------|-------------------|
| `201 Created` | HTTP status | Response → SPA | Servicio creado |
| `id` | uuid | Response body | ID del servicio creado |
| `ServiceCreatedEvent` | evento async | RabbitMQ → Audit Service | Registro en audit_log |

#### Errores Tipados

| Codigo | Causa | Condicion de disparo | Respuesta esperada |
|--------|-------|---------------------|--------------------|
| `CFG_DUPLICATE_SERVICE_NAME` | Nombre duplicado | `external_services(tenant_id, name)` ya existe | HTTP 409, mensaje: "Ya existe un servicio con el nombre '{name}'" |
| `CFG_INVALID_AUTH_CONFIG` | Credenciales incompletas | `auth_configs[]` no contiene las keys requeridas para el `auth_type` | HTTP 422, mensaje detallando keys faltantes |
| `CFG_VALIDATION_ERROR` | Entrada invalida | Campos requeridos vacios, URL mal formada, tipo invalido | HTTP 422 |
| `CFG_ENCRYPTION_ERROR` | Error de encriptacion | Clave AES-256 no disponible o corrupta | HTTP 500, mensaje: "Error interno de configuracion" |
| `CFG_UNAUTHORIZED` | Sin permiso | JWT no contiene `p_cfg_service_create` | HTTP 403 |

#### Casos Especiales y Variantes

- **auth_type = none:** No se requieren `auth_configs[]`. Si se envian, se ignoran.
- **Validacion de keys por auth_type (RN-CFG-07):**
  - `api_key` → requiere `header_name` y `api_key`
  - `bearer_token` → requiere `token`
  - `basic_auth` → requiere `username` y `password`
  - `oauth2` → requiere `client_id`, `client_secret`, `token_url`; `scopes` es opcional
  - `custom_header` → requiere `header_name` y `header_value`
- **Credenciales write-only:** La respuesta del POST no incluye `auth_configs[]`. Las credenciales nunca se retornan en ninguna respuesta API.

#### Impacto en Modelo de Datos

| Entidad | Operacion | Campo(s) afectado(s) | Evento |
|---------|-----------|---------------------|--------|
| `external_services` | INSERT | todos los campos | `ServiceCreatedEvent` |
| `service_auth_configs` | INSERT | `service_id`, `key`, `value_encrypted` | — |
| `audit_log` (SA.audit) | INSERT (async) | operation = 'Crear', module = 'servicios externos' | consume `ServiceCreatedEvent` |

#### Criterios de Aceptacion (Gherkin)

```gherkin
Scenario: Crear servicio REST con API key exitosamente
  Given no existe un servicio con nombre "Mi API" para el tenant actual
  And el usuario tiene permiso p_cfg_service_create
  And la clave AES-256 esta disponible
  When envio POST /external-services con name="Mi API", type="rest_api", base_url="https://api.ejemplo.com", auth_type="api_key", auth_configs=[{key:"header_name", value:"X-API-Key"}, {key:"api_key", value:"secret123"}]
  Then recibo HTTP 201 con id del servicio creado
  And la respuesta NO incluye auth_configs
  And las credenciales se almacenan encriptadas en service_auth_configs
  And se publica ServiceCreatedEvent

Scenario: Nombre duplicado retorna 409
  Given ya existe un servicio con nombre "Mi API" para el tenant actual
  When envio POST /external-services con name="Mi API"
  Then recibo HTTP 409 con codigo CFG_DUPLICATE_SERVICE_NAME

Scenario: Auth type oauth2 sin client_secret retorna 422
  When envio POST /external-services con auth_type="oauth2" y auth_configs sin client_secret
  Then recibo HTTP 422 con codigo CFG_INVALID_AUTH_CONFIG indicando "client_secret requerido"

Scenario: Auth type none no requiere auth_configs
  When envio POST /external-services con auth_type="none" sin auth_configs
  Then recibo HTTP 201 sin registros en service_auth_configs
```

#### Trazabilidad de Pruebas

| Test ID | Tipo | Descripcion |
|---------|------|-------------|
| TP-CFG-09-01 | Positivo | Crear servicio REST con api_key retorna 201 |
| TP-CFG-09-02 | Positivo | Crear servicio con auth_type none sin auth_configs |
| TP-CFG-09-03 | Positivo | Crear servicio oauth2 con todas las keys requeridas |
| TP-CFG-09-04 | Negativo | 409 por nombre duplicado |
| TP-CFG-09-05 | Negativo | 422 por auth_configs incompletas para el auth_type |
| TP-CFG-09-06 | Negativo | 422 por campos requeridos vacios |
| TP-CFG-09-07 | Negativo | 403 sin permiso p_cfg_service_create |
| TP-CFG-09-08 | Seguridad | Credenciales almacenadas encriptadas con AES-256 |
| TP-CFG-09-09 | Seguridad | Respuesta no contiene credenciales |
| TP-CFG-09-10 | Integracion | ServiceCreatedEvent publicado y consumido por Audit |
| TP-CFG-09-11 | Integracion | RLS asegura aislamiento por tenant |
| TP-CFG-09-12 | E2E | Click "Nuevo Servicio" → formulario → completar → submit → servicio visible en tabla |

#### Sin Ambiguedades

- **Supuestos prohibidos:** No se asume que las credenciales se validan contra el servicio externo al crear (eso es responsabilidad de RF-CFG-11 — Probar conexion). No se asume que `base_url` es accesible al momento de crear.
- **Decisiones cerradas:** Encriptacion AES-256 en DB (D-CFG-03). Credenciales write-only (RN-CFG-02). Keys requeridas por auth_type son fijas (RN-CFG-07).
- **Fuera de alcance explicito:** Validacion de conectividad al crear. Rotacion automatica de credenciales. Import de configuracion desde OpenAPI/Swagger.
- **TODO explicitos = 0**

---

### RF-CFG-10 — Editar servicio externo

#### Execution Sheet

| Campo | Valor |
|-------|-------|
| **ID** | RF-CFG-10 |
| **Titulo** | Editar servicio externo existente |
| **Actor(es)** | Super Admin |
| **Prioridad** | Alta |
| **Severidad** | P0 |
| **Flujo origen** | FL-CFG-01 seccion 7 |
| **HU origen** | HU021 |

#### Precondiciones

| # | Condicion | Detalle |
|---|-----------|---------|
| 1 | Config Service operativo | SA.config accesible |
| 2 | RabbitMQ operativo | Para publicacion de ServiceUpdatedEvent |
| 3 | Clave AES-256 disponible | Variable de entorno configurada |
| 4 | Servicio existente | `id` valido en `external_services` para el tenant actual |
| 5 | Usuario autenticado como Super Admin | JWT con permiso `p_cfg_service_edit` |

#### Entradas

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| `id` | uuid | Si | Path param `/external-services/:id` | UUID valido, servicio existente para el tenant | — |
| `name` | string | No | Body JSON | Max 200 chars, unico por tenant si se modifica | RN-CFG-05 |
| `description` | string | No | Body JSON | Max 500 chars | — |
| `type` | enum | No | Body JSON | Uno de: rest_api, mcp, graphql, soap, webhook | — |
| `base_url` | string | No | Body JSON | URL valida | — |
| `status` | enum | No | Body JSON | active, inactive | — |
| `timeout_ms` | integer | No | Body JSON | Min 1000, max 120000 | — |
| `max_retries` | integer | No | Body JSON | Min 0, max 10 | — |
| `auth_type` | enum | No | Body JSON | Si se cambia, `auth_configs[]` es requerido | — |
| `auth_configs[]` | array | Condicional | Body JSON | Requerido si `auth_type` se modifica. Keys segun RN-CFG-07 | RN-CFG-07 |
| `tenant_id` | uuid | Si | JWT claim | UUID valido | — |

#### Proceso (Happy Path)

| # | Paso | Responsable |
|---|------|-------------|
| 1 | Recibir `PUT /external-services/:id` con body JSON | API Gateway |
| 2 | Verificar permiso `p_cfg_service_edit` en JWT | API Gateway |
| 3 | Ejecutar `SET LOCAL app.current_tenant = '{tenant_id}'` | Config Service |
| 4 | SELECT servicio por `id` (RLS filtra por tenant) | Config Service |
| 5 | Si `name` se modifica, validar unicidad por tenant | Config Service |
| 6 | Si `auth_type` se modifica o `auth_configs[]` esta presente: validar keys segun RN-CFG-07 | Config Service |
| 7 | UPDATE `external_services` con campos modificados, `updated_at = NOW()`, `updated_by = :user_id` | Config Service |
| 8 | Si `auth_configs[]` presente: DELETE existentes de `service_auth_configs`, encriptar e INSERT nuevas | Config Service |
| 9 | Publicar `ServiceUpdatedEvent` a RabbitMQ con `{ service_id, name, type, tenant_id }` | Config Service |
| 10 | Retornar `200 OK` con `{ id, name, description, type, base_url, status, auth_type, timeout_ms, max_retries }` (sin credenciales) | Config Service |

#### Salidas

| Campo | Tipo | Destino | Efecto observable |
|-------|------|---------|-------------------|
| `200 OK` | HTTP status | Response → SPA | Servicio actualizado |
| `ServiceUpdatedEvent` | evento async | RabbitMQ → Audit | Registro en audit_log |

#### Errores Tipados

| Codigo | Causa | Condicion de disparo | Respuesta esperada |
|--------|-------|---------------------|--------------------|
| `CFG_SERVICE_NOT_FOUND` | Servicio inexistente | `id` no existe o no pertenece al tenant | HTTP 404 |
| `CFG_DUPLICATE_SERVICE_NAME` | Nombre duplicado | Nuevo `name` ya existe para otro servicio del tenant | HTTP 409 |
| `CFG_INVALID_AUTH_CONFIG` | Credenciales incompletas | `auth_configs[]` no tiene keys requeridas para `auth_type` | HTTP 422 |
| `CFG_VALIDATION_ERROR` | Entrada invalida | Campos con formato incorrecto | HTTP 422 |
| `CFG_UNAUTHORIZED` | Sin permiso | JWT no contiene `p_cfg_service_edit` | HTTP 403 |

#### Casos Especiales y Variantes

- **Edicion parcial:** Solo se actualizan los campos enviados en el body. Si `auth_configs[]` no se incluye, las credenciales existentes se mantienen intactas.
- **Cambio de auth_type:** Si se cambia `auth_type`, las credenciales anteriores se eliminan y se requieren nuevas `auth_configs[]` con las keys del nuevo tipo.
- **Credenciales write-only:** La respuesta nunca incluye credenciales. El frontend muestra campos de credenciales vacios indicando que "las credenciales actuales se mantendran si no se modifican".

#### Impacto en Modelo de Datos

| Entidad | Operacion | Campo(s) afectado(s) | Evento |
|---------|-----------|---------------------|--------|
| `external_services` | SELECT | `id` (validacion) | — |
| `external_services` | UPDATE | campos modificados, `updated_at`, `updated_by` | `ServiceUpdatedEvent` |
| `service_auth_configs` | DELETE + INSERT (condicional) | `service_id`, `key`, `value_encrypted` | — |
| `audit_log` (SA.audit) | INSERT (async) | operation = 'Editar', module = 'servicios externos' | consume `ServiceUpdatedEvent` |

#### Criterios de Aceptacion (Gherkin)

```gherkin
Scenario: Editar nombre de servicio exitosamente
  Given existe un servicio con id "svc-001" y nombre "API Vieja"
  And el usuario tiene permiso p_cfg_service_edit
  When envio PUT /external-services/svc-001 con name="API Nueva"
  Then recibo HTTP 200 con name="API Nueva"
  And las credenciales existentes se mantienen
  And se publica ServiceUpdatedEvent

Scenario: Cambiar auth_type requiere nuevas credenciales
  Given existe un servicio con auth_type="api_key"
  When envio PUT /external-services/:id con auth_type="oauth2" y auth_configs con client_id y client_secret
  Then recibo HTTP 200
  And las credenciales api_key anteriores se eliminan
  And las nuevas credenciales oauth2 se almacenan encriptadas

Scenario: Editar servicio inexistente retorna 404
  When envio PUT /external-services/00000000-0000-0000-0000-000000000000
  Then recibo HTTP 404 con codigo CFG_SERVICE_NOT_FOUND

Scenario: Nombre duplicado retorna 409
  Given existe otro servicio con nombre "Existente"
  When envio PUT /external-services/:id con name="Existente"
  Then recibo HTTP 409 con codigo CFG_DUPLICATE_SERVICE_NAME
```

#### Trazabilidad de Pruebas

| Test ID | Tipo | Descripcion |
|---------|------|-------------|
| TP-CFG-10-01 | Positivo | Editar nombre de servicio retorna 200 |
| TP-CFG-10-02 | Positivo | Editar sin auth_configs mantiene credenciales existentes |
| TP-CFG-10-03 | Positivo | Cambiar auth_type reemplaza credenciales |
| TP-CFG-10-04 | Negativo | 404 para servicio inexistente |
| TP-CFG-10-05 | Negativo | 409 por nombre duplicado |
| TP-CFG-10-06 | Negativo | 422 por auth_configs incompletas |
| TP-CFG-10-07 | Negativo | 403 sin permiso p_cfg_service_edit |
| TP-CFG-10-08 | Seguridad | Nuevas credenciales encriptadas con AES-256 |
| TP-CFG-10-09 | Seguridad | Respuesta no contiene credenciales |
| TP-CFG-10-10 | Integracion | ServiceUpdatedEvent publicado y consumido por Audit |
| TP-CFG-10-11 | Integracion | RLS impide editar servicio de otro tenant |
| TP-CFG-10-12 | E2E | Editar servicio → cambiar nombre → guardar → nombre actualizado en tabla |

#### Sin Ambiguedades

- **Supuestos prohibidos:** No se asume que el frontend envia todos los campos. Solo los modificados. No se asume que las credenciales se retornan para edicion.
- **Decisiones cerradas:** Edicion parcial (PATCH semantics en PUT). Si auth_type cambia, credenciales se reemplazan completamente.
- **Fuera de alcance explicito:** Eliminacion de servicios externos. Clonacion de servicios.
- **TODO explicitos = 0**

---

### RF-CFG-11 — Probar conexion de servicio externo

#### Execution Sheet

| Campo | Valor |
|-------|-------|
| **ID** | RF-CFG-11 |
| **Titulo** | Ejecutar prueba de conexion contra servicio externo |
| **Actor(es)** | Super Admin |
| **Prioridad** | Alta |
| **Severidad** | P1 |
| **Flujo origen** | FL-CFG-01 seccion 7 (Probar conexion) |
| **HU origen** | HU021 |

#### Precondiciones

| # | Condicion | Detalle |
|---|-----------|---------|
| 1 | Config Service operativo | SA.config accesible |
| 2 | RabbitMQ operativo | Para publicacion de ServiceTestedEvent |
| 3 | Clave AES-256 disponible | Para desencriptar credenciales |
| 4 | Servicio existente | `id` valido en `external_services` para el tenant actual |
| 5 | Usuario autenticado como Super Admin | JWT con permiso `p_cfg_service_test` |

#### Entradas

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| `id` | uuid | Si | Path param `/external-services/:id/test` | UUID valido, servicio existente para el tenant | — |
| `tenant_id` | uuid | Si | JWT claim | UUID valido | — |

#### Proceso (Happy Path)

| # | Paso | Responsable |
|---|------|-------------|
| 1 | Recibir `POST /external-services/:id/test` | API Gateway |
| 2 | Verificar permiso `p_cfg_service_test` en JWT | API Gateway |
| 3 | Ejecutar `SET LOCAL app.current_tenant = '{tenant_id}'` | Config Service |
| 4 | SELECT servicio + `service_auth_configs` por `id` (RLS filtra por tenant) | Config Service |
| 5 | Desencriptar credenciales de `service_auth_configs` usando AES-256 | Config Service |
| 6 | Construir request de prueba segun `type` del servicio: HEAD o GET a `base_url` con autenticacion configurada | Config Service |
| 7 | Ejecutar request con `timeout_ms` del servicio y medir `response_time_ms` | Config Service |
| 8 | Si respuesta exitosa (2xx): UPDATE `last_tested_at = NOW()`, `last_test_success = true` | Config Service |
| 9 | Publicar `ServiceTestedEvent` a RabbitMQ con `{ service_id, success: true, response_time_ms, tenant_id }` | Config Service |
| 10 | Retornar `200 OK` con `{ success: true, response_time_ms }` | Config Service |

#### Proceso (Conexion Fallida)

| # | Paso | Responsable |
|---|------|-------------|
| 7a | Si respuesta con error o timeout: UPDATE `last_tested_at = NOW()`, `last_test_success = false`, `status = 'error'` | Config Service |
| 8a | Publicar `ServiceTestedEvent` con `{ success: false, error_message, tenant_id }` | Config Service |
| 9a | Retornar `200 OK` con `{ success: false, response_time_ms, error: "mensaje de error" }` | Config Service |

#### Salidas

| Campo | Tipo | Destino | Efecto observable |
|-------|------|---------|-------------------|
| `success` | boolean | Response body → SPA | Resultado de la prueba |
| `response_time_ms` | integer | Response body → SPA | Tiempo de respuesta en milisegundos |
| `error` | string? | Response body → SPA | Mensaje de error si fallo (nullable) |
| `ServiceTestedEvent` | evento async | RabbitMQ → Audit | Registra resultado en audit_log |
| `last_tested_at` | timestamp | SA.config | Actualizado al momento de la prueba |
| `last_test_success` | boolean | SA.config | Resultado de la prueba |

#### Errores Tipados

| Codigo | Causa | Condicion de disparo | Respuesta esperada |
|--------|-------|---------------------|--------------------|
| `CFG_SERVICE_NOT_FOUND` | Servicio inexistente | `id` no existe o no pertenece al tenant | HTTP 404 |
| `CFG_DECRYPTION_ERROR` | Error al desencriptar | Clave AES-256 incorrecta o datos corruptos | HTTP 500, mensaje: "Error interno al procesar credenciales" |
| `CFG_UNAUTHORIZED` | Sin permiso | JWT no contiene `p_cfg_service_test` | HTTP 403 |

#### Casos Especiales y Variantes

- **Siempre retorna 200:** La prueba de conexion siempre retorna HTTP 200 al cliente. El campo `success` indica si la conexion al servicio externo fue exitosa o no. Un fallo de conexion NO es un error del Config Service.
- **Timeout:** Si el servicio externo no responde dentro de `timeout_ms`, la prueba se considera fallida. `response_time_ms` refleja el tiempo hasta el timeout.
- **Status error:** Un fallo de prueba cambia `status` a `'error'`, pero el servicio sigue siendo utilizable y editable (RN-CFG-08). El status se puede restaurar a `active` editando el servicio o con una prueba exitosa posterior.
- **Request de prueba segun tipo:**
  - `rest_api` → HEAD a `base_url`
  - `graphql` → POST a `base_url` con query `{ __schema { types { name } } }` (introspection)
  - `soap` → GET del WSDL (`base_url?wsdl`)
  - `webhook` → HEAD a `base_url`
  - `mcp` → GET a `base_url` (health check)
- **Credenciales en logs:** Los logs de la prueba NUNCA incluyen valores de credenciales desencriptados (RN-CFG-11).

#### Impacto en Modelo de Datos

| Entidad | Operacion | Campo(s) afectado(s) | Evento |
|---------|-----------|---------------------|--------|
| `external_services` | SELECT | `id`, `type`, `base_url`, `timeout_ms`, `auth_type` | — |
| `service_auth_configs` | SELECT | `key`, `value_encrypted` (desencriptar en memoria) | — |
| `external_services` | UPDATE | `last_tested_at`, `last_test_success`, `status` (si fallo) | `ServiceTestedEvent` |
| `audit_log` (SA.audit) | INSERT (async) | operation = 'Consultar', module = 'servicios externos' | consume `ServiceTestedEvent` |

#### Criterios de Aceptacion (Gherkin)

```gherkin
Scenario: Prueba de conexion exitosa
  Given existe un servicio REST con credenciales validas y el endpoint responde 200
  And el usuario tiene permiso p_cfg_service_test
  When envio POST /external-services/:id/test
  Then recibo HTTP 200 con success=true y response_time_ms > 0
  And last_tested_at se actualiza y last_test_success = true
  And se publica ServiceTestedEvent con success=true

Scenario: Prueba de conexion fallida por timeout
  Given existe un servicio con timeout_ms=5000 y el endpoint no responde
  When envio POST /external-services/:id/test
  Then recibo HTTP 200 con success=false y error conteniendo "timeout"
  And last_test_success = false y status = "error"
  And se publica ServiceTestedEvent con success=false

Scenario: Prueba de conexion fallida por credenciales invalidas
  Given existe un servicio con credenciales incorrectas
  When envio POST /external-services/:id/test
  Then recibo HTTP 200 con success=false y error describiendo el fallo
  And last_test_success = false

Scenario: Servicio inexistente retorna 404
  When envio POST /external-services/00000000-0000-0000-0000-000000000000/test
  Then recibo HTTP 404 con codigo CFG_SERVICE_NOT_FOUND
```

#### Trazabilidad de Pruebas

| Test ID | Tipo | Descripcion |
|---------|------|-------------|
| TP-CFG-11-01 | Positivo | Prueba exitosa retorna success=true con response_time_ms |
| TP-CFG-11-02 | Positivo | Prueba exitosa actualiza last_tested_at y last_test_success=true |
| TP-CFG-11-03 | Positivo | Prueba exitosa restaura status a active si estaba en error |
| TP-CFG-11-04 | Negativo | Prueba fallida retorna success=false con error |
| TP-CFG-11-05 | Negativo | Timeout respeta timeout_ms configurado |
| TP-CFG-11-06 | Negativo | Prueba fallida cambia status a error |
| TP-CFG-11-07 | Negativo | 404 para servicio inexistente |
| TP-CFG-11-08 | Negativo | 403 sin permiso p_cfg_service_test |
| TP-CFG-11-09 | Seguridad | Logs de prueba no contienen credenciales desencriptadas |
| TP-CFG-11-10 | Integracion | ServiceTestedEvent publicado y consumido por Audit |
| TP-CFG-11-11 | Integracion | Desencriptacion AES-256 funciona correctamente |
| TP-CFG-11-12 | E2E | Click "Probar Conexion" → spinner → resultado exito/fallo visible en UI |

#### Sin Ambiguedades

- **Supuestos prohibidos:** No se asume que la prueba bloquea otras operaciones del servicio. No se asume que un fallo de prueba desactiva el servicio permanentemente. No se asume que las credenciales se validan por esquema durante la prueba.
- **Decisiones cerradas:** La prueba siempre retorna HTTP 200 al cliente (el fallo es del servicio externo, no del Config Service). Status 'error' no bloquea uso del servicio (RN-CFG-08).
- **Fuera de alcance explicito:** Monitoreo continuo (health check periodico). Prueba con payload custom. Reintentos automaticos de prueba.
- **TODO explicitos = 0**

---

---

### RF-CFG-12 — Listar workflows con estado y version

#### Execution Sheet

| Campo | Valor |
|-------|-------|
| **ID** | RF-CFG-12 |
| **Titulo** | Listar workflows con estado y version |
| **Actor(es)** | Super Admin, Disenador de Procesos |
| **Prioridad** | Alta |
| **Severidad** | P1 |
| **Flujo origen** | FL-CFG-02 seccion 6 (listar) |
| **HU origen** | HU022 |

#### Precondiciones

| # | Condicion | Detalle |
|---|-----------|---------|
| 1 | Config Service operativo | SA.config accesible |
| 2 | Usuario autenticado | JWT valido con claim `tenant_id` |
| 3 | Permiso requerido | `p_wf_list` |

#### Entradas

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| page | integer | No (default 1) | Query param | >= 1 | — |
| page_size | integer | No (default 20) | Query param | 1..100 | — |
| search | string | No | Query param | ILIKE sobre `name` y `description` | — |
| status | string | No | Query param | Enum: draft, active, inactive | — |
| sort_by | string | No (default created_at) | Query param | Enum: name, status, version, created_at | — |
| sort_dir | string | No (default desc) | Query param | Enum: asc, desc | — |

#### Pasos del Proceso (Happy Path)

| # | Paso | Responsable |
|---|------|-------------|
| 1 | SPA envia GET /workflows con query params | Frontend |
| 2 | API Gateway valida JWT y enruta a Config Service | Gateway |
| 3 | Config Service valida permiso `p_wf_list` | Config Service |
| 4 | SELECT sobre `workflow_definitions` con filtros, RLS por `tenant_id` | SA.config |
| 5 | Retorna 200 con items paginados | Config Service |

#### Salidas

| Campo | Tipo | Destino | Efecto |
|-------|------|---------|--------|
| items[] | array | Response body | Lista de workflows |
| items[].id | uuid | — | ID del workflow |
| items[].name | string | — | Nombre |
| items[].description | string | — | Descripcion |
| items[].status | string | — | draft / active / inactive |
| items[].version | integer | — | Numero de version |
| items[].created_at | timestamp | — | Fecha de creacion |
| items[].updated_at | timestamp | — | Fecha de ultima modificacion |
| total | integer | — | Total de registros |
| page | integer | — | Pagina actual |
| page_size | integer | — | Tamano de pagina |

#### Errores Tipados

| Codigo | Causa | Condicion | Respuesta |
|--------|-------|-----------|-----------|
| 401 | JWT ausente o invalido | Header Authorization faltante o token expirado | `{ "error": "Unauthorized" }` |
| 403 | Sin permiso | Usuario no tiene `p_wf_list` | `{ "error": "Forbidden", "detail": "Permission p_wf_list required" }` |
| 400 | Parametros invalidos | `page_size` > 100 o `status` no es enum valido | `{ "error": "Bad Request", "detail": "..." }` |

#### Casos Especiales y Variantes

- **Sin resultados:** Retorna 200 con `items: []`, `total: 0`.
- **Busqueda:** ILIKE simultaneo sobre `name` y `description` (OR).
- **RLS:** Config Service aplica `tenant_id` del JWT automaticamente.

#### Data Model Impact

| Entidad | Operacion | Evento |
|---------|-----------|--------|
| `workflow_definitions` | SELECT | — |

#### Criterios de Aceptacion (Gherkin)

```gherkin
Feature: RF-CFG-12 Listar workflows

  Scenario: Listado exitoso con paginacion
    Given usuario autenticado como Super Admin con p_wf_list
    When GET /workflows?page=1&page_size=10
    Then response status 200
    And body contiene items[], total, page, page_size

  Scenario: Filtro por status
    Given existen workflows draft y active en el tenant
    When GET /workflows?status=active
    Then todos los items tienen status = "active"

  Scenario: Sin permiso retorna 403
    Given usuario sin permiso p_wf_list
    When GET /workflows
    Then response status 403
```

#### Trazabilidad de Tests

| Test ID | Tipo | Descripcion |
|---------|------|-------------|
| TP-CFG-12-01 | Positivo | Lista paginada con campos esperados |
| TP-CFG-12-02 | Positivo | Busqueda ILIKE filtra por name y description |
| TP-CFG-12-03 | Positivo | Filtro por status retorna solo workflows del estado solicitado |
| TP-CFG-12-04 | Negativo | Sin permiso p_wf_list retorna 403 |
| TP-CFG-12-05 | Negativo | Sin JWT retorna 401 |
| TP-CFG-12-06 | Negativo | page_size > 100 retorna 400 |
| TP-CFG-12-07 | Negativo | status invalido retorna 400 |
| TP-CFG-12-08 | Integracion | RLS filtra workflows por tenant_id del JWT |
| TP-CFG-12-09 | Integracion | Ordenamiento por name/status/version/created_at funciona |
| TP-CFG-12-10 | E2E | Navegar a /workflows, ver grilla, buscar, filtrar |

#### Sin Ambiguedades

- **Supuestos prohibidos:** No se asume que el campo `description` se trunca en el listado (se retorna completo). No se asume exportacion de workflows.
- **Decisiones cerradas:** Busqueda aplica ILIKE simultaneo sobre `name` y `description` (OR). No hay filtro por version. Sort default es `created_at DESC`.
- **Fuera de alcance explicito:** Exportacion de listado de workflows. Filtro por fecha. Contador de nodos por workflow.
- **TODO explicitos = 0**

---

### RF-CFG-13 — Crear/editar workflow con estados y transiciones

#### Execution Sheet

| Campo | Valor |
|-------|-------|
| **ID** | RF-CFG-13 |
| **Titulo** | Crear/editar workflow con estados y transiciones |
| **Actor(es)** | Super Admin, Disenador de Procesos |
| **Prioridad** | Alta |
| **Severidad** | P0 |
| **Flujo origen** | FL-CFG-02 seccion 6 (crear/guardar) y seccion 8a (editar) |
| **HU origen** | HU022 |

#### Precondiciones

| # | Condicion | Detalle |
|---|-----------|---------|
| 1 | Config Service operativo | SA.config accesible |
| 2 | Usuario autenticado | JWT valido con claim `tenant_id` |
| 3 | Permiso requerido | `p_wf_create` (crear) o `p_wf_edit` (editar) |
| 4 | Workflow existe (solo edicion) | `workflow_definitions.id` valido para PUT |

#### Entradas

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| name | string | Si | Body | max 200 chars, no vacio | — |
| description | string | No | Body | max 1000 chars | — |
| states[] | array | Si | Body | Al menos 1 estado | — |
| states[].name | string | Si | Body | max 100 chars, no vacio | — |
| states[].label | string | No | Body | max 200 chars | — |
| states[].type | string | Si | Body | Enum: start, end, service_call, decision, send_message, data_capture, timer | — |
| states[].position_x | decimal | Si | Body | Coordenada X en canvas | — |
| states[].position_y | decimal | Si | Body | Coordenada Y en canvas | — |
| states[].configs[] | array | No | Body | Key-value pairs para el tipo de nodo | RN-CFG-19 |
| states[].fields[] | array | No | Body | Solo para type=data_capture | RN-CFG-19 |
| transitions[] | array | No | Body | Conexiones entre estados | — |
| transitions[].from_state_ref | string | Si | Body | Referencia al estado origen (por indice o temp_id) | — |
| transitions[].to_state_ref | string | Si | Body | Referencia al estado destino | — |
| transitions[].label | string | No | Body | max 200 chars | — |
| transitions[].condition | string | No | Body | max 500 chars | — |
| transitions[].sla_hours | integer | No | Body | > 0 si presente | — |

#### Pasos del Proceso (Happy Path) — Crear

| # | Paso | Responsable |
|---|------|-------------|
| 1 | SPA envia POST /workflows con payload completo | Frontend |
| 2 | API Gateway valida JWT y enruta a Config Service | Gateway |
| 3 | Config Service valida permiso `p_wf_create` | Config Service |
| 4 | Validar campos requeridos (name, states con type) | Config Service |
| 5 | Generar UUIDv7 para workflow_definition | Config Service |
| 6 | INSERT en `workflow_definitions` con status=draft, version=0 | SA.config |
| 7 | INSERT en `workflow_states` (con tenant_id heredado) | SA.config |
| 8 | INSERT en `workflow_state_configs` para cada config de nodo | SA.config |
| 9 | INSERT en `workflow_state_fields` para nodos data_capture | SA.config |
| 10 | INSERT en `workflow_transitions` con from/to state IDs resueltos | SA.config |
| 11 | Publicar WorkflowCreatedEvent { workflow_id, tenant_id, name } | RabbitMQ |
| 12 | Retorna 201 { id, status: "draft", version: 0 } | Config Service |

#### Pasos del Proceso (Happy Path) — Editar

| # | Paso | Responsable |
|---|------|-------------|
| 1 | SPA envia GET /workflows/:id para cargar workflow completo | Frontend |
| 2 | Config Service retorna workflow con states, configs, fields, transitions | Config Service |
| 3 | SPA renderiza grafo en canvas React Flow | Frontend |
| 4 | Disenador modifica nodos/transiciones | Frontend |
| 5 | SPA envia PUT /workflows/:id con payload actualizado | Frontend |
| 6 | Config Service valida permiso `p_wf_edit` | Config Service |
| 7 | Si status=active: revertir a draft (RN-CFG-12) | Config Service |
| 8 | DELETE + re-INSERT states, configs, fields, transitions (replace strategy) | SA.config |
| 9 | Publicar WorkflowUpdatedEvent { workflow_id, tenant_id, reverted_from_active } | RabbitMQ |
| 10 | Retorna 200 { id, status: "draft" } | Config Service |

#### Salidas — Crear (POST)

| Campo | Tipo | Destino | Efecto |
|-------|------|---------|--------|
| id | uuid | Response body | ID del workflow creado |
| name | string | Response body | Nombre |
| status | string | Response body | "draft" |
| version | integer | Response body | 0 |
| created_at | timestamp | Response body | Fecha de creacion |

#### Salidas — Editar (PUT)

| Campo | Tipo | Destino | Efecto |
|-------|------|---------|--------|
| id | uuid | Response body | ID del workflow |
| name | string | Response body | Nombre actualizado |
| status | string | Response body | "draft" (siempre, por RN-CFG-12) |
| version | integer | Response body | Version actual (sin incrementar) |
| updated_at | timestamp | Response body | Fecha de actualizacion |

#### Salidas — Detalle (GET /workflows/:id)

| Campo | Tipo | Destino | Efecto |
|-------|------|---------|--------|
| id | uuid | Response body | ID del workflow |
| name | string | Response body | Nombre |
| description | string | Response body | Descripcion |
| status | string | Response body | Estado actual |
| version | integer | Response body | Version |
| states[] | array | Response body | Estados con configs y fields |
| states[].id | uuid | — | ID del estado |
| states[].name | string | — | Nombre del estado |
| states[].label | string | — | Label visual |
| states[].type | string | — | Tipo de nodo (flow_node_type) |
| states[].position_x | decimal | — | Coordenada X |
| states[].position_y | decimal | — | Coordenada Y |
| states[].configs[] | array | — | Key-value configs del nodo |
| states[].fields[] | array | — | Fields (solo data_capture) |
| transitions[] | array | Response body | Transiciones |
| transitions[].id | uuid | — | ID de la transicion |
| transitions[].from_state_id | uuid | — | Estado origen |
| transitions[].to_state_id | uuid | — | Estado destino |
| transitions[].label | string | — | Label |
| transitions[].condition | string | — | Condicion (nullable) |
| transitions[].sla_hours | integer | — | SLA en horas (nullable) |

#### Errores Tipados

| Codigo | Causa | Condicion | Respuesta |
|--------|-------|-----------|-----------|
| 401 | JWT ausente o invalido | Header Authorization faltante o token expirado | `{ "error": "Unauthorized" }` |
| 403 | Sin permiso | Sin `p_wf_create` o `p_wf_edit` | `{ "error": "Forbidden", "detail": "Permission required" }` |
| 404 | Workflow no encontrado | PUT/GET con ID inexistente o de otro tenant | `{ "error": "Not Found" }` |
| 422 | Datos invalidos | `name` vacio, `states[].type` no es enum valido, `states` vacio | `{ "error": "Validation Error", "detail": [...] }` |

#### Casos Especiales y Variantes

- **Edicion de workflow activo:** Automaticamente revierte a draft (RN-CFG-12). El campo `reverted_from_active` se incluye en el evento.
- **Replace strategy:** Al editar, se eliminan todos los states/configs/fields/transitions existentes y se re-insertan. Simplifica el manejo de deltas en grafos complejos.
- **States referencing:** En el payload de creacion, transitions referencian estados por indice temporal. El backend resuelve los IDs reales tras INSERT de states.
- **Workflow inactivo:** Se puede editar (revierte a draft igual que un activo).

#### Data Model Impact

| Entidad | Operacion | Evento |
|---------|-----------|--------|
| `workflow_definitions` | INSERT (crear), UPDATE (editar) | WorkflowCreatedEvent, WorkflowUpdatedEvent |
| `workflow_states` | INSERT, DELETE (replace) | — (incluido en workflow events) |
| `workflow_state_configs` | INSERT, DELETE (replace) | — (incluido en workflow events) |
| `workflow_state_fields` | INSERT, DELETE (replace) | — (incluido en workflow events) |
| `workflow_transitions` | INSERT, DELETE (replace) | — (incluido en workflow events) |

#### Criterios de Aceptacion (Gherkin)

```gherkin
Feature: RF-CFG-13 Crear/editar workflow

  Scenario: Crear workflow como draft
    Given usuario autenticado como Disenador con p_wf_create
    When POST /workflows con name="Flujo Prestamo" y states con 1 start + 1 end
    Then response status 201
    And body contiene id, status="draft", version=0
    And WorkflowCreatedEvent se publica en RabbitMQ

  Scenario: Editar workflow activo revierte a draft
    Given workflow existente con status="active"
    And usuario autenticado con p_wf_edit
    When PUT /workflows/:id con estados modificados
    Then response status 200
    And body contiene status="draft"

  Scenario: Name vacio retorna 422
    Given usuario autenticado con p_wf_create
    When POST /workflows con name=""
    Then response status 422
```

#### Trazabilidad de Tests

| Test ID | Tipo | Descripcion |
|---------|------|-------------|
| TP-CFG-13-01 | Positivo | Crear workflow draft con estados y transiciones |
| TP-CFG-13-02 | Positivo | Editar workflow draft actualiza estados y transiciones |
| TP-CFG-13-03 | Positivo | Editar workflow activo revierte a draft automaticamente |
| TP-CFG-13-04 | Positivo | GET /workflows/:id retorna workflow completo con states, configs, fields, transitions |
| TP-CFG-13-05 | Negativo | Name vacio retorna 422 |
| TP-CFG-13-06 | Negativo | States vacio retorna 422 |
| TP-CFG-13-07 | Negativo | Workflow inexistente retorna 404 (PUT/GET) |
| TP-CFG-13-08 | Negativo | Sin permiso p_wf_create/p_wf_edit retorna 403 |
| TP-CFG-13-09 | Negativo | Sin JWT retorna 401 |
| TP-CFG-13-10 | Integracion | WorkflowCreatedEvent se publica y Audit lo registra |
| TP-CFG-13-11 | Integracion | WorkflowUpdatedEvent se publica con reverted_from_active |
| TP-CFG-13-12 | Integracion | Replace strategy elimina states/transitions previas y re-inserta |
| TP-CFG-13-13 | E2E | Crear workflow en editor visual, guardar, ver en listado |

#### Sin Ambiguedades

- **Supuestos prohibidos:** No se asume validacion de grafo al guardar como draft (RN-CFG-13). No se asume que el nombre de workflow es unico por tenant (el modelo no tiene constraint UNIQUE en name). No se asume lock optimista.
- **Decisiones cerradas:** Replace strategy para edicion (DELETE + re-INSERT de children). Workflow activo revierte a draft al editar (RN-CFG-12). Version no se incrementa al guardar draft (solo al activar, RN-CFG-15). ID generado como UUIDv7.
- **Fuera de alcance explicito:** Versionado con diff. Import/export de workflows. Clonado de workflow. Lock optimista.
- **TODO explicitos = 0**

---

### RF-CFG-14 — Configurar nodo por tipo con persistencia key-value

#### Execution Sheet

| Campo | Valor |
|-------|-------|
| **ID** | RF-CFG-14 |
| **Titulo** | Configurar nodo por tipo con persistencia key-value |
| **Actor(es)** | Super Admin, Disenador de Procesos |
| **Prioridad** | Alta |
| **Severidad** | P0 |
| **Flujo origen** | FL-CFG-02 seccion 8c (configuracion por tipo) |
| **HU origen** | HU022 |

#### Precondiciones

| # | Condicion | Detalle |
|---|-----------|---------|
| 1 | Workflow en edicion | Workflow cargado en editor visual |
| 2 | Nodo creado con tipo valido | `flow_node_type` asignado al state |

#### Entradas — Contrato por tipo de nodo

**service_call:**

| Key | Tipo | Requerido | Validacion | Persistencia |
|-----|------|-----------|------------|--------------|
| service_id | uuid | Si | Debe existir en `external_services` del tenant (validado al activar) | workflow_state_configs |
| endpoint | string | Si | max 500 chars, no vacio | workflow_state_configs |
| method | string | Si | Enum: GET, POST, PUT, DELETE, PATCH | workflow_state_configs |

**decision:**

| Key | Tipo | Requerido | Validacion | Persistencia |
|-----|------|-----------|------------|--------------|
| condition | string | Si | max 1000 chars, acepta `{{Objeto.atributo}}` | workflow_state_configs |

**send_message:**

| Key | Tipo | Requerido | Validacion | Persistencia |
|-----|------|-----------|------------|--------------|
| channel | string | Si | Enum: email, sms, whatsapp | workflow_state_configs |
| template_id | uuid | Si | Debe existir en `notification_templates` del tenant (validado al activar) | workflow_state_configs |

**data_capture:**

| Campo | Tipo | Requerido | Validacion | Persistencia |
|-------|------|-----------|------------|--------------|
| field_name | string | Si | max 100 chars, no vacio | workflow_state_fields |
| field_type | string | Si | Enum: text, number, boolean, date, select | workflow_state_fields |
| is_required | boolean | Si | — | workflow_state_fields |
| sort_order | integer | Si | >= 0 | workflow_state_fields |

**timer:**

| Key | Tipo | Requerido | Validacion | Persistencia |
|-----|------|-----------|------------|--------------|
| timer_minutes | integer | Si | > 0 | workflow_state_configs |

**start / end:** Sin configs adicionales. Solo nombre y label en `workflow_states`.

#### Pasos del Proceso (Happy Path)

| # | Paso | Responsable |
|---|------|-------------|
| 1 | Disenador hace click en nodo del canvas | Frontend |
| 2 | SPA abre NodeConfigDialog con campos especificos del tipo | Frontend |
| 3 | Disenador completa campos de configuracion | Frontend |
| 4 | SPA valida campos localmente (formato, requeridos) | Frontend |
| 5 | SPA almacena config en estado local del workflow | Frontend |
| 6 | Al guardar workflow (RF-CFG-13), configs se incluyen en el payload | Frontend |
| 7 | Config Service persiste en workflow_state_configs o workflow_state_fields | Config Service |

#### Salidas

| Campo | Tipo | Destino | Efecto |
|-------|------|---------|--------|
| states[].configs[] | array de {key, value} | Payload de RF-CFG-13 | Persistido en workflow_state_configs |
| states[].fields[] | array de {field_name, field_type, is_required, sort_order} | Payload de RF-CFG-13 | Persistido en workflow_state_fields |

#### Errores Tipados

| Codigo | Causa | Condicion | Respuesta |
|--------|-------|-----------|-----------|
| 422 | Config incompleta | Campo requerido faltante al guardar (ej: service_call sin service_id) | `{ "error": "Validation Error", "detail": "state '{name}': missing required config 'service_id'" }` |
| 422 | Tipo de field invalido | data_capture con field_type fuera del enum | `{ "error": "Validation Error", "detail": "state '{name}': invalid field_type '{value}'" }` |
| 422 | timer_minutes invalido | timer con valor <= 0 | `{ "error": "Validation Error", "detail": "state '{name}': timer_minutes must be > 0" }` |

**Nota:** La validacion de existencia de `service_id` y `template_id` se aplica al activar (RF-CFG-17, RN-CFG-16), no al guardar como draft.

#### Casos Especiales y Variantes

- **data_capture sin fields:** Se permite guardar un nodo data_capture sin fields en draft. La validacion al activar verificara que tenga al menos 1 field.
- **Lookups para selects:** Los selects de `service_id` y `template_id` se cargan desde GET /external-services y GET /notification-templates respectivamente.

#### Data Model Impact

| Entidad | Operacion | Evento |
|---------|-----------|--------|
| `workflow_state_configs` | INSERT, DELETE (como parte de RF-CFG-13) | — |
| `workflow_state_fields` | INSERT, DELETE (como parte de RF-CFG-13) | — |
| `external_services` | SELECT (lookup) | — |
| `notification_templates` | SELECT (lookup) | — |

#### Criterios de Aceptacion (Gherkin)

```gherkin
Feature: RF-CFG-14 Configurar nodo por tipo

  Scenario Outline: Configuracion valida por tipo de nodo
    Given workflow en edicion con nodo tipo <tipo>
    When disenador completa <campos> en NodeConfigDialog y guarda
    Then la configuracion se persiste en <tabla>

    Examples:
      | tipo          | campos                            | tabla                   |
      | service_call  | service_id, endpoint, method      | workflow_state_configs  |
      | decision      | condition                         | workflow_state_configs  |
      | send_message  | channel, template_id              | workflow_state_configs  |
      | timer         | timer_minutes                     | workflow_state_configs  |
      | data_capture  | field_name, field_type, is_req    | workflow_state_fields   |

  Scenario: timer_minutes invalido retorna 422
    Given nodo timer con timer_minutes = -5
    When se guarda el workflow
    Then response status 422
```

#### Trazabilidad de Tests

| Test ID | Tipo | Descripcion |
|---------|------|-------------|
| TP-CFG-14-01 | Positivo | service_call guarda config con service_id, endpoint, method |
| TP-CFG-14-02 | Positivo | decision guarda config con condition conteniendo {{Objeto.atributo}} |
| TP-CFG-14-03 | Positivo | send_message guarda config con channel y template_id |
| TP-CFG-14-04 | Positivo | data_capture guarda fields con field_name, field_type, is_required, sort_order |
| TP-CFG-14-05 | Positivo | timer guarda config con timer_minutes |
| TP-CFG-14-06 | Negativo | service_call sin service_id retorna 422 |
| TP-CFG-14-07 | Negativo | data_capture con field_type invalido retorna 422 |
| TP-CFG-14-08 | Negativo | timer con timer_minutes <= 0 retorna 422 |
| TP-CFG-14-09 | Integracion | Configs se persisten como key-value en workflow_state_configs |
| TP-CFG-14-10 | Integracion | Fields de data_capture se persisten en workflow_state_fields con sort_order |
| TP-CFG-14-11 | E2E | Configurar nodo service_call, guardar, recargar, verificar config |

#### Sin Ambiguedades

- **Supuestos prohibidos:** No se asume validacion de existencia de service_id/template_id al guardar draft (solo al activar, RN-CFG-16). No se asume validacion sintactica de expresiones `condition`. No se asume que start/end tienen campos de configuracion.
- **Decisiones cerradas:** Persistencia key-value para todos los tipos excepto data_capture (RN-CFG-19). Validacion de formato al guardar. Validacion de referencias solo al activar.
- **Fuera de alcance explicito:** Validacion de sintaxis de condiciones. Preview de configuracion. Configuracion avanzada de data_capture.
- **TODO explicitos = 0**

---

### RF-CFG-15 — Panel de atributos del dominio con insert en condiciones

#### Execution Sheet

| Campo | Valor |
|-------|-------|
| **ID** | RF-CFG-15 |
| **Titulo** | Panel de atributos del dominio con insert en condiciones |
| **Actor(es)** | Super Admin, Disenador de Procesos |
| **Prioridad** | Alta |
| **Severidad** | P1 |
| **Flujo origen** | FL-CFG-02 seccion 6 (decision + atributos) |
| **HU origen** | HU034 |

#### Precondiciones

| # | Condicion | Detalle |
|---|-----------|---------|
| 1 | Editor de workflow activo | WorkflowEditor.tsx cargado |
| 2 | Catalogo de atributos definido | Constante estatica en frontend con 11 objetos |

#### Catalogo de Objetos del Dominio

| Objeto | Atributos | Ejemplo |
|--------|-----------|---------|
| Persona (Solicitante) | 10 | firstName, lastName, documentType, documentNumber, email, phone, birthDate, age, gender, occupation |
| Domicilio | 9 | street, number, floor, apartment, city, province, postalCode, country, type |
| Solicitud | 9 | applicationNumber, requestedAmount, term, status, createdAt, entityName, branchName, channelCode, priority |
| Producto | 7 | code, name, type, familyCode, familyName, status, currency |
| SubProducto (Plan) | 6 | name, planType, rate, minAmount, maxAmount, maxTerm |
| SubProducto — Prestamo | 4 | loanType, amortizationType, gracePeriodMonths, earlyRepaymentFee |
| SubProducto — Seguro | 4 | insuranceType, coverageAmount, deductible, renewalPeriod |
| SubProducto — Tarjeta | 6 | cardBrand, cardType, creditLimit, annualFee, interestRate, gracePeriodDays |
| SubProducto — Cuenta | 4 | accountType, maintenanceFee, minimumBalance, interestRate |
| SubProducto — Inversion | 5 | investmentType, minimumDeposit, termDays, projectedReturn, currency |
| Cobertura (Seguro) | 4 | coverageName, maxAmount, deductible, waitingPeriodDays |

#### Pasos del Proceso (Happy Path) — Panel lateral

| # | Paso | Responsable |
|---|------|-------------|
| 1 | Editor carga y muestra panel lateral derecho con catalogo | Frontend |
| 2 | Catalogo se muestra agrupado por objeto, colapsado por defecto | Frontend |
| 3 | Disenador expande un grupo (ej: "Solicitud") | Frontend |
| 4 | Se muestran atributos con nombre descriptivo, ruta completa, y badge de tipo | Frontend |

#### Pasos del Proceso (Happy Path) — Insert en decision

| # | Paso | Responsable |
|---|------|-------------|
| 1 | Disenador hace doble click en nodo decision | Frontend |
| 2 | SPA abre NodeConfigDialog con campo "Condicion" y panel de atributos a la derecha | Frontend |
| 3 | Disenador posiciona cursor en campo Condicion | Frontend |
| 4 | Disenador hace click en atributo "Solicitud.requestedAmount" | Frontend |
| 5 | SPA inserta `{{Solicitud.requestedAmount}}` en la posicion del cursor | Frontend |
| 6 | Disenador completa expresion: `{{Solicitud.requestedAmount}} > 100000` | Frontend |

#### Salidas

| Campo | Tipo | Destino | Efecto |
|-------|------|---------|--------|
| Panel lateral | UI Component | WorkflowEditor | 11 objetos con atributos agrupados |
| Atributo.badge | UI Badge | Panel | Color por tipo: string=azul, number=verde, date=ambar, enum=rosa |
| Texto insertado | string | Campo condition | Formato `{{Objeto.atributo}}` en posicion del cursor |

#### Errores Tipados

| Codigo | Causa | Condicion | Respuesta (UI) |
|--------|-------|-----------|----------------|
| N/A | Click sin foco en campo | Campo Condicion no tiene foco | No se inserta nada; tooltip "Posicione el cursor en el campo Condicion" |

#### Casos Especiales y Variantes

- **Drag desde panel lateral al canvas:** Genera texto `{{Objeto.atributo}}` como referencia (uso libre del disenador).
- **Multiples inserciones:** Se pueden insertar varios atributos en la misma condicion.
- **Panel siempre visible:** El panel se muestra siempre en el editor; panel adicional en dialogo de decision con click-to-insert.

#### Data Model Impact

| Entidad | Operacion | Evento |
|---------|-----------|--------|
| (ninguna — frontend puro) | — | — |

#### Criterios de Aceptacion (Gherkin)

```gherkin
Feature: RF-CFG-15 Panel de atributos del dominio

  Scenario: Panel muestra catalogo completo agrupado
    Given editor de workflow activo
    Then panel lateral derecho muestra 11 objetos colapsados
    When expando "Persona (Solicitante)"
    Then se muestran 10 atributos con nombre, ruta y badge de tipo

  Scenario: Click en atributo inserta en condicion de decision
    Given dialogo de decision abierto con campo Condicion enfocado
    When click en atributo "Solicitud.requestedAmount"
    Then se inserta "{{Solicitud.requestedAmount}}" en la posicion del cursor

  Scenario: Click sin foco no inserta
    Given dialogo de decision con campo Condicion SIN foco
    When click en atributo
    Then no se inserta nada en el campo
```

#### Trazabilidad de Tests

| Test ID | Tipo | Descripcion |
|---------|------|-------------|
| TP-CFG-15-01 | Positivo | Panel muestra 11 objetos agrupados con atributos correctos |
| TP-CFG-15-02 | Positivo | Atributos muestran nombre, ruta y badge de tipo con color correcto |
| TP-CFG-15-03 | Positivo | Click en atributo con campo Condicion enfocado inserta {{Objeto.atributo}} |
| TP-CFG-15-04 | Positivo | Multiples inserciones en la misma condicion funcionan |
| TP-CFG-15-05 | Negativo | Click en atributo sin foco en campo no inserta nada |
| TP-CFG-15-06 | Negativo | Catalogo valida que tiene exactamente 11 objetos y ~68 atributos |
| TP-CFG-15-07 | Integracion | Catalogo completo con conteo de atributos por objeto |
| TP-CFG-15-08 | E2E | Abrir editor, abrir decision, insertar atributo, completar condicion, guardar |

#### Sin Ambiguedades

- **Supuestos prohibidos:** No se asume catalogo dinamico desde backend. No se asume validacion del catalogo contra modelo de datos en runtime. No se asume que atributos se resuelven en backend al guardar.
- **Decisiones cerradas:** Catalogo estatico en frontend (RN-CFG-18). Formato `{{Objeto.atributo}}`. Badges: string=azul, number=verde, date=ambar, enum=rosa. Panel siempre visible en editor.
- **Fuera de alcance explicito:** Catalogo dinamico. Autocompletado de expresiones. Validacion de sintaxis. Evaluacion en tiempo de diseno.
- **TODO explicitos = 0**

---

### RF-CFG-16 — Gestionar transiciones con label, condicion y SLA

#### Execution Sheet

| Campo | Valor |
|-------|-------|
| **ID** | RF-CFG-16 |
| **Titulo** | Gestionar transiciones con label, condicion y SLA |
| **Actor(es)** | Super Admin, Disenador de Procesos |
| **Prioridad** | Alta |
| **Severidad** | P1 |
| **Flujo origen** | FL-CFG-02 seccion 6 (conectar nodos) |
| **HU origen** | HU022 |

#### Precondiciones

| # | Condicion | Detalle |
|---|-----------|---------|
| 1 | Workflow en edicion | Al menos 2 estados existen en el canvas |
| 2 | Nodos tienen puertos de conexion | React Flow handles de conexion |

#### Entradas

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| from_state_id | uuid | Si | Drag desde puerto de salida | Debe existir en states[] del workflow | — |
| to_state_id | uuid | Si | Drop en puerto de entrada | Debe existir en states[], diferente de from | — |
| label | string | No | Dialogo de transicion | max 200 chars | — |
| condition | string | No | Dialogo de transicion | max 500 chars, acepta `{{Objeto.atributo}}` | — |
| sla_hours | integer | No | Dialogo de transicion | > 0 si presente | — |

#### Pasos del Proceso (Happy Path)

| # | Paso | Responsable |
|---|------|-------------|
| 1 | Disenador arrastra desde puerto de salida de un nodo | Frontend |
| 2 | Disenador suelta en puerto de entrada de otro nodo | Frontend |
| 3 | SPA crea transicion visual en React Flow | Frontend |
| 4 | SPA abre dialogo de propiedades de transicion | Frontend |
| 5 | Disenador completa label, condition, sla_hours (opcionales) | Frontend |
| 6 | SPA almacena transicion en estado local del workflow | Frontend |
| 7 | Al guardar workflow (RF-CFG-13), transiciones se incluyen en el payload | Frontend |
| 8 | Config Service persiste en `workflow_transitions` | Config Service |

#### Salidas

| Campo | Tipo | Destino | Efecto |
|-------|------|---------|--------|
| transitions[] | array | Payload de RF-CFG-13 | Persistido en workflow_transitions |
| Edge visual | React Flow Edge | Canvas | Linea con label entre nodos |

#### Errores Tipados

| Codigo | Causa | Condicion | Respuesta |
|--------|-------|-----------|-----------|
| 422 | Transicion reflexiva | from_state_id = to_state_id | `{ "error": "Validation Error", "detail": "Self-referencing transitions not allowed" }` |
| 422 | Transicion duplicada | Misma combinacion from + to ya existe | `{ "error": "Validation Error", "detail": "Duplicate transition" }` |
| 422 | sla_hours invalido | sla_hours <= 0 | `{ "error": "Validation Error", "detail": "sla_hours must be > 0" }` |

#### Casos Especiales y Variantes

- **Sin label:** Transicion se muestra como flecha sin texto.
- **Sin condition:** Transicion incondicional (siempre se sigue).
- **Multiples desde un nodo:** Permitido (bifurcacion, tipico en decision).
- **Transicion desde `end`:** No permitida (validacion en frontend).
- **Eliminar transicion:** Click en transicion + Delete.

#### Data Model Impact

| Entidad | Operacion | Evento |
|---------|-----------|--------|
| `workflow_transitions` | INSERT, DELETE (como parte de RF-CFG-13) | — |

#### Criterios de Aceptacion (Gherkin)

```gherkin
Feature: RF-CFG-16 Gestionar transiciones

  Scenario: Crear transicion entre nodos
    Given workflow con nodo start y nodo decision
    When disenador arrastra desde start hasta decision
    Then se crea transicion visual

  Scenario: Transicion con propiedades
    Given transicion creada
    When disenador configura label="Aprobado", condition="{{Solicitud.requestedAmount}} < 50000", sla_hours=24
    And guarda el workflow
    Then workflow_transitions contiene la transicion con los 3 campos

  Scenario: Transicion reflexiva retorna 422
    Given nodo decision existente
    When se intenta crear transicion de decision a decision (mismo nodo)
    Then response status 422
```

#### Trazabilidad de Tests

| Test ID | Tipo | Descripcion |
|---------|------|-------------|
| TP-CFG-16-01 | Positivo | Crear transicion drag-and-drop entre dos nodos |
| TP-CFG-16-02 | Positivo | Configurar label, condition y sla_hours |
| TP-CFG-16-03 | Positivo | Multiples transiciones desde nodo decision (bifurcacion) |
| TP-CFG-16-04 | Negativo | Transicion reflexiva (from = to) retorna 422 |
| TP-CFG-16-05 | Negativo | Transicion duplicada retorna 422 |
| TP-CFG-16-06 | Negativo | sla_hours <= 0 retorna 422 |
| TP-CFG-16-07 | Integracion | Transiciones se persisten con from/to IDs correctos |
| TP-CFG-16-08 | Integracion | Eliminar transicion y guardar verifica DELETE en DB |
| TP-CFG-16-09 | E2E | Conectar nodos, configurar transicion, guardar, recargar, verificar |

#### Sin Ambiguedades

- **Supuestos prohibidos:** No se asume que toda transicion requiere label. No se asume limite maximo de transiciones. No se asume validacion sintactica de condiciones.
- **Decisiones cerradas:** Reflexivas no permitidas. Duplicadas (misma from/to) no permitidas. Desde `end` no permitidas. Label y condition opcionales.
- **Fuera de alcance explicito:** Validacion de sintaxis de condiciones. Probabilidades en bifurcaciones.
- **TODO explicitos = 0**

---

### RF-CFG-17 — Activar workflow con validacion de grafo

#### Execution Sheet

| Campo | Valor |
|-------|-------|
| **ID** | RF-CFG-17 |
| **Titulo** | Activar workflow con validacion de grafo |
| **Actor(es)** | Super Admin, Disenador de Procesos |
| **Prioridad** | Alta |
| **Severidad** | P0 |
| **Flujo origen** | FL-CFG-02 seccion 7 (activar con validacion) |
| **HU origen** | HU022 |

#### Precondiciones

| # | Condicion | Detalle |
|---|-----------|---------|
| 1 | Config Service operativo | SA.config accesible |
| 2 | Usuario autenticado | JWT valido con claim `tenant_id` |
| 3 | Permiso requerido | `p_wf_activate` |
| 4 | Workflow existe | Estado draft o inactive |

#### Entradas

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| id | uuid | Si | Path param | Workflow existente del tenant | — |

#### Pasos del Proceso (Happy Path)

| # | Paso | Responsable |
|---|------|-------------|
| 1 | SPA envia PUT /workflows/:id/activate | Frontend |
| 2 | API Gateway valida JWT y enruta | Gateway |
| 3 | Config Service valida permiso `p_wf_activate` | Config Service |
| 4 | Cargar grafo completo: states + transitions + configs + fields | SA.config |
| 5 | Validar: exactamente 1 nodo start (RN-CFG-14) | Config Service |
| 6 | Validar: al menos 1 nodo end (RN-CFG-14) | Config Service |
| 7 | Validar: todos los nodos conectados (RN-CFG-14) | Config Service |
| 8 | Validar: sin ciclos que no pasen por end (RN-CFG-14) | Config Service |
| 9 | Validar: service_call refs → external_services existen y activos (RN-CFG-16) | Config Service |
| 10 | Validar: send_message refs → notification_templates existen (RN-CFG-16) | Config Service |
| 11 | Validar: data_capture nodes tienen al menos 1 field | Config Service |
| 12 | UPDATE status = active, version++ (RN-CFG-15) | SA.config |
| 13 | Publicar WorkflowPublishedEvent { workflow_id, version, tenant_id } | RabbitMQ |
| 14 | Retorna 200 { id, status: "active", version } | Config Service |

#### Salidas

| Campo | Tipo | Destino | Efecto |
|-------|------|---------|--------|
| id | uuid | Response body | ID del workflow |
| status | string | Response body | "active" |
| version | integer | Response body | Version incrementada |
| updated_at | timestamp | Response body | Fecha de activacion |

#### Errores Tipados

| Codigo | Causa | Condicion | Respuesta |
|--------|-------|-----------|-----------|
| 401 | JWT ausente | Header Authorization faltante | `{ "error": "Unauthorized" }` |
| 403 | Sin permiso | Sin `p_wf_activate` | `{ "error": "Forbidden" }` |
| 404 | No encontrado | ID inexistente o de otro tenant | `{ "error": "Not Found" }` |
| 409 | Ya activo | status=active | `{ "error": "Conflict", "detail": "Workflow already active" }` |
| 422 | Sin nodo start | 0 o >1 nodos start | `{ "error": "Validation Error", "errors": ["Debe haber exactamente un nodo de inicio"] }` |
| 422 | Sin nodo end | 0 nodos end | `{ "error": "Validation Error", "errors": ["Debe haber al menos un nodo de fin"] }` |
| 422 | Nodo huerfano | Nodo sin transiciones | `{ "error": "Validation Error", "errors": ["Nodo '{name}' no tiene conexiones"] }` |
| 422 | Ciclo sin end | Ciclo detectado | `{ "error": "Validation Error", "errors": ["Ciclo detectado entre '{node_a}' y '{node_b}' sin nodo de fin"] }` |
| 422 | Servicio no encontrado | service_call ref invalida | `{ "error": "Validation Error", "errors": ["Nodo '{name}': servicio externo '{id}' no encontrado o inactivo"] }` |
| 422 | Plantilla no encontrada | send_message ref invalida | `{ "error": "Validation Error", "errors": ["Nodo '{name}': plantilla '{id}' no encontrada"] }` |
| 422 | data_capture sin fields | Nodo sin fields | `{ "error": "Validation Error", "errors": ["Nodo '{name}': data_capture debe tener al menos un campo"] }` |

#### Casos Especiales y Variantes

- **Multiples errores:** La validacion retorna TODOS los errores (no falla en el primero). Array `errors[]` puede tener multiples entradas.
- **Workflow inactive:** Se puede activar directamente (no requiere pasar por draft).
- **Version:** Se incrementa en 1. Primer activacion: 0 → 1.

#### Data Model Impact

| Entidad | Operacion | Evento |
|---------|-----------|--------|
| `workflow_definitions` | UPDATE (status, version, updated_at) | WorkflowPublishedEvent |
| `external_services` | SELECT (validacion) | — |
| `notification_templates` | SELECT (validacion) | — |

#### Criterios de Aceptacion (Gherkin)

```gherkin
Feature: RF-CFG-17 Activar workflow con validacion

  Scenario: Activacion exitosa
    Given workflow draft con grafo valido y referencias validas
    When PUT /workflows/:id/activate
    Then response status 200
    And status="active", version incrementada
    And WorkflowPublishedEvent publicado

  Scenario: Sin nodo start retorna 422
    Given workflow draft sin nodo start
    When PUT /workflows/:id/activate
    Then response status 422 con errors[]

  Scenario: Multiples errores juntos
    Given workflow draft sin start, sin end, con nodo huerfano
    When PUT /workflows/:id/activate
    Then response status 422 con 3+ mensajes en errors[]
```

#### Trazabilidad de Tests

| Test ID | Tipo | Descripcion |
|---------|------|-------------|
| TP-CFG-17-01 | Positivo | Grafo valido se activa con version++ |
| TP-CFG-17-02 | Positivo | WorkflowPublishedEvent se publica |
| TP-CFG-17-03 | Negativo | Sin nodo start retorna 422 |
| TP-CFG-17-04 | Negativo | Multiples nodos start retorna 422 |
| TP-CFG-17-05 | Negativo | Sin nodo end retorna 422 |
| TP-CFG-17-06 | Negativo | Nodo huerfano retorna 422 |
| TP-CFG-17-07 | Negativo | Ciclo sin end retorna 422 |
| TP-CFG-17-08 | Negativo | Servicio externo inexistente/inactivo retorna 422 |
| TP-CFG-17-09 | Negativo | Plantilla inexistente retorna 422 |
| TP-CFG-17-10 | Negativo | data_capture sin fields retorna 422 |
| TP-CFG-17-11 | Negativo | Workflow ya activo retorna 409 |
| TP-CFG-17-12 | Negativo | Sin permiso retorna 403 |
| TP-CFG-17-13 | Negativo | Sin JWT retorna 401 |
| TP-CFG-17-14 | Integracion | Multiples errores en un solo response |
| TP-CFG-17-15 | Integracion | Version incrementa correctamente (0→1, 1→2) |
| TP-CFG-17-16 | E2E | Activar workflow desde UI, ver cambio de estado y version |

#### Sin Ambiguedades

- **Supuestos prohibidos:** No se asume que la validacion falla al primer error. No se asume que inactive debe pasar por draft para activarse. No se asume validacion de sintaxis de condiciones.
- **Decisiones cerradas:** Multiples errores en array `errors[]`. Version incrementa al activar (RN-CFG-15). Ya activo retorna 409. Validacion de referencias incluye check de status=active para servicios.
- **Fuera de alcance explicito:** Validacion de sintaxis de expresiones. Simulacion previa. Activacion condicional.
- **TODO explicitos = 0**

---

### RF-CFG-18 — Desactivar workflow

#### Execution Sheet

| Campo | Valor |
|-------|-------|
| **ID** | RF-CFG-18 |
| **Titulo** | Desactivar workflow |
| **Actor(es)** | Super Admin, Disenador de Procesos |
| **Prioridad** | Media |
| **Severidad** | P2 |
| **Flujo origen** | FL-CFG-02 seccion 8b (desactivar) |
| **HU origen** | HU022 |

#### Precondiciones

| # | Condicion | Detalle |
|---|-----------|---------|
| 1 | Config Service operativo | SA.config accesible |
| 2 | Usuario autenticado | JWT valido con claim `tenant_id` |
| 3 | Permiso requerido | `p_wf_activate` (mismo permiso que activar) |
| 4 | Workflow con status=active | Solo se puede desactivar un workflow activo |

#### Entradas

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| id | uuid | Si | Path param | Workflow existente del tenant con status=active | — |

#### Pasos del Proceso (Happy Path)

| # | Paso | Responsable |
|---|------|-------------|
| 1 | SPA envia PUT /workflows/:id/deactivate | Frontend |
| 2 | API Gateway valida JWT y enruta | Gateway |
| 3 | Config Service valida permiso `p_wf_activate` | Config Service |
| 4 | Verificar que workflow tiene status=active | Config Service |
| 5 | UPDATE status = inactive | SA.config |
| 6 | Publicar WorkflowDeactivatedEvent { workflow_id, tenant_id } | RabbitMQ |
| 7 | Retorna 200 { id, status: "inactive" } | Config Service |

#### Salidas

| Campo | Tipo | Destino | Efecto |
|-------|------|---------|--------|
| id | uuid | Response body | ID del workflow |
| status | string | Response body | "inactive" |
| updated_at | timestamp | Response body | Fecha de desactivacion |

#### Errores Tipados

| Codigo | Causa | Condicion | Respuesta |
|--------|-------|-----------|-----------|
| 401 | JWT ausente | Header Authorization faltante | `{ "error": "Unauthorized" }` |
| 403 | Sin permiso | Sin `p_wf_activate` | `{ "error": "Forbidden" }` |
| 404 | No encontrado | ID inexistente o de otro tenant | `{ "error": "Not Found" }` |
| 409 | No es activo | status != active | `{ "error": "Conflict", "detail": "Only active workflows can be deactivated" }` |

#### Casos Especiales y Variantes

- **Solicitudes en curso:** Continuan con la version vigente (RN-CFG-17). La desactivacion solo impide nuevas solicitudes.
- **Re-edicion:** Un workflow inactive puede editarse (RF-CFG-13), lo cual lo lleva a draft. Luego puede reactivarse.
- **Version:** No se incrementa al desactivar.

#### Data Model Impact

| Entidad | Operacion | Evento |
|---------|-----------|--------|
| `workflow_definitions` | UPDATE (status, updated_at) | WorkflowDeactivatedEvent |

#### Criterios de Aceptacion (Gherkin)

```gherkin
Feature: RF-CFG-18 Desactivar workflow

  Scenario: Desactivacion exitosa
    Given workflow con status="active"
    When PUT /workflows/:id/deactivate
    Then response status 200 y status="inactive"
    And WorkflowDeactivatedEvent publicado

  Scenario: Workflow draft retorna 409
    Given workflow con status="draft"
    When PUT /workflows/:id/deactivate
    Then response status 409

  Scenario: Solicitudes en curso no afectadas
    Given workflow activo con solicitudes en proceso
    When se desactiva
    Then solicitudes continuan normalmente
```

#### Trazabilidad de Tests

| Test ID | Tipo | Descripcion |
|---------|------|-------------|
| TP-CFG-18-01 | Positivo | Desactivacion exitosa de workflow activo |
| TP-CFG-18-02 | Negativo | Workflow draft retorna 409 |
| TP-CFG-18-03 | Negativo | Workflow inactive retorna 409 |
| TP-CFG-18-04 | Negativo | Workflow inexistente retorna 404 |
| TP-CFG-18-05 | Negativo | Sin permiso retorna 403 |
| TP-CFG-18-06 | Negativo | Sin JWT retorna 401 |
| TP-CFG-18-07 | Integracion | WorkflowDeactivatedEvent publicado y Audit registra |
| TP-CFG-18-08 | Integracion | Solicitudes en curso no afectadas |
| TP-CFG-18-09 | E2E | Desactivar desde UI, verificar estado en grilla |

#### Sin Ambiguedades

- **Supuestos prohibidos:** No se asume que desactivar afecta solicitudes en curso (RN-CFG-17). No se asume que es irreversible. No se asume eliminacion de workflows.
- **Decisiones cerradas:** Solo activos se pueden desactivar (409 para draft/inactive). Version no incrementa. Mismo permiso `p_wf_activate` para activar/desactivar.
- **Fuera de alcance explicito:** Eliminacion de workflows. Desactivacion automatica. Notificacion a solicitudes en curso.
- **TODO explicitos = 0**

---

### RF-CFG-19 — CRUD de plantillas de notificacion

#### Execution Sheet

| Campo | Valor |
|-------|-------|
| **ID** | RF-CFG-19 |
| **Titulo** | CRUD de plantillas de notificacion |
| **Actor(es)** | Super Admin, Disenador de Procesos |
| **Prioridad** | Alta |
| **Severidad** | P0 |
| **Flujo origen** | FL-CFG-01 (dependencia de FL-OPS-03 y FL-CFG-02 nodos send_message) |
| **HU origen** | HU022 (nodos send_message), HU037 (envio de mensajes) |

#### Precondiciones

| # | Condicion | Detalle |
|---|-----------|---------|
| 1 | Config Service operativo | SA.config accesible |
| 2 | Usuario autenticado | JWT valido con permiso `p_cfg_notif_list` (listar), `p_cfg_notif_create` (crear/editar), `p_cfg_notif_delete` (eliminar) |
| 3 | RLS activo | `notification_templates` filtrado por `tenant_id` |

#### Entradas

**Listar:** `GET /notification-templates`

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| `channel` | string | No | Query param | Enum: `email`, `sms`, `whatsapp` | — |
| `search` | string | No | Query param | ILIKE sobre `code` y `name` | — |
| `page` | integer | No (default 1) | Query param | >= 1 | — |
| `page_size` | integer | No (default 20) | Query param | 1-100 | — |

**Crear:** `POST /notification-templates`

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| `code` | string | Si | Body JSON | No vacio, max 50 chars, UNIQUE por tenant | RN-CFG-20 |
| `name` | string | Si | Body JSON | No vacio, max 200 chars | — |
| `channel` | string | Si | Body JSON | Enum: `email`, `sms`, `whatsapp` | RN-CFG-21 |
| `subject` | string | Condicional | Body JSON | Requerido si channel=email, max 200 chars | — |
| `body` | string | Si | Body JSON | No vacio, max 4000 chars. Puede contener `{{variable}}` | — |
| `status` | string | No (default `active`) | Body JSON | Enum: `active`, `inactive` | — |

**Editar:** `PUT /notification-templates/:id`

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| `id` | uuid | Si | Path param | Debe existir en tenant | — |
| `name` | string | Si | Body JSON | No vacio, max 200 chars | — |
| `subject` | string | Condicional | Body JSON | Requerido si channel=email | — |
| `body` | string | Si | Body JSON | No vacio, max 4000 chars | — |
| `status` | string | No | Body JSON | Enum: `active`, `inactive` | — |

> **Nota:** `code` y `channel` no son editables despues de la creacion.

**Eliminar:** `DELETE /notification-templates/:id`

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| `id` | uuid | Si | Path param | Debe existir en tenant; no referenciado por workflows activos | — |

#### Proceso (Happy Path)

| # | Paso | Responsable |
|---|------|-------------|
| 1 | Recibir request (GET/POST/PUT/DELETE) | API Gateway |
| 2 | Validar JWT y permisos | Config Service |
| 3 | Aplicar RLS por tenant_id | Config Service |
| 4 | Ejecutar operacion CRUD en `notification_templates` | SA.config |
| 5 | Publicar evento async (NotificationTemplateCreatedEvent / UpdatedEvent / DeletedEvent) | RabbitMQ → Audit |
| 6 | Retornar respuesta | Config Service |

#### Salidas

**Listar:** `200 { items[], total, page, page_size }`

| Campo | Tipo | Descripcion |
|-------|------|-------------|
| `items[].id` | uuid | ID de plantilla |
| `items[].code` | string | Codigo unico |
| `items[].name` | string | Nombre descriptivo |
| `items[].channel` | string | Canal (email/sms/whatsapp) |
| `items[].status` | string | Estado (active/inactive) |
| `items[].created_at` | ISO 8601 | Fecha de creacion |

**Crear:** `201 Created { id, code, name, channel, status }`
**Editar:** `200 OK { id, code, name, channel, status }`
**Eliminar:** `204 No Content`

#### Errores Tipados

| Codigo | HTTP | Causa |
|--------|------|-------|
| CFG-19-E01 | 401 | JWT ausente o invalido |
| CFG-19-E02 | 403 | Sin permiso requerido |
| CFG-19-E03 | 404 | Plantilla no encontrada |
| CFG-19-E04 | 409 | Codigo duplicado en tenant (RN-CFG-20) |
| CFG-19-E05 | 409 | Plantilla referenciada por workflow activo (no eliminable) |
| CFG-19-E06 | 422 | Validacion fallida (campos requeridos, formato, channel invalido) |

#### Casos Especiales

| Variante | Comportamiento |
|----------|---------------|
| Filtro por canal | `GET /notification-templates?channel=email` retorna solo plantillas de email |
| Plantilla inactiva | No aparece en selects de FL-OPS-03 ni FL-CFG-02 (nodos send_message). Las ya referenciadas en workflows draft no se afectan |
| Eliminacion con referencia en workflow | Si la plantilla esta referenciada por un workflow activo, retorna 409. Si el workflow esta en draft, se permite eliminar (el draft fallara al activar) |
| Variables en body | Las variables `{{...}}` son texto libre. No se validan al guardar la plantilla. Se validan al resolver en RF-OPS-15 |

#### Criterios de Aceptacion (Gherkin)

```gherkin
Feature: RF-CFG-19 CRUD de plantillas de notificacion

  Background:
    Given usuario autenticado como Super Admin
    And tenant_id "T1" en JWT

  Scenario: Listar plantillas
    When GET /notification-templates
    Then status 200
    And response contiene "items" array con plantillas del tenant

  Scenario: Listar filtrado por canal
    When GET /notification-templates?channel=sms
    Then status 200
    And todos los items tienen channel = "sms"

  Scenario: Crear plantilla de email
    When POST /notification-templates { code: "WELCOME", name: "Bienvenida", channel: "email", subject: "Bienvenido {{nombre}}", body: "Hola {{nombre}}, tu solicitud {{codigo_solicitud}} fue recibida." }
    Then status 201
    And response contiene id, code, name, channel, status

  Scenario: 409 codigo duplicado
    Given existe plantilla con code "WELCOME"
    When POST /notification-templates { code: "WELCOME", ... }
    Then status 409

  Scenario: Editar plantilla
    When PUT /notification-templates/:id { name: "Bienvenida v2", body: "..." }
    Then status 200

  Scenario: Eliminar plantilla sin referencias
    When DELETE /notification-templates/:id
    Then status 204

  Scenario: 409 eliminar plantilla referenciada por workflow activo
    Given workflow activo referencia plantilla
    When DELETE /notification-templates/:id
    Then status 409
```

#### Trazabilidad de Pruebas

| TP | Escenario | Tipo |
|----|-----------|------|
| TP-CFG-19-01 | Listar plantillas por tenant | Positivo |
| TP-CFG-19-02 | Filtrar por canal | Positivo |
| TP-CFG-19-03 | Crear plantilla de email con variables | Positivo |
| TP-CFG-19-04 | Crear plantilla de sms | Positivo |
| TP-CFG-19-05 | Editar body y name | Positivo |
| TP-CFG-19-06 | Eliminar plantilla sin referencias | Positivo |
| TP-CFG-19-07 | 409 codigo duplicado | Negativo |
| TP-CFG-19-08 | 409 eliminar con workflow activo | Negativo |
| TP-CFG-19-09 | 422 channel invalido | Negativo |
| TP-CFG-19-10 | 422 subject vacio para email | Negativo |
| TP-CFG-19-11 | 401 sin JWT | Negativo |
| TP-CFG-19-12 | 403 sin permiso | Negativo |
| TP-CFG-19-13 | Aislamiento multi-tenant | Integracion |
| TP-CFG-19-14 | Evento de auditoria publicado | Integracion |
| TP-CFG-19-15 | E2E: crear plantilla → usar en envio de mensaje | E2E |

**Total RF-CFG-19:** 6 positivos + 6 negativos + 2 integracion + 1 E2E = **15 tests**

#### Sin Ambiguedades

- **Supuestos prohibidos:** No se asume que plantillas existen sin este RF. No se asume que code es editable post-creacion.
- **Decisiones cerradas:** `code` y `channel` inmutables post-creacion. Subject requerido solo para email. Eliminacion fisica (no soft-delete). Validacion de referencia solo contra workflows activos.
- **Fuera de alcance explicito:** Versionado de plantillas. Preview renderizado de plantilla. Import/export de plantillas entre tenants.
- **TODO explicitos = 0**

---

## Changelog

### v2.1.0 (2026-03-15)
- RF-CFG-19 documentado (CRUD de plantillas de notificacion)
- 2 reglas de negocio nuevas (RN-CFG-20, RN-CFG-21)
- Total: 19 RF, 21 reglas de negocio
- Correccion de revision cruzada FL↔RF: dependencia critica de FL-OPS-03 y FL-CFG-02 sin RF cubierta

### v2.0.0 (2026-03-15)
- RF-CFG-12 a RF-CFG-18 documentados (FL-CFG-02: Disenar Workflow)
- 8 reglas de negocio nuevas (RN-CFG-12 a RN-CFG-19)
- Total: 18 RF, 19 reglas de negocio

### v1.0.0 (2026-03-15)
- RF-CFG-01 a RF-CFG-11 documentados (FL-CFG-01: Gestionar Parametros y Servicios Externos)
- 11 reglas de negocio (RN-CFG-01 a RN-CFG-11)
