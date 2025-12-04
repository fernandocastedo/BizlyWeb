namespace BizlyWeb.Models.ViewModels
{
    /// <summary>
    /// ViewModel para el Punto de Venta (POS) - Capa de Datos
    /// </summary>
    public class PuntoVentaViewModel
    {
        public List<ProductoVentaViewModel> Productos { get; set; } = new();
        public List<ClienteViewModel> Clientes { get; set; } = new();
        public List<ItemCarritoViewModel> Carrito { get; set; } = new();
        public decimal Total { get; set; }
        public string? ClienteId { get; set; }
        public string MetodoPago { get; set; } = "efectivo";
        public bool EsEnvio { get; set; }
    }

    /// <summary>
    /// ViewModel para items del carrito en el POS
    /// </summary>
    public class ItemCarritoViewModel
    {
        public string ProductoId { get; set; } = string.Empty;
        public string ProductoNombre { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
        public bool StockSuficiente { get; set; }
    }
}


