using Microsoft.Extensions.DependencyInjection;
using MusicDistributionSystem.Application.Contracts.Services;
using MusicDistributionSystem.Application.Services;

namespace MusicDistributionSystem.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IHomeService, HomeService>();
            services.AddScoped<IMusicService, MusicService>();
            services.AddScoped<IArtistService, ArtistService>();
            services.AddScoped<IAlbumService, AlbumService>();
            services.AddScoped<IVideoService, VideoService>();
            services.AddScoped<IPlaylistService, PlaylistService>();
            services.AddScoped<IImageService, ImageService>();
            services.AddScoped<IMediaStreamingService, MediaStreamingService>();
            services.AddScoped<IAiContentService, AiContentService>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IAdministrationService, AdministrationService>();
            services.AddScoped<IModerationService, ModerationService>();
            services.AddScoped<IDashboardService, DashboardService>();

            return services;
        }

    }
}
