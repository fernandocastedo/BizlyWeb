namespace BizlyWeb.Models.DTOs
{
    /// <summary>
    /// DTO para la respuesta de la API de login (estructura anidada)
    /// </summary>
    public class LoginApiResponseDto
    {
        public string? Token { get; set; }
        public UsuarioApiDto? Usuario { get; set; }
    }

    /// <summary>
    /// DTO para el objeto usuario dentro de la respuesta de login
    /// </summary>
    public class UsuarioApiDto
    {
        public string? Id { get; set; }
        public string? Nombre { get; set; }
        public string? Email { get; set; }
        public string? TipoUsuario { get; set; }
        public string? EmpresaId { get; set; }
        public string? SucursalId { get; set; }
        public string? TrabajadorId { get; set; }
    }
}

