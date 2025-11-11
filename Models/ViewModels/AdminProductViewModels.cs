using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using ValiModern.Services;

namespace ValiModern.Models.ViewModels
{
    public class ProductFormVM
    {
        public int? Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        [Display(Name = "Original Price")]
        public decimal OriginalPrice { get; set; }
        [Display(Name = "Price")]
        public decimal Price { get; set; }
        public int Stock { get; set; }
        [Display(Name = "Sold")]
        public int Sold { get; set; }
        [Display(Name = "Active")]
        public bool IsActive { get; set; }
        [Display(Name = "Category")]
        public int CategoryId { get; set; }
        [Display(Name = "Brand")]
        public int BrandId { get; set; }
        public string ImageUrl { get; set; }

        // Palettes
        public List<ColorOption> AllColors { get; set; } = new List<ColorOption>();
        public List<SizeOption> AllSizes { get; set; } = new List<SizeOption>();
        public int[] SelectedColorIds { get; set; } = new int[0];
        public int[] SelectedSizeIds { get; set; } = new int[0];

        // Dropdowns
        public IEnumerable<SelectListItem> Categories { get; set; }
        public IEnumerable<SelectListItem> Brands { get; set; }
    }
}
