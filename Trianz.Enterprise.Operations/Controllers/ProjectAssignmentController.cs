using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using Trianz.Enterprise.Operations.Models;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Configuration;
using Trianz.Enterprise.Operations.General;

namespace Trianz.Enterprise.Operations.Controllers
{
    public class ProjectAssignmentController : Controller
    {
        TrianzOperationsEntities _db = new TrianzOperationsEntities();

        ValidationModel viewResult = new ValidationModel();
        //var projectDetails;

        // GET: ProjectAssignment

        public ActionResult Index(string ddlProject)
        {
            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            {
                try
                {

              
                //Below single statement is added by Sarath, for security reason to access RoleMaster page.
                TempData["IsRoleMasterPageAccess"] = null;

                List<ProjectAssignments> lstProjectAssignment = new List<ProjectAssignments>();
                string Role = (string)Session["Role"];  // PM, OM
                int empID = (int)Session["EmployeeId"]; //101098 Manger ID;

                #region bILLINGSTATUS  
                //dropdown data for Billing Status from master lookup



                var billingStatus = from billingstatus in _db.MasterLookUps.Where(x => x.LookupType == "BillingStatus")
                                    select new
                                    {
                                        LookupName = billingstatus.LookupName,
                                        Description = billingstatus.Description,
                                        LookupCode = billingstatus.LookupCode,
                                        SeqNumber = billingstatus.SeqNumber
                                    };
                    ViewData["_BillingStatus"] = billingStatus.ToList();
                List<SelectListItem> lstBillingStatus = new List<SelectListItem>();
                lstBillingStatus.Add(new SelectListItem { Value = "Billed", Text = "Billed" });
                lstBillingStatus.Add(new SelectListItem { Value = "Internal", Text = "Internal" });
                lstBillingStatus.Add(new SelectListItem { Value = "Shadow", Text = "Shadow" });
                lstBillingStatus.Add(new SelectListItem { Value = "Support", Text = "Support" });
                lstBillingStatus.Add(new SelectListItem { Value = "Investment", Text = "Investment" });
                lstBillingStatus.Add(new SelectListItem { Value = "Delivery", Text = "Delivery" });
                lstBillingStatus.Add(new SelectListItem { Value = "Practice Support", Text = "Practice Support" });
                lstBillingStatus.Add(new SelectListItem { Value = "Business Operations", Text = "Business Operations" });
                ///New status added as per BO requirment
                lstBillingStatus.Add(new SelectListItem { Value = "ESS", Text = "ESS" });
                lstBillingStatus.Add(new SelectListItem { Value = "Solution Development", Text = "Solution Development" });
                lstBillingStatus.Add(new SelectListItem { Value = "Bench", Text = "Bench" });
                lstBillingStatus.Add(new SelectListItem { Value = "Presales", Text = "Presales" });
                lstBillingStatus.Add(new SelectListItem { Value = "Account Ops", Text = "Account Ops" });
                lstBillingStatus.Add(new SelectListItem { Value = "Management", Text = "Management" });
                lstBillingStatus.Add(new SelectListItem { Value = "Practice Delivery", Text = "Practice Delivery" });
                lstBillingStatus.Add(new SelectListItem { Value = "Internal Application", Text = "Internal Application" });
                lstBillingStatus.Add(new SelectListItem { Value = "Interns", Text = "Interns" });

                ViewBag.BillingStatus = lstBillingStatus;

                var _Practise = (from emp in _db.PracticeWiseBenchCodes
                                 select new
                                 {
                                     Practice = emp.Practice,
                                 }).Distinct().OrderBy(p => p.Practice).ToList();


                if (ViewData["_Practice"] == null)
                {
                    ViewData["_Practice"] = _Practise;
                }


                #endregion


                #region ForPM
                if (Role == "PM")
                {
                    var employeeLog = _db.Employees.Where(e => e.EmployeeId == empID).FirstOrDefault();
                    if (employeeLog != null)
                    {
                        var _ProjectNames = (from projects in _db.Projects.
                                             //Where(p => p.ProjectManagerId == empID && p.IsActive == true)
                                             Where(p => p.ProjectManagerId == empID && p.IsActive == true && p.SOWEndDate >= DateTime.Now)
                                             select new
                                             {
                                                 ProjectName = projects.ProjectCode + "-" + projects.ProjectName,
                                                 ProjectCode = projects.ProjectCode
                                             }).OrderBy(p => p.ProjectName).ToList();
                        if (Session["_ProjectNames"] == null)
                        {
                            Session["_ProjectNames"] = _ProjectNames;
                        }
                        var _Accounts = (from projects in _db.Projects.
                                             Where(p => p.ProjectManagerId == empID && p.IsActive == true)
                                         select new
                                         {
                                             Accountname = projects.AccountName

                                         }).Distinct().OrderBy(p => p.Accountname).ToList();
                        if (ViewData["_Accounts"] == null)
                        {
                            ViewData["_Accounts"] = _Accounts;
                        }
                    }
                    try
                    {
                        if (ddlProject != null)
                        {
                            var resourceDetails = (from pa in _db.ProjectAssignments
                                                   join emp in _db.Employees on pa.EmployeeId equals emp.EmployeeId
                                                   join prjct in _db.Projects on pa.ProjectCode equals prjct.ProjectCode
                                                   //join empDoc in _db.EmployeeDocs on emp.EmployeeId equals empDoc.Employeeid into tempEmpDoc
                                                   //from empDoc in tempEmpDoc.DefaultIfEmpty()
                                                   where emp.IsActive == true
                                                   && prjct.ProjectManagerId == empID
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

                                                   }).OrderBy(x => x.IsActive).ToList();

                            foreach (var x in resourceDetails)
                            {
                                ProjectAssignments objProjectAssignments = new ProjectAssignments();
                                objProjectAssignments.ProjectName = x.ProjectName;
                                objProjectAssignments.ProjectCode = x.ProjectCode;
                                objProjectAssignments.EmployeeId = x.EmployeeId;
                                objProjectAssignments.EmployeeName = x.FirstName + " " + x.MiddleName + " " + x.LastName;
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
                        }

                        viewResult = new ValidationModel()
                        {
                            ProjectAssignments = lstProjectAssignment
                            //MasterLookUps = _db.MasterLookUps.Where(m => m.LookupType == "BillingStatus").ToList()
                        };
                    }
                    catch (Exception ex)
                    {

                    }
                }
                #endregion

                #region ForOM
                else if (Role == "OM")
                {
                    var _AllProjectNames = (from projects in _db.Projects.
                                           //  Where(p => p.IsActive == true)
                                             Where(p => p.IsActive == true && p.SOWEndDate >= DateTime.Now)
                                            select new
                                            {
                                                ProjectName = projects.ProjectCode + "-" + projects.ProjectName,
                                                ProjectCode = projects.ProjectCode
                                            }).OrderBy(p => p.ProjectName).ToList();
                    if (Session["_ProjectNames"] == null)
                    {
                        Session["_ProjectNames"] = _AllProjectNames;
                    }
                    var _Accounts = (from projects in _db.Projects.
                                            Where(p => p.IsActive == true)
                                     select new
                                     {
                                         Accountname = projects.AccountName

                                     }).Distinct().OrderBy(p => p.Accountname).ToList();
                    if (ViewData["_Accounts"] == null)
                    {
                        ViewData["_Accounts"] = _Accounts;
                    }
                    if (ddlProject != null)
                    {
                        var _projectAssignment = (from assignment in _db.ProjectAssignments
                                                  join emp in _db.Employees on assignment.EmployeeId equals emp.EmployeeId
                                                  join prjct in _db.Projects on assignment.ProjectCode equals prjct.ProjectCode
                                                  //join empDoc in _db.EmployeeDocs on emp.EmployeeId equals empDoc.Employeeid into tempEmpDoc
                                                  //from empDoc in tempEmpDoc.DefaultIfEmpty()
                                                  where emp.IsActive == true                                                
                                                  select new
                                                  {
                                                      assignment.EmployeeId,
                                                      assignment.Utilization,
                                                      emp.Practice,
                                                      emp.AssignmentStatus,
                                                      assignment.Assigned_By,
                                                      assignment.ProjectCode,
                                                      assignment.ProjectName,
                                                      emp.FirstName,
                                                      emp.MiddleName,
                                                      assignment.BillingStatus,
                                                      emp.LastName,
                                                      assignment.StartDate,
                                                      assignment.EndDate,
                                                      prjct.BillingType,
                                                      assignment.Assignment_Id,
                                                      assignment.IsActive
                                                  }).OrderBy(x => x.IsActive).ToList();
                        foreach (var x in _projectAssignment)
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
                    }
                    viewResult = new ValidationModel()
                    {
                        ProjectAssignments = lstProjectAssignment
                        //MasterLookUps = _db.MasterLookUps.Where(m => m.LookupType == "BillingStatus").ToList()
                    };
                }
                #endregion

                #region ForDH
                else if (Role == "DH")
                {
                    var _AllProjectNames = (from projects in _db.Projects.
                                           // Where(p => p.DELIVERY_MANAGER_ID == empID && p.IsActive == true)
                                            Where(p => p.DELIVERY_MANAGER_ID == empID && p.IsActive == true && p.SOWEndDate >= DateTime.Now)
                                            select new
                                            {
                                                ProjectName = projects.ProjectCode + "-" + projects.ProjectName,
                                                ProjectCode = projects.ProjectCode
                                            }).OrderBy(p => p.ProjectName).ToList();
                    if (Session["_ProjectNames"] == null)
                    {
                        Session["_ProjectNames"] = _AllProjectNames;
                    }
                    var _Accounts = (from projects in _db.Projects.
                                            Where(p => p.DELIVERY_MANAGER_ID == empID && p.IsActive == true)
                                     select new
                                     {
                                         Accountname = projects.AccountName

                                     }).Distinct().OrderBy(p => p.Accountname).ToList();
                    if (ViewData["_Accounts"] == null)
                    {
                        ViewData["_Accounts"] = _Accounts;
                    }
                    if (ddlProject != null)
                    {
                        var _projectAssignment = (from assignment in _db.ProjectAssignments
                                                  join emp in _db.Employees on assignment.EmployeeId equals emp.EmployeeId
                                                  join prjct in _db.Projects on assignment.ProjectCode equals prjct.ProjectCode
                                                  //join empDoc in _db.EmployeeDocs on emp.EmployeeId equals empDoc.Employeeid into tempEmpDoc
                                                  //from empDoc in tempEmpDoc.DefaultIfEmpty()
                                                  where emp.IsActive == true && prjct.DELIVERY_MANAGER_ID == empID
                                                  select new
                                                  {
                                                      assignment.EmployeeId,
                                                      assignment.Utilization,
                                                      emp.Practice,
                                                      emp.AssignmentStatus,
                                                      assignment.Assigned_By,
                                                      assignment.ProjectCode,
                                                      assignment.ProjectName,
                                                      emp.FirstName,
                                                      emp.MiddleName,
                                                      assignment.BillingStatus,
                                                      emp.LastName,
                                                      assignment.StartDate,
                                                      assignment.EndDate,
                                                      prjct.BillingType,
                                                      assignment.Assignment_Id,
                                                      assignment.IsActive
                                                  }).OrderBy(x => x.IsActive).ToList();
                        foreach (var x in _projectAssignment)
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
                    }
                    viewResult = new ValidationModel()
                    {
                        ProjectAssignments = lstProjectAssignment
                        //MasterLookUps = _db.MasterLookUps.Where(m => m.LookupType == "BillingStatus").ToList()
                    };
                }
                #endregion

                return View(viewResult);
                }
                catch (Exception ex)
                {
                    Common.WriteExceptionErrorLog(ex);
                    return RedirectToAction("Error", "Error");
                    //return Json("Error", JsonRequestBehavior.AllowGet);
                }
            }
        
            else
            {
                //ermsg = "Session expired";
                //return Json("Sessionexpired", JsonRequestBehavior.AllowGet);
                return RedirectToAction("SessionExpire", "Signout");
            }
        }

        public ActionResult GetAssignementinfobySearch(string ProjectCode, string Practise, string BillingStatus, string Account)
        {
            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            {
                try
                {

               
                List<ProjectAssignments> lstProjectAssignment = new List<ProjectAssignments>();
                string Role = (string)Session["Role"];  // PM, OM
                int empID = (int)Session["EmployeeId"]; //101098 Manger ID;
                var js = new ProjectAssignment();
                string code = string.Empty;
                if (ProjectCode != null && ProjectCode != string.Empty)
                {
                    code = ProjectCode;
                }

                #region ForPM
                if (Role == "PM")
                {
                    try
                    {

                        var resourceDetails = (from pa in _db.ProjectAssignments
                                               join emp in _db.Employees on pa.EmployeeId equals emp.EmployeeId
                                               join prjct in _db.Projects on pa.ProjectCode equals prjct.ProjectCode
                                               //join empDoc in _db.EmployeeDocs on emp.EmployeeId equals empDoc.Employeeid into tempEmpDoc
                                               //from empDoc in tempEmpDoc.DefaultIfEmpty()
                                               where emp.IsActive == true
                                               && prjct.ProjectManagerId == empID
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
                                                   pa.IsActive,
                                                   prjct.AccountName

                                               }).OrderBy(x => x.IsActive).ToList();
                        if (ProjectCode != null && ProjectCode != string.Empty)
                        {
                            resourceDetails = resourceDetails.Where(p => p.ProjectCode.ToLower() == ProjectCode.Trim().ToLower()).ToList();
                        }
                        if (Practise != null && Practise != string.Empty)
                        {
                            resourceDetails = resourceDetails.Where(p => p.Practice.ToLower() == Practise.Trim().ToLower()).ToList();
                        }
                        if (BillingStatus != null && BillingStatus != string.Empty)
                        {
                            resourceDetails = resourceDetails.Where(p => p.BillingStatus != null && p.BillingStatus.ToLower() == BillingStatus.Trim().ToLower()).ToList();
                        }
                        if (Account != null && Account != string.Empty)
                        {
                            resourceDetails = resourceDetails.Where(p => p.AccountName != null && p.AccountName.ToLower() == Account.Trim().ToLower()).ToList();
                        }
                        foreach (var x in resourceDetails)
                        {
                            ProjectAssignments objProjectAssignments = new ProjectAssignments();
                            objProjectAssignments.ProjectName = x.ProjectName;
                            objProjectAssignments.ProjectCode = x.ProjectCode;
                            objProjectAssignments.EmployeeId = x.EmployeeId;
                            objProjectAssignments.EmployeeName = x.FirstName + " " + x.MiddleName + " " + x.LastName;
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
                            ProjectAssignments = lstProjectAssignment,
                            MasterLookUps = _db.MasterLookUps.Where(m => m.LookupType == "BillingStatus").ToList()
                        };
                    }
                    catch (Exception ex)
                    {

                    }
                }
                #endregion

                #region ForOM
                else if (Role == "OM")
                {
                    var _projectAssignment = (from assignment in _db.ProjectAssignments
                                              join emp in _db.Employees on assignment.EmployeeId equals emp.EmployeeId
                                              join prjct in _db.Projects on assignment.ProjectCode equals prjct.ProjectCode
                                              //join empDoc in _db.EmployeeDocs on emp.EmployeeId equals empDoc.Employeeid into tempEmpDoc
                                              //from empDoc in tempEmpDoc.DefaultIfEmpty()
                                              where emp.IsActive == true
                                              //where (emp.IsActive == true
                                              //    || emp.EmployeeId == 108000
                                              //    || emp.EmployeeId == 107707
                                              //    || emp.EmployeeId == 103333
                                              //    || emp.EmployeeId == 107602
                                              //    || emp.EmployeeId == 107877
                                              //    || emp.EmployeeId == 108033
                                              //    || emp.EmployeeId == 107304)

                                              select new
                                              {
                                                  assignment.EmployeeId,
                                                  assignment.Utilization,
                                                  emp.Practice,
                                                  emp.AssignmentStatus,
                                                  assignment.Assigned_By,
                                                  assignment.ProjectCode,
                                                  assignment.ProjectName,
                                                  emp.FirstName,
                                                  emp.MiddleName,
                                                  assignment.BillingStatus,
                                                  emp.LastName,
                                                  assignment.StartDate,
                                                  assignment.EndDate,
                                                  prjct.BillingType,
                                                  assignment.Assignment_Id,
                                                  assignment.IsActive,
                                                  prjct.AccountName
                                              }).OrderBy(x => x.IsActive).ToList();

                    if (ProjectCode != null && ProjectCode != string.Empty)
                    {
                        _projectAssignment = _projectAssignment.Where(p => p.ProjectCode.ToLower() == ProjectCode.Trim().ToLower()).ToList();
                    }
                    if (Practise != null && Practise != string.Empty)
                    {
                        _projectAssignment = _projectAssignment.Where(p => p.Practice.ToLower() == Practise.Trim().ToLower()).ToList();
                    }
                    if (BillingStatus != null && BillingStatus != string.Empty)
                    {
                        _projectAssignment = _projectAssignment.Where(p => p.BillingStatus != null && p.BillingStatus.ToLower() == BillingStatus.Trim().ToLower()).ToList();
                    }
                    if (Account != null && Account != string.Empty)
                    {
                        _projectAssignment = _projectAssignment.Where(p => p.AccountName != null && p.AccountName.ToLower() == Account.Trim().ToLower()).ToList();
                    }
                    foreach (var x in _projectAssignment)
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
                        ProjectAssignments = lstProjectAssignment,
                        MasterLookUps = _db.MasterLookUps.Where(m => m.LookupType == "BillingStatus").ToList()
                    };
                }
                #endregion

                #region ForDH
                if (Role == "DH")
                {
                    try
                    {

                        var resourceDetails = (from pa in _db.ProjectAssignments
                                               join emp in _db.Employees on pa.EmployeeId equals emp.EmployeeId
                                               join prjct in _db.Projects on pa.ProjectCode equals prjct.ProjectCode
                                               //join empDoc in _db.EmployeeDocs on emp.EmployeeId equals empDoc.Employeeid into tempEmpDoc
                                               //from empDoc in tempEmpDoc.DefaultIfEmpty()
                                               where emp.IsActive == true
                                               && prjct.DELIVERY_MANAGER_ID == empID
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
                                                   pa.IsActive,
                                                   prjct.AccountName

                                               }).OrderBy(x => x.IsActive).ToList();
                        if (ProjectCode != null && ProjectCode != string.Empty)
                        {
                            resourceDetails = resourceDetails.Where(p => p.ProjectCode.ToLower() == ProjectCode.Trim().ToLower()).ToList();
                        }
                        if (Practise != null && Practise != string.Empty)
                        {
                            resourceDetails = resourceDetails.Where(p => p.Practice.ToLower() == Practise.Trim().ToLower()).ToList();
                        }
                        if (BillingStatus != null && BillingStatus != string.Empty)
                        {
                            resourceDetails = resourceDetails.Where(p => p.BillingStatus != null && p.BillingStatus.ToLower() == BillingStatus.Trim().ToLower()).ToList();
                        }
                        if (Account != null && Account != string.Empty)
                        {
                            resourceDetails = resourceDetails.Where(p => p.AccountName != null && p.AccountName.ToLower() == Account.Trim().ToLower()).ToList();
                        }
                        foreach (var x in resourceDetails)
                        {
                            ProjectAssignments objProjectAssignments = new ProjectAssignments();
                            objProjectAssignments.ProjectName = x.ProjectName;
                            objProjectAssignments.ProjectCode = x.ProjectCode;
                            objProjectAssignments.EmployeeId = x.EmployeeId;
                            objProjectAssignments.EmployeeName = x.FirstName + " " + x.MiddleName + " " + x.LastName;
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
                            ProjectAssignments = lstProjectAssignment,
                            MasterLookUps = _db.MasterLookUps.Where(m => m.LookupType == "BillingStatus").ToList()
                        };
                    }
                    catch (Exception ex)
                    {

                    }
                }
                #endregion

                return PartialView("_ProjectAssignmentPartialView", viewResult);
                }
                catch (Exception ex)
                {
                    Common.WriteExceptionErrorLog(ex);
                    return RedirectToAction("Error", "Error");
                    //return Json("Error", JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                //ermsg = "Session expired";
                return Json("Sessionexpired", JsonRequestBehavior.AllowGet);
                //return RedirectToAction("SessionExpire", "Signout");
            }

        }

        public JsonResult GetProjectSFilterAccount(string Account)
        {
            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            {
                try
                {


                    JsonResult js = new JsonResult();
                    string Role = (string)Session["Role"];  // PM, OM
                    int empID = (int)Session["EmployeeId"]; //101098 Manger ID;
                    if (Role == "OM")
                    {
                        var _AllProjectNames = (from projects in _db.Projects.
                                             Where(p => p.IsActive == true)
                                            //  Where(p => p.IsActive == true && p.SOWEndDate >= DateTime.Now)
                                                select new
                                                {
                                                    AccountName = projects.AccountName,
                                                    ProjectName = projects.ProjectCode + "-" + projects.ProjectName,
                                                    ProjectCode = projects.ProjectCode
                                                }).OrderBy(p => p.ProjectName).ToList();

                        if (Account != null && Account != string.Empty)
                            _AllProjectNames = _AllProjectNames.Where(p => p.AccountName.ToLower() == Account.ToLower()).ToList();

                        js.Data = _AllProjectNames;

                    }
                    else if (Role == "PM")
                    {
                        var _Accounts = (from projects in _db.Projects.
                                    Where(p => p.ProjectManagerId == empID && p.IsActive == true)
                                         select new
                                         {
                                             AccountName = projects.AccountName,
                                             ProjectName = projects.ProjectCode + "-" + projects.ProjectName,
                                             ProjectCode = projects.ProjectCode
                                         }).OrderBy(p => p.ProjectName).ToList();

                        if (Account != null && Account != string.Empty)
                            _Accounts = _Accounts.Where(p => p.AccountName.ToLower() == Account.ToLower()).ToList();
                        js.Data = _Accounts;


                    }
                    else if (Role == "DH")
                    {
                        var _AllProjectNames = (from projects in _db.Projects.
                                                   Where(p => p.DELIVERY_MANAGER_ID == empID && p.IsActive == true
                                                   )
                                                select new
                                                {
                                                    AccountName = projects.AccountName,
                                                    ProjectName = projects.ProjectCode + "-" + projects.ProjectName,
                                                    ProjectCode = projects.ProjectCode
                                                }).OrderBy(p => p.ProjectName).ToList();

                        if (Account != null && Account != string.Empty)
                            _AllProjectNames = _AllProjectNames.Where(p => p.AccountName.ToLower() == Account.ToLower()).ToList();

                        js.Data = _AllProjectNames;


                    }
                    js.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
                    return (js);
                }
                catch (Exception ex)
                {
                    Common.WriteExceptionErrorLog(ex);
                    //return RedirectToAction("Error", "Error");
                    return Json("Error", JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                //ermsg = "Session expired";
                return Json("Sessionexpired", JsonRequestBehavior.AllowGet);
                //return RedirectToAction("SessionExpire", "Signout");
            }
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult Autocomplete(string term)
        {

            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            {
                try
                {

                    var result = new List<KeyValuePair<string, string>>();
                    DateTime currentDate = System.DateTime.Now.Date;
                    IList<SelectListItem> List = new List<SelectListItem>();

                    var Projects = (from pro in _db.Employees
                                    where pro.IsActive == true
                                    select new SelectListItem
                                    {
                                        Text = pro.FirstName + " " + pro.MiddleName + " " + pro.LastName,
                                        Value = pro.EmployeeId + "-" + pro.FirstName + " " + pro.MiddleName + " " + pro.LastName
                                    }).Distinct().ToList();

                    foreach (var item in Projects)
                    {
                        result.Add(new KeyValuePair<string, string>(item.Value.ToString(), item.Text));
                    }
                    var result3 = result.Where(s => s.Key.ToLower().Contains
                                  (term.ToLower())).Select(w => w).ToList();
                    return Json(result3, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    Common.WriteExceptionErrorLog(ex);
                   // return RedirectToAction("Error", "Error");
                    return Json("Error", JsonRequestBehavior.AllowGet);
                }

            }
            else
            {
                //ermsg = "Session expired";
                return Json("Sessionexpired", JsonRequestBehavior.AllowGet);
                //return RedirectToAction("SessionExpire", "Signout");
            }
        }

        public JsonResult EmployeeInfo(String EmployeeID)
        {
            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            {
                try
                {
                    JsonResult js = new JsonResult();
                    int emin = 0;
                    if (EmployeeID != null && EmployeeID != string.Empty)
                    {
                        string[] em = EmployeeID.Split('-');
                        emin = Convert.ToInt32(em[0]);
                    }
                    List<EmployeeInfo> Employeinf = new List<EmployeeInfo>();
                    Employeinf = (from emp in _db.Employees
                                  where emp.IsActive == true
                                        && emp.EmployeeId == emin
                                  select new EmployeeInfo
                                  {
                                      DOJ = emp.DateOfJoin,
                                      EmailID = emp.Email,
                                      Position = emp.Designation,
                                      Grade = emp.Grade.ToString(),
                                      Location = emp.Location,
                                      Practice = emp.Practice,
                                      EmployeeCode = emp.EmployeeId,
                                      EmployeeName = emp.FirstName + " " + emp.MiddleName + " " + emp.LastName,
                                      AssignmenStatus = emp.AssignmentStatus
                                  }).ToList();


                    js.Data = Employeinf;
                    js.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
                    return js;
                }
                catch (Exception ex)
                {
                    Common.WriteExceptionErrorLog(ex);
                    //return RedirectToAction("Error", "Error");
                    return Json("Error", JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                //ermsg = "Session expired";
                return Json("Sessionexpired", JsonRequestBehavior.AllowGet);
               // return RedirectToAction("SessionExpire", "Signout");
            }
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult ProjectAutocomplete(string Project)
        {

            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            {
                try
                {
                    var result = new List<KeyValuePair<string, string>>();
                    IList<SelectListItem> List = new List<SelectListItem>();

                    var Projects = (from pro in _db.Projects
                                    where pro.IsActive == true
                                  //  where pro.IsActive == true && pro.SOWEndDate >= DateTime.Now
                                    select new SelectListItem
                                    {
                                        Text = pro.ProjectCode + "-" + pro.ProjectName,
                                        Value = pro.ProjectCode + "-" + pro.ProjectName
                                    }).Distinct().ToList();

                    foreach (var item in Projects)
                    {
                        result.Add(new KeyValuePair<string, string>(item.Value.ToString(), item.Text));
                    }
                    var result3 = result.Where(s => s.Key.ToLower().Contains
                                  (Project.ToLower())).Select(w => w).ToList();
                    return Json(result3, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    Common.WriteExceptionErrorLog(ex);
                    //return RedirectToAction("Error", "Error");
                    return Json("Error", JsonRequestBehavior.AllowGet);
                }
            }

            else
            {
                //ermsg = "Session expired";
                return Json("Sessionexpired", JsonRequestBehavior.AllowGet);
               // return RedirectToAction("SessionExpire", "Signout");
            }
        }

        public JsonResult ProjectInfo(String ProjectCode)
        {
            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            {
                try
                {
                    JsonResult js = new JsonResult();
                    string emin = "";
                    if (ProjectCode != null && ProjectCode != string.Empty)
                    {
                        string[] em = ProjectCode.Split('-');
                        emin = em[0].ToString();
                    }
                    List<ProjectInfo> Employeinf = new List<ProjectInfo>();
                    Employeinf = (from pin in _db.Projects
                                  join emp in _db.Employees on pin.ProjectManagerId equals emp.EmployeeId
                                  where emp.IsActive == true
                                        && pin.ProjectCode == emin
                                  select new ProjectInfo
                                  {
                                      projectID = pin.ProjectId,
                                      ProjectCode = pin.ProjectCode,
                                      ProjectManage = emp.LastName + "," + emp.FirstName + " " + emp.MiddleName,
                                      ProjectName = pin.ProjectName,
                                      BillingType = pin.BillingType
                                  }).ToList();


                    js.Data = Employeinf;
                    js.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
                    return js;
                }
                catch (Exception ex)
                {
                    Common.WriteExceptionErrorLog(ex);
                    //return RedirectToAction("Error", "Error");
                    return Json("Error", JsonRequestBehavior.AllowGet);
                }

            }
            else
            {
                //ermsg = "Session expired";
                return Json("Sessionexpired", JsonRequestBehavior.AllowGet);
                //return RedirectToAction("SessionExpire", "Signout");
            }
        }

        public ActionResult SaveProjectAssignment(string EmployeeId, string ProjectName, int ProjectID, string ProjectCode, string StartDate, string EndDate,
                                            string AssignmentStatus, string BillingStatus,
                                            string Utilization, string Projectmanger, string Status)
        {

            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            {
                try
                {
                    JsonResult js = new JsonResult();



                    int emplyid = 0;
                    int utli = Convert.ToInt32(Utilization);
                    if (EmployeeId != null && EmployeeId != string.Empty)
                    {

                        string[] at = EmployeeId.Split('-');
                        emplyid = Convert.ToInt32(at[0]);
                    }
                    var preprj = (from pt in _db.ProjectAssignments
                                  where pt.EmployeeId == emplyid && pt.IsActive == true
                                  select pt).ToList();
                    bool Isprj = true;
                    int utl = 0;
                    foreach (var pt in preprj)
                    {
                        if (!pt.BillingStatus.ToLower().Contains("bench"))
                        {

                            utl = utl + Convert.ToInt32(pt.Utilization);
                            int mnu = 100 - utl;
                            int chkutl = utl + utli;
                            if (chkutl > 100)
                            {
                                Isprj = false;
                                //  js.Data = "This employee working on another project with " + utl + " % utilization. so we can utilize " + mnu  + " % only";
                                js.Data = "This employee working on another project with (%) " + utl + "utilization. so we can utilize (%)" + mnu + "only";    //observation change
                                js.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
                                return js;

                            }

                        }
                    }
                    bool sta = false;
                    if (Status.ToLower() == "true")
                        sta = true;
                    // updating assignment dates and BILLING STATUS

                    IFormatProvider theCultureInfo = new System.Globalization.CultureInfo("en-us", true);
                    DateTime DstartDate = DateTime.Parse(StartDate, theCultureInfo);
                    DateTime DendDate = DateTime.Parse(EndDate, theCultureInfo);
                    if (Isprj)
                    {

                        var unass = (from pt in preprj
                                     where pt.BillingStatus.ToLower().Contains("bench")
                                     select pt
                                     ).ToList();
                        foreach (var ut in unass)
                        {
                            ut.IsActive = false;
                            ut.EndDate = DateTime.Now;
                            _db.Entry(ut).State = System.Data.Entity.EntityState.Modified;
                        }

                        ProjectAssignment prj = new ProjectAssignment();
                        prj.EmployeeId = emplyid;
                        prj.ProjectID = ProjectID;
                        prj.ProjectCode = ProjectCode.Split('-')[0];
                        prj.ProjectName = ProjectCode.Split('-')[1];
                        prj.StartDate = DstartDate;
                        prj.EndDate = DendDate;
                        prj.IsActive = sta;
                        prj.Assigned_By = Projectmanger;
                        prj.Assigned_Date = DateTime.Now;
                        prj.BillingStatus = BillingStatus;
                        prj.Utilization = utli;
                        _db.ProjectAssignments.Add(prj);

                        // updating employee billing status
                        var empStatus = (from estatus in _db.Employees
                                         where estatus.EmployeeId == emplyid &&
                                         estatus.IsActive == true
                                         select estatus).FirstOrDefault();

                        //projAssignment.BillingStatus = BillingStatus;
                        empStatus.AssignmentStatus = AssignmentStatus;

                        _db.SaveChanges();
                        js.Data = "true";
                        js.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
                    }

                    return js;
                }
                catch (Exception ex)
                {
                    Common.WriteExceptionErrorLog(ex);
                   // return RedirectToAction("Error", "Error");
                    return Json("Error", JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                //ermsg = "Session expired";
                return Json("Sessionexpired", JsonRequestBehavior.AllowGet);
                //return RedirectToAction("SessionExpire", "Signout");
            }
        }

        public ActionResult UpdateProjectAssignment(string EmployeeId, string ProjectName, int ProjectID, string ProjectCode, string StartDate, string EndDate,
                                            string AssignmentStatus, string BillingStatus, string Utilization, int AssignmentID,
                                            string Status)
        {

            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            {
                try
                {
                    JsonResult js = new JsonResult();
                    int emplyid = 0;
                    int utli = Convert.ToInt32(Utilization);

                    if (utli <= 100)
                    {
                        //if (EmployeeId != null && EmployeeId != string.Empty)
                        //{

                        string[] at = EmployeeId.Split('-');
                        emplyid = Convert.ToInt32(at[0]);
                        //}
                        var empStatus = (from estatus in _db.Employees
                                         where estatus.EmployeeId == emplyid &&
                                         estatus.IsActive == true
                                         select estatus).FirstOrDefault();

                        //DateTime dtStartDate = Convert.ToDateTime(StartDate);
                        //DateTime dtEndDate = Convert.ToDateTime(EndDate);
                        CultureInfo cultures = new CultureInfo("en-US");
                        DateTime dtStartDate = Convert.ToDateTime(StartDate, cultures);
                        DateTime dtEndDate = Convert.ToDateTime(EndDate, cultures);

                        var preprj = (from pt in _db.ProjectAssignments
                                      where pt.EmployeeId == emplyid && pt.IsActive == true && pt.Assignment_Id != AssignmentID
                                      select pt).ToList();
                        bool Isprj = true;
                        bool isprjexst = true;

                        if (preprj == null)
                            isprjexst = false;
                        //else if (Status.ToLower() != "true" && preprj.Count == 0)
                        //else if (preprj.Count == 0)
                        // isprjexst = false;
                        DateTime dts = Convert.ToDateTime(DateTime.Now.Date);
                        if (dtEndDate > dts)
                            dts = Convert.ToDateTime(DateTime.Now.Date).AddDays(1);

                        if (isprjexst)
                        {
                            var projAssignment = (from pa in _db.ProjectAssignments
                                                  where pa.EmployeeId == emplyid &&
                                                  pa.Assignment_Id == AssignmentID
                                                  select pa).FirstOrDefault();

                            var objpras1 = (from b in _db.ProjectAssignments
                                            where b.EmployeeId == emplyid && !(b.BillingStatus.Contains("Bench"))
                                             && b.Assignment_Id != AssignmentID && b.IsActive == true
                                            select b).ToList();

                            if (projAssignment.Utilization == utli && projAssignment.BillingStatus == BillingStatus)
                            {
                                objpras1 = objpras1.Where((b => (b.StartDate >= dtStartDate && b.StartDate <= dtEndDate)
                                    || (b.EndDate >= dtStartDate && b.EndDate <= dtEndDate)
                                    || (b.StartDate <= dtStartDate && b.EndDate >= dtEndDate))).ToList();
                            }
                            else
                            {
                                objpras1 = objpras1.Where((b => (b.StartDate >= dts && b.StartDate <= dtEndDate)
                                    || (b.EndDate >= dts && b.EndDate <= dtEndDate)
                                    || (b.StartDate <= dts && b.EndDate >= dtEndDate))).ToList();
                            }

                            Isprj = empStatus.DateOfJoin.Date <= dtStartDate.Date ? true : false;
                            if (Isprj)
                            {

                                if (objpras1.Count > 0)
                                {
                                    var sumUtilization = objpras1.Sum(p => p.Utilization);
                                    if ((sumUtilization + utli) > 100)
                                    {
                                        Isprj = false;
                                        js.Data = empStatus.FirstName + " already assigned to some other project with selected dates";
                                        js.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
                                        return js;
                                    }
                                }
                                //else
                                //{
                                //    foreach (var pt in preprj)
                                //    {
                                //        if (!pt.BillingStatus.ToLower().Contains("bench"))
                                //        {
                                //            if (Status.ToLower() == "true")
                                //            {
                                //                utl = utl + Convert.ToInt32(pt.Utilization);
                                //                int mnu = 100 - utl;
                                //                int chkutl = utl + utli;
                                //                if (chkutl > 100)
                                //                {
                                //                    Isprj = false;
                                //                    js.Data = "This employee working on another project with " + utl + "% utilization. so we can utilize " + mnu + "% only";
                                //                    js.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
                                //                    return js;

                                //                }
                                //            }
                                //        }
                                //    }
                                //}

                            }
                            else
                            {
                                Isprj = false;
                                js.Data = "Employee date of joining should be less than billing date of the TR.";
                                js.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
                                return js;
                            }

                            bool sta = false;
                            if (Status.ToLower() == "true")
                                sta = true;
                            if (Isprj)
                            {
                                // updating employee billing status

                                IFormatProvider theCultureInfo = new System.Globalization.CultureInfo("en-us", true);
                                DateTime DstartDate = DateTime.Parse(StartDate, theCultureInfo);
                                DateTime DendDate = DateTime.Parse(EndDate, theCultureInfo);
                                // binding data 


                                //empStatus.AssignmentStatus = AssignmentStatus;

                                int empID = (int)Session["EmployeeId"];
                                var previousprojectass = _db.ProjectAssignments.Where(p => p.IsActive == true && p.EmployeeId == emplyid
                                && p.BillingStatus == "Bench").ToList();



                                var dtProjectAssignSatrtAsEndDate = _db.ProjectAssignments.Where(pa => pa.EmployeeId == emplyid
                                                && dts <= pa.StartDate && pa.Assignment_Id != AssignmentID).OrderBy(pa => pa.StartDate).Select(pa => pa.StartDate).FirstOrDefault();
                                var dtProjectAssignEndDate = _db.ProjectAssignments.Where(pa => pa.EmployeeId == emplyid && pa.IsActive == true && pa.BillingStatus != "Bench"
                                       && dts <= pa.EndDate).OrderBy(pa => pa.EndDate).Select(pa => pa.EndDate).FirstOrDefault();

                                //if (projAssignment.Utilization == utli && projAssignment.BillingStatus == BillingStatus)   // without util and billing if we editing dates na going to creating new 
                                //{
                                    #region assignmenthistory
                                    ProjectAssignmenthistory pash = new ProjectAssignmenthistory();
                                    pash.AssignmentId = AssignmentID;
                                    pash.ProjectCode = projAssignment.ProjectCode;
                                    pash.ProjectName = projAssignment.ProjectName;
                                    pash.ProjectID = projAssignment.ProjectID;
                                    pash.Assigned_ByOld = projAssignment.Assigned_By;
                                    pash.BillingStatusOld = projAssignment.BillingStatus;
                                    pash.EmployeeId = projAssignment.EmployeeId;
                                    pash.EnddateOld = projAssignment.EndDate;
                                    pash.IsActiveOld = projAssignment.IsActive;
                                    pash.StartDateOld = projAssignment.StartDate;
                                    pash.UtilizationOld = projAssignment.Utilization;
                                    pash.modifiedBy = empID;
                                    pash.ModifiedDate = DateTime.Now;
                              
                                    projAssignment.StartDate = DstartDate;
                                //  projAssignment.EndDate = DendDate;

                                  //Need to add condition for projectassignment enddate should not be greater than  Project enddate 
                                   var projectEndDate = _db.Projects.Where(pa => pa.ProjectCode == projAssignment.ProjectCode && pa.IsActive == true).Select(pa => pa.SOWEndDate).FirstOrDefault();
                                   if (DendDate > projectEndDate)
                                   {
                                    // Assignment End Date is greater than the Project End Date
                                    //Convert.ToDateTime(objExternalHire.FulfilmentDate).ToString("dd/MM/yyyy")

                                   // js.Data = "Assignment End Date is greater than the Project End Date";
                                    js.Data = "Assignment End Date" + " ( " +   Convert.ToDateTime(DendDate).ToString("dd/MM/yyyy") + " ) " + " is greater than the Project  End Date" + " ( " + Convert.ToDateTime(projectEndDate).ToString("dd/MM/yyyy") + " ) ";
                                    js.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
                                    return js;
                                   }
                                   else
                                   {
                                    projAssignment.EndDate = DendDate;
                                   }
                                    projAssignment.BillingStatus = BillingStatus;
                                    projAssignment.Utilization = utli;
                                    projAssignment.IsActive = Convert.ToBoolean(Status);
                                    if (DateTime.Now.Date.Equals(Convert.ToDateTime(DstartDate).Date))
                                    {
                                        projAssignment.IsActive = true;
                                    }
                                    projAssignment.Assigned_Date = DateTime.Now;


                                    pash.UtilizationNew = projAssignment.Utilization;
                                    pash.StartDateNew = projAssignment.StartDate;
                                    pash.IsActiveNew = projAssignment.IsActive;
                                    pash.EndDateNew = projAssignment.EndDate;
                                    pash.BillingStatusNew = projAssignment.BillingStatus;
                                    pash.Assigned_byNew = projAssignment.Assigned_By;
                                    _db.ProjectAssignmenthistories.Add(pash);
                                    #endregion
                                    #region Need to update bench recordds
                                    if (previousprojectass.Count != 0)
                                    {
                                        foreach (var lt in previousprojectass)
                                        {

                                            #region Bench Assignment History
                                            pash = new ProjectAssignmenthistory();
                                            pash.AssignmentId = lt.Assignment_Id;
                                            pash.ProjectCode = lt.ProjectCode;
                                            pash.ProjectName = lt.ProjectName;
                                            pash.ProjectID = lt.ProjectID;
                                            pash.Assigned_ByOld = lt.Assigned_By;
                                            pash.BillingStatusOld = lt.BillingStatus;
                                            pash.EmployeeId = lt.EmployeeId;
                                            pash.EnddateOld = lt.EndDate;
                                            pash.IsActiveOld = lt.IsActive;
                                            pash.StartDateOld = lt.StartDate;
                                            pash.UtilizationOld = lt.Utilization;
                                            pash.modifiedBy = empID;
                                            pash.ModifiedDate = DateTime.Now;


                                            #region Exisitng Bench Record Update based on selected Dates
                                            // Selecting top 1 Start Date order by desc to Set Bench Enda date
                                            int? ul = null;
                                            if (DateTime.Now.Date.Equals(Convert.ToDateTime(DstartDate).Date))
                                            {

                                                ul = lt.Utilization - Convert.ToInt32(utli);

                                            }
                                            else
                                            {
                                                ul = lt.Utilization;
                                            }

                                            if (dtProjectAssignSatrtAsEndDate != null
                                                && Convert.ToDateTime(dtProjectAssignSatrtAsEndDate).Date <= Convert.ToDateTime(dtStartDate).Date)
                                            {
                                                lt.EndDate = Convert.ToDateTime(dtProjectAssignSatrtAsEndDate);
                                            }
                                            else if (dtProjectAssignEndDate != null && dtProjectAssignEndDate <= dtEndDate
                                                && DateTime.Now.Date.Equals(dtStartDate.Date))
                                            {

                                                if (Convert.ToDateTime(dtStartDate) < dtProjectAssignEndDate)
                                                    lt.EndDate = Convert.ToDateTime(dtStartDate);
                                                else
                                                    lt.EndDate = Convert.ToDateTime(dtProjectAssignEndDate);
                                            }
                                            else if (DateTime.Now.Date >= (dtStartDate.Date))
                                            {
                                                lt.EndDate = dtEndDate;
                                                if (ul > 0)
                                                    lt.EndDate = Convert.ToDateTime(dtEndDate);
                                                else
                                                {
                                                    if (Convert.ToDateTime(dtStartDate) >= Convert.ToDateTime(lt.StartDate))
                                                        lt.EndDate = Convert.ToDateTime(dtStartDate);
                                                    else
                                                        lt.EndDate = lt.StartDate;

                                                }
                                            }
                                            else
                                            {
                                                lt.EndDate = dtStartDate;
                                            }
                                            // billdatge == getdate

                                            if (ul > 0)
                                            {
                                                lt.Utilization = ul;

                                            }
                                            else
                                            {
                                                lt.IsActive = false;
                                            }
                                            #endregion

                                            pash.UtilizationNew = lt.Utilization;
                                            pash.StartDateNew = lt.StartDate;
                                            pash.EndDateNew = lt.EndDate;
                                            pash.IsActiveNew = true;
                                            pash.BillingStatusNew = lt.BillingStatus;
                                            pash.Assigned_byNew = lt.Assigned_By;
                                            _db.ProjectAssignmenthistories.Add(pash);
                                            #endregion

                                            _db.Entry(lt).State = System.Data.Entity.EntityState.Modified;         // for update purpose
                                            _db.SaveChanges();
                                        }
                                    }
                                    #endregion


                                //}                                                                  //commented 10/10/22 
                                //else
                                //{
                                //    #region assignmenthistory
                                //    ProjectAssignmenthistory pash = new ProjectAssignmenthistory();
                                //    pash.AssignmentId = AssignmentID;
                                //    pash.ProjectCode = projAssignment.ProjectCode;
                                //    pash.ProjectName = projAssignment.ProjectName;
                                //    pash.ProjectID = projAssignment.ProjectID;
                                //    pash.Assigned_ByOld = projAssignment.Assigned_By;
                                //    pash.BillingStatusOld = projAssignment.BillingStatus;
                                //    pash.EmployeeId = projAssignment.EmployeeId;
                                //    pash.EnddateOld = projAssignment.EndDate;
                                //    pash.IsActiveOld = projAssignment.IsActive;
                                //    pash.StartDateOld = projAssignment.StartDate;
                                //    pash.UtilizationOld = projAssignment.Utilization;
                                //    pash.modifiedBy = empID;
                                //    pash.ModifiedDate = DateTime.Now;


                                //    if (DateTime.Now.Date < Convert.ToDateTime(dtStartDate).Date)
                                //    {
                                //        projAssignment.StartDate = DstartDate;
                                //        projAssignment.EndDate = DendDate;
                                //        projAssignment.BillingStatus = BillingStatus;
                                //        projAssignment.Utilization = utli;
                                //        projAssignment.IsActive = false;
                                //        projAssignment.Assigned_Date = DateTime.Now;
                                //    }
                                //    else
                                //    {
                                //        projAssignment.EndDate = DateTime.Now;
                                //        projAssignment.IsActive = true;


                                //        if (previousprojectass.Count > 0)
                                //        {
                                //            previousprojectass.FirstOrDefault().EndDate = DateTime.Now;
                                //            previousprojectass.FirstOrDefault().IsActive = true;
                                //        }
                                //    }


                                //    pash.UtilizationNew = projAssignment.Utilization;
                                //    pash.StartDateNew = projAssignment.StartDate;
                                //    pash.IsActiveNew = projAssignment.IsActive;
                                //    pash.EndDateNew = projAssignment.EndDate;
                                //    pash.BillingStatusNew = projAssignment.BillingStatus;
                                //    pash.Assigned_byNew = projAssignment.Assigned_By;
                                //    _db.ProjectAssignmenthistories.Add(pash);

                                //    #endregion

                                //    if (DateTime.Now.Date >= Convert.ToDateTime(dtStartDate).Date)
                                //    {
                                //        #region Creating New Project Assignment
                                //        ProjectAssignment prj1 = new ProjectAssignment();
                                //        prj1.EmployeeId = emplyid;
                                //        prj1.ProjectID = projAssignment.ProjectID;
                                //        prj1.ProjectCode = projAssignment.ProjectCode;
                                //        prj1.ProjectName = projAssignment.ProjectName;
                                //        prj1.StartDate = dts;
                                //        prj1.EndDate = DendDate;
                                //        if (dts == DateTime.Now.Date)
                                //            prj1.IsActive = true;
                                //        else
                                //        {
                                //            prj1.IsActive = false;
                                //        }
                                //        prj1.Assigned_By = projAssignment.Assigned_By;
                                //        prj1.Assigned_Date = DateTime.Now;
                                //        prj1.BillingStatus = BillingStatus;
                                //        if (prj1.BillingStatus == "Bench")
                                //        {
                                //            prj1.Category = "Deployable Bench";
                                //            prj1.Bechstatus = "Free Pool";
                                //        }
                                //        prj1.Utilization = utli;
                                //        _db.ProjectAssignments.Add(prj1);                             // from here adding like new record       

                                //        pash = new ProjectAssignmenthistory();
                                //        pash.AssignmentId = 0;
                                //        pash.ProjectCode = prj1.ProjectCode;
                                //        pash.ProjectName = prj1.ProjectName;
                                //        pash.ProjectID = prj1.ProjectID;
                                //        pash.Assigned_ByOld = null;
                                //        pash.BillingStatusOld = null;
                                //        pash.EmployeeId = prj1.EmployeeId;
                                //        pash.EnddateOld = null;
                                //        pash.IsActiveOld = null;
                                //        pash.StartDateOld = null;
                                //        pash.UtilizationOld = null;
                                //        pash.modifiedBy = empID;
                                //        pash.ModifiedDate = DateTime.Now;
                                //        pash.UtilizationNew = prj1.Utilization;
                                //        pash.StartDateNew = prj1.StartDate;
                                //        pash.EndDateNew = prj1.EndDate;
                                //        pash.IsActiveNew = prj1.IsActive;
                                //        pash.BillingStatusNew = prj1.BillingStatus;
                                //        pash.Assigned_byNew = prj1.Assigned_By;
                                //        _db.ProjectAssignmenthistories.Add(pash);
                                //        #endregion

                                //        #region need to create new Bench Record
                                //        int? remnutil = null;
                                //        if (objpras1.Count() > 0)
                                //        {
                                //            var sumUtilization = objpras1.Sum(p => p.Utilization);
                                //            remnutil = 100 - sumUtilization;
                                //        }

                                //        int benchUtilization = 0;

                                //        if (remnutil == null)
                                //        {
                                //            benchUtilization = 100 - utli;
                                //        }
                                //        else
                                //        {
                                //            benchUtilization = Convert.ToInt32(remnutil - utli);
                                //        }

                                //        //projAssignment.Utilization = Utilzation;

                                //        if (benchUtilization > 0)
                                //        {
                                //            if (dts == DateTime.Now.Date)
                                //            {
                                //                #region First Bench Record if Bench record not exist for the Employee
                                //                var unass = (from bpr in _db.PracticeWiseBenchCodes
                                //                             join pt in _db.Projects on bpr.BenchCode equals pt.ProjectCode
                                //                             join emp in _db.Employees on pt.ProjectManagerId equals emp.EmployeeId
                                //                             where bpr.CostCenter.ToLower().Contains(empStatus.CostCenter.ToLower())
                                //                             select new
                                //                             {
                                //                                 Projectcode = pt.ProjectCode,
                                //                                 ProjectName = pt.ProjectName,
                                //                                 projectid = pt.ProjectId,
                                //                                 projmanger = emp.LastName + "," + emp.FirstName + " " + emp.MiddleName,
                                //                                 Billingstatus = bpr.BillingStatus,
                                //                                 Practice = bpr.Practice
                                //                             }).Distinct().ToList();

                                //                if (empStatus.CostCenter.ToLower() != "testing")
                                //                {

                                //                    unass = unass.Where(p => p.Practice.ToLower() == empStatus.Practice.ToLower()).ToList();
                                //                }

                                //                projAssignment = new ProjectAssignment();
                                //                projAssignment.ProjectCode = unass.FirstOrDefault().Projectcode;
                                //                projAssignment.ProjectID = unass.FirstOrDefault().projectid;
                                //                projAssignment.ProjectName = unass.FirstOrDefault().ProjectName;
                                //                //projAssignment.StartDate = unass.BillingDate;
                                //                projAssignment.StartDate = dts;

                                //                if (dtProjectAssignEndDate != null && dtProjectAssignEndDate <= dtEndDate)
                                //                {
                                //                    projAssignment.EndDate = Convert.ToDateTime(dtProjectAssignEndDate);
                                //                }
                                //                else
                                //                {
                                //                    projAssignment.EndDate = dtEndDate;
                                //                }

                                //                // billdatge == getdate


                                //                projAssignment.Utilization = benchUtilization;
                                //                projAssignment.EmployeeId = emplyid;
                                //                // need to check future date and active and inactive
                                //                if (dts == DateTime.Now.Date)
                                //                {
                                //                    projAssignment.IsActive = true;
                                //                }
                                //                else
                                //                {
                                //                    projAssignment.IsActive = false;
                                //                }
                                //                projAssignment.Assigned_By = unass.FirstOrDefault().projmanger;
                                //                projAssignment.Assigned_Date = System.DateTime.Now;
                                //                projAssignment.BillingStatus = unass.FirstOrDefault().Billingstatus;
                                //                if (projAssignment.BillingStatus == "Bench")
                                //                {
                                //                    projAssignment.Category = "Deployable Bench";
                                //                    projAssignment.Bechstatus = "Free Pool";
                                //                }
                                //                _db.ProjectAssignments.Add(projAssignment);                             // tooo
                                //                #endregion

                                //                #region Selected Project Assignment History
                                //                ProjectAssignmenthistory projBenchHistory = new ProjectAssignmenthistory();
                                //                projBenchHistory.AssignmentId = 0;
                                //                projBenchHistory.ProjectCode = projAssignment.ProjectCode;
                                //                projBenchHistory.ProjectName = projAssignment.ProjectName;
                                //                projBenchHistory.ProjectID = projAssignment.ProjectID;
                                //                projBenchHistory.Assigned_ByOld = null;
                                //                projBenchHistory.BillingStatusOld = null;
                                //                projBenchHistory.EmployeeId = emplyid;
                                //                projBenchHistory.EnddateOld = null;
                                //                projBenchHistory.IsActiveOld = null;
                                //                projBenchHistory.StartDateOld = null;
                                //                projBenchHistory.UtilizationOld = null;
                                //                projBenchHistory.modifiedBy = empID;
                                //                projBenchHistory.ModifiedDate = DateTime.Now;
                                //                projBenchHistory.UtilizationNew = projAssignment.Utilization;
                                //                projBenchHistory.StartDateNew = projAssignment.StartDate;
                                //                projBenchHistory.IsActiveNew = projAssignment.IsActive;
                                //                projBenchHistory.EndDateNew = projAssignment.EndDate;
                                //                projBenchHistory.BillingStatusNew = projAssignment.BillingStatus;
                                //                projBenchHistory.Assigned_byNew = projAssignment.Assigned_By;
                                //                _db.ProjectAssignmenthistories.Add(projBenchHistory);         
                                //                #endregion
                                //            }
                                //        }


                                //        #endregion
                                //    }

                                //}

                                _db.SaveChanges();
                                js.Data = "true";
                                js.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
                            }
                        }
                        //else
                        //{
                        //    js.Data = "Can not inactivate the assignment becase this employee doesn't have active assignments";
                        //    js.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
                        //}
                    }
                    else
                    {
                        js.Data = "Utilization should not be greater than 100%";
                        js.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
                    }
                    return js;
                }
                catch (Exception ex)
                {
                    Common.WriteExceptionErrorLog(ex);
                    //return RedirectToAction("Error", "Error");
                    return Json("Error", JsonRequestBehavior.AllowGet);
                }
            }

            else
            {
                //ermsg = "Session expired";
                return Json("Sessionexpired", JsonRequestBehavior.AllowGet);
                //return RedirectToAction("SessionExpire", "Signout");
            }
        }
        /// <summary>
        /// Checking for assignment exists
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="empId"></param>
        /// <param name="currentAssignmentId"></param>
        /// <returns></returns>

        public JsonResult GetAssignmentDetails(int AssignmentID)
        {
            JsonResult js = new JsonResult();

            var projAssignment = (from pa in _db.ProjectAssignments
                                  join pin in _db.Projects on pa.ProjectCode equals pin.ProjectCode
                                  join emp in _db.Employees on pin.ProjectManagerId equals emp.EmployeeId
                                  join emp1 in _db.Employees on pa.EmployeeId equals emp1.EmployeeId
                                  where pa.IsActive == true &&
                                  pa.Assignment_Id == AssignmentID
                                  select new
                                  {

                                      EmailID = emp1.Email,
                                      Location = emp1.Location,
                                      Practice = emp1.Practice,
                                      EmployeeCode = emp1.EmployeeId,
                                      EmployeeName = emp1.FirstName + " " + emp1.MiddleName + " " + emp1.LastName,
                                      AssignmenStatus = emp1.AssignmentStatus,
                                      ProjectCode = pin.ProjectCode,
                                      ProjectManage = emp.LastName + "," + emp.FirstName + " " + emp.MiddleName,
                                      ProjectName = pin.ProjectName,
                                      BillingType = pin.BillingType,
                                      StartDate = pa.StartDate,
                                      EndDate = pa.EndDate,
                                      Billingstatu = pa.BillingStatus,
                                      Utilization = pa.Utilization

                                  }).ToList().FirstOrDefault();
            js.Data = projAssignment;
            js.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return js;
        }
        public ActionResult ExportReport()
        {

            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            {
                try
                {
                    return Json("success", JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    Common.WriteExceptionErrorLog(ex);
                    //return RedirectToAction("Error", "Error");
                    return Json("Error", JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                //ermsg = "Session expired";
                return Json("Sessionexpired", JsonRequestBehavior.AllowGet);
                //return RedirectToAction("SessionExpire", "Signout");
            }
        }
        public ActionResult Downloadactiveassignment()
        {
            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            {
                return View();
            }
            else
            {
                //ermsg = "Session expired";
                //return Json("Sessionexpired", JsonRequestBehavior.AllowGet);
                return RedirectToAction("SessionExpire", "Signout");
            }
        }
        public ActionResult AssignmentByBusinessGroup()           
        {
            try
            {
				//string UserName = User.Identity.Name.Split('\\')[1].ToLower();
				string usermail = Common.GetAzureLoggedInUserID();
				int empID = _db.Employees.Where(e => e.Email.Equals(usermail) && e.IsActive == true).Select(e => e.EmployeeId).FirstOrDefault();


                //   int empID = (int)Session["EmployeeId"];
                string authorizedUser = Environment.GetEnvironmentVariable("businessGroupId") ?? "";
                bool isAuthorised = false;
                if (authorizedUser != null && authorizedUser != "")
                {
                    string[] authorizedUserArry = authorizedUser.ToString().Split(',');
                    if (authorizedUserArry.Length > 0)
                    {
                        for (int i = 0; i < authorizedUserArry.Length; i++)
                        {
                            if (Convert.ToInt32(authorizedUserArry[i]) == empID)
                            {
                                isAuthorised = true;
                            }
                        }
                    }
                }

                if (isAuthorised == true)
                {
                    List<ProjectAssignment> listAllProjectAssignments = new List<ProjectAssignment>();



                    listAllProjectAssignments = _db.Database.SqlQuery<ProjectAssignment>("exec GetAssignmentByBusinessGroup").ToList();


                    return View(listAllProjectAssignments);
                }

                else
                {
                    return null;
                }
            }

            catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
            {
                Exception raise = dbEx;
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        string message = string.Format("{0}:{1}",
                            validationErrors.Entry.Entity.ToString(),
                            validationError.ErrorMessage);
                        // raise a new exception nesting
                        // the current instance as InnerException
                        raise = new InvalidOperationException(message, raise);
                    }
                }
                ErrorHandling expcls = new ErrorHandling();
                expcls.Error(raise);

                throw raise;
            }

        }        
        public JsonResult GetAssignmentByAssignmentId(int assignmentId)
        {
            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            {
                try
                {
                    List<ProjectAssignment> listAllProjectAssignments = new List<ProjectAssignment>();
                    var result = (from p in _db.ProjectAssignments.Where(p => p.Assignment_Id == assignmentId)
                                  select new
                                  {
                                      AssignmentID = p.Assignment_Id,
                                      ProjectCode = p.ProjectCode,
                                      EmployeeId = p.EmployeeId,
                                      EmployeeName = _db.Employees.Where(l => l.EmployeeId == p.EmployeeId).Select(l => l.FirstName).FirstOrDefault(),
                                      StartDate = p.StartDate,
                                      EndDate = p.EndDate,
                                      IsActive = p.IsActive,
                                      Utilization = p.Utilization,
                                      BillingStatus = p.BillingStatus,


                                  }).FirstOrDefault();

                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
                {
                    Exception raise = dbEx;
                    foreach (var validationErrors in dbEx.EntityValidationErrors)
                    {
                        foreach (var validationError in validationErrors.ValidationErrors)
                        {
                            string message = string.Format("{0}:{1}",
                                validationErrors.Entry.Entity.ToString(),
                                validationError.ErrorMessage);
                            // raise a new exception nesting
                            // the current instance as InnerException
                            raise = new InvalidOperationException(message, raise);
                        }
                    }
                    ErrorHandling expcls = new ErrorHandling();
                    expcls.Error(raise);

                    throw raise;
                }
            }
            else
            {
                //ermsg = "Session expired";
                return Json("Sessionexpired", JsonRequestBehavior.AllowGet);
                //return RedirectToAction("SessionExpire", "Signout");
            }

        }
        public ActionResult UpdateProjectAssignments(string assignmentId, string stDate, string endDate, string utilization, string isActive, string billingStatus)
        {
            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            {
                try
                {

                int Id = Convert.ToInt32(assignmentId);
               

                bool isActiveProject = true;
                //isActive != null ? (isActive == "1" ? true : false) : null;
                // DateTime dt = Convert.ToDateTime(endDate);
                IFormatProvider culture = new System.Globalization.CultureInfo("en-US", true);
                DateTime stdt = DateTime.ParseExact(stDate, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                DateTime enddt = DateTime.ParseExact(endDate, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                var todaysDate = DateTime.Today;

                if (enddt < todaysDate || isActive == "false")
                {
                    isActiveProject = false;
                }

                var data = _db.ProjectAssignments.Where(p => p.Assignment_Id == Id).FirstOrDefault();
                var joiningdate = _db.Employees.Where(p => p.EmployeeId == data.EmployeeId).FirstOrDefault().DateOfJoin;
                if (stdt < joiningdate)
                {
                    stdt = joiningdate;
                }


                if (stDate.Trim() != "")
                {
                    data.StartDate = stdt;
                }
                if (endDate.Trim() != "")
                {
                    data.EndDate = enddt;
                }
                if (utilization.Trim() != "")
                {
                    int utli = Convert.ToInt32(utilization);
                    data.Utilization = utli;
                }
                if (isActive.Trim() != "")
                {
                    data.IsActive = isActiveProject;

                }
                if (billingStatus.Trim() != "")
                {
                    if (billingStatus.Trim()== "Bench")
                    {
                        data.Category = "Deployable Bench";
                        data.Bechstatus = "Free Pool";
                    }
                    else
                    {
                        data.Category = null;
                        data.Bechstatus = null;
                    }
                    {

                    }
                        data.BillingStatus = billingStatus;


                    
                }
                data.Assigned_Date = DateTime.Now;
                _db.SaveChanges();
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
            {
                Exception raise = dbEx;
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        string message = string.Format("{0}:{1}",
                            validationErrors.Entry.Entity.ToString(),
                            validationError.ErrorMessage);
                        // raise a new exception nesting
                        // the current instance as InnerException
                        raise = new InvalidOperationException(message, raise);
                    }
                }
                ErrorHandling expcls = new ErrorHandling();
                expcls.Error(raise);

                throw raise;
            }
        }
            else
            {
                //ermsg = "Session expired";
                return Json("Sessionexpired", JsonRequestBehavior.AllowGet);
                //return RedirectToAction("SessionExpire", "Signout");
            }
        }
        public ActionResult AddProjectAssignment(string projectcode, string projectname, string name, string stDate, string endDate, string utilization, string isActive, string billingStatus)
        {
            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            {
                try
            {
                bool isActiveProject = true;
                //isActive != null ? (isActive == "1" ? true : false) : null;

                IFormatProvider culture = new System.Globalization.CultureInfo("en-US", true);
                DateTime stdt = DateTime.ParseExact(stDate, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                DateTime enddt = DateTime.ParseExact(endDate, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                var todaysDate = DateTime.Today;

                if (enddt < todaysDate || isActive == "false")
                {
                    isActiveProject = false;
                }


               

                int empid = 0;
                var projectId = _db.Projects.Where(p => p.ProjectCode == projectcode).Select(p => p.ProjectId).FirstOrDefault();
                var projectManagerName = (from p in _db.Projects
                                          join e in _db.Employees on p.ProjectManagerId equals e.EmployeeId
                                          where p.ProjectCode == projectcode
                                          select e.FirstName + " " + e.MiddleName + " " + e.LastName).FirstOrDefault();


                int utli = Convert.ToInt32(utilization);




                if (name != null && name != string.Empty)
                {

                    string[] at = name.Split('-');
                    empid = Convert.ToInt32(at[0]);
                }

                var joiningdate = _db.Employees.Where(p => p.EmployeeId == empid).FirstOrDefault().DateOfJoin;
                if (stdt < joiningdate)
                {
                    stdt = joiningdate;
                }
              

                if (billingStatus.Trim() != "Bench" )
                {


                    int exsum = 0;
                    var existingUtilisation = _db.ProjectAssignments.Where(p => p.EmployeeId == empid && p.IsActive == true && p.BillingStatus.Trim().ToUpper() != "BENCH").Select(p => p.Utilization).ToList();
                    if (existingUtilisation != null)
                    {
                        foreach (var o in existingUtilisation)
                        {
                            exsum += Convert.ToInt32(o);
                        }

                    }
                    int billedremaining = 100 - exsum;
                    var existingProject = _db.ProjectAssignments.Where(p => p.EmployeeId == empid && p.BillingStatus.Trim().ToUpper() == "BENCH" && p.IsActive==true).FirstOrDefault();
                 
                    if (existingProject != null )
                    {

                        int allremainng = billedremaining - existingProject.Utilization.Value;
                        if (allremainng < utli)
                        {
                            if (Convert.ToInt32(utilization) == existingProject.Utilization)
                            {
                                existingProject.IsActive = false;
                                existingProject.Assigned_Date = DateTime.Now;
                            }
                            else if ((Convert.ToInt32(utilization) < existingProject.Utilization))
                            {

                                existingProject.Utilization = existingProject.Utilization - Convert.ToInt32(utilization);
                                existingProject.Assigned_Date = DateTime.Now;
                            }
                            _db.SaveChanges();
                        }
                    }

                }

              
                var data = new ProjectAssignment
                {
                    ProjectCode = projectcode,
                    ProjectID = projectId,
                    ProjectName = projectname,
                    EmployeeId = empid,
                    StartDate = stdt,
                    EndDate =enddt,
                    Utilization = utli,
                    Category= billingStatus.Trim() == "Bench" ? "Deployable Bench" : null,
                    Bechstatus = billingStatus.Trim() == "Bench" ?  "Free Pool" : null,
                    IsActive = isActiveProject,
                    BillingStatus = billingStatus,
                    Assigned_Date = DateTime.Now,
                    Assigned_By = projectManagerName
                };
              
                _db.ProjectAssignments.Add(data);
                _db.SaveChanges();




                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
            {
                Exception raise = dbEx;
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        string message = string.Format("{0}:{1}",
                            validationErrors.Entry.Entity.ToString(),
                            validationError.ErrorMessage);
                        // raise a new exception nesting
                        // the current instance as InnerException
                        raise = new InvalidOperationException(message, raise);
                    }
                }
                ErrorHandling expcls = new ErrorHandling();
                expcls.Error(raise);

                throw raise;
            }
        }
            else
            {
                //ermsg = "Session expired";
                return Json("Sessionexpired", JsonRequestBehavior.AllowGet);
                //return RedirectToAction("SessionExpire", "Signout");
            }
        }
        public ActionResult GetProjectNamebyCode(string projectcode)
        {
            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            {
                try
            {
                var projectName = (from k in _db.Projects
                                   where k.ProjectCode == projectcode
                                   select k.ProjectName).Distinct().ToList();

                return Json(projectName, JsonRequestBehavior.AllowGet);
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
            {
                Exception raise = dbEx;
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        string message = string.Format("{0}:{1}",
                            validationErrors.Entry.Entity.ToString(),
                            validationError.ErrorMessage);
                        // raise a new exception nesting
                        // the current instance as InnerException
                        raise = new InvalidOperationException(message, raise);
                    }
                }
                ErrorHandling expcls = new ErrorHandling();
                expcls.Error(raise);

                throw raise;
            }

        }
            else
            {
                //ermsg = "Session expired";
                return Json("Sessionexpired", JsonRequestBehavior.AllowGet);
                //return RedirectToAction("SessionExpire", "Signout");
            }
        }        
        public ActionResult GetProjectCode()
        {
            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            {
                try
            {
                var gradevalues = (from k in _db.Projects

                                   where k.IsActive == true
                                   select k.ProjectCode).Distinct().OrderBy(k => k).ToList();

                return Json(gradevalues, JsonRequestBehavior.AllowGet);
            }

            catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
            {
                Exception raise = dbEx;
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        string message = string.Format("{0}:{1}",
                            validationErrors.Entry.Entity.ToString(),
                            validationError.ErrorMessage);
                        // raise a new exception nesting
                        // the current instance as InnerException
                        raise = new InvalidOperationException(message, raise);
                    }
                }
                ErrorHandling expcls = new ErrorHandling();
                expcls.Error(raise);

                throw raise;
            }

        }
            else
            {
                //ermsg = "Session expired";
                return Json("Sessionexpired", JsonRequestBehavior.AllowGet);
                //return RedirectToAction("SessionExpire", "Signout");
            }
        }       
        public ActionResult AutoPopulateEmployeeId(string term)
        {
            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            {
                try
                {



                {
                    var result = new List<KeyValuePair<string, string>>();
                    DateTime currentDate = System.DateTime.Now.Date;
                    IList<SelectListItem> List = new List<SelectListItem>();

                    var Projects = (from pro in _db.Employees
                                    where pro.IsActive == true && pro.BusinessGroup.Equals("TCTS")
                                    select new SelectListItem
                                    {
                                        Text = pro.FirstName + " " + pro.MiddleName + " " + pro.LastName,
                                        Value = pro.EmployeeId + "-" + pro.FirstName + " " + pro.MiddleName + " " + pro.LastName
                                    }).Distinct().ToList();

                    foreach (var item in Projects)
                    {
                        result.Add(new KeyValuePair<string, string>(item.Value.ToString(), item.Text));
                    }
                    var result3 = result.Where(s => s.Key.ToLower().Contains
                                  (term.ToLower())).Select(w => w).ToList();
                    return Json(result3, JsonRequestBehavior.AllowGet);



                }
            }

            catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
            {
                Exception raise = dbEx;
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        string message = string.Format("{0}:{1}",
                            validationErrors.Entry.Entity.ToString(),
                            validationError.ErrorMessage);
                        // raise a new exception nesting
                        // the current instance as InnerException
                        raise = new InvalidOperationException(message, raise);
                    }
                }
                ErrorHandling expcls = new ErrorHandling();
                expcls.Error(raise);

                throw raise;
            }

        }
            else
            {
                //ermsg = "Session expired";
                return Json("Sessionexpired", JsonRequestBehavior.AllowGet);
                //return RedirectToAction("SessionExpire", "Signout");
            }
        }
        public ActionResult VerifyEmployee(string name)
        {
            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            {
                try
                {

                string[] at = name.Split('-');
                int empid = Convert.ToInt32(at[0]);

                var isExist = _db.Employees.Where(p => p.EmployeeId == empid && p.IsActive == true).FirstOrDefault();
                if (isExist == null)
                {
                    return Json(false, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(true, JsonRequestBehavior.AllowGet);
                }


            }
            catch
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }

        }
            else
            {
                //ermsg = "Session expired";
                return Json("Sessionexpired", JsonRequestBehavior.AllowGet);
               // return RedirectToAction("SessionExpire", "Signout");
            }
        }
        public ActionResult VerifyUtilization(int utli, string name)
        {
            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            {
                try
            {
                string[] at = name.Split('-');
                int empId = Convert.ToInt32(at[0]);


                JsonResult js = new JsonResult();
                var preprj = (from pt in _db.ProjectAssignments
                              where pt.EmployeeId == empId && pt.IsActive == true
                              select pt).ToList();
                bool Isprj = true;
                int utl = 0;
                foreach (var pt in preprj)
                {
                    if (!pt.BillingStatus.ToLower().Contains("bench"))
                    {

                        if (pt.Utilization == 100)
                        {
                            js.Data = "This employee is working on  " + pt.ProjectCode + " with (%) " + pt.Utilization;
                          //  js.Data = "This employee working on another project with (%) " + utl + "utilization. so we can utilize (%)" + mnu + "only";    //observation change
                            }
                        else
                        {
                           
                            utl = utl + Convert.ToInt32(pt.Utilization);


                            int mnu = 100 - utl;
                            int chkutl = utl + utli;
                            if (chkutl > 100)
                            {
                                Isprj = false;
                               // js.Data = "This employee working on another project with " + utl + "utilization. so we can utilize " + mnu + "only";
                                js.Data = "This employee working on another project with (%) " + utl + "utilization. so we can utilize (%)" + mnu + "only";    //observation change

                                }

                        }
                    }
                    else
                    {
                        js.Data = true;
                    }

                }
                js.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
                return js;


            }
            catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
            {
                Exception raise = dbEx;
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        string message = string.Format("{0}:{1}",
                            validationErrors.Entry.Entity.ToString(),
                            validationError.ErrorMessage);
                        // raise a new exception nesting
                        // the current instance as InnerException
                        raise = new InvalidOperationException(message, raise);
                    }
                }
                ErrorHandling expcls = new ErrorHandling();
                expcls.Error(raise);

                throw raise;
            }
        }
            else
            {
                //ermsg = "Session expired";
                return Json("Sessionexpired", JsonRequestBehavior.AllowGet);
                //return RedirectToAction("SessionExpire", "Signout");
            }
        }
        public ActionResult VerifyUtilizationByProjectCode(int utli, string name, string projectCode)
        {
            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            {
                string[] at = name.Split('-');
            int empId = Convert.ToInt32(at[0]);
            try
            {

                JsonResult js = new JsonResult();
                var preprj = (from pt in _db.ProjectAssignments
                              where pt.EmployeeId == empId && pt.IsActive == true
                              && pt.ProjectCode != projectCode
                              select pt).ToList();
                bool Isprj = true;
                int utl = 0;
                foreach (var pt in preprj)
                {
                    if (!pt.BillingStatus.ToLower().Contains("bench"))
                    {

                        if (pt.Utilization == 100)
                        {
                            js.Data = "This employee is working on  " + pt.ProjectCode + " with (%) " + pt.Utilization;
                        }
                        else
                        {
                            


                            utl = utl + Convert.ToInt32(pt.Utilization);


                            int mnu = 100 - utl;
                            int chkutl = utl + utli;
                            if (chkutl > 100)
                            {
                                Isprj = false;
                               // js.Data = "This employee working on another project with " + utl + "utilization. so we can utilize " + mnu + "only";
                                js.Data = "This employee working on another project with (%) " + utl + "utilization. so we can utilize (%)" + mnu + "only";    //observation change

                                }


                        }
                    }
                    else
                    {
                        js.Data = true;
                    }

                }
                js.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
                return js;
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
            {
                Exception raise = dbEx;
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        string message = string.Format("{0}:{1}",
                            validationErrors.Entry.Entity.ToString(),
                            validationError.ErrorMessage);
                        // raise a new exception nesting
                        // the current instance as InnerException
                        raise = new InvalidOperationException(message, raise);
                    }
                }
                ErrorHandling expcls = new ErrorHandling();
                expcls.Error(raise);

                throw raise;
            }
        }
            else
            {
                //ermsg = "Session expired";
                return Json("Sessionexpired", JsonRequestBehavior.AllowGet);
                //return RedirectToAction("SessionExpire", "Signout");
            }
        }
        
        public ActionResult DeleteProjectAssignmentData(int assignmentId)
        {

            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            {

                try
                {

                var dataToRemove = _db.ProjectAssignments.Where(k => k.Assignment_Id == assignmentId).FirstOrDefault();
                _db.ProjectAssignments.Remove(dataToRemove);
                _db.SaveChanges();
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
            {
                Exception raise = dbEx;
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        string message = string.Format("{0}:{1}",
                            validationErrors.Entry.Entity.ToString(),
                            validationError.ErrorMessage);
                        // raise a new exception nesting
                        // the current instance as InnerException
                        raise = new InvalidOperationException(message, raise);
                    }
                }
                ErrorHandling expcls = new ErrorHandling();
                expcls.Error(raise);

                throw raise;
            }

        }
            else
            {
                //ermsg = "Session expired";
                //return Json("Sessionexpired", JsonRequestBehavior.AllowGet);
                return RedirectToAction("SessionExpire", "Signout");
            }
        }

        public class AllProjectAssignments
        {
            public int EmployeeID { get; set; }
            public string ProjectCode { get; set; }
            public string ProjectName { get; set; }
            public string EMPName { get; set; }
            public int ProjectManagerId { get; set; }
            public string Assigned_By { get; set; }
            public int Utilization { get; set; }
            public string Practice { get; set; }
            public string BillingStatus { get; set; }
            public Nullable<System.DateTime> StartDate { get; set; }
            public Nullable<System.DateTime> EndDate { get; set; }
            public string BillingType { get; set; }
            public int Assignment_Id { get; set; }
            public bool IsActive { get; set; }
            public int DELIVERYMANAGERID { get; set; }
            public string AccountName { get; set; }
            public string Assigstatus { get; set; }
            public string BusinessGroup { get; set; }


        }
        public ActionResult GenerateProjectReport(string strPractice, string strProject, string strBillingStatus,
            string strAccount, string Assignment_All, string From_Date, string To_Date)
        {

            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            {
                try
                {
                    string Role = (string)Session["Role"];


                    int? EmpId = Convert.ToInt32(Session["EmployeeId"]);
                    DateTime? fromdate;
                    DateTime? todate;
                    if ((From_Date != null && From_Date != string.Empty) && (To_Date != null && To_Date != string.Empty))
                    {
                        IFormatProvider theCultureInfo = new System.Globalization.CultureInfo("en-us", true);
                        fromdate = DateTime.Parse(From_Date, theCultureInfo);
                        todate = DateTime.Parse(To_Date, theCultureInfo);
                    }
                    else
                    {
                        fromdate = null;
                        todate = null;

                    }

                    var projectDetails = _db.Database.SqlQuery
                        <AllProjectAssignments>("exec GetAllEmployeesAssignmentDetails @Practice, @BillingStatus,@Account, @Project,@IsActive,@StartDate,@enddate,@Role,@EmpID",
                    new System.Data.SqlClient.SqlParameter("@Practice", strPractice == "" ? (object)DBNull.Value : strPractice),
                    new System.Data.SqlClient.SqlParameter("@BillingStatus", strBillingStatus == "" ? (object)DBNull.Value : strBillingStatus),
                    new System.Data.SqlClient.SqlParameter("@Account", strAccount == "" ? (object)DBNull.Value : strAccount),
                    new System.Data.SqlClient.SqlParameter("@Project", strProject == "" ? (object)DBNull.Value : strProject),
                    new System.Data.SqlClient.SqlParameter("@IsActive", Assignment_All == "" ? (object)DBNull.Value : Assignment_All),
                    new System.Data.SqlClient.SqlParameter("@StartDate", fromdate != null ? fromdate : (object)DBNull.Value),
                    new System.Data.SqlClient.SqlParameter("@enddate", todate != null ? todate : (object)DBNull.Value),
                    new System.Data.SqlClient.SqlParameter("@Role", Role == "" ? (object)DBNull.Value : Role),
                    new System.Data.SqlClient.SqlParameter("@EmpID", EmpId)
                    ).ToList<AllProjectAssignments>();     
                    #region Export to Excel
                    using (ExcelPackage package = new ExcelPackage())
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("TR Information");
                        worksheet.TabColor = System.Drawing.Color.Green;
                        worksheet.DefaultRowHeight = 18f;
                        worksheet.Row(1).Height = 20f;
                        using (var range = worksheet.Cells[1, 1, 1, 13])
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
                        worksheet.Cells[1, 1].Value = "Employee Code";
                        worksheet.Cells[1, 2].Value = "Employee Name";
                        worksheet.Cells[1, 3].Value = "Project Name";
                        worksheet.Cells[1, 4].Value = "Project Code";
                        worksheet.Cells[1, 5].Value = "Assigned_By";
                        worksheet.Cells[1, 6].Value = "Utilisation %";
                        worksheet.Cells[1, 7].Value = "Start Date";
                        worksheet.Cells[1, 8].Value = "End Date";
                        worksheet.Cells[1, 9].Value = "Billing Type";
                        worksheet.Cells[1, 10].Value = "Billing Status";
                        worksheet.Cells[1, 11].Value = "Practice";
                        worksheet.Cells[1, 12].Value = "IsActive";
                        worksheet.Cells[1, 13].Value = "Business Group";
                        //worksheet.Cells[1, 13].Value = "Assignment Status";
                        //Add the each row
                        for (int rowIndex = 0, row = 2; rowIndex < projectDetails.Count; rowIndex++, row++) // row indicates number of rows
                        {
                            worksheet.Cells[row, 1].Value = projectDetails[rowIndex].EmployeeID;
                            worksheet.Cells[row, 2].Value = projectDetails[rowIndex].EMPName;
                            worksheet.Cells[row, 3].Value = projectDetails[rowIndex].ProjectName;
                            worksheet.Cells[row, 4].Value = projectDetails[rowIndex].ProjectCode;
                            worksheet.Cells[row, 5].Value = projectDetails[rowIndex].Assigned_By;
                            worksheet.Cells[row, 6].Value = projectDetails[rowIndex].Utilization;

                            worksheet.Cells[row, 7].Value = projectDetails[rowIndex].StartDate;
                            worksheet.Cells[row, 7].Style.Numberformat.Format = "dd-MMM-yyyy";
                            worksheet.Cells[row, 7].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                            worksheet.Cells[row, 8].Value = projectDetails[rowIndex].EndDate;
                            worksheet.Cells[row, 8].Style.Numberformat.Format = "dd-MMM-yyyy";
                            worksheet.Cells[row, 8].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;


                            worksheet.Cells[row, 9].Value = projectDetails[rowIndex].BillingType;
                            worksheet.Cells[row, 10].Value = projectDetails[rowIndex].BillingStatus;
                            worksheet.Cells[row, 11].Value = projectDetails[rowIndex].Practice;

                            worksheet.Cells[row, 12].Value = projectDetails[rowIndex].Assigstatus;
                            worksheet.Cells[row, 13].Value = projectDetails[rowIndex].BusinessGroup;

                            if (row % 2 == 1)
                            {
                                using (var range = worksheet.Cells[row, 1, row, 13])
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
                        Response.Buffer = true;
                        Response.AddHeader("content-disposition", "attachment;filename=" + ((strProject == "") ? "My" : strProject) + "-Project Report" + DateTime.Now.ToString("_dd-MMM-yyyy_hh-mm-ss") + ".xlsx");

                        Response.Charset = "";
                        Response.ContentType = "application/vnd.ms-excel";
                        //StringWriter sw = new StringWriter();
                        Response.BinaryWrite(fileBytes);
                        Response.End();
                    }

                    #endregion

                    return new EmptyResult();
                }
                catch (Exception ex)
                {
                    Common.WriteExceptionErrorLog(ex);
                    return RedirectToAction("Error", "Error");
                    //return Json("Error", JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                //ermsg = "Session expired";
                //return Json("Sessionexpired", JsonRequestBehavior.AllowGet);
                return RedirectToAction("SessionExpire", "Signout");
            }
            
        }

        public ActionResult GenerateActiveProjectAssignment(string strPractice, string strProject, string strBillingStatus,
                  string strAccount, string Assignment_All, string From_Date, string To_Date)
        {
            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            {
                try
                {

                    string Role = "OM";


                    int? EmpId = Convert.ToInt32(Session["EmployeeId"]);
                    DateTime? fromdate;
                    DateTime? todate;
                    if ((From_Date != null && From_Date != string.Empty) && (To_Date != null && To_Date != string.Empty))
                    {
                        IFormatProvider theCultureInfo = new System.Globalization.CultureInfo("en-us", true);
                        fromdate = DateTime.Parse(From_Date, theCultureInfo);
                        todate = DateTime.Parse(To_Date, theCultureInfo);
                    }
                    else
                    {
                        fromdate = null;
                        todate = null;

                    }

                    var projectDetails = _db.Database.SqlQuery
                        <AllProjectAssignments>("exec GetAllEmployeesAssignmentDetails @Practice, @BillingStatus,@Account, @Project,@IsActive,@StartDate,@enddate,@Role,@EmpID",
                    new System.Data.SqlClient.SqlParameter("@Practice", strPractice == "" ? (object)DBNull.Value : strPractice),
                    new System.Data.SqlClient.SqlParameter("@BillingStatus", strBillingStatus == "" ? (object)DBNull.Value : strBillingStatus),
                    new System.Data.SqlClient.SqlParameter("@Account", strAccount == "" ? (object)DBNull.Value : strAccount),
                    new System.Data.SqlClient.SqlParameter("@Project", strProject == "" ? (object)DBNull.Value : strProject),
                    new System.Data.SqlClient.SqlParameter("@IsActive", Assignment_All == "" ? (object)DBNull.Value : Assignment_All),
                    new System.Data.SqlClient.SqlParameter("@StartDate", fromdate != null ? fromdate : (object)DBNull.Value),
                    new System.Data.SqlClient.SqlParameter("@enddate", todate != null ? todate : (object)DBNull.Value),
                    new System.Data.SqlClient.SqlParameter("@Role", Role == "" ? (object)DBNull.Value : Role),
                    new System.Data.SqlClient.SqlParameter("@EmpID", EmpId)
                    ).ToList<AllProjectAssignments>();
                    //var projectDetails = (from pa in _db.ProjectAssignments
                    //                      join emp in _db.Employees on pa.EmployeeId equals emp.EmployeeId
                    //                      join prjct in _db.Projects on pa.ProjectCode equals prjct.ProjectCode
                    //                      where emp.IsActive == true

                    //                      select new
                    //                      {
                    //                          pa.EmployeeId,
                    //                          pa.ProjectCode,
                    //                          pa.ProjectName,
                    //                          emp.FirstName,
                    //                          emp.MiddleName,
                    //                          emp.LastName,
                    //                          prjct.ProjectManagerId,
                    //                          pa.Assigned_By,
                    //                          pa.Utilization,
                    //                          emp.Practice,
                    //                          pa.BillingStatus,
                    //                          pa.StartDate,
                    //                          pa.EndDate,
                    //                          prjct.BillingType,
                    //                          pa.Assignment_Id,
                    //                          pa.IsActive,
                    //                          prjct.DELIVERY_MANAGER_ID,
                    //                          prjct.AccountName

                    //                      }).OrderBy(x => x.IsActive).ToList();
                    //if ((From_Date != null && From_Date != string.Empty) && (To_Date != null && To_Date != string.Empty))
                    //{
                    //    IFormatProvider theCultureInfo = new System.Globalization.CultureInfo("en-us", true);
                    //    DateTime fromdate = DateTime.Parse(From_Date, theCultureInfo);
                    //    DateTime todate = DateTime.Parse(To_Date, theCultureInfo);
                    //    projectDetails = projectDetails.Where(p => p.StartDate >= fromdate &&
                    //    p.EndDate <= fromdate || (p.StartDate >= todate && p.EndDate <= todate) || (p.StartDate <= fromdate && p.EndDate >= todate)).ToList();
                    //}
                    //if (Assignment_All == "Active")
                    //{
                    //    projectDetails = projectDetails.Where(p => p.IsActive == true).ToList();
                    //}
                    //else if (Assignment_All == "NotYet")
                    //{
                    //    projectDetails = projectDetails.Where(p => p.StartDate >= DateTime.Now.Date).ToList();
                    //}

                    //if (Role == "PM")
                    //{
                    //    projectDetails = projectDetails.Where(p => p.ProjectManagerId == EmpId).ToList();
                    //}
                    //else if (Role == "DH")
                    //{
                    //    projectDetails = projectDetails.Where(p => p.DELIVERY_MANAGER_ID == EmpId).ToList();

                    //}


                    //if (strProject != null && strProject != string.Empty)
                    //{
                    //    projectDetails = projectDetails.Where(p => p.ProjectCode.ToLower() == strProject.Trim().ToLower()).ToList();
                    //}
                    //if (strPractice != null && strPractice != string.Empty)
                    //{
                    //    projectDetails = projectDetails.Where(p => p.Practice.ToLower() == strPractice.Trim().ToLower()).ToList();
                    //}
                    //if (strBillingStatus != null && strBillingStatus != string.Empty)
                    //{
                    //    projectDetails = projectDetails.Where(p => p.BillingStatus != null && p.BillingStatus.ToLower() == strBillingStatus.Trim().ToLower()).ToList();
                    //}
                    //if (strAccount != null && strAccount != string.Empty)
                    //{
                    //    projectDetails = projectDetails.Where(p => p.AccountName != null && p.AccountName.ToLower() == strAccount.Trim().ToLower()).ToList();
                    //}




                    #region Export to Excel

                    using (ExcelPackage package = new ExcelPackage())
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("TR Information");
                        worksheet.TabColor = System.Drawing.Color.Green;
                        worksheet.DefaultRowHeight = 18f;
                        worksheet.Row(1).Height = 20f;

                        using (var range = worksheet.Cells[1, 1, 1, 13])
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
                        worksheet.Cells[1, 1].Value = "Employee Code";
                        worksheet.Cells[1, 2].Value = "Employee Name";
                        worksheet.Cells[1, 3].Value = "Project Name";
                        worksheet.Cells[1, 4].Value = "Project Code";
                        worksheet.Cells[1, 5].Value = "Assigned_By";
                        worksheet.Cells[1, 6].Value = "Utilisation %";
                        worksheet.Cells[1, 7].Value = "Start Date";
                        worksheet.Cells[1, 8].Value = "End Date";
                        worksheet.Cells[1, 9].Value = "Billing Type";
                        worksheet.Cells[1, 10].Value = "Billing Status";
                        worksheet.Cells[1, 11].Value = "Practice";
                        worksheet.Cells[1, 12].Value = "IsActive";
                        worksheet.Cells[1, 13].Value = "Business Group";
                        //worksheet.Cells[1, 13].Value = "Assignment Status";


                        //Add the each row
                        for (int rowIndex = 0, row = 2; rowIndex < projectDetails.Count; rowIndex++, row++) // row indicates number of rows
                        {
                            worksheet.Cells[row, 1].Value = projectDetails[rowIndex].EmployeeID;
                            worksheet.Cells[row, 2].Value = projectDetails[rowIndex].EMPName;
                            worksheet.Cells[row, 3].Value = projectDetails[rowIndex].ProjectName;
                            worksheet.Cells[row, 4].Value = projectDetails[rowIndex].ProjectCode;
                            worksheet.Cells[row, 5].Value = projectDetails[rowIndex].Assigned_By;
                            worksheet.Cells[row, 6].Value = projectDetails[rowIndex].Utilization;

                            worksheet.Cells[row, 7].Value = projectDetails[rowIndex].StartDate;
                            worksheet.Cells[row, 7].Style.Numberformat.Format = "dd-MMM-yyyy";
                            worksheet.Cells[row, 7].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                            worksheet.Cells[row, 8].Value = projectDetails[rowIndex].EndDate;
                            worksheet.Cells[row, 8].Style.Numberformat.Format = "dd-MMM-yyyy";
                            worksheet.Cells[row, 8].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;


                            worksheet.Cells[row, 9].Value = projectDetails[rowIndex].BillingType;
                            worksheet.Cells[row, 10].Value = projectDetails[rowIndex].BillingStatus;
                            worksheet.Cells[row, 11].Value = projectDetails[rowIndex].Practice;

                            worksheet.Cells[row, 12].Value = projectDetails[rowIndex].Assigstatus;
                            worksheet.Cells[row, 13].Value = projectDetails[rowIndex].BusinessGroup;

                            if (row % 2 == 1)
                            {
                                using (var range = worksheet.Cells[row, 1, row, 13])
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
                        Response.Buffer = true;
                        Response.AddHeader("content-disposition", "attachment;filename=" + ((strProject == "") ? "My" : strProject) + "-Project Report" + DateTime.Now.ToString("_dd-MMM-yyyy_hh-mm-ss") + ".xlsx");

                        Response.Charset = "";
                        Response.ContentType = "application/vnd.ms-excel";
                        //StringWriter sw = new StringWriter();
                        Response.BinaryWrite(fileBytes);
                        Response.End();
                    }

                    #endregion

                    return new EmptyResult();
                }
                catch (Exception ex)
                {
                    Common.WriteExceptionErrorLog(ex);
                    return RedirectToAction("Error", "Error");
                    //return Json("Error", JsonRequestBehavior.AllowGet);
                }

            }
            else
            {
                //ermsg = "Session expired";
                //return Json("Sessionexpired", JsonRequestBehavior.AllowGet);
                return RedirectToAction("SessionExpire", "Signout");
            }
        }
    


  


        //return Json("1", JsonRequestBehavior.AllowGet);
    }

    public class EmployeeInfo
    {

        public int EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public DateTime DOJ { get; set; }
        public string EmailID { get; set; }
        public string Position { get; set; }
        public string Grade { get; set; }
        public string Location { get; set; }
        public string AssignmenStatus { get; set; }
        public string Practice { get; set; }

    }
    public class ProjectInfo
    {
        public int projectID { get; set; }
        public string ProjectCode { get; set; }
        public string ProjectName { get; set; }
        public string ProjectManage { get; set; }
        public string BillingType { get; set; }

    }

    //public string global { get; set; }




}