using System;
using System.Data.Entity;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using ValiModern.Models.EF;

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
            
            // OPTIMIZE: Pre-warm Entity Framework to reduce cold start
            PreWarmEntityFramework();
            
            // OPTIMIZE: Configure Entity Framework settings
            ConfigureEntityFramework();
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

        /// <summary>
        /// OPTIMIZE: Pre-warm Entity Framework to avoid cold start delay
        /// Runs in background thread to not block application startup
        /// </summary>
        private void PreWarmEntityFramework()
        {
            System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    using (var db = new ValiModernDBEntities())
                    {
                        // Force EF to compile queries on startup
                        // Execute simple queries to warm up the most common entities
                        
                        db.Database.ExecuteSqlCommand("SELECT TOP 1 * FROM Categories");
                        db.Database.ExecuteSqlCommand("SELECT TOP 1 * FROM Brands");
                        db.Database.ExecuteSqlCommand("SELECT TOP 1 * FROM Products");
                        db.Database.ExecuteSqlCommand("SELECT TOP 1 * FROM Users");
                        db.Database.ExecuteSqlCommand("SELECT TOP 1 * FROM Orders");
                        
                        System.Diagnostics.Debug.WriteLine("[WARMUP] ✅ Entity Framework warmed up successfully");
                    }
                }
                catch (System.Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[WARMUP] ⚠️ Error: {ex.Message}");
                }
            });
        }

        /// <summary>
        /// OPTIMIZE: Configure Entity Framework settings for better performance
        /// </summary>
        private void ConfigureEntityFramework()
        {
            // Disable database initializer (we don't want EF to create/modify schema)
            Database.SetInitializer<ValiModernDBEntities>(null);
            
            // Note: Lazy loading is controlled per DbContext instance
            // We use AsNoTracking() explicitly in queries for read-only operations
        }
    }
}
