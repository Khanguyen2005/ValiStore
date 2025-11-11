using System.Collections.Generic;
using ValiModern.Models.EF;

namespace ValiModern.Models.ViewModels
{
    public class ProductCardVM
    {
        public int id { get; set; }
        public string name { get; set; }
        public decimal price { get; set; }
        public string image_url { get; set; }
        public string description { get; set; }
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
}
