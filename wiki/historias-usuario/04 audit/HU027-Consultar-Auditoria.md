# HU027 — Consultar Log de Auditoría

**Módulo:** Seguridad  
**Rol:** Super Admin, Auditor  
**Prioridad:** Alta  
**Estado:** Implementada

---

## Descripción

**Como** auditor  
**Quiero** consultar el registro inmutable de todas las acciones del sistema  
**Para** verificar la trazabilidad de operaciones y detectar anomalías

## Criterios de Aceptación

1. **DADO** que el usuario accede a `/auditoria`, **CUANDO** carga, **ENTONCES** se muestra una grilla con:
   - Usuario (nombre)
   - Tipo de operación (badge: Crear, Editar, Eliminar, Login, Logout, etc. — mapea a `audit_log.operation`)
   - Acción realizada (mapea a `audit_log.action`)
   - Módulo afectado
   - Detalle de la operación
   - Fecha y hora (timestamp)
   - Dirección IP

2. **DADO** que hay registros, **CUANDO** busca, **ENTONCES** filtra por usuario, tipo de operación, acción y módulo

3. **DADO** que hay registros, **CUANDO** exporta, **ENTONCES** puede descargar en Excel o CSV

4. **DADO** que el registro es inmutable, **CUANDO** se visualiza, **ENTONCES** no hay opciones de editar o eliminar entradas

## Componentes Involucrados

- `src/pages/AuditPage.tsx`
- `src/components/shared/DataTable.tsx`
