using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Trianz.Enterprise.Operations.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Session;

namespace Trianz.Enterprise.Operations.Controllers
{
    public class EmployeeDashBoardController : Controller
    {
        // GET: EmployeeDashBoard
        private TrianzOperationsEntities db = new TrianzOperationsEntities();
        public ActionResult Index()
        {
            return View();
        }

        //public ActionResult EmployeeDashBoard()
        //{
        //    return View();
        //}

        public ActionResult EmployeeDashBoard()

        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                var emp = new ValidationModel();
                emp.EmployeeLocationlst = GenerateEmployeeLocation();
                emp.EmployeeDesignationlst = GetEmployeeDesignation();
                emp.EmployeeCountrieslst = GetEmployeeByCountry();
                return View(emp);
            }
            else
            {
                //ermsg = "Session expired";
                return Json("Sessionexpired");
                //return RedirectToAction("SessionExpire", "Signout");
            }
        }
        private List<EmployeeLocation> GenerateEmployeeLocation()
        {
            int empId = Convert.ToInt32(HttpContext.Session.GetString("EmployeeId"));

            List<EmployeeLocation> lstEmpLocationsummary = new List<EmployeeLocation>();

            //lstEmpLocationsummary = db.Set<EmployeeLocation>().FromSqlRaw
            //  ("exec GetEmployeeLocation").ToList<EmployeeLocation>();
            lstEmpLocationsummary = db.Set<EmployeeLocation>().FromSqlRaw("exec GetEmployeeLocation").ToList();

            //var studentGrades = db.GetEmployeeLocation();

            return lstEmpLocationsummary;
        }
       private List<EmployeeDesignation> GetEmployeeDesignation()
        {
            int empId = Convert.ToInt32(HttpContext.Session.GetString("EmployeeId"));

            List<EmployeeDesignation> lstEmployeeDesignation = new List<EmployeeDesignation>();

            lstEmployeeDesignation = db.Set<EmployeeDesignation>().FromSqlRaw("exec GetEmployeeByDesignation").ToList();

            return lstEmployeeDesignation;
        }

        private List<EmployeeCountry> GetEmployeeByCountry()
        {
            int empId = Convert.ToInt32(HttpContext.Session.GetString("EmployeeId"));

            List<EmployeeCountry> lstEmployeeCountry = new List<EmployeeCountry>();

            lstEmployeeCountry = db.Set<EmployeeCountry>().FromSqlRaw("exec GetEmployeeByCountry").ToList();

            return lstEmployeeCountry;
        }
    }
}
