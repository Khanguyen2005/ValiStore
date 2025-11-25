using System.Linq;
using System.Web;
using System.Web.Mvc;
using ValiModern.Models.EF;

namespace ValiModern.Filters
{
    public class AuthorizeAdminAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (!httpContext.User?.Identity?.IsAuthenticated ?? true)
                return false;

            var identityName = httpContext.User.Identity.Name;
            
            // identityName now contains user ID
            int userId;
            if (!int.TryParse(identityName, out userId))
                return false;

            try
            {
                using (var db = new ValiModernDBEntities())
                {
                    var user = db.Users.Find(userId);
                    if (user == null) return false;
                    return user.is_admin;
                }
            }
            catch
            {
                return false;
            }
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            // Nếu chưa đăng nhập -> redirect login
            if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                base.HandleUnauthorizedRequest(filterContext); // default -> redirect to login
                return;
            }

            // Nếu đã đăng nhập nhưng không phải admin -> show Unauthorized view (HTTP 403)
            filterContext.Result = new ViewResult { ViewName = "~/Views/Account/Unauthorized.cshtml" };
            filterContext.HttpContext.Response.StatusCode = 403;
        }
    }
}