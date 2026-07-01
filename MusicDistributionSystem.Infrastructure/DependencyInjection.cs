using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MusicDistributionSystem.Application.Contracts.Payments;
using MusicDistributionSystem.Application.Contracts.Services;
using MusicDistributionSystem.Domain.Contracts.Interface;
using MusicDistributionSystem.Domain.Contracts.Logging;
using MusicDistributionSystem.Domain.Contracts.Security;
using MusicDistributionSystem.Infrastructure.Configuration;
using MusicDistributionSystem.Infrastructure.EntityFrameworkCore;
using MusicDistributionSystem.Infrastructure.EntityFrameworkCore.Repositories;
using MusicDistributionSystem.Infrastructure.Logging;
using MusicDistributionSystem.Infrastructure.Notifications;
using MusicDistributionSystem.Infrastructure.Security;
using MusicDistributionSystem.Infrastructure.Services;

namespace MusicDistributionSystem.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            string connectionString,
            IConfiguration configuration)
        {
            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
            services.Configure<DefaultAdminSettings>(configuration.GetSection("DefaultAdmin"));
            services.Configure<PaystackSettings>(configuration.GetSection("Paystack"));

            services.AddDbContextPool<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            services.AddScoped<IUploadedFileSecurityService, UploadedFileSecurityService>();
            services.AddScoped<IPasswordHasherService, PasswordHasherService>();
            services.AddScoped<IAccountNotificationService, AccountNotificationService>();

            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IMembershipPlanRepository, MembershipPlanRepository>();
            services.AddScoped<IMusicRepository, MusicRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IAccountTokenRepository, AccountTokenRepository>();

            // New entity repositories
            services.AddScoped<IMediaAssetRepository, MediaAssetRepository>();
            services.AddScoped<ICommentRepository, CommentRepository>();
            services.AddScoped<ILikeRepository, LikeRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<ITagRepository, TagRepository>();
            services.AddScoped<IPaymentTransactionRepository, PaymentTransactionRepository>();
            services.AddScoped<IUserSubscriptionRepository, UserSubscriptionRepository>();

            services.AddHttpClient("Paystack", client =>
            {
                client.BaseAddress = new Uri("https://api.paystack.co");
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            });

            services.AddScoped<IPaymentGateway, PaystackGateway>();
            services.AddScoped<IPaymentService, PaystackService>();
            services.AddSingleton<IAppLogger, FileAppLogger>();

            return services;
        }
    }
}