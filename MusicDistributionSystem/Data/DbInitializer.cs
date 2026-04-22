using MusicDistributionSystem.Models;

namespace MusicDistributionSystem.Data
{
    public static class DbInitializer
    {
        public static void Initialize(Context.ApplicationDbContext context)
        {
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
        }
    }
}
