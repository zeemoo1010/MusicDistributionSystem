namespace MusicDistributionSystem.Domain.Constants
{
    public static class RoleNames
    {
        public const string SuperAdmin = "SuperAdmin";
        public const string Admin = "Admin";
        public const string Moderator = "Moderator";
        public const string Artist = "Artist";
        public const string User = "User";

        public static readonly string[] All =
        {
            SuperAdmin,
            Admin,
            Moderator,
            Artist,
            User
        };
    }
}

