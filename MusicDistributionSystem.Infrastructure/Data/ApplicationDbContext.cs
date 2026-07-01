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
        public DbSet<MusicTrack> MusicTracks { get; set; }
        public DbSet<ImageAsset> ImageAssets { get; set; }
        public DbSet<DownloadRecord> DownloadRecords { get; set; }
        public DbSet<MembershipPlan> MembershipPlans { get; set; }
        public DbSet<AccountToken> AccountTokens { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<MediaAsset> MediaAssets { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<MediaAssetTag> MediaAssetTags { get; set; }
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
            modelBuilder.Entity<Role>().HasIndex(role => role.Name).IsUnique();
            modelBuilder.Entity<Tag>().HasIndex(tag => tag.Name).IsUnique();

            modelBuilder.Entity<AccountToken>()
                .HasIndex(token => new { token.UserId, token.Type, token.ConsumedAtUtc });

            modelBuilder.Entity<Comment>()
                .HasIndex(c => new { c.MediaAssetId, c.CreatedAt });

            modelBuilder.Entity<Like>()
                .HasIndex(l => new { l.UserId, l.MediaAssetId });

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
            modelBuilder.Entity<ImageAsset>().Ignore(image => image.UpdatedAt);
            modelBuilder.Entity<DownloadRecord>().Ignore(download => download.CreatedAt);
            modelBuilder.Entity<DownloadRecord>().Ignore(download => download.UpdatedAt);

            // New entities — use their own CreatedAt fields
            modelBuilder.Entity<MediaAsset>().Ignore(e => e.UpdatedAt);
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
                .Property(track => track.AccessLevel).HasDefaultValue(ContentAccessLevel.Free);

            modelBuilder.Entity<MembershipPlan>()
                .Property(plan => plan.MonthlyPrice).HasPrecision(18, 2);

            modelBuilder.Entity<User>()
                .Property(user => user.MembershipTier).HasDefaultValue(MembershipTier.Free);

            modelBuilder.Entity<MediaAsset>()
                .Property(a => a.DownloadCount).HasDefaultValue(0);

            modelBuilder.Entity<MediaAsset>()
                .Property(a => a.ViewCount).HasDefaultValue(0);

            modelBuilder.Entity<MediaAsset>()
                .Property(a => a.Type).HasDefaultValue(MediaType.Music);

            modelBuilder.Entity<MediaAsset>()
                .Property(a => a.AccessLevel).HasDefaultValue(ContentAccessLevel.Free);

            modelBuilder.Entity<PaymentTransaction>()
                .Property(pt => pt.Amount).HasPrecision(18, 2);
        }

        private static void ConfigureRelationships(ModelBuilder modelBuilder)
        {
            // Existing relationships
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

            // MediaAsset
            modelBuilder.Entity<MediaAsset>()
                .HasOne(a => a.Category)
                .WithMany(c => c.MediaAssets)
                .HasForeignKey(a => a.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MediaAsset>()
                .HasOne(a => a.UploadedByUser)
                .WithMany()
                .HasForeignKey(a => a.UploadedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MediaAsset>()
                .HasOne(a => a.ReviewedByUser)
                .WithMany()
                .HasForeignKey(a => a.ReviewedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Comment
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.MediaAsset)
                .WithMany(a => a.Comments)
                .HasForeignKey(c => c.MediaAssetId)
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
                .HasOne(l => l.MediaAsset)
                .WithMany(a => a.Likes)
                .HasForeignKey(l => l.MediaAssetId)
                .OnDelete(DeleteBehavior.Cascade);

            // Notification
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // MediaAssetTag (composite key)
            modelBuilder.Entity<MediaAssetTag>()
                .HasKey(mt => new { mt.MediaAssetId, mt.TagId });

            modelBuilder.Entity<MediaAssetTag>()
                .HasOne(mt => mt.MediaAsset)
                .WithMany(a => a.MediaAssetTags)
                .HasForeignKey(mt => mt.MediaAssetId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MediaAssetTag>()
                .HasOne(mt => mt.Tag)
                .WithMany(t => t.MediaAssetTags)
                .HasForeignKey(mt => mt.TagId)
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