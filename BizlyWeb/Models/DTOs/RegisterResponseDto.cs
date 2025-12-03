namespace BizlyWeb.Models.DTOs
{
    /// <summary>
    /// DTO para la respuesta del registro
    /// </summary>
    public class RegisterResponseDto
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public LoginResponseDto? Usuario { get; set; }
    }
}

