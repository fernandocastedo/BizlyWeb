# Guía de Despliegue en Render.com

Este documento explica cómo desplegar la aplicación BizlyWeb en Render.com usando Docker.

## Requisitos Previos

1. Cuenta en Render.com
2. Repositorio Git con el código (GitHub, GitLab, etc.)
3. API de backend desplegada y accesible

## Pasos para Desplegar

### 1. Preparar el Repositorio

Asegúrate de que los siguientes archivos estén en el repositorio:
- `Dockerfile` (en la raíz del proyecto WebApp/BizlyWeb/)
- `.dockerignore` (en la raíz del proyecto WebApp/BizlyWeb/)
- Todo el código fuente del proyecto

### 2. Crear un Nuevo Servicio en Render.com

1. Inicia sesión en [Render.com](https://render.com)
2. Haz clic en "New +" y selecciona "Web Service"
3. Conecta tu repositorio de Git

### 3. Configurar el Servicio

#### Configuración Básica:
- **Name**: `bizlyweb` (o el nombre que prefieras)
- **Environment**: `Docker`
- **Region**: Selecciona la región más cercana a tus usuarios
- **Branch**: `main` (o la rama que uses)

#### Configuración de Build:
- **Dockerfile Path**: `WebApp/BizlyWeb/Dockerfile`
- **Docker Context**: `WebApp/BizlyWeb`

#### Variables de Entorno:
Configura las siguientes variables de entorno en Render.com:

```
ASPNETCORE_ENVIRONMENT=Production
PORT=10000
ApiSettings__BaseUrl=https://apibizly.onrender.com
```

**Nota**: Render.com automáticamente asigna el puerto a través de la variable `PORT`, pero puedes configurarla manualmente si es necesario.

### 4. Configuración Avanzada (Opcional)

#### Health Check:
- **Health Check Path**: `/` o `/Auth/Login`
- **Health Check Interval**: 300 segundos

#### Auto-Deploy:
- Activa "Auto-Deploy" si quieres que se despliegue automáticamente en cada push a la rama principal

### 5. Desplegar

1. Haz clic en "Create Web Service"
2. Render.com comenzará a construir la imagen Docker
3. El proceso puede tardar varios minutos la primera vez
4. Una vez completado, tu aplicación estará disponible en la URL proporcionada

## Verificación Post-Despliegue

1. Verifica que la aplicación carga correctamente
2. Prueba el login y otras funcionalidades
3. Revisa los logs en Render.com para detectar errores

## Solución de Problemas

### Error: "failed to read dockerfile"
- Verifica que el Dockerfile esté en la ruta correcta: `WebApp/BizlyWeb/Dockerfile`
- Verifica que el Docker Context esté configurado como `WebApp/BizlyWeb`

### Error: "Port already in use"
- Render.com maneja automáticamente el puerto, no necesitas configurarlo manualmente
- Asegúrate de que `Program.cs` use la variable de entorno `PORT`

### Error: "Cannot connect to API"
- Verifica que la variable de entorno `ApiSettings__BaseUrl` esté configurada correctamente
- Verifica que la API esté desplegada y accesible

### La aplicación no inicia
- Revisa los logs en Render.com
- Verifica que todas las dependencias estén instaladas
- Asegúrate de que `ASPNETCORE_ENVIRONMENT=Production` esté configurado

## Estructura del Proyecto

```
WebApp/BizlyWeb/
├── Dockerfile              # Archivo Docker para el despliegue
├── .dockerignore          # Archivos a ignorar en Docker
├── BizlyWeb.sln          # Solución de Visual Studio
└── BizlyWeb/             # Proyecto principal
    ├── Program.cs
    ├── appsettings.json
    └── ...
```

## Notas Importantes

1. **Puerto**: Render.com asigna automáticamente un puerto. La aplicación está configurada para usar la variable de entorno `PORT`.

2. **HTTPS**: Render.com proporciona HTTPS automáticamente. No necesitas configurar certificados.

3. **Archivos Estáticos**: Los archivos en `wwwroot/` se sirven automáticamente.

4. **Uploads**: El directorio `wwwroot/uploads/` se crea automáticamente en el contenedor, pero los archivos se perderán al reiniciar. Considera usar un servicio de almacenamiento externo para producción.

5. **Sesiones**: Las sesiones están configuradas para usar `DistributedMemoryCache`, que funciona en memoria. Para producción, considera usar Redis o SQL Server para sesiones distribuidas.

## Costos

Render.com ofrece un plan gratuito con limitaciones:
- Servicios que se "duermen" después de 15 minutos de inactividad
- Límites de recursos (CPU, RAM)

Para producción, considera actualizar a un plan de pago.

