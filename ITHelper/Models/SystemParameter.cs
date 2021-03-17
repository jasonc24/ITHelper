using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ITHelper.Models
{
    public class SystemParameter
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Parameter Name")]
        public string Name { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Value")]
        public string Value { get; set; }

        [Display(Name = "Timestamp Value")]
        public Nullable<System.DateTimeOffset> TimeStamp { get; set; }

        [Display(Name = "Required for Operation")]
        public bool Required { get; set; }

        [Display(Name = "User Editable")]
        public bool CanBeEdited { get; set; }

        [Display(Name = "Password")]
        public bool Password { get; set; }

        [Display(Name = "Date Created")]
        public System.DateTimeOffset DateCreated { get; set; }

        [Display(Name = "Last Updated")]
        public System.DateTimeOffset LastUpdated { get; set; }

        [Display(Name = "Updated By")]
        public string UpdatedBy { get; set; }
    }
}
