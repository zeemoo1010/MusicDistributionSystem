using System.ComponentModel.DataAnnotations;

namespace MusicDistributionSystem.Application.DTOs.Admin
{
    public class UpdateCategoryRequestDto
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        [StringLength(80)]
        public string Name { get; set; } = string.Empty;

        [StringLength(250)]
        public string? Description { get; set; }
    }
}
