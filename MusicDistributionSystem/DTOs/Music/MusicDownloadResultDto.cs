namespace MusicDistributionSystem.DTOs.Music
{
    public class MusicDownloadResultDto
    {
        public bool Found { get; set; }
        public bool Allowed { get; set; }
        public bool FileExists { get; set; }
        public string? ErrorMessage { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = "audio/mpeg";
    }
}
