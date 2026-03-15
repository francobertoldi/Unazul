# TP-ORG — Plan de Pruebas: Organizacion

> **Proyecto:** Unazul Backoffice
> **Version:** 1.0.0
> **Fecha:** 2026-03-15
> **Modulo:** Organization Service (SA.organization)
> **Fuente:** [RF-ORG](../RF/RF-ORG.md)

---

## 1. Resumen de Cobertura

| RF | Titulo | Positivos | Negativos | Integracion | E2E | Total |
|----|--------|-----------|-----------|-------------|-----|-------|
| RF-ORG-01 | Listar organizaciones | 5 | 4 | 2 | 1 | 12 |
| RF-ORG-02 | Crear organizacion | 2 | 6 | 2 | 1 | 11 |
| RF-ORG-03 | Editar organizacion | 3 | 5 | 2 | 1 | 11 |
| RF-ORG-04 | Ver detalle organizacion | 2 | 4 | 1 | 1 | 8 |
| RF-ORG-05 | Eliminar organizacion | 1 | 4 | 2 | 1 | 8 |
| RF-ORG-06 | Listar entidades | 6 | 4 | 3 | 2 | 15 |
| RF-ORG-07 | Crear/editar entidad | 7 | 9 | 3 | 3 | 22 |
| RF-ORG-08 | Ver detalle entidad | 5 | 4 | 2 | 2 | 13 |
| RF-ORG-09 | Eliminar/desactivar entidad | 3 | 5 | 3 | 2 | 13 |
| RF-ORG-10 | CRUD sucursales | 6 | 8 | 4 | 4 | 22 |
| **Total** | | **40** | **53** | **24** | **18** | **135** |

---

## 2. Casos de Prueba por RF

### TP-ORG-01 — Listar organizaciones con busqueda, filtros y exportacion

| Test ID | Tipo | Descripcion | RF |
|---------|------|-------------|-----|
| TP-ORG-01-01 | Positivo | Lista paginada con campos esperados (id, name, identifier, status, created_at) | RF-ORG-01 |
| TP-ORG-01-02 | Positivo | Busqueda ILIKE filtra por nombre e identifier | RF-ORG-01 |
| TP-ORG-01-03 | Positivo | Filtro por status retorna solo organizaciones del estado solicitado | RF-ORG-01 |
| TP-ORG-01-04 | Positivo | Exportacion Excel genera archivo correcto | RF-ORG-01 |
| TP-ORG-01-05 | Positivo | Exportacion CSV genera archivo correcto | RF-ORG-01 |
| TP-ORG-01-06 | Negativo | Sin permiso p_org_list retorna 403 | RF-ORG-01 |
| TP-ORG-01-07 | Negativo | Sin JWT retorna 401 | RF-ORG-01 |
| TP-ORG-01-08 | Negativo | page_size > 100 retorna 400 | RF-ORG-01 |
| TP-ORG-01-09 | Negativo | status invalido retorna 400 | RF-ORG-01 |
| TP-ORG-01-10 | Integracion | Admin Entidad solo ve su organizacion | RF-ORG-01 |
| TP-ORG-01-11 | Integracion | Ordenamiento por name/identifier/created_at funciona | RF-ORG-01 |
| TP-ORG-01-12 | E2E | Navegar a /tenants, ver grilla, buscar, filtrar, exportar | RF-ORG-01 |

### TP-ORG-02 — Crear organizacion con datos de contacto

| Test ID | Tipo | Descripcion | RF |
|---------|------|-------------|-----|
| TP-ORG-02-01 | Positivo | Creacion exitosa con todos los campos validos | RF-ORG-02 |
| TP-ORG-02-02 | Positivo | Creacion con status = inactive funciona | RF-ORG-02 |
| TP-ORG-02-03 | Negativo | Identifier duplicado retorna 409 | RF-ORG-02 |
| TP-ORG-02-04 | Negativo | Campo name vacio retorna 422 | RF-ORG-02 |
| TP-ORG-02-05 | Negativo | Email invalido retorna 422 | RF-ORG-02 |
| TP-ORG-02-06 | Negativo | Status invalido retorna 422 | RF-ORG-02 |
| TP-ORG-02-07 | Negativo | Sin permiso p_org_create retorna 403 | RF-ORG-02 |
| TP-ORG-02-08 | Negativo | Sin JWT retorna 401 | RF-ORG-02 |
| TP-ORG-02-09 | Integracion | TenantCreatedEvent se publica y Audit lo registra | RF-ORG-02 |
| TP-ORG-02-10 | Integracion | created_by y updated_by se setean del JWT | RF-ORG-02 |
| TP-ORG-02-11 | E2E | Navegar a /tenants/nuevo, completar formulario, guardar, ver en listado | RF-ORG-02 |

### TP-ORG-03 — Editar organizacion con entidades dependientes

| Test ID | Tipo | Descripcion | RF |
|---------|------|-------------|-----|
| TP-ORG-03-01 | Positivo | Edicion exitosa actualiza todos los campos | RF-ORG-03 |
| TP-ORG-03-02 | Positivo | GET /tenants/:id retorna tenant + entidades dependientes | RF-ORG-03 |
| TP-ORG-03-03 | Positivo | Desactivar organizacion con entidades activas se permite | RF-ORG-03 |
| TP-ORG-03-04 | Negativo | Identifier duplicado al editar retorna 409 | RF-ORG-03 |
| TP-ORG-03-05 | Negativo | Organizacion inexistente retorna 404 | RF-ORG-03 |
| TP-ORG-03-06 | Negativo | Campo name vacio retorna 422 | RF-ORG-03 |
| TP-ORG-03-07 | Negativo | Sin permiso p_org_edit retorna 403 | RF-ORG-03 |
| TP-ORG-03-08 | Negativo | Sin JWT retorna 401 | RF-ORG-03 |
| TP-ORG-03-09 | Integracion | TenantUpdatedEvent se publica y Audit lo registra | RF-ORG-03 |
| TP-ORG-03-10 | Integracion | updated_by y updated_at se actualizan correctamente | RF-ORG-03 |
| TP-ORG-03-11 | E2E | Navegar a /tenants/:id/editar, modificar, guardar, ver cambios | RF-ORG-03 |

### TP-ORG-04 — Ver detalle de organizacion con entidades dependientes

| Test ID | Tipo | Descripcion | RF |
|---------|------|-------------|-----|
| TP-ORG-04-01 | Positivo | GET /tenants/:id retorna datos completos + entidades | RF-ORG-04 |
| TP-ORG-04-02 | Positivo | Organizacion sin entidades retorna entities[] vacio | RF-ORG-04 |
| TP-ORG-04-03 | Negativo | Organizacion inexistente retorna 404 | RF-ORG-04 |
| TP-ORG-04-04 | Negativo | Sin permiso p_org_detail retorna 403 | RF-ORG-04 |
| TP-ORG-04-05 | Negativo | Admin Entidad accediendo a otro tenant retorna 403 | RF-ORG-04 |
| TP-ORG-04-06 | Negativo | Sin JWT retorna 401 | RF-ORG-04 |
| TP-ORG-04-07 | Integracion | Entidades se cargan con tipo y estado correcto | RF-ORG-04 |
| TP-ORG-04-08 | E2E | Navegar a /tenants/:id, ver detalle, click editar | RF-ORG-04 |

### TP-ORG-05 — Eliminar organizacion con validacion de dependencias

| Test ID | Tipo | Descripcion | RF |
|---------|------|-------------|-----|
| TP-ORG-05-01 | Positivo | Eliminacion exitosa de organizacion sin entidades | RF-ORG-05 |
| TP-ORG-05-02 | Negativo | Organizacion con entidades retorna 409 con conteo | RF-ORG-05 |
| TP-ORG-05-03 | Negativo | Organizacion inexistente retorna 404 | RF-ORG-05 |
| TP-ORG-05-04 | Negativo | Sin permiso p_org_delete retorna 403 | RF-ORG-05 |
| TP-ORG-05-05 | Negativo | Sin JWT retorna 401 | RF-ORG-05 |
| TP-ORG-05-06 | Integracion | TenantDeletedEvent se publica y Audit lo registra | RF-ORG-05 |
| TP-ORG-05-07 | Integracion | Eliminacion fisica verificada (SELECT post-DELETE retorna 0) | RF-ORG-05 |
| TP-ORG-05-08 | E2E | Click eliminar en grilla, confirmar, organizacion desaparece del listado | RF-ORG-05 |

---

## 3. Reglas de Negocio Validadas

| RN | Regla | Tests que la cubren |
|----|-------|-------------------|
| RN-ORG-01 | Unicidad de identifier (CUIT) global | TP-ORG-02-03, TP-ORG-03-04 |
| RN-ORG-02 | Eliminacion solo sin entidades | TP-ORG-05-01, TP-ORG-05-02 |
| RN-ORG-03 | Estados active/inactive | TP-ORG-01-03, TP-ORG-02-02, TP-ORG-03-03 |
| RN-ORG-04 | Tenants sin RLS | TP-ORG-01-10, TP-ORG-04-05 |
| RN-ORG-05 | Super Admin ve todas | TP-ORG-01-10, TP-ORG-04-05 |
| RN-ORG-06 | Eliminacion fisica con evento | TP-ORG-05-06, TP-ORG-05-07 |
| RN-ORG-07 | Exportacion limitada | TP-ORG-01-04, TP-ORG-01-05, TP-ORG-06-05, TP-ORG-06-06 |
| RN-ORG-08 | Unicidad CUIT entidad por tenant | TP-ORG-07-08 |
| RN-ORG-09 | Eliminacion condicionada a sucursales | TP-ORG-09-01, TP-ORG-09-04 |
| RN-ORG-10 | Canales como diff | TP-ORG-07-04 |
| RN-ORG-11 | Selects cascada provincia→ciudad | TP-ORG-07-10, TP-ORG-10-11, TP-ORG-10-20 |
| RN-ORG-12 | Unicidad codigo sucursal por tenant | TP-ORG-10-07 |
| RN-ORG-13 | Tipos entidad parametrizados | TP-ORG-07-09, TP-ORG-06-03 |
| RN-ORG-14 | Entidad eliminada referenciada | TP-ORG-09-11 |
| RN-ORG-15 | Eliminacion fisica de sucursal | TP-ORG-10-03, TP-ORG-10-18 |

---

## 4. Casos de Prueba — FL-ORG-02 (Entidades y Sucursales)

### TP-ORG-06 — Listar entidades con filtros, busqueda y exportacion

| Test ID | Tipo | Descripcion | RF |
|---------|------|-------------|-----|
| TP-ORG-06-01 | Positivo | Lista paginada con campos esperados (id, name, identifier, type, status) | RF-ORG-06 |
| TP-ORG-06-02 | Positivo | Busqueda ILIKE filtra por name e identifier | RF-ORG-06 |
| TP-ORG-06-03 | Positivo | Filtro por type retorna solo entidades del tipo | RF-ORG-06 |
| TP-ORG-06-04 | Positivo | Filtro por status funciona correctamente | RF-ORG-06 |
| TP-ORG-06-05 | Positivo | Exportacion Excel genera archivo correcto | RF-ORG-06 |
| TP-ORG-06-06 | Positivo | Exportacion CSV genera archivo correcto | RF-ORG-06 |
| TP-ORG-06-07 | Negativo | Sin permiso p_entities_list retorna 403 | RF-ORG-06 |
| TP-ORG-06-08 | Negativo | Sin JWT retorna 401 | RF-ORG-06 |
| TP-ORG-06-09 | Negativo | type invalido retorna 422 | RF-ORG-06 |
| TP-ORG-06-10 | Negativo | page_size > 100 retorna 422 | RF-ORG-06 |
| TP-ORG-06-11 | Integracion | RLS filtra por tenant | RF-ORG-06 |
| TP-ORG-06-12 | Integracion | Admin Entidad solo ve su entidad | RF-ORG-06 |
| TP-ORG-06-13 | Integracion | Ordenamiento por name/type/created_at funciona | RF-ORG-06 |
| TP-ORG-06-14 | E2E | Navegar a /entidades, ver grilla, buscar, filtrar, exportar | RF-ORG-06 |
| TP-ORG-06-15 | E2E | Click en fila navega a detalle | RF-ORG-06 |

### TP-ORG-07 — Crear/editar entidad con tipo, canales y selects en cascada

| Test ID | Tipo | Descripcion | RF |
|---------|------|-------------|-----|
| TP-ORG-07-01 | Positivo | Crear entidad retorna 201 con todos los campos | RF-ORG-07 |
| TP-ORG-07-02 | Positivo | Entidad se crea con canales (entity_channels) | RF-ORG-07 |
| TP-ORG-07-03 | Positivo | Editar entidad retorna 200 con campos actualizados | RF-ORG-07 |
| TP-ORG-07-04 | Positivo | Diff de canales: removidos se eliminan, agregados se insertan | RF-ORG-07 |
| TP-ORG-07-05 | Positivo | EntityCreatedEvent y EntityUpdatedEvent publicados | RF-ORG-07 |
| TP-ORG-07-06 | Positivo | Canales duplicados en el array se deduplicar | RF-ORG-07 |
| TP-ORG-07-07 | Positivo | Crear entidad sin canales es valido | RF-ORG-07 |
| TP-ORG-07-08 | Negativo | CUIT duplicado retorna 409 | RF-ORG-07 |
| TP-ORG-07-09 | Negativo | type invalido retorna 422 | RF-ORG-07 |
| TP-ORG-07-10 | Negativo | city no pertenece a province retorna 422 | RF-ORG-07 |
| TP-ORG-07-11 | Negativo | email invalido retorna 422 | RF-ORG-07 |
| TP-ORG-07-12 | Negativo | name vacio retorna 422 | RF-ORG-07 |
| TP-ORG-07-13 | Negativo | Canal invalido retorna 422 | RF-ORG-07 |
| TP-ORG-07-14 | Negativo | Entidad inexistente al editar retorna 404 | RF-ORG-07 |
| TP-ORG-07-15 | Negativo | Sin JWT retorna 401 | RF-ORG-07 |
| TP-ORG-07-16 | Negativo | Sin permiso retorna 403 | RF-ORG-07 |
| TP-ORG-07-17 | Seguridad | RLS garantiza tenant_id del JWT al crear | RF-ORG-07 |
| TP-ORG-07-18 | Transaccion | Fallo en entity_channels revierte entities (rollback) | RF-ORG-07 |
| TP-ORG-07-19 | Integracion | Audit log registra creacion y edicion | RF-ORG-07 |
| TP-ORG-07-20 | E2E | Click "Nueva Entidad" → formulario con selects en cascada → guardar | RF-ORG-07 |
| TP-ORG-07-21 | E2E | Seleccionar provincia → ciudades se filtran automaticamente | RF-ORG-07 |
| TP-ORG-07-22 | E2E | Editar entidad → checkboxes canales precargados → modificar → guardar | RF-ORG-07 |

### TP-ORG-08 — Ver detalle de entidad con sucursales y usuarios

| Test ID | Tipo | Descripcion | RF |
|---------|------|-------------|-----|
| TP-ORG-08-01 | Positivo | Detalle retorna todos los campos de la entidad | RF-ORG-08 |
| TP-ORG-08-02 | Positivo | Channels incluidos correctamente | RF-ORG-08 |
| TP-ORG-08-03 | Positivo | Branches incluidas con todos sus campos | RF-ORG-08 |
| TP-ORG-08-04 | Positivo | user_count retorna conteo correcto | RF-ORG-08 |
| TP-ORG-08-05 | Positivo | Entidad sin sucursales retorna branches vacio | RF-ORG-08 |
| TP-ORG-08-06 | Negativo | Entidad inexistente retorna 404 | RF-ORG-08 |
| TP-ORG-08-07 | Negativo | Sin permiso retorna 403 | RF-ORG-08 |
| TP-ORG-08-08 | Negativo | Sin JWT retorna 401 | RF-ORG-08 |
| TP-ORG-08-09 | Negativo | ID no UUID retorna 422 | RF-ORG-08 |
| TP-ORG-08-10 | Seguridad | RLS impide ver entidad de otro tenant | RF-ORG-08 |
| TP-ORG-08-11 | Integracion | Admin Entidad solo ve detalle de su entidad | RF-ORG-08 |
| TP-ORG-08-12 | E2E | Click en fila de grilla → ver detalle con sucursales | RF-ORG-08 |
| TP-ORG-08-13 | E2E | Detalle muestra boton editar/eliminar segun permisos | RF-ORG-08 |

### TP-ORG-09 — Eliminar/desactivar entidad con validacion de dependencias

| Test ID | Tipo | Descripcion | RF |
|---------|------|-------------|-----|
| TP-ORG-09-01 | Positivo | Eliminar entidad sin sucursales retorna 204 | RF-ORG-09 |
| TP-ORG-09-02 | Positivo | entity_channels eliminados junto con la entidad | RF-ORG-09 |
| TP-ORG-09-03 | Positivo | EntityDeletedEvent publicado | RF-ORG-09 |
| TP-ORG-09-04 | Negativo | Entidad con sucursales retorna 409 con branch_count | RF-ORG-09 |
| TP-ORG-09-05 | Negativo | Entidad inexistente retorna 404 | RF-ORG-09 |
| TP-ORG-09-06 | Negativo | Sin JWT retorna 401 | RF-ORG-09 |
| TP-ORG-09-07 | Negativo | Sin permiso retorna 403 | RF-ORG-09 |
| TP-ORG-09-08 | Negativo | ID no UUID retorna 422 | RF-ORG-09 |
| TP-ORG-09-09 | Seguridad | RLS impide eliminar entidad de otro tenant | RF-ORG-09 |
| TP-ORG-09-10 | Transaccion | DELETE entity_channels y DELETE entities atomico | RF-ORG-09 |
| TP-ORG-09-11 | Integracion | Audit log registra eliminacion | RF-ORG-09 |
| TP-ORG-09-12 | E2E | Click eliminar → confirmacion → entidad desaparece | RF-ORG-09 |
| TP-ORG-09-13 | E2E | Entidad con sucursales → dialogo informativo | RF-ORG-09 |

### TP-ORG-10 — CRUD de sucursales con selects en cascada

| Test ID | Tipo | Descripcion | RF |
|---------|------|-------------|-----|
| TP-ORG-10-01 | Positivo | Crear sucursal retorna 201 con todos los campos | RF-ORG-10 |
| TP-ORG-10-02 | Positivo | Editar sucursal retorna 200 con campos actualizados | RF-ORG-10 |
| TP-ORG-10-03 | Positivo | Eliminar sucursal retorna 204 | RF-ORG-10 |
| TP-ORG-10-04 | Positivo | BranchCreatedEvent publicado | RF-ORG-10 |
| TP-ORG-10-05 | Positivo | BranchUpdatedEvent publicado | RF-ORG-10 |
| TP-ORG-10-06 | Positivo | BranchDeletedEvent publicado | RF-ORG-10 |
| TP-ORG-10-07 | Negativo | Codigo duplicado en tenant retorna 409 | RF-ORG-10 |
| TP-ORG-10-08 | Negativo | Entidad inexistente retorna 404 | RF-ORG-10 |
| TP-ORG-10-09 | Negativo | Sucursal inexistente retorna 404 | RF-ORG-10 |
| TP-ORG-10-10 | Negativo | Sucursal no pertenece a la entidad retorna 404 | RF-ORG-10 |
| TP-ORG-10-11 | Negativo | city no pertenece a province retorna 422 | RF-ORG-10 |
| TP-ORG-10-12 | Negativo | name vacio retorna 422 | RF-ORG-10 |
| TP-ORG-10-13 | Negativo | Sin JWT retorna 401 | RF-ORG-10 |
| TP-ORG-10-14 | Negativo | Sin permiso retorna 403 | RF-ORG-10 |
| TP-ORG-10-15 | Seguridad | RLS impide operar sucursales de otro tenant | RF-ORG-10 |
| TP-ORG-10-16 | Seguridad | Sucursal hereda tenant_id de la entidad al crear | RF-ORG-10 |
| TP-ORG-10-17 | Integracion | Audit log registra creacion, edicion y eliminacion | RF-ORG-10 |
| TP-ORG-10-18 | Integracion | Eliminacion fisica verificada (SELECT post-DELETE = 0) | RF-ORG-10 |
| TP-ORG-10-19 | E2E | En detalle entidad → click "Agregar Sucursal" → formulario → guardar | RF-ORG-10 |
| TP-ORG-10-20 | E2E | Seleccionar provincia → ciudades se filtran | RF-ORG-10 |
| TP-ORG-10-21 | E2E | Editar sucursal → formulario precargado → modificar → guardar | RF-ORG-10 |
| TP-ORG-10-22 | E2E | Eliminar sucursal → confirmacion → desaparece de grilla | RF-ORG-10 |

---

## Changelog

### v1.1.0 (2026-03-15)
- 85 casos de prueba agregados para RF-ORG-06 a RF-ORG-10 (entidades y sucursales)
- Total: 135 tests (40 positivos, 53 negativos, 24 integracion, 18 E2E)
- 8 reglas de negocio nuevas validadas (RN-ORG-08 a RN-ORG-15)

### v1.0.0 (2026-03-15)
- 50 tests iniciales para RF-ORG-01 a RF-ORG-05 (FL-ORG-01)
- Cobertura de 7 reglas de negocio
