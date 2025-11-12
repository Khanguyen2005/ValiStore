using System.Collections.Generic;

namespace ValiModern.Models.ViewModels
{
    // ViewModel for Category page with filters
    public class CategoryProductsVM
    {
     public CategoryVM Category { get; set; }
        public List<ProductCardVM> Products { get; set; }
        public List<FilterOptionVM> AvailableColors { get; set; }
      public List<FilterOptionVM> AvailableSizes { get; set; }
        public int CurrentPage { get; set; }
   public int TotalPages { get; set; }
  public int TotalProducts { get; set; }
        public string Sort { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public List<string> SelectedColorNames { get; set; }
        public List<string> SelectedSizeNames { get; set; }
}

    public class CategoryVM
    {
      public int Id { get; set; }
     public string Name { get; set; }
        public string Slug { get; set; }
        public string Description { get; set; }
    }

    public class FilterOptionVM
    {
        public string Name { get; set; }
        public string HexCode { get; set; } // For colors
        public int Count { get; set; } // Number of products (distinct)
 }
}
