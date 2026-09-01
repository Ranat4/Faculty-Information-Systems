using System.ComponentModel.DataAnnotations;

namespace FacultyInformationSystem_FIS_.Models
{
    public class Degree : BaseEntity
    {
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        [Required(ErrorMessage = "Please enter the degree title.")]
        [MaxLength(150)]
        [Display(Name = "Degree title")]
        public string Title { get; set; } = "";

        [Required(ErrorMessage = "Please enter the institution.")]
        [MaxLength(150)]
        public string Institution { get; set; } = "";

        [MaxLength(150)]
        [Display(Name = "Field of study")]
        public string? FieldOfStudy { get; set; }

        [Range(1950, 2100, ErrorMessage = "Please enter a valid year.")]
        [Display(Name = "Year obtained")]
        public int? YearObtained { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public DocumentStatus Status { get; set; } = DocumentStatus.Draft;

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
