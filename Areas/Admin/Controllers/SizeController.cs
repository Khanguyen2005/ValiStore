using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using ValiModern.Filters;
using ValiModern.Services;

namespace ValiModern.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    public class SizeController : Controller
    {
        // GET: Admin/Size
        public ActionResult Index(string sort)
        {
            var items = PaletteService.GetSizes().AsQueryable();
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

        // GET: Admin/Size/Create
        public ActionResult Create()
        {
            return View(new SizeOption());
        }
        // POST: Admin/Size/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(SizeOption model)
        {
            if (string.IsNullOrWhiteSpace(model?.Name))
                ModelState.AddModelError("Name", "Name is required");
            if (!ModelState.IsValid) return View(model);
            try
            {
                PaletteService.AddSize(model.Name.Trim());
                TempData["Success"] = "Size created.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }

        // GET: Admin/Size/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var item = PaletteService.GetSizes().FirstOrDefault(x => x.Id == id.Value);
            if (item == null) return HttpNotFound();
            return View(item);
        }
        // POST: Admin/Size/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, SizeOption model)
        {
            if (string.IsNullOrWhiteSpace(model?.Name))
                ModelState.AddModelError("Name", "Name is required");
            if (!ModelState.IsValid) return View(model);
            try
            {
                PaletteService.UpdateSize(id, model.Name.Trim());
                TempData["Success"] = "Size updated.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }

        // POST: Admin/Size/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            try
            {
                PaletteService.DeleteSize(id);
                TempData["Success"] = "Size deleted.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("Index");
        }
    }
}
