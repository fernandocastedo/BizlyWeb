# Instrucciones para Agregar Logo en el Sidebar

## Ubicación del Logo

El logo se puede agregar en el **sidebar** (barra lateral) de la aplicación. La ubicación exacta está en:

**Archivo:** `WebApp/BizlyWeb/BizlyWeb/Views/Shared/_Layout.cshtml`

**Ubicación en el código (líneas 24-32):**

```html
<div class="sidebar-header">
    <!-- Aquí puedes agregar tu logo -->
    <img src="~/img/logo.png" alt="Logo" class="sidebar-logo mb-2" />
    <h3><i class="fas fa-store"></i> Bizly</h3>
</div>
```

## Pasos para Agregar el Logo

### Opción 1: Logo Estático

1. Coloca tu imagen de logo en la carpeta:
   ```
   WebApp/BizlyWeb/BizlyWeb/wwwroot/img/logo.png
   ```
   (Puedes usar cualquier formato: `.png`, `.jpg`, `.svg`, etc.)

2. Descomenta/modifica la línea en `_Layout.cshtml`:
   ```html
   <img src="~/img/logo.png" alt="Logo" class="sidebar-logo mb-2" />
   ```
   Cambia `logo.png` por el nombre de tu archivo.

### Opción 2: Logo Dinámico desde la Empresa

Si quieres usar el logo que se sube en la sección de Configuración:

1. El logo ya se guarda en: `wwwroot/uploads/logos/`
2. Para mostrarlo en el sidebar, puedes obtenerlo desde la base de datos usando el servicio de Empresa.

## Estilos CSS

Los estilos para el logo ya están configurados en:

**Archivo:** `WebApp/BizlyWeb/BizlyWeb/wwwroot/css/sidebar.css`

```css
.sidebar-logo {
    max-width: 120px;
    max-height: 60px;
    width: auto;
    height: auto;
    display: block;
    margin: 0 auto 10px auto;
    border-radius: 5px;
}
```

Puedes ajustar estos valores según necesites:
- `max-width`: ancho máximo del logo
- `max-height`: altura máxima del logo
- `border-radius`: bordes redondeados

## Ejemplo Completo

```html
<div class="sidebar-header">
    <img src="~/img/mi-logo.png" alt="Mi Logo" class="sidebar-logo mb-2" />
    <h3><i class="fas fa-store"></i> Bizly</h3>
</div>
```

## Notas

- El logo aparecerá centrado en la parte superior del sidebar
- Se adaptará automáticamente al tamaño especificado en CSS
- Si no agregas un logo, solo aparecerá el texto "Bizly" con el icono

