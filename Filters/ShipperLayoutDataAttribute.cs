using System.Web.Mvc;
using ValiModern.Services;

namespace ValiModern.Filters
{
    /// <summary>
    /// Action filter to inject shipper statistics into ViewBag
    /// Used in _ShipperLayout to avoid duplicate DB queries
    /// </summary>
    public class ShipperLayoutDataAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            // Only inject for successful GET requests that return ViewResult
            if (filterContext.Result is ViewResult && 
                filterContext.HttpContext.Request.HttpMethod == "GET")
            {
                var user = filterContext.HttpContext.User;
                if (user != null && user.Identity != null && user.Identity.IsAuthenticated)
                {
                    int userId;
                    if (int.TryParse(user.Identity.Name, out userId))
                    {
                        using (var service = new ShipperService())
                        {
                            var shipper = service.GetShipperByUserId(userId);
                            if (shipper != null && shipper.role == "shipper")
                            {
                                // Inject data into ViewBag
                                filterContext.Controller.ViewBag.ShipperName = 
                                    string.IsNullOrWhiteSpace(shipper.username)
                                        ? (string.IsNullOrWhiteSpace(shipper.email) 
                                            ? "Shipper #" + shipper.id 
                                            : shipper.email)
                                        : shipper.username;

                                filterContext.Controller.ViewBag.ShipperId = shipper.id;

                                // Get pending deliveries count (optimized)
                                var pendingCount = service.GetPendingDeliveriesCount(shipper.id);
                                filterContext.Controller.ViewBag.PendingDeliveries = pendingCount;
                                filterContext.Controller.ViewBag.PendingDisplay = pendingCount > 99 ? "99+" : pendingCount.ToString();
                            }
                        }
                    }
                }
            }

            base.OnActionExecuted(filterContext);
        }
    }
}
