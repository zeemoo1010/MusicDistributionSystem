namespace MusicDistributionSystem.Application.Contracts.Services
{
    public interface IAccountNotificationService
    {
        Task SendVerificationCodeAsync(string destination, string code);
        Task SendPasswordResetCodeAsync(string destination, string code);
    }
}

