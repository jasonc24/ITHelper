using System.ComponentModel.DataAnnotations;

namespace ITHelper.Models
{
    public class BuildingTicket : Ticket
    {
        public new enum TicketCategory
        {
            [Display(Name = "Uncertain of Problem")]
            Uncertain = 1,
            [Display(Name = "HVAC")]
            Electrical = 2,
            [Display(Name = "Roof")]
            Plumbing = 3,
            [Display(Name = "Painting")]
            Grounds = 4,
            [Display(Name = "Mechanical")]
            Snow = 5,
            [Display(Name = "Electrical")]
            Office = 6,
            [Display(Name = "Lighting")]
            Roofing = 7,
            [Display(Name = "Outdoors")]
            WaterLeak = 8,
            [Display(Name = "Security")]
            Security = 9,
            [Display(Name = "Utilities")]
            Utilities = 10,
            [Display(Name = "All Other Problems")]
            Other = 99
        };

        #region Accessors

        [Display(Name = "Location")]
        [Required]
        public string Location { get; set; }

        #endregion
    }
}
