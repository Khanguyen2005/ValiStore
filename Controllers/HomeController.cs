using System.Linq;
using System.Web.Mvc;
using ValiModern.Models.EF;

namespace ValiModern.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            using (var db = new ValiModernDBEntities())
            {
                // Banners
                var banners = db.Banners.OrderBy(b => b.id).ToList();

                // Map category names (assumed) -> tiếng Việt yêu cầu: Vali, Túi xách, Balo, Phụ kiện
                var valiProducts = GetProductsByCategoryName(db, "Vali",8);
                var tuiXachProducts = GetProductsByCategoryName(db, "Túi xách",8);
                var baloProducts = GetProductsByCategoryName(db, "Balo",8);
                var phuKienProducts = GetProductsByCategoryName(db, "Phụ kiện",8);

                ViewBag.Banners = banners;
                ViewBag.ValiProducts = valiProducts;
                ViewBag.TuiXachProducts = tuiXachProducts;
                ViewBag.BaloProducts = baloProducts;
                ViewBag.PhuKienProducts = phuKienProducts;
            }
            return View();
        }

        private static object GetProductsByCategoryName(ValiModernDBEntities db, string categoryName, int take)
        {
            var products = db.Products
                .Where(p => p.is_active && p.Category.name == categoryName)
                .OrderByDescending(p => p.sold)
                .Take(take)
                .Select(p => new
                {
                    p.id,
                    p.name,
                    p.price,
                    p.image_url,
                    p.description
                })
                .ToList();
            return products;
        }
    }
}