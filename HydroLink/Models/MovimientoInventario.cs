using System;
using System.ComponentModel.DataAnnotations;
using HydroLink.Models;

namespace HydroLink.Models
{
    public class MovimientoInventario
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int MateriaPrimaId { get; set; }

        [Required]
        public DateTime FechaMovimiento { get; set; }

        [Required]
        public string TipoMovimiento { get; set; }

        public int Cantidad { get; set; }

        public decimal PrecioUnitario { get; set; }

        public decimal CostoTotal { get; set; }

        public string NumeroLote { get; set; }

        public string? Proveedor { get; set; }
        
        public int? CompraId { get; set; }

        public string? Observaciones { get; set; }

        public MateriaPrima MateriaPrima { get; set; }

        public Compra Compra { get; set; }
    }
}
