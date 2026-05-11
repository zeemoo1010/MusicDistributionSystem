namespace MusicDistributionSystem.Application.DTOs.Admin
{
    public class AdminDashboardDto
    {
        public int TotalUsers { get; set; }
        public int TotalTracks { get; set; }
        public int PendingTracks { get; set; }
        public int ApprovedTracks { get; set; }
        public int RejectedTracks { get; set; }
        public int TotalCategories { get; set; }
        public IReadOnlyCollection<AdminUserDto> Users { get; set; } = Array.Empty<AdminUserDto>();
        public IReadOnlyCollection<string> AvailableRoles { get; set; } = Array.Empty<string>();
        public IReadOnlyCollection<AdminCategoryDto> Categories { get; set; } = Array.Empty<AdminCategoryDto>();
    }
}

