# HU020 — Gestionar Parámetros del Sistema

**Módulo:** Configuración  
**Rol:** Super Admin  
**Prioridad:** Alta  
**Estado:** Implementada

---

## Descripción

**Como** Super Admin  
**Quiero** administrar los parámetros de configuración del sistema organizados en grupos jerárquicos  
**Para** personalizar el comportamiento del sistema sin modificar código

## Criterios de Aceptación

### Navegación por Grupos

1. **DADO** que el usuario accede a `/parametros`, **CUANDO** carga, **ENTONCES** se muestra un sidebar izquierdo con los grupos organizados bajo categorías padre:
   - **General:** Canales, Generales, Máscaras, Seguridad, Tipos de Entidades
   - **Técnicos:** Integraciones, Notificaciones, Workflow
   - **Posicionamiento:** Ciudades, Provincias
   - **Seguros:** Coberturas de seguros
   - **Tarjetas:** Niveles de tarjetas, Red de tarjetas

2. **DADO** que los grupos se muestran, **CUANDO** se renderizan, **ENTONCES** dentro de cada categoría están ordenados alfabéticamente

3. **DADO** que un grupo está seleccionado, **CUANDO** se visualiza, **ENTONCES** el botón tiene fondo `bg-primary`, texto `text-primary-foreground` y shadow-sm

### Gestión de Parámetros

4. **DADO** que se selecciona un grupo, **CUANDO** se muestra, **ENTONCES** se listan todos los parámetros del grupo con: clave, valor, tipo, descripción

5. **DADO** que el usuario edita un parámetro, **CUANDO** modifica el valor inline, **ENTONCES** el cambio se persiste

6. **DADO** que el usuario presiona "Agregar Parámetro", **CUANDO** completa los campos, **ENTONCES** se agrega al grupo seleccionado

7. **DADO** que el usuario hace hover sobre un parámetro, **CUANDO** aparece el botón eliminar, **ENTONCES** puede eliminar el parámetro con confirmación

### Gestión de Grupos

8. **DADO** que el usuario presiona "Agregar Grupo", **CUANDO** completa el nombre, **ENTONCES** se crea un nuevo grupo vacío

9. **DADO** que un grupo NO tiene parámetros, **CUANDO** presiona eliminar grupo, **ENTONCES** se elimina

10. **DADO** que un grupo TIENE parámetros, **CUANDO** intenta eliminar, **ENTONCES** no se permite la eliminación

### Relaciones Jerárquicas

11. **DADO** que existen parámetros de ciudades, **CUANDO** se visualizan, **ENTONCES** cada ciudad tiene un `parentKey` que la vincula a una provincia

12. **DADO** que se usa un select de ciudades en cualquier formulario, **CUANDO** se selecciona una provincia, **ENTONCES** las ciudades se filtran automáticamente por el `parentKey`

## Componentes Involucrados

- `src/pages/config/ParametersPage.tsx`
- `src/components/parameters/ListParamEditor.tsx`
