using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using ValiModern.Filters;
using ValiModern.Models.EF;
using ValiModern.Models.ViewModels;

namespace ValiModern.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    public class DashboardController : Controller
    {
        private readonly ValiModernDBEntities _db = new ValiModernDBEntities();

        // GET: Admin/Dashboard
        public ActionResult Index()
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);

            var vm = new DashboardVM
            {
                // Orders statistics - Use AsNoTracking for read-only queries
                TotalOrders = _db.Orders.AsNoTracking().Count(),
                PendingOrders = _db.Orders.AsNoTracking().Count(o => o.status == "Pending"),
                ConfirmedOrders = _db.Orders.AsNoTracking().Count(o => o.status == "Confirmed"),
                ShippedOrders = _db.Orders.AsNoTracking().Count(o => o.status == "Shipped"),
                CompletedOrders = _db.Orders.AsNoTracking().Count(o => o.status == "Completed"),
                CancelledOrders = _db.Orders.AsNoTracking().Count(o => o.status == "Cancelled"),

                // Revenue statistics (count only Completed orders)
                TotalRevenue = (decimal)(_db.Orders
                    .AsNoTracking()
                    .Where(o => o.status == "Completed")
                    .Sum(o => (long?)o.total_amount) ?? 0),
                
                TodayRevenue = (decimal)(_db.Orders
                    .AsNoTracking()
                    .Where(o => o.status == "Completed" && o.order_date >= today && o.order_date < tomorrow)
                    .Sum(o => (long?)o.total_amount) ?? 0),
                
                MonthRevenue = (decimal)(_db.Orders
                    .AsNoTracking()
                    .Where(o => o.status == "Completed" && o.order_date >= firstDayOfMonth && o.order_date < tomorrow)
                    .Sum(o => (long?)o.total_amount) ?? 0),

                // Products statistics
                TotalProducts = _db.Products.AsNoTracking().Count(),
                ActiveProducts = _db.Products.AsNoTracking().Count(p => p.is_active),
                LowStockProducts = _db.Products.AsNoTracking().Count(p => p.stock < 10 && p.is_active),

                // Users statistics
                TotalUsers = _db.Users.AsNoTracking().Count(),
                NewUsersThisMonth = _db.Users.AsNoTracking().Count(u => u.created_at >= firstDayOfMonth),

                // Recent orders - Optimize with projection first
                RecentOrders = _db.Orders
                    .AsNoTracking()
                    .OrderByDescending(o => o.order_date)
                    .Take(5)
                    .Select(o => new RecentOrderVM
                    {
                        Id = o.id,
                        UserName = (o.User != null ? o.User.username : null) ?? "N/A",
                        TotalAmount = (decimal)o.total_amount,
                        Status = o.status,
                        OrderDate = o.order_date
                    }).ToList(),

                // Top products by QUANTITY SOLD (not revenue) - only from COMPLETED orders
                TopProducts = _db.Order_Details
                    .AsNoTracking()
                    .Where(od => od.Order.status == "Completed") // Only count completed orders
                    .GroupBy(od => new
                    {
                        ProductId = od.product_id,
                        ProductName = od.Product.name,
                        ProductImage = od.Product.image_url
                    })
                    .Select(g => new
                    {
                        ProductId = g.Key.ProductId,
                        ProductName = g.Key.ProductName,
                        ProductImage = g.Key.ProductImage,
                        TotalQuantitySold = g.Sum(od => od.quantity),
                        TotalRevenue = g.Sum(od => (long)od.quantity * od.price)
                    })
                    .OrderByDescending(x => x.TotalQuantitySold) // Sort by quantity sold
                    .Take(5)
                    .AsEnumerable()
                    .Select(x => new TopProductVM
                    {
                        Id = x.ProductId,
                        Name = x.ProductName ?? "Unknown Product",
                        ImageUrl = x.ProductImage,
                        Sold = x.TotalQuantitySold,
                        Revenue = (decimal)x.TotalRevenue
                    })
                    .ToList()
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