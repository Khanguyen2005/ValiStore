using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using ValiModern.Filters;
using ValiModern.Models.EF;
using ValiModern.Models.ViewModels;

namespace ValiModern.Controllers
{
    [AuthorizeShipper]
    public class ShipperController : Controller
    {
        private readonly ValiModernDBEntities _db = new ValiModernDBEntities();

        // GET: Shipper/Delivery
        public ActionResult Index(string filter = "assigned")
        {
            // User.Identity.Name now contains user ID
            int userId;
            if (!int.TryParse(User.Identity.Name, out userId))
            {
                TempData["Error"] = "Invalid user session.";
                return RedirectToAction("Index", "Home");
            }

            var shipper = _db.Users.Find(userId);
            if (shipper == null || shipper.role != "shipper")
            {
                TempData["Error"] = "You do not have permission to access this page.";
                return RedirectToAction("Index", "Home");
            }

            // L?y danh sách ??n hàng ???c gán cho shipper này
            var ordersQuery = _db.Orders
                .Include(o => o.User)
                .Include(o => o.Order_Details)
                .Where(o => o.shipper_id == shipper.id && o.status == "Shipped");

            // Calculate counts BEFORE filtering
            int totalCount = ordersQuery.Count();
            int assignedCount = ordersQuery.Count(o => o.delivered_at == null);
            int deliveredCount = ordersQuery.Count(o => o.delivered_at != null);

            // L?c theo tr?ng thái
            if (filter == "delivered")
            {
                ordersQuery = ordersQuery.Where(o => o.delivered_at != null);
            }
            else if (filter == "assigned")
            {
                ordersQuery = ordersQuery.Where(o => o.delivered_at == null);
            }
            // else: show all (no additional filter)

            var orders = ordersQuery
                .OrderByDescending(o => o.assigned_at)
                .ToList();

            var vm = new ShipperDeliveryListVM
            {
                Orders = orders.Select(o => new ShipperOrderItemVM
                {
                    OrderId = o.id,
                    OrderCode = "#" + o.id.ToString("D6"),
                    OrderDate = o.order_date,
                    AssignedAt = o.assigned_at ?? DateTime.Now,
                    DeliveredAt = o.delivered_at,
                    TotalAmount = o.total_amount,
                    Phone = o.phone,
                    ShippingAddress = o.shipping_address,
                    Status = o.status,
                    ItemCount = o.Order_Details.Sum(od => od.quantity),
                    CustomerName = o.User?.username ?? "N/A"
                }).ToList(),
                TotalCount = totalCount,
                AssignedCount = assignedCount,
                DeliveredCount = deliveredCount,
                FilterStatus = filter
            };

            ViewBag.Filter = filter;

            return View(vm);
        }

        // GET: Shipper/Details/5
        public ActionResult Details(int id)
        {
            // User.Identity.Name now contains user ID
            int userId;
            if (!int.TryParse(User.Identity.Name, out userId))
            {
                TempData["Error"] = "Invalid user session.";
                return RedirectToAction("Index");
            }

            var shipper = _db.Users.Find(userId);
            if (shipper == null || shipper.role != "shipper")
            {
                TempData["Error"] = "You do not have permission to access this.";
                return RedirectToAction("Index");
            }

            var order = _db.Orders
                .Include(o => o.User)
                .Include(o => o.Order_Details.Select(od => od.Product))
                .Include(o => o.Order_Details.Select(od => od.Color))
                .Include(o => o.Order_Details.Select(od => od.Size))
                .FirstOrDefault(o => o.id == id && o.shipper_id == shipper.id);

            if (order == null)
            {
                TempData["Error"] = "Order not found or you do not have permission to access it.";
                return RedirectToAction("Index");
            }

            var vm = new ShipperOrderDetailsVM
            {
                OrderId = order.id,
                OrderCode = "#" + order.id.ToString("D6"),
                OrderDate = order.order_date,
                AssignedAt = order.assigned_at ?? DateTime.Now,
                DeliveredAt = order.delivered_at,
                DeliveryNote = order.delivery_note,
                TotalAmount = order.total_amount,
                Phone = order.phone,
                ShippingAddress = order.shipping_address,
                Status = order.status,
                CustomerName = order.User?.username ?? "N/A",
                CustomerEmail = order.User?.email ?? "N/A",
                CustomerPhone = order.User?.phone ?? order.phone,
                Items = order.Order_Details.Select(od => new ShipperOrderItemDetailVM
                {
                    ProductId = od.product_id,
                    ProductName = od.Product?.name ?? "N/A",
                    ProductImageUrl = od.Product?.image_url,
                    Quantity = od.quantity,
                    Price = od.price,
                    ColorName = od.Color?.name,
                    ColorCode = od.Color?.color_code,
                    SizeName = od.Size?.name
                }).ToList()
            };

            return View(vm);
        }

        // POST: Shipper/MarkDelivered
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MarkDelivered(int id, string deliveryNote)
        {
            // User.Identity.Name now contains user ID
            int userId;
            if (!int.TryParse(User.Identity.Name, out userId))
            {
                TempData["Error"] = "Invalid user session.";
                return RedirectToAction("Index");
            }

            var shipper = _db.Users.Find(userId);
            if (shipper == null || shipper.role != "shipper")
            {
                TempData["Error"] = "You do not have permission to perform this action.";
                return RedirectToAction("Index");
            }

            var order = _db.Orders.FirstOrDefault(o => o.id == id && o.shipper_id == shipper.id);

            if (order == null)
            {
                TempData["Error"] = "Order not found.";
                return RedirectToAction("Index");
            }

            if (order.delivered_at != null)
            {
                TempData["Error"] = "Order has already been marked as delivered.";
                return RedirectToAction("Details", new { id });
            }

            // ?ánh d?u ?ã giao (GI? NGUYÊN STATUS = "Shipped")
            // Admin ho?c customer s? xác nh?n Completed sau
            order.delivered_at = DateTime.Now;
            order.delivery_note = deliveryNote;
            order.updated_at = DateTime.Now;

            _db.SaveChanges();

            TempData["Success"] = "Successfully marked as delivered! Order is waiting for customer confirmation.";
            return RedirectToAction("Details", new { id });
        }

        // GET: Shipper/History - View completed delivery history
        public ActionResult History(int page = 1, int pageSize = 20)
        {
            // User.Identity.Name now contains user ID
            int userId;
            if (!int.TryParse(User.Identity.Name, out userId))
            {
                TempData["Error"] = "Invalid user session.";
                return RedirectToAction("Index", "Home");
            }

            var shipper = _db.Users.Find(userId);
            if (shipper == null || shipper.role != "shipper")
            {
                TempData["Error"] = "You do not have permission to access this page.";
                return RedirectToAction("Index", "Home");
            }

            // Get all completed deliveries (delivered_at is not null AND status = "Completed")
            var completedQuery = _db.Orders
                .Include(o => o.User)
                .Include(o => o.Order_Details)
                .Where(o => o.shipper_id == shipper.id && 
                           o.delivered_at != null && 
                           o.status == "Completed")
                .OrderByDescending(o => o.delivered_at);

            int totalCount = completedQuery.Count();
            int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            
            var orders = completedQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var vm = new ShipperHistoryVM
            {
                Orders = orders.Select(o => new ShipperOrderItemVM
                {
                    OrderId = o.id,
                    OrderCode = "#" + o.id.ToString("D6"),
                    OrderDate = o.order_date,
                    AssignedAt = o.assigned_at ?? DateTime.Now,
                    DeliveredAt = o.delivered_at,
                    TotalAmount = o.total_amount,
                    Phone = o.phone,
                    ShippingAddress = o.shipping_address,
                    Status = o.status,
                    ItemCount = o.Order_Details.Sum(od => od.quantity),
                    CustomerName = o.User?.username ?? "N/A"
                }).ToList(),
                CurrentPage = page,
                TotalPages = totalPages,
                TotalCount = totalCount,
                PageSize = pageSize
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
