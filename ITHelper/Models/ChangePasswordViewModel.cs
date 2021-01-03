using System.ComponentModel.DataAnnotations;

namespace ITHelper.Models
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Your B313 username is required.")]
        [Display(Name = "Enter your B313 Logon Username")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "You must enter your correct B313 password.")]
        [Display(Name = "Enter your Current B313 User Account Password")]
        public string Password { get; set; }

        [Required(ErrorMessage = "A new password meeting complexity requirements is required.")]
        [StringLength(50, ErrorMessage = "The password must be at least 6 characters.", MinimumLength = 6)]
        [Display(Name = "Enter your NEW Password")]
        public string NewPassword { get; set; }

        [Required]
        [Display(Name = "Confirm Your Password")]
        [Compare("NewPassword")]
        public string ConfirmPassword { get; set; }
    }
}
