# TP-CFG — Plan de Pruebas: Configuracion

> **Proyecto:** Unazul Backoffice
> **Version:** 2.0.0
> **Fecha:** 2026-03-15
> **Modulo:** Config Service (SA.config)
> **Fuente:** [RF-CFG](../RF/RF-CFG.md)

---

## 1. Resumen de Cobertura

| RF | Titulo | Positivos | Negativos | Integracion | Seguridad | E2E | Total |
|----|--------|-----------|-----------|-------------|-----------|-----|-------|
| RF-CFG-01 | Listar grupos por categoria | 2 | 2 | 1 | 0 | 1 | 6 |
| RF-CFG-02 | Listar parametros de grupo | 3 | 3 | 1 | 0 | 1 | 8 |
| RF-CFG-03 | Crear parametro | 3 | 4 | 2 | 0 | 1 | 10 |*
| RF-CFG-04 | Editar parametro inline | 3 | 3 | 2 | 0 | 1 | 9 |
| RF-CFG-05 | Eliminar parametro | 2 | 2 | 2 | 0 | 1 | 7 |
| RF-CFG-06 | Gestionar grupos | 2 | 4 | 1 | 0 | 1 | 8 |
| RF-CFG-07 | Filtro jerarquico | 2 | 2 | 1 | 0 | 1 | 6 |
| RF-CFG-08 | Listar servicios externos | 3 | 2 | 1 | 1 | 1 | 8 |
| RF-CFG-09 | Crear servicio externo | 3 | 3 | 2 | 2 | 1 | 11 |*
| RF-CFG-10 | Editar servicio externo | 3 | 4 | 2 | 2 | 1 | 12 |
| RF-CFG-11 | Probar conexion | 3 | 4 | 2 | 1 | 1 | 11 |*
| RF-CFG-12 | Listar workflows | 3 | 4 | 2 | 0 | 1 | 10 |
| RF-CFG-13 | Crear/editar workflow | 4 | 5 | 3 | 0 | 1 | 13 |
| RF-CFG-14 | Configurar nodo por tipo | 5 | 3 | 2 | 0 | 1 | 11 |
| RF-CFG-15 | Panel atributos dominio | 4 | 2 | 1 | 0 | 1 | 8 |
| RF-CFG-16 | Gestionar transiciones | 3 | 3 | 2 | 0 | 1 | 9 |
| RF-CFG-17 | Activar workflow | 2 | 11 | 2 | 0 | 1 | 16 |
| RF-CFG-18 | Desactivar workflow | 1 | 4 | 2 | 0 | 1 | 8 |*
| **Total** | | **51** | **65** | **31** | **6** | **18** | **171** |

---

## 2. Casos de Prueba por RF

### TP-CFG-01 — Listar grupos de parametros por categoria

| Test ID | Tipo | Descripcion | RF |
|---------|------|-------------|-----|
| TP-CFG-01-01 | Positivo | Listar grupos retorna 4 categorias con 14+ grupos ordenados | RF-CFG-01 |
| TP-CFG-01-02 | Positivo | Grupos ordenados por sort_order dentro de cada categoria | RF-CFG-01 |
| TP-CFG-01-03 | Negativo | 403 sin permiso p_cfg_param_list | RF-CFG-01 |
| TP-CFG-01-04 | Negativo | 401 sin JWT | RF-CFG-01 |
| TP-CFG-01-05 | Integracion | Seed data de parameter_groups presente tras migracion | RF-CFG-01 |
| TP-CFG-01-06 | E2E | Navegacion a /parametros muestra sidebar con grupos colapsados por categoria | RF-CFG-01 |

### TP-CFG-02 — Listar parametros de un grupo

| Test ID | Tipo | Descripcion | RF |
|---------|------|-------------|-----|
| TP-CFG-02-01 | Positivo | Listar parametros de grupo existente retorna items con key, value, type | RF-CFG-02 |
| TP-CFG-02-02 | Positivo | Parametros tipo select incluyen options[] ordenadas | RF-CFG-02 |
| TP-CFG-02-03 | Positivo | Filtro parent_key retorna solo parametros hijos | RF-CFG-02 |
| TP-CFG-02-04 | Negativo | 404 para group_id inexistente | RF-CFG-02 |
| TP-CFG-02-05 | Negativo | 422 para group_id no UUID | RF-CFG-02 |
| TP-CFG-02-06 | Negativo | 403 sin permiso p_cfg_param_list | RF-CFG-02 |
| TP-CFG-02-07 | Integracion | RLS filtra parametros por tenant correctamente | RF-CFG-02 |
| TP-CFG-02-08 | E2E | Click en grupo "Provincias" en sidebar muestra tabla de parametros | RF-CFG-02 |

### TP-CFG-03 — Crear parametro

| Test ID | Tipo | Descripcion | RF |
|---------|------|-------------|-----|
| TP-CFG-03-01 | Positivo | Crear parametro tipo text retorna 201 | RF-CFG-03 |
| TP-CFG-03-02 | Positivo | Crear parametro tipo select con opciones persiste opciones | RF-CFG-03 |
| TP-CFG-03-03 | Positivo | Crear parametro con parent_key persiste relacion jerarquica | RF-CFG-03 |
| TP-CFG-03-04 | Negativo | 409 por key duplicada en mismo grupo y tenant | RF-CFG-03 |
| TP-CFG-03-05 | Negativo | 404 por group_id inexistente | RF-CFG-03 |
| TP-CFG-03-06 | Negativo | 422 por type select sin opciones | RF-CFG-03 |
| TP-CFG-03-07 | Negativo | 403 sin permiso p_cfg_param_create | RF-CFG-03 |
| TP-CFG-03-08 | Integracion | ParameterUpdatedEvent publicado y consumido por Audit | RF-CFG-03 |
| TP-CFG-03-09 | Integracion | RLS asegura que el parametro se crea con tenant_id del JWT | RF-CFG-03 |
| TP-CFG-03-10 | E2E | Click "Agregar Parametro" → formulario → submit → parametro visible en tabla | RF-CFG-03 |

### TP-CFG-04 — Editar parametro inline

| Test ID | Tipo | Descripcion | RF |
|---------|------|-------------|-----|
| TP-CFG-04-01 | Positivo | Editar value de parametro text retorna 200 | RF-CFG-04 |
| TP-CFG-04-02 | Positivo | Editar value actualiza updated_at y updated_by | RF-CFG-04 |
| TP-CFG-04-03 | Positivo | Editar opciones de parametro select reemplaza correctamente | RF-CFG-04 |
| TP-CFG-04-04 | Negativo | 404 para parametro inexistente | RF-CFG-04 |
| TP-CFG-04-05 | Negativo | 422 para value incoherente con tipo | RF-CFG-04 |
| TP-CFG-04-06 | Negativo | 403 sin permiso p_cfg_param_edit | RF-CFG-04 |
| TP-CFG-04-07 | Integracion | ParameterUpdatedEvent publicado y cache invalidado | RF-CFG-04 |
| TP-CFG-04-08 | Integracion | RLS impide editar parametro de otro tenant | RF-CFG-04 |
| TP-CFG-04-09 | E2E | Click en valor de parametro → edicion inline → guardar → valor actualizado | RF-CFG-04 |

### TP-CFG-05 — Eliminar parametro

| Test ID | Tipo | Descripcion | RF |
|---------|------|-------------|-----|
| TP-CFG-05-01 | Positivo | Eliminar parametro retorna 204 | RF-CFG-05 |
| TP-CFG-05-02 | Positivo | Eliminar parametro tipo select elimina opciones en cascada | RF-CFG-05 |
| TP-CFG-05-03 | Negativo | 404 para parametro inexistente | RF-CFG-05 |
| TP-CFG-05-04 | Negativo | 403 sin permiso p_cfg_param_delete | RF-CFG-05 |
| TP-CFG-05-05 | Integracion | ParameterUpdatedEvent publicado y consumido por Audit | RF-CFG-05 |
| TP-CFG-05-06 | Integracion | Servicios consumidores invalidan cache al recibir evento | RF-CFG-05 |
| TP-CFG-05-07 | E2E | Hover parametro → click eliminar → confirmar → desaparece de tabla | RF-CFG-05 |

### TP-CFG-06 — Gestionar grupos de parametros

| Test ID | Tipo | Descripcion | RF |
|---------|------|-------------|-----|
| TP-CFG-06-01 | Positivo | Crear grupo retorna 201 | RF-CFG-06 |
| TP-CFG-06-02 | Positivo | Eliminar grupo sin parametros retorna 204 | RF-CFG-06 |
| TP-CFG-06-03 | Negativo | 409 por code duplicado | RF-CFG-06 |
| TP-CFG-06-04 | Negativo | 409 al eliminar grupo con parametros | RF-CFG-06 |
| TP-CFG-06-05 | Negativo | 404 al eliminar grupo inexistente | RF-CFG-06 |
| TP-CFG-06-06 | Negativo | 403 sin permiso p_cfg_group_manage | RF-CFG-06 |
| TP-CFG-06-07 | Integracion | Grupo creado es visible para todos los tenants | RF-CFG-06 |
| TP-CFG-06-08 | E2E | Crear grupo → aparece en sidebar bajo la categoria correspondiente | RF-CFG-06 |

### TP-CFG-07 — Filtrar parametros por relacion jerarquica

| Test ID | Tipo | Descripcion | RF |
|---------|------|-------------|-----|
| TP-CFG-07-01 | Positivo | Filtro jerarquico retorna solo parametros hijos | RF-CFG-07 |
| TP-CFG-07-02 | Positivo | parent_key sin hijos retorna array vacio | RF-CFG-07 |
| TP-CFG-07-03 | Negativo | 404 para group_id inexistente | RF-CFG-07 |
| TP-CFG-07-04 | Negativo | 403 sin permiso | RF-CFG-07 |
| TP-CFG-07-05 | Integracion | RLS filtra por tenant en consulta jerarquica | RF-CFG-07 |
| TP-CFG-07-06 | E2E | Seleccionar provincia en formulario filtra ciudades automaticamente | RF-CFG-07 |

### TP-CFG-08 — Listar servicios externos

| Test ID | Tipo | Descripcion | RF |
|---------|------|-------------|-----|
| TP-CFG-08-01 | Positivo | Listar servicios retorna items sin credenciales | RF-CFG-08 |
| TP-CFG-08-02 | Positivo | Servicios incluyen last_tested_at y last_test_success | RF-CFG-08 |
| TP-CFG-08-03 | Positivo | Sin servicios retorna items: [] | RF-CFG-08 |
| TP-CFG-08-04 | Negativo | 403 sin permiso p_cfg_service_list | RF-CFG-08 |
| TP-CFG-08-05 | Negativo | 401 sin JWT | RF-CFG-08 |
| TP-CFG-08-06 | Integracion | RLS filtra servicios por tenant | RF-CFG-08 |
| TP-CFG-08-07 | Seguridad | Respuesta no contiene valores de service_auth_configs | RF-CFG-08 |
| TP-CFG-08-08 | E2E | Navegar a /servicios muestra tabla con servicios del tenant | RF-CFG-08 |

### TP-CFG-09 — Crear servicio externo con credenciales encriptadas

| Test ID | Tipo | Descripcion | RF |
|---------|------|-------------|-----|
| TP-CFG-09-01 | Positivo | Crear servicio REST con api_key retorna 201 | RF-CFG-09 |
| TP-CFG-09-02 | Positivo | Crear servicio con auth_type none sin auth_configs | RF-CFG-09 |
| TP-CFG-09-03 | Positivo | Crear servicio oauth2 con todas las keys requeridas | RF-CFG-09 |
| TP-CFG-09-04 | Negativo | 409 por nombre duplicado | RF-CFG-09 |
| TP-CFG-09-05 | Negativo | 422 por auth_configs incompletas | RF-CFG-09 |
| TP-CFG-09-06 | Negativo | 403 sin permiso p_cfg_service_create | RF-CFG-09 |
| TP-CFG-09-07 | Integracion | ServiceCreatedEvent publicado y consumido por Audit | RF-CFG-09 |
| TP-CFG-09-08 | Integracion | RLS asegura aislamiento por tenant | RF-CFG-09 |
| TP-CFG-09-09 | Seguridad | Credenciales almacenadas encriptadas con AES-256 | RF-CFG-09 |
| TP-CFG-09-10 | Seguridad | Respuesta no contiene credenciales | RF-CFG-09 |
| TP-CFG-09-11 | E2E | Click "Nuevo Servicio" → formulario → submit → servicio visible en tabla | RF-CFG-09 |

### TP-CFG-10 — Editar servicio externo

| Test ID | Tipo | Descripcion | RF |
|---------|------|-------------|-----|
| TP-CFG-10-01 | Positivo | Editar nombre de servicio retorna 200 | RF-CFG-10 |
| TP-CFG-10-02 | Positivo | Editar sin auth_configs mantiene credenciales existentes | RF-CFG-10 |
| TP-CFG-10-03 | Positivo | Cambiar auth_type reemplaza credenciales | RF-CFG-10 |
| TP-CFG-10-04 | Negativo | 404 para servicio inexistente | RF-CFG-10 |
| TP-CFG-10-05 | Negativo | 409 por nombre duplicado | RF-CFG-10 |
| TP-CFG-10-06 | Negativo | 422 por auth_configs incompletas | RF-CFG-10 |
| TP-CFG-10-07 | Negativo | 403 sin permiso p_cfg_service_edit | RF-CFG-10 |
| TP-CFG-10-08 | Integracion | ServiceUpdatedEvent publicado y consumido por Audit | RF-CFG-10 |
| TP-CFG-10-09 | Integracion | RLS impide editar servicio de otro tenant | RF-CFG-10 |
| TP-CFG-10-10 | Seguridad | Nuevas credenciales encriptadas con AES-256 | RF-CFG-10 |
| TP-CFG-10-11 | Seguridad | Respuesta no contiene credenciales | RF-CFG-10 |
| TP-CFG-10-12 | E2E | Editar servicio → cambiar nombre → guardar → actualizado en tabla | RF-CFG-10 |

### TP-CFG-11 — Probar conexion de servicio externo

| Test ID | Tipo | Descripcion | RF |
|---------|------|-------------|-----|
| TP-CFG-11-01 | Positivo | Prueba exitosa retorna success=true con response_time_ms | RF-CFG-11 |
| TP-CFG-11-02 | Positivo | Prueba exitosa actualiza last_tested_at y last_test_success | RF-CFG-11 |
| TP-CFG-11-03 | Positivo | Prueba exitosa restaura status a active si estaba en error | RF-CFG-11 |
| TP-CFG-11-04 | Negativo | Prueba fallida retorna success=false con error | RF-CFG-11 |
| TP-CFG-11-05 | Negativo | Timeout respeta timeout_ms configurado | RF-CFG-11 |
| TP-CFG-11-06 | Negativo | Prueba fallida cambia status a error | RF-CFG-11 |
| TP-CFG-11-07 | Negativo | 404 para servicio inexistente | RF-CFG-11 |
| TP-CFG-11-08 | Negativo | 403 sin permiso p_cfg_service_test | RF-CFG-11 |
| TP-CFG-11-09 | Seguridad | Logs de prueba no contienen credenciales desencriptadas | RF-CFG-11 |
| TP-CFG-11-10 | Integracion | ServiceTestedEvent publicado y consumido por Audit | RF-CFG-11 |
| TP-CFG-11-11 | Integracion | Desencriptacion AES-256 funciona correctamente | RF-CFG-11 |
| TP-CFG-11-12 | E2E | Click "Probar Conexion" → spinner → resultado visible en UI | RF-CFG-11 |

---

## 3. Casos de Prueba — FL-CFG-02 (Workflows)

### TP-CFG-12 — Listar workflows con estado y version

| Test ID | Tipo | Descripcion | RF |
|---------|------|-------------|-----|
| TP-CFG-12-01 | Positivo | Lista paginada con campos esperados (id, name, status, version, created_at) | RF-CFG-12 |
| TP-CFG-12-02 | Positivo | Busqueda ILIKE filtra por name y description | RF-CFG-12 |
| TP-CFG-12-03 | Positivo | Filtro por status retorna solo workflows del estado solicitado | RF-CFG-12 |
| TP-CFG-12-04 | Negativo | Sin permiso p_wf_list retorna 403 | RF-CFG-12 |
| TP-CFG-12-05 | Negativo | Sin JWT retorna 401 | RF-CFG-12 |
| TP-CFG-12-06 | Negativo | page_size > 100 retorna 400 | RF-CFG-12 |
| TP-CFG-12-07 | Negativo | status invalido retorna 400 | RF-CFG-12 |
| TP-CFG-12-08 | Integracion | RLS filtra workflows por tenant_id del JWT | RF-CFG-12 |
| TP-CFG-12-09 | Integracion | Ordenamiento por name/status/version/created_at funciona | RF-CFG-12 |
| TP-CFG-12-10 | E2E | Navegar a /workflows, ver grilla, buscar, filtrar | RF-CFG-12 |

### TP-CFG-13 — Crear/editar workflow con estados y transiciones

| Test ID | Tipo | Descripcion | RF |
|---------|------|-------------|-----|
| TP-CFG-13-01 | Positivo | Crear workflow draft con estados y transiciones | RF-CFG-13 |
| TP-CFG-13-02 | Positivo | Editar workflow draft actualiza estados y transiciones | RF-CFG-13 |
| TP-CFG-13-03 | Positivo | Editar workflow activo revierte a draft automaticamente | RF-CFG-13 |
| TP-CFG-13-04 | Positivo | GET /workflows/:id retorna workflow completo con states, configs, fields, transitions | RF-CFG-13 |
| TP-CFG-13-05 | Negativo | Name vacio retorna 422 | RF-CFG-13 |
| TP-CFG-13-06 | Negativo | States vacio retorna 422 | RF-CFG-13 |
| TP-CFG-13-07 | Negativo | Workflow inexistente retorna 404 (PUT/GET) | RF-CFG-13 |
| TP-CFG-13-08 | Negativo | Sin permiso p_wf_create/p_wf_edit retorna 403 | RF-CFG-13 |
| TP-CFG-13-09 | Negativo | Sin JWT retorna 401 | RF-CFG-13 |
| TP-CFG-13-10 | Integracion | WorkflowCreatedEvent se publica y Audit lo registra | RF-CFG-13 |
| TP-CFG-13-11 | Integracion | WorkflowUpdatedEvent se publica con reverted_from_active | RF-CFG-13 |
| TP-CFG-13-12 | Integracion | Replace strategy elimina states/transitions previas y re-inserta | RF-CFG-13 |
| TP-CFG-13-13 | E2E | Crear workflow en editor visual, guardar, ver en listado | RF-CFG-13 |

### TP-CFG-14 — Configurar nodo por tipo con persistencia key-value

| Test ID | Tipo | Descripcion | RF |
|---------|------|-------------|-----|
| TP-CFG-14-01 | Positivo | service_call guarda config con service_id, endpoint, method | RF-CFG-14 |
| TP-CFG-14-02 | Positivo | decision guarda config con condition conteniendo {{Objeto.atributo}} | RF-CFG-14 |
| TP-CFG-14-03 | Positivo | send_message guarda config con channel y template_id | RF-CFG-14 |
| TP-CFG-14-04 | Positivo | data_capture guarda fields con field_name, field_type, is_required, sort_order | RF-CFG-14 |
| TP-CFG-14-05 | Positivo | timer guarda config con timer_minutes | RF-CFG-14 |
| TP-CFG-14-06 | Negativo | service_call sin service_id retorna 422 | RF-CFG-14 |
| TP-CFG-14-07 | Negativo | data_capture con field_type invalido retorna 422 | RF-CFG-14 |
| TP-CFG-14-08 | Negativo | timer con timer_minutes <= 0 retorna 422 | RF-CFG-14 |
| TP-CFG-14-09 | Integracion | Configs se persisten como key-value en workflow_state_configs | RF-CFG-14 |
| TP-CFG-14-10 | Integracion | Fields de data_capture se persisten en workflow_state_fields con sort_order | RF-CFG-14 |
| TP-CFG-14-11 | E2E | Configurar nodo service_call, guardar, recargar, verificar config | RF-CFG-14 |

### TP-CFG-15 — Panel de atributos del dominio con insert en condiciones

| Test ID | Tipo | Descripcion | RF |
|---------|------|-------------|-----|
| TP-CFG-15-01 | Positivo | Panel muestra 11 objetos agrupados con atributos correctos | RF-CFG-15 |
| TP-CFG-15-02 | Positivo | Atributos muestran nombre, ruta y badge de tipo con color correcto | RF-CFG-15 |
| TP-CFG-15-03 | Positivo | Click en atributo con campo Condicion enfocado inserta {{Objeto.atributo}} | RF-CFG-15 |
| TP-CFG-15-04 | Positivo | Multiples inserciones en la misma condicion funcionan | RF-CFG-15 |
| TP-CFG-15-05 | Negativo | Click en atributo sin foco en campo no inserta nada | RF-CFG-15 |
| TP-CFG-15-06 | Negativo | Catalogo valida que tiene exactamente 11 objetos y ~68 atributos | RF-CFG-15 |
| TP-CFG-15-07 | Integracion | Catalogo completo con conteo de atributos por objeto | RF-CFG-15 |
| TP-CFG-15-08 | E2E | Abrir editor, abrir decision, insertar atributo, completar condicion, guardar | RF-CFG-15 |

### TP-CFG-16 — Gestionar transiciones con label, condicion y SLA

| Test ID | Tipo | Descripcion | RF |
|---------|------|-------------|-----|
| TP-CFG-16-01 | Positivo | Crear transicion drag-and-drop entre dos nodos | RF-CFG-16 |
| TP-CFG-16-02 | Positivo | Configurar label, condition y sla_hours | RF-CFG-16 |
| TP-CFG-16-03 | Positivo | Multiples transiciones desde nodo decision (bifurcacion) | RF-CFG-16 |
| TP-CFG-16-04 | Negativo | Transicion reflexiva (from = to) retorna 422 | RF-CFG-16 |
| TP-CFG-16-05 | Negativo | Transicion duplicada retorna 422 | RF-CFG-16 |
| TP-CFG-16-06 | Negativo | sla_hours <= 0 retorna 422 | RF-CFG-16 |
| TP-CFG-16-07 | Integracion | Transiciones se persisten con from/to IDs correctos | RF-CFG-16 |
| TP-CFG-16-08 | Integracion | Eliminar transicion y guardar verifica DELETE en DB | RF-CFG-16 |
| TP-CFG-16-09 | E2E | Conectar nodos, configurar transicion, guardar, recargar, verificar | RF-CFG-16 |

### TP-CFG-17 — Activar workflow con validacion de grafo

| Test ID | Tipo | Descripcion | RF |
|---------|------|-------------|-----|
| TP-CFG-17-01 | Positivo | Grafo valido se activa con version++ | RF-CFG-17 |
| TP-CFG-17-02 | Positivo | WorkflowPublishedEvent se publica | RF-CFG-17 |
| TP-CFG-17-03 | Negativo | Sin nodo start retorna 422 | RF-CFG-17 |
| TP-CFG-17-04 | Negativo | Multiples nodos start retorna 422 | RF-CFG-17 |
| TP-CFG-17-05 | Negativo | Sin nodo end retorna 422 | RF-CFG-17 |
| TP-CFG-17-06 | Negativo | Nodo huerfano retorna 422 | RF-CFG-17 |
| TP-CFG-17-07 | Negativo | Ciclo sin end retorna 422 | RF-CFG-17 |
| TP-CFG-17-08 | Negativo | Servicio externo inexistente/inactivo retorna 422 | RF-CFG-17 |
| TP-CFG-17-09 | Negativo | Plantilla inexistente retorna 422 | RF-CFG-17 |
| TP-CFG-17-10 | Negativo | data_capture sin fields retorna 422 | RF-CFG-17 |
| TP-CFG-17-11 | Negativo | Workflow ya activo retorna 409 | RF-CFG-17 |
| TP-CFG-17-12 | Negativo | Sin permiso retorna 403 | RF-CFG-17 |
| TP-CFG-17-13 | Negativo | Sin JWT retorna 401 | RF-CFG-17 |
| TP-CFG-17-14 | Integracion | Multiples errores en un solo response | RF-CFG-17 |
| TP-CFG-17-15 | Integracion | Version incrementa correctamente (0→1, 1→2) | RF-CFG-17 |
| TP-CFG-17-16 | E2E | Activar workflow desde UI, ver cambio de estado y version | RF-CFG-17 |

### TP-CFG-18 — Desactivar workflow

| Test ID | Tipo | Descripcion | RF |
|---------|------|-------------|-----|
| TP-CFG-18-01 | Positivo | Desactivacion exitosa de workflow activo | RF-CFG-18 |
| TP-CFG-18-02 | Negativo | Workflow draft retorna 409 | RF-CFG-18 |
| TP-CFG-18-03 | Negativo | Workflow inactive retorna 409 | RF-CFG-18 |
| TP-CFG-18-04 | Negativo | Workflow inexistente retorna 404 | RF-CFG-18 |
| TP-CFG-18-05 | Negativo | Sin permiso retorna 403 | RF-CFG-18 |
| TP-CFG-18-06 | Negativo | Sin JWT retorna 401 | RF-CFG-18 |
| TP-CFG-18-07 | Integracion | WorkflowDeactivatedEvent publicado y Audit registra | RF-CFG-18 |
| TP-CFG-18-08 | Integracion | Solicitudes en curso no afectadas | RF-CFG-18 |
| TP-CFG-18-09 | E2E | Desactivar desde UI, verificar estado en grilla | RF-CFG-18 |

---

## 4. Reglas de Negocio Validadas — Workflows

| RN | Regla | Tests que la cubren |
|----|-------|-------------------|
| RN-CFG-12 | Ciclo de estados workflow | TP-CFG-13-03, TP-CFG-17-01, TP-CFG-18-01 |
| RN-CFG-13 | Draft sin validacion de grafo | TP-CFG-13-01, TP-CFG-13-02 |
| RN-CFG-14 | Validacion de grafo | TP-CFG-17-03..07 |
| RN-CFG-15 | Version incrementa al activar | TP-CFG-17-15 |
| RN-CFG-16 | Validacion de referencias | TP-CFG-17-08, TP-CFG-17-09 |
| RN-CFG-17 | Solicitudes mantienen version | TP-CFG-18-08 |
| RN-CFG-18 | Catalogo atributos estatico | TP-CFG-15-01, TP-CFG-15-07 |
| RN-CFG-19 | Persistencia key-value por tipo | TP-CFG-14-01..05, TP-CFG-14-09, TP-CFG-14-10 |

---

## Changelog

### v2.0.0 (2026-03-15)
- TP-CFG-12 a TP-CFG-18 documentados (75 tests para workflows, FL-CFG-02)
- 8 reglas de negocio de workflows validadas (RN-CFG-12 a RN-CFG-19)
- Total: 171 tests

### v1.0.0 (2026-03-15)
- TP-CFG-01 a TP-CFG-11 documentados (96 tests totales)
- Cobertura: 29 positivos, 33 negativos, 17 integracion, 6 seguridad, 11 E2E
