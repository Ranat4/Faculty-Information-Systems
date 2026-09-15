using System.ComponentModel.DataAnnotations;

namespace FacultyInformationSystem_FIS_.Models
{
    public class ContactFormViewModel
    {
        [Required(ErrorMessage = "Please enter your name.")]
        [Display(Name = "Full name")]
        public string Name { get; set; } = "";

        [Required(ErrorMessage = "Please enter your email.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [Display(Name = "Email address")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Please enter a message.")]
        [Display(Name = "Message")]
        public string Message { get; set; } = "";
    }
}
