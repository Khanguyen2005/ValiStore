using System;
using System.Collections.Generic;
using ValiModern.Models.EF;
using ValiModern.Models.ViewModels;

namespace ValiModern.Services
{
    public interface IShipperService : IDisposable
    {
        User GetShipperByUserId(int userId);
        bool IsValidShipper(int userId);
        int GetPendingDeliveriesCount(int shipperId);
        ShipperStatsVM GetShipperStats(int shipperId);
        List<Order> GetShipperOrders(int shipperId, string filter = "");
        Order GetOrderDetails(int orderId, int shipperId);
        List<Order> GetDeliveryHistory(int shipperId, int page, int pageSize, out int totalCount);
        List<Order> SearchOrders(int shipperId, string searchTerm, string filter = "");
        OperationResult MarkAsDelivered(int orderId, int shipperId, string deliveryNote);
        ShipperOrderItemVM MapToOrderItemVM(Order order);
        ShipperOrderDetailsVM MapToOrderDetailsVM(Order order);
    }
}
