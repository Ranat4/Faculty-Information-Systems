using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace FacultyInformationSystem_FIS_.Models
{
    public enum ProfessionalDevelopmentType
    {
        Workshop,
        Conference,
        Seminar,
        Training,
        Course,
        Webinar,
        Other
    }

    public enum ParticipationRole
    {
        Attendee,
        Presenter,
        Panelist,
        Organizer
    }

    public class ProfessionalDevelopment : BaseEntity
    {
        public int UserId { get; set; }

        [ValidateNever]
        public User User { get; set; } = null!;

        [Required(ErrorMessage = "Please enter the activity title.")]
        [MaxLength(150)]
        [Display(Name = "Activity title")]
        public string Title { get; set; } = "";

        [Required(ErrorMessage = "Please select the activity type.")]
        [Display(Name = "Activity type")]
        public ProfessionalDevelopmentType ActivityType { get; set; }

        [Required(ErrorMessage = "Please enter the organizer.")]
        [MaxLength(150)]
        [Display(Name = "Organizer")]
        public string Organizer { get; set; } = "";

        [MaxLength(150)]
        [Display(Name = "Location")]
        public string? Location { get; set; }

        [Required(ErrorMessage = "Please select your role.")]
        [Display(Name = "Your role")]
        public ParticipationRole Role { get; set; }

        [Required(ErrorMessage = "Please enter the start date.")]
        [Display(Name = "Start date")]
        public DateTime? StartDate { get; set; }

        [Display(Name = "End date")]
        public DateTime? EndDate { get; set; }

        [Range(0, 1000, ErrorMessage = "Please enter a valid number of hours.")]
        [Column(TypeName = "decimal(6,2)")]
        [Display(Name = "Duration (hours)")]
        public decimal? DurationHours { get; set; }

        [Display(Name = "Certificate of completion received")]
        public bool CertificateReceived { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

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
