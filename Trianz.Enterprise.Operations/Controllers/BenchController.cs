using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Trianz.Enterprise.Operations.General;
//using ResourceEXPERT.Models;
//using ResourceEXPERT.ViewModels;
using Trianz.Enterprise.Operations.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Session;

namespace Trianz.Enterprise.Operations.Controllers
{
	
	public class BenchController : Controller
    {
        TrianzOperationsEntities _db = new TrianzOperationsEntities();
        // GET: Bench
         public ActionResult Resources(string page = "", string ddlPractise = "", string EmpFName = "")
         {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                try
                {
                    if (!string.IsNullOrEmpty(ddlPractise) || !string.IsNullOrEmpty(EmpFName))
                    {
                        if (ddlPractise == "")
                        {
                            ddlPractise = null;
                        }
                        if (EmpFName == "")
                        {
                            EmpFName = null;
                        }
                        var employees = (from emp in _db.Employees
                                         where emp.ReservationStatus != "Free" &&
                                         (emp.Practice == ddlPractise || ddlPractise == null) &&
                                         (emp.FirstName == EmpFName || EmpFName == null)
                                         select emp);
                        ViewBag.SelectedEmployee = employees.FirstOrDefault();
                        return View(employees.ToList());
                    }
                    else
                    {
                        var employees = (from emp in _db.Employees
                                         where emp.ReservationStatus != "Free"
                                         select emp);
                        ViewBag.SelectedEmployee = employees.FirstOrDefault();
                        //ViewBag.DefaultEmployee = employees.FirstOrDefault();
                        return View(employees.ToList());
                    }
                }
                catch (Exception ex)
                {
                    Common.WriteExceptionErrorLog(ex);
                    return RedirectToAction("Error", "Error");
                    //return Json("Error");
                }
            }
            else
            {
                //ermsg = "Session expired";
                //return Json("Sessionexpired");
                return RedirectToAction("SessionExpire", "Signout");
            }


        }

        public ActionResult GetResource(string term)
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {

                var result = (from r in _db.Employees
                              where r.ReservationStatus != "Free" && r.FirstName.ToLower().Contains(term)
                              select r.FirstName).Distinct();

                return Json(result);
            }
            else
            {
                //ermsg = "Session expired";
                //return Json("Sessionexpired");
                return RedirectToAction("SessionExpire", "Signout");
            }
        }


        public JsonResult GetBenchResourcewithID()
        {
            JsonResult result = new JsonResult(null);

            var Reources = (from e in _db.Employees
                            join be in _db.BenchEmployees on e.EmployeeId
                            equals be.EmployeeId
                            where e.ReservationStatus == "Free"
                            select new
                            {
                            FirstName = e.FirstName,
                            LastName=e.LastName,
                            EmpID=e.EmployeeId
                            }).Distinct().ToList();
            result.Value = Reources.ToList();
            return result;
        }


        public ActionResult GetBenchResource(string term)
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                try
                {
                    var result = (from e in _db.Employees
                                  join be in _db.BenchEmployees on e.EmployeeId
                                  equals be.EmployeeId
                                  where e.FirstName.ToLower().Contains(term) && e.ReservationStatus == "Free"
                                  select e.FirstName).Distinct();

                    return Json(result);
                }
                catch (Exception ex)
                {
                    Common.WriteExceptionErrorLog(ex);
                    return RedirectToAction("Error", "Error");
                    //return Json("Error");
                }
            }
            else
            {
                //ermsg = "Session expired";
                //return Json("Sessionexpired");
                return RedirectToAction("SessionExpire", "Signout");
            }
        }
        public ActionResult GetBenchSPOC(string term)
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                try
                {
                    var result = "";
                    //var result = (from r in _db.Employees
                    //              where r.Supervisor.ToLower().Contains(term)
                    //              select r.Supervisor).Distinct();

                    return Json(result);
                }
                catch (Exception ex)
                {
                    Common.WriteExceptionErrorLog(ex);
                    return RedirectToAction("Error", "Error");
                    //return Json("Error");
                }
            }
            else
            {
                //ermsg = "Session expired";
                //return Json("Sessionexpired");
                return RedirectToAction("SessionExpire", "Signout");
            }

        }
        public ActionResult AddResources(string page = "", string BenchEmpFirstName = "", string AutoSPOC = "", string ddlPractise="")
        {


            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                try
                {

                    if (!string.IsNullOrEmpty(BenchEmpFirstName) || !string.IsNullOrEmpty(AutoSPOC) || !string.IsNullOrEmpty(ddlPractise))
                    {
                        if (BenchEmpFirstName == "")
                        {
                            BenchEmpFirstName = null;
                        }
                        if (ddlPractise == "")
                        {
                            ddlPractise = null;
                        }
                        if (AutoSPOC == "")
                        {
                            AutoSPOC = null;
                        }
                        var benchEmployees = (from e in _db.Employees
                                              join be in _db.BenchEmployees on e.EmployeeId
                                              equals be.EmployeeId
                                              where (e.Practice == ddlPractise || ddlPractise == null) &&
                                              (e.FirstName == BenchEmpFirstName || BenchEmpFirstName == null) &&
                                              (be.SPOC == AutoSPOC || AutoSPOC == null)


                                              select new BenchEmployees
                                              {
                                                  BenchEmployeeId = be.BenchEmployeeId,
                                                  EmployeeId = be.EmployeeId,
                                                  FirstName = e.FirstName,
                                                  LastName = e.LastName,
                                                  SPOC = be.SPOC,
                                                  ProjectCode = be.ProjectCode,
                                                  StartDate = be.StartDate,
                                                  EndDate = be.EndDate,
                                                  Utilization = be.Utilization,
                                                  LocationType = be.LocationType,
                                                  Location = be.Location,
                                                  AssignmentStatus = be.AssignmentStatus,
                                                  Comments = be.Comments
                                              }).OrderByDescending(be => be.BenchEmployeeId);
                        return View(benchEmployees.ToList());
                    }
                    else
                    {
                        var benchEmployees = (from e in _db.Employees
                                              join be in _db.BenchEmployees on e.EmployeeId
                                              equals be.EmployeeId
                                              select new BenchEmployees
                                              {
                                                  BenchEmployeeId = be.BenchEmployeeId,
                                                  EmployeeId = be.EmployeeId,
                                                  FirstName = e.FirstName,
                                                  LastName = e.LastName,
                                                  SPOC = be.SPOC,
                                                  ProjectCode = be.ProjectCode,
                                                  StartDate = be.StartDate,
                                                  EndDate = be.EndDate,
                                                  Utilization = be.Utilization,
                                                  LocationType = be.LocationType,
                                                  Location = be.Location,
                                                  AssignmentStatus = be.AssignmentStatus,
                                                  Comments = be.Comments
                                              }).OrderByDescending(be => be.BenchEmployeeId);

                        return View(benchEmployees.ToList());
                    }
                }
                catch (Exception ex)
                {
                    Common.WriteExceptionErrorLog(ex);
                    //return RedirectToAction("Error", "Error");
                    return Json("Error");
                }
               
                //return View();
            }
            else
            {
                //ermsg = "Session expired";
                return Json("Sessionexpired");
                //return RedirectToAction("SessionExpire", "Signout");
            }
        }
        [HttpPost]
        public ActionResult AddResources(string empNumbers)
        {
            try
            {
                List<BenchEmployee> _benchEmployee = new List<BenchEmployee>();

                int empNumber;
                //string[] employees = empNumbers.Split(',');
                //employees = empNumbers.Split('\n');
                char[] delimiters = new[] { ',', ' ' };

                string existingEmpNumber = string.Empty;
                foreach (var empID in empNumbers.Split(delimiters, StringSplitOptions.RemoveEmptyEntries))
                {
                    //var _employee = new Employee { EmployeeId= Convert.ToInt32(empID.Trim()), ReservationStatus = "Free" };


                    empNumber = int.Parse(empID.Trim());
                    var empnumberExists = (from t in _db.BenchEmployees
                                           where t.EmployeeId == empNumber
                                           select t).Count();


                    //bool exists = _db.BenchEmployees.Any(t => t.EmployeeId == Convert.ToInt32(empID.Trim()));
                    if (empnumberExists == 0)
                    {
                        Employee _employee = _db.Employees.Single(e => e.EmployeeId == empNumber);
                        _employee.ReservationStatus = "Free";

                        var benchEmployee = new BenchEmployee { EmployeeId = Convert.ToInt32(empID.Trim()) };
                        _db.BenchEmployees.Add(benchEmployee);
                    }
                    else
                    {
                        existingEmpNumber = existingEmpNumber + "," + empID;
                    }

                }
                _db.SaveChanges();

                if (!string.IsNullOrEmpty(existingEmpNumber))
                {
                    existingEmpNumber = existingEmpNumber.Substring(1);
                    return Json(new { success = true, responseText = existingEmpNumber });
                }
                else
                {
                    return Json(_benchEmployee);
                }
            }
            catch (Exception ex)
            {
                Common.WriteExceptionErrorLog(ex);
               // return RedirectToAction("Error", "Error");
                return Json("Error");
            }
           
        }
        [HttpPost]
        public ActionResult Resources(int? empNum)
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                try
                {

                    var selectedEmployee = (from emp in _db.Employees
                                            where emp.EmployeeId == empNum
                                            select emp);

                    //var displayEmployee = (from emp in _db.Employees
                    //                       where emp.ReservationStatus != "Free"
                    //                       select emp).FirstOrDefault();

                    ViewBag.SelectedEmployee = selectedEmployee.FirstOrDefault();
                    //return Json(selectedEmployee.ToList());
                    //return PartialView("_EmployeeGrid", selectedEmployee.ToList());
                    return View(selectedEmployee);
                }
                catch (Exception ex)
                {
                    Common.WriteExceptionErrorLog(ex);
                    return RedirectToAction("Error", "Error");
                    //return Json("Error");
                }


            }
            else
            {
                //ermsg = "Session expired";
              //  return Json("Sessionexpired");
                return RedirectToAction("SessionExpire", "Signout");
            }

            }
        public ActionResult EmployeeDetails(int empNum)
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                try
                {
                    var selectedEmployee = (from emp in _db.Employees
                                            where emp.EmployeeId == empNum
                                            select emp);
                    ViewBag.SelectedEmployee = selectedEmployee;
                    return View(selectedEmployee);
                }
                catch (Exception ex)
                {
                    Common.WriteExceptionErrorLog(ex);
                    return RedirectToAction("Error", "Error");
                    //return Json("Error");
                }


            }
            else
            {
                //ermsg = "Session expired";
                //return Json("Sessionexpired");
                return RedirectToAction("SessionExpire", "Signout");
            }
        }
        [HttpPost]
        public ActionResult EmployeeDetails(int? empNum)
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {

                try
                {
                    var selectedEmployee = (from emp in _db.Employees
                                            where emp.EmployeeId == empNum
                                            select emp);

                    //var displayEmployee = (from emp in _db.Employees
                    //                       where emp.ReservationStatus != "Free"
                    //                       select emp).FirstOrDefault();

                    ViewBag.SelectedEmployee = selectedEmployee.FirstOrDefault();
                    // return View("EmployeeDetails",selectedEmployee);

                    return RedirectToAction("EmployeeDetails", "Bench", new { empNum = empNum });
                    //return Json(selectedEmployee.ToList());
                    //return PartialView("_EmployeeGrid", selectedEmployee.ToList());
                    //return View("Resources", selectedEmployee.ToList());
                }
                catch (Exception ex)
                {
                    Common.WriteExceptionErrorLog(ex);
                    return RedirectToAction("Error", "Error");
                    //return Json("Error");
                }


            }
            else
            {
                //ermsg = "Session expired";
                return Json("Sessionexpired");
                //return RedirectToAction("SessionExpire", "Signout");
            }

            }

        [HttpPost]
        public ActionResult SearchResources(string employeeName)
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                try
                {
                    var searchEmployee = from e in _db.Employees
                                         where e.FirstName == employeeName
                                         select e;
                    ViewBag.DefaultEmployee = searchEmployee.FirstOrDefault();
                    return View("Resources", searchEmployee.ToList());

                    //return Json(searchEmployee.ToList());
                    //return PartialView("_EmployeeGrid", searchEmployee.ToList());
                }
                catch (Exception ex)
                {
                    Common.WriteExceptionErrorLog(ex);
                    return RedirectToAction("Error", "Error");
                    //return Json("Error");
                }


            }

            else
            {
                //ermsg = "Session expired";
                //return Json("Sessionexpired");
                return RedirectToAction("SessionExpire", "Signout");
            }


            }
        [HttpGet]
        public ActionResult SearchBenchEmployee()
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                try
                {
                    List<string> tempSearchData = TempData["SearchParameters"] as List<string>;
                    TempData.Keep();
                    string practice = tempSearchData[1];
                    var benchEmployees = (from e in _db.Employees
                                          join be in _db.BenchEmployees on e.EmployeeId
                                          equals be.EmployeeId
                                          where e.Practice == practice
                                          select new BenchEmployees
                                          {
                                              BenchEmployeeId = be.BenchEmployeeId,
                                              EmployeeId = be.EmployeeId,
                                              FirstName = e.FirstName,
                                              LastName = e.LastName,
                                              SPOC = be.SPOC,
                                              ProjectCode = be.ProjectCode,
                                              StartDate = be.StartDate,
                                              EndDate = be.EndDate,
                                              Utilization = be.Utilization,
                                              LocationType = be.LocationType,
                                              Location = be.Location,
                                              AssignmentStatus = be.AssignmentStatus,
                                              Comments = be.Comments
                                          }).OrderByDescending(be => be.BenchEmployeeId);
                    return View("AddResources", benchEmployees.ToList());
                }
                catch (Exception ex)
                {
                    Common.WriteExceptionErrorLog(ex);
                    return RedirectToAction("Error", "Error");
                    //return Json("Error");
                }


            }
            else
            {
                //ermsg = "Session expired";
                //return Json("Sessionexpired");
                return RedirectToAction("SessionExpire", "Signout");
            }

            }
        [HttpPost]
        public ActionResult SearchBenchEmployee(string SPOC, string Practice, string Resource)
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                try
                {
                    List<string> TempSearchParameters = new List<string>();
                    TempSearchParameters.Add(SPOC);
                    TempSearchParameters.Add(Practice);
                    TempSearchParameters.Add(Resource);
                    TempData["SearchParameters"] = TempSearchParameters;

                    if (Resource == "")
                    {
                        Resource = null;
                    }
                    if (Practice == "")
                    {
                        Practice = null;
                    }
                    if (SPOC == "")
                    {
                        SPOC = null;
                    }
                    //var benchEmployees = (from e in _db.Employees
                    //                      join be in _db.BenchEmployees on e.EmployeeId
                    //                      equals be.EmployeeId
                    //                      where (Practice == "" ? e.Practice == null : e.Practice == Practice) &&
                    //                                            (SPOC == "" ? be.SPOC == null : be.SPOC == SPOC) &&
                    //                                            (Resource == "" ? e.FirstName == null : e.FirstName == Resource)
                    var benchEmployees = (from e in _db.Employees
                                          join be in _db.BenchEmployees on e.EmployeeId
                                          equals be.EmployeeId
                                          where (e.Practice == Practice || Practice == null) &&
                                          (e.FirstName == Resource || Resource == null) &&
                                          (be.SPOC == SPOC || SPOC == null)


                                          select new BenchEmployees
                                          {
                                              BenchEmployeeId = be.BenchEmployeeId,
                                              EmployeeId = be.EmployeeId,
                                              FirstName = e.FirstName,
                                              LastName = e.LastName,
                                              SPOC = be.SPOC,
                                              ProjectCode = be.ProjectCode,
                                              StartDate = be.StartDate,
                                              EndDate = be.EndDate,
                                              Utilization = be.Utilization,
                                              LocationType = be.LocationType,
                                              Location = be.Location,
                                              AssignmentStatus = be.AssignmentStatus,
                                              Comments = be.Comments
                                          }).OrderByDescending(be => be.BenchEmployeeId);


                    //return PartialView("_BenchEmployeeGrid", benchEmployees.ToList());
                    //return Json(benchEmployees.ToList());
                    return View("AddResources", benchEmployees.ToList());
                }
                catch (Exception ex)
                {
                    Common.WriteExceptionErrorLog(ex);
                   // return RedirectToAction("Error", "Error");
                    return Json("Error");
                }


                //return View();
            }
            else
            {
                //ermsg = "Session expired";
                return Json("Sessionexpired");
                //return RedirectToAction("SessionExpire", "Signout");
            }
            }

        [HttpGet]
        public ActionResult SearchEmployeeWithPractice()
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                try
                {
                    string strPractice = string.Empty;
                    //if (TempData["Practice"] != null)
                    //{
                    //    strPractice =Convert.ToString(TempData["Practice"]);
                    //}

                    if (HttpContext.Session.GetString("Practice") != null)
                    {
                        strPractice = Convert.ToString(HttpContext.Session.GetString("Practice"));
                    }
                    var searchPractice = from e in _db.Employees
                                         where e.Practice == strPractice
                                         select e;
                    ViewBag.DefaultEmployee = searchPractice.FirstOrDefault();
                    //return Json(searchEmployee.ToList());
                    return View("Resources", searchPractice.ToList());
                    //return Json(searchEmployee.ToList());
                }
                catch (Exception ex)
                {
                    Common.WriteExceptionErrorLog(ex);
                    return RedirectToAction("Error", "Error");
                    //return Json("Error");
                }
            }
            else
            {
                //ermsg = "Session expired";
                //return Json("Sessionexpired");
                return RedirectToAction("SessionExpire", "Signout");
            }

            }

        [HttpPost]
        public ActionResult SearchEmployeeWithPractice(string Practice)
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                try
                {
                    var searchPractice = from e in _db.Employees
                                         where e.Practice == Practice
                                         select e;
                    //TempData["Practice"] = Practice;
                    HttpContext.Session.SetString("Practice", Practice.ToString());
                    ViewBag.DefaultEmployee = searchPractice.FirstOrDefault();
                    //return Json(searchEmployee.ToList());
                    return View("Resources", searchPractice.ToList());
                    //return Json(searchEmployee.ToList());
                }
                catch (Exception ex)
                {
                    Common.WriteExceptionErrorLog(ex);
                    return RedirectToAction("Error", "Error");
                    //return Json("Error");
                }
            }
             else
            {
                //ermsg = "Session expired";
                //return Json("Sessionexpired");
                return RedirectToAction("SessionExpire", "Signout");
            }
        }

        [HttpPost]
        public ActionResult UpdateBenchResource(int BenchEmployeeID, string SPOC, string ProjectCode, DateTime StartDate, DateTime EndDate, string LocationType, string Location, string Status, int Utilization, string Comments)
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                try
                {
                    var benchEmployee = (from be in _db.BenchEmployees
                                         where be.BenchEmployeeId == BenchEmployeeID
                                         select be).Single();
                    benchEmployee.SPOC = SPOC;
                    benchEmployee.ProjectCode = ProjectCode;
                    benchEmployee.StartDate = StartDate;
                    benchEmployee.EndDate = EndDate;
                    benchEmployee.LocationType = LocationType;
                    benchEmployee.Location = Location;
                    benchEmployee.AssignmentStatus = Status;
                    benchEmployee.Utilization = Utilization;
                    benchEmployee.Comments = Comments;

                    _db.SaveChanges();

                    return Json(benchEmployee);
                }
                catch (Exception ex)
                {
                    Common.WriteExceptionErrorLog(ex);
                   // return RedirectToAction("Error", "Error");
                    return Json("Error");
                }
            }
            else
            {
                //ermsg = "Session expired";
                return Json("Sessionexpired");
               // return RedirectToAction("SessionExpire", "Signout");
            }
            }
    }
}
