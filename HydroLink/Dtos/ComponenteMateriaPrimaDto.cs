namespace HydroLink.Dtos
{
    public class ComponenteMateriaPrimaDto
    {
        public int ComponenteId { get; set; }
        public int MateriaPrimaId { get; set; }
        public decimal CantidadNecesaria { get; set; } = 1.0m;
        public decimal FactorConversion { get; set; } = 1.0m;
        public decimal PorcentajeMerma { get; set; } = 0.0m;
        public bool EsPrincipal { get; set; } = true;
        public string Notas { get; set; } = string.Empty;
    }
}
