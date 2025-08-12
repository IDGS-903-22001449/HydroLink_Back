namespace HydroLink.Dtos
{
    public class CotizacionHidroponicaCreateDto
    {
        public string NombreProyecto { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string NombreCliente { get; set; } = string.Empty;
        public string EmailCliente { get; set; } = string.Empty;
        public string TelefonoCliente { get; set; } = string.Empty;
        public decimal PorcentajeGanancia { get; set; } = 20.0m;
        public DateTime? FechaVencimiento { get; set; }
        public string Observaciones { get; set; } = string.Empty;
        public List<CotizacionDetalleItemDto> Componentes { get; set; } = new List<CotizacionDetalleItemDto>();
    }

    public class CotizacionDetalleItemDto
    {
        public int? ComponenteId { get; set; }
        public string NombreItem { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public decimal Cantidad { get; set; }
        public string UnidadMedida { get; set; } = string.Empty;
        public decimal PrecioUnitarioEstimado { get; set; }
        public string Especificaciones { get; set; } = string.Empty;
    }

    public class CotizacionHidroponicaResponseDto
    {
        public int Id { get; set; }
        public string NombreProyecto { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public string NombreCliente { get; set; } = string.Empty;
        public string EmailCliente { get; set; } = string.Empty;
        public string TelefonoCliente { get; set; } = string.Empty;
        public decimal SubtotalComponentes { get; set; }
        public decimal SubtotalManoObra { get; set; }
        public decimal SubtotalMateriales { get; set; }
        public decimal TotalEstimado { get; set; }
        public decimal PorcentajeGanancia { get; set; }
        public string Estado { get; set; } = string.Empty;
        public DateTime? FechaVencimiento { get; set; }
        public string Observaciones { get; set; } = string.Empty;
        public List<CotizacionDetalleResponseDto> Detalles { get; set; } = new List<CotizacionDetalleResponseDto>();
        public ResumenCostosDto ResumenCostos { get; set; } = new ResumenCostosDto();
    }

    public class CotizacionDetalleResponseDto
    {
        public int Id { get; set; }
        public string NombreItem { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public decimal Cantidad { get; set; }
        public string UnidadMedida { get; set; } = string.Empty;
        public decimal PrecioUnitarioEstimado { get; set; }
        public decimal Subtotal { get; set; }
        public string Especificaciones { get; set; } = string.Empty;
    }

    public class ResumenCostosDto
    {
        public List<CategoriaResumenDto> PorCategoria { get; set; } = new List<CategoriaResumenDto>();
        public decimal TotalSensores { get; set; }
        public decimal TotalRiego { get; set; }
        public decimal TotalProteccion { get; set; }
        public decimal TotalManoObra { get; set; }
        public decimal TotalMateriales { get; set; }
        public decimal Subtotal { get; set; }
        public decimal PorcentajeGanancia { get; set; }
        public decimal MontoGanancia { get; set; }
        public decimal Total { get; set; }
    }

    public class CategoriaResumenDto
    {
        public string Categoria { get; set; } = string.Empty;
        public int CantidadItems { get; set; }
        public decimal Subtotal { get; set; }
    }

    public class CotizacionPromedioRequestDto
    {
        public string NombreProyecto { get; set; } = string.Empty;
        public string NombreCliente { get; set; } = string.Empty;
        public string EmailCliente { get; set; } = string.Empty;
        public string TelefonoCliente { get; set; } = string.Empty;
        public List<ComponenteSolicitadoDto> ComponentesSolicitados { get; set; } = new List<ComponenteSolicitadoDto>();
        public decimal? PorcentajeGananciaPersonalizado { get; set; }
    }

    public class ComponenteSolicitadoDto
    {
        public string Categoria { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public decimal Cantidad { get; set; } = 1;
        public string Especificaciones { get; set; } = string.Empty;
    }
}
