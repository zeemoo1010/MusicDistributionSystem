using Microsoft.EntityFrameworkCore;
using MusicDistributionSystem.Domain.Entities;
using MusicDistributionSystem.Domain.Enums;

namespace MusicDistributionSystem.Infrastructure.EntityFrameworkCore
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Artist> Artists { get; set; }
        public DbSet<Album> Albums { get; set; }
        public DbSet<MusicTrack> MusicTracks { get; set; }
        public DbSet<Video> Videos { get; set; }
        public DbSet<Playlist> Playlists { get; set; }
        public DbSet<PlaylistTrack> PlaylistTracks { get; set; }
        public DbSet<ImageAsset> ImageAssets { get; set; }
        public DbSet<DownloadRecord> DownloadRecords { get; set; }
        public DbSet<MembershipPlan> MembershipPlans { get; set; }
        public DbSet<AccountToken> AccountTokens { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<TrackTag> TrackTags { get; set; }
        public DbSet<AnalyticsEvent> AnalyticsEvents { get; set; }
        public DbSet<PaymentTransaction> PaymentTransactions { get; set; }
        public DbSet<UserSubscription> UserSubscriptions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigureIndexes(modelBuilder);
            ConfigureIgnores(modelBuilder);
            ConfigureDefaults(modelBuilder);
            ConfigureRelationships(modelBuilder);
        }

        private static void ConfigureIndexes(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasIndex(user => user.Email).IsUnique();
            modelBuilder.Entity<Category>().HasIndex(category => category.Name).IsUnique();
            modelBuilder.Entity<Category>().HasIndex(category => category.Slug).IsUnique();
            modelBuilder.Entity<Artist>().HasIndex(artist => artist.Slug).IsUnique();
            modelBuilder.Entity<Album>().HasIndex(album => album.Slug).IsUnique();
            modelBuilder.Entity<MusicTrack>().HasIndex(track => track.Slug).IsUnique();
            modelBuilder.Entity<Video>().HasIndex(video => video.Slug).IsUnique();
            modelBuilder.Entity<Playlist>().HasIndex(playlist => playlist.Slug);
            modelBuilder.Entity<Role>().HasIndex(role => role.Name).IsUnique();
            modelBuilder.Entity<Tag>().HasIndex(tag => tag.Name).IsUnique();

            modelBuilder.Entity<AccountToken>()
                .HasIndex(token => new { token.UserId, token.Type, token.ConsumedAtUtc });

            modelBuilder.Entity<MusicTrack>()
                .HasIndex(t => new { t.ApprovalStatus, t.IsFeatured, t.CreatedAt });

            modelBuilder.Entity<Video>()
                .HasIndex(v => new { v.ApprovalStatus, v.IsFeatured, v.CreatedAt });

            modelBuilder.Entity<Comment>()
                .HasIndex(c => new { c.MusicTrackId, c.CreatedAt });

            modelBuilder.Entity<Comment>()
                .HasIndex(c => new { c.VideoId, c.CreatedAt });

            modelBuilder.Entity<Like>()
                .HasIndex(l => new { l.UserId, l.MusicTrackId });

            modelBuilder.Entity<Like>()
                .HasIndex(l => new { l.UserId, l.VideoId });

            modelBuilder.Entity<Notification>()
                .HasIndex(n => new { n.UserId, n.IsRead, n.CreatedAt });

            modelBuilder.Entity<AnalyticsEvent>()
                .HasIndex(e => new { e.EventType, e.CreatedAt });

            modelBuilder.Entity<PaymentTransaction>()
                .HasIndex(t => t.Reference).IsUnique();
        }

        private static void ConfigureIgnores(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AccountToken>().Ignore(token => token.CreatedAt);
            modelBuilder.Entity<AccountToken>().Ignore(token => token.UpdatedAt);
            modelBuilder.Entity<Category>().Ignore(category => category.CreatedAt);
            modelBuilder.Entity<Category>().Ignore(category => category.UpdatedAt);
            modelBuilder.Entity<MembershipPlan>().Ignore(plan => plan.CreatedAt);
            modelBuilder.Entity<MembershipPlan>().Ignore(plan => plan.UpdatedAt);
            modelBuilder.Entity<Role>().Ignore(role => role.CreatedAt);
            modelBuilder.Entity<Role>().Ignore(role => role.UpdatedAt);
            modelBuilder.Entity<User>().Ignore(user => user.CreatedAt);
            modelBuilder.Entity<User>().Ignore(user => user.UpdatedAt);
            modelBuilder.Entity<UserRole>().Ignore(userRole => userRole.CreatedAt);
            modelBuilder.Entity<UserRole>().Ignore(userRole => userRole.UpdatedAt);
            modelBuilder.Entity<UserRole>().Ignore(userRole => userRole.Id);
            modelBuilder.Entity<MusicTrack>().Ignore(track => track.UpdatedAt);
            modelBuilder.Entity<Video>().Ignore(video => video.UpdatedAt);
            modelBuilder.Entity<Artist>().Ignore(artist => artist.UpdatedAt);
            modelBuilder.Entity<Album>().Ignore(album => album.UpdatedAt);
            modelBuilder.Entity<Playlist>().Ignore(playlist => playlist.UpdatedAt);
            modelBuilder.Entity<ImageAsset>().Ignore(image => image.UpdatedAt);
            modelBuilder.Entity<DownloadRecord>().Ignore(download => download.CreatedAt);
            modelBuilder.Entity<DownloadRecord>().Ignore(download => download.UpdatedAt);

            modelBuilder.Entity<Comment>().Ignore(e => e.UpdatedAt);
            modelBuilder.Entity<Like>().Ignore(e => e.UpdatedAt);
            modelBuilder.Entity<Notification>().Ignore(e => e.UpdatedAt);
            modelBuilder.Entity<Tag>().Ignore(e => e.UpdatedAt);
            modelBuilder.Entity<AnalyticsEvent>().Ignore(e => e.UpdatedAt);
            modelBuilder.Entity<PaymentTransaction>().Ignore(e => e.UpdatedAt);
            modelBuilder.Entity<UserSubscription>().Ignore(e => e.UpdatedAt);
        }

        private static void ConfigureDefaults(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MusicTrack>()
                .Property(track => track.DownloadCount).HasDefaultValue(0);

            modelBuilder.Entity<MusicTrack>()
                .Property(track => track.PlayCount).HasDefaultValue(0);

            modelBuilder.Entity<MusicTrack>()
                .Property(track => track.AccessLevel).HasDefaultValue(ContentAccessLevel.Free);

            modelBuilder.Entity<Video>()
                .Property(video => video.ViewCount).HasDefaultValue(0);

            modelBuilder.Entity<Video>()
                .Property(video => video.DownloadCount).HasDefaultValue(0);

            modelBuilder.Entity<Video>()
                .Property(video => video.AccessLevel).HasDefaultValue(ContentAccessLevel.Free);

            modelBuilder.Entity<MembershipPlan>()
                .Property(plan => plan.MonthlyPrice).HasPrecision(18, 2);

            modelBuilder.Entity<User>()
                .Property(user => user.MembershipTier).HasDefaultValue(MembershipTier.Free);

            modelBuilder.Entity<PaymentTransaction>()
                .Property(pt => pt.Amount).HasPrecision(18, 2);
        }

        private static void ConfigureRelationships(ModelBuilder modelBuilder)
        {
            // User & Roles
            modelBuilder.Entity<AccountToken>()
                .HasOne(token => token.User)
                .WithMany(user => user.AccountTokens)
                .HasForeignKey(token => token.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserRole>()
                .HasKey(userRole => new { userRole.UserId, userRole.RoleId });

            modelBuilder.Entity<UserRole>()
                .HasOne(userRole => userRole.User)
                .WithMany(user => user.UserRoles)
                .HasForeignKey(userRole => userRole.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserRole>()
                .HasOne(userRole => userRole.Role)
                .WithMany(role => role.UserRoles)
                .HasForeignKey(userRole => userRole.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Artist
            modelBuilder.Entity<Artist>()
                .HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Album
            modelBuilder.Entity<Album>()
                .HasOne(a => a.Artist)
                .WithMany(ar => ar.Albums)
                .HasForeignKey(a => a.ArtistId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Album>()
                .HasOne(a => a.Category)
                .WithMany(c => c.Albums)
                .HasForeignKey(a => a.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Album>()
                .HasOne(a => a.UploadedByUser)
                .WithMany()
                .HasForeignKey(a => a.UploadedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // MusicTrack
            modelBuilder.Entity<MusicTrack>()
                .HasOne(track => track.Category)
                .WithMany(c => c.MusicTracks)
                .HasForeignKey(track => track.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MusicTrack>()
                .HasOne(track => track.ArtistEntity)
                .WithMany(a => a.Tracks)
                .HasForeignKey(track => track.ArtistId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<MusicTrack>()
                .HasOne(track => track.Album)
                .WithMany(a => a.Tracks)
                .HasForeignKey(track => track.AlbumId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<MusicTrack>()
                .HasOne(track => track.UploadedByUser)
                .WithMany(user => user.UploadedTracks)
                .HasForeignKey(track => track.UploadedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MusicTrack>()
                .HasOne(track => track.ReviewedByUser)
                .WithMany()
                .HasForeignKey(track => track.ReviewedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Video
            modelBuilder.Entity<Video>()
                .HasOne(v => v.Artist)
                .WithMany(a => a.Videos)
                .HasForeignKey(v => v.ArtistId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Video>()
                .HasOne(v => v.Category)
                .WithMany(c => c.Videos)
                .HasForeignKey(v => v.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Video>()
                .HasOne(v => v.UploadedByUser)
                .WithMany(u => u.UploadedVideos)
                .HasForeignKey(v => v.UploadedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Video>()
                .HasOne(v => v.ReviewedByUser)
                .WithMany()
                .HasForeignKey(v => v.ReviewedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Playlist
            modelBuilder.Entity<Playlist>()
                .HasOne(p => p.User)
                .WithMany(u => u.Playlists)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PlaylistTrack>()
                .HasKey(pt => new { pt.PlaylistId, pt.MusicTrackId });

            modelBuilder.Entity<PlaylistTrack>()
                .HasOne(pt => pt.Playlist)
                .WithMany(p => p.PlaylistTracks)
                .HasForeignKey(pt => pt.PlaylistId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PlaylistTrack>()
                .HasOne(pt => pt.MusicTrack)
                .WithMany(t => t.PlaylistTracks)
                .HasForeignKey(pt => pt.MusicTrackId)
                .OnDelete(DeleteBehavior.Cascade);

            // ImageAsset
            modelBuilder.Entity<ImageAsset>()
                .HasOne(img => img.Category)
                .WithMany(c => c.ImageAssets)
                .HasForeignKey(img => img.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ImageAsset>()
                .HasOne(img => img.UploadedByUser)
                .WithMany(u => u.UploadedImages)
                .HasForeignKey(img => img.UploadedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ImageAsset>()
                .HasOne(img => img.ReviewedByUser)
                .WithMany()
                .HasForeignKey(img => img.ReviewedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Comment
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.MusicTrack)
                .WithMany(t => t.Comments)
                .HasForeignKey(c => c.MusicTrackId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Video)
                .WithMany(v => v.Comments)
                .HasForeignKey(c => c.VideoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.ParentComment)
                .WithMany(c => c.Replies)
                .HasForeignKey(c => c.ParentCommentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Like
            modelBuilder.Entity<Like>()
                .HasOne(l => l.User)
                .WithMany()
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Like>()
                .HasOne(l => l.MusicTrack)
                .WithMany(t => t.Likes)
                .HasForeignKey(l => l.MusicTrackId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Like>()
                .HasOne(l => l.Video)
                .WithMany(v => v.Likes)
                .HasForeignKey(l => l.VideoId)
                .OnDelete(DeleteBehavior.Cascade);

            // Notification
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // TrackTag (composite key)
            modelBuilder.Entity<TrackTag>()
                .HasKey(tt => new { tt.MusicTrackId, tt.TagId });

            modelBuilder.Entity<TrackTag>()
                .HasOne(tt => tt.MusicTrack)
                .WithMany(t => t.TrackTags)
                .HasForeignKey(tt => tt.MusicTrackId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TrackTag>()
                .HasOne(tt => tt.Tag)
                .WithMany(t => t.TrackTags)
                .HasForeignKey(tt => tt.TagId)
                .OnDelete(DeleteBehavior.Cascade);

            // PaymentTransaction
            modelBuilder.Entity<PaymentTransaction>()
                .HasOne(pt => pt.User)
                .WithMany()
                .HasForeignKey(pt => pt.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PaymentTransaction>()
                .HasOne(pt => pt.Plan)
                .WithMany()
                .HasForeignKey(pt => pt.PlanId)
                .OnDelete(DeleteBehavior.SetNull);

            // UserSubscription
            modelBuilder.Entity<UserSubscription>()
                .HasOne(us => us.User)
                .WithMany()
                .HasForeignKey(us => us.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserSubscription>()
                .HasOne(us => us.Plan)
                .WithMany()
                .HasForeignKey(us => us.PlanId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}