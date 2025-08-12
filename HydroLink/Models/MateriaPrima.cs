using HydroLink.Models;
using System.ComponentModel.DataAnnotations;

public class MateriaPrima
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Name { get; set; }
    public decimal CostoUnitario { get; set; } = 0;

    public int Stock { get; set; } = 0;

    [MaxLength(50)]
    public string UnidadMedida { get; set; }

    public ICollection<CompraDetalle> Compras { get; set; }
    public ICollection<ListaMaterial> Explosiones { get; set; }
}
