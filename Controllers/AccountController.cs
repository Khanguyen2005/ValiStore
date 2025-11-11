using System;
using System.Linq;
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
            if (Session["Email"] != null)
            {
                return RedirectToAction("Home_Page", "HomePage");
            }
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            using (var db = new ValiModernDBEntities())
            {
                var existing = db.Users.FirstOrDefault(u => u.email == model.Email);
                if (existing != null)
                {
                    ModelState.AddModelError("", "Email đã được sử dụng.");
                    return View(model);
                }

                // Create user with plain password (no hashing, as requested)
                var user = new User
                {
                    username = model.Email.Split('@')[0],
                    email = model.Email,
                    password = model.Password, // không hash
                    phone = "",
                    is_admin = false,
                    address = "",
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                };

                db.Users.Add(user);
                db.SaveChanges();

                // Auto-login after registration
                FormsAuthentication.SetAuthCookie(user.email, false);

                // Fill session for layout usage
                Session["IsAdmin"] = user.is_admin;
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

            using (var db = new ValiModernDBEntities())
            {
                var user = db.Users.FirstOrDefault(u => u.email == model.Email && u.password == model.Password);
                if (user == null)
                {
                    ModelState.AddModelError("", "Email hoặc mật khẩu không đúng.");
                    return View(model);
                }

                FormsAuthentication.SetAuthCookie(user.email, model.RememberMe);

                Session["IsAdmin"] = user.is_admin;
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
            Session.Remove("IsAdmin");
            Session.Remove("DisplayName");
            Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        // Optional: simple unauthorized view used by filters
        [HttpGet]
        public ActionResult Unauthorized()
        {
            return View();
        }
    }
}




