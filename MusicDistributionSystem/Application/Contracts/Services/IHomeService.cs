using MusicDistributionSystem.Application.DTOs.Home;

namespace MusicDistributionSystem.Application.Contracts.Services
{
    public interface IHomeService
    {
        Task<HomeIndexDto> GetHomeIndexAsync();
    }
}

