using System;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;

namespace ValiModern
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            BundleTable.EnableOptimizations = true;

            // FIX: Use NameIdentifier (user ID) instead of Email for anti-forgery
            // This allows users without email (like shippers) to use forms
            AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.NameIdentifier;
        }

        protected void Application_PostAuthenticateRequest(object sender, EventArgs e)
        {
            var authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie == null) return;
            
            FormsAuthenticationTicket ticket;
            try
            {
                ticket = FormsAuthentication.Decrypt(authCookie.Value);
            }
            catch { return; }
            
            if (ticket == null || ticket.Expired) return;
            
            // Create claims identity
            var identity = new ClaimsIdentity("Forms", ClaimTypes.Name, ClaimTypes.Role);
            
            // ticket.Name now contains user ID (from login fix)
            // Add NameIdentifier claim (REQUIRED for anti-forgery)
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, ticket.Name));
            
            // Add Name claim for User.Identity.Name
            identity.AddClaim(new Claim(ClaimTypes.Name, ticket.Name));
            
            // Add role claim from UserData
            if (!string.IsNullOrEmpty(ticket.UserData))
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, ticket.UserData));
            }
            
            HttpContext.Current.User = new ClaimsPrincipal(identity);
        }
    }
}
