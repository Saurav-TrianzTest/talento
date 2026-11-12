using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using X.PagedList;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Trianz.Enterprise.Operations.Models;
using System.IO;
using OfficeOpenXml;
using Trianz.Enterprise.Operations.General;
using ExcelDataReader;
using Microsoft.AspNetCore.Session;
using Microsoft.Extensions.Hosting;

namespace Trianz.Enterprise.Operations.Controllers
{
	[Authorize]
	public class EmployeesController : Controller
    {
    private readonly IWebHostEnvironment Env;
        private TrianzOperationsEntities db = new TrianzOperationsEntities();
        List<Employee> objlst = new List<Employee>();
        // GET: Employees
        public ActionResult Index(string sortOrder, string currentFilter, string searchString, int? page)
        {
            try
            {
                //Below single statement is added by Sarath, for security reason to access RoleMaster page.
                TempData["IsRoleMasterPageAccess"] = null;

                ViewBag.CurrentSort = sortOrder;
                ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "Username" : "";

                if (searchString != null)
                {
                    page = 1;
                }
                else
                {
                    searchString = currentFilter;
                }

                ViewBag.CurrentFilter = searchString;




                var EmpDetails = from e in db.Employees select e;


                foreach (var role in EmpDetails)
                {

                    Employee objemployee = new Employee();
                    objemployee.EmployeeId = role.EmployeeId;
                    objemployee.FirstName = role.FirstName;
                    objemployee.MiddleName = role.MiddleName;
                    objemployee.LastName = role.LastName;
                    objemployee.UserName = role.UserName;
                    objemployee.Email = role.Email;
                    objemployee.Gender = role.Gender;
                    objemployee.Grade = role.Grade;
                    objemployee.LocationType = role.LocationType;
                    objemployee.Location = role.Location;
                    objemployee.EmployeeType = role.EmployeeType;
                    objemployee.Designation = role.Designation;
                    objemployee.DateOfBirth = role.DateOfBirth;
                    objemployee.DateOfJoin = role.DateOfJoin;
                    objemployee.PreviousExperience = role.PreviousExperience;
                    objemployee.PassportNumber = role.PassportNumber;
                    objemployee.Visa = role.Visa;
                    objemployee.BusinessGroup = role.BusinessGroup;
                    objemployee.CostCenter = role.CostCenter;
                    objemployee.PARENT_ORG = role.PARENT_ORG;
                    objemployee.MasterSkillSet = role.MasterSkillSet;
                    objemployee.PrimarySkills = role.PrimarySkills;
                    objemployee.SecondarySkills = role.SecondarySkills;
                    objemployee.ResourceStatus = role.ResourceStatus;
                    objemployee.ReservationStatus = role.ReservationStatus;
                    objemployee.AssignmentStatus = role.AssignmentStatus;
                    objemployee.SupervisorId = role.SupervisorId;
                    objemployee.ProjectManagerId = role.ProjectManagerId;
                    objemployee.Utilization = role.Utilization;
                    objemployee.IsServicingNoticePeriod = role.IsServicingNoticePeriod;
                    objemployee.IsTravelReady = role.IsTravelReady;
                    objemployee.IsActive = role.IsActive;
                    objemployee.RelievingDate = role.RelievingDate;
                    objemployee.CreatedDate = role.CreatedDate;
                    objemployee.ModifiedDate = role.ModifiedDate;
                    objemployee.CreatedBy = role.CreatedBy;
                    objemployee.ModifiedBy = role.ModifiedBy;
                    objlst.Add(objemployee);

                }
                ViewData["emplist"] = objlst;
                if (!String.IsNullOrEmpty(searchString))
                {
                    objlst = objlst.Where(s => (s.UserName).Contains(searchString.ToUpper())).ToList();
                }
                switch (sortOrder)
                {
                    case "Username":
                        objlst = objlst.OrderByDescending(s => s.UserName).ToList();
                        break;

                    default:  // Name ascending 
                        objlst = objlst.OrderBy(s => s.UserName).ToList();
                        break;
                }

                int pageSize = 10;
                int pageNumber = (page ?? 1);
                return View(objlst.ToPagedList(pageNumber, pageSize));
            }
            catch (Exception ex)
            {
                Common.WriteExceptionErrorLog(ex);
                return RedirectToAction("Error", "Error");
                //return Json("Error");
            }

            //    return View(db.Employees.ToList());
        }


        //public ActionResult AllAssignmentsUpdate()
        //{
        //    ValidationModel viewResult = new ValidationModel();
        //    //Below single statement is added by Sarath, for security reason to access RoleMaster page.
        //    TempData["IsRoleMasterPageAccess"] = null;

        //    List<ProjectAssignments> lstProjectAssignment = new List<ProjectAssignments>();
        //    string Role = (string)HttpContext.Session.GetString("Role");  // PM, OM
        //    int empID = 103974; //101098 Manger ID;

        //    #region bILLINGSTATUS  
        //    //dropdown data for Billing Status from master lookup



        //    var billingStatus = from billingstatus in db.MasterLookUps.Where(x => x.LookupType == "BillingStatus")
        //                        select new
        //                        {
        //                            LookupName = billingstatus.LookupName,
        //                            Description = billingstatus.Description,
        //                            LookupCode = billingstatus.LookupCode,
        //                            SeqNumber = billingstatus.SeqNumber
        //                        };
        //    ViewData["_BillingStatus"] = billingStatus.ToList();

        //    var _Practise = (from emp in db.PracticeWiseBenchCodes
        //                     select new
        //                     {
        //                         Practice = emp.Practice,
        //                     }).Distinct().OrderBy(p => p.Practice).ToList();


        //    if (ViewData["_Practice"] == null)
        //    {
        //        ViewData["_Practice"] = _Practise;
        //    }


        //    #endregion


        //    #region ForPM
        //    if (Role == "PM")
        //    {
        //        var employeeLog = db.Employees.Where(e => e.EmployeeId == empID).FirstOrDefault();
        //        if (employeeLog != null)
        //        {
        //            var _ProjectNames = (from projects in db.Projects.
        //                                 Where(p => p.ProjectManagerId == empID && p.IsActive == true)
        //                                 select new
        //                                 {
        //                                     ProjectName = projects.ProjectCode + "-" + projects.ProjectName,
        //                                     ProjectCode = projects.ProjectCode
        //                                 }).OrderBy(p => p.ProjectName).ToList();
        //            if (HttpContext.Session.GetString("_ProjectNames") == null)
        //            {
        //                HttpContext.Session.SetString("_ProjectNames", _ProjectNames.ToString());
        //            }
        //            var _Accounts = (from projects in db.Projects.
        //                                 Where(p => p.ProjectManagerId == empID && p.IsActive == true)
        //                             select new
        //                             {
        //                                 Accountname = projects.AccountName

        //                             }).Distinct().OrderBy(p => p.Accountname).ToList();
        //            if (ViewData["_Accounts"] == null)
        //            {
        //                ViewData["_Accounts"] = _Accounts;
        //            }
        //        }
        //        try
        //        {

        //            var resourceDetails = (from pa in db.ProjectAssignments
        //                                   join emp in db.Employees on pa.EmployeeId equals emp.EmployeeId
        //                                   join prjct in db.Projects on pa.ProjectCode equals prjct.ProjectCode
        //                                   //join empDoc in _db.EmployeeDocs on emp.EmployeeId equals empDoc.Employeeid into tempEmpDoc
        //                                   //from empDoc in tempEmpDoc.DefaultIfEmpty()
        //                                   where emp.IsActive == true
        //                                   && prjct.ProjectManagerId == empID
        //                                   select new
        //                                   {
        //                                       pa.EmployeeId,
        //                                       pa.ProjectCode,
        //                                       pa.ProjectName,
        //                                       emp.FirstName,
        //                                       emp.MiddleName,
        //                                       emp.LastName,
        //                                       prjct.ProjectManagerId,
        //                                       pa.Assigned_By,
        //                                       pa.Utilization,
        //                                       emp.Practice,
        //                                       pa.BillingStatus,
        //                                       emp.AssignmentStatus,
        //                                       pa.StartDate,
        //                                       pa.EndDate,
        //                                       prjct.BillingType,
        //                                       pa.Assignment_Id,
        //                                       pa.IsActive

        //                                   }).OrderBy(x => x.IsActive).ToList();

        //            foreach (var x in resourceDetails)
        //            {
        //                ProjectAssignments objProjectAssignments = new ProjectAssignments();
        //                objProjectAssignments.ProjectName = x.ProjectName;
        //                objProjectAssignments.ProjectCode = x.ProjectCode;
        //                objProjectAssignments.EmployeeId = x.EmployeeId;
        //                objProjectAssignments.EmployeeName = x.FirstName + " " + x.MiddleName + " " + x.LastName;
        //                objProjectAssignments.StartDate = x.StartDate;
        //                objProjectAssignments.EndDate = x.EndDate;
        //                objProjectAssignments.Assigned_By = x.Assigned_By;
        //                objProjectAssignments.Utilisation = x.Utilization;
        //                objProjectAssignments.AssignmentStatus = x.AssignmentStatus;
        //                objProjectAssignments.Practice = x.Practice;
        //                objProjectAssignments.BillingType = x.BillingType;
        //                objProjectAssignments.BillingStatus = x.BillingStatus;
        //                objProjectAssignments.Assignment_Id = x.Assignment_Id;
        //                objProjectAssignments.IsActive = x.IsActive;
        //                lstProjectAssignment.Add(objProjectAssignments);
        //            }


        //            viewResult = new ValidationModel()
        //            {
        //                ProjectAssignments = lstProjectAssignment
        //                //MasterLookUps = _db.MasterLookUps.Where(m => m.LookupType == "BillingStatus").ToList()
        //            };
        //        }
        //        catch (Exception ex)
        //        {

        //        }
        //    }
        //    #endregion

        //    #region ForOM
        //    else if (Role == "OM")
        //    {
        //        var _AllProjectNames = (from projects in db.Projects.
        //                                 Where(p => p.IsActive == true)
        //                                select new
        //                                {
        //                                    ProjectName = projects.ProjectCode + "-" + projects.ProjectName,
        //                                    ProjectCode = projects.ProjectCode
        //                                }).OrderBy(p => p.ProjectName).ToList();
        //        if (HttpContext.Session.GetString("_ProjectNames") == null)
        //        {
        //            HttpContext.Session.SetString("_ProjectNames", _AllProjectNames.ToString());
        //        }
        //        var _Accounts = (from projects in db.Projects.
        //                                Where(p => p.IsActive == true)
        //                         select new
        //                         {
        //                             Accountname = projects.AccountName

        //                         }).Distinct().OrderBy(p => p.Accountname).ToList();
        //        if (ViewData["_Accounts"] == null)
        //        {
        //            ViewData["_Accounts"] = _Accounts;
        //        }

        //        var _projectAssignment = (from assignment in db.ProjectAssignments
        //                                  join emp in db.Employees on assignment.EmployeeId equals emp.EmployeeId
        //                                  join prjct in db.Projects on assignment.ProjectCode equals prjct.ProjectCode
        //                                  //join empDoc in _db.EmployeeDocs on emp.EmployeeId equals empDoc.Employeeid into tempEmpDoc
        //                                  //from empDoc in tempEmpDoc.DefaultIfEmpty()
        //                                  where emp.IsActive == true
        //                                  select new
        //                                  {
        //                                      assignment.EmployeeId,
        //                                      assignment.Utilization,
        //                                      emp.Practice,
        //                                      emp.AssignmentStatus,
        //                                      assignment.Assigned_By,
        //                                      assignment.ProjectCode,
        //                                      assignment.ProjectName,
        //                                      emp.FirstName,
        //                                      emp.MiddleName,
        //                                      assignment.BillingStatus,
        //                                      emp.LastName,
        //                                      assignment.StartDate,
        //                                      assignment.EndDate,
        //                                      prjct.BillingType,
        //                                      assignment.Assignment_Id,
        //                                      assignment.IsActive
        //                                  }).OrderBy(x => x.IsActive).ToList();
        //        foreach (var x in _projectAssignment)
        //        {
        //            ProjectAssignments objProjectAssignments = new ProjectAssignments();
        //            objProjectAssignments.ProjectName = x.ProjectName;
        //            objProjectAssignments.ProjectCode = x.ProjectCode;
        //            objProjectAssignments.EmployeeId = x.EmployeeId;
        //            objProjectAssignments.EmployeeName = x.FirstName + " " + x.LastName;
        //            objProjectAssignments.StartDate = x.StartDate;
        //            objProjectAssignments.EndDate = x.EndDate;
        //            objProjectAssignments.Assigned_By = x.Assigned_By;
        //            objProjectAssignments.Utilisation = x.Utilization;
        //            objProjectAssignments.AssignmentStatus = x.AssignmentStatus;
        //            objProjectAssignments.Practice = x.Practice;
        //            objProjectAssignments.BillingType = x.BillingType;
        //            objProjectAssignments.BillingStatus = x.BillingStatus;
        //            objProjectAssignments.Assignment_Id = x.Assignment_Id;
        //            objProjectAssignments.IsActive = x.IsActive;
        //            lstProjectAssignment.Add(objProjectAssignments);
        //        }

        //        viewResult = new ValidationModel()
        //        {
        //            ProjectAssignments = lstProjectAssignment
        //            //MasterLookUps = _db.MasterLookUps.Where(m => m.LookupType == "BillingStatus").ToList()
        //        };
        //    }
        //    #endregion

        //    #region ForDH
        //    else if (Role == "DH")
        //    {
        //        var _AllProjectNames = (from projects in db.Projects.
        //                                 Where(p => p.DELIVERY_MANAGER_ID == empID && p.IsActive == true)
        //                                select new
        //                                {
        //                                    ProjectName = projects.ProjectCode + "-" + projects.ProjectName,
        //                                    ProjectCode = projects.ProjectCode
        //                                }).OrderBy(p => p.ProjectName).ToList();
        //        if (HttpContext.Session.GetString("_ProjectNames") == null)
        //        {
        //            HttpContext.Session.SetString("_ProjectNames", _AllProjectNames.ToString());
        //        }
        //        var _Accounts = (from projects in db.Projects.
        //                                Where(p => p.DELIVERY_MANAGER_ID == empID && p.IsActive == true)
        //                         select new
        //                         {
        //                             Accountname = projects.AccountName

        //                         }).Distinct().OrderBy(p => p.Accountname).ToList();
        //        if (ViewData["_Accounts"] == null)
        //        {
        //            ViewData["_Accounts"] = _Accounts;
        //        }

        //        var _projectAssignment = (from assignment in db.ProjectAssignments
        //                                  join emp in db.Employees on assignment.EmployeeId equals emp.EmployeeId
        //                                  join prjct in db.Projects on assignment.ProjectCode equals prjct.ProjectCode
        //                                  //join empDoc in _db.EmployeeDocs on emp.EmployeeId equals empDoc.Employeeid into tempEmpDoc
        //                                  //from empDoc in tempEmpDoc.DefaultIfEmpty()
        //                                  where emp.IsActive == true && prjct.DELIVERY_MANAGER_ID == empID
        //                                  select new
        //                                  {
        //                                      assignment.EmployeeId,
        //                                      assignment.Utilization,
        //                                      emp.Practice,
        //                                      emp.AssignmentStatus,
        //                                      assignment.Assigned_By,
        //                                      assignment.ProjectCode,
        //                                      assignment.ProjectName,
        //                                      emp.FirstName,
        //                                      emp.MiddleName,
        //                                      assignment.BillingStatus,
        //                                      emp.LastName,
        //                                      assignment.StartDate,
        //                                      assignment.EndDate,
        //                                      prjct.BillingType,
        //                                      assignment.Assignment_Id,
        //                                      assignment.IsActive
        //                                  }).OrderBy(x => x.IsActive).ToList();
        //        foreach (var x in _projectAssignment)
        //        {
        //            ProjectAssignments objProjectAssignments = new ProjectAssignments();
        //            objProjectAssignments.ProjectName = x.ProjectName;
        //            objProjectAssignments.ProjectCode = x.ProjectCode;
        //            objProjectAssignments.EmployeeId = x.EmployeeId;
        //            objProjectAssignments.EmployeeName = x.FirstName + " " + x.LastName;
        //            objProjectAssignments.StartDate = x.StartDate;
        //            objProjectAssignments.EndDate = x.EndDate;
        //            objProjectAssignments.Assigned_By = x.Assigned_By;
        //            objProjectAssignments.Utilisation = x.Utilization;
        //            objProjectAssignments.AssignmentStatus = x.AssignmentStatus;
        //            objProjectAssignments.Practice = x.Practice;
        //            objProjectAssignments.BillingType = x.BillingType;
        //            objProjectAssignments.BillingStatus = x.BillingStatus;
        //            objProjectAssignments.Assignment_Id = x.Assignment_Id;
        //            objProjectAssignments.IsActive = x.IsActive;
        //            lstProjectAssignment.Add(objProjectAssignments);
        //        }

        //        viewResult = new ValidationModel()
        //        {
        //            ProjectAssignments = lstProjectAssignment
        //            //MasterLookUps = _db.MasterLookUps.Where(m => m.LookupType == "BillingStatus").ToList()
        //        };
        //    }
        //    #endregion

        //    return View(viewResult);
        //}

        public ActionResult AllAssignmentsUpdate(int EmployeeId = 0)
        {
           
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                try
                {
                    ValidationModel viewResult = new ValidationModel();
                    List<ProjectAssignments> lstProjectAssignment = new List<ProjectAssignments>();

                    var objHRRFDetails = (from pa in db.ProjectAssignments
                                          join emp in db.Employees on pa.EmployeeId equals emp.EmployeeId
                                          join prjct in db.Projects on pa.ProjectCode equals prjct.ProjectCode
                                          where pa.EmployeeId == EmployeeId
                                          select new
                                          {
                                              pa.EmployeeId,
                                              pa.ProjectCode,
                                              pa.ProjectName,
                                              emp.FirstName,
                                              emp.MiddleName,
                                              emp.LastName,
                                              prjct.ProjectManagerId,
                                              pa.Assigned_By,
                                              pa.Utilization,
                                              emp.Practice,
                                              pa.BillingStatus,
                                              emp.AssignmentStatus,
                                              pa.StartDate,
                                              pa.EndDate,
                                              prjct.BillingType,
                                              pa.Assignment_Id,
                                              pa.IsActive
                                          }
                                       ).ToList();
                    foreach (var x in objHRRFDetails)
                    {
                        ProjectAssignments objProjectAssignments = new ProjectAssignments();
                        objProjectAssignments.ProjectName = x.ProjectName;
                        objProjectAssignments.ProjectCode = x.ProjectCode;
                        objProjectAssignments.EmployeeId = x.EmployeeId;
                        objProjectAssignments.EmployeeName = x.FirstName + " " + x.LastName;
                        objProjectAssignments.StartDate = x.StartDate;
                        objProjectAssignments.EndDate = x.EndDate;
                        objProjectAssignments.Assigned_By = x.Assigned_By;
                        objProjectAssignments.Utilisation = x.Utilization;
                        objProjectAssignments.AssignmentStatus = x.AssignmentStatus;
                        objProjectAssignments.Practice = x.Practice;
                        objProjectAssignments.BillingType = x.BillingType;
                        objProjectAssignments.BillingStatus = x.BillingStatus;
                        objProjectAssignments.Assignment_Id = x.Assignment_Id;
                        objProjectAssignments.IsActive = x.IsActive;
                        lstProjectAssignment.Add(objProjectAssignments);
                    }

                    viewResult = new ValidationModel()
                    {
                        ProjectAssignments = lstProjectAssignment

                    };
                    GetProjects();
                    return View(viewResult);
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
        public List<SelectListItem> GetProjects()
        {
           
                var AllProjects = (from pro in db.Projects
                                   //where pro.IsActive == true
                                   where pro.IsActive == true && pro.SOWEndDate >= DateTime.Now
                                   select new SelectListItem
                                   {
                                       Value = pro.ProjectCode,
                                       Text = pro.ProjectCode + " " + pro.ProjectName
                                   }).OrderBy(o => o.Text).ToList();

                ViewBag.AllProjects = AllProjects;
                #region Bind Resource Type in dropdownlist

                List<SelectListItem> lstResourceType = new List<SelectListItem>();
                lstResourceType.Add(new SelectListItem { Value = "Billed", Text = "Billed" });
                lstResourceType.Add(new SelectListItem { Value = "Business Operations", Text = "Business Operations" });
                lstResourceType.Add(new SelectListItem { Value = "BSA", Text = "BSA" });
                lstResourceType.Add(new SelectListItem { Value = "Delivery", Text = "Delivery" });
                lstResourceType.Add(new SelectListItem { Value = "Internal", Text = "Internal" });
                lstResourceType.Add(new SelectListItem { Value = "Shadow", Text = "Shadow" });

                lstResourceType.Add(new SelectListItem { Value = "Management", Text = "Management" });
                lstResourceType.Add(new SelectListItem { Value = "Presales", Text = "Presales" });
                lstResourceType.Add(new SelectListItem { Value = "Internal Application", Text = "Internal Application" });
                lstResourceType.Add(new SelectListItem { Value = "ESS", Text = "ESS" });
                lstResourceType.Add(new SelectListItem { Value = "Account Ops", Text = "Account Ops" });
                lstResourceType.Add(new SelectListItem { Value = "Solution Development", Text = "Solution Development" });
                lstResourceType.Add(new SelectListItem { Value = "Intern", Text = "Intern" });
                lstResourceType.Add(new SelectListItem { Value = "Practice Support", Text = "Practice Support" });

                ViewBag.ResourceType = lstResourceType;
                #endregion
                #region Bind Utilization in dropdownlist
                List<SelectListItem> lstUtilization = new List<SelectListItem>();
                lstUtilization.Add(new SelectListItem { Value = "25", Text = "25" });
                lstUtilization.Add(new SelectListItem { Value = "50", Text = "50" });
                lstUtilization.Add(new SelectListItem { Value = "75", Text = "75" });
                lstUtilization.Add(new SelectListItem { Value = "100", Text = "100" });

                ViewBag.Utilization = lstUtilization;
                #endregion

                return (AllProjects);
           
        }
        public ActionResult GetEmployeeAssignDetails(int EmployeeId)
        {

            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                try
                {
                    JsonResult result = new JsonResult(null);
                    ValidationModel viewResult = new ValidationModel();
                    List<ProjectAssignments> lstProjectAssignment = new List<ProjectAssignments>();

                    var objHRRFDetails = (from pa in db.ProjectAssignments
                                          join emp in db.Employees on pa.EmployeeId equals emp.EmployeeId
                                          join prjct in db.Projects on pa.ProjectCode equals prjct.ProjectCode
                                          where pa.EmployeeId == EmployeeId
                                          select new
                                          {
                                              pa.EmployeeId,
                                              pa.ProjectCode,
                                              pa.ProjectName,
                                              emp.FirstName,
                                              emp.MiddleName,
                                              emp.LastName,
                                              prjct.ProjectManagerId,
                                              pa.Assigned_By,
                                              pa.Utilization,
                                              emp.Practice,
                                              pa.BillingStatus,
                                              emp.AssignmentStatus,
                                              pa.StartDate,
                                              pa.EndDate,
                                              prjct.BillingType,
                                              pa.Assignment_Id,
                                              pa.IsActive
                                          }
                                       ).ToList();
                    foreach (var x in objHRRFDetails)
                    {
                        ProjectAssignments objProjectAssignments = new ProjectAssignments();
                        objProjectAssignments.ProjectName = x.ProjectName;
                        objProjectAssignments.ProjectCode = x.ProjectCode;
                        objProjectAssignments.EmployeeId = x.EmployeeId;
                        objProjectAssignments.EmployeeName = x.FirstName + " " + x.LastName;
                        objProjectAssignments.StartDate = x.StartDate;
                        objProjectAssignments.EndDate = x.EndDate;
                        objProjectAssignments.Assigned_By = x.Assigned_By;
                        objProjectAssignments.Utilisation = x.Utilization;
                        objProjectAssignments.AssignmentStatus = x.AssignmentStatus;
                        objProjectAssignments.Practice = x.Practice;
                        objProjectAssignments.BillingType = x.BillingType;
                        objProjectAssignments.BillingStatus = x.BillingStatus;
                        objProjectAssignments.Assignment_Id = x.Assignment_Id;
                        objProjectAssignments.IsActive = x.IsActive;
                        lstProjectAssignment.Add(objProjectAssignments);
                    }

                    viewResult = new ValidationModel()
                    {
                        ProjectAssignments = lstProjectAssignment

                    };
                    result.Value = viewResult;
                    return PartialView("_EmployeeAssignments", viewResult);
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


        public ActionResult ProjectAutoAssignment(int EmployeeId, string Project, string StartDate,
           string EndDate, string ResourceType, string PrevProjectCode,string Utilization)
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                try
                { 

                string ermsg = "";
                string GTRNumber = "";
                bool chkforpropsed = true;
                DateTime dtStartDate = Convert.ToDateTime(StartDate);
                DateTime dtEndDate = Convert.ToDateTime(EndDate);
                if (dtStartDate >= dtEndDate)
                {
                    ermsg = "Start Date should be less than End Date";
                    return Json(ermsg);
                }
                var empAssignments = (from pa in db.ProjectAssignments
                                      join emp in db.Employees on pa.EmployeeId equals emp.EmployeeId
                                      join prjct in db.Projects on pa.ProjectCode equals prjct.ProjectCode
                                      where pa.EmployeeId == EmployeeId
                                      select new
                                      {
                                          EmpId = pa.EmployeeId,
                                          util = pa.Utilization,
                                          AssignmentID = pa.Assignment_Id

                                      }).ToList();
                List<SelectedEmployees> objSelectedEmployees = new List<SelectedEmployees>();
                foreach (var x in empAssignments)
                {

                    SelectedEmployees objProjectAssignments = new SelectedEmployees();
                    objProjectAssignments.AssignmentID = Convert.ToString(x.AssignmentID);
                    objProjectAssignments.util = Convert.ToString(x.util);
                    objProjectAssignments.EmployeeId = x.EmpId;
                    objSelectedEmployees.Add(objProjectAssignments);


                }


                foreach (var eachEmployee in objSelectedEmployees)
                {
                    Int32 EmpID = Convert.ToInt32(eachEmployee.EmployeeId);
                    Employee objEmp = db.Employees.Where(e => e.EmployeeId.Equals(EmpID)).FirstOrDefault();

                    if (objEmp != null)
                    {

                        var objpras1 = db.ProjectAssignments.Where(b => b.EmployeeId == EmpID && !(b.BillingStatus.Contains("Bench")) && ((b.StartDate >= dtStartDate && b.StartDate <= dtEndDate) || (b.EndDate >= dtStartDate && b.EndDate <= dtEndDate) || (b.StartDate <= dtStartDate && b.EndDate >= dtEndDate))).ToList();
                        if (objpras1.Count() > 0)
                        {
                            var sumUtilization = objpras1.Sum(p => p.Utilization);
                            if ((sumUtilization + Convert.ToInt32(Utilization)) > 100)
                            {
                                chkforpropsed = false;
                                ermsg = objEmp.EmployeeId + " already assigned to some other project with selected dates";
                                break;
                            }
                        }

                        var objpras = db.ProjectAssignments.Where(b => b.EmployeeId == EmpID && b.IsActive == true).ToList();
                        if (objpras != null)
                        {

                            int utl = 0;
                            int utli = Convert.ToInt32(Utilization);

                            foreach (var lt in objpras)
                            {
                                if (!lt.BillingStatus.ToLower().Contains("bench"))
                                {
                                    if (lt.EndDate >= DateTime.Now)
                                    {
                                        utl = utl + Convert.ToInt32(lt.Utilization);
                                        int mnu = 100 - utl;
                                        int chkutl = utl + utli;
                                        if (chkutl > 100)
                                        {
                                            chkforpropsed = false;
                                            ermsg = objEmp.EmployeeId + " Working on another projects with " + utl + "% utilization. so we can utilize " + mnu + "% only";
                                            break;
                                        }
                                    }



                                }
                            }
                        }

                    }

                }
                if (chkforpropsed)
                {
                    foreach (var eachEmployee in objSelectedEmployees)
                    {
                        string HRRFNumber = "";
                        Int32 EmpID = Convert.ToInt32(eachEmployee.EmployeeId);
                        Employee objEmp = db.Employees.Where(e => e.EmployeeId.Equals(EmpID)).FirstOrDefault();
                        int utilza = Convert.ToInt32(Utilization);

                        if (objEmp != null)
                        {
                            var objpras1 = db.ProjectAssignments.Where(b => b.EmployeeId == EmpID && !(b.BillingStatus.Contains("Bench")) && ((b.StartDate >= dtStartDate && b.StartDate <= dtEndDate) || (b.EndDate >= dtStartDate && b.EndDate <= dtEndDate) || (b.StartDate <= dtStartDate && b.EndDate >= dtEndDate))).ToList();
                            if (objpras1.Count() > 0)
                            {
                                var sumUtilization = objpras1.Sum(p => p.Utilization);
                                if ((sumUtilization + Convert.ToInt32(Utilization)) > 100)
                                {
                                    chkforpropsed = false;
                                    ermsg = objEmp.EmployeeId + " already assigned to some other project with selected dates";
                                    break;
                                }
                            }

                            #region Update in ProjectAssignment 
                            int assignmenid = Convert.ToInt32(eachEmployee.AssignmentID);

                            List<ProjectAssignment> objPrjAssgnt = db.ProjectAssignments.Where(pa => pa.ProjectCode.Equals(PrevProjectCode)
                            && pa.EmployeeId == EmpID && pa.Assignment_Id == assignmenid).ToList();
                            foreach (ProjectAssignment proAssgnt in objPrjAssgnt)
                            {


                                ProjectAssignmenthistory pash1 = new ProjectAssignmenthistory();
                                pash1.AssignmentId = proAssgnt.Assignment_Id;
                                pash1.ProjectCode = proAssgnt.ProjectCode;
                                pash1.ProjectName = proAssgnt.ProjectName;
                                pash1.ProjectID = proAssgnt.ProjectID;
                                pash1.Assigned_ByOld = proAssgnt.Assigned_By;
                                pash1.BillingStatusOld = proAssgnt.BillingStatus;
                                pash1.EmployeeId = proAssgnt.EmployeeId;
                                pash1.EnddateOld = proAssgnt.EndDate;
                                pash1.IsActiveOld = proAssgnt.IsActive;
                                pash1.modifiedBy = EmpID;
                                pash1.ModifiedDate = DateTime.Now;
                                pash1.StartDateOld = proAssgnt.StartDate;
                                pash1.UtilizationOld = proAssgnt.Utilization;
                                pash1.UtilizationNew = proAssgnt.Utilization;
                                pash1.StartDateNew = proAssgnt.StartDate;
                                if (DateTime.Compare(DateTime.Now, Convert.ToDateTime(StartDate)) == 0)
                                {
                                    pash1.IsActiveNew = false;
                                }
                                else
                                {
                                    pash1.IsActiveNew = true;
                                }
                                pash1.EndDateNew = proAssgnt.EndDate;
                                pash1.BillingStatusNew = proAssgnt.BillingStatus;
                                pash1.Assigned_byNew = proAssgnt.Assigned_By;
                                db.ProjectAssignmenthistories.Add(pash1);

                                if (DateTime.Compare(DateTime.Now, Convert.ToDateTime(StartDate)) < 0)
                                {
                                    proAssgnt.IsActive = true;
                                    var dtProjectAssignEndDate = db.ProjectAssignments.Where(pa => pa.EmployeeId == EmpID && DateTime.Now <= pa.StartDate).OrderBy(pa => pa.StartDate).Select(pa => pa.StartDate).FirstOrDefault();
                                    if (dtProjectAssignEndDate != null && dtProjectAssignEndDate <= dtStartDate)
                                    {
                                        proAssgnt.EndDate = Convert.ToDateTime(dtProjectAssignEndDate).AddDays(-1).Date;
                                    }
                                    else
                                    {
                                        proAssgnt.EndDate = Convert.ToDateTime(StartDate).AddDays(-1).Date;
                                    }
                                }
                                else
                                {
                                    proAssgnt.IsActive = false;
                                    proAssgnt.EndDate = DateTime.Now;
                                }


                                proAssgnt.Assigned_Date = System.DateTime.Now;
                                db.Entry(proAssgnt).State = Microsoft.EntityFrameworkCore.EntityState.Modified;

                            }

                            #endregion

                            #region Project Assignment Creation

                            ProjectAssignment objProjectAssignment = new ProjectAssignment();
                            objProjectAssignment.ProjectName = db.Projects.Where(p => p.ProjectCode == Project).FirstOrDefault().ProjectName;
                            objProjectAssignment.ProjectID = db.Projects.Where(p => p.ProjectCode.Equals(Project)).FirstOrDefault().ProjectId;
                            objProjectAssignment.EmployeeId = objEmp.EmployeeId;
                            objProjectAssignment.StartDate = Convert.ToDateTime(StartDate);
                            objProjectAssignment.EndDate = Convert.ToDateTime(EndDate);
                            objProjectAssignment.Utilization = Convert.ToInt32(Utilization);
                            if (DateTime.Compare(DateTime.Now, Convert.ToDateTime(StartDate)) == 0)
                            {
                                objProjectAssignment.IsActive = true;
                            }
                            else
                            {
                                objProjectAssignment.IsActive = false;
                            }
                            objProjectAssignment.Assigned_By = db.Projects.Where(p => p.ProjectCode.Equals(Project)).FirstOrDefault().ProjectManager;
                            objProjectAssignment.Assigned_Date = DateTime.Now;
                            objProjectAssignment.ProjectCode = Project;
                            objProjectAssignment.BillingStatus = ResourceType;

                            db.ProjectAssignments.Add(objProjectAssignment);

                            #region assignmenthistroy
                            ProjectAssignmenthistory pash = new ProjectAssignmenthistory();
                            pash.AssignmentId = 0;
                            pash.ProjectCode = Project;
                            pash.ProjectName = db.Projects.Where(p => p.ProjectCode == Project).FirstOrDefault().ProjectName;
                            pash.ProjectID = db.Projects.Where(p => p.ProjectCode.Equals(Project)).FirstOrDefault().ProjectId;
                            pash.EmployeeId = objEmp.EmployeeId;
                            pash.Assigned_ByOld = null;
                            pash.BillingStatusOld = null;
                            pash.EnddateOld = null;
                            pash.IsActiveOld = null;
                            pash.StartDateOld = null;
                            pash.UtilizationOld = null;
                            pash.modifiedBy = EmpID;
                            pash.ModifiedDate = DateTime.Now;
                            pash.UtilizationNew = Convert.ToInt32(Utilization);
                            pash.StartDateNew = Convert.ToDateTime(StartDate);
                            if (DateTime.Compare(DateTime.Now, Convert.ToDateTime(StartDate)) == 0)
                            {
                                pash.IsActiveNew = true;
                            }
                            else
                            {
                                pash.IsActiveNew = false;
                            }
                            pash.EndDateNew = Convert.ToDateTime(EndDate);
                            pash.BillingStatusNew = ResourceType;
                            pash.Assigned_byNew = db.Projects.Where(p => p.ProjectCode.Equals(Project)).FirstOrDefault().ProjectManager;
                            db.ProjectAssignmenthistories.Add(pash);
                            #endregion

                            var notbenchprojects = db.ProjectAssignments.Where(p => p.IsActive == true && p.EmployeeId == objEmp.EmployeeId).ToList();
                            if (notbenchprojects != null)
                            {
                                int? preutil = 0;
                                foreach (var lt in notbenchprojects)
                                {
                                    if (!(lt.BillingStatus.ToLower().Contains("bench")))
                                    {
                                        preutil = preutil + lt.Utilization;
                                    }
                                }
                                if (preutil + Convert.ToInt32(Utilization) < 100)
                                {
                                    int? penutil = 100 - (preutil + Convert.ToInt32(Utilization));
                                    if (penutil > 0)
                                    {

                                        #region becnhfor remaing util

                                        var unass = (from bpr in db.PracticeWiseBenchCodes
                                                     join pt in db.Projects on bpr.BenchCode equals pt.ProjectCode
                                                     join emp in db.Employees on pt.ProjectManagerId equals emp.EmployeeId
                                                     where bpr.Practice.ToLower().Equals(objEmp.Practice.ToLower())
                                                     && bpr.CostCenter.ToLower().Contains(objEmp.CostCenter.ToLower())
                                                     select new
                                                     {
                                                         Projectcode = pt.ProjectCode,
                                                         ProjectName = pt.ProjectName,
                                                         projectid = pt.ProjectId,
                                                         projmanger = emp.LastName + "," + emp.FirstName + " " + emp.MiddleName,
                                                         Billingstatus = bpr.BillingStatus
                                                     }).Distinct().ToList();

                                        if (unass.Count > 0)
                                        {
                                            ProjectAssignment prj = new ProjectAssignment();
                                            prj.EmployeeId = objEmp.EmployeeId;
                                            prj.ProjectID = unass[0].projectid;
                                            prj.ProjectCode = unass[0].Projectcode;
                                            prj.ProjectName = unass[0].ProjectName;
                                            prj.StartDate = Convert.ToDateTime(StartDate);
                                            prj.EndDate = Convert.ToDateTime(EndDate);
                                            if (DateTime.Compare(DateTime.Now, Convert.ToDateTime(StartDate)) == 0)
                                            {
                                                prj.IsActive = true;
                                            }
                                            else
                                            {
                                                prj.IsActive = false;
                                            }
                                            prj.Assigned_By = unass[0].projmanger;
                                            prj.Assigned_Date = DateTime.Now;
                                            prj.BillingStatus = unass[0].Billingstatus;
                                            prj.Utilization = penutil;
                                            db.ProjectAssignments.Add(prj);


                                            #region assignmenthistory
                                            ProjectAssignmenthistory pash1 = new ProjectAssignmenthistory();
                                            pash1.AssignmentId = 0;
                                            pash1.ProjectCode = unass[0].Projectcode;
                                            pash1.ProjectName = unass[0].ProjectName;
                                            pash1.ProjectID = unass[0].projectid;
                                            pash1.Assigned_ByOld = null;
                                            pash1.BillingStatusOld = null;
                                            pash1.EmployeeId = objEmp.EmployeeId;
                                            pash1.EnddateOld = null;
                                            pash1.IsActiveOld = null;
                                            pash1.StartDateOld = null;
                                            pash1.UtilizationOld = null;
                                            pash1.modifiedBy = EmpID;
                                            pash1.ModifiedDate = DateTime.Now;
                                            pash1.UtilizationNew = penutil;
                                            pash1.StartDateNew = Convert.ToDateTime(StartDate);
                                            if (DateTime.Compare(DateTime.Now, Convert.ToDateTime(StartDate)) == 0)
                                            {
                                                pash1.IsActiveNew = true;
                                            }
                                            else
                                            {
                                                pash1.IsActiveNew = false;
                                            }
                                            pash1.EndDateNew = Convert.ToDateTime(EndDate);
                                            pash1.BillingStatusNew = unass[0].Billingstatus;
                                            pash1.Assigned_byNew = unass[0].projmanger;
                                            db.ProjectAssignmenthistories.Add(pash1);
                                            #endregion
                                        }




                                        #endregion


                                    }
                                }
                            }
                            #endregion

                            db.SaveChanges();
                            if (HRRFNumber != null && HRRFNumber != string.Empty)
                            {
                                if (GTRNumber == string.Empty)
                                    GTRNumber = HRRFNumber;
                                else
                                    GTRNumber = GTRNumber + "," + HRRFNumber;
                            }




                        }
                    }
                    ermsg = "Project auto assignment done for selected resources " + GTRNumber + ".";    //observation
                }



                return Json(ermsg);
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
                //return Json("Sessionexpired");
                return RedirectToAction("SessionExpire", "Signout");
            }
        }


        public ActionResult UpdateEndStartDates(int AssignmentId,DateTime ExpectedEndDate,DateTime StartDate,int Utilization)
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                try
                { 
                JsonResult result = new JsonResult(null);
                var updatePA = new ProjectAssignment();
                updatePA = (from pr in db.ProjectAssignments
                            where pr.Assignment_Id == AssignmentId
                            select pr).FirstOrDefault();
                Employee objEmp = db.Employees.Where(e => e.EmployeeId.Equals(updatePA.EmployeeId)).FirstOrDefault();
                int utilza = Convert.ToInt32(Utilization);
                string ermsg = string.Empty;
                if (objEmp != null)
                {
                    var objpras1 = db.ProjectAssignments.Where(b => b.EmployeeId == updatePA.EmployeeId && !(b.BillingStatus.Contains("Bench")) && ((b.StartDate >= StartDate && b.StartDate <= ExpectedEndDate) || (b.EndDate >= StartDate && b.EndDate <= ExpectedEndDate) || (b.StartDate <= StartDate && b.EndDate >= ExpectedEndDate))).ToList();
                    if (objpras1.Count() > 0)
                    {
                        var sumUtilization = objpras1.Sum(p => p.Utilization);
                        if ((sumUtilization + Convert.ToInt32(Utilization)) > 100)
                        {

                            ermsg = objEmp.EmployeeId + " already assigned to some other project with selected dates";
                            return Json(ermsg);
                        }
                    }
                }
                updatePA.EndDate = ExpectedEndDate;
                updatePA.StartDate = StartDate;
                updatePA.Utilization = Utilization;
                db.SaveChanges();
                result.Value = updatePA;
                ermsg = "Updated Succesfully!!!";
                return Json(ermsg);
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
                //return Json("Sessionexpired");
                return RedirectToAction("SessionExpire", "Signout");
            }
        }

        // GET: Employees/Details/5
        public ActionResult Details(int? id)
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                try
                {
                    if (id == null)
                    {
                        return BadRequest();
                    }
                    Employee employee = db.Employees.Find(id);
                    if (employee == null)
                    {
                        return NotFound();
                    }
                    return View(employee);
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

        // GET: Employees/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Employees/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Microsoft.AspNetCore.Mvc.Bind] Employee employee)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.Employees.Add(employee);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    Common.WriteExceptionErrorLog(ex);
                    return RedirectToAction("Error", "Error");
                    //return Json("Error");
                }
            }

            return View(employee);
        }

        // GET: Employees/Edit/5
        public ActionResult Edit(int? id)
        {
            try
            {
                if (id == null)
                {
                    return BadRequest();
                }
                Employee employee = db.Employees.Find(id);
                if (employee == null)
                {
                    return NotFound();
                }
                return View(employee);
            }
            catch (Exception ex)
            {
                Common.WriteExceptionErrorLog(ex);
                return RedirectToAction("Error", "Error");
                //return Json("Error");
            }
        }

        // POST: Employees/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Microsoft.AspNetCore.Mvc.Bind] Employee employee)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    //string CreatingUser = User.Identity.Name.Split('\\')[1].ToUpper();
                    string usermail = Common.GetAzureLoggedInUserID();
                    var empId = (from r in db.Employees
                                 where r.Email == usermail && r.IsActive == true
                                 select r.EmployeeId).FirstOrDefault();
                    var row = db.Employees.Where(r => r.EmployeeId == employee.EmployeeId).FirstOrDefault();

                    var _supervior = row.SupervisorId;
                    var _Pmanager = row.ProjectManagerId;
                    row.ModifiedBy = empId.ToString();
                    row.ModifiedDate = DateTime.Now;
                    //row.Practice = roleMaster.Practice;
                    //row.Role = roleMaster.Role;
                    row.SupervisorId = employee.SupervisorId;
                    row.ProjectManagerId = employee.ProjectManagerId;
                    db.Entry(row).State = EntityState.Modified;
                    db.SaveChanges();

                    //notification
                    //check supervisor id
                    if (_supervior != employee.SupervisorId)
                        InsertNotification(empId.ToString(), Convert.ToString(row.EmployeeId), db.ApplicationSettings.Where(a => a.ApplicationCode == "ADMIN" && a.KeyName == "Supervisor_Change").Select(s => s.Description).FirstOrDefault(), "ADMIN", _supervior, employee.SupervisorId);
                    //check projmanagerid
                    if (_Pmanager != employee.ProjectManagerId)
                        InsertNotification(empId.ToString(), Convert.ToString(row.EmployeeId), db.ApplicationSettings.Where(a => a.ApplicationCode == "ADMIN" && a.KeyName == "ProjectManager_Change").Select(s => s.Description).FirstOrDefault(), "ADMIN", _Pmanager, employee.ProjectManagerId);


                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    Common.WriteExceptionErrorLog(ex);
                    return RedirectToAction("Error", "Error");
                    //return Json("Error");
                }
            }
            return View(employee);
        }
        private void InsertNotification(string From, string To, string NotificationMsg, string NOtificationType, int? fromMnagaer, int? toManager)
        {
            Notification tblNotification = new Notification();
            tblNotification.NotificationType = NOtificationType;
            tblNotification.NotificationDate = System.DateTime.Now;
            tblNotification.NotificationFrom = Convert.ToInt32(From);
            tblNotification.NotificationTo = Convert.ToInt32(To);
            var Body = NotificationMsg + db.Employees.Where(e => e.EmployeeId == toManager).Select(e => e.FirstName + " " + e.LastName).FirstOrDefault();
            tblNotification.IsActive = true;
            tblNotification.AssetID = "";
            tblNotification.ApplicationCode = db.Applications.Where(s => s.ApplicationCode == "ADMIN").Select(s => s.ApplicationCode).FirstOrDefault();


            string body = string.Empty;
            using (StreamReader reader = new StreamReader(Path.Combine(Env.ContentRootPath, "Views/Shared/EmailTemplate.html")))
            {
                body = reader.ReadToEnd();
            }
            body = body.Replace("{ToUserName}", db.Employees.Where(e => e.EmployeeId == tblNotification.NotificationTo).Select(s => s.FirstName + " " + s.LastName).FirstOrDefault());
            body = body.Replace("{FromUserName}", db.Employees.Where(e => e.EmployeeId == tblNotification.NotificationFrom).Select(s => s.FirstName + " " + s.LastName).FirstOrDefault());
            body = body.Replace("{Description}", Body);

            tblNotification.NotificationMessage = body;


            string IsEmailSent = System.Configuration.ConfigurationManager.AppSettings["IsEmailSent"].ToString();
            bool IsEmail = Convert.ToBoolean(IsEmailSent);
            if (IsEmail == true)
            {
                db.Notifications.Add(tblNotification);

            }
            db.SaveChanges();
        }
        // GET: Employees/Delete/5
        public ActionResult Delete(int? id)
        {
            try
            {
                if (id == null)
                {
                    return BadRequest();
                }
                Employee employee = db.Employees.Find(id);
                if (employee == null)
                {
                    return NotFound();
                }
                return View(employee);
            }
            catch (Exception ex)
            {
                Common.WriteExceptionErrorLog(ex);
                return RedirectToAction("Error", "Error");
                //return Json("Error");
            }
        }

        // POST: Employees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                Employee employee = db.Employees.Find(id);
                db.Employees.Remove(employee);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Common.WriteExceptionErrorLog(ex);
                return RedirectToAction("Error", "Error");
                //return Json("Error");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public JsonResult GetAllSearchProducts(string name = "")
        {
            try
            {
                //get all products

                var EmpDetails = from e in db.Employees select e;
                var EmployeeDetails = from e in db.Employees select e;

                foreach (var role in EmpDetails)
                {

                    Employee objemployee = new Employee();
                    objemployee.EmployeeId = role.EmployeeId;
                    objemployee.FirstName = role.FirstName;
                    objemployee.MiddleName = role.MiddleName;
                    objemployee.LastName = role.LastName;
                    objemployee.UserName = role.UserName;
                    objemployee.Email = role.Email;
                    objemployee.Gender = role.Gender;
                    objemployee.Grade = role.Grade;
                    objemployee.LocationType = role.LocationType;
                    objemployee.Location = role.Location;
                    objemployee.EmployeeType = role.EmployeeType;
                    objemployee.Designation = role.Designation;
                    objemployee.DateOfBirth = role.DateOfBirth;
                    objemployee.DateOfJoin = role.DateOfJoin;
                    objemployee.PreviousExperience = role.PreviousExperience;
                    objemployee.PassportNumber = role.PassportNumber;
                    objemployee.Visa = role.Visa;
                    objemployee.BusinessGroup = role.BusinessGroup;
                    objemployee.CostCenter = role.CostCenter;
                    objemployee.Practice = role.Practice;
                    objemployee.MasterSkillSet = role.MasterSkillSet;
                    objemployee.PrimarySkills = role.PrimarySkills;
                    objemployee.SecondarySkills = role.SecondarySkills;
                    objemployee.ResourceStatus = role.ResourceStatus;
                    objemployee.ReservationStatus = role.ReservationStatus;
                    objemployee.AssignmentStatus = role.AssignmentStatus;
                    objemployee.SupervisorId = role.SupervisorId;
                    objemployee.ProjectManagerId = role.ProjectManagerId;
                    objemployee.Utilization = role.Utilization;
                    objemployee.IsServicingNoticePeriod = role.IsServicingNoticePeriod;
                    objemployee.IsTravelReady = role.IsTravelReady;
                    objemployee.IsActive = role.IsActive;
                    objemployee.RelievingDate = role.RelievingDate;
                    objemployee.CreatedDate = role.CreatedDate;
                    objemployee.ModifiedDate = role.ModifiedDate;
                    objemployee.CreatedBy = role.CreatedBy;
                    objemployee.ModifiedBy = role.ModifiedBy;
                    objlst.Add(objemployee);
                }
                objlst = objlst.Where(s => s.UserName.StartsWith(name.ToUpper())).ToList();

                return Json(objlst);
            }
            catch (Exception ex)
            {
                Common.WriteExceptionErrorLog(ex);
                //return RedirectToAction("Error", "Error");
                return Json("Error");
            }
        }

        public ActionResult EditMyAddress(int id = 0)
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                try
                {
                    EmployeeAddressInformation objEmployeeAddressInfo = db.EmployeeAddressInformations.Where(eai => eai.EmployeeId.Equals(id)).FirstOrDefault();
                    if (objEmployeeAddressInfo == null)
                    {
                        Employee objEmp = db.Employees.Find(id);

                        if (objEmp != null)
                        {
                            EmployeeAddressInformation objEAI = new EmployeeAddressInformation();
                            objEAI.EmployeeId = id;
                            objEAI.EmployeeName = (!string.IsNullOrWhiteSpace(objEmp.MiddleName.Trim())) ? objEmp.FirstName + " " + objEmp.MiddleName + " " + objEmp.LastName : objEmp.FirstName + " " + objEmp.LastName;

                            db.EmployeeAddressInformations.Add(objEAI);
                            db.SaveChanges();

                            objEmployeeAddressInfo = db.EmployeeAddressInformations.Where(eai => eai.EmployeeId.Equals(id)).FirstOrDefault();
                        }
                        else
                        {
                            return NotFound();
                        }
                    }
                    return View(objEmployeeAddressInfo);
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
        [ValidateAntiForgeryToken]
        public ActionResult EditMyAddress(IFormCollection form)
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                try
                {
                    int EmpAddressInfoID = Convert.ToInt32(form.ElementAt(1).Value);

                    //Update employee address details.
                    EmployeeAddressInformation objEAI = db.EmployeeAddressInformations.Find(EmpAddressInfoID);

                    //objEAI.EmpAddressInfoID = form[1] --> pre-defined
                    //objEAI.EmployeeId = form[2] --> pre-defined
                    objEAI.CurrentLocation = form.ElementAt(3).Value;
                    //objEAI.EmployeeName = form[4] --> EmployeeName
                    objEAI.BaseLocation = form.ElementAt(5).Value;
                    objEAI.CubicleNumber = form.ElementAt(6).Value;
                    objEAI.PrimaryMobileNumber = form.ElementAt(7).Value;
                    objEAI.SecondaryMobileNumber = form.ElementAt(8).Value;
                    objEAI.EmergencyContactPersonName = form.ElementAt(9).Value;
                    objEAI.EmergencyContactPersonRelationship = form.ElementAt(10).Value;
                    objEAI.EmergencyContactPersonMobileNumber = form.ElementAt(11).Value;

                    objEAI.CurrentAddressDoorNo = form.ElementAt(12).Value;
                    objEAI.CurrentAddressStreet = form.ElementAt(13).Value;
                    objEAI.CurrentAddressArea = form.ElementAt(14).Value;
                    objEAI.CurrentAddressCity = form.ElementAt(15).Value;
                    objEAI.CurrentAddressState = form.ElementAt(16).Value;
                    objEAI.CurrentAddressCountry = form.ElementAt(17).Value;
                    objEAI.CurrentAddressPostalCode = form.ElementAt(18).Value;

                    objEAI.PermanentAddressDoorNo = form.ElementAt(19).Value;
                    objEAI.PermanentAddressStreet = form.ElementAt(20).Value;
                    objEAI.PermanentAddressArea = form.ElementAt(21).Value;
                    objEAI.PermanentAddressCity = form.ElementAt(22).Value;
                    objEAI.PermanentAddressState = form.ElementAt(23).Value;
                    objEAI.PermanentAddressCountry = form.ElementAt(24).Value;
                    objEAI.PermanentAddressPostalCode = form.ElementAt(25).Value;

                    db.Entry(objEAI).State = EntityState.Modified;

                    //Update base location of emp parent table called 'Emmployee' table.
                    Employee objEmp = db.Employees.Find(objEAI.EmployeeId);
                    objEmp.Location = objEAI.BaseLocation;

                    db.Entry(objEmp).State = EntityState.Modified;

                    db.SaveChanges();

                    return Json("Success");
                }
                catch (Exception ex)
                {
                    Common.WriteExceptionErrorLog(ex);
                    //return RedirectToAction("Error", "Error");
                    return Json("Error");
                }
            }
            else
            {
                //ermsg = "Session expired";
                //return Json("Sessionexpired");
                return RedirectToAction("SessionExpire", "Signout");
            }
        }


        public ActionResult SkillsReport()
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                try
                {
                    List<EmployeeSkillsReport> lstEmployeeSkillsReport = new List<EmployeeSkillsReport>();
                    lstEmployeeSkillsReport = db.Set<EmployeeSkillsReport>().FromSqlRaw("exec GetSkillsReport").ToList();
                    //if (lstEmployeeSkillsReport[].EmployeeID == 107881)
                    //{


                    //}
                    return View(lstEmployeeSkillsReport);
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

    

    public ActionResult GenerateEmployeeSkillReport()
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                try
                {
                    List<EmployeeSkillsReport> lstEmpSkills = db.Set<EmployeeSkillsReport>().FromSqlRaw("exec GetSkillsReport").ToList();
                    using (ExcelPackage package = new ExcelPackage())
            {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Employee Skills Information");
                        worksheet.TabColor = System.Drawing.Color.Green;
                        worksheet.DefaultRowHeight = 18f;
                        worksheet.Row(1).Height = 20f;

                        using (var range = worksheet.Cells[1, 1, 1, 18])//21
{
                        range.Style.Font.Bold = true;
                            range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                            range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#5B9BD5"));
                            range.Style.Font.Color.SetColor(System.Drawing.Color.White);

                            range.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                            range.Style.Border.Right.Color.SetColor(System.Drawing.Color.LightGray);
                        }

                        //Add the headers
                        worksheet.Cells[1, 1].Value = "SNO";
                        worksheet.Cells[1, 2].Value = "Employee ID";
                        worksheet.Cells[1, 3].Value = "Employee";
                        worksheet.Cells[1, 4].Value = "ParentOrganization";
                        worksheet.Cells[1, 5].Value = "Grade";
                        worksheet.Cells[1, 6].Value = "Location";
                        worksheet.Cells[1, 7].Value = "Project Code";
                        worksheet.Cells[1, 8].Value = "Project Name";
                        worksheet.Cells[1, 9].Value = "Billing Status";
                        worksheet.Cells[1, 10].Value = "Supervisor";
                        worksheet.Cells[1, 11].Value = "Skill Status";
                        worksheet.Cells[1, 12].Value = "Resume Uploaded";
                        worksheet.Cells[1, 13].Value = "Competency";
                        worksheet.Cells[1, 14].Value = "Skillset";
                        worksheet.Cells[1, 15].Value = "Expertise Level";
                        worksheet.Cells[1, 16].Value = "LastUsed";
                        worksheet.Cells[1, 17].Value = "Business Group";
                        worksheet.Cells[1, 18].Value = "Last Modified Date";

                        worksheet.DefaultColWidth = 18f;
                        worksheet.Column(1).Width = 5f;
                        worksheet.Column(2).Width = 12f;
                        worksheet.Column(3).AutoFit(42f);
                        worksheet.Column(10).AutoFit(42);
                        worksheet.Column(11).Width = 23f;
                        worksheet.Column(12).AutoFit(45f);
                        worksheet.Column(13).AutoFit(45f);
                        worksheet.Column(14).AutoFit(45f);
                        worksheet.Column(15).AutoFit(45f);
                        worksheet.Column(16).AutoFit(45f);
                        worksheet.Column(17).AutoFit(45f);
                        worksheet.Column(18).AutoFit(45f);

                        //Add the each row
                        for (int rowIndex = 0, row = 2; rowIndex < lstEmpSkills.Count; rowIndex++, row++) // row indicates number of rows
                        {
                            worksheet.Cells[row, 1].Value = lstEmpSkills[rowIndex].Sno;
                            worksheet.Cells[row, 2].Value = lstEmpSkills[rowIndex].EmployeeID;
                            worksheet.Cells[row, 3].Value = lstEmpSkills[rowIndex].EmployeeName;
                            worksheet.Cells[row, 4].Value = lstEmpSkills[rowIndex].ParentOrganization;
                            worksheet.Cells[row, 5].Value = lstEmpSkills[rowIndex].Grade;
                            worksheet.Cells[row, 6].Value = lstEmpSkills[rowIndex].Location;
                            worksheet.Cells[row, 7].Value = lstEmpSkills[rowIndex].ProjectCode;
                            worksheet.Cells[row, 8].Value = lstEmpSkills[rowIndex].ProjectName;
                            worksheet.Cells[row, 9].Value = lstEmpSkills[rowIndex].BillingStatus;
                            worksheet.Cells[row, 10].Value = lstEmpSkills[rowIndex].Supervisor;
                            worksheet.Cells[row, 11].Value = lstEmpSkills[rowIndex].SKillstatus;
                            worksheet.Cells[row, 12].Value = lstEmpSkills[rowIndex].ResumeUploaded;
                            worksheet.Cells[row, 13].Value = lstEmpSkills[rowIndex].Competency;
                            worksheet.Cells[row, 14].Value = lstEmpSkills[rowIndex].Skillset;
                            worksheet.Cells[row, 15].Value = lstEmpSkills[rowIndex].Expertiselevel;
                            worksheet.Cells[row, 16].Value = lstEmpSkills[rowIndex].LastUsed != null ? System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Convert.ToInt32(lstEmpSkills[rowIndex].LastUsed.Split(',')[0])) + "-" + lstEmpSkills[rowIndex].LastUsed.Split(',')[1] : "";
                            worksheet.Cells[row, 17].Value = lstEmpSkills[rowIndex].BusinessGroup;
                            worksheet.Cells[row, 18].Value = lstEmpSkills[rowIndex].LastModifiedDate;

                            if (row % 2 == 1)
                            {
                                using (var range = worksheet.Cells[row, 1, row, 18])//21
{
                                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#DDEBF7"));

                                    range.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                    range.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                    range.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                    range.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                                    range.Style.Border.Top.Color.SetColor(System.Drawing.Color.LightGray);
                                    range.Style.Border.Left.Color.SetColor(System.Drawing.Color.LightGray);
                                    range.Style.Border.Right.Color.SetColor(System.Drawing.Color.LightGray);
                                    range.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.LightGray);
                                }
                            }
                        }

                        Byte[] fileBytes = package.GetAsByteArray();
                        Response.Clear();
                        
                        Response.Headers.Add("content-disposition", "attachment;filename=EmployeeSkillsInfo" + DateTime.Now.ToString("_dd-MMM-yyyy_hh-mm-ss") + ".xlsx");

                        
                        Response.ContentType = "application/vnd.ms-excel";
                        StringWriter sw = new StringWriter();
                        Response.Body.Write(fileBytes);
                        
                    }

                    return new EmptyResult();
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
                return RedirectToAction("Sessionexpired");
               // return RedirectToAction("SessionExpire", "Signout");
            }
        }

        public ActionResult ExportReport()
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                return Json("success");
        }
            else
            {
                //ermsg = "Session expired";
                return Json("Sessionexpired");
                //return RedirectToAction("SessionExpire", "Signout");
            }
        }

        public ActionResult UpdateMySkills(int id = 0)
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                try
                {
                    Employee objEmp;
                    if (HttpContext.Session.GetString("EmployeeId").ToString() == id.ToString())
                    {
                        objEmp = db.Employees.Find(id);

                        var lstCompetency = (from skillCategory in db.MasterLookUps.Where(x => x.LookupType == "SkillCategory").OrderBy(o => o.LookupName)
                                             select new SelectListItem
                                             {
                                                 Value = skillCategory.LookupCode,
                                                 Text = skillCategory.LookupName,
                                                 //LookupName = skillCategory.LookupName,
                                                 //Description = skillCategory.Description,
                                                 //LookupCode = skillCategory.LookupCode,
                                                 //SeqNumber = skillCategory.SeqNumber
                                             }).ToList();

                        ViewData["Competency"] = lstCompetency;

                        ViewData["Skill"] = new SelectListItem { Value = "", Text = "" };


                        ViewData["ExpertiseLevel"] = "";
                    }
                    else
                    {
                        objEmp = db.Employees.Find(Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")));
                        ViewBag.PermissionDenied = "You don't have permission to view others information.";
                    }
                    if (objEmp == null)
                    {
                        return NotFound();
                    }

                    return View(objEmp);
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

        public ActionResult GenerateNewSkillReport()
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                try
                {
                    List<EmployeeSkillsReportNew> lstEmployeeSkillsReportNew = new List<EmployeeSkillsReportNew>();
                    lstEmployeeSkillsReportNew = db.Set<EmployeeSkillsReportNew>().FromSqlRaw("exec GetSkillsonExpertisebase").ToList();
                    return View(lstEmployeeSkillsReportNew);
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

        public ActionResult NewEmployeeSkillReport()
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                try
                {
                    List<EmployeeSkillsReportNew> lstEmpSkillsNew = db.Set<EmployeeSkillsReportNew>().FromSqlRaw("exec GetSkillsonExpertisebase").ToList();
                    using (ExcelPackage package = new ExcelPackage())
            {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Employee Skills Information");
                        worksheet.TabColor = System.Drawing.Color.Green;
                        worksheet.DefaultRowHeight = 18f;
                        worksheet.Row(1).Height = 20f;
                        using (var range = worksheet.Cells[1, 1, 1, 15])//21
{
                        range.Style.Font.Bold = true;
                            range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                            range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#5B9BD5"));
                            range.Style.Font.Color.SetColor(System.Drawing.Color.White);

                            range.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                            range.Style.Border.Right.Color.SetColor(System.Drawing.Color.LightGray);
                        }
                        //Add the headers
                        worksheet.Cells[1, 1].Value = "SNO";
                        worksheet.Cells[1, 2].Value = "Employee ID";
                        worksheet.Cells[1, 3].Value = "Employee";
                        worksheet.Cells[1, 4].Value = "ParentOrganization";
                        worksheet.Cells[1, 5].Value = "Grade";
                        worksheet.Cells[1, 6].Value = "Location";
                        worksheet.Cells[1, 7].Value = "Project Code";
                        worksheet.Cells[1, 8].Value = "Project Name";
                        worksheet.Cells[1, 9].Value = "Billing Status";
                        worksheet.Cells[1, 10].Value = "Supervisor";
                        worksheet.Cells[1, 11].Value = "Business Group";
                        worksheet.Cells[1, 12].Value = "Trained";
                        worksheet.Cells[1, 13].Value = "Experienced";
                        worksheet.Cells[1, 14].Value = "Proficient";
                        worksheet.Cells[1, 15].Value = "Expert";

                        worksheet.DefaultColWidth = 18f;
                        worksheet.Column(1).Width = 5f;
                        worksheet.Column(2).Width = 12f;
                        worksheet.Column(3).AutoFit(42f);
                        worksheet.Column(10).AutoFit(42);
                        worksheet.Column(11).Width = 23f;
                        worksheet.Column(12).AutoFit(45f);
                        worksheet.Column(13).AutoFit(45f);
                        worksheet.Column(14).AutoFit(45f);
                        worksheet.Column(15).AutoFit(45f);


                        //Add the each row
                        for (int rowIndex = 0, row = 2; rowIndex < lstEmpSkillsNew.Count; rowIndex++, row++) // row indicates number of rows
                        {
                            worksheet.Cells[row, 1].Value = lstEmpSkillsNew[rowIndex].Sno;
                            worksheet.Cells[row, 2].Value = lstEmpSkillsNew[rowIndex].EmployeeID;
                            worksheet.Cells[row, 3].Value = lstEmpSkillsNew[rowIndex].EmployeeName;
                            worksheet.Cells[row, 4].Value = lstEmpSkillsNew[rowIndex].ParentOrganization;
                            worksheet.Cells[row, 5].Value = lstEmpSkillsNew[rowIndex].Grade;
                            worksheet.Cells[row, 6].Value = lstEmpSkillsNew[rowIndex].Location;
                            worksheet.Cells[row, 7].Value = lstEmpSkillsNew[rowIndex].ProjectCode;
                            worksheet.Cells[row, 8].Value = lstEmpSkillsNew[rowIndex].ProjectName;
                            worksheet.Cells[row, 9].Value = lstEmpSkillsNew[rowIndex].BillingStatus;
                            worksheet.Cells[row, 10].Value = lstEmpSkillsNew[rowIndex].Supervisor;
                            worksheet.Cells[row, 11].Value = lstEmpSkillsNew[rowIndex].BusinessGroup;
                            worksheet.Cells[row, 12].Value = lstEmpSkillsNew[rowIndex].Trained;
                            worksheet.Cells[row, 13].Value = lstEmpSkillsNew[rowIndex].Experienced;
                            worksheet.Cells[row, 14].Value = lstEmpSkillsNew[rowIndex].Proficient;
                            worksheet.Cells[row, 15].Value = lstEmpSkillsNew[rowIndex].Expert;


                            if (row % 2 == 1)
                            {
                                using (var range = worksheet.Cells[row, 1, row, 15])//21
{
                                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#DDEBF7"));

                                    range.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                    range.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                    range.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                    range.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                                    range.Style.Border.Top.Color.SetColor(System.Drawing.Color.LightGray);
                                    range.Style.Border.Left.Color.SetColor(System.Drawing.Color.LightGray);
                                    range.Style.Border.Right.Color.SetColor(System.Drawing.Color.LightGray);
                                    range.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.LightGray);
                                }
                            }
                        }

                        Byte[] fileBytes = package.GetAsByteArray();
                        Response.Clear();
                        
                        Response.Headers.Add("content-disposition", "attachment;filename=EmployeeSkillsonExpetiseLevel" + DateTime.Now.ToString("_dd-MMM-yyyy_hh-mm-ss") + ".xlsx");

                        
                        Response.ContentType = "application/vnd.ms-excel";
                        StringWriter sw = new StringWriter();
                        Response.Body.Write(fileBytes);
                        
                    }

                    return new EmptyResult();
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


        public ActionResult ProjectResourceUtilizationReport()
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                try
                {
                    List<ProjectResourceUtilizationReport> lstEmployeeSkillsReport = new List<ProjectResourceUtilizationReport>();                   
                    var ProjectResourceUtilizationReport = db.sp_GetProjectAllocations(" ", " ").ToList();                   
                    ViewBag.projectList1 = ProjectResourceUtilizationReport.ToList();  //pass data from coontroller to view.
                    return View();
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


        //public ActionResult _ProjectResourceUtilizationReport(string projectcode, string startdate)
        //{
        //    if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
        //    {
        //        try
        //        {

        //            List<ProjectResourceUtilizationReport> lstEmployeeSkillsReport = new List<ProjectResourceUtilizationReport>();                   
        //            var ProjectResourceUtilizationReport = db.sp_GetProjectAllocations(projectcode, startdate).ToList();                
        //            ViewBag.projectList1 = ProjectResourceUtilizationReport.ToList();  //pass data from coontroller to view.
        //            return PartialView("_ProjectResourceUtilizationReport");
        //        }
        //        catch (Exception ex)
        //        {
        //            Common.WriteExceptionErrorLog(ex);
        //            return RedirectToAction("Error", "Error");
        //            //return Json("Error");
        //        }
        //    }
        //    else
        //    {
        //        //ermsg = "Session expired";
        //        //return Json("Sessionexpired");
        //        return RedirectToAction("SessionExpire", "Signout");
        //    }
        //}
        public ActionResult _ProjectResourceUtilizationReport(string projectcode, string startdate)
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                try
                {
                    if (startdate != null)
                    {
                        startdate = Convert.ToDateTime(startdate).ToString("MM/dd/yyyy");
                    }                   
                    List<ProjectResourceUtilizationReport> lstEmployeeSkillsReport = new List<ProjectResourceUtilizationReport>();
                    var ProjectResourceUtilizationReport = db.sp_GetProjectAllocations(projectcode, startdate).ToList();
                    ViewBag.projectList1 = ProjectResourceUtilizationReport.ToList();  //pass data from coontroller to view.
                    return PartialView("_ProjectResourceUtilizationReport");
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

        public ActionResult ProjectResourceSumUtilizationReport( string projectcode, string startdate)
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                try
                {                 
                DateTime? Fromdate = null;
    
                  if (startdate != null)
                  {
                    Fromdate = Convert.ToDateTime(startdate);
                  }       
                    List<ProjectResourceUtilizationReport> lstEmployeeSkillsReport = new List<ProjectResourceUtilizationReport>();                   
                    var ProjectResourceSumUtilizationReport = db.Set<sp_GetProjectAllocationsSum_Result>().FromSqlRaw("exec [dbo].[sp_GetProjectAllocationsSum] @ProjectCode, @StartDate",
                                new Microsoft.Data.SqlClient.SqlParameter("ProjectCode", projectcode),
                                new Microsoft.Data.SqlClient.SqlParameter("StartDate", Fromdate ?? (object)DBNull.Value)).ToList<sp_GetProjectAllocationsSum_Result>();                  
                    ViewBag.projectList = ProjectResourceSumUtilizationReport.ToList();  //pass data from coontroller to view.                   
                    return PartialView("_ProjectResourceSumUtilizationReport");
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
        
        public JsonResult ProjectInfo(String ProjectCode)
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                try
                {
                    var result = db.Projects.Where(p => p.ProjectCode == ProjectCode).Select(p => p.ProjectCode).FirstOrDefault();
                    if (result == null)
                    {
                        return Json("false");
                    }
                    else
                    {
                        return Json("true");
                    }
                   
                }
                catch (Exception ex)
                {
                    Common.WriteExceptionErrorLog(ex);
                    //return RedirectToAction("Error", "Error");
                    return Json("Error");
                }
            }
            else
            {
                //ermsg = "Session expired";
                return Json("Sessionexpired");
                //return RedirectToAction("SessionExpire", "Signout");
            }
        }

        public ActionResult BenchStatusReport(string name)
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                try
                {
                    //select* from PracticeWiseBenchCodes where BillingStatus = 'Bench' and Practice not in('GTS-Others', 'SG&A', 'Digital Studio', 'Strategy & Roadmap')                      
                    var result = (from p in db.PracticeWiseBenchCodes
                                  where p.BillingStatus == "Bench" && p.Practice != "GTS-Others" && p.Practice != "SG&A" && p.Practice != "Digital Studio" && p.Practice != "Strategy & Roadmap"
                                  select new SelectListItem
                                  {
                                      Value = p.BenchCode,
                                      Text = p.Practice

                                  }).OrderBy(o => o.Text).ToList();
                    ViewBag.ProjectCodes = result;

                    List<SelectListItem> lstQuaters = new List<SelectListItem>();
                    lstQuaters.Add(new SelectListItem { Value = "Q1", Text = "Q1" });
                    lstQuaters.Add(new SelectListItem { Value = "Q2", Text = "Q2" });
                    lstQuaters.Add(new SelectListItem { Value = "Q3", Text = "Q3" });
                    lstQuaters.Add(new SelectListItem { Value = "Q4", Text = "Q4" });
                    ViewBag.QuatersType = lstQuaters;

                    var AllOrgSubTypes = (from pro in db.OrganisationGroups

                                          select new SelectListItem
                                          {
                                              Value = pro.OrganisationSubGroup,
                                              Text = pro.OrganisationSubGroup

                                          }).Distinct().ToList();
                    ViewBag.OrgSubType = AllOrgSubTypes;

                    var BenchResourceStatusReport = db.sp_GetProjectAllocations(" ", " ").ToList();
                    ViewBag.projectList1 = BenchResourceStatusReport.ToList();  //pass data from coontroller to view.
                    if (name == null)
                    {
                        ViewBag.Message = "No";
                    }
                    else
                    {
                        ViewBag.Message = "Yes";
                    }
                    return View();
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

        public ActionResult _BenchResourceCostReport(string projectcode, string startdate, string enddate)
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                try
                {
                    DateTime? Fromdate = null;
                    DateTime? Todate = null;

                    if (startdate != null)
                    {
                        Fromdate = Convert.ToDateTime(startdate);
                    }
                    if (enddate != null)
                    {
                        Todate = Convert.ToDateTime(enddate);
                    }

                    var BenchResourceStatusReport = db.Set<BenchResourceStatusReport>().FromSqlRaw("exec [dbo].[sp_GetBenchEmployeeCost] @StartDate, @EndDate",
                                //  new Microsoft.Data.SqlClient.SqlParameter("ProjectCode", projectcode),
                                new Microsoft.Data.SqlClient.SqlParameter("StartDate", Fromdate ?? (object)DBNull.Value),
                                new Microsoft.Data.SqlClient.SqlParameter("EndDate", Todate ?? (object)DBNull.Value)).ToList<BenchResourceStatusReport>();
                    foreach (var item in BenchResourceStatusReport)
                    {
                        if (item.TOTALCOST != null)
                        {
                            //  Double dc = Math.Round((Double)item.TOTALCOST, 2);
                             var    dc = Math.Round((Double)item.TOTALCOST);
                            item.TOTALCOST = dc;
                        }
                    }
                    if (projectcode != string.Empty)
                    {
                        BenchResourceStatusReport = BenchResourceStatusReport.Where(b => b.PROJECTCODE.ToUpper() == projectcode.Trim().ToUpper()).ToList();
                    }
                    ViewBag.projectList1 = BenchResourceStatusReport.ToList();  //pass data from coontroller to view.
                    return PartialView("_BenchResourceCostReport");
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

        public ActionResult BenchTotalCostReport(string projectcode, string startdate, string enddate)
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                try
                {
                    DateTime? Fromdate = null;
                    DateTime? Todate = null;
                    if (startdate != null)
                    {
                        Fromdate = Convert.ToDateTime(startdate);
                        startdate = Convert.ToDateTime(startdate).ToString("MM/dd/yyyy");
                    }
                    if (enddate != null)
                    {
                        Todate = Convert.ToDateTime(enddate);
                        enddate = Convert.ToDateTime(enddate).ToString("MM/dd/yyyy");
                    }

                    var BenchCostReport = db.Set<BenchCostReport>().FromSqlRaw("exec [dbo].[sp_GetBenchCost] @StartDate, @EndDate",
                                //  new Microsoft.Data.SqlClient.SqlParameter("ProjectCode", projectcode),
                                new Microsoft.Data.SqlClient.SqlParameter("StartDate", Fromdate ?? (object)DBNull.Value),
                                new Microsoft.Data.SqlClient.SqlParameter("EndDate", Todate ?? (object)DBNull.Value)).ToList<BenchCostReport>();

                    //foreach (var item in BenchCostReport)
                    //{
                    //    if (item.TOTALCOST != null || item.TOTALCOST != 0)
                    //    {
                    //        Double dc = Math.Round((Double)item.TOTALCOST, 2);

                    //        item.TOTALCOST = dc;
                    //    }
                    //}
                    var BenchTotalCostReport = BenchCostReport.GroupBy(x => x.PROJECTCODE).Select(x => new BenchTotalCostReport
                    {
                        PROJECTCODE = x.Key,
                        PROJECTNAME = x.FirstOrDefault().PROJECTNAME,
                        STARTDATE = x.FirstOrDefault().STARTDATE,
                        ENDDATE = x.FirstOrDefault().ENDDATE,
                        DEPLOYABLECOUNT = x.Where(y => y.CATEGORY == "Deployable Bench").Sum(y => y.HEADCOUNT),
                        NONDEPLOYABLECOUNT = x.Where(y => y.CATEGORY == "NonDeployable Bench" || y.CATEGORY == null).Sum(y => y.HEADCOUNT),
                        RESERVEDCOUNT = x.Where(y => y.CATEGORY == "Reserved").Sum(y => y.HEADCOUNT),

                        //DEPLOYABLECOUNT = x.All(y => y.CATEGORY == "Deployable Bench").,
                        // NONDEPLOYABLECOUNT = x.Count(y => y.CATEGORY == "NonDeployable Bench"),
                        // RESERVEDCOUNT = x.Count(y => y.CATEGORY == "Reserved"),
                        //HEADCOUNT = x.ToList().Sum(y => y.HEADCOUNT),
                        HEADCOUNT = x.Sum(y => y.HEADCOUNT),
                        TOTALDAYS = x.Sum(y => y.TOTALDAYS),
                        TOTALHOURS = x.Sum(y => y.TOTALHOURS),
                        TOTALCOST = x.Sum(y => Math.Round(y.TOTALCOST))
                       // TOTALCOST = x.Sum(y => y.TOTALCOST)
                    });
                    if (projectcode != string.Empty)
                    {
                        BenchTotalCostReport = BenchTotalCostReport.Where(b => b.PROJECTCODE.ToUpper() == projectcode.Trim().ToUpper()).ToList();
                    }
                    ViewBag.projectList = BenchTotalCostReport.ToList();  //pass data from controller to view.
                    ViewBag.TotalHeadCount = BenchTotalCostReport.Sum(b => b.HEADCOUNT).ToString();
                    ViewBag.TotalBenchDays = BenchTotalCostReport.Sum(b => b.TOTALDAYS).ToString();
                    ViewBag.TotalBenchHours = BenchTotalCostReport.Sum(b => b.TOTALHOURS).ToString();
                    ViewBag.TotalBenchCost = BenchTotalCostReport.Sum(b => b.TOTALCOST).ToString();
                    return PartialView("_BenchTotalCostReport");
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

        public ActionResult GenerateBenchEmployeeCostReport(string projectcode, string startDate, string endDate)
        {
            //string projectcode= ""; string enddate = "";
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                try
                {
                    DateTime? Fromdate = null;
                    DateTime? Todate = null;
                    if (startDate != null)
                    {
                        Fromdate = Convert.ToDateTime(startDate);
                        startDate = Convert.ToDateTime(startDate).ToString("MM/dd/yyyy");
                    }
                    if (endDate != null)
                    {
                        Todate = Convert.ToDateTime(endDate);
                        endDate = Convert.ToDateTime(endDate).ToString("MM/dd/yyyy");
                    }
                    var BenchCostReport = db.Set<BenchCostReport>().FromSqlRaw("exec [dbo].[sp_GetBenchCost] @StartDate, @EndDate",
                                //  new Microsoft.Data.SqlClient.SqlParameter("ProjectCode", projectcode),
                                new Microsoft.Data.SqlClient.SqlParameter("StartDate", Fromdate ?? (object)DBNull.Value),
                                new Microsoft.Data.SqlClient.SqlParameter("EndDate", Todate ?? (object)DBNull.Value)).ToList<BenchCostReport>();
                    var BenchTotalCostReport = BenchCostReport.GroupBy(x => x.PROJECTCODE).Select(x => new BenchTotalCostReport
                    {
                        PROJECTCODE = x.Key,
                        PROJECTNAME = x.FirstOrDefault().PROJECTNAME,
                        STARTDATE = x.FirstOrDefault().STARTDATE,
                        ENDDATE = x.FirstOrDefault().ENDDATE,
                        DEPLOYABLECOUNT = x.Where(y => y.CATEGORY == "Deployable Bench").Sum(y => y.HEADCOUNT),
                        NONDEPLOYABLECOUNT = x.Where(y => y.CATEGORY == "NonDeployable Bench" || y.CATEGORY == null).Sum(y => y.HEADCOUNT),
                        RESERVEDCOUNT = x.Where(y => y.CATEGORY == "Reserved").Sum(y => y.HEADCOUNT),

                        //DEPLOYABLECOUNT = x.All(y => y.CATEGORY == "Deployable Bench").,
                        // NONDEPLOYABLECOUNT = x.Count(y => y.CATEGORY == "NonDeployable Bench"),
                        // RESERVEDCOUNT = x.Count(y => y.CATEGORY == "Reserved"),
                        //HEADCOUNT = x.ToList().Sum(y => y.HEADCOUNT),
                        HEADCOUNT = x.Sum(y => y.HEADCOUNT),
                        TOTALDAYS = x.Sum(y => y.TOTALDAYS),
                        TOTALHOURS = x.Sum(y => y.TOTALHOURS),
                        TOTALCOST = x.Sum(y => y.TOTALCOST)
                    });
                    if (projectcode != string.Empty)
                    {
                        BenchTotalCostReport = BenchTotalCostReport.Where(b => b.PROJECTCODE.ToUpper() == projectcode.Trim().ToUpper()).ToList();
                    }
                    List<BenchTotalCostReport> lstBenchTotalCost = BenchTotalCostReport.ToList();

                    var BenchResourceStatusReport = db.Set<BenchResourceStatusReport>().FromSqlRaw("exec [dbo].[sp_GetBenchEmployeeCost] @StartDate, @EndDate",
                                //  new Microsoft.Data.SqlClient.SqlParameter("ProjectCode", projectcode),
                                new Microsoft.Data.SqlClient.SqlParameter("StartDate", Fromdate ?? (object)DBNull.Value),
                                new Microsoft.Data.SqlClient.SqlParameter("EndDate", Todate ?? (object)DBNull.Value)).ToList<BenchResourceStatusReport>();
                    if (projectcode != string.Empty)
                    {
                        BenchResourceStatusReport = BenchResourceStatusReport.Where(b => b.PROJECTCODE.ToUpper() == projectcode.Trim().ToUpper()).ToList();
                    }
                    List<BenchResourceStatusReport> lstBenchResourceReport = BenchResourceStatusReport.ToList();

                    using (ExcelPackage package = new ExcelPackage())
            {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Bench Resource Summary");
                        ExcelWorksheet worksheetone = package.Workbook.Worksheets.Add("Bench Resource List");
                        worksheet.TabColor = System.Drawing.Color.Green;
                        worksheet.DefaultRowHeight = 18f;
                        worksheet.Row(1).Height = 20f;

                        List<string> lstOpsStatus = System.Configuration.ConfigurationManager.AppSettings["OpsStatus"].Split(',').ToList();

                        bool IsValid = lstOpsStatus.Where(e => HttpContext.Session.GetString("EmployeeId").ToString().Contains(e)).Any();

                        worksheetone.TabColor = System.Drawing.Color.Blue;
                        worksheetone.DefaultRowHeight = 18f;
                        worksheetone.Row(1).Height = 20f;
                        int numcloumn = 13;

                        using (var range = worksheetone.Cells[1, 1, 1, numcloumn])
{
                            range.Style.Font.Bold = true;
                            range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                            range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#5B9BD5"));
                            range.Style.Font.Color.SetColor(System.Drawing.Color.White);
                            range.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                        }
                        //Add the headers for worksheetone                       
                        worksheetone.Cells[1, 1].Value = "Employee Name";
                        worksheetone.Cells[1, 2].Value = "Employee ID";
                        worksheetone.Cells[1, 3].Value = "Grade";
                        worksheetone.Cells[1, 4].Value = "Project Code";
                        worksheetone.Cells[1, 5].Value = "Bench Category";
                        worksheetone.Cells[1, 6].Value = "Actual Start Date";
                        worksheetone.Cells[1, 7].Value = "Start Date";
                        worksheetone.Cells[1, 8].Value = "End Date";
                        worksheetone.Cells[1, 9].Value = "Current Date";
                        worksheetone.Cells[1, 10].Value = "No of Working Days";
                        worksheetone.Cells[1, 11].Value = "Utilization(%)";
                        worksheetone.Cells[1, 12].Value = "Total Hours";
                        worksheetone.Cells[1, 13].Value = "Total Cost";
                        worksheetone.DefaultColWidth = 18f;
                        //worksheetone.Column(1).Width = 20f;
                        //worksheetone.Column(2).AutoFit(20f);
                        //worksheetone.Column(3).AutoFit(30f);
                        //worksheetone.Column(4).AutoFit(30f);
                        //worksheetone.Column(5).AutoFit(30f);

                        int ExcelRow = 2;

                        for (int rowIndex = 0, row = 2; rowIndex < lstBenchResourceReport.Count; rowIndex++, row++) // row indicates number of rows
                        {
                            worksheetone.Cells[row, 1].Value = lstBenchResourceReport[rowIndex].EMPLOYEENAME;
                            worksheetone.Cells[row, 2].Value = lstBenchResourceReport[rowIndex].EMPLOYEEID;
                            worksheetone.Cells[row, 3].Value = lstBenchResourceReport[rowIndex].GRADE;
                            worksheetone.Cells[row, 4].Value = lstBenchResourceReport[rowIndex].PROJECTCODE;

                            worksheetone.Cells[row, 5].Value = lstBenchResourceReport[rowIndex].CATEGORY;
                            worksheetone.Cells[row, 6].Value = lstBenchResourceReport[rowIndex].ACTUALSTARTDATE;
                            worksheetone.Cells[row, 6].Style.Numberformat.Format = "dd-MMM-yyyy";
                            worksheetone.Cells[row, 6].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                            worksheetone.Cells[row, 7].Value = lstBenchResourceReport[rowIndex].STARTDATE;
                            worksheetone.Cells[row, 7].Style.Numberformat.Format = "dd-MMM-yyyy";
                            worksheetone.Cells[row, 7].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                            worksheetone.Cells[row, 8].Value = lstBenchResourceReport[rowIndex].ENDDATE;
                            worksheetone.Cells[row, 8].Style.Numberformat.Format = "dd-MMM-yyyy";
                            worksheetone.Cells[row, 8].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                            worksheetone.Cells[row, 9].Value = lstBenchResourceReport[rowIndex].CURRENTDATE;
                            worksheetone.Cells[row, 9].Style.Numberformat.Format = "dd-MMM-yyyy";
                            worksheetone.Cells[row, 9].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                            worksheetone.Cells[row, 10].Value = lstBenchResourceReport[rowIndex].TOTALDAYS;
                            worksheetone.Cells[row, 11].Value = lstBenchResourceReport[rowIndex].UTILIZATION;
                            worksheetone.Cells[row, 12].Value = lstBenchResourceReport[rowIndex].TOTALHOURS;
                            worksheetone.Cells[row, 13].Value = lstBenchResourceReport[rowIndex].TOTALCOST;


                            ExcelRow = ExcelRow + 1;
                        }
                        int nocloumn = 11;

                        using (var range = worksheet.Cells[1, 1, 1, nocloumn])
{
                        range.Style.Font.Bold = true;
                            range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                            range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#5B9BD5"));
                            range.Style.Font.Color.SetColor(System.Drawing.Color.White);
                            range.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                            range.Style.Border.Right.Color.SetColor(System.Drawing.Color.LightGray);
                        }
                        //Add the headers
                        worksheet.Cells[1, 1].Value = "Project Code";
                        worksheet.Cells[1, 2].Value = "Project Name";
                        worksheet.Cells[1, 3].Value = "Start Date";
                        worksheet.Cells[1, 4].Value = "End Date";
                        worksheet.Cells[1, 5].Value = "Deployable";
                        worksheet.Cells[1, 6].Value = "Non-deployable";
                        worksheet.Cells[1, 7].Value = "Reserved";
                        worksheet.Cells[1, 8].Value = "Total Head Count";
                        worksheet.Cells[1, 9].Value = "Total Work Days";
                        worksheet.Cells[1, 10].Value = "Total Hours";
                        worksheet.Cells[1, 11].Value = "Total Cost";

                        worksheet.DefaultColWidth = 18f;
                        //worksheet.Column(1).Width = 12f;
                        //worksheet.Column(2).AutoFit(20f);
                        //worksheet.Column(3).AutoFit(30f);
                        //worksheet.Column(4).AutoFit(30f);
                        //worksheet.Column(5).AutoFit(12f);
                        //worksheet.Column(6).AutoFit(15f);
                        //worksheet.Column(7).Width = 40f;
                        //worksheet.Column(8).AutoFit(15f);
                        //worksheet.Column(9).AutoFit(15f);
                        //worksheet.Column(10).AutoFit(30f);
                        //worksheet.Column(11).AutoFit(20f);

                        //Add the each row
                        ExcelRow = 2;
                        for (int rowIndex = 0, row = 2; rowIndex < lstBenchTotalCost.Count; rowIndex++, row++) // row indicates number of rows
                        {
                            worksheet.Cells[row, 1].Value = lstBenchTotalCost[rowIndex].PROJECTCODE;
                            worksheet.Cells[row, 2].Value = lstBenchTotalCost[rowIndex].PROJECTNAME;
                            worksheet.Cells[row, 3].Value = lstBenchTotalCost[rowIndex].STARTDATE;

                            worksheet.Cells[row, 3].Style.Numberformat.Format = "dd-MMM-yyyy";
                            worksheet.Cells[row, 3].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                            worksheet.Cells[row, 4].Value = lstBenchTotalCost[rowIndex].ENDDATE;
                            worksheet.Cells[row, 4].Style.Numberformat.Format = "dd-MMM-yyyy";
                            worksheet.Cells[row, 4].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                            worksheet.Cells[row, 5].Value = lstBenchTotalCost[rowIndex].DEPLOYABLECOUNT;
                            worksheet.Cells[row, 6].Value = lstBenchTotalCost[rowIndex].NONDEPLOYABLECOUNT;
                            worksheet.Cells[row, 7].Value = lstBenchTotalCost[rowIndex].RESERVEDCOUNT;
                            worksheet.Cells[row, 8].Value = lstBenchTotalCost[rowIndex].HEADCOUNT;
                            worksheet.Cells[row, 9].Value = lstBenchTotalCost[rowIndex].TOTALDAYS;
                            worksheet.Cells[row, 10].Value = lstBenchTotalCost[rowIndex].TOTALHOURS;
                            worksheet.Cells[row, 11].Value = lstBenchTotalCost[rowIndex].TOTALCOST;

                            if (row % 2 == 1)
                            {
                                using (var range = worksheet.Cells[row, 1, row, nocloumn])
{
                        range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#DDEBF7"));

                                    range.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                    range.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                    range.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                    range.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                                    range.Style.Border.Top.Color.SetColor(System.Drawing.Color.LightGray);
                                    range.Style.Border.Left.Color.SetColor(System.Drawing.Color.LightGray);
                                    range.Style.Border.Right.Color.SetColor(System.Drawing.Color.LightGray);
                                    range.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.LightGray);
                                }
                            }
                            ExcelRow = ExcelRow + 1;
                        }

                        worksheet.Cells[ExcelRow, 1].Value = "Grand Total:";
                        worksheet.Cells[ExcelRow, 2].Value = "";
                        worksheet.Cells[ExcelRow, 3].Value = "";
                        worksheet.Cells[ExcelRow, 4].Value = "";
                        worksheet.Cells[ExcelRow, 5].Value = "";
                        worksheet.Cells[ExcelRow, 6].Value = "";
                        worksheet.Cells[ExcelRow, 7].Value = "";
                        worksheet.Cells[ExcelRow, 8].Value = BenchTotalCostReport.Sum(b => b.HEADCOUNT).ToString();
                        worksheet.Cells[ExcelRow, 8].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                        worksheet.Cells[ExcelRow, 9].Value = BenchTotalCostReport.Sum(b => b.TOTALDAYS).ToString();
                        worksheet.Cells[ExcelRow, 9].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                        worksheet.Cells[ExcelRow, 10].Value = BenchTotalCostReport.Sum(b => b.TOTALHOURS).ToString();
                        worksheet.Cells[ExcelRow, 10].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                        worksheet.Cells[ExcelRow, 11].Value = BenchTotalCostReport.Sum(b => b.TOTALCOST).ToString();
                        worksheet.Cells[ExcelRow, 11].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                        Byte[] fileBytes = package.GetAsByteArray();
                        Response.Clear();
                        
                        Response.Headers.Add("content-disposition", "attachment;filename=Bench Resource Status Report " + DateTime.Now.ToString("dd MMM yyyy") + ".xlsx");
                        
                        Response.ContentType = "application/vnd.ms-excel";
                        StringWriter sw = new StringWriter();
                        Response.Body.Write(fileBytes);
                        
                    }

                    return new EmptyResult();
                }
                catch (Exception ex)
                {

                    Common.WriteExceptionErrorLog(ex);
                    return RedirectToAction("Error", "Error");
                }

            }

            else
            {
                //ermsg = "Session expired";
                //return Json("Sessionexpired");
                return RedirectToAction("SessionExpire", "Signout");
            }
        }

        public JsonResult BenchProjectInfo(String ProjectCode)
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                try
                {
                    var result = db.ProjectAssignments.Where(p => p.ProjectCode.ToUpper() == ProjectCode.Trim().ToUpper() & p.BillingStatus == "Bench").Select(p => p.ProjectCode).FirstOrDefault();
                    if (result == null)
                    {
                        return Json("false");
                    }
                    else
                    {
                        return Json("true");
                    }

                }
                catch (Exception ex)
                {
                    Common.WriteExceptionErrorLog(ex);
                    //return RedirectToAction("Error", "Error");
                    return Json("Error");
                }
            }
            else
            {
                //ermsg = "Session expired";
                return Json("Sessionexpired");
                //return RedirectToAction("SessionExpire", "Signout");
            }
        }

        public JsonResult ValidateStartAndEndDate(String StartDate, string EndDate)
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                try
                {
                    DateTime? Fromdate = null;
                    DateTime? Todate = null;
                    if (StartDate != null)
                    {
                        Fromdate = Convert.ToDateTime(StartDate);
                    }
                    if (EndDate != null)
                    {
                        Todate = Convert.ToDateTime(EndDate);
                    }

                    if (Fromdate > Todate)
                    {
                        return Json("false");
                    }
                    else
                    {
                        return Json("true");
                    }
                }
                catch (Exception ex)
                {
                    Common.WriteExceptionErrorLog(ex);
                    //return RedirectToAction("Error", "Error");
                    return Json("Error");
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
        [ValidateAntiForgeryToken]
        public ActionResult InsertCostPerHourData(IFormFile upload)
        {

            // return RedirectToAction("BenchStatusReport", new { name = "Yes"});
            if (ModelState.IsValid)
            {

                if (upload != null && upload.Length > 0)
                {
                    // ExcelDataReader works with the binary Excel file, so it needs a FileStream
                    // to get started. This is how we avoid dependencies on ACE or Interop:
                    Stream stream = upload.OpenReadStream();

                    // We return the interface, so that
                    IExcelDataReader reader = null;


                    if (upload.FileName.EndsWith(".xls"))
                    {
                        reader = ExcelReaderFactory.CreateBinaryReader(stream);
                    }
                    else if (upload.FileName.EndsWith(".xlsx"))
                    {
                        reader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                    }
                    else
                    {
                        ModelState.AddModelError("File", "This file format is not supported");
                        return View();
                    }
                    int fieldcount = reader.FieldCount;
                    int rowcount = reader.RowCount;


                    DataTable dt = new DataTable();
                    //dt.Columns.Add("UserName");
                    //dt.Columns.Add("Adddress");
                    DataRow row;


                    DataTable dt_ = new DataTable();
                    try
                    {
                        dt_ = reader.AsDataSet().Tables[0];

                        string ret = "";

                        for (int i = 0; i < dt_.Columns.Count; i++)
                        {
                            dt.Columns.Add(dt_.Rows[0][i].ToString());
                        }

                        int rowcounter = 0;
                        for (int row_ = 1; row_ < dt_.Rows.Count; row_++)
                        {
                            row = dt.NewRow();

                            for (int col = 0; col < dt_.Columns.Count; col++)
                            {
                                row[col] = dt_.Rows[row_][col].ToString();
                                rowcounter++;
                            }
                            dt.Rows.Add(row);
                        }

                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("File", "Unable to Upload file!");
                        return View();
                    }

                    DataSet result = new DataSet();//reader.AsDataSet();
                    result.Tables.Add(dt);


                    db.Database.ExecuteSqlRaw("TRUNCATE TABLE [EMPLOYEERATE]");


                    foreach (DataRow item in result.Tables[0].Rows)
                    {
                        try
                        {

                            //int employeeid = Convert.ToInt32(item["EMPLOYEEID"]);
                            //double costperhr = Convert.ToDouble(item["COSTPERHOUR"]);


                            int employeeid = Convert.ToInt32(item["Employee No"]);
                            double costperhr;
                            if (item.ItemArray[1] != "")
                            {
                                 costperhr = Convert.ToDouble(item["Cost Rate"]);
                            }
                            else
                            {

                                 costperhr = 0;
                            }
                            //if (item["Cost Rate"] != "")
                            //{
                            //    double costperhr = Convert.ToDouble(item["Cost Rate"]);
                            //}
                            //else
                            //{

                            //    double costperhr = 0;
                            //}
                            var alreadyexist = db.EMPLOYEERATEs.Where(d => d.EMPLOYEEID == employeeid).FirstOrDefault();
                            var probId = 0;
                            if (alreadyexist != null)
                            {
                                probId = alreadyexist.EMPLOYEEID;
                            }
                            else if (costperhr == 0 )
                            {
                                probId = 0;
                            }
                            else
                            {
                                EMPLOYEERATE objProb = new EMPLOYEERATE();
                                objProb.EMPLOYEEID = employeeid;
                                objProb.COSTPERHOUR = costperhr;
                                db.EMPLOYEERATEs.Add(objProb);
                                db.SaveChanges();

                            }
                        }
                        catch (Exception ex)
                        {
                            Response.WriteAsync("Error: " + ex.Message);
                        }

                    }
                    //if ((System.IO.File.Exists(pathToExcelFile)))
                    //{
                    //    System.IO.File.Delete(pathToExcelFile);
                    //}
                    reader.Close();
                    reader.Dispose();

                    return RedirectToAction("BenchStatusReport", new { name = "Yes" });

                }
                else
                {
                    ModelState.AddModelError("File", "Please Upload Your file");
                }
            }
            return View();



        }
        public class EmployeeSkillsReportNew
        {
            public string ProjectCode { get; set; }
            public int EmployeeID { get; set; }
            public long? Sno { get; set; }
            public string EmployeeName { get; set; }
            public string Practice { get; set; }
            public string ParentOrganization { get; set; }
            public int Grade { get; set; }
            public string Location { get; set; }
            public string ProjectName { get; set; }
            public string BillingStatus { get; set; }
            public string Supervisor { get; set; }
            public string BusinessGroup { get; set; }
            public string Trained { get; set; }
            public string Experienced { get; set; }
            public string Proficient { get; set; }
            public string Expert { get; set; }
        }
    }
}

