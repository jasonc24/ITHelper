using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ITHelper.Models
{
    public class UserUpdateViewModel
    {
        [Required]
        [Display(Name = "Account Name")]
        public string SamAccountName { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string GivenName { get; set; }

        [Required]
        [Display(Name = "Middle Name/Middle Initial")]
        public string MiddleName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string Surname { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "OCD E-Mail Address")]
        public string EMailAddress { get; set; }

        public bool IsAccountLockedOut { get; set; }

        [Display(Name = "Security Group Membership")]
        public List<string> Groups { get; set; }
    }
}
