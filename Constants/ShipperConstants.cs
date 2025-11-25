namespace ValiModern.Constants
{
    /// <summary>
    /// Constants for Shipper module
    /// </summary>
    public static class ShipperConstants
    {
        #region Order Status

        public const string OrderStatusShipped = "Shipped";
        public const string OrderStatusCompleted = "Completed";
        public const string OrderStatusCancelled = "Cancelled";
        public const string OrderStatusPending = "Pending";

        #endregion

        #region User Roles

        public const string RoleShipper = "shipper";
        public const string RoleAdmin = "admin";
        public const string RoleCustomer = "customer";

        #endregion

        #region Filter Options

        public const string FilterAll = "";
        public const string FilterAssigned = "assigned";
        public const string FilterPending = "pending";
        public const string FilterDelivered = "delivered";

        #endregion

        #region Validation

        public const int MaxDeliveryNoteLength = 500;
        public const int MaxPendingDisplayCount = 99;
        public const int DefaultPageSize = 20;
        public const int MaxPageSize = 100;

        #endregion

        #region Messages

        public const string MsgInvalidSession = "Invalid shipper session.";
        public const string MsgUnauthorized = "You do not have permission to access this.";
        public const string MsgOrderNotFound = "Order not found.";
        public const string MsgAlreadyDelivered = "Order has already been marked as delivered.";
        public const string MsgDeliverySuccess = "Successfully marked as delivered! Order is waiting for customer confirmation.";
        public const string MsgInvalidStatus = "Order status is invalid. Only 'Shipped' orders can be marked as delivered.";

        #endregion

        #region Cache Keys

        public const string CacheKeyPendingCount = "Shipper_PendingCount_{0}"; // {0} = shipperId
        public const string CacheKeyStats = "Shipper_Stats_{0}"; // {0} = shipperId
        public const int CacheDurationMinutes = 5;

        #endregion
    }
}
