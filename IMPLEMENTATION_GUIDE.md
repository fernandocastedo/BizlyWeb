# Guía de Implementación - Bizly WebApp

## 📋 Resumen del Proyecto

**Bizly** es un sistema web para la gestión integral de microemprendimientos que permite:
- Control de inventario de insumos
- Gestión de productos de venta
- Registro y seguimiento de ventas
- Control de costos y gastos
- Generación de reportes y métricas financieras
- Gestión de trabajadores y sucursales

**API Base URL:** `https://apibizly.onrender.com/`

**Tecnología:** ASP.NET Core MVC 8.0

---

## 🎯 Pantallas Principales (con Sidebar)

### 1. **Autenticación** (Sin Sidebar)
- **Login** (`/Auth/Login`)
- **Registro** (`/Auth/Register`)

### 2. **Dashboard Principal** (`/Dashboard`)
- Resumen general del negocio
- Métricas clave (ventas del día, stock bajo, costos del mes)
- Gráficas rápidas

### 3. **Configuración del Emprendimiento** (`/Configuracion`)
- Perfil del emprendimiento
- Edición de datos (nombre, rubro, margen de ganancia, logo)
- Gestión de sucursales

### 4. **Inventario** (`/Inventario`)
- Lista de insumos
- Registro manual de insumos
- Actualización de stock
- Historial de movimientos
- Alertas de stock bajo

### 5. **Productos de Venta** (`/Productos`)
- Lista de productos
- Crear/Editar producto
- Asociación de insumos
- Cálculo automático de precio sugerido

### 6. **Ventas** (`/Ventas`)
- Punto de Venta (POS)
- Historial de ventas
- Pedidos pendientes (con envío)
- Cancelación/Corrección de ventas

### 7. **Clientes** (`/Clientes`)
- Lista de clientes
- Crear/Editar cliente
- Top clientes

### 8. **Costos y Gastos** (`/CostosGastos`)
- Registro de costos y gastos
- Lista de registros financieros
- Clasificación (fijo/variable, directo/administrativo)
- Visualización con gráficas

### 9. **Trabajadores** (`/Trabajadores`)
- Lista de trabajadores
- Crear/Editar trabajador
- Crear usuario vinculado
- Desactivar acceso

### 10. **Reportes y Métricas** (`/Reportes`)
- Reportes de ventas (por día/semana/mes)
- Reportes de costos y gastos
- Top vendedores
- Top productos más vendidos
- Margen de ganancia promedio
- Punto de equilibrio
- Meta mensual de ventas
- Comparativa mensual
- Exportación a PDF

### 11. **Categorías** (`/Categorias`)
- Lista de categorías
- Crear/Editar categoría

---

## 🏗️ Estructura del Proyecto (Arquitectura de 3 Capas)

### **Capa de Presentación** (Controllers y Views)
- Maneja la interacción con el usuario
- Recibe requests HTTP y renderiza vistas
- Valida datos de entrada
- Llama a la Capa de Negocio

### **Capa de Negocio** (Services)
- Contiene la lógica de negocio
- Orquesta las operaciones
- Valida reglas de negocio
- Llama a la Capa de Datos para comunicación con API

### **Capa de Datos** (Models/DTOs y Servicios de API)
- Modelos de datos (DTOs)
- Servicios de comunicación con API
- Transformación de datos entre API y aplicación

```
BizlyWeb/
├── 📱 CAPA DE PRESENTACIÓN
│   ├── Controllers/
│   │   ├── AuthController.cs
│   │   ├── DashboardController.cs
│   │   ├── ConfiguracionController.cs
│   │   ├── InventarioController.cs
│   │   ├── ProductosController.cs
│   │   ├── VentasController.cs
│   │   ├── ClientesController.cs
│   │   ├── CostosGastosController.cs
│   │   ├── TrabajadoresController.cs
│   │   ├── ReportesController.cs
│   │   └── CategoriasController.cs
│   └── Views/
│       ├── Shared/
│       │   ├── _Layout.cshtml (con Sidebar)
│       │   └── _Sidebar.cshtml
│       ├── Auth/
│       ├── Dashboard/
│       ├── Configuracion/
│       ├── Inventario/
│       ├── Productos/
│       ├── Ventas/
│       ├── Clientes/
│       ├── CostosGastos/
│       ├── Trabajadores/
│       ├── Reportes/
│       └── Categorias/
│
├── 💼 CAPA DE NEGOCIO
│   └── Services/
│       ├── ApiService.cs (Cliente HTTP base)
│       ├── AuthService.cs
│       ├── EmpresaService.cs
│       ├── SucursalService.cs
│       ├── InventarioService.cs
│       ├── ProductoService.cs
│       ├── VentaService.cs
│       ├── ClienteService.cs
│       ├── CostoGastoService.cs
│       ├── TrabajadorService.cs
│       └── ReporteService.cs
│
├── 💾 CAPA DE DATOS
│   └── Models/
│       ├── ViewModels/ (Modelos para vistas)
│       │   ├── LoginViewModel.cs
│       │   ├── RegisterViewModel.cs
│       │   ├── DashboardViewModel.cs
│       │   ├── ProductoVentaViewModel.cs
│       │   └── VentaViewModel.cs
│       └── DTOs/ (Data Transfer Objects para API)
│           ├── EmpresaDto.cs
│           ├── InsumoDto.cs
│           ├── ProductoVentaDto.cs
│           ├── VentaDto.cs
│           └── ...
│
├── wwwroot/ (Recursos estáticos)
│   ├── css/
│   │   ├── site.css
│   │   └── sidebar.css
│   ├── js/
│   │   ├── site.js
│   │   └── api-client.js
│   └── lib/
│
└── Program.cs (Configuración y Startup)
```

### **Flujo de Datos entre Capas:**

```
Usuario → Capa de Presentación (Controller)
           ↓
    Capa de Negocio (Service)
           ↓
    Capa de Datos (ApiService → API Externa)
           ↓
    API (https://apibizly.onrender.com)
           ↓
    MongoDB (Base de Datos)
```

**Nota:** En este proyecto, la Capa de Datos no accede directamente a MongoDB, sino que consume la API REST que maneja la persistencia.

---

## 🏛️ Arquitectura de 3 Capas

### **Separación de Responsabilidades:**

#### **1. Capa de Presentación** (Controllers + Views)
- **Responsabilidad**: Interfaz de usuario y manejo de requests HTTP
- **Componentes**:
  - `Controllers/`: Reciben requests, validan entrada, llaman a servicios, retornan vistas
  - `Views/`: Vistas Razor que renderizan HTML
- **No debe contener**: Lógica de negocio ni comunicación directa con API

#### **2. Capa de Negocio** (Services)
- **Responsabilidad**: Lógica de negocio y orquestación de operaciones
- **Componentes**:
  - `Services/`: Servicios que implementan reglas de negocio
  - Validaciones de negocio
  - Cálculos (precios, márgenes, totales)
  - Transformaciones de datos
- **No debe contener**: Lógica de presentación ni acceso directo a API (usa Capa de Datos)

#### **3. Capa de Datos** (Models/DTOs + ApiService)
- **Responsabilidad**: Comunicación con API externa y modelos de datos
- **Componentes**:
  - `Models/DTOs/`: Objetos de transferencia de datos (DTOs) para comunicación con API
  - `Models/ViewModels/`: Modelos específicos para vistas
  - `Services/ApiService.cs`: Servicio base para comunicación HTTP con API
- **No debe contener**: Lógica de negocio ni lógica de presentación

### **Ejemplo de Flujo:**

```
1. Usuario hace click en "Crear Producto"
   ↓
2. Capa de Presentación: ProductosController.Create() recibe request
   ↓
3. Capa de Negocio: ProductoService.CrearProducto() valida reglas de negocio
   ↓
4. Capa de Datos: ApiService.Post() envía datos a API
   ↓
5. API procesa y guarda en MongoDB
   ↓
6. Respuesta fluye de vuelta por las capas
   ↓
7. Capa de Presentación: Controller retorna vista con resultado
```

---

## 📦 Dependencias Necesarias

Basado en VentaTransacciones y requerimientos del proyecto:

```xml
<ItemGroup>
  <!-- HTTP Client para consumo de API -->
  <PackageReference Include="Microsoft.Extensions.Http" Version="8.0.0" />
  
  <!-- Autenticación y Sesiones -->
  <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.0" />
  <PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />
  
  <!-- Exportación de Reportes -->
  <PackageReference Include="ClosedXML" Version="0.102.2" />
  <PackageReference Include="QuestPDF" Version="2025.1.0" />
  
  <!-- Gráficas (opcional, para reportes) -->
  <PackageReference Include="Chart.js" Version="4.4.0" />
  
  <!-- JSON Serialization -->
  <PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
</ItemGroup>
```

---

## 🔐 Autenticación y Sesiones

### Configuración en `Program.cs`:
- Usar sesiones para almacenar el JWT token
- Configurar HttpClient para consumo de API
- Middleware para agregar token a requests

### Flujo de Autenticación:
1. Usuario ingresa email/password en Login
2. Se llama a `POST /api/auth/login`
3. Se recibe JWT token
4. Se almacena en sesión
5. Todos los requests a la API incluyen `Authorization: Bearer {token}`

---

## 📝 Plan de Implementación por Fases

### **FASE 1: Configuración Base e Infraestructura**
1. ✅ Configurar dependencias en `.csproj`
2. ✅ Configurar `Program.cs` con servicios HTTP, sesiones, autenticación
3. ✅ Crear `ApiService` base para consumo de API
4. ✅ Configurar `appsettings.json` con URL de API
5. ✅ Crear layout con Sidebar
6. ✅ Configurar manejo de errores y mensajes

### **FASE 2: Autenticación (RF-01, RF-02, RF-49)**
1. ✅ Crear `AuthController` y vistas (Login, Register)
2. ✅ Implementar `AuthService` para login/registro
3. ✅ Middleware para validar sesión y redirigir a login
4. ✅ Manejo de roles (EMPRENDEDOR/TRABAJADOR)

### **FASE 3: Configuración del Emprendimiento (RF-03 a RF-07)**
1. ✅ Crear `ConfiguracionController`
2. ✅ Implementar `EmpresaService` (CRUD empresas)
3. ✅ Vista de perfil del emprendimiento
4. ✅ Vista de edición (nombre, rubro, margen, logo)
5. ✅ Subida de logo (RF-04)
6. ✅ Gestión de sucursales

### **FASE 4: Inventario (RF-08, RF-11, RF-12, RF-13, RF-14)**
**NOTA: RF-09 y RF-10 (ML Kit) se omiten - solo móvil**

1. ✅ Crear `InventarioController`
2. ✅ Implementar `InventarioService` (CRUD insumos)
3. ✅ Vista de lista de insumos con filtros
4. ✅ Vista de registro manual (RF-08)
5. ✅ Vista de actualización de stock (RF-12)
6. ✅ Vista de historial de movimientos
7. ✅ Alertas de stock bajo (RF-14)
8. ✅ Eliminación lógica (RF-13)

### **FASE 5: Productos de Venta (RF-15 a RF-21)**
1. ✅ Crear `ProductosController`
2. ✅ Implementar `ProductoService` (CRUD productos)
3. ✅ Vista de lista de productos
4. ✅ Vista de crear/editar producto
5. ✅ Asociación de insumos con cantidades (RF-16)
6. ✅ Cálculo automático de precio sugerido (RF-17)
7. ✅ Validación de stock antes de venta (RF-16)
8. ✅ Descuento automático de inventario (RF-21)

### **FASE 6: Ventas (RF-22 a RF-31)**
1. ✅ Crear `VentasController`
2. ✅ Implementar `VentaService` (CRUD ventas)
3. ✅ Punto de Venta (POS) - RF-22
4. ✅ Validación de disponibilidad de stock (RF-23)
5. ✅ Cálculo automático del total (RF-24)
6. ✅ Registro del vendedor (RF-25)
7. ✅ Historial de ventas con filtros (RF-26)
8. ✅ Top de vendedores (RF-27)
9. ✅ Cancelación/corrección de ventas (RF-28)
10. ✅ Registro de venta con envío (RF-29)
11. ✅ Ventana de pedidos pendientes (RF-30)
12. ✅ Actualización automática de estado (RF-31)

### **FASE 7: Clientes (RF-26, RF-40)** ✅ COMPLETADA
1. ✅ Crear `ClientesController`
2. ✅ Implementar `ClienteService` (CRUD clientes)
3. ✅ Vista de lista de clientes
4. ✅ Vista de crear/editar cliente
5. ✅ Top clientes (RF-40)

### **FASE 8: Costos y Gastos (RF-32 a RF-37)** ✅ COMPLETADA
**NOTA: RF-33 (ML Kit) se omite - solo móvil**

1. ✅ Crear `CostosGastosController`
2. ✅ Implementar `CostoGastoService` (CRUD costos/gastos)
3. ✅ Vista de registro manual (RF-32)
4. ✅ Clasificación (fijo/variable, directo/administrativo) (RF-34)
5. ✅ Vista de lista con filtros (RF-36)
6. ✅ Edición y eliminación (RF-35)
7. ✅ Alerta de incremento de costos (RF-37)

### **FASE 9: Trabajadores (RF-46 a RF-52)** ✅ COMPLETADA
1. ✅ Crear `TrabajadoresController`
2. ✅ Implementar `TrabajadorService` (CRUD trabajadores)
3. ✅ Vista de lista de trabajadores
4. ✅ Vista de crear/editar trabajador (RF-46)
5. ✅ Creación de usuario vinculado (RF-48)
6. ✅ Desactivación de acceso (RF-52)
7. ✅ Reporte de desempeño (RF-51)

### **FASE 10: Reportes y Métricas (RF-38 a RF-45)** ✅ COMPLETADA
1. ✅ Crear `ReportesController`
2. ✅ Implementar `ReporteService`
3. ✅ Reporte de ventas (RF-38)
4. ✅ Reporte de costos y gastos (RF-39)
5. ✅ Top clientes y productos (RF-40)
6. ✅ Cálculo de margen de ganancia promedio (RF-41)
7. ✅ Cálculo del punto de equilibrio (RF-42)
8. ✅ Meta mensual de ventas (RF-43)
9. ✅ Comparativa mensual (RF-44)
10. ✅ Exportación a PDF (RF-45)

### **FASE 11: Categorías** ✅ COMPLETADA
1. ✅ Crear `CategoriasController`
2. ✅ Implementar `CategoriaService` (CRUD categorías)
3. ✅ Vista de lista de categorías
4. ✅ Vista de crear/editar categoría

### **FASE 12: Dashboard Principal (RF-07)** ✅ COMPLETADA
1. ✅ Crear `DashboardController`
2. ✅ Implementar vista con métricas clave
3. ✅ Gráficas rápidas
4. ✅ Alertas y notificaciones

---

## 🔌 Integración con API

### Endpoints Principales por Módulo:

#### Autenticación
- `POST /api/auth/login`
- `POST /api/auth/registro-emprendedor`
- `POST /api/auth/crear-trabajador`

#### Empresas
- `GET /api/empresas`
- `GET /api/empresas/{id}`
- `POST /api/empresas`
- `PUT /api/empresas/{id}`
- `DELETE /api/empresas/{id}`

#### Sucursales
- `GET /api/sucursales`
- `POST /api/sucursales`
- `PUT /api/sucursales/{id}`
- `DELETE /api/sucursales/{id}`

#### Insumos
- `GET /api/insumos`
- `POST /api/insumos`
- `PUT /api/insumos/{id}`
- `DELETE /api/insumos/{id}`

#### Registros de Inventario
- `GET /api/registrosinventario`
- `POST /api/registrosinventario`
- `GET /api/registrosinventario/por-insumo/{insumoId}`

#### Productos de Venta
- `GET /api/productosventa`
- `POST /api/productosventa`
- `PUT /api/productosventa/{id}`
- `DELETE /api/productosventa/{id}`

#### Insumo-Producto-Venta
- `GET /api/insumoproductoventa`
- `POST /api/insumoproductoventa`
- `GET /api/insumoproductoventa/producto/{productoVentaId}`

#### Ventas
- `GET /api/ventas`
- `POST /api/ventas`
- `PUT /api/ventas/{id}`
- `DELETE /api/ventas/{id}`

#### Detalle Ventas
- `GET /api/detalleventas`
- `POST /api/detalleventas`
- `GET /api/detalleventas/venta/{ventaId}`

#### Clientes
- `GET /api/clientes`
- `POST /api/clientes`
- `PUT /api/clientes/{id}`
- `DELETE /api/clientes/{id}`

#### Costos y Gastos
- `GET /api/costosgastos`
- `POST /api/costosgastos`
- `PUT /api/costosgastos/{id}`
- `DELETE /api/costosgastos/{id}`

#### Trabajadores
- `GET /api/trabajadores`
- `POST /api/trabajadores`
- `PUT /api/trabajadores/{id}`
- `DELETE /api/trabajadores/{id}`

#### Usuarios
- `GET /api/usuarios`
- `POST /api/usuarios`
- `PUT /api/usuarios/{id}`
- `DELETE /api/usuarios/{id}`

#### Categorías
- `GET /api/categorias`
- `POST /api/categorias`
- `PUT /api/categorias/{id}`
- `DELETE /api/categorias/{id}`

---

## 🎨 Diseño de Sidebar

El sidebar debe incluir:

```
┌─────────────────────────┐
│  🏢 Bizly                │
├─────────────────────────┤
│  📊 Dashboard           │
│  ⚙️  Configuración      │
│  📦 Inventario          │
│  🛍️  Productos          │
│  💰 Ventas              │
│  👥 Clientes            │
│  💸 Costos y Gastos     │
│  👷 Trabajadores       │
│  📈 Reportes            │
│  🏷️  Categorías         │
├─────────────────────────┤
│  👤 [Usuario]           │
│  🚪 Cerrar Sesión       │
└─────────────────────────┘
```

---

## 🔑 Consideraciones Importantes

1. **Base de Datos**: No se requiere conexión directa a MongoDB. La API maneja toda la persistencia.

2. **Autenticación**: Todos los endpoints (excepto login y registro) requieren JWT token en header `Authorization: Bearer {token}`

3. **Roles**:
   - **EMPRENDEDOR**: Acceso completo a todos los módulos
   - **TRABAJADOR**: Solo puede acceder a Ventas y Clientes

4. **ML Kit**: Las funcionalidades de escaneo (RF-09, RF-33) NO se implementan en la versión web.

5. **Sesiones**: Usar sesiones de ASP.NET Core para almacenar token y datos del usuario.

6. **Manejo de Errores**: Implementar manejo centralizado de errores de API y mostrar mensajes amigables al usuario.

7. **Validaciones**: Validar datos tanto en cliente (JavaScript) como antes de enviar a la API.

---

## 📊 Requerimientos Funcionales por Módulo

### Módulo 1: Autenticación
- ✅ RF-01: Registro de usuario (emprendedor)
- ✅ RF-02: Inicio de sesión
- ✅ RF-49: Inicio de sesión del trabajador

### Módulo 2: Configuración del Emprendimiento
- ✅ RF-03: Registro de emprendimiento
- ✅ RF-04: Subida del logotipo
- ✅ RF-05: Edición de datos del emprendimiento
- ✅ RF-06: Selección del margen de ganancia
- ✅ RF-07: Visualización del perfil del emprendimiento

### Módulo 3: Inventario Inteligente
- ✅ RF-08: Registro manual de insumos
- ❌ RF-09: Registro mediante cámara (ML Kit) - **OMITIDO (solo móvil)**
- ❌ RF-10: Edición de datos tras escaneo - **OMITIDO (solo móvil)**
- ✅ RF-11: Visualización del inventario
- ✅ RF-12: Actualización de stock (manual)
- ✅ RF-13: Eliminación de productos del inventario
- ✅ RF-14: Alertas de stock bajo

### Módulo 4: Productos de Venta
- ✅ RF-15: Creación de productos de venta
- ✅ RF-16: Asociación de insumos del inventario
- ✅ RF-17: Cálculo de precio sugerido
- ✅ RF-18: Edición de productos de venta
- ✅ RF-19: Deshabilitación de productos de venta
- ✅ RF-20: Visualización de productos registrados
- ✅ RF-21: Descuento automático del inventario

### Módulo 5: Ventas
- ✅ RF-22: Registro de venta
- ✅ RF-23: Validación de disponibilidad de stock
- ✅ RF-24: Cálculo automático del total de la venta
- ✅ RF-25: Registro del vendedor
- ✅ RF-26: Historial de ventas
- ✅ RF-27: Top de vendedores
- ✅ RF-28: Cancelación o corrección de ventas
- ✅ RF-29: Registro de venta con opción de envío
- ✅ RF-30: Ventana de pedidos pendientes
- ✅ RF-31: Actualización automática del estado del pedido

### Módulo 6: Costos y Gastos
- ✅ RF-32: Registro de costos y gastos
- ❌ RF-33: Registro mediante cámara (ML Kit) - **OMITIDO (solo móvil)**
- ✅ RF-34: Clasificación de costos fijos y variables
- ✅ RF-35: Edición y eliminación de registros financieros
- ✅ RF-36: Visualización de costos y gastos
- ✅ RF-37: Alerta de incremento de costos

### Módulo 7: Reportes y Métricas
- ✅ RF-38: Generación de reportes de ventas
- ✅ RF-39: Reporte de costos y gastos
- ✅ RF-40: Top de clientes y productos más vendidos
- ✅ RF-41: Cálculo del margen de ganancia promedio
- ✅ RF-42: Cálculo del punto de equilibrio
- ✅ RF-43: Meta mensual de ventas
- ✅ RF-44: Comparativa mensual de desempeño
- ✅ RF-45: Exportación de reportes

### Módulo 8: Trabajadores
- ✅ RF-46: Registro de trabajadores
- ✅ RF-47: Edición y eliminación de trabajadores
- ✅ RF-48: Creación de usuario vinculado al trabajador
- ✅ RF-49: Inicio de sesión del trabajador (ya cubierto en RF-02)
- ✅ RF-50: Registro de ventas por trabajador
- ✅ RF-51: Reporte de desempeño de trabajadores
- ✅ RF-52: Desactivación de usuario vinculado

---

## 🚀 Próximos Pasos Inmediatos

### **Fase 1: Configuración Base (Arquitectura de 3 Capas)**

1. **Configurar dependencias** en `BizlyWeb.csproj`
2. **Configurar `Program.cs`** con servicios HTTP, sesiones, autenticación

#### **Capa de Datos:**
3. **Crear `ApiService` base** (`Services/ApiService.cs`) para consumo de API
4. **Crear DTOs base** (`Models/DTOs/`) para comunicación con API

#### **Capa de Negocio:**
5. **Crear servicios base** (`Services/`) con estructura inicial

#### **Capa de Presentación:**
6. **Crear layout con Sidebar** (`Views/Shared/_Layout.cshtml`)
7. **Implementar módulo de Autenticación** (Controller + Views + Service)

---

## 📝 Notas Finales

- Este documento servirá como guía durante toda la implementación
- Se actualizará conforme se avance en el desarrollo
- Cada fase debe completarse antes de pasar a la siguiente
- Se debe probar cada módulo antes de continuar

