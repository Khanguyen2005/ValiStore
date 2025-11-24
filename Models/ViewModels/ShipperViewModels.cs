using System;
using System.Collections.Generic;

namespace ValiModern.Models.ViewModels
{
    // ViewModel cho danh sách ??n hàng c?a shipper
    public class ShipperDeliveryListVM
    {
        public List<ShipperOrderItemVM> Orders { get; set; } = new List<ShipperOrderItemVM>();
        
        // Counts for filter buttons
        public int TotalCount { get; set; }        // All orders
        public int AssignedCount { get; set; }     // Pending deliveries (not delivered yet)
        public int DeliveredCount { get; set; }    // Completed deliveries
        
        public string FilterStatus { get; set; }   // "assigned", "delivered", "" (all)
        
        // Backwards compatibility
        public int PendingCount => AssignedCount;
    }

    // ViewModel cho t?ng ??n hàng trong danh sách shipper
    public class ShipperOrderItemVM
    {
        public int OrderId { get; set; }
        public string OrderCode { get; set; }
        public DateTime OrderDate { get; set; }    // Added for display
        public DateTime AssignedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public long TotalAmount { get; set; }
        public string Phone { get; set; }
        public string ShippingAddress { get; set; }
        public string Status { get; set; }
        public int ItemCount { get; set; }
        public bool IsDelivered => DeliveredAt.HasValue;
        public string CustomerName { get; set; }
    }

    // ViewModel cho chi ti?t ??n hàng c?a shipper
    public class ShipperOrderDetailsVM
    {
        public int OrderId { get; set; }
        public string OrderCode { get; set; }
        public DateTime OrderDate { get; set; }    // Added for display
        public DateTime AssignedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public string DeliveryNote { get; set; }
        public long TotalAmount { get; set; }
        public string Phone { get; set; }
        public string ShippingAddress { get; set; }
        public string Status { get; set; }
        
        // Customer info
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerPhone { get; set; }
        
        // Order items
        public List<ShipperOrderItemDetailVM> Items { get; set; } = new List<ShipperOrderItemDetailVM>();
        
        public bool IsDelivered => DeliveredAt.HasValue;
    }

    public class ShipperOrderItemDetailVM
    {
        public int ProductId { get; set; }        // Added for reference
        public string ProductName { get; set; }
        public string ProductImageUrl { get; set; }
        public int Quantity { get; set; }
        public int Price { get; set; }
        public string ColorName { get; set; }
        public string ColorCode { get; set; }     // Added for color display
        public string SizeName { get; set; }
        
        public int Subtotal => Quantity * Price;
    }

    // ViewModel cho form ?ánh d?u ?ã giao
    public class MarkDeliveredVM
    {
        public int OrderId { get; set; }
        public string DeliveryNote { get; set; }
    }
}
