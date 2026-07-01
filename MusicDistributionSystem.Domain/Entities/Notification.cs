using MusicDistributionSystem.Domain.Common;

namespace MusicDistributionSystem.Domain.Entities
{
    public class Notification : BaseEntity
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Link { get; set; }
        public bool IsRead { get; set; }
    }
}