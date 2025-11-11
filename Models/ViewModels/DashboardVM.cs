using System;
using System.Collections.Generic;

namespace ValiModern.Models.ViewModels
{
    public class DashboardVM
    {
        public int TotalOrders { get; set; }
  public int PendingOrders { get; set; }
      public int ProcessingOrders { get; set; }
        public int ShippedOrders { get; set; }
        public int DeliveredOrders { get; set; }
        public int CancelledOrders { get; set; }

  public decimal TotalRevenue { get; set; }
        public decimal TodayRevenue { get; set; }
        public decimal MonthRevenue { get; set; }

        public int TotalProducts { get; set; }
   public int ActiveProducts { get; set; }
        public int LowStockProducts { get; set; }

        public int TotalUsers { get; set; }
        public int NewUsersThisMonth { get; set; }

        public List<RecentOrderVM> RecentOrders { get; set; } = new List<RecentOrderVM>();
        public List<TopProductVM> TopProducts { get; set; } = new List<TopProductVM>();
    }

    public class RecentOrderVM
    {
        public int Id { get; set; }
        public string UserName { get; set; }
     public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public DateTime OrderDate { get; set; }
    }

    public class TopProductVM
    {
 public int Id { get; set; }
     public string Name { get; set; }
        public string ImageUrl { get; set; }
        public int Sold { get; set; }
        public decimal Revenue { get; set; }
    }
}
