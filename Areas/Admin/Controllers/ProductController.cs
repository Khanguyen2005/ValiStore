using System;
using System.Data.Entity;
using System.Globalization;
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

        private bool TryParseDecimal(string key, out decimal value)
        {
            value = 0m;
            var str = Request[key];
            if (string.IsNullOrWhiteSpace(str)) return false;
            // Try current culture, then invariant, then swap separators
            if (decimal.TryParse(str, NumberStyles.Any, CultureInfo.CurrentCulture, out value)) return true;
            if (decimal.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out value)) return true;
            var swapped = str.Replace(".", "_").Replace(",", ".").Replace("_", ",");
            return decimal.TryParse(swapped, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
        }

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
            // Normalize decimal inputs to avoid culture issues and cast to int
            if (TryParseDecimal("Price", out var parsedPrice)) { vm.Price = (int)parsedPrice; }
            if (TryParseDecimal("OriginalPrice", out var parsedOriginal)) { vm.OriginalPrice = (int)parsedOriginal; }

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

            // main image record
            if (!string.IsNullOrEmpty(relImage))
            {
                _db.Product_Images.Add(new Product_Images { product_id = product.id, image_url = relImage, is_main = true, sort_order = 1 });
            }

            // colors
            if (vm.SelectedColorIds != null && vm.SelectedColorIds.Length > 0)
            {
                var paletteColors = PaletteService.GetColors().Where(c => vm.SelectedColorIds.Contains(c.Id)).ToList();
                foreach (var c in paletteColors)
                {
                    _db.Colors.Add(new Color { product_id = product.id, name = c.Name, color_code = c.ColorCode });
                }
            }
            // sizes
            if (vm.SelectedSizeIds != null && vm.SelectedSizeIds.Length > 0)
            {
                var paletteSizes = PaletteService.GetSizes().Where(s => vm.SelectedSizeIds.Contains(s.Id)).ToList();
                foreach (var s in paletteSizes)
                {
                    _db.Sizes.Add(new Size { product_id = product.id, name = s.Name });
                }
            }
            _db.SaveChanges();

            TempData["Success"] = "Product created.";
            return RedirectToAction("Index");
        }

        // GET: Admin/Product/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var product = _db.Products.Include(p => p.Colors).Include(p => p.Sizes).FirstOrDefault(p => p.id == id);
            if (product == null) return HttpNotFound();
            var vm = BuildFormVM(product);
            return View(vm);
        }

        // POST: Admin/Product/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, ProductFormVM vm, HttpPostedFileBase imageFile)
        {
            // Normalize decimal inputs again and cast to int
            if (TryParseDecimal("Price", out var parsedPrice)) { vm.Price = (int)parsedPrice; }
            if (TryParseDecimal("OriginalPrice", out var parsedOriginal)) { vm.OriginalPrice = (int)parsedOriginal; }

            var product = _db.Products.Include(p => p.Colors).Include(p => p.Sizes).FirstOrDefault(p => p.id == id);
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
                // shift existing images
                var images = _db.Product_Images.Where(x => x.product_id == product.id).ToList();
                foreach (var img in images) { img.sort_order += 1; img.is_main = false; }
                _db.Product_Images.Add(new Product_Images { product_id = product.id, image_url = relImage, is_main = true, sort_order = 1 });
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

            // Update colors: only remove those NOT referenced in Order_Details
            var usedColorIds = _db.Order_Details.Where(od => od.color_id.HasValue && od.product_id == product.id).Select(od => od.color_id.Value).Distinct().ToList();
            var colorsToRemove = product.Colors.Where(c => !usedColorIds.Contains(c.id)).ToList();
            foreach (var c in colorsToRemove) _db.Colors.Remove(c);

            // Add new colors from palette (avoid duplicates by name)
            if (vm.SelectedColorIds != null && vm.SelectedColorIds.Length > 0)
            {
                var paletteColors = PaletteService.GetColors().Where(c => vm.SelectedColorIds.Contains(c.Id)).ToList();
                var existingColorNames = product.Colors.Select(c => c.name.ToLower()).ToList();
                foreach (var c in paletteColors)
                {
                    if (!existingColorNames.Contains(c.Name.ToLower()))
                    {
                        _db.Colors.Add(new Color { product_id = product.id, name = c.Name, color_code = c.ColorCode });
                    }
                }
            }

            // Update sizes: only remove those NOT referenced in Order_Details
            var usedSizeIds = _db.Order_Details.Where(od => od.size_id.HasValue && od.product_id == product.id).Select(od => od.size_id.Value).Distinct().ToList();
            var sizesToRemove = product.Sizes.Where(s => !usedSizeIds.Contains(s.id)).ToList();
            foreach (var s in sizesToRemove) _db.Sizes.Remove(s);

            // Add new sizes from palette (avoid duplicates by name)
            if (vm.SelectedSizeIds != null && vm.SelectedSizeIds.Length > 0)
            {
                var paletteSizes = PaletteService.GetSizes().Where(s => vm.SelectedSizeIds.Contains(s.Id)).ToList();
                var existingSizeNames = product.Sizes.Select(s => s.name.ToLower()).ToList();
                foreach (var s in paletteSizes)
                {
                    if (!existingSizeNames.Contains(s.Name.ToLower()))
                    {
                        _db.Sizes.Add(new Size { product_id = product.id, name = s.Name });
                    }
                }
            }

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
            var imgs = _db.Product_Images.Where(pi => pi.product_id == id).ToList();
            foreach (var i in imgs)
            {
                DeletePhysical(i.image_url);
                _db.Product_Images.Remove(i);
            }
            DeletePhysical(product.image_url);
            // remove colors & sizes
            var colors = _db.Colors.Where(c => c.product_id == id).ToList();
            foreach (var c in colors) _db.Colors.Remove(c);
            var sizes = _db.Sizes.Where(s => s.product_id == id).ToList();
            foreach (var s in sizes) _db.Sizes.Remove(s);
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

            if (product != null)
            {
                // Always populate from product if available (whether source is null or not)
                vm.Id = product.id;
                vm.Name = source?.Name ?? product.name;
                vm.Description = source?.Description ?? product.description;
                vm.OriginalPrice = source?.OriginalPrice ?? (int)product.original_price;
                vm.Price = source?.Price ?? (int)product.price;
                vm.Stock = source?.Stock ?? product.stock;
                vm.Sold = source?.Sold ?? product.sold;
                vm.CategoryId = source?.CategoryId ?? product.category_id;
                vm.BrandId = source?.BrandId ?? product.brandId;
                vm.IsActive = source?.IsActive ?? product.is_active;
                vm.ImageUrl = product.image_url;

                // map existing product colors/sizes back to palette by name match
                var paletteColors = vm.AllColors;
                vm.SelectedColorIds = source?.SelectedColorIds ?? product.Colors.Select(pc => paletteColors.FirstOrDefault(ac => ac.Name.Equals(pc.name, StringComparison.OrdinalIgnoreCase))?.Id ?? -1).Where(i => i > 0).ToArray();
                var paletteSizes = vm.AllSizes;
                vm.SelectedSizeIds = source?.SelectedSizeIds ?? product.Sizes.Select(ps => paletteSizes.FirstOrDefault(asz => asz.Name.Equals(ps.name, StringComparison.OrdinalIgnoreCase))?.Id ?? -1).Where(i => i > 0).ToArray();
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
