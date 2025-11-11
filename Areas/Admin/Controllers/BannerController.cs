using System.Linq;
using System.Net;
using System.Web.Mvc;
using ValiModern.Filters;
using ValiModern.Models.EF;

namespace ValiModern.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    public class BannerController : Controller
    {
        private readonly ValiModernDBEntities _db = new ValiModernDBEntities();

        // GET: Admin/Banner
        public ActionResult Index(string sort)
        {
            var banners = _db.Banners.AsQueryable();
            switch (sort)
            {
                case "id_desc":
                    banners = banners.OrderByDescending(b => b.id);
                    break;
                case "id_asc":
                default:
                    banners = banners.OrderBy(b => b.id);
                    break;
            }
            ViewBag.Sort = string.IsNullOrEmpty(sort) ? "id_asc" : sort;
            return View(banners.ToList());
        }

        // GET: Admin/Banner/Create
        public ActionResult Create()
        {
            return View(new Banner());
        }

        // POST: Admin/Banner/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "name,image_url")] Banner model)
        {
            if (!ModelState.IsValid) return View(model);
            var name = (model.name ?? string.Empty).Trim();
            var url = (model.image_url ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(name))
                ModelState.AddModelError("name", "Name is required.");
            if (string.IsNullOrWhiteSpace(url))
                ModelState.AddModelError("image_url", "Image URL is required.");
            if (!ModelState.IsValid) return View(model);
            _db.Banners.Add(new Banner { name = name, image_url = url });
            _db.SaveChanges();
            TempData["Success"] = "Banner created.";
            return RedirectToAction("Index");
        }

        // GET: Admin/Banner/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var banner = _db.Banners.Find(id);
            if (banner == null) return HttpNotFound();
            return View(banner);
        }

        // POST: Admin/Banner/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, [Bind(Include = "id,name,image_url")] Banner model)
        {
            var banner = _db.Banners.Find(id);
            if (banner == null) return HttpNotFound();
            if (!ModelState.IsValid) return View(banner);
            var name = (model.name ?? string.Empty).Trim();
            var url = (model.image_url ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(name))
                ModelState.AddModelError("name", "Name is required.");
            if (string.IsNullOrWhiteSpace(url))
                ModelState.AddModelError("image_url", "Image URL is required.");
            if (!ModelState.IsValid) return View(banner);
            banner.name = name;
            banner.image_url = url;
            _db.SaveChanges();
            TempData["Success"] = "Banner updated.";
            return RedirectToAction("Index");
        }

        // POST: Admin/Banner/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var banner = _db.Banners.Find(id);
            if (banner == null) return HttpNotFound();
            _db.Banners.Remove(banner);
            _db.SaveChanges();
            TempData["Success"] = "Banner deleted.";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
