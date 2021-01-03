using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITHelper.Helpers
{
    public static class DecorationsHelper
    {
        public static string GetStatusColor(Models.Ticket.TicketStatus status)
        {
            var color = "Transparent";
            switch (status)
            {
                case Models.Ticket.TicketStatus.Submitted:
                    color = "Red";
                    break;

                case Models.Ticket.TicketStatus.Closed:
                    color = "Grey";
                    break;

                case Models.Ticket.TicketStatus.AssignedExternally:
                case Models.Ticket.TicketStatus.AssignedInternally:
                    color = "Green";
                    break;

                case Models.Ticket.TicketStatus.Reviewed:
                    color = "Yellow";
                    break;

                case Models.Ticket.TicketStatus.AwaitingUser:
                case Models.Ticket.TicketStatus.Monitoring:
                    color = "Orange";
                    break;
            }

            return color;
        }

    }
}
