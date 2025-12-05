namespace BizlyWeb.Models.DTOs
{
    /// <summary>
    /// DTO para Venta - Capa de Datos
    /// </summary>
    public class VentaDto
    {
        public string? Id { get; set; }
        public string EmpresaId { get; set; } = string.Empty;
        public string SucursalId { get; set; } = string.Empty;
        public string UsuarioId { get; set; } = string.Empty;
        public string UsuarioNombre { get; set; } = string.Empty;
        public string? ClienteId { get; set; }
        public DateTime Fecha { get; set; }
        public string MetodoPago { get; set; } = string.Empty; // "efectivo" o "qr"
        public decimal Total { get; set; }
        public bool EsEnvio { get; set; }
        public string EstadoPago { get; set; } = "pagado"; // "pagado" / "pendiente"
        public string EstadoPedido { get; set; } = "completado"; // "pendiente" / "completado" / "cancelado"
        public DateTime CreatedAt { get; set; }
    }
}


