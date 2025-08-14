namespace HydroLink.Dtos
{
    public class ProductoModularDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public int CapacidadPorModulo { get; set; }
        public decimal PrecioBaseModulo { get; set; }
        public decimal PrecioModuloAdicional { get; set; }
        public string TipoPlanta { get; set; } = string.Empty;
        public string Dimensiones { get; set; } = string.Empty;
        public string Especificaciones { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public List<ComponenteModuloDto> ComponentesBase { get; set; } = new List<ComponenteModuloDto>();
        public List<ComponenteModuloDto> ComponentesAdicionales { get; set; } = new List<ComponenteModuloDto>();
    }

    public class ProductoModularCreateDto
    {
        public string Nombre { get; set; } = "Sistema Hidropónico HydroLink";
        public string Version { get; set; } = "v1.0";
        public string Descripcion { get; set; } = string.Empty;
        public int CapacidadPorModulo { get; set; } = 20;
        public decimal PrecioBaseModulo { get; set; }
        public decimal PrecioModuloAdicional { get; set; }
        public string TipoPlanta { get; set; } = "Lechugas, aromáticas, vegetales";
        public string Dimensiones { get; set; } = "100cm x 60cm x 180cm por módulo";
        public string Especificaciones { get; set; } = string.Empty;
    }

    public class ComponenteModuloDto
    {
        public int Id { get; set; }
        public int ComponenteId { get; set; }
        public string NombreComponente { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public decimal CantidadBase { get; set; }
        public decimal CantidadPorModuloAdicional { get; set; }
        public string TipoComponente { get; set; } = string.Empty;
        public string UnidadMedida { get; set; } = string.Empty;
        public decimal PrecioUnitario { get; set; }
        public string NotasInstalacion { get; set; } = string.Empty;
        public bool EsObligatorio { get; set; }
    }

    public class ComponenteModuloCreateDto
    {
        public int ComponenteId { get; set; }
        public decimal CantidadBase { get; set; }
        public decimal CantidadPorModuloAdicional { get; set; }
        public string TipoComponente { get; set; } = "BASE"; 
        public string NotasInstalacion { get; set; } = string.Empty;
        public bool EsObligatorio { get; set; } = true;
    }

    public class CotizacionModularRequestDto
    {
        public string NombreCliente { get; set; } = string.Empty;
        public string EmailCliente { get; set; } = string.Empty;
        public string TelefonoCliente { get; set; } = string.Empty;
        public string NombreProyecto { get; set; } = string.Empty;
        public int ProductoModularId { get; set; } = 1;
        public int CantidadModulos { get; set; } = 1;
        public string EspecificacionesEspeciales { get; set; } = string.Empty;
        public decimal? PorcentajeGananciaPersonalizado { get; set; }
    }

    public class CotizacionModularResponseDto
    {
        public int Id { get; set; }
        public string NombreCliente { get; set; } = string.Empty;
        public string EmailCliente { get; set; } = string.Empty;
        public string TelefonoCliente { get; set; } = string.Empty;
        public string NombreProyecto { get; set; } = string.Empty;
        public ProductoModularInfoDto ProductoInfo { get; set; } = new ProductoModularInfoDto();
        public int CantidadModulos { get; set; }
        public int CapacidadTotalPlantas { get; set; }
        public decimal SubtotalComponentes { get; set; }
        public decimal SubtotalManoObra { get; set; }
        public decimal SubtotalMateriales { get; set; }
        public decimal TotalEstimado { get; set; }
        public decimal PorcentajeGanancia { get; set; }
        public decimal MontoGanancia { get; set; }
        public string Estado { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public List<CotizacionModularDetalleDto> ComponentesCalculados { get; set; } = new List<CotizacionModularDetalleDto>();
        public ResumenModularDto Resumen { get; set; } = new ResumenModularDto();
    }

    public class ProductoModularInfoDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public int CapacidadPorModulo { get; set; }
        public string TipoPlanta { get; set; } = string.Empty;
        public string Dimensiones { get; set; } = string.Empty;
    }

    public class CotizacionModularDetalleDto
    {
        public string NombreComponente { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public decimal CantidadBase { get; set; }
        public decimal CantidadAdicional { get; set; }
        public decimal CantidadTotal { get; set; }
        public string UnidadMedida { get; set; } = string.Empty;
        public decimal PrecioUnitario { get; set; }
        public decimal SubtotalBase { get; set; }
        public decimal SubtotalAdicional { get; set; }
        public decimal SubtotalTotal { get; set; }
        public string TipoComponente { get; set; } = string.Empty;
        public string NotasInstalacion { get; set; } = string.Empty;
    }

    public class ResumenModularDto
    {
        public string TituloProyecto { get; set; } = string.Empty;
        public string DescripcionCapacidad { get; set; } = string.Empty;
        public string DescripcionDimensiones { get; set; } = string.Empty;
        public List<ResumenCategoriaModularDto> PorCategoria { get; set; } = new List<ResumenCategoriaModularDto>();
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

    public class ResumenCategoriaModularDto
    {
        public string Categoria { get; set; } = string.Empty;
        public int CantidadItems { get; set; }
        public decimal Subtotal { get; set; }
        public string Descripcion { get; set; } = string.Empty;
    }
}
