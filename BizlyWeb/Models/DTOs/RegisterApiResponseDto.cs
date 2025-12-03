namespace BizlyWeb.Models.DTOs
{
    /// <summary>
    /// DTO para la respuesta de la API de registro (estructura anidada)
    /// </summary>
    public class RegisterApiResponseDto
    {
        public UsuarioApiDto? Usuario { get; set; }
    }
}

