using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using ValiModern.Models.EF;
using ValiModern.Models.ViewModels;

namespace ValiModern.Controllers
{
    public class ProductController : Controller
    {
        private readonly ValiModernDBEntities _db = new ValiModernDBEntities();

// GET: Product
     public ActionResult Index(string category, string brand, string sort, string q, int page = 1)
        {
            int pageSize = 12;
  var products = _db.Products
  .Include(p => p.Category)
           .Include(p => p.Brand)
                .Include(p => p.Colors)
    .Include(p => p.Sizes)
              .Where(p => p.is_active)
     .AsQueryable();

       // Filter by search query
      if (!string.IsNullOrWhiteSpace(q))
 {
   q = q.Trim().ToLower();
       products = products.Where(p => p.name.ToLower().Contains(q) || p.description.ToLower().Contains(q));
         }

            // Filter by category
     if (!string.IsNullOrWhiteSpace(category))
      {
       products = products.Where(p => p.Category != null && p.Category.name == category);
   }

       // Filter by brand
   if (!string.IsNullOrWhiteSpace(brand))
   {
          products = products.Where(p => p.Brand != null && p.Brand.name == brand);
  }

     // Sort
            switch (sort)
            {
  case "price_asc":
    products = products.OrderBy(p => p.price);
   break;
        case "price_desc":
   products = products.OrderByDescending(p => p.price);
             break;
        case "name_asc":
       products = products.OrderBy(p => p.name);
     break;
  case "name_desc":
         products = products.OrderByDescending(p => p.name);
         break;
      case "newest":
        products = products.OrderByDescending(p => p.id);
                break;
        default:
         products = products.OrderByDescending(p => p.sold); // Best selling
              break;
   }

            var totalProducts = products.Count();
        var totalPages = (int)Math.Ceiling(totalProducts / (double)pageSize);

    var productList = products
                .Skip((page - 1) * pageSize)
 .Take(pageSize)
      .ToList();

            var vm = new ProductIndexVM
 {
     Products = productList.Select(p => new ProductCardVM
           {
         id = p.id,
 name = p.name,
       price = p.price,
          original_price = p.original_price,
       image_url = p.image_url ?? "/Content/images/placeholder.png",
   sold = p.sold
    }).ToList(),
        Categories = _db.Categories.OrderBy(c => c.name).ToList(),
                Brands = _db.Brands.OrderBy(b => b.name).ToList(),
    CurrentCategory = category,
       CurrentBrand = brand,
           CurrentSort = sort,
            SearchQuery = q,
    CurrentPage = page,
       TotalPages = totalPages,
 TotalProducts = totalProducts
  };

   return View(vm);
}

        // GET: Product/Details/5
      public ActionResult Details(int? id)
        {
            if (id == null)
          {
     return RedirectToAction("Index");
      }

  var product = _db.Products
         .Include(p => p.Category)
   .Include(p => p.Brand)
  .Include(p => p.Colors)
       .Include(p => p.Sizes)
       .Include(p => p.Product_Images)
            .FirstOrDefault(p => p.id == id && p.is_active);

       if (product == null)
     {
          TempData["Error"] = "Product not found.";
                return RedirectToAction("Index");
        }

            // Get related products (same category)
    var relatedProducts = _db.Products
         .Where(p => p.is_active && p.category_id == product.category_id && p.id != product.id)
                .OrderByDescending(p => p.sold)
   .Take(4)
 .Select(p => new ProductCardVM
         {
           id = p.id,
        name = p.name,
          price = p.price,
             original_price = p.original_price,
   image_url = p.image_url,
        sold = p.sold
            })
                .ToList();

     var vm = new ProductDetailVM
            {
      Product = product,
  RelatedProducts = relatedProducts
            };

       return View(vm);
        }

      protected override void Dispose(bool disposing)
        {
      if (disposing) _db.Dispose();
 base.Dispose(disposing);
      }
    }
}
