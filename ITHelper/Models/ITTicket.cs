using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ITHelper.Models
{
    public class ITTicket : Ticket
    {
        #region Enumerations

        public enum TicketCategory
        {
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
            Other = 99
        };

        #endregion

        #region Accessors

        [Display(Name = "PC/System Name")]
        [Required]
        public string PCName { get; set; } = System.Environment.MachineName;

        [Display(Name = "Nature of Issue")]
        [Required]
        public TicketCategory Category { get; set; } = TicketCategory.Uncertain;

        #endregion

        #region Display Properties

        public string CategoryDisplay => Utilities.SystemHelpers.EnumHelper<TicketCategory>.GetDisplayName(Category);

        #endregion
    }
}
