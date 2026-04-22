using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using MusicDistributionSystem.Enums;

namespace MusicDistributionSystem.ViewModels
{
    public class MusicUploadViewModel
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
        [StringLength(100)]
        [Display(Name = "Your name")]
        public string UploadedByName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Your email")]
        public string UploadedByEmail { get; set; } = string.Empty;

        [Required]
        [Display(Name = "MP3 file")]
        public IFormFile? MusicFile { get; set; }

        public IEnumerable<SelectListItem> Categories { get; set; } = Enumerable.Empty<SelectListItem>();
    }
}
