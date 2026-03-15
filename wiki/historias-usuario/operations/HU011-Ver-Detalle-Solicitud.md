# HU011 — Ver Detalle de Solicitud

**Módulo:** Operaciones
**Rol:** Super Admin, Admin Entidad, Operador, Auditor, Consulta
**Prioridad:** Alta
**Estado:** Implementada

---

## Descripción

**Como** usuario con permisos de "Ver Solicitudes"
**Quiero** ver el detalle completo de una solicitud
**Para** consultar toda la información del trámite, su estado y progreso

## Criterios de Aceptación

### Cabecera

1. **DADO** que el usuario accede a `/solicitudes/:id`, **CUANDO** carga, **ENTONCES** se muestra una cabecera con:
   - Ícono FileCheck en un contenedor circular con fondo primary/10
   - Línea 1: Código de solicitud en negrita (ej. "SOL-2026-001")
   - Línea 2: Nombre completo del solicitante · Tipo y número de documento (ej. "Juan Pérez · DNI 30555888")
   - Línea 3: Nombre del producto — Nombre del plan (texto secundario)
   - Badge de estado con color semántico
   - Badge outline con el nombre del estado actual del workflow
   - Botón "Editar" con ícono Pencil a la derecha

### Solapas (en orden)

2. **DADO** que el detalle carga, **CUANDO** se muestran las solapas, **ENTONCES** el orden es:
   1. Solicitante
   2. Producto
   3. Contactos
   4. Direcciones
   5. Beneficiarios (N)
   6. Documentos (N)
   7. Observaciones (N)
   8. Trazabilidad

### Solapa Solicitante

3. **DADO** que el usuario está en la solapa "Solicitante", **CUANDO** la visualiza, **ENTONCES** se muestra una card con grilla 2 columnas:
   - Nombre: {firstName} {lastName}
   - Documento: {documentType} {documentNumber}
   - Fecha de nacimiento
   - Género
   - Ocupación
   - Nota informativa: si el solicitante tiene más de una solicitud asociada, mostrar "Este solicitante tiene N solicitudes" con link a búsqueda filtrada

**Advertencia de datos compartidos:** DADO que el solicitante tiene más de 1 solicitud, CUANDO el usuario intenta editar contactos o direcciones desde el detalle de la solicitud, ENTONCES se muestra un aviso: "Atención: estos datos pertenecen al solicitante y afectarán a todas sus solicitudes (N solicitudes activas)."

### Solapa Contactos

4. **DADO** que el solicitante tiene contactos, **CUANDO** el usuario selecciona la solapa "Contactos", **ENTONCES** se muestra una lista donde cada item tiene:
   - Badge con tipo de contacto (personal, laboral, emergencia, otro)
   - Email
   - Teléfono (código + número)

5. **DADO** que el solicitante NO tiene contactos, **CUANDO** selecciona la solapa, **ENTONCES** muestra "No se han registrado contactos para este solicitante."

### Solapa Direcciones

6. **DADO** que el solicitante tiene direcciones, **CUANDO** el usuario selecciona la solapa "Direcciones", **ENTONCES** se muestra una lista donde cada dirección tiene:
   - Badge con tipo de dirección (hogar, laboral, legal, otro)
   - Grilla de datos: Calle, Número, Piso, Dpto, Provincia, Ciudad, Código Postal
   - Mapa de Google Maps embebido (iframe, 300px de alto) con georreferenciación automática

7. **DADO** que el solicitante NO tiene direcciones, **CUANDO** selecciona la solapa, **ENTONCES** muestra "No se han registrado direcciones para este solicitante."

### Solapa Beneficiarios

8. **DADO** que hay beneficiarios, **CUANDO** se visualiza la solapa, **ENTONCES** se muestra una lista donde cada item tiene:
   - Nombre completo
   - Parentesco
   - Badge con porcentaje (%)

9. **DADO** que NO hay beneficiarios, **ENTONCES** muestra "Sin beneficiarios"

### Solapa Documentos

10. **DADO** que hay documentos, **CUANDO** se visualiza, **ENTONCES** cada documento muestra:
   - Ícono FileText
   - Nombre del documento
   - Tipo · Fecha de carga (formato corto es-AR)
   - Badge de estado: Aprobado (default), Rechazado (destructive), Pendiente (secondary)

### Solapa Observaciones

11. **DADO** que hay observaciones, **CUANDO** se visualiza, **ENTONCES** cada observación muestra:
   - Avatar circular con ícono MessageSquare
   - Nombre del usuario + timestamp (formato corto es-AR)
   - Texto de la observación

### Solapa Trazabilidad

12. Ver HU012

### Panel Lateral (Información General)

13. **DADO** que el detalle carga, **CUANDO** se observa el panel derecho (1/3 del ancho en desktop), **ENTONCES** se muestra una card con:
   - Entidad
   - Producto
   - Plan
   - Asignado a (si aplica)
   - Fecha de creación (formato es-AR)
   - Fecha de actualización (formato es-AR)

## Componentes Involucrados

- `src/pages/applications/ApplicationDetail.tsx`
