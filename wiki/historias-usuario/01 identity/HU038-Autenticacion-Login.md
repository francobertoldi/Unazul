# HU038 — Autenticación y Login

**Módulo:** Seguridad
**Rol:** Todos los usuarios
**Prioridad:** Alta
**Estado:** Pendiente

---

## Descripción

**Como** usuario del sistema
**Quiero** autenticarme de forma segura con usuario/contraseña y verificación OTP
**Para** acceder al backoffice con mis permisos asignados

## Criterios de Aceptación

### Login

1. **DADO** que el usuario accede a `/login`, **CUANDO** carga, **ENTONCES** se muestra un formulario con:
   - Usuario (input text)
   - Contraseña (input password)
   - Botón "Iniciar sesión"

2. **DADO** que las credenciales son válidas, **CUANDO** el usuario presiona "Iniciar sesión", **ENTONCES** se genera un JWT con claims: `userId`, `tenantId`, `roles`, `permissions` y un refresh token almacenado en `refresh_tokens`

3. **DADO** que las credenciales son inválidas, **CUANDO** falla la autenticación, **ENTONCES** se muestra "Usuario o contraseña incorrectos" sin revelar cuál es incorrecto

4. **DADO** que el usuario falla 5 intentos consecutivos, **CUANDO** se alcanza el límite, **ENTONCES** la cuenta se bloquea (`status = locked`) y se registra evento de auditoría

### Verificación OTP (si habilitado por configuración)

5. **DADO** que OTP está habilitado para el tenant, **CUANDO** el login es exitoso, **ENTONCES** se envía un código OTP por email y se muestra pantalla de verificación

6. **DADO** que el usuario ingresa el OTP correcto, **CUANDO** verifica, **ENTONCES** se completa el login y se redirige al dashboard

7. **DADO** que el OTP expira (configurable, default 5 minutos), **CUANDO** el usuario intenta verificar, **ENTONCES** se muestra "Código expirado" con opción de reenvío

### Recuperación de contraseña

8. **DADO** que el usuario presiona "Olvidé mi contraseña", **CUANDO** ingresa su email, **ENTONCES** se envía un enlace de recuperación por email

9. **DADO** que el usuario accede al enlace, **CUANDO** carga, **ENTONCES** se muestra formulario de nueva contraseña (validada con máscara `mask.password`)

### Refresh Token

10. **DADO** que el JWT expira, **CUANDO** hay un refresh token válido, **ENTONCES** se renueva el JWT automáticamente con rotación del refresh token (el anterior se marca como `revoked`)

### Logout

11. **DADO** que el usuario presiona "Cerrar sesión", **CUANDO** confirma, **ENTONCES** se revoca el refresh token, se limpia el estado local y se redirige a `/login`

## Componentes Involucrados

- `src/pages/auth/LoginPage.tsx`
- `src/pages/auth/OtpVerificationPage.tsx`
- `src/pages/auth/ForgotPasswordPage.tsx`
- `src/pages/auth/ResetPasswordPage.tsx`
