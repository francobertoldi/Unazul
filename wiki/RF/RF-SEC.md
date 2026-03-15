# RF-SEC — Requerimientos Funcionales: Seguridad e Identidad

> **Proyecto:** Unazul Backoffice
> **Modulo:** Identity / Security (SA.identity)
> **Version:** 1.2.1
> **Fecha:** 2026-03-15
> **Prerequisitos:** `01_alcance_funcional.md`, `02_arquitectura.md`, `03_FL.md`, `05_modelo_datos.md`
> **Flujos origen:** FL-SEC-01 (Autenticacion y Gestion de Usuarios), FL-SEC-02 (Gestionar Roles y Permisos Atomicos)
> **HUs origen:** HU023, HU024, HU025, HU026, HU038

---

## Resumen de Requerimientos

| ID | Titulo | Prioridad | Severidad | HU | Estado |
|----|--------|-----------|-----------|-----|--------|
| RF-SEC-01 | Login con credenciales | Alta | P0 | HU038 | Documentado |
| RF-SEC-02 | Verificacion OTP | Alta | P0 | HU038 | Documentado |
| RF-SEC-03 | Refresh token con rotacion estricta | Alta | P0 | HU038 | Documentado |
| RF-SEC-04 | Recuperacion de contrasena | Alta | P1 | HU038 | Documentado |
| RF-SEC-05 | Logout con revocacion | Alta | P1 | HU038 | Documentado |
| RF-SEC-06 | Listar usuarios con filtros y paginacion | Alta | P1 | HU023 | Documentado |
| RF-SEC-07 | Crear/editar usuario con roles y asignaciones | Alta | P0 | HU024 | Documentado |
| RF-SEC-08 | Ver detalle de usuario con permisos efectivos | Media | P2 | HU025 | Documentado |
| RF-SEC-09 | Listar roles con contadores | Alta | P1 | HU026 | Documentado |
| RF-SEC-10 | Crear rol con permisos atomicos | Alta | P0 | HU026 | Documentado |
| RF-SEC-11 | Editar rol con diff de permisos | Alta | P0 | HU026 | Documentado |
| RF-SEC-12 | Eliminar rol con validacion de uso | Alta | P1 | HU026 | Documentado |

---

## Reglas de Negocio

| ID | Regla | Detalle | Decision |
|----|-------|---------|----------|
| RN-SEC-01 | Bloqueo permanente tras 5 intentos fallidos | El campo `users.status` cambia a `locked` al alcanzar 5 intentos fallidos consecutivos. Solo un Super Admin puede desbloquear manualmente. | D-SEC-01 |
| RN-SEC-02 | Rotacion estricta de refresh token | Cada uso de refresh token invalida el token anterior y genera uno nuevo. Si se detecta reuso de un token ya revocado, se revocan TODOS los refresh tokens del usuario. | D-SEC-02 |
| RN-SEC-03 | Propagacion de permisos al refresh | Los cambios en roles/permisos de un usuario se reflejan en el JWT recien al siguiente refresh token. El JWT activo mantiene los claims originales hasta su expiracion (15 min). | D-SEC-03 |
| RN-SEC-04 | Mensaje de error generico en login | Ante credenciales invalidas, el sistema responde "Usuario o contrasena incorrectos" sin revelar cual de los dos es incorrecto. | — |
| RN-SEC-05 | Respuesta opaca en recuperacion | El endpoint `POST /auth/forgot-password` siempre responde 200 independientemente de si el email existe, para no revelar la existencia de cuentas. | — |
| RN-SEC-06 | Unicidad de username y email por tenant | `username` y `email` son unicos dentro del mismo `tenant_id`. Indices: `users(tenant_id, username)` UNIQUE, `users(tenant_id, email)` UNIQUE. | — |
| RN-SEC-07 | Validacion de mascaras dinamicas | Las mascaras `mask.username` y `mask.password` se obtienen del Config Service (cache Redis). Si Config no responde, se aplica fallback: username `^[a-zA-Z0-9._-]{3,30}$`, password `^.{8,}$`. | — |
| RN-SEC-08 | JWT claims obligatorios | Todo JWT emitido contiene: `sub` (user_id), `tenant_id`, `roles[]`, `permissions[]`, `entity_id` (nullable), `iat`, `exp`. Tiempo de vida: 15 minutos. | — |
| RN-SEC-09 | Cuenta inactiva no puede autenticarse | Si `users.status = inactive`, el login se rechaza con error tipado `AUTH_ACCOUNT_INACTIVE` sin incrementar contador de intentos fallidos. | — |
| RN-SEC-10 | Cuenta bloqueada no puede autenticarse | Si `users.status = locked`, el login se rechaza con error tipado `AUTH_ACCOUNT_LOCKED` sin incrementar contador de intentos fallidos. | — |
| RN-SEC-11 | Hash de refresh token | El refresh token se almacena como hash SHA-256 en `refresh_tokens.token_hash`. El valor en claro se envia al cliente una sola vez. | — |
| RN-SEC-12 | Token de recuperacion de un solo uso | El token de reset de contrasena se invalida tras su primer uso exitoso. Tiene un TTL configurable via parametro (default: 30 minutos). | — |
| RN-SEC-13 | Roles predefinidos protegidos | Los 7 roles seed tienen `is_system = true` y no pueden eliminarse ni renombrarse. Sus permisos SI pueden editarse. | — |
| RN-SEC-14 | Unicidad de nombre de rol por tenant | `roles.name` es unico dentro del mismo `tenant_id`. Indice: `roles(tenant_id, name)` UNIQUE. | — |
| RN-SEC-15 | Eliminacion fisica de rol sin usuarios | Un rol solo puede eliminarse si no tiene usuarios asignados (`user_roles` vacio para ese `role_id`). La eliminacion es fisica (DELETE `role_permissions` + DELETE `roles`). | — |
| RN-SEC-16 | Diff de permisos en edicion | Al editar un rol, el backend calcula la diferencia entre permisos actuales y nuevos. Solo ejecuta DELETE de los removidos e INSERT de los agregados. El evento `RoleUpdatedEvent` incluye `added[]` y `removed[]`. | — |
| RN-SEC-17 | Catalogo de permisos global | La tabla `permissions` contiene 88 permisos fijos organizados en 15 modulos. No tiene `tenant_id` (es catalogo compartido). Solo se modifica via migracion. | — |
| RN-SEC-18 | Propagacion de cambios de rol | Cuando se editan los permisos de un rol, los usuarios afectados reciben los nuevos permisos al proximo refresh token (D-SEC-03). No se fuerza logout. | D-SEC-03 |
| RN-SEC-19 | Formato estandar de errores HTTP | Todos los endpoints del modulo Identity retornan errores en formato uniforme: `{ "error": "<mensaje legible>", "code": "<CODIGO_ERROR_TIPADO>", "details": { ... } }`. El campo `error` contiene el mensaje para el usuario, `code` el codigo tipado documentado en cada RF, y `details` (opcional) contiene campos adicionales como validaciones fallidas o metadata. Aplica a todos los RF-SEC-*. | — |

---

## Requerimientos Funcionales

---

### RF-SEC-01 — Login con credenciales

#### Execution Sheet

| Campo | Valor |
|-------|-------|
| **ID** | RF-SEC-01 |
| **Titulo** | Login con credenciales (usuario y contrasena) |
| **Actor(es)** | Usuario (cualquier rol autenticable) |
| **Prioridad** | Alta |
| **Severidad** | P0 |
| **Flujo origen** | FL-SEC-01 seccion 6 |
| **HU origen** | HU038 |

#### Precondiciones

| # | Condicion | Detalle |
|---|-----------|---------|
| 1 | Identity Service operativo | SA.identity accesible y saludable |
| 2 | Tabla `users` con al menos un registro activo | Seed inicial ejecutado |
| 3 | Tabla `permissions` inicializada | 88 permisos atomicos cargados (seed data) |
| 4 | Roles predefinidos creados | 7 roles seed en tabla `roles` con `is_system = true` |
| 5 | Parametros de mascaras accesibles | `mask.username` y `mask.password` disponibles en Config Service o cache Redis |
| 6 | RabbitMQ operativo | Para publicacion de eventos asincrona |

#### Entradas

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| `username` | string | Si | Body JSON | Regex de `mask.username` (fallback: `^[a-zA-Z0-9._-]{3,30}$`). Trim de espacios. | RN-SEC-07 |
| `password` | string | Si | Body JSON | No vacia, longitud maxima 128 caracteres. No se valida contra mascara en login (solo en creacion). | — |
| `tenant_id` | uuid | Si | Header `X-Tenant-Id` o subdominio | UUID v4 valido, tenant existente y activo | — |

#### Proceso (Happy Path)

| # | Paso | Responsable |
|---|------|-------------|
| 1 | Recibir `POST /auth/login` con `username`, `password` | API Gateway |
| 2 | Resolver `tenant_id` desde header o subdominio | API Gateway |
| 3 | Ejecutar `SET LOCAL app.current_tenant = '{tenant_id}'` para activar RLS | Identity Service |
| 4 | Buscar usuario en tabla `users` por `username` dentro del tenant (indice `users(tenant_id, username)`) | Identity Service |
| 5 | Verificar que `users.status = 'active'` | Identity Service |
| 6 | Comparar `password` contra `users.password_hash` usando bcrypt verify | Identity Service |
| 7 | Resetear contador de intentos fallidos: `failed_attempts = 0` | Identity Service |
| 8 | Cargar roles del usuario via `user_roles` JOIN `roles` | Identity Service |
| 9 | Cargar permisos efectivos via `role_permissions` JOIN `permissions` (union de todos los roles) | Identity Service |
| 10 | Generar JWT con claims: `sub`, `tenant_id`, `roles[]`, `permissions[]`, `entity_id`, `iat`, `exp` (15 min) | Identity Service |
| 11 | Generar refresh token aleatorio (256 bits), calcular SHA-256, insertar en `refresh_tokens` (`user_id`, `token_hash`, `expires_at`, `revoked = false`, `created_at`) | Identity Service |
| 12 | Actualizar `users.last_login = NOW()` | Identity Service |
| 13 | Publicar `UserLoggedInEvent` a RabbitMQ con `{ user_id, tenant_id, username, ip_address, timestamp }` | Identity Service |
| 14 | Retornar `200 OK` con `{ access_token, refresh_token, expires_in, token_type: "Bearer" }` | Identity Service |

#### Salidas

| Campo | Tipo | Destino | Efecto observable |
|-------|------|---------|-------------------|
| `access_token` | string (JWT) | Response body → SPA | Token para autorizacion en headers subsiguientes |
| `refresh_token` | string (opaco) | Response body → SPA | Token para renovacion; almacenado en memoria del SPA |
| `expires_in` | integer | Response body → SPA | Segundos hasta expiracion del JWT (900) |
| `token_type` | string | Response body → SPA | Siempre `"Bearer"` |
| `UserLoggedInEvent` | evento async | RabbitMQ → Audit Service | Registro en `audit_log` con operation `Login` |
| `users.last_login` | timestamp | SA.identity | Actualizado al momento del login exitoso |
| `users.failed_attempts` | integer | SA.identity | Reseteado a 0 |

#### Errores Tipados

| Codigo | Causa | Condicion de disparo | Respuesta esperada |
|--------|-------|---------------------|--------------------|
| `AUTH_INVALID_CREDENTIALS` | Credenciales incorrectas | `username` no encontrado en tenant O `password` no coincide con `password_hash` | HTTP 401, mensaje: "Usuario o contrasena incorrectos" (RN-SEC-04) |
| `AUTH_ACCOUNT_LOCKED` | Cuenta bloqueada | `users.status = 'locked'` al momento de buscar el usuario | HTTP 403, mensaje: "Cuenta bloqueada. Contacte al administrador." |
| `AUTH_ACCOUNT_INACTIVE` | Cuenta inactiva | `users.status = 'inactive'` al momento de buscar el usuario | HTTP 403, mensaje: "Cuenta inactiva. Contacte al administrador." |
| `AUTH_MAX_ATTEMPTS_REACHED` | Bloqueo por 5to intento fallido | `failed_attempts` alcanza 5 tras incrementar (D-SEC-01) | HTTP 403, mensaje: "Cuenta bloqueada por multiples intentos fallidos." Se publica `UserLockedEvent`. |
| `AUTH_TENANT_NOT_FOUND` | Tenant invalido | `tenant_id` no existe o no esta activo | HTTP 400, mensaje: "Tenant no valido." |
| `AUTH_VALIDATION_ERROR` | Entrada malformada | `username` o `password` vacios, `username` no cumple formato basico | HTTP 422, mensaje: detalle de campos invalidos |

#### Casos Especiales y Variantes

- **Incremento de intentos fallidos:** Si la contrasena es incorrecta, se incrementa `failed_attempts` en la tabla `users`. Si tras el incremento `failed_attempts >= 5`, se ejecuta `UPDATE users SET status = 'locked'` y se publica `UserLockedEvent` a RabbitMQ. El usuario recibe `AUTH_MAX_ATTEMPTS_REACHED` en lugar de `AUTH_INVALID_CREDENTIALS`.
- **OTP habilitado:** Si el parametro de tenant indica que OTP esta habilitado, este RF NO emite tokens finales. En su lugar, retorna `200 OK` con `{ requires_otp: true, otp_token }`. La emision de tokens se delega a RF-SEC-02.
- **Orden de validacion:** Primero se verifica existencia del usuario, luego estado (`locked`/`inactive` se rechazan sin incrementar intentos), luego contrasena.
- **Concurrencia en failed_attempts:** Se usa `UPDATE users SET failed_attempts = failed_attempts + 1 WHERE id = :id RETURNING failed_attempts` para evitar race conditions.
- **Tiempo de respuesta constante:** Si el usuario no existe, se ejecuta igualmente un bcrypt verify contra un hash dummy para evitar timing attacks.
- **Campo `failed_attempts` no existe como columna explicita en el modelo actual:** Se asume que se agrega como columna `integer DEFAULT 0` en tabla `users` durante la implementacion, o se gestiona en cache Redis por usuario. Decision de implementacion.

#### Impacto en Modelo de Datos

| Entidad | Operacion | Campo(s) afectado(s) | Evento |
|---------|-----------|---------------------|--------|
| `users` | SELECT | `username`, `password_hash`, `status` | — |
| `users` | UPDATE | `last_login`, `failed_attempts` | — |
| `users` | UPDATE (lock) | `status = 'locked'` | `UserLockedEvent` |
| `user_roles` | SELECT | `user_id`, `role_id` | — |
| `role_permissions` | SELECT | `role_id`, `permission_id` | — |
| `permissions` | SELECT | `code`, `module`, `action` | — |
| `refresh_tokens` | INSERT | `user_id`, `token_hash`, `expires_at`, `revoked`, `created_at` | — |
| `audit_log` (SA.audit) | INSERT (async) | `user_id`, `operation = 'Login'`, `ip_address` | `UserLoggedInEvent` |

#### Criterios de Aceptacion (Gherkin)

```gherkin
Scenario: Login exitoso con credenciales validas
  Given un usuario con username "jperez" y status "active" en el tenant "tenant-001"
  And la contrasena almacenada coincide con "MiPassword123!"
  When envio POST /auth/login con username "jperez" y password "MiPassword123!"
  Then recibo HTTP 200 con access_token, refresh_token y expires_in = 900
  And el campo users.last_login se actualiza al timestamp actual
  And se publica UserLoggedInEvent en RabbitMQ

Scenario: Login fallido por contrasena incorrecta
  Given un usuario con username "jperez" y status "active" con failed_attempts = 0
  When envio POST /auth/login con username "jperez" y password "incorrecta"
  Then recibo HTTP 401 con codigo AUTH_INVALID_CREDENTIALS
  And el campo failed_attempts se incrementa a 1

Scenario: Bloqueo de cuenta tras 5to intento fallido
  Given un usuario con username "jperez" y status "active" con failed_attempts = 4
  When envio POST /auth/login con username "jperez" y password "incorrecta"
  Then recibo HTTP 403 con codigo AUTH_MAX_ATTEMPTS_REACHED
  And el campo users.status cambia a "locked"
  And se publica UserLockedEvent en RabbitMQ

Scenario: Login rechazado para cuenta bloqueada sin incrementar intentos
  Given un usuario con username "jperez" y status "locked"
  When envio POST /auth/login con username "jperez" y password "cualquiera"
  Then recibo HTTP 403 con codigo AUTH_ACCOUNT_LOCKED
  And el campo failed_attempts NO se incrementa

Scenario: Login rechazado para cuenta inactiva
  Given un usuario con username "jperez" y status "inactive"
  When envio POST /auth/login con username "jperez" y password "cualquiera"
  Then recibo HTTP 403 con codigo AUTH_ACCOUNT_INACTIVE

Scenario: Login con OTP habilitado retorna paso intermedio
  Given un usuario con credenciales validas en un tenant con OTP habilitado
  When envio POST /auth/login con credenciales correctas
  Then recibo HTTP 200 con requires_otp = true y otp_token
  And NO recibo access_token ni refresh_token
```

#### Trazabilidad de Pruebas

| Test ID | Tipo | Descripcion |
|---------|------|-------------|
| TP-SEC-01-01 | Positivo | Login exitoso retorna JWT y refresh token con claims correctos |
| TP-SEC-01-02 | Positivo | Login exitoso actualiza `users.last_login` |
| TP-SEC-01-03 | Positivo | Login exitoso resetea `failed_attempts` a 0 |
| TP-SEC-01-04 | Positivo | Login exitoso publica `UserLoggedInEvent` |
| TP-SEC-01-05 | Negativo | Login con contrasena incorrecta retorna 401 e incrementa `failed_attempts` |
| TP-SEC-01-06 | Negativo | Login con username inexistente retorna 401 (mismo mensaje que contrasena incorrecta) |
| TP-SEC-01-07 | Negativo | Quinto intento fallido bloquea cuenta y retorna 403 |
| TP-SEC-01-08 | Negativo | Login a cuenta locked retorna 403 sin incrementar intentos |
| TP-SEC-01-09 | Negativo | Login a cuenta inactive retorna 403 sin incrementar intentos |
| TP-SEC-01-10 | Negativo | Login con tenant inexistente retorna 400 |
| TP-SEC-01-11 | Negativo | Login con campos vacios retorna 422 |
| TP-SEC-01-12 | Seguridad | Tiempo de respuesta constante entre usuario existente e inexistente (timing attack) |
| TP-SEC-01-13 | Integracion | Login con OTP habilitado retorna `requires_otp: true` sin emitir tokens |

#### Sin Ambiguedades

- **Supuestos prohibidos:** No se asume que el frontend envia tenant_id en el body. No se asume que el hash de contrasena usa un algoritmo distinto a bcrypt. No se asume que failed_attempts se persiste fuera de SA.identity.
- **Decisiones cerradas:** D-SEC-01 (bloqueo permanente tras 5 intentos). El desbloqueo es manual por Super Admin (RF-SEC-07). El contador de intentos es por usuario, no por IP.
- **Fuera de alcance explicito:** Captcha, rate limiting por IP (delegado a infraestructura/API Gateway), MFA por app authenticator, login federado SSO/OAuth.
- **TODO explicitos = 0**

---

### RF-SEC-02 — Verificacion OTP

#### Execution Sheet

| Campo | Valor |
|-------|-------|
| **ID** | RF-SEC-02 |
| **Titulo** | Verificacion de codigo OTP enviado por email |
| **Actor(es)** | Usuario (cualquier rol autenticable) |
| **Prioridad** | Alta |
| **Severidad** | P0 |
| **Flujo origen** | FL-SEC-01 seccion 6 (alternativa OTP) |
| **HU origen** | HU038 |

#### Precondiciones

| # | Condicion | Detalle |
|---|-----------|---------|
| 1 | Login exitoso completado (RF-SEC-01) | El usuario recibio `requires_otp: true` y un `otp_token` valido |
| 2 | OTP generado y persistido | Codigo de 6 digitos hasheado, asociado al `otp_token`, con TTL de 5 minutos |
| 3 | Email de OTP enviado | `SendOtpEvent` procesado por Notification Service; email entregado al usuario |
| 4 | Notification Service operativo | Para reenvio de OTP si se solicita |

#### Entradas

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| `otp_token` | string | Si | Body JSON | Token opaco emitido por RF-SEC-01, no expirado, no usado | — |
| `otp_code` | string | Si | Body JSON | Exactamente 6 digitos numericos (`^[0-9]{6}$`) | — |
| `tenant_id` | uuid | Si | Header `X-Tenant-Id` o subdominio | UUID v4 valido | — |

#### Proceso (Happy Path)

| # | Paso | Responsable |
|---|------|-------------|
| 1 | Recibir `POST /auth/verify-otp` con `otp_token` y `otp_code` | API Gateway |
| 2 | Resolver `tenant_id` y activar RLS | Identity Service |
| 3 | Buscar registro OTP por `otp_token` (hash) | Identity Service |
| 4 | Verificar que el OTP no ha expirado (`created_at + TTL > NOW()`) | Identity Service |
| 5 | Verificar que el OTP no ha sido usado previamente | Identity Service |
| 6 | Comparar `otp_code` contra el hash almacenado | Identity Service |
| 7 | Marcar OTP como usado | Identity Service |
| 8 | Recuperar `user_id` asociado al OTP | Identity Service |
| 9 | Cargar roles y permisos del usuario (misma logica que RF-SEC-01, pasos 8-9) | Identity Service |
| 10 | Generar JWT con claims completos (misma logica que RF-SEC-01, paso 10) | Identity Service |
| 11 | Generar refresh token y persistir hash en `refresh_tokens` | Identity Service |
| 12 | Actualizar `users.last_login = NOW()` | Identity Service |
| 13 | Publicar `UserLoggedInEvent` a RabbitMQ | Identity Service |
| 14 | Retornar `200 OK` con `{ access_token, refresh_token, expires_in, token_type: "Bearer" }` | Identity Service |

#### Salidas

| Campo | Tipo | Destino | Efecto observable |
|-------|------|---------|-------------------|
| `access_token` | string (JWT) | Response body → SPA | Token de autorizacion con claims completos |
| `refresh_token` | string (opaco) | Response body → SPA | Token para renovacion |
| `expires_in` | integer | Response body → SPA | Segundos hasta expiracion (900) |
| `token_type` | string | Response body → SPA | `"Bearer"` |
| `UserLoggedInEvent` | evento async | RabbitMQ → Audit Service | Registro en `audit_log` con operation `Login` |
| `users.last_login` | timestamp | SA.identity | Actualizado |

#### Errores Tipados

| Codigo | Causa | Condicion de disparo | Respuesta esperada |
|--------|-------|---------------------|--------------------|
| `OTP_INVALID_CODE` | Codigo incorrecto | `otp_code` no coincide con el hash almacenado | HTTP 401, mensaje: "Codigo invalido o expirado" |
| `OTP_EXPIRED` | Codigo expirado | `created_at + TTL < NOW()` | HTTP 401, mensaje: "Codigo invalido o expirado" |
| `OTP_TOKEN_INVALID` | Token OTP invalido | `otp_token` no encontrado o ya usado | HTTP 401, mensaje: "Sesion de verificacion invalida. Inicie sesion nuevamente." |
| `OTP_MAX_ATTEMPTS` | Maximo de intentos de verificacion OTP | 3 intentos fallidos de verificacion para el mismo `otp_token` | HTTP 403, mensaje: "Demasiados intentos. Solicite un nuevo codigo." |
| `AUTH_VALIDATION_ERROR` | Entrada malformada | `otp_code` no cumple formato de 6 digitos | HTTP 422, mensaje: detalle de campos invalidos |

#### Casos Especiales y Variantes

- **Reenvio de OTP:** El SPA puede solicitar reenvio via `POST /auth/resend-otp` con el `otp_token`. Se invalida el OTP anterior, se genera uno nuevo con TTL fresco, y se publica nuevo `SendOtpEvent`. Maximo 3 reenvios por sesion de login.
- **Maximo de intentos de verificacion:** Tras 3 intentos fallidos contra el mismo `otp_token`, este se invalida. El usuario debe reiniciar el flujo de login desde RF-SEC-01.
- **Mensaje unificado:** Los errores `OTP_INVALID_CODE` y `OTP_EXPIRED` retornan el mismo mensaje al cliente para no revelar si el codigo existia pero expiro.
- **Canal de entrega OTP:** En MVP, el unico canal soportado para envio de OTP es **email**. No se soporta SMS ni app authenticator (TOTP). El `SendOtpEvent` siempre se procesa por Notification Service como email. Canales adicionales se consideran evolucion futura (ver FL-SEC-01, alcance "Fuera").
- **Limpieza de OTPs:** Los registros OTP expirados o usados se eliminan periodicamente mediante un job de limpieza (fuera del alcance de este RF, tarea operativa).

#### Impacto en Modelo de Datos

| Entidad | Operacion | Campo(s) afectado(s) | Evento |
|---------|-----------|---------------------|--------|
| OTP store (Redis o tabla temporal) | SELECT, UPDATE | `otp_token`, `otp_code_hash`, `used`, `attempts` | — |
| `users` | UPDATE | `last_login` | — |
| `user_roles` | SELECT | `user_id`, `role_id` | — |
| `role_permissions` | SELECT | `role_id`, `permission_id` | — |
| `permissions` | SELECT | `code`, `module`, `action` | — |
| `refresh_tokens` | INSERT | `user_id`, `token_hash`, `expires_at`, `revoked`, `created_at` | — |
| `audit_log` (SA.audit) | INSERT (async) | `user_id`, `operation = 'Login'`, `ip_address` | `UserLoggedInEvent` |

#### Criterios de Aceptacion (Gherkin)

```gherkin
Scenario: Verificacion OTP exitosa completa el login
  Given un usuario que completo RF-SEC-01 y recibio otp_token "abc123"
  And el codigo OTP enviado por email es "482916"
  When envio POST /auth/verify-otp con otp_token "abc123" y otp_code "482916"
  Then recibo HTTP 200 con access_token, refresh_token y expires_in = 900
  And el OTP se marca como usado
  And se publica UserLoggedInEvent

Scenario: OTP incorrecto rechaza verificacion
  Given un otp_token valido con codigo "482916"
  When envio POST /auth/verify-otp con otp_code "000000"
  Then recibo HTTP 401 con codigo OTP_INVALID_CODE
  And el contador de intentos del otp_token se incrementa

Scenario: OTP expirado rechaza verificacion
  Given un otp_token cuyo codigo fue generado hace mas de 5 minutos
  When envio POST /auth/verify-otp con el codigo correcto
  Then recibo HTTP 401 con mensaje "Codigo invalido o expirado"

Scenario: Tercer intento fallido invalida otp_token
  Given un otp_token con 2 intentos fallidos previos
  When envio POST /auth/verify-otp con un codigo incorrecto
  Then recibo HTTP 403 con codigo OTP_MAX_ATTEMPTS
  And el otp_token se invalida permanentemente

Scenario: Reenvio de OTP genera nuevo codigo
  Given un otp_token valido con 0 reenvios previos
  When envio POST /auth/resend-otp con otp_token
  Then recibo HTTP 200
  And se genera un nuevo codigo OTP con TTL fresco
  And se publica SendOtpEvent
```

#### Trazabilidad de Pruebas

| Test ID | Tipo | Descripcion |
|---------|------|-------------|
| TP-SEC-02-01 | Positivo | OTP correcto y vigente completa login con JWT y refresh token |
| TP-SEC-02-02 | Positivo | OTP correcto actualiza `users.last_login` |
| TP-SEC-02-03 | Positivo | Reenvio de OTP genera nuevo codigo y publica `SendOtpEvent` |
| TP-SEC-02-04 | Negativo | OTP incorrecto retorna 401 |
| TP-SEC-02-05 | Negativo | OTP expirado retorna 401 con mismo mensaje que incorrecto |
| TP-SEC-02-06 | Negativo | Tercer intento fallido retorna 403 e invalida otp_token |
| TP-SEC-02-07 | Negativo | otp_token ya usado retorna 401 |
| TP-SEC-02-08 | Negativo | otp_code con formato invalido (no 6 digitos) retorna 422 |
| TP-SEC-02-09 | Negativo | Cuarto reenvio de OTP es rechazado |
| TP-SEC-02-10 | Integracion | Flujo completo: RF-SEC-01 con OTP → RF-SEC-02 exitoso → JWT valido |

#### Sin Ambiguedades

- **Supuestos prohibidos:** No se asume que el OTP se almacena en texto plano. No se asume que el canal de entrega es distinto a email. No se asume que el TTL es fijo (es configurable via parametro, default 5 min).
- **Decisiones cerradas:** OTP es de 6 digitos numericos. Maximo 3 intentos de verificacion por otp_token. Maximo 3 reenvios por sesion de login. El OTP se almacena hasheado.
- **Fuera de alcance explicito:** MFA por app authenticator (TOTP), OTP por SMS, validacion de dispositivo, remember device.
- **TODO explicitos = 0**

---

### RF-SEC-03 — Refresh token con rotacion estricta

#### Execution Sheet

| Campo | Valor |
|-------|-------|
| **ID** | RF-SEC-03 |
| **Titulo** | Renovacion de JWT mediante refresh token con rotacion estricta |
| **Actor(es)** | Usuario (sesion activa — accion automatica por SPA) |
| **Prioridad** | Alta |
| **Severidad** | P0 |
| **Flujo origen** | FL-SEC-01 seccion 7a |
| **HU origen** | HU038 |

#### Precondiciones

| # | Condicion | Detalle |
|---|-----------|---------|
| 1 | Sesion activa | El usuario completo login (RF-SEC-01 o RF-SEC-02) y posee un refresh token valido |
| 2 | Refresh token no expirado | `refresh_tokens.expires_at > NOW()` |
| 3 | Refresh token no revocado | `refresh_tokens.revoked = false` |

#### Entradas

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| `refresh_token` | string | Si | Body JSON | No vacio, formato opaco valido | RN-SEC-02 |
| `tenant_id` | uuid | Si | Header `X-Tenant-Id` o subdominio | UUID v4 valido | — |

#### Proceso (Happy Path)

| # | Paso | Responsable |
|---|------|-------------|
| 1 | Recibir `POST /auth/refresh` con `refresh_token` | API Gateway |
| 2 | Calcular SHA-256 del `refresh_token` recibido | Identity Service |
| 3 | Buscar en `refresh_tokens` por `token_hash` (indice UNIQUE) | Identity Service |
| 4 | Verificar que `revoked = false` | Identity Service |
| 5 | Verificar que `expires_at > NOW()` | Identity Service |
| 6 | Marcar token actual como revocado: `UPDATE refresh_tokens SET revoked = true WHERE id = :id` | Identity Service |
| 7 | Cargar `user_id` del token; verificar `users.status = 'active'` | Identity Service |
| 8 | Recargar roles y permisos actuales del usuario (D-SEC-03: propagacion al refresh) | Identity Service |
| 9 | Generar nuevo JWT con claims actualizados | Identity Service |
| 10 | Generar nuevo refresh token, calcular SHA-256, insertar en `refresh_tokens` con `expires_at = NOW() + refresh_token_ttl` (default: 7 dias, configurable). La expiracion es **absoluta** desde la emision original; cada rotacion genera un nuevo token con su propio TTL de 7 dias. | Identity Service |
| 11 | Retornar `200 OK` con `{ access_token, refresh_token, expires_in, token_type: "Bearer" }` | Identity Service |

#### Salidas

| Campo | Tipo | Destino | Efecto observable |
|-------|------|---------|-------------------|
| `access_token` | string (JWT) | Response body → SPA | Nuevo JWT con permisos actualizados |
| `refresh_token` | string (opaco) | Response body → SPA | Nuevo refresh token (el anterior ya no es valido) |
| `expires_in` | integer | Response body → SPA | 900 segundos |
| `token_type` | string | Response body → SPA | `"Bearer"` |
| `refresh_tokens` (anterior) | UPDATE | SA.identity | `revoked = true` |
| `refresh_tokens` (nuevo) | INSERT | SA.identity | Nuevo registro con `revoked = false` |

#### Errores Tipados

| Codigo | Causa | Condicion de disparo | Respuesta esperada |
|--------|-------|---------------------|--------------------|
| `AUTH_TOKEN_EXPIRED` | Refresh token expirado | `refresh_tokens.expires_at <= NOW()` | HTTP 401, mensaje: "Sesion expirada. Inicie sesion nuevamente." |
| `AUTH_TOKEN_REVOKED_REUSE` | Reuso de token ya revocado (posible robo) | `refresh_tokens.revoked = true` al buscar el token | HTTP 401, mensaje: "Sesion expirada." Se revocan TODOS los tokens del usuario (D-SEC-02). |
| `AUTH_TOKEN_NOT_FOUND` | Token no existe | SHA-256 del token no encontrado en `refresh_tokens` | HTTP 401, mensaje: "Sesion invalida." |
| `AUTH_ACCOUNT_LOCKED` | Cuenta bloqueada entre refreshes | `users.status = 'locked'` al recargar usuario | HTTP 403, mensaje: "Cuenta bloqueada." Se revoca el token actual. |
| `AUTH_ACCOUNT_INACTIVE` | Cuenta desactivada entre refreshes | `users.status = 'inactive'` al recargar usuario | HTTP 403, mensaje: "Cuenta inactiva." Se revoca el token actual. |

#### Casos Especiales y Variantes

- **Deteccion de reuso (D-SEC-02):** Si se recibe un refresh token que ya esta marcado como `revoked = true`, se interpreta como posible robo de token. Se ejecuta `UPDATE refresh_tokens SET revoked = true WHERE user_id = :user_id AND revoked = false` para revocar TODA la cadena de tokens del usuario. Esto fuerza re-login en todos los dispositivos.
- **Propagacion de permisos (D-SEC-03):** Los roles y permisos se recargan de la base de datos en cada refresh. Si un admin cambio los permisos del usuario entre el login y el refresh, el nuevo JWT refleja los permisos actualizados.
- **Cuenta bloqueada/inactiva durante sesion:** Si el estado del usuario cambio a `locked` o `inactive` entre el login y el refresh, el refresh se rechaza y se revoca el token.
- **Concurrencia:** Si dos pestanas del SPA intentan refresh simultaneamente con el mismo token, la primera tendra exito y la segunda recibira `AUTH_TOKEN_REVOKED_REUSE`, lo cual revocara todos los tokens. Para mitigar, el SPA debe serializar las peticiones de refresh y compartir el resultado.
- **Expiracion del refresh token:** El TTL del refresh token es configurable via parametro (default: 7 dias). Se almacena en `refresh_tokens.expires_at`.

#### Impacto en Modelo de Datos

| Entidad | Operacion | Campo(s) afectado(s) | Evento |
|---------|-----------|---------------------|--------|
| `refresh_tokens` | SELECT | `token_hash`, `revoked`, `expires_at`, `user_id` | — |
| `refresh_tokens` | UPDATE | `revoked = true` (token usado) | — |
| `refresh_tokens` | UPDATE (reuso) | `revoked = true` en TODOS los tokens del usuario | — |
| `refresh_tokens` | INSERT | nuevo token: `user_id`, `token_hash`, `expires_at`, `revoked = false`, `created_at` | — |
| `users` | SELECT | `status` | — |
| `user_roles` | SELECT | `user_id`, `role_id` | — |
| `role_permissions` | SELECT | `role_id`, `permission_id` | — |
| `permissions` | SELECT | `code`, `module`, `action` | — |

#### Criterios de Aceptacion (Gherkin)

```gherkin
Scenario: Refresh exitoso rota token y actualiza permisos
  Given un refresh token valido "rt-old" no expirado y no revocado
  And el usuario tiene status "active"
  When envio POST /auth/refresh con refresh_token "rt-old"
  Then recibo HTTP 200 con un nuevo access_token y un nuevo refresh_token "rt-new"
  And el token "rt-old" queda con revoked = true en refresh_tokens
  And el nuevo JWT contiene los permisos actuales del usuario

Scenario: Refresh con token expirado requiere re-login
  Given un refresh token "rt-expired" con expires_at en el pasado
  When envio POST /auth/refresh con refresh_token "rt-expired"
  Then recibo HTTP 401 con codigo AUTH_TOKEN_EXPIRED

Scenario: Deteccion de reuso revoca toda la cadena
  Given un refresh token "rt-reused" que ya fue marcado como revoked = true
  And el usuario tiene 3 refresh tokens activos en otros dispositivos
  When envio POST /auth/refresh con refresh_token "rt-reused"
  Then recibo HTTP 401 con codigo AUTH_TOKEN_REVOKED_REUSE
  And TODOS los refresh tokens del usuario quedan con revoked = true

Scenario: Refresh rechazado si cuenta fue bloqueada
  Given un refresh token valido para un usuario cuyo status cambio a "locked"
  When envio POST /auth/refresh
  Then recibo HTTP 403 con codigo AUTH_ACCOUNT_LOCKED
  And el refresh token se revoca

Scenario: Permisos actualizados se reflejan en nuevo JWT
  Given un usuario con rol "operador" que fue ascendido a "admin" por un Super Admin
  And el usuario tiene un refresh token valido
  When envio POST /auth/refresh
  Then el nuevo access_token contiene los permisos del rol "admin"
```

#### Trazabilidad de Pruebas

| Test ID | Tipo | Descripcion |
|---------|------|-------------|
| TP-SEC-03-01 | Positivo | Refresh exitoso retorna nuevo JWT y nuevo refresh token |
| TP-SEC-03-02 | Positivo | Token anterior queda revocado tras refresh exitoso |
| TP-SEC-03-03 | Positivo | Nuevo JWT refleja permisos actualizados (D-SEC-03) |
| TP-SEC-03-04 | Negativo | Token expirado retorna 401 |
| TP-SEC-03-05 | Negativo | Token inexistente retorna 401 |
| TP-SEC-03-06 | Negativo | Reuso de token revocado revoca toda la cadena del usuario |
| TP-SEC-03-07 | Negativo | Refresh a cuenta locked retorna 403 |
| TP-SEC-03-08 | Negativo | Refresh a cuenta inactive retorna 403 |
| TP-SEC-03-09 | Concurrencia | Dos refreshes simultaneos con mismo token: uno exitoso, otro detecta reuso |
| TP-SEC-03-10 | Seguridad | Token se almacena como SHA-256, no en texto plano |

#### Sin Ambiguedades

- **Supuestos prohibidos:** No se asume que el SPA maneja automaticamente la serializacion de refreshes concurrentes. No se asume que el refresh token tiene duracion infinita. No se asume que el refresh puede hacerse sin tenant_id.
- **Decisiones cerradas:** D-SEC-02 (rotacion estricta one-time-use, reuso revoca toda la cadena). D-SEC-03 (permisos recargados al refresh). TTL default del refresh token: 7 dias (configurable).
- **Fuera de alcance explicito:** Refresh token en cookie HttpOnly (el SPA lo maneja en memoria), refresh silencioso con iframe, token binding a dispositivo.
- **TODO explicitos = 0**

---

### RF-SEC-04 — Recuperacion de contrasena

#### Execution Sheet

| Campo | Valor |
|-------|-------|
| **ID** | RF-SEC-04 |
| **Titulo** | Recuperacion de contrasena via email con enlace temporal |
| **Actor(es)** | Usuario no autenticado |
| **Prioridad** | Alta |
| **Severidad** | P1 |
| **Flujo origen** | FL-SEC-01 seccion 7b |
| **HU origen** | HU038 |

#### Precondiciones

| # | Condicion | Detalle |
|---|-----------|---------|
| 1 | Identity Service operativo | SA.identity accesible |
| 2 | Notification Service operativo | Para envio de email con enlace de recuperacion |
| 3 | RabbitMQ operativo | Para publicacion de `SendPasswordResetEvent` |
| 4 | Plantilla de email de recuperacion configurada | En Notification Service |
| 5 | Parametro de mascara de password accesible | `mask.password` disponible en Config Service o cache |
| 6 | Parametro de TTL de reset token configurado | Default: 30 minutos |

#### Entradas

**Paso 1 — Solicitud de recuperacion:**

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| `email` | string | Si | Body JSON | Formato email valido (RFC 5322 basico). Trim y lowercase. | RN-SEC-05 |
| `tenant_id` | uuid | Si | Header `X-Tenant-Id` o subdominio | UUID v4 valido | — |

**Paso 2 — Reset de contrasena:**

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| `token` | string | Si | Body JSON (extraido del enlace) | No vacio, formato opaco valido | RN-SEC-12 |
| `new_password` | string | Si | Body JSON | Cumple regex de `mask.password` (fallback: `^.{8,}$`) | RN-SEC-07 |
| `tenant_id` | uuid | Si | Header `X-Tenant-Id` o subdominio | UUID v4 valido | — |

#### Proceso (Happy Path)

**Paso 1 — Solicitud (`POST /auth/forgot-password`):**

| # | Paso | Responsable |
|---|------|-------------|
| 1 | Recibir `POST /auth/forgot-password` con `email` | API Gateway |
| 2 | Resolver `tenant_id` y activar RLS | Identity Service |
| 3 | Buscar usuario en `users` por `email` dentro del tenant (indice `users(tenant_id, email)`) | Identity Service |
| 4 | Si el usuario no existe: NO retornar error, continuar al paso 8 (RN-SEC-05) | Identity Service |
| 5 | Si el usuario existe: generar token aleatorio (256 bits) con TTL configurable (default 30 min) | Identity Service |
| 6 | Persistir hash del token asociado al `user_id`, con `expires_at` y `used = false` | Identity Service |
| 7 | Publicar `SendPasswordResetEvent` con `{ email, reset_link, user_name }` a RabbitMQ | Identity Service |
| 8 | Retornar `200 OK` con `{ message: "Si el email esta registrado, recibira instrucciones." }` | Identity Service |

**Paso 2 — Reset (`POST /auth/reset-password`):**

| # | Paso | Responsable |
|---|------|-------------|
| 1 | Recibir `POST /auth/reset-password` con `token` y `new_password` | API Gateway |
| 2 | Calcular hash del `token` y buscar en storage de tokens de reset | Identity Service |
| 3 | Verificar que el token no ha expirado (`expires_at > NOW()`) | Identity Service |
| 4 | Verificar que el token no ha sido usado (`used = false`) | Identity Service |
| 5 | Validar `new_password` contra `mask.password` del Config Service | Identity Service |
| 6 | Generar nuevo `password_hash` con bcrypt | Identity Service |
| 7 | Ejecutar `UPDATE users SET password_hash = :hash, updated_at = NOW() WHERE id = :user_id` | Identity Service |
| 8 | Resetear `users.failed_attempts = 0` | Identity Service |
| 9 | Marcar token de reset como usado (`used = true`) | Identity Service |
| 10 | Revocar TODOS los refresh tokens del usuario: `UPDATE refresh_tokens SET revoked = true WHERE user_id = :user_id AND revoked = false` | Identity Service |
| 11 | Publicar `PasswordChangedEvent` con `{ user_id, tenant_id, timestamp }` a RabbitMQ | Identity Service |
| 12 | Retornar `200 OK` con `{ message: "Contrasena actualizada exitosamente." }` | Identity Service |

#### Salidas

**Paso 1:**

| Campo | Tipo | Destino | Efecto observable |
|-------|------|---------|-------------------|
| `message` | string | Response body → SPA | Mensaje generico independiente de si el email existe |
| `SendPasswordResetEvent` | evento async | RabbitMQ → Notification Service | Email con enlace de recuperacion (solo si el email existe) |

**Paso 2:**

| Campo | Tipo | Destino | Efecto observable |
|-------|------|---------|-------------------|
| `message` | string | Response body → SPA | Confirmacion de cambio exitoso |
| `users.password_hash` | string | SA.identity | Actualizado con nuevo hash bcrypt |
| `refresh_tokens` | UPDATE | SA.identity | Todos los tokens del usuario revocados (fuerza re-login) |
| `PasswordChangedEvent` | evento async | RabbitMQ → Audit Service | Registro en `audit_log` con operation `Cambiar Contrasena` |

#### Errores Tipados

| Codigo | Causa | Condicion de disparo | Respuesta esperada |
|--------|-------|---------------------|--------------------|
| `RESET_TOKEN_INVALID` | Token de reset invalido | Hash del token no encontrado en storage | HTTP 400, mensaje: "Enlace invalido o expirado." |
| `RESET_TOKEN_EXPIRED` | Token de reset expirado | `expires_at <= NOW()` | HTTP 400, mensaje: "Enlace invalido o expirado." |
| `RESET_TOKEN_USED` | Token ya utilizado | `used = true` | HTTP 400, mensaje: "Enlace invalido o expirado." |
| `RESET_PASSWORD_INVALID` | Contrasena no cumple politica | `new_password` no pasa validacion de `mask.password` | HTTP 422, mensaje: "La contrasena no cumple los requisitos de seguridad." con detalle de la politica. |
| `AUTH_VALIDATION_ERROR` | Email malformado (paso 1) | `email` no cumple formato basico | HTTP 422, mensaje: detalle del campo invalido |

#### Casos Especiales y Variantes

- **Respuesta opaca (RN-SEC-05):** El endpoint `POST /auth/forgot-password` SIEMPRE retorna 200, incluso si el email no existe en el sistema. Esto evita que un atacante enumere emails validos.
- **Mensaje unificado:** Los errores `RESET_TOKEN_INVALID`, `RESET_TOKEN_EXPIRED` y `RESET_TOKEN_USED` retornan el mismo mensaje al cliente para no revelar el motivo exacto.
- **Multiples solicitudes de reset:** Si un usuario solicita reset multiples veces, cada solicitud genera un token nuevo. Los tokens anteriores no se invalidan explicitamente (expiran por TTL), pero solo el mas reciente es practicable dado que el enlace se envia al ultimo email.
- **Revocacion de sesiones activas:** Al cambiar la contrasena, se revocan TODOS los refresh tokens del usuario. Esto fuerza re-login en todos los dispositivos/pestanas.
- **Usuario bloqueado o inactivo:** Si el usuario tiene status `locked` o `inactive`, se genera y envia el enlace igualmente. Al completar el reset, la contrasena se actualiza pero el usuario debera ser desbloqueado/activado por un admin para poder iniciar sesion.
- **Tiempo de respuesta constante (paso 1):** Si el email no existe, se ejecuta un delay artificial equivalente al tiempo de generacion y publicacion para evitar timing attacks.

#### Impacto en Modelo de Datos

| Entidad | Operacion | Campo(s) afectado(s) | Evento |
|---------|-----------|---------------------|--------|
| `users` | SELECT | `email`, `id`, `status` | — |
| `users` | UPDATE | `password_hash`, `updated_at` | — |
| Reset token store (Redis o tabla temporal) | INSERT | `token_hash`, `user_id`, `expires_at`, `used` | — |
| Reset token store | SELECT, UPDATE | `token_hash`, `used`, `expires_at` | — |
| `refresh_tokens` | UPDATE | `revoked = true` (todos del usuario) | — |
| `audit_log` (SA.audit) | INSERT (async) | `user_id`, `operation = 'Cambiar Contrasena'` | `PasswordChangedEvent` |
| `notification_log` (SA.notification) | INSERT (async) | `channel = 'email'`, `recipient`, `template_title` | `SendPasswordResetEvent` |

#### Criterios de Aceptacion (Gherkin)

```gherkin
Scenario: Solicitud de recuperacion con email existente
  Given un usuario con email "juan@empresa.com" y status "active" en el tenant
  When envio POST /auth/forgot-password con email "juan@empresa.com"
  Then recibo HTTP 200 con mensaje "Si el email esta registrado, recibira instrucciones."
  And se publica SendPasswordResetEvent con enlace de recuperacion

Scenario: Solicitud de recuperacion con email inexistente (respuesta opaca)
  Given que no existe usuario con email "noexiste@empresa.com" en el tenant
  When envio POST /auth/forgot-password con email "noexiste@empresa.com"
  Then recibo HTTP 200 con mensaje "Si el email esta registrado, recibira instrucciones."
  And NO se publica SendPasswordResetEvent

Scenario: Reset de contrasena exitoso
  Given un token de reset valido, no expirado y no usado para el usuario "juan"
  And la nueva contrasena "NuevaPass2026!" cumple mask.password
  When envio POST /auth/reset-password con token y new_password
  Then recibo HTTP 200 con mensaje "Contrasena actualizada exitosamente."
  And users.password_hash se actualiza con el nuevo hash bcrypt
  And TODOS los refresh tokens del usuario quedan revocados
  And se publica PasswordChangedEvent

Scenario: Reset con token expirado
  Given un token de reset cuyo expires_at ya paso
  When envio POST /auth/reset-password con token y new_password valida
  Then recibo HTTP 400 con mensaje "Enlace invalido o expirado."

Scenario: Reset con contrasena que no cumple politica
  Given un token de reset valido
  And la nueva contrasena "123" no cumple mask.password
  When envio POST /auth/reset-password con token y new_password "123"
  Then recibo HTTP 422 con codigo RESET_PASSWORD_INVALID
```

#### Trazabilidad de Pruebas

| Test ID | Tipo | Descripcion |
|---------|------|-------------|
| TP-SEC-04-01 | Positivo | Solicitud de recuperacion con email existente publica evento y retorna 200 |
| TP-SEC-04-02 | Positivo | Solicitud de recuperacion con email inexistente retorna 200 sin publicar evento |
| TP-SEC-04-03 | Positivo | Reset exitoso actualiza password_hash y revoca refresh tokens |
| TP-SEC-04-04 | Positivo | Reset exitoso publica PasswordChangedEvent |
| TP-SEC-04-05 | Negativo | Token expirado retorna 400 |
| TP-SEC-04-06 | Negativo | Token ya usado retorna 400 |
| TP-SEC-04-07 | Negativo | Token inexistente retorna 400 |
| TP-SEC-04-08 | Negativo | Contrasena que no cumple mascara retorna 422 |
| TP-SEC-04-09 | Negativo | Email con formato invalido retorna 422 |
| TP-SEC-04-10 | Seguridad | Tiempo de respuesta constante en forgot-password (email existente vs inexistente) |
| TP-SEC-04-11 | Seguridad | Mensajes de error unificados no revelan motivo exacto del rechazo del token |
| TP-SEC-04-12 | Integracion | Flujo completo: forgot-password → email → reset-password → login exitoso con nueva contrasena |

#### Sin Ambiguedades

- **Supuestos prohibidos:** No se asume que el enlace de recuperacion contiene el token en texto plano en la URL (se usa un token opaco mapeado server-side). No se asume que la contrasena anterior se valida antes del cambio. No se asume que el usuario debe estar logueado para recuperar contrasena.
- **Decisiones cerradas:** TTL default del token de reset: 30 minutos (configurable). La contrasena se hashea con bcrypt. Al resetear, se revocan todos los refresh tokens. Respuesta opaca en forgot-password (RN-SEC-05). Al completar reset-password, `failed_attempts` se resetea a 0. Si el usuario estaba locked, la contrasena se actualiza pero `status = locked` permanece — el desbloqueo es manual por Super Admin.
- **Fuera de alcance explicito:** Cambio de contrasena estando logueado (seria otro RF), preguntas secretas, verificacion por SMS, link magic (passwordless).
- **TODO explicitos = 0**

---

### RF-SEC-05 — Logout con revocacion

#### Execution Sheet

| Campo | Valor |
|-------|-------|
| **ID** | RF-SEC-05 |
| **Titulo** | Cierre de sesion con revocacion de refresh token |
| **Actor(es)** | Usuario autenticado (cualquier rol) |
| **Prioridad** | Alta |
| **Severidad** | P1 |
| **Flujo origen** | FL-SEC-01 seccion 7c |
| **HU origen** | HU038 |

#### Precondiciones

| # | Condicion | Detalle |
|---|-----------|---------|
| 1 | Sesion activa | El usuario esta autenticado y posee un refresh token valido |
| 2 | JWT valido en header Authorization | Necesario para identificar al usuario |

#### Entradas

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| `refresh_token` | string | Si | Body JSON | No vacio; debe existir en `refresh_tokens` | — |
| `Authorization` | string | Si | Header HTTP | JWT Bearer valido (puede estar expirado pero debe ser decodificable para obtener `user_id`) | — |
| `tenant_id` | uuid | Si | Header `X-Tenant-Id` o subdominio | UUID v4 valido | — |

#### Proceso (Happy Path)

| # | Paso | Responsable |
|---|------|-------------|
| 1 | Recibir `POST /auth/logout` con `refresh_token` en body | API Gateway |
| 2 | Extraer `user_id` y `tenant_id` del JWT en header Authorization | Identity Service |
| 3 | Activar RLS con `tenant_id` | Identity Service |
| 4 | Calcular SHA-256 del `refresh_token` recibido | Identity Service |
| 5 | Buscar token en `refresh_tokens` por `token_hash` | Identity Service |
| 6 | Verificar que el token pertenece al `user_id` del JWT | Identity Service |
| 7 | Marcar token como revocado: `UPDATE refresh_tokens SET revoked = true WHERE id = :id` | Identity Service |
| 8 | Publicar `UserLoggedOutEvent` con `{ user_id, tenant_id, timestamp, ip_address }` a RabbitMQ | Identity Service |
| 9 | Retornar `200 OK` con `{ message: "Sesion cerrada exitosamente." }` | Identity Service |
| 10 | SPA limpia access_token y refresh_token del estado local | SPA (cliente) |
| 11 | SPA redirige a `/login` | SPA (cliente) |

#### Salidas

| Campo | Tipo | Destino | Efecto observable |
|-------|------|---------|-------------------|
| `message` | string | Response body → SPA | Confirmacion de logout |
| `refresh_tokens.revoked` | boolean | SA.identity | Token marcado como `true` |
| `UserLoggedOutEvent` | evento async | RabbitMQ → Audit Service | Registro en `audit_log` con operation `Logout` |
| Estado local del SPA | limpieza | SPA | access_token y refresh_token eliminados de memoria |

#### Errores Tipados

| Codigo | Causa | Condicion de disparo | Respuesta esperada |
|--------|-------|---------------------|--------------------|
| `AUTH_TOKEN_NOT_FOUND` | Refresh token no existe | SHA-256 del token no encontrado en `refresh_tokens` | HTTP 200 (logout es idempotente; se considera exito) |
| `AUTH_TOKEN_MISMATCH` | Token no pertenece al usuario | `refresh_tokens.user_id` no coincide con `user_id` del JWT | HTTP 403, mensaje: "Operacion no autorizada." |
| `AUTH_VALIDATION_ERROR` | Entrada malformada | `refresh_token` vacio o ausente | HTTP 422, mensaje: detalle del campo invalido |

#### Casos Especiales y Variantes

- **Idempotencia:** Si el refresh token ya esta revocado o no existe, el endpoint retorna 200 igualmente. El logout es una operacion idempotente; no es un error cerrar una sesion que ya fue cerrada.
- **JWT expirado en logout:** El endpoint acepta JWTs expirados en el header Authorization para el caso en que el usuario intente hacer logout despues de que su JWT expiro. Se decodifica sin verificar expiracion, pero se verifica la firma.
- **Logout no revoca el JWT activo:** El JWT sigue siendo valido hasta su expiracion natural (15 min). El SPA elimina el token localmente, pero si un atacante capturo el JWT, este permanece utilizable hasta que expire. Esto es una limitacion aceptada del modelo JWT stateless.
- **Logout de todos los dispositivos:** Este RF solo revoca el refresh token enviado. Un "logout de todos los dispositivos" seria un RF separado que revoque todos los refresh tokens del usuario (similar a lo que ocurre en RF-SEC-03 ante reuso o en RF-SEC-04 al resetear contrasena).

#### Impacto en Modelo de Datos

| Entidad | Operacion | Campo(s) afectado(s) | Evento |
|---------|-----------|---------------------|--------|
| `refresh_tokens` | SELECT | `token_hash`, `user_id`, `revoked` | — |
| `refresh_tokens` | UPDATE | `revoked = true` | — |
| `audit_log` (SA.audit) | INSERT (async) | `user_id`, `operation = 'Logout'`, `ip_address` | `UserLoggedOutEvent` |

#### Criterios de Aceptacion (Gherkin)

```gherkin
Scenario: Logout exitoso revoca refresh token
  Given un usuario autenticado con refresh token valido "rt-current"
  When envio POST /auth/logout con refresh_token "rt-current"
  Then recibo HTTP 200 con mensaje "Sesion cerrada exitosamente."
  And el refresh token "rt-current" queda con revoked = true en refresh_tokens
  And se publica UserLoggedOutEvent

Scenario: Logout con token ya revocado es idempotente
  Given un refresh token "rt-old" que ya esta revocado
  When envio POST /auth/logout con refresh_token "rt-old"
  Then recibo HTTP 200 con mensaje "Sesion cerrada exitosamente."

Scenario: Logout con token inexistente es idempotente
  Given un refresh token "rt-fake" que no existe en la base de datos
  When envio POST /auth/logout con refresh_token "rt-fake"
  Then recibo HTTP 200

Scenario: Logout con token de otro usuario es rechazado
  Given un refresh token "rt-other" que pertenece al usuario "maria"
  And el JWT en el header corresponde al usuario "juan"
  When envio POST /auth/logout con refresh_token "rt-other"
  Then recibo HTTP 403 con codigo AUTH_TOKEN_MISMATCH

Scenario: SPA limpia estado local tras logout
  Given un logout exitoso
  When el SPA recibe la respuesta 200
  Then el access_token y refresh_token se eliminan del estado local
  And el SPA redirige a /login
```

#### Trazabilidad de Pruebas

| Test ID | Tipo | Descripcion |
|---------|------|-------------|
| TP-SEC-05-01 | Positivo | Logout exitoso revoca refresh token y retorna 200 |
| TP-SEC-05-02 | Positivo | Logout exitoso publica `UserLoggedOutEvent` |
| TP-SEC-05-03 | Positivo | Logout con token ya revocado retorna 200 (idempotente) |
| TP-SEC-05-04 | Positivo | Logout con token inexistente retorna 200 (idempotente) |
| TP-SEC-05-05 | Negativo | Logout con token de otro usuario retorna 403 |
| TP-SEC-05-06 | Negativo | Logout sin refresh_token en body retorna 422 |
| TP-SEC-05-07 | Seguridad | Logout con JWT expirado pero firma valida es aceptado |
| TP-SEC-05-08 | Seguridad | Logout con JWT de firma invalida es rechazado |
| TP-SEC-05-09 | Integracion | Tras logout, intento de refresh con el token revocado retorna 401 |
| TP-SEC-05-10 | E2E | Flujo completo: login → uso → logout → redireccion a /login |

#### Sin Ambiguedades

- **Supuestos prohibidos:** No se asume que el logout invalida el JWT activo (es stateless). No se asume que el logout cierra sesiones en otros dispositivos. No se asume que el SPA envia el refresh token en cookie (se envia en body).
- **Decisiones cerradas:** Logout es idempotente (siempre retorna 200). Solo se revoca el refresh token enviado, no todos. Se acepta JWT expirado en el header para permitir logout tardio.
- **Fuera de alcance explicito:** Logout de todos los dispositivos, blacklist de JWTs en Redis, logout automatico por inactividad (delegado al SPA por expiracion de JWT).
- **TODO explicitos = 0**

---

### RF-SEC-06 — Listar usuarios con filtros y paginacion

#### Execution Sheet

| Campo | Valor |
|-------|-------|
| **ID** | RF-SEC-06 |
| **Titulo** | Listar usuarios con filtros, busqueda y paginacion |
| **Actor(es)** | Super Admin, Admin Entidad |
| **Prioridad** | Alta |
| **Severidad** | P1 |
| **Flujo origen** | FL-SEC-01 seccion 8 |
| **HU origen** | HU023 |

#### Precondiciones

| # | Condicion | Detalle |
|---|-----------|---------|
| 1 | Usuario autenticado con permiso `p_users_list` | JWT valido con `p_users_list` en `permissions[]` |
| 2 | Identity Service operativo | SA.identity accesible y saludable |
| 3 | RLS activo con `tenant_id` | El contexto de tenant se establece desde el JWT |
| 4 | Tabla `users` inicializada | Al menos el usuario seed existe |

#### Entradas

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| `tenant_id` | uuid | Si | Header `X-Tenant-Id` o JWT claim | UUID v4 valido; debe coincidir con JWT | — |
| `page` | integer | No | Query param | >= 1; default: 1 | — |
| `page_size` | integer | No | Query param | 1–100; default: 20 | — |
| `search` | string | No | Query param | Max 100 caracteres; trim de espacios; busca por `username` o `email` con `ILIKE '%search%'` | — |
| `status` | string | No | Query param | Enum: `active`, `inactive`, `locked`; si no se envia, retorna todos | — |
| `entity_id` | uuid | No | Query param | UUID v4 valido; filtra por `users.entity_id` | — |
| `role_id` | uuid | No | Query param | UUID v4 valido; filtra usuarios que tengan el rol en `user_roles` | — |
| `sort_by` | string | No | Query param | Enum: `username`, `email`, `created_at`, `last_login`; default: `created_at` | — |
| `sort_dir` | string | No | Query param | Enum: `asc`, `desc`; default: `desc` | — |
| `export` | string | No | Query param | Enum: `csv`, `xlsx`; si se envia, retorna archivo en vez de JSON | — |

#### Proceso (Happy Path)

| # | Paso | Responsable |
|---|------|-------------|
| 1 | Recibir `GET /users` con query params opcionales | API Gateway |
| 2 | Validar JWT y extraer `tenant_id`, `permissions[]`, `entity_id` | API Gateway (YARP) |
| 3 | Verificar que `p_users_list` esta presente en `permissions[]` del JWT | Identity Service |
| 4 | Activar RLS con `tenant_id` | Identity Service |
| 5 | Si el actor es Admin Entidad (no Super Admin), agregar filtro implicito `users.entity_id = :jwt_entity_id` para restringir resultados a su entidad | Identity Service |
| 6 | Construir query base: `SELECT u.id, u.username, u.email, u.first_name, u.last_name, u.entity_name, u.status, u.last_login, u.created_at FROM users u WHERE u.tenant_id = :tenant_id` | Identity Service |
| 7 | Aplicar filtro `search`: si presente, agregar `AND (u.username ILIKE '%:search%' OR u.email ILIKE '%:search%')` con parametro sanitizado | Identity Service |
| 8 | Aplicar filtro `status`: si presente, agregar `AND u.status = :status` | Identity Service |
| 9 | Aplicar filtro `entity_id`: si presente, agregar `AND u.entity_id = :entity_id` | Identity Service |
| 10 | Aplicar filtro `role_id`: si presente, agregar `AND EXISTS (SELECT 1 FROM user_roles ur WHERE ur.user_id = u.id AND ur.role_id = :role_id)` | Identity Service |
| 11 | Ejecutar `COUNT(*)` sobre la query filtrada para obtener `total_count` | Identity Service |
| 12 | Aplicar `ORDER BY :sort_by :sort_dir` y `LIMIT :page_size OFFSET (:page - 1) * :page_size` | Identity Service |
| 13 | Para cada usuario en el resultado, obtener roles: `SELECT r.name FROM roles r JOIN user_roles ur ON ur.role_id = r.id WHERE ur.user_id = :user_id` | Identity Service |
| 14 | Si `export` esta presente, generar archivo CSV o XLSX con todas las filas (sin paginacion, maximo 10.000 filas) y retornar como descarga | Identity Service |
| 15 | Si no hay `export`, retornar respuesta paginada JSON | Identity Service |

#### Salidas

| Campo | Tipo | Destino | Efecto observable |
|-------|------|---------|-------------------|
| `data` | array de objetos | Response body → SPA | Lista de usuarios con campos: `id`, `username`, `email`, `first_name`, `last_name`, `entity_name`, `roles[]` (array de nombres), `status`, `last_login`, `created_at` |
| `pagination.page` | integer | Response body → SPA | Pagina actual |
| `pagination.page_size` | integer | Response body → SPA | Tamano de pagina actual |
| `pagination.total_count` | integer | Response body → SPA | Total de registros que coinciden con los filtros |
| `pagination.total_pages` | integer | Response body → SPA | Calculado: `ceil(total_count / page_size)` |
| Archivo CSV/XLSX | binary | Response como descarga | Solo si query param `export` fue enviado; Content-Type: `text/csv` o `application/vnd.openxmlformats-officedocument.spreadsheetml.sheet` |

> **Nota de estandarizacion:** RF-SEC-06 usa formato `{ data[], pagination { page, page_size, total_count, total_pages } }` mientras que RF-AUD-01 y otros modulos usan `{ items[], total, page, page_size }`. Unificar en fase de implementacion adoptando un unico contrato de paginacion en el API Gateway o en un paquete compartido. El formato canonico recomendado es `{ items[], total, page, page_size }` (mas simple, alineado con la mayoria de RFs).

#### Errores Tipados

| Codigo | Causa | Condicion de disparo | Respuesta esperada |
|--------|-------|---------------------|--------------------|
| `AUTH_FORBIDDEN` | Sin permiso | `p_users_list` no presente en JWT `permissions[]` | HTTP 403, mensaje: "No tiene permisos para listar usuarios." |
| `AUTH_UNAUTHORIZED` | JWT ausente o invalido | Header Authorization ausente, expirado o firma invalida | HTTP 401, mensaje: "Token de autenticacion invalido o expirado." |
| `VALIDATION_ERROR` | Parametro invalido | `page` < 1, `page_size` fuera de rango 1–100, `status` no es enum valido, `sort_by` no es campo permitido | HTTP 422, mensaje: detalle del campo invalido |
| `USERS_EXPORT_LIMIT` | Limite de exportacion superado | El resultado filtrado supera 10.000 filas al intentar exportar | HTTP 422, mensaje: "El resultado supera el limite de 10.000 registros para exportacion. Aplique filtros adicionales." |

#### Casos Especiales y Variantes

- **Admin Entidad:** Cuando el actor tiene rol Admin Entidad (no Super Admin), el sistema agrega automaticamente el filtro `users.entity_id = :jwt_entity_id`. Esto ocurre a nivel de servicio, ademas del RLS por `tenant_id`. El Admin Entidad no puede ver usuarios de otras entidades aunque envie `entity_id` de otra entidad en el query param.
- **Resultado vacio:** Si no hay usuarios que coincidan con los filtros, se retorna `data: []` con `pagination.total_count: 0`. No es un error.
- **Exportacion sin paginacion:** Cuando `export` esta presente, se ignoran `page` y `page_size` y se retornan todas las filas hasta el limite de 10.000.
- **Busqueda case-insensitive:** El filtro `search` usa `ILIKE` de PostgreSQL para busqueda sin distincion de mayusculas/minusculas.
- **Roles en respuesta:** Los roles de cada usuario se incluyen como array de nombres (no IDs) para facilitar la visualizacion en el DataTable sin consultas adicionales del SPA.
- **Columna last_login:** Puede ser `null` si el usuario nunca inicio sesion.

#### Impacto en Modelo de Datos

| Entidad | Operacion | Campo(s) afectado(s) | Evento |
|---------|-----------|---------------------|--------|
| `users` | SELECT | `id`, `username`, `email`, `first_name`, `last_name`, `entity_name`, `entity_id`, `status`, `last_login`, `created_at` | — |
| `user_roles` | SELECT (JOIN) | `user_id`, `role_id` | — |
| `roles` | SELECT (JOIN) | `id`, `name` | — |

#### Criterios de Aceptacion (Gherkin)

```gherkin
Scenario: Listar usuarios exitosamente con paginacion
  Given un usuario autenticado con permiso "p_users_list"
  And existen 25 usuarios en el tenant actual
  When envio GET /users?page=1&page_size=10
  Then recibo HTTP 200 con data conteniendo 10 usuarios
  And pagination.total_count = 25
  And pagination.total_pages = 3

Scenario: Buscar usuarios por username
  Given un usuario autenticado con permiso "p_users_list"
  And existe un usuario con username "jperez"
  When envio GET /users?search=jperez
  Then recibo HTTP 200 con data conteniendo al usuario "jperez"

Scenario: Filtrar usuarios por estado
  Given un usuario autenticado con permiso "p_users_list"
  And existen 3 usuarios activos y 2 inactivos
  When envio GET /users?status=active
  Then recibo HTTP 200 con data conteniendo exactamente 3 usuarios
  And todos tienen status = "active"

Scenario: Filtrar usuarios por rol
  Given un usuario autenticado con permiso "p_users_list"
  And existen 2 usuarios con rol "Operador"
  When envio GET /users?role_id=<id-rol-operador>
  Then recibo HTTP 200 con data conteniendo exactamente 2 usuarios

Scenario: Admin Entidad solo ve usuarios de su entidad
  Given un Admin Entidad de entidad "Entidad-A" con permiso "p_users_list"
  And existen 5 usuarios en "Entidad-A" y 3 en "Entidad-B"
  When envio GET /users
  Then recibo HTTP 200 con data conteniendo exactamente 5 usuarios
  And todos tienen entity_id correspondiente a "Entidad-A"

Scenario: Admin Entidad no puede ver usuarios de otra entidad
  Given un Admin Entidad de entidad "Entidad-A" con permiso "p_users_list"
  When envio GET /users?entity_id=<id-entidad-B>
  Then recibo HTTP 200 con data conteniendo exactamente 5 usuarios de "Entidad-A"
  And el filtro entity_id de otra entidad es ignorado

Scenario: Exportar usuarios a CSV
  Given un usuario autenticado con permiso "p_users_list"
  And existen 50 usuarios en el tenant
  When envio GET /users?export=csv
  Then recibo HTTP 200 con Content-Type "text/csv"
  And el archivo contiene 50 filas de datos mas cabecera

Scenario: Exportar con mas de 10.000 registros es rechazado
  Given un usuario autenticado con permiso "p_users_list"
  And existen 10.500 usuarios en el tenant
  When envio GET /users?export=xlsx
  Then recibo HTTP 422 con codigo USERS_EXPORT_LIMIT

Scenario: Sin permiso retorna 403
  Given un usuario autenticado sin permiso "p_users_list"
  When envio GET /users
  Then recibo HTTP 403 con codigo AUTH_FORBIDDEN

Scenario: Resultado vacio retorna lista vacia
  Given un usuario autenticado con permiso "p_users_list"
  And no existen usuarios que coincidan con el filtro
  When envio GET /users?search=inexistente
  Then recibo HTTP 200 con data = [] y pagination.total_count = 0
```

#### Trazabilidad de Pruebas

| Test ID | Tipo | Descripcion |
|---------|------|-------------|
| TP-SEC-06-01 | Positivo | Listar usuarios con paginacion por defecto retorna pagina 1 con 20 registros |
| TP-SEC-06-02 | Positivo | Listar usuarios con page=2 y page_size=10 retorna offset correcto |
| TP-SEC-06-03 | Positivo | Busqueda por username con `search` filtra correctamente con ILIKE |
| TP-SEC-06-04 | Positivo | Busqueda por email con `search` filtra correctamente con ILIKE |
| TP-SEC-06-05 | Positivo | Filtro por `status=active` retorna solo usuarios activos |
| TP-SEC-06-06 | Positivo | Filtro por `status=locked` retorna solo usuarios bloqueados |
| TP-SEC-06-07 | Positivo | Filtro por `role_id` retorna solo usuarios con ese rol |
| TP-SEC-06-08 | Positivo | Filtro por `entity_id` retorna solo usuarios de esa entidad |
| TP-SEC-06-09 | Positivo | Ordenamiento por `username asc` retorna orden correcto |
| TP-SEC-06-10 | Positivo | Exportacion CSV genera archivo valido con todas las filas |
| TP-SEC-06-11 | Positivo | Exportacion XLSX genera archivo valido con todas las filas |
| TP-SEC-06-12 | Positivo | Resultado vacio retorna data=[] con total_count=0 |
| TP-SEC-06-13 | Negativo | Sin permiso `p_users_list` retorna 403 |
| TP-SEC-06-14 | Negativo | JWT ausente retorna 401 |
| TP-SEC-06-15 | Negativo | Parametro `page` invalido (0, -1, texto) retorna 422 |
| TP-SEC-06-16 | Negativo | Parametro `status` con valor no permitido retorna 422 |
| TP-SEC-06-17 | Negativo | Exportacion con mas de 10.000 registros retorna 422 |
| TP-SEC-06-18 | Seguridad | Admin Entidad solo ve usuarios de su propia entidad |
| TP-SEC-06-19 | Seguridad | Admin Entidad no puede forzar `entity_id` de otra entidad |
| TP-SEC-06-20 | Seguridad | RLS impide acceso a usuarios de otro tenant |
| TP-SEC-06-21 | Seguridad | Parametro `search` es sanitizado contra SQL injection |
| TP-SEC-06-22 | Integracion | Roles de cada usuario se resuelven correctamente via JOIN |
| TP-SEC-06-23 | E2E | Flujo completo: login → navegar a listado → buscar → filtrar → exportar CSV |

#### Sin Ambiguedades

- **Supuestos prohibidos:** No se asume que el Admin Entidad puede ver usuarios de otras entidades. No se asume que la exportacion no tiene limite de filas. No se asume que el campo `search` busca en campos distintos a `username` y `email`.
- **Decisiones cerradas:** Busqueda es case-insensitive con `ILIKE`. Limite de exportacion es 10.000 filas. Admin Entidad tiene filtro implicito por `entity_id` del JWT ademas del RLS por `tenant_id`. Roles se retornan como array de nombres, no de IDs.
- **Fuera de alcance explicito:** Filtros avanzados por fecha de creacion, filtro por multiples roles simultaneos, exportacion en PDF, busqueda full-text con ranking.
- **TODO explicitos = 0**

---

### RF-SEC-07 — Crear/editar usuario con roles y asignaciones

#### Execution Sheet

| Campo | Valor |
|-------|-------|
| **ID** | RF-SEC-07 |
| **Titulo** | Crear o editar usuario con roles y asignaciones de alcance |
| **Actor(es)** | Super Admin, Admin Entidad |
| **Prioridad** | Alta |
| **Severidad** | P0 |
| **Flujo origen** | FL-SEC-01 seccion 8 |
| **HU origen** | HU024 |

#### Precondiciones

| # | Condicion | Detalle |
|---|-----------|---------|
| 1 | Usuario autenticado con permiso `p_users_create` (creacion) o `p_users_edit` (edicion) | JWT valido con el permiso correspondiente en `permissions[]` |
| 2 | Identity Service operativo | SA.identity accesible y saludable |
| 3 | RLS activo con `tenant_id` | El contexto de tenant se establece desde el JWT |
| 4 | Config Service accesible | Para obtener `mask.username` y `mask.password` (con fallback en cache Redis) |
| 5 | Tabla `roles` inicializada | Al menos los roles seed existen |
| 6 | RabbitMQ operativo | Para publicacion de eventos asincrona |

#### Entradas

**Creacion (`POST /users`):**

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| `tenant_id` | uuid | Si | Header `X-Tenant-Id` o JWT claim | UUID v4 valido; debe coincidir con JWT | — |
| `username` | string | Si | Body JSON | Regex de `mask.username` (fallback: `^[a-zA-Z0-9._-]{3,30}$`). Trim de espacios. Unico por `tenant_id`. | RN-SEC-06, RN-SEC-07 |
| `password` | string | Si | Body JSON | Regex de `mask.password` (fallback: `^.{8,}$`). Solo requerido en creacion. | RN-SEC-07 |
| `email` | string | Si | Body JSON | Formato email valido (RFC 5322 simplificado). Max 255 caracteres. Trim. Unico por `tenant_id`. | RN-SEC-06 |
| `first_name` | string | Si | Body JSON | Min 1, max 100 caracteres. Trim. | — |
| `last_name` | string | Si | Body JSON | Min 1, max 100 caracteres. Trim. | — |
| `entity_id` | uuid / null | No | Body JSON | UUID v4 valido o `null`. Si es `null`, el usuario es de plataforma ("_platform"). Si se proporciona, debe existir como entidad activa en el tenant. | — |
| `entity_name` | string / null | No | Body JSON | Requerido si `entity_id` no es null. Max 200 caracteres. Nombre desnormalizado de la entidad. | — |
| `status` | string | Si | Body JSON | Enum: `active`, `inactive`. No se puede crear con `locked` (solo el sistema bloquea). | — |
| `role_ids` | array de uuid | Si | Body JSON | Min 1 rol. Cada UUID debe existir en `roles` dentro del tenant. | — |
| `assignments` | array de objetos | No | Body JSON | Cada objeto: `{ scope_type: string, scope_id: uuid, scope_name: string }`. `scope_type` enum: `organization`, `entity`, `branch`. `scope_id` debe existir segun el tipo. | — |

**Edicion (`PUT /users/:id`):**

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| `id` | uuid | Si | Path param | UUID v4 valido; debe existir en `users` dentro del tenant | — |
| `tenant_id` | uuid | Si | Header `X-Tenant-Id` o JWT claim | UUID v4 valido; debe coincidir con JWT | — |
| `username` | string | Si | Body JSON | Misma validacion que creacion. Unico por `tenant_id` excluyendo el propio registro. | RN-SEC-06, RN-SEC-07 |
| `email` | string | Si | Body JSON | Misma validacion que creacion. Unico por `tenant_id` excluyendo el propio registro. | RN-SEC-06 |
| `first_name` | string | Si | Body JSON | Min 1, max 100 caracteres. Trim. | — |
| `last_name` | string | Si | Body JSON | Min 1, max 100 caracteres. Trim. | — |
| `entity_id` | uuid / null | No | Body JSON | Misma validacion que creacion. | — |
| `entity_name` | string / null | No | Body JSON | Misma validacion que creacion. | — |
| `status` | string | Si | Body JSON | Enum: `active`, `inactive`, `locked`. En edicion se permite `locked` para que Super Admin pueda desbloquear cambiando a `active`. | — |
| `role_ids` | array de uuid | Si | Body JSON | Misma validacion que creacion. | — |
| `assignments` | array de objetos | No | Body JSON | Misma validacion que creacion. Se reemplaza la lista completa (delete + insert). | — |

#### Proceso (Happy Path)

**Creacion:**

| # | Paso | Responsable |
|---|------|-------------|
| 1 | Recibir `POST /users` con body JSON | API Gateway |
| 2 | Validar JWT y extraer `tenant_id`, `permissions[]`, `entity_id` | API Gateway (YARP) |
| 3 | Verificar que `p_users_create` esta presente en `permissions[]` del JWT | Identity Service |
| 4 | Activar RLS con `tenant_id` | Identity Service |
| 5 | Si el actor es Admin Entidad, verificar que `entity_id` del body es `null` o coincide con `entity_id` del JWT | Identity Service |
| 6 | Obtener `mask.username` y `mask.password` de Config Service (cache Redis, fallback hardcoded) | Identity Service |
| 7 | Validar `username` contra `mask.username` | Identity Service |
| 8 | Validar `password` contra `mask.password` | Identity Service |
| 9 | Verificar unicidad de `username` en tenant: `SELECT COUNT(*) FROM users WHERE tenant_id = :tenant_id AND username = :username` | Identity Service |
| 10 | Verificar unicidad de `email` en tenant: `SELECT COUNT(*) FROM users WHERE tenant_id = :tenant_id AND email = :email` | Identity Service |
| 11 | Verificar que todos los `role_ids` existen: `SELECT id FROM roles WHERE id = ANY(:role_ids) AND tenant_id = :tenant_id` | Identity Service |
| 12 | Si `entity_id` no es null, verificar que la entidad existe y esta activa en el tenant | Identity Service |
| 13 | Si `assignments` esta presente, verificar que cada `scope_id` existe segun su `scope_type` | Identity Service |
| 14 | Hashear `password` con bcrypt (cost factor 12) | Identity Service |
| 15 | Insertar en `users`: `INSERT INTO users (id, tenant_id, username, password_hash, email, first_name, last_name, entity_id, entity_name, status, created_at, updated_at, created_by, updated_by) VALUES (...)` | Identity Service |
| 16 | Insertar roles: `INSERT INTO user_roles (user_id, role_id) VALUES (:user_id, :role_id)` para cada rol | Identity Service |
| 17 | Si `assignments` esta presente, insertar: `INSERT INTO user_assignments (id, user_id, scope_type, scope_id, scope_name) VALUES (...)` para cada asignacion | Identity Service |
| 18 | Publicar `UserCreatedEvent` con `{ user_id, tenant_id, username, email, roles[], created_by, timestamp }` a RabbitMQ | Identity Service |
| 19 | Retornar `201 Created` con `{ id, username, email, first_name, last_name, entity_id, entity_name, status, roles[], assignments[], created_at }` | Identity Service |

**Edicion:**

| # | Paso | Responsable |
|---|------|-------------|
| 1 | Recibir `PUT /users/:id` con body JSON | API Gateway |
| 2 | Validar JWT y extraer `tenant_id`, `permissions[]`, `entity_id` | API Gateway (YARP) |
| 3 | Verificar que `p_users_edit` esta presente en `permissions[]` del JWT | Identity Service |
| 4 | Activar RLS con `tenant_id` | Identity Service |
| 5 | Buscar usuario: `SELECT * FROM users WHERE id = :id AND tenant_id = :tenant_id` | Identity Service |
| 6 | Si el actor es Admin Entidad, verificar que el usuario pertenece a la misma entidad: `user.entity_id = jwt.entity_id` | Identity Service |
| 7 | Obtener `mask.username` de Config Service (cache Redis, fallback hardcoded) | Identity Service |
| 8 | Validar `username` contra `mask.username` | Identity Service |
| 9 | Verificar unicidad de `username` excluyendo el propio registro: `SELECT COUNT(*) FROM users WHERE tenant_id = :tenant_id AND username = :username AND id != :id` | Identity Service |
| 10 | Verificar unicidad de `email` excluyendo el propio registro: `SELECT COUNT(*) FROM users WHERE tenant_id = :tenant_id AND email = :email AND id != :id` | Identity Service |
| 11 | Verificar que todos los `role_ids` existen en el tenant | Identity Service |
| 12 | Si `entity_id` no es null, verificar que la entidad existe y esta activa | Identity Service |
| 13 | Si `assignments` esta presente, verificar que cada `scope_id` existe segun su `scope_type` | Identity Service |
| 13b | Si el `status` enviado es `inactive` o `locked` y el usuario actual tiene rol Super Admin: verificar que no es el ultimo Super Admin activo en el tenant (`SELECT COUNT(*) FROM users u JOIN user_roles ur ON u.id = ur.user_id JOIN roles r ON ur.role_id = r.id WHERE r.name = 'Super Admin' AND r.is_system = true AND u.tenant_id = :tenant_id AND u.status = 'active' AND u.id != :id`). Si count = 0, retornar 409 `USERS_LAST_SUPER_ADMIN`. | Identity Service |
| 14 | Actualizar `users`: `UPDATE users SET username = :username, email = :email, first_name = :first_name, last_name = :last_name, entity_id = :entity_id, entity_name = :entity_name, status = :status, updated_at = NOW(), updated_by = :jwt_user_id WHERE id = :id` | Identity Service |
| 15 | Reemplazar roles: `DELETE FROM user_roles WHERE user_id = :id` seguido de `INSERT INTO user_roles (user_id, role_id) VALUES (:id, :role_id)` para cada rol | Identity Service |
| 16 | Reemplazar asignaciones: `DELETE FROM user_assignments WHERE user_id = :id` seguido de insertar nuevas asignaciones | Identity Service |
| 17 | Publicar `UserUpdatedEvent` con `{ user_id, tenant_id, changes: { campo: { old, new } }, updated_by, timestamp }` a RabbitMQ | Identity Service |
| 18 | Retornar `200 OK` con `{ id, username, email, first_name, last_name, entity_id, entity_name, status, roles[], assignments[], updated_at }` | Identity Service |

#### Salidas

**Creacion (201):**

| Campo | Tipo | Destino | Efecto observable |
|-------|------|---------|-------------------|
| `id` | uuid | Response body → SPA | ID del nuevo usuario |
| `username` | string | Response body → SPA | Username creado |
| `email` | string | Response body → SPA | Email del usuario |
| `first_name` | string | Response body → SPA | Nombre |
| `last_name` | string | Response body → SPA | Apellido |
| `entity_id` | uuid / null | Response body → SPA | Entidad asignada o null si es plataforma |
| `entity_name` | string / null | Response body → SPA | Nombre de la entidad |
| `status` | string | Response body → SPA | Estado inicial del usuario |
| `roles` | array de objetos | Response body → SPA | `[{ id, name }]` — roles asignados |
| `assignments` | array de objetos | Response body → SPA | `[{ id, scope_type, scope_id, scope_name }]` — asignaciones de alcance |
| `created_at` | datetime | Response body → SPA | Timestamp de creacion |
| `users` row | INSERT | SA.identity | Nuevo registro en tabla `users` |
| `user_roles` rows | INSERT | SA.identity | Relaciones usuario-rol creadas |
| `user_assignments` rows | INSERT | SA.identity | Asignaciones de alcance creadas |
| `UserCreatedEvent` | evento async | RabbitMQ → Audit Service | Registro en `audit_log` con operation `UserCreated` |

**Edicion (200):**

| Campo | Tipo | Destino | Efecto observable |
|-------|------|---------|-------------------|
| `id` | uuid | Response body → SPA | ID del usuario editado |
| `username` | string | Response body → SPA | Username actualizado |
| `email` | string | Response body → SPA | Email actualizado |
| `first_name` | string | Response body → SPA | Nombre actualizado |
| `last_name` | string | Response body → SPA | Apellido actualizado |
| `entity_id` | uuid / null | Response body → SPA | Entidad asignada |
| `entity_name` | string / null | Response body → SPA | Nombre de la entidad |
| `status` | string | Response body → SPA | Estado actualizado |
| `roles` | array de objetos | Response body → SPA | `[{ id, name }]` — roles actualizados |
| `assignments` | array de objetos | Response body → SPA | `[{ id, scope_type, scope_id, scope_name }]` — asignaciones actualizadas |
| `updated_at` | datetime | Response body → SPA | Timestamp de actualizacion |
| `users` row | UPDATE | SA.identity | Registro actualizado en tabla `users` |
| `user_roles` rows | DELETE + INSERT | SA.identity | Relaciones usuario-rol reemplazadas |
| `user_assignments` rows | DELETE + INSERT | SA.identity | Asignaciones de alcance reemplazadas |
| `UserUpdatedEvent` | evento async | RabbitMQ → Audit Service | Registro en `audit_log` con operation `UserUpdated` y detalle de cambios |

#### Errores Tipados

| Codigo | Causa | Condicion de disparo | Respuesta esperada |
|--------|-------|---------------------|--------------------|
| `AUTH_FORBIDDEN` | Sin permiso | `p_users_create` o `p_users_edit` no presente en JWT `permissions[]` | HTTP 403, mensaje: "No tiene permisos para crear/editar usuarios." |
| `AUTH_UNAUTHORIZED` | JWT ausente o invalido | Header Authorization ausente, expirado o firma invalida | HTTP 401, mensaje: "Token de autenticacion invalido o expirado." |
| `USERS_USERNAME_TAKEN` | Username duplicado | Ya existe un usuario con el mismo `username` en el `tenant_id` | HTTP 409, mensaje: "El nombre de usuario ya esta en uso." |
| `USERS_EMAIL_TAKEN` | Email duplicado | Ya existe un usuario con el mismo `email` en el `tenant_id` | HTTP 409, mensaje: "El email ya esta registrado." |
| `USERS_NOT_FOUND` | Usuario inexistente (edicion) | No existe usuario con el `id` proporcionado en el tenant | HTTP 404, mensaje: "Usuario no encontrado." |
| `USERS_ENTITY_NOT_FOUND` | Entidad inexistente | `entity_id` proporcionado no existe o no esta activo en el tenant | HTTP 422, mensaje: "La entidad seleccionada no existe o no esta activa." |
| `USERS_ROLE_NOT_FOUND` | Rol inexistente | Algun `role_id` no existe en `roles` dentro del tenant | HTTP 422, mensaje: "Uno o mas roles seleccionados no existen." |
| `USERS_SCOPE_NOT_FOUND` | Scope inexistente | Algun `scope_id` en `assignments` no existe segun su `scope_type` | HTTP 422, mensaje: "Una o mas asignaciones de alcance son invalidas." |
| `USERS_ENTITY_MISMATCH` | Admin Entidad intenta crear/editar usuario de otra entidad | `entity_id` del body no coincide con `entity_id` del JWT del Admin Entidad | HTTP 403, mensaje: "No puede gestionar usuarios de otra entidad." |
| `USERS_MIN_ROLES` | Sin roles asignados | `role_ids` es array vacio | HTTP 422, mensaje: "Debe asignar al menos un rol al usuario." |
| `TENANT_CLAIM_MISSING` | tenant_id ausente o invalido en JWT | Claim `tenant_id` del JWT es null, vacio o no es UUID valido | HTTP 401, mensaje: "Tenant no identificado." |
| `USERS_LAST_SUPER_ADMIN` | Ultimo Super Admin activo | Se intenta desactivar (`status = inactive`) o bloquear al usuario que es el ultimo Super Admin activo en el tenant | HTTP 409, mensaje: "No se puede desactivar al ultimo Super Admin activo del tenant." |
| `VALIDATION_ERROR` | Campo invalido | `username` no cumple `mask.username`, `password` no cumple `mask.password`, email invalido, campos requeridos ausentes | HTTP 422, mensaje: detalle del campo invalido |

#### Casos Especiales y Variantes

- **Contrasena solo en creacion:** El campo `password` es obligatorio en `POST /users` y no se incluye en `PUT /users/:id`. Para cambiar la contrasena existe un flujo separado (RF-SEC-04 o similar endpoint dedicado).
- **Admin Entidad restringido a su entidad:** El Admin Entidad solo puede crear usuarios con `entity_id = null` (plataforma) o `entity_id = jwt.entity_id` (su propia entidad). En edicion, solo puede editar usuarios cuyo `entity_id` coincida con el suyo. El Super Admin no tiene esta restriccion.
- **Reemplazo completo de roles y asignaciones:** En edicion, los `role_ids` y `assignments` enviados reemplazan completamente los existentes (estrategia delete + insert). El SPA debe enviar la lista completa, no deltas.
- **Entidad _platform:** Si `entity_id` es `null`, el usuario es considerado usuario de plataforma sin entidad asociada. `entity_name` debe ser `null` en este caso.
- **Permisos efectivos en SPA:** El SPA muestra los permisos efectivos (union de permisos de todos los roles seleccionados) como informacion de lectura. Este calculo lo realiza el SPA a partir de los permisos de cada rol, no el backend en este endpoint.
- **Propagacion de cambios de roles:** Los cambios en roles se reflejan en el JWT del usuario afectado recien al siguiente refresh token (RN-SEC-03). El JWT activo mantiene los permisos anteriores hasta que expire.
- **Bcrypt cost factor:** El password se hashea con bcrypt cost factor 12. Esto aplica solo en creacion.
- **Proteccion de ultimo Super Admin:** Si el usuario que se esta editando es el ultimo Super Admin activo en el tenant (unico usuario activo con rol Super Admin seed), no se permite cambiar su status a `inactive` o `locked`. Esto previene un lockout irreversible del tenant.

#### Impacto en Modelo de Datos

| Entidad | Operacion | Campo(s) afectado(s) | Evento |
|---------|-----------|---------------------|--------|
| `users` | INSERT (crear) | `id`, `tenant_id`, `username`, `password_hash`, `email`, `first_name`, `last_name`, `entity_id`, `entity_name`, `status`, `created_at`, `updated_at`, `created_by`, `updated_by` | `UserCreatedEvent` |
| `users` | UPDATE (editar) | `username`, `email`, `first_name`, `last_name`, `entity_id`, `entity_name`, `status`, `updated_at`, `updated_by` | `UserUpdatedEvent` |
| `user_roles` | INSERT (crear) / DELETE + INSERT (editar) | `user_id`, `role_id` | — |
| `user_assignments` | INSERT (crear) / DELETE + INSERT (editar) | `id`, `user_id`, `scope_type`, `scope_id`, `scope_name` | — |
| `roles` | SELECT | `id`, `tenant_id` (validacion de existencia) | — |
| `audit_log` (SA.audit) | INSERT (async) | `user_id`, `operation`, `details`, `ip_address` | via evento |

#### Criterios de Aceptacion (Gherkin)

```gherkin
Scenario: Crear usuario exitosamente
  Given un Super Admin autenticado con permiso "p_users_create"
  And no existe usuario con username "nuevo.usuario" en el tenant
  When envio POST /users con username "nuevo.usuario", email "nuevo@test.com", password "SecurePass123!", first_name "Juan", last_name "Perez", status "active", role_ids [<id-rol-operador>]
  Then recibo HTTP 201 con el usuario creado
  And el usuario existe en la tabla users con password_hash (bcrypt)
  And existe un registro en user_roles con el rol asignado
  And se publica UserCreatedEvent

Scenario: Crear usuario con asignaciones de alcance
  Given un Super Admin autenticado con permiso "p_users_create"
  When envio POST /users con datos validos y assignments [{ scope_type: "entity", scope_id: <id>, scope_name: "Entidad Test" }]
  Then recibo HTTP 201 con el usuario creado
  And existe un registro en user_assignments con los datos de la asignacion

Scenario: Editar usuario exitosamente
  Given un Super Admin autenticado con permiso "p_users_edit"
  And existe un usuario con id <user-id> en el tenant
  When envio PUT /users/<user-id> con first_name "Carlos", role_ids [<id-rol-admin>]
  Then recibo HTTP 200 con el usuario actualizado
  And first_name es "Carlos" en la tabla users
  And user_roles contiene solo el rol admin
  And se publica UserUpdatedEvent con los cambios

Scenario: Username duplicado rechazado en creacion
  Given un Super Admin autenticado con permiso "p_users_create"
  And existe un usuario con username "existente" en el tenant
  When envio POST /users con username "existente"
  Then recibo HTTP 409 con codigo USERS_USERNAME_TAKEN

Scenario: Email duplicado rechazado en edicion
  Given un Super Admin autenticado con permiso "p_users_edit"
  And existe un usuario con email "usado@test.com" en el tenant
  When envio PUT /users/<otro-user-id> con email "usado@test.com"
  Then recibo HTTP 409 con codigo USERS_EMAIL_TAKEN

Scenario: Admin Entidad no puede crear usuario de otra entidad
  Given un Admin Entidad de "Entidad-A" con permiso "p_users_create"
  When envio POST /users con entity_id = <id-entidad-B>
  Then recibo HTTP 403 con codigo USERS_ENTITY_MISMATCH

Scenario: Admin Entidad no puede editar usuario de otra entidad
  Given un Admin Entidad de "Entidad-A" con permiso "p_users_edit"
  And existe un usuario en "Entidad-B" con id <user-id-B>
  When envio PUT /users/<user-id-B> con first_name "Hacker"
  Then recibo HTTP 403 con codigo USERS_ENTITY_MISMATCH

Scenario: Crear usuario sin roles es rechazado
  Given un Super Admin autenticado con permiso "p_users_create"
  When envio POST /users con role_ids = []
  Then recibo HTTP 422 con codigo USERS_MIN_ROLES

Scenario: Username invalido segun mascara es rechazado
  Given un Super Admin autenticado con permiso "p_users_create"
  When envio POST /users con username "ab" (menos de 3 caracteres)
  Then recibo HTTP 422 con codigo VALIDATION_ERROR

Scenario: Password no cumple mascara en creacion es rechazado
  Given un Super Admin autenticado con permiso "p_users_create"
  When envio POST /users con password "123" (menos de 8 caracteres)
  Then recibo HTTP 422 con codigo VALIDATION_ERROR

Scenario: Editar usuario inexistente retorna 404
  Given un Super Admin autenticado con permiso "p_users_edit"
  When envio PUT /users/<id-inexistente>
  Then recibo HTTP 404 con codigo USERS_NOT_FOUND

Scenario: Rol inexistente es rechazado
  Given un Super Admin autenticado con permiso "p_users_create"
  When envio POST /users con role_ids = [<id-rol-inexistente>]
  Then recibo HTTP 422 con codigo USERS_ROLE_NOT_FOUND
```

#### Trazabilidad de Pruebas

| Test ID | Tipo | Descripcion |
|---------|------|-------------|
| TP-SEC-07-01 | Positivo | Crear usuario con todos los campos obligatorios retorna 201 |
| TP-SEC-07-02 | Positivo | Crear usuario con asignaciones de alcance retorna 201 con assignments |
| TP-SEC-07-03 | Positivo | Crear usuario con entity_id null (plataforma) retorna 201 |
| TP-SEC-07-04 | Positivo | Editar usuario cambiando nombre y roles retorna 200 |
| TP-SEC-07-05 | Positivo | Editar usuario reemplaza roles completamente (delete + insert) |
| TP-SEC-07-06 | Positivo | Editar usuario reemplaza asignaciones completamente (delete + insert) |
| TP-SEC-07-07 | Positivo | Crear usuario publica `UserCreatedEvent` a RabbitMQ |
| TP-SEC-07-08 | Positivo | Editar usuario publica `UserUpdatedEvent` con detalle de cambios |
| TP-SEC-07-09 | Positivo | Super Admin puede desbloquear usuario cambiando status de `locked` a `active` |
| TP-SEC-07-10 | Negativo | Username duplicado en creacion retorna 409 |
| TP-SEC-07-11 | Negativo | Email duplicado en creacion retorna 409 |
| TP-SEC-07-12 | Negativo | Username duplicado en edicion (otro usuario) retorna 409 |
| TP-SEC-07-13 | Negativo | Email duplicado en edicion (otro usuario) retorna 409 |
| TP-SEC-07-14 | Negativo | Crear usuario sin roles retorna 422 |
| TP-SEC-07-15 | Negativo | Username invalido segun mascara retorna 422 |
| TP-SEC-07-16 | Negativo | Password invalido segun mascara retorna 422 |
| TP-SEC-07-17 | Negativo | Rol inexistente retorna 422 |
| TP-SEC-07-18 | Negativo | Entidad inexistente retorna 422 |
| TP-SEC-07-19 | Negativo | Scope inexistente en asignaciones retorna 422 |
| TP-SEC-07-20 | Negativo | Usuario inexistente en edicion retorna 404 |
| TP-SEC-07-21 | Negativo | Sin permiso `p_users_create` retorna 403 |
| TP-SEC-07-22 | Negativo | Sin permiso `p_users_edit` retorna 403 |
| TP-SEC-07-23 | Seguridad | Admin Entidad no puede crear usuario con entity_id de otra entidad |
| TP-SEC-07-24 | Seguridad | Admin Entidad no puede editar usuario de otra entidad |
| TP-SEC-07-25 | Seguridad | RLS impide crear/editar usuarios de otro tenant |
| TP-SEC-07-26 | Seguridad | Password se almacena como bcrypt hash, no en texto plano |
| TP-SEC-07-27 | Seguridad | Crear usuario con status `locked` es rechazado (solo el sistema bloquea) |
| TP-SEC-07-28 | Integracion | Tras crear usuario, el usuario puede autenticarse con las credenciales |
| TP-SEC-07-29 | Integracion | Tras editar roles, el proximo refresh token del usuario contiene los nuevos permisos |
| TP-SEC-07-30 | E2E | Flujo completo: login → navegar a crear usuario → llenar formulario → guardar → verificar en listado |

#### Sin Ambiguedades

- **Supuestos prohibidos:** No se asume que la contrasena se puede cambiar en edicion (este RF no lo soporta). No se asume que los cambios de roles se reflejan inmediatamente en el JWT activo del usuario (se aplica RN-SEC-03). No se asume que el Admin Entidad puede gestionar usuarios de cualquier entidad.
- **Decisiones cerradas:** Roles y asignaciones se reemplazan completamente (no delta). Password solo en creacion. Bcrypt cost factor 12. Status `locked` no permitido en creacion. Admin Entidad restringido a su `entity_id`. Permisos efectivos calculados por el SPA, no por este endpoint.
- **Fuera de alcance explicito:** Cambio de contrasena de usuario existente, asignacion de avatar, importacion masiva de usuarios, validacion de fortaleza de contrasena mas alla de la mascara, envio de email de bienvenida.
- **TODO explicitos = 0**

---

### RF-SEC-08 — Ver detalle de usuario con permisos efectivos

#### Execution Sheet

| Campo | Valor |
|-------|-------|
| **ID** | RF-SEC-08 |
| **Titulo** | Ver detalle de usuario con roles y permisos efectivos |
| **Actor(es)** | Super Admin, Admin Entidad |
| **Prioridad** | Media |
| **Severidad** | P2 |
| **Flujo origen** | FL-SEC-01 seccion 8 |
| **HU origen** | HU025 |

#### Precondiciones

| # | Condicion | Detalle |
|---|-----------|---------|
| 1 | Usuario autenticado con permiso `p_users_detail` | JWT valido con `p_users_detail` en `permissions[]` |
| 2 | Identity Service operativo | SA.identity accesible y saludable |
| 3 | RLS activo con `tenant_id` | El contexto de tenant se establece desde el JWT |

#### Entradas

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| `id` | uuid | Si | Path param | UUID v4 valido; debe existir en `users` dentro del tenant | — |
| `tenant_id` | uuid | Si | Header `X-Tenant-Id` o JWT claim | UUID v4 valido; debe coincidir con JWT | — |

#### Proceso (Happy Path)

| # | Paso | Responsable |
|---|------|-------------|
| 1 | Recibir `GET /users/:id` | API Gateway |
| 2 | Validar JWT y extraer `tenant_id`, `permissions[]`, `entity_id` | API Gateway (YARP) |
| 3 | Verificar que `p_users_detail` esta presente en `permissions[]` del JWT | Identity Service |
| 4 | Activar RLS con `tenant_id` | Identity Service |
| 5 | Buscar usuario: `SELECT u.* FROM users u WHERE u.id = :id AND u.tenant_id = :tenant_id` | Identity Service |
| 6 | Si el actor es Admin Entidad, verificar que el usuario pertenece a la misma entidad: `user.entity_id = jwt.entity_id` | Identity Service |
| 7 | Obtener roles del usuario: `SELECT r.id, r.name, r.description FROM roles r JOIN user_roles ur ON ur.role_id = r.id WHERE ur.user_id = :id` | Identity Service |
| 8 | Obtener permisos efectivos (union de permisos de todos los roles): `SELECT DISTINCT p.id, p.module, p.action, p.code, p.description FROM permissions p JOIN role_permissions rp ON rp.permission_id = p.id JOIN user_roles ur ON ur.role_id = rp.role_id WHERE ur.user_id = :id ORDER BY p.module, p.code` | Identity Service |
| 9 | Obtener asignaciones de alcance: `SELECT ua.id, ua.scope_type, ua.scope_id, ua.scope_name FROM user_assignments ua WHERE ua.user_id = :id` | Identity Service |
| 10 | Retornar `200 OK` con el detalle completo del usuario | Identity Service |

#### Salidas

| Campo | Tipo | Destino | Efecto observable |
|-------|------|---------|-------------------|
| `id` | uuid | Response body → SPA | ID del usuario |
| `username` | string | Response body → SPA | Nombre de usuario |
| `email` | string | Response body → SPA | Email del usuario |
| `first_name` | string | Response body → SPA | Nombre |
| `last_name` | string | Response body → SPA | Apellido |
| `entity_id` | uuid / null | Response body → SPA | Entidad asignada o null si es plataforma |
| `entity_name` | string / null | Response body → SPA | Nombre de la entidad |
| `status` | string | Response body → SPA | Estado actual: `active`, `inactive` o `locked` |
| `last_login` | datetime / null | Response body → SPA | Fecha del ultimo inicio de sesion o null si nunca inicio sesion |
| `created_at` | datetime | Response body → SPA | Fecha de creacion del usuario |
| `updated_at` | datetime | Response body → SPA | Fecha de ultima modificacion |
| `created_by` | uuid | Response body → SPA | ID del usuario que creo el registro |
| `updated_by` | uuid | Response body → SPA | ID del usuario que realizo la ultima modificacion |
| `avatar` | string / null | Response body → SPA | URL del avatar o null |
| `roles` | array de objetos | Response body → SPA | `[{ id, name, description }]` — roles asignados al usuario |
| `effective_permissions` | array de objetos | Response body → SPA | `[{ id, module, action, code, description }]` — union de permisos de todos los roles, sin duplicados, ordenados por `module` y `code` |
| `assignments` | array de objetos | Response body → SPA | `[{ id, scope_type, scope_id, scope_name }]` — asignaciones de alcance |

#### Errores Tipados

| Codigo | Causa | Condicion de disparo | Respuesta esperada |
|--------|-------|---------------------|--------------------|
| `AUTH_FORBIDDEN` | Sin permiso | `p_users_detail` no presente en JWT `permissions[]` | HTTP 403, mensaje: "No tiene permisos para ver el detalle de usuarios." |
| `AUTH_UNAUTHORIZED` | JWT ausente o invalido | Header Authorization ausente, expirado o firma invalida | HTTP 401, mensaje: "Token de autenticacion invalido o expirado." |
| `USERS_NOT_FOUND` | Usuario inexistente | No existe usuario con el `id` proporcionado en el tenant | HTTP 404, mensaje: "Usuario no encontrado." |
| `USERS_ENTITY_MISMATCH` | Admin Entidad consulta usuario de otra entidad | `user.entity_id` no coincide con `jwt.entity_id` del Admin Entidad | HTTP 403, mensaje: "No puede ver usuarios de otra entidad." |

#### Casos Especiales y Variantes

- **Admin Entidad restringido:** El Admin Entidad solo puede ver el detalle de usuarios cuyo `entity_id` coincida con el `entity_id` de su JWT. Si intenta acceder a un usuario de otra entidad, recibe 403 con codigo `USERS_ENTITY_MISMATCH`.
- **Permisos efectivos calculados en backend:** A diferencia de la vista de creacion/edicion (RF-SEC-07) donde el SPA calcula los permisos efectivos, en el detalle el backend calcula la union de permisos de todos los roles del usuario. Esto asegura que la informacion mostrada sea la real y no dependa de logica del SPA.
- **Permisos sin duplicados:** Si dos roles del usuario comparten el mismo permiso, este aparece una sola vez en `effective_permissions` gracias al `SELECT DISTINCT`.
- **Ordenamiento de permisos:** Los permisos efectivos se ordenan por `module` y luego por `code` para facilitar la visualizacion agrupada en el SPA.
- **Boton Editar:** El SPA muestra un boton "Editar" que navega a `/users/:id/edit` (formulario de edicion, RF-SEC-07). Este boton se muestra solo si el usuario logueado tiene permiso `p_users_edit`.
- **Usuario de plataforma:** Si `entity_id` es `null`, `entity_name` tambien es `null`. El SPA muestra "Plataforma" como texto descriptivo.
- **last_login nulo:** Si el usuario nunca inicio sesion, `last_login` es `null`. El SPA muestra "Nunca" o similar.

#### Impacto en Modelo de Datos

| Entidad | Operacion | Campo(s) afectado(s) | Evento |
|---------|-----------|---------------------|--------|
| `users` | SELECT | `id`, `tenant_id`, `username`, `email`, `first_name`, `last_name`, `entity_id`, `entity_name`, `status`, `last_login`, `avatar`, `created_at`, `updated_at`, `created_by`, `updated_by` | — |
| `user_roles` | SELECT (JOIN) | `user_id`, `role_id` | — |
| `roles` | SELECT (JOIN) | `id`, `name`, `description` | — |
| `role_permissions` | SELECT (JOIN) | `role_id`, `permission_id` | — |
| `permissions` | SELECT (JOIN) | `id`, `module`, `action`, `code`, `description` | — |
| `user_assignments` | SELECT | `id`, `user_id`, `scope_type`, `scope_id`, `scope_name` | — |

#### Criterios de Aceptacion (Gherkin)

```gherkin
Scenario: Ver detalle de usuario exitosamente
  Given un Super Admin autenticado con permiso "p_users_detail"
  And existe un usuario con id <user-id> en el tenant
  When envio GET /users/<user-id>
  Then recibo HTTP 200 con el detalle completo del usuario
  And la respuesta contiene username, email, first_name, last_name, entity_name, status, last_login, created_at
  And la respuesta contiene roles como array de objetos con id, name, description
  And la respuesta contiene effective_permissions como array de objetos con id, module, action, code, description
  And la respuesta contiene assignments como array de objetos

Scenario: Permisos efectivos son la union sin duplicados
  Given un usuario con rol "Operador" (permisos: p_users_list, p_users_detail) y rol "Supervisor" (permisos: p_users_list, p_users_edit)
  And un Super Admin autenticado con permiso "p_users_detail"
  When envio GET /users/<user-id>
  Then recibo HTTP 200
  And effective_permissions contiene exactamente 3 permisos: p_users_list, p_users_detail, p_users_edit
  And p_users_list aparece una sola vez (sin duplicados)

Scenario: Permisos efectivos ordenados por modulo y codigo
  Given un usuario con multiples roles que otorgan permisos de distintos modulos
  And un Super Admin autenticado con permiso "p_users_detail"
  When envio GET /users/<user-id>
  Then recibo HTTP 200
  And effective_permissions esta ordenado por module ASC y luego por code ASC

Scenario: Admin Entidad ve detalle de usuario de su entidad
  Given un Admin Entidad de "Entidad-A" con permiso "p_users_detail"
  And existe un usuario en "Entidad-A" con id <user-id-A>
  When envio GET /users/<user-id-A>
  Then recibo HTTP 200 con el detalle completo del usuario

Scenario: Admin Entidad no puede ver detalle de usuario de otra entidad
  Given un Admin Entidad de "Entidad-A" con permiso "p_users_detail"
  And existe un usuario en "Entidad-B" con id <user-id-B>
  When envio GET /users/<user-id-B>
  Then recibo HTTP 403 con codigo USERS_ENTITY_MISMATCH

Scenario: Usuario inexistente retorna 404
  Given un Super Admin autenticado con permiso "p_users_detail"
  When envio GET /users/<id-inexistente>
  Then recibo HTTP 404 con codigo USERS_NOT_FOUND

Scenario: Sin permiso retorna 403
  Given un usuario autenticado sin permiso "p_users_detail"
  When envio GET /users/<user-id>
  Then recibo HTTP 403 con codigo AUTH_FORBIDDEN

Scenario: Usuario con last_login nulo
  Given un Super Admin autenticado con permiso "p_users_detail"
  And existe un usuario que nunca inicio sesion
  When envio GET /users/<user-id>
  Then recibo HTTP 200
  And last_login es null en la respuesta

Scenario: Usuario de plataforma sin entidad
  Given un Super Admin autenticado con permiso "p_users_detail"
  And existe un usuario con entity_id = null
  When envio GET /users/<user-id>
  Then recibo HTTP 200
  And entity_id es null y entity_name es null en la respuesta
```

#### Trazabilidad de Pruebas

| Test ID | Tipo | Descripcion |
|---------|------|-------------|
| TP-SEC-08-01 | Positivo | Ver detalle de usuario retorna 200 con todos los campos |
| TP-SEC-08-02 | Positivo | Detalle incluye roles del usuario con id, name, description |
| TP-SEC-08-03 | Positivo | Detalle incluye permisos efectivos como union de permisos de todos los roles |
| TP-SEC-08-04 | Positivo | Permisos efectivos no contienen duplicados |
| TP-SEC-08-05 | Positivo | Permisos efectivos ordenados por module y code |
| TP-SEC-08-06 | Positivo | Detalle incluye asignaciones de alcance del usuario |
| TP-SEC-08-07 | Positivo | Usuario con last_login null retorna null en el campo |
| TP-SEC-08-08 | Positivo | Usuario de plataforma (entity_id null) retorna null en entity_id y entity_name |
| TP-SEC-08-09 | Positivo | Admin Entidad puede ver detalle de usuario de su propia entidad |
| TP-SEC-08-10 | Negativo | Usuario inexistente retorna 404 |
| TP-SEC-08-11 | Negativo | Sin permiso `p_users_detail` retorna 403 |
| TP-SEC-08-12 | Negativo | JWT ausente retorna 401 |
| TP-SEC-08-13 | Negativo | ID invalido (no UUID) retorna 422 |
| TP-SEC-08-14 | Seguridad | Admin Entidad no puede ver detalle de usuario de otra entidad (403) |
| TP-SEC-08-15 | Seguridad | RLS impide acceso a usuarios de otro tenant |
| TP-SEC-08-16 | Seguridad | No se expone password_hash en la respuesta |
| TP-SEC-08-17 | Integracion | Permisos efectivos reflejan correctamente los roles asignados via user_roles y role_permissions |
| TP-SEC-08-18 | E2E | Flujo completo: login → listar usuarios → click en fila → ver detalle → click editar → navega a formulario edicion |

#### Sin Ambiguedades

- **Supuestos prohibidos:** No se asume que el Admin Entidad puede ver usuarios de cualquier entidad. No se asume que el password_hash se incluye en la respuesta. No se asume que los permisos efectivos se calculan en el SPA (en detalle los calcula el backend).
- **Decisiones cerradas:** Permisos efectivos calculados en backend con `SELECT DISTINCT` + JOIN. Ordenamiento por `module` y `code`. Admin Entidad restringido por `entity_id` del JWT. Boton "Editar" visible solo con permiso `p_users_edit` (logica de SPA). Endpoint de solo lectura (SELECT), sin efectos secundarios ni eventos.
- **Fuera de alcance explicito:** Historial de cambios del usuario, log de actividad del usuario, estadisticas de uso, detalle de las entidades/organizaciones/sucursales asociadas via assignments.
- **TODO explicitos = 0**

---

### RF-SEC-09 — Listar roles con contadores

#### Execution Sheet

| Campo | Valor |
|-------|-------|
| **ID** | RF-SEC-09 |
| **Titulo** | Listar roles con contadores de permisos y usuarios |
| **Actor(es)** | Super Admin |
| **Prioridad** | Alta |
| **Severidad** | P1 |
| **Flujo origen** | FL-SEC-02 seccion 6 (listar) |
| **HU origen** | HU026 |

#### Precondiciones

| # | Condicion | Detalle |
|---|-----------|---------|
| 1 | Identity Service operativo | SA.identity accesible y saludable |
| 2 | Tabla `roles` inicializada | 7 roles seed con `is_system = true` |
| 3 | Tabla `permissions` inicializada | 88 permisos atomicos en 15 modulos |
| 4 | Super Admin autenticado | JWT valido con permiso `p_roles_list` |
| 5 | RLS activo | `tenant_id` resuelto desde JWT |

#### Entradas

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| `Authorization` | string (Bearer JWT) | Si | Header | JWT valido y no expirado con permiso `p_roles_list` | RN-SEC-08 |
| `tenant_id` | uuid | Si | JWT claim | Extraido automaticamente del token | — |
| `search` | string | No | Query param | Busqueda parcial por `name` o `description`. Trim, max 100 chars. Si vacio, se ignora. | — |
| `page` | integer | No | Query param | >= 1, default 1 | — |
| `page_size` | integer | No | Query param | 1-100, default 20 | — |
| `sort_by` | string | No | Query param | Valores permitidos: `name`, `created_at`. Default: `name` | — |
| `sort_dir` | string | No | Query param | `asc` o `desc`. Default: `asc` | — |

#### Proceso (Happy Path)

| # | Paso | Responsable |
|---|------|-------------|
| 1 | Recibir `GET /roles` con query params opcionales | API Gateway |
| 2 | Validar JWT y extraer `tenant_id` y `permissions[]` | API Gateway |
| 3 | Verificar que `permissions[]` incluye `p_roles_list` | Identity Service |
| 4 | Ejecutar `SET LOCAL app.current_tenant = '{tenant_id}'` para activar RLS | Identity Service |
| 5 | Construir query: `SELECT r.id, r.name, r.description, r.is_system, r.created_at, r.updated_at, COUNT(DISTINCT rp.permission_id) AS permission_count, COUNT(DISTINCT ur.user_id) AS user_count FROM roles r LEFT JOIN role_permissions rp ON r.id = rp.role_id LEFT JOIN user_roles ur ON r.id = ur.role_id` | Identity Service |
| 6 | Aplicar filtro `WHERE r.name ILIKE '%{search}%' OR r.description ILIKE '%{search}%'` si `search` tiene valor | Identity Service |
| 7 | Agrupar por `r.id`, aplicar ORDER BY y paginacion (`LIMIT page_size OFFSET (page-1)*page_size`) | Identity Service |
| 8 | Ejecutar COUNT total sin paginacion para metadata de paginacion | Identity Service |
| 9 | Retornar `200 OK` con `{ items[], total, page, page_size }` | Identity Service |

#### Salidas

| Campo | Tipo | Destino | Efecto observable |
|-------|------|---------|-------------------|
| `items[]` | array of role summary | Response body → SPA | Lista de roles con contadores para renderizar grilla |
| `items[].id` | uuid | Response body | ID del rol |
| `items[].name` | string | Response body | Nombre del rol |
| `items[].description` | string | Response body | Descripcion del rol |
| `items[].is_system` | boolean | Response body | Indica si es rol predefinido (protegido) |
| `items[].permission_count` | integer | Response body | Cantidad de permisos asignados al rol |
| `items[].user_count` | integer | Response body | Cantidad de usuarios que tienen este rol |
| `items[].created_at` | timestamp | Response body | Fecha de creacion |
| `items[].updated_at` | timestamp | Response body | Fecha de ultima modificacion |
| `total` | integer | Response body | Total de roles que coinciden con el filtro |
| `page` | integer | Response body | Pagina actual |
| `page_size` | integer | Response body | Tamano de pagina |

#### Errores Tipados

| Codigo | Causa | Condicion de disparo | Respuesta esperada |
|--------|-------|---------------------|--------------------|
| `AUTH_UNAUTHORIZED` | JWT ausente o invalido | Header `Authorization` faltante o token expirado/malformado | HTTP 401, mensaje: "No autenticado" |
| `AUTH_FORBIDDEN` | Sin permiso requerido | JWT valido pero `permissions[]` no incluye `p_roles_list` | HTTP 403, mensaje: "No tiene permisos para esta accion" |
| `VALIDATION_ERROR` | Parametros invalidos | `page < 1`, `page_size > 100`, `sort_by` no reconocido | HTTP 422, mensaje: detalle de campos invalidos |
| `TENANT_CLAIM_MISSING` | tenant_id ausente o invalido en JWT | Claim `tenant_id` del JWT es null, vacio o no es UUID valido | HTTP 401, mensaje: "Tenant no identificado." (RN-SEC-19) |

#### Casos Especiales y Variantes

- **Sin roles:** Si el tenant no tiene roles (improbable por seed), retorna `{ items: [], total: 0, page: 1, page_size: 20 }`.
- **Busqueda sin resultados:** Retorna lista vacia con `total: 0`, HTTP 200.
- **Roles seed visibles:** Los 7 roles con `is_system = true` aparecen en la lista con un indicador visual. No pueden eliminarse desde la grilla (boton deshabilitado en SPA).
- **Contadores en tiempo real:** `permission_count` y `user_count` se calculan con JOINs en la query, no se cachean.

#### Impacto en Modelo de Datos

| Entidad | Operacion | Campo(s) afectado(s) | Evento |
|---------|-----------|---------------------|--------|
| `roles` | SELECT | `id`, `name`, `description`, `is_system`, `created_at`, `updated_at` | — |
| `role_permissions` | SELECT (COUNT) | `role_id`, `permission_id` | — |
| `user_roles` | SELECT (COUNT) | `role_id`, `user_id` | — |

#### Criterios de Aceptacion (Gherkin)

```gherkin
Scenario: Listar roles exitosamente con contadores
  Given un Super Admin autenticado con permiso "p_roles_list" en tenant "tenant-001"
  And existen 3 roles: "Administrador" (5 permisos, 2 usuarios), "Operador" (3 permisos, 10 usuarios), "Consulta" (1 permiso, 0 usuarios)
  When envio GET /roles
  Then recibo HTTP 200 con items[] de 3 elementos
  And cada item incluye permission_count y user_count correctos
  And total = 3

Scenario: Buscar roles por nombre
  Given roles "Administrador", "Admin Entidad", "Operador" en el tenant
  When envio GET /roles?search=admin
  Then recibo HTTP 200 con items[] de 2 elementos ("Administrador" y "Admin Entidad")

Scenario: Paginacion de roles
  Given 25 roles en el tenant
  When envio GET /roles?page=2&page_size=10
  Then recibo HTTP 200 con items[] de 10 elementos
  And total = 25 y page = 2

Scenario: Roles seed marcados como is_system
  Given los 7 roles predefinidos del seed
  When envio GET /roles
  Then los 7 roles tienen is_system = true

Scenario: Sin permiso retorna 403
  Given un usuario autenticado sin permiso "p_roles_list"
  When envio GET /roles
  Then recibo HTTP 403 con codigo AUTH_FORBIDDEN
```

#### Trazabilidad de Pruebas

| Test ID | Tipo | Descripcion |
|---------|------|-------------|
| TP-SEC-09-01 | Positivo | Listar roles retorna 200 con items, total, page, page_size |
| TP-SEC-09-02 | Positivo | Cada rol incluye permission_count calculado correctamente |
| TP-SEC-09-03 | Positivo | Cada rol incluye user_count calculado correctamente |
| TP-SEC-09-04 | Positivo | Roles seed aparecen con is_system = true |
| TP-SEC-09-05 | Positivo | Busqueda por nombre parcial filtra correctamente (ILIKE) |
| TP-SEC-09-06 | Positivo | Busqueda por descripcion parcial filtra correctamente |
| TP-SEC-09-07 | Positivo | Paginacion retorna el subconjunto correcto |
| TP-SEC-09-08 | Positivo | Ordenamiento por name ASC/DESC funciona correctamente |
| TP-SEC-09-09 | Positivo | Ordenamiento por created_at funciona correctamente |
| TP-SEC-09-10 | Positivo | Sin parametros retorna default page=1, page_size=20, sort_by=name ASC |
| TP-SEC-09-11 | Negativo | Sin JWT retorna 401 |
| TP-SEC-09-12 | Negativo | Sin permiso p_roles_list retorna 403 |
| TP-SEC-09-13 | Negativo | page_size > 100 retorna 422 |
| TP-SEC-09-14 | Negativo | sort_by invalido retorna 422 |
| TP-SEC-09-15 | Seguridad | RLS impide ver roles de otro tenant |
| TP-SEC-09-16 | Integracion | Crear rol → listar → verificar que aparece con contadores correctos |

#### Sin Ambiguedades

- **Supuestos prohibidos:** No se asume que los contadores se precalculan en columnas desnormalizadas. No se asume que el endpoint soporta filtrado por `is_system`. No se asume que la busqueda es full-text (es ILIKE simple).
- **Decisiones cerradas:** Contadores calculados en query con JOINs. Busqueda por ILIKE en name y description. Paginacion offset-based. Ordenamiento por name o created_at. Endpoint de solo lectura sin eventos.
- **Fuera de alcance explicito:** Exportacion de roles a Excel, filtro por is_system, filtro por rango de permission_count o user_count.
- **Aclaracion:** permission_count refleja filas en role_permissions, no se valida contra la tabla permissions en este endpoint.
- **TODO explicitos = 0**

---

### RF-SEC-10 — Crear rol con permisos atomicos

#### Execution Sheet

| Campo | Valor |
|-------|-------|
| **ID** | RF-SEC-10 |
| **Titulo** | Crear rol con seleccion de permisos atomicos por modulo |
| **Actor(es)** | Super Admin |
| **Prioridad** | Alta |
| **Severidad** | P0 |
| **Flujo origen** | FL-SEC-02 seccion 6 (crear) |
| **HU origen** | HU026 |

#### Precondiciones

| # | Condicion | Detalle |
|---|-----------|---------|
| 1 | Identity Service operativo | SA.identity accesible y saludable |
| 2 | Tabla `permissions` inicializada | 88 permisos atomicos en 15 modulos (seed data) |
| 3 | Super Admin autenticado | JWT valido con permiso `p_roles_create` |
| 4 | RLS activo | `tenant_id` resuelto desde JWT |
| 5 | RabbitMQ operativo | Para publicacion de `RoleCreatedEvent` |

#### Entradas

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| `Authorization` | string (Bearer JWT) | Si | Header | JWT valido con permiso `p_roles_create` | RN-SEC-08 |
| `name` | string | Si | Body JSON | No vacio, trim, 3-100 chars, unico por tenant (indice `roles(tenant_id, name)` UNIQUE) | RN-SEC-14 |
| `description` | string | Si | Body JSON | No vacio, trim, 3-500 chars | — |
| `permission_ids` | uuid[] | Si | Body JSON | Array no vacio, cada UUID debe existir en tabla `permissions`. Sin duplicados. | RN-SEC-17 |
| `user_id` | uuid | Si | JWT claim `sub` | Extraido del JWT, no enviado por el cliente. Pobla `created_by` y `updated_by` | — |

#### Proceso (Happy Path)

| # | Paso | Responsable |
|---|------|-------------|
| 1 | Recibir `POST /roles` con body `{ name, description, permission_ids[] }` | API Gateway |
| 2 | Validar JWT y verificar permiso `p_roles_create` | Identity Service |
| 3 | Ejecutar `SET LOCAL app.current_tenant = '{tenant_id}'` para activar RLS | Identity Service |
| 4 | Validar formato de entradas: name (trim, longitud), description (trim, longitud), permission_ids (no vacio, sin duplicados) | Identity Service |
| 5 | Verificar unicidad de `name` en el tenant: `SELECT COUNT(*) FROM roles WHERE name = :name` (RLS filtra por tenant) | Identity Service |
| 6 | Verificar que todos los `permission_ids` existen: `SELECT id FROM permissions WHERE id IN (:ids)`. Comparar count con longitud del array | Identity Service |
| 7 | Generar UUIDv7 para el nuevo rol | Identity Service |
| 8 | En una transaccion: `INSERT INTO roles (id, tenant_id, name, description, is_system, created_at, updated_at, created_by, updated_by)` con `is_system = false` | Identity Service |
| 9 | `INSERT INTO role_permissions (role_id, permission_id)` batch para todos los `permission_ids` | Identity Service |
| 10 | Commit transaccion | Identity Service |
| 11 | Publicar `RoleCreatedEvent { role_id, role_name, tenant_id, permission_ids[], created_by, timestamp }` a RabbitMQ | Identity Service |
| 12 | Retornar `201 Created` con `{ id, name, description, is_system, permission_count, created_at }` | Identity Service |

#### Salidas

| Campo | Tipo | Destino | Efecto observable |
|-------|------|---------|-------------------|
| `id` | uuid | Response body → SPA | ID del rol creado |
| `name` | string | Response body → SPA | Nombre del rol |
| `description` | string | Response body → SPA | Descripcion del rol |
| `is_system` | boolean | Response body → SPA | Siempre `false` para roles creados por usuario |
| `permission_count` | integer | Response body → SPA | Cantidad de permisos asignados |
| `created_at` | timestamp | Response body → SPA | Timestamp de creacion |
| `RoleCreatedEvent` | evento async | RabbitMQ → Audit Service | Registro en `audit_log` con operation `Crear`, module `roles` |
| `roles` (row) | INSERT | SA.identity | Nuevo registro de rol |
| `role_permissions` (rows) | INSERT batch | SA.identity | Relaciones rol-permiso |

#### Errores Tipados

| Codigo | Causa | Condicion de disparo | Respuesta esperada |
|--------|-------|---------------------|--------------------|
| `AUTH_UNAUTHORIZED` | JWT ausente o invalido | Header `Authorization` faltante o token expirado/malformado | HTTP 401, mensaje: "No autenticado" |
| `AUTH_FORBIDDEN` | Sin permiso requerido | JWT valido pero sin `p_roles_create` | HTTP 403, mensaje: "No tiene permisos para esta accion" |
| `ROLE_NAME_DUPLICATE` | Nombre de rol duplicado | Ya existe un rol con el mismo `name` en el tenant | HTTP 409, mensaje: "Ya existe un rol con este nombre" |
| `ROLE_PERMISSIONS_EMPTY` | Sin permisos seleccionados | `permission_ids` es array vacio | HTTP 422, mensaje: "Seleccione al menos un permiso" |
| `ROLE_PERMISSION_NOT_FOUND` | Permiso inexistente | Uno o mas UUIDs en `permission_ids` no existen en `permissions` | HTTP 422, mensaje: "Uno o mas permisos no existen: {ids}" |
| `VALIDATION_ERROR` | Entrada malformada | `name` fuera de rango, `description` vacia, `permission_ids` con duplicados | HTTP 422, mensaje: detalle de campos invalidos |
| `TENANT_CLAIM_MISSING` | tenant_id ausente o invalido en JWT | Claim `tenant_id` del JWT es null, vacio o no es UUID valido | HTTP 401, mensaje: "Tenant no identificado." (RN-SEC-19) |
| `ROLE_LIMIT_EXCEEDED` | Limite de roles alcanzado | Tenant tiene >= 500 roles (configurable) | HTTP 422, mensaje: "Se alcanzo el limite maximo de roles para este tenant." Detalle: `{ "details": { "max": 500 } }` |

#### Casos Especiales y Variantes

- **Permisos duplicados en el array:** Si `permission_ids` contiene UUIDs duplicados, se deduplicar silenciosamente antes de la insercion. No se retorna error.
- **Transaccionalidad:** El INSERT en `roles` y el INSERT batch en `role_permissions` se ejecutan en una unica transaccion. Si falla alguno, se hace rollback completo.
- **is_system siempre false:** Los roles creados por usuario nunca pueden tener `is_system = true`. Este campo se fuerza a `false` en el backend independientemente del payload.
- **Evento publicado post-commit:** `RoleCreatedEvent` se publica solo despues del commit exitoso de la transaccion (outbox pattern o post-commit hook).

#### Impacto en Modelo de Datos

| Entidad | Operacion | Campo(s) afectado(s) | Evento |
|---------|-----------|---------------------|--------|
| `roles` | INSERT | `id`, `tenant_id`, `name`, `description`, `is_system=false`, `created_at`, `updated_at`, `created_by`, `updated_by` | `RoleCreatedEvent` |
| `role_permissions` | INSERT (batch) | `role_id`, `permission_id` (N filas) | — (incluido en RoleCreatedEvent) |
| `permissions` | SELECT | `id` (verificacion de existencia) | — |
| `audit_log` (SA.audit) | INSERT (async) | `operation='Crear'`, `module='roles'`, `entity_type='role'`, `entity_id` | Consume `RoleCreatedEvent` |

#### Criterios de Aceptacion (Gherkin)

```gherkin
Scenario: Crear rol exitosamente con permisos seleccionados
  Given un Super Admin autenticado con permiso "p_roles_create" en tenant "tenant-001"
  And existen 88 permisos en la tabla permissions
  When envio POST /roles con name "Analista", description "Rol de analisis", permission_ids [uuid1, uuid2, uuid3]
  Then recibo HTTP 201 con id, name "Analista", is_system = false, permission_count = 3
  And se insertan 1 fila en roles y 3 filas en role_permissions
  And se publica RoleCreatedEvent en RabbitMQ

Scenario: Nombre duplicado retorna 409
  Given existe un rol "Operador" en tenant "tenant-001"
  When envio POST /roles con name "Operador"
  Then recibo HTTP 409 con codigo ROLE_NAME_DUPLICATE

Scenario: Sin permisos seleccionados retorna 422
  When envio POST /roles con name "Vacio", description "test", permission_ids []
  Then recibo HTTP 422 con codigo ROLE_PERMISSIONS_EMPTY

Scenario: Permiso inexistente retorna 422
  When envio POST /roles con permission_ids que incluye un UUID que no existe en permissions
  Then recibo HTTP 422 con codigo ROLE_PERMISSION_NOT_FOUND

Scenario: Permisos duplicados se deduplicar silenciosamente
  When envio POST /roles con permission_ids [uuid1, uuid1, uuid2]
  Then recibo HTTP 201 con permission_count = 2
  And role_permissions tiene 2 filas (no 3)

Scenario: Sin permiso p_roles_create retorna 403
  Given un usuario autenticado sin permiso "p_roles_create"
  When envio POST /roles
  Then recibo HTTP 403 con codigo AUTH_FORBIDDEN
```

#### Trazabilidad de Pruebas

| Test ID | Tipo | Descripcion |
|---------|------|-------------|
| TP-SEC-10-01 | Positivo | Crear rol retorna 201 con todos los campos esperados |
| TP-SEC-10-02 | Positivo | Rol creado se persiste en tabla roles con is_system = false |
| TP-SEC-10-03 | Positivo | Permisos se insertan en role_permissions correctamente (batch) |
| TP-SEC-10-04 | Positivo | RoleCreatedEvent se publica con role_id, permission_ids, created_by |
| TP-SEC-10-05 | Positivo | Audit log registra la creacion del rol |
| TP-SEC-10-06 | Positivo | Permisos duplicados en el array se deduplicar |
| TP-SEC-10-07 | Negativo | Nombre duplicado retorna 409 ROLE_NAME_DUPLICATE |
| TP-SEC-10-08 | Negativo | permission_ids vacio retorna 422 ROLE_PERMISSIONS_EMPTY |
| TP-SEC-10-09 | Negativo | Permiso inexistente retorna 422 ROLE_PERMISSION_NOT_FOUND |
| TP-SEC-10-10 | Negativo | name vacio retorna 422 VALIDATION_ERROR |
| TP-SEC-10-11 | Negativo | name > 100 chars retorna 422 |
| TP-SEC-10-12 | Negativo | description vacia retorna 422 |
| TP-SEC-10-13 | Negativo | Sin JWT retorna 401 |
| TP-SEC-10-14 | Negativo | Sin permiso p_roles_create retorna 403 |
| TP-SEC-10-15 | Seguridad | RLS garantiza que el rol se crea con el tenant_id del JWT |
| TP-SEC-10-16 | Seguridad | is_system siempre false aunque se envie true en el body |
| TP-SEC-10-17 | Transaccion | Si falla INSERT en role_permissions, roles no se persiste (rollback) |
| TP-SEC-10-18 | Integracion | Crear rol → listar roles → verificar que aparece con permission_count correcto |

#### Sin Ambiguedades

- **Supuestos prohibidos:** No se asume que el frontend valida unicidad de nombre (el backend es autoridad). No se asume que `is_system` puede enviarse en el body. No se asume que el evento se publica antes del commit.
- **Decisiones cerradas:** Deduplicacion silenciosa de permission_ids. is_system siempre false. Transaccion atomica para roles + role_permissions. Evento post-commit. Validacion de existencia de todos los permission_ids antes de insertar.
- **Fuera de alcance explicito:** Clonacion de roles, importacion/exportacion de roles, asignacion de usuarios al crear rol, roles con fecha de vigencia.
- **TODO explicitos = 0**

---

### RF-SEC-11 — Editar rol con diff de permisos

#### Execution Sheet

| Campo | Valor |
|-------|-------|
| **ID** | RF-SEC-11 |
| **Titulo** | Editar nombre, descripcion y permisos de un rol con diff |
| **Actor(es)** | Super Admin |
| **Prioridad** | Alta |
| **Severidad** | P0 |
| **Flujo origen** | FL-SEC-02 seccion 6 (editar) |
| **HU origen** | HU026 |

#### Precondiciones

| # | Condicion | Detalle |
|---|-----------|---------|
| 1 | Identity Service operativo | SA.identity accesible y saludable |
| 2 | Rol existente en el tenant | El `role_id` existe en tabla `roles` dentro del tenant actual |
| 3 | Super Admin autenticado | JWT valido con permiso `p_roles_edit` |
| 4 | RLS activo | `tenant_id` resuelto desde JWT |
| 5 | RabbitMQ operativo | Para publicacion de `RoleUpdatedEvent` |

#### Entradas

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| `Authorization` | string (Bearer JWT) | Si | Header | JWT valido con permiso `p_roles_edit` | RN-SEC-08 |
| `id` | uuid | Si | URL path `/roles/:id` | UUID valido, rol existente en el tenant | — |
| `name` | string | Si | Body JSON | No vacio, trim, 3-100 chars, unico por tenant (excluyendo el propio rol) | RN-SEC-14 |
| `description` | string | Si | Body JSON | No vacio, trim, 3-500 chars | — |
| `permission_ids` | uuid[] | Si | Body JSON | Array no vacio, cada UUID debe existir en tabla `permissions`. Sin duplicados. | RN-SEC-17 |
| `role_id` (GET) | uuid | Si | Path param (GET /roles/:id) | UUID valido, debe existir | — |
| `user_id` | uuid | Si | JWT claim `sub` | Extraido del JWT, no enviado por el cliente. Pobla `updated_by` | — |

#### Proceso (Happy Path)

> **Operaciones cubiertas:** GET /roles/:id (precarga para edicion, requiere p_roles_edit) y PUT /roles/:id (guardar cambios).

| # | Paso | Responsable |
|---|------|-------------|
| 1 | Recibir `PUT /roles/:id` con body `{ name, description, permission_ids[] }` | API Gateway |
| 2 | Validar JWT y verificar permiso `p_roles_edit` | Identity Service |
| 3 | Ejecutar `SET LOCAL app.current_tenant = '{tenant_id}'` para activar RLS | Identity Service |
| 4 | Buscar rol por `id`: `SELECT * FROM roles WHERE id = :id` (RLS filtra por tenant) | Identity Service |
| 5 | Verificar que el rol existe (404 si no) | Identity Service |
| 6 | Si `is_system = true`: verificar que `name` no ha cambiado respecto al valor actual. Los roles de sistema no pueden renombrarse (RN-SEC-13). Permisos SI pueden editarse. | Identity Service |
| 7 | Validar formato de entradas: name, description, permission_ids | Identity Service |
| 8 | Verificar unicidad de `name`: `SELECT COUNT(*) FROM roles WHERE name = :name AND id != :id` | Identity Service |
| 9 | Verificar que todos los `permission_ids` existen en tabla `permissions` | Identity Service |
| 10 | Si el rol es el Super Admin seed (`is_system = true` AND `name = 'Super Admin'`), validar que los `permission_ids` resultantes incluyan todos los `p_roles_*` y `p_users_*`. Si no: 422 `ROLE_ADMIN_LOCKOUT_RISK`. | Identity Service |
| 11 | Obtener permisos actuales: `SELECT permission_id FROM role_permissions WHERE role_id = :id` | Identity Service |
| 12 | Calcular diff: `added = permission_ids - current_ids`, `removed = current_ids - permission_ids` | Identity Service |
| 13 | En una transaccion: `UPDATE roles SET name = :name, description = :description, updated_at = NOW(), updated_by = :user_id` | Identity Service |
| 14 | `DELETE FROM role_permissions WHERE role_id = :id AND permission_id IN (:removed)` | Identity Service |
| 15 | `INSERT INTO role_permissions (role_id, permission_id) VALUES (:id, :added_id)` batch | Identity Service |
| 16 | Commit transaccion | Identity Service |
| 17 | Publicar `RoleUpdatedEvent { role_id, role_name, tenant_id, added[], removed[], updated_by, timestamp }` a RabbitMQ | Identity Service |
| 18 | Retornar `200 OK` con `{ id, name, description, is_system, permission_count, updated_at }` | Identity Service |

#### Salidas

| Campo | Tipo | Destino | Efecto observable |
|-------|------|---------|-------------------|
| `id` | uuid | Response body → SPA | ID del rol editado |
| `name` | string | Response body → SPA | Nombre actualizado |
| `description` | string | Response body → SPA | Descripcion actualizada |
| `is_system` | boolean | Response body → SPA | Indicador de rol de sistema |
| `permission_count` | integer | Response body → SPA | Cantidad de permisos tras edicion |
| `updated_at` | timestamp | Response body → SPA | Timestamp de actualizacion |
| `RoleUpdatedEvent` | evento async | RabbitMQ → Audit Service | Registro en `audit_log` con operation `Editar`, detalle de added/removed |
| `roles` (row) | UPDATE | SA.identity | name, description, updated_at, updated_by |
| `role_permissions` | DELETE + INSERT | SA.identity | Diff aplicado |

**Salidas (GET /roles/:id — precarga):**

| Campo | Tipo | Destino | Efecto observable |
|-------|------|---------|-------------------|
| `id` | uuid | Response body | ID del rol |
| `name` | string | Response body | Nombre del rol |
| `description` | string | Response body | Descripcion |
| `is_system` | boolean | Response body | Indica si es rol de sistema |
| `permissions[]` | array | Response body | `[{ id, module, action, code }]` |

#### Errores Tipados

| Codigo | Causa | Condicion de disparo | Respuesta esperada |
|--------|-------|---------------------|--------------------|
| `AUTH_UNAUTHORIZED` | JWT ausente o invalido | Header `Authorization` faltante o token expirado | HTTP 401, mensaje: "No autenticado" |
| `AUTH_FORBIDDEN` | Sin permiso requerido | JWT valido pero sin `p_roles_edit` | HTTP 403, mensaje: "No tiene permisos para esta accion" |
| `ROLE_NOT_FOUND` | Rol inexistente | `id` no existe en tabla `roles` del tenant | HTTP 404, mensaje: "Rol no encontrado" |
| `ROLE_SYSTEM_RENAME` | Intento de renombrar rol de sistema | `is_system = true` y `name` difiere del valor actual | HTTP 403, mensaje: "No se puede renombrar un rol del sistema" |
| `ROLE_NAME_DUPLICATE` | Nombre duplicado | Ya existe otro rol con el mismo `name` en el tenant | HTTP 409, mensaje: "Ya existe un rol con este nombre" |
| `ROLE_PERMISSIONS_EMPTY` | Sin permisos seleccionados | `permission_ids` es array vacio | HTTP 422, mensaje: "Seleccione al menos un permiso" |
| `ROLE_PERMISSION_NOT_FOUND` | Permiso inexistente | Uno o mas UUIDs en `permission_ids` no existen en `permissions` | HTTP 422, mensaje: "Uno o mas permisos no existen: {ids}" |
| `VALIDATION_ERROR` | Entrada malformada | Campos fuera de rango o formato invalido | HTTP 422, mensaje: detalle de campos invalidos |
| `AUTH_FORBIDDEN` | Sin permiso GET detalle | JWT tiene `p_roles_list` pero NO `p_roles_edit` al llamar `GET /roles/:id` | HTTP 403, mensaje: "Se requiere permiso p_roles_edit para acceder al detalle del rol." (RN-SEC-19) |
| `TENANT_CLAIM_MISSING` | tenant_id ausente o invalido en JWT | Claim `tenant_id` del JWT es null, vacio o no es UUID valido | HTTP 401, mensaje: "Tenant no identificado." (RN-SEC-19) |
| `ROLE_ADMIN_LOCKOUT_RISK` | Riesgo de lockout de Super Admin | Edicion del rol Super Admin (seed, `is_system = true`) remueve permisos criticos (`p_roles_*`, `p_users_*`) | HTTP 422, mensaje: "No se pueden remover permisos criticos del rol Super Admin." (RN-SEC-19) |

#### Casos Especiales y Variantes

- **Rol de sistema (is_system = true):** Se pueden editar permisos y descripcion, pero NO renombrar. El backend compara `name` del request con `name` actual; si difieren, retorna `ROLE_SYSTEM_RENAME`.
- **Sin cambios en permisos:** Si `permission_ids` es identico a los permisos actuales, el diff resulta en `added = []` y `removed = []`. Se actualiza solo `name`/`description`/`updated_at` sin tocar `role_permissions`. El evento incluye added/removed vacios.
- **Deduplicacion:** Si `permission_ids` contiene duplicados, se deduplicar antes de calcular el diff.
- **Impacto en usuarios:** Los usuarios con este rol reciben los nuevos permisos al proximo refresh token (RN-SEC-18, D-SEC-03). No se fuerza logout ni se invalidan JWT activos.
- **GET previo para precarga:** El SPA obtiene el rol con permisos actuales via `GET /roles/:id` que retorna `{ id, name, description, is_system, permissions[{ id, module, action, code }] }`. Este GET requiere permiso `p_roles_edit`.

#### Impacto en Modelo de Datos

| Entidad | Operacion | Campo(s) afectado(s) | Evento |
|---------|-----------|---------------------|--------|
| `roles` | SELECT | `id`, `name`, `description`, `is_system` | — |
| `roles` | UPDATE | `name`, `description`, `updated_at`, `updated_by` | `RoleUpdatedEvent` |
| `role_permissions` | SELECT | `role_id`, `permission_id` (actuales) | — |
| `role_permissions` | DELETE | `role_id`, `permission_id` (removidos) | — (incluido en RoleUpdatedEvent) |
| `role_permissions` | INSERT | `role_id`, `permission_id` (agregados) | — (incluido en RoleUpdatedEvent) |
| `permissions` | SELECT | `id` (verificacion de existencia) | — |
| `audit_log` (SA.audit) | INSERT (async) | `operation='Editar'`, `module='roles'`, `entity_type='role'`, `entity_id`, `detail` con added/removed | Consume `RoleUpdatedEvent` |

#### Criterios de Aceptacion (Gherkin)

```gherkin
Scenario: Editar rol exitosamente con cambio de permisos
  Given un rol "Analista" con permisos [p1, p2, p3] en tenant "tenant-001"
  And un Super Admin autenticado con permiso "p_roles_edit"
  When envio PUT /roles/:id con name "Analista Senior", permission_ids [p2, p3, p4]
  Then recibo HTTP 200 con name "Analista Senior", permission_count = 3
  And role_permissions tiene p2, p3, p4 (p1 removido, p4 agregado)
  And se publica RoleUpdatedEvent con added [p4] y removed [p1]

Scenario: Editar rol de sistema permite cambiar permisos pero no nombre
  Given un rol con is_system = true y name "Super Admin"
  When envio PUT /roles/:id con name "Super Admin" y permission_ids diferentes
  Then recibo HTTP 200 con permisos actualizados

Scenario: Renombrar rol de sistema retorna 403
  Given un rol con is_system = true y name "Super Admin"
  When envio PUT /roles/:id con name "Mega Admin"
  Then recibo HTTP 403 con codigo ROLE_SYSTEM_RENAME

Scenario: Nombre duplicado retorna 409
  Given roles "Operador" y "Analista" en el tenant
  When envio PUT /roles/:analista_id con name "Operador"
  Then recibo HTTP 409 con codigo ROLE_NAME_DUPLICATE

Scenario: Editar sin cambios en permisos solo actualiza metadata
  Given un rol "Analista" con permisos [p1, p2]
  When envio PUT /roles/:id con name "Analista Pro" y permission_ids [p1, p2]
  Then recibo HTTP 200 con name "Analista Pro"
  And role_permissions no cambia (added = [], removed = [])

Scenario: Rol inexistente retorna 404
  When envio PUT /roles/:uuid_invalido
  Then recibo HTTP 404 con codigo ROLE_NOT_FOUND

Scenario: GET /roles/:id retorna rol con permisos para precarga del formulario
  Given un rol "Analista" con permisos p1 y p2
  When envio GET /roles/:id
  Then recibo HTTP 200 con name, description, is_system, permissions[{ id, module, action, code }]
```

#### Trazabilidad de Pruebas

| Test ID | Tipo | Descripcion |
|---------|------|-------------|
| TP-SEC-11-01 | Positivo | Editar rol retorna 200 con campos actualizados |
| TP-SEC-11-02 | Positivo | Diff de permisos: permisos removidos se eliminan de role_permissions |
| TP-SEC-11-03 | Positivo | Diff de permisos: permisos agregados se insertan en role_permissions |
| TP-SEC-11-04 | Positivo | RoleUpdatedEvent incluye added[] y removed[] correctos |
| TP-SEC-11-05 | Positivo | Audit log registra edicion con detalle de cambios |
| TP-SEC-11-06 | Positivo | Editar solo name/description sin cambiar permisos funciona |
| TP-SEC-11-07 | Positivo | Rol de sistema permite cambiar permisos y descripcion |
| TP-SEC-11-08 | Positivo | GET /roles/:id retorna rol con permisos para precarga |
| TP-SEC-11-09 | Positivo | Permisos duplicados en el array se deduplicar |
| TP-SEC-11-10 | Negativo | Rol inexistente retorna 404 |
| TP-SEC-11-11 | Negativo | Renombrar rol de sistema retorna 403 ROLE_SYSTEM_RENAME |
| TP-SEC-11-12 | Negativo | Nombre duplicado con otro rol retorna 409 |
| TP-SEC-11-13 | Negativo | permission_ids vacio retorna 422 |
| TP-SEC-11-14 | Negativo | Permiso inexistente retorna 422 |
| TP-SEC-11-15 | Negativo | Sin JWT retorna 401 |
| TP-SEC-11-16 | Negativo | Sin permiso p_roles_edit retorna 403 |
| TP-SEC-11-17 | Seguridad | RLS impide editar roles de otro tenant |
| TP-SEC-11-18 | Transaccion | Si falla INSERT de permisos nuevos, UPDATE de roles se revierte (rollback) |
| TP-SEC-11-19 | Integracion | Editar permisos de rol → refresh token de usuario con ese rol → JWT refleja permisos nuevos |
| TP-SEC-11-20 | Negativo | Usuario con solo p_roles_list recibe 403 al llamar GET /roles/:id |
| TP-SEC-11-21 | Negativo | Editar rol Super Admin removiendo p_roles_edit retorna 422 ROLE_ADMIN_LOCKOUT_RISK |

#### Sin Ambiguedades

- **Supuestos prohibidos:** No se asume que el SPA calcula el diff (lo calcula el backend). No se asume que editar un rol fuerza logout de usuarios afectados. No se asume que roles de sistema son completamente inmutables (sus permisos SI se pueden editar).
- **Decisiones cerradas:** Diff calculado en backend. Roles de sistema: permisos editables, nombre inmutable. Propagacion via refresh token (D-SEC-03). PUT reemplaza la lista completa de permisos (no PATCH parcial). Deduplicacion silenciosa. GET /roles/:id requiere p_roles_edit para precarga. Si el rol es el Super Admin seed (`is_system = true` AND `name = 'Super Admin'`), validar que los permisos resultantes incluyan todos los `p_roles_*` y `p_users_*`. Si no: 422 ROLE_ADMIN_LOCKOUT_RISK.
- **Fuera de alcance explicito:** Edicion parcial (PATCH), versionado de roles, historial de cambios del rol, notificacion a usuarios afectados por cambio de permisos.
- **TODO explicitos = 0**

---

### RF-SEC-12 — Eliminar rol con validacion de uso

#### Execution Sheet

| Campo | Valor |
|-------|-------|
| **ID** | RF-SEC-12 |
| **Titulo** | Eliminar rol con validacion de asignacion y proteccion de sistema |
| **Actor(es)** | Super Admin |
| **Prioridad** | Alta |
| **Severidad** | P1 |
| **Flujo origen** | FL-SEC-02 seccion 7a |
| **HU origen** | HU026 |

#### Precondiciones

| # | Condicion | Detalle |
|---|-----------|---------|
| 1 | Identity Service operativo | SA.identity accesible y saludable |
| 2 | Rol existente en el tenant | El `role_id` existe en tabla `roles` dentro del tenant actual |
| 3 | Super Admin autenticado | JWT valido con permiso `p_roles_delete` |
| 4 | RLS activo | `tenant_id` resuelto desde JWT |
| 5 | RabbitMQ operativo | Para publicacion de `RoleDeletedEvent` |

#### Entradas

| Campo | Tipo | Requerido | Origen | Validacion | RN |
|-------|------|-----------|--------|------------|-----|
| `Authorization` | string (Bearer JWT) | Si | Header | JWT valido con permiso `p_roles_delete` | RN-SEC-08 |
| `id` | uuid | Si | URL path `/roles/:id` | UUID valido, rol existente en el tenant | — |

#### Proceso (Happy Path)

| # | Paso | Responsable |
|---|------|-------------|
| 1 | Recibir `DELETE /roles/:id` | API Gateway |
| 2 | Validar JWT y verificar permiso `p_roles_delete` | Identity Service |
| 3 | Ejecutar `SET LOCAL app.current_tenant = '{tenant_id}'` para activar RLS | Identity Service |
| 4 | Buscar rol por `id`: `SELECT id, name, is_system FROM roles WHERE id = :id` | Identity Service |
| 5 | Verificar que el rol existe (404 si no) | Identity Service |
| 6 | Verificar que `is_system = false` (403 si true) (RN-SEC-13) | Identity Service |
| 7 | Verificar que no hay usuarios asignados: `SELECT COUNT(*) FROM user_roles ur JOIN roles r ON ur.role_id = r.id WHERE ur.role_id = :id AND r.tenant_id = :tenant_id`. Si count > 0, retornar 409 con la cantidad (RN-SEC-15). | Identity Service |
| 8 | En una transaccion: `DELETE FROM role_permissions WHERE role_id = :id` | Identity Service |
| 9 | `DELETE FROM roles WHERE id = :id` | Identity Service |
| 10 | Commit transaccion | Identity Service |
| 11 | Publicar `RoleDeletedEvent { role_id, role_name, tenant_id, deleted_by, timestamp }` a RabbitMQ | Identity Service |
| 12 | Retornar `204 No Content` | Identity Service |

#### Salidas

| Campo | Tipo | Destino | Efecto observable |
|-------|------|---------|-------------------|
| HTTP 204 | — | Response → SPA | Confirmacion de eliminacion exitosa, SPA refresca la grilla |
| `RoleDeletedEvent` | evento async | RabbitMQ → Audit Service | Registro en `audit_log` con operation `Eliminar`, module `roles` |
| `role_permissions` (rows) | DELETE | SA.identity | Todas las relaciones rol-permiso eliminadas |
| `roles` (row) | DELETE | SA.identity | Registro de rol eliminado fisicamente |

#### Errores Tipados

| Codigo | Causa | Condicion de disparo | Respuesta esperada |
|--------|-------|---------------------|--------------------|
| `AUTH_UNAUTHORIZED` | JWT ausente o invalido | Header `Authorization` faltante o token expirado | HTTP 401, mensaje: "No autenticado" |
| `AUTH_FORBIDDEN` | Sin permiso requerido | JWT valido pero sin `p_roles_delete` | HTTP 403, mensaje: "No tiene permisos para esta accion" |
| `ROLE_NOT_FOUND` | Rol inexistente | `id` no existe en tabla `roles` del tenant | HTTP 404, mensaje: "Rol no encontrado" |
| `ROLE_SYSTEM_PROTECTED` | Rol de sistema protegido | `is_system = true` | HTTP 403, mensaje: "No se puede eliminar un rol del sistema" |
| `ROLE_IN_USE` | Rol asignado a usuarios | `user_roles` contiene registros para este `role_id` | HTTP 409, mensaje: "Rol en uso por {N} usuario(s). Reasigne los usuarios antes de eliminar." |
| `VALIDATION_ERROR` | ID invalido | `id` no es un UUID valido | HTTP 422, mensaje: "ID invalido" |
| `TENANT_CLAIM_MISSING` | tenant_id ausente o invalido en JWT | Claim `tenant_id` del JWT es null, vacio o no es UUID valido | HTTP 401, mensaje: "Tenant no identificado." (RN-SEC-19) |

#### Casos Especiales y Variantes

- **Eliminacion fisica:** La eliminacion es fisica (DELETE), no soft-delete. Consistente con la politica del proyecto (principio 5 del modelo de datos).
- **Orden de validacion:** 1) Existencia del rol, 2) is_system check, 3) usuarios asignados check. Se retorna el primer error encontrado.
- **Confirmacion en SPA:** El SPA muestra un dialogo de confirmacion: "Este rol sera eliminado permanentemente. ¿Desea continuar?" antes de enviar el DELETE. Esta confirmacion es responsabilidad del SPA, no del backend.
- **Evento post-commit:** `RoleDeletedEvent` se publica solo despues del commit exitoso.
- **Concurrencia:** Si entre la verificacion de user_roles y el DELETE otro proceso asigna un usuario al rol, la FK constraint en `user_roles` lo impedira (el DELETE de `roles` fallara). Esto es un caso extremadamente raro y se maneja con un 500 generico + retry.

#### Impacto en Modelo de Datos

| Entidad | Operacion | Campo(s) afectado(s) | Evento |
|---------|-----------|---------------------|--------|
| `roles` | SELECT | `id`, `name`, `is_system` | — |
| `roles` | DELETE | Fila completa | `RoleDeletedEvent` |
| `role_permissions` | DELETE (batch) | Todas las filas con `role_id = :id` | — (incluido en RoleDeletedEvent) |
| `user_roles` | SELECT (COUNT) | `role_id` (verificacion de uso) | — |
| `audit_log` (SA.audit) | INSERT (async) | `operation='Eliminar'`, `module='roles'`, `entity_type='role'`, `entity_id` | Consume `RoleDeletedEvent` |

#### Criterios de Aceptacion (Gherkin)

```gherkin
Scenario: Eliminar rol exitosamente sin usuarios asignados
  Given un rol "Temporal" con is_system = false y 0 usuarios asignados
  And un Super Admin autenticado con permiso "p_roles_delete"
  When envio DELETE /roles/:id
  Then recibo HTTP 204
  And el rol y sus role_permissions son eliminados fisicamente
  And se publica RoleDeletedEvent en RabbitMQ

Scenario: Eliminar rol de sistema retorna 403
  Given un rol con is_system = true
  When envio DELETE /roles/:id
  Then recibo HTTP 403 con codigo ROLE_SYSTEM_PROTECTED

Scenario: Eliminar rol con usuarios asignados retorna 409
  Given un rol "Operador" con 5 usuarios asignados
  When envio DELETE /roles/:id
  Then recibo HTTP 409 con codigo ROLE_IN_USE y mensaje que incluye "5 usuario(s)"

Scenario: Eliminar rol inexistente retorna 404
  When envio DELETE /roles/:uuid_inexistente
  Then recibo HTTP 404 con codigo ROLE_NOT_FOUND

Scenario: Sin permiso p_roles_delete retorna 403
  Given un usuario autenticado sin permiso "p_roles_delete"
  When envio DELETE /roles/:id
  Then recibo HTTP 403 con codigo AUTH_FORBIDDEN
```

#### Trazabilidad de Pruebas

| Test ID | Tipo | Descripcion |
|---------|------|-------------|
| TP-SEC-12-01 | Positivo | Eliminar rol sin usuarios retorna 204 |
| TP-SEC-12-02 | Positivo | Rol eliminado: registro desaparece de tabla roles |
| TP-SEC-12-03 | Positivo | Role_permissions del rol eliminado desaparecen |
| TP-SEC-12-04 | Positivo | RoleDeletedEvent se publica con role_id, role_name, deleted_by |
| TP-SEC-12-05 | Positivo | Audit log registra la eliminacion del rol |
| TP-SEC-12-06 | Negativo | Eliminar rol de sistema (is_system=true) retorna 403 ROLE_SYSTEM_PROTECTED |
| TP-SEC-12-07 | Negativo | Eliminar rol con usuarios asignados retorna 409 ROLE_IN_USE con cantidad |
| TP-SEC-12-08 | Negativo | Rol inexistente retorna 404 ROLE_NOT_FOUND |
| TP-SEC-12-09 | Negativo | ID no UUID retorna 422 |
| TP-SEC-12-10 | Negativo | Sin JWT retorna 401 |
| TP-SEC-12-11 | Negativo | Sin permiso p_roles_delete retorna 403 |
| TP-SEC-12-12 | Seguridad | RLS impide eliminar roles de otro tenant |
| TP-SEC-12-13 | Seguridad | Roles seed (is_system=true) nunca se eliminan |
| TP-SEC-12-14 | Transaccion | DELETE role_permissions y DELETE roles en misma transaccion |
| TP-SEC-12-15 | Integracion | Crear rol → asignar usuario → intentar eliminar → 409 → desasignar usuario → eliminar → 204 |

#### Sin Ambiguedades

- **Supuestos prohibidos:** No se asume que la eliminacion es logica (es fisica). No se asume que se puede forzar eliminacion de un rol con usuarios (debe reasignar primero). No se asume que el SPA no muestra confirmacion (la muestra).
- **Decisiones cerradas:** Eliminacion fisica (coherente con politica del proyecto). Proteccion de roles de sistema via `is_system`. Verificacion de uso via COUNT en `user_roles`. 409 con cantidad de usuarios afectados. Confirmacion de UI en SPA. Evento post-commit.
- **Fuera de alcance explicito:** Reasignacion automatica de usuarios al eliminar rol, desactivacion de rol en lugar de eliminacion, papelera de reciclaje de roles.
- **TODO explicitos = 0**

---

## Changelog

### v1.2.1 (2026-03-15) — Cross-reference con FL-SEC-01 y FL-SEC-02
- **RN-SEC-19:** Agregada regla de formato estandar de errores HTTP `{ error, code, details? }` aplicable a todos los RF-SEC-*.
- **RF-SEC-02:** Agregada especificacion de canal OTP soportado (solo email en MVP) en Casos Especiales.
- **RF-SEC-03:** Explicitada expiracion absoluta del refresh token (7 dias, configurable) en paso 10 del proceso.
- **RF-SEC-07:** Agregado error tipado `TENANT_CLAIM_MISSING` (401) para tenant_id null/invalido en JWT. Agregado error tipado `USERS_LAST_SUPER_ADMIN` (409) y paso 13b para prevenir desactivacion del ultimo Super Admin activo. Agregada nota en Casos Especiales sobre proteccion de ultimo Super Admin.
- **RF-SEC-09, RF-SEC-10, RF-SEC-11, RF-SEC-12:** Normalizados errores inline al formato tabular consistente con codigos tipados (`TENANT_CLAIM_MISSING`, `ROLE_LIMIT_EXCEEDED`, `ROLE_ADMIN_LOCKOUT_RISK`, `AUTH_FORBIDDEN` para GET detalle). Referencia a RN-SEC-19.

### v1.1.0 (2026-03-15)
- RF-SEC-09 a RF-SEC-12 documentados (FL-SEC-02: Gestionar Roles y Permisos Atomicos)
- 6 reglas de negocio agregadas (RN-SEC-13 a RN-SEC-18)
- Trazabilidad completa con FL-SEC-02 y HU026

### v1.0.0 (2026-03-15)
- RF-SEC-01 a RF-SEC-08 documentados (modulo Identity/Security)
- 12 reglas de negocio definidas (RN-SEC-01 a RN-SEC-12)
- Trazabilidad completa con FL-SEC-01 y HU023, HU024, HU025, HU038
