using MusicDistributionSystem.Domain.Enums;

namespace MusicDistributionSystem.Application.DTOs.Dashboard
{
    public class DashboardIndexDto
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public IReadOnlyCollection<string> Roles { get; set; } = Array.Empty<string>();
        public IReadOnlyCollection<DashboardTrackDto> Uploads { get; set; } = Array.Empty<DashboardTrackDto>();
    }

    public class DashboardTrackDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Artist { get; set; } = string.Empty;
        public string? CategoryName { get; set; }
        public ApprovalStatus ApprovalStatus { get; set; }
        public ContentAccessLevel AccessLevel { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

