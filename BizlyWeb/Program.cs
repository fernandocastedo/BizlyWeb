using BizlyWeb.Services;
using BizlyWeb.Filters;
using BizlyWeb.Middleware;
using BizlyWeb.DataProtection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    // Agregar filtro global para manejo de excepciones
    options.Filters.Add<ApiExceptionFilter>();
});

// Configurar límites para subida de archivos
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 20971520; // 20MB
    options.ValueLengthLimit = 20971520; // 20MB
});

// Configurar límites de Kestrel para requests grandes
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 20971520; // 20MB
});

// Configurar Data Protection para producción (Render.com/Docker)
// En contenedores, usar almacenamiento en memoria en lugar del sistema de archivos
// Esto evita warnings sobre persistencia no disponible
if (builder.Environment.IsProduction())
{
    // Crear un repositorio en memoria para evitar warnings de persistencia
    var inMemoryRepository = new InMemoryXmlRepository();
    builder.Services.AddDataProtection()
        .SetApplicationName("BizlyWeb")
        .PersistKeysToRepository(inMemoryRepository);
}
else
{
    // En desarrollo, usar configuración por defecto
    builder.Services.AddDataProtection()
        .SetApplicationName("BizlyWeb");
}

// Configurar sesiones para almacenar JWT token
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax; // Lax es más compatible con HTTPS
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // Secure en HTTPS, no en HTTP
});

// Configurar HttpContextAccessor para acceder a la sesión desde servicios
builder.Services.AddHttpContextAccessor();

// Configurar puerto desde variable de entorno (Render.com/Docker)
// Render.com inyecta la variable PORT, debemos usarla con prioridad
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
{
    // Render.com asigna un puerto dinámico, usarlo directamente
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
    // También establecer la variable de entorno para que ASP.NET Core la use
    Environment.SetEnvironmentVariable("ASPNETCORE_URLS", $"http://0.0.0.0:{port}");
}

// Configurar HttpClient para consumo de API
builder.Services.AddHttpClient<ApiService>(client =>
{
    var baseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "https://apibizly.onrender.com";
    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Registrar servicios de la Capa de Negocio
builder.Services.AddScoped<ApiService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<FileService>();
builder.Services.AddScoped<EmpresaService>();
builder.Services.AddScoped<SucursalService>();
builder.Services.AddScoped<InventarioService>();
builder.Services.AddScoped<ProductoService>();
builder.Services.AddScoped<VentaService>();
builder.Services.AddScoped<ClienteService>();
builder.Services.AddScoped<CostoGastoService>();
builder.Services.AddScoped<TrabajadorService>();
builder.Services.AddScoped<ReporteService>();
builder.Services.AddScoped<CategoriaService>();

var app = builder.Build();

// Manejo global de excepciones no capturadas
AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
{
    try
    {
        var ex = args.ExceptionObject as Exception;
        Console.WriteLine($"[FATAL] UnhandledException: {ex?.Message}\n{ex?.StackTrace}");
    }
    catch { }
};

TaskScheduler.UnobservedTaskException += (sender, args) =>
{
    try
    {
        Console.WriteLine($"[FATAL] UnobservedTaskException: {args.Exception?.Message}\n{args.Exception?.StackTrace}");
        args.SetObserved(); // Marcar como observado para evitar crash
    }
    catch { }
};

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // Deshabilitar HSTS si no hay HTTPS configurado (Render.com puede usar proxy reverso)
    // app.UseHsts(); // Comentado porque Render.com maneja HTTPS en el proxy
}
else
{
    // En desarrollo, usar página de errores detallada
    app.UseDeveloperExceptionPage();
}

// Solo usar HTTPS redirection si estamos en desarrollo o si hay HTTPS configurado
// En Render.com, el proxy maneja HTTPS, así que no necesitamos redirección
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
// En producción, Render.com maneja HTTPS en el proxy, no redirigir

app.UseStaticFiles();

app.UseRouting();

// Usar sesiones antes de autorización y middleware
app.UseSession();

// Usar middleware de autenticación personalizado
app.UseAuthMiddleware();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();
