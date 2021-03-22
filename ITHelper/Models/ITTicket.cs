using System.ComponentModel.DataAnnotations;

namespace ITHelper.Models
{
    public class ITTicket : Ticket
    {
        #region Accessors

        [Display(Name = "PC/System Name")]
        [Required]
        public string PCName { get; set; } = System.Environment.MachineName;

        #endregion
    }
}
