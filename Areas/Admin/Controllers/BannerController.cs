using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ValiModern.Filters;
using ValiModern.Models.EF;

namespace ValiModern.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    public class BannerController : Controller
    {
        private readonly ValiModernDBEntities _db = new ValiModernDBEntities();
        private const string UploadRootVirtual = "/Uploads/banners"; // stored in DB
        private static readonly string[] AllowedExt = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

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
        public ActionResult Create([Bind(Include = "name")] Banner model, HttpPostedFileBase imageFile)
        {
            if (!ModelState.IsValid) return View(model);
            var name = (model.name ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(name))
                ModelState.AddModelError("name", "Name is required.");

            var relPath = SaveUpload(imageFile, out string error);
            if (relPath == null)
            {
                if (!string.IsNullOrEmpty(error)) ModelState.AddModelError("imageFile", error);
                else ModelState.AddModelError("imageFile", "Image is required.");
            }
            if (!ModelState.IsValid) return View(model);

            _db.Banners.Add(new Banner { name = name, image_url = relPath });
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
        public ActionResult Edit(int id, [Bind(Include = "id,name")] Banner model, HttpPostedFileBase imageFile)
        {
            var banner = _db.Banners.Find(id);
            if (banner == null) return HttpNotFound();
            var name = (model.name ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(name))
                ModelState.AddModelError("name", "Name is required.");

            string relPath = banner.image_url; // keep current
            if (imageFile != null && imageFile.ContentLength > 0)
            {
                relPath = SaveUpload(imageFile, out string error);
                if (relPath == null)
                {
                    if (!string.IsNullOrEmpty(error)) ModelState.AddModelError("imageFile", error);
                }
            }
            if (!ModelState.IsValid) return View(banner);

            // update
            banner.name = name;
            if (!string.IsNullOrEmpty(relPath) && relPath != banner.image_url)
            {
                DeletePhysical(banner.image_url);
                banner.image_url = relPath;
            }
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
            DeletePhysical(banner.image_url);
            _db.Banners.Remove(banner);
            _db.SaveChanges();
            TempData["Success"] = "Banner deleted.";
            return RedirectToAction("Index");
        }

        private string SaveUpload(HttpPostedFileBase file, out string error)
        {
            error = null;
            if (file == null || file.ContentLength == 0) return null;
            var ext = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            if (string.IsNullOrEmpty(ext) || !AllowedExt.Contains(ext))
            {
                error = "Unsupported image format.";
                return null;
            }
            var fileNameNoExt = Path.GetFileNameWithoutExtension(file.FileName);
            var safeBase = Slugify(fileNameNoExt);
            var finalName = string.Format("{0}-{1}{2}", safeBase, System.DateTime.UtcNow.Ticks, ext);
            var physicalDir = Server.MapPath(UploadRootVirtual);
            if (!Directory.Exists(physicalDir)) Directory.CreateDirectory(physicalDir);
            var physicalPath = Path.Combine(physicalDir, finalName);
            file.SaveAs(physicalPath);
            // return relative path for DB
            return UploadRootVirtual + "/" + finalName;
        }

        private void DeletePhysical(string relPath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(relPath)) return;
                if (!relPath.StartsWith("/Uploads/banners")) return; // safety
                var physical = Server.MapPath("~" + relPath);
                if (System.IO.File.Exists(physical)) System.IO.File.Delete(physical);
            }
            catch { }
        }

        private static string Slugify(string input)
        {
            if (string.IsNullOrEmpty(input)) return "banner";
            var s = new string(input.Where(ch => char.IsLetterOrDigit(ch) || ch == '-' || ch == '_').ToArray());
            if (string.IsNullOrEmpty(s)) s = "banner";
            return s.ToLowerInvariant();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
