using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using ValiModern.Filters;
using ValiModern.Models.EF;
using ValiModern.Models.ViewModels;
using ValiModern.Services;

namespace ValiModern.Controllers
{
    [AuthorizeShipper]
    [ShipperLayoutData] // Inject layout data automatically
    public class ShipperController : Controller
    {
        private readonly ValiModernDBEntities _db = new ValiModernDBEntities();
        private readonly ShipperService _shipperService;

        public ShipperController()
        {
            _shipperService = new ShipperService(_db);
        }

        #region Helper Methods

        /// <summary>
        /// Get current shipper user ID from authentication
        /// </summary>
        private int? GetCurrentShipperId()
        {
            if (int.TryParse(User.Identity.Name, out int userId))
            {
                return userId;
            }
            return null;
        }

        /// <summary>
        /// Validate current shipper and return user object
        /// </summary>
        private User GetCurrentShipper()
        {
            var userId = GetCurrentShipperId();
            if (!userId.HasValue)
            {
                return null;
            }

            var shipper = _shipperService.GetShipperByUserId(userId.Value);
            if (shipper == null || shipper.role != "shipper")
            {
                return null;
            }

            return shipper;
        }

        #endregion

        // GET: Shipper/Index
        public ActionResult Index(string filter = "", string search = "")
        {
            var shipper = GetCurrentShipper();
            if (shipper == null)
            {
                TempData["Error"] = "Invalid shipper session.";
                return RedirectToAction("Index", "Home");
            }

            // Get orders using service
            List<Order> orders;
            if (!string.IsNullOrWhiteSpace(search))
            {
                orders = _shipperService.SearchOrders(shipper.id, search, filter);
                ViewBag.SearchTerm = search;
            }
            else
            {
                orders = _shipperService.GetShipperOrders(shipper.id, filter);
            }

            // Get statistics
            var stats = _shipperService.GetShipperStats(shipper.id);

            // Calculate counts
            int totalCount = orders.Count;
            int assignedCount = orders.Count(o => o.delivered_at == null);

            var vm = new ShipperDeliveryListVM
            {
                Orders = orders.Select(o => _shipperService.MapToOrderItemVM(o)).ToList(),
                TotalCount = totalCount,
                AssignedCount = assignedCount,
                DeliveredCount = 0,
                FilterStatus = filter,
                Stats = stats
            };

            ViewBag.Filter = filter;

            return View(vm);
        }

        // GET: Shipper/Details/5
        public ActionResult Details(int id)
        {
            var shipper = GetCurrentShipper();
            if (shipper == null)
            {
                TempData["Error"] = "Invalid shipper session.";
                return RedirectToAction("Index");
            }

            var order = _shipperService.GetOrderDetails(id, shipper.id);

            if (order == null)
            {
                TempData["Error"] = "Order not found or you do not have permission to access it.";
                return RedirectToAction("Index");
            }

            var vm = _shipperService.MapToOrderDetailsVM(order);

            return View(vm);
        }

        // POST: Shipper/MarkDelivered
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MarkDelivered(int id, string deliveryNote)
        {
            var shipper = GetCurrentShipper();
            if (shipper == null)
            {
                TempData["Error"] = "Invalid shipper session.";
                return RedirectToAction("Index");
            }

            var result = _shipperService.MarkAsDelivered(id, shipper.id, deliveryNote);

            if (result.IsSuccess)
            {
                TempData["Success"] = result.Message;
            }
            else
            {
                TempData["Error"] = result.Message;
            }

            return RedirectToAction("Details", new { id });
        }

        // GET: Shipper/History
        public ActionResult History(int page = 1, int pageSize = 20)
        {
            var shipper = GetCurrentShipper();
            if (shipper == null)
            {
                TempData["Error"] = "Invalid shipper session.";
                return RedirectToAction("Index", "Home");
            }

            int totalCount;
            var orders = _shipperService.GetDeliveryHistory(shipper.id, page, pageSize, out totalCount);

            int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var vm = new ShipperHistoryVM
            {
                Orders = orders.Select(o => _shipperService.MapToOrderItemVM(o)).ToList(),
                CurrentPage = page,
                TotalPages = totalPages,
                TotalCount = totalCount,
                PageSize = pageSize
            };

            return View(vm);
        }

        #region Chat Methods

        // GET: Shipper/GetMessages - Load chat messages for an order
        [HttpGet]
        public JsonResult GetMessages(int orderId)
        {
            try
            {
                var shipper = GetCurrentShipper();
                if (shipper == null)
                {
                    return Json(new { success = false, message = "Unauthorized" }, JsonRequestBehavior.AllowGet);
                }

                // OPTIMIZE: Lightweight check using Any()
                var orderExists = _db.Orders.AsNoTracking().Any(o => o.id == orderId && o.shipper_id == shipper.id);
                if (!orderExists)
                {
                    return Json(new { success = false, message = "Order not found" }, JsonRequestBehavior.AllowGet);
                }

                // OPTIMIZE: Get messages with AsNoTracking
                var dbMessages = _db.Messages
                    .AsNoTracking()
                    .Where(m => m.order_id == orderId)
                    .OrderBy(m => m.created_at)
                    .ToList();

                // Format in memory
                var messages = dbMessages.Select(m => new
                {
                    isSent = m.sender_id == shipper.id,
                    message = m.message1,
                    time = m.created_at.HasValue ? m.created_at.Value.ToString("HH:mm") : ""
                }).ToList();
                
                return Json(messages, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[Chat] GetMessages Error: " + ex.Message);
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // POST: Shipper/SendMessage - Send a chat message
        [HttpPost]
        public JsonResult SendMessage(int orderId, string message)
        {
            try
            {
                var shipper = GetCurrentShipper();
                if (shipper == null)
                {
                    return Json(new { success = false, message = "Unauthorized" });
                }

                // OPTIMIZE: Only select needed fields
                var order = _db.Orders
                    .AsNoTracking()
                    .Where(o => o.id == orderId && o.shipper_id == shipper.id)
                    .Select(o => new { o.id, o.user_id })
                    .FirstOrDefault();
                    
                if (order == null)
                {
                    return Json(new { success = false, message = "Order not found" });
                }

                // Validate message
                if (string.IsNullOrWhiteSpace(message) || message.Length > 500)
                {
                    return Json(new { success = false, message = "Invalid message" });
                }

                // Get customer ID from order
                int customerId = order.user_id;

                // Save message to database
                var newMessage = new Message
                {
                    order_id = orderId,
                    sender_id = shipper.id,
                    receiver_id = customerId,
                    message1 = message.Trim(),
                    created_at = DateTime.Now,
                    is_read = false
                };

                _db.Messages.Add(newMessage);
                _db.SaveChanges();

                System.Diagnostics.Debug.WriteLine("[Chat] Message saved: Order " + orderId + ", Shipper " + shipper.id);
                
                return Json(new { success = true, message = "Message sent successfully" });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[Chat] SendMessage Error: " + ex.Message);
                return Json(new { success = false, message = "Failed to send message" });
            }
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _shipperService?.Dispose();
                _db?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
