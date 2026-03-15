# HU030 — Gestionar Coberturas de Seguros (CRUD Inline)

**Módulo:** Catálogo  
**Rol:** Super Admin, Admin Entidad, Admin Producto  
**Prioridad:** Media  
**Estado:** Implementada

---

## Descripción

**Como** administrador de productos  
**Quiero** agregar, editar y eliminar coberturas dentro de un sub producto de seguros  
**Para** definir qué coberturas incluye cada plan de seguro

## Criterios de Aceptación

1. **DADO** que se edita un sub producto de la categoría seguros, **CUANDO** se muestra la sección de coberturas, **ENTONCES** aparece:
   - Select para agregar cobertura (opciones parametrizadas desde `insurance_coverages`)
   - Las coberturas ya agregadas NO aparecen en el select (filtrado automático)

2. **DADO** que se selecciona una cobertura del select, **CUANDO** se agrega, **ENTONCES** aparece en la lista con:
   - Ícono ShieldCheck + Nombre de la cobertura
   - Campo "Suma aseg." con prefijo `$` (ancho w-28)
   - Campo "Prima" con prefijo `$` (ancho w-28)
   - Botón eliminar (X)

3. **DADO** que no hay coberturas agregadas, **CUANDO** se muestra la lista, **ENTONCES** aparece el mensaje "No se han agregado coberturas aún"

4. **DADO** que se elimina una cobertura, **CUANDO** presiona X, **ENTONCES** la cobertura se quita de la lista y vuelve a estar disponible en el select

## Componentes Involucrados

- `src/pages/products/ProductForm.tsx` (sección coberturas dentro del PlanDialog)
