namespace MusicDistributionSystem.Services.Interfaces
{
    public interface IAccountNotificationService
    {
        Task SendVerificationCodeAsync(string destination, string code);
        Task SendPasswordResetCodeAsync(string destination, string code);
    }
}
