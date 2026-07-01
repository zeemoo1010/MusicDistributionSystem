namespace MusicDistributionSystem.Infrastructure.Configuration
{
    public class PaystackSettings
    {
        public string SecretKey { get; set; } = string.Empty;
        public string PublicKey { get; set; } = string.Empty;
        public string CallbackUrl { get; set; } = string.Empty;
    }
}