using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Trianz.Enterprise.Operations.Models;

namespace Trianz.Enterprise.Operations.Controllers
{
    public class ErrorDetailsController : Controller
    {
      
        public ActionResult ErrorDetailsView()
        {
            TrianzOperationsEntities Db = new TrianzOperationsEntities();
            Error_Info ei = new Error_Info();
            var errorlist = (from e in Db.Error_Info select e);
            return View(errorlist);
        }
    }
}
