using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ITHelper.Models
{
    public class Category
    {
        #region Properties

        [Required]
        public Guid Id { get; set; } = new Guid();

        [Display(Name = "Category Name")]
        [Required]
        public string Name { get; set; }

        [Display(Name = "Parent Category (Optional)")]
        [ForeignKey("ParentCategory")]
        public Guid? ParentCategoryId { get; set; }
        public Category ParentCategory { get; set; }

        [Display(Name = "Primary Contact")]
        [Required]
        public string PrimaryContact { get; set; }

        [Display(Name = "Primary Contact Username (ex: \"HopeChurch\\JDoe\")")]
        [Required]
        public string UserName { get; set; }

        [Display(Name = "Primary Contact's E-Mail Address")]
        [EmailAddress]
        [Required]
        public string PrimaryEMail { get; set; }

        [Display(Name = "Phone Number (Best Available)")]
        [RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Not a valid phone number")]
        public string Phone { get; set; }

        #endregion

        #region Display Methods

        [NotMapped]
        public string DisplayName => ParentCategory == null ? $"{Name} - General" : $"{ParentCategory.Name} - {Name}";

        [NotMapped]
        public List<SelectListItem> ParentCategories { get; set; } = new List<SelectListItem>();

        #endregion
    }
}
