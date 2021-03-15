using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ITHelper.Models
{
    public class BuildingTicket : Ticket
    {

        public enum TicketCategory
        {
            [Display(Name = "Uncertain of Problem")]
            Uncertain = 1,
            [Display(Name = "Electrical")]
            Electrical = 2,
            [Display(Name = "Plumbing")]
            Plumbing = 3,
            [Display(Name = "Lawn & Landscaping")]
            Grounds = 4,
            [Display(Name = "Snow Removal")]
            Snow = 5,
            [Display(Name = "Furniture or Office")]
            Office = 6,
            [Display(Name = "Roof or Ceiling")]
            Roofing = 7,
            [Display(Name = "Water Leaks")]
            WaterLeak = 8,
            [Display(Name = "All Other Problems")]
            Other = 99
        };

        #region Accessors

        [Display(Name = "Location")]
        [Required]
        public string Location { get; set; }

        [Display(Name = "Nature of Issue")]
        [Required]
        public TicketCategory Category { get; set; } = TicketCategory.Uncertain;

        #endregion

        #region Display Properties

        public string CategoryDisplay => Utilities.SystemHelpers.EnumHelper<TicketCategory>.GetDisplayName(Category);

        #endregion

    }
}
