using System.ComponentModel.DataAnnotations;

namespace FacultyInformationSystem_FIS_.Models
{
    public class Certificate : BaseEntity
    {
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        [Required(ErrorMessage = "Please enter the certificate title.")]
        [MaxLength(150)]
        [Display(Name = "Certificate title")]
        public string Title { get; set; } = "";

        [Required(ErrorMessage = "Please enter the issuing organization.")]
        [MaxLength(150)]
        [Display(Name = "Issuing organization")]
        public string IssuingOrganization { get; set; } = "";

        [MaxLength(100)]
        [Display(Name = "Certificate number")]
        public string? CertificateNumber { get; set; }

        [Display(Name = "Start date")]
        public DateTime? StartDate { get; set; }

        [Display(Name = "End date")]
        public DateTime? EndDate { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

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
