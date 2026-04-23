namespace MusicDistributionSystem.DTOs.Moderation
{
    public class ModerationDashboardDto
    {
        public IReadOnlyCollection<ModerationTrackDto> PendingTracks { get; set; } = Array.Empty<ModerationTrackDto>();
    }
}
