using System;
using System.Collections.Generic;
using System.Data.Entity;
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
            // OPTIMIZE: Use AsNoTracking and aggregate all stats in one query
            var shippers = _db.Users
                .AsNoTracking()
                .Where(u => u.role == "shipper")
                .ToList();

            // OPTIMIZE: Aggregate all order stats in one query per shipper
            var shipperStats = shippers.Select(s =>
            {
                var stats = _db.Orders
                    .AsNoTracking()
                    .Where(o => o.shipper_id == s.id)
                    .GroupBy(o => 1)
                    .Select(g => new
                    {
                        TotalAssigned = g.Count(),
                        PendingDeliveries = g.Count(o => o.status == "Shipped" && o.delivered_at == null),
                        DeliveredAwaitingConfirm = g.Count(o => o.delivered_at != null && o.status == "Shipped"),
                        CompletedOrders = g.Count(o => o.status == "Completed")
                    })
                    .FirstOrDefault();

                return new ShipperStatsVM
                {
                    ShipperId = s.id,
                    ShipperName = s.username,
                    ShipperEmail = s.email,
                    ShipperPhone = s.phone,
                    TotalAssigned = stats?.TotalAssigned ?? 0,
                    PendingDeliveries = stats?.PendingDeliveries ?? 0,
                    DeliveredAwaitingConfirm = stats?.DeliveredAwaitingConfirm ?? 0,
                    CompletedOrders = stats?.CompletedOrders ?? 0,
                    CreatedAt = s.created_at
                };
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
