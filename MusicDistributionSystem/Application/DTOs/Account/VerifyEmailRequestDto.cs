using System.ComponentModel.DataAnnotations;

namespace MusicDistributionSystem.Application.DTOs.Account
{
    public class VerifyEmailRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Verification code")]
        [StringLength(6, MinimumLength = 6)]
        public string Code { get; set; } = string.Empty;
    }
}

