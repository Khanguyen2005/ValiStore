using System;
using System.Collections.Generic;
using System.Linq;

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

        // Statistics
        public ShipperStatsVM Stats { get; set; }
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
        public int ItemCount { get; set; } // total quantity (legacy)
        
        // New explicit counts for clarity
        public int TotalQuantity { get; set; } // T?ng s? l??ng (units)
        public int ProductCount { get; set; } // S? s?n ph?m khác nhau (lines)
        public bool IsDelivered => DeliveredAt.HasValue;
        public string CustomerName { get; set; }
        
        // NEW: Delivery duration
        public TimeSpan DeliveryDuration => DeliveredAt.HasValue 
            ? (DeliveredAt.Value - AssignedAt) 
            : (DateTime.Now - AssignedAt);
        
        // NEW: Coordinates for mapping
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
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
        
        // New derived properties for consistency
        public int ProductCount => Items?.Count ?? 0;
        public int TotalQuantity => Items?.Sum(i => i.Quantity) ?? 0;
        
        // NEW: Delivery photos
        public List<string> DeliveryPhotoUrls { get; set; } = new List<string>();
        
        // NEW: Coordinates
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        
        // NEW: Delivery duration
        public TimeSpan DeliveryDuration => DeliveredAt.HasValue 
            ? (DeliveredAt.Value - AssignedAt) 
            : (DateTime.Now - AssignedAt);
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
        public List<string> PhotoUrls { get; set; } = new List<string>();
    }

    // ViewModel cho l?ch s? giao hàng c?a shipper (pagination)
    public class ShipperHistoryVM
    {
        public List<ShipperOrderItemVM> Orders { get; set; } = new List<ShipperOrderItemVM>();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
        public int PageSize { get; set; }
        
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
    }

    // ViewModel cho th?ng kê shipper
    public class ShipperStatsVM
    {
        public int TotalPending { get; set; }
        public int TotalDeliveredToday { get; set; }
        public int TotalDeliveredThisMonth { get; set; }
        public int TotalCompletedAllTime { get; set; }
        public long TotalEarningsThisMonth { get; set; }
        
        // Calculated properties
        public decimal AverageDeliveryValue => TotalCompletedAllTime > 0 
            ? (decimal)TotalEarningsThisMonth / TotalDeliveredThisMonth 
            : 0;
            
        // NEW: Average delivery time
        public double AverageDeliveryTimeHours { get; set; }
    }

    // NEW: ViewModel for nearby orders (route optimization)
    public class NearbyOrdersVM
    {
        public ShipperOrderItemVM CurrentOrder { get; set; }
        public List<ShipperOrderItemVM> NearbyOrders { get; set; } = new List<ShipperOrderItemVM>();
        public double RadiusKm { get; set; } = 5.0; // Default 5km radius
    }
}
