using System;
using System.Web.Mvc;
using Trianz.Enterprise.Operations.Filters;

namespace Trianz.Enterprise.Operations.Controllers
{
    [SkipAuthorizationFilter]
    public class HealthController : Controller
    {
        // GET: /health
        public ActionResult Index()
        {
            return Json(new
            {
                status = "Healthy",
                timestamp = DateTime.UtcNow
            }, JsonRequestBehavior.AllowGet);
        }
    }
}
