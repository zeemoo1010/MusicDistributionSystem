using MusicDistributionSystem.Models;

namespace MusicDistributionSystem.Repositories.Interfaces
{
    public interface IMembershipPlanRepository
    {
        Task<IReadOnlyCollection<MembershipPlan>> GetAllAsync();
        Task<MembershipPlan?> GetByIdAsync(Guid id);
    }
}
