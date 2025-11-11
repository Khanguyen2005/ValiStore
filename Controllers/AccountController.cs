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

            using (var db = new ValiModernDBEntities())
            {
                var existing = db.Users.FirstOrDefault(u => u.email == email);
                if (existing != null)
                {
                    ModelState.AddModelError("", "Email đã được sử dụng.");
                    return View(model);
                }

                // Create user with plain password (no hashing, as requested)
                var user = new User
                {
                    username = email.Split('@')[0],
                    email = email,
                    password = model.Password, // không hash
                    phone = string.Empty,
                    is_admin = false,
                    address = string.Empty,
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                };

                db.Users.Add(user);
                db.SaveChanges();

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

            using (var db = new ValiModernDBEntities())
            {
                var user = db.Users.FirstOrDefault(u => u.email == email && u.password == model.Password);
                if (user == null)
                {
                    ModelState.AddModelError("", "Email hoặc mật khẩu không đúng.");
                    return View(model);
                }

                var role = user.is_admin ? "admin" : "member";
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

                Session["DisplayName"] = string.IsNullOrWhiteSpace(user.username) ? user.email : user.username;

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index", "Home");
            }
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Remove("DisplayName");
            Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public ActionResult Unauthorized()
        {
            return View();
        }
    }
}




