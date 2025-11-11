using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using ValiModern.Filters;
using ValiModern.Models.EF;

namespace ValiModern.Areas.Admin.Controllers
{
 [AuthorizeAdmin]
 public class UserController : Controller
 {
 private readonly ValiModernDBEntities _db = new ValiModernDBEntities();

 // GET: Admin/User
 public ActionResult Index(string q)
 {
 var users = _db.Users.AsQueryable();
 if (!string.IsNullOrWhiteSpace(q))
 {
 q = q.Trim().ToLower();
 users = users.Where(u => u.email.ToLower().Contains(q) ||
 (u.username != null && u.username.ToLower().Contains(q)) ||
 (u.phone != null && u.phone.ToLower().Contains(q)));
 }
 var list = users.OrderByDescending(u => u.created_at).Take(200).ToList();
 ViewBag.Query = q;
 return View(list);
 }

 // GET: Admin/User/Create
 public ActionResult Create()
 {
 return View();
 }

 // POST: Admin/User/Create
 [HttpPost]
 [ValidateAntiForgeryToken]
 public ActionResult Create([Bind(Include="username,email,phone,address,is_admin,password")] User model)
 {
 if (!ModelState.IsValid)
 return View(model);

 var email = (model.email ?? string.Empty).Trim().ToLowerInvariant();
 if (_db.Users.Any(u => u.email == email))
 {
 ModelState.AddModelError("email", "Email already exists.");
 return View(model);
 }

 model.email = email;
 model.username = string.IsNullOrWhiteSpace(model.username) ? email.Split('@')[0] : model.username.Trim();
 model.created_at = DateTime.UtcNow;
 model.updated_at = DateTime.UtcNow;
 // password saved as plain text as per requirement

 _db.Users.Add(model);
 _db.SaveChanges();
 TempData["Success"] = "User created.";
 return RedirectToAction("Index");
 }

 // GET: Admin/User/Edit/5
 public ActionResult Edit(int? id)
 {
 if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
 var user = _db.Users.Find(id);
 if (user == null) return HttpNotFound();
 return View(user);
 }

 // POST: Admin/User/Edit/5
 [HttpPost]
 [ValidateAntiForgeryToken]
 public ActionResult Edit(int id, [Bind(Include="id,username,email,phone,address,is_admin,password")] User model, string newPassword)
 {
 var user = _db.Users.Find(id);
 if (user == null) return HttpNotFound();

 if (!ModelState.IsValid)
 return View(user);

 var email = (model.email ?? string.Empty).Trim().ToLowerInvariant();
 if (_db.Users.Any(u => u.email == email && u.id != id))
 {
 ModelState.AddModelError("email", "Email already exists.");
 return View(user);
 }

 user.username = string.IsNullOrWhiteSpace(model.username) ? email.Split('@')[0] : model.username.Trim();
 user.email = email;
 user.phone = model.phone;
 user.address = model.address;
 user.is_admin = model.is_admin;
 if (!string.IsNullOrWhiteSpace(newPassword))
 {
 user.password = newPassword; // plain text as requested
 }
 user.updated_at = DateTime.UtcNow;

 _db.Entry(user).State = EntityState.Modified;
 _db.SaveChanges();
 TempData["Success"] = "User updated.";
 return RedirectToAction("Index");
 }

 // POST: Admin/User/Delete/5
 [HttpPost]
 [ValidateAntiForgeryToken]
 public ActionResult Delete(int id)
 {
 var user = _db.Users.Find(id);
 if (user == null) return HttpNotFound();
 _db.Users.Remove(user);
 _db.SaveChanges();
 TempData["Success"] = "User deleted.";
 return RedirectToAction("Index");
 }

 protected override void Dispose(bool disposing)
 {
 if (disposing) _db.Dispose();
 base.Dispose(disposing);
 }
 }
}
