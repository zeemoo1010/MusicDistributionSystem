using System.Security.Cryptography;
using MusicDistributionSystem.Domain.Contracts.Security;

namespace MusicDistributionSystem.Infrastructure.Security
{
    public class PasswordHasherService : IPasswordHasherService
    {
        private const int SaltSize = 16;
        private const int KeySize = 32;
        private const int CurrentIterations = 310_000;
        private const string CurrentVersion = "v1";

        public string HashPassword(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var hash = Rfc2898DeriveBytes.Pbkdf2(
                password, salt, CurrentIterations, HashAlgorithmName.SHA256, KeySize);

            return $"{CurrentVersion}.{CurrentIterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }

        public bool VerifyPassword(string hashedPassword, string providedPassword)
        {
            var parts = hashedPassword.Split('.');

            // Supports both old format (iterations.salt.hash) and new format (version.iterations.salt.hash)
            int iterations;
            byte[] salt;
            byte[] expectedHash;

            if (parts.Length == 4 && parts[0] == CurrentVersion)
            {
                // New format: v1.{iterations}.{salt}.{hash}
                if (!int.TryParse(parts[1], out iterations))
                    return false;

                salt = Convert.FromBase64String(parts[2]);
                expectedHash = Convert.FromBase64String(parts[3]);
            }
            else if (parts.Length == 3)
            {
                // Old format: {iterations}.{salt}.{hash}
                if (!int.TryParse(parts[0], out iterations))
                    return false;

                salt = Convert.FromBase64String(parts[1]);
                expectedHash = Convert.FromBase64String(parts[2]);
            }
            else
            {
                return false;
            }

            var actualHash = Rfc2898DeriveBytes.Pbkdf2(
                providedPassword, salt, iterations, HashAlgorithmName.SHA256, expectedHash.Length);

            return CryptographicOperations.FixedTimeEquals(expectedHash, actualHash);
        }
    }
}