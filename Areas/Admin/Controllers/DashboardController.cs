using System.Web.Mvc;
using ValiModern.Filters;

namespace ValiModern.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    public class DashboardController : Controller
    {
        // GET: Admin/Dashboard
        public ActionResult Index()
        {
            return View();
        }
    }
}