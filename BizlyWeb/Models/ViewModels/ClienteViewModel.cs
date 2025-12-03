namespace BizlyWeb.Models.ViewModels
{
    /// <summary>
    /// ViewModel para Cliente - Capa de Datos
    /// </summary>
    public class ClienteViewModel
    {
        public string? Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int Nit { get; set; }
        public string Telefono { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
    }
}

