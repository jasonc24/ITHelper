using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ITHelper.Models
{
    public class Location
    {
        #region Properties
        public Guid Id { get; set; } = new Guid();

        [Display(Name = "Location Name")]
        [Required]
        public string Name { get; set; }

        [Display(Name = "Address 1")]
        public string Address1 { get; set; }

        [Display(Name = "Address 2")]
        public string Address2 { get; set; }

        [Display(Name = "City")]
        public string City { get; set; }

        [Display(Name = "State")]
        [Required]
        public string State { get; set; } = "NY";

        [RegularExpression(@"^\d{5}(-\d{4})?$", ErrorMessage = "Invalid Zip")]
        [Display(Name = "Zip Code")]
        public string Zip { get; set; }

        [Display(Name = "Primary Contact")]
        [Required]
        public string PrimaryContact { get; set; }

        [Display(Name = "Primary Contact's E-Mail Address")]
        [EmailAddress]
        [Required]
        public string PrimaryEMail { get; set; }

        [Display(Name = "Phone Number (Best Available)")]
        [RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Not a valid phone number")]
        [Required]
        public string Phone { get; set; }
                
        [Display(Name = "Does Primary Contact Receive EMail Updates?")]
        [Required]
        public bool SendEmail { get; set; } = false;

        #endregion

        #region Display Properties

        [NotMapped]
        public List<SelectListItem> States => Utilities.SystemHelpers.SystemHelper.GetStateSelectList(State);

        #endregion
    }
}
