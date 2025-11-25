using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ValiModern.Filters;
using ValiModern.Helpers;
using ValiModern.Models.EF;

namespace ValiModern.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    public class BrandController : Controller
    {
        private readonly ValiModernDBEntities _db = new ValiModernDBEntities();
        private const string UploadRootVirtual = "/Uploads/brands";
        private static readonly string[] AllowedExt = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        // GET: Admin/Brand
        public ActionResult Index(string q, string sort)
        {
            // OPTIMIZE: Use AsNoTracking for read-only listing
            var brands = _db.Brands.AsNoTracking().AsQueryable();
            
            if (!string.IsNullOrWhiteSpace(q))
            {
                var k = q.Trim().ToLower();
                brands = brands.Where(b => b.name.ToLower().Contains(k));
            }
            switch (sort)
            {
                case "id_desc": brands = brands.OrderByDescending(b => b.id); break;
                case "name_asc": brands = brands.OrderBy(b => b.name); break;
                case "name_desc": brands = brands.OrderByDescending(b => b.name); break;
                case "id_asc":
                default: brands = brands.OrderBy(b => b.id); break;
            }
            ViewBag.Sort = string.IsNullOrEmpty(sort) ? "id_asc" : sort;
            ViewBag.Query = q;
            return View(brands.ToList());
        }

        // GET: Admin/Brand/Create
        public ActionResult Create()
        {
            return View(new Brand());
        }

        // POST: Admin/Brand/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "name")] Brand model, HttpPostedFileBase imageFile)
        {
            var name = (model.name ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(name)) ModelState.AddModelError("name", "Name is required.");
            
            // OPTIMIZE: Use AsNoTracking for duplicate check
            if (_db.Brands.AsNoTracking().Any(b => b.name.ToLower() == name.ToLower())) 
                ModelState.AddModelError("name", "Brand already exists.");
                
            var relPath = SaveUpload(imageFile, out string error);
            if (relPath == null) ModelState.AddModelError("imageFile", error ?? "Image is required.");
            if (!ModelState.IsValid) return View(model);
            var brand = new Brand { name = name, image_url = relPath };
            _db.Brands.Add(brand);
            _db.SaveChanges();
            
            // Invalidate cache when brand is created
            CacheHelper.InvalidateBrandCache();
            
            TempData["Success"] = "Brand created.";
            return RedirectToAction("Index");
        }

        // GET: Admin/Brand/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            
            // Use Find for edit (needs tracking)
            var brand = _db.Brands.Find(id);
            if (brand == null) return HttpNotFound();
            return View(brand);
        }

        // POST: Admin/Brand/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, [Bind(Include = "id,name")] Brand model, HttpPostedFileBase imageFile)
        {
            var brand = _db.Brands.Find(id);
            if (brand == null) return HttpNotFound();
            var name = (model.name ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(name)) ModelState.AddModelError("name", "Name is required.");
            
            // OPTIMIZE: Use AsNoTracking for duplicate check
            if (_db.Brands.AsNoTracking().Any(b => b.id != id && b.name.ToLower() == name.ToLower())) 
                ModelState.AddModelError("name", "Brand already exists.");
                
            string rel = brand.image_url;
            if (imageFile != null && imageFile.ContentLength > 0)
            {
                rel = SaveUpload(imageFile, out string error);
                if (rel == null) ModelState.AddModelError("imageFile", error ?? "Invalid image.");
            }
            if (!ModelState.IsValid) return View(brand);
            brand.name = name;
            if (!string.IsNullOrEmpty(rel) && rel != brand.image_url)
            {
                DeletePhysical(brand.image_url);
                brand.image_url = rel;
            }
            _db.SaveChanges();
            
            // Invalidate cache when brand is updated
            CacheHelper.InvalidateBrandCache();
            
            TempData["Success"] = "Brand updated.";
            return RedirectToAction("Index");
        }

        // POST: Admin/Brand/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var brand = _db.Brands.Find(id);
            if (brand == null) return HttpNotFound();
            
            // OPTIMIZE: Use AsNoTracking for usage check
            var inUse = _db.Products.AsNoTracking().Any(p => p.brandId == id);
            if (inUse)
            {
                TempData["Error"] = "Cannot delete brand while products are using it.";
                return RedirectToAction("Index");
            }
            DeletePhysical(brand.image_url);
            _db.Brands.Remove(brand);
            _db.SaveChanges();
            
            // Invalidate cache when brand is deleted
            CacheHelper.InvalidateBrandCache();
            
            TempData["Success"] = "Brand deleted.";
            return RedirectToAction("Index");
        }

        private string SaveUpload(HttpPostedFileBase file, out string error)
        {
            error = null;
            if (file == null || file.ContentLength == 0) return null;
            var ext = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            if (string.IsNullOrEmpty(ext) || !AllowedExt.Contains(ext)) { error = "Unsupported image format."; return null; }
            var safeBase = Slugify(Path.GetFileNameWithoutExtension(file.FileName));
            var final = safeBase + "-" + System.DateTime.UtcNow.Ticks + ext;
            var dir = Server.MapPath(UploadRootVirtual);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            var path = Path.Combine(dir, final);
            file.SaveAs(path);
            return UploadRootVirtual + "/" + final;
        }
        private void DeletePhysical(string rel)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(rel)) return;
                if (!rel.StartsWith("/Uploads/brands")) return;
                var p = Server.MapPath("~" + rel);
                if (System.IO.File.Exists(p)) System.IO.File.Delete(p);
            }
            catch { }
        }
        private static string Slugify(string input)
        {
            if (string.IsNullOrEmpty(input)) return "brand";
            var s = new string(input.Where(ch => char.IsLetterOrDigit(ch) || ch == '-' || ch == '_').ToArray());
            if (string.IsNullOrEmpty(s)) s = "brand"; return s.ToLowerInvariant();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
