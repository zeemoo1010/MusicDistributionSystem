namespace MusicDistributionSystem.Application.DTOs.Admin
{
    public class AdminCategoryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int LinkedTrackCount { get; set; }
    }
}
