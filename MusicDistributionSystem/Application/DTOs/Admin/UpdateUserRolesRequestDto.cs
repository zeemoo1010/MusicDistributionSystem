using System.ComponentModel.DataAnnotations;

namespace MusicDistributionSystem.Application.DTOs.Admin
{
    public class UpdateUserRolesRequestDto
    {
        [Required]
        public Guid UserId { get; set; }

        public IReadOnlyCollection<string> Roles { get; set; } = Array.Empty<string>();
    }
}

