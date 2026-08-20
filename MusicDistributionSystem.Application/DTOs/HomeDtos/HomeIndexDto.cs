using MusicDistributionSystem.Application.DTOs.AlbumDtos;
using MusicDistributionSystem.Application.DTOs.ArtistDtos;
using MusicDistributionSystem.Application.DTOs.Common;
using MusicDistributionSystem.Application.DTOs.ImageDtos;
using MusicDistributionSystem.Application.DTOs.Membership;
using MusicDistributionSystem.Application.DTOs.Music;
using MusicDistributionSystem.Application.DTOs.PlaylistDtos;
using MusicDistributionSystem.Application.DTOs.VideoDtos;

namespace MusicDistributionSystem.Application.DTOs.Home
{
    public class HomeIndexDto
    {
        public IReadOnlyCollection<MusicCardDto> LatestTracks { get; set; } = Array.Empty<MusicCardDto>();
        public IReadOnlyCollection<MusicCardDto> TrendingTracks { get; set; } = Array.Empty<MusicCardDto>();
        public IReadOnlyCollection<MusicCardDto> TopChartTracks { get; set; } = Array.Empty<MusicCardDto>();
        public IReadOnlyCollection<ArtistCardDto> FeaturedArtists { get; set; } = Array.Empty<ArtistCardDto>();
        public IReadOnlyCollection<AlbumCardDto> FeaturedAlbums { get; set; } = Array.Empty<AlbumCardDto>();
        public IReadOnlyCollection<VideoCardDto> FeaturedVideos { get; set; } = Array.Empty<VideoCardDto>();
        public IReadOnlyCollection<PlaylistCardDto> PublicPlaylists { get; set; } = Array.Empty<PlaylistCardDto>();
        public IReadOnlyCollection<ImageCardDto> FeaturedImages { get; set; } = Array.Empty<ImageCardDto>();
        public IReadOnlyCollection<CategoryOptionDto> Categories { get; set; } = Array.Empty<CategoryOptionDto>();
        public IReadOnlyCollection<MembershipPlanDto> MembershipPlans { get; set; } = Array.Empty<MembershipPlanDto>();
        public int ApprovedTrackCount { get; set; }
        public int TotalDownloads { get; set; }
        public int PremiumTrackCount { get; set; }
        public int VideoCount { get; set; }
        public int ArtistCount { get; set; }
    }
}


