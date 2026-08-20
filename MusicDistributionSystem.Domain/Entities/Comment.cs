using MusicDistributionSystem.Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace MusicDistributionSystem.Domain.Entities
{
    public class Comment : BaseEntity
    {
        [Required]
        [StringLength(1000)]
        public string Content { get; set; } = string.Empty;

        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public Guid? MusicTrackId { get; set; }
        public MusicTrack? MusicTrack { get; set; }

        public Guid? VideoId { get; set; }
        public Video? Video { get; set; }

        public Guid? ParentCommentId { get; set; }
        public Comment? ParentComment { get; set; }
        public ICollection<Comment> Replies { get; set; } = new List<Comment>();

        public bool IsApproved { get; set; } = true;
    }
}