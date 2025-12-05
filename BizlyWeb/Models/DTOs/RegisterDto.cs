using System.Text.Json.Serialization;

namespace BizlyWeb.Models.DTOs
{
    /// <summary>
    /// DTO para el request de registro de emprendedor
    /// NO incluye IDs - estos se generan automáticamente en la API
    /// </summary>
    public class RegisterDto
    {
        // Datos del emprendimiento
        [JsonPropertyName("nombreEmpresa")]
        public string NombreEmpresa { get; set; } = string.Empty;
        
        [JsonPropertyName("rubro")]
        public string Rubro { get; set; } = string.Empty;
        
        [JsonPropertyName("descripcionEmpresa")]
        public string? DescripcionEmpresa { get; set; }
        
        [JsonPropertyName("margenGanancia")]
        public decimal MargenGanancia { get; set; }
        
        [JsonPropertyName("logoUrl")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? LogoUrl { get; set; }
        
        [JsonPropertyName("nombreSucursal")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? NombreSucursal { get; set; }
        
        [JsonPropertyName("direccionSucursal")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? DireccionSucursal { get; set; }
        
        [JsonPropertyName("ciudadSucursal")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? CiudadSucursal { get; set; }
        
        [JsonPropertyName("departamentoSucursal")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? DepartamentoSucursal { get; set; }
        
        // Datos del usuario
        [JsonPropertyName("nombreUsuario")]
        public string NombreUsuario { get; set; } = string.Empty;
        
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;
        
        [JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty;
    }
}

