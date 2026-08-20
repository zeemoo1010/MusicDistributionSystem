namespace MusicDistributionSystem.Application.Contracts.Services
{
    public interface IMediaStreamingService
    {
        Task<(bool Found, bool Allowed, string? FilePath, string? ContentType, long FileLength, string? ErrorMessage)> GetMediaStreamAsync(Guid id, string mediaType, Guid? userId);
        Task RecordPlayAsync(Guid trackId, Guid? userId, string? ipAddress);
        Task RecordVideoViewAsync(Guid videoId, Guid? userId, string? ipAddress);
    }
}
