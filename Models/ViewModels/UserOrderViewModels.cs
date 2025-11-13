using System;
using System.Collections.Generic;

namespace ValiModern.Models.ViewModels
{
    // User Order List View Model
    public class UserOrdersVM
    {
        public List<UserOrderItemVM> Orders { get; set; } = new List<UserOrderItemVM>();
        public string CurrentStatus { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalOrders { get; set; }
    }

    // User Order Item (for list view)
    public class UserOrderItemVM
    {
        public int OrderId { get; set; }
        public string OrderCode { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
        public long TotalAmount { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentStatus { get; set; }
        public int ItemCount { get; set; }
    }

    // User Order Details View Model
    public class UserOrderDetailsVM
    {
        public int OrderId { get; set; }
        public string OrderCode { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
        public long TotalAmount { get; set; }
        public string Phone { get; set; }
        public string ShippingAddress { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public List<UserOrderItemDetailVM> Items { get; set; } = new List<UserOrderItemDetailVM>();
        public List<UserOrderPaymentVM> Payments { get; set; } = new List<UserOrderPaymentVM>();

        // Helper property
        public bool CanCancel => Status == "Pending";
    }

    // User Order Item Detail
    public class UserOrderItemDetailVM
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductImageUrl { get; set; }
        public int Quantity { get; set; }
        public long Price { get; set; }
        public long Subtotal { get; set; }
        public string ColorName { get; set; }
        public string ColorCode { get; set; }
        public string SizeName { get; set; }
    }

    // User Order Payment
    public class UserOrderPaymentVM
    {
        public int PaymentId { get; set; }
        public string PaymentMethod { get; set; }
        public long Amount { get; set; }
        public string Status { get; set; }
        public string TransactionId { get; set; }
        public DateTime PaymentDate { get; set; }
    }
}
