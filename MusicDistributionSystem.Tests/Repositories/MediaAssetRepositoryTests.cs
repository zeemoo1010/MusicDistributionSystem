using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MusicDistributionSystem.Domain.Entities;
using MusicDistributionSystem.Domain.Enums;
using MusicDistributionSystem.Infrastructure.EntityFrameworkCore;
using MusicDistributionSystem.Infrastructure.EntityFrameworkCore.Repositories;

namespace MusicDistributionSystem.Tests.Repositories;

public class MediaAssetRepositoryTests
{
    private static ApplicationDbContext CreateContext()
    {
        var opts = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(opts);
    }

    /// <summary>
    /// Seeds a Category and User so that Include navigation properties resolve correctly.
    /// Returns a tuple (category, user) for reuse.
    /// </summary>
    private static (Category cat, User uploader) SeedLookups(ApplicationDbContext ctx)
    {
        var cat = new Category { Name = "Afrobeat" };
        var uploader = new User
        {
            Username = "uploader",
            Email = "up@test.com",
            PasswordHash = "x",
            IsActive = true,
            IsEmailVerified = true,
            MembershipTier = MembershipTier.Free
        };
        ctx.Categories.Add(cat);
        ctx.Users.Add(uploader);
        ctx.SaveChanges();
        return (cat, uploader);
    }

    private static MediaAsset MakeAsset(string title = "Test Track", MediaType type = MediaType.Music,
        ApprovalStatus status = ApprovalStatus.Approved, int downloads = 0,
        Guid? categoryId = null, Guid? uploaderId = null) => new()
    {
        Title = title,
        Type = type,
        ApprovalStatus = status,
        AccessLevel = ContentAccessLevel.Free,
        DownloadCount = downloads,
        ViewCount = 0,
        CategoryId = categoryId ?? Guid.NewGuid(),
        UploadedByUserId = uploaderId ?? Guid.NewGuid(),
        FilePath = "uploads/music/test.mp3",
        OriginalFileName = "test.mp3",
        UploadedByName = "test",
        UploadedByEmail = "test@test.com"
    };

    // ── AddAsync / SaveChangesAsync ──

    [Fact]
    public async Task AddAsync_ShouldPersistMediaAsset()
    {
        using var ctx = CreateContext();
        var repo = new MediaAssetRepository(ctx);
        var (cat, uploader) = SeedLookups(ctx);
        var asset = MakeAsset(categoryId: cat.Id, uploaderId: uploader.Id);

        await repo.AddAsync(asset);
        await repo.SaveChangesAsync();

        var saved = await ctx.MediaAssets.FindAsync(asset.Id);
        saved.Should().NotBeNull();
        saved!.Title.Should().Be("Test Track");
    }

    // ── GetByIdAsync ──

    [Fact]
    public async Task GetByIdAsync_WhenExists_ShouldReturnAsset()
    {
        using var ctx = CreateContext();
        var repo = new MediaAssetRepository(ctx);
        var (cat, uploader) = SeedLookups(ctx);
        var asset = MakeAsset(categoryId: cat.Id, uploaderId: uploader.Id);
        ctx.MediaAssets.Add(asset);
        await ctx.SaveChangesAsync();

        var result = await repo.GetByIdAsync(asset.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(asset.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotExists_ShouldReturnNull()
    {
        using var ctx = CreateContext();
        var repo = new MediaAssetRepository(ctx);

        var result = await repo.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    // ── GetApprovedAsync ──

    [Fact]
    public async Task GetApprovedAsync_ShouldReturnPagedResults()
    {
        using var ctx = CreateContext();
        var repo = new MediaAssetRepository(ctx);
        var (cat, uploader) = SeedLookups(ctx);
        for (int i = 0; i < 10; i++)
            ctx.MediaAssets.Add(MakeAsset($"Track {i}", categoryId: cat.Id, uploaderId: uploader.Id));
        await ctx.SaveChangesAsync();

        var page1 = await repo.GetApprovedAsync(null, null, null, 1, 5);
        var page2 = await repo.GetApprovedAsync(null, null, null, 2, 5);

        page1.Should().HaveCount(5);
        page2.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetApprovedAsync_ShouldFilterByType()
    {
        using var ctx = CreateContext();
        var repo = new MediaAssetRepository(ctx);
        var (cat, uploader) = SeedLookups(ctx);
        ctx.MediaAssets.Add(MakeAsset("Music", MediaType.Music, categoryId: cat.Id, uploaderId: uploader.Id));
        ctx.MediaAssets.Add(MakeAsset("Video", MediaType.Video, categoryId: cat.Id, uploaderId: uploader.Id));
        await ctx.SaveChangesAsync();

        var result = await repo.GetApprovedAsync(null, null, MediaType.Video, 1, 10);

        result.Should().HaveCount(1);
        result.First().Title.Should().Be("Video");
    }

    // ── GetPendingAsync ──

    [Fact]
    public async Task GetPendingAsync_ShouldReturnOnlyPending()
    {
        using var ctx = CreateContext();
        var repo = new MediaAssetRepository(ctx);
        var (cat, uploader) = SeedLookups(ctx);
        ctx.MediaAssets.Add(MakeAsset("Approved", categoryId: cat.Id, uploaderId: uploader.Id));
        ctx.MediaAssets.Add(MakeAsset("Pending", status: ApprovalStatus.Pending, categoryId: cat.Id, uploaderId: uploader.Id));
        await ctx.SaveChangesAsync();

        var result = await repo.GetPendingAsync();

        result.Should().HaveCount(1);
        result.First().Title.Should().Be("Pending");
    }

    // ── CountByStatusAsync ──

    [Fact]
    public async Task CountByStatusAsync_ShouldReturnCorrectCount()
    {
        using var ctx = CreateContext();
        var repo = new MediaAssetRepository(ctx);
        var (cat, uploader) = SeedLookups(ctx);
        ctx.MediaAssets.Add(MakeAsset("A1", categoryId: cat.Id, uploaderId: uploader.Id));
        ctx.MediaAssets.Add(MakeAsset("A2", categoryId: cat.Id, uploaderId: uploader.Id));
        ctx.MediaAssets.Add(MakeAsset("P", status: ApprovalStatus.Pending, categoryId: cat.Id, uploaderId: uploader.Id));
        await ctx.SaveChangesAsync();

        var count = await repo.CountByStatusAsync(ApprovalStatus.Approved);

        count.Should().Be(2);
    }

    // ── CountByTypeAsync ──

    [Fact]
    public async Task CountByTypeAsync_ShouldReturnCorrectCount()
    {
        using var ctx = CreateContext();
        var repo = new MediaAssetRepository(ctx);
        var (cat, uploader) = SeedLookups(ctx);
        ctx.MediaAssets.Add(MakeAsset("M1", MediaType.Music, categoryId: cat.Id, uploaderId: uploader.Id));
        ctx.MediaAssets.Add(MakeAsset("M2", MediaType.Music, categoryId: cat.Id, uploaderId: uploader.Id));
        ctx.MediaAssets.Add(MakeAsset("V", MediaType.Video, categoryId: cat.Id, uploaderId: uploader.Id));
        await ctx.SaveChangesAsync();

        var count = await repo.CountByTypeAsync(MediaType.Video);

        count.Should().Be(1);
    }

    // ── GetTotalDownloadsAsync ──

    [Fact]
    public async Task GetTotalDownloadsAsync_ShouldSumAllDownloadCounts()
    {
        using var ctx = CreateContext();
        var repo = new MediaAssetRepository(ctx);
        var (cat, uploader) = SeedLookups(ctx);
        ctx.MediaAssets.Add(MakeAsset("A", downloads: 10, categoryId: cat.Id, uploaderId: uploader.Id));
        ctx.MediaAssets.Add(MakeAsset("B", downloads: 20, categoryId: cat.Id, uploaderId: uploader.Id));
        ctx.MediaAssets.Add(MakeAsset("C", downloads: 5, categoryId: cat.Id, uploaderId: uploader.Id));
        await ctx.SaveChangesAsync();

        var total = await repo.GetTotalDownloadsAsync();

        total.Should().Be(35);
    }

    // ── Remove ──

    [Fact]
    public async Task Remove_ShouldDeleteAsset()
    {
        using var ctx = CreateContext();
        var repo = new MediaAssetRepository(ctx);
        var (cat, uploader) = SeedLookups(ctx);
        var asset = MakeAsset(categoryId: cat.Id, uploaderId: uploader.Id);
        ctx.MediaAssets.Add(asset);
        await ctx.SaveChangesAsync();

        repo.Remove(asset);
        await repo.SaveChangesAsync();

        var exists = await ctx.MediaAssets.FindAsync(asset.Id);
        exists.Should().BeNull();
    }

    // ── GetLatestAsync ──

    [Fact]
    public async Task GetLatestAsync_ShouldReturnNewestFirst()
    {
        using var ctx = CreateContext();
        var repo = new MediaAssetRepository(ctx);
        var (cat, uploader) = SeedLookups(ctx);
        var old = MakeAsset("Old", categoryId: cat.Id, uploaderId: uploader.Id);
        ctx.MediaAssets.Add(old);
        await ctx.SaveChangesAsync();

        var newer = MakeAsset("New", categoryId: cat.Id, uploaderId: uploader.Id);
        ctx.MediaAssets.Add(newer);
        await ctx.SaveChangesAsync();

        var result = await repo.GetLatestAsync(1);

        result.Should().HaveCount(1);
        result.First().Title.Should().Be("New");
    }
}
