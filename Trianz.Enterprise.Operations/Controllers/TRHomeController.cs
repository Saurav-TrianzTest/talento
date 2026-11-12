using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Trianz.Enterprise.Operations.Filters;
using Trianz.Enterprise.Operations.Models;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Trianz.Enterprise.Operations.General;
using System.Data.SqlClient;						

namespace Trianz.Enterprise.Operations.Controllers
{
    [Authorize]
    public class TRHomeController : Controller
    {
        TrianzOperationsEntities db = new TrianzOperationsEntities();
        string clusterlead = System.Configuration.ConfigurationManager.AppSettings["ClusterLeadview"].ToString();
        string strclustermgr = System.Configuration.ConfigurationManager.AppSettings["clustermanager"].ToString();
        // [SkipAuthorizationFilter]
       
        public ActionResult Index()
        {
            //Below single statement is added by Sarath, for security reason to access RoleMaster page.
            TempData["IsRoleMasterPageAccess"] = null;

            //Identifying prev page
            TempData["PrevPage"] = Request.Url.Segments[Request.Url.Segments.Count() - 1];

            //var ServiceLine = (from serviceLine in db.MasterLookUps.Where(x => x.LookupType == "ServiceLine")
            //                   select new
            //                   {
            //                       LookupName = serviceLine.LookupName,
            //                       Description = serviceLine.Description,
            //                       LookupCode = serviceLine.LookupCode,
            //                       SeqNumber = serviceLine.SeqNumber
            //                   }).OrderBy(p => p.LookupName).ToList();

            var ServiceLine = (from data in db.PracticeWiseBenchCodes
                               select new
                               {
                                   LookupName = data.Practice,
                               }).Distinct().ToList().OrderBy(p => p.LookupName);
            //var ServiceLine = db.PracticeWiseBenchCodes.Select(x => x.Practice).Distinct().ToList();

            ViewData["ServiceLine"] = ServiceLine;
            Common.WriteErrorLog("ServiceLine" + ViewData["ServiceLine"]);
            Common.WriteErrorLog("ServiceLine" + ViewData["ServiceLine"] + "Line No: 47");
            Common.WriteErrorLog("Before GetAzureLoggedInUserID - Line No: 56");
            string usermail = Common.GetAzureLoggedInUserID();
            Common.WriteErrorLog(usermail + "Line No: 58");

            var empLoggedin = (from emp in db.Employees where emp.Email == usermail select emp).FirstOrDefault();
            if (empLoggedin != null)
            {
                string UserName = empLoggedin.UserName;
                //if ((UserName != "Sathyapriya.S" ) && (UserName != "Mani.Simha")  && (UserName != "Anasuya.Rangaiah") && (UserName != "Muralidhar.Gumnur")) // who ever needs talento access
                //{
                //    return RedirectToAction("Unavailable", "Home");
                //}

                if (UserName != null)
                {

                    int EmployeeId = db.Employees.Where(e => e.UserName.Equals(UserName) && e.IsActive == true).Select(e => e.EmployeeId).FirstOrDefault();

                    if (EmployeeId > 0)
                    {
                        var isSupervisor = (from p in db.Employees
                                            join v in db.Employees on p.EmployeeId equals v.SupervisorId
                                            where p.EmployeeId == EmployeeId && v.IsActive == true
                                            select p.Designation).FirstOrDefault();
                        //db.Employees.Where(p => p.SupervisorId == EmployeeId).Count();

                        List<string> ClusterMaster = System.Configuration.ConfigurationManager.AppSettings["ClusterMaster"].Split(',').ToList();
                        bool IsUserExist = ClusterMaster.Contains(EmployeeId.ToString());
                        ViewBag.ClusterMaster = IsUserExist;
                        if (isSupervisor != null)
                        {
                            Session["Supervisor"] = "Supervisor";
                        }
                        if (EmployeeId > 0)
                        {
                            string RoleValues = ConfigurationManager.AppSettings["RoleManagement"];
                            List<string> lstRoles = new List<string>();
                            lstRoles = RoleValues.Split(',').ToList();
                            Employee objEmployee = db.Employees.Where(e => e.UserName.Equals(UserName) && e.IsActive == true).FirstOrDefault();
                            RoleMaster objRoleMaster = db.RoleMasters.Where(r => r.EmployeeId == objEmployee.EmployeeId && r.ApplicationCode == "TALENTREQ").FirstOrDefault();
                            if (objRoleMaster != null)
                            {
                                Session["Role"] = objRoleMaster.Role;
                                Session["Grade"] = objEmployee.Grade;
                                Session["EmployeeId"] = objEmployee.EmployeeId;
                                Session["Practice"] = objEmployee.Practice;
                            }
                            else
                            {
                                Session["Role"] = "PM"; //DefaultRole;
                                Session["Grade"] = objEmployee.Grade;
                                Session["EmployeeId"] = objEmployee.EmployeeId;
                                Session["Practice"] = objEmployee.Practice;
                            }
                            if ((Session["Role"].ToString() == "OM" || Session["Role"].ToString() == "PM" || Session["Role"].ToString() == "DH" || Session["Role"].ToString() == "SM" || Session["Role"].ToString() == "RL" || Session["Role"].ToString() == "LDAdmin" || Session["Role"].ToString() == "CompetencyAdmin" || Session["Role"].ToString() == "Finance"))
                            {
                                string trVisibleValues = ConfigurationManager.AppSettings["OnlyTRVisibleEmployee"];
                                List<string> lstTRVisibleEmps = new List<string>();
                                lstTRVisibleEmps = trVisibleValues.Split(',').ToList();
                                if (Session["Role"].ToString() == "OM" || Convert.ToInt32(Session["Grade"]) >= 4 || Session["Role"].ToString() == "RL" || Session["Role"].ToString() == "DH")
                                {
                                    List<SelectListItem> lstTRRequests = new List<SelectListItem>();
                                    lstTRRequests.Add(new SelectListItem { Text = "My Request", Value = "MyRequest", Selected = true });
                                    lstTRRequests.Add(new SelectListItem { Text = "ALL TRs", Value = "ALL" });
                                    lstTRRequests.Add(new SelectListItem { Text = "Open TRs", Value = "Open" });
                                    lstTRRequests.Add(new SelectListItem { Text = "Fulfilled TRs", Value = "Fulfilled" });
                                    lstTRRequests.Add(new SelectListItem { Text = "Cancelled TRs", Value = "Cancelled" });
                                    ViewData["_TRRequests"] = lstTRRequests;
                                }
                                // else if(EmployeeId.ToString() == ConfigurationManager.AppSettings["OnlyTRVisibleEmployee"].ToString())
                                else if (lstTRVisibleEmps.Contains(EmployeeId.ToString()))
                                {
                                    List<SelectListItem> lstTRRequests = new List<SelectListItem>();
                                    lstTRRequests.Add(new SelectListItem { Text = "My Request", Value = "MyRequest", Selected = true });
                                    lstTRRequests.Add(new SelectListItem { Text = "ALL TRs", Value = "ALL" });
                                    ViewData["_TRRequests"] = lstTRRequests;
                                }
                                else if (Convert.ToInt32(Session["Grade"]) < 4)
                                {
                                    List<SelectListItem> lstTRRequests = new List<SelectListItem>();
                                    lstTRRequests.Add(new SelectListItem { Text = "My Request", Value = "MyRequest", Selected = true });
                                    ViewData["_TRRequests"] = lstTRRequests;
                                }
                            }
                            int empID = (int)Session["EmployeeId"];
                            #region ForPM
                            if (Session["Role"].ToString() == "PM")
                            {
                                if (Session["EmployeeId"] != null)
                                {
                                    var _Accounts = (from projects in db.Projects.
                                                      Where(p => p.ProjectManagerId == empID && p.IsActive == true)
                                                         // Where(p => p.ProjectManagerId == empID && p.IsActive == true && p.SOWEndDate >= DateTime.UtcNow)
                                                     select new
                                                     {
                                                         Accountname = projects.AccountName

                                                     }).Distinct().OrderBy(p => p.Accountname).ToList();
                                    if (ViewData["_Accounts"] == null)
                                    {
                                        ViewData["_Accounts"] = _Accounts;
                                    }
                                }
                            }
                            #endregion

                            #region ForOM
                            else if (Session["Role"].ToString() == "OM")
                            {
                                var _Accounts = (from projects in db.Projects.
                                                        Where(p => p.IsActive == true)
                                                     //Where(p => p.IsActive == true && p.SOWEndDate >= DateTime.UtcNow)
                                                 select new
                                                 {
                                                     Accountname = projects.AccountName

                                                 }).Distinct().OrderBy(p => p.Accountname).ToList();
                                if (ViewData["_Accounts"] == null)
                                {
                                    ViewData["_Accounts"] = _Accounts;
                                }
                            }
                            #endregion

                            #region ForDH
                            else if (Session["Role"].ToString() == "DH")
                            {
                                var _Accounts = (from projects in db.Projects.
                                                        Where(p => p.DELIVERY_MANAGER_ID == empID && p.IsActive == true)
                                                 select new
                                                 {
                                                     Accountname = projects.AccountName

                                                 }).Distinct().OrderBy(p => p.Accountname).ToList();
                                if (ViewData["_Accounts"] == null)
                                {
                                    ViewData["_Accounts"] = _Accounts;
                                }
                            }
                            #endregion
                            //List<HRRF> lstHRRF = db.HRRFs.Where(h => h.HRRFCreatedBy == objEmployee.EmployeeId).OrderByDescending(o => o.HRRFCreatedDate).ToList();
                            //return View(lstHRRF);
                            bool isDM = false;
                            int? DeliveryManagerId = null;
                            DeliveryManagerId = (from P in db.Projects
                                                 where P.DELIVERY_MANAGER_ID == empID && P.IsActive == true
                                                 select P.DELIVERY_MANAGER_ID).FirstOrDefault();
                            isDM = DeliveryManagerId != null ? true : false;
                            ViewBag.IsDM = isDM;
                            // PracticeClusterLead pcl = new PracticeClusterLead();
                            var clusterLeadId = (from data in db.PracticeClusterLeads.Where(x => x.LeadID == EmployeeId) select data).FirstOrDefault();
                            // Session["ClusterLeadId"] = clusterLeadId.LeadID;             
                            if (clusterLeadId != null)
                            {
                                Session["ClusterLeadId"] = clusterlead.ToString();
                                Session["LeadId"] = clusterLeadId.LeadID;
                            }
                            else
                            {
                                Session["ClusterLeadId"] = "0";
                                //Session["LeadId"] = clusterlead;
                            }
                            List<string> ClusterManager = System.Configuration.ConfigurationManager.AppSettings["clustermanager"].Split(',').ToList();
                            bool Ismanagerexist = ClusterManager.Contains(EmployeeId.ToString());
                            if (Ismanagerexist)
                            {
                                Session["ClusterManager"] = "1";
                            }
                            else
                            {
                                Session["ClusterManager"] = "2";
                            }
                            var InternalGraph = db.Database.SqlQuery<GraphStatus>("exec [GetSummary] @RequestType", new SqlParameter("RequestType", "Internal")).ToList<GraphStatus>();
                            var ExternalGraph = db.Database.SqlQuery<GraphStatus>("exec [GetSummary] @RequestType", new SqlParameter("RequestType", "External")).ToList<GraphStatus>();
                            ViewBag.InternalGraph = InternalGraph;
                            ViewBag.ExternalGraph = ExternalGraph;
                            return View();
                        }
                    }
                    return RedirectToAction("Unavailable", "TRHome");
                }
                return RedirectToAction("Unavailable", "TRHome");
            }
            return RedirectToAction("SignOut", "Signout");
        }
        public ActionResult GetHRRFsByFilters(string strPractice, string strRequest, string strAccount, string list = null, string status= null, string RequestType=null)
        {
            try
            {
                ViewBag.ActiveTR = strRequest;
                int? EmpId = Convert.ToInt32(Session["EmployeeId"]);
                if (status == "")
                {
                    status = null;
                }
                strRequest = strRequest != null ? (strRequest.ToLower() == "myrequest") ? "" : strRequest.ToUpper() : string.Empty;

                var lstHRRF = db.Database.SqlQuery<sp_GetHRRFListByEmpIdPractice_Result>("exec sp_GetHRRFListByEmpIdPractice @Practice, @Request,@Account, @EmpID",
                new System.Data.SqlClient.SqlParameter("Practice", strPractice),
                new System.Data.SqlClient.SqlParameter("Request", strRequest),
                new System.Data.SqlClient.SqlParameter("Account", strAccount),
                new System.Data.SqlClient.SqlParameter("EmpID", EmpId)).ToList<sp_GetHRRFListByEmpIdPractice_Result>();
                if (strRequest == "OPEN" && status != null)
                {
                    lstHRRF = lstHRRF.Where(s => s.RequestStatus.ToLower() == status.ToLower() && s.RequestType == RequestType).ToList();
                }
                if (strRequest == "SUMMARY")
                {                
				ViewBag.Holdex = lstHRRF.Where(a => a.RequestStatus == "Hold" && a.RequestType == "External").Count();
        
                    ViewBag.PendingDH = lstHRRF.Where(a => a.RequestStatus == "Pending For DH Approval" && a.RequestType == "External").Count();
                    ViewBag.Resume = lstHRRF.Where(a => a.RequestStatus == "Resume Pending"|| a.RequestStatus == "Resume pending" && a.RequestType == "External").Count();
                    ViewBag.Holdin = lstHRRF.Where(a => a.RequestStatus == "Hold" && a.RequestType == "Internal").Count();
                    ViewBag.Submit = lstHRRF.Where(a => a.RequestStatus == "Submitted" && a.RequestType == "Internal").Count();
                    ViewBag.Qualified = lstHRRF.Where(a => a.RequestStatus == "Qualified" && a.RequestType == "Internal").Count();
                    ViewBag.PendingPM = lstHRRF.Where(a => a.RequestStatus == "Pending For PM Approval" && a.RequestType == "Internal").Count();
                    var InternalGraph = db.Database.SqlQuery<GraphStatus>("exec [GetSummary] @RequestType", new SqlParameter("RequestType", "Internal")).ToList<GraphStatus>();
                    var ExternalGraph = db.Database.SqlQuery<GraphStatus>("exec [GetSummary] @RequestType", new SqlParameter("RequestType", "External")).ToList<GraphStatus>();
                    ViewBag.InternalGraph = InternalGraph;
                    var InternalTotal = 0;
                    InternalGraph.ForEach(summary => InternalTotal = InternalTotal + summary.ReqStatusCount);
                    ViewBag.InternalTotal = InternalTotal;
                    ViewBag.ExternalGraph = ExternalGraph;
                    var ExternalTotal = 0;
                    ExternalGraph.ForEach(summary => ExternalTotal = ExternalTotal + summary.ReqStatusCount);
                    ViewBag.ExternalTotal = ExternalTotal;
                    var ExternalSummaryList = db.Database.SqlQuery<SummaryExtrernalList>("exec [GetSummaryExternalList] @RequestType", new SqlParameter("RequestType", "External")).ToList<SummaryExtrernalList>();
                    ViewBag.ExternalSummaryList = ExternalSummaryList;
                    var Total = 0;
                    ExternalSummaryList.ForEach(summary => Total = Total + summary.TrCount);
                    ViewBag.Total = Total;
                    var InternalSummaryList = db.Database.SqlQuery<SummaryExtrernalList>("exec [GetSummaryExternalList] @RequestType", new SqlParameter("RequestType", "Internal")).ToList<SummaryExtrernalList>();
                    ViewBag.InternalSummaryList = InternalSummaryList;
                    var inTotal = 0;
                    InternalSummaryList.ForEach(summary => inTotal = inTotal + summary.TrCount);
                    ViewBag.inTotal = inTotal;
                 //   List<SummaryExtrernalByCriticality> Criticality = new List<SummaryExtrernalByCriticality>();
                 //   Criticality = db.Database.SqlQuery<SummaryExtrernalByCriticality>
                 // ("exec [GetCriticalityList] @RequestType", new SqlParameter("RequestType", "External")).ToList<SummaryExtrernalByCriticality>();
                 //   ViewBag.Criticality = Criticality;
                 //   Criticality = db.Database.SqlQuery<SummaryExtrernalByCriticality>
                 // ("exec [GetCriticalityList] @RequestType", new SqlParameter("RequestType", "Internal")).ToList<SummaryExtrernalByCriticality>();
                 //   ViewBag.InternalCriticality = Criticality;   
                 //   List<SummaryExtrernalByRequestStatus> RequestStatus = new List<SummaryExtrernalByRequestStatus>();
                 //   RequestStatus = db.Database.SqlQuery<SummaryExtrernalByRequestStatus>
                 // ("exec [GetRequestStatusList] @RequestType", new SqlParameter("RequestType", "External")).ToList<SummaryExtrernalByRequestStatus>();
                 //   ViewBag.RequestStatus = RequestStatus;
                 //   RequestStatus = db.Database.SqlQuery<SummaryExtrernalByRequestStatus>
                 //("exec [GetRequestStatusList] @RequestType", new SqlParameter("RequestType", "Internal")).ToList<SummaryExtrernalByRequestStatus>();
                 //   ViewBag.InternalRequestStatus = RequestStatus;
                  
					  List<CriticalityByAccountNamelst> lstcriticality = new List<CriticalityByAccountNamelst>();

             
                    lstcriticality = db.Database.SqlQuery<CriticalityByAccountNamelst>("exec GetCriticalityByAccountName").ToList<CriticalityByAccountNamelst>();
                    ViewBag.lstcriticality = lstcriticality;
					var EP0Total = 0;
                  lstcriticality.ForEach(Count => EP0Total = EP0Total + Count.EP0);
                    ViewBag.EP0Total = EP0Total;
                    var EP1Total = 0;
                    lstcriticality.ForEach(Count => EP1Total = EP1Total + Count.EP1);
                    ViewBag.EP1Total = EP1Total;
                    var EP2Total = 0;
                    lstcriticality.ForEach(Count => EP2Total = EP2Total + Count.EP2);
                    ViewBag.EP2Total = EP2Total;
                    var EP3Total = 0;
                    lstcriticality.ForEach(Count => EP3Total = EP3Total + Count.EP3);
                    ViewBag.EP3Total = EP3Total;
                    var IP0Total = 0;
                    lstcriticality.ForEach(Count => IP0Total = IP0Total + Count.IP0);
                    ViewBag.IP0Total = IP0Total;
                    var IP1Total = 0;
                    lstcriticality.ForEach(Count => IP1Total = IP1Total + Count.IP1);
                    ViewBag.IP1Total = IP1Total;
                    var IP2Total = 0;
                    lstcriticality.ForEach(Count => IP2Total = IP2Total + Count.IP2);
                    ViewBag.IP2Total = IP2Total;
                    var IP3Total = 0;
                    lstcriticality.ForEach(Count => IP3Total = IP3Total + Count.IP3);
                    ViewBag.IP3Total = IP3Total;					
                    return PartialView("_SummaryTRHome");
                }
                else
                {
                    return PartialView("_HRRFList", lstHRRF);
                }
            }
            catch (Exception ex)
            {
                Common.WriteExceptionErrorLog(ex);
                return Json("Error", JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetSummaryByAccountName(string AccountName, string Type, string Cricticality)
        {
            try
            {
                var accname = AccountName;
                var type = Type;
				var criticality = Cricticality;							   
                int? EmpId = Convert.ToInt32(Session["EmployeeId"]);
                 // strRequest = strRequest != null ? (strRequest.ToLower() == "myrequest") ? "" : strRequest.ToUpper() : string.Empty;
                 // var lstHRRF = db.Database.SqlQuery<sp_GetHRRFListByEmpIdPractice_Result>("exec sp_GetHRRFListByEmpIdPractice @Practice, @Request,@Account, @EmpID",        new SqlParameter("RequestType", "External")    
                  var ExternalSummarybyaccount = db.Database.SqlQuery<SummaryExtrernalByAccount>("exec [GetSummaryExternalListBySelect] @AccountName,@RequestType,@Criticality", new SqlParameter("AccountName",accname), new SqlParameter("RequestType", type), new SqlParameter("Criticality", criticality)).ToList<SummaryExtrernalByAccount>();
                 ViewBag.ExternalSummarybyaccount = ExternalSummarybyaccount;           
                 return Json(ExternalSummarybyaccount, JsonRequestBehavior.AllowGet);
            }
           
            catch (Exception ex)
            {
                Common.WriteExceptionErrorLog(ex);
                return Json("Error", JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetCountByCriticality( string Type, string Cricticality)
        {
            try
            {
               
                var type = Type;
                var criticality = Cricticality;
                int? EmpId = Convert.ToInt32(Session["EmployeeId"]);                 
                var GetcountbyCriticality = db.Database.SqlQuery<SummaryExtrernalByAccount>("exec [GetCriticalityList] @RequestType,@Criticality",  new SqlParameter("RequestType", type), new SqlParameter("Criticality", criticality)).ToList<SummaryExtrernalByAccount>();
                ViewBag.GetcountbyCriticality = GetcountbyCriticality;
                return Json(GetcountbyCriticality, JsonRequestBehavior.AllowGet);
            }

            catch (Exception ex)
            {
                Common.WriteExceptionErrorLog(ex);
                return Json("Error", JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetCountByStatus(string Type, string Status)
        {
            try
            {
                var type = Type;
                var status = Status;
                int? EmpId = Convert.ToInt32(Session["EmployeeId"]);                 
                List<SummaryExtrernalByAccount> GetcountbyStatus = new List<SummaryExtrernalByAccount>();
                GetcountbyStatus = db.Database.SqlQuery<SummaryExtrernalByAccount>("exec [GetRequestStatusList] @RequestType,@RequestStatus", new SqlParameter("RequestType", type), new SqlParameter("RequestStatus", status)).ToList<SummaryExtrernalByAccount>();
                GetcountbyStatus = GetcountbyStatus.OrderBy(s => s.Name).ToList();
                ViewBag.GetcountbyStatus = GetcountbyStatus;     
                return Json(GetcountbyStatus, JsonRequestBehavior.AllowGet);
            }

            catch (Exception ex)
            {
                Common.WriteExceptionErrorLog(ex);
                return Json("Error", JsonRequestBehavior.AllowGet);
            }
        }					
        public ActionResult ExportReport()
        {
            try
            {
                if (Convert.ToInt32(Session["EmployeeId"]) > 0)
                {
                    return Json("success", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    //ermsg = "Session expired";
                    return Json("Sessionexpired", JsonRequestBehavior.AllowGet);
                    //return RedirectToAction("SessionExpire", "Signout");
                }
            }
            catch (Exception ex)
            {
                Common.WriteExceptionErrorLog(ex);
                return Json("Error", JsonRequestBehavior.AllowGet);
            }    
        }
        //public ActionResult GenerateHRRFReport(string strPractice, string strRequest, string strAccount)
        //{
        //    try
        //    {
        //        int? EmpId = Convert.ToInt32(Session["EmployeeId"]);
        //        strRequest = (strRequest.ToLower() == "myrequest") ? "" : strRequest.ToUpper();
        //        //var lstHRRF = db.Database.SqlQuery<GetHRRFReportByPractice>("exec sp_GetHRRFReportByEmpIdPractice @Practice, @Request,@Account, @EmpID",
        //        var lstHRRF = db.Database.SqlQuery<GetHRRFReportByPractice>("exec sp_GetHRRFReportByEmpIdPractice @Practice, @Request,@Account, @EmpID",
        //                       new System.Data.SqlClient.SqlParameter("Practice", strPractice),
        //            new System.Data.SqlClient.SqlParameter("Request", strRequest),
        //            new System.Data.SqlClient.SqlParameter("Account", strAccount),
        //            new System.Data.SqlClient.SqlParameter("EmpID", EmpId)).ToList<GetHRRFReportByPractice>();
        //        int LastRow;
        //        // LastRow = (strRequest.ToUpper() == "OPEN") ? 64 : 68;
        //        LastRow = (strRequest.ToUpper() == "OPEN") ? 64 : 69;
        //        lstHRRF = lstHRRF.Where(x => x.Remarks != "This is an auto generated TR during bulk assignment" && x.Remarks != "This is an auto generated TR during new employee creation").ToList();
        //        #region Export to Excel
        //        using (ExcelPackage package = new ExcelPackage())
        //        {
        //            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("TR Information");
        //            worksheet.TabColor = System.Drawing.Color.Green;
        //            worksheet.DefaultRowHeight = 18f;
        //            worksheet.Row(1).Height = 20f;
        //            using (var range = worksheet.Cells[1, 1, 1, LastRow])
        //            {
        //                range.Style.Font.Bold = true;
        //                range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
        //                range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
        //                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#5B9BD5"));
        //                range.Style.Font.Color.SetColor(System.Drawing.Color.White);
        //                range.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
        //                range.Style.Border.Right.Color.SetColor(System.Drawing.Color.LightGray);
        //            }
        //            worksheet.Cells[1, 1].Value = "TR Number";
        //            //worksheet.Cells[1, 2].Value = "Old TR Number";
        //            worksheet.Cells[1, 2].Value = "Created Date";
        //            worksheet.Cells[1, 3].Value = "Ageing";
        //            worksheet.Cells[1, 4].Value = "Ageing Bucket";
        //            worksheet.Cells[1, 5].Value = "Internal to External Converstion Ageing";
        //            worksheet.Cells[1, 6].Value = "Internal to External Converstion AgeingBucket";
        //            worksheet.Cells[1, 7].Value = "External to internal Converstion Ageing";
        //            worksheet.Cells[1, 8].Value = "External to internal Converstion AgeingBucket";
        //            worksheet.Cells[1, 9].Value = "Organization Group";
        //            worksheet.Cells[1, 10].Value = "Purpose";
        //            // worksheet.Cells[1, 11].Value = "Project Name with Code";
        //            //worksheet.Cells[1, 12].Value = "Opportunity Code";
        //            worksheet.Cells[1, 11].Value = "Project Name / Opportunity Name";
        //            worksheet.Cells[1, 12].Value = "Project Code / Opportunity Code";
        //            // worksheet.Cells[1, 13].Value = "Opportunity Name";
        //            worksheet.Cells[1, 13].Value = "Account Name";
        //            worksheet.Cells[1, 14].Value = "Request Type";
        //            worksheet.Cells[1, 15].Value = "Cost Center";
        //            worksheet.Cells[1, 16].Value = "Practice";
        //            //worksheet.Cells[1, 17].Value = "Skill Category";

        //            worksheet.Cells[1, 17].Value = "Criticality";
        //            worksheet.Cells[1, 18].Value = "Skill Cluster";
        //            worksheet.Cells[1, 19].Value = "Skill";
        //            worksheet.Cells[1, 20].Value = "Skill Code";
        //            worksheet.Cells[1, 21].Value = "Cluster Description";
        //            worksheet.Cells[1, 22].Value = "Cluster JD";
        //            //worksheet.Cells[1, 20].Value = "Expert";

        //            worksheet.Cells[1, 23].Value = "Specific Platform";
        //            worksheet.Cells[1, 24].Value = "Certifications";

        //            worksheet.Cells[1, 25].Value = "Job Description";
        //            worksheet.Cells[1, 26].Value = "Grade";
        //            worksheet.Cells[1, 27].Value = "Years Of Experience";
        //            worksheet.Cells[1, 28].Value = "Billing Status";

        //            worksheet.Cells[1, 29].Value = "Stage";
        //            worksheet.Cells[1, 30].Value = "Request Status";
        //            worksheet.Cells[1, 31].Value = "TR Remarks";
        //            worksheet.Cells[1, 32].Value = "Fulfillment Date";
        //            worksheet.Cells[1, 33].Value = "Fulfillment Remarks";
        //            worksheet.Cells[1, 34].Value = "Expected Fulfillment Date";
        //            worksheet.Cells[1, 35].Value = "Location Type";
        //            worksheet.Cells[1, 36].Value = "Location Name";
        //            worksheet.Cells[1, 37].Value = "Positions";
        //            worksheet.Cells[1, 38].Value = "Fulfilled EmployeeId";
        //            worksheet.Cells[1, 39].Value = "Resource Name";
        //            worksheet.Cells[1, 40].Value = "Assignment Start Date";
        //            worksheet.Cells[1, 41].Value = "OverDue";
        //            worksheet.Cells[1, 42].Value = "Billing Date";
        //            worksheet.Cells[1, 43].Value = "Request Reason";
        //            worksheet.Cells[1, 44].Value = "Replaced EmpID";
        //            worksheet.Cells[1, 45].Value = "Replaced EmpName";
        //            worksheet.Cells[1, 46].Value = "Role Required";
        //            worksheet.Cells[1, 47].Value = "Created By EmpID";
        //            worksheet.Cells[1, 48].Value = "Created By EmpName";
        //            worksheet.Cells[1, 49].Value = "Date of Joining";
        //            worksheet.Cells[1, 50].Value = "Joining Month";
        //            worksheet.Cells[1, 51].Value = "Date from Internal to External";
        //            worksheet.Cells[1, 52].Value = "Date of Hold";
        //            worksheet.Cells[1, 53].Value = "Cancel Date";
        //            worksheet.Cells[1, 54].Value = "Cancel Reason";
        //            worksheet.Cells[1, 55].Value = "Recruiter Name";
        //            worksheet.Cells[1, 56].Value = "Contracting Allowed";
        //            worksheet.Cells[1, 57].Value = "Impact";
        //            worksheet.Cells[1, 58].Value = "DH Approval Date";
        //            worksheet.Cells[1, 59].Value = "Client Interview";
        //            worksheet.Cells[1, 60].Value = "First Level Technical Panel";
        //            worksheet.Cells[1, 61].Value = "Second Level Technical Panel";
        //            worksheet.Cells[1, 62].Value = "Requisition ID";
        //            worksheet.Cells[1, 63].Value = "Bill Rate";
        //            worksheet.Cells[1, 64].Value = "Max Salary";
        //            if (strRequest.ToUpper() != "OPEN")
        //            {
        //                worksheet.Cells[1, 65].Value = "Discipline";
        //                worksheet.Cells[1, 66].Value = "Role Group";
        //                worksheet.Cells[1, 67].Value = "Core Skill";
        //                worksheet.Cells[1, 68].Value = "Intermediate Skill";
        //                worksheet.Cells[1, 69].Value = "Advanced Skill";
        //            }

        //            //Set default column width
        //            worksheet.DefaultColWidth = 18f;

        //            worksheet.Column(1).Width = 13f;
        //            worksheet.Column(2).AutoFit(20f);
        //            worksheet.Column(3).AutoFit(20f);
        //            worksheet.Column(4).AutoFit(20f);

        //            worksheet.Column(5).AutoFit(30f);
        //            worksheet.Column(6).AutoFit(30f);
        //            worksheet.Column(7).AutoFit(30f);
        //            worksheet.Column(8).AutoFit(30f);

        //            worksheet.Column(9).AutoFit(20f);
        //            worksheet.Column(10).AutoFit(20f);
        //            worksheet.Column(11).AutoFit(40f);
        //            worksheet.Column(12).AutoFit(20f);
        //            worksheet.Column(13).AutoFit(50f);
        //            worksheet.Column(14).AutoFit(44f);
        //            worksheet.Column(16).AutoFit(30f);
        //            worksheet.Column(17).AutoFit(30f);
        //            worksheet.Column(18).AutoFit(30f);
        //            worksheet.Column(19).AutoFit(30f);
        //            worksheet.Column(20).AutoFit(60f);
        //            worksheet.Column(21).AutoFit(25f);
        //            worksheet.Column(22).AutoFit(60f);

        //            worksheet.Column(23).AutoFit(60f);
        //            worksheet.Column(24).AutoFit(10f);
        //            worksheet.Column(28).AutoFit(30f);
        //            worksheet.Column(29).AutoFit(30f);
        //            worksheet.Column(30).AutoFit(31f);
        //            worksheet.Column(31).AutoFit(30f);
        //            worksheet.Column(32).AutoFit(30f);
        //            worksheet.Column(35).AutoFit(30f);
        //            worksheet.Column(36).AutoFit(25f);
        //            worksheet.Column(37).AutoFit(20f);
        //            worksheet.Column(39).AutoFit(40f);
        //            worksheet.Column(41).AutoFit(40f);
        //            worksheet.Column(42).AutoFit(20f);
        //            worksheet.Column(43).AutoFit(40f);
        //            worksheet.Column(44).AutoFit(20f);
        //            worksheet.Column(45).AutoFit(30f);
        //            worksheet.Column(46).AutoFit(20f);
        //            worksheet.Column(47).AutoFit(30f);
        //            worksheet.Column(48).AutoFit(20f);
        //            worksheet.Column(49).AutoFit(20f);
        //            worksheet.Column(50).AutoFit(20f);
        //            worksheet.Column(51).AutoFit(30f);
        //            worksheet.Column(52).AutoFit(30f);
        //            worksheet.Column(53).AutoFit(30f);
        //            worksheet.Column(54).AutoFit(30f);
        //            worksheet.Column(55).AutoFit(30f);
        //            worksheet.Column(57).AutoFit(20f);
        //            worksheet.Column(58).AutoFit(20f);
        //            worksheet.Column(59).AutoFit(20f);
        //            worksheet.Column(60).AutoFit(20f);
        //            worksheet.Column(61).AutoFit(30f);
        //            worksheet.Column(62).AutoFit(30f);
        //            worksheet.Column(63).AutoFit(20f);
        //            worksheet.Column(64).AutoFit(20f);
        //            //worksheet.Column(65).Width = 50f;
        //            worksheet.Column(65).AutoFit(20f);
        //            worksheet.Column(66).Width = 60f;
        //            worksheet.Column(67).Width = 60f;
        //            worksheet.Column(68).Width = 60f;
        //            worksheet.Column(69).Width = 60f;
        //            //  worksheet.Column(70).Width = 60f;
        //            //Add the each row
        //            for (int rowIndex = 0, row = 2; rowIndex < lstHRRF.Count; rowIndex++, row++) // row indicates number of rows
        //            {

        //                worksheet.Cells[row, 1].Value = lstHRRF[rowIndex].HRRFNumber;
        //                //worksheet.Cells[row, 2].Value = lstHRRF[rowIndex].OldHRRFNumber;

        //                worksheet.Cells[row, 2].Value = lstHRRF[rowIndex].CreatedDate;
        //                worksheet.Cells[row, 2].Style.Numberformat.Format = "dd-MMM-yyyy";
        //                worksheet.Cells[row, 2].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

        //                worksheet.Cells[row, 3].Value = lstHRRF[rowIndex].Ageing;
        //                worksheet.Cells[row, 4].Value = lstHRRF[rowIndex].AgeingBucket;
        //                worksheet.Cells[row, 5].Value = lstHRRF[rowIndex].ExternalConverstionAgeing;
        //                worksheet.Cells[row, 6].Value = lstHRRF[rowIndex].ExternalConverstionAgeingBucket;
        //                worksheet.Cells[row, 7].Value = lstHRRF[rowIndex].InternalConverstionAgeing;
        //                worksheet.Cells[row, 8].Value = lstHRRF[rowIndex].InternalConverstionAgeingBucket;

        //                // worksheet.Cells[row, 9].Value = lstHRRF[rowIndex].OrganizationGroup;

        //                if (lstHRRF[rowIndex].OrganizationGroup == null)
        //                {
        //                    worksheet.Cells[row, 9].Value = "";
        //                }
        //                else
        //                {
        //                    worksheet.Cells[row, 9].Value = lstHRRF[rowIndex].OrganizationGroup;
        //                }
        //                worksheet.Cells[row, 10].Value = lstHRRF[rowIndex].Purpose;
        //                //worksheet.Cells[row, 11].Value = lstHRRF[rowIndex].ProjectNameWithCode;
        //                //worksheet.Cells[row, 12].Value = lstHRRF[rowIndex].OpportunityCode;
        //                if (lstHRRF[rowIndex].Purpose == "Project")
        //                {
        //                    worksheet.Cells[row, 11].Value = lstHRRF[rowIndex].ProjectName;
        //                    worksheet.Cells[row, 12].Value = lstHRRF[rowIndex].ProjectCode;
        //                }
        //                else if (lstHRRF[rowIndex].Purpose == "Opportunity")
        //                {
        //                    worksheet.Cells[row, 11].Value = lstHRRF[rowIndex].OpportunityName;
        //                    worksheet.Cells[row, 12].Value = lstHRRF[rowIndex].OpportunityCode;
        //                }
        //                else
        //                {

        //                    if ((lstHRRF[rowIndex].ProjectName != "" || lstHRRF[rowIndex].ProjectName != "NULL ") && (lstHRRF[rowIndex].OpportunityName != "" || lstHRRF[rowIndex].OpportunityName != "NULL "))
        //                    {
        //                        if (lstHRRF[rowIndex].OpportunityName != "NULL ")
        //                        {
        //                            worksheet.Cells[row, 11].Value = lstHRRF[rowIndex].ProjectName + "---" + lstHRRF[rowIndex].OpportunityName;
        //                        }
        //                        else
        //                        {
        //                            worksheet.Cells[row, 11].Value = lstHRRF[rowIndex].ProjectName;

        //                        }
        //                    }
        //                    if ((lstHRRF[rowIndex].ProjectCode != "" || lstHRRF[rowIndex].ProjectCode != "NULL ") && (lstHRRF[rowIndex].OpportunityCode != "" || lstHRRF[rowIndex].OpportunityCode != "NULL "))
        //                    {
        //                        if (lstHRRF[rowIndex].OpportunityCode != "NULL ")
        //                        {
        //                            worksheet.Cells[row, 12].Value = lstHRRF[rowIndex].ProjectCode + "---" + lstHRRF[rowIndex].OpportunityCode;

        //                        }
        //                        else
        //                        {
        //                            worksheet.Cells[row, 12].Value = lstHRRF[rowIndex].ProjectCode;

        //                        }
        //                    }

        //                    if (lstHRRF[rowIndex].OpportunityName == "")
        //                    {
        //                        worksheet.Cells[row, 11].Value = lstHRRF[rowIndex].ProjectName;
        //                    }
        //                    if (lstHRRF[rowIndex].OpportunityCode == "")
        //                    {
        //                        worksheet.Cells[row, 12].Value = lstHRRF[rowIndex].ProjectCode;
        //                    }

        //                    if (lstHRRF[rowIndex].ProjectName == "")
        //                    {
        //                        worksheet.Cells[row, 11].Value = lstHRRF[rowIndex].OpportunityName;
        //                    }
        //                    if (lstHRRF[rowIndex].ProjectCode == "")
        //                    {
        //                        worksheet.Cells[row, 12].Value = lstHRRF[rowIndex].OpportunityCode;
        //                    }




        //                }

        //                //  worksheet.Cells[row, 13].Value = lstHRRF[rowIndex].OpportunityName;
        //                worksheet.Cells[row, 13].Value = lstHRRF[rowIndex].AccountName;
        //                worksheet.Cells[row, 14].Value = lstHRRF[rowIndex].RequestType;
        //                worksheet.Cells[row, 15].Value = lstHRRF[rowIndex].CostCenter;
        //                worksheet.Cells[row, 16].Value = lstHRRF[rowIndex].ServiceLine;
        //                // worksheet.Cells[row, 17].Value = lstHRRF[rowIndex].SkillCategory;

        //                worksheet.Cells[row, 17].Value = lstHRRF[rowIndex].Criticality;
        //                worksheet.Cells[row, 18].Value = lstHRRF[rowIndex].SkillCluster;
        //                worksheet.Cells[row, 19].Value = lstHRRF[rowIndex].CSkill;
        //                worksheet.Cells[row, 20].Value = lstHRRF[rowIndex].SkillCode;
        //                worksheet.Cells[row, 21].Value = lstHRRF[rowIndex].CLSTRDESC;
        //                worksheet.Cells[row, 22].Value = lstHRRF[rowIndex].CLSTRJD;
        //                // worksheet.Cells[row, 20].Value = lstHRRF[rowIndex].Expert;


        //                worksheet.Cells[row, 23].Value = lstHRRF[rowIndex].SpecificPlatform;
        //                worksheet.Cells[row, 24].Value = lstHRRF[rowIndex].Certifications;

        //                // worksheet.Cells[row, 21].Value = lstHRRF[rowIndex].Expert + " " + lstHRRF[rowIndex].Proficient + " " + lstHRRF[rowIndex].Experienced;
        //                worksheet.Cells[row, 25].Value = lstHRRF[rowIndex].JobDescription;
        //                worksheet.Cells[row, 26].Value = lstHRRF[rowIndex].Grade;
        //                worksheet.Cells[row, 27].Value = lstHRRF[rowIndex].YearsOfExperience;
        //                worksheet.Cells[row, 28].Value = lstHRRF[rowIndex].BillingStatus;

        //                worksheet.Cells[row, 29].Value = lstHRRF[rowIndex].Stage;
        //                worksheet.Cells[row, 30].Value = lstHRRF[rowIndex].RequestStatus;
        //                worksheet.Cells[row, 31].Value = lstHRRF[rowIndex].Remarks;

        //                worksheet.Cells[row, 32].Value = lstHRRF[rowIndex].FulfillmentDate;
        //                worksheet.Cells[row, 32].Style.Numberformat.Format = "dd-MMM-yyyy";
        //                worksheet.Cells[row, 32].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

        //                worksheet.Cells[row, 33].Value = lstHRRF[rowIndex].FulfillmentRemarks;

        //                worksheet.Cells[row, 34].Value = lstHRRF[rowIndex].ExpectedFulfilmentDate;
        //                worksheet.Cells[row, 34].Style.Numberformat.Format = "dd-MMM-yyyy";
        //                worksheet.Cells[row, 34].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

        //                worksheet.Cells[row, 35].Value = lstHRRF[rowIndex].LocationType;
        //                worksheet.Cells[row, 36].Value = lstHRRF[rowIndex].LocationName;
        //                worksheet.Cells[row, 37].Value = lstHRRF[rowIndex].Positions;
        //                worksheet.Cells[row, 38].Value = lstHRRF[rowIndex].EmployeeId;
        //                worksheet.Cells[row, 39].Value = lstHRRF[rowIndex].ResourceName;

        //                worksheet.Cells[row, 40].Value = lstHRRF[rowIndex].AssignmentStartDate;
        //                worksheet.Cells[row, 40].Style.Numberformat.Format = "dd-MMM-yyyy";
        //                worksheet.Cells[row, 40].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

        //                worksheet.Cells[row, 41].Value = lstHRRF[rowIndex].OverDue;

        //                worksheet.Cells[row, 42].Value = lstHRRF[rowIndex].BillingDate;
        //                worksheet.Cells[row, 42].Style.Numberformat.Format = "dd-MMM-yyyy";
        //                worksheet.Cells[row, 42].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

        //                //worksheet.Cells[row, 40].Value = lstHRRF[rowIndex].DemandType;
        //                worksheet.Cells[row, 43].Value = lstHRRF[rowIndex].RequestReason;
        //                worksheet.Cells[row, 44].Value = lstHRRF[rowIndex].ReplacementEmpID;
        //                worksheet.Cells[row, 45].Value = lstHRRF[rowIndex].ReplacementName;
        //                worksheet.Cells[row, 46].Value = lstHRRF[rowIndex].RoleRequired;
        //                worksheet.Cells[row, 47].Value = lstHRRF[rowIndex].HRRFCreatedById;
        //                worksheet.Cells[row, 48].Value = lstHRRF[rowIndex].HRRFCreatedByName;

        //                worksheet.Cells[row, 49].Value = lstHRRF[rowIndex].DOJ;
        //                worksheet.Cells[row, 49].Style.Numberformat.Format = "dd-MMM-yyyy";
        //                worksheet.Cells[row, 49].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

        //                worksheet.Cells[row, 50].Value = lstHRRF[rowIndex].JoiningMonth;

        //                worksheet.Cells[row, 51].Value = lstHRRF[rowIndex].DateFromIntToExt;
        //                worksheet.Cells[row, 51].Style.Numberformat.Format = "dd-MMM-yyyy";
        //                worksheet.Cells[row, 51].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

        //                worksheet.Cells[row, 52].Value = lstHRRF[rowIndex].DateOfHold;
        //                worksheet.Cells[row, 52].Style.Numberformat.Format = "dd-MMM-yyyy";
        //                worksheet.Cells[row, 52].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

        //                worksheet.Cells[row, 53].Value = lstHRRF[rowIndex].CancelDate;
        //                worksheet.Cells[row, 53].Style.Numberformat.Format = "dd-MMM-yyyy";
        //                worksheet.Cells[row, 53].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

        //                worksheet.Cells[row, 54].Value = lstHRRF[rowIndex].CancelRemarks;
        //                worksheet.Cells[row, 55].Value = lstHRRF[rowIndex].RecruiterName;

        //                worksheet.Cells[row, 56].Value = lstHRRF[rowIndex].Contracting;
        //                worksheet.Cells[row, 57].Value = lstHRRF[rowIndex].Impact;


        //                worksheet.Cells[row, 58].Value = lstHRRF[rowIndex].DHApprovaerDate;
        //                worksheet.Cells[row, 58].Style.Numberformat.Format = "dd-MMM-yyyy";
        //                worksheet.Cells[row, 58].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

        //                worksheet.Cells[row, 59].Value = lstHRRF[rowIndex].ClientInterview;
        //                worksheet.Cells[row, 60].Value = lstHRRF[rowIndex].TECHPANEL;
        //                worksheet.Cells[row, 61].Value = lstHRRF[rowIndex].SECONDTECHPANEL;
        //                worksheet.Cells[row, 62].Value = lstHRRF[rowIndex].RequisitionID;
        //                if (lstHRRF[rowIndex].Billrate == null)
        //                {
        //                    worksheet.Cells[row, 63].Value = "";
        //                }
        //                else
        //                {
        //                    worksheet.Cells[row, 63].Value = lstHRRF[rowIndex].Billrate;
        //                }

        //                //maxsal

        //                if (lstHRRF[rowIndex].Maxsal == null)
        //                {
        //                    worksheet.Cells[row, 64].Value = "";
        //                }
        //                else
        //                {
        //                    worksheet.Cells[row, 64].Value = lstHRRF[rowIndex].Maxsal;
        //                }
        //                //maxsal

        //                if (strRequest.ToUpper() != "OPEN")
        //                {
        //                    worksheet.Cells[row, 65].Value = lstHRRF[rowIndex].Discipline;
        //                    worksheet.Cells[row, 66].Value = lstHRRF[rowIndex].RoleGroup;
        //                    worksheet.Cells[row, 67].Value = lstHRRF[rowIndex].CoreSkill;
        //                    worksheet.Cells[row, 68].Value = lstHRRF[rowIndex].IntermediateSkill;
        //                    worksheet.Cells[row, 69].Value = lstHRRF[rowIndex].AdvancedSkill;
        //                }
        //                if (row % 2 == 1)
        //                {
        //                    using (var range = worksheet.Cells[row, 1, row, LastRow])
        //                    {
        //                        range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
        //                        range.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#DDEBF7"));

        //                        range.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
        //                        range.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
        //                        range.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
        //                        range.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

        //                        range.Style.Border.Top.Color.SetColor(System.Drawing.Color.LightGray);
        //                        range.Style.Border.Left.Color.SetColor(System.Drawing.Color.LightGray);
        //                        range.Style.Border.Right.Color.SetColor(System.Drawing.Color.LightGray);
        //                        range.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.LightGray);
        //                    }
        //                }
        //            }

        //            Byte[] fileBytes = package.GetAsByteArray();
        //            Response.Clear();
        //            Response.Buffer = true;
        //            Response.AddHeader("content-disposition", "attachment;filename=" + ((strRequest == "") ? "My" : strRequest) + "-TR-Report" + DateTime.UtcNow.ToString("_dd-MMM-yyyy_hh-mm-ss") + ".xlsx");

        //            Response.Charset = "";
        //            Response.ContentType = "application/vnd.ms-excel";
        //            StringWriter sw = new StringWriter();
        //            Response.BinaryWrite(fileBytes);
        //            Response.End();
        //        }

        //        #endregion

        //        return new EmptyResult();
        //    }
        //    catch (Exception ex)
        //    {
        //        Common.WriteExceptionErrorLog(ex);
        //        //return Json("Error", JsonRequestBehavior.AllowGet);
        //        return RedirectToAction("Error", "Error");
        //    }
        //}
        public ActionResult GenerateHRRFReport(string strPractice, string strRequest, string strAccount)
        {
            try
            {
                int? EmpId = Convert.ToInt32(Session["EmployeeId"]);
                strRequest = (strRequest.ToLower() == "myrequest") ? "" : strRequest.ToUpper();
                var lstHRRF = db.Database.SqlQuery<GetHRRFReportByPractice>("exec sp_GetHRRFReportByEmpIdPractice @Practice, @Request,@Account, @EmpID",
                    new System.Data.SqlClient.SqlParameter("Practice", strPractice),
                    new System.Data.SqlClient.SqlParameter("Request", strRequest),
                    new System.Data.SqlClient.SqlParameter("Account", strAccount),
                    new System.Data.SqlClient.SqlParameter("EmpID", EmpId)).ToList<GetHRRFReportByPractice>();
                int LastRow;
                // LastRow = (strRequest.ToUpper() == "OPEN") ? 64 : 68;
                LastRow = (strRequest.ToUpper() == "OPEN") ? 68 : 73;
                lstHRRF = lstHRRF.Where(x => x.Remarks != "This is an auto generated TR during bulk assignment" && x.Remarks != "This is an auto generated TR during new employee creation").ToList();
                #region Export to Excel
                using (ExcelPackage package = new ExcelPackage())
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("TR Information");
                    worksheet.TabColor = System.Drawing.Color.Green;
                    worksheet.DefaultRowHeight = 18f;
                    worksheet.Row(1).Height = 20f;
                    using (var range = worksheet.Cells[1, 1, 1, LastRow])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                        range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#5B9BD5"));
                        range.Style.Font.Color.SetColor(System.Drawing.Color.White);
                        range.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        range.Style.Border.Right.Color.SetColor(System.Drawing.Color.LightGray);
                    }
                    worksheet.Cells[1, 1].Value = "TR Number";
                    //worksheet.Cells[1, 2].Value = "Old TR Number";
                    worksheet.Cells[1, 2].Value = "Created Date";
                    worksheet.Cells[1, 3].Value = "Ageing";
                    worksheet.Cells[1, 4].Value = "Ageing Bucket";
                    worksheet.Cells[1, 5].Value = "Internal to External Converstion Ageing";
                    worksheet.Cells[1, 6].Value = "Internal to External Converstion AgeingBucket";
                    worksheet.Cells[1, 7].Value = "External to internal Converstion Ageing";
                    worksheet.Cells[1, 8].Value = "External to internal Converstion AgeingBucket";
                    worksheet.Cells[1, 9].Value = "Organization Group";
                    worksheet.Cells[1, 10].Value = "Organization Sub Group";
                    worksheet.Cells[1, 11].Value = "Purpose";
                    worksheet.Cells[1, 12].Value = "Project Name with Code";
                    worksheet.Cells[1, 13].Value = "Opportunity Code";
                    worksheet.Cells[1, 14].Value = "Opportunity Name";
                    worksheet.Cells[1, 15].Value = "Account Name";
                    worksheet.Cells[1, 16].Value = "Request Type";
                    worksheet.Cells[1, 17].Value = "Cost Center";
                    worksheet.Cells[1, 18].Value = "Practice";
                    //worksheet.Cells[1, 17].Value = "Skill Category";

                    worksheet.Cells[1, 19].Value = "Criticality";
                    worksheet.Cells[1, 20].Value = "Skill Cluster";
                    worksheet.Cells[1, 21].Value = "Skill";
                    worksheet.Cells[1, 22].Value = "Skill Code";
                    worksheet.Cells[1, 23].Value = "Cluster Description";
                    worksheet.Cells[1, 24].Value = "Cluster JD";
                    //worksheet.Cells[1, 20].Value = "Expert";

                    worksheet.Cells[1, 25].Value = "Specific Platform";
                    worksheet.Cells[1, 26].Value = "Certifications";

                    worksheet.Cells[1, 27].Value = "Job Description";
                    worksheet.Cells[1, 28].Value = "Grade";
                    worksheet.Cells[1, 29].Value = "Years Of Experience";
                    worksheet.Cells[1, 30].Value = "Billing Status";

                    worksheet.Cells[1, 31].Value = "Stage";
                    worksheet.Cells[1, 32].Value = "Request Status";
                    worksheet.Cells[1, 33].Value = "TR Remarks";
                    worksheet.Cells[1, 34].Value = "Fulfillment Date";
                    worksheet.Cells[1, 35].Value = "Fulfillment Remarks";
                    worksheet.Cells[1, 36].Value = "Expected Fulfillment Date";
                    worksheet.Cells[1, 37].Value = "Location Type";
                    worksheet.Cells[1, 38].Value = "Location Name";
                    worksheet.Cells[1, 39].Value = "Positions";
                    worksheet.Cells[1, 40].Value = "Fulfilled EmployeeId";
                    worksheet.Cells[1, 41].Value = "Resource Name";
                    worksheet.Cells[1, 42].Value = "Assignment Start Date";
                    worksheet.Cells[1, 43].Value = "OverDue";
                    worksheet.Cells[1, 44].Value = "Billing Date";
                    worksheet.Cells[1, 45].Value = "Request Reason";
                    worksheet.Cells[1, 46].Value = "Replaced EmpID";
                    worksheet.Cells[1, 47].Value = "Replaced EmpName";
                    worksheet.Cells[1, 48].Value = "Role Required";
                    worksheet.Cells[1, 49].Value = "Created By EmpID";
                    worksheet.Cells[1, 50].Value = "Created By EmpName";
                    worksheet.Cells[1, 51].Value = "Date of Joining";
                    worksheet.Cells[1, 52].Value = "Joining Month";
                    worksheet.Cells[1, 53].Value = "Date from Internal to External";
                    worksheet.Cells[1, 54].Value = "Date of Hold";
                    worksheet.Cells[1, 55].Value = "Cancel Date";
                    worksheet.Cells[1, 56].Value = "Cancel Reason";
                    worksheet.Cells[1, 57].Value = "Recruiter Name";
                    worksheet.Cells[1, 58].Value = "Contracting Allowed";
                    worksheet.Cells[1, 59].Value = "Impact";
                    worksheet.Cells[1, 60].Value = "DH Approval Date";
                    worksheet.Cells[1, 61].Value = "Client Interview";
                    worksheet.Cells[1, 62].Value = "First Level Technical Panel";
                    worksheet.Cells[1, 63].Value = "Second Level Technical Panel";
                    worksheet.Cells[1, 64].Value = "Requisition ID";
                    worksheet.Cells[1, 65].Value = "Bill Rate";
                    worksheet.Cells[1, 66].Value = "Max Salary";
                    worksheet.Cells[1, 67].Value = "Status";
                    worksheet.Cells[1, 68].Value = "Reason For Raising TR";
                    if (strRequest.ToUpper() != "OPEN")
                    {
                        worksheet.Cells[1, 69].Value = "Discipline";
                        worksheet.Cells[1, 70].Value = "Role Group";
                        worksheet.Cells[1, 71].Value = "Core Skill";
                        worksheet.Cells[1, 72].Value = "Intermediate Skill";
                        worksheet.Cells[1, 73].Value = "Advanced Skill";
                     
                    }
                   
                    //Set default column width
                    worksheet.DefaultColWidth = 18f;

                    worksheet.Column(1).Width = 13f;
                    worksheet.Column(2).AutoFit(20f);
                    worksheet.Column(3).AutoFit(20f);
                    worksheet.Column(4).AutoFit(20f);

                    worksheet.Column(5).AutoFit(30f);
                    worksheet.Column(6).AutoFit(30f);
                    worksheet.Column(7).AutoFit(30f);
                    worksheet.Column(8).AutoFit(30f);

                    worksheet.Column(9).AutoFit(20f);
                    worksheet.Column(10).AutoFit(20f);
                    worksheet.Column(11).AutoFit(40f);
                    worksheet.Column(12).AutoFit(20f);
                    worksheet.Column(13).AutoFit(13f);
                    worksheet.Column(14).AutoFit(44f);
                    worksheet.Column(15).AutoFit(40f);
                    worksheet.Column(16).AutoFit(20f);
                    worksheet.Column(17).AutoFit(20f);
                    worksheet.Column(18).AutoFit(30f);
                    worksheet.Column(19).AutoFit(30f);
                    worksheet.Column(20).AutoFit(60f);
                    worksheet.Column(21).AutoFit(25f);
                    worksheet.Column(22).AutoFit(60f);

                    worksheet.Column(23).AutoFit(60f);
                    worksheet.Column(24).AutoFit(10f);
                    worksheet.Column(28).AutoFit(30f);
                    worksheet.Column(29).AutoFit(30f);
                    worksheet.Column(30).AutoFit(31f);
                    worksheet.Column(31).AutoFit(30f);
                    worksheet.Column(32).AutoFit(30f);
                    worksheet.Column(35).AutoFit(30f);
                    worksheet.Column(36).AutoFit(25f);
                    worksheet.Column(37).AutoFit(20f);
                    worksheet.Column(39).AutoFit(40f);
                    worksheet.Column(41).AutoFit(40f);
                    worksheet.Column(42).AutoFit(20f);
                    worksheet.Column(43).AutoFit(40f);
                    worksheet.Column(44).AutoFit(20f);
                    worksheet.Column(45).AutoFit(30f);
                    worksheet.Column(46).AutoFit(20f);
                    worksheet.Column(47).AutoFit(30f);
                    worksheet.Column(48).AutoFit(20f);
                    worksheet.Column(49).AutoFit(20f);
                    worksheet.Column(50).AutoFit(20f);
                    worksheet.Column(51).AutoFit(30f);
                    worksheet.Column(52).AutoFit(30f);
                    worksheet.Column(53).AutoFit(30f);
                    worksheet.Column(54).AutoFit(30f);
                    worksheet.Column(55).AutoFit(30f);
                    worksheet.Column(57).AutoFit(20f);
                    worksheet.Column(58).AutoFit(20f);
                    worksheet.Column(59).AutoFit(20f);
                    worksheet.Column(60).AutoFit(20f);
                    worksheet.Column(61).AutoFit(30f);
                    worksheet.Column(62).AutoFit(30f);
                    worksheet.Column(63).AutoFit(20f);
                    worksheet.Column(64).AutoFit(20f);
                    //worksheet.Column(65).Width = 50f;
                    worksheet.Column(65).AutoFit(20f);
                    worksheet.Column(66).Width = 18f;
                    worksheet.Column(67).Width = 18f;
                    worksheet.Column(68).Width = 60f;
                    worksheet.Column(69).Width = 60f;
                    worksheet.Column(70).Width = 60f;
                    worksheet.Column(71).Width = 20f;
                    //  worksheet.Column(70).Width = 60f;
                    //Add the each row
                    for (int rowIndex = 0, row = 2; rowIndex < lstHRRF.Count; rowIndex++, row++) // row indicates number of rows
                    {

                        worksheet.Cells[row, 1].Value = lstHRRF[rowIndex].HRRFNumber;
                        //worksheet.Cells[row, 2].Value = lstHRRF[rowIndex].OldHRRFNumber;

                        worksheet.Cells[row, 2].Value = lstHRRF[rowIndex].CreatedDate;
                        worksheet.Cells[row, 2].Style.Numberformat.Format = "dd-MMM-yyyy";
                        worksheet.Cells[row, 2].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                        worksheet.Cells[row, 3].Value = lstHRRF[rowIndex].Ageing;
                        worksheet.Cells[row, 4].Value = lstHRRF[rowIndex].AgeingBucket;
                        worksheet.Cells[row, 5].Value = lstHRRF[rowIndex].ExternalConverstionAgeing;
                        worksheet.Cells[row, 6].Value = lstHRRF[rowIndex].ExternalConverstionAgeingBucket;
                        worksheet.Cells[row, 7].Value = lstHRRF[rowIndex].InternalConverstionAgeing;
                        worksheet.Cells[row, 8].Value = lstHRRF[rowIndex].InternalConverstionAgeingBucket;

                        // worksheet.Cells[row, 9].Value = lstHRRF[rowIndex].OrganizationGroup;

                        if (lstHRRF[rowIndex].OrganizationGroup == null)
                        {
                            worksheet.Cells[row, 9].Value = "";
                        }
                        else
                        {
                            worksheet.Cells[row, 9].Value = lstHRRF[rowIndex].OrganizationGroup;
                        }


                        if (lstHRRF[rowIndex].OrganizationSubGroup == null)
                        {
                            worksheet.Cells[row, 10].Value = "";
                        }
                        else
                        {
                            worksheet.Cells[row, 10].Value = lstHRRF[rowIndex].OrganizationSubGroup;
                        }

                        worksheet.Cells[row, 11].Value = lstHRRF[rowIndex].Purpose;
                        worksheet.Cells[row, 12].Value = lstHRRF[rowIndex].ProjectNameWithCode;
                        worksheet.Cells[row, 13].Value = lstHRRF[rowIndex].OpportunityCode;
                        worksheet.Cells[row, 14].Value = lstHRRF[rowIndex].OpportunityName;
                        worksheet.Cells[row, 15].Value = lstHRRF[rowIndex].AccountName;
                        worksheet.Cells[row, 16].Value = lstHRRF[rowIndex].RequestType;
                        worksheet.Cells[row, 17].Value = lstHRRF[rowIndex].CostCenter;
                        worksheet.Cells[row, 18].Value = lstHRRF[rowIndex].ServiceLine;
                        // worksheet.Cells[row, 17].Value = lstHRRF[rowIndex].SkillCategory;

                        worksheet.Cells[row, 19].Value = lstHRRF[rowIndex].Criticality;
                        worksheet.Cells[row, 20].Value = lstHRRF[rowIndex].SkillCluster;
                        worksheet.Cells[row, 21].Value = lstHRRF[rowIndex].CSkill;
                        worksheet.Cells[row, 22].Value = lstHRRF[rowIndex].SkillCode;
                        worksheet.Cells[row, 23].Value = lstHRRF[rowIndex].CLSTRDESC;
                        worksheet.Cells[row, 24].Value = lstHRRF[rowIndex].CLSTRJD;
                        // worksheet.Cells[row, 20].Value = lstHRRF[rowIndex].Expert;


                        worksheet.Cells[row, 25].Value = lstHRRF[rowIndex].SpecificPlatform;
                        worksheet.Cells[row, 26].Value = lstHRRF[rowIndex].Certifications;

                        // worksheet.Cells[row, 21].Value = lstHRRF[rowIndex].Expert + " " + lstHRRF[rowIndex].Proficient + " " + lstHRRF[rowIndex].Experienced;
                        worksheet.Cells[row, 27].Value = lstHRRF[rowIndex].JobDescription;
                        worksheet.Cells[row, 28].Value = lstHRRF[rowIndex].Grade;
                        worksheet.Cells[row, 29].Value = lstHRRF[rowIndex].YearsOfExperience;
                        worksheet.Cells[row, 30].Value = lstHRRF[rowIndex].BillingStatus;
                        worksheet.Cells[row, 31].Value = lstHRRF[rowIndex].Stage;
                        worksheet.Cells[row, 32].Value = lstHRRF[rowIndex].RequestStatus;
                        worksheet.Cells[row, 33].Value = lstHRRF[rowIndex].Remarks;

                        worksheet.Cells[row, 34].Value = lstHRRF[rowIndex].FulfillmentDate;
                        worksheet.Cells[row, 34].Style.Numberformat.Format = "dd-MMM-yyyy";
                        worksheet.Cells[row, 34].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                        worksheet.Cells[row, 35].Value = lstHRRF[rowIndex].FulfillmentRemarks;

                        worksheet.Cells[row, 36].Value = lstHRRF[rowIndex].ExpectedFulfilmentDate;
                        worksheet.Cells[row, 36].Style.Numberformat.Format = "dd-MMM-yyyy";
                        worksheet.Cells[row, 36].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                        worksheet.Cells[row, 37].Value = lstHRRF[rowIndex].LocationType;
                        worksheet.Cells[row, 38].Value = lstHRRF[rowIndex].LocationName;
                        worksheet.Cells[row, 39].Value = lstHRRF[rowIndex].Positions;
                        worksheet.Cells[row, 40].Value = lstHRRF[rowIndex].EmployeeId;
                        worksheet.Cells[row, 41].Value = lstHRRF[rowIndex].ResourceName;

                        worksheet.Cells[row, 42].Value = lstHRRF[rowIndex].AssignmentStartDate;
                        worksheet.Cells[row, 42].Style.Numberformat.Format = "dd-MMM-yyyy";
                        worksheet.Cells[row, 42].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                        worksheet.Cells[row, 43].Value = lstHRRF[rowIndex].OverDue;

                        worksheet.Cells[row, 44].Value = lstHRRF[rowIndex].BillingDate;
                        worksheet.Cells[row, 44].Style.Numberformat.Format = "dd-MMM-yyyy";
                        worksheet.Cells[row, 44].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                        //worksheet.Cells[row, 40].Value = lstHRRF[rowIndex].DemandType;
                        worksheet.Cells[row, 45].Value = lstHRRF[rowIndex].RequestReason;
                        worksheet.Cells[row, 46].Value = lstHRRF[rowIndex].ReplacementEmpID;
                        worksheet.Cells[row, 47].Value = lstHRRF[rowIndex].ReplacementName;
                        worksheet.Cells[row, 48].Value = lstHRRF[rowIndex].RoleRequired;
                        worksheet.Cells[row, 49].Value = lstHRRF[rowIndex].HRRFCreatedById;
                        worksheet.Cells[row, 50].Value = lstHRRF[rowIndex].HRRFCreatedByName;

                        worksheet.Cells[row, 51].Value = lstHRRF[rowIndex].DOJ;
                        worksheet.Cells[row, 51].Style.Numberformat.Format = "dd-MMM-yyyy";
                        worksheet.Cells[row, 51].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                        worksheet.Cells[row, 52].Value = lstHRRF[rowIndex].JoiningMonth;

                        worksheet.Cells[row, 53].Value = lstHRRF[rowIndex].DateFromIntToExt;
                        worksheet.Cells[row, 53].Style.Numberformat.Format = "dd-MMM-yyyy";
                        worksheet.Cells[row, 53].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                        worksheet.Cells[row, 54].Value = lstHRRF[rowIndex].DateOfHold;
                        worksheet.Cells[row, 54].Style.Numberformat.Format = "dd-MMM-yyyy";
                        worksheet.Cells[row, 54].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                        worksheet.Cells[row, 55].Value = lstHRRF[rowIndex].CancelDate;
                        worksheet.Cells[row, 55].Style.Numberformat.Format = "dd-MMM-yyyy";
                        worksheet.Cells[row, 55].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                        worksheet.Cells[row, 56].Value = lstHRRF[rowIndex].CancelRemarks;
                        worksheet.Cells[row, 57].Value = lstHRRF[rowIndex].RecruiterName;

                        worksheet.Cells[row, 58].Value = lstHRRF[rowIndex].Contracting;
                        worksheet.Cells[row, 59].Value = lstHRRF[rowIndex].Impact;


                        worksheet.Cells[row, 60].Value = lstHRRF[rowIndex].DHApprovaerDate;
                        worksheet.Cells[row, 60].Style.Numberformat.Format = "dd-MMM-yyyy";
                        worksheet.Cells[row, 60].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                        worksheet.Cells[row, 61].Value = lstHRRF[rowIndex].ClientInterview;
                        worksheet.Cells[row, 62].Value = lstHRRF[rowIndex].TECHPANEL;
                        worksheet.Cells[row, 63].Value = lstHRRF[rowIndex].SECONDTECHPANEL;
                        worksheet.Cells[row, 64].Value = lstHRRF[rowIndex].RequisitionID;
                        if (lstHRRF[rowIndex].Billrate == null)
                        {
                            worksheet.Cells[row, 65].Value = "";
                        }
                        else
                        {
                            worksheet.Cells[row, 65].Value = lstHRRF[rowIndex].Billrate;
                        }

                        //maxsal

                        if (lstHRRF[rowIndex].Maxsal == null)
                        {
                            worksheet.Cells[row, 66].Value = "";
                        }
                        else
                        {
                            worksheet.Cells[row, 66].Value = lstHRRF[rowIndex].Maxsal;
                        }
                        //maxsal

                        if (strRequest.ToUpper() != "OPEN")
                        {
                            worksheet.Cells[row, 69].Value = lstHRRF[rowIndex].Discipline;
                            worksheet.Cells[row, 70].Value = lstHRRF[rowIndex].RoleGroup;
                            worksheet.Cells[row, 71].Value = lstHRRF[rowIndex].CoreSkill;
                            worksheet.Cells[row, 72].Value = lstHRRF[rowIndex].IntermediateSkill;
                            worksheet.Cells[row, 73].Value = lstHRRF[rowIndex].AdvancedSkill;
                        }
                        if (lstHRRF[rowIndex].HrrfStatus == null)
                        {
                            worksheet.Cells[row, 67].Value = "";
                        }
                        else
                        {
                            worksheet.Cells[row, 67].Value = lstHRRF[rowIndex].HrrfStatus;
                        }
                        
                        worksheet.Cells[row, 67].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                        if (lstHRRF[rowIndex].ReasonRaisingTR == null)
                        {
                            worksheet.Cells[row, 68].Value = "";
                        }
                        else
                        {
                            worksheet.Cells[row, 68].Value = lstHRRF[rowIndex].ReasonRaisingTR;
                        }
                        worksheet.Cells[row, 68].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                        if (row % 2 == 1)
                        {
                            using (var range = worksheet.Cells[row, 1, row, LastRow])
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
                    Response.AddHeader("content-disposition", "attachment;filename=" + ((strRequest == "") ? "My" :  strRequest) + "-TR-Report" + DateTime.UtcNow.ToString("_dd-MMM-yyyy_hh-mm-ss") + ".xlsx");

                    Response.Charset = "";
                    Response.ContentType = "application/vnd.ms-excel";
                    StringWriter sw = new StringWriter();
                    Response.BinaryWrite(fileBytes);
                    Response.End();
                }

                #endregion

                return new EmptyResult();
            }
            catch (Exception ex)
            {
                Common.WriteExceptionErrorLog(ex);
                //return Json("Error", JsonRequestBehavior.AllowGet);
                return RedirectToAction("Error", "Error");
            }
        }


        public ActionResult GenerateHRRFSubReport(string strPractice, string strRequest, string strAccount)
        {
            try
            {
                int? EmpId = Convert.ToInt32(Session["EmployeeId"]);
                strRequest = (strRequest.ToLower() == "myrequest") ? "" : strRequest.ToUpper();
                var lstHRRF = db.Database.SqlQuery<GetHRRFReportByPracticeSub>("exec sp_GetHRRFReportByEmpIdPracticeSub @Practice, @Request,@Account, @EmpID",
                    new System.Data.SqlClient.SqlParameter("Practice", strPractice),
                    new System.Data.SqlClient.SqlParameter("Request", strRequest),
                    new System.Data.SqlClient.SqlParameter("Account", strAccount),
                    new System.Data.SqlClient.SqlParameter("EmpID", EmpId)).ToList<GetHRRFReportByPracticeSub>();
                int LastRow;
                // LastRow = (strRequest.ToUpper() == "OPEN") ? 64 : 68;
                //  LastRow = (strRequest.ToUpper() == "OPEN") ? 17: 70;
                LastRow = 33;
                lstHRRF = lstHRRF.Where(x => x.Remarks != "This is an auto generated TR during bulk assignment" && x.Remarks != "This is an auto generated TR during new employee creation").ToList();
                #region Export to Excel
                using (ExcelPackage package = new ExcelPackage())
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("TR Information");
                    worksheet.TabColor = System.Drawing.Color.Green;
                    worksheet.DefaultRowHeight = 18f;
                    worksheet.Row(1).Height = 20f;
                    using (var range = worksheet.Cells[1, 1, 1, LastRow])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                        range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#5B9BD5"));
                        range.Style.Font.Color.SetColor(System.Drawing.Color.White);
                        range.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        range.Style.Border.Right.Color.SetColor(System.Drawing.Color.LightGray);
                    }

                    worksheet.Cells[1, 1].Value = "BU";
                    worksheet.Cells[1, 2].Value = "Account Name ";
                    worksheet.Cells[1, 3].Value = "Practice ";
                    worksheet.Cells[1, 4].Value = "Ageing Bucket";

                    worksheet.Cells[1, 5].Value = "Skill Cluster";
                    worksheet.Cells[1, 6].Value = "Skill";
                    worksheet.Cells[1, 7].Value = "Skill Code";
                    worksheet.Cells[1, 8].Value = "Specific skills";
                    worksheet.Cells[1, 9].Value = "TR Number";
                    worksheet.Cells[1, 10].Value = "Criticality";
                    worksheet.Cells[1, 11].Value = "Impact";
                    worksheet.Cells[1, 12].Value = "Reason";
                    worksheet.Cells[1, 13].Value = "Assignment Start Date";
                    worksheet.Cells[1, 14].Value = "Submitted Date";
                    worksheet.Cells[1, 15].Value = "Ageing";
                    worksheet.Cells[1, 16].Value = "Grade";
                    worksheet.Cells[1, 17].Value = "Raised by";
                    worksheet.Cells[1, 18].Value = "Replaced EmpID";
                    worksheet.Cells[1, 19].Value = "Replaced EmpName";
                    worksheet.Cells[1, 20].Value = "First Level Technical Panel";
                    worksheet.Cells[1, 21].Value = "Second Level Technical Panel";
                    worksheet.Cells[1, 22].Value = "TA Lead";
                    worksheet.Cells[1, 23].Value = "Recruiter";
                    worksheet.Cells[1, 24].Value = "BO SPOC";
                    worksheet.Cells[1, 25].Value = "Validated By";
                    worksheet.Cells[1, 26].Value = "Opportunity/ Project Code";
                    worksheet.Cells[1, 27].Value = "Opporuntiy/ Project Name";
                    worksheet.Cells[1, 28].Value = "Sales Stage";
                    worksheet.Cells[1, 29].Value = "Request Type";
                    worksheet.Cells[1, 30].Value = "Organisation Subgroup";
					worksheet.Cells[1, 31].Value = "Status";
                    worksheet.Cells[1, 32].Value = "Remarks";
                    worksheet.Cells[1, 33].Value = "Practice A";
                    //Set default column width
                    worksheet.DefaultColWidth = 20f;

                    worksheet.Column(1).Width = 20f;
                    worksheet.Column(2).AutoFit(40f);
                    worksheet.Column(3).AutoFit(20f);
                    worksheet.Column(4).AutoFit(20f);
                    worksheet.Column(5).AutoFit(20f);
                    worksheet.Column(6).AutoFit(20f);
                    worksheet.Column(7).AutoFit(14f);
                    worksheet.Column(8).AutoFit(43f);
                    worksheet.Column(9).AutoFit(14f);
                    worksheet.Column(10).AutoFit(10f);

                    worksheet.Column(11).AutoFit(20f);
                    worksheet.Column(12).AutoFit(20f);
                    worksheet.Column(13).AutoFit(27f);
                    worksheet.Column(14).AutoFit(27f);
                    worksheet.Column(15).AutoFit(10f);
                    worksheet.Column(16).AutoFit(10f);
                    worksheet.Column(17).AutoFit(31f);
                    worksheet.Column(18).AutoFit(17f);
                    worksheet.Column(19).AutoFit(29f);
                    worksheet.Column(20).AutoFit(30f);
                    worksheet.Column(21).AutoFit(30f);
                    worksheet.Column(22).AutoFit(20f);
                    worksheet.Column(23).AutoFit(20f);
                    worksheet.Column(24).AutoFit(20f);
                    worksheet.Column(25).AutoFit(20f);
                    worksheet.Column(26).AutoFit(25f);
                    worksheet.Column(27).AutoFit(53f);
                    worksheet.Column(28).AutoFit(20f);
                    worksheet.Column(29).AutoFit(20f);
                    worksheet.Column(30).AutoFit(20f);
					worksheet.Column(31).AutoFit(20f);
                    worksheet.Column(32).AutoFit(50f);
                    worksheet.Column(33).AutoFit(20f);
                    //Add the each row
                    for (int rowIndex = 0, row = 2; rowIndex < lstHRRF.Count; rowIndex++, row++) // row indicates number of rows
                    {

                        if (lstHRRF[rowIndex].OrganizationGroup == null)
                        {
                            worksheet.Cells[row, 1].Value = "";
                        }
                        else
                        {
                            worksheet.Cells[row, 1].Value = lstHRRF[rowIndex].OrganizationGroup;
                        }
                        worksheet.Cells[row, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;

                        worksheet.Cells[row, 2].Value = lstHRRF[rowIndex].AccountName;
                        worksheet.Cells[row, 2].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;

                        worksheet.Cells[row, 3].Value = lstHRRF[rowIndex].Practice;
                        worksheet.Cells[row, 3].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;

                        worksheet.Cells[row, 4].Value = lstHRRF[rowIndex].AgeingBucket;
                        worksheet.Cells[row, 4].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;

                        worksheet.Cells[row, 5].Value = lstHRRF[rowIndex].SkillCluster;
                        worksheet.Cells[row, 5].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;

                        worksheet.Cells[row, 6].Value = lstHRRF[rowIndex].CSkill;
                        worksheet.Cells[row, 6].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                   
                        worksheet.Cells[row, 7].Value = lstHRRF[rowIndex].SkillCode;
                        worksheet.Cells[row, 7].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;

                        worksheet.Cells[row, 8].Value = lstHRRF[rowIndex].SpecificPlatform;
                        worksheet.Cells[row, 8].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;

                        worksheet.Cells[row, 9].Value = lstHRRF[rowIndex].HRRFNumber;
                        worksheet.Cells[row, 9].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;

                        worksheet.Cells[row, 10].Value = lstHRRF[rowIndex].Criticality;
                        worksheet.Cells[row, 10].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;

                        worksheet.Cells[row, 11].Value = lstHRRF[rowIndex].Impact;
                        worksheet.Cells[row, 11].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;

                        worksheet.Cells[row, 12].Value = lstHRRF[rowIndex].RequestReason;
                        worksheet.Cells[row, 12].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;

                        worksheet.Cells[row, 13].Value = lstHRRF[rowIndex].AssignmentStartDate;
                        worksheet.Cells[row, 13].Style.Numberformat.Format = "dd-MMM-yyyy";
                        worksheet.Cells[row, 13].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                        worksheet.Cells[row, 14].Value = lstHRRF[rowIndex].CreatedDate;
                        worksheet.Cells[row, 14].Style.Numberformat.Format = "dd-MMM-yyyy";
                        worksheet.Cells[row, 14].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                        worksheet.Cells[row, 15].Value = lstHRRF[rowIndex].Ageing;
                        worksheet.Cells[row, 15].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;

                        worksheet.Cells[row, 16].Value = lstHRRF[rowIndex].Grade;
                        worksheet.Cells[row, 16].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;

                        worksheet.Cells[row, 17].Value = lstHRRF[rowIndex].HRRFCreatedByName;
                        worksheet.Cells[row, 17].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;

                        worksheet.Cells[row, 18].Value = lstHRRF[rowIndex].ReplacementEmpID;
                        worksheet.Cells[row, 18].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;

                        worksheet.Cells[row, 19].Value = lstHRRF[rowIndex].ReplacementName;
                        worksheet.Cells[row, 19].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;

                        worksheet.Cells[row, 20].Value = lstHRRF[rowIndex].TECHPANEL;
                        worksheet.Cells[row, 20].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;

                        worksheet.Cells[row, 21].Value = lstHRRF[rowIndex].SECONDTECHPANEL;
                        worksheet.Cells[row, 21].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;

                        if (lstHRRF[rowIndex].Talead == null)
                        {
                            worksheet.Cells[row, 22].Value = "";
                        }
                        else
                        {
                            worksheet.Cells[row, 22].Value = lstHRRF[rowIndex].Talead;
                        }
                        worksheet.Cells[row, 22].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;


                        if (lstHRRF[rowIndex].Rectr == null)
                        {
                            worksheet.Cells[row, 23].Value = "";
                        }
                        else
                        {
                            worksheet.Cells[row, 23].Value = lstHRRF[rowIndex].Rectr;
                        }
                        worksheet.Cells[row, 23].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;

                        if (lstHRRF[rowIndex].BoSpoc == null)
                        {
                            worksheet.Cells[row, 24].Value = "";
                        }
                        else
                        {
                            worksheet.Cells[row, 24].Value = lstHRRF[rowIndex].BoSpoc;
                        }
                        worksheet.Cells[row, 24].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;

                        //worksheet.Cells[row, 25].Value = lstHRRF[rowIndex].Validatedby;

                        if (lstHRRF[rowIndex].OpportunityCode == null || lstHRRF[rowIndex].OpportunityCode == "")
                        {
                            worksheet.Cells[row, 26].Value = lstHRRF[rowIndex].ProjectCode;
                        }
                        else
                        {
                            worksheet.Cells[row, 26].Value = lstHRRF[rowIndex].OpportunityCode;
                        }
                        worksheet.Cells[row, 26].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;

                        if (lstHRRF[rowIndex].OpportunityName == null || lstHRRF[rowIndex].OpportunityName == "")
                        {
                            worksheet.Cells[row, 27].Value = lstHRRF[rowIndex].ProjectName;
                        }
                        else
                        {
                            worksheet.Cells[row, 27].Value = lstHRRF[rowIndex].OpportunityName;
                        }
                        worksheet.Cells[row, 27].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;

                        worksheet.Cells[row, 28].Value = lstHRRF[rowIndex].SalesStage;
                        worksheet.Cells[row, 28].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                 

                        worksheet.Cells[row, 29].Value = lstHRRF[rowIndex].RequestType;
                        worksheet.Cells[row, 29].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;

                        worksheet.Cells[row, 30].Value = lstHRRF[rowIndex].Organization_SubGroup;
                        worksheet.Cells[row, 30].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;

						 if (lstHRRF[rowIndex].HrrfStatus == null)
                        {
                            worksheet.Cells[row, 31].Value = "";
                        }
                        else
                        {
                            worksheet.Cells[row, 31].Value = lstHRRF[rowIndex].HrrfStatus;
                        }
                        worksheet.Cells[row, 31].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;

                        if (lstHRRF[rowIndex].HrrfRemarks == null)
                        {
                            worksheet.Cells[row, 32].Value = "";
                        }
                        else
                        {
                            worksheet.Cells[row, 32].Value = lstHRRF[rowIndex].HrrfRemarks;
                        }
                        worksheet.Cells[row, 32].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;

                        worksheet.Cells[row, 33].Value = lstHRRF[rowIndex].PracticeA;
                        worksheet.Cells[row, 33].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;

                        if (row % 2 == 1)
                        {
                            using (var range = worksheet.Cells[row, 1, row, LastRow])
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
                    Response.AddHeader("content-disposition", "attachment;filename=" + ((strRequest == "") ? "My" :"Sub_" + strRequest) + "-TR-Report" + DateTime.UtcNow.ToString("_dd-MMM-yyyy_hh-mm-ss") + ".xlsx");

                    Response.Charset = "";
                    Response.ContentType = "application/vnd.ms-excel";
                    StringWriter sw = new StringWriter();
                    Response.BinaryWrite(fileBytes);
                    Response.End();
                }

                #endregion

                return new EmptyResult();
            }
            catch (Exception ex)
            {
                Common.WriteExceptionErrorLog(ex);
                //return Json("Error", JsonRequestBehavior.AllowGet);
                return RedirectToAction("Error", "Error");
            }
        }

        public ActionResult GetHRRFDetailsBySearchTR(string HRRFNumber="",string SkillCode="",string strRequest="")
        {
            try
            {
                if (Convert.ToInt32(Session["EmployeeId"]) > 0)
                {
                    ViewBag.ActiveTR = strRequest;
                    List<sp_GetHRRFListByEmpIdPractice_Result> objHRRFDetails = new List<sp_GetHRRFListByEmpIdPractice_Result>();
                    if (HRRFNumber != "" && SkillCode == "")
                    {
                        objHRRFDetails = (from h in db.HRRFs
                                          where h.HRRFNumber.Contains(HRRFNumber)
                                          select new
                                          {
                                              HRRFID = h.HRRFID,
                                              HRRFNumber = h.HRRFNumber,
                                              HRRFSubmitedDate = h.HRRFSubmitedDate,
                                              JobDescription = h.JobDescription,
                                              CLSTRJD = h.CLSTRJD,
                                              LocationName = h.LocationName,
                                              Practice = h.Practice,
                                              SkillCode = h.SkillCode,
                                              Project = (h.Purpose.ToLower().Contains("project") ? "For Project: " + h.ProjectName :
                                              (h.Purpose.ToLower().Contains("opportunity") ? "For Oppotunity: " + h.OpportunityName :
                                              (h.Purpose.ToLower().Contains("proactive hire") ? "Proactive Hire" : "Corporate Function"))),
                                              Purpose = h.Purpose,
                                              RequestStatus = h.RequestStatus,
                                              RequestType = h.RequestType,
                                              ResourceName = h.ResourceName,
                                              RoleRequired = h.RoleRequired,
                                              grade = h.Grade,
                                              Remarks = h.Remarks,
                                              OverDue = "",
                                              FLAG = ""
                                          }
                                       )
                                       .AsEnumerable().Select(x => new sp_GetHRRFListByEmpIdPractice_Result
                                       {
                                          HRRFID = x.HRRFID,
                                           HRRFNumber = x.HRRFNumber,
                                           HRRFSubmitedDate = x.HRRFSubmitedDate,
                                           JobDescription = x.JobDescription,
                                           CLSTRJD = x.CLSTRJD,
                                           LocationName = x.LocationName,
                                           Practice = x.Practice,
                                           Project = x.Project,
                                           SkillCode = x.SkillCode,
                                           Purpose = x.Purpose,
                                           RequestStatus = x.RequestStatus,
                                           RequestType = x.RequestType,
                                           ResourceName = x.ResourceName,
                                           RoleRequired = ((db.DesignationMasters.Where(y => y.DesignationCode.Equals(x.RoleRequired))).FirstOrDefault() == null) ? x.RoleRequired : (db.DesignationMasters.Where(y => y.DesignationCode.Equals(x.RoleRequired))).FirstOrDefault().DesignationName,
                                           Grade = x.grade,
                                           Remarks = x.Remarks,
                                           OverDue = "",
                                           FLAG = null
                                       }).ToList();
                    }
                    else if (HRRFNumber == "" && SkillCode != "")
                    {
                        objHRRFDetails = (from h in db.HRRFs
                                          where h.SkillCode.Contains(SkillCode)
                                          select new
                                          {
                                              HRRFID = h.HRRFID,
                                              HRRFNumber = h.HRRFNumber,
                                              HRRFSubmitedDate = h.HRRFSubmitedDate,
                                              JobDescription = h.JobDescription,
                                              CLSTRJD = h.CLSTRJD,
                                              LocationName = h.LocationName,
                                              Practice = h.Practice,
                                              SkillCode = h.SkillCode,
                                              Project = (h.Purpose.ToLower().Contains("project") ? "For Project: " + h.ProjectName :
                                              (h.Purpose.ToLower().Contains("opportunity") ? "For Oppotunity: " + h.OpportunityName :
                                              (h.Purpose.ToLower().Contains("proactive hire") ? "Proactive Hire" : "Corporate Function"))),
                                              Purpose = h.Purpose,
                                              RequestStatus = h.RequestStatus,
                                              RequestType = h.RequestType,
                                              ResourceName = h.ResourceName,
                                              RoleRequired = h.RoleRequired,
                                              grade = h.Grade,
                                              Remarks = h.Remarks,
                                              OverDue = "",
                                              FLAG = ""
                                          }
                                       )
                                       .AsEnumerable().Select(x => new sp_GetHRRFListByEmpIdPractice_Result
                                       {
                                           HRRFID = x.HRRFID,
                                           HRRFNumber = x.HRRFNumber,
                                           HRRFSubmitedDate = x.HRRFSubmitedDate,
                                           JobDescription = x.JobDescription,
                                           CLSTRJD = x.CLSTRJD,
                                           LocationName = x.LocationName,
                                           Practice = x.Practice,
                                           Project = x.Project,
                                           SkillCode = x.SkillCode,
                                           Purpose = x.Purpose,
                                           RequestStatus = x.RequestStatus,
                                           RequestType = x.RequestType,
                                           ResourceName = x.ResourceName,
                                           RoleRequired = ((db.DesignationMasters.Where(y => y.DesignationCode.Equals(x.RoleRequired))).FirstOrDefault() == null) ? x.RoleRequired : (db.DesignationMasters.Where(y => y.DesignationCode.Equals(x.RoleRequired))).FirstOrDefault().DesignationName,
                                           Grade = x.grade,
                                           Remarks = x.Remarks,
                                           OverDue = "",
                                           FLAG = null
                                       }).ToList();
                    }
                    else if (HRRFNumber != "" && SkillCode != "")
                    {
                        objHRRFDetails = (from h in db.HRRFs
                                          where h.HRRFNumber.Contains(HRRFNumber) && h.SkillCode.Contains(SkillCode)
                                          select new
                                          {
                                              HRRFID = h.HRRFID,
                                              HRRFNumber = h.HRRFNumber,
                                              HRRFSubmitedDate = h.HRRFSubmitedDate,
                                              JobDescription = h.JobDescription,
                                              CLSTRJD = h.CLSTRJD,
                                              LocationName = h.LocationName,
                                              Practice = h.Practice,
                                              SkillCode = h.SkillCode,
                                              Project = (h.Purpose.ToLower().Contains("project") ? "For Project: " + h.ProjectName :
                                              (h.Purpose.ToLower().Contains("opportunity") ? "For Oppotunity: " + h.OpportunityName :
                                              (h.Purpose.ToLower().Contains("proactive hire") ? "Proactive Hire" : "Corporate Function"))),
                                              Purpose = h.Purpose,
                                              RequestStatus = h.RequestStatus,
                                              RequestType = h.RequestType,
                                              ResourceName = h.ResourceName,
                                              RoleRequired = h.RoleRequired,
                                              grade = h.Grade,
                                              Remarks = h.Remarks,
                                              OverDue = "",
                                              FLAG = ""
                                          }
                                       )
                                       .AsEnumerable().Select(x => new sp_GetHRRFListByEmpIdPractice_Result
                                       {
                                           HRRFID = x.HRRFID,
                                           HRRFNumber = x.HRRFNumber,
                                           HRRFSubmitedDate = x.HRRFSubmitedDate,
                                           JobDescription = x.JobDescription,
                                           CLSTRJD = x.CLSTRJD,
                                           LocationName = x.LocationName,
                                           Practice = x.Practice,
                                           Project = x.Project,
                                           SkillCode = x.SkillCode,
                                           Purpose = x.Purpose,
                                           RequestStatus = x.RequestStatus,
                                           RequestType = x.RequestType,
                                           ResourceName = x.ResourceName,
                                           RoleRequired = ((db.DesignationMasters.Where(y => y.DesignationCode.Equals(x.RoleRequired))).FirstOrDefault() == null) ? x.RoleRequired : (db.DesignationMasters.Where(y => y.DesignationCode.Equals(x.RoleRequired))).FirstOrDefault().DesignationName,
                                           Grade = x.grade,
                                           Remarks = x.Remarks,
                                           OverDue = "",
                                           FLAG = null
                                       }).ToList();
                    }




                    foreach (var hrrfvalue in objHRRFDetails)
                    {
                        bool Flag = false;
                        var externalexpecteddate = db.ExternalHires.Where(eh => eh.HRRFNumber == hrrfvalue.HRRFNumber).FirstOrDefault();
                        var hrrfbillingdate = db.HRRFs.Where(h => h.HRRFNumber == hrrfvalue.HRRFNumber).FirstOrDefault();
                        if (hrrfvalue.RequestType.ToLower().Contains("external"))
                        {
                            var externalhire_Tr = db.ExternalHires.Where(h => h.HRRFNumber == hrrfvalue.HRRFNumber).Count();
                            if (externalhire_Tr >= 1)
                            {
                                if (hrrfvalue.RequestStatus.ToLower() != "draft" && hrrfvalue.RequestStatus.ToLower() != "fulfilled"
                                     && hrrfvalue.RequestStatus.ToLower() != "terminated" && hrrfvalue.RequestStatus.ToLower() != "cancelled")
                                {
                                    if (externalexpecteddate.FulfilmentDate > hrrfbillingdate.BillingDate)
                                    {
                                        Flag = true;
                                    }
                                    else
                                    {
                                        Flag = false;
                                    }
                                }
                                else
                                {
                                    Flag = false;
                                }
                            }
                            else
                            {
                                if (hrrfvalue.RequestStatus.ToLower() != "draft" && hrrfvalue.RequestStatus.ToLower() != "fulfilled"
                                   && hrrfvalue.RequestStatus.ToLower() != "terminated" && hrrfvalue.RequestStatus.ToLower() != "cancelled")
                                {
                                    if (hrrfbillingdate.InternalExpectedFulfilmentDate > hrrfbillingdate.BillingDate)
                                    {
                                        Flag = true;
                                    }
                                    else
                                    {
                                        Flag = false;
                                    }
                                }
                                else
                                {
                                    Flag = false;
                                }
                            }
                        }
                        else
                        {
                            if (hrrfvalue.RequestStatus.ToLower() != "draft" && hrrfvalue.RequestStatus.ToLower() != "fulfilled"
                                 && hrrfvalue.RequestStatus.ToLower() != "terminated" && hrrfvalue.RequestStatus.ToLower() != "cancelled")
                            {
                                if (hrrfbillingdate.InternalExpectedFulfilmentDate > hrrfbillingdate.BillingDate)
                                {
                                    Flag = true;
                                }
                                else
                                {
                                    Flag = false;
                                }
                            }
                            else
                            {
                                Flag = false;
                            }
                        }
                        hrrfvalue.FLAG = Flag;

                        string Overdue = "";
                        if (hrrfvalue.RequestType.ToLower().Contains("external"))
                        {
                            var externalhire_Tr = db.ExternalHires.Where(h => h.HRRFNumber == hrrfvalue.HRRFNumber).Count();
                            if (externalhire_Tr >= 1)
                            {
                                DateTime dt1 = Convert.ToDateTime(externalexpecteddate.FulfilmentDate);
                                DateTime dt2 = Convert.ToDateTime(hrrfbillingdate.BillingDate);
                                if (hrrfvalue.RequestStatus.ToLower() != "draft" && hrrfvalue.RequestStatus.ToLower() != "fulfilled"
                                      && hrrfvalue.RequestStatus.ToLower() != "terminated" && hrrfvalue.RequestStatus.ToLower() != "cancelled")
                                {
                                    if (dt1 > dt2)
                                    {
                                        var days = dt1.Subtract(dt2).TotalDays;
                                        // Difference in days, hours, and minutes.
                                        TimeSpan ts = dt1 - dt2;
                                        // Difference in days.
                                        int differenceInDays = ts.Days;
                                        Overdue = differenceInDays.ToString() + " day(s)";
                                    }
                                    else
                                    {
                                        // Difference in days, hours, and minutes.
                                        TimeSpan ts = dt2 - dt1;
                                        // Difference in days.
                                        int differenceInDays = ts.Days;
                                        Overdue = differenceInDays.ToString() + " day(s)";
                                    }
                                }
                                else
                                {
                                    Overdue = "";
                                }
                            }
                            else
                            {
                                DateTime dt1 = Convert.ToDateTime(hrrfbillingdate.InternalExpectedFulfilmentDate);
                                DateTime dt2 = Convert.ToDateTime(hrrfbillingdate.BillingDate);
                                if (hrrfvalue.RequestStatus.ToLower() != "draft" && hrrfvalue.RequestStatus.ToLower() != "fulfilled"
                                      && hrrfvalue.RequestStatus.ToLower() != "terminated" && hrrfvalue.RequestStatus.ToLower() != "cancelled")
                                {
                                    if (dt1 > dt2)
                                    {
                                        // Difference in days, hours, and minutes.
                                        TimeSpan ts = dt1 - dt2;
                                        // Difference in days.
                                        int differenceInDays = ts.Days;
                                        Overdue = differenceInDays.ToString() + " day(s)";
                                    }
                                    else
                                    {
                                        // Difference in days, hours, and minutes.
                                        TimeSpan ts = dt2 - dt1;
                                        // Difference in days.
                                        int differenceInDays = ts.Days;
                                        Overdue = differenceInDays.ToString() + " day(s)";
                                    }
                                }
                                else
                                {
                                    Overdue = "";
                                }
                            }

                        }
                        else
                        {
                            DateTime dt1 = Convert.ToDateTime(hrrfbillingdate.InternalExpectedFulfilmentDate).Date;
                            DateTime dt2 = Convert.ToDateTime(hrrfbillingdate.BillingDate).Date;
                            if (hrrfvalue.RequestStatus.ToLower() != "draft" && hrrfvalue.RequestStatus.ToLower() != "fulfilled"
                                 && hrrfvalue.RequestStatus.ToLower() != "terminated" && hrrfvalue.RequestStatus.ToLower() != "cancelled")
                            {
                                if (dt1 > dt2)
                                {
                                    // Difference in days, hours, and minutes.
                                    TimeSpan ts = dt1 - dt2;
                                    // Difference in days.
                                    int differenceInDays = ts.Days;
                                    Overdue = differenceInDays.ToString() + " day(s)";
                                }
                                else
                                {
                                    // Difference in days, hours, and minutes.
                                    TimeSpan ts = dt2 - dt1;
                                    // Difference in days.
                                    int differenceInDays = ts.Days;
                                    Overdue = differenceInDays.ToString() + " day(s)";
                                }
                            }
                            else
                            {
                                Overdue = "";
                            }
                        }
                        hrrfvalue.OverDue = Overdue.ToString();
                    }


                    return PartialView("_HRRFList", objHRRFDetails);
                }
                else
                {
                    //ermsg = "Session expired";
                    return Json("Sessionexpired", JsonRequestBehavior.AllowGet);
                    //return RedirectToAction("SessionExpire", "Signout");
                }
            }
            catch (Exception ex)
            {

                Common.WriteExceptionErrorLog(ex);
                return Json("Error", JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Unavailable()
        {
            return View();
        }

	 //added for piechart
        public ActionResult GetSummaryPieChartInternal()
        {
            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            {
                try
                {                    
                    var Graph = db.Database.SqlQuery<GraphStatus>("exec [GetSummary] @RequestType", new SqlParameter("RequestType", "Internal")).ToList<GraphStatus>();
                    //var ExternalGraph = db.Database.SqlQuery<GraphStatus>("exec [GetSummary] @RequestType", new SqlParameter("RequestType", "External")).ToList<GraphStatus>();
                    List<GraphSummaryPieChartInternal> lstchartdata = new List<GraphSummaryPieChartInternal>();
                    //lstchartdata = Graph
                    //     .Select(n => new GraphSummaryPieChartInternal
                    //     {
                    //         ReqStatus = n.ReqStatus,
                    //         ReqStatusCount = n.ReqStatusCount
                    //     })
                    //     .ToList();                    
                    //lstchartdata.RemoveAt(lstchartdata.Count - 1);
                    return Json(Graph, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    Common.WriteExceptionErrorLog(ex);
                    return Json("Sessionexpired", JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json("Sessionexpired", JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetSummaryPieChartExternal()
        {
            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            {
                try
                {
                    
                    var Graph = db.Database.SqlQuery<GraphStatus>("exec [GetSummary] @RequestType", new SqlParameter("RequestType", "External")).ToList<GraphStatus>();
                   
                    return Json(Graph, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    Common.WriteExceptionErrorLog(ex);
                    return Json("Sessionexpired", JsonRequestBehavior.AllowGet);
                }


            }
            else
            {
                return Json("Sessionexpired", JsonRequestBehavior.AllowGet);
            }
        }
      
        /// <summary>
        /// Send an OpenID Connect sign-in request.
        /// Alternatively, you can just decorate the SignIn method with the [Authorize] attribute
        /// </summary>
        public void SignIn()
		{
            if (!Request.IsAuthenticated)
            {
                HttpContext.GetOwinContext().Authentication.Challenge(
                new AuthenticationProperties { RedirectUri = "/" },
                OpenIdConnectAuthenticationDefaults.AuthenticationType);
            }

		}

		///// <summary>
		///// Send an OpenID Connect sign-out request.
		///// </summary>
		//public void SignOut()
		//{
		//	HttpContext.GetOwinContext().Authentication.SignOut(
		//			OpenIdConnectAuthenticationDefaults.AuthenticationType,
		//			CookieAuthenticationDefaults.AuthenticationType);
		//}
	}
}