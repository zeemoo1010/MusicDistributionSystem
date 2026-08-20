using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using MusicDistributionSystem.Application.Contracts.Infrastructure;
using MusicDistributionSystem.Application.DTOs.Music;
using MusicDistributionSystem.Application.Services;
using MusicDistributionSystem.Domain.Contracts.Interface;
using MusicDistributionSystem.Domain.Contracts.Logging;
using MusicDistributionSystem.Application.Contracts.Security;
using MusicDistributionSystem.Domain.Entities;
using MusicDistributionSystem.Domain.Enums;

namespace MusicDistributionSystem.Tests.Services;

public class MusicServiceTests
{
    private readonly Mock<IMusicRepository> _musicRepo = new();
    private readonly Mock<ICategoryRepository> _categoryRepo = new();
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IUploadedFileSecurityService> _fileSecurity = new();
    private readonly Mock<IFileStorageService> _fileStorage = new();
    private readonly Mock<IAppLogger> _logger = new();
    private readonly MusicService _sut;

    public MusicServiceTests()
    {
        _sut = new MusicService(_musicRepo.Object, _categoryRepo.Object, _userRepo.Object, _fileSecurity.Object, _fileStorage.Object, _logger.Object);
    }

    private static MusicTrack MakeTrack() => new()
    {
        Title = "Test Track",
        Artist = "Test Artist",
        Description = "A test track",
        Category = new Category { Name = "Afrobeat" },
        CategoryId = Guid.NewGuid(),
        AccessLevel = ContentAccessLevel.Free,
        DownloadCount = 42,
        FileSizeBytes = 5_000_000,
        UploadedByName = "uploader",
        IsFeatured = false,
        ApprovalStatus = ApprovalStatus.Approved,
        FilePath = "uploads/music/test.mp3",
        OriginalFileName = "test.mp3",
        CoverImagePath = "uploads/covers/test.jpg"
    };

    // ── GetMusicIndexAsync ──

    [Fact]
    public async Task GetMusicIndexAsync_ShouldReturnMappedTracksAndCategories()
    {
        var tracks = new[] { MakeTrack() };
        var paged = new MusicDistributionSystem.Domain.Common.PaginatedResult<MusicTrack>
        {
            Items = tracks,
            TotalCount = 1,
            Page = 1,
            PageSize = 12
        };
        var categories = new[] { new Category { Name = "Afrobeat" } };
        _musicRepo.Setup(r => r.GetApprovedTracksPagedAsync(null, null, 1, 12)).ReturnsAsync(paged);
        _categoryRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(categories);

        var result = await _sut.GetMusicIndexAsync(null, null, 1, 12);

        result.Tracks.Should().HaveCount(1);
        result.Tracks.First().Title.Should().Be("Test Track");
        result.Categories.Should().HaveCount(1);
        result.Categories.First().Name.Should().Be("Afrobeat");
    }

    [Fact]
    public async Task GetMusicIndexAsync_ShouldPassSearchAndFilterToRepository()
    {
        var catId = Guid.NewGuid();
        var paged = new MusicDistributionSystem.Domain.Common.PaginatedResult<MusicTrack>
        {
            Items = Array.Empty<MusicTrack>(),
            TotalCount = 0,
            Page = 1,
            PageSize = 12
        };
        _musicRepo.Setup(r => r.GetApprovedTracksPagedAsync("search", catId, 1, 12)).ReturnsAsync(paged);
        _categoryRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(Array.Empty<Category>());

        await _sut.GetMusicIndexAsync("search", catId, 1, 12);

        _musicRepo.Verify(r => r.GetApprovedTracksPagedAsync("search", catId, 1, 12), Times.Once);
    }


    // ── GetMusicDetailsAsync ──

    [Fact]
    public async Task GetMusicDetailsAsync_WhenTrackExists_ShouldReturnDetails()
    {
        var track = MakeTrack();
        _musicRepo.Setup(r => r.GetApprovedTrackByIdAsync(track.Id, true)).ReturnsAsync(track);

        var result = await _sut.GetMusicDetailsAsync(track.Id);

        result.Should().NotBeNull();
        result!.Title.Should().Be("Test Track");
        result.CategoryName.Should().Be("Afrobeat");
        result.DownloadCount.Should().Be(42);
    }

    [Fact]
    public async Task GetMusicDetailsAsync_WhenTrackNotFound_ShouldReturnNull()
    {
        _musicRepo.Setup(r => r.GetApprovedTrackByIdAsync(It.IsAny<Guid>(), true)).ReturnsAsync((MusicTrack?)null);

        var result = await _sut.GetMusicDetailsAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    // ── UploadAsync ──

    [Fact]
    public async Task UploadAsync_WhenMusicFileIsNull_ShouldReturnError()
    {
        var result = await _sut.UploadAsync(new MusicUploadRequestDto(), Guid.NewGuid(), "test", "test@test.com");

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Contain("MP3");
    }

    [Fact]
    public async Task UploadAsync_WhenMusicFileFailsValidation_ShouldReturnError()
    {
        var request = new MusicUploadRequestDto
        {
            MusicFile = new FormFile(new MemoryStream(), 0, 100, "file", "test.mp3"),
            CategoryId = Guid.NewGuid(),
            Title = "T", Artist = "A", AccessLevel = ContentAccessLevel.Free
        };

        _fileSecurity.Setup(s => s.ValidateMusicFileAsync(request.MusicFile, It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, "Not a valid MP3."));

        var result = await _sut.UploadAsync(request, Guid.NewGuid(), "test", "test@test.com");

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Be("Not a valid MP3.");
    }

    [Fact]
    public async Task UploadAsync_WhenCoverImageFailsValidation_ShouldReturnError()
    {
        var request = new MusicUploadRequestDto
        {
            MusicFile = new FormFile(new MemoryStream(), 0, 100, "file", "test.mp3"),
            CoverImage = new FormFile(new MemoryStream(), 0, 100, "cover", "cover.png"),
            CategoryId = Guid.NewGuid(),
            Title = "T", Artist = "A", AccessLevel = ContentAccessLevel.Free
        };

        _fileSecurity.Setup(s => s.ValidateMusicFileAsync(request.MusicFile, It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, null));
        _fileSecurity.Setup(s => s.ValidateCoverImageAsync(request.CoverImage, It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, "Invalid image."));
        _fileSecurity.Setup(s => s.SanitizeFileName(It.IsAny<string>())).Returns<string>(n => n);

        var result = await _sut.UploadAsync(request, Guid.NewGuid(), "test", "test@test.com");

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Be("Invalid image.");
    }

    [Fact]
    public async Task UploadAsync_WhenValid_ShouldUploadAndReturnSuccess()
    {
        var request = new MusicUploadRequestDto
        {
            MusicFile = new FormFile(new MemoryStream(new byte[] { 0, 1, 2 }), 0, 3, "file", "track.mp3"),
            CategoryId = Guid.NewGuid(),
            Title = "My Track",
            Artist = "My Artist",
            Description = "Desc",
            AccessLevel = ContentAccessLevel.Free
        };

        _fileSecurity.Setup(s => s.ValidateMusicFileAsync(request.MusicFile, It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, null));
        _fileSecurity.Setup(s => s.ValidateCoverImageAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, null));
        _fileSecurity.Setup(s => s.SanitizeFileName(It.IsAny<string>())).Returns<string>(n => n);
        _fileStorage.Setup(s => s.SaveFileAsync(request.MusicFile, "music", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("uploads/music/track.mp3");

        var result = await _sut.UploadAsync(request, Guid.NewGuid(), "uploader", "up@test.com");

        result.Succeeded.Should().BeTrue();
        _musicRepo.Verify(r => r.AddAsync(It.Is<MusicTrack>(t => t.Title == "My Track")), Times.Once);
        _musicRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    // ── PrepareDownloadAsync ──

    [Fact]
    public async Task PrepareDownloadAsync_WhenTrackNotFound_ShouldReturnFoundFalse()
    {
        _musicRepo.Setup(r => r.GetApprovedTrackByIdAsync(It.IsAny<Guid>(), false)).ReturnsAsync((MusicTrack?)null);

        var result = await _sut.PrepareDownloadAsync(Guid.NewGuid(), null, null);

        result.Found.Should().BeFalse();
    }

    [Fact]
    public async Task PrepareDownloadAsync_WhenPremiumTrack_AnonymousUser_ShouldBlockDownload()
    {
        var track = MakeTrack();
        track.AccessLevel = ContentAccessLevel.Premium;
        _musicRepo.Setup(r => r.GetApprovedTrackByIdAsync(track.Id, false)).ReturnsAsync(track);

        var result = await _sut.PrepareDownloadAsync(track.Id, null, null);

        result.Found.Should().BeTrue();
        result.Allowed.Should().BeFalse();
        result.ErrorMessage.Should().Contain("log in");
    }

    [Fact]
    public async Task PrepareDownloadAsync_WhenPremiumTrack_UserWithInsufficientTier_ShouldBlockDownload()
    {
        var track = MakeTrack();
        track.AccessLevel = ContentAccessLevel.Premium;
        var userId = Guid.NewGuid();
        _musicRepo.Setup(r => r.GetApprovedTrackByIdAsync(track.Id, false)).ReturnsAsync(track);
        _userRepo.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(new User
        {
            Username = "freeuser",
            Email = "free@test.com",
            PasswordHash = "hash",
            IsActive = true,
            IsEmailVerified = true,
            MembershipTier = MembershipTier.Free
        });

        var result = await _sut.PrepareDownloadAsync(track.Id, userId, null);

        result.Found.Should().BeTrue();
        result.Allowed.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Premium");
    }

    [Fact]
    public async Task PrepareDownloadAsync_WhenPremiumTrack_UserWithSufficientTier_ShouldAllow()
    {
        var track = MakeTrack();
        track.AccessLevel = ContentAccessLevel.Premium;
        var userId = Guid.NewGuid();
        _musicRepo.Setup(r => r.GetApprovedTrackByIdAsync(track.Id, false)).ReturnsAsync(track);
        _userRepo.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(new User
        {
            Username = "premiumuser",
            Email = "premium@test.com",
            PasswordHash = "hash",
            IsActive = true,
            IsEmailVerified = true,
            MembershipTier = MembershipTier.Premium
        });
        _fileStorage.Setup(s => s.FileExists(track.FilePath)).Returns(false);

        var result = await _sut.PrepareDownloadAsync(track.Id, userId, null);

        result.Found.Should().BeTrue();
        result.Allowed.Should().BeTrue();
        result.FileExists.Should().BeFalse();
    }
}
