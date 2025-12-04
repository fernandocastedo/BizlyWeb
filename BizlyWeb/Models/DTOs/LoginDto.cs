namespace BizlyWeb.Models.DTOs
{
    /// <summary>
    /// DTO para el request de login
    /// </summary>
    public class LoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}


