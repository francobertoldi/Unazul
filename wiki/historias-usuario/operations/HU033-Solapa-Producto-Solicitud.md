# HU033 — Solapa Producto en Detalle de Solicitud

**Módulo:** Operaciones  
**Rol:** Todos los usuarios con acceso a solicitudes  
**Prioridad:** Alta  
**Estado:** Implementada

---

## Descripción

**Como** usuario que gestiona solicitudes  
**Quiero** ver los atributos completos del producto asociado dentro del detalle de la solicitud  
**Para** tener toda la información del producto sin navegar a otra pantalla

## Criterios de Aceptación

1. **DADO** que el usuario accede al detalle de una solicitud, **CUANDO** ve las solapas, **ENTONCES** existe una solapa "Producto" ubicada entre "Solicitante" y "Contactos" (posición 2 en el orden definido en HU011).

2. **DADO** que el usuario selecciona la solapa "Producto", **CUANDO** se renderiza, **ENTONCES** se muestran:
   - Información general: nombre, código, familia, versión, estado, entidad
   - Datos del plan: descripción, precio, moneda, cuotas, otros costos
   - Atributos específicos según la familia del producto

3. **DADO** que el producto es de tipo Seguro, **CUANDO** se renderizan los atributos, **ENTONCES** se muestra la sección de coberturas con nombre, descripción, suma asegurada y prima de cada una.

4. **DADO** que la grilla de solicitudes muestra una columna "Producto", **CUANDO** el usuario observa la columna, **ENTONCES** se muestra un resumen de los atributos principales del plan (familia + valores clave).

5. **DADO** que la columna "Solicitante" de la grilla, **CUANDO** se renderiza, **ENTONCES** muestra el nombre completo y debajo el tipo y número de documento.

## Componentes Involucrados

- `src/pages/applications/ApplicationDetail.tsx`
- `src/pages/applications/ApplicationList.tsx`

## Datos de Entrada

Datos de los stores `useApplicationStore` y `useProductStore`.

## Datos de Salida

Solapa con datos del producto y plan, columnas mejoradas en la grilla.
