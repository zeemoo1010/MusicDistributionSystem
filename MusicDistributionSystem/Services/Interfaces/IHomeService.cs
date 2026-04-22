using MusicDistributionSystem.DTOs.Home;

namespace MusicDistributionSystem.Services.Interfaces
{
    public interface IHomeService
    {
        Task<HomeIndexDto> GetHomeIndexAsync();
    }
}
