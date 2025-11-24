using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using ValiModern.Filters;
using ValiModern.Helpers;
using ValiModern.Models.EF;

namespace ValiModern.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    public class CategoryController : Controller
    {
        private readonly ValiModernDBEntities _db = new ValiModernDBEntities();

        // GET: Admin/Category
        public ActionResult Index(string sort)
        {
            var categories = _db.Categories.AsQueryable();
            // Sorting
            switch (sort)
            {
                case "id_desc":
                    categories = categories.OrderByDescending(c => c.id);
                    break;
                case "id_asc":
                default:
                    categories = categories.OrderBy(c => c.id);
                    break;
            }
            ViewBag.Sort = string.IsNullOrEmpty(sort) ? "id_asc" : sort;
            var list = categories.ToList();
            return View(list);
        }

        // GET: Admin/Category/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/Category/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "name")] Category model)
        {
            if (!ModelState.IsValid) return View(model);
            var name = (model.name ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                ModelState.AddModelError("name", "Name required.");
                return View(model);
            }
            if (_db.Categories.Any(c => c.name.ToLower() == name.ToLower()))
            {
                ModelState.AddModelError("name", "Category already exists.");
                return View(model);
            }
            _db.Categories.Add(new Category { name = name });
            _db.SaveChanges();
            
            // Invalidate cache when category is created
            CacheHelper.InvalidateCategoryCache();
            
            TempData["Success"] = "Category created.";
            return RedirectToAction("Index");
        }

        // GET: Admin/Category/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var cat = _db.Categories.Find(id);
            if (cat == null) return HttpNotFound();
            return View(cat);
        }

        // POST: Admin/Category/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, [Bind(Include = "id,name")] Category model)
        {
            var cat = _db.Categories.Find(id);
            if (cat == null) return HttpNotFound();
            if (!ModelState.IsValid) return View(cat);
            var name = (model.name ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                ModelState.AddModelError("name", "Name required.");
                return View(cat);
            }
            if (_db.Categories.Any(c => c.id != id && c.name.ToLower() == name.ToLower()))
            {
                ModelState.AddModelError("name", "Category already exists.");
                return View(cat);
            }
            cat.name = name;
            _db.Entry(cat).State = EntityState.Modified;
            _db.SaveChanges();
            
            // Invalidate cache when category is updated
            CacheHelper.InvalidateCategoryCache();
            
            TempData["Success"] = "Category updated.";
            return RedirectToAction("Index");
        }

        // POST: Admin/Category/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var cat = _db.Categories.Find(id);
            if (cat == null) return HttpNotFound();
            _db.Categories.Remove(cat);
            _db.SaveChanges();
            
            // Invalidate cache when category is deleted
            CacheHelper.InvalidateCategoryCache();
            
            TempData["Success"] = "Category deleted.";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
