namespace MusicDistributionSystem.DTOs.Account
{
    public class AuthResultDto
    {
        public bool Succeeded { get; set; }
        public string? ErrorMessage { get; set; }
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
