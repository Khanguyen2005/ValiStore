using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using ValiModern.Filters;
using ValiModern.Models.EF;
using ValiModern.Models.ViewModels;

namespace ValiModern.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    public class OrderController : Controller
    {
        private readonly ValiModernDBEntities _db = new ValiModernDBEntities();

        // GET: Admin/Order
        public ActionResult Index(string status, string q, string sort)
        {
            var orders = _db.Orders
                .Include(o => o.User)
                .Include(o => o.Order_Details)
                .AsQueryable();

            // Filter by status
            if (!string.IsNullOrWhiteSpace(status))
            {
                orders = orders.Where(o => o.status == status);
            }

            // Search by user name, email, phone, or address
            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim().ToLower();
                orders = orders.Where(o =>
                    o.User.username.ToLower().Contains(q) ||
                    o.User.email.ToLower().Contains(q) ||
                    o.phone.ToLower().Contains(q) ||
                    o.shipping_address.ToLower().Contains(q));
            }

            // Sort
            switch (sort)
            {
                case "id_asc":
                    orders = orders.OrderBy(o => o.id);
                    break;
                case "date_asc":
                    orders = orders.OrderBy(o => o.order_date);
                    break;
                case "date_desc":
                    orders = orders.OrderByDescending(o => o.order_date);
                    break;
                case "total_asc":
                    orders = orders.OrderBy(o => o.total_amount);
                    break;
                case "total_desc":
                    orders = orders.OrderByDescending(o => o.total_amount);
                    break;
                case "id_desc":
                default:
                    orders = orders.OrderByDescending(o => o.id);
                    break;
            }

            ViewBag.Status = status;
            ViewBag.Query = q;
            ViewBag.Sort = string.IsNullOrEmpty(sort) ? "id_desc" : sort;

            // Map to ViewModel
            var list = orders.Take(300).ToList().Select(o => new OrderListItemVM
            {
                Id = o.id,
                UserId = o.user_id,
                UserName = o.User?.username ?? "N/A",
                UserEmail = o.User?.email ?? "N/A",
                OrderDate = o.order_date,
                Status = o.status,
                TotalAmount = o.total_amount,
                Phone = o.phone,
                ShippingAddress = o.shipping_address,
                ItemCount = o.Order_Details.Count,
                CreatedAt = o.created_at,
                UpdatedAt = o.updated_at
            }).ToList();

            return View(list);
        }

        // GET: Admin/Order/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var order = _db.Orders
                .Include(o => o.User)
                .Include(o => o.User1) // User1 = Shipper (EF generated navigation property)
                .Include(o => o.Order_Details.Select(od => od.Product))
                .Include(o => o.Order_Details.Select(od => od.Color))
                .Include(o => o.Order_Details.Select(od => od.Size))
                .Include(o => o.Payments)
                .FirstOrDefault(o => o.id == id);

            if (order == null) return HttpNotFound();

            var vm = new OrderDetailsVM
            {
                Id = order.id,
                UserId = order.user_id,
                UserName = order.User?.username ?? "N/A",
                UserEmail = order.User?.email ?? "N/A",
                UserPhone = order.User?.phone ?? "N/A",
                OrderDate = order.order_date,
                Status = order.status,
                TotalAmount = order.total_amount,
                Phone = order.phone,
                ShippingAddress = order.shipping_address,
                CreatedAt = order.created_at,
                UpdatedAt = order.updated_at,
                Items = order.Order_Details.Select(od => new OrderDetailItemVM
                {
                    Id = od.id,
                    ProductId = od.product_id,
                    ProductName = od.Product?.name ?? "N/A",
                    ProductImageUrl = od.Product?.image_url,
                    Quantity = od.quantity,
                    Price = od.price,
                    Subtotal = od.quantity * od.price,
                    ColorName = od.Color?.name,
                    ColorCode = od.Color?.color_code,
                    SizeName = od.Size?.name,
                    CreatedAt = od.created_at
                }).ToList(),
                Payments = order.Payments.Select(p => new PaymentVM
                {
                    Id = p.id,
                    Amount = p.amount,
                    PaymentMethod = p.payment_method,
                    Status = p.status,
                    TransactionId = p.transaction_id,
                    PaymentDate = p.payment_date
                }).ToList()
            };

            // L?y danh sách shipper ?? hi?n th? dropdown
            ViewBag.Shippers = _db.Users
                .Where(u => u.role == "shipper")
                .OrderBy(u => u.username)
                .Select(u => new SelectListItem
                {
                    Value = u.id.ToString(),
                    Text = u.username + " - " + u.phone
                })
                .ToList();

            // Thông tin shipper hi?n t?i (n?u có) - User1 = Shipper
            ViewBag.CurrentShipperId = order.shipper_id;
            ViewBag.CurrentShipperName = order.User1?.username; // User1 instead of Shipper
            ViewBag.AssignedAt = order.assigned_at;
            ViewBag.DeliveredAt = order.delivered_at;
            ViewBag.DeliveryNote = order.delivery_note;

            return View(vm);
        }

        // POST: Admin/Order/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateStatus(int id, string status)
        {
            var order = _db.Orders
                .Include(o => o.Order_Details)
                .FirstOrDefault(o => o.id == id);

            if (order == null) return HttpNotFound();

            if (string.IsNullOrWhiteSpace(status))
            {
                TempData["Error"] = "Status is required.";
                return RedirectToAction("Details", new { id });
            }

            var validStatuses = new[] { "Pending", "Confirmed", "Shipped", "Completed", "Cancelled" };
            if (!validStatuses.Contains(status))
            {
                TempData["Error"] = "Invalid status.";
                return RedirectToAction("Details", new { id });
            }

            var oldStatus = order.status;

            // If changing to Cancelled, restore stock
            if (status == "Cancelled" && oldStatus != "Cancelled")
            {
                foreach (var detail in order.Order_Details)
                {
                    var product = _db.Products.Find(detail.product_id);
                    if (product != null)
                    {
                        product.stock += detail.quantity;  // Restore stock
                        product.sold -= detail.quantity;   // Decrease sold count

                        // Prevent negative sold count
                        if (product.sold < 0) product.sold = 0;
                    }
                }
            }

            order.status = status;
            order.updated_at = DateTime.Now;
            _db.SaveChanges();

            TempData["Success"] = $"Order status updated to '{status}'.";
            return RedirectToAction("Details", new { id });
        }

        // POST: Admin/Order/AssignShipper
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AssignShipper(int id, int shipperId)
        {
            var order = _db.Orders.Find(id);
            if (order == null) return HttpNotFound();

            // Kiem tra order phai o trang thai Confirmed moi duoc assign shipper
            if (order.status != "Confirmed")
            {
                TempData["Error"] = "Chi co the gan shipper cho don hang da Confirmed.";
                return RedirectToAction("Details", new { id });
            }

            // Kiem tra shipper co ton tai khong
            var shipper = _db.Users.Find(shipperId);
            if (shipper == null || shipper.role != "shipper")
            {
                TempData["Error"] = "Shipper khong ton tai hoac khong hop le.";
                return RedirectToAction("Details", new { id });
            }

            // Gan shipper va cap nhat status
            order.shipper_id = shipperId;
            order.assigned_at = DateTime.Now;
            order.status = "Shipped"; // Tu dong chuyen sang Shipped khi assign
            order.updated_at = DateTime.Now;

            _db.SaveChanges();

            TempData["Success"] = string.Format("Da gan don hang cho shipper {0}.", shipper.username);
            return RedirectToAction("Details", new { id });
        }

        // POST: Admin/Order/UnassignShipper
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UnassignShipper(int id)
        {
            var order = _db.Orders.Find(id);
            if (order == null) return HttpNotFound();

            if (order.shipper_id == null)
            {
                TempData["Error"] = "Don hang chua duoc gan shipper.";
                return RedirectToAction("Details", new { id });
            }

            // Huy gan shipper
            order.shipper_id = null;
            order.assigned_at = null;
            order.delivered_at = null;
            order.delivery_note = null;
            order.status = "Confirmed"; // Quay lai Confirmed
            order.updated_at = DateTime.Now;

            _db.SaveChanges();

            TempData["Success"] = "Da huy gan shipper.";
            return RedirectToAction("Details", new { id });
        }

        // POST: Admin/Order/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var order = _db.Orders
                .Include(o => o.Order_Details)
                .Include(o => o.Payments)
                .FirstOrDefault(o => o.id == id);

            if (order == null) return HttpNotFound();

            // Restore stock if order wasn't cancelled
            if (order.status != "Cancelled")
            {
                foreach (var detail in order.Order_Details)
                {
                    var product = _db.Products.Find(detail.product_id);
                    if (product != null)
                    {
                        product.stock += detail.quantity;  // Restore stock
                        product.sold -= detail.quantity;   // Decrease sold count

                        // Prevent negative sold count
                        if (product.sold < 0) product.sold = 0;
                    }
                }
            }

            // Remove payments
            foreach (var payment in order.Payments.ToList())
            {
                _db.Payments.Remove(payment);
            }

            // Remove order details
            foreach (var detail in order.Order_Details.ToList())
            {
                _db.Order_Details.Remove(detail);
            }

            // Remove order
            _db.Orders.Remove(order);
            _db.SaveChanges();

            TempData["Success"] = "Order deleted and stock restored.";
            return RedirectToAction("Index");
        }

        // GET: Admin/Order/ShipperDeliveries/5 - View all deliveries by a specific shipper
        public ActionResult ShipperDeliveries(int? id, int page = 1, int pageSize = 20)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var shipper = _db.Users.Find(id);
            if (shipper == null || shipper.role != "shipper")
            {
                TempData["Error"] = "Shipper not found.";
                return RedirectToAction("Index");
            }

            // Get all orders assigned to this shipper
            var ordersQuery = _db.Orders
                .Include(o => o.User)
                .Include(o => o.Order_Details)
                .Where(o => o.shipper_id == id)
                .OrderByDescending(o => o.assigned_at);

            int totalCount = ordersQuery.Count();
            int completedCount = ordersQuery.Count(o => o.status == "Completed");
            int pendingCount = ordersQuery.Count(o => o.status == "Shipped" && o.delivered_at == null);
            int deliveredCount = ordersQuery.Count(o => o.delivered_at != null && o.status == "Shipped");

            int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            
            var orders = ordersQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var vm = new ShipperDeliveriesVM
            {
                ShipperId = shipper.id,
                ShipperName = shipper.username,
                ShipperEmail = shipper.email,
                ShipperPhone = shipper.phone,
                Orders = orders.Select(o => new OrderListItemVM
                {
                    Id = o.id,
                    UserId = o.user_id,
                    UserName = o.User?.username ?? "N/A",
                    UserEmail = o.User?.email ?? "N/A",
                    OrderDate = o.order_date,
                    Status = o.status,
                    TotalAmount = o.total_amount,
                    Phone = o.phone,
                    ShippingAddress = o.shipping_address,
                    ItemCount = o.Order_Details.Count,
                    CreatedAt = o.created_at,
                    UpdatedAt = o.updated_at,
                    AssignedAt = o.assigned_at,
                    DeliveredAt = o.delivered_at
                }).ToList(),
                TotalCount = totalCount,
                CompletedCount = completedCount,
                PendingCount = pendingCount,
                DeliveredCount = deliveredCount,
                CurrentPage = page,
                TotalPages = totalPages,
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
