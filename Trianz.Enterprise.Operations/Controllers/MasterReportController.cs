using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
//using System.DirectoryServices;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Security;
using System.Configuration;
using Trianz.Enterprise.Operations.Models;

namespace Trianz.Enterprise.Operations.Controllers
{
    public class MasterReportController : Controller
    {
        TrianzOperationsEntities db = new TrianzOperationsEntities();
         // GET: MasterReport
        public ActionResult MasterReportView(string currentFilter, string ddlStatus, string SearchEmployee, int? page, string ddlSearch, string SearchField, string ddlSearchField)
        {
            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            {

                //List<Employee> emplist = new List<Employee>();
                //List<EmployeeResume> EmployeeResumes = new List<EmployeeResume>();
                //Employee empslist = new Employee();
                //EmployeeResume EmployeeResume = new EmployeeResume();


                //Below single statement is added by Sarath, for security reason to access RoleMaster page.
                TempData["IsRoleMasterPageAccess"] = null;

            List<string> statuslist = new List<string>() { "Billed", "Bench", "Business Operations", "BSA","Delivery", "Shadow" };
            List<string> searchlist = new List<string>() { "StartsWith", "Contains", "Equals", "EndsWith" };
            List<string> searchfieldlist = new List<string>() { "EmployeeId", "Name", "Practice", "Grade" };
            ViewData["SearchType"] = searchlist;
            ViewData["StatusType"] = statuslist;
            ViewData["SearchTypeField"] = searchfieldlist;
            if (SearchEmployee != null)
            {
                page = 1;
            }
            else
            {
                SearchEmployee = currentFilter;
            }
            if (SearchField != null)
            {
                page = 1;
            }
            else
            {
                SearchField = currentFilter;
            }
            
            ViewBag.CurrentFilter = SearchEmployee;
            ViewBag.CurrentFilter = SearchField;
            var emp =
                (from employee in db.Employees.DefaultIfEmpty()
                 // join Project Assignment
                 join projectassignment in db.ProjectAssignments
                    on employee.EmployeeId equals projectassignment.EmployeeId into tempProjectAssignments
                   from projectassignment in tempProjectAssignments.DefaultIfEmpty()
                 // join Project
                 join project in db.Projects
                    on projectassignment.ProjectCode equals project.ProjectCode into tempProjects
                    from project in tempProjects.DefaultIfEmpty()
                 // join EmployeeDoc
                 join employeeDoc in db.EmployeeDocs
                 on employee.EmployeeId equals employeeDoc.Employeeid into tempEmployeeDocs
                    from employeeDoc in tempEmployeeDocs.DefaultIfEmpty() 
                 //Conditions
                 where employee.IsActive == true && projectassignment.IsActive == true && project.IsActive == true 
                 select new
                  {
                      employee.EmployeeId,
                      employee.UserName,
                      employee.Email,
                      employee.Grade,
                      employee.LocationType,
                      employee.Location,
                      employee.ReservationStatus,
                      employee.AssignmentStatus,
                      employee.EmployeeType,
                      //projectassignment.BillingStatus,
                      employee.Designation,
                      employee.DateOfJoin, 
                      employee.Practice,
                      employee.PreviousExperience,
                      employee.MasterSkillSet,
                      employee.PrimarySkills,
                      //employee.ProsedStartDate,
                      //employee.ProsedEndDate,
                      employee.Utilization,
                      employee.ResourceStatus,
                      project.ProjectManager,
                      //e.ProjectManagerId,
                      employee.SupervisorId,
                      employee.IsServicingNoticePeriod,
                      employeeDoc.Empcv,
                      ProjectName = projectassignment.ProjectName,
                      projectassignment.ProjectCode,
                      projectassignment.StartDate,
                      projectassignment.EndDate,
                      ////EndDate = 
                      BillingType = project.BillingType,
                      ClientName = project.AccountName, 
                  }).ToList();

            #region masterreportsCodeLogic

            //(from e in db.Employees 
            // join projAssgn in db.ProjectAssignments on e.EmployeeId equals projAssgn.EmployeeId   
            // join eR in db.EmployeeDocs.DefaultIfEmpty() on e.EmployeeId equals eR.Employeeid
            // join proj in db.Projects on projAssgn.ProjectCode equals proj.ProjectCode
            // where projAssgn.IsActive == true
            //  //&& proj.IsActive == true /*&& projAssgn.ProjectName == proj.ProjectName*/
            // select new
            // {
            //     e.EmployeeId,
            //     e.UserName,
            //     e.Email,
            //     e.Grade,
            //     e.LocationType,
            //     e.Location,
            //     e.ReservationStatus,
            //     e.EmployeeType,
            //     e.Designation,
            //     e.DateOfJoin,

            //     e.Practice,
            //     e.PreviousExperience,
            //     e.MasterSkillSet,
            //     e.PrimarySkills,
            //     //e.ProsedStartDate,
            //     //e.ProsedEndDate,
            //     e.Utilization,
            //     e.ResourceStatus,
            //     proj.ProjectManager,
            //     //e.ProjectManagerId,
            //     e.SupervisorId,
            //     e.IsServicingNoticePeriod,
            //     eR.Empcv,
            //     ProjectName = projAssgn.ProjectName,
            //     projAssgn.ProjectCode,
            //     projAssgn.StartDate,
            //     projAssgn.EndDate,
            //     ////EndDate = 
            //      BillingType = proj.BillingType,
            //     ClientName = proj.AccountName,

            // }).ToList();
            // empList.Add(emp);
            //var emp = (from e in db.Employees select e).ToList();
            //ViewData["DS"] = emp;
            //foreach (var emps in emp)
            //{
            //    empslist.EmployeeId = emps.EmployeeId;
            //    //emplist.UserName = emps.FirstName + emps.LastName;
            //    empslist.Grade = emps.Grade;
            //    empslist.DateOfBirth = emps.DateOfBirth;
            //    empslist.LocationType = emps.LocationType;
            //    empslist.Location = emps.Location;
            //    //empslist.PrimarySkills = emps.PrimarySkills;
            //    //empslist.SecondarySkills = emps.SecondarySkills;
            //    empslist.ReservationStatus = emps.ReservationStatus;
            //    empslist.EmployeeType = emps.EmployeeType;
            //    empslist.Designation = emps.Designation;
            //    empslist.ProsedStartDate = emps.ProsedStartDate;
            //    empslist.ProsedEndDate = emps.ProsedEndDate;
            //    empslist.Project = emps.Project;
            //    empslist.Practice = emps.Practice;
            //    empslist.PreviousExperience = emps.PreviousExperience;
            //    empslist.ProjectManagerId = emps.ProjectManagerId;
            //    empslist.SupervisorId = emps.SupervisorId;
            //    empslist.SPOC = emps.SPOC;
            //    empslist.AssignmentStatus = emps.AssignmentStatus;
            //    emplist.Add(empslist);
            //}

            #endregion

            Session["Request"] = ddlStatus;
            Session["Search"] = SearchEmployee;
            ////return View();
            ////emp = emp.OrderBy(s => s.EmployeeId).ToList();
            if (!String.IsNullOrEmpty(SearchEmployee))
            {
                emp = emp.Where(s => s.EmployeeId.ToString().Contains(SearchEmployee)).ToList();
            }
            if (!String.IsNullOrEmpty(ddlStatus))
            {
                if (ddlStatus == "Bench")
                {
                    var x = "Free";
                    emp = emp.Where(s => s.ReservationStatus == x).ToList();
                }
                else if (ddlStatus == "Billed")
                {
                    emp = emp.Where(s => s.ReservationStatus == ddlStatus).ToList();
                }
                else if (ddlStatus == "Shadow")
                {
                    emp = emp.Where(s => s.ReservationStatus == ddlStatus).ToList();
                }
                else { }
            }

            if (!string.IsNullOrEmpty(ddlSearch) && !string.IsNullOrEmpty(ddlSearchField) && !string.IsNullOrEmpty(SearchField))
            {
                //////emp = SearchResult(ddlSearch, ddlSearchField, SearchField);
                switch (ddlSearch)
        {
                case "Contains":
                        if (ddlSearchField == "EmployeeId")
                    {
                            emp = emp.Where(s => s.EmployeeId.ToString().Contains(SearchField)).ToList();
                    }
                        else if (ddlSearchField == "Name")
                    {
                            emp = emp.Where(s => s.UserName.ToUpper().Contains(SearchField.ToUpper())).ToList();
                    }
                        else if (ddlSearchField == "Practice")
                    {
                            emp = emp.Where(s => s.Practice.ToUpper().Contains(SearchField.ToUpper())).ToList();
                    }
                        else if (ddlSearchField == "Grade")
                    {
                            emp = emp.Where(s => s.Grade.ToString().Contains(SearchField)).ToList();
                    }
                        //else if (DDLField == "PrimarySkills")
                        //{
                        //    skill = skill.Where(s => s.PrimarySkills.ToUpper().Contains(SearchText.ToUpper())).ToList();
                        //    result = skill;
                        //    break;
                        //}
                     
                        break;
                case "StartsWith":
                        if (ddlSearchField == "EmployeeId")
                    {
                            emp = emp.Where(s => s.EmployeeId.ToString().StartsWith(SearchField)).ToList();
                    }
                        else if (ddlSearchField == "Name")
                    {
                            emp = emp.Where(s => s.UserName.ToUpper().StartsWith(SearchField.ToUpper())).ToList();
                    }
                        else if (ddlSearchField == "Practice")
                    {
                            emp = emp.Where(s => s.Practice.ToUpper().StartsWith(SearchField.ToUpper())).ToList();
                    }
                        else if (ddlSearchField == "Grade")
                    {
                            emp = emp.Where(s => s.Grade.ToString().StartsWith(SearchField)).ToList();
                    }
                        //else if (DDLField == "PrimarySkills")
                        //{
                        //    skill = skill.Where(s => s.PrimarySkills.ToUpper().StartsWith(SearchText.ToUpper())).ToList();
                        //    result = skill;
                        //    break;
                        //}
                      
                        break;
                case "EndsWith":
                        if (ddlSearchField == "EmployeeId")
                    {
                            emp = emp.Where(s => s.EmployeeId.ToString().EndsWith(SearchField)).ToList();
                    }
                        else if (ddlSearchField == "Name")
                    {
                            emp = emp.Where(s => s.UserName.ToUpper().EndsWith(SearchField.ToUpper())).ToList();
                    }
                        else if (ddlSearchField == "Practice")
                    {
                            emp = emp.Where(s => s.Practice.ToUpper().EndsWith(SearchField.ToUpper())).ToList();
                    }
                        else if (ddlSearchField == "Grade")
                    {
                            emp = emp.Where(s => s.Grade.ToString().EndsWith(SearchField)).ToList();
                    }
                        //else if (DDLField == "PrimarySkills")
                        //{
                        //    skill = skill.Where(s => s.PrimarySkills.ToUpper().EndsWith(SearchText.ToUpper())).ToList();
                        //    result = skill;
                        //    break;
                        //}
                       
                        break;
                case "Equals":
                        if (ddlSearchField == "EmployeeId")
                    {
                            emp = emp.Where(s => s.EmployeeId.ToString().Equals(SearchField)).ToList();
                    }
                        else if (ddlSearchField == "Name")
                    {
                            emp = emp.Where(s => s.UserName.ToUpper().Equals(SearchField.ToUpper())).ToList();
                    }
                        else if (ddlSearchField == "Practice")
                    {
                            emp = emp.Where(s => s.Practice.ToUpper().Equals(SearchField.ToUpper())).ToList();
                    }
                        else if (ddlSearchField == "Grade")
                    {
                            emp = emp.Where(s => s.Grade.ToString().Equals(SearchField)).ToList();
                    }
                        break;
                    }
                emp = emp.OrderBy(s => s.EmployeeId).ToList();
                            
            }

            List<EmployeeMaster> empMaster = new List<EmployeeMaster>();
            ////empMaster = emp;

            empMaster = emp.AsEnumerable().Select(p => new EmployeeMaster
            {
                employee = new Employee
                {
                    Email = p.Email,
                    EmployeeId = p.EmployeeId,
                    UserName = p.UserName,
                    Grade = p.Grade,
                    LocationType = p.LocationType,
                    Location = p.Location,
                    ReservationStatus = p.ReservationStatus,
                    EmployeeType = p.EmployeeType,
                    Designation = p.Designation,
                    DateOfJoin = p.DateOfJoin,
                    AssignmentStatus = p.AssignmentStatus,
                    Practice = p.Practice,
                    PreviousExperience = p.PreviousExperience,
                    MasterSkillSet = p.MasterSkillSet,
                    PrimarySkills = p.PrimarySkills,
                    //ProsedStartDate = p.ProsedStartDate,
                    //ProsedEndDate = p.ProsedEndDate,
                    Utilization = p.Utilization,
                    ResourceStatus = p.ResourceStatus,
                    //BillingStatus = p.BillingStatus,
                    //ProjectManagerId = p.ProjectManagerId,
                    SupervisorId = p.SupervisorId,
                    IsServicingNoticePeriod = p.IsServicingNoticePeriod

                },

                projectAssignment = new ProjectAssignment
                {
                    ProjectName = p.ProjectName,
                    ProjectCode = p.ProjectCode,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate
                    //,BillingStatus = p.BillingStatus
                },

                project = new Project
                {
                    BillingType = p.BillingType,
                    AccountName = p.ClientName,
                    ProjectManager = p.ProjectManager
                },

                empdoc = new EmployeeDoc
                {
                    Empcv = p.Empcv 
                }

            }).ToList<EmployeeMaster>();

            //return View(emp);

            return View(empMaster);

        }


            ////public static ActionResult SearchResult(string SearchDDL, string DDLField, string SearchText)
            ////{
            ////    TrianzOperationsEntities db = new TrianzOperationsEntities();
            ////    //var emp = (from e in db.Employees select e).ToList();
            ////    var emp = (from e in db.Employees
            ////               join projAssgn in db.ProjectAssignments on e.EmployeeId equals projAssgn.EmployeeId
            ////               where projAssgn.IsActive == true
            ////               select new
            ////               {
            ////                   e.EmployeeId,
            //                   e.UserName,
            //                   e.Email,
            //                   e.Grade,
            //                   e.LocationType,
            //                   e.Location,
            //                   e.ReservationStatus,
            //                   e.EmployeeType,
            //                   e.Designation,
            //                   e.DateOfJoin,
            //                   e.Project,
            //                   e.Practice,
            //                   e.PreviousExperience,
            //                   e.ProsedStartDate,
            //                   e.ProsedEndDate,
            //                   e.Utilization,
            //                   e.ResourceStatus,
            //                   e.ProjectManagerId,
            //                   e.SupervisorId,
            //                   e.IsServicingNoticePeriod,
            //                   ProjectName = projAssgn.ProjectName
            //               }).ToList();
            //                var result = emp.OrderBy(s => s.EmployeeId).ToList();
            //    ////var skill = (from sk in db.Employees where sk.PrimarySkills != null select sk).ToList();
            //    switch (SearchDDL)
            //    {
            //        case "Contains":
            //            if (DDLField == "EmployeeId")
            //            {
            //                emp = emp.Where(s => s.EmployeeId.ToString().Contains(SearchText)).ToList();
            //            }
            //            else if (DDLField == "Name")
            //            {
            //                emp = emp.Where(s => s.UserName.ToUpper().Contains(SearchText.ToUpper())).ToList();
            //            }
            //            else if (DDLField == "Practice")
            //            {
            //                emp = emp.Where(s => s.Practice.ToUpper().Contains(SearchText.ToUpper())).ToList();
            //            }
            //            else if (DDLField == "Grade")
            //            {
            //                emp = emp.Where(s => s.Grade.ToString().Contains(SearchText)).ToList();
            //            }
            //            //else if (DDLField == "PrimarySkills")
            //            //{
            //            //    skill = skill.Where(s => s.PrimarySkills.ToUpper().Contains(SearchText.ToUpper())).ToList();
            //            //    result = skill;
            //            //    break;
            //            //}
            //            result = emp;
            //            break;
            //        case "StartsWith":
            //            if (DDLField == "EmployeeId")
            //            {
            //                emp = emp.Where(s => s.EmployeeId.ToString().StartsWith(SearchText)).ToList();
            //            }
            //            else if (DDLField == "Name")
            //            {
            //                emp = emp.Where(s => s.UserName.ToUpper().StartsWith(SearchText.ToUpper())).ToList();
            //            }
            //            else if (DDLField == "Practice")
            //            {
            //                emp = emp.Where(s => s.Practice.ToUpper().StartsWith(SearchText.ToUpper())).ToList();
            //            }
            //            else if (DDLField == "Grade")
            //            {
            //                emp = emp.Where(s => s.Grade.ToString().StartsWith(SearchText)).ToList();
            //            }
            //            //else if (DDLField == "PrimarySkills")
            //            //{
            //            //    skill = skill.Where(s => s.PrimarySkills.ToUpper().StartsWith(SearchText.ToUpper())).ToList();
            //            //    result = skill;
            //            //    break;
            //            //}
            //            result = emp;
            //            break;
            //        case "EndsWith":
            //            if (DDLField == "EmployeeId")
            //            {
            //                emp = emp.Where(s => s.EmployeeId.ToString().EndsWith(SearchText)).ToList();
            //            }
            //            else if (DDLField == "Name")
            //            {
            //                emp = emp.Where(s => s.UserName.ToUpper().EndsWith(SearchText.ToUpper())).ToList();
            //            }
            //            else if (DDLField == "Practice")
            //            {
            //                emp = emp.Where(s => s.Practice.ToUpper().EndsWith(SearchText.ToUpper())).ToList();
            //            }
            //            else if (DDLField == "Grade")
            //            {
            //                emp = emp.Where(s => s.Grade.ToString().EndsWith(SearchText)).ToList();
            //            }
            //            //else if (DDLField == "PrimarySkills")
            //            //{
            //            //    skill = skill.Where(s => s.PrimarySkills.ToUpper().EndsWith(SearchText.ToUpper())).ToList();
            //            //    result = skill;
            //            //    break;
            //            //}
            //            result = emp;
            //            break;
            //        case "Equals":
            //            if (DDLField == "EmployeeId")
            //            {
            //                emp = emp.Where(s => s.EmployeeId.ToString().Equals(SearchText)).ToList();
            //            }
            //            else if (DDLField == "Name")
            //            {
            //                emp = emp.Where(s => s.UserName.ToUpper().Equals(SearchText.ToUpper())).ToList();
            //            }
            //            else if (DDLField == "Practice")
            //            {
            //                emp = emp.Where(s => s.Practice.ToUpper().Equals(SearchText.ToUpper())).ToList();
            //            }
            //            else if (DDLField == "Grade")
            //            {
            //                emp = emp.Where(s => s.Grade.ToString().Equals(SearchText)).ToList();
            //            }
            //            //else if (DDLField == "PrimarySkills")
            //            //{
            //            //    skill = skill.Where(s => s.PrimarySkills.ToUpper().Equals(SearchText.ToUpper())).ToList();
            //            //    result = skill;
            //            //    break;
            //            //}
            //            result = emp;
            //            break;
            //    }
            //    return result.ToList();
            //}
            else
            {
                //ermsg = "Session expired";
                //return Json("Sessionexpired", JsonRequestBehavior.AllowGet);
                return RedirectToAction("SessionExpire", "Signout");
            }
        }

        public void GetExcel()
        {
            try
            {
                //  var HrrfDetailsExport = new List<HRRF>();
                var Employee = new List<Employee>();
                List<EmployeeMaster> empMaster = new List<EmployeeMaster>();
                var emp = (from employee in db.Employees.DefaultIfEmpty()
                             // join Project Assignment
                             join projectassignment in db.ProjectAssignments
                                on employee.EmployeeId equals projectassignment.EmployeeId into tempProjectAssignments
                             from projectassignment in tempProjectAssignments.DefaultIfEmpty()
                            // join Project
                             join project in db.Projects
                                on projectassignment.ProjectCode equals project.ProjectCode into tempProjects
                             from project in tempProjects.DefaultIfEmpty()
                            // join EmployeeDoc
                             join employeeDoc in db.EmployeeDocs
                                on employee.EmployeeId equals employeeDoc.Employeeid into tempEmployeeDocs
                             from employeeDoc in tempEmployeeDocs.DefaultIfEmpty()
                            //Conditions
                             where employee.IsActive == true && projectassignment.IsActive == true && project.IsActive == true
                             select new
                             {
                                 employee.EmployeeId,
                                 employee.UserName,
                                 employee.Email,
                                 employee.Grade,
                                 employee.LocationType,
                                 employee.Location,
                                 employee.ReservationStatus,
                                 employee.AssignmentStatus,
                                 //projectassignment.BillingStatus,
                                 employee.EmployeeType,
                                 employee.Designation,
                                 employee.DateOfJoin,
                                 employee.Practice,
                                 employee.PreviousExperience,
                                 employee.MasterSkillSet,
                                 employee.PrimarySkills,
                                 //employee.ProsedStartDate,
                                 //employee.ProsedEndDate,
                                 employee.Utilization,
                                 employee.ResourceStatus,
                                 project.ProjectManager,
                                 //e.ProjectManagerId,
                                 employee.SupervisorId,
                                 employee.IsServicingNoticePeriod,
                                 employeeDoc.Empcv,
                                 ProjectName = projectassignment.ProjectName,
                                 projectassignment.ProjectCode,
                                 projectassignment.StartDate,
                                 projectassignment.EndDate,
                                 ////EndDate = 
                                 BillingType = project.BillingType,
                                 ClientName = project.AccountName,
                             }).ToList();

                #region employeeMaster

                //(from e in db.Employees
                //           join eR in db.EmployeeDocs on e.EmployeeId equals eR.Employeeid
                //           join projAssgn in db.ProjectAssignments on e.EmployeeId equals projAssgn.EmployeeId
                //           join proj in db.Projects on projAssgn.ProjectCode equals proj.ProjectCode
                //           where projAssgn.IsActive == true && proj.IsActive == true /*&& projAssgn.ProjectName == proj.ProjectName*/
                //           select new
                //           {
                //               e.EmployeeId,
                //               e.UserName,
                //               e.Email,
                //               e.Grade,
                //               e.LocationType,
                //               e.Location,
                //               e.ReservationStatus,
                //               e.EmployeeType,
                //               e.Designation,
                //               e.DateOfJoin,

                //               e.Practice,
                //               e.PreviousExperience,
                //               e.MasterSkillSet,
                //               e.PrimarySkills,
                //               //e.ProsedStartDate,
                //               //e.ProsedEndDate,
                //               e.Utilization,
                //               e.ResourceStatus,
                //               proj.ProjectManager,
                //               //e.ProjectManagerId,
                //               e.SupervisorId,
                //               e.IsServicingNoticePeriod,
                //               eR.Empcv,
                //               ProjectName = projAssgn.ProjectName,
                //               projAssgn.ProjectCode,
                //               projAssgn.StartDate,
                //               projAssgn.EndDate,
                //               ////EndDate = 
                //               BillingType = proj.BillingType,
                //               ClientName = proj.AccountName,

                //           }).ToList();
                #endregion

                string ddlStatus = string.Empty;
                if (Session["Request"] != null)
                {
                    ddlStatus = Session["Request"].ToString();
                }
                string searchString = string.Empty;
                if (Session["Search"] != null)
                {
                    searchString = Session["Search"].ToString();
                }

                if (string.IsNullOrEmpty(searchString))
                {

                    if (ddlStatus == "Billed")
                    {
                        emp = (from Emp in emp
                                    where (Emp.ReservationStatus == ddlStatus)
                                    select Emp).OrderBy(a => a.EmployeeId).ToList();
                    }
                    else if (ddlStatus == "Bench")
                    {
                        emp = (from Emp in emp
                                    where (Emp.ReservationStatus == "Free")
                                    select Emp).OrderBy(a => a.EmployeeId).ToList();
                    }
                    else if (ddlStatus == "Shadow")
                    {
                        emp = (from Emp in emp
                                    where (Emp.ReservationStatus == ddlStatus)
                                    select Emp).OrderBy(a => a.EmployeeId).ToList();
                    }
                    else
                    {
                        emp = (from Emp in emp select Emp).ToList();
                    }
                    int employeeId = Convert.ToInt32(Session["EmployeeId"]);

                }

                else
                {
                    int x = Convert.ToInt32(searchString);
                    emp = (from Emp in emp
                                where (Emp.EmployeeId == x)
                                select Emp).OrderBy(a => a.EmployeeId).ToList();
                    //where Convert.ToBoolean((Emp.EmployeeId = Convert.ToInt32(searchString)))

                }
                // casting emp to EmployeeMaster object 
                //empMaster = emp.AsEnumerable().Select(p => new EmployeeMaster
                //{
                //    employee = new Employee
                //    {
                //        Email = p.Email,
                //        EmployeeId = p.EmployeeId,
                //        UserName = p.UserName,
                //        Grade = p.Grade,
                //        LocationType = p.LocationType,
                //        Location = p.Location,
                //        ReservationStatus = p.ReservationStatus,
                //        EmployeeType = p.EmployeeType,
                //        Designation = p.Designation,
                //        DateOfJoin = p.DateOfJoin,

                //        Practice = p.Practice,
                //        PreviousExperience = p.PreviousExperience,
                //        MasterSkillSet = p.MasterSkillSet,
                //        PrimarySkills = p.PrimarySkills,
                //        //ProsedStartDate = p.ProsedStartDate,
                //        //ProsedEndDate = p.ProsedEndDate,
                //        Utilization = p.Utilization,
                //        ResourceStatus = p.ResourceStatus,

                //        //ProjectManagerId = p.ProjectManagerId,
                //        SupervisorId = p.SupervisorId,
                //        IsServicingNoticePeriod = p.IsServicingNoticePeriod

                //    },

                //    projectAssignment = new ProjectAssignment
                //    {
                //        ProjectName = p.ProjectName,
                //        ProjectCode = p.ProjectCode,
                //        StartDate = p.StartDate,
                //        EndDate = p.EndDate
                //    },

                //    project = new Project
                //    {
                //        BillingType = p.BillingType,
                //        AccountName = p.ClientName,
                //        ProjectManager = p.ProjectManager
                //    },

                //    empdoc = new EmployeeDoc
                //    {
                //        Empcv = p.Empcv
                //    }

                //}).ToList<EmployeeMaster>();
                WebGrid grid = new WebGrid(source: emp, canPage: false, canSort: false);
                string gridData = grid.GetHtml(
                columns: grid.Columns(
                  grid.Column("EmployeeId", "EmployeeId"),
                  grid.Column("UserName", "UserName"),
                  grid.Column("Email", "Email"),
                  grid.Column("Grade", "Grade"),
                   grid.Column("LocationType", "Location Type"),
                  grid.Column("Location", "Location"),
                   grid.Column("MasterSkillSet", "MasterSkillSet"),
                    grid.Column("PrimarySkills", "Sub Skill 1"),
                   grid.Column("ReservationStatus", "Reservation Status"),
                   grid.Column("AssignmentStatus", "Assignment Status"),
                   //grid.Column("DateOfBirth", "DateOfBirth"),
                   grid.Column("EmployeeType", "EmployeeType"),
                   grid.Column("Designation", "Designation"),
                   grid.Column("DateOfJoin", "DateOfJoin", format: (item) => string.Format("{0:yyyy-MM-dd}", item.DateOfJoin)),
                     grid.Column("ProjectCode", "Project Code"),
                     grid.Column("ProjectName", "Project Name"),
                   grid.Column("Practice", "Practice"),
                   grid.Column("PreviousExperience", "PreviousExperience"),
                   grid.Column("StartDate", "Start Date", format: (item) => string.Format("{0:yyyy-MM-dd}", item.StartDate)),
                  grid.Column("EndDate", "End Date", format: (item) => string.Format("{0:yyyy-MM-dd}", item.EndDate)),
               grid.Column("Utilization", "Utilization %"),
               grid.Column("ResourceStatus", "Resource Status"),
                grid.Column("ProjectManager", "Project Manager"),
                grid.Column("SupervisorId", "Supervisor Id"),
                 //grid.Column("Empcv", "Empcv"),
                  grid.Column("IsServicingNoticePeriod", "ServicingNoticePeriod"),
                  grid.Column("BillingType", "BillingType"),
                //grid.Column("BillingStatus", "Billing Status"),
               grid.Column("ClientName", "Client Name")
               //grid.Column("SPOC", "SPOC"),
               //grid.Column("AssignmentStatus", "Assignment Status")
               )
                ).ToString();
                Response.ClearContent();
                Response.AddHeader("content-disposition", "attachment; filename=EmployeeInfo.xls");
                Response.ContentType = "application/excel";
                Response.Write(gridData);
                Response.End();
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

        public ActionResult FileDownload(int id)
        {
            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            {


                //declare byte array to get file content from database and string to store file name
                byte[] fileData;
            string fileName, x;
            var name = from d in db.Employees where d.EmployeeId == id select d.FirstName;

            //create object of LINQ to SQL class
            //using LINQ expression to get record from database for given id value

            var record = (from temp in db.EmployeeDocs where temp.Employeeid == id select temp.Empcv);
            //only one record will be returned from database as expression uses condtion on primary field
            //so get first record from returned values and retrive file content (binary) and filename
            
            fileData = (byte[])record.First().ToArray();
            fileName = name.First();
            x = fileName.ToString();
            return File(fileData, ".docx", x + "_Resume.docx");

        }
            else
            {
                //ermsg = "Session expired";
                //return Json("Sessionexpired", JsonRequestBehavior.AllowGet);
                return RedirectToAction("SessionExpire", "Signout");
            }
        }
    }
}
