using MusicDistributionSystem.Domain.Entities;

namespace MusicDistributionSystem.Application.Contracts.Repositories
{
    public interface IMembershipPlanRepository
    {
        Task<IReadOnlyCollection<MembershipPlan>> GetAllAsync();
        Task<MembershipPlan?> GetByIdAsync(Guid id);
    }
}

