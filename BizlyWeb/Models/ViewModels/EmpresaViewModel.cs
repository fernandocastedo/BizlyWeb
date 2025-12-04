namespace BizlyWeb.Models.ViewModels
{
    public class EmpresaViewModel
    {
        public string? Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Rubro { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal MargenGanancia { get; set; }
        public string? LogoUrl { get; set; }
    }
}


