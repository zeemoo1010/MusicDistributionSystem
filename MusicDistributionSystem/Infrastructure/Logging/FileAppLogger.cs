using MusicDistributionSystem.Infrastructure.Logging;
using Microsoft.Extensions.Logging;

namespace MusicDistributionSystem.Infrastructure.Logging
{
    public class FileAppLogger : IAppLogger
    {
        private readonly ILogger<FileAppLogger> _logger;

        public FileAppLogger(ILogger<FileAppLogger> logger)
        {
            _logger = logger;
        }

        public Task LogInformationAsync(string category, string message)
        {
            _logger.LogInformation("[{Category}] {Message}", category, message);
            return Task.CompletedTask;
        }

        public Task LogWarningAsync(string category, string message)
        {
            _logger.LogWarning("[{Category}] {Message}", category, message);
            return Task.CompletedTask;
        }

        public Task LogErrorAsync(string category, string message, Exception? exception = null)
        {
            _logger.LogError(exception, "[{Category}] {Message}", category, message);
            return Task.CompletedTask;
        }
    }
}

