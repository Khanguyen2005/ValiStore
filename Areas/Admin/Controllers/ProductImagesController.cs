using System;
using System.Collections.Generic;
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
    public class ProductImagesController : Controller
    {
        private readonly ValiModernDBEntities _db = new ValiModernDBEntities();
        private const string UploadRootVirtual = "/Uploads/products";
        private static readonly string[] AllowedExt = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        // GET: Admin/ProductImages?productId=1
        public ActionResult Index(int? productId)
        {
            if (productId == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var product = _db.Products.Find(productId);
            if (product == null) return HttpNotFound();
            var images = _db.Product_Images.Where(pi => pi.product_id == product.id)
            .OrderBy(pi => pi.sort_order).ThenBy(pi => pi.id).ToList();
            ViewBag.Product = product;
            return View(images);
        }

        // GET: Admin/ProductImages/Create?productId=1
        public ActionResult Create(int? productId)
        {
            if (productId == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var product = _db.Products.Find(productId);
            if (product == null) return HttpNotFound();
            ViewBag.Product = product;
            return View();
        }

        // POST: Admin/ProductImages/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(int productId, IEnumerable<HttpPostedFileBase> imageFiles)
        {
            var product = _db.Products.Find(productId);
            if (product == null) return HttpNotFound();
            if (imageFiles == null || !imageFiles.Any() || imageFiles.All(f => f == null || f.ContentLength == 0))
            {
                TempData["Error"] = "Please select at least one image.";
                return RedirectToAction("Create", new { productId });
            }
            int maxSort = _db.Product_Images.Where(pi => pi.product_id == productId).Select(pi => (int?)pi.sort_order).Max() ?? 0;
            var savedAny = false;
            foreach (var file in imageFiles)
            {
                if (file == null || file.ContentLength == 0) continue;
                string err;
                var rel = SaveUpload(file, out err);
                if (rel == null) { TempData["Error"] = err ?? "Invalid image"; continue; }
                maxSort++;
                var pi = new Product_Images
                {
                    product_id = productId,
                    image_url = rel,
                    is_main = false,
                    sort_order = maxSort
                };
                _db.Product_Images.Add(pi);
                savedAny = true;
            }
            _db.SaveChanges();
            if (savedAny)
            {
                // ensure at least one main image
                var hasMain = _db.Product_Images.Any(x => x.product_id == productId && x.is_main);
                if (!hasMain)
                {
                    var first = _db.Product_Images.Where(x => x.product_id == productId).OrderBy(x => x.sort_order).FirstOrDefault();
                    if (first != null) { first.is_main = true; _db.SaveChanges(); }
                }
                // sync product.image_url with current main
                var mainImg = _db.Product_Images.Where(x => x.product_id == productId && x.is_main).OrderBy(x => x.sort_order).FirstOrDefault();
                product.image_url = mainImg != null ? mainImg.image_url : product.image_url;
                _db.SaveChanges();
                TempData["Success"] = "Images uploaded.";
            }
            return RedirectToAction("Index", new { productId });
        }

        // POST: Admin/ProductImages/UploadMultiple (New endpoint for dropzone)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadMultiple(int productId, IEnumerable<HttpPostedFileBase> imageFiles)
        {
            var product = _db.Products.Find(productId);
            if (product == null)
            {
                TempData["Error"] = "Product not found.";
                return RedirectToAction("Index", "Product");
            }

            if (imageFiles == null || !imageFiles.Any() || imageFiles.All(f => f == null || f.ContentLength == 0))
            {
                TempData["Error"] = "Please select at least one image.";
                return RedirectToAction("Index", new { productId });
            }

            int maxSort = _db.Product_Images.Where(pi => pi.product_id == productId).Select(pi => (int?)pi.sort_order).Max() ?? 0;
            int uploadedCount = 0;
            var errors = new List<string>();

            foreach (var file in imageFiles)
            {
                if (file == null || file.ContentLength == 0) continue;

                string err;
                var relPath = SaveUpload(file, out err);

                if (relPath == null)
                {
                    errors.Add($"{file.FileName}: {err ?? "Upload failed"}");
                    continue;
                }

                maxSort++;
                var pi = new Product_Images
                {
                    product_id = productId,
                    image_url = relPath,
                    is_main = false,
                    sort_order = maxSort
                };
                _db.Product_Images.Add(pi);
                uploadedCount++;
            }

            if (uploadedCount > 0)
            {
                _db.SaveChanges();

                // Ensure at least one main image exists
                var hasMain = _db.Product_Images.Any(x => x.product_id == productId && x.is_main);
                if (!hasMain)
                {
                    var first = _db.Product_Images
                        .Where(x => x.product_id == productId)
                        .OrderBy(x => x.sort_order)
                        .FirstOrDefault();
                    if (first != null)
                    {
                        first.is_main = true;
                        _db.SaveChanges();
                    }
                }

                // Sync product.image_url with current main
                var mainImg = _db.Product_Images
                    .Where(x => x.product_id == productId && x.is_main)
                    .OrderBy(x => x.sort_order)
                    .FirstOrDefault();

                if (product != null && mainImg != null)
                {
                    product.image_url = mainImg.image_url;
                    _db.SaveChanges();
                }

                TempData["Success"] = $"Successfully uploaded {uploadedCount} image(s).";
            }

            if (errors.Any())
            {
                TempData["Error"] = string.Join("<br>", errors);
            }

            return RedirectToAction("Index", new { productId });
        }

        // POST: Admin/ProductImages/SetMain/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SetMain(int id)
        {
            var img = _db.Product_Images.Find(id);
            if (img == null) return HttpNotFound();
            var productId = img.product_id;
            var others = _db.Product_Images.Where(x => x.product_id == productId);
            foreach (var o in others) o.is_main = (o.id == id);
            // sync product main image
            var product = _db.Products.Find(productId);
            if (product != null) product.image_url = img.image_url;
            _db.SaveChanges();
            TempData["Success"] = "Main image updated.";
            return RedirectToAction("Index", new { productId });
        }

        // POST: Admin/ProductImages/UpdateOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateOrder(int productId, int[] ids, int[] orders)
        {
            if (ids != null && orders != null && ids.Length == orders.Length)
            {
                for (int i = 0; i < ids.Length; i++)
                {
                    var img = _db.Product_Images.Find(ids[i]);
                    if (img != null && img.product_id == productId)
                    {
                        img.sort_order = orders[i];
                    }
                }
                _db.SaveChanges();
                TempData["Success"] = "Sort order saved.";
            }
            return RedirectToAction("Index", new { productId });
        }

        // POST: Admin/ProductImages/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var img = _db.Product_Images.Find(id);
            if (img == null) return HttpNotFound();
            var productId = img.product_id;
            DeletePhysical(img.image_url);
            _db.Product_Images.Remove(img);
            _db.SaveChanges();
            // ensure main still exists
            var hasMain = _db.Product_Images.Any(x => x.product_id == productId && x.is_main);
            if (!hasMain)
            {
                var first = _db.Product_Images.Where(x => x.product_id == productId).OrderBy(x => x.sort_order).FirstOrDefault();
                if (first != null) { first.is_main = true; _db.SaveChanges(); }
            }
            // sync product.image_url with current main (or clear if none)
            var product = _db.Products.Find(productId);
            var mainImg = _db.Product_Images.Where(x => x.product_id == productId && x.is_main).OrderBy(x => x.sort_order).FirstOrDefault();
            if (product != null) product.image_url = mainImg != null ? mainImg.image_url : null;
            _db.SaveChanges();
            TempData["Success"] = "Image deleted.";
            return RedirectToAction("Index", new { productId });
        }

        private string SaveUpload(HttpPostedFileBase file, out string error)
        {
            error = null;
            if (file == null || file.ContentLength == 0) return null;
            var ext = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            if (string.IsNullOrEmpty(ext) || !AllowedExt.Contains(ext))
            { error = "Unsupported image format."; return null; }
            var safeBase = Slugify(Path.GetFileNameWithoutExtension(file.FileName));
            var finalName = safeBase + "-" + DateTime.UtcNow.Ticks + ext;
            var physicalDir = Server.MapPath(UploadRootVirtual);
            if (!Directory.Exists(physicalDir)) Directory.CreateDirectory(physicalDir);
            var physicalPath = Path.Combine(physicalDir, finalName);
            file.SaveAs(physicalPath);
            return UploadRootVirtual + "/" + finalName;
        }
        private void DeletePhysical(string relPath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(relPath)) return;
                if (!relPath.StartsWith("/Uploads/products")) return;
                var physical = Server.MapPath("~" + relPath);
                if (System.IO.File.Exists(physical)) System.IO.File.Delete(physical);
            }
            catch { }
        }
        private static string Slugify(string input)
        {
            if (string.IsNullOrEmpty(input)) return "product";
            var s = new string(input.Where(ch => char.IsLetterOrDigit(ch) || ch == '-' || ch == '_').ToArray());
            if (string.IsNullOrEmpty(s)) s = "product";
            return s.ToLowerInvariant();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
