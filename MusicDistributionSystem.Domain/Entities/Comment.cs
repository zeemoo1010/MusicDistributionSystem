using MusicDistributionSystem.Domain.Common;

namespace MusicDistributionSystem.Domain.Entities
{
    public class Comment : BaseEntity
    {
        public string Content { get; set; } = string.Empty;

        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public Guid MediaAssetId { get; set; }
        public MediaAsset MediaAsset { get; set; } = null!;

        public Guid? ParentCommentId { get; set; }
        public Comment? ParentComment { get; set; }
        public ICollection<Comment> Replies { get; set; } = new List<Comment>();

        public bool IsApproved { get; set; } = true;
    }
}