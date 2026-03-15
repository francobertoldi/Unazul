# HU018 — Ver Detalle de Producto

**Módulo:** Catálogo  
**Rol:** Todos los roles con "Ver Productos"  
**Prioridad:** Media  
**Estado:** Implementada

---

## Descripción

**Como** usuario con permisos de "Ver Productos"  
**Quiero** ver el detalle completo de un producto  
**Para** consultar su información, planes, requisitos y atributos específicos

## Criterios de Aceptación

1. **DADO** que el usuario accede a `/productos/:id`, **CUANDO** carga, **ENTONCES** muestra: nombre, código, entidad, familia, estado, versión, vigencia y descripción

2. **DADO** que el producto tiene planes, **CUANDO** se visualiza, **ENTONCES** se listan los sub productos con sus atributos

3. **DADO** que el producto tiene requisitos, **CUANDO** se visualiza, **ENTONCES** se listan los requisitos documentales

4. **DADO** que el usuario presiona "Editar", **ENTONCES** navega a `/productos/:id/editar`

## Componentes Involucrados

- `src/pages/products/ProductDetail.tsx`
