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
            var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);

            var vm = new DashboardVM
            {
                // Orders statistics
                TotalOrders = _db.Orders.Count(),
                PendingOrders = _db.Orders.Count(o => o.status == "Pending"),
                ProcessingOrders = _db.Orders.Count(o => o.status == "Processing"),
                ShippedOrders = _db.Orders.Count(o => o.status == "Shipped"),
                DeliveredOrders = _db.Orders.Count(o => o.status == "Delivered"),
                CancelledOrders = _db.Orders.Count(o => o.status == "Cancelled"),

                // Revenue statistics
                TotalRevenue = _db.Orders.Where(o => o.status == "Delivered").Sum(o => (decimal?)o.total_amount) ?? 0,
                TodayRevenue = _db.Orders.Where(o => o.status == "Delivered" && DbFunctions.TruncateTime(o.order_date) == today).Sum(o => (decimal?)o.total_amount) ?? 0,
                MonthRevenue = _db.Orders.Where(o => o.status == "Delivered" && o.order_date >= firstDayOfMonth).Sum(o => (decimal?)o.total_amount) ?? 0,

                // Products statistics
                TotalProducts = _db.Products.Count(),
                ActiveProducts = _db.Products.Count(p => p.is_active),
                LowStockProducts = _db.Products.Count(p => p.stock < 10 && p.is_active),

                // Users statistics
                TotalUsers = _db.Users.Count(),
                NewUsersThisMonth = _db.Users.Count(u => u.created_at >= firstDayOfMonth),

                // Recent orders
                RecentOrders = _db.Orders
                    .Include(o => o.User)
                    .OrderByDescending(o => o.order_date)
                    .Take(5)
                    .ToList()
                    .Select(o => new RecentOrderVM
                    {
                        Id = o.id,
                        UserName = o.User?.username ?? "N/A",
                        TotalAmount = o.total_amount,
                        Status = o.status,
                        OrderDate = o.order_date
                    }).ToList(),

                // Top products
                TopProducts = _db.Products
                    .OrderByDescending(p => p.sold)
                    .Take(5)
                    .ToList()
                    .Select(p => new TopProductVM
                    {
                        Id = p.id,
                        Name = p.name,
                        ImageUrl = p.image_url,
                        Sold = p.sold,
                        Revenue = p.sold * p.price
                    }).ToList()
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