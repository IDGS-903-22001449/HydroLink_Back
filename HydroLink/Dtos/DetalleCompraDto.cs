namespace HydroLink.Dtos
{
    public class DetalleCompraDto
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public int ProveedorId { get; set; }
        public string NombreProveedor { get; set; } = string.Empty;
        public string EmpresaProveedor { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public int CantidadItems { get; set; }
        public List<CompraDetalleInfo> Detalles { get; set; } = new List<CompraDetalleInfo>();
    }

    public class CompraDetalleInfo
    {
        public int Id { get; set; }
        public int MateriaPrimaId { get; set; }
        public string NombreMateriaPrima { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }
}
