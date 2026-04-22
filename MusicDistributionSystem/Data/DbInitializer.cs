using MusicDistributionSystem.Enums;
using MusicDistributionSystem.Models;

namespace MusicDistributionSystem.Data
{
    public static class DbInitializer
    {
        public static void Initialize(Context.ApplicationDbContext context, bool resetDatabaseOnStartup = false)
        {
            if (resetDatabaseOnStartup)
            {
                context.Database.EnsureDeleted();
            }

            context.Database.EnsureCreated();

            if (!context.Categories.Any())
            {
                context.Categories.AddRange(
                    new Category { Name = "Afrobeats", Description = "Trending Afrobeats singles and albums." },
                    new Category { Name = "Gospel", Description = "Inspirational songs and worship music." },
                    new Category { Name = "Hip Hop", Description = "Rap releases, freestyles, and mixtapes." },
                    new Category { Name = "Highlife", Description = "Classic and contemporary highlife sounds." },
                    new Category { Name = "Mixtapes", Description = "DJ mixes and curated listening sessions." }
                );

                context.SaveChanges();
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

                context.SaveChanges();
            }
        }
    }
}
