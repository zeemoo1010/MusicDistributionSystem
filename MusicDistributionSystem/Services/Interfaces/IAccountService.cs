using MusicDistributionSystem.DTOs.Account;

namespace MusicDistributionSystem.Services.Interfaces
{
    public interface IAccountService
    {
        Task<AuthResultDto> RegisterAsync(RegisterRequestDto request);
        Task<AuthResultDto> LoginAsync(LoginRequestDto request);
    }
}
