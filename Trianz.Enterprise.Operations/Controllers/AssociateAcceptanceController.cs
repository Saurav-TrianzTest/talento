using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Trianz.Enterprise.Operations.Models;

namespace TZOperations.Controllers
{
    public class AssociateAcceptanceController : Controller
    {
        TrianzOperationsEntities db = new TrianzOperationsEntities();
       
        // GET: AssociateAcceptance
        public ActionResult AssociateAcceptance(int? hrrfID)
        {
            ValidationModel model = new ValidationModel();
            //if (hrrfID == null)
            //{
            //    return BadRequest();
            //}
           // HRRF hrrf = db.HRRF.Find(hrrfID);
            //if (hrrf == null)
            //{
            //    return NotFound();
            //}
            //return view
            return View();
            
        }


        // GET: AssociateAcceptance
        public ActionResult Accept(HRRF HRRF, ValidationModel model, string Submit)
        {
           
            return View();

        }
    }
}
