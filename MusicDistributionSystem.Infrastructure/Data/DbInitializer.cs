using MusicDistributionSystem.Domain.Constants;
using MusicDistributionSystem.Domain.Contracts.Security;
using MusicDistributionSystem.Domain.Entities;
using MusicDistributionSystem.Domain.Enums;
using MusicDistributionSystem.Infrastructure.Configuration;
using MusicDistributionSystem.Infrastructure.EntityFrameworkCore;

namespace MusicDistributionSystem.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(
            ApplicationDbContext context,
            IPasswordHasherService passwordHasherService,
            DefaultAdminSettings defaultAdminSettings)
        {
            if (!context.Categories.Any())
            {
                context.Categories.AddRange(
                    new Category { Name = "Afrobeats", Description = "Trending Afrobeats singles and albums." },
                    new Category { Name = "Gospel", Description = "Inspirational songs and worship music." },
                    new Category { Name = "Hip Hop", Description = "Rap releases, freestyles, and mixtapes." },
                    new Category { Name = "Highlife", Description = "Classic and contemporary highlife sounds." },
                    new Category { Name = "Mixtapes", Description = "DJ mixes and curated listening sessions." }
                );
            }

            if (!context.MembershipPlans.Any())
            {
                context.MembershipPlans.AddRange(
                    new MembershipPlan
                    {
                        Name = "Free Stream",
                        Tier = MembershipTier.Free,
                        MonthlyPrice = 0,
                        MonthlyDownloadLimit = 25,
                        Description = "Entry access for discovery, standard downloads, and ad-supported listening.",
                        HasAdFreeExperience = false,
                        HasPriorityReview = false,
                        HasArtistPromotionTools = false
                    },
                    new MembershipPlan
                    {
                        Name = "Premium Pulse",
                        Tier = MembershipTier.Premium,
                        MonthlyPrice = 2500,
                        MonthlyDownloadLimit = 250,
                        Description = "Ad-free access, premium-only releases, and faster content review queues.",
                        HasAdFreeExperience = true,
                        HasPriorityReview = true,
                        HasArtistPromotionTools = false
                    },
                    new MembershipPlan
                    {
                        Name = "Gold Creator",
                        Tier = MembershipTier.Gold,
                        MonthlyPrice = 7500,
                        MonthlyDownloadLimit = 1000,
                        Description = "Built for artists, labels, and publishers with promotion tools and release priority.",
                        HasAdFreeExperience = true,
                        HasPriorityReview = true,
                        HasArtistPromotionTools = true
                    }
                );
            }

            foreach (var roleName in RoleNames.All)
            {
                if (!context.Roles.Any(role => role.Name == roleName))
                {
                    context.Roles.Add(new Role
                    {
                        Name = roleName,
                        Description = roleName switch
                        {
                            RoleNames.SuperAdmin => "Full system access, including settings and platform ownership.",
                            RoleNames.Admin => "Full platform administration access.",
                            RoleNames.Moderator => "Review and moderate submitted content.",
                            RoleNames.Artist => "Upload and manage owned music releases.",
                            _ => "General registered account for authenticated listeners."
                        }
                    });
                }
            }

            await context.SaveChangesAsync();

            if (!context.Users.Any(user => user.Email == defaultAdminSettings.Email.ToLower()))
            {
                var adminRole = context.Roles.First(role => role.Name == RoleNames.Admin);
                var admin = new User
                {
                    Username = defaultAdminSettings.Username,
                    Email = defaultAdminSettings.Email.Trim().ToLowerInvariant(),
                    PasswordHash = passwordHasherService.HashPassword(defaultAdminSettings.Password),
                    IsActive = true,
                    IsEmailVerified = true
                };

                context.Users.Add(admin);
                context.UserRoles.Add(new UserRole
                {
                    UserId = admin.Id,
                    RoleId = adminRole.Id
                });

                var superAdminRole = context.Roles.FirstOrDefault(role => role.Name == RoleNames.SuperAdmin);
                if (superAdminRole is not null)
                {
                    context.UserRoles.Add(new UserRole
                    {
                        UserId = admin.Id,
                        RoleId = superAdminRole.Id
                    });
                }
            }

            await context.SaveChangesAsync();
        }
    }
}

