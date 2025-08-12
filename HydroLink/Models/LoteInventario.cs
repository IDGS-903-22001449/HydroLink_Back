using System;
using System.ComponentModel.DataAnnotations;
using HydroLink.Models;

namespace HydroLink.Models
{
    public class LoteInventario
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string NumeroLote { get; set; } = string.Empty;

        [Required]
        public int MateriaPrimaId { get; set; }

        [Required]
        public DateTime FechaIngreso { get; set; }

        [Required]
        public int CantidadInicial { get; set; }

        [Required]
        public int CantidadDisponible { get; set; }

        [Required]
        public decimal CostoUnitario { get; set; }

        [Required]
        public decimal CostoTotal { get; set; }

        public int? ProveedorId { get; set; }

        public int? CompraId { get; set; }
        public string? ActualizadoPor { get; set; }

        public MateriaPrima MateriaPrima { get; set; }
        
        public Proveedor? Proveedor { get; set; }
        
        public Compra? Compra { get; set; }

        public ICollection<MovimientoInventario> Movimientos { get; set; } = new List<MovimientoInventario>();

    }
}
