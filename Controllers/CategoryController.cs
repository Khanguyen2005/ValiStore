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
        public ActionResult Products(int id, string sort, decimal? minPrice, decimal? maxPrice, string colors, string sizes, int page = 1)
        {
            var category = _db.Categories.Find(id);
            if (category == null)
            {
                TempData["Error"] = "Category not found.";
                return RedirectToAction("Index");
            }

            int pageSize = 12;
            var products = _db.Products
            .Include(p => p.Brand)
            .Include(p => p.Colors)
            .Include(p => p.Sizes)
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

            // Parse color and size filters
            List<int> selectedColorIds = new List<int>();
            List<int> selectedSizeIds = new List<int>();

            if (!string.IsNullOrWhiteSpace(colors))
            {
                selectedColorIds = colors.Split(',')
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => int.Parse(s.Trim()))
                .ToList();
            }

            if (!string.IsNullOrWhiteSpace(sizes))
            {
                selectedSizeIds = sizes.Split(',')
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => int.Parse(s.Trim()))
                .ToList();
            }

            // Filter by colors
            if (selectedColorIds.Any())
            {
                products = products.Where(p => p.Colors.Any(c => selectedColorIds.Contains(c.id)));
            }

            // Filter by sizes
            if (selectedSizeIds.Any())
            {
                products = products.Where(p => p.Sizes.Any(s => selectedSizeIds.Contains(s.id)));
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

            var productList = products
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

            // Get all available colors for this category
            var availableColors = _db.Colors
               .Where(c => c.Product.category_id == id && c.Product.is_active)
               .GroupBy(c => new { c.name, c.color_code })
                .Select(g => new FilterOptionVM
                {
                    Id = g.FirstOrDefault().id,
                    Name = g.Key.name,
                    HexCode = g.Key.color_code,
                    Count = g.Count()
                })
               .OrderBy(c => c.Name)
               .ToList();

            // Get all available sizes for this category
            var availableSizes = _db.Sizes
                .Where(s => s.Product.category_id == id && s.Product.is_active)
                .GroupBy(s => s.name)
                .Select(g => new FilterOptionVM
                {
                    Id = g.FirstOrDefault().id,
                    Name = g.Key,
                    Count = g.Count()
                })
                .OrderBy(s => s.Name)
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
                SelectedColorIds = selectedColorIds,
                SelectedSizeIds = selectedSizeIds
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
