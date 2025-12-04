namespace BizlyWeb.Helpers
{
    /// <summary>
    /// Helper para formatear valores monetarios en Bolivianos (Bs.)
    /// </summary>
    public static class CurrencyHelper
    {
        /// <summary>
        /// Formatea un valor decimal como moneda boliviana (Bs.)
        /// Ejemplo: 100.50 -> "100.50 Bs."
        /// </summary>
        public static string ToBolivianos(this decimal value)
        {
            return $"{value:N2} Bs.";
        }

        /// <summary>
        /// Formatea un valor decimal nullable como moneda boliviana (Bs.)
        /// </summary>
        public static string ToBolivianos(this decimal? value)
        {
            return value.HasValue ? value.Value.ToBolivianos() : "0.00 Bs.";
        }
    }
}

