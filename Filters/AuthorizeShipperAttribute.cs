using System.Linq;
using System.Web;
using System.Web.Mvc;
using ValiModern.Models.EF;

namespace ValiModern.Filters
{
    /// <summary>
    /// Authorize attribute ?? ki?m tra user có role 'shipper' hay không
    /// </summary>
    public class AuthorizeShipperAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            // Ki?m tra user ?ã ??ng nh?p ch?a
            var isAuthenticated = base.AuthorizeCore(httpContext);
            if (!isAuthenticated)
            {
                return false;
            }

            // L?y email c?a user ?ang ??ng nh?p
            var userEmail = httpContext.User.Identity.Name;
            if (string.IsNullOrEmpty(userEmail))
            {
                return false;
            }

            // Ki?m tra role trong database
            using (var db = new ValiModernDBEntities())
            {
                var user = db.Users.FirstOrDefault(u => u.email == userEmail);
                if (user == null)
                {
                    return false;
                }

                // Ki?m tra có ph?i shipper không
                return user.role == "shipper";
            }
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                // ?ã ??ng nh?p nh?ng không ph?i shipper
                filterContext.Result = new ViewResult
                {
                    ViewName = "~/Views/Shared/Unauthorized.cshtml"
                };
            }
            else
            {
                // Ch?a ??ng nh?p
                filterContext.Result = new RedirectResult("~/Account/Login");
            }
        }
    }
}
