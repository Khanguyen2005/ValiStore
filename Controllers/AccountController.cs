using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using ValiModern.Models.EF;
using ValiModern.Models.ViewModels;

namespace ValiModern.Controllers
{
    public class AccountController : Controller
    {
        private readonly ValiModernDBEntities _db = new ValiModernDBEntities();

        // GET: /Account/Index (Profile)
        [Authorize]
        public ActionResult Index()
        {
            var email = User.Identity.Name;
            var user = _db.Users.FirstOrDefault(u => u.email == email);
            
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("Login");
            }

            var model = new ProfileViewModel
            {
                UserId = user.id,
                Username = user.username,
                Email = user.email,
                Phone = user.phone,
                Address = user.address,
                CreatedAt = user.created_at,
                UpdatedAt = user.updated_at,
                Role = user.role ?? (user.is_admin ? "admin" : "customer"),
                IsAdmin = user.is_admin
            };

            return View(model);
        }

        // POST: /Account/UpdateProfile
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateProfile(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            var email = User.Identity.Name;
            var user = _db.Users.FirstOrDefault(u => u.email == email);
            
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("Login");
            }

            // Update user info
            user.username = (model.Username ?? string.Empty).Trim();
            user.phone = (model.Phone ?? string.Empty).Trim();
            user.address = (model.Address ?? string.Empty).Trim();
            user.updated_at = DateTime.Now;

            try
            {
                _db.SaveChanges();
                
                // Update session
                Session["DisplayName"] = string.IsNullOrWhiteSpace(user.username) ? user.email : user.username;
                
                TempData["Success"] = "Profile updated successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error updating profile: " + ex.Message;
                return View("Index", model);
            }
        }

        // GET: /Account/ChangePassword
        [Authorize]
        public ActionResult ChangePassword()
        {
            return View(new ChangePasswordViewModel());
        }

        // POST: /Account/ChangePassword
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var email = User.Identity.Name;
            var user = _db.Users.FirstOrDefault(u => u.email == email);
            
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("Login");
            }

            // Verify current password
            if (user.password != model.CurrentPassword)
            {
                ModelState.AddModelError("CurrentPassword", "Current password is incorrect.");
                return View(model);
            }

            // Update password
            user.password = model.NewPassword;
            user.updated_at = DateTime.Now;

            try
            {
                _db.SaveChanges();
                TempData["Success"] = "Password changed successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error changing password: " + ex.Message;
                return View(model);
            }
        }

        // GET: /Account/Register
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var email = (model.Email ?? string.Empty).Trim().ToLowerInvariant();

            var existing = _db.Users.FirstOrDefault(u => u.email == email);
            if (existing != null)
            {
                ModelState.AddModelError("", "Email is already in use.");
                return View(model);
            }

            // Create user with plain password (no hashing, as requested)
            var user = new User
            {
                username = email.Split('@')[0],
                email = email,
                password = model.Password, // no hashing
                phone = string.Empty,
                is_admin = false,
                address = string.Empty,
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };

            _db.Users.Add(user);
            _db.SaveChanges();

            // Create auth ticket with role in UserData
            var role = user.is_admin ? "admin" : "member";
            var ticket = new FormsAuthenticationTicket(
                1,
                user.email,
                DateTime.Now,
                DateTime.Now.AddMinutes(60),
                false,
                role,
                FormsAuthentication.FormsCookiePath);

            var enc = FormsAuthentication.Encrypt(ticket);
            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, enc)
            {
                HttpOnly = true,
                Secure = FormsAuthentication.RequireSSL,
                Path = FormsAuthentication.FormsCookiePath
            };
            Response.Cookies.Add(cookie);

            // For UI only
            Session["DisplayName"] = string.IsNullOrWhiteSpace(user.username) ? user.email : user.username;

            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Login
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid) return View(model);

            var email = (model.Email ?? string.Empty).Trim().ToLowerInvariant();

            var user = _db.Users.FirstOrDefault(u => u.email == email && u.password == model.Password);
            if (user == null)
            {
                ModelState.AddModelError("", "Incorrect email or password.");
                return View(model);
            }

            // Determine role: admin > shipper > customer
            string role = "customer";
            if (user.is_admin)
            {
                role = "admin";
            }
            else if (!string.IsNullOrEmpty(user.role) && user.role == "shipper")
            {
                role = "shipper";
            }

            var ticket = new FormsAuthenticationTicket(
                1,
                user.email,
                DateTime.Now,
                DateTime.Now.AddDays(model.RememberMe ? 14 : 1),
                model.RememberMe,
                role,
                FormsAuthentication.FormsCookiePath);

            var enc = FormsAuthentication.Encrypt(ticket);
            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, enc)
            {
                HttpOnly = true,
                Secure = FormsAuthentication.RequireSSL,
                Path = FormsAuthentication.FormsCookiePath
            };
            if (model.RememberMe)
            {
                cookie.Expires = ticket.Expiration;
            }
            Response.Cookies.Add(cookie);

            // Store in session for UI
            Session["DisplayName"] = string.IsNullOrWhiteSpace(user.username) ? user.email : user.username;
            Session["UserRole"] = role;
            Session["UserId"] = user.id;

            // Redirect based on role
            if (role == "admin")
            {
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            }
            else if (role == "shipper")
            {
                return RedirectToAction("Index", "Shipper");
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Remove("DisplayName");
            Session.Remove("UserRole");
            Session.Remove("UserId");
            Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public ActionResult Unauthorized()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}




