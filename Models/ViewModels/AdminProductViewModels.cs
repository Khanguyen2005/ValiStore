using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using ValiModern.Models.EF;
using ValiModern.Services;

namespace ValiModern.Models.ViewModels
{
    public class ProductFormVM
    {
        public int? Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        [Display(Name = "Original Price"), Required(ErrorMessage = "Original price required")]
        public int OriginalPrice { get; set; }
        [Display(Name = "Price"), Required(ErrorMessage = "Price required")]
        public int Price { get; set; }
        [Range(0, int.MaxValue)]
        public int Stock { get; set; }
        [Display(Name = "Sold"), Range(0, int.MaxValue)]
        public int Sold { get; set; }
        [Display(Name = "Active")]
        public bool IsActive { get; set; }
        [Display(Name = "Category"), Required]
        public int CategoryId { get; set; }
        [Display(Name = "Brand"), Required]
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
