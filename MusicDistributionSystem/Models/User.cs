using System.ComponentModel.DataAnnotations;
using MusicDistributionSystem.Enums;

namespace MusicDistributionSystem.Models
{
    public class User
    {
        public Guid Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Role { get; set; } = "User";

        public MembershipTier MembershipTier { get; set; } = MembershipTier.Free;

        public bool IsEmailVerified { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
