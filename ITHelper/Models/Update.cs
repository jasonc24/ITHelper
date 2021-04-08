using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static ITHelper.Models.Ticket;

namespace ITHelper.Models
{
    public class Update
    {
        [Key]
        public Guid Id { get; set; } = new Guid();

        [Display(Name = "Ticket")]
        [Required]
        public Ticket Ticket { get; set; }
        
        [Display(Name = "User Name")]
        [Required]
        public string Username { get; set; }

        [Display(Name = "Ticket Status")]
        [NotMapped]
        public TicketStatus Status { get; set; } = TicketStatus.Submitted;

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
