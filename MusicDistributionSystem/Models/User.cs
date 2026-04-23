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

        public MembershipTier MembershipTier { get; set; } = MembershipTier.Free;

        public bool IsEmailVerified { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public DateTime? LastLoginAtUtc { get; set; }

        public ICollection<AccountToken> AccountTokens { get; set; } = new List<AccountToken>();

        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

        public ICollection<MusicTrack> UploadedTracks { get; set; } = new List<MusicTrack>();
    }
}
