# Historias de Usuario — Unazul Backoffice

> **Total:** 38 historias · **Versión:** 3.0.0 · **Fecha:** 2026-03-15
> **Backend:** .NET Core 10 · Arquitectura de Microservicios
> **Especificaciones completas:** [docs/ESPECIFICACIONES.md](../ESPECIFICACIONES.md)

---

## Índice por Microservicio

### Identity Service (`identity/`)

| ID | Título | Prioridad | Estado |
|----|--------|:---------:|:------:|
| [HU023](identity/HU023-Listar-Usuarios.md) | Listar Usuarios | Alta | Implementada |
| [HU024](identity/HU024-Crear-Editar-Usuario.md) | Crear y Editar Usuario | Alta | Implementada |
| [HU025](identity/HU025-Ver-Detalle-Usuario.md) | Ver Detalle de Usuario | Media | Implementada |
| [HU026](identity/HU026-Gestionar-Roles-Permisos.md) | Gestionar Roles y Permisos Atómicos | Alta | Implementada |
| [HU038](identity/HU038-Autenticacion-Login.md) | Autenticación y Login | Alta | Pendiente |

### Organization Service (`organization/`)

| ID | Título | Prioridad | Estado |
|----|--------|:---------:|:------:|
| [HU002](organization/HU002-Listar-Organizaciones.md) | Listar Organizaciones | Alta | Implementada |
| [HU003](organization/HU003-Crear-Editar-Organizacion.md) | Crear y Editar Organización | Alta | Implementada |
| [HU004](organization/HU004-Ver-Detalle-Organizacion.md) | Ver Detalle de Organización | Media | Implementada |
| [HU005](organization/HU005-Listar-Entidades.md) | Listar Entidades | Alta | Implementada |
| [HU006](organization/HU006-Crear-Editar-Entidad.md) | Crear y Editar Entidad | Alta | Implementada |
| [HU007](organization/HU007-Ver-Detalle-Entidad.md) | Ver Detalle de Entidad | Media | Implementada |
| [HU008](organization/HU008-Gestionar-Sucursales.md) | Gestionar Sucursales | Media | Implementada |

### Catalog Service (`catalog/`)

| ID | Título | Prioridad | Estado |
|----|--------|:---------:|:------:|
| [HU014](catalog/HU014-Listar-Familias-Productos.md) | Listar Familias de Productos | Alta | Implementada |
| [HU015](catalog/HU015-Listar-Productos.md) | Listar Productos | Alta | Implementada |
| [HU016](catalog/HU016-Crear-Editar-Producto.md) | Crear y Editar Producto | Alta | Implementada |
| [HU017](catalog/HU017-Gestionar-SubProductos.md) | Gestionar Sub Productos (Planes) | Alta | Implementada |
| [HU018](catalog/HU018-Ver-Detalle-Producto.md) | Ver Detalle de Producto | Media | Implementada |
| [HU019](catalog/HU019-Gestionar-Planes-Comisiones.md) | Gestionar Planes de Comisiones | Media | Implementada |
| [HU030](catalog/HU030-Gestionar-Coberturas-Seguros.md) | Gestionar Coberturas de Seguros | Media | Implementada |
| [HU031](catalog/HU031-Gestionar-Requisitos-Producto.md) | Gestionar Requisitos de Producto | Media | Implementada |

### Operations Service (`operations/`)

| ID | Título | Prioridad | Estado |
|----|--------|:---------:|:------:|
| [HU009](operations/HU009-Listar-Solicitudes.md) | Listar Solicitudes | Alta | Implementada |
| [HU010](operations/HU010-Crear-Editar-Solicitud.md) | Crear y Editar Solicitud | Alta | Implementada |
| [HU011](operations/HU011-Ver-Detalle-Solicitud.md) | Ver Detalle de Solicitud | Alta | Implementada |
| [HU012](operations/HU012-Trazabilidad-Solicitud.md) | Trazabilidad de Solicitud (Timeline BPM) | Alta | Implementada |
| [HU013](operations/HU013-Domicilio-Solicitud.md) | Direcciones del Solicitante | Media | Implementada |
| [HU033](operations/HU033-Solapa-Producto-Solicitud.md) | Solapa Producto en Detalle de Solicitud | Alta | Implementada |
| [HU035](operations/HU035-Liquidar-Comisiones.md) | Liquidar Comisiones | Alta | Pendiente |
| [HU036](operations/HU036-Historial-Liquidaciones.md) | Historial de Liquidaciones | Alta | Pendiente |
| [HU037](operations/HU037-Enviar-Mensaje-Solicitud.md) | Enviar Mensaje desde Solicitud | Media | Pendiente |

### Config Service (`config/`)

| ID | Título | Prioridad | Estado |
|----|--------|:---------:|:------:|
| [HU020](config/HU020-Gestionar-Parametros.md) | Gestionar Parámetros del Sistema | Alta | Implementada |
| [HU021](config/HU021-Gestionar-Servicios-Externos.md) | Gestionar Servicios Externos | Media | Implementada |
| [HU022](config/HU022-Editor-Visual-Workflows.md) | Editor Visual de Workflows | Alta | Implementada |
| [HU034](config/HU034-Panel-Atributos-Workflow.md) | Panel de Atributos en Editor de Workflows | Alta | Implementada |

### Audit Service (`audit/`)

| ID | Título | Prioridad | Estado |
|----|--------|:---------:|:------:|
| [HU027](audit/HU027-Consultar-Auditoria.md) | Consultar Log de Auditoría | Alta | Implementada |

### Frontend (cross-cutting) (`frontend/`)

| ID | Título | Prioridad | Estado |
|----|--------|:---------:|:------:|
| [HU001](frontend/HU001-Visualizar-Dashboard.md) | Visualizar Dashboard | Alta | Implementada |
| [HU028](frontend/HU028-Navegacion-Sidebar.md) | Navegación del Sistema (Sidebar) | Alta | Implementada |
| [HU029](frontend/HU029-Componente-DataTable.md) | Componente DataTable Reutilizable | Alta | Implementada |
| [HU032](frontend/HU032-Dashboard-Graficos-Por-Entidad.md) | Dashboard: Gráficos por Entidad | Media | Implementada |

---

## Resumen por Servicio

| Servicio | Historias | Alta | Media | Pendientes |
|----------|:---------:|:----:|:-----:|:----------:|
| Identity | 5 | 4 | 1 | 1 |
| Organization | 7 | 3 | 4 | 0 |
| Catalog | 8 | 3 | 5 | 0 |
| Operations | 9 | 7 | 2 | 3 |
| Config | 4 | 3 | 1 | 0 |
| Audit | 1 | 1 | 0 | 0 |
| Frontend | 4 | 3 | 1 | 0 |
| **Total** | **38** | **24** | **14** | **4** |

---

## Resumen por Estado

| Estado | Cantidad |
|--------|:--------:|
| Implementada | 34 |
| Pendiente | 4 |

---

## Changelog

### v3.0.0 (2026-03-15)
- **Revisión de consistencia** contra modelo de datos, arquitectura y alcance funcional
- **Reorganización por microservicio:** HUs clasificadas en directorios identity/, organization/, catalog/, operations/, config/, audit/, frontend/
- Correcciones P0: HU009 (estado settled), HU011 (advertencia datos compartidos), HU013 (ownership applicant), HU017 (precio, inversiones, tarjetas), HU022 (7 tipos de nodo workflow)
- Correcciones P1: HU003/HU008 (phone_code), HU005 (entity_type), HU010 (labels enum), HU019 (módulo Catálogo, relación 1:N), HU024 (user_assignments), HU027 (campo operation)
- Correcciones P2: HU006 (province), HU011/HU033 (orden solapas con Producto)
- **HU035** — Liquidar Comisiones (gap G1: FL-OPS-02)
- **HU036** — Historial de Liquidaciones (gap G2)
- **HU037** — Enviar Mensaje desde Solicitud (gap G3: FL-OPS-03)
- **HU038** — Autenticación y Login (gap G5: FL-SEC-01)

### v1.3.0 (2026-03-10)
- **HU032** — Gráficos individuales por entidad en el Dashboard (reemplaza gráfico único)
- **HU033** — Solapa "Producto" en detalle de solicitud + columna "Producto" en grilla + documento en columna Solicitante + atributos extendidos del solicitante (birthDate, gender, occupation, age calculada)
- **HU034** — Panel de atributos arrastrables en el editor de workflows + integración en nodos de Decisión

### v1.2.0 (2026-03-09)
- HU001–HU031 — Implementación inicial completa
