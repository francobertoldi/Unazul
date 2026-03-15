# TP-SEC — Plan de Pruebas: Seguridad e Identidad

> **Proyecto:** Unazul Backoffice
> **Version:** 1.0.0
> **Fecha:** 2026-03-15
> **Modulo:** Identity Service (SA.identity)
> **Fuente:** [RF-SEC](../RF/RF-SEC.md)

---

## 1. Resumen de Cobertura

| RF | Titulo | Positivos | Negativos | Integracion | E2E | Total |
|----|--------|-----------|-----------|-------------|-----|-------|
| RF-SEC-01 | Login con credenciales | 2 | 6 | 3 | 2 | 13 |
| RF-SEC-02 | Verificacion OTP | 2 | 4 | 2 | 2 | 10 |
| RF-SEC-03 | Refresh token rotacion | 2 | 4 | 2 | 2 | 10 |
| RF-SEC-04 | Recuperacion contrasena | 2 | 5 | 3 | 2 | 12 |
| RF-SEC-05 | Logout con revocacion | 2 | 4 | 2 | 2 | 10 |
| RF-SEC-06 | Listar usuarios | 3 | 4 | 3 | 2 | 12 |
| RF-SEC-07 | Crear/editar usuario | 3 | 8 | 4 | 2 | 17 |
| RF-SEC-08 | Ver detalle usuario | 2 | 3 | 2 | 2 | 9 |
| RF-SEC-09 | Listar roles | 4 | 4 | 4 | 4 | 16 |
| RF-SEC-10 | Crear rol con permisos | 3 | 7 | 4 | 4 | 18 |
| RF-SEC-11 | Editar rol con diff | 4 | 7 | 4 | 4 | 19 |
| RF-SEC-12 | Eliminar rol | 3 | 4 | 2 | 4 | 13 |
| **Total** | | **32** | **60** | **35** | **32** | **159** |

---

## 2. Casos de Prueba por RF

### TP-SEC-01 — Login con credenciales

| Test ID | Tipo | Descripcion | RF |
|---------|------|-------------|-----|
| TP-SEC-01-01 | Positivo | Login exitoso retorna JWT + refresh token con claims correctos | RF-SEC-01 |
| TP-SEC-01-02 | Positivo | Login exitoso resetea `failed_attempts` a 0 y actualiza `last_login` | RF-SEC-01 |
| TP-SEC-01-03 | Negativo | Credenciales invalidas retornan 401 con mensaje generico | RF-SEC-01 |
| TP-SEC-01-04 | Negativo | Usuario inexistente retorna 401 (sin revelar existencia) | RF-SEC-01 |
| TP-SEC-01-05 | Negativo | Cuenta bloqueada retorna 403 "Cuenta bloqueada" | RF-SEC-01 |
| TP-SEC-01-06 | Negativo | Cuenta inactiva retorna 403 "Cuenta deshabilitada" | RF-SEC-01 |
| TP-SEC-01-07 | Negativo | Quinto intento fallido bloquea cuenta permanentemente | RF-SEC-01 |
| TP-SEC-01-08 | Negativo | Campos faltantes retornan 400 | RF-SEC-01 |
| TP-SEC-01-09 | Integracion | Login publica UserLoggedInEvent y Audit lo registra | RF-SEC-01 |
| TP-SEC-01-10 | Integracion | Bloqueo publica UserLockedEvent y Audit lo registra | RF-SEC-01 |
| TP-SEC-01-11 | Integracion | RLS filtra correctamente por tenant | RF-SEC-01 |
| TP-SEC-01-12 | E2E | Flujo completo login → JWT → acceso a endpoint protegido | RF-SEC-01 |
| TP-SEC-01-13 | E2E | Flujo login con OTP habilitado retorna requires_otp | RF-SEC-01 |

### TP-SEC-02 — Verificacion OTP

| Test ID | Tipo | Descripcion | RF |
|---------|------|-------------|-----|
| TP-SEC-02-01 | Positivo | OTP correcto dentro de TTL emite JWT + refresh token | RF-SEC-02 |
| TP-SEC-02-02 | Positivo | Reenvio de OTP genera nuevo codigo y envia email | RF-SEC-02 |
| TP-SEC-02-03 | Negativo | OTP incorrecto retorna 401 | RF-SEC-02 |
| TP-SEC-02-04 | Negativo | OTP expirado (>5min) retorna 401 | RF-SEC-02 |
| TP-SEC-02-05 | Negativo | Tercer intento fallido invalida otp_token | RF-SEC-02 |
| TP-SEC-02-06 | Negativo | otp_token invalido retorna 401 | RF-SEC-02 |
| TP-SEC-02-07 | Integracion | SendOtpEvent envia email via Notification Service | RF-SEC-02 |
| TP-SEC-02-08 | Integracion | OTP hash se persiste y verifica correctamente | RF-SEC-02 |
| TP-SEC-02-09 | E2E | Flujo login → pantalla OTP → verificacion → dashboard | RF-SEC-02 |
| TP-SEC-02-10 | E2E | Flujo OTP expirado → reenvio → verificacion exitosa | RF-SEC-02 |

### TP-SEC-03 — Refresh token con rotacion estricta

| Test ID | Tipo | Descripcion | RF |
|---------|------|-------------|-----|
| TP-SEC-03-01 | Positivo | Refresh valido emite nuevo JWT + nuevo refresh token | RF-SEC-03 |
| TP-SEC-03-02 | Positivo | Permisos actualizados se reflejan en nuevo JWT | RF-SEC-03 |
| TP-SEC-03-03 | Negativo | Token ya revocado (reuso) revoca TODA la cadena del usuario | RF-SEC-03 |
| TP-SEC-03-04 | Negativo | Token expirado retorna 401 | RF-SEC-03 |
| TP-SEC-03-05 | Negativo | Token inexistente retorna 401 | RF-SEC-03 |
| TP-SEC-03-06 | Negativo | Usuario locked/inactive entre refreshes retorna 403 | RF-SEC-03 |
| TP-SEC-03-07 | Integracion | Token anterior se marca como revocado tras refresh | RF-SEC-03 |
| TP-SEC-03-08 | Integracion | Roles/permisos modificados se cargan al nuevo JWT | RF-SEC-03 |
| TP-SEC-03-09 | E2E | SPA detecta JWT expirado → refresh automatico → continua | RF-SEC-03 |
| TP-SEC-03-10 | E2E | Deteccion de reuso → logout forzado en SPA | RF-SEC-03 |

### TP-SEC-04 — Recuperacion de contrasena

| Test ID | Tipo | Descripcion | RF |
|---------|------|-------------|-----|
| TP-SEC-04-01 | Positivo | Solicitud con email existente envia enlace de recuperacion | RF-SEC-04 |
| TP-SEC-04-02 | Positivo | Token valido permite cambiar contrasena y revoca refresh tokens | RF-SEC-04 |
| TP-SEC-04-03 | Negativo | Email inexistente retorna 200 (opacidad, no revela existencia) | RF-SEC-04 |
| TP-SEC-04-04 | Negativo | Token expirado retorna 400 | RF-SEC-04 |
| TP-SEC-04-05 | Negativo | Token ya usado retorna 400 | RF-SEC-04 |
| TP-SEC-04-06 | Negativo | Contrasena no cumple mascara retorna 422 | RF-SEC-04 |
| TP-SEC-04-07 | Negativo | Token inexistente retorna 400 | RF-SEC-04 |
| TP-SEC-04-08 | Integracion | SendPasswordResetEvent envia email correctamente | RF-SEC-04 |
| TP-SEC-04-09 | Integracion | PasswordChangedEvent registra auditoria | RF-SEC-04 |
| TP-SEC-04-10 | Integracion | Todos los refresh tokens del usuario se revocan | RF-SEC-04 |
| TP-SEC-04-11 | E2E | Flujo completo: forgot → email → click enlace → reset → login | RF-SEC-04 |
| TP-SEC-04-12 | E2E | Intento con enlace expirado muestra error y opcion de reenvio | RF-SEC-04 |

### TP-SEC-05 — Logout con revocacion

| Test ID | Tipo | Descripcion | RF |
|---------|------|-------------|-----|
| TP-SEC-05-01 | Positivo | Logout revoca refresh token y retorna 200 | RF-SEC-05 |
| TP-SEC-05-02 | Positivo | Logout con token ya revocado retorna 200 (idempotente) | RF-SEC-05 |
| TP-SEC-05-03 | Negativo | Refresh token de otro usuario retorna 403 | RF-SEC-05 |
| TP-SEC-05-04 | Negativo | Sin Authorization header retorna 401 | RF-SEC-05 |
| TP-SEC-05-05 | Negativo | Body sin refresh_token retorna 400 | RF-SEC-05 |
| TP-SEC-05-06 | Negativo | Token inexistente retorna 200 (idempotente) | RF-SEC-05 |
| TP-SEC-05-07 | Integracion | UserLoggedOutEvent publicado a Audit Service | RF-SEC-05 |
| TP-SEC-05-08 | Integracion | Solo el token enviado se revoca, no todos | RF-SEC-05 |
| TP-SEC-05-09 | E2E | Login → uso → logout → redireccion a /login | RF-SEC-05 |
| TP-SEC-05-10 | E2E | Tras logout, refresh con token revocado retorna 401 | RF-SEC-05 |

### TP-SEC-06 — Listar usuarios

| Test ID | Tipo | Descripcion | RF |
|---------|------|-------------|-----|
| TP-SEC-06-01 | Positivo | GET /users retorna lista paginada con campos esperados | RF-SEC-06 |
| TP-SEC-06-02 | Positivo | Busqueda por username/email filtra correctamente (ILIKE) | RF-SEC-06 |
| TP-SEC-06-03 | Positivo | Filtro por status retorna solo usuarios del estado solicitado | RF-SEC-06 |
| TP-SEC-06-04 | Negativo | Sin permiso p_users_list retorna 403 | RF-SEC-06 |
| TP-SEC-06-05 | Negativo | Pagina fuera de rango retorna lista vacia | RF-SEC-06 |
| TP-SEC-06-06 | Negativo | page_size > 100 se limita a 100 | RF-SEC-06 |
| TP-SEC-06-07 | Negativo | Sin JWT retorna 401 | RF-SEC-06 |
| TP-SEC-06-08 | Integracion | RLS filtra por tenant — usuarios de otro tenant no aparecen | RF-SEC-06 |
| TP-SEC-06-09 | Integracion | Admin Entidad solo ve usuarios de su entidad | RF-SEC-06 |
| TP-SEC-06-10 | Integracion | Exportacion Excel genera archivo con datos correctos | RF-SEC-06 |
| TP-SEC-06-11 | E2E | Navegar a /usuarios → ver grilla → buscar → filtrar → exportar | RF-SEC-06 |
| TP-SEC-06-12 | E2E | Click en fila navega a /usuarios/:id | RF-SEC-06 |

### TP-SEC-07 — Crear/editar usuario

| Test ID | Tipo | Descripcion | RF |
|---------|------|-------------|-----|
| TP-SEC-07-01 | Positivo | POST /users crea usuario con roles y asignaciones | RF-SEC-07 |
| TP-SEC-07-02 | Positivo | PUT /users/:id actualiza datos, roles y asignaciones | RF-SEC-07 |
| TP-SEC-07-03 | Positivo | Password se hashea con bcrypt (solo en create) | RF-SEC-07 |
| TP-SEC-07-04 | Negativo | Username duplicado por tenant retorna 409 | RF-SEC-07 |
| TP-SEC-07-05 | Negativo | Email duplicado por tenant retorna 409 | RF-SEC-07 |
| TP-SEC-07-06 | Negativo | Username no cumple mask.username retorna 422 | RF-SEC-07 |
| TP-SEC-07-07 | Negativo | Password no cumple mask.password retorna 422 | RF-SEC-07 |
| TP-SEC-07-08 | Negativo | Sin roles seleccionados retorna 422 | RF-SEC-07 |
| TP-SEC-07-09 | Negativo | Sin permiso p_users_create retorna 403 | RF-SEC-07 |
| TP-SEC-07-10 | Negativo | Admin Entidad intenta crear usuario en otra entidad retorna 403 | RF-SEC-07 |
| TP-SEC-07-11 | Negativo | Role_id inexistente retorna 422 | RF-SEC-07 |
| TP-SEC-07-12 | Integracion | UserCreatedEvent publicado a Audit Service | RF-SEC-07 |
| TP-SEC-07-13 | Integracion | UserUpdatedEvent publicado a Audit Service | RF-SEC-07 |
| TP-SEC-07-14 | Integracion | Mascaras cargadas dinamicamente desde Config Service | RF-SEC-07 |
| TP-SEC-07-15 | Integracion | Asignaciones se sincronizan correctamente (delete+insert) | RF-SEC-07 |
| TP-SEC-07-16 | E2E | Crear usuario → aparece en listado → ver detalle | RF-SEC-07 |
| TP-SEC-07-17 | E2E | Editar usuario → cambiar roles → verificar permisos al refresh | RF-SEC-07 |

### TP-SEC-08 — Ver detalle usuario

| Test ID | Tipo | Descripcion | RF |
|---------|------|-------------|-----|
| TP-SEC-08-01 | Positivo | GET /users/:id retorna datos completos + roles + permisos efectivos | RF-SEC-08 |
| TP-SEC-08-02 | Positivo | Permisos efectivos son DISTINCT union de todos los roles | RF-SEC-08 |
| TP-SEC-08-03 | Negativo | Usuario inexistente retorna 404 | RF-SEC-08 |
| TP-SEC-08-04 | Negativo | Sin permiso p_users_detail retorna 403 | RF-SEC-08 |
| TP-SEC-08-05 | Negativo | Usuario de otro tenant retorna 404 (RLS) | RF-SEC-08 |
| TP-SEC-08-06 | Integracion | Permisos agrupados por modulo y ordenados por code | RF-SEC-08 |
| TP-SEC-08-07 | Integracion | Asignaciones incluyen scope_name denormalizado | RF-SEC-08 |
| TP-SEC-08-08 | E2E | Navegar a /usuarios/:id → ver detalle → click editar | RF-SEC-08 |
| TP-SEC-08-09 | E2E | Admin Entidad solo ve usuarios de su entidad | RF-SEC-08 |

### TP-SEC-09 — Listar roles con contadores

| Test ID | Tipo | Descripcion | RF |
|---------|------|-------------|-----|
| TP-SEC-09-01 | Positivo | GET /roles retorna lista paginada con campos esperados | RF-SEC-09 |
| TP-SEC-09-02 | Positivo | Cada rol incluye permission_count y user_count calculados | RF-SEC-09 |
| TP-SEC-09-03 | Positivo | Busqueda por nombre/descripcion (ILIKE) filtra correctamente | RF-SEC-09 |
| TP-SEC-09-04 | Positivo | Roles seed aparecen con is_system = true | RF-SEC-09 |
| TP-SEC-09-05 | Negativo | Sin permiso p_roles_list retorna 403 | RF-SEC-09 |
| TP-SEC-09-06 | Negativo | Sin JWT retorna 401 | RF-SEC-09 |
| TP-SEC-09-07 | Negativo | page_size > 100 retorna 422 | RF-SEC-09 |
| TP-SEC-09-08 | Negativo | sort_by invalido retorna 422 | RF-SEC-09 |
| TP-SEC-09-09 | Integracion | RLS filtra por tenant — roles de otro tenant no aparecen | RF-SEC-09 |
| TP-SEC-09-10 | Integracion | Paginacion retorna subconjunto correcto con metadata | RF-SEC-09 |
| TP-SEC-09-11 | Integracion | Ordenamiento por name y created_at funciona | RF-SEC-09 |
| TP-SEC-09-12 | Integracion | Busqueda sin resultados retorna items vacio con total = 0 | RF-SEC-09 |
| TP-SEC-09-13 | E2E | Navegar a /roles → ver grilla con contadores | RF-SEC-09 |
| TP-SEC-09-14 | E2E | Buscar rol → filtrar → verificar resultados | RF-SEC-09 |
| TP-SEC-09-15 | E2E | Crear rol → listar → verificar que aparece con contadores | RF-SEC-09 |
| TP-SEC-09-16 | E2E | Roles seed con icono/badge de proteccion visible | RF-SEC-09 |

### TP-SEC-10 — Crear rol con permisos atomicos

| Test ID | Tipo | Descripcion | RF |
|---------|------|-------------|-----|
| TP-SEC-10-01 | Positivo | POST /roles crea rol con permisos y retorna 201 | RF-SEC-10 |
| TP-SEC-10-02 | Positivo | Rol creado tiene is_system = false | RF-SEC-10 |
| TP-SEC-10-03 | Positivo | Permisos duplicados en el array se deduplicar | RF-SEC-10 |
| TP-SEC-10-04 | Negativo | Nombre duplicado en tenant retorna 409 ROLE_NAME_DUPLICATE | RF-SEC-10 |
| TP-SEC-10-05 | Negativo | permission_ids vacio retorna 422 ROLE_PERMISSIONS_EMPTY | RF-SEC-10 |
| TP-SEC-10-06 | Negativo | Permiso UUID inexistente retorna 422 ROLE_PERMISSION_NOT_FOUND | RF-SEC-10 |
| TP-SEC-10-07 | Negativo | name vacio retorna 422 | RF-SEC-10 |
| TP-SEC-10-08 | Negativo | name > 100 chars retorna 422 | RF-SEC-10 |
| TP-SEC-10-09 | Negativo | Sin JWT retorna 401 | RF-SEC-10 |
| TP-SEC-10-10 | Negativo | Sin permiso p_roles_create retorna 403 | RF-SEC-10 |
| TP-SEC-10-11 | Integracion | RoleCreatedEvent publicado y Audit lo registra | RF-SEC-10 |
| TP-SEC-10-12 | Integracion | Transaccion atomica: fallo en role_permissions revierte roles | RF-SEC-10 |
| TP-SEC-10-13 | Integracion | RLS asigna tenant_id del JWT al rol creado | RF-SEC-10 |
| TP-SEC-10-14 | Integracion | GET /permissions retorna 88 permisos agrupados por modulo | RF-SEC-10 |
| TP-SEC-10-15 | E2E | Click "Nuevo rol" → formulario con modulos colapsables → guardar | RF-SEC-10 |
| TP-SEC-10-16 | E2E | Checkbox maestro selecciona/deselecciona todos los permisos del modulo | RF-SEC-10 |
| TP-SEC-10-17 | E2E | Contadores se actualizan en tiempo real al seleccionar permisos | RF-SEC-10 |
| TP-SEC-10-18 | E2E | Guardar sin permisos muestra error "Seleccione al menos un permiso" | RF-SEC-10 |

### TP-SEC-11 — Editar rol con diff de permisos

| Test ID | Tipo | Descripcion | RF |
|---------|------|-------------|-----|
| TP-SEC-11-01 | Positivo | PUT /roles/:id actualiza nombre, descripcion y permisos | RF-SEC-11 |
| TP-SEC-11-02 | Positivo | Diff de permisos: removidos se eliminan, agregados se insertan | RF-SEC-11 |
| TP-SEC-11-03 | Positivo | Editar solo metadata sin cambiar permisos funciona | RF-SEC-11 |
| TP-SEC-11-04 | Positivo | Rol de sistema permite cambiar permisos y descripcion | RF-SEC-11 |
| TP-SEC-11-05 | Negativo | Renombrar rol de sistema retorna 403 ROLE_SYSTEM_RENAME | RF-SEC-11 |
| TP-SEC-11-06 | Negativo | Nombre duplicado con otro rol retorna 409 | RF-SEC-11 |
| TP-SEC-11-07 | Negativo | permission_ids vacio retorna 422 | RF-SEC-11 |
| TP-SEC-11-08 | Negativo | Rol inexistente retorna 404 | RF-SEC-11 |
| TP-SEC-11-09 | Negativo | Sin JWT retorna 401 | RF-SEC-11 |
| TP-SEC-11-10 | Negativo | Sin permiso p_roles_edit retorna 403 | RF-SEC-11 |
| TP-SEC-11-11 | Negativo | Permiso inexistente retorna 422 | RF-SEC-11 |
| TP-SEC-11-12 | Integracion | RoleUpdatedEvent incluye added[] y removed[] correctos | RF-SEC-11 |
| TP-SEC-11-13 | Integracion | Transaccion atomica: fallo en INSERT revierte UPDATE de roles | RF-SEC-11 |
| TP-SEC-11-14 | Integracion | Refresh token de usuario afectado refleja nuevos permisos | RF-SEC-11 |
| TP-SEC-11-15 | Integracion | GET /roles/:id retorna rol con permisos actuales para precarga | RF-SEC-11 |
| TP-SEC-11-16 | E2E | Click en rol → formulario precargado → modificar permisos → guardar | RF-SEC-11 |
| TP-SEC-11-17 | E2E | Checkboxes marcados reflejan permisos actuales del rol | RF-SEC-11 |
| TP-SEC-11-18 | E2E | Editar rol de sistema → cambiar permisos → verificar nombre bloqueado | RF-SEC-11 |
| TP-SEC-11-19 | E2E | Contadores actualizados al marcar/desmarcar permisos | RF-SEC-11 |

### TP-SEC-12 — Eliminar rol con validacion de uso

| Test ID | Tipo | Descripcion | RF |
|---------|------|-------------|-----|
| TP-SEC-12-01 | Positivo | DELETE /roles/:id elimina rol sin usuarios y retorna 204 | RF-SEC-12 |
| TP-SEC-12-02 | Positivo | role_permissions eliminadas junto con el rol | RF-SEC-12 |
| TP-SEC-12-03 | Positivo | RoleDeletedEvent publicado y Audit lo registra | RF-SEC-12 |
| TP-SEC-12-04 | Negativo | Rol de sistema (is_system=true) retorna 403 ROLE_SYSTEM_PROTECTED | RF-SEC-12 |
| TP-SEC-12-05 | Negativo | Rol con usuarios asignados retorna 409 ROLE_IN_USE con cantidad | RF-SEC-12 |
| TP-SEC-12-06 | Negativo | Rol inexistente retorna 404 | RF-SEC-12 |
| TP-SEC-12-07 | Negativo | Sin JWT retorna 401 | RF-SEC-12 |
| TP-SEC-12-08 | Integracion | Eliminacion fisica: SELECT posterior retorna 0 filas | RF-SEC-12 |
| TP-SEC-12-09 | Integracion | RLS impide eliminar rol de otro tenant | RF-SEC-12 |
| TP-SEC-12-10 | E2E | Click "Eliminar" → confirmacion → rol desaparece de grilla | RF-SEC-12 |
| TP-SEC-12-11 | E2E | Boton "Eliminar" deshabilitado para roles de sistema | RF-SEC-12 |
| TP-SEC-12-12 | E2E | Intentar eliminar rol en uso → mensaje con cantidad de usuarios | RF-SEC-12 |
| TP-SEC-12-13 | E2E | Reasignar usuarios → eliminar rol → verificar eliminacion | RF-SEC-12 |

---

## Changelog

### v1.1.0 (2026-03-15)
- 66 casos de prueba agregados para RF-SEC-09 a RF-SEC-12 (roles y permisos)
- Total: 159 tests (32 positivos, 60 negativos, 35 integracion, 32 E2E)

### v1.0.0 (2026-03-15)
- 93 casos de prueba definidos para RF-SEC-01 a RF-SEC-08
- Cobertura: 18 positivos, 38 negativos, 21 integracion, 16 E2E
