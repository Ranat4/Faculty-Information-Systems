using System.ComponentModel.DataAnnotations;

namespace FacultyInformationSystem_FIS_.Models
{
    public class VerifyCodeViewModel
    {
        [Required]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Please enter the verification code.")]
        [Display(Name = "Verification code")]
        public string Code { get; set; } = "";
    }
}
