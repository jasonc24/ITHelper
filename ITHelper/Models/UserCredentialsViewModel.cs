using System.ComponentModel.DataAnnotations;

namespace ITHelper.Models
{
    public class UserCredentialsViewModel
    {
        [Required]
        [Display(Name = "Enter your HopeChurch Logon Username")]
        public string UserName { get; set; }

        [Required]
        [Display(Name = "Enter your HopeChurch User Account Password")]
        public string Password { get; set; }
    }
}
