using System.ComponentModel.DataAnnotations;

namespace HydroLink.Dtos
{
    public class TokenDto
    {
        [Required]
        public string Token { get; set; } = string.Empty;
        
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
