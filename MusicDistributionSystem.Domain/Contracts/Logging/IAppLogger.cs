namespace MusicDistributionSystem.Domain.Contracts.Logging
{
    public interface IAppLogger
    {
        Task LogInformationAsync(string category, string message);
        Task LogWarningAsync(string category, string message);
        Task LogErrorAsync(string category, string message, Exception? exception = null);
    }
}
