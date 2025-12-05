# Etapa 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar archivo del proyecto para restaurar dependencias
COPY ["BizlyWeb/BizlyWeb.csproj", "BizlyWeb/"]
RUN dotnet restore "BizlyWeb/BizlyWeb.csproj"

# Copiar todo el código fuente
COPY . .

# Build de la aplicación
WORKDIR "/src/BizlyWeb"
RUN dotnet build "BizlyWeb.csproj" -c Release -o /app/build

# Etapa 2: Publish
FROM build AS publish
WORKDIR "/src/BizlyWeb"
RUN dotnet publish "BizlyWeb.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Etapa 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Crear directorio para uploads
RUN mkdir -p /app/wwwroot/uploads

# Copiar archivos publicados
COPY --from=publish /app/publish .

# Exponer el puerto (Render.com usará la variable de entorno PORT)
EXPOSE 8080

# Variables de entorno
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Punto de entrada
ENTRYPOINT ["dotnet", "BizlyWeb.dll"]

