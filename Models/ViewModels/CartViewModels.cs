using System;
using System.Collections.Generic;
using System.Linq;

namespace ValiModern.Models.ViewModels
{
    // Cart item stored in session
    [Serializable]
    public class CartItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductImageUrl { get; set; }
        public int Price { get; set; }
        public int Quantity { get; set; }
        public int? ColorId { get; set; }
        public string ColorName { get; set; }
        public string ColorCode { get; set; }
        public int? SizeId { get; set; }
        public string SizeName { get; set; }

        public long Subtotal => (long)Price * Quantity;
    }

    // Cart view model
    public class CartVM
    {
        public List<CartItem> Items { get; set; } = new List<CartItem>();
        public long TotalAmount => Items.Sum(i => i.Subtotal);
        public int TotalItems => Items.Sum(i => i.Quantity);
    }

    // Checkout view model
    public class CheckoutVM
    {
        public List<CartItem> Items { get; set; } = new List<CartItem>();
        public long TotalAmount => Items.Sum(i => i.Subtotal);

        // Shipping information
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string Note { get; set; }

        // Payment method: "COD" or "VNPay"
        public string PaymentMethod { get; set; }
    }

    // Order confirmation VM
    public class OrderConfirmationVM
    {
        public int OrderId { get; set; }
        public string OrderCode { get; set; }
        public long TotalAmount { get; set; }
        public string PaymentMethod { get; set; }
        public string Status { get; set; }
        public DateTime OrderDate { get; set; }
    }
}
