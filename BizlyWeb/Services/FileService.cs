using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace BizlyWeb.Services
{
    /// <summary>
    /// Servicio para manejo de archivos - Capa de Negocio
    /// </summary>
    public class FileService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FileService> _logger;
        private const long MaxFileSize = 20 * 1024 * 1024; // 20MB
        private readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        public FileService(IWebHostEnvironment environment, ILogger<FileService> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        /// <summary>
        /// Guarda un archivo de imagen (logo) y retorna la URL relativa
        /// </summary>
        public async Task<string?> SaveLogoAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0)
            {
                return null;
            }

            try
            {
                // Validar tamaño
                if (file.Length > MaxFileSize)
                {
                    throw new Exception($"El archivo excede el tamaño máximo permitido de {MaxFileSize / (1024 * 1024)}MB");
                }

                // Validar extensión
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!AllowedExtensions.Contains(extension))
                {
                    throw new Exception($"Tipo de archivo no permitido. Formatos permitidos: {string.Join(", ", AllowedExtensions)}");
                }

                // Crear directorio si no existe
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "logos");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Generar nombre único para el archivo
                var fileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                // Guardar archivo
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Retornar URL relativa
                return $"/uploads/logos/{fileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar logo: {FileName}", file?.FileName);
                throw;
            }
        }

        /// <summary>
        /// Elimina un archivo de logo
        /// </summary>
        public void DeleteLogo(string? logoUrl)
        {
            if (string.IsNullOrEmpty(logoUrl))
            {
                return;
            }

            try
            {
                var filePath = Path.Combine(_environment.WebRootPath, logoUrl.TrimStart('/'));
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar logo: {LogoUrl}", logoUrl);
                // No lanzar excepción, solo registrar el error
            }
        }
    }
}

