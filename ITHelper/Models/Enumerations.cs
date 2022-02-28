using System.ComponentModel.DataAnnotations;

namespace ITHelper.Models
{
    public class Enumerations
    {
        public enum TicketStatus
        {
            [Display(Name = "New Ticket")]
            New = -1,
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
            Closed = 6,
            [Display(Name = "Rejected")]
            Rejected = 7
        };

        public enum TicketSeverity { Low = 1, Medium = 2, High = 3, Stratosphere = 4 }

    }
}
