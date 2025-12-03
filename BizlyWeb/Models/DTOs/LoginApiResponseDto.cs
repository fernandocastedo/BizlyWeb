using System.Text.Json.Serialization;

namespace BizlyWeb.Models.DTOs
{
    /// <summary>
    /// DTO para la respuesta de la API de login (estructura anidada)
    /// </summary>
    public class LoginApiResponseDto
    {
        [JsonPropertyName("token")]
        public string? Token { get; set; }
        
        [JsonPropertyName("usuario")]
        public UsuarioApiDto? Usuario { get; set; }
    }

    /// <summary>
    /// DTO para el objeto usuario dentro de la respuesta de login
    /// </summary>
    public class UsuarioApiDto
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }
        
        [JsonPropertyName("nombre")]
        public string? Nombre { get; set; }
        
        [JsonPropertyName("email")]
        public string? Email { get; set; }
        
        [JsonPropertyName("tipoUsuario")]
        public string? TipoUsuario { get; set; }
        
        [JsonPropertyName("empresaId")]
        public string? EmpresaId { get; set; }
        
        [JsonPropertyName("sucursalId")]
        public string? SucursalId { get; set; }
        
        [JsonPropertyName("trabajadorId")]
        public string? TrabajadorId { get; set; }
        
        [JsonPropertyName("activo")]
        public bool Activo { get; set; } = true;
    }
}

