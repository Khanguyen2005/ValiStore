using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ValiModern.Filters;
using ValiModern.Models.EF;
using ValiModern.Models.ViewModels;
using ValiModern.Services;

namespace ValiModern.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    public class ProductController : Controller
    {
        private readonly ValiModernDBEntities _db = new ValiModernDBEntities();
        private const string UploadRootVirtual = "/Uploads/products"; // main image
        private static readonly string[] AllowedExt = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        // GET: Admin/Product
        public ActionResult Index(string q, string sort)
        {
            var products = _db.Products.Include(p => p.Category).Include(p => p.Brand).AsQueryable();
            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim().ToLower();
                products = products.Where(p => p.name.ToLower().Contains(q) || (p.Category != null && p.Category.name.ToLower().Contains(q)) || (p.Brand != null && p.Brand.name.ToLower().Contains(q)));
            }
            switch (sort)
            {
                case "id_desc": products = products.OrderByDescending(p => p.id); break;
                case "price_asc": products = products.OrderBy(p => p.price); break;
                case "price_desc": products = products.OrderByDescending(p => p.price); break;
                case "sold_desc": products = products.OrderByDescending(p => p.sold); break;
                case "sold_asc": products = products.OrderBy(p => p.sold); break;
                case "id_asc":
                default: products = products.OrderBy(p => p.id); break;
            }
            ViewBag.Sort = string.IsNullOrEmpty(sort) ? "id_asc" : sort;
            ViewBag.Query = q;
            var list = products.Take(300).ToList();
            return View(list);
        }

        // GET: Admin/Product/Create
        public ActionResult Create()
        {
            var vm = BuildFormVM(null);
            return View(vm);
        }

        // POST: Admin/Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ProductFormVM vm, HttpPostedFileBase imageFile)
        {
            if (!ModelState.IsValid)
            {
                vm = BuildFormVM(null, vm);
                return View(vm);
            }
            string relImage = SaveUpload(imageFile, out string error);
            if (relImage == null && !string.IsNullOrEmpty(error))
                ModelState.AddModelError("ImageUrl", error);
            if (!ModelState.IsValid)
            {
                vm = BuildFormVM(null, vm);
                return View(vm);
            }
            var product = new Product
            {
                name = vm.Name.Trim(),
                description = vm.Description,
                original_price = vm.OriginalPrice,
                price = vm.Price,
                stock = vm.Stock,
                sold = vm.Sold,
                category_id = vm.CategoryId,
                brandId = vm.BrandId,
                is_active = vm.IsActive,
                image_url = relImage
            };
            _db.Products.Add(product);
            _db.SaveChanges();
            TempData["Success"] = "Product created.";
            return RedirectToAction("Index");
        }

        // GET: Admin/Product/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var product = _db.Products.Find(id);
            if (product == null) return HttpNotFound();
            var vm = BuildFormVM(product);
            return View(vm);
        }

        // POST: Admin/Product/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, ProductFormVM vm, HttpPostedFileBase imageFile)
        {
            var product = _db.Products.Find(id);
            if (product == null) return HttpNotFound();
            if (!ModelState.IsValid)
            {
                vm = BuildFormVM(product, vm);
                return View(vm);
            }
            if (imageFile != null && imageFile.ContentLength > 0)
            {
                var relImage = SaveUpload(imageFile, out string error);
                if (relImage == null && !string.IsNullOrEmpty(error))
                {
                    ModelState.AddModelError("ImageUrl", error);
                    vm = BuildFormVM(product, vm);
                    return View(vm);
                }
                DeletePhysical(product.image_url);
                product.image_url = relImage;
            }
            product.name = vm.Name.Trim();
            product.description = vm.Description;
            product.original_price = vm.OriginalPrice;
            product.price = vm.Price;
            product.stock = vm.Stock;
            product.sold = vm.Sold;
            product.category_id = vm.CategoryId;
            product.brandId = vm.BrandId;
            product.is_active = vm.IsActive;
            _db.SaveChanges();
            TempData["Success"] = "Product updated.";
            return RedirectToAction("Index");
        }

        // POST: Admin/Product/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var product = _db.Products.Find(id);
            if (product == null) return HttpNotFound();
            DeletePhysical(product.image_url);
            _db.Products.Remove(product);
            _db.SaveChanges();
            TempData["Success"] = "Product deleted.";
            return RedirectToAction("Index");
        }

        private ProductFormVM BuildFormVM(Product product, ProductFormVM source = null)
        {
            var vm = source ?? new ProductFormVM();
            vm.AllColors = PaletteService.GetColors();
            vm.AllSizes = PaletteService.GetSizes();
            vm.Categories = _db.Categories.OrderBy(c => c.name).Select(c => new SelectListItem { Value = c.id.ToString(), Text = c.name }).ToList();
            vm.Brands = _db.Brands.OrderBy(b => b.name).Select(b => new SelectListItem { Value = b.id.ToString(), Text = b.name }).ToList();
            if (product != null && source == null)
            {
                vm.Id = product.id;
                vm.Name = product.name;
                vm.Description = product.description;
                vm.OriginalPrice = product.original_price;
                vm.Price = product.price;
                vm.Stock = product.stock;
                vm.Sold = product.sold;
                vm.CategoryId = product.category_id;
                vm.BrandId = product.brandId;
                vm.IsActive = product.is_active;
                vm.ImageUrl = product.image_url;
            }
            return vm;
        }

        private string SaveUpload(HttpPostedFileBase file, out string error)
        {
            error = null;
            if (file == null || file.ContentLength == 0) return null; // optional image
            var ext = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            if (string.IsNullOrEmpty(ext) || !AllowedExt.Contains(ext))
            {
                error = "Unsupported image format."; return null;
            }
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
            if (string.IsNullOrEmpty(s)) s = "product"; return s.ToLowerInvariant();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
