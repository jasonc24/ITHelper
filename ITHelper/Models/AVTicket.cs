using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static ITHelper.Models.Enumerations;

namespace ITHelper.Models
{
    public class AVTicket
    {
        #region Properties

        [Key]
        public Guid Id { get; set; } = new Guid();

        [Display(Name = "User Name")]
        [Required]
        public string Username { get; set; } = Environment.UserName;

        [Display(Name = "First Name")]
        [Required]
        public string FName { get; set; }

        [Display(Name = "Last Name")]
        public string LName { get; set; }

        [Display(Name = "E-Mail Address")]
        [EmailAddress]
        [Required]
        public string EMail { get; set; }

        [Display(Name = "Phone Number (Best Available)")]
        [Phone]
        [Required]
        public string Phone { get; set; }

        [Display(Name = "Ticket Category")]
        [Required]
        [ForeignKey("Category")]
        public Guid CategoryId { get; set; }
        public Category Category { get; set; }

        [Display(Name = "Who or what ministry is requesting video? ")]
        public string TargetMinistry { get; set; }

        [Display(Name = "Location")]
        [Required]
        [ForeignKey("Location")]
        public Guid? LocationId { get; set; }
        public Location Location { get; set; }

        [Display(Name = "What is your general idea that you looking to film and or have edited?")]
        [Required]
        public string GeneralIdea { get; set; }

        [Display(Name = "I already have existing footage.")]
        public bool ExistingFootage { get; set; } = false;

        [Display(Name = "I have created storyboards or a script for the video.")]
        public bool StoryBoards { get; set; } = false;

        [Display(Name = "When is the video needed by or what is the deadline for it?")]
        [Required]
        public DateTime Deadline { get; set; } = DateTime.Now.AddDays(30);

        [Display(Name = "Other relevant information or notes?")]
        public string Notes { get; set; }

        [Display(Name = "Ticket Status")]
        [Required]
        public TicketStatus Status { get; set; } = TicketStatus.New;

        [Display(Name = "Date Submitted")]
        public DateTimeOffset DateSubmitted { get; set; } = DateTimeOffset.Now;

        [Display(Name = "Last Updated")]
        public DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.Now;

        [Display(Name = "Updates")]
        public List<Update> Updates { get; set; }

        #region Foreign Key Support

        [NotMapped]
        public List<SelectListItem> Categories { get; set; }

        [NotMapped]
        public List<SelectListItem> Locations { get; set; }

        #endregion

        #endregion

        #region Display Properties

        [NotMapped]
        [Display(Name = "Date Submitted")]
        public string DateSubmittedDisplay => DateSubmitted.LocalDateTime.ToString();

        [NotMapped]
        [Display(Name = "Last Updated")]
        public string LastUpdatedDisplay => LastUpdated.LocalDateTime.ToString();

        [NotMapped]
        [Display(Name = "Ticket Status")]
        public string TicketStatusDisplay => Utilities.SystemHelpers.EnumHelper<TicketStatus>.GetDisplayName(Status);

        [NotMapped]
        [Display(Name = "Deadline")]
        public string DeadlineDisplay => Deadline.ToLocalTime().ToString();

        #endregion

        #region Methods

        public string GetColor()
        {
            var color = "none";
            switch (Status)
            {
                case TicketStatus.New:
                case TicketStatus.Reviewed:
                    color = "Orange";
                    break;

                case TicketStatus.AssignedInternally:
                case TicketStatus.AssignedExternally:
                    color = "LemonChiffon";
                    break;

                case TicketStatus.Closed:
                    color = "LimeGreen";
                    break;

                case TicketStatus.Rejected:
                    color = "Tomato";
                    break;

                default:
                    color = "none";
                    break;
            }

            return color;
        }

        #endregion

    }

}
