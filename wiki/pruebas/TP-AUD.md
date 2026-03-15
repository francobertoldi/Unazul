# TP-AUD — Plan de Pruebas del Modulo Auditoria

> **Proyecto:** Unazul Backoffice
> **Modulo:** Auditoria (SA.audit)
> **Version:** 1.0.0
> **Fecha:** 2026-03-15
> **RF cubiertos:** RF-AUD-01 a RF-AUD-03
> **Flujos:** FL-AUD-01

---

## 1. Resumen

| RF | Titulo | Positivos | Negativos | Integracion | E2E | Total |
|----|--------|-----------|-----------|-------------|-----|-------|
| RF-AUD-01 | Consultar log con filtros y paginacion | 8 | 7 | 4 | 2 | 21 |
| RF-AUD-02 | Exportar log a Excel/CSV | 4 | 5 | 3 | 2 | 14 |
| RF-AUD-03 | Ingesta asincrona de eventos de dominio | 4 | 4 | 5 | 2 | 15 |
| **Total** | | **16** | **16** | **12** | **6** | **50** |

---

## 2. Casos de Prueba — FL-AUD-01

### RF-AUD-01: Consultar log de auditoria con filtros y paginacion

#### Positivos

| TP | Escenario | Entrada | Resultado esperado |
|----|-----------|---------|-------------------|
| TP-AUD-01 | Consulta exitosa sin filtros | GET /audit-log?page=1&size=20 | 200, items[] con todos los registros del tenant, orden occurred_at DESC |
| TP-AUD-02 | Filtro por user_id | GET /audit-log?user_id={uuid} | 200, todos los items con user_id coincidente |
| TP-AUD-03 | Filtro por operation | GET /audit-log?operation=Crear | 200, todos los items con operation="Crear" |
| TP-AUD-04 | Filtro por module | GET /audit-log?module=solicitudes | 200, todos los items con module="solicitudes" |
| TP-AUD-05 | Filtro por rango de fechas | GET /audit-log?from=2026-01-01T00:00:00Z&to=2026-01-31T23:59:59Z | 200, items con occurred_at dentro del rango |
| TP-AUD-06 | Combinacion de filtros (AND) | GET /audit-log?operation=Editar&module=productos&from=...&to=... | 200, items cumplen todos los filtros simultaneamente |
| TP-AUD-07 | Resultado vacio | GET /audit-log?module=modulo_inexistente | 200, items=[], total=0 |
| TP-AUD-08 | Sort por module ASC | GET /audit-log?sort=module&order=asc | 200, items ordenados por module ascendente |

#### Negativos

| TP | Escenario | Entrada | Resultado esperado |
|----|-----------|---------|-------------------|
| TP-AUD-09 | 401 sin JWT | GET /audit-log sin Authorization header | 401 Unauthorized |
| TP-AUD-10 | 403 sin permiso audit:read | GET /audit-log con usuario sin permiso | 403 Forbidden |
| TP-AUD-11 | 422 rango de fechas invertido | GET /audit-log?from=2026-12-31&to=2026-01-01 | 422, "from must be <= to" |
| TP-AUD-12 | 422 operation invalida | GET /audit-log?operation=INVALIDA | 422, "Invalid operation value" |
| TP-AUD-13 | 422 page < 1 | GET /audit-log?page=0 | 422, "page >= 1" |
| TP-AUD-14 | 422 size > 100 | GET /audit-log?size=101 | 422, "1 <= size <= 100" |
| TP-AUD-15 | 422 size < 1 | GET /audit-log?size=0 | 422, "1 <= size <= 100" |

#### Integracion

| TP | Escenario | Verificacion |
|----|-----------|-------------|
| TP-AUD-16 | Paginacion con offset correcto | page=2, size=20 con 25 registros → items.length=5, total=25 |
| TP-AUD-17 | Partition pruning por rango de fechas | EXPLAIN muestra solo particiones del rango consultado |
| TP-AUD-18 | Indice tenant_id+module+occurred_at | EXPLAIN muestra Index Scan al filtrar por module |
| TP-AUD-19 | Aislamiento multi-tenant | Tenant A no ve registros de Tenant B |

#### E2E

| TP | Escenario | Flujo |
|----|-----------|-------|
| TP-AUD-20 | Consulta completa desde UI | Login → /auditoria → DataTable muestra registros con columnas correctas |
| TP-AUD-21 | Filtros desde UI | /auditoria → aplicar filtros usuario+operacion+modulo+fechas → resultados filtrados |

---

### RF-AUD-02: Exportar log de auditoria a Excel/CSV

#### Positivos

| TP | Escenario | Entrada | Resultado esperado |
|----|-----------|---------|-------------------|
| TP-AUD-22 | Exportar a Excel sin filtros | GET /audit-log/export?format=xlsx | 200, Content-Type xlsx, archivo con N filas + encabezados |
| TP-AUD-23 | Exportar a CSV con filtro por module | GET /audit-log/export?format=csv&module=solicitudes | 200, Content-Type csv, todas las filas con module=solicitudes |
| TP-AUD-24 | Exportar resultado vacio | GET /audit-log/export?format=xlsx&module=inexistente | 200, archivo con solo fila de encabezados |
| TP-AUD-25 | Exportar exactamente 10,000 registros | GET /audit-log/export?format=xlsx con 10000 registros | 200, archivo con 10000 filas de datos |

#### Negativos

| TP | Escenario | Entrada | Resultado esperado |
|----|-----------|---------|-------------------|
| TP-AUD-26 | 401 sin JWT | GET /audit-log/export sin Authorization | 401 Unauthorized |
| TP-AUD-27 | 403 sin permiso audit:export | GET /audit-log/export con usuario sin permiso | 403 Forbidden |
| TP-AUD-28 | 422 formato invalido | GET /audit-log/export?format=pdf | 422, "format must be xlsx or csv" |
| TP-AUD-29 | 422 rango de fechas invertido | GET /audit-log/export?format=xlsx&from=2026-12-31&to=2026-01-01 | 422, "from must be <= to" |
| TP-AUD-30 | 422 excede limite 10,000 registros | GET /audit-log/export?format=xlsx con 10001 registros | 422, "Result exceeds 10000 records" |

#### Integracion

| TP | Escenario | Verificacion |
|----|-----------|-------------|
| TP-AUD-31 | Archivo xlsx tiene columnas correctas y aislamiento tenant | Abrir archivo, verificar 9 columnas, datos solo del tenant solicitante |
| TP-AUD-32 | Archivo csv encoding UTF-8 BOM con separador coma | Verificar BOM bytes, separador coma, primera fila encabezados |
| TP-AUD-33 | Exportacion genera evento de auditoria | Exportar → verificar nuevo registro en audit_log con operation=Exportar, module=auditoria |

#### E2E

| TP | Escenario | Flujo |
|----|-----------|-------|
| TP-AUD-34 | Exportar Excel desde UI | Login → /auditoria → click "Exportar Excel" → descarga archivo .xlsx |
| TP-AUD-35 | Exportar CSV con filtros desde UI | /auditoria → aplicar filtros → click "Exportar CSV" → descarga .csv con datos filtrados |

---

### RF-AUD-03: Ingesta asincrona de eventos de dominio

#### Positivos

| TP | Escenario | Entrada | Resultado esperado |
|----|-----------|---------|-------------------|
| TP-AUD-36 | Ingesta exitosa con todos los campos | DomainEvent completo | Registro insertado en audit_log, documento en ES, ack en RabbitMQ |
| TP-AUD-37 | Ingesta con campos opcionales nulos | DomainEvent sin entity_type, entity_id, detail | Registro con nulls en campos opcionales, operacion exitosa |
| TP-AUD-38 | Ingesta con ES no disponible | DomainEvent valido, ES caido | INSERT en PG OK, log warning, ack (no DLQ) |
| TP-AUD-39 | Redelivery genera registro separado | Mismo evento entregado 2 veces | 2 registros con id diferentes, ambos validos (duplicado aceptable) |

#### Negativos

| TP | Escenario | Entrada | Resultado esperado |
|----|-----------|---------|-------------------|
| TP-AUD-40 | Evento con tenant_id nulo | DomainEvent sin tenant_id | Rechazado, retry, tras 3 intentos → DLQ |
| TP-AUD-41 | Evento con campos requeridos faltantes | DomainEvent sin user_id | Rechazado, retry, tras 3 intentos → DLQ |
| TP-AUD-42 | Evento con operation invalida | DomainEvent con operation="FALSA" | Rechazado, retry, tras 3 intentos → DLQ |
| TP-AUD-43 | Evento con occurred_at futuro (>5 min) | DomainEvent con occurred_at=now+10min | Rechazado por clock skew |

#### Integracion

| TP | Escenario | Verificacion |
|----|-----------|-------------|
| TP-AUD-44 | Retry 3 veces con backoff exponencial | Simular fallo PG, verificar 3 intentos a 5s/15s/45s |
| TP-AUD-45 | DLQ tras 3 reintentos fallidos | Simular fallo PG persistente, verificar mensaje en DLQ |
| TP-AUD-46 | Registro en particion mensual correcta | Insertar evento con occurred_at=2026-03-15, verificar particion audit_log_2026_03 |
| TP-AUD-47 | Documento indexado en ES | Insertar evento, verificar documento en indice audit-2026.03.15 |
| TP-AUD-48 | Competing consumers sin duplicados | 2 consumers activos, 100 eventos, verificar 100 registros (no 200) |

#### E2E

| TP | Escenario | Flujo |
|----|-----------|-------|
| TP-AUD-49 | Crear usuario genera registro de auditoria | Crear usuario en Identity → esperar → consultar /audit-log → registro con operation=Crear, module=identity |
| TP-AUD-50 | Editar producto genera registro con entity | Editar producto en Catalog → esperar → consultar /audit-log → registro con entity_type=product, entity_id={uuid} |

---

## 3. Reglas de Negocio Validadas

| RN | Descripcion | Tests que validan |
|----|-------------|-------------------|
| RN-AUD-01 | audit_log INSERT-only, sin UPDATE/DELETE | TP-AUD-20 (UI sin botones editar/eliminar) |
| RN-AUD-02 | Filtro por tenant en query, no RLS | TP-AUD-19 (aislamiento multi-tenant) |
| RN-AUD-03 | tenant_id sync desde JWT | TP-AUD-01, TP-AUD-09 |
| RN-AUD-04 | tenant_id async desde payload evento | TP-AUD-36, TP-AUD-40 |
| RN-AUD-05 | Particionamiento mensual | TP-AUD-17, TP-AUD-46 |
| RN-AUD-06 | Dual write PG+ES, PG fuente de verdad | TP-AUD-38, TP-AUD-47 |
| RN-AUD-07 | Retry 3x exponencial + DLQ | TP-AUD-44, TP-AUD-45 |
| RN-AUD-08 | Exportacion limitada a 10,000 | TP-AUD-25, TP-AUD-30 |
| RN-AUD-09 | user_name denormalizado | TP-AUD-36 |
| RN-AUD-10 | Enum audit_operation_type | TP-AUD-03, TP-AUD-12, TP-AUD-42 |

---

## Changelog

### v1.0.0 (2026-03-15)
- RF-AUD-01 a RF-AUD-03: 50 tests totales
- 16 positivos + 16 negativos + 12 integracion + 6 E2E
- 10 reglas de negocio validadas
