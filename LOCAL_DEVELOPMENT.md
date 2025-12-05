# Guía para Ejecutar el Proyecto Localmente

Esta guía explica cómo ejecutar el proyecto BizlyWeb en tu máquina local después de configurar Docker para Render.com.

## ✅ Cambios Realizados

Se modificó `Program.cs` para que:
- **En desarrollo local**: Use los puertos configurados en `launchSettings.json` (como antes)
- **En producción/Docker**: Use la variable de entorno `PORT` (para Render.com)

## 🚀 Formas de Ejecutar el Proyecto Localmente

### Opción 1: Desde Visual Studio / Visual Studio Code

1. Abre el proyecto en Visual Studio o VS Code
2. Presiona `F5` o haz clic en "Run"
3. La aplicación se ejecutará en los puertos configurados en `launchSettings.json`:
   - HTTP: `http://localhost:5287`
   - HTTPS: `https://localhost:7064`

### Opción 2: Desde la Terminal (PowerShell/CMD)

#### Método A: Usando dotnet run (recomendado)

```powershell
# Navegar al directorio del proyecto
cd "C:\Users\ferna\Downloads\FINAL MOMENT BIZLY CONTEXT\WebApp\BizlyWeb\BizlyWeb"

# Ejecutar con perfil HTTP
dotnet run --launch-profile http

# O ejecutar con perfil HTTPS
dotnet run --launch-profile https
```

#### Método B: Especificando puerto manualmente

```powershell
# Ejecutar en puerto específico (sin usar variable PORT)
dotnet run --urls "http://localhost:5287"
```

#### Método C: Sin especificar puerto (usa launchSettings.json)

```powershell
# Simplemente ejecutar (usa configuración por defecto)
dotnet run
```

### Opción 3: Compilar y Ejecutar Manualmente

```powershell
# 1. Compilar el proyecto
dotnet build

# 2. Ejecutar el ejecutable compilado
dotnet bin\Debug\net8.0\BizlyWeb.dll
```

## 🔍 Verificar que Funciona

1. Abre tu navegador
2. Ve a `http://localhost:5287` o `https://localhost:7064`
3. Deberías ver la página de login

## ⚠️ Solución de Problemas

### Error: "El proceso no puede acceder al archivo porque está siendo usado"

**Causa**: La aplicación ya está corriendo en otro proceso.

**Solución**:
1. Cierra la aplicación si está corriendo (Ctrl+C en la terminal)
2. O cierra Visual Studio/VS Code si está ejecutando la app
3. O termina el proceso manualmente:
   ```powershell
   # Encontrar el proceso
   Get-Process | Where-Object {$_.ProcessName -like "*BizlyWeb*"}
   
   # Terminar el proceso (reemplaza PID con el número del proceso)
   Stop-Process -Id <PID> -Force
   ```

### Error: "Puerto ya en uso"

**Causa**: Otro proceso está usando el puerto.

**Solución**:
1. Cambia el puerto en `launchSettings.json`
2. O termina el proceso que está usando el puerto:
   ```powershell
   # Ver qué proceso usa el puerto 5287
   netstat -ano | findstr :5287
   
   # Terminar el proceso (reemplaza PID)
   taskkill /PID <PID> /F
   ```

### La aplicación no inicia

**Verificaciones**:
1. ¿Tienes .NET 8.0 SDK instalado?
   ```powershell
   dotnet --version
   ```
   Debería mostrar `8.0.x` o superior

2. ¿Las dependencias están restauradas?
   ```powershell
   dotnet restore
   ```

3. ¿El proyecto compila sin errores?
   ```powershell
   dotnet build
   ```

## 📝 Notas Importantes

1. **Variable PORT**: En desarrollo local, NO necesitas definir la variable de entorno `PORT`. El código detecta automáticamente si está definida y solo la usa en ese caso.

2. **Docker vs Local**: 
   - **Local**: Usa `launchSettings.json` para puertos
   - **Docker/Render.com**: Usa variable de entorno `PORT`

3. **Configuración de API**: La URL de la API se lee de `appsettings.json`:
   ```json
   {
     "ApiSettings": {
       "BaseUrl": "https://apibizly.onrender.com"
     }
   }
   ```

## 🐳 Probar Docker Localmente (Opcional)

Si quieres probar el Dockerfile localmente antes de desplegar:

```powershell
# Desde el directorio WebApp/BizlyWeb/
cd "C:\Users\ferna\Downloads\FINAL MOMENT BIZLY CONTEXT\WebApp\BizlyWeb"

# Construir la imagen
docker build -t bizlyweb .

# Ejecutar el contenedor
docker run -p 8080:8080 -e PORT=8080 bizlyweb
```

Luego accede a `http://localhost:8080`

## ✅ Resumen

- ✅ El proyecto funciona localmente igual que antes
- ✅ Los cambios de Docker NO afectan el desarrollo local
- ✅ Puedes seguir usando Visual Studio, VS Code o `dotnet run`
- ✅ La configuración del puerto es automática según el entorno

