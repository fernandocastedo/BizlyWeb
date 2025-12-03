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
        public string? UsuarioId { get; set; }
        public string? EmpresaId { get; set; }
        public string? SucursalId { get; set; }
    }
}

