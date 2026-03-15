# HU009 — Listar Solicitudes

**Módulo:** Operaciones  
**Rol:** Super Admin, Admin Entidad, Operador, Auditor, Consulta  
**Prioridad:** Alta  
**Estado:** Implementada

---

## Descripción

**Como** usuario con permisos de "Ver Solicitudes"  
**Quiero** ver un listado de todas las solicitudes del sistema  
**Para** gestionar y dar seguimiento a los trámites de productos financieros

## Criterios de Aceptación

1. **DADO** que el usuario accede a `/solicitudes`, **CUANDO** carga, **ENTONCES** se muestra una grilla con columnas:
   - Código (SOL-YYYY-NNN)
   - Entidad
   - Producto
   - Solicitante (nombre completo)
   - Estado (badge con color semántico)
   - Etapa workflow (badge outline, nullable)
   - Fecha de creación

2. **DADO** que hay solicitudes, **CUANDO** busca, **ENTONCES** filtra por código y nombre del solicitante

3. **DADO** que hace click en una fila, **CUANDO** navega, **ENTONCES** redirige a `/solicitudes/:id`

4. **DADO** que usa acciones de fila, **CUANDO** presiona editar, **ENTONCES** navega a `/solicitudes/:id/editar`

5. **DADO** que usa exportación, **CUANDO** descarga, **ENTONCES** obtiene archivo Excel o CSV

## Estados y sus colores

| Estado | Label | Color |
|--------|-------|-------|
| draft | Borrador | gris (muted) |
| pending | Pendiente | amarillo |
| in_review | En Revisión | azul |
| approved | Aprobada | verde |
| rejected | Rechazada | rojo |
| cancelled | Cancelada | gris (muted) |
| settled | Liquidada | teal |

## Componentes Involucrados

- `src/pages/applications/ApplicationList.tsx`
- `src/components/shared/DataTable.tsx`
