using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using MusicDistributionSystem.Domain.Enums;

namespace MusicDistributionSystem.Application.DTOs.Music
{
    public class MusicUploadRequestDto
    {
        [Required]
        [StringLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(120)]
        public string Artist { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        [Display(Name = "Category")]
        public Guid? CategoryId { get; set; }

        [Required]
        [Display(Name = "Access level")]
        public ContentAccessLevel AccessLevel { get; set; } = ContentAccessLevel.Free;

        [Required]
        [Display(Name = "MP3 file")]
        public IFormFile? MusicFile { get; set; }

        [Display(Name = "Cover image")]
        public IFormFile? CoverImage { get; set; }

        public IEnumerable<SelectListItem> Categories { get; set; } = Enumerable.Empty<SelectListItem>();
    }
}

