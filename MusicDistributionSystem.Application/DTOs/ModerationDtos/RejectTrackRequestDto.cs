using System.ComponentModel.DataAnnotations;

namespace MusicDistributionSystem.Application.DTOs.Moderation
{
    public class RejectTrackRequestDto
    {
        [Required]
        public Guid TrackId { get; set; }

        [Required]
        [StringLength(500)]
        public string Reason { get; set; } = string.Empty;
    }
}
