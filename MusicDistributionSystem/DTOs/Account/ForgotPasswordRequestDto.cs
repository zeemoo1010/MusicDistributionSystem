using System.ComponentModel.DataAnnotations;

namespace MusicDistributionSystem.DTOs.Account
{
    public class ForgotPasswordRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
