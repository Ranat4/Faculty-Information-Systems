using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace FacultyInformationSystem_FIS_.Models
{
    public class CvRecord : BaseEntity
    {
        public int UserId { get; set; }

        [ValidateNever]
        public User User { get; set; } = null!;

        [Required(ErrorMessage = "Please enter the CV title.")]
        [MaxLength(150)]
        [Display(Name = "CV title")]
        public string Title { get; set; } = "";

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Display(Name = "Date")]
        public DateTime? Date { get; set; }

        public DocumentStatus Status { get; set; } = DocumentStatus.Submitted;

        [MaxLength(255)]
        public string? FileName { get; set; }

        [MaxLength(500)]
        public string? FilePath { get; set; }

        [MaxLength(1000)]
        [Display(Name = "Reviewer comment")]
        public string? ReviewComment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}