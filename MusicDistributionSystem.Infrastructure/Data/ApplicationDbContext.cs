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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasIndex(user => user.Email)
                .IsUnique();

            modelBuilder.Entity<Category>()
                .HasIndex(category => category.Name)
                .IsUnique();

            modelBuilder.Entity<Role>()
                .HasIndex(role => role.Name)
                .IsUnique();

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

            modelBuilder.Entity<User>()
                .Property(user => user.CreatedAtUtc)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity<MusicTrack>()
                .Property(track => track.DownloadCount)
                .HasDefaultValue(0);

            modelBuilder.Entity<MusicTrack>()
                .Property(track => track.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity<MusicTrack>()
                .Property(track => track.AccessLevel)
                .HasDefaultValue(ContentAccessLevel.Free);

            modelBuilder.Entity<ImageAsset>()
                .Property(image => image.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity<DownloadRecord>()
                .Property(download => download.DownloadedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity<MembershipPlan>()
                .Property(plan => plan.MonthlyPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<User>()
                .Property(user => user.MembershipTier)
                .HasDefaultValue(MembershipTier.Free);

            modelBuilder.Entity<AccountToken>()
                .Property(token => token.CreatedAtUtc)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity<AccountToken>()
                .HasIndex(token => new { token.UserId, token.Type, token.ConsumedAtUtc });

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
        }
    }
}

