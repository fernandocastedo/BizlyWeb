using BizlyWeb.Models.DTOs;

namespace BizlyWeb.Models.ViewModels
{
    /// <summary>
    /// ViewModel para el Dashboard Principal - Capa de Presentación
    /// </summary>
    public class DashboardViewModel
    {
        // Métricas del día
        public decimal VentasDelDia { get; set; }
        public int VentasDelDiaCantidad { get; set; }
        
        // Métricas del mes
        public decimal VentasDelMes { get; set; }
        public int VentasDelMesCantidad { get; set; }
        public decimal CostosDelMes { get; set; }
        public decimal GananciaDelMes { get; set; }
        
        // Inventario
        public int ProductosActivos { get; set; }
        public int InsumosConStockBajo { get; set; }
        public int TotalInsumos { get; set; }
        
        // Clientes y Trabajadores
        public int TotalClientes { get; set; }
        public int TotalTrabajadores { get; set; }
        
        // Top productos y clientes
        public List<TopProductoDto> TopProductos { get; set; } = new();
        public List<TopClienteDto> TopClientes { get; set; } = new();
        
        // Ventas recientes (últimas 5)
        public List<VentaResumenDto> VentasRecientes { get; set; } = new();
        
        // Alertas
        public List<AlertaDashboardDto> Alertas { get; set; } = new();
        
        // Datos para gráficas
        public List<VentasPorDiaDto> VentasUltimos7Dias { get; set; } = new();
    }

    /// <summary>
    /// DTO para resumen de venta en el dashboard
    /// </summary>
    public class VentaResumenDto
    {
        public string Id { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public decimal Total { get; set; }
        public string ClienteNombre { get; set; } = string.Empty;
        public string EstadoPedido { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para alertas en el dashboard
    /// </summary>
    public class AlertaDashboardDto
    {
        public string Tipo { get; set; } = string.Empty; // "warning", "danger", "info"
        public string Titulo { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public string Icono { get; set; } = string.Empty;
    }
}

