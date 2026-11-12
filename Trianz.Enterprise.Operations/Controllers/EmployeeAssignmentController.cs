using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Trianz.Enterprise.Operations.Models;
using System.Configuration;
using System.Data.Entity.Infrastructure;
using System.Data;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;
using Trianz.Enterprise.Operations.General;
using OfficeOpenXml;

namespace Trianz.Enterprise.Operations.Controllers
{
    public class EmployeeAssignmentController : Controller
    {
        TrianzOperationsEntities db = new TrianzOperationsEntities();

        string AllReleasedates = System.Configuration.ConfigurationManager.AppSettings["All_Releasedates"].ToString();
        string BeforeReleasedates = System.Configuration.ConfigurationManager.AppSettings["Before_Releasedates"].ToString();
        string AfterRelasedates = System.Configuration.ConfigurationManager.AppSettings["After_Relasedates"].ToString();

        // GET: EmployeeAssignment
        public ActionResult Index()
        {
            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            {
                try
                {
                    GetProjects();

                    #region Bind Resource Type in dropdownlist

                    List<SelectListItem> lstResourceType = new List<SelectListItem>();
                    lstResourceType.Add(new SelectListItem { Value = "Bench", Text = "Bench" });
                    lstResourceType.Add(new SelectListItem { Value = "Billed", Text = "Billed" });
                    lstResourceType.Add(new SelectListItem { Value = "Internal Support", Text = "Internal Support" });
                    lstResourceType.Add(new SelectListItem { Value = "Interns", Text = "Interns" });
                    lstResourceType.Add(new SelectListItem { Value = "Management", Text = "Management" });
                    lstResourceType.Add(new SelectListItem { Value = "Shadow", Text = "Shadow" });
                    lstResourceType.Add(new SelectListItem { Value = "Solution Development", Text = "Solution Development" });
                    //lstResourceType.Add(new SelectListItem { Value = "Billed", Text = "Billed" });
                    //lstResourceType.Add(new SelectListItem { Value = "Internal", Text = "Internal" });
                    //lstResourceType.Add(new SelectListItem { Value = "Shadow", Text = "Shadow" });
                    //lstResourceType.Add(new SelectListItem { Value = "Support", Text = "Support" });
                    //lstResourceType.Add(new SelectListItem { Value = "Investment", Text = "Investment" });
                    //lstResourceType.Add(new SelectListItem { Value = "Delivery", Text = "Delivery" });
                    //lstResourceType.Add(new SelectListItem { Value = "BSA", Text = "BSA" });
                    //lstResourceType.Add(new SelectListItem { Value = "Business Operations", Text = "Business Operations" });
                    /////New status added as per BO requirment
                    //lstResourceType.Add(new SelectListItem { Value = "ESS", Text = "ESS" });
                    //lstResourceType.Add(new SelectListItem { Value = "Solution Development", Text = "Solution Development" });
                    //lstResourceType.Add(new SelectListItem { Value = "Bench", Text = "Bench" });
                    //lstResourceType.Add(new SelectListItem { Value = "Presales", Text = "Presales" });
                    //lstResourceType.Add(new SelectListItem { Value = "Account Ops", Text = "Account Ops" });
                    //lstResourceType.Add(new SelectListItem { Value = "Management", Text = "Management" });
                    //lstResourceType.Add(new SelectListItem { Value = "Practice Delivery", Text = "Practice Delivery" });
                    //lstResourceType.Add(new SelectListItem { Value = "Practice Support", Text = "Practice Support" });
                    //lstResourceType.Add(new SelectListItem { Value = "Internal Application", Text = "Internal Application" });
                    //lstResourceType.Add(new SelectListItem { Value = "Interns", Text = "Interns" });

                    ViewBag.ResourceType = lstResourceType;

                    #endregion

                    #region Bind Utilization in dropdownlist

                    List<SelectListItem> lstUtilization = new List<SelectListItem>();
                    lstUtilization.Add(new SelectListItem { Value = "25", Text = "25" });
                    lstUtilization.Add(new SelectListItem { Value = "50", Text = "50" });
                    lstUtilization.Add(new SelectListItem { Value = "75", Text = "75" });
                    lstUtilization.Add(new SelectListItem { Value = "100", Text = "100" });

                    ViewBag.Utilization = lstUtilization;
                    var AllOrgTypes = (from pro in db.OrganisationGroups
                                       select new SelectListItem
                                       {
                                           Value = pro.OrganisationGroup1,
                                           Text = pro.OrganisationGroup1

                                       }).Distinct().ToList();
                    ViewBag.OrgType = AllOrgTypes;

                    var AllOrgSubTypes = (from pro in db.OrganisationGroups

                                          select new SelectListItem
                                          {
                                              Value = pro.OrganisationSubGroup,
                                              Text = pro.OrganisationSubGroup

                                          }).Distinct().ToList();
                    ViewBag.OrgSubType = AllOrgSubTypes;
                    #endregion

                    return View();
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
        public ActionResult GetEmployeeInfobyPractice(int? EmpCode, bool InActive)
        {
            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            {

                try
                {
                    var AllProjects = (from pro in db.Projects
                                      // where pro.IsActive == true
                                      where pro.IsActive == true && pro.SOWEndDate >= DateTime.UtcNow
                                       select new SelectListItem
                                       {
                                           Value = pro.ProjectCode,
                                           Text = pro.ProjectCode + " " + pro.ProjectName
                                       }).OrderBy(o => o.Text).ToList();
                    ViewBag.AllProjects = AllProjects;

                    #region Bind Resource Type in dropdownlist

                    List<SelectListItem> lstResourceType = new List<SelectListItem>();
                    lstResourceType.Add(new SelectListItem { Value = "Bench", Text = "Bench" });
                    lstResourceType.Add(new SelectListItem { Value = "Billed", Text = "Billed" });
                    lstResourceType.Add(new SelectListItem { Value = "Internal Support", Text = "Internal Support" });
                    lstResourceType.Add(new SelectListItem { Value = "Interns", Text = "Interns" });
                    lstResourceType.Add(new SelectListItem { Value = "Management", Text = "Management" });
                    lstResourceType.Add(new SelectListItem { Value = "Shadow", Text = "Shadow" });
                    lstResourceType.Add(new SelectListItem { Value = "Solution Development", Text = "Solution Development" });
                    //lstResourceType.Add(new SelectListItem { Value = "Billed", Text = "Billed" });
                    //lstResourceType.Add(new SelectListItem { Value = "Internal", Text = "Internal" });
                    //lstResourceType.Add(new SelectListItem { Value = "Shadow", Text = "Shadow" });
                    //lstResourceType.Add(new SelectListItem { Value = "Support", Text = "Support" });
                    //lstResourceType.Add(new SelectListItem { Value = "Investment", Text = "Investment" });
                    //lstResourceType.Add(new SelectListItem { Value = "Delivery", Text = "Delivery" });
                    //lstResourceType.Add(new SelectListItem { Value = "BSA", Text = "BSA" });
                    //lstResourceType.Add(new SelectListItem { Value = "Business Operations", Text = "Business Operations" });
                    /////New status added as per BO requirment
                    //lstResourceType.Add(new SelectListItem { Value = "ESS", Text = "ESS" });
                    //lstResourceType.Add(new SelectListItem { Value = "Solution Development", Text = "Solution Development" });
                    //lstResourceType.Add(new SelectListItem { Value = "Bench", Text = "Bench" });
                    //lstResourceType.Add(new SelectListItem { Value = "Presales", Text = "Presales" });
                    //lstResourceType.Add(new SelectListItem { Value = "Account Ops", Text = "Account Ops" });
                    //lstResourceType.Add(new SelectListItem { Value = "Management", Text = "Management" });
                    //lstResourceType.Add(new SelectListItem { Value = "Practice Delivery", Text = "Practice Delivery" });
                    //lstResourceType.Add(new SelectListItem { Value = "Practice Support", Text = "Practice Support" });
                    //lstResourceType.Add(new SelectListItem { Value = "Internal Application", Text = "Internal Application" });
                    //lstResourceType.Add(new SelectListItem { Value = "Interns", Text = "Interns" });

                    ViewBag.ResourceType = lstResourceType;
                    var AllOrgTypes = (from pro in db.OrganisationGroups
                                       select new SelectListItem
                                       {
                                           Value = pro.OrganisationGroup1,
                                           Text = pro.OrganisationGroup1

                                       }).Distinct().OrderBy(o => o.Text).ToList();
                    ViewBag.OrgType = AllOrgTypes;

                    var AllOrgSubTypes = (from pro in db.OrganisationGroups

                                          select new SelectListItem
                                          {
                                              Value = pro.OrganisationSubGroup,
                                              Text = pro.OrganisationSubGroup

                                          }).Distinct().OrderBy(o => o.Text).ToList();
                    ViewBag.OrgSubType = AllOrgSubTypes;
                    #endregion

                    var js = new BenchAssignment();
                    var empdetails = new BenchAssignment();

                    //string code = string.Empty;
                    int? code = null;
                    string practice = string.Empty;



                    if (EmpCode != null)
                    {
                        code = EmpCode;

                    }

                    js.Getemployeedetails = (from emp in db.Employees
                                             where emp.EmployeeId == code
                                             select new Employeedetails
                                             {
                                                 EmployeeName = emp.FirstName + " " + emp.LastName,
                                                 EmployeeCode = emp.EmployeeId.ToString(),
                                                 Grade = emp.Grade,
                                                 Designation = emp.Designation
                                             }).ToList();
                    if (InActive == false)
                    {
                        js.AssignmentEmployees = (
                                             from pro in db.Projects
                                             join pa in db.ProjectAssignments on pro.ProjectCode equals pa.ProjectCode
                                             join emp in db.Employees on pa.EmployeeId equals emp.EmployeeId
                                             where (pa.IsActive == true)
                                             //&& pro.ProjectCode == code && emp.IsActive == true
                                             && pa.EmployeeId == code && emp.IsActive == true
                                             select new EmployeeDetailsPractiseWiseAssignment
                                             {
                                                 EmployeeName = emp.FirstName + " " + emp.LastName,
                                                 EmployeeCode = emp.EmployeeId.ToString(),
                                                 Grade = emp.Grade,
                                                 Manager = pro.ProjectManager,
                                                 Utilization = (pa.Utilization),
                                                 Assignment_Id = pa.Assignment_Id,
                                                 Designation = emp.Designation,
                                                 ProjectName = pa.ProjectName,
                                                 ProjectCode = pa.ProjectCode,
                                                 StartDate = pa.StartDate,
                                                 EndDate = pa.EndDate,
                                                 BillingStatus = pa.BillingStatus,
                                                 OrgGroup = pa.OrganisationGroup,
                                                 OrgSubGroup = pa.OrganisationSubGroup,
                                                 IsActive =  pa.IsActive
                                             }
                       //).GroupBy(x => x.EmployeeCode).Select(x => x.FirstOrDefault()).ToList();
                       ).ToList();
                    }
                    else
                    {
                        js.AssignmentEmployees = (
                                             from pro in db.Projects
                                             join pa in db.ProjectAssignments on pro.ProjectCode equals pa.ProjectCode
                                             join emp in db.Employees on pa.EmployeeId equals emp.EmployeeId
                                             where (pa.IsActive == false)
                                             //&& pro.ProjectCode == code && emp.IsActive == true
                                             && pa.EmployeeId == code && emp.IsActive == true
                                             select new EmployeeDetailsPractiseWiseAssignment
                                             {
                                                 EmployeeName = emp.FirstName + " " + emp.LastName,
                                                 EmployeeCode = emp.EmployeeId.ToString(),
                                                 Grade = emp.Grade,
                                                 Manager = pro.ProjectManager,
                                                 Utilization = (pa.Utilization),
                                                 Assignment_Id = pa.Assignment_Id,
                                                 Designation = emp.Designation,
                                                 ProjectName = pa.ProjectName,
                                                 ProjectCode = pa.ProjectCode,
                                                 StartDate = pa.StartDate,
                                                 EndDate = pa.EndDate,
                                                 BillingStatus = pa.BillingStatus,
                                                 OrgGroup = pa.OrganisationGroup,
                                                 OrgSubGroup = pa.OrganisationSubGroup,
                                                 IsActive = pa.IsActive
                                             }
                       //).GroupBy(x => x.EmployeeCode).Select(x => x.FirstOrDefault()).ToList();
                       ).ToList();
                    }
                  
                    if (InActive == true)
                    {
                        ViewBag.IsInactive = true;
                    }
                    else
                    {
                        ViewBag.IsInactive = false;
                    }         
                    int? totalutil = js.AssignmentEmployees.Sum(p => p.Utilization);
                    ViewBag.totutilization = totalutil;
                    //Session["totalutilization"] = totalutil;

                    ViewBag.EmpCode = js.Getemployeedetails[0].EmployeeCode;
                    ViewBag.EmpName = js.Getemployeedetails[0].EmployeeName;

                    ViewBag.Designation = js.Getemployeedetails[0].Designation;
                    ViewBag.Grade = js.Getemployeedetails[0].Grade;

                    return View("Index", js);
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
        //public ActionResult GetEmployeeInfobyPractice(int? EmpCode)
        //{
        //    if (Convert.ToInt32(Session["EmployeeId"]) > 0)
        //    {

        //        try
        //        {
        //            var AllProjects = (from pro in db.Projects
        //                               where pro.IsActive == true
        //                               select new SelectListItem
        //                               {
        //                                   Value = pro.ProjectCode,
        //                                   Text = pro.ProjectCode + " " + pro.ProjectName
        //                               }).OrderBy(o => o.Text).ToList();
        //            ViewBag.AllProjects = AllProjects;

        //            #region Bind Resource Type in dropdownlist

        //            List<SelectListItem> lstResourceType = new List<SelectListItem>();
        //            lstResourceType.Add(new SelectListItem { Value = "Billed", Text = "Billed" });
        //            lstResourceType.Add(new SelectListItem { Value = "Internal", Text = "Internal" });
        //            lstResourceType.Add(new SelectListItem { Value = "Shadow", Text = "Shadow" });
        //            lstResourceType.Add(new SelectListItem { Value = "Support", Text = "Support" });
        //            lstResourceType.Add(new SelectListItem { Value = "Investment", Text = "Investment" });
        //            lstResourceType.Add(new SelectListItem { Value = "Delivery", Text = "Delivery" });
        //            lstResourceType.Add(new SelectListItem { Value = "BSA", Text = "BSA" });
        //            lstResourceType.Add(new SelectListItem { Value = "Business Operations", Text = "Business Operations" });
        //            ///New status added as per BO requirment
        //            lstResourceType.Add(new SelectListItem { Value = "ESS", Text = "ESS" });
        //            lstResourceType.Add(new SelectListItem { Value = "Solution Development", Text = "Solution Development" });
        //            lstResourceType.Add(new SelectListItem { Value = "Bench", Text = "Bench" });
        //            lstResourceType.Add(new SelectListItem { Value = "Presales", Text = "Presales" });
        //            lstResourceType.Add(new SelectListItem { Value = "Account Ops", Text = "Account Ops" });
        //            lstResourceType.Add(new SelectListItem { Value = "Management", Text = "Management" });
        //            lstResourceType.Add(new SelectListItem { Value = "Practice Delivery", Text = "Practice Delivery" });
        //            lstResourceType.Add(new SelectListItem { Value = "Practice Support", Text = "Practice Support" });
        //            lstResourceType.Add(new SelectListItem { Value = "Internal Application", Text = "Internal Application" });
        //            lstResourceType.Add(new SelectListItem { Value = "Interns", Text = "Interns" });

        //            ViewBag.ResourceType = lstResourceType;
        //            var AllOrgTypes = (from pro in db.OrganisationGroups
        //                               select new SelectListItem
        //                               {
        //                                   Value = pro.OrganisationGroup1,
        //                                   Text = pro.OrganisationGroup1

        //                               }).Distinct().OrderBy(o => o.Text).ToList();
        //            ViewBag.OrgType = AllOrgTypes;

        //            var AllOrgSubTypes = (from pro in db.OrganisationGroups

        //                                  select new SelectListItem
        //                                  {
        //                                      Value = pro.OrganisationSubGroup,
        //                                      Text = pro.OrganisationSubGroup

        //                                  }).Distinct().OrderBy(o => o.Text).ToList();
        //            ViewBag.OrgSubType = AllOrgSubTypes;
        //            #endregion

        //            var js = new BenchAssignment();
        //            var empdetails = new BenchAssignment();

        //            //string code = string.Empty;
        //            int? code = null;
        //            string practice = string.Empty;



        //            if (EmpCode != null)
        //            {
        //                code = EmpCode;

        //            }

        //            js.Getemployeedetails = (from emp in db.Employees
        //                                     where emp.EmployeeId == code
        //                                     select new Employeedetails
        //                                     {
        //                                         EmployeeName = emp.FirstName + " " + emp.LastName,
        //                                         EmployeeCode = emp.EmployeeId.ToString(),
        //                                         Grade = emp.Grade,
        //                                         Designation = emp.Designation
        //                                     }).ToList();

        //            js.AssignmentEmployees = (
        //                                 from pro in db.Projects
        //                                 join pa in db.ProjectAssignments on pro.ProjectCode equals pa.ProjectCode
        //                                 join emp in db.Employees on pa.EmployeeId equals emp.EmployeeId
        //                                 where (pa.IsActive == true)
        //                                 //&& pro.ProjectCode == code && emp.IsActive == true
        //                                 && pa.EmployeeId == code && emp.IsActive == true
        //                                 select new EmployeeDetailsPractiseWiseAssignment
        //                                 {
        //                                     EmployeeName = emp.FirstName + " " + emp.LastName,
        //                                     EmployeeCode = emp.EmployeeId.ToString(),
        //                                     Grade = emp.Grade,
        //                                     Manager = pro.ProjectManager,
        //                                     Utilization = (pa.Utilization),
        //                                     Assignment_Id = pa.Assignment_Id,
        //                                     Designation = emp.Designation,
        //                                     ProjectName = pa.ProjectName,
        //                                     ProjectCode = pa.ProjectCode,
        //                                     StartDate = pa.StartDate,
        //                                     EndDate = pa.EndDate,
        //                                     BillingStatus = pa.BillingStatus,
        //                                     OrgGroup = pa.OrganisationGroup,
        //                                     OrgSubGroup = pa.OrganisationSubGroup
        //                                 }
        //           //).GroupBy(x => x.EmployeeCode).Select(x => x.FirstOrDefault()).ToList();
        //           ).ToList();
        //            //GetAllPractices();

        //            int? totalutil = js.AssignmentEmployees.Sum(p => p.Utilization);
        //            ViewBag.totutilization = totalutil;
        //            //Session["totalutilization"] = totalutil;

        //            ViewBag.EmpCode = js.Getemployeedetails[0].EmployeeCode;
        //            ViewBag.EmpName = js.Getemployeedetails[0].EmployeeName;

        //            ViewBag.Designation = js.Getemployeedetails[0].Designation;
        //            ViewBag.Grade = js.Getemployeedetails[0].Grade;

        //            return View("Index", js);
        //        }
        //        catch (Exception ex)
        //        {
        //            Common.WriteExceptionErrorLog(ex);
        //            return RedirectToAction("Error", "Error");
        //            //return Json("Error", JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //    else
        //    {
        //        //ermsg = "Session expired";
        //        //return Json("Sessionexpired", JsonRequestBehavior.AllowGet);
        //        return RedirectToAction("SessionExpire", "Signout");
        //    }
        //}
        public ActionResult GetOrganisationSubGroup(string Organisationgroup)
        {
            try
            {
                // Common.WriteErrorLog("GetSkillCode Method Inside" + SkillCluster);
                var orgsubgroup = (from data in db.OrganisationGroups.Where(x => x.OrganisationGroup1.ToLower().Equals(Organisationgroup.Trim().ToLower()))
                                   select new SelectListItem
                                   {
                                       Value = data.OrganisationSubGroup,
                                       Text = data.OrganisationSubGroup
                                   }).Distinct().OrderBy(o => o.Text).ToList();
                ViewBag.OrgSubType = orgsubgroup;

                return Json(orgsubgroup, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                Common.WriteExceptionErrorLog(ex);
                return Json("Error", JsonRequestBehavior.AllowGet);


            }
        }
        public List<SelectListItem> GetProjects()
        {
            var AllProjects = (from pro in db.Projects
                             // where pro.IsActive == true
                              where pro.IsActive == true && pro.SOWEndDate >= DateTime.UtcNow
                               select new SelectListItem
                               {
                                   Value = pro.ProjectCode,
                                   Text = pro.ProjectCode + " " + pro.ProjectName
                               }).OrderBy(o => o.Text).ToList();
            ViewBag.AllProjects = AllProjects;
            return (AllProjects);

        }
        public ActionResult InsertEmployeeAssignment(List<EmployeeDetailsPractiseWiseAssignment> employeesDetails)
        {
            string ermsg = "";
            string sucessmsg = "";
            int sumUtilizationEmployee;
            JsonResult js = new JsonResult();
            if (Convert.ToInt32(Session["EmployeeId"]) > 0)

            {
                try
                {

                    Int32 empCode = Convert.ToInt32(employeesDetails[0].EmployeeCode);
                    var objprasoneActive = db.ProjectAssignments.Where(b => b.EmployeeId == empCode && b.IsActive == true).ToList();
                    foreach (var eachEmployee in employeesDetails)
                    {

                        Int32 EmpID = Convert.ToInt32(eachEmployee.EmployeeCode);
                        Employee objEmp = db.Employees.Where(e => e.EmployeeId.Equals(EmpID)).FirstOrDefault();

                        if (objEmp != null)
                        {
                            ProjectAssignment objProjectAssignment = new ProjectAssignment();

                            objProjectAssignment.ProjectName = db.Projects.Where(p => p.ProjectCode == eachEmployee.ProjectName).FirstOrDefault().ProjectName;
                            objProjectAssignment.ProjectID = db.Projects.Where(p => p.ProjectCode.Equals(eachEmployee.ProjectName)).FirstOrDefault().ProjectId;
                            objProjectAssignment.EmployeeId = objEmp.EmployeeId;
                            objProjectAssignment.StartDate = Convert.ToDateTime(eachEmployee.StartDate);
                            //objProjectAssignment.EndDate = Convert.ToDateTime(eachEmployee.EndDate);
                            //Need to add condition for projectassignment enddate should not be greater than  Project enddate 
                            var projectEndDate = db.Projects.Where(pa => pa.ProjectCode == eachEmployee.ProjectName && pa.IsActive == true).Select(pa => pa.SOWEndDate).FirstOrDefault();
                            if (eachEmployee.EndDate > projectEndDate)
                            {
                                js.Data = "ProjectEndDate";
                                js.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
                                // return js;
                                return Json(js.Data, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                objProjectAssignment.EndDate = Convert.ToDateTime(eachEmployee.EndDate);
                            }
                            objProjectAssignment.Utilization = Convert.ToInt32(eachEmployee.Utilization);
                            //  objProjectAssignment.IsActive = true;

                          //  newly added on 8th june
                            if (eachEmployee.StartDate > DateTime.UtcNow.Date)
                            {
                                objProjectAssignment.IsActive = false;
                            }
                            else
                            {
                                objProjectAssignment.IsActive = true;
                            }
                           // newly added on 8th june

                            objProjectAssignment.Assigned_By = Convert.ToString(Session["EmployeeId"]);
                            objProjectAssignment.Assigned_Date = DateTime.UtcNow;
                            objProjectAssignment.ProjectCode = db.Projects.Where(p => p.ProjectCode == eachEmployee.ProjectName).FirstOrDefault().ProjectCode;
                            objProjectAssignment.BillingStatus = eachEmployee.resourceType;
                            objProjectAssignment.CreatedDate = DateTime.UtcNow;
                            objProjectAssignment.OrganisationGroup = eachEmployee.OrgGroup;
                            objProjectAssignment.OrganisationSubGroup = eachEmployee.OrgSubGroup;

                            db.ProjectAssignments.Add(objProjectAssignment);

                            // For Saving in project history table

                            ProjectAssignmenthistory pash = new ProjectAssignmenthistory();

                            pash.AssignmentId = 0;
                            pash.ProjectCode = db.Projects.Where(p => p.ProjectCode == eachEmployee.ProjectName).FirstOrDefault().ProjectCode;
                            pash.ProjectName = db.Projects.Where(p => p.ProjectCode == eachEmployee.ProjectName).FirstOrDefault().ProjectName;
                            pash.ProjectID = db.Projects.Where(p => p.ProjectCode.Equals(eachEmployee.ProjectName)).FirstOrDefault().ProjectId;
                            pash.EmployeeId = objEmp.EmployeeId;
                            pash.Assigned_ByOld = null;
                            pash.BillingStatusOld = null;
                            pash.EnddateOld = null;
                            pash.IsActiveOld = null;
                            pash.StartDateOld = null;
                            pash.UtilizationOld = null;
                            pash.modifiedBy = EmpID;
                            pash.ModifiedDate = DateTime.UtcNow;
                            pash.UtilizationNew = Convert.ToInt32(eachEmployee.Utilization);
                            pash.StartDateNew = Convert.ToDateTime(eachEmployee.StartDate);
                            pash.IsActiveNew = true;
                            pash.EndDateNew = Convert.ToDateTime(eachEmployee.EndDate);
                            pash.BillingStatusNew = eachEmployee.resourceType;
                            pash.Assigned_byNew = Convert.ToString(Session["EmployeeId"]);

                            db.ProjectAssignmenthistories.Add(pash);

                            db.SaveChanges();

                            sucessmsg = "Data Saved successfully";
                        }

                    }

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
                return Json("Sessionexpired", JsonRequestBehavior.AllowGet);

            }

            return Json(ermsg, JsonRequestBehavior.AllowGet);

        }
        public ActionResult ProjectAutoAssignment(List<BenchAssignment> benchAssignments)
        {
            string ermsg = "";

            return Json(ermsg, JsonRequestBehavior.AllowGet);

        }
        public ActionResult GetEmployeeUtilization(string EmpCode)
        {
            int intempCode = Convert.ToInt32(EmpCode);
            var objprasoneActive = db.ProjectAssignments.Where(b => b.EmployeeId == intempCode && b.IsActive == true).ToList();
            Employee employeedata = new Employee();
            employeedata = db.Employees.Where(b => b.EmployeeId == intempCode).FirstOrDefault();
            int totalutil = Convert.ToInt32(objprasoneActive.Sum(p => p.Utilization));
            var x = objprasoneActive.Max(y => y.Utilization);
            ProjectAssignment prj = new ProjectAssignment();
            ProjectAssignment prjdata = new ProjectAssignment();
            prj = (from job in db.ProjectAssignments
                   where job.EmployeeId == intempCode && job.Utilization == db.ProjectAssignments.Where(p => p.EmployeeId == intempCode).Max(p => p.Utilization)
                   select job).FirstOrDefault();
            prjdata.Utilization = totalutil;
            prjdata.ResourcePerformance = employeedata.FirstName + ' ' + employeedata.LastName;
            //if (prj != null)
            //{
            //    prjdata.OrganisationGroup = prj.OrganisationGroup;
            //    prjdata.OrganisationSubGroup = prj.OrganisationSubGroup;
            //}
            //else
            //{
            //    prjdata.OrganisationGroup = "";
            //    prjdata.OrganisationSubGroup = "";
            //}
            return Json(prjdata, JsonRequestBehavior.AllowGet);
            // return totalutil;
        }
        public string GetEmployeeId(string EmpCode)
        {
            int intempCode = Convert.ToInt32(EmpCode);
            var objprasoneActive = db.Employees.Where(b => b.EmployeeId == intempCode && b.IsActive == true).FirstOrDefault();


            if (objprasoneActive != null)
            {
                return "true";
            }
            else
            {
                return "false";
            }
        }
        public ActionResult ResourceRelease()
        {
            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            {
                try
                {
                    GetProjects();

                    #region Bind Utilization in dropdownlist
                    List<SelectListItem> lstUtilization = new List<SelectListItem>();
                    lstUtilization.Add(new SelectListItem { Value = "25", Text = "25" });
                    lstUtilization.Add(new SelectListItem { Value = "50", Text = "50" });
                    lstUtilization.Add(new SelectListItem { Value = "75", Text = "75" });
                    lstUtilization.Add(new SelectListItem { Value = "100", Text = "100" });
                    ViewBag.Utilization = lstUtilization;
                    #endregion

                    #region Bind Performance Rating  in dropdownlist
                    List<SelectListItem> lstperformance = new List<SelectListItem>();
                    lstperformance.Add(new SelectListItem { Value = "Needs Improvement", Text = "Billed" });
                    lstperformance.Add(new SelectListItem { Value = "Below Expectations", Text = "Internal" });
                    lstperformance.Add(new SelectListItem { Value = "Meet Expectations", Text = "Shadow" });
                    lstperformance.Add(new SelectListItem { Value = "Exceeds Expectations", Text = "Support" });
                    lstperformance.Add(new SelectListItem { Value = "Significantly Exceeds Expectations", Text = "Investment" });
                    ViewBag.PerformanceType = lstperformance;
                    #endregion

                    #region Bind Client/Project   in dropdownlist
                    List<SelectListItem> lstclientproject = new List<SelectListItem>();
                    lstclientproject.Add(new SelectListItem { Value = "Client", Text = "Client" });
                    lstclientproject.Add(new SelectListItem { Value = "Project", Text = "Project" });
                    ViewBag.ClientOrProject = lstclientproject;
                    #endregion

                    #region Bind ProjectMovement Rating  in dropdownlist
                    List<SelectListItem> lstProjectMovemnt = new List<SelectListItem>();
                    lstProjectMovemnt.Add(new SelectListItem { Value = "Yes", Text = "Yes" });
                    lstProjectMovemnt.Add(new SelectListItem { Value = "No", Text = "No" });
                    ViewBag.ProjectMovement = lstProjectMovemnt;
                    #endregion
                    return View();
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
        public ActionResult GetEmployeeInfoData(int? EmpCode)
        {
            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            {

                try
                {
                    var AllProjects = (from pro in db.Projects
                                     //  where pro.IsActive == true
                                      where pro.IsActive == true && pro.SOWEndDate >= DateTime.UtcNow
                                       select new SelectListItem
                                       {
                                           Value = pro.ProjectCode,
                                           Text = pro.ProjectCode + " " + pro.ProjectName
                                       }).OrderBy(o => o.Text).ToList();
                    ViewBag.AllProjects = AllProjects;

                    #region Bind Performance Rating  in dropdownlist
                    List<SelectListItem> lstperformance = new List<SelectListItem>();
                    lstperformance.Add(new SelectListItem { Value = "Needs Improvement", Text = "Needs Improvement" });
                    lstperformance.Add(new SelectListItem { Value = "Below Expectations", Text = "Below Expectations" });
                    lstperformance.Add(new SelectListItem { Value = "Meet Expectations", Text = "Meet Expectations" });
                    lstperformance.Add(new SelectListItem { Value = "Exceeds Expectations", Text = "Exceeds Expectations" });
                    lstperformance.Add(new SelectListItem { Value = "Significantly Exceeds Expectations", Text = "Significantly Exceeds Expectations" });
                    ViewBag.PerformanceType = lstperformance;
                    #endregion

                    #region Bind Performance Rating  in dropdownlist
                    List<SelectListItem> lstclientproject = new List<SelectListItem>();
                    lstclientproject.Add(new SelectListItem { Value = "Client", Text = "Client" });
                    lstclientproject.Add(new SelectListItem { Value = "Project", Text = "Project" });
                    ViewBag.ClientOrProject = lstclientproject;
                    #endregion

                    #region Bind Performance Rating  in dropdownlist
                    List<SelectListItem> lstProjectMovemnt = new List<SelectListItem>();
                    lstProjectMovemnt.Add(new SelectListItem { Value = "Yes", Text = "Yes" });
                    lstProjectMovemnt.Add(new SelectListItem { Value = "No", Text = "No" });
                    ViewBag.ProjectMovement = lstProjectMovemnt;
                    #endregion

                    var js = new BenchAssignment();
                    var empdetails = new BenchAssignment();
                    //string code = string.Empty;
                    int? code = null;
                    string practice = string.Empty;
                    if (EmpCode != null)
                    {
                        code = EmpCode;
                    }
                    js.Getemployeedetails = (from emp in db.Employees
                                             where emp.EmployeeId == code
                                             select new Employeedetails
                                             {
                                                 EmployeeName = emp.FirstName + " " + emp.LastName,
                                                 EmployeeCode = emp.EmployeeId.ToString(),
                                                 Grade = emp.Grade,
                                                 Designation = emp.Designation
                                             }).ToList();

                    js.AssignmentEmployees = (
                                         from pro in db.Projects
                                         join pa in db.ProjectAssignments on pro.ProjectCode equals pa.ProjectCode
                                         join emp in db.Employees on pa.EmployeeId equals emp.EmployeeId
                                         where (pa.IsActive == true)
                                         //&& pro.ProjectCode == code && emp.IsActive == true
                                         && pa.EmployeeId == code && emp.IsActive == true
                                         select new EmployeeDetailsPractiseWiseAssignment
                                         {
                                             EmployeeName = emp.FirstName + " " + emp.LastName,
                                             EmployeeCode = emp.EmployeeId.ToString(),
                                             Grade = emp.Grade,
                                             Manager = pro.ProjectManager,
                                             Utilization = (pa.Utilization),
                                             Assignment_Id = pa.Assignment_Id,
                                             Designation = emp.Designation,
                                             ProjectName = pa.ProjectName,
                                             ProjectCode = pa.ProjectCode,
                                             StartDate = pa.StartDate,
                                             EndDate = pa.EndDate,
                                             BillingStatus = pa.BillingStatus
                                         }).ToList();
                    int? totalutil = js.AssignmentEmployees.Sum(p => p.Utilization);
                    ViewBag.totutilization = totalutil;
                    ViewBag.EmpCode = js.Getemployeedetails[0].EmployeeCode;
                    ViewBag.EmpName = js.Getemployeedetails[0].EmployeeName;
                    ViewBag.Designation = js.Getemployeedetails[0].Designation;
                    ViewBag.Grade = js.Getemployeedetails[0].Grade;
                    return View("ResourceRelease", js);
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
        public ActionResult InsertResourceReleaseData(ProjectAssignment employeeDetails)
        {
            //string ermsg = "";
            string sucessmsg = "";
            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            {
                try
                {
                    Int32 empCode = Convert.ToInt32(employeeDetails.EmployeeId);
                    ProjectAssignment objdatadetails = new ProjectAssignment();
                    var objdata = db.ProjectAssignments.Where(x => x.Assignment_Id == employeeDetails.Assignment_Id && x.IsActive == true).FirstOrDefault();
                    if (objdata != null)
                    {
                        objdata.ReleaseDate = Convert.ToDateTime(employeeDetails.ReleaseDate);
                        objdata.ClientorProject = employeeDetails.ClientorProject;
                        objdata.ReasonForRelease = employeeDetails.ReasonForRelease;
                        objdata.IsDiscResourceOnProject = employeeDetails.IsDiscResourceOnProject;
                        objdata.SkillsGoodAt = employeeDetails.SkillsGoodAt;
                        objdata.DetailedFeedBack = employeeDetails.DetailedFeedBack;
                        objdata.ResourcePerformance = employeeDetails.ResourcePerformance;
                        objdata.ReleasedBy = Convert.ToInt32(Session["EmployeeId"]);
                        objdata.ReleaseAuditDate = DateTime.UtcNow;
                        db.Entry(objdata).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        //ViewData["message"] = "Description updated suscessfully...";
                    }
                    sucessmsg = "Data Saved successfully";
                    return Json(sucessmsg, JsonRequestBehavior.AllowGet);
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
                return Json("Sessionexpired", JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetProjectCode(int assignmentid, int empid)
        {
            ProjectAssignments data = new ProjectAssignments();
            var assdata = db.ProjectAssignments.Where(b => b.Assignment_Id == assignmentid && b.IsActive == true && b.EmployeeId == empid).FirstOrDefault();
            data.Utilization = assdata.Utilization;
            data.ProjectCode = assdata.ProjectCode;
            if (assdata.ReleaseDate != null)
            {
                data.ReleaseDate = assdata.ReleaseDate;
                data.ClientorProject = assdata.ClientorProject;
                data.ReasonForRelease = assdata.ReasonForRelease;
                data.ResourcePerformance = assdata.ResourcePerformance;
                data.SkillsGoodAt = assdata.SkillsGoodAt;
                data.IsDiscResourceOnProject = assdata.IsDiscResourceOnProject;
                data.DetailedFeedBack = assdata.DetailedFeedBack;
            }
            return Json(data, JsonRequestBehavior.AllowGet);
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
        public ActionResult GenerateResourceReleaseReport(string data)
        {
            DateTime releasedate = Convert.ToDateTime((DateTime.UtcNow).ToString("yyyy-MM-dd"));
            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            {
                try
                {
                    if (data == AllReleasedates)
                    {
                        //  var projectDetails = db.ProjectAssignments.Where(x => x.Assignment_Id == Assignmentid && x.IsActive == true).ToList();
                        var projectDetails = (
                                               from pa in db.ProjectAssignments
                                               join emp in db.Employees on pa.EmployeeId equals emp.EmployeeId
                                               where pa.ReleaseDate != null && emp.IsActive == true
                                               select new EmployeeDetailsPractiseWiseAssignment
                                               {

                                                   EmployeeName = emp.FirstName + " " + emp.LastName,
                                                   EmployeeCode = emp.EmployeeId.ToString(),
                                                   Designation = emp.Designation,
                                                   ProjectCode = pa.ProjectCode,
                                                   ProjectName = pa.ProjectName,
                                                   ReleaseDate = pa.ReleaseDate,
                                                   Assignment_Id = pa.Assignment_Id,
                                                   ClientorProject = pa.ClientorProject,
                                                   ReasonForRelease = pa.ReasonForRelease,
                                                   IsDiscResourceOnProject = pa.IsDiscResourceOnProject,
                                                   SkillsGoodAt = pa.SkillsGoodAt,
                                                   ResourcePerformance = pa.ResourcePerformance,
                                                   DetailedFeedBack = pa.DetailedFeedBack,
                                                   ReleasedBy = pa.ReleasedBy,
                                                   ReleaseAuditDate = pa.ReleaseAuditDate,
                                                   ReleaseByName = db.Employees.Where(b => b.EmployeeId == pa.ReleasedBy && b.IsActive == true).Select(x => x.FirstName + "" + x.LastName).FirstOrDefault(),

                                               }).ToList();
                        #region Export to Excel
                        using (ExcelPackage package = new ExcelPackage())
                        {
                            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Project Release");      /* observation*/
                            worksheet.TabColor = System.Drawing.Color.Green;
                            worksheet.DefaultRowHeight = 18f;
                            worksheet.Row(1).Height = 20f;
                            using (var range = worksheet.Cells[1, 1, 1, 17])
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
                            worksheet.Cells[1, 3].Value = "Designation";
                            worksheet.Cells[1, 4].Value = "Project Code";
                            worksheet.Cells[1, 5].Value = "Project Name";
                            worksheet.Cells[1, 6].Value = "Release Date";
                            worksheet.Cells[1, 7].Value = "Client/Project";
                            worksheet.Cells[1, 8].Value = "Reason for Release";
                            worksheet.Cells[1, 9].Value = "Have discussed with resource on project movement (Y/N)";
                            worksheet.Cells[1, 10].Value = "Skills Good At";
                            worksheet.Cells[1, 11].Value = "Detailed Feedback";
                            worksheet.Cells[1, 12].Value = "Resource Performance";
                            worksheet.Cells[1, 13].Value = "Released By";
                            worksheet.Cells[1, 14].Value = "Released Audit Date";

                            //for add width              //observation
                            worksheet.DefaultColWidth = 18f;

                            worksheet.Column(1).Width = 14f;
                            worksheet.Column(2).AutoFit(50f);

                            worksheet.Column(3).Width = 50f;
                            worksheet.Column(4).Width = 18f;
                            worksheet.Column(5).Width = 50f;
                            worksheet.Column(6).Width = 25f;

                            worksheet.Column(7).Width = 20f;
                            worksheet.Column(8).Width = 25f;
                            worksheet.Column(9).Width = 50f;
                            worksheet.Column(10).Width = 50f;
                            worksheet.Column(11).Width = 40f;
                            worksheet.Column(12).Width = 50f;
                            worksheet.Column(13).Width = 40f;
                            worksheet.Column(14).Width = 30f;
                            //observation


                            //Add the each row
                            for (int rowIndex = 0, row = 2; rowIndex < projectDetails.Count; rowIndex++, row++) // row indicates number of rows
                            {
                                worksheet.Cells[row, 1].Value = projectDetails[rowIndex].EmployeeCode;
                                worksheet.Cells[row, 2].Value = projectDetails[rowIndex].EmployeeName;
                                worksheet.Cells[row, 3].Value = projectDetails[rowIndex].Designation;
                                worksheet.Cells[row, 4].Value = projectDetails[rowIndex].ProjectCode;
                                worksheet.Cells[row, 5].Value = projectDetails[rowIndex].ProjectName;
                                // worksheet.Column(9).Width = 50f;
                                worksheet.Cells[row, 6].Value = projectDetails[rowIndex].ReleaseDate;
                                worksheet.Cells[row, 6].Style.Numberformat.Format = "dd-MMM-yyyy";
                                worksheet.Cells[row, 6].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                                worksheet.Cells[row, 7].Value = projectDetails[rowIndex].ClientorProject;
                                worksheet.Cells[row, 8].Value = projectDetails[rowIndex].ReasonForRelease;
                                worksheet.Cells[row, 9].Value = projectDetails[rowIndex].IsDiscResourceOnProject;

                                worksheet.Cells[row, 10].Value = projectDetails[rowIndex].SkillsGoodAt;
                                worksheet.Cells[row, 11].Value = projectDetails[rowIndex].DetailedFeedBack;
                                worksheet.Cells[row, 12].Value = projectDetails[rowIndex].ResourcePerformance;
                                worksheet.Cells[row, 13].Value = projectDetails[rowIndex].ReleaseByName;

                                worksheet.Cells[row, 14].Value = projectDetails[rowIndex].ReleaseAuditDate;
                                worksheet.Cells[row, 14].Style.Numberformat.Format = "dd-MMM-yyyy";
                                worksheet.Cells[row, 14].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                                if (row % 2 == 1)
                                {
                                    using (var range = worksheet.Cells[row, 1, row, 14])
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
                            Response.AddHeader("content-disposition", "attachment;filename=" + "Project Release Report" + DateTime.UtcNow.ToString("_dd-MMM-yyyy_hh-mm-ss") + ".xlsx");

                            Response.Charset = "";
                            Response.ContentType = "application/vnd.ms-excel";
                            //StringWriter sw = new StringWriter();
                            Response.BinaryWrite(fileBytes);
                            Response.End();
                        }

                        #endregion
                    }
                    else if (data == BeforeReleasedates)
                    {

                        //DateTime releasedate = Convert.ToDateTime((DateTime.UtcNow).ToString("yyyy-MM-dd"));
                        var projectDetails = (
                                           from pa in db.ProjectAssignments
                                           join emp in db.Employees on pa.EmployeeId equals emp.EmployeeId
                                           where pa.ReleaseDate <= releasedate && emp.IsActive == true
                                           select new EmployeeDetailsPractiseWiseAssignment
                                           {

                                               EmployeeName = emp.FirstName + " " + emp.LastName,
                                               EmployeeCode = emp.EmployeeId.ToString(),
                                               Designation = emp.Designation,
                                               ProjectCode = pa.ProjectCode,
                                               ProjectName = pa.ProjectName,
                                               ReleaseDate = pa.ReleaseDate,
                                               Assignment_Id = pa.Assignment_Id,
                                               ClientorProject = pa.ClientorProject,
                                               ReasonForRelease = pa.ReasonForRelease,
                                               IsDiscResourceOnProject = pa.IsDiscResourceOnProject,
                                               SkillsGoodAt = pa.SkillsGoodAt,
                                               ResourcePerformance = pa.ResourcePerformance,
                                               DetailedFeedBack = pa.DetailedFeedBack,
                                               ReleasedBy = pa.ReleasedBy,
                                               ReleaseAuditDate = pa.ReleaseAuditDate,
                                               ReleaseByName = db.Employees.Where(b => b.EmployeeId == pa.ReleasedBy && b.IsActive == true).Select(x => x.FirstName + "" + x.LastName).FirstOrDefault(),

                                           }).ToList();

                        #region Export to Excel
                        using (ExcelPackage package = new ExcelPackage())
                        {
                            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Project Release");                 /* observation*/
                            worksheet.TabColor = System.Drawing.Color.Green;
                            worksheet.DefaultRowHeight = 18f;
                            worksheet.Row(1).Height = 20f;
                            using (var range = worksheet.Cells[1, 1, 1, 17])
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
                            worksheet.Cells[1, 3].Value = "Designation";
                            worksheet.Cells[1, 4].Value = "Project Code";
                            worksheet.Cells[1, 5].Value = "Project Name";
                            worksheet.Cells[1, 6].Value = "Release Date";
                            worksheet.Cells[1, 7].Value = "Client/Project";
                            worksheet.Cells[1, 8].Value = "Reason for Release";
                            worksheet.Cells[1, 9].Value = "Have discussed with resource on project movement (Y/N)";
                            worksheet.Cells[1, 10].Value = "Skills Good At";
                            worksheet.Cells[1, 11].Value = "Detailed Feedback";
                            worksheet.Cells[1, 12].Value = "Resource Performance";
                            worksheet.Cells[1, 13].Value = "Released By";
                            worksheet.Cells[1, 14].Value = "Released Audit Date";
                            //for add width              //observation
                            worksheet.DefaultColWidth = 18f;
                            worksheet.Column(1).Width = 14f;
                            worksheet.Column(2).AutoFit(50f);
                            worksheet.Column(3).Width = 50f;
                            worksheet.Column(4).Width = 18f;
                            worksheet.Column(5).Width = 50f;
                            worksheet.Column(6).Width = 25f;
                            worksheet.Column(7).Width = 20f;
                            worksheet.Column(8).Width = 25f;
                            worksheet.Column(9).Width = 50f;
                            worksheet.Column(10).Width = 50f;
                            worksheet.Column(11).Width = 40f;
                            worksheet.Column(12).Width = 50f;
                            worksheet.Column(13).Width = 40f;
                            worksheet.Column(14).Width = 30f;
                            //observation
                            //Add the each row
                            for (int rowIndex = 0, row = 2; rowIndex < projectDetails.Count; rowIndex++, row++) // row indicates number of rows
                            {
                                worksheet.Cells[row, 1].Value = projectDetails[rowIndex].EmployeeCode;
                                worksheet.Cells[row, 2].Value = projectDetails[rowIndex].EmployeeName;
                                worksheet.Cells[row, 3].Value = projectDetails[rowIndex].Designation;
                                worksheet.Cells[row, 4].Value = projectDetails[rowIndex].ProjectCode;
                                worksheet.Cells[row, 5].Value = projectDetails[rowIndex].ProjectName;

                                worksheet.Cells[row, 6].Value = projectDetails[rowIndex].ReleaseDate;
                                worksheet.Cells[row, 6].Style.Numberformat.Format = "dd-MMM-yyyy";
                                worksheet.Cells[row, 6].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                                worksheet.Cells[row, 7].Value = projectDetails[rowIndex].ClientorProject;
                                worksheet.Cells[row, 8].Value = projectDetails[rowIndex].ReasonForRelease;
                                worksheet.Cells[row, 9].Value = projectDetails[rowIndex].IsDiscResourceOnProject;

                                worksheet.Cells[row, 10].Value = projectDetails[rowIndex].SkillsGoodAt;
                                worksheet.Cells[row, 11].Value = projectDetails[rowIndex].DetailedFeedBack;
                                worksheet.Cells[row, 12].Value = projectDetails[rowIndex].ResourcePerformance;
                                worksheet.Cells[row, 13].Value = projectDetails[rowIndex].ReleaseByName;

                                worksheet.Cells[row, 14].Value = projectDetails[rowIndex].ReleaseAuditDate;
                                worksheet.Cells[row, 14].Style.Numberformat.Format = "dd-MMM-yyyy";
                                worksheet.Cells[row, 14].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                                if (row % 2 == 1)
                                {
                                    using (var range = worksheet.Cells[row, 1, row, 14])
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
                            Response.AddHeader("content-disposition", "attachment;filename=" + "Project Release Report" + DateTime.UtcNow.ToString("_dd-MMM-yyyy_hh-mm-ss") + ".xlsx");

                            Response.Charset = "";
                            Response.ContentType = "application/vnd.ms-excel";
                            //StringWriter sw = new StringWriter();
                            Response.BinaryWrite(fileBytes);
                            Response.End();
                        }

                        #endregion
                    }
                    else if (data == AfterRelasedates)
                    {
                        var projectDetails = (
                                            from pa in db.ProjectAssignments
                                            join emp in db.Employees on pa.EmployeeId equals emp.EmployeeId
                                            where pa.ReleaseDate >= releasedate && emp.IsActive == true
                                            select new EmployeeDetailsPractiseWiseAssignment
                                            {

                                                EmployeeName = emp.FirstName + " " + emp.LastName,
                                                EmployeeCode = emp.EmployeeId.ToString(),
                                                Designation = emp.Designation,
                                                ProjectCode = pa.ProjectCode,
                                                ProjectName = pa.ProjectName,
                                                ReleaseDate = pa.ReleaseDate,
                                                Assignment_Id = pa.Assignment_Id,
                                                ClientorProject = pa.ClientorProject,
                                                ReasonForRelease = pa.ReasonForRelease,
                                                IsDiscResourceOnProject = pa.IsDiscResourceOnProject,
                                                SkillsGoodAt = pa.SkillsGoodAt,
                                                ResourcePerformance = pa.ResourcePerformance,
                                                DetailedFeedBack = pa.DetailedFeedBack,
                                                ReleasedBy = pa.ReleasedBy,
                                                ReleaseAuditDate = pa.ReleaseAuditDate,
                                                ReleaseByName = db.Employees.Where(b => b.EmployeeId == pa.ReleasedBy && b.IsActive == true).Select(x => x.FirstName + "" + x.LastName).FirstOrDefault(),

                                            }).ToList();
                        #region Export to Excel
                        using (ExcelPackage package = new ExcelPackage())
                        {
                            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Project Release");           /* observation*/
                            worksheet.TabColor = System.Drawing.Color.Green;
                            worksheet.DefaultRowHeight = 18f;
                            worksheet.Row(1).Height = 20f;
                            using (var range = worksheet.Cells[1, 1, 1, 17])
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
                            worksheet.Cells[1, 3].Value = "Designation";
                            worksheet.Cells[1, 4].Value = "Project Code";
                            worksheet.Cells[1, 5].Value = "Project Name";
                            worksheet.Cells[1, 6].Value = "Release Date";
                            worksheet.Cells[1, 7].Value = "Client/Project";
                            worksheet.Cells[1, 8].Value = "Reason for Release";
                            worksheet.Cells[1, 9].Value = "Have discussed with resource on project movement (Y/N)";
                            worksheet.Cells[1, 10].Value = "Skills Good At";
                            worksheet.Cells[1, 11].Value = "Detailed Feedback";
                            worksheet.Cells[1, 12].Value = "Resource Performance";
                            worksheet.Cells[1, 13].Value = "Released By";
                            worksheet.Cells[1, 14].Value = "Released Audit Date";
                            //for add width              //observation
                            worksheet.DefaultColWidth = 18f;
                            worksheet.Column(1).Width = 14f;
                            worksheet.Column(2).AutoFit(50f);
                            worksheet.Column(3).Width = 50f;
                            worksheet.Column(4).Width = 18f;
                            worksheet.Column(5).Width = 50f;
                            worksheet.Column(6).Width = 25f;
                            worksheet.Column(7).Width = 20f;
                            worksheet.Column(8).Width = 25f;
                            worksheet.Column(9).Width = 50f;
                            worksheet.Column(10).Width = 50f;
                            worksheet.Column(11).Width = 40f;
                            worksheet.Column(12).Width = 50f;
                            worksheet.Column(13).Width = 40f;
                            worksheet.Column(14).Width = 30f;
                            //observation
                            //Add the each row
                            for (int rowIndex = 0, row = 2; rowIndex < projectDetails.Count; rowIndex++, row++) // row indicates number of rows
                            {
                                worksheet.Cells[row, 1].Value = projectDetails[rowIndex].EmployeeCode;
                                worksheet.Cells[row, 2].Value = projectDetails[rowIndex].EmployeeName;
                                worksheet.Cells[row, 3].Value = projectDetails[rowIndex].Designation;
                                worksheet.Cells[row, 4].Value = projectDetails[rowIndex].ProjectCode;
                                worksheet.Cells[row, 5].Value = projectDetails[rowIndex].ProjectName;

                                worksheet.Cells[row, 6].Value = projectDetails[rowIndex].ReleaseDate;
                                worksheet.Cells[row, 6].Style.Numberformat.Format = "dd-MMM-yyyy";
                                worksheet.Cells[row, 6].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                                worksheet.Cells[row, 7].Value = projectDetails[rowIndex].ClientorProject;
                                worksheet.Cells[row, 8].Value = projectDetails[rowIndex].ReasonForRelease;
                                worksheet.Cells[row, 9].Value = projectDetails[rowIndex].IsDiscResourceOnProject;

                                worksheet.Cells[row, 10].Value = projectDetails[rowIndex].SkillsGoodAt;
                                worksheet.Cells[row, 11].Value = projectDetails[rowIndex].DetailedFeedBack;
                                worksheet.Cells[row, 12].Value = projectDetails[rowIndex].ResourcePerformance;
                                worksheet.Cells[row, 13].Value = projectDetails[rowIndex].ReleaseByName;

                                worksheet.Cells[row, 14].Value = projectDetails[rowIndex].ReleaseAuditDate;
                                worksheet.Cells[row, 14].Style.Numberformat.Format = "dd-MMM-yyyy";
                                worksheet.Cells[row, 14].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                                if (row % 2 == 1)
                                {
                                    using (var range = worksheet.Cells[row, 1, row, 14])
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
                            Response.AddHeader("content-disposition", "attachment;filename=" + "Project Release Report" + DateTime.UtcNow.ToString("_dd-MMM-yyyy_hh-mm-ss") + ".xlsx");

                            Response.Charset = "";
                            Response.ContentType = "application/vnd.ms-excel";
                            //StringWriter sw = new StringWriter();
                            Response.BinaryWrite(fileBytes);
                            Response.End();
                        }

                        #endregion
                    }
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
                return RedirectToAction("SessionExpire", "Signout");
            }
        }
        public ActionResult EmployeeAssignmentdetails(int id)
        {
            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            {
                try
                {
                    GetProjects();
                    #region Bind Resource Type in dropdownlist
                    List<SelectListItem> lstResourceType = new List<SelectListItem>();
                    lstResourceType.Add(new SelectListItem { Value = "Bench", Text = "Bench" });
                    lstResourceType.Add(new SelectListItem { Value = "Billed", Text = "Billed" });
                    lstResourceType.Add(new SelectListItem { Value = "Internal Support", Text = "Internal Support" });
                    lstResourceType.Add(new SelectListItem { Value = "Interns", Text = "Interns" });
                    lstResourceType.Add(new SelectListItem { Value = "Management", Text = "Management" });
                    lstResourceType.Add(new SelectListItem { Value = "Shadow", Text = "Shadow" });
                    lstResourceType.Add(new SelectListItem { Value = "Solution Development", Text = "Solution Development" });
                    //lstResourceType.Add(new SelectListItem { Value = "Billed", Text = "Billed" });
                    //lstResourceType.Add(new SelectListItem { Value = "Internal", Text = "Internal" });
                    //lstResourceType.Add(new SelectListItem { Value = "Shadow", Text = "Shadow" });
                    //lstResourceType.Add(new SelectListItem { Value = "Support", Text = "Support" });
                    //lstResourceType.Add(new SelectListItem { Value = "Investment", Text = "Investment" });
                    //lstResourceType.Add(new SelectListItem { Value = "Delivery", Text = "Delivery" });
                    //lstResourceType.Add(new SelectListItem { Value = "BSA", Text = "BSA" });
                    //lstResourceType.Add(new SelectListItem { Value = "Business Operations", Text = "Business Operations" });                    ///New status added as per BO requirment                    lstResourceType.Add(new SelectListItem { Value = "ESS", Text = "ESS" });                    lstResourceType.Add(new SelectListItem { Value = "Solution Development", Text = "Solution Development" });                    lstResourceType.Add(new SelectListItem { Value = "Bench", Text = "Bench" });                    lstResourceType.Add(new SelectListItem { Value = "Presales", Text = "Presales" });                    lstResourceType.Add(new SelectListItem { Value = "Account Ops", Text = "Account Ops" });                    lstResourceType.Add(new SelectListItem { Value = "Management", Text = "Management" });                    lstResourceType.Add(new SelectListItem { Value = "Practice Delivery", Text = "Practice Delivery" });                    
                    //lstResourceType.Add(new SelectListItem { Value = "Practice Support", Text = "Practice Support" }); lstResourceType.Add(new SelectListItem { Value = "Internal Application", Text = "Internal Application" }); lstResourceType.Add(new SelectListItem { Value = "Interns", Text = "Interns" });

                    ViewBag.ResourceType = lstResourceType;
                    #endregion
                    #region Bind Utilization in dropdownlist
                    List<SelectListItem> lstUtilization = new List<SelectListItem>();
                    lstUtilization.Add(new SelectListItem { Value = "25", Text = "25" });
                    lstUtilization.Add(new SelectListItem { Value = "50", Text = "50" });
                    lstUtilization.Add(new SelectListItem { Value = "75", Text = "75" });
                    lstUtilization.Add(new SelectListItem { Value = "100", Text = "100" });
                    ViewBag.Utilization = lstUtilization;
                    ViewBag.flag = true;
                    ViewBag.EmployeeId = id;       // Viewbag for storing data and pass the data from controller to view only one time.

                    #endregion
                    var AllOrgTypes = (from pro in db.OrganisationGroups
                                       select new SelectListItem
                                       {
                                           Value = pro.OrganisationGroup1,
                                           Text = pro.OrganisationGroup1

                                       }).Distinct().ToList();
                    ViewBag.OrgType = AllOrgTypes;

                    var AllOrgSubTypes = (from pro in db.OrganisationGroups

                                          select new SelectListItem
                                          {
                                              Value = pro.OrganisationSubGroup,
                                              Text = pro.OrganisationSubGroup

                                          }).Distinct().ToList();
                    ViewBag.OrgSubType = AllOrgSubTypes;

                    return View("Index");
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

        public ActionResult EditEmployeeAssignment(int Assignment_Id)
        {
            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            {
                try
                {
                    var AllOrgTypes = (from pro in db.OrganisationGroups
                                       select new SelectListItem
                                       {
                                           Value = pro.OrganisationGroup1,
                                           Text = pro.OrganisationGroup1

                                       }).Distinct().ToList();
                    ViewBag.OrgType = AllOrgTypes;

                    var AllOrgSubTypes = (from pro in db.OrganisationGroups

                                          select new SelectListItem
                                          {
                                              Value = pro.OrganisationSubGroup,
                                              Text = pro.OrganisationSubGroup

                                          }).Distinct().ToList();
                    ViewBag.OrgSubType = AllOrgSubTypes;
                    List<SelectListItem> lstResourceType = new List<SelectListItem>();
                    lstResourceType.Add(new SelectListItem { Value = "Bench", Text = "Bench" });
                    lstResourceType.Add(new SelectListItem { Value = "Billed", Text = "Billed" });
                    lstResourceType.Add(new SelectListItem { Value = "Internal Support", Text = "Internal Support" });
                    lstResourceType.Add(new SelectListItem { Value = "Interns", Text = "Interns" });
                    lstResourceType.Add(new SelectListItem { Value = "Management", Text = "Management" });
                    lstResourceType.Add(new SelectListItem { Value = "Shadow", Text = "Shadow" });
                    lstResourceType.Add(new SelectListItem { Value = "Solution Development", Text = "Solution Development" });
                    //lstResourceType.Add(new SelectListItem { Value = "Billed", Text = "Billed" });
                    //lstResourceType.Add(new SelectListItem { Value = "Internal", Text = "Internal" });
                    //lstResourceType.Add(new SelectListItem { Value = "Shadow", Text = "Shadow" });
                    //lstResourceType.Add(new SelectListItem { Value = "Support", Text = "Support" });
                    //lstResourceType.Add(new SelectListItem { Value = "Investment", Text = "Investment" });
                    //lstResourceType.Add(new SelectListItem { Value = "Delivery", Text = "Delivery" });
                    //lstResourceType.Add(new SelectListItem { Value = "BSA", Text = "BSA" });
                    //lstResourceType.Add(new SelectListItem { Value = "Business Operations", Text = "Business Operations" });
                    /////New status added as per BO requirment
                    //lstResourceType.Add(new SelectListItem { Value = "ESS", Text = "ESS" });
                    //lstResourceType.Add(new SelectListItem { Value = "Solution Development", Text = "Solution Development" });
                    //lstResourceType.Add(new SelectListItem { Value = "Bench", Text = "Bench" });
                    //lstResourceType.Add(new SelectListItem { Value = "Presales", Text = "Presales" });
                    //lstResourceType.Add(new SelectListItem { Value = "Account Ops", Text = "Account Ops" });
                    //lstResourceType.Add(new SelectListItem { Value = "Management", Text = "Management" });
                    //lstResourceType.Add(new SelectListItem { Value = "Practice Delivery", Text = "Practice Delivery" });
                    //lstResourceType.Add(new SelectListItem { Value = "Practice Support", Text = "Practice Support" });
                    //lstResourceType.Add(new SelectListItem { Value = "Internal Application", Text = "Internal Application" });
                    //lstResourceType.Add(new SelectListItem { Value = "Interns", Text = "Interns" });

                    ViewBag.ResourceType = lstResourceType;
                    //int empid = Convert.ToInt32(EmployeeId);
                  //  int assignment_Id = Assignment_Id;
                    ProjectAssignment data = new ProjectAssignment();
                    // data = db.ProjectAssignments.Where(k => k.EmployeeId == empid && k.Assignment_Id == assignment_Id && k.ProjectCode == Projectcode).FirstOrDefault();
                    data = db.ProjectAssignments.Where(k => k.Assignment_Id == Assignment_Id).FirstOrDefault();

                  
                    if (data.IsActive != null)
                    {
                        if (data.IsActive == true)
                        {
                            ViewBag.IsActive = true;
                        }
                        else
                        {
                            ViewBag.IsActive = false;
                        }
                    }
                    else
                    {
                        ViewBag.IsActive = null;
                    }

                    return Json(data, JsonRequestBehavior.AllowGet);
                    //return View(data);
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
               // return RedirectToAction("SessionExpire", "Signout");
            }
        }

        [HttpPost]
        public ActionResult EditEmployeeAssignmentData(ProjectAssignment assignmentdata)
        {
            string ermsg = "";
            string sucessmsg = "";
            int sumUtilizationEmployee;
            JsonResult js = new JsonResult();
            //   var somedata = formdata["IsChecked"];

            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            {
                try
                {
                    Int32 empCode = Convert.ToInt32(assignmentdata.EmployeeId);
                    ProjectAssignment objdatadetails = new ProjectAssignment();
                    var objdata = db.ProjectAssignments.Where(x => x.Assignment_Id == assignmentdata.Assignment_Id).FirstOrDefault();
                    if (objdata != null)
                    {
                        objdata.BillingStatus = assignmentdata.BillingStatus;
                        objdata.OrganisationGroup =assignmentdata.OrganisationGroup;
                        objdata.OrganisationSubGroup = assignmentdata.OrganisationSubGroup;
                        objdata.StartDate = assignmentdata.StartDate;
                        //objdata.EndDate = assignmentdata.EndDate;
                        //Need to add condition for projectassignment enddate should not be greater than  Project enddate 
                        var projectEndDate = db.Projects.Where(pa => pa.ProjectCode == assignmentdata.ProjectCode && pa.IsActive == true).Select(pa => pa.SOWEndDate).FirstOrDefault();
                        if (assignmentdata.EndDate > projectEndDate)
                        {
                            js.Data = "ProjectEndDate";
                            js.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
                            // return js;
                            return Json(js.Data, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            objdata.EndDate = assignmentdata.EndDate;
                        }

                        //  objdata.IsActive = assignmentdata.IsActive;
                        // objdata.Utilization = assignmentdata.Utilization;
                        //if (somedata == "1")
                        //{
                        //    objdata.IsActive = true;
                        //}
                        //else
                        //{
                        //    objdata.IsActive = false;
                        //}
                        objdata.Assigned_Date = DateTime.UtcNow;
                        objdata.ModifiedDate = DateTime.UtcNow;
                        db.Entry(objdata).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        sucessmsg = "Data Saved successfully";
                        //ViewData["message"] = "Description updated suscessfully...";
                    }
                    // sucessmsg = "Data Saved successfully";
                    //if (objdata.IsActive == true)
                    //{
                    //    return RedirectToAction("GetEmployeeInfobyPractice", new { EmpCode = assignmentdata.EmployeeId, InActive = false });
                    //}
                    //else
                    //{
                    //    return RedirectToAction("GetEmployeeInfobyPractice", new { EmpCode = assignmentdata.EmployeeId, InActive = true });
                    //}


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
                return Json("Sessionexpired", JsonRequestBehavior.AllowGet);
            }

            return Json(ermsg, JsonRequestBehavior.AllowGet);
        }
        public ActionResult DeleteEmployeeAssignment(string EmployeeId, string Assignment_Id, string Projectcode)
        {
            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            {
                try
                {
                    int empid = Convert.ToInt32(EmployeeId);
                    int assignment_Id = Convert.ToInt32(Assignment_Id);
                    ProjectAssignment dataToRemove = new ProjectAssignment();
                    dataToRemove = db.ProjectAssignments.Where(k => k.EmployeeId == empid && k.Assignment_Id == assignment_Id && k.ProjectCode == Projectcode).FirstOrDefault();



                    if (dataToRemove != null)
                    {
                        ProjectAssignmentsDel objProjectAssignment = new ProjectAssignmentsDel();



                        objProjectAssignment.ProjectName = dataToRemove.ProjectName;
                        objProjectAssignment.ProjectID = dataToRemove.ProjectID;
                        objProjectAssignment.EmployeeId = empid;
                        objProjectAssignment.StartDate = Convert.ToDateTime(dataToRemove.StartDate);
                        objProjectAssignment.EndDate = Convert.ToDateTime(dataToRemove.EndDate);
                        objProjectAssignment.Utilization = Convert.ToInt32(dataToRemove.Utilization);
                        objProjectAssignment.IsActive = true;
                        objProjectAssignment.Assigned_By = dataToRemove.Assigned_By;
                        objProjectAssignment.Assigned_Date = dataToRemove.Assigned_Date;
                        objProjectAssignment.Deleted_By = Convert.ToString(Session["EmployeeId"]);
                        objProjectAssignment.Deleted_Date = DateTime.UtcNow;
                        objProjectAssignment.ProjectCode = dataToRemove.ProjectCode;
                        objProjectAssignment.BillingStatus = dataToRemove.BillingStatus;
                        objProjectAssignment.CreatedDate = dataToRemove.CreatedDate;
                        objProjectAssignment.OrganisationGroup = dataToRemove.BillingStatus;
                        objProjectAssignment.OrganisationSubGroup = dataToRemove.OrganisationSubGroup;
                        objProjectAssignment.Assignment_Id = dataToRemove.Assignment_Id;
                        db.ProjectAssignmentsDels.Add(objProjectAssignment);
                    }
                    db.ProjectAssignments.Remove(dataToRemove);
                    db.SaveChanges();
                    //  }
                    return Json("true", JsonRequestBehavior.AllowGet);
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
    }

    public class EmployeeDetailsPractiseWiseAssignment
    {

        public string EmployeeName { get; set; }
        public string EmployeeCode { get; set; }
        public Nullable<int> Grade { get; set; }
        public string Manager { get; set; }
        public int Assignment_Id { get; set; }
        public int? Utilization { get; set; }
        public string Designation { get; set; }
        public List<Role> Role { get; set; }

        public string ProjectName { get; set; }
        public string ProjectCode { get; set; }

        public string resourceType { get; set; }

        public Nullable<System.DateTime> StartDate { get; set; }
        public Nullable<System.DateTime> EndDate { get; set; }
        public string BillingStatus { get; set; }

        //newly Added
        public Nullable<System.DateTime> ReleaseDate { get; set; }
        public string ClientorProject { get; set; }
        public string ReasonForRelease { get; set; }
        public string IsDiscResourceOnProject { get; set; }
        public string SkillsGoodAt { get; set; }
        public string DetailedFeedBack { get; set; }
        public string ResourcePerformance { get; set; }
        public Nullable<int> ReleasedBy { get; set; }
        public Nullable<System.DateTime> ReleaseAuditDate { get; set; }
        public string ReleaseByName { get; set; }

        public string OrgGroup { get; set; }
        public string OrgSubGroup { get; set; }

        public Nullable<bool> IsActive { get; set; }

    }

    public class Employeedetails
    {

        public string EmployeeName { get; set; }
        public string EmployeeCode { get; set; }
        public Nullable<int> Grade { get; set; }
        public string Designation { get; set; }
    }

}
