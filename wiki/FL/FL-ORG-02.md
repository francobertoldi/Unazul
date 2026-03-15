# FL-ORG-02 — Gestionar Entidades y Sucursales

> **Dominio:** Organization
> **Version:** 1.0.0
> **HUs:** HU005, HU006, HU007, HU008

---

## 1. Objetivo

Permitir la gestion completa de entidades financieras (bancos, aseguradoras, fintechs, cooperativas, SGRs, tarjetas regionales) y sus sucursales, incluyendo canales habilitados y selects parametrizados en cascada.

## 2. Alcance

**Dentro:**
- Listar entidades con filtros, busqueda y exportacion.
- Crear y editar entidades con tipo, canales, provincia/ciudad en cascada.
- Ver detalle de entidad con sucursales y usuarios asignados.
- CRUD de sucursales dentro de una entidad.
- Eliminacion de entidades (solo sin sucursales) o desactivacion.

**Fuera:**
- Gestion de organizaciones (ver FL-ORG-01).
- Configuracion de canales por entidad (solo habilitacion on/off).
- Arbol organizacional visual (mencionado en alcance, fuera del MVP detallado).

## 3. Actores y Ownership

| Actor | Rol en el flujo |
|-------|----------------|
| Super Admin | CRUD completo de entidades y sucursales de cualquier organizacion |
| Admin Entidad | CRUD de entidades y sucursales de su organizacion |
| Consulta / Auditor / Operador | Solo lectura (listar, ver detalle) |
| Organization Service | Persiste entidades, canales y sucursales |
| Config Service | Provee parametros: provinces, cities, entity_types, channels (via cache) |
| Audit Service | Registra cambios via eventos async |

## 4. Precondiciones

- Organization Service y SA.organization operativos.
- Al menos una organizacion activa (para asociar entidades).
- Parametros cargados: `provinces`, `cities` (con parent_key), `entity_types`, `channels`.

## 5. Postcondiciones

- Entidad creada: registro en `entities` + `entity_channels`, EntityCreatedEvent publicado.
- Entidad editada: registros actualizados, EntityUpdatedEvent publicado.
- Entidad sin sucursales eliminada: DELETE fisico + EntityDeletedEvent.
- Entidad con sucursales desactivada: status = inactive/suspended + EntityUpdatedEvent.
- Sucursal creada/editada/eliminada: registros en `branches`, BranchCreatedEvent/BranchUpdatedEvent/BranchDeletedEvent.

## 6. Secuencia Principal — Entidades

```mermaid
sequenceDiagram
    participant A as Admin
    participant SPA as Frontend SPA
    participant GW as API Gateway
    participant ORG as Organization Service
    participant DB as SA.organization
    participant RMQ as RabbitMQ
    participant AUD as Audit Svc

    Note over A,SPA: Listar entidades
    A->>SPA: Navegar a /entidades
    SPA->>GW: GET /entities?page&search&type&status
    GW->>ORG: Forward (RLS por tenant)
    ORG->>DB: SELECT entities con filtros + paginacion
    ORG-->>SPA: 200 { items[], total }

    Note over A,SPA: Crear entidad
    A->>SPA: Click "Nueva Entidad"
    SPA->>SPA: Cargar parametros: provinces, entity_types, channels (cache)
    A->>SPA: Seleccionar organizacion + tipo + completar datos
    A->>SPA: Seleccionar provincia
    SPA->>SPA: Filtrar ciudades por parent_key de provincia
    A->>SPA: Seleccionar ciudad + marcar canales habilitados
    SPA->>GW: POST /entities { name, identifier, type, status, email, phone_code, phone, address, province, city, country, channels[] }
    GW->>ORG: Forward
    ORG->>DB: Validar identifier unico por tenant
    ORG->>DB: INSERT entities + INSERT entity_channels (batch)
    ORG->>RMQ: EntityCreatedEvent
    RMQ-->>AUD: Registrar creacion
    ORG-->>SPA: 201 Created
    SPA-->>A: Redirigir a /entidades

    Note over A,SPA: Ver detalle
    A->>SPA: Click en fila
    SPA->>GW: GET /entities/:id
    GW->>ORG: Forward
    ORG->>DB: SELECT entity + channels + branches + user count
    ORG-->>SPA: 200 { entity, channels[], branches[], user_count }
```

## 7. Secuencia — Sucursales

```mermaid
sequenceDiagram
    participant A as Admin
    participant SPA as Frontend SPA
    participant GW as API Gateway
    participant ORG as Organization Service
    participant DB as SA.organization
    participant RMQ as RabbitMQ
    participant AUD as Audit Svc

    Note over A,SPA: En edicion de entidad, seccion sucursales
    A->>SPA: Click "Agregar Sucursal"
    SPA-->>A: Formulario: nombre, codigo, direccion, provincia, ciudad, estado, responsable, telefono

    A->>SPA: Seleccionar provincia
    SPA->>SPA: Filtrar ciudades por parent_key
    A->>SPA: Completar y guardar

    SPA->>GW: POST /entities/:entityId/branches { name, code, address, province, city, status, manager, phone_code, phone }
    GW->>ORG: Forward
    ORG->>DB: Validar code unico por tenant
    ORG->>DB: INSERT branches
    ORG->>RMQ: BranchCreatedEvent
    RMQ-->>AUD: Registrar creacion
    ORG-->>SPA: 201 Created
    SPA-->>A: Sucursal agregada a la grilla

    Note over A,SPA: Editar sucursal
    A->>SPA: Click editar en sucursal
    SPA-->>A: Formulario precargado
    A->>SPA: Modificar campos + guardar
    SPA->>GW: PUT /entities/:entityId/branches/:id { ... }
    GW->>ORG: Forward
    ORG->>DB: UPDATE branches
    ORG->>RMQ: BranchUpdatedEvent
    RMQ-->>AUD: Registrar edicion
    ORG-->>SPA: 200 OK

    Note over A,SPA: Eliminar sucursal
    A->>SPA: Click eliminar en sucursal
    SPA->>GW: DELETE /entities/:entityId/branches/:id
    GW->>ORG: Forward
    ORG->>DB: DELETE branches
    ORG->>RMQ: BranchDeletedEvent
    RMQ-->>AUD: Registrar eliminacion
    ORG-->>SPA: 204 No Content
```

## 8. Secuencias Alternativas

### 8a. Eliminar / Desactivar Entidad

| Condicion | Resultado |
|-----------|-----------|
| Entidad sin sucursales | Eliminacion fisica + EntityDeletedEvent (HTTP 204) |
| Entidad con sucursales | HTTP 409 — informa `"Entidad tiene N sucursal(es). Solo se puede desactivar."` El admin debe desactivar via PUT /entities/:id (edicion de status) |

### 8b. Selects en Cascada (provincia → ciudad)

| Paso | Detalle |
|------|---------|
| 1 | Frontend carga provincias desde parametros (cache Config) |
| 2 | Al seleccionar provincia, filtra ciudades por `parent_key` |
| 3 | Mismo patron en entidad y sucursal |

### 8c. Canales Habilitados

| Paso | Detalle |
|------|---------|
| 1 | Checkboxes cargados desde parametro `channels` |
| 2 | Al guardar, se sincronizan en `entity_channels` (diff: insert nuevos, delete desmarcados) |

## 9. Slice de Arquitectura

- **Servicio owner:** Organization Service (.NET 10, SA.organization)
- **Comunicacion sync:** SPA → API Gateway → Organization Service
- **Comunicacion async:** Organization → RabbitMQ → Audit Service
- **Parametros:** Config Service provee provinces, cities, entity_types, channels (cache Redis)
- **RLS:** aplica a `entities`, `entity_channels`, `branches`

## 10. Data Touchpoints

| Entidad | Operacion | Evento |
|---------|-----------|--------|
| `entities` | INSERT, UPDATE, DELETE | EntityCreatedEvent, EntityUpdatedEvent, EntityDeletedEvent |
| `entity_channels` | INSERT, DELETE (sync con entidad) | — (incluido en Entity events) |
| `branches` | INSERT, UPDATE, DELETE | BranchCreatedEvent, BranchUpdatedEvent, BranchDeletedEvent |
| `audit_log` (SA.audit) | INSERT (async) | Consume eventos via RabbitMQ |

**Estados relevantes:**
- `entity_status`: active → inactive / suspended (entidades con sucursales no se eliminan)
- `entity_status` en branches: active, inactive, suspended (independiente de la entidad)

## 11. RF Candidatos para `04_RF.md`

| RF ID | Descripcion | Origen FL |
|-------------|-------------|-----------|
| RF-ORG-06 | Listar entidades con filtros, busqueda y exportacion | Seccion 6 |
| RF-ORG-07 | Crear/editar entidad con tipo, canales y selects en cascada | Seccion 6 |
| RF-ORG-08 | Ver detalle de entidad con sucursales y usuarios | Seccion 6 |
| RF-ORG-09 | Eliminar/desactivar entidad con validacion de dependencias | Seccion 8a |
| RF-ORG-10 | CRUD de sucursales con selects en cascada | Seccion 7 |

## 12. Riesgos y Mitigaciones

| Riesgo | Impacto | Mitigacion |
|--------|---------|------------|
| Entidad eliminada referenciada en Catalog/Identity | Alto | Solo eliminar sin sucursales; datos denormalizados en otros servicios; EntityDeletedEvent para limpieza eventual |
| Parametros de provincias/ciudades no cargados | Medio | Fallback a input text libre; seed data obligatorio |
| CUIT duplicado por tenant | Medio | Indice UNIQUE (tenant_id, identifier); validacion al crear/editar |
| Canal deshabilitado pero usado en solicitudes existentes | Bajo | Deshabilitar canal no afecta solicitudes ya creadas; solo nuevas |

## 13. RF Handoff Checklist

- [x] Actor ownership explicito en cada paso.
- [x] Diagramas explican el flujo sin prosa larga.
- [x] Riesgos y mitigaciones documentados.
- [x] Traducible a RF atomicos y testeables.
- [x] Dentro del limite de 2 paginas.
- [x] Sin dependencias criticas desconocidas.
