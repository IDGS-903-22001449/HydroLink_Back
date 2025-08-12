namespace HydroLink.Dtos
{
    public class CostoPromedioInfoDto
    {
        public int MateriaPrimaId { get; set; }
        public string NombreMateriaPrima { get; set; } = string.Empty;
        public string UnidadMedida { get; set; } = string.Empty;
        public decimal CostoPromedioActual { get; set; }
        public int ExistenciaActual { get; set; }
        public decimal ValorInventarioTotal { get; set; }
        public DateTime FechaUltimaActualizacion { get; set; }
        public string? ActualizadoPor { get; set; }
        public bool TieneDatos { get; set; }
        public string? Observaciones { get; set; }
    }
}
