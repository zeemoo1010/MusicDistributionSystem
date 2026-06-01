using MusicDistributionSystem.Domain.Common;
using MusicDistributionSystem.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace MusicDistributionSystem.Domain.Entities
{
    public class AccountToken : BaseEntity
    {
        public Guid UserId { get; set; }

        public User? User { get; set; }

        public AccountTokenType Type { get; set; }

        [Required]
        [StringLength(500)]
        public string TokenHash { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string Destination { get; set; } = string.Empty;

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public DateTime ExpiresAtUtc { get; set; }

        public DateTime? ConsumedAtUtc { get; set; }
    }
}

