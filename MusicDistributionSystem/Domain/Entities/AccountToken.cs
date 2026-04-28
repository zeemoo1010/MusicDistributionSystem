using System.ComponentModel.DataAnnotations;
using MusicDistributionSystem.Domain.Enums;

namespace MusicDistributionSystem.Domain.Entities
{
    public class AccountToken
    {
        public Guid Id { get; set; } = Guid.NewGuid();

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

