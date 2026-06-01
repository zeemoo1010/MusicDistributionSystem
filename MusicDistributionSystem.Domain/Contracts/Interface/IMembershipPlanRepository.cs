using MusicDistributionSystem.Domain.Entities;

namespace MusicDistributionSystem.Domain.Contracts.Interface
{
    public interface IMembershipPlanRepository
    {
        Task<IReadOnlyCollection<MembershipPlan>> GetAllAsync();
        Task<MembershipPlan?> GetByIdAsync(Guid id);
    }
}

