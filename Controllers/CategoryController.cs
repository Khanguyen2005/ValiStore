using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using ValiModern.Models.EF;
using ValiModern.Models.ViewModels;

namespace ValiModern.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ValiModernDBEntities _db = new ValiModernDBEntities();

        // GET: Category
        [OutputCache(Duration = 300, VaryByParam = "none")] // Cache 5 minutes
        public ActionResult Index()
        {
            var categories = _db.Categories
                .OrderBy(c => c.name)
                .Select(c => new CategoryVM
                {
                    Id = c.id,
                    Name = c.name,
                    Slug = c.name.ToLower().Replace(" ", "-")
                })
                .ToList();

            return View(categories);
        }

        // GET: Category/Products/5
        public ActionResult Products(int id, string sort, int? minPrice, int? maxPrice, string colors, string sizes, int page = 1)
        {
            var category = _db.Categories.Find(id);
            if (category == null)
            {
                TempData["Error"] = "Category not found.";
                return RedirectToAction("Index");
            }

            int pageSize = 12;
            
            // Start with base query - NO eager loading yet
            var products = _db.Products
                .Where(p => p.is_active && p.category_id == id)
                .AsQueryable();

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
                .Include(p => p.Brand)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Get distinct product IDs for filter options - more efficient
            var baseProductIds = products.Select(p => p.id).ToList();

            // Get all available colors for this category (count distinct products, not color records)
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

            // Get all available sizes for this category (count distinct products, not size records)
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

            var vm = new CategoryProductsVM
            {
                Category = new CategoryVM
                {
                    Id = category.id,
                    Name = category.name,
                    Slug = category.name.ToLower().Replace(" ", "-")
                },
                Products = productList.Select(p => new ProductCardVM
                {
                    id = p.id,
                    name = p.name,
                    price = p.price,
                    original_price = p.original_price,
                    image_url = p.image_url ?? "/Content/images/placeholder.png",
                    description = p.description,
                    sold = p.sold,
                    brand_name = p.Brand?.name,
                    category_name = category.name
                }).ToList(),
                AvailableColors = availableColors,
                AvailableSizes = availableSizes,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalProducts = totalProducts,
                Sort = sort,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                SelectedColorNames = selectedColorNames,
                SelectedSizeNames = selectedSizeNames
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
