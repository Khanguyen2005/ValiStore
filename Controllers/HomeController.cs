using System.Linq;
using System.Web.Mvc;
using ValiModern.Models.EF;
using ValiModern.Models.ViewModels;

namespace ValiModern.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var vm = new HomeIndexVM
            {
                Banners = new System.Collections.Generic.List<Banner>(),
                Blocks = new System.Collections.Generic.List<CategoryBlockVM>()
            };
            using (var db = new ValiModernDBEntities())
            {
                vm.Banners = db.Banners.OrderBy(b => b.id).ToList();
                var categories = db.Categories.OrderBy(c => c.id).ToList();
                foreach (var c in categories)
                {
                    var products = db.Products
                        .Where(p => p.is_active && p.category_id == c.id)
                        .OrderByDescending(p => p.sold)
                        .Take(8)
                        .Select(p => new ProductCardVM
                        {
                            id = p.id,
                            name = p.name,
                            price = p.price,
                            original_price = p.original_price,
                            image_url = p.image_url,
                            description = p.description,
                            sold = p.sold
                        }).ToList();
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
    }
}