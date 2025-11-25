using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using ValiModern.Models.EF;
using ValiModern.Models.ViewModels;

namespace ValiModern.Services
{
    /// <summary>
    /// Service layer for Shipper operations
    /// Separates business logic from controller
    /// </summary>
    public class ShipperService : IDisposable
    {
        private readonly ValiModernDBEntities _db;
        private bool _disposed = false;

        public ShipperService()
        {
            _db = new ValiModernDBEntities();
        }

        public ShipperService(ValiModernDBEntities db)
        {
            _db = db;
        }

        #region Shipper Info

        /// <summary>
        /// Get shipper info by user ID
        /// </summary>
        public User GetShipperByUserId(int userId)
        {
            return _db.Users.Find(userId);
        }

        /// <summary>
        /// Validate if user is a shipper
        /// </summary>
        public bool IsValidShipper(int userId)
        {
            var user = GetShipperByUserId(userId);
            return user != null && user.role == "shipper";
        }

        #endregion

        #region Pending Deliveries Count

        /// <summary>
        /// Get count of pending deliveries for a shipper
        /// Optimized for frequent calls (e.g., in layout)
        /// </summary>
        public int GetPendingDeliveriesCount(int shipperId)
        {
            return _db.Orders
                .Where(o => o.shipper_id == shipperId && 
                           o.status == "Shipped" && 
                           o.delivered_at == null)
                .Count();
        }

        /// <summary>
        /// Get statistics for shipper dashboard
        /// </summary>
        public ShipperStatsVM GetShipperStats(int shipperId)
        {
            var now = DateTime.Now;
            var today = now.Date;
            var thisMonth = new DateTime(now.Year, now.Month, 1);

            // Query database directly to use DbFunctions
            var totalPending = _db.Orders
                .Count(o => o.shipper_id == shipperId && 
                           o.status == "Shipped" && 
                           o.delivered_at == null);

            var totalDeliveredToday = _db.Orders
                .Count(o => o.shipper_id == shipperId &&
                           o.delivered_at.HasValue && 
                           DbFunctions.TruncateTime(o.delivered_at) == today);

            var totalDeliveredThisMonth = _db.Orders
                .Count(o => o.shipper_id == shipperId &&
                           o.delivered_at.HasValue && 
                           o.delivered_at >= thisMonth);

            var totalCompletedAllTime = _db.Orders
                .Count(o => o.shipper_id == shipperId && 
                           o.status == "Completed");

            var totalEarningsThisMonth = _db.Orders
                .Where(o => o.shipper_id == shipperId &&
                           o.status == "Completed" && 
                           o.delivered_at.HasValue &&
                           o.delivered_at >= thisMonth)
                .Sum(o => (long?)o.total_amount) ?? 0;

            return new ShipperStatsVM
            {
                TotalPending = totalPending,
                TotalDeliveredToday = totalDeliveredToday,
                TotalDeliveredThisMonth = totalDeliveredThisMonth,
                TotalCompletedAllTime = totalCompletedAllTime,
                TotalEarningsThisMonth = totalEarningsThisMonth
            };
        }

        #endregion

        #region Order Queries

        /// <summary>
        /// Get delivery orders for shipper with filtering
        /// </summary>
        public List<Order> GetShipperOrders(int shipperId, string filter = "")
        {
            var query = _db.Orders
                .Include(o => o.User)
                .Include(o => o.Order_Details)
                .Where(o => o.shipper_id == shipperId && o.status == "Shipped");

            // Apply filter
            if (filter == "assigned" || filter == "pending")
            {
                query = query.Where(o => o.delivered_at == null);
            }
            else if (filter == "delivered")
            {
                query = query.Where(o => o.delivered_at != null);
            }

            return query
                .OrderByDescending(o => o.assigned_at)
                .ToList();
        }

        /// <summary>
        /// Get order details for shipper with all related data
        /// </summary>
        public Order GetOrderDetails(int orderId, int shipperId)
        {
            return _db.Orders
                .Include(o => o.User)
                .Include(o => o.Order_Details.Select(od => od.Product))
                .Include(o => o.Order_Details.Select(od => od.Color))
                .Include(o => o.Order_Details.Select(od => od.Size))
                .FirstOrDefault(o => o.id == orderId && o.shipper_id == shipperId);
        }

        /// <summary>
        /// Get completed delivery history with pagination
        /// </summary>
        public List<Order> GetDeliveryHistory(int shipperId, int page, int pageSize, out int totalCount)
        {
            var query = _db.Orders
                .Include(o => o.User)
                .Include(o => o.Order_Details)
                .Where(o => o.shipper_id == shipperId && 
                           o.delivered_at != null && 
                           o.status == "Completed")
                .OrderByDescending(o => o.delivered_at);

            totalCount = query.Count();

            return query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }

        /// <summary>
        /// Search orders by customer name, phone, or address
        /// </summary>
        public List<Order> SearchOrders(int shipperId, string searchTerm, string filter = "")
        {
            var query = _db.Orders
                .Include(o => o.User)
                .Include(o => o.Order_Details)
                .Where(o => o.shipper_id == shipperId && o.status == "Shipped");

            // Apply filter
            if (filter == "assigned")
            {
                query = query.Where(o => o.delivered_at == null);
            }
            else if (filter == "delivered")
            {
                query = query.Where(o => o.delivered_at != null);
            }

            // Search
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower().Trim();
                query = query.Where(o => 
                    o.phone.Contains(searchTerm) ||
                    o.shipping_address.ToLower().Contains(searchTerm) ||
                    o.User.username.ToLower().Contains(searchTerm) ||
                    o.User.email.ToLower().Contains(searchTerm)
                );
            }

            return query
                .OrderByDescending(o => o.assigned_at)
                .ToList();
        }

        #endregion

        #region Mark Delivery

        /// <summary>
        /// Mark order as delivered with validation
        /// </summary>
        public OperationResult MarkAsDelivered(int orderId, int shipperId, string deliveryNote)
        {
            var order = _db.Orders.FirstOrDefault(o => o.id == orderId);

            // Validation
            if (order == null)
            {
                System.Diagnostics.Debug.WriteLine($"[ShipperService] Attempt to mark non-existent order #{orderId} by shipper #{shipperId}");
                return OperationResult.Failure("Order not found.");
            }

            if (order.shipper_id != shipperId)
            {
                System.Diagnostics.Debug.WriteLine($"[ShipperService] Unauthorized attempt to mark order #{orderId} by shipper #{shipperId}");
                return OperationResult.Failure("You do not have permission to mark this order.");
            }

            if (order.status != "Shipped")
            {
                System.Diagnostics.Debug.WriteLine($"[ShipperService] Attempt to mark order #{orderId} with invalid status: {order.status}");
                return OperationResult.Failure($"Order status is '{order.status}'. Only 'Shipped' orders can be marked as delivered.");
            }

            if (order.delivered_at != null)
            {
                System.Diagnostics.Debug.WriteLine($"[ShipperService] Attempt to mark already delivered order #{orderId}");
                return OperationResult.Failure("Order has already been marked as delivered.");
            }

            // Validate delivery note length
            if (!string.IsNullOrEmpty(deliveryNote) && deliveryNote.Length > 500)
            {
                return OperationResult.Failure("Delivery note is too long. Maximum 500 characters.");
            }

            // Mark as delivered
            order.delivered_at = DateTime.Now;
            order.delivery_note = deliveryNote?.Trim();
            order.updated_at = DateTime.Now;

            try
            {
                _db.SaveChanges();
                
                // Log successful delivery
                System.Diagnostics.Debug.WriteLine($"[ShipperService] Order #{orderId} marked as delivered by shipper #{shipperId}");
                
                return OperationResult.Success("Successfully marked as delivered! Order is waiting for customer confirmation.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ShipperService] Failed to mark order #{orderId} as delivered: {ex.Message}");
                return OperationResult.Failure("Error saving delivery status: " + ex.Message);
            }
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Convert Order entity to ShipperOrderItemVM
        /// </summary>
        public ShipperOrderItemVM MapToOrderItemVM(Order order)
        {
            return new ShipperOrderItemVM
            {
                OrderId = order.id,
                OrderCode = "#" + order.id,
                OrderDate = order.order_date,
                AssignedAt = order.assigned_at ?? DateTime.Now,
                DeliveredAt = order.delivered_at,
                TotalAmount = order.total_amount,
                Phone = order.phone,
                ShippingAddress = order.shipping_address,
                Status = order.status,
                ItemCount = order.Order_Details?.Sum(od => od.quantity) ?? 0,
                TotalQuantity = order.Order_Details?.Sum(od => od.quantity) ?? 0,
                ProductCount = order.Order_Details?.Select(od => od.product_id).Distinct().Count() ?? 0,
                CustomerName = order.User?.username ?? "N/A"
            };
        }

        /// <summary>
        /// Convert Order entity to ShipperOrderDetailsVM
        /// </summary>
        public ShipperOrderDetailsVM MapToOrderDetailsVM(Order order)
        {
            return new ShipperOrderDetailsVM
            {
                OrderId = order.id,
                OrderCode = "#" + order.id,
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
                Items = order.Order_Details?.Select(od => new ShipperOrderItemDetailVM
                {
                    ProductId = od.product_id,
                    ProductName = od.Product?.name ?? "N/A",
                    ProductImageUrl = od.Product?.image_url,
                    Quantity = od.quantity,
                    Price = od.price,
                    ColorName = od.Color?.name,
                    ColorCode = od.Color?.color_code,
                    SizeName = od.Size?.name
                }).ToList() ?? new List<ShipperOrderItemDetailVM>()
            };
        }

        #endregion

        #region Dispose

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _db?.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }

    #region Helper Classes

    /// <summary>
    /// Result object for operations
    /// </summary>
    public class OperationResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }

        public static OperationResult Success(string message = "Operation successful")
        {
            return new OperationResult { IsSuccess = true, Message = message };
        }

        public static OperationResult Failure(string message = "Operation failed")
        {
            return new OperationResult { IsSuccess = false, Message = message };
        }
    }

    #endregion
}
