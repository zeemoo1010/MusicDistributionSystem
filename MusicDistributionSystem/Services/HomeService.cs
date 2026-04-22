using MusicDistributionSystem.DTOs.Common;
using MusicDistributionSystem.DTOs.Home;
using MusicDistributionSystem.DTOs.Membership;
using MusicDistributionSystem.DTOs.Music;
using MusicDistributionSystem.Repositories.Interfaces;
using MusicDistributionSystem.Services.Interfaces;

namespace MusicDistributionSystem.Services
{
    public class HomeService : IHomeService
    {
        private readonly IMusicRepository _musicRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMembershipPlanRepository _membershipPlanRepository;

        public HomeService(
            IMusicRepository musicRepository,
            ICategoryRepository categoryRepository,
            IMembershipPlanRepository membershipPlanRepository)
        {
            _musicRepository = musicRepository;
            _categoryRepository = categoryRepository;
            _membershipPlanRepository = membershipPlanRepository;
        }

        public async Task<HomeIndexDto> GetHomeIndexAsync()
        {
            var latestTracks = await _musicRepository.GetLatestApprovedTracksAsync(6);
            var categories = await _categoryRepository.GetAllAsync();
            var membershipPlans = await _membershipPlanRepository.GetAllAsync();

            return new HomeIndexDto
            {
                LatestTracks = latestTracks.Select(MapMusicCard).ToList(),
                Categories = categories.Select(MapCategoryOption).ToList(),
                MembershipPlans = membershipPlans.Select(MapMembershipPlan).ToList(),
                ApprovedTrackCount = await _musicRepository.CountApprovedAsync(),
                TotalDownloads = await _musicRepository.GetTotalDownloadsAsync(),
                PremiumTrackCount = await _musicRepository.CountPremiumApprovedAsync()
            };
        }

        private static MusicCardDto MapMusicCard(Models.MusicTrack track)
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

        private static CategoryOptionDto MapCategoryOption(Models.Category category)
        {
            return new CategoryOptionDto
            {
                Id = category.Id,
                Name = category.Name
            };
        }

        private static MembershipPlanDto MapMembershipPlan(Models.MembershipPlan plan)
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
