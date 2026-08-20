using Microsoft.Extensions.Caching.Memory;
using MusicDistributionSystem.Application.Contracts.Services;
using MusicDistributionSystem.Application.DTOs.Common;
using MusicDistributionSystem.Application.DTOs.Home;
using MusicDistributionSystem.Application.DTOs.Membership;
using MusicDistributionSystem.Application.DTOs.Music;
using MusicDistributionSystem.Domain.Contracts.Interface;
using MusicDistributionSystem.Domain.Entities;

namespace MusicDistributionSystem.Application.Services
{
    public class HomeService : IHomeService
    {
        private static readonly TimeSpan HomeCacheDuration = TimeSpan.FromMinutes(3);

        private readonly IMusicRepository _musicRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMembershipPlanRepository _membershipPlanRepository;
        private readonly IArtistRepository _artistRepository;
        private readonly IAlbumRepository _albumRepository;
        private readonly IVideoRepository _videoRepository;
        private readonly IPlaylistRepository _playlistRepository;
        private readonly IImageAssetRepository _imageRepository;
        private readonly IMemoryCache _memoryCache;

        public HomeService(
            IMusicRepository musicRepository,
            ICategoryRepository categoryRepository,
            IMembershipPlanRepository membershipPlanRepository,
            IArtistRepository artistRepository,
            IAlbumRepository albumRepository,
            IVideoRepository videoRepository,
            IPlaylistRepository playlistRepository,
            IImageAssetRepository imageRepository,
            IMemoryCache memoryCache)
        {
            _musicRepository = musicRepository;
            _categoryRepository = categoryRepository;
            _membershipPlanRepository = membershipPlanRepository;
            _artistRepository = artistRepository;
            _albumRepository = albumRepository;
            _videoRepository = videoRepository;
            _playlistRepository = playlistRepository;
            _imageRepository = imageRepository;
            _memoryCache = memoryCache;
        }

        public async Task<HomeIndexDto> GetHomeIndexAsync()
        {
            if (_memoryCache.TryGetValue<HomeIndexDto>("home:index", out var cachedHomeIndex) && cachedHomeIndex is not null)
            {
                return cachedHomeIndex;
            }

            var latestTracks = await _musicRepository.GetLatestApprovedTracksAsync(8);
            var trendingTracks = await _musicRepository.GetTrendingTracksAsync(8);
            var topChartTracks = await _musicRepository.GetTopChartTracksAsync(8);
            var featuredArtists = await _artistRepository.GetFeaturedArtistsAsync(6);
            var featuredAlbums = await _albumRepository.GetFeaturedAlbumsAsync(4);
            var featuredVideos = await _videoRepository.GetFeaturedVideosAsync(4);
            var publicPlaylists = await _playlistRepository.GetPublicPlaylistsAsync();
            var featuredImages = await _imageRepository.GetFeaturedImagesAsync(6);
            var categories = await _categoryRepository.GetAllAsync();
            var membershipPlans = await _membershipPlanRepository.GetAllAsync();

            var homeIndex = new HomeIndexDto
            {
                LatestTracks = latestTracks.Select(MapMusicCard).ToList(),
                TrendingTracks = trendingTracks.Select(MapMusicCard).ToList(),
                TopChartTracks = topChartTracks.Select(MapMusicCard).ToList(),
                FeaturedArtists = featuredArtists.Select(MapArtistCard).ToList(),
                FeaturedAlbums = featuredAlbums.Select(MapAlbumCard).ToList(),
                FeaturedVideos = featuredVideos.Select(MapVideoCard).ToList(),
                PublicPlaylists = publicPlaylists.Take(4).Select(MapPlaylistCard).ToList(),
                FeaturedImages = featuredImages.Select(MapImageCard).ToList(),
                Categories = categories.Select(MapCategoryOption).ToList(),
                MembershipPlans = membershipPlans.Select(MapMembershipPlan).ToList(),
                ApprovedTrackCount = await _musicRepository.CountApprovedAsync(),
                TotalDownloads = await _musicRepository.GetTotalDownloadsAsync(),
                PremiumTrackCount = await _musicRepository.CountPremiumApprovedAsync(),
                VideoCount = await _videoRepository.CountApprovedAsync(),
                ArtistCount = (await _artistRepository.GetAllAsync()).Count
            };

            _memoryCache.Set("home:index", homeIndex, HomeCacheDuration);

            return homeIndex;
        }

        private static MusicCardDto MapMusicCard(MusicTrack track)
        {
            return new MusicCardDto
            {
                Id = track.Id,
                Title = track.Title,
                Artist = track.ArtistEntity?.Name ?? track.Artist,
                Slug = track.Slug,
                ArtistId = track.ArtistId,
                AlbumId = track.AlbumId,
                AlbumTitle = track.Album?.Title,
                Description = track.Description,
                CategoryName = track.Category?.Name,
                CoverImagePath = track.CoverImagePath,
                Duration = track.Duration,
                AccessLevel = track.AccessLevel,
                DownloadCount = track.DownloadCount,
                PlayCount = track.PlayCount,
                LikeCount = track.Likes.Count,
                CreatedAt = track.CreatedAt,
                IsFeatured = track.IsFeatured
            };
        }

        private static DTOs.ArtistDtos.ArtistCardDto MapArtistCard(Artist a) => new()
        {
            Id = a.Id,
            Name = a.Name,
            Slug = a.Slug,
            Bio = a.Bio,
            ProfilePicturePath = a.ProfilePicturePath,
            Country = a.Country,
            IsVerified = a.IsVerified,
            TrackCount = a.Tracks.Count,
            AlbumCount = a.Albums.Count,
            VideoCount = a.Videos.Count
        };

        private static DTOs.AlbumDtos.AlbumCardDto MapAlbumCard(Album a) => new()
        {
            Id = a.Id,
            Title = a.Title,
            Slug = a.Slug,
            ArtistName = a.Artist?.Name ?? "Various Artists",
            CategoryName = a.Category?.Name,
            CoverImagePath = a.CoverImagePath,
            ReleaseDate = a.ReleaseDate,
            AccessLevel = a.AccessLevel,
            TrackCount = a.Tracks.Count,
            IsFeatured = a.IsFeatured
        };

        private static DTOs.VideoDtos.VideoCardDto MapVideoCard(Video v) => new()
        {
            Id = v.Id,
            Title = v.Title,
            Slug = v.Slug,
            ArtistName = v.Artist?.Name ?? v.ArtistName,
            CategoryName = v.Category?.Name,
            ThumbnailPath = v.ThumbnailPath,
            Duration = v.Duration,
            ViewCount = v.ViewCount,
            DownloadCount = v.DownloadCount,
            AccessLevel = v.AccessLevel,
            IsFeatured = v.IsFeatured,
            CreatedAt = v.CreatedAt
        };

        private static DTOs.PlaylistDtos.PlaylistCardDto MapPlaylistCard(Playlist p) => new()
        {
            Id = p.Id,
            Title = p.Title,
            Slug = p.Slug,
            Description = p.Description,
            CoverImagePath = p.CoverImagePath,
            OwnerName = p.User?.Username ?? "Editorial",
            IsPublic = p.IsPublic,
            IsEditorial = p.IsEditorial,
            TrackCount = p.PlaylistTracks.Count,
            CreatedAt = p.CreatedAt
        };

        private static DTOs.ImageDtos.ImageCardDto MapImageCard(ImageAsset i) => new()
        {
            Id = i.Id,
            Title = i.Title,
            Slug = i.Slug,
            CategoryName = i.Category?.Name,
            FilePath = i.FilePath,
            ThumbnailPath = i.ThumbnailPath ?? i.FilePath,
            Width = i.Width,
            Height = i.Height,
            DownloadCount = i.DownloadCount,
            AccessLevel = i.AccessLevel,
            CreatedAt = i.CreatedAt
        };

        private static CategoryOptionDto MapCategoryOption(Category category)
        {
            return new CategoryOptionDto
            {
                Id = category.Id,
                Name = category.Name
            };
        }

        private static MembershipPlanDto MapMembershipPlan(MembershipPlan plan)
        {
            return new MembershipPlanDto
            {
                Id = plan.Id,
                Name = plan.Name,
                Tier = plan.Tier,
                MonthlyPrice = plan.MonthlyPrice,
                MonthlyDownloadLimit = plan.MonthlyDownloadLimit,
                HasAdFreeExperience = plan.HasAdFreeExperience,
                HasPriorityReview = plan.HasPriorityReview,
                HasArtistPromotionTools = plan.HasArtistPromotionTools,
                Description = plan.Description
            };
        }
    }
}


