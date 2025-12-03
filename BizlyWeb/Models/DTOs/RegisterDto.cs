namespace BizlyWeb.Models.DTOs
{
    /// <summary>
    /// DTO para el request de registro de emprendedor
    /// </summary>
    public class RegisterDto
    {
        // Datos del emprendimiento
        public string NombreEmpresa { get; set; } = string.Empty;
        public string Rubro { get; set; } = string.Empty;
        public string? DescripcionEmpresa { get; set; }
        public decimal MargenGanancia { get; set; }
        public string? LogoUrl { get; set; }
        public string? NombreSucursal { get; set; }
        public string? DireccionSucursal { get; set; }
        public string? CiudadSucursal { get; set; }
        public string? DepartamentoSucursal { get; set; }
        
        // Datos del usuario
        public string NombreUsuario { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}

