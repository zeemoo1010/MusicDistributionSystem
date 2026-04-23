namespace MusicDistributionSystem.Constants
{
    public static class RoleNames
    {
        public const string Admin = "Admin";
        public const string Moderator = "Moderator";
        public const string Uploader = "Uploader";
        public const string RegisteredUser = "RegisteredUser";

        public static readonly string[] All =
        {
            Admin,
            Moderator,
            Uploader,
            RegisteredUser
        };
    }
}
