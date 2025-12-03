namespace BizlyWeb.Models.DTOs
{
    /// <summary>
    /// DTO para la respuesta del login
    /// </summary>
    public class LoginResponseDto
    {
        public string? Token { get; set; }
        public string? TipoUsuario { get; set; }
        public string? Nombre { get; set; }
        public string? Email { get; set; }
        public int? UsuarioId { get; set; }
        public int? EmpresaId { get; set; }
    }
}

