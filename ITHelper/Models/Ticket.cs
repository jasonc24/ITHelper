using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ITHelper.Models
{
    public class Ticket
    {
        #region Enumerations

        public enum TicketCategory { 
            [Display(Name = "Uncertain of Problem")]
            Uncertain = 1,
            [Display(Name = "E-Mail Related Issue")]
            Email = 2,
            [Display(Name = "Adobe Suite of Products")]
            Adobe = 3,
            [Display(Name = "Networking (excluding WiFi)")]
            Networking = 4,
            [Display(Name = "WiFi Connectivity")]
            WiFi = 5,
            [Display(Name = "User Logins or Password Reset")]
            Login = 6,
            [Display(Name = "QuickBooks")]
            Quickbooks = 7,
            [Display(Name = "Hardware")]
            Hardware = 8,
            [Display(Name = "VPN and Remote Access")]
            VPN = 9,
            [Display(Name = "Microsoft Office")]
            MSOffice = 10,
            [Display(Name = "New Equipment Requests")]
            NewEquipmet = 11,
            [Display(Name = "G-Suite (Google Docs, Google Sheets, etc.)")]
            GSuite = 12,
            [Display(Name = "All Other Problems")]
            Other = 99 };

        public enum TicketStatus {
            [Display(Name = "Ticket Submitted - Awaiting Review")]
            Submitted = 0,
            [Display(Name = "Ticket Reviewed - Awaiting Assignment")]
            Reviewed = 1,
            [Display(Name = "Assigned to Internal Hope Resource")]
            AssignedInternally = 2,
            [Display(Name = "Assigned to External Service Provider")]
            AssignedExternally = 3,
            [Display(Name = "Problem Resolved - Monitoring for Confirmation")]
            Monitoring = 4,
            [Display(Name = "Awaiting Information/Action from User")]
            AwaitingUser = 5,
            [Display(Name = "Issue Resolved")]
            Closed = 6 
        };

        public enum TicketSeverity { Low = 1, Medium = 2, High = 3, Stratosphere = 4 }

        #endregion

        #region Accessors

        [System.ComponentModel.DataAnnotations.Key]
        public Guid Id { get; set; } = new Guid();

        [Display(Name = "User Name")]
        [Required]
        public string Username { get; set; }

        [Display(Name = "First Name")]
        [Required]
        public string FName { get; set; }

        [Display(Name = "Last Name")]
        public string LName { get; set; }

        [Display(Name = "PC/System Name")]
        [Required]
        public string PCName { get; set; } = System.Environment.MachineName;

        [Display(Name = "E-Mail Address")]
        [EmailAddress]
        [Required]
        public string EMail { get; set; }

        [Display(Name = "Phone Number (Best Available)")]
        [Phone]
        [Required]
        public string Phone { get; set; }

        [Display(Name = "Issue Category")]
        [Required]
        public TicketCategory Category { get; set; }

        [Display(Name = "Description of Problem")]
        [Required]
        public string Description { get; set; }

        [Display(Name = "Ticket Urgency")]
        [Required]
        [Range(1, 4)]
        public TicketSeverity Severity { get; set; } = TicketSeverity.Low;

        [Display(Name ="Ticket Status")]
        public TicketStatus Status { get; set; } = TicketStatus.Submitted;

        [Display(Name = "Ticket Assigned To")]
        public string AssignedTo { get; set; }

        [Display(Name = "Issue Notes")]
        public string Notes { get; set; }

        [Display(Name = "Resolution Notes")]
        public string Resolution { get; set; }

        [Display(Name = "Date Submitted")]
        public DateTimeOffset DateSubmitted { get; set; } = DateTimeOffset.Now;

        [Display(Name = "Last Updated")]
        public DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.Now;

        [Display(Name = "Updates")]
        public List<Update> Updates { get; set; }

        #endregion
    }
}
