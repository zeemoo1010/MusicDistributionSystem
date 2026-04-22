using Microsoft.EntityFrameworkCore;
using MusicDistributionSystem.Context;
using MusicDistributionSystem.Models;
using MusicDistributionSystem.Repositories.Interfaces;

namespace MusicDistributionSystem.Repositories
{
    public class MembershipPlanRepository : IMembershipPlanRepository
    {
        private readonly ApplicationDbContext _context;

        public MembershipPlanRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyCollection<MembershipPlan>> GetAllAsync()
        {
            return await _context.MembershipPlans
                .AsNoTracking()
                .OrderBy(plan => plan.MonthlyPrice)
                .ToListAsync();
        }
    }
}
