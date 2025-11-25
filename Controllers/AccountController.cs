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
            // User.Identity.Name now contains user ID
            int userId;
            if (!int.TryParse(User.Identity.Name, out userId))
            {
                TempData["Error"] = "Invalid user session.";
                return RedirectToAction("Login");
            }

            // Use AsNoTracking for read-only query
            var user = _db.Users.AsNoTracking().FirstOrDefault(u => u.id == userId);
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

            int userId;
            if (!int.TryParse(User.Identity.Name, out userId))
            {
                TempData["Error"] = "Invalid user session.";
                return RedirectToAction("Login");
            }

            var user = _db.Users.Find(userId);
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
                Session["DisplayName"] = string.IsNullOrWhiteSpace(user.username) 
                    ? (string.IsNullOrWhiteSpace(user.email) ? "User #" + user.id : user.email) 
                    : user.username;
                
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

            int userId;
            if (!int.TryParse(User.Identity.Name, out userId))
            {
                TempData["Error"] = "Invalid user session.";
                return RedirectToAction("Login");
            }

            var user = _db.Users.Find(userId);
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
            // Redirect to home and let them use the modal
            if (Request.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            TempData["ShowRegisterModal"] = true;
            return RedirectToAction("Index", "Home");
        }

        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var email = (model.Email ?? string.Empty).Trim().ToLowerInvariant();

            // Use AsNoTracking for read check
            var existing = _db.Users.AsNoTracking().FirstOrDefault(u => u.email == email);
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

            // FIX: Store user.id in ticket.Name instead of email
            var role = user.is_admin ? "admin" : "member";
            var ticket = new FormsAuthenticationTicket(
                1,
                user.id.ToString(), // CHANGED: Store ID instead of email
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
            Session["UserId"] = user.id;
            Session["UserRole"] = role;

            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Login
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            // If user tries to access /Account/Login directly,
            // redirect to home and let them use the modal
            if (Request.IsAuthenticated)
            {
                // Already logged in, redirect to returnUrl or home
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);
                return RedirectToAction("Index", "Home");
            }

            // Store returnUrl in TempData so we can redirect after modal login
            if (!string.IsNullOrEmpty(returnUrl))
            {
                TempData["ReturnUrl"] = returnUrl;
                TempData["ShowLoginModal"] = true;
            }

            // Redirect to home page (modal will open via JavaScript)
            return RedirectToAction("Index", "Home");
        }

        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid) return View(model);

            var email = (model.Email ?? string.Empty).Trim().ToLowerInvariant();

            // Optimize: Select only needed fields for login check
            var user = _db.Users
                .AsNoTracking()
                .Where(u => u.email == email && u.password == model.Password)
                .Select(u => new { u.id, u.username, u.email, u.is_admin, u.role })
                .FirstOrDefault();

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

            // FIX: Store user.id in ticket.Name instead of email
            var ticket = new FormsAuthenticationTicket(
                1,
                user.id.ToString(), // CHANGED: Store ID instead of email
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
            Session["DisplayName"] = string.IsNullOrWhiteSpace(user.username) 
                ? (string.IsNullOrWhiteSpace(user.email) ? "User #" + user.id : user.email)
                : user.username;
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

        // POST: /Account/LoginAjax - Ajax version for modal
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public JsonResult LoginAjax(string Email, string Password, string RememberMe, string ReturnUrl)
        {
            try
            {
                // Parse RememberMe checkbox (can be "on", "true", or null)
                bool rememberMe = (RememberMe == "on" || RememberMe == "true");
                
                // Manual validation
                if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
                {
                    return Json(new { success = false, message = "Please fill in all fields correctly." });
                }

                var email = Email.Trim().ToLowerInvariant();

                // Optimize: Select only needed fields and use AsNoTracking
                var user = _db.Users
                    .AsNoTracking()
                    .Where(u => u.email == email && u.password == Password)
                    .Select(u => new { u.id, u.username, u.email, u.is_admin, u.role })
                    .FirstOrDefault();

                if (user == null)
                {
                    return Json(new { success = false, message = "Incorrect email or password." });
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

                // Create authentication ticket
                var ticket = new FormsAuthenticationTicket(
                    1,
                    user.id.ToString(),
                    DateTime.Now,
                    DateTime.Now.AddDays(rememberMe ? 14 : 1),
                    rememberMe,
                    role,
                    FormsAuthentication.FormsCookiePath);

                var enc = FormsAuthentication.Encrypt(ticket);
                var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, enc)
                {
                    HttpOnly = true,
                    Secure = FormsAuthentication.RequireSSL,
                    Path = FormsAuthentication.FormsCookiePath
                };
                if (rememberMe)
                {
                    cookie.Expires = ticket.Expiration;
                }
                Response.Cookies.Add(cookie);

                // Store in session for UI
                Session["DisplayName"] = string.IsNullOrWhiteSpace(user.username) 
                    ? (string.IsNullOrWhiteSpace(user.email) ? "User #" + user.id : user.email)
                    : user.username;
                Session["UserRole"] = role;
                Session["UserId"] = user.id;

                // Determine redirect URL
                string redirectUrl = Url.Action("Index", "Home");
                
                // Priority: ReturnUrl > Role-based redirect > Home
                if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
                {
                    redirectUrl = ReturnUrl;
                }
                else if (role == "admin")
                {
                    redirectUrl = Url.Action("Index", "Dashboard", new { area = "Admin" });
                }
                else if (role == "shipper")
                {
                    redirectUrl = Url.Action("Index", "Shipper");
                }

                return Json(new { success = true, redirectUrl = redirectUrl });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LoginAjax] Exception: {ex.Message}");
                return Json(new { success = false, message = "An error occurred during login. Please try again." });
            }
        }

        // POST: /Account/RegisterAjax - Ajax version for modal
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public JsonResult RegisterAjax(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Json(new { success = false, message = string.Join(". ", errors) });
            }

            var email = (model.Email ?? string.Empty).Trim().ToLowerInvariant();

            // Use AsNoTracking for existence check
            var existing = _db.Users.AsNoTracking().Any(u => u.email == email);
            if (existing)
            {
                return Json(new { success = false, message = "Email is already in use." });
            }

            // Create user
            var user = new User
            {
                username = email.Split('@')[0],
                email = email,
                password = model.Password,
                phone = string.Empty,
                is_admin = false,
                address = string.Empty,
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };

            _db.Users.Add(user);
            _db.SaveChanges();

            // Auto-login after registration
            var role = user.is_admin ? "admin" : "member";
            var ticket = new FormsAuthenticationTicket(
                1,
                user.id.ToString(),
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

            // Store in session
            Session["DisplayName"] = string.IsNullOrWhiteSpace(user.username) ? user.email : user.username;
            Session["UserId"] = user.id;
            Session["UserRole"] = role;

            return Json(new { success = true, redirectUrl = Url.Action("Index", "Home") });
        }

        // POST: /Account/Logout
        [HttpPost]
        [Authorize] // Require authenticated user, remove anti-forgery to avoid mismatch when identity already cleared
        public ActionResult Logout()
        {
            // Preserve current auth state for debugging (optional)
            bool wasAuth = (User != null && User.Identity != null && User.Identity.IsAuthenticated);

            // Sign out (Forms auth cookie will be removed after response)
            FormsAuthentication.SignOut();

            // Clear session data
            Session.Remove("DisplayName");
            Session.Remove("UserRole");
            Session.Remove("UserId");
            Session.Clear();
            Session.Abandon();

            // Expire anti-forgery cookie if exists (prevent stale token reuse)
            var antiCookie = Request.Cookies["__RequestVerificationToken"];
            if (antiCookie != null)
            {
                antiCookie.Expires = DateTime.UtcNow.AddDays(-1);
                Response.Cookies.Add(antiCookie);
            }

            // Expire forms auth cookie explicitly (defensive)
            var authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie != null)
            {
                authCookie.Expires = DateTime.UtcNow.AddDays(-1);
                Response.Cookies.Add(authCookie);
            }

            // Redirect to home (or Login page)
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Logout (Fallback for direct link)
        [HttpGet]
        [Authorize]
        public ActionResult LogoutDirect()
        {
            FormsAuthentication.SignOut();
            Session.Remove("DisplayName");
            Session.Remove("UserRole");
            Session.Remove("UserId");
            Session.Clear();
            Session.Abandon();

            var antiCookie = Request.Cookies["__RequestVerificationToken"];
            if (antiCookie != null)
            {
                antiCookie.Expires = DateTime.UtcNow.AddDays(-1);
                Response.Cookies.Add(antiCookie);
            }
            var authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie != null)
            {
                authCookie.Expires = DateTime.UtcNow.AddDays(-1);
                Response.Cookies.Add(authCookie);
            }
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

































































































