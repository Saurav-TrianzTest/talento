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

namespace Trianz.Enterprise.Operations.Controllers
{
    public class BulkAssignmentController : Controller
    {
        TrianzOperationsEntities db = new TrianzOperationsEntities();

        public ActionResult Index()
        {
            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            {
                try
                {
                    //var Practices = (from bi in db.PracticeWiseBenchCodes
                    //                 join pro in db.Projects on bi.BenchCode equals pro.ProjectCode
                    //                 where bi.Practice != "SG&A"
                    //                 && pro.IsActive == true
                    //                 select new SelectListItem
                    //                 {
                    //                     Value = pro.ProjectCode,
                    //                     Text = pro.ProjectCode + "-" + pro.ProjectName

                    //                 }).Distinct().ToList();
                    var Practices = (
                                 from pro in db.Projects
                                 join pa in db.ProjectAssignments on pro.ProjectCode equals pa.ProjectCode
                                 where pro.IsActive == true
                              //   where pro.IsActive == true && pro.SOWEndDate >= DateTime.UtcNow
                                 select new SelectListItem
                                 {
                                     Value = pro.ProjectCode,
                                     Text = pro.ProjectCode + "-" + pro.ProjectName

                                 }).Distinct().ToList();

                    ViewBag.Practices = Practices;

                    var bench = new BenchAssignment();
                    bench.BenchEmployees = new List<EmployeeDetailsPractiseWise>();
                    //GetAllPractices();
                    GetProjects();


                    #region Bind Resource Type in dropdownlist

                    List<SelectListItem> lstResourceType = new List<SelectListItem>();
                    lstResourceType.Add(new SelectListItem { Value = "Billed", Text = "Billed" });
                    lstResourceType.Add(new SelectListItem { Value = "Internal", Text = "Internal" });
                    lstResourceType.Add(new SelectListItem { Value = "Shadow", Text = "Shadow" });
                    lstResourceType.Add(new SelectListItem { Value = "Support", Text = "Support" });
                    lstResourceType.Add(new SelectListItem { Value = "Investment", Text = "Investment" });
                    lstResourceType.Add(new SelectListItem { Value = "Delivery", Text = "Delivery" });
                    lstResourceType.Add(new SelectListItem { Value = "BSA", Text = "BSA" });
                    lstResourceType.Add(new SelectListItem { Value = "Business Operations", Text = "Business Operations" });
                    ///New status added as per BO requirment
                    lstResourceType.Add(new SelectListItem { Value = "ESS", Text = "ESS" });
                    lstResourceType.Add(new SelectListItem { Value = "Solution Development", Text = "Solution Development" });
                    lstResourceType.Add(new SelectListItem { Value = "Bench", Text = "Bench" });
                    lstResourceType.Add(new SelectListItem { Value = "Presales", Text = "Presales" });
                    lstResourceType.Add(new SelectListItem { Value = "Account Ops", Text = "Account Ops" });
                    lstResourceType.Add(new SelectListItem { Value = "Management", Text = "Management" });
                    lstResourceType.Add(new SelectListItem { Value = "Practice Delivery", Text = "Practice Delivery" });
                    lstResourceType.Add(new SelectListItem { Value = "Practice Support", Text = "Practice Support" });
                    lstResourceType.Add(new SelectListItem { Value = "Internal Application", Text = "Internal Application" });
                    lstResourceType.Add(new SelectListItem { Value = "Interns", Text = "Interns" });

                    ViewBag.ResourceType = lstResourceType;

                    #endregion

                    #region Bind Utilization in dropdownlist

                    //List<SelectListItem> lstUtilization = new List<SelectListItem>();
                    //lstUtilization.Add(new SelectListItem { Value = "25", Text = "25" });
                    //lstUtilization.Add(new SelectListItem { Value = "50", Text = "50" });
                    //lstUtilization.Add(new SelectListItem { Value = "75", Text = "75" });
                    //lstUtilization.Add(new SelectListItem { Value = "100", Text = "100" });

                    //ViewBag.Utilization = lstUtilization;

                    #endregion

                    return View(bench);
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
                // Json("Sessionexpired", JsonRequestBehavior.AllowGet);
                return RedirectToAction("SessionExpire", "Signout");
            }
        }
        public ActionResult GetEmployeeInfobyPractice(string ProjectCode)
        {
            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            {
                try
                {
                    var js = new BenchAssignment();
                    string code = string.Empty;
                    string practice = string.Empty;


                    if (ProjectCode != null && ProjectCode != string.Empty)
                    {
                        //if (ProjectCode == "TRZ071")
                        //{
                        //    practice = "AIM";
                        //}
                        //else if (ProjectCode == "TRZ074")
                        //{
                        //    practice = "EAS";
                        //}
                        //else if (ProjectCode == "TRZ-852" || ProjectCode == "TRZ952")
                        //{
                        //    practice = "IMS-ITSM";
                        //}
                        //else if (ProjectCode == "TRZ-854")
                        //{
                        //    practice = "IMS-MAINTENANCE";
                        //}
                        //else if (ProjectCode == "TRZ-853")
                        //{
                        //    practice = "IMS-SECURITY";
                        //}
                        //else if (ProjectCode == "TRZ953")
                        //{
                        //    practice = "IT SERVICES OTHERS";
                        //}
                        //else if (ProjectCode == "TRZ089")
                        //{
                        //    practice = "DIGITAL SOLUTIONS";
                        //}
                        //else if (ProjectCode == "TRZ111")
                        //{
                        //    practice = "CLOUD SOLUTIONS";
                        //}
                        //else if (ProjectCode == "TRZ073")
                        //{
                        //    practice = "IT SERVICES OTHERS";
                        //}
                        //else if (ProjectCode == "TRZ951")
                        //{
                        //    practice = "BUSINESS ENGAGEMENT";
                        //}


                        code = ProjectCode;

                    }

                    //js.BenchEmployees = (from bpr in db.PracticeWiseBenchCodes
                    //                     join pro in db.Projects on bpr.BenchCode equals pro.ProjectCode
                    //                     join pa in db.ProjectAssignments on pro.ProjectCode equals pa.ProjectCode
                    //                     join emp in db.Employees on pa.EmployeeId equals emp.EmployeeId
                    //                     where (pa.IsActive == true)
                    //                     && pro.ProjectCode == code && emp.IsActive == true
                    js.BenchEmployees = (
                                         from pro in db.Projects
                                         join pa in db.ProjectAssignments on pro.ProjectCode equals pa.ProjectCode
                                         join emp in db.Employees on pa.EmployeeId equals emp.EmployeeId
                                         where (pa.IsActive == true)
                                         && pro.ProjectCode == code && emp.IsActive == true
                                         select new EmployeeDetailsPractiseWise
                                         {
                                             EmployeeName = emp.FirstName + " " + emp.LastName,
                                             EmployeeCode = emp.EmployeeId.ToString(),
                                             Grade = emp.Grade,
                                             Manager = pro.ProjectManager,
                                             Utilization = pa.Utilization.ToString(),
                                             Assignment_Id = pa.Assignment_Id,
                                             Designation = emp.Designation,
                                         //Role = (from roles in db.DesignationMasters.Where(x => x.Practice.ToLower().Contains(bpr.Practice.Trim().ToLower()) && x.Grade == emp.Grade && x.CostCenter.Contains(emp.CostCenter))
                                         Role = (from roles in db.DesignationMasters.Where(x => x.Grade == emp.Grade && x.CostCenter.Contains(emp.CostCenter))
                                                     select new Role
                                                     {
                                                         DesignationName = roles.DesignationName,
                                                         DesignationCode = roles.DesignationCode,
                                                     }).OrderBy(p => p.DesignationName).Distinct().ToList()}).GroupBy(x => x.EmployeeCode).Select(x => x.FirstOrDefault()).ToList();
                    //GetAllPractices();

                    return PartialView("BulkAssignmentPartialView", js);

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

        public List<SelectListItem> GetProjects()
        {
            //if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            //{

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

        //    else
        //    {
        //        //ermsg = "Session expired";
        //        //return Json("Sessionexpired", JsonRequestBehavior.AllowGet);
        //        return RedirectToAction("SessionExpire", "Signout");
        //    }
        //}


        public ActionResult GetDesignations(string Practice, int Grade)

        {
            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            {

                try
                {
                    if (!string.IsNullOrEmpty(Practice))
                    {
                        var GetRoles = (from roles in db.DesignationMasters.Where(x => x.Practice.ToLower().Contains(Practice.ToLower()) && x.Grade == Grade)
                                        select new
                                        {
                                            LookupName = roles.DesignationName,
                                            LookupCode = roles.DesignationCode,
                                        }).OrderBy(p => p.LookupName).ToList();

                        return Json(GetRoles, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(string.Empty, JsonRequestBehavior.AllowGet);
                    }

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
               //  return Json("Sessionexpired", JsonRequestBehavior.AllowGet);
               return RedirectToAction("SessionExpire", "Signout");
            }
        }
        public ActionResult ProjectAutoAssignment(List<SelectedEmployees> SelectedEmployees, string Project, string StartDate,
            string EndDate, string ResourceType, string PrevProjectCode, string project1)
        {
                string name = String.Join(", ", SelectedEmployees.Select(a => a.EmployeeName));
                string ermsg = "";
                string GTRNumber = "";
                bool chkforpropsed = true;
                DateTime dtStartDate = Convert.ToDateTime(StartDate);
                DateTime dtEndDate = Convert.ToDateTime(EndDate);

            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            {
                try { 
                foreach (var eachEmployee in SelectedEmployees)
                {
                    Int32 EmpID = Convert.ToInt32(eachEmployee.EmployeeId);
                    Employee objEmp = db.Employees.Where(e => e.EmployeeId.Equals(EmpID)).FirstOrDefault();
                    DateTime ttt = objEmp.DateOfJoin.Date;
                    // var lstHRRF = db.Database.SqlQuery<IObjectContextAdapter>("exec PrcAssignDefaultBenchUsingGrade @EmployeeId", new System.Data.SqlClient.SqlParameter("employeeId", EmpID)).ToList<IObjectContextAdapter>();
                    if (objEmp != null)
                    {
                        chkforpropsed = objEmp.DateOfJoin.Date <= dtStartDate.Date ? true : false;
                        if (chkforpropsed)
                        {
                            var objpras1 = db.ProjectAssignments.Where(b => b.EmployeeId == objEmp.EmployeeId && !(b.BillingStatus.Contains("Bench"))
                            &&
                                ((b.StartDate >= dtStartDate && b.StartDate <= dtEndDate) || (b.EndDate >= dtStartDate
                                && b.EndDate <= dtEndDate)
                                || (b.StartDate <= dtStartDate && b.EndDate >= dtEndDate))).ToList();

                            if (objpras1.Count() > 0)
                            {
                                var sumUtilization = objpras1.Sum(p => p.Utilization);
                                if ((sumUtilization + Convert.ToInt32(eachEmployee.util)) >= 100)
                                {
                                    chkforpropsed = false;
                                    ermsg = objEmp.EmployeeId + " already assigned to some other project with selected dates";
                                    break;
                                }
                            }
                            //else
                            //{
                            //    var objpras = db.ProjectAssignments.Where(b => b.EmployeeId == EmpID && b.IsActive == true).ToList();
                            //    if (objpras != null)
                            //    {

                            //        int utl = 0;
                            //        int utli = Convert.ToInt32(eachEmployee.util);

                            //        foreach (var lt in objpras)
                            //        {
                            //            if (!lt.BillingStatus.ToLower().Contains("bench"))
                            //            {
                            //                if (lt.EndDate >= DateTime.UtcNow)
                            //                {
                            //                    utl = utl + Convert.ToInt32(lt.Utilization);
                            //                    int mnu = 100 - utl;
                            //                    int chkutl = utl + utli;
                            //                    if (chkutl > 100)
                            //                    {
                            //                        chkforpropsed = false;
                            //                        ermsg = objEmp.EmployeeId + " Working on another projects with " + utl + "% utilization. so we can utilize " + mnu + "% only";
                            //                        break;
                            //                    }
                            //                }



                            //            }
                            //        }
                            //    }
                            //}
                        }
                        else
                        {
                            chkforpropsed = false;
                            ermsg = objEmp.EmployeeId + " Employee date of joining should be greater than assignment Start date .";
                            break;
                        }

                    }
                    else
                    {
                        chkforpropsed = false;
                        ermsg = objEmp.EmployeeId + " not exists in the employee list";
                        break;
                    }

                }
                if (chkforpropsed)
                {
                    foreach (var eachEmployee in SelectedEmployees)
                    {
                        int? Utilzation = null;
                        string HRRFNumber = "";
                        Int32 EmpID = Convert.ToInt32(eachEmployee.EmployeeId);
                        Employee objEmp = db.Employees.Where(e => e.EmployeeId.Equals(EmpID)).FirstOrDefault();
                        int utilza = Convert.ToInt32(eachEmployee.util);

                        if (objEmp != null)
                        {
                            #region HRRF Creation

                            string TR_CompanyCode = System.Configuration.ConfigurationManager.AppSettings["TR_CompanyCode"].ToString();

                            ServiceAgent AgentService = new ServiceAgent();
                            string NumberingSystem = AgentService.NumberingSystem(TR_CompanyCode, "", "");

                            if (!string.IsNullOrEmpty(NumberingSystem))
                            {
                                string[] SimplyNumber = NumberingSystem.Split(new[] { ":" }, StringSplitOptions.None);
                                string output = SimplyNumber[1].Replace('{', '/').Replace('}', ' ').Replace('"', ' ');
                                HRRFNumber = output.Trim();
                            }

                            HRRF objHRRF = new HRRF();
                            objHRRF.HRRFNumber = HRRFNumber;
                            objHRRF.Purpose = "Project";
                            objHRRF.ProjectName = db.Projects.Where(p => p.ProjectCode == Project).FirstOrDefault().ProjectName;
                            objHRRF.ProjectCode = Project;
                            objHRRF.DemandType = "Order Confirmed-SOW Not Available";
                            objHRRF.Grade = objEmp.Grade;
                            objHRRF.RoleRequired = eachEmployee.DesignationCode;
                            objHRRF.ExpFrom = db.MasterLookUps.Where(x => x.LookupType == "ExpFrom" && x.ParentCode.Equals(objEmp.Grade.ToString())).FirstOrDefault().LookupName;
                            objHRRF.EnagagementType = "Managed by Trianz";
                            objHRRF.JobDescription = db.DesignationMasters.Where(d => d.DesignationCode.Equals(eachEmployee.DesignationCode)).FirstOrDefault().JobDescription;
                            objHRRF.AssignmentStartDate = Convert.ToDateTime(StartDate);
                            objHRRF.AssignmentEndDate = Convert.ToDateTime(EndDate);
                            objHRRF.LocationType = (objEmp.LocationType.ToUpper() == "ONSITE") ? "1" : "2";
                            objHRRF.LocationName = objEmp.Location;
                            objHRRF.RequestReason = "New addition";
                            objHRRF.RequestType = "Internal";
                            objHRRF.RequestStatus = "Fulfilled";
                            objHRRF.ClientInterview = "No";
                            objHRRF.IsActive = true;
                            objHRRF.HRRFCreatedBy = Convert.ToInt32(Session["EmployeeId"]);
                            objHRRF.HRRFCreatedDate = DateTime.UtcNow;
                            objHRRF.HRRFSubmitedDate = DateTime.UtcNow;
                            objHRRF.Positions = 1;
                            objHRRF.Practice = objEmp.Practice;
                            objHRRF.TRParent = HRRFNumber;
                            objHRRF.Isparent = true;
                            objHRRF.AccountName = db.Projects.Where(p => p.ProjectCode == Project).FirstOrDefault().AccountName;
                            objHRRF.CostCenter = objEmp.CostCenter;
                            objHRRF.ResourceType = ResourceType;
                            objHRRF.IsContracting = false;
                            objHRRF.Impact = "Medium";
                            objHRRF.Criticality = "Medium";
                            objHRRF.Utilization = utilza.ToString();
                            objHRRF.BillingDate = Convert.ToDateTime(StartDate);
                            objHRRF.Remarks = "This is an auto generated TR during bulk assignment";
                            objHRRF.ResourceName = objEmp.EmployeeId + "(" + objEmp.FirstName + "" + objEmp.MiddleName + "" + objEmp.LastName + ")";
                            db.HRRFs.Add(objHRRF);

                            #endregion

                            #region HRRFHistory Creation

                            HRRFHistory objHRRFHistory = new HRRFHistory();
                            objHRRFHistory.HRRFNumber = HRRFNumber;
                            objHRRFHistory.HRRFID = 0;
                            objHRRFHistory.HistoryDescription = objEmp.FirstName + " " + objEmp.LastName + " - has been Fulfilled for - " + HRRFNumber + " - Auto";
                            objHRRFHistory.ModifiedBy = Convert.ToInt32(Session["EmployeeId"]);
                            objHRRFHistory.ModifiedDate = DateTime.UtcNow;
                            objHRRFHistory.Remarks = "This is an auto generated TR during bulk assignment";

                            db.HRRFHistories.Add(objHRRFHistory);

                            #endregion

                            #region Update in ProjectAssignment 
                            int assignmenid = Convert.ToInt32(eachEmployee.AssignmentID);

                            var objpras1 = db.ProjectAssignments.Where(b => b.EmployeeId == eachEmployee.EmployeeId
                              && !(b.BillingStatus.Contains("Bench")) && ((b.StartDate >= dtStartDate
                              && b.StartDate <= dtEndDate) || (b.EndDate >= dtStartDate && b.EndDate <= dtEndDate) ||
                              (b.StartDate <= dtStartDate && b.EndDate >= dtEndDate))).ToList();
                            if (objpras1.Count() > 0)
                            {
                                var sumUtilization = objpras1.Sum(p => p.Utilization);
                                Utilzation = 100 - sumUtilization;
                            }

                            List<ProjectAssignment> objPrjAssgnt = db.ProjectAssignments.Where(pa => pa.ProjectCode.Equals(PrevProjectCode)
                            && pa.EmployeeId == EmpID && pa.Assignment_Id == assignmenid).ToList();

                            var dtProjectAssignSatrtAsEndDate = db.ProjectAssignments.Where(pa => pa.EmployeeId == eachEmployee.EmployeeId
                                              && DateTime.UtcNow <= pa.StartDate).OrderBy(pa => pa.StartDate).Select(pa => pa.StartDate).FirstOrDefault();

                            var dtProjectAssignEndDate = db.ProjectAssignments.Where(pa => pa.EmployeeId == eachEmployee.EmployeeId
                            && pa.IsActive == true && pa.BillingStatus != "Bench"
                                   && DateTime.UtcNow <= pa.EndDate).OrderBy(pa => pa.EndDate).Select(pa => pa.EndDate).FirstOrDefault();

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
                                pash1.StartDateOld = proAssgnt.StartDate;
                                pash1.IsActiveOld = proAssgnt.IsActive;
                                pash1.UtilizationOld = proAssgnt.Utilization;
                                pash1.modifiedBy = EmpID;
                                pash1.ModifiedDate = DateTime.UtcNow;

                                #region Exisitng Bench Record Update based on selected Dates
                                // Selecting top 1 Start Date order by desc to Set Bench Enda date
                                int? ul = null;
                                if (DateTime.UtcNow.Date >= (Convert.ToDateTime(objHRRF.BillingDate).Date))
                                {
                                    if (Utilzation == null)
                                    {
                                        ul = 100 - Convert.ToInt32(objHRRF.Utilization);
                                    }
                                    else
                                    {
                                        ul = Utilzation - Convert.ToInt32(objHRRF.Utilization);
                                    }
                                }
                                else
                                    ul = proAssgnt.Utilization;

                                if (dtProjectAssignSatrtAsEndDate != null
                                    && Convert.ToDateTime(dtProjectAssignSatrtAsEndDate).Date <= Convert.ToDateTime(objHRRF.BillingDate).Date)
                                {
                                    proAssgnt.EndDate = Convert.ToDateTime(dtProjectAssignSatrtAsEndDate);
                                }
                                else if (dtProjectAssignEndDate != null && dtProjectAssignEndDate <= Convert.ToDateTime(objHRRF.AssignmentEndDate)
                                    && DateTime.UtcNow.Date.Equals(Convert.ToDateTime(objHRRF.BillingDate).Date))
                                {
                                    if (Convert.ToDateTime(objHRRF.BillingDate) < dtProjectAssignEndDate)
                                        proAssgnt.EndDate = Convert.ToDateTime(objHRRF.BillingDate);
                                    else
                                        proAssgnt.EndDate = Convert.ToDateTime(dtProjectAssignEndDate);
                                }
                                else if (DateTime.UtcNow.Date >= (Convert.ToDateTime(objHRRF.BillingDate).Date))
                                {
                                    if (ul > 0)
                                        proAssgnt.EndDate = Convert.ToDateTime(objHRRF.AssignmentEndDate);
                                    else
                                    {
                                        if (Convert.ToDateTime(objHRRF.BillingDate) >= Convert.ToDateTime(proAssgnt.StartDate))
                                            proAssgnt.EndDate = Convert.ToDateTime(objHRRF.BillingDate);
                                        else
                                            proAssgnt.EndDate = proAssgnt.StartDate;

                                    }
                                }
                                else
                                {
                                    proAssgnt.EndDate = Convert.ToDateTime(objHRRF.BillingDate);
                                }
                                // billdatge == getdate

                                //else if (DateTime.UtcNow.Date < Convert.ToDateTime(objHRRF.BillingDate).Date)
                                //{
                                //    proAssgnt.StartDate = DateTime.UtcNow;
                                //    proAssgnt.Utilization = Utilzation ?? 100;
                                //}
                                if (ul > 0)
                                {
                                    proAssgnt.Utilization = ul;

                                }
                                else
                                {
                                    proAssgnt.IsActive = false;
                                }
                                #endregion
                                pash1.UtilizationNew = proAssgnt.Utilization;
                                pash1.StartDateNew = proAssgnt.StartDate;
                                pash1.EndDateNew = proAssgnt.EndDate;
                                pash1.IsActiveNew = true;
                                pash1.BillingStatusNew = proAssgnt.BillingStatus;
                                pash1.Assigned_byNew = proAssgnt.Assigned_By;
                                db.ProjectAssignmenthistories.Add(pash1);

                                db.Entry(proAssgnt).State = System.Data.Entity.EntityState.Modified;

                            }

                            #endregion

                            #region Project Assignment Creation

                            ProjectAssignment objProjectAssignment = new ProjectAssignment();
                            objProjectAssignment.ProjectName = objHRRF.ProjectName;
                            objProjectAssignment.ProjectID = db.Projects.Where(p => p.ProjectCode.Equals(Project)).FirstOrDefault().ProjectId;
                            objProjectAssignment.EmployeeId = objEmp.EmployeeId;
                            objProjectAssignment.StartDate = Convert.ToDateTime(StartDate);
                            objProjectAssignment.EndDate = Convert.ToDateTime(EndDate);
                            objProjectAssignment.Utilization = Convert.ToInt32(eachEmployee.util);
                            if (DateTime.UtcNow.Date >= Convert.ToDateTime(objHRRF.BillingDate).Date)
                            {
                                objProjectAssignment.IsActive = true;
                            }
                            else
                            {
                                objProjectAssignment.IsActive = false;
                            }
                            objProjectAssignment.Assigned_By = db.Projects.Where(p => p.ProjectCode.Equals(Project)).FirstOrDefault().ProjectManager;
                            objProjectAssignment.Assigned_Date = DateTime.UtcNow;
                            objProjectAssignment.ProjectCode = Project;
                            objProjectAssignment.BillingStatus = ResourceType;

                            db.ProjectAssignments.Add(objProjectAssignment);

                            #region assignmenthistroy
                            ProjectAssignmenthistory pash = new ProjectAssignmenthistory();
                            pash.AssignmentId = 0;
                            pash.ProjectCode = Project;
                            pash.ProjectName = objHRRF.ProjectName;
                            pash.ProjectID = db.Projects.Where(p => p.ProjectCode.Equals(Project)).FirstOrDefault().ProjectId;
                            pash.EmployeeId = objEmp.EmployeeId;
                            pash.Assigned_ByOld = null;
                            pash.BillingStatusOld = null;
                            pash.EnddateOld = null;
                            pash.IsActiveOld = null;
                            pash.StartDateOld = null;
                            pash.UtilizationOld = null;
                            pash.modifiedBy = EmpID;
                            pash.ModifiedDate = DateTime.UtcNow;
                            pash.UtilizationNew = Convert.ToInt32(eachEmployee.util);
                            pash.StartDateNew = Convert.ToDateTime(StartDate);
                            if (DateTime.UtcNow.Date >= Convert.ToDateTime(objHRRF.BillingDate).Date)
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

                            #endregion

                            db.SaveChanges();

                            #region Below Code to assign Default bench codes to associate based on practice and 
                            DataSet retVal = new DataSet();
                            string ConnectionString = ConfigurationManager.ConnectionStrings["ADOConnectionString"].ConnectionString;
                            // RDS Proxy managed connection via EF6 database context
                            SqlConnection sqlConn = new SqlConnection(ConnectionString); // RDS Proxy managed connection

                            try
                            {

                                SqlCommand cmdReport = new SqlCommand("PrcAssignDefaultBenchUsingGrade", sqlConn);
                                SqlDataAdapter daReport = new SqlDataAdapter(cmdReport);
                                using (cmdReport)
                                {
                                    SqlParameter empIdPram = new SqlParameter("@EmployeeId", EmpID);
                                    cmdReport.CommandType = CommandType.StoredProcedure;
                                    cmdReport.Parameters.Add(empIdPram);
                                    daReport.Fill(retVal);
                                }

                            }
                            catch (Exception ex)
                            {
                                ErrorHandling expcls = new ErrorHandling();
                                expcls.Error(ex);
                                return null;
                            }
                            finally
                            {
                                sqlConn.Close();
                            }
                            #endregion
                            // db.Database.SqlQuery(sp_GetHRRFListByEmpIdPractice_Result, "exec PrcAssignDefaultBench @EmployeeId", EmpID);



                            if (HRRFNumber != null && HRRFNumber != string.Empty)
                            {
                                if (GTRNumber == string.Empty)
                                    GTRNumber = HRRFNumber;
                                else
                                    GTRNumber = GTRNumber + "," + HRRFNumber;
                            }




                        }
                    }
                        //string name = "";
                        //foreach (var eachEmployee in SelectedEmployees)
                        //{
                        //    name = db.Employees.Where(a => a.EmployeeId == eachEmployee.EmployeeId).Select(a => a.Empl)
                        //    name += name + ",";
                        //}
                        ermsg = "Selected Resources are assigned to the project  " + project1 + "."+
                       "<br>" + "TRs have been created and fulfilled by the System." + "</br>"
                        + GTRNumber;           /* observation*/

                }
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
                //ermsg = "Session expired";
               // return Json("Sessionexpired", JsonRequestBehavior.AllowGet);
                return RedirectToAction("SessionExpire", "Signout");
            }



            return Json(ermsg, JsonRequestBehavior.AllowGet);
            }
        }

    
        public class EmployeeDetailsPractiseWise
        {
            public string EmployeeName { get; set; }
            public string EmployeeCode { get; set; }
            public Nullable<int> Grade { get; set; }
            public string Manager { get; set; }
            public int Assignment_Id { get; set; }
            public string Utilization { get; set; }
            public string Designation { get; set; }
            public List<Role> Role { get; set; }
        }

        public class Role
        {
            public string DesignationCode { get; set; }
            public string DesignationName { get; set; }
        }
        public class SelectedEmployees
        {

            public int? EmployeeId { get; set; }
            public string EmployeeName { get; set; }                //observation
            public string DesignationCode { get; set; }
            public string util { get; set; }
            public string AssignmentID { get; set; }

        }
    }
