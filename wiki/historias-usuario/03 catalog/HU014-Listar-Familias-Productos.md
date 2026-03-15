# HU014 — Listar Familias de Productos

**Módulo:** Catálogo  
**Rol:** Todos los roles con "Ver Productos"  
**Prioridad:** Alta  
**Estado:** Implementada

---

## Descripción

**Como** usuario con permisos de "Ver Productos"  
**Quiero** ver un listado de las familias de productos  
**Para** consultar y gestionar las categorías de productos financieros

## Criterios de Aceptación

1. **DADO** que el usuario accede a `/familias-productos`, **CUANDO** carga, **ENTONCES** se muestra una grilla con columnas:
   - Código (máximo 15 caracteres)
   - Descripción (máximo 30 caracteres)
   - Acciones (editar, eliminar)

2. **DADO** que el usuario presiona "Nueva Familia", **CUANDO** completa código y descripción, **ENTONCES** la familia se crea

3. **DADO** que el código de familia determina la categoría del producto, **CUANDO** se muestra, **ENTONCES** se aplican las reglas:
   - Prefijo `PREST` → categoría `loan`
   - Prefijo `SEG` → categoría `insurance`
   - Prefijo `CTA` → categoría `account`
   - Prefijo `TARJETA` → categoría `card`
   - Prefijo `INV` → categoría `investment`

4. **DADO** que una familia tiene productos asociados, **CUANDO** se intenta eliminar, **ENTONCES** se muestra una advertencia

## Componentes Involucrados

- `src/pages/products/ProductFamilyList.tsx`
