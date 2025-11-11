using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using ValiModern.Filters;
using ValiModern.Services;

namespace ValiModern.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    public class ColorController : Controller
    {
        // GET: Admin/Color
        public ActionResult Index(string sort)
        {
            var items = PaletteService.GetColors().AsQueryable();
            switch (sort)
            {
                case "name_desc": items = items.OrderByDescending(x => x.Name); break;
                case "name_asc": items = items.OrderBy(x => x.Name); break;
                case "id_desc": items = items.OrderByDescending(x => x.Id); break;
                case "id_asc":
                default: items = items.OrderBy(x => x.Id); break;
            }
            ViewBag.Sort = string.IsNullOrEmpty(sort) ? "id_asc" : sort;
            return View(items.ToList());
        }

        // GET: Admin/Color/Create
        public ActionResult Create()
        {
            return View(new ColorOption());
        }
        // POST: Admin/Color/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ColorOption model)
        {
            if (string.IsNullOrWhiteSpace(model?.Name))
                ModelState.AddModelError("Name", "Name is required");
            if (string.IsNullOrWhiteSpace(model?.ColorCode))
                ModelState.AddModelError("ColorCode", "Color code is required");
            if (!ModelState.IsValid) return View(model);
            try
            {
                PaletteService.AddColor(model.Name.Trim(), model.ColorCode.Trim());
                TempData["Success"] = "Color created.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }

        // GET: Admin/Color/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var item = PaletteService.GetColors().FirstOrDefault(x => x.Id == id.Value);
            if (item == null) return HttpNotFound();
            return View(item);
        }
        // POST: Admin/Color/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, ColorOption model)
        {
            if (string.IsNullOrWhiteSpace(model?.Name))
                ModelState.AddModelError("Name", "Name is required");
            if (string.IsNullOrWhiteSpace(model?.ColorCode))
                ModelState.AddModelError("ColorCode", "Color code is required");
            if (!ModelState.IsValid) return View(model);
            try
            {
                PaletteService.UpdateColor(id, model.Name.Trim(), model.ColorCode.Trim());
                TempData["Success"] = "Color updated.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }

        // POST: Admin/Color/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            try
            {
                PaletteService.DeleteColor(id);
                TempData["Success"] = "Color deleted.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("Index");
        }
    }
}
