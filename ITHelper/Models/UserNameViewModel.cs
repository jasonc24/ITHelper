using System.ComponentModel.DataAnnotations;

namespace ITHelper.Models
{
    public class UserNameViewModel
    {
        [Required]
        [Display(Name = "Enter your HopeChurch Logon Username")]
        public string UserName { get; set; }
    }
}
