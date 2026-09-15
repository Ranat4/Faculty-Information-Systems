using System.ComponentModel.DataAnnotations;

namespace FacultyInformationSystem_FIS_.Models
{
    public class DemoRequestViewModel
    {
        [Required(ErrorMessage = "Please enter your name.")]
        [Display(Name = "Full name")]
        public string Name { get; set; } = "";

        [Required(ErrorMessage = "Please enter your email.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [Display(Name = "Email address")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Please enter your institution.")]
        [Display(Name = "Institution / Department")]
        public string Institution { get; set; } = "";

        [Required(ErrorMessage = "Please tell us a bit about what you need.")]
        [Display(Name = "What would you like to see in the demo?")]
        public string Message { get; set; } = "";
    }
}
