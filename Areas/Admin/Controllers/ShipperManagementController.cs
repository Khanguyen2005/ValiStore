using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ValiModern.Filters;
using ValiModern.Models.EF;

namespace ValiModern.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    public class ShipperManagementController : Controller
    {
        private readonly ValiModernDBEntities _db = new ValiModernDBEntities();

        // GET: Admin/ShipperManagement
        public ActionResult Index()
        {
            // Get all shippers with their statistics
            var shippers = _db.Users
                .Where(u => u.role == "shipper")
                .ToList();

            var shipperStats = shippers.Select(s => new ShipperStatsVM
            {
                ShipperId = s.id,
                ShipperName = s.username,
                ShipperEmail = s.email,
                ShipperPhone = s.phone,
                TotalAssigned = _db.Orders.Count(o => o.shipper_id == s.id),
                PendingDeliveries = _db.Orders.Count(o => o.shipper_id == s.id && o.status == "Shipped" && o.delivered_at == null),
                DeliveredAwaitingConfirm = _db.Orders.Count(o => o.shipper_id == s.id && o.delivered_at != null && o.status == "Shipped"),
                CompletedOrders = _db.Orders.Count(o => o.shipper_id == s.id && o.status == "Completed"),
                CreatedAt = s.created_at
            }).OrderByDescending(s => s.CompletedOrders).ToList();

            return View(shipperStats);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }

    // ViewModel for shipper statistics
    public class ShipperStatsVM
    {
        public int ShipperId { get; set; }
        public string ShipperName { get; set; }
        public string ShipperEmail { get; set; }
        public string ShipperPhone { get; set; }
        public int TotalAssigned { get; set; }
        public int PendingDeliveries { get; set; }
        public int DeliveredAwaitingConfirm { get; set; }
        public int CompletedOrders { get; set; }
        public DateTime CreatedAt { get; set; }
        
        public double CompletionRate => TotalAssigned > 0 
            ? Math.Round((CompletedOrders / (double)TotalAssigned) * 100, 1) 
            : 0;
    }
}
