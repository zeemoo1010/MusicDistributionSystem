using MusicDistributionSystem.Application.DTOs.Common;
using MusicDistributionSystem.Application.DTOs.Home;
using MusicDistributionSystem.Application.DTOs.Membership;
using MusicDistributionSystem.Application.DTOs.Music;
using Microsoft.Extensions.Caching.Memory;
using MusicDistributionSystem.Application.Contracts.Repositories;
using MusicDistributionSystem.Application.Contracts.Services;
using MusicDistributionSystem.Domain.Entities;

namespace MusicDistributionSystem.Application.Services
{
    public class HomeService : IHomeService
    {
        private static readonly TimeSpan HomeCacheDuration = TimeSpan.FromMinutes(5);

        private readonly IMusicRepository _musicRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMembershipPlanRepository _membershipPlanRepository;
        private readonly IMemoryCache _memoryCache;

        public HomeService(
            IMusicRepository musicRepository,
            ICategoryRepository categoryRepository,
            IMembershipPlanRepository membershipPlanRepository,
            IMemoryCache memoryCache)
        {
            _musicRepository = musicRepository;
            _categoryRepository = categoryRepository;
            _membershipPlanRepository = membershipPlanRepository;
            _memoryCache = memoryCache;
        }

        public async Task<HomeIndexDto> GetHomeIndexAsync()
        {
            if (_memoryCache.TryGetValue<HomeIndexDto>("home:index", out var cachedHomeIndex) && cachedHomeIndex is not null)
            {
                return cachedHomeIndex;
            }

            var latestTracks = await _musicRepository.GetLatestApprovedTracksAsync(6);
            var categories = await _categoryRepository.GetAllAsync();
            var membershipPlans = await _membershipPlanRepository.GetAllAsync();

            var homeIndex = new HomeIndexDto
            {
                LatestTracks = latestTracks.Select(MapMusicCard).ToList(),
                Categories = categories.Select(MapCategoryOption).ToList(),
                MembershipPlans = membershipPlans.Select(MapMembershipPlan).ToList(),
                ApprovedTrackCount = await _musicRepository.CountApprovedAsync(),
                TotalDownloads = await _musicRepository.GetTotalDownloadsAsync(),
                PremiumTrackCount = await _musicRepository.CountPremiumApprovedAsync()
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
                Artist = track.Artist,
                Description = track.Description,
                CategoryName = track.Category?.Name,
                AccessLevel = track.AccessLevel,
                DownloadCount = track.DownloadCount,
                CreatedAt = track.CreatedAt,
                IsFeatured = track.IsFeatured
            };
        }

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

