using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using ValiModern.Helpers;
using ValiModern.Models.EF;

namespace ValiModern.Controllers
{
    /// <summary>
    /// WarmUp Controller - Used to pre-warm the application
    /// Can be called by IIS Application Initialization or monitoring tools
    /// </summary>
    public class WarmUpController : Controller
    {
        private readonly ValiModernDBEntities _db = new ValiModernDBEntities();

        /// <summary>
        /// GET: /WarmUp
        /// Pre-loads common data and caches to reduce first-request latency
        /// </summary>
        public ActionResult Index()
        {
            try
            {
                // OPTIMIZE: Pre-load and cache categories
                var categories = CacheHelper.GetOrSet(
                    CacheHelper.KEY_CATEGORIES,
                    () => _db.Categories.AsNoTracking().OrderBy(c => c.name).ToList(),
                    10
                );

                // OPTIMIZE: Pre-load and cache brands
                var brands = CacheHelper.GetOrSet(
                    CacheHelper.KEY_BRANDS,
                    () => _db.Brands.AsNoTracking().OrderBy(b => b.name).ToList(),
                    10
                );

                // OPTIMIZE: Pre-load layout categories
                var layoutCategories = CacheHelper.GetOrSet(
                    CacheHelper.KEY_LAYOUT_CATEGORIES,
                    () => _db.Categories.AsNoTracking().OrderBy(c => c.name).ToList(),
                    10
                );

                // Return simple success message
                return Content($"OK - Warmed up at {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}. Categories: {categories?.Count ?? 0}, Brands: {brands?.Count ?? 0}");
            }
            catch (System.Exception ex)
            {
                return Content($"ERROR - {ex.Message}");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
