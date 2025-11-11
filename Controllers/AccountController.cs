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
        public ActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
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

                // Lưu trực tiếp password (đơn giản, KHÔNG mã hóa)
                var newUser = new User
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

                db.Users.Add(newUser);
                db.SaveChanges();

                // Đăng nhập tự động sau khi đăng ký
                FormsAuthentication.SetAuthCookie(model.Email, false);

                return RedirectToAction("Index", "Home");
            }
        }

        // GET: /Account/Login
        [HttpGet]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
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

                // Đăng nhập thành công
                FormsAuthentication.SetAuthCookie(user.email, model.RememberMe);

                // Lưu vào session để layout dùng
                Session["IsAdmin"] = user.is_admin;
                Session["DisplayName"] = string.IsNullOrWhiteSpace(user.username)
                    ? user.email
                    : user.username;

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index", "Home");
            }
        }

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

        public ActionResult Unauthorized()
        {
            return View();
        }
    }
}
