# TP-CAT — Plan de Pruebas: Catalogo de Productos

> **Proyecto:** Unazul Backoffice
> **Version:** 1.0.0
> **Fecha:** 2026-03-15
> **Modulo:** Catalog Service (SA.catalog)
> **Fuente:** [RF-CAT](../RF/RF-CAT.md)

---

## 1. Resumen de Cobertura

| RF | Titulo | Positivos | Negativos | Integracion | E2E | Total |
|----|--------|-----------|-----------|-------------|-----|-------|
| RF-CAT-01 | CRUD familias | 5 | 7 | 3 | 1 | 16 |
| RF-CAT-02 | Listar productos | 7 | 4 | 2 | 1 | 14 |
| RF-CAT-03 | Crear/editar producto | 3 | 8 | 4 | 1 | 16 |
| RF-CAT-04 | Ver detalle producto | 4 | 4 | 2 | 1 | 11 |
| RF-CAT-05 | CRUD planes con atributos | 7 | 7 | 3 | 2 | 19 |
| RF-CAT-06 | CRUD coberturas seguros | 4 | 5 | 2 | 1 | 12 |
| RF-CAT-07 | CRUD requisitos | 4 | 4 | 1 | 2 | 11 |
| RF-CAT-08 | CRUD comisiones | 5 | 7 | 2 | 1 | 15 |
| RF-CAT-09 | Deprecar/eliminar producto | 2 | 5 | 4 | 1 | 12 |
| **Total** | | **41** | **51** | **23** | **11** | **126** |

---

## 2. Reglas de Negocio Validadas

| ID | Regla | Tests que validan |
|----|-------|-------------------|
| RN-CAT-01 | Prefijo determina categoria | TP-CAT-01-02, TP-CAT-01-03, TP-CAT-01-06, TP-CAT-05-01..05 |
| RN-CAT-02 | Unicidad codigo familia por tenant | TP-CAT-01-07 |
| RN-CAT-03 | Familia sin productos → delete, con productos → 409 | TP-CAT-01-05, TP-CAT-01-08 |
| RN-CAT-04 | Estados de producto (draft, active, inactive, deprecated) | TP-CAT-02-03, TP-CAT-03-01, TP-CAT-09-02 |
| RN-CAT-05 | Producto sin planes → delete, con planes → deprecated | TP-CAT-09-01, TP-CAT-09-03 |
| RN-CAT-06 | Tabla atributos 1:1 por categoria | TP-CAT-05-01..07, TP-CAT-05-15 |
| RN-CAT-07 | Coberturas solo para insurance | TP-CAT-06-01, TP-CAT-06-06 |
| RN-CAT-08 | Unicidad codigo comision por tenant | TP-CAT-08-06 |
| RN-CAT-09 | Comision en uso → 409 | TP-CAT-08-07 |
| RN-CAT-10 | Validacion entity_id cross-service | TP-CAT-03-04, TP-CAT-03-13, TP-CAT-03-15 |
| RN-CAT-11 | Doble validacion prefijo-categoria | TP-CAT-05-08 |
| RN-CAT-12 | Deprecated es terminal | TP-CAT-09-04, TP-CAT-03-09 |
| RN-CAT-13 | Ownership comisiones en Catalog | TP-CAT-08-01..15 |
| RN-CAT-14 | Exportacion limitada 10,000 | TP-CAT-02-06 |
| RN-CAT-15 | Deprecated excluido por defecto | TP-CAT-02-07, TP-CAT-09-11 |
| RN-CAT-16 | Coberturas parametrizadas | TP-CAT-06-10 |
| RN-CAT-17 | RLS en todas las tablas | TP-CAT-01-15, TP-CAT-02-12 |

---

## 3. Casos de Prueba por RF

### TP-CAT-01 — CRUD de familias de productos con validacion de prefijo

| Test ID | Tipo | Descripcion | RF |
|---------|------|-------------|-----|
| TP-CAT-01-01 | Positivo | Listar familias paginado con product_count | RF-CAT-01 |
| TP-CAT-01-02 | Positivo | Crear familia con prefijo PREST valido | RF-CAT-01 |
| TP-CAT-01-03 | Positivo | Crear familia con prefijo SEG valido | RF-CAT-01 |
| TP-CAT-01-04 | Positivo | Editar description de familia existente | RF-CAT-01 |
| TP-CAT-01-05 | Positivo | Eliminar familia sin productos retorna 204 | RF-CAT-01 |
| TP-CAT-01-06 | Negativo | Prefijo no reconocido retorna 422 | RF-CAT-01 |
| TP-CAT-01-07 | Negativo | Codigo duplicado retorna 409 | RF-CAT-01 |
| TP-CAT-01-08 | Negativo | Eliminar familia con productos retorna 409 | RF-CAT-01 |
| TP-CAT-01-09 | Negativo | Code vacio retorna 422 | RF-CAT-01 |
| TP-CAT-01-10 | Negativo | 403 sin permiso p_cat_create | RF-CAT-01 |
| TP-CAT-01-11 | Negativo | 401 sin JWT | RF-CAT-01 |
| TP-CAT-01-12 | Negativo | 404 familia no encontrada en edicion | RF-CAT-01 |
| TP-CAT-01-13 | Integracion | ProductFamilyCreatedEvent publicado y registrado en audit | RF-CAT-01 |
| TP-CAT-01-14 | Integracion | ProductFamilyDeletedEvent publicado y registrado en audit | RF-CAT-01 |
| TP-CAT-01-15 | Integracion | RLS: familia de otro tenant no visible | RF-CAT-01 |
| TP-CAT-01-16 | E2E | Navegar a /familias-productos, crear, editar, eliminar | RF-CAT-01 |

### TP-CAT-02 — Listar productos con filtros, busqueda y exportacion

| Test ID | Tipo | Descripcion | RF |
|---------|------|-------------|-----|
| TP-CAT-02-01 | Positivo | Lista paginada con campos esperados y plan_count | RF-CAT-02 |
| TP-CAT-02-02 | Positivo | Busqueda ILIKE filtra por name y code | RF-CAT-02 |
| TP-CAT-02-03 | Positivo | Filtro por status retorna solo productos del estado | RF-CAT-02 |
| TP-CAT-02-04 | Positivo | Filtro por family_id retorna solo productos de esa familia | RF-CAT-02 |
| TP-CAT-02-05 | Positivo | Filtro por entity_id retorna productos de esa entidad | RF-CAT-02 |
| TP-CAT-02-06 | Positivo | Exportacion Excel genera archivo correcto | RF-CAT-02 |
| TP-CAT-02-07 | Positivo | Deprecated excluido por defecto sin filtro status | RF-CAT-02 |
| TP-CAT-02-08 | Negativo | Sin permiso p_cat_list retorna 403 | RF-CAT-02 |
| TP-CAT-02-09 | Negativo | Sin JWT retorna 401 | RF-CAT-02 |
| TP-CAT-02-10 | Negativo | page_size > 100 retorna 400 | RF-CAT-02 |
| TP-CAT-02-11 | Negativo | status invalido retorna 400 | RF-CAT-02 |
| TP-CAT-02-12 | Integracion | Admin Entidad solo ve productos de su entidad | RF-CAT-02 |
| TP-CAT-02-13 | Integracion | Ordenamiento por name/code/status/family_code/created_at funciona | RF-CAT-02 |
| TP-CAT-02-14 | E2E | Navegar a /productos, buscar, filtrar por familia y status, exportar | RF-CAT-02 |

### TP-CAT-03 — Crear/editar producto con entidad y familia

| Test ID | Tipo | Descripcion | RF |
|---------|------|-------------|-----|
| TP-CAT-03-01 | Positivo | Creacion exitosa con todos los campos validos | RF-CAT-03 |
| TP-CAT-03-02 | Positivo | Creacion con status = draft | RF-CAT-03 |
| TP-CAT-03-03 | Positivo | Edicion actualiza nombre, status y version | RF-CAT-03 |
| TP-CAT-03-04 | Negativo | entity_id inexistente retorna 422 | RF-CAT-03 |
| TP-CAT-03-05 | Negativo | family_id inexistente retorna 422 | RF-CAT-03 |
| TP-CAT-03-06 | Negativo | Campo name vacio retorna 422 | RF-CAT-03 |
| TP-CAT-03-07 | Negativo | valid_to < valid_from retorna 422 | RF-CAT-03 |
| TP-CAT-03-08 | Negativo | Admin Entidad con entidad ajena retorna 403 | RF-CAT-03 |
| TP-CAT-03-09 | Negativo | Editar producto deprecado retorna 422 | RF-CAT-03 |
| TP-CAT-03-10 | Negativo | Sin permiso p_cat_create retorna 403 | RF-CAT-03 |
| TP-CAT-03-11 | Negativo | Sin JWT retorna 401 | RF-CAT-03 |
| TP-CAT-03-12 | Integracion | ProductCreatedEvent publicado y registrado en audit | RF-CAT-03 |
| TP-CAT-03-13 | Integracion | entity_id validado sync contra Organization Service | RF-CAT-03 |
| TP-CAT-03-14 | Integracion | Admin Entidad solo crea para su entidad | RF-CAT-03 |
| TP-CAT-03-15 | Integracion | Organization Service caido retorna 503 | RF-CAT-03 |
| TP-CAT-03-16 | E2E | Crear producto: seleccionar entidad, familia, completar form, guardar | RF-CAT-03 |

### TP-CAT-04 — Ver detalle de producto con planes y requisitos

| Test ID | Tipo | Descripcion | RF |
|---------|------|-------------|-----|
| TP-CAT-04-01 | Positivo | Detalle con planes loan y requisitos | RF-CAT-04 |
| TP-CAT-04-02 | Positivo | Detalle con plan insurance incluye coberturas | RF-CAT-04 |
| TP-CAT-04-03 | Positivo | Detalle con plan card incluye card_attributes | RF-CAT-04 |
| TP-CAT-04-04 | Positivo | Producto sin planes ni requisitos retorna arrays vacios | RF-CAT-04 |
| TP-CAT-04-05 | Negativo | 404 producto no encontrado | RF-CAT-04 |
| TP-CAT-04-06 | Negativo | 403 sin permiso p_cat_detail | RF-CAT-04 |
| TP-CAT-04-07 | Negativo | 401 sin JWT | RF-CAT-04 |
| TP-CAT-04-08 | Negativo | Admin Entidad con producto de otra entidad retorna 403 | RF-CAT-04 |
| TP-CAT-04-09 | Integracion | Plans cargados con atributos por categoria correctos | RF-CAT-04 |
| TP-CAT-04-10 | Integracion | Commission plan name resuelto via JOIN | RF-CAT-04 |
| TP-CAT-04-11 | E2E | Navegar a detalle de producto, ver planes, requisitos, coberturas | RF-CAT-04 |

### TP-CAT-05 — CRUD de planes con atributos especificos por categoria

| Test ID | Tipo | Descripcion | RF |
|---------|------|-------------|-----|
| TP-CAT-05-01 | Positivo | Crear plan loan con atributos correctos | RF-CAT-05 |
| TP-CAT-05-02 | Positivo | Crear plan insurance con atributos y coberturas | RF-CAT-05 |
| TP-CAT-05-03 | Positivo | Crear plan account con atributos correctos | RF-CAT-05 |
| TP-CAT-05-04 | Positivo | Crear plan card con network y level validos | RF-CAT-05 |
| TP-CAT-05-05 | Positivo | Crear plan investment con risk_level valido | RF-CAT-05 |
| TP-CAT-05-06 | Positivo | Editar plan actualiza atributos 1:1 | RF-CAT-05 |
| TP-CAT-05-07 | Positivo | Eliminar plan elimina atributos y coberturas en cascada | RF-CAT-05 |
| TP-CAT-05-08 | Negativo | Atributos no corresponden a categoria retorna 422 | RF-CAT-05 |
| TP-CAT-05-09 | Negativo | Producto deprecated bloquea creacion 422 | RF-CAT-05 |
| TP-CAT-05-10 | Negativo | commission_plan_id inexistente retorna 422 | RF-CAT-05 |
| TP-CAT-05-11 | Negativo | Campos requeridos faltantes retorna 422 | RF-CAT-05 |
| TP-CAT-05-12 | Negativo | Plan no encontrado retorna 404 | RF-CAT-05 |
| TP-CAT-05-13 | Negativo | 403 sin permiso p_cat_edit | RF-CAT-05 |
| TP-CAT-05-14 | Negativo | Admin Entidad producto ajeno retorna 403 | RF-CAT-05 |
| TP-CAT-05-15 | Integracion | Tabla 1:1 creada correctamente segun categoria | RF-CAT-05 |
| TP-CAT-05-16 | Integracion | card_levels validado contra Config Service | RF-CAT-05 |
| TP-CAT-05-17 | Integracion | Coberturas batch creadas con plan insurance | RF-CAT-05 |
| TP-CAT-05-18 | E2E | Abrir PlanDialog, completar campos por categoria, guardar | RF-CAT-05 |
| TP-CAT-05-19 | E2E | Editar plan existente, eliminar plan | RF-CAT-05 |

### TP-CAT-06 — CRUD de coberturas de seguros inline en plan

| Test ID | Tipo | Descripcion | RF |
|---------|------|-------------|-----|
| TP-CAT-06-01 | Positivo | Agregar cobertura a plan insurance exitosamente | RF-CAT-06 |
| TP-CAT-06-02 | Positivo | Editar sum_insured y premium de cobertura | RF-CAT-06 |
| TP-CAT-06-03 | Positivo | Eliminar cobertura retorna 204 | RF-CAT-06 |
| TP-CAT-06-04 | Positivo | Select filtra coberturas ya agregadas | RF-CAT-06 |
| TP-CAT-06-05 | Negativo | Cobertura duplicada retorna 422 | RF-CAT-06 |
| TP-CAT-06-06 | Negativo | Producto no-insurance retorna 422 | RF-CAT-06 |
| TP-CAT-06-07 | Negativo | sum_insured <= 0 retorna 422 | RF-CAT-06 |
| TP-CAT-06-08 | Negativo | Plan no encontrado retorna 404 | RF-CAT-06 |
| TP-CAT-06-09 | Negativo | 403 sin permiso | RF-CAT-06 |
| TP-CAT-06-10 | Integracion | insurance_coverages cargadas desde Config Service | RF-CAT-06 |
| TP-CAT-06-11 | Integracion | Cobertura eliminada vuelve al select | RF-CAT-06 |
| TP-CAT-06-12 | E2E | Flujo completo: agregar, editar montos, eliminar cobertura | RF-CAT-06 |

### TP-CAT-07 — CRUD de requisitos documentales por producto

| Test ID | Tipo | Descripcion | RF |
|---------|------|-------------|-----|
| TP-CAT-07-01 | Positivo | Agregar requisito tipo document exitosamente | RF-CAT-07 |
| TP-CAT-07-02 | Positivo | Agregar requisito tipo data exitosamente | RF-CAT-07 |
| TP-CAT-07-03 | Positivo | Editar requisito actualiza campos | RF-CAT-07 |
| TP-CAT-07-04 | Positivo | Eliminar requisito retorna 204 | RF-CAT-07 |
| TP-CAT-07-05 | Negativo | name vacio retorna 422 | RF-CAT-07 |
| TP-CAT-07-06 | Negativo | type invalido retorna 422 | RF-CAT-07 |
| TP-CAT-07-07 | Negativo | Producto deprecado bloquea CRUD 422 | RF-CAT-07 |
| TP-CAT-07-08 | Negativo | Requisito no encontrado retorna 404 | RF-CAT-07 |
| TP-CAT-07-09 | Integracion | Requisitos persisten con product_id y tenant_id correctos | RF-CAT-07 |
| TP-CAT-07-10 | E2E | Gestionar requisitos en grilla inline del formulario | RF-CAT-07 |
| TP-CAT-07-11 | E2E | Agregar requisito validation, editar, eliminar | RF-CAT-07 |

### TP-CAT-08 — CRUD de planes de comisiones con validacion de uso

| Test ID | Tipo | Descripcion | RF |
|---------|------|-------------|-----|
| TP-CAT-08-01 | Positivo | Listar planes de comision con assigned_plan_count | RF-CAT-08 |
| TP-CAT-08-02 | Positivo | Crear plan tipo fixed_per_sale | RF-CAT-08 |
| TP-CAT-08-03 | Positivo | Crear plan tipo percentage_capital con max_amount | RF-CAT-08 |
| TP-CAT-08-04 | Positivo | Editar plan de comision existente | RF-CAT-08 |
| TP-CAT-08-05 | Positivo | Eliminar plan sin asignaciones retorna 204 | RF-CAT-08 |
| TP-CAT-08-06 | Negativo | Codigo duplicado retorna 409 | RF-CAT-08 |
| TP-CAT-08-07 | Negativo | Eliminar plan asignado retorna 409 | RF-CAT-08 |
| TP-CAT-08-08 | Negativo | code vacio retorna 422 | RF-CAT-08 |
| TP-CAT-08-09 | Negativo | type invalido retorna 422 | RF-CAT-08 |
| TP-CAT-08-10 | Negativo | value <= 0 retorna 422 | RF-CAT-08 |
| TP-CAT-08-11 | Negativo | 403 sin permiso | RF-CAT-08 |
| TP-CAT-08-12 | Negativo | 401 sin JWT | RF-CAT-08 |
| TP-CAT-08-13 | Integracion | CommissionPlanCreatedEvent publicado y registrado en audit | RF-CAT-08 |
| TP-CAT-08-14 | Integracion | assigned_plan_count calculado correctamente via LEFT JOIN | RF-CAT-08 |
| TP-CAT-08-15 | E2E | Flujo completo: listar, crear, editar, eliminar comision | RF-CAT-08 |

### TP-CAT-09 — Deprecar o eliminar producto segun dependencias

| Test ID | Tipo | Descripcion | RF |
|---------|------|-------------|-----|
| TP-CAT-09-01 | Positivo | Eliminar producto sin planes retorna 204 | RF-CAT-09 |
| TP-CAT-09-02 | Positivo | Deprecar producto con planes retorna 200 | RF-CAT-09 |
| TP-CAT-09-03 | Negativo | DELETE producto con planes retorna 409 | RF-CAT-09 |
| TP-CAT-09-04 | Negativo | Deprecar producto ya deprecated retorna 422 | RF-CAT-09 |
| TP-CAT-09-05 | Negativo | 404 producto no encontrado | RF-CAT-09 |
| TP-CAT-09-06 | Negativo | 403 sin permiso p_cat_delete | RF-CAT-09 |
| TP-CAT-09-07 | Negativo | Admin Entidad producto ajeno retorna 403 | RF-CAT-09 |
| TP-CAT-09-08 | Integracion | ProductDeletedEvent publicado y registrado en audit | RF-CAT-09 |
| TP-CAT-09-09 | Integracion | ProductDeprecatedEvent publicado y registrado en audit | RF-CAT-09 |
| TP-CAT-09-10 | Integracion | Requisitos eliminados en cascada con producto | RF-CAT-09 |
| TP-CAT-09-11 | Integracion | Deprecated excluido de listado por defecto | RF-CAT-09 |
| TP-CAT-09-12 | E2E | Flujo: click eliminar, 409, deprecar, verificar no aparece en listado | RF-CAT-09 |

---

## Changelog

### v1.0.0 (2026-03-15)
- TP-CAT-01 a TP-CAT-09 documentados (126 tests)
- Cobertura completa de RF-CAT-01 a RF-CAT-09
- 17 reglas de negocio validadas
