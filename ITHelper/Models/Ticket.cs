﻿using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static ITHelper.Models.Enumerations;

namespace ITHelper.Models
{
    public class Ticket
    {    
        #region Accessors

        [Key]
        public Guid Id { get; set; } = new Guid();

        [Display(Name = "User Name")]
        [Required]
        public string Username { get; set; }

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

        [Display(Name = "Description of Problem")]
        [Required]
        public string Description { get; set; }

        [Display(Name = "PC/System Name")]
        public string PCName { get; set; } = Environment.MachineName;

        [Display(Name = "Location")]
        [Required]
        [ForeignKey("Location")]
        public  Guid? LocationId { get; set; }
        public Location Location { get; set; }  

        [Display(Name = "Ticket Urgency")]
        [Required]
        [Range(1, 4)]
        public TicketSeverity Severity { get; set; } = TicketSeverity.Low;

        [Display(Name ="Ticket Status")]
        [Required]
        public TicketStatus Status { get; set; } = TicketStatus.New;

        [Display(Name = "Ticket Assigned To (EMail Address) - Optional")]
        [EmailAddress]
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
        [Display(Name = "Ticket Severity")]
        public string TicketSeverityDisplay => Severity.ToString();

        [NotMapped]
        [Display(Name = "Parent Category")]
        public Guid ParentCategory { get; set; }

        [NotMapped]
        public List<SelectListItem> ParentCategories { get; set; }

        [NotMapped]
        public List<SelectListItem> Categories { get; set; }

        [NotMapped]
        public List<SelectListItem> Locations { get; set; }

        #endregion

        #region Methods

        public string GetColor()
        {
            if (Status == TicketStatus.Closed)
                return "LimeGreen";

            var color = "none";
            switch(Severity)
            {
                case TicketSeverity.Low:
                    color = "none";
                    break;

                case TicketSeverity.Medium:
                    color = "LemonChiffon";
                    break;

                case TicketSeverity.High:
                    color = "Orange";
                    break;

                case TicketSeverity.Stratosphere:
                    color = "Tomato";
                    break;
            }

            return color;
        }

        #endregion
    }
}
