using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using ValiModern.Models.EF;
using ValiModern.Models.ViewModels;

namespace ValiModern.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly ValiModernDBEntities _db = new ValiModernDBEntities();

        // GET: Order
        public ActionResult Index(string status, int page = 1)
        {
            // User.Identity.Name now contains user ID
            int userId;
            if (!int.TryParse(User.Identity.Name, out userId))
            {
                TempData["Error"] = "Invalid user session.";
                return RedirectToAction("Index", "Home");
            }

            var ordersQuery = _db.Orders
                .Include(o => o.Payments)
                .Where(o => o.user_id == userId)
                .AsQueryable();

            // Filter by status
            if (!string.IsNullOrWhiteSpace(status) && status != "All")
            {
                ordersQuery = ordersQuery.Where(o => o.status == status);
            }

            // Order by date descending (newest first)
            ordersQuery = ordersQuery.OrderByDescending(o => o.order_date);

            // Pagination
            int pageSize = 10;
            var totalOrders = ordersQuery.Count();
            var totalPages = (int)Math.Ceiling(totalOrders / (double)pageSize);

            var orders = ordersQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var vm = new UserOrdersVM
            {
                Orders = orders.Select(o => new UserOrderItemVM
                {
                    OrderId = o.id,
                    OrderCode = "#" + o.id,
                    OrderDate = o.order_date,
                    Status = o.status,
                    TotalAmount = o.total_amount,
                    PaymentMethod = o.Payments.FirstOrDefault()?.payment_method ?? "N/A",
                    PaymentStatus = o.Payments.FirstOrDefault()?.status ?? "N/A",
                    ItemCount = o.Order_Details.Sum(od => od.quantity)
                }).ToList(),
                CurrentStatus = status ?? "All",
                CurrentPage = page,
                TotalPages = totalPages,
                TotalOrders = totalOrders
            };

            // Count orders by status for filter buttons
            ViewBag.AllCount = _db.Orders.Count(o => o.user_id == userId);
            ViewBag.PendingCount = _db.Orders.Count(o => o.user_id == userId && o.status == "Pending");
            ViewBag.ConfirmedCount = _db.Orders.Count(o => o.user_id == userId && o.status == "Confirmed");
            ViewBag.ShippedCount = _db.Orders.Count(o => o.user_id == userId && o.status == "Shipped");
            ViewBag.CompletedCount = _db.Orders.Count(o => o.user_id == userId && o.status == "Completed");
            ViewBag.CancelledCount = _db.Orders.Count(o => o.user_id == userId && o.status == "Cancelled");

            return View(vm);
        }

        // GET: Order/Details/5
        public ActionResult Details(int id)
        {
            // User.Identity.Name now contains user ID
            int userId;
            if (!int.TryParse(User.Identity.Name, out userId))
            {
                TempData["Error"] = "Invalid user session.";
                return RedirectToAction("Index", "Home");
            }

            var order = _db.Orders
                .Include(o => o.Order_Details.Select(od => od.Product))
                .Include(o => o.Order_Details.Select(od => od.Color))
                .Include(o => o.Order_Details.Select(od => od.Size))
                .Include(o => o.Payments)
                .Include(o => o.User1) // User1 = Shipper
                .FirstOrDefault(o => o.id == id && o.user_id == userId);

            if (order == null)
            {
                TempData["Error"] = "Order not found or you don't have permission to view it.";
                return RedirectToAction("Index");
            }

            var vm = new UserOrderDetailsVM
            {
                OrderId = order.id,
                OrderCode = "#" + order.id,
                OrderDate = order.order_date,
                Status = order.status,
                TotalAmount = order.total_amount,
                Phone = order.phone,
                ShippingAddress = order.shipping_address,
                CreatedAt = order.created_at,
                UpdatedAt = order.updated_at,
                DeliveredAt = order.delivered_at,
                DeliveryNote = order.delivery_note,
                ShipperName = order.User1?.username, // User1 = Shipper
                Items = order.Order_Details.Select(od => new UserOrderItemDetailVM
                {
                    ProductId = od.product_id,
                    ProductName = od.Product.name,
                    ProductImageUrl = od.Product.image_url,
                    Quantity = od.quantity,
                    Price = od.price,
                    Subtotal = od.quantity * od.price,
                    ColorName = od.Color?.name,
                    ColorCode = od.Color?.color_code,
                    SizeName = od.Size?.name
                }).ToList(),
                Payments = order.Payments.Select(p => new UserOrderPaymentVM
                {
                    PaymentId = p.id,
                    PaymentMethod = p.payment_method,
                    Amount = p.amount,
                    Status = p.status,
                    TransactionId = p.transaction_id,
                    PaymentDate = p.payment_date
                }).ToList()
            };

            return View(vm);
        }

        // POST: Order/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Cancel(int id)
        {
            // User.Identity.Name now contains user ID
            int userId;
            if (!int.TryParse(User.Identity.Name, out userId))
            {
                TempData["Error"] = "Invalid user session.";
                return RedirectToAction("Index", "Home");
            }

            var order = _db.Orders
                .Include(o => o.Order_Details.Select(od => od.Product))
                .FirstOrDefault(o => o.id == id && o.user_id == userId);

            if (order == null)
            {
                TempData["Error"] = "Order not found or you don't have permission to cancel it.";
                return RedirectToAction("Index");
            }

            // Only allow cancellation for Pending orders
            if (order.status != "Pending")
            {
                TempData["Error"] = "Only pending orders can be cancelled.";
                return RedirectToAction("Details", new { id = id });
            }

            // Update order status
            order.status = "Cancelled";
            order.updated_at = DateTime.Now;

            // Restore product stock
            foreach (var item in order.Order_Details)
            {
                var product = item.Product;
                if (product != null)
                {
                    product.stock += item.quantity;
                    product.sold -= item.quantity;
                }
            }

            // Update payment status if exists
            var payment = _db.Payments.FirstOrDefault(p => p.order_id == order.id);
            if (payment != null && payment.status == "Pending")
            {
                payment.status = "Failed";
            }

            _db.SaveChanges();

            TempData["Success"] = "Order cancelled successfully. Stock has been restored.";
            return RedirectToAction("Details", new { id = id });
        }

        // POST: Order/ConfirmReceived/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmReceived(int id)
        {
            // User.Identity.Name now contains user ID
            int userId;
            if (!int.TryParse(User.Identity.Name, out userId))
            {
                TempData["Error"] = "Invalid user session.";
                return RedirectToAction("Index", "Home");
            }

            var order = _db.Orders.FirstOrDefault(o => o.id == id && o.user_id == userId);

            if (order == null)
            {
                TempData["Error"] = "Order not found or you don't have permission to access it.";
                return RedirectToAction("Index");
            }

            // Only allow confirmation if order is Shipped and has been delivered by shipper
            if (order.status != "Shipped" || !order.delivered_at.HasValue)
            {
                TempData["Error"] = "Order cannot be confirmed. It must be in Shipped status and delivered by shipper.";
                return RedirectToAction("Details", new { id = id });
            }

            // Update order status to Completed
            order.status = "Completed";
            order.updated_at = DateTime.Now;

            _db.SaveChanges();

            TempData["Success"] = "Thank you! Order has been confirmed as received successfully.";
            return RedirectToAction("Details", new { id = id });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
