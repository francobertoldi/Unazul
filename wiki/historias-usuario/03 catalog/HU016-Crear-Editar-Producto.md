# HU016 — Crear y Editar Producto

**Módulo:** Catálogo  
**Rol:** Super Admin, Admin Entidad, Admin Producto  
**Prioridad:** Alta  
**Estado:** Implementada

---

## Descripción

**Como** administrador de productos  
**Quiero** crear y editar productos financieros  
**Para** definir la oferta de productos de cada entidad

## Criterios de Aceptación

1. **DADO** que el usuario accede a `/productos/nuevo`, **CUANDO** carga, **ENTONCES** se muestra un formulario con:
   - Entidad (select, requerido)
   - Familia de producto (select, requerido)
   - Nombre (requerido)
   - Código (requerido)
   - Descripción (requerido)
   - Estado: draft, active, inactive, deprecated (requerido)
   - Fecha vigencia desde (requerido)
   - Fecha vigencia hasta (opcional)

2. **DADO** que se edita un producto existente, **CUANDO** carga, **ENTONCES** se muestran además:
   - Grilla de **Sub Productos / Planes** con CRUD vía diálogos modales
   - Grilla de **Requisitos documentales** con CRUD inline

3. **DADO** que guarda, **CUANDO** todos los campos son válidos, **ENTONCES** se crea/actualiza y redirige con notificación

## Componentes Involucrados

- `src/pages/products/ProductForm.tsx`
