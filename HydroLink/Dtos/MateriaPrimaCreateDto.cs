using System.ComponentModel.DataAnnotations;

public class MateriaPrimaCreateDto
{
    [Required]
    [MaxLength(100)]
    public string Nombre { get; set; }

    [MaxLength(50)]
    public string UnidadMedida { get; set; }
}
