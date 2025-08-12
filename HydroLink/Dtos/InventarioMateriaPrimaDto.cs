namespace HydroLink.Dtos
{
    public class InventarioMateriaPrimaDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string UnidadMedida { get; set; } = string.Empty;
        public int StockActual { get; set; }
        public decimal CostoUnitarioPromedio { get; set; }
        public decimal ValorTotalInventario { get; set; }
        public int StockMinimo { get; set; }
        public int StockMaximo { get; set; }
        public string EstadoStock { get; set; } = string.Empty; // Crítico, Bajo, Normal, Alto
        public DateTime? FechaUltimaCompra { get; set; }
        public decimal? UltimoPrecioCompra { get; set; }
        public List<MovimientoInventarioDto> MovimientosRecientes { get; set; } = new List<MovimientoInventarioDto>();
        public List<LoteInventarioDto> Lotes { get; set; } = new List<LoteInventarioDto>();
    }

    public class MovimientoInventarioDto
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string TipoMovimiento { get; set; } = string.Empty; // Entrada, Salida
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public string Concepto { get; set; } = string.Empty; // Compra, Producción, Ajuste
        public string? Proveedor { get; set; }
        public int? CompraId { get; set; }
    }

    public class LoteInventarioDto
    {
        public int Id { get; set; }
        public string NumeroLote { get; set; } = string.Empty;
        public DateTime FechaIngreso { get; set; }
        public int CantidadInicial { get; set; }
        public int CantidadDisponible { get; set; }
        public decimal CostoUnitario { get; set; }
        public decimal CostoTotal { get; set; }
        public string? Proveedor { get; set; }
        public int? CompraId { get; set; }
    }

    public class ResumenInventarioDto
    {
        public int TotalMateriasPrimas { get; set; }
        public decimal ValorTotalInventario { get; set; }
        public int MaterialesBajoStock { get; set; }
        public int MaterialesStockCritico { get; set; }
        public int LotesPorVencer { get; set; }
        public int LotesVencidos { get; set; }
        public List<MaterialBajoStockDto> MaterialesCriticos { get; set; } = new List<MaterialBajoStockDto>();
        public List<LoteProximoVencerDto> LotesProximosVencer { get; set; } = new List<LoteProximoVencerDto>();
    }

    public class MaterialBajoStockDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int StockActual { get; set; }
        public int StockMinimo { get; set; }
        public string EstadoStock { get; set; } = string.Empty;
    }

    public class LoteProximoVencerDto
    {
        public int Id { get; set; }
        public string NumeroLote { get; set; } = string.Empty;
        public string MateriaPrima { get; set; } = string.Empty;
        public DateTime FechaVencimiento { get; set; }
        public int DiasParaVencer { get; set; }
        public int CantidadDisponible { get; set; }
    }

    public class AjusteInventarioDto
    {
        public int MateriaPrimaId { get; set; }
        public int CantidadAjuste { get; set; } // Positivo para aumentar, negativo para disminuir
        public string Motivo { get; set; } = string.Empty;
        public string? Observaciones { get; set; }
    }
}
