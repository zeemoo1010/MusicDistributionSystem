using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

namespace MusicDistributionSystem.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            string connectionString,
            IConfiguration configuration)
        {
            services.Configure<EmailSettings>(
                configuration.GetSection("EmailSettings"));

            services.Configure<DefaultAdminSettings>(
                configuration.GetSection("DefaultAdmin"));

            services.AddDbContext<ApplicationDbContext>(options =>
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

            services.AddSingleton<IAppLogger, FileAppLogger>();

            return services;
        }
    }
}
