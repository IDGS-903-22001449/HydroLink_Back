using System.ComponentModel.DataAnnotations;

namespace HydroLink.Dtos
{
    public class CreateRoleDto
    {
        [Required]
        public string RoleName { get; set; } = string.Empty;
    }
}
