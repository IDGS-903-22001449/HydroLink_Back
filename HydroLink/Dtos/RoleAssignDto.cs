using System.ComponentModel.DataAnnotations;

namespace HydroLink.Dtos
{
    public class RoleAssignDto
    {
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        [Required]
        public string RoleId { get; set; } = string.Empty;
    }
}
