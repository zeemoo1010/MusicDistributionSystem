using MusicDistributionSystem.Application.DTOs.Account;

namespace MusicDistributionSystem.Application.Contracts.Services
{
    public interface IAccountService
    {
        Task<AuthResultDto> RegisterAsync(RegisterRequestDto request);
        Task<AuthResultDto> LoginAsync(LoginRequestDto request);
        Task<OperationResultDto> VerifyEmailAsync(VerifyEmailRequestDto request);
        Task RequestPasswordResetAsync(ForgotPasswordRequestDto request);
        Task<OperationResultDto> ResetPasswordAsync(ResetPasswordRequestDto request);
        Task<OperationResultDto> ResendVerificationCodeAsync(string email);
        Task<IReadOnlyCollection<string>> GetUserRolesAsync(Guid userId);
    }
}

