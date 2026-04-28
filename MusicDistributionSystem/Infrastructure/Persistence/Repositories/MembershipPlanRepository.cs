using Microsoft.EntityFrameworkCore;
using MusicDistributionSystem.Infrastructure.Persistence;
using MusicDistributionSystem.Domain.Entities;
using MusicDistributionSystem.Application.Contracts.Repositories;

namespace MusicDistributionSystem.Infrastructure.Persistence.Repositories
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

        public Task<MembershipPlan?> GetByIdAsync(Guid id)
        {
            return _context.MembershipPlans
                .AsNoTracking()
                .FirstOrDefaultAsync(plan => plan.Id == id);
        }
    }
}

