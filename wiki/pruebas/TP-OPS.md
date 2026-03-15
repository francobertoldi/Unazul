# TP-OPS — Plan de Pruebas: Operaciones

> **Proyecto:** Unazul Backoffice
> **Version:** 3.0.0
> **Fecha:** 2026-03-15
> **Modulo:** Operations Service (SA.operations)
> **Fuente:** [RF-OPS](../RF/RF-OPS.md)
> **Flujos:** FL-OPS-01, FL-OPS-02, FL-OPS-03

---

## 1. Resumen de Cobertura

| RF | Titulo | Positivos | Negativos | Integracion | E2E | Total |
|----|--------|-----------|-----------|-------------|-----|-------|
| RF-OPS-01 | Listar solicitudes | 5 | 2 | 2 | 1 | 10 |
| RF-OPS-02 | Buscar solicitante | 2 | 3 | 1 | 1 | 7 |
| RF-OPS-03 | Crear solicitud | 4 | 6 | 3 | 1 | 14 |
| RF-OPS-04 | Editar solicitud draft | 3 | 4 | 1 | 1 | 9 |
| RF-OPS-05 | Transicion de estado | 5 | 5 | 2 | 1 | 13 |
| RF-OPS-06 | Detalle 8 solapas | 4 | 2 | 2 | 1 | 9 |
| RF-OPS-07 | Trazabilidad BPM | 4 | 2 | 1 | 1 | 8 |
| RF-OPS-08 | CRUD contactos/direcciones | 6 | 4 | 2 | 1 | 13 |
| RF-OPS-09 | CRUD beneficiarios | 3 | 4 | 1 | 1 | 9 |
| RF-OPS-10 | Gestionar documentos | 4 | 4 | 1 | 1 | 10 |
| RF-OPS-11 | Agregar observacion | 2 | 3 | 1 | 1 | 7 |
| RF-OPS-12 | Preliquidacion con filtros | 4 | 6 | 3 | 1 | 14 |
| RF-OPS-13 | Confirmacion liquidacion lock | 3 | 7 | 4 | 1 | 15 |
| RF-OPS-14 | Generacion Excel | 3 | 3 | 3 | 1 | 10 |
| RF-OPS-15 | Enviar mensaje solicitud | 5 | 7 | 2 | 1 | 15 |
| RF-OPS-16 | Historial liquidaciones | 4 | 4 | 2 | 1 | 11 |
| RF-OPS-17 | Detalle liquidacion | 3 | 4 | 2 | 1 | 10 |
| RF-OPS-18 | Descargar Excel | 2 | 6 | 2 | 1 | 11 |
| **Total** | | **66** | **76** | **35** | **18** | **195** |

---

## 2. Casos de Prueba por RF

### TP-OPS-01 — Listar solicitudes con filtros, busqueda y exportacion

| Test ID | Tipo | Descripcion | RF |
|---------|------|-------------|-----|
| TP-OPS-01-01 | Positivo | Listar solicitudes paginadas retorna items + total | RF-OPS-01 |
| TP-OPS-01-02 | Positivo | Filtro por status retorna solo solicitudes del estado | RF-OPS-01 |
| TP-OPS-01-03 | Positivo | Filtro por entity_id retorna solo solicitudes de la entidad | RF-OPS-01 |
| TP-OPS-01-04 | Positivo | Busqueda ILIKE por code, nombre, documento | RF-OPS-01 |
| TP-OPS-01-05 | Positivo | Exportacion Excel genera archivo y retorna URL | RF-OPS-01 |
| TP-OPS-01-06 | Negativo | 403 sin permiso p_ops_app_list | RF-OPS-01 |
| TP-OPS-01-07 | Negativo | 422 por status invalido | RF-OPS-01 |
| TP-OPS-01-08 | Integracion | RLS filtra por tenant correctamente | RF-OPS-01 |
| TP-OPS-01-09 | Integracion | JOIN con applicants funciona para search y display | RF-OPS-01 |
| TP-OPS-01-10 | E2E | Navegar a /solicitudes muestra tabla paginada con filtros | RF-OPS-01 |

### TP-OPS-02 — Buscar solicitante por documento

| Test ID | Tipo | Descripcion | RF |
|---------|------|-------------|-----|
| TP-OPS-02-01 | Positivo | Buscar solicitante existente retorna datos + contactos + direcciones | RF-OPS-02 |
| TP-OPS-02-02 | Positivo | application_count refleja numero correcto de solicitudes | RF-OPS-02 |
| TP-OPS-02-03 | Negativo | 404 para solicitante no encontrado | RF-OPS-02 |
| TP-OPS-02-04 | Negativo | 422 para doc_type invalido | RF-OPS-02 |
| TP-OPS-02-05 | Negativo | 403 sin permiso | RF-OPS-02 |
| TP-OPS-02-06 | Integracion | RLS filtra por tenant correctamente | RF-OPS-02 |
| TP-OPS-02-07 | E2E | Escribir DNI en formulario → buscar → datos precargados | RF-OPS-02 |

### TP-OPS-03 — Crear solicitud con selects en cascada y solicitante reutilizable

| Test ID | Tipo | Descripcion | RF |
|---------|------|-------------|-----|
| TP-OPS-03-01 | Positivo | Crear solicitud con solicitante nuevo retorna 201 con code | RF-OPS-03 |
| TP-OPS-03-02 | Positivo | Crear solicitud reutilizando solicitante existente (upsert) | RF-OPS-03 |
| TP-OPS-03-03 | Positivo | Crear con beneficiarios que suman 100% | RF-OPS-03 |
| TP-OPS-03-04 | Positivo | Crear con contactos y direcciones sincronizados | RF-OPS-03 |
| TP-OPS-03-05 | Negativo | 422 por producto inexistente o inactivo | RF-OPS-03 |
| TP-OPS-03-06 | Negativo | 422 por plan inexistente | RF-OPS-03 |
| TP-OPS-03-07 | Negativo | 503 si Catalog no disponible | RF-OPS-03 |
| TP-OPS-03-08 | Negativo | 422 por beneficiarios que no suman 100 | RF-OPS-03 |
| TP-OPS-03-09 | Negativo | 422 por campos requeridos vacios | RF-OPS-03 |
| TP-OPS-03-10 | Negativo | 403 sin permiso p_ops_app_create | RF-OPS-03 |
| TP-OPS-03-11 | Integracion | ApplicationCreatedEvent publicado y consumido por Audit | RF-OPS-03 |
| TP-OPS-03-12 | Integracion | trace_event creado con state=draft | RF-OPS-03 |
| TP-OPS-03-13 | Integracion | Transaccion atomica: fallo en beneficiarios revierte applicant | RF-OPS-03 |
| TP-OPS-03-14 | E2E | Formulario cascada entidad→producto→plan → submit → redireccion | RF-OPS-03 |

### TP-OPS-04 — Editar solicitud en estado draft

| Test ID | Tipo | Descripcion | RF |
|---------|------|-------------|-----|
| TP-OPS-04-01 | Positivo | Editar solicitud draft con nuevo producto retorna 200 | RF-OPS-04 |
| TP-OPS-04-02 | Positivo | Editar datos del solicitante actualiza applicants | RF-OPS-04 |
| TP-OPS-04-03 | Positivo | Editar beneficiarios reemplaza correctamente | RF-OPS-04 |
| TP-OPS-04-04 | Negativo | 422 para solicitud en status != draft | RF-OPS-04 |
| TP-OPS-04-05 | Negativo | 404 para solicitud inexistente | RF-OPS-04 |
| TP-OPS-04-06 | Negativo | 422 por producto inactivo | RF-OPS-04 |
| TP-OPS-04-07 | Negativo | 403 sin permiso p_ops_app_edit | RF-OPS-04 |
| TP-OPS-04-08 | Integracion | RLS impide editar solicitud de otro tenant | RF-OPS-04 |
| TP-OPS-04-09 | E2E | Abrir solicitud draft → editar → guardar → datos actualizados | RF-OPS-04 |

### TP-OPS-05 — Transicion de estado con state machine y trazabilidad

| Test ID | Tipo | Descripcion | RF |
|---------|------|-------------|-----|
| TP-OPS-05-01 | Positivo | Transicion draft→pending exitosa | RF-OPS-05 |
| TP-OPS-05-02 | Positivo | Transicion pending→in_review exitosa | RF-OPS-05 |
| TP-OPS-05-03 | Positivo | Transicion in_review→approved exitosa | RF-OPS-05 |
| TP-OPS-05-04 | Positivo | Transicion in_review→rejected con detail/reason | RF-OPS-05 |
| TP-OPS-05-05 | Positivo | Transicion draft/pending/in_review→cancelled | RF-OPS-05 |
| TP-OPS-05-06 | Negativo | 422 por transicion invalida (draft→approved) | RF-OPS-05 |
| TP-OPS-05-07 | Negativo | 422 por approved→settled (reservada FL-OPS-02) | RF-OPS-05 |
| TP-OPS-05-08 | Negativo | 409 por conflicto de concurrencia | RF-OPS-05 |
| TP-OPS-05-09 | Negativo | 404 para solicitud inexistente | RF-OPS-05 |
| TP-OPS-05-10 | Negativo | 403 sin permiso p_ops_app_transition | RF-OPS-05 |
| TP-OPS-05-11 | Integracion | ApplicationStatusChangedEvent publicado y consumido por Audit | RF-OPS-05 |
| TP-OPS-05-12 | Integracion | trace_event + trace_event_details creados correctamente | RF-OPS-05 |
| TP-OPS-05-13 | E2E | Click "Enviar a revision" → badge cambia → trace visible en timeline | RF-OPS-05 |

### TP-OPS-06 — Ver detalle de solicitud con 8 solapas

| Test ID | Tipo | Descripcion | RF |
|---------|------|-------------|-----|
| TP-OPS-06-01 | Positivo | Detalle completo retorna 8 secciones de datos | RF-OPS-06 |
| TP-OPS-06-02 | Positivo | Solicitante incluye application_count correcto | RF-OPS-06 |
| TP-OPS-06-03 | Positivo | Solicitud sin beneficiarios retorna array vacio | RF-OPS-06 |
| TP-OPS-06-04 | Positivo | Trace events incluyen details anidados | RF-OPS-06 |
| TP-OPS-06-05 | Negativo | 404 para solicitud inexistente | RF-OPS-06 |
| TP-OPS-06-06 | Negativo | 403 sin permiso p_ops_app_detail | RF-OPS-06 |
| TP-OPS-06-07 | Integracion | RLS filtra por tenant | RF-OPS-06 |
| TP-OPS-06-08 | Integracion | Observaciones ordenadas por created_at DESC | RF-OPS-06 |
| TP-OPS-06-09 | E2E | Click en solicitud del listado → pantalla detalle con 8 solapas | RF-OPS-06 |

### TP-OPS-07 — Trazabilidad BPM (timeline visual)

| Test ID | Tipo | Descripcion | RF |
|---------|------|-------------|-----|
| TP-OPS-07-01 | Positivo | Timeline con nodos completed, current y pending | RF-OPS-07 |
| TP-OPS-07-02 | Positivo | Timeline de solicitud aprobada muestra final_approved | RF-OPS-07 |
| TP-OPS-07-03 | Positivo | Timeline de solicitud rechazada muestra final_rejected | RF-OPS-07 |
| TP-OPS-07-04 | Positivo | Timeline sin workflow muestra solo state machine | RF-OPS-07 |
| TP-OPS-07-05 | Negativo | 404 para solicitud inexistente | RF-OPS-07 |
| TP-OPS-07-06 | Negativo | 403 sin permiso | RF-OPS-07 |
| TP-OPS-07-07 | Integracion | trace_events ordenados cronologicamente | RF-OPS-07 |
| TP-OPS-07-08 | E2E | Solapa trazabilidad muestra timeline visual con iconos | RF-OPS-07 |

### TP-OPS-08 — CRUD contactos y direcciones del solicitante

| Test ID | Tipo | Descripcion | RF |
|---------|------|-------------|-----|
| TP-OPS-08-01 | Positivo | Crear contacto retorna 201 | RF-OPS-08 |
| TP-OPS-08-02 | Positivo | Editar contacto retorna 200 | RF-OPS-08 |
| TP-OPS-08-03 | Positivo | Eliminar contacto retorna 204 | RF-OPS-08 |
| TP-OPS-08-04 | Positivo | Crear direccion con coordenadas retorna 201 | RF-OPS-08 |
| TP-OPS-08-05 | Positivo | Editar direccion retorna 200 | RF-OPS-08 |
| TP-OPS-08-06 | Positivo | Eliminar direccion retorna 204 | RF-OPS-08 |
| TP-OPS-08-07 | Negativo | 404 para solicitante inexistente | RF-OPS-08 |
| TP-OPS-08-08 | Negativo | 404 para contacto/direccion inexistente | RF-OPS-08 |
| TP-OPS-08-09 | Negativo | 422 por campos requeridos vacios | RF-OPS-08 |
| TP-OPS-08-10 | Negativo | 403 sin permiso p_ops_app_edit | RF-OPS-08 |
| TP-OPS-08-11 | Integracion | RLS filtra por tenant | RF-OPS-08 |
| TP-OPS-08-12 | Integracion | Contacto editado visible desde otra solicitud del mismo solicitante | RF-OPS-08 |
| TP-OPS-08-13 | E2E | Solapa contactos → agregar → editar → eliminar contacto | RF-OPS-08 |

### TP-OPS-09 — CRUD beneficiarios de solicitud

| Test ID | Tipo | Descripcion | RF |
|---------|------|-------------|-----|
| TP-OPS-09-01 | Positivo | Crear beneficiario retorna 201 | RF-OPS-09 |
| TP-OPS-09-02 | Positivo | Editar beneficiario retorna 200 | RF-OPS-09 |
| TP-OPS-09-03 | Positivo | Eliminar beneficiario retorna 204 | RF-OPS-09 |
| TP-OPS-09-04 | Negativo | 404 para solicitud inexistente | RF-OPS-09 |
| TP-OPS-09-05 | Negativo | 404 para beneficiario inexistente | RF-OPS-09 |
| TP-OPS-09-06 | Negativo | 422 por percentage fuera de rango | RF-OPS-09 |
| TP-OPS-09-07 | Negativo | 403 sin permiso | RF-OPS-09 |
| TP-OPS-09-08 | Integracion | RLS filtra por tenant | RF-OPS-09 |
| TP-OPS-09-09 | E2E | Solapa beneficiarios → agregar → editar → eliminar | RF-OPS-09 |

### TP-OPS-10 — Gestionar documentos de solicitud

| Test ID | Tipo | Descripcion | RF |
|---------|------|-------------|-----|
| TP-OPS-10-01 | Positivo | Subir documento PDF retorna 201 con file_url | RF-OPS-10 |
| TP-OPS-10-02 | Positivo | Aprobar documento cambia status | RF-OPS-10 |
| TP-OPS-10-03 | Positivo | Rechazar documento cambia status | RF-OPS-10 |
| TP-OPS-10-04 | Positivo | Eliminar documento borra registro y archivo | RF-OPS-10 |
| TP-OPS-10-05 | Negativo | 413 archivo excede 10MB | RF-OPS-10 |
| TP-OPS-10-06 | Negativo | 422 tipo de archivo no permitido | RF-OPS-10 |
| TP-OPS-10-07 | Negativo | 404 solicitud inexistente | RF-OPS-10 |
| TP-OPS-10-08 | Negativo | 403 sin permiso p_ops_doc_manage | RF-OPS-10 |
| TP-OPS-10-09 | Integracion | Archivo almacenado en ruta correcta por tenant | RF-OPS-10 |
| TP-OPS-10-10 | E2E | Solapa documentos → subir → badge pending → aprobar → badge approved | RF-OPS-10 |

### TP-OPS-11 — Agregar observacion a solicitud

| Test ID | Tipo | Descripcion | RF |
|---------|------|-------------|-----|
| TP-OPS-11-01 | Positivo | Agregar observacion retorna 201 con user_name | RF-OPS-11 |
| TP-OPS-11-02 | Positivo | Observacion permitida en cualquier estado | RF-OPS-11 |
| TP-OPS-11-03 | Negativo | 422 por contenido vacio | RF-OPS-11 |
| TP-OPS-11-04 | Negativo | 404 para solicitud inexistente | RF-OPS-11 |
| TP-OPS-11-05 | Negativo | 403 sin permiso p_ops_obs_create | RF-OPS-11 |
| TP-OPS-11-06 | Integracion | Observacion visible en detalle (RF-OPS-06) | RF-OPS-11 |
| TP-OPS-11-07 | E2E | Solapa observaciones → escribir texto → submit → aparece con avatar | RF-OPS-11 |

### TP-OPS-12 — Preliquidacion con filtros y calculo estimado

| Test ID | Tipo | Descripcion | RF |
|---------|------|-------------|-----|
| TP-OPS-12-01 | Positivo | Preview exitoso con solicitudes aprobadas y comisiones calculadas | RF-OPS-12 |
| TP-OPS-12-02 | Positivo | Preview con filtro entity_id retorna solo solicitudes de esa entidad | RF-OPS-12 |
| TP-OPS-12-03 | Positivo | Preview sin resultados retorna 200 con arrays vacios | RF-OPS-12 |
| TP-OPS-12-04 | Positivo | Preview con multiples monedas genera totals separados por currency | RF-OPS-12 |
| TP-OPS-12-05 | Negativo | 401 sin JWT | RF-OPS-12 |
| TP-OPS-12-06 | Negativo | 403 sin permiso p_ops_settlement_create | RF-OPS-12 |
| TP-OPS-12-07 | Negativo | 422 date_from > date_to (INVALID_DATE_RANGE) | RF-OPS-12 |
| TP-OPS-12-08 | Negativo | 422 date_from en futuro (FUTURE_DATE) | RF-OPS-12 |
| TP-OPS-12-09 | Negativo | 503 Catalog no disponible (timeout 5s) | RF-OPS-12 |
| TP-OPS-12-10 | Negativo | Solicitud sin commission_plan incluida con calculated_amount=0 | RF-OPS-12 |
| TP-OPS-12-11 | Integracion | Lookup batch a Catalog con product_ids y plan_ids unicos | RF-OPS-12 |
| TP-OPS-12-12 | Integracion | RLS filtra por tenant_id correctamente | RF-OPS-12 |
| TP-OPS-12-13 | Integracion | Calculo correcto para cada tipo de formula (fixed_per_sale, percentage_capital, percentage_total_loan) | RF-OPS-12 |
| TP-OPS-12-14 | E2E | Filtros fecha/entidad → preview → grilla con desglose y totales por moneda | RF-OPS-12 |

### TP-OPS-13 — Confirmacion de liquidacion con lock optimista

| Test ID | Tipo | Descripcion | RF |
|---------|------|-------------|-----|
| TP-OPS-13-01 | Positivo | Confirmacion exitosa con N solicitudes: settlement + items + totals creados | RF-OPS-13 |
| TP-OPS-13-02 | Positivo | Settlement con multiples monedas genera settlement_totals separados | RF-OPS-13 |
| TP-OPS-13-03 | Positivo | Excel generado y excel_url presente en respuesta 201 | RF-OPS-13 |
| TP-OPS-13-04 | Negativo | 409 conflicto: solicitud cambio de estado entre preview y confirm | RF-OPS-13 |
| TP-OPS-13-05 | Negativo | 409 conflicto parcial: rollback completo, ninguna solicitud liquidada | RF-OPS-13 |
| TP-OPS-13-06 | Negativo | 422 application_ids vacio (EMPTY_APPLICATION_IDS) | RF-OPS-13 |
| TP-OPS-13-07 | Negativo | 422 application_id no existe en tenant (APPLICATION_NOT_FOUND) | RF-OPS-13 |
| TP-OPS-13-08 | Negativo | 401 sin JWT | RF-OPS-13 |
| TP-OPS-13-09 | Negativo | 403 sin permiso p_ops_settlement_create | RF-OPS-13 |
| TP-OPS-13-10 | Negativo | 503 Catalog no disponible al calcular comisiones | RF-OPS-13 |
| TP-OPS-13-11 | Integracion | SELECT FOR UPDATE bloquea filas concurrentes correctamente | RF-OPS-13 |
| TP-OPS-13-12 | Integracion | Trace events creados por cada solicitud con settlement_id en details | RF-OPS-13 |
| TP-OPS-13-13 | Integracion | ApplicationStatusChangedEvent publicado por cada solicitud (approved→settled) | RF-OPS-13 |
| TP-OPS-13-14 | Integracion | CommissionsSettledEvent publicado con settlement_id y totales | RF-OPS-13 |
| TP-OPS-13-15 | E2E | Preview → confirmar dialogo → settlement creado → solicitudes en settled → redirect historial | RF-OPS-13 |

### TP-OPS-14 — Generacion de reporte Excel post-confirmacion

| Test ID | Tipo | Descripcion | RF |
|---------|------|-------------|-----|
| TP-OPS-14-01 | Positivo | Excel generado con estructura correcta (hoja Resumen + hoja Detalle) | RF-OPS-14 |
| TP-OPS-14-02 | Positivo | Ruta deterministica: {tenant_id}/settlements/{yyyy}/{MM}/{settlement_id}.xlsx | RF-OPS-14 |
| TP-OPS-14-03 | Positivo | Directorio creado automaticamente si no existe | RF-OPS-14 |
| TP-OPS-14-04 | Negativo | Filesystem no disponible: excel_url queda NULL, settlement intacto | RF-OPS-14 |
| TP-OPS-14-05 | Negativo | Permisos insuficientes en directorio: log error, excel_url NULL | RF-OPS-14 |
| TP-OPS-14-06 | Negativo | Settlement no encontrado (edge case): log error sin crash | RF-OPS-14 |
| TP-OPS-14-07 | Integracion | excel_url actualizado en settlements despues de generacion exitosa | RF-OPS-14 |
| TP-OPS-14-08 | Integracion | Contenido Excel coincide con settlement_items y settlement_totals en DB | RF-OPS-14 |
| TP-OPS-14-09 | Integracion | Archivo no se sobreescribe si ya existe (RN-OPS-25 inmutabilidad) | RF-OPS-14 |
| TP-OPS-14-10 | E2E | Confirm settlement → Excel generado → descarga exitosa del archivo .xlsx | RF-OPS-14 |

### TP-OPS-15 — Enviar mensaje desde solicitud con resolucion de variables de plantilla

| Test ID | Tipo | Descripcion | RF |
|---------|------|-------------|-----|
| TP-OPS-15-01 | Positivo | Enviar email con variables resueltas retorna 202 con status=queued | RF-OPS-15 |
| TP-OPS-15-02 | Positivo | Enviar SMS con body_override retorna 202 (sin resolucion de variables) | RF-OPS-15 |
| TP-OPS-15-03 | Positivo | Enviar WhatsApp con recipient manual retorna 202 | RF-OPS-15 |
| TP-OPS-15-04 | Positivo | Observacion auto-creada con observation_type=message y content descriptivo | RF-OPS-15 |
| TP-OPS-15-05 | Positivo | Variable sin dato (plan_name=null) se reemplaza por cadena vacia | RF-OPS-15 |
| TP-OPS-15-06 | Negativo | 404 para solicitud inexistente | RF-OPS-15 |
| TP-OPS-15-07 | Negativo | 422 para plantilla inexistente (OPS_TEMPLATE_NOT_FOUND) | RF-OPS-15 |
| TP-OPS-15-08 | Negativo | 422 para canal no coincide con plantilla (OPS_TEMPLATE_CHANNEL_MISMATCH) | RF-OPS-15 |
| TP-OPS-15-09 | Negativo | 422 para recipient con formato invalido | RF-OPS-15 |
| TP-OPS-15-10 | Negativo | 422 email sin subject en request ni en template (OPS_EMAIL_SUBJECT_REQUIRED) | RF-OPS-15 |
| TP-OPS-15-11 | Negativo | 503 cuando Config Service no disponible (timeout) | RF-OPS-15 |
| TP-OPS-15-12 | Negativo | 403 sin permiso p_ops_msg_send | RF-OPS-15 |
| TP-OPS-15-13 | Integracion | MessageSentEvent publicado con cuerpo resuelto y consumido por Notification | RF-OPS-15 |
| TP-OPS-15-14 | Integracion | Observacion visible en detalle solicitud (RF-OPS-06) con observation_type=message | RF-OPS-15 |
| TP-OPS-15-15 | E2E | Dialogo enviar mensaje → seleccionar canal y plantilla → enviar → confirmacion + observacion visible en solapa | RF-OPS-15 |

### TP-OPS-16 — Listar historial de liquidaciones con filtros

| Test ID | Tipo | Descripcion | RF |
|---------|------|-------------|-----|
| TP-OPS-16-01 | Positivo | Listar settlements exitoso con totales por moneda inline | RF-OPS-16 |
| TP-OPS-16-02 | Positivo | Filtro por rango de fechas retorna solo settlements en rango | RF-OPS-16 |
| TP-OPS-16-03 | Positivo | Filtro por settled_by retorna solo settlements del ejecutor | RF-OPS-16 |
| TP-OPS-16-04 | Positivo | Paginacion y ordenamiento por settled_at desc | RF-OPS-16 |
| TP-OPS-16-05 | Negativo | 401 sin JWT | RF-OPS-16 |
| TP-OPS-16-06 | Negativo | 403 sin permiso p_ops_settlement_list | RF-OPS-16 |
| TP-OPS-16-07 | Negativo | 422 date_from > date_to (INVALID_DATE_RANGE) | RF-OPS-16 |
| TP-OPS-16-08 | Negativo | Sin resultados retorna 200 con items=[] y total=0 | RF-OPS-16 |
| TP-OPS-16-09 | Integracion | RLS filtra por tenant_id correctamente | RF-OPS-16 |
| TP-OPS-16-10 | Integracion | JOIN settlement_totals retorna totales correctos por moneda | RF-OPS-16 |
| TP-OPS-16-11 | E2E | Navegar a /liquidaciones → grilla con fechas, ejecutor, totales, icono Excel | RF-OPS-16 |

### TP-OPS-17 — Ver detalle de liquidacion con items

| Test ID | Tipo | Descripcion | RF |
|---------|------|-------------|-----|
| TP-OPS-17-01 | Positivo | Detalle exitoso con settlement, totals e items completos | RF-OPS-17 |
| TP-OPS-17-02 | Positivo | Items incluyen datos denormalizados de solicitud via JOIN | RF-OPS-17 |
| TP-OPS-17-03 | Positivo | Settlement con multiples monedas muestra totals separados | RF-OPS-17 |
| TP-OPS-17-04 | Negativo | 404 settlement no encontrado (SETTLEMENT_NOT_FOUND) | RF-OPS-17 |
| TP-OPS-17-05 | Negativo | 404 settlement de otro tenant filtrado por RLS | RF-OPS-17 |
| TP-OPS-17-06 | Negativo | 401 sin JWT | RF-OPS-17 |
| TP-OPS-17-07 | Negativo | 403 sin permiso p_ops_settlement_list | RF-OPS-17 |
| TP-OPS-17-08 | Integracion | JOIN correcto entre settlements, settlement_totals, settlement_items y applications | RF-OPS-17 |
| TP-OPS-17-09 | Integracion | RLS filtra por tenant_id correctamente | RF-OPS-17 |
| TP-OPS-17-10 | E2E | Click en fila de historial → detalle con resumen y grilla de items con desglose | RF-OPS-17 |

### TP-OPS-18 — Descargar reporte Excel de liquidacion

| Test ID | Tipo | Descripcion | RF |
|---------|------|-------------|-----|
| TP-OPS-18-01 | Positivo | Descarga exitosa con Content-Type xlsx y Content-Disposition attachment | RF-OPS-18 |
| TP-OPS-18-02 | Positivo | Archivo descargado coincide con el generado originalmente (checksum) | RF-OPS-18 |
| TP-OPS-18-03 | Negativo | 404 settlement no encontrado (SETTLEMENT_NOT_FOUND) | RF-OPS-18 |
| TP-OPS-18-04 | Negativo | 404 excel_url es NULL (EXCEL_NOT_AVAILABLE) | RF-OPS-18 |
| TP-OPS-18-05 | Negativo | 404 archivo no existe en filesystem (EXCEL_FILE_MISSING) | RF-OPS-18 |
| TP-OPS-18-06 | Negativo | 404 settlement de otro tenant filtrado por RLS | RF-OPS-18 |
| TP-OPS-18-07 | Negativo | 401 sin JWT | RF-OPS-18 |
| TP-OPS-18-08 | Negativo | 403 sin permiso p_ops_settlement_list | RF-OPS-18 |
| TP-OPS-18-09 | Integracion | Lectura de archivo desde ruta deterministica en filesystem | RF-OPS-18 |
| TP-OPS-18-10 | Integracion | RLS filtra por tenant_id en settlements | RF-OPS-18 |
| TP-OPS-18-11 | E2E | Click icono descarga en historial → archivo .xlsx descargado en navegador | RF-OPS-18 |

---

## 3. Reglas de Negocio Validadas

| RN | Regla | Tests que la cubren |
|----|-------|-------------------|
| RN-OPS-01 | State machine fija | TP-OPS-05-01..05, TP-OPS-05-06, TP-OPS-05-07 |
| RN-OPS-03 | Solicitante reutilizable 1:N | TP-OPS-02-01, TP-OPS-03-01, TP-OPS-03-02 |
| RN-OPS-04 | Codigo autogenerado SOL-YYYY-NNN | TP-OPS-03-01 |
| RN-OPS-06 | Advertencia datos compartidos | TP-OPS-02-02 (application_count) |
| RN-OPS-07 | Edicion solo en draft | TP-OPS-04-04 |
| RN-OPS-08 | Validacion via Catalog | TP-OPS-03-05, TP-OPS-03-06, TP-OPS-03-07 |
| RN-OPS-09 | Trace event en toda transicion | TP-OPS-05-11, TP-OPS-05-12 |
| RN-OPS-10 | Beneficiarios suman 100% | TP-OPS-03-08 |
| RN-OPS-12 | Observaciones append-only | TP-OPS-11-01, TP-OPS-11-02 |
| RN-OPS-13 | Contactos pertenecen al solicitante | TP-OPS-08-12 |
| RN-OPS-14 | Concurrencia optimistic lock | TP-OPS-05-08 |
| RN-OPS-15 | Operations resuelve variables antes de publicar | TP-OPS-15-01, TP-OPS-15-05, TP-OPS-15-13 |
| RN-OPS-16 | Observacion automatica al enviar mensaje | TP-OPS-15-04, TP-OPS-15-14 |
| RN-OPS-17 | Envio siempre asincrono (202) | TP-OPS-15-01, TP-OPS-15-02, TP-OPS-15-03 |
| RN-OPS-18 | body_override anula resolucion | TP-OPS-15-02 |
| RN-OPS-19 | Lock optimista en confirmacion liquidacion | TP-OPS-13-04, TP-OPS-13-05, TP-OPS-13-11 |
| RN-OPS-20 | Settlement inmutable post-creacion | TP-OPS-13-01, TP-OPS-14-04 |
| RN-OPS-21 | Excel generacion best-effort | TP-OPS-14-04, TP-OPS-14-05 |
| RN-OPS-22 | Transicion approved→settled solo via liquidacion | TP-OPS-13-01, TP-OPS-13-12 |
| RN-OPS-23 | Calculo comision segun formula | TP-OPS-12-13, TP-OPS-13-01 |
| RN-OPS-24 | Totales agrupados por moneda | TP-OPS-12-04, TP-OPS-13-02 |
| RN-OPS-25 | Excel inmutable ruta deterministica | TP-OPS-14-02, TP-OPS-14-09, TP-OPS-18-09 |

---

## Changelog

### v3.0.0 (2026-03-15)
- TP-OPS-12 a TP-OPS-14, TP-OPS-16 a TP-OPS-18 agregados (FL-OPS-02: Liquidar Comisiones, 71 tests)
- 7 reglas de negocio nuevas validadas (RN-OPS-19 a RN-OPS-25)
- Total: 195 tests (66 positivos, 76 negativos, 35 integracion, 18 E2E)

### v2.0.0 (2026-03-15)
- TP-OPS-15 agregado (FL-OPS-03: Enviar Mensaje desde Solicitud, 15 tests)
- 4 reglas de negocio nuevas validadas (RN-OPS-15 a RN-OPS-18)
- Total: 124 tests (47 positivos, 46 negativos, 19 integracion, 12 E2E)

### v1.0.0 (2026-03-15)
- TP-OPS-01 a TP-OPS-11 documentados (109 tests totales)
- Cobertura: 42 positivos, 39 negativos, 17 integracion, 11 E2E
- 11 reglas de negocio validadas
