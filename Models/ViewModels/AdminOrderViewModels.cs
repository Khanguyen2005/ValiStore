using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ValiModern.Models.EF;

namespace ValiModern.Models.ViewModels
{
    public class OrderListItemVM
    {
  public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public DateTime OrderDate { get; set; }
  public string Status { get; set; }
   public decimal TotalAmount { get; set; }
        public string Phone { get; set; }
    public string ShippingAddress { get; set; }
  public int ItemCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class OrderDetailsVM
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
      public string UserEmail { get; set; }
        public string UserPhone { get; set; }
        public DateTime OrderDate { get; set; }
      public string Status { get; set; }
  public decimal TotalAmount { get; set; }
 public string Phone { get; set; }
        public string ShippingAddress { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Order items
        public List<OrderDetailItemVM> Items { get; set; } = new List<OrderDetailItemVM>();

     // Payment info
     public List<PaymentVM> Payments { get; set; } = new List<PaymentVM>();
    }

    public class OrderDetailItemVM
    {
      public int Id { get; set; }
  public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductImageUrl { get; set; }
   public int Quantity { get; set; }
        public decimal Price { get; set; }
  public decimal Subtotal { get; set; }
        public string ColorName { get; set; }
        public string ColorCode { get; set; }
   public string SizeName { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class PaymentVM
    {
 public int Id { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
        public string Status { get; set; }
        public string TransactionId { get; set; }
        public DateTime PaymentDate { get; set; }
    }

    public class UpdateOrderStatusVM
    {
        public int OrderId { get; set; }
        [Required(ErrorMessage = "Status is required")]
        public string Status { get; set; }
    }
}
