using System.Collections.Generic;
using ValiModern.Models.EF;

namespace ValiModern.Models.ViewModels
{
    public class ProductCardVM
    {
        public int id { get; set; }
        public string name { get; set; }
        public decimal price { get; set; }
        public decimal original_price { get; set; }
        public string image_url { get; set; }
        public string description { get; set; }
        public int sold { get; set; }
        public string brand_name { get; set; }
        public string category_name { get; set; }
        public bool HasDiscount => original_price > price && original_price > 0;
        public int DiscountPercent => HasDiscount ? (int)((original_price - price) / original_price * 100) : 0;
    }

    public class CategoryBlockVM
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public List<ProductCardVM> Products { get; set; }
    }

    public class HomeIndexVM
    {
        public List<Banner> Banners { get; set; }
        public List<CategoryBlockVM> Blocks { get; set; }
    }

    // ViewModel for Product Index (Shop page)
    public class ProductIndexVM
    {
        public List<ProductCardVM> Products { get; set; }
        public List<Category> Categories { get; set; }
        public List<Brand> Brands { get; set; }
        public string CurrentCategory { get; set; }
        public string CurrentBrand { get; set; }
        public string CurrentSort { get; set; }
        public string SearchQuery { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalProducts { get; set; }
    }

    // ViewModel for Product Details page
    public class ProductDetailVM
    {
        public Product Product { get; set; }
        public List<ProductCardVM> RelatedProducts { get; set; }
    }
}
