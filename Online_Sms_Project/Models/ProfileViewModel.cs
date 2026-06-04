using System.ComponentModel.DataAnnotations;

namespace Online_Sms_Project.Models
{
    public class ProfileViewModel
    {
        public int Id { get; set; } // Yeh wahi Unique ID hai jo change nahi hogi

        [Required(ErrorMessage = "Username likhna zaroori hai")]
        [StringLength(50)]
        public string Username { get; set; }

        [Required(ErrorMessage = "Email likhna zaroori hai")]
        [EmailAddress(ErrorMessage = "Sahi email address likhein")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone number likhna zaroori hai")]
        [StringLength(20)]
        public string PhoneNumber { get; set; }
    }
}