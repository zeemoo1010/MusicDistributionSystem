namespace MusicDistributionSystem.Application.DTOs.Account
{
    public class AuthResultDto
    {
        public bool Succeeded { get; set; }
        public string? ErrorMessage { get; set; }
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public IReadOnlyCollection<string> Roles { get; set; } = Array.Empty<string>();
        public bool RequiresVerification { get; set; }
    }
}

