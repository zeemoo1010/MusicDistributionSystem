using Microsoft.EntityFrameworkCore;
using MusicDistributionSystem.Models;

namespace MusicDistributionSystem.Context
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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasIndex(user => user.Email)
                .IsUnique();

            modelBuilder.Entity<Category>()
                .HasIndex(category => category.Name)
                .IsUnique();

            modelBuilder.Entity<MusicTrack>()
                .Property(track => track.DownloadCount)
                .HasDefaultValue(0);

            modelBuilder.Entity<MusicTrack>()
                .Property(track => track.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity<ImageAsset>()
                .Property(image => image.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity<DownloadRecord>()
                .Property(download => download.DownloadedAt)
                .HasDefaultValueSql("GETUTCDATE()");
        }
    }
}
