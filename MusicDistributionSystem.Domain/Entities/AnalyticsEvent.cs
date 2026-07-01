using MusicDistributionSystem.Domain.Common;

namespace MusicDistributionSystem.Domain.Entities
{
    public class AnalyticsEvent : BaseEntity
    {
        public string EventType { get; set; } = string.Empty;
        public Guid? MediaAssetId { get; set; }
        public Guid? UserId { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? ReferrerUrl { get; set; }
    }
}