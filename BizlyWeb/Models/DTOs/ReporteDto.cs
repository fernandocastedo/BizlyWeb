namespace BizlyWeb.Models.DTOs
{
    /// <summary>
    /// DTO para Reporte de Ventas (RF-38)
    /// </summary>
    public class ReporteVentasDto
    {
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public int TotalVentas { get; set; }
        public decimal TotalIngresos { get; set; }
        public decimal PromedioVenta { get; set; }
        public List<VentasPorDiaDto> VentasPorDia { get; set; } = new();
    }

    public class VentasPorDiaDto
    {
        public DateTime Fecha { get; set; }
        public int Cantidad { get; set; }
        public decimal Total { get; set; }
    }

    /// <summary>
    /// DTO para Reporte de Costos y Gastos (RF-39)
    /// </summary>
    public class ReporteCostosGastosDto
    {
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public decimal TotalCostosGastos { get; set; }
        public decimal TotalDirectos { get; set; }
        public decimal TotalAdministrativos { get; set; }
        public decimal TotalFijos { get; set; }
        public decimal TotalVariables { get; set; }
        public List<CostosPorDiaDto> CostosPorDia { get; set; } = new();
    }

    public class CostosPorDiaDto
    {
        public DateTime Fecha { get; set; }
        public decimal Total { get; set; }
    }

    /// <summary>
    /// DTO para Top Productos (RF-40)
    /// </summary>
    public class TopProductoDto
    {
        public string ProductoId { get; set; } = string.Empty;
        public string ProductoNombre { get; set; } = string.Empty;
        public decimal CantidadVendida { get; set; }
        public decimal TotalIngresos { get; set; }
    }

    /// <summary>
    /// DTO para Top Clientes (RF-40)
    /// </summary>
    public class TopClienteDto
    {
        public string ClienteId { get; set; } = string.Empty;
        public string ClienteNombre { get; set; } = string.Empty;
        public int TotalCompras { get; set; }
        public decimal TotalGastado { get; set; }
    }

    /// <summary>
    /// DTO para Punto de Equilibrio (RF-42)
    /// </summary>
    public class PuntoEquilibrioDto
    {
        public decimal CostosFijos { get; set; }
        public decimal CostosVariables { get; set; }
        public decimal Ingresos { get; set; }
        public decimal PuntoEquilibrio { get; set; }
        public decimal MargenContribucion { get; set; } // Porcentaje
    }

    /// <summary>
    /// DTO para Meta Mensual (RF-43)
    /// </summary>
    public class MetaMensualDto
    {
        public decimal MetaMensual { get; set; }
        public decimal IngresosActuales { get; set; }
        public decimal PorcentajeMeta { get; set; }
        public decimal PorcentajeTiempo { get; set; }
        public decimal Proyeccion { get; set; }
        public decimal Diferencia { get; set; }
        public int Mes { get; set; }
        public int Año { get; set; }
    }

    /// <summary>
    /// DTO para Comparativa Mensual (RF-44)
    /// </summary>
    public class ComparativaMensualDto
    {
        public List<MesComparativaDto> Meses { get; set; } = new();
    }

    public class MesComparativaDto
    {
        public int Mes { get; set; }
        public int Año { get; set; }
        public string NombreMes { get; set; } = string.Empty;
        public decimal Ingresos { get; set; }
        public decimal Costos { get; set; }
        public decimal Ganancia { get; set; }
        public int TotalVentas { get; set; }
    }
}


