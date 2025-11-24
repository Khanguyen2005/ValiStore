using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using ValiModern.Helpers;
using ValiModern.Models.EF;
using ValiModern.Models.ViewModels;

namespace ValiModern.Controllers
{
    public class ProductController : Controller
    {
        private readonly ValiModernDBEntities _db = new ValiModernDBEntities();

        // GET: Product
        public ActionResult Index(string category, string brand, string sort, string q, int? minPrice, int? maxPrice, string colors, string sizes, int page = 1)
        {
            int pageSize = 12;
            
            // Start with base query - NO eager loading yet to avoid loading unnecessary data
            var products = _db.Products
                .Where(p => p.is_active)
                .AsQueryable();

            // Filter by search query
            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim().ToLower();
                products = products.Where(p => p.name.ToLower().Contains(q) || p.description.ToLower().Contains(q));
            }

            // Filter by category
            if (!string.IsNullOrWhiteSpace(category))
            {
                products = products.Where(p => p.Category != null && p.Category.name == category);
            }

            // Filter by brand
            if (!string.IsNullOrWhiteSpace(brand))
            {
                products = products.Where(p => p.Brand != null && p.Brand.name == brand);
            }

            // Filter by price range
            if (minPrice.HasValue)
            {
                products = products.Where(p => p.price >= minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                products = products.Where(p => p.price <= maxPrice.Value);
            }

            // Parse color and size filters - USE NAMES not IDs
            List<string> selectedColorNames = new List<string>();
            List<string> selectedSizeNames = new List<string>();

            if (!string.IsNullOrWhiteSpace(colors))
            {
                selectedColorNames = colors.Split(',')
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(s => s.Trim())
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(sizes))
            {
                selectedSizeNames = sizes.Split(',')
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(s => s.Trim())
                    .ToList();
            }

            // Filter by colors - using color NAMES
            if (selectedColorNames.Any())
            {
                products = products.Where(p => p.Colors.Any(c => selectedColorNames.Contains(c.name)));
            }

            // Filter by sizes - using size NAMES
            if (selectedSizeNames.Any())
            {
                products = products.Where(p => p.Sizes.Any(s => selectedSizeNames.Contains(s.name)));
            }

            // Sort
            switch (sort)
            {
                case "price_asc":
                    products = products.OrderBy(p => p.price);
                    break;
                case "price_desc":
                    products = products.OrderByDescending(p => p.price);
                    break;
                case "name_asc":
                    products = products.OrderBy(p => p.name);
                    break;
                case "name_desc":
                    products = products.OrderByDescending(p => p.name);
                    break;
                case "newest":
                    products = products.OrderByDescending(p => p.id);
                    break;
                default:
                    products = products.OrderByDescending(p => p.sold); // Best selling
                    break;
            }

            var totalProducts = products.Count();
            var totalPages = (int)Math.Ceiling(totalProducts / (double)pageSize);

            // NOW apply eager loading only for the page we need
            var productList = products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Get distinct product IDs for filter options - more efficient
            var baseProductIds = products.Select(p => p.id).ToList();

            // Get available colors based on filtered products (count distinct products, not color records)
            var availableColors = _db.Colors
                .Where(c => baseProductIds.Contains(c.product_id))
                .GroupBy(c => new { c.name, c.color_code })
                .Select(g => new
                {
                    Name = g.Key.name,
                    HexCode = g.Key.color_code,
                    ProductCount = g.Select(c => c.product_id).Distinct().Count()
                })
                .OrderBy(c => c.Name)
                .ToList()
                .Select(c => new FilterOptionVM
                {
                    Name = c.Name,
                    HexCode = c.HexCode,
                    Count = c.ProductCount
                })
                .ToList();

            // Get available sizes based on filtered products (count distinct products, not size records)
            var availableSizes = _db.Sizes
                .Where(s => baseProductIds.Contains(s.product_id))
                .GroupBy(s => s.name)
                .Select(g => new
                {
                    Name = g.Key,
                    ProductCount = g.Select(s => s.product_id).Distinct().Count()
                })
                .OrderBy(s => s.Name)
                .ToList()
                .Select(s => new FilterOptionVM
                {
                    Name = s.Name,
                    Count = s.ProductCount
                })
                .ToList();

            // Use CacheHelper for categories and brands - they don't change often
            var categoriesList = CacheHelper.GetOrSet(
                CacheHelper.KEY_CATEGORIES,
                () => _db.Categories.OrderBy(c => c.name).ToList(),
                10
            );
            
            var brandsList = CacheHelper.GetOrSet(
                CacheHelper.KEY_BRANDS,
                () => _db.Brands.OrderBy(b => b.name).ToList(),
                10
            );

            var vm = new ProductIndexVM
            {
                Products = productList.Select(p => new ProductCardVM
                {
                    id = p.id,
                    name = p.name,
                    price = p.price,
                    original_price = p.original_price,
                    image_url = p.image_url ?? "/Content/images/placeholder.png",
                    sold = p.sold,
                    brand_name = p.Brand?.name,
                    category_name = p.Category?.name
                }).ToList(),
                Categories = categoriesList,
                Brands = brandsList,
                AvailableColors = availableColors,
                AvailableSizes = availableSizes,
                CurrentCategory = category,
                CurrentBrand = brand,
                CurrentSort = sort,
                SearchQuery = q,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                SelectedColorNames = selectedColorNames,
                SelectedSizeNames = selectedSizeNames,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalProducts = totalProducts
            };

            return View(vm);
        }

        // GET: Product/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index");
            }

            var product = _db.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Colors)
                .Include(p => p.Sizes)
                .Include(p => p.Product_Images)
                .FirstOrDefault(p => p.id == id && p.is_active);

            if (product == null)
            {
                TempData["Error"] = "Product not found.";
                return RedirectToAction("Index");
            }

            // Get related products (same category) - only select needed fields
            var relatedProducts = _db.Products
                .Where(p => p.is_active && p.category_id == product.category_id && p.id != product.id)
                .OrderByDescending(p => p.sold)
                .Take(4)
                .Select(p => new ProductCardVM
                {
                    id = p.id,
                    name = p.name,
                    price = p.price,
                    original_price = p.original_price,
                    image_url = p.image_url,
                    sold = p.sold
                })
                .ToList();

            var vm = new ProductDetailVM
            {
                Product = product,
                RelatedProducts = relatedProducts
            };

            return View(vm);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
