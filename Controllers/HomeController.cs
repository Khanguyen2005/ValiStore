using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using ValiModern.Models.EF;
using ValiModern.Models.ViewModels;

namespace ValiModern.Controllers
{
    public class HomeController : Controller
    {
        [OutputCache(Duration = 300, VaryByParam = "none")] // Cache 5 minutes
        public ActionResult Index()
        {
            var vm = new HomeIndexVM
            {
                Banners = new System.Collections.Generic.List<Banner>(),
                Blocks = new System.Collections.Generic.List<CategoryBlockVM>()
            };
            using (var db = new ValiModernDBEntities())
            {
                // Optimize: get banners
                vm.Banners = db.Banners.OrderBy(b => b.id).ToList();
                
                // Optimize: get all categories and products in one go instead of N+1 queries
                var categories = db.Categories.OrderBy(c => c.id).ToList();
                
                // Get all active products for these categories in ONE query
                var categoryIds = categories.Select(c => c.id).ToList();
                var allProducts = db.Products
                    .Where(p => p.is_active && categoryIds.Contains(p.category_id))
                    .OrderByDescending(p => p.sold)
                    .Select(p => new {
                        p.id,
                        p.name,
                        p.price,
                        p.original_price,
                        p.image_url,
                        p.description,
                        p.sold,
                        p.category_id
                    })
                    .ToList()
                    .GroupBy(p => p.category_id)
                    .ToDictionary(g => g.Key, g => g.Take(8).ToList());
                
                // Build category blocks
                foreach (var c in categories)
                {
                    var products = allProducts.ContainsKey(c.id) 
                        ? allProducts[c.id].Select(p => new ProductCardVM
                        {
                            id = p.id,
                            name = p.name,
                            price = p.price,
                            original_price = p.original_price,
                            image_url = p.image_url,
                            description = p.description,
                            sold = p.sold
                        }).ToList()
                        : new System.Collections.Generic.List<ProductCardVM>();
                        
                    vm.Blocks.Add(new CategoryBlockVM
                    {
                        CategoryId = c.id,
                        CategoryName = c.name,
                        Products = products
                    });
                }
            }
            return View(vm);
        }

        public ActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Contact(string name, string email, string subject, string message)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || 
                string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(message))
            {
                TempData["Error"] = "Please fill in all fields.";
                return View();
            }

            // Here you can add logic to save to database or send email
            // For now, just show success message
            TempData["Success"] = "Thank you for contacting us! We will get back to you soon.";
            return RedirectToAction("Contact");
        }
    }
}