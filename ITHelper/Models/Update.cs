using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ITHelper.Models
{
    public class Update
    {
        [System.ComponentModel.DataAnnotations.Key]
        public Guid Id { get; set; } = new Guid();

        [Display(Name = "Ticket")]
        [Required]
        public Ticket Ticket { get; set; }
        
        [Display(Name = "User Name")]
        [Required]
        public string Username { get; set; }

        [Display(Name = "Notes")]
        [Required]
        public string Notes { get; set; }

        [Display(Name = "Is the Issue Resolved?")]
        [Required]
        public bool IsResolved { get; set; } = false;

        [Display(Name = "Update Time")]
        [Required]
        public DateTimeOffset DateCreated { get; set; }
    }
}
