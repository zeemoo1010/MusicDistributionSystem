namespace MusicDistributionSystem.Application.DTOs.Moderation
{
    public class ModerationDashboardDto
    {
        public IReadOnlyCollection<ModerationTrackDto> PendingTracks { get; set; } = Array.Empty<ModerationTrackDto>();
    }
}

