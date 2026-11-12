using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Trianz.Enterprise.Operations.General;

namespace Trianz.Enterprise.Operations.Controllers
{
    public class LoginController : Controller
    {
        // GET: Login
        public ActionResult Index()
        {
			string usermail = Common.GetAzureLoggedInUserID();
			string name = usermail.Split('@')[0].ToLower();

            if (name.ToLower() == "shaik.hakeem" || name.ToLower() == "sangeeth.vallampatla")
            {
                TempData["IsRoleMasterPageAccess"] = true;
                return RedirectToAction("index", "rolemaster");
            }

            return View();
        }

        [HttpPost]
        public ActionResult Index(string userName, string password)
        {
            if (userName == "admin" && password == "Test@1234") //T@lent0@123
            {
                TempData["IsRoleMasterPageAccess"] = true;
                return RedirectToAction("index", "rolemaster");
            }

            TempData["IsInvalidCredentials"] = true;
            return View("index");
        }
    }
}
