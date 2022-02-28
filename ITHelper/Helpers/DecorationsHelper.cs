namespace ITHelper.Helpers
{
    public static class DecorationsHelper
    {
        public static string GetStatusColor(Models.Enumerations.TicketStatus status)
        {
            var color = "Transparent";
            switch (status)
            {
                case Models.Enumerations.TicketStatus.Submitted:
                    color = "Red";
                    break;

                case Models.Enumerations.TicketStatus.Closed:
                    color = "Green";
                    break;

                case Models.Enumerations.TicketStatus.AssignedExternally:
                case Models.Enumerations.TicketStatus.AssignedInternally:
                    color = "Yellow";
                    break;

                case Models.Enumerations.TicketStatus.Reviewed:
                    color = "Yellow";
                    break;

                case Models.Enumerations.TicketStatus.AwaitingUser:
                case Models.Enumerations.TicketStatus.Monitoring:
                    color = "Orange";
                    break;
            }

            return color;
        }

    }
}
