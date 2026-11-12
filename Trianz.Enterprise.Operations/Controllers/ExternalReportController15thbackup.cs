using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Trianz.Enterprise.Operations.Models;
using System.ComponentModel.DataAnnotations;
using OfficeOpenXml;
using System.Web.Helpers;
using Trianz.Enterprise.Operations.General;

namespace Trianz.Enterprise.Operations.Controllers
{
    public class ExternalReportController : Controller
    {
        TrianzOperationsEntities db = new TrianzOperationsEntities();
        string emp_Assignment = System.Configuration.ConfigurationManager.AppSettings["AssignmentStatus"].ToString();
        // GET: ExternalReport        
        public ActionResult Index(string SearchTR, string SearchExternalTR, string ddlPractise = "", string ddlHRRFRequestStatus = "", string ddlExternalPractise = "", string ddlExternalRequestStatus = "")
        {
            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            {
                try { 
                //Below single statement is added by Sarath, for security reason to access RoleMaster page.
                TempData["IsRoleMasterPageAccess"] = null;

            //Identifying prev page
            TempData["PrevPage"] = Request.Url.Segments[Request.Url.Segments.Count() - 1];

            var viewResult = new ValidationModel();


            List<string> ddlRequestStatus = new List<string>()
            {  "Awaiting Screening",
               "Cancelled",
               "Client Interview Pending",
               "Feedback Pending",
               "First Level Pending",
               "Fulfilled",
               "Hold",
               "HR Interview Pending",
               "Offer to Go",
               "Resume Pending",
               "Second Level Pending",
               "Yet to Join"
            };

            if (Session["Role"].ToString() == "RL")
            {
                ddlRequestStatus.Remove("Cancelled");
                ddlRequestStatus.Remove("Hold");
            }

           ViewData["_ddlRequestStatus"] = ddlRequestStatus;



            //List<string> Cancelreason = new List<string>()
            //{  "Dropped By Client",

            //   "Duplicate",
            //   "Lost Business",
            //   "Dropped by Delivery",
            //   "Retention",
            //   "Potential Opportunity",
            //   "Change in the client need",
            //};
            var Cancelreason = from cancel in db.TR_CancelReason orderby cancel.CancelReasonName select cancel.CancelReasonName;

            ViewData["_Cancelreason"] = Cancelreason;

            List<string> lstRLEmp = db.RoleMasters.Where(r => r.Role == "RL" && r.ApplicationCode == "TALENTREQ").Select(r => r.EmployeeId.ToString()).ToList();
            List<string> lstRLActiveEmp = db.Employees
                                            .Where(e => lstRLEmp.Contains(e.EmployeeId.ToString()) && e.IsActive == true)
                                            .Select(emp => emp.EmployeeId.ToString()).ToList();

            List<SelectListItem> lstRLEmployees = (from dt in db.Employees.
                              Where(n => lstRLActiveEmp.Contains(n.EmployeeId.ToString()) && n.IsActive == true)
                                                   select new SelectListItem
                                                   {
                                                       Text = dt.FirstName + " " + dt.LastName,
                                                       Value = dt.EmployeeId.ToString()
                                                   }
                             ).ToList();
          ViewData["_RecruiterName"] = lstRLEmployees.OrderBy(ord => ord.Text).ToList();

            var ServiceLine = (from data in db.PracticeWiseBenchCodes
                               select new
                               {
                                   LookupName = data.Practice,
                               }).Distinct().ToList().OrderBy(p => p.LookupName);
            ViewData["ServiceLine"] = ServiceLine;


            var exdetails = (from eh in db.ExternalHires.DefaultIfEmpty()
                             join eff in db.ExternalFulfillments on eh.HRRFNumber equals eff.HRRFNumber
                             join emp in db.Employees on eh.RecruiterName equals emp.EmployeeId.ToString() into tempExterHire
                             from emp in tempExterHire.DefaultIfEmpty()
                             where eff.Practice != null && eh.RequestStatus != null
                             select new ExtFulfillment
                             {
                                 ExternalHireId = eh.ExternalHireId,
                                 RecruiterId = eh.RecruiterName,
                                 RecruiterName = emp.FirstName + " " + emp.LastName, //"", //db.Employees.Where(f => f.EmployeeId == eh.RecruiterName).Select(emp => emp.FirstName).ToString(),
                                 Name = eh.Name,
                                 EmployeeId = eh.EmployeeId,
                                 RequestStatus = eh.RequestStatus,
                                 FulfilmentRemarks = eh.FulfilmentRemarks,
                                 DOJ = eh.DOJ,
                                 FulfilmentDate = eh.FulfilmentDate,
                                 CancellationReason = eh.CancellationReason != "" ? (eh.CancellationReason == "Duplicate" ? eh.CancellationReason + " (" + eh.DuplicateHRRFNo + ")" : eh.CancellationReason) : "",
                                 CancelDate = eh.CancelDate,
                                 HRRFNumber = eh.HRRFNumber,
                                 HRRFId = eh.HRRFId,
                                 ModifiedBy = eff.ModifiedBy,
                                 ModifiedDate = eff.ModifiedDate,
                                 Practice = eff.Practice
                             }
                           ).OrderByDescending(ef => ef.ModifiedDate).OrderByDescending(ef => ef.ExternalHireId).ToList();

            ViewData["ExtDetailsoutsearch"] = exdetails;

            if (SearchExternalTR != null && SearchExternalTR.Trim() != "")
            {
                exdetails = exdetails.Where(s => s.HRRFNumber.ToLower().Contains(SearchExternalTR.ToLower().Trim())).ToList();
            }

            if (ddlExternalPractise != null && ddlExternalPractise != "")
            {
                exdetails = exdetails.Where(p => p.Practice.ToLower() == ddlExternalPractise.ToLower()).ToList();
            }

            if (ddlExternalRequestStatus != null && ddlExternalRequestStatus != "")
            {
                exdetails = exdetails.Where(rs => rs.RequestStatus.ToLower() == ddlExternalRequestStatus.ToLower()).ToList();

            }

            #region Code for Top Grid
            List<HRRF> ht = new List<HRRF>();

            var hrrfdetails = (from h in
                                   db.HRRFs
                               where h.RequestType.ToLower() == "external" &&
                               h.RequestStatus.ToLower() != "fulfilled" && h.RequestStatus != null
                               //select h
                               select new hrrfdetails
                               {
                                   HRRFID = h.HRRFID,
                                   HRRFNumber = h.HRRFNumber,
                                   Purpose = h.Purpose,
                                   ProjectCode = h.ProjectCode,
                                   ProjectName = h.Purpose.ToLower() == "opportunity" ? h.Purpose + ":" + h.OpportunityName : h.Purpose + ":" + h.ProjectName,
                                   OpportunityName = h.OpportunityName,
                                   OpportunityCode = h.OpportunityCode,
                                   DemandType = h.DemandType,
                                   Grade = h.Grade,
                                   RoleRequired = h.RoleRequired,
                                   ExpFrom = h.ExpFrom,
                                   ExpTo = h.ExpTo,
                                   EnagagementType = h.EnagagementType,
                                   ResourceName = h.ResourceName,
                                   Domain = h.Domain,
                                   JobDescription = h.JobDescription,
                                   AssignmentStartDate = h.AssignmentStartDate,
                                   AssignmentEndDate = h.AssignmentEndDate,
                                   LocationType = h.LocationType,
                                   LocationName = h.LocationName,
                                   RequestReason = h.RequestReason,
                                   Remarks = h.Remarks,
                                   RequestType = h.RequestType,
                                   RequestStatus = h.RequestStatus,
                                   ClientInterview = h.ClientInterview,
                                   CallCompleted = h.CallCompleted,
                                   IsActive = h.IsActive,
                                   HRRFCreatedBy = h.HRRFCreatedBy,
                                   HRRFCreatedDate = h.HRRFCreatedDate,
                                   HRRFSubmitedDate = h.HRRFSubmitedDate,
                                   ModifiedBy = h.ModifiedBy,
                                   ModifiedDate = h.ModifiedDate,
                                   Positions = h.Positions,
                                   Practice = h.Practice,
                                   TRParent = h.TRParent,
                                   Isparent = h.Isparent,
                                   VisaType = h.VisaType,
                                   AccountName = h.AccountName,
                                   OldHRRFNumber = h.OldHRRFNumber,
                                   CostCenter = h.CostCenter,
                                   ReplacementEmpId = h.ReplacementEmpId,
                                   ResourceType = h.ResourceType,
                                   IsContracting = h.IsContracting,
                                   Impact = h.Impact,
                                   Criticality = h.Criticality,
                                   Utilization = h.Utilization,
                                   BillingDate = h.BillingDate,
                                   InternalExpectedFulfilmentDate = h.InternalExpectedFulfilmentDate,
                                   CancelReason = h.CancelReason,
                                   DuplicateHRRFNo = h.DuplicateHRRFNo,
                                   TECHPANEL = h.TECHPANEL,
                                   SECONDTECHPANEL = h.SECONDTECHPANEL,
                                   chkflag = false
                               }
                               ).ToList();

            hrrfdetails = (from h in db.HRRFs
                           join eff in db.ExternalFulfillments on h.HRRFNumber equals eff.HRRFNumber
                           where h.RequestType.ToLower() == "external" && h.RequestStatus.ToLower() != "fulfilled"
                           && h.RequestStatus != null && eff.ApprovalStatus.ToLower() == "accepted"
                           //select h
                           select new hrrfdetails
                           {
                               HRRFID = h.HRRFID,
                               HRRFNumber = h.HRRFNumber,
                               Purpose = h.Purpose,
                               ProjectCode = h.ProjectCode,
                               ProjectName = h.Purpose.ToLower() == "opportunity" ? h.Purpose + ":" + h.OpportunityName : h.Purpose + ":" + h.ProjectName,
                               OpportunityName = h.OpportunityName,
                               OpportunityCode = h.OpportunityCode,
                               DemandType = h.DemandType,
                               Grade = h.Grade,
                               RoleRequired = h.RoleRequired,
                               ExpFrom = h.ExpFrom,
                               ExpTo = h.ExpTo,
                               EnagagementType = h.EnagagementType,
                               ResourceName = h.ResourceName,
                               Domain = h.Domain,
                               JobDescription = h.JobDescription,
                               AssignmentStartDate = h.AssignmentStartDate,
                               AssignmentEndDate = h.AssignmentEndDate,
                               LocationType = h.LocationType,
                               LocationName = h.LocationName,
                               RequestReason = h.RequestReason,
                               Remarks = h.Remarks,
                               RequestType = h.RequestType,
                               RequestStatus = h.RequestStatus,
                               ClientInterview = h.ClientInterview,
                               CallCompleted = h.CallCompleted,
                               IsActive = h.IsActive,
                               HRRFCreatedBy = h.HRRFCreatedBy,
                               HRRFCreatedDate = h.HRRFCreatedDate,
                               HRRFSubmitedDate = h.HRRFSubmitedDate,
                               ModifiedBy = h.ModifiedBy,
                               ModifiedDate = h.ModifiedDate,
                               Positions = h.Positions,
                               Practice = h.Practice,
                               TRParent = h.TRParent,
                               Isparent = h.Isparent,
                               VisaType = h.VisaType,
                               AccountName = h.AccountName,
                               OldHRRFNumber = h.OldHRRFNumber,
                               CostCenter = h.CostCenter,
                               ReplacementEmpId = h.ReplacementEmpId,
                               ResourceType = h.ResourceType,
                               IsContracting = h.IsContracting,
                               Impact = h.Impact,
                               Criticality = h.Criticality,
                               Utilization = h.Utilization,
                               BillingDate = h.BillingDate,
                               InternalExpectedFulfilmentDate = h.InternalExpectedFulfilmentDate,
                               CancelReason = h.CancelReason,
                               DuplicateHRRFNo = h.DuplicateHRRFNo,
                               TECHPANEL = h.TECHPANEL,
                               SECONDTECHPANEL = h.SECONDTECHPANEL,
                               //chkflag = (from et in db.ExternalHires
                               //           where et.HRRFNumber==h.HRRFNumber select et).FirstOrDefault()!=null?true:false
                               chkflag= false
                           }
            ).ToList();
            if (hrrfdetails != null)
            {
                //foreach (var TRParents in hrrfdetails)
                //{
                //    if (TRParents.Purpose.ToLower() == "opportunity")
                //    {
                //        TRParents.ProjectName = "Opportunity: " + TRParents.OpportunityName;
                //    }
                //    else if (TRParents.Purpose.ToLower() == "project")
                //    {
                //        TRParents.ProjectName = "Project: " + TRParents.ProjectName;
                //    }
                //    else if (TRParents.Purpose.ToLower() == "proactive hire")
                //    {
                //        TRParents.ProjectName = "Proactive Hire";
                //    }
                //    else if (TRParents.Purpose.ToLower() == "corporate function")
                //    {
                //        TRParents.ProjectName = "Corporate Function";
                //    }
                //}
            }

            if (SearchTR != null && SearchTR.Trim() != "")
            {
                hrrfdetails = hrrfdetails.Where(s => s.HRRFNumber.ToLower().Contains(SearchTR.ToLower().Trim())).ToList();
            }

            if (ddlPractise != null && ddlPractise != "")
            {
                hrrfdetails = hrrfdetails.Where(p => p.Practice.ToLower() == ddlPractise.ToLower()).ToList();
            }

            if (ddlHRRFRequestStatus != null && ddlHRRFRequestStatus != "")
            {

                hrrfdetails = hrrfdetails.Where(rs => rs.RequestStatus.ToLower() == ddlHRRFRequestStatus.ToLower()).ToList();
            }

            #endregion

           ViewData["ExtDetails"] = exdetails;
            ViewData["HrrDetails"] = hrrfdetails;

            var exdetailsresult = ViewData["ExtDetails"] as List<ExtFulfillment>;
            var exdetailsoutresult = ViewData["ExtDetailsoutsearch"] as List<ExtFulfillment>;
            var hrrfdetailsresult = ViewData["HrrDetails"] as List<hrrfdetails>;

            viewResult = new ValidationModel()
            {
                HRRFDet = hrrfdetailsresult.ToList(),
                //ExternalHireDetails = exdetailsresult
                ExternalFulfullmentHireDetails = exdetailsresult,
                ExternalhiredetailsRemarks = exdetailsoutresult

            };
            return View(viewResult);
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
                //return Json("Sessionexpired", JsonRequestBehavior.AllowGet);
                return RedirectToAction("SessionExpire", "Signout");
            }
        }
        private int? GetEmployeeID()
        {
			//string UserName = User.Identity.Name.Split('\\')[1].ToLower();
			string usermail = Common.GetAzureLoggedInUserID();
			if (usermail != null)
            {
                string emailId = (from data in db.Employees where (data.Email.ToLower().Equals(usermail) && data.IsActive == true) select data.Email).FirstOrDefault();
                Employee employee = (from data in db.Employees.Where(x => x.Email == emailId) select data).FirstOrDefault();

                if (employee != null)
                {
                    return employee.EmployeeId;
                }
            }

            return null;
        }

        [HttpGet]
        public ActionResult Save(int HrrfId, string HrrfNumber)
        {
            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            {
                try { 
                if (HrrfNumber != null)
            {
                Session["hrrfno"] = HrrfNumber.Trim().ToString();
                Session["hrrfId"] = HrrfId;
            }
            List<string> ddlRequestStatus = new List<string>() { "Yet to Join", "Resume Pending", "Hold", "Offer to Go", "Awaiting Screening", "Fulfilled", "Cancelled", "HR Interview Pending", "Client Interview Pending", "Feedback Pending", "First Level Pending", "Second Level Pending" };
            if (Session["Role"].ToString() == "RL")
            {
                ddlRequestStatus.Remove("Cancelled");
                ddlRequestStatus.Remove("Hold");
            }

            ViewData["_ddlRequestStatus"] = ddlRequestStatus.OrderBy(ord => ord).ToList();

            var Cancelreason = from cancel in db.TR_CancelReason orderby cancel.CancelReasonName select cancel.CancelReasonName;
            ViewData["_Cancelreason"] = Cancelreason;

            string strLocationType = (db.HRRFs.Where(h => h.HRRFNumber == HrrfNumber).FirstOrDefault().LocationType == "1") ? "ONSITE" : "OFFSHORE";

            List<string> lstRLEmp = db.RoleMasters.Where(r => r.Role == "RL" && r.ApplicationCode == "TALENTREQ").Select(r => r.EmployeeId.ToString()).ToList();

            List<string> lstRLActiveEmp = db.Employees
                                            .Where(e => lstRLEmp.Contains(e.EmployeeId.ToString())
                                                    && e.IsActive == true
                                                    && e.LocationType == strLocationType)
                                            .Select(emp => emp.EmployeeId.ToString()).ToList();

            if (strLocationType == "ONSITE")
            {
                HRRF objHRRF = db.HRRFs.Find(HrrfId);
                if (objHRRF != null)
                {
                    if (objHRRF.LocationName.ToUpper() == System.Configuration.ConfigurationManager.AppSettings["OnSiteCountryName"]) 
                    {
                        string onsiteEmpIds = (System.Configuration.ConfigurationManager.AppSettings["OnsiteEmployeeId"]);
                        List<string> lstEmpIds = new List<string>();
                        lstEmpIds = onsiteEmpIds.Split(',').ToList();
                        foreach(string empId in lstEmpIds)
                        {
                            lstRLActiveEmp.Add(empId); 
                        }
                        //lstRLActiveEmp.Add((System.Configuration.ConfigurationManager.AppSettings["OnsiteEmployeeId"])); // Parvez Pasha EmpID
                    }
                }
            }

            List<SelectListItem> lstRLEmployees = (from dt in db.Employees.
                              Where(n => lstRLActiveEmp.Contains(n.EmployeeId.ToString()) && n.IsActive == true)
                                                   select new SelectListItem
                                                   {
                                                       Text = dt.FirstName + " " + dt.LastName,
                                                       Value = dt.EmployeeId.ToString()
                                                   }
                             ).ToList();
            ViewData["_RecruiterName"] = lstRLEmployees.OrderBy(ord => ord.Text).ToList();

            return View();
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
                //return Json("Sessionexpired", JsonRequestBehavior.AllowGet);
                return RedirectToAction("SessionExpire", "Signout");
            }
        }
        [HttpPost]
        public ActionResult Save([Bind(Include = "ExternalHireId,RecruiterName,Name,EmployeeId,RequestStatus,FulfilmentRemarks,DOJ,FulfilmentDate,CancellationReason,HRRFNumber,HRRFId,DuplicateHRRFNo")] ExternalHire externalhire)
        {
            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            {
            try
            {
                //string HRRFNumber = Session["hrrfno"].ToString();
                //HRRF objHRRFs = db.HRRFs.Where(h => h.HRRFNumber.Equals(HRRFNumber)).FirstOrDefault();
                //string PrevRequestStatus = objHRRFs.RequestStatus;
                
                externalhire.Name = Request.Form["txtResourceName"];

                if (!string.IsNullOrWhiteSpace(Request.Form["txtEmpId"]))
                {
                    externalhire.EmployeeId = Convert.ToInt32(Request.Form["txtEmpId"]);
                }
                externalhire.RequestStatus = Request.Form["ddlRequestStatus"];
                externalhire.FulfilmentRemarks = Request.Form["txtFulfilmentRemarks"];

                IFormatProvider theCultureInfo = new System.Globalization.CultureInfo("en-us", true);

                if (!string.IsNullOrWhiteSpace(Request.Form["DOJ"]))
                {
                    DateTime? DOJ = DateTime.Parse(Request.Form["DOJ"].ToString(), theCultureInfo);
                    externalhire.DOJ = DOJ;
                }
                else
                {
                    externalhire.DOJ = null;
                }
                //externalhire.DOJ = string.IsNullOrEmpty(Request.Form["DOJ"]) ? new DateTime?() : DateTime.ParseExact(Request.Form["DOJ"], "MM/dd/yyyy", null);//DateTime.Parse(Request.Form["DOJ"]); //string.IsNullOrEmpty(Request.Form["DOJ"].ToString()) ? (DateTime?)null : DateTime.Parse(Request.Form["DOJ"]);
                //externalhire.FulfilmentDate = DateTime.ParseExact(Request.Form["FulfilmentDate"], "MM/dd/yyyy", CultureInfo.InvariantCulture);// string.IsNullOrEmpty(Request.Form["FulfilmentDate"].ToString()) ? (DateTime?)null : DateTime.Parse(Request.Form["FulfilmentDate"]);
                externalhire.FulfilmentDate = string.IsNullOrEmpty(Request.Form["FulfilmentDate"]) ? new DateTime?() : DateTime.ParseExact(Request.Form["FulfilmentDate"], "MM/dd/yyyy", null);
                externalhire.CancellationReason = Request.Form["txtCancelRemarks"];
                externalhire.CancelDate = string.IsNullOrEmpty(Request.Form["DOC"]) ? new DateTime?() : DateTime.ParseExact(Request.Form["DOC"], "MM/dd/yyyy", null);
                externalhire.HRRFId = Convert.ToInt32(Session["hrrfId"]);
                externalhire.HRRFNumber = Session["hrrfno"].ToString();
                externalhire.RecruiterName = (Request.Form["RecruiterName"] == null) ? "" : Request.Form["RecruiterName"].ToString();
                externalhire.DuplicateHRRFNo = Request.Form["txtDuplicateHrrfNumber"].ToString();
                db.ExternalHires.Add(externalhire);

                bool dojchk = false;
                string errormsg = "";
                HRRF objHRRF = db.HRRFs.Where(h => h.HRRFNumber.Equals(externalhire.HRRFNumber)).FirstOrDefault();
                if (externalhire.RequestStatus == "Fulfilled")
                {
                    var objEmployee = db.Employees.Where(b => b.EmployeeId == externalhire.EmployeeId).ToList().FirstOrDefault();

                    if (objEmployee != null)
                    {

                            DateTime? DOJ = DateTime.Parse(objEmployee.DateOfJoin.ToString(), theCultureInfo);
                            //DateTime? DOJ = Convert.ToDateTime(objEmployee.DateOfJoin.ToString(), theCultureInfo);

                        if (Convert.ToDateTime(DOJ).Date <= Convert.ToDateTime(objHRRF.BillingDate).Date)
                        {
                            dojchk = true;
                        }
                        if (dojchk)
                        {

                            var objpras1 = db.ProjectAssignments.Where(b => b.EmployeeId == objEmployee.EmployeeId && !(b.BillingStatus.Contains("Bench")) &&
                                 ((b.StartDate >= objHRRF.BillingDate && b.StartDate <= objHRRF.AssignmentEndDate) || (b.EndDate >= objHRRF.BillingDate && b.EndDate <= objHRRF.AssignmentEndDate)
                                 || (b.StartDate <= objHRRF.BillingDate && b.EndDate >= objHRRF.AssignmentEndDate))).ToList();

                            if (objpras1.Count > 0)
                            {
                                var sumUtilization = objpras1.Sum(p => p.Utilization);
                                if ((sumUtilization + Convert.ToInt32(objHRRF.Utilization)) > 100)
                                {
                                    dojchk = false;
                                    errormsg = objEmployee.FirstName + " already assigned to some other project with selected dates";
                                }
                            }
                        }
                        else
                        {
                            errormsg = "Employee date of joining should be less than billing date of the TR.";
                        }
                    }
                    else
                    {
                        errormsg = "Employee details not exists";
                        dojchk = false;
                    }
                }
                else
                {
                    dojchk = true;
                }
                if (dojchk)
                {

                    int? Utilzation = null;
                    //Below code is added by Sarath on 05 Mar 2016 
                    //Reason: Update ModifiedBy and ModifiedDate columns in the ExternalFulFillment table.
                    if (Session["EmployeeId"] != null)
                    {
                        ExternalFulfillment externalFulFillmentHire = db.ExternalFulfillments.Where(eff => eff.HRRFNumber.Equals(externalhire.HRRFNumber)).FirstOrDefault<ExternalFulfillment>();

                        if (externalFulFillmentHire != null)
                        {
                            externalFulFillmentHire.ModifiedBy = Convert.ToInt32(Session["EmployeeId"]);
                            externalFulFillmentHire.ModifiedDate = DateTime.UtcNow;

                            db.Entry(externalFulFillmentHire).State = System.Data.Entity.EntityState.Modified;
                        }
                    }
                    string strLocationType = "";

                    string prevstatus = objHRRF.RequestStatus;

                    //int propsedEmployeeid;
                    if (objHRRF != null)
                    {
                        #region ProjectAssignment
                        strLocationType = (objHRRF.LocationType == "1") ? "ONSITE" : "OFFSHORE";
                        objHRRF.RequestStatus = externalhire.RequestStatus;
                            if (externalhire.CancelDate != null)
                            {
                                objHRRF.ResourceName = externalhire.Name;
                                objHRRF.CancellationDate = externalhire.CancelDate;
                            }

                            if (externalhire.RequestStatus == "Fulfilled")
                            {
                            objHRRF.ResourceName = externalhire.Name;
                                //some test data 
                            objHRRF.FulfillmentDate = externalhire.FulfilmentDate;
                                //somesample test data
                                var objEmployeeAssignment = db.Employees.Where(a => a.EmployeeId == externalhire.EmployeeId).FirstOrDefault();

                            DateTime? DOJC = DateTime.Parse(objEmployeeAssignment.DateOfJoin.ToString(), theCultureInfo);

                            DateTime dtStartDate = Convert.ToDateTime(objHRRF.BillingDate);
                            DateTime dtEndDate = Convert.ToDateTime(objHRRF.AssignmentEndDate);

                            var objpras1 = db.ProjectAssignments.Where(b => b.EmployeeId == externalhire.EmployeeId && !(b.BillingStatus.Contains("Bench")) && ((b.StartDate >= dtStartDate && b.StartDate <= dtEndDate) || (b.EndDate >= dtStartDate && b.EndDate <= dtEndDate) || (b.StartDate <= dtStartDate && b.EndDate >= dtEndDate))).ToList();
                            if (objpras1.Count() > 0)
                            {
                                var sumUtilization = objpras1.Sum(p => p.Utilization);
                                Utilzation = 100 - sumUtilization;
                            }

                            //to checkprevious Bench 
                            #region Existing Bench Record
                            var previousprojectass = db.ProjectAssignments.Where(p => p.IsActive == true && p.EmployeeId == externalhire.EmployeeId && p.BillingStatus == "Bench").ToList();
                            var dtProjectAssignSatrtAsEndDate = db.ProjectAssignments.Where(pa => pa.EmployeeId == externalhire.EmployeeId
                                            && DateTime.UtcNow <= pa.StartDate).OrderBy(pa => pa.StartDate).Select(pa => pa.StartDate).FirstOrDefault();
                            var dtProjectAssignEndDate = db.ProjectAssignments.Where(pa => pa.EmployeeId == externalhire.EmployeeId && pa.IsActive == true && pa.BillingStatus != "Bench"
                                   && DateTime.UtcNow <= pa.EndDate).OrderBy(pa => pa.EndDate).Select(pa => pa.EndDate).FirstOrDefault();
                            if (previousprojectass.Count != 0)
                            {
                                foreach (var lt in previousprojectass)
                                {
                                    #region Bench Assignment History
                                    ProjectAssignmenthistory pash = new ProjectAssignmenthistory();
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
                                    pash.modifiedBy = Convert.ToInt32(Session["EmployeeId"]);
                                    pash.ModifiedDate = DateTime.UtcNow;

                                    #region Exisitng Bench Record Update based on selected Dates
                                    // Selecting top 1 Start Date order by desc to Set Bench Enda date
                                    // billdatge == getdate
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
                                        ul = lt.Utilization;
                                    if (dtProjectAssignSatrtAsEndDate != null && Convert.ToDateTime(dtProjectAssignSatrtAsEndDate).Date <= Convert.ToDateTime(objHRRF.BillingDate).Date)
                                    {
                                        lt.EndDate = Convert.ToDateTime(dtProjectAssignSatrtAsEndDate);
                                    }
                                    else if (dtProjectAssignEndDate != null && dtProjectAssignEndDate <= Convert.ToDateTime(objHRRF.AssignmentEndDate) && DateTime.UtcNow.Date.Equals(Convert.ToDateTime(objHRRF.BillingDate).Date))
                                    {
                                        if (Convert.ToDateTime(objHRRF.BillingDate) < dtProjectAssignEndDate)
                                            lt.EndDate = Convert.ToDateTime(objHRRF.BillingDate);
                                        else
                                            lt.EndDate = Convert.ToDateTime(dtProjectAssignEndDate);
                                    }
                                    else if (DateTime.UtcNow.Date >= (Convert.ToDateTime(objHRRF.BillingDate).Date))
                                    {
                                        if (ul > 0)
                                            lt.EndDate = Convert.ToDateTime(objHRRF.AssignmentEndDate);
                                        else
                                        {
                                            if (Convert.ToDateTime(objHRRF.BillingDate) >= Convert.ToDateTime(lt.StartDate))
                                                lt.EndDate = Convert.ToDateTime(objHRRF.BillingDate);
                                            else
                                                lt.EndDate = lt.StartDate;

                                        }
                                    }
                                    else
                                    {
                                        lt.EndDate = Convert.ToDateTime(objHRRF.BillingDate);
                                    }

                                    //else if (DateTime.UtcNow.Date < Convert.ToDateTime(objHRRF.BillingDate).Date)
                                    //{
                                    //    lt.StartDate = DateTime.UtcNow;
                                    //    lt.Utilization = Utilzation ?? 100;
                                    //}
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
                                    db.ProjectAssignmenthistories.Add(pash);
                                    #endregion

                                    db.Entry(lt).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                }
                            }
                            else
                            {
                                int benchUtilization = 0;
                                if (DateTime.UtcNow.Date >= Convert.ToDateTime(objHRRF.BillingDate).Date)
                                {
                                    if (Utilzation == null)
                                    {
                                        benchUtilization = 100 - Convert.ToInt32(objHRRF.Utilization);
                                    }
                                    else
                                    {
                                        benchUtilization = Convert.ToInt32(Utilzation - Convert.ToInt32(objHRRF.Utilization));
                                    }

                                    //projAssignment.Utilization = Utilzation;

                                }
                                else if (DateTime.UtcNow.Date < Convert.ToDateTime(objHRRF.BillingDate).Date)
                                {
                                    benchUtilization = 100;
                                }
                                if (benchUtilization > 0)
                                {
                                    #region First Bench Record if Bench record not exist for the Employee
                                    DateTime stdat = new DateTime();
                                    DateTime etdat = new DateTime();

                                    if (DateTime.UtcNow.Date >= Convert.ToDateTime(objHRRF.BillingDate).Date)
                                    {
                                        stdat = Convert.ToDateTime(objHRRF.BillingDate);
                                    }
                                    else
                                    {
                                        stdat = DateTime.UtcNow.Date;
                                    }
                                    if (dtProjectAssignSatrtAsEndDate != null && dtProjectAssignSatrtAsEndDate <= Convert.ToDateTime(objHRRF.BillingDate) && DateTime.UtcNow.Date.Equals(Convert.ToDateTime(objHRRF.BillingDate).Date))
                                    {
                                        etdat = Convert.ToDateTime(dtProjectAssignSatrtAsEndDate);
                                    }
                                    else if (dtProjectAssignEndDate != null && dtProjectAssignEndDate <= Convert.ToDateTime(objHRRF.AssignmentEndDate) && DateTime.UtcNow.Date.Equals(Convert.ToDateTime(objHRRF.BillingDate).Date))
                                    {
                                        if (Convert.ToDateTime(objHRRF.BillingDate) < dtProjectAssignEndDate)
                                            etdat = Convert.ToDateTime(objHRRF.BillingDate);
                                        else
                                            etdat = Convert.ToDateTime(dtProjectAssignEndDate);
                                    }
                                    else if (DateTime.UtcNow.Date >= (Convert.ToDateTime(objHRRF.BillingDate).Date))
                                    {
                                        etdat = Convert.ToDateTime(objHRRF.AssignmentEndDate);
                                    }
                                    else
                                    {
                                        etdat = Convert.ToDateTime(objHRRF.BillingDate);
                                    }


                                    bool dojbenck = true;

                                    var objpras11 = db.ProjectAssignments.Where(b => b.EmployeeId == externalhire.EmployeeId
                                    && ((b.StartDate >= stdat
                                    && b.StartDate <= etdat) || (b.EndDate >= stdat && b.EndDate <= etdat) ||
                                    (b.StartDate <= stdat && b.EndDate >= etdat))).ToList();

                                    if (objpras11.Count > 0)
                                    {
                                        var sumUtilization = objpras11.Sum(p => p.Utilization);
                                        if ((sumUtilization + Convert.ToInt32(objHRRF.Utilization)) > 100)
                                            dojbenck = false;

                                    }
                                        if (dojbenck)
                                        {
                                            var unass = (from bpr in db.PracticeWiseBenchCodes
                                                         join pt in db.Projects on bpr.BenchCode equals pt.ProjectCode
                                                         join emp in db.Employees on pt.ProjectManagerId equals emp.EmployeeId
                                                         where bpr.CostCenter.ToLower().Contains(objEmployeeAssignment.CostCenter.ToLower())
                                                         select new
                                                         {
                                                             Projectcode = pt.ProjectCode,
                                                             ProjectName = pt.ProjectName,
                                                             projectid = pt.ProjectId,
                                                             projmanger = emp.LastName + "," + emp.FirstName + " " + emp.MiddleName,
                                                             Billingstatus = bpr.BillingStatus,
                                                             Practice = bpr.Practice
                                                         }).Distinct().ToList();

                                            if (objEmployeeAssignment.CostCenter != null)
                                            {
                                           if (objEmployeeAssignment.CostCenter.ToLower() != "testing")
                                            {

                                                unass = unass.Where(p => p.Practice.ToLower() == objEmployeeAssignment.Practice.ToLower()).ToList();
                                            }
                                            }
                                            if (unass.Count > 0)
                                            {
                                            ProjectAssignment projAssignment = new ProjectAssignment();
                                            projAssignment.ProjectCode = unass.FirstOrDefault().Projectcode;
                                            projAssignment.ProjectID = unass.FirstOrDefault().projectid;
                                            projAssignment.ProjectName = unass.FirstOrDefault().ProjectName;

                                            projAssignment.StartDate = stdat;
                                            projAssignment.EndDate = etdat;
                                            // billdatge == getdate

                                            projAssignment.Utilization = benchUtilization;
                                            projAssignment.EmployeeId = externalhire.EmployeeId;
                                            // need to check future date and active and inactive
                                            projAssignment.IsActive = true;
                                            projAssignment.Assigned_By = unass.FirstOrDefault().projmanger;
                                            projAssignment.Assigned_Date = System.DateTime.UtcNow;
                                            projAssignment.BillingStatus = unass.FirstOrDefault().Billingstatus;
                                            if (projAssignment.BillingStatus == "Bench")
                                            {
                                                projAssignment.Category = "Deployable Bench";
                                                projAssignment.Bechstatus = "Free Pool";
                                            }
                                            db.ProjectAssignments.Add(projAssignment);
                                            #endregion

                                            #region Selected Project Assignment History
                                            ProjectAssignmenthistory projBenchHistory = new ProjectAssignmenthistory();
                                            projBenchHistory.AssignmentId = 0;
                                            projBenchHistory.ProjectCode = projAssignment.ProjectCode;
                                            projBenchHistory.ProjectName = projAssignment.ProjectName;
                                            projBenchHistory.ProjectID = projAssignment.ProjectID;
                                            projBenchHistory.Assigned_ByOld = null;
                                            projBenchHistory.BillingStatusOld = null;
                                            projBenchHistory.EmployeeId = externalhire.EmployeeId;
                                            projBenchHistory.EnddateOld = null;
                                            projBenchHistory.IsActiveOld = null;
                                            projBenchHistory.StartDateOld = null;
                                            projBenchHistory.UtilizationOld = null;
                                            projBenchHistory.modifiedBy = Convert.ToInt32(Session["EmployeeId"]);
                                            projBenchHistory.ModifiedDate = DateTime.UtcNow;
                                            projBenchHistory.UtilizationNew = projAssignment.Utilization;
                                            projBenchHistory.StartDateNew = projAssignment.StartDate;
                                            projBenchHistory.IsActiveNew = projAssignment.IsActive;
                                            projBenchHistory.EndDateNew = projAssignment.EndDate;
                                            projBenchHistory.BillingStatusNew = projAssignment.BillingStatus;
                                            projBenchHistory.Assigned_byNew = projAssignment.Assigned_By;
                                            db.ProjectAssignmenthistories.Add(projBenchHistory);
                                            #endregion
                                        }
                                        //for testing
                                    }
                                }
                                else
                                {

                                    if ((Convert.ToDateTime(objHRRF.BillingDate).Date - Convert.ToDateTime(DOJC).Date).TotalDays > 0)
                                    {
                                        bool dojbenck = true;
                                        var objpras11 = db.ProjectAssignments.Where(b => b.EmployeeId == externalhire.EmployeeId
                                        && ((b.StartDate >= DOJC && b.StartDate <= (objHRRF.BillingDate)
                                        || (b.EndDate >= DOJC && b.EndDate <= objHRRF.BillingDate)
                               || (b.StartDate <= DOJC && b.EndDate >= objHRRF.BillingDate)))).ToList();

                                        if (objpras11.Count > 0)
                                        {
                                            var sumUtilization = objpras11.Sum(p => p.Utilization);
                                            if ((sumUtilization + Convert.ToInt32(objHRRF.Utilization)) > 100)
                                                dojbenck = false;

                                        }
                                            if (dojbenck)
                                            {
                                                #region First Bench Record we get diff b/w doj to billing date 
                                                var unass = (from bpr in db.PracticeWiseBenchCodes
                                                             join pt in db.Projects on bpr.BenchCode equals pt.ProjectCode
                                                             join emp in db.Employees on pt.ProjectManagerId equals emp.EmployeeId
                                                             where bpr.CostCenter.ToLower().Contains(objEmployeeAssignment.CostCenter.ToLower())
                                                             select new
                                                             {
                                                                 Projectcode = pt.ProjectCode,
                                                                 ProjectName = pt.ProjectName,
                                                                 projectid = pt.ProjectId,
                                                                 projmanger = emp.LastName + "," + emp.FirstName + " " + emp.MiddleName,
                                                                 Billingstatus = bpr.BillingStatus,
                                                                 Practice = bpr.Practice
                                                             }).Distinct().ToList();


                                                if (objEmployeeAssignment.CostCenter != null)
                                                {
                                                    if (objEmployeeAssignment.CostCenter.ToLower() != "testing")
                                                    {
                                                        unass = unass.Where(p => p.Practice.ToLower() == objEmployeeAssignment.Practice.ToLower()).ToList();
                                                    }
                                                }
                                                if (unass.Count > 0)
                                                {
                                                    ProjectAssignment projAssignment = new ProjectAssignment();
                                                    projAssignment.ProjectCode = unass.FirstOrDefault().Projectcode;
                                                    projAssignment.ProjectID = unass.FirstOrDefault().projectid;
                                                    projAssignment.ProjectName = unass.FirstOrDefault().ProjectName;
                                                    //projAssignment.StartDate = unass.BillingDate;
                                                    projAssignment.StartDate = DOJC;

                                                    projAssignment.EndDate = Convert.ToDateTime(objHRRF.BillingDate);

                                                    // billdatge == getdate

                                                    projAssignment.Utilization = 100;
                                                    projAssignment.EmployeeId = externalhire.EmployeeId;
                                                    // need to check future date and active and inactive
                                                    if (DateTime.UtcNow >= Convert.ToDateTime(objHRRF.BillingDate))
                                                        projAssignment.IsActive = false;
                                                    else
                                                        projAssignment.IsActive = true;
                                                    projAssignment.Assigned_By = unass.FirstOrDefault().projmanger;
                                                    projAssignment.Assigned_Date = System.DateTime.UtcNow;
                                                    projAssignment.BillingStatus = unass.FirstOrDefault().Billingstatus;
                                                    if (projAssignment.BillingStatus == "Bench")
                                                    {
                                                        projAssignment.Category = "Deployable Bench";
                                                        projAssignment.Bechstatus = "Free Pool";
                                                    }
                                                    db.ProjectAssignments.Add(projAssignment);
                                                    #endregion

                                                    #region Selected Project Assignment History we get diff b/w doj to billing date 
                                                    ProjectAssignmenthistory projBenchHistory = new ProjectAssignmenthistory();
                                                    projBenchHistory.AssignmentId = 0;
                                                    projBenchHistory.ProjectCode = projAssignment.ProjectCode;
                                                    projBenchHistory.ProjectName = projAssignment.ProjectName;
                                                    projBenchHistory.ProjectID = projAssignment.ProjectID;
                                                    projBenchHistory.Assigned_ByOld = null;
                                                    projBenchHistory.BillingStatusOld = null;
                                                    projBenchHistory.EmployeeId = externalhire.EmployeeId;
                                                    projBenchHistory.EnddateOld = null;
                                                    projBenchHistory.IsActiveOld = null;
                                                    projBenchHistory.StartDateOld = null;
                                                    projBenchHistory.UtilizationOld = null;
                                                    projBenchHistory.modifiedBy = Convert.ToInt32(Session["EmployeeId"]);
                                                    projBenchHistory.ModifiedDate = DateTime.UtcNow;
                                                    projBenchHistory.UtilizationNew = projAssignment.Utilization;
                                                    projBenchHistory.StartDateNew = projAssignment.StartDate;
                                                    projBenchHistory.IsActiveNew = projAssignment.IsActive;
                                                    projBenchHistory.EndDateNew = projAssignment.EndDate;
                                                    projBenchHistory.BillingStatusNew = projAssignment.BillingStatus;
                                                    projBenchHistory.Assigned_byNew = projAssignment.Assigned_By;
                                                    db.ProjectAssignmenthistories.Add(projBenchHistory);
                                                    #endregion
                                                }
                                            }
                                    }
                                }
                            }
                            #endregion


                            ProjectAssignment objProjectAssignment = new ProjectAssignment();
                            var objProject = db.Projects.Where(a => a.ProjectCode == objHRRF.ProjectCode || a.ProjectName == objHRRF.ProjectName).FirstOrDefault();


                            if (objProject != null)
                            {
                                // Assignment status
                                if (objEmployeeAssignment != null)
                                {
                                    objEmployeeAssignment.AssignmentStatus = emp_Assignment;
                                    objEmployeeAssignment.ProjectManagerId = objProject.ProjectManagerId;
                                    db.Entry(objEmployeeAssignment).State = System.Data.Entity.EntityState.Modified;

                                }
                                #region ProjectAssignment
                                var objprojectmanagername = db.Employees.Where(a => a.EmployeeId == objProject.ProjectManagerId).FirstOrDefault();
                                string name = objprojectmanagername.LastName + "," + objprojectmanagername.FirstName + "." + objprojectmanagername.MiddleName;
                                int utl = Convert.ToInt32(objHRRF.Utilization);
                                objProjectAssignment.ProjectCode = objProject.ProjectCode;
                                objProjectAssignment.ProjectID = objProject.ProjectId;
                                objProjectAssignment.ProjectName = objProject.ProjectName;
                                objProjectAssignment.StartDate = objHRRF.BillingDate;
                                objProjectAssignment.EndDate = objHRRF.AssignmentEndDate;
                                objProjectAssignment.EmployeeId = externalhire.EmployeeId;
                                objProjectAssignment.Utilization = utl;
                                // need to check future date and active and inactive
                                if (DateTime.UtcNow.Date >= Convert.ToDateTime(objHRRF.BillingDate).Date)
                                {
                                    objProjectAssignment.IsActive = true;
                                }
                                else
                                {
                                    objProjectAssignment.IsActive = false;
                                }
                                objProjectAssignment.Assigned_By = name;
                                objProjectAssignment.Assigned_Date = System.DateTime.UtcNow;
                                if (objHRRF.Purpose.ToLower() == "project")
                                {
                                    objProjectAssignment.BillingStatus = objHRRF.ResourceType;
                                }
                                else
                                {
                                    objProjectAssignment.BillingStatus = (from bll in db.PracticeWiseBenchCodes
                                                                          where bll.BenchCode == objProject.ProjectCode
                                                                          select bll.BillingStatus).FirstOrDefault();
                                    if (objProjectAssignment.BillingStatus == "Bench")
                                    {
                                        objProjectAssignment.Category = "Deployable Bench";
                                        objProjectAssignment.Bechstatus = "Free Pool";
                                    }
                                }
                                db.ProjectAssignments.Add(objProjectAssignment);


                                #region Selected Project Assignment History

                                ProjectAssignmenthistory pash = new ProjectAssignmenthistory();
                                pash.AssignmentId = 0;
                                pash.ProjectCode = objProject.ProjectCode;
                                pash.ProjectName = objProject.ProjectName;
                                pash.ProjectID = objProject.ProjectId;
                                pash.Assigned_ByOld = null;
                                pash.BillingStatusOld = null;
                                pash.EmployeeId = externalhire.EmployeeId;
                                pash.EnddateOld = null;
                                pash.IsActiveOld = null;
                                pash.StartDateOld = null;
                                pash.UtilizationOld = null;
                                pash.modifiedBy = Convert.ToInt32(Session["EmployeeId"]);
                                pash.ModifiedDate = DateTime.UtcNow;
                                pash.UtilizationNew = utl;
                                pash.StartDateNew = objHRRF.BillingDate;
                                if (DateTime.UtcNow.Date >= Convert.ToDateTime(objHRRF.BillingDate).Date)
                                {
                                    pash.IsActiveNew = true;
                                }
                                else
                                {
                                    pash.IsActiveNew = false;
                                }
                                pash.EndDateNew = objHRRF.AssignmentEndDate;
                                if (objHRRF.Purpose.ToLower() == "project")
                                {
                                    pash.BillingStatusNew = objHRRF.ResourceType;
                                }
                                else
                                {
                                    pash.BillingStatusNew = (from bll in db.PracticeWiseBenchCodes
                                                             where bll.BenchCode == objProject.ProjectCode
                                                             select bll.BillingStatus).FirstOrDefault();
                                  
                                }
                                pash.Assigned_byNew = name;
                                db.ProjectAssignmenthistories.Add(pash);
                                #endregion

                                #endregion

                            }
                            else
                            {
                                if (objHRRF.Purpose.ToLower() != "project")
                                {
                                    int utl = Convert.ToInt32(objHRRF.Utilization);

                                    #region becnhfor util

                                    var unass = (from bpr in db.PracticeWiseBenchCodes
                                                 join pt in db.Projects on bpr.BenchCode equals pt.ProjectCode
                                                 join emp in db.Employees on pt.ProjectManagerId equals emp.EmployeeId
                                                 where bpr.Practice.ToLower().Equals(objHRRF.Practice.ToLower())
                                                 && bpr.CostCenter.ToLower().Contains(objHRRF.CostCenter.ToLower())
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
                                        ProjectAssignment prj1 = new ProjectAssignment();
                                        prj1.EmployeeId = externalhire.EmployeeId;
                                        prj1.ProjectID = unass[0].projectid;
                                        prj1.ProjectCode = unass[0].Projectcode;
                                        prj1.ProjectName = unass[0].ProjectName;
                                        prj1.StartDate = objHRRF.BillingDate;
                                        prj1.EndDate = objHRRF.AssignmentEndDate;
                                        if (DateTime.UtcNow.Date >= Convert.ToDateTime(objHRRF.BillingDate).Date)
                                        {
                                            prj1.IsActive = true;
                                        }
                                        else
                                        {
                                            prj1.IsActive = false;
                                        }
                                        prj1.Assigned_By = unass[0].projmanger;
                                        prj1.Assigned_Date = DateTime.UtcNow;
                                        prj1.BillingStatus = unass[0].Billingstatus;
                                        if (prj1.BillingStatus == "Bench")
                                        {
                                            prj1.Category = "Deployable Bench";
                                            prj1.Bechstatus = "Free Pool";
                                        }
                                        prj1.Utilization = utl;
                                        db.ProjectAssignments.Add(prj1);


                                        #region assignmenthistory
                                        ProjectAssignmenthistory pash = new ProjectAssignmenthistory();
                                        pash.AssignmentId = 0;
                                        pash.ProjectCode = unass[0].Projectcode;
                                        pash.ProjectName = unass[0].ProjectName;
                                        pash.ProjectID = unass[0].projectid;
                                        pash.Assigned_ByOld = null;
                                        pash.BillingStatusOld = null;
                                        pash.EmployeeId = externalhire.EmployeeId;
                                        pash.EnddateOld = null;
                                        pash.IsActiveOld = null;
                                        pash.StartDateOld = null;
                                        pash.UtilizationOld = null;
                                        pash.modifiedBy = Convert.ToInt32(Session["EmployeeId"]);
                                        pash.ModifiedDate = DateTime.UtcNow;
                                        pash.UtilizationNew = utl;
                                        pash.StartDateNew = objHRRF.BillingDate;
                                        if (DateTime.UtcNow.Date >= Convert.ToDateTime(objHRRF.BillingDate).Date)
                                        {
                                            pash.IsActiveNew = true;
                                        }
                                        else
                                        {
                                            pash.IsActiveNew = false;
                                        }
                                        pash.EndDateNew = objHRRF.AssignmentEndDate;
                                        pash.BillingStatusNew = unass[0].Billingstatus;
                                        pash.Assigned_byNew = unass[0].projmanger;
                                        db.ProjectAssignmenthistories.Add(pash);
                                        #endregion
                                    }

                                    #endregion

                                }

                            }

                        }

                            #endregion
                           
                        }

                    db.SaveChanges();


                    string strDesc = "";
                    if (!string.IsNullOrWhiteSpace(externalhire.RecruiterName.Trim()))
                    {
                        Employee RecruiterDetails = db.Employees.Find(Convert.ToInt32(externalhire.RecruiterName));
                        if (RecruiterDetails != null)
                        {
                            strDesc = "assigned to '" + RecruiterDetails.FirstName + " " + RecruiterDetails.LastName + "' and ";
                        }

                        //strDescription = externalhire.HRRFNumber + " - " + "has been assigned to '" + strRecruiterName + "' and changed to '" + externalhire.RequestStatus + "' status";
                    }

                    if (!string.IsNullOrWhiteSpace(externalhire.RequestStatus))
                    {
                        string strDescription = externalhire.HRRFNumber + " - " + "has been " + strDesc + "changed to '" + externalhire.RequestStatus + "' status";
                        string strRemarks = externalhire.FulfilmentRemarks.Trim();

                        switch (externalhire.RequestStatus.ToLower())
                        {
                            //case "fulfilled":
                            //    strRemarks = externalhire.FulfilmentRemarks.Trim();
                            //    break;
                            case "cancelled":
                                strDescription += " on " + externalhire.CancelDate;
                                strRemarks = externalhire.FulfilmentRemarks.Trim();
                                break;
                        }

                        // Capturing TR History
                        TRScernarioHistroy(externalhire.HRRFNumber, strDescription, strRemarks, Convert.ToInt32(Session["EmployeeId"]), prevstatus);

                    }

                    if (Session["EmployeeId"] != null)
                    {
                        Employee objEmp = db.Employees.Find(Session["EmployeeId"]);
                        string strStatusChangedBy = "";
                        if (objEmp != null)
                        {
                            strStatusChangedBy = objEmp.FirstName.Trim() + (!string.IsNullOrWhiteSpace(objEmp.MiddleName) ? " " + objEmp.MiddleName + " " : " ") + objEmp.LastName.Trim();
                            List<string> lstRecipients = new List<string>();

                            ConfigurationManager.AppSettings["ITS-OPS"].Split(',').ToList<string>().ForEach(Ops => lstRecipients.Add(Ops)); // mail to ITS-OPS
                                                                                                                                            //db.RoleMasters.Where(r => r.Role == "RL").Select(r => r.EmployeeId.ToString()).ToList<string>().ForEach(RLs => lstRecipients.Add(RLs)); // mail to all RLs
                            lstRecipients.Add(Session["EmployeeId"].ToString());
                          
                            if (!string.IsNullOrWhiteSpace(externalhire.RecruiterName.Trim()))
                            {
                                lstRecipients.Add(externalhire.RecruiterName.ToString());
                            }
                            //List<string> lstRLEmp = db.RoleMasters.Where(r => r.Role == "RL" && r.ApplicationCode == "TALENTREQ").Select(r => r.EmployeeId.ToString()).ToList();
                            //List<string> lstRLActiveEmp = db.Employees.Where(e => lstRLEmp.Contains(e.EmployeeId.ToString()) && e.IsActive == true && e.LocationType == strLocationType).Select(emp => emp.EmployeeId.ToString()).ToList();

                            //lstRLActiveEmp.ForEach(RLs => lstRecipients.Add(RLs)); // mail to all active RLs

                            if (externalhire.RequestStatus.ToLower() == "fulfilled" || externalhire.RequestStatus.ToLower() == "cancelled")
                            {
                                lstRecipients.Add(objHRRF.HRRFCreatedBy.ToString()); // mail to TR Created By

                                if (externalhire.RequestStatus.ToLower() == "fulfilled")
                                {

                                    ConfigurationManager.AppSettings["Assurance"].Split(',').ToList<string>().ForEach(Assurance => lstRecipients.Add(Assurance));
                                }
                            }

                            string strNotificationMessage = ConfigurationManager.AppSettings["ChangeStatusByRL"].Replace("#TRXXXX", externalhire.HRRFNumber) + "'" + externalhire.RequestStatus + "' by " + strStatusChangedBy + ".";
                            string loginEmp_Name = "";
                            int loginEmp_Id = Convert.ToInt32(Session["EmployeeId"]);
                            Employee obj_Emp = db.Employees.Find(loginEmp_Id);
                            if (obj_Emp != null)
                            {
                                loginEmp_Name = obj_Emp.FirstName + " " + (!string.IsNullOrWhiteSpace(obj_Emp.MiddleName) ? obj_Emp.MiddleName + " " : "") + obj_Emp.LastName;
                            }
                            string strNotification_Subject = ConfigurationManager.AppSettings["TrProposedExternalRes_NOtification"] + loginEmp_Name;

                            foreach (string recipient in lstRecipients.Distinct())
                            {
                                InsertNotification(Convert.ToString(Session["EmployeeId"]), recipient, strNotificationMessage, strNotification_Subject, externalhire.HRRFNumber, string.Empty);
                            }
                        }
                    }

                    return RedirectToAction("Index", "ExternalReport");
                }
                else
                {
                    TempData["EXMessage"] = errormsg;
                    return RedirectToAction("Save", "ExternalReport", new { HrrfId = externalhire.HRRFId, HrrfNumber = externalhire.HRRFNumber });
                }

            }

            //catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
            //{
            //    Exception raise = dbEx;
            //    foreach (var validationErrors in dbEx.EntityValidationErrors)
            //    {
            //        foreach (var validationError in validationErrors.ValidationErrors)
            //        {
            //            string message = string.Format("{0}:{1}",
            //                validationErrors.Entry.Entity.ToString(),
            //                validationError.ErrorMessage);
            //            // raise a new exception nesting
            //            // the current instance as InnerException
            //            raise = new InvalidOperationException(message, raise);
            //        }
            //    }
            //    ErrorHandling expcls = new ErrorHandling();
            //    expcls.Error(raise);
            //    throw raise;
            //}
            
                catch (Exception ex)
            {

                Common.WriteExceptionErrorLog(ex);
                return RedirectToAction("Error", "Error");
            }


        }
            else
            {
                //ermsg = "Session expired";
                //return Json("Sessionexpired", JsonRequestBehavior.AllowGet);
                return RedirectToAction("SessionExpire", "Signout");
            }
        }
        public ActionResult UpdateExternalHire(string ResourceName, string RecruiterName, string EmployeeId, string RequestStatus, int ExternalHireID, string FulfilmentRemarks, string Doj, string FulfilmentDate, string CancelReason, string dtCancel, string DuplicateHRRFNo)
        {
            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            {

                JsonResult result = new JsonResult();

            result.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            string HRRFNumber = string.Empty;
            string To = string.Empty;
            int EmpId;
            try
            {
                //var externalhireUpdate = (from exthire in db.ExternalHires where exthire.ExternalHireId == ExternalHireID select exthire).Single();
                //HRRFNumber = (from exthire in db.ExternalHires where exthire.ExternalHireId == ExternalHireID select exthire.HRRFNumber).Single();
                bool dojchk = false;
                string errormsg = "";
                ExternalHire externalhireUpdate = db.ExternalHires.Find(ExternalHireID);
                HRRFNumber = externalhireUpdate.HRRFNumber;
                HRRF objHRRF = db.HRRFs.Where(h => h.HRRFNumber.Equals(HRRFNumber)).FirstOrDefault();
                if (RequestStatus == "Fulfilled")
                {
                    int exempid = Convert.ToInt32(EmployeeId);
                    var objEmployee = db.Employees.Where(b => b.EmployeeId == exempid).ToList().FirstOrDefault();



                    if (objEmployee != null)
                    {
                        IFormatProvider theCultureInfo = new System.Globalization.CultureInfo("en-us", true);
                        DateTime? DOJ = DateTime.Parse(objEmployee.DateOfJoin.ToString(), theCultureInfo);
                        if (Convert.ToDateTime(DOJ).Date <= Convert.ToDateTime(objHRRF.BillingDate).Date)
                        {
                            dojchk = true;
                        }

                        if (dojchk)
                        {

                            var objpras1 = db.ProjectAssignments.Where(b => b.EmployeeId == objEmployee.EmployeeId && !(b.BillingStatus.Contains("Bench")) &&
                                 ((b.StartDate >= objHRRF.BillingDate && b.StartDate <= objHRRF.AssignmentEndDate) || (b.EndDate >= objHRRF.BillingDate && b.EndDate <= objHRRF.AssignmentEndDate)
                                 || (b.StartDate <= objHRRF.BillingDate && b.EndDate >= objHRRF.AssignmentEndDate))).ToList();

                            if (objpras1.Count > 0)
                            {
                                var sumUtilization = objpras1.Sum(p => p.Utilization);
                                if ((sumUtilization + Convert.ToInt32(objHRRF.Utilization)) > 100)
                                {
                                    dojchk = false;
                                    errormsg = objEmployee.FirstName + " already assigned to some other project with selected dates";
                                }
                            }

                        }
                        else
                        {
                            errormsg = "Employee date of joining should be less than billing date of the TR.";
                        }
                    }
                    else
                    {
                        errormsg = "Employee details not exists";
                        dojchk = false;
                    }
                }
                else
                {
                    dojchk = true;
                }




                if (dojchk)
                {
                    result.Data = "Success";
                    int? Utilzation = null;


                    bool flag = false;
                    if (externalhireUpdate.RecruiterName != RecruiterName)
                    {
                        flag = true;
                    }

                    string PrevRequestStatus = externalhireUpdate.RequestStatus;

					//string name = User.Identity.Name.Split('\\')[1].ToLower();
					string usermail = Common.GetAzureLoggedInUserID();
					EmpId = Convert.ToInt32((from data in db.Employees where (data.Email.Equals(usermail) & data.IsActive == true) select data.EmployeeId).FirstOrDefault());
                    To = Convert.ToString((from Toid in db.Employees where Toid.EmployeeId == EmpId select Toid.SupervisorId).Single());

                    externalhireUpdate.Name = ResourceName;
                    if (!string.IsNullOrWhiteSpace(EmployeeId))
                    {
                        externalhireUpdate.EmployeeId = Convert.ToInt32(EmployeeId);
                    }
                    //externalhireUpdate.EmployeeId 
                    externalhireUpdate.RequestStatus = RequestStatus;
                    externalhireUpdate.RecruiterName = RecruiterName;
                    externalhireUpdate.FulfilmentRemarks = FulfilmentRemarks;

                    IFormatProvider theCultureInfo = new System.Globalization.CultureInfo("en-us", true);

                    if (Doj != string.Empty)
                    {
                        DateTime? DOJ1 = DateTime.Parse(Doj.ToString(), theCultureInfo);
                        externalhireUpdate.DOJ = DOJ1;
                    }
                    else
                    {
                        externalhireUpdate.DOJ = null;
                    }

                    externalhireUpdate.FulfilmentDate = string.IsNullOrEmpty(FulfilmentDate) ? new DateTime?() : Convert.ToDateTime(FulfilmentDate, CultureInfo.GetCultureInfo("en-US"));
                    //externalhireUpdate.DOJ = DateTime.ParseExact(Doj,"MM/dd/yyyy",CultureInfo.InvariantCulture); 
                    //externalhireUpdate.FulfilmentDate = DateTime.ParseExact(FulfilmentDate, "MM/dd/yyyy", CultureInfo.InvariantCulture);
                    externalhireUpdate.CancellationReason = CancelReason;
                    externalhireUpdate.CancelDate = string.IsNullOrEmpty(dtCancel) ? new DateTime?() : DateTime.ParseExact(dtCancel, "MM/dd/yyyy", null);
                    externalhireUpdate.DuplicateHRRFNo = DuplicateHRRFNo;
                    db.Entry(externalhireUpdate).State = System.Data.Entity.EntityState.Modified;
                    //db.SaveChanges();
                    string strLocationType = "";

                    if (objHRRF != null)
                    {
                        strLocationType = (objHRRF.LocationType == "1") ? "ONSITE" : "OFFSHORE";
                        objHRRF.RequestStatus = RequestStatus;

                        if (RequestStatus == "Fulfilled")
                        {
                            #region ProjectAssignment
                            int empId1 = Convert.ToInt32(EmployeeId);
                            objHRRF.ResourceName = ResourceName;
                            var objEmployeeAssignment = db.Employees.Where(a => a.EmployeeId == empId1).FirstOrDefault();
                            DateTime? DOJC = DateTime.Parse(objEmployeeAssignment.DateOfJoin.ToString(), theCultureInfo);
                            DateTime dtStartDate = Convert.ToDateTime(objHRRF.BillingDate);
                            DateTime dtEndDate = Convert.ToDateTime(objHRRF.AssignmentEndDate);


                            var objpras1 = db.ProjectAssignments.Where(b => b.EmployeeId == empId1 && !(b.BillingStatus.Contains("Bench")) && ((b.StartDate >= dtStartDate && b.StartDate <= dtEndDate)
                            || (b.EndDate >= dtStartDate && b.EndDate <= dtEndDate) || (b.StartDate <= dtStartDate && b.EndDate >= dtEndDate))).ToList();
                            if (objpras1.Count() > 0)
                            {
                                var sumUtilization = objpras1.Sum(p => p.Utilization);
                                Utilzation = 100 - sumUtilization;
                            }

                            //to checkprevious Bench 
                            #region Existing Bench Record
                            var previousprojectass = db.ProjectAssignments.Where(p => p.IsActive == true && p.EmployeeId == empId1 && p.BillingStatus == "Bench").ToList();
                            var dtProjectAssignSatrtAsEndDate = db.ProjectAssignments.Where(pa => pa.EmployeeId == empId1
                                             && DateTime.UtcNow <= pa.StartDate).OrderBy(pa => pa.StartDate).Select(pa => pa.StartDate).FirstOrDefault();
                            var dtProjectAssignEndDate = db.ProjectAssignments.Where(pa => pa.EmployeeId == empId1 && pa.IsActive == true && pa.BillingStatus != "Bench"
                                   && DateTime.UtcNow <= pa.EndDate).OrderBy(pa => pa.EndDate).Select(pa => pa.EndDate).FirstOrDefault();
                            if (previousprojectass.Count != 0)
                            {
                                foreach (var lt in previousprojectass)
                                {
                                    #region Bench Assignment History
                                    ProjectAssignmenthistory pash = new ProjectAssignmenthistory();
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
                                    pash.modifiedBy = Convert.ToInt32(Session["EmployeeId"]);
                                    pash.ModifiedDate = DateTime.UtcNow;

                                    #region Exisitng Bench Record Update based on selected Dates
                                    // Selecting top 1 Start Date order by desc to Set Bench Enda date
                                    // billdatge == getdate
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
                                        ul = lt.Utilization;

                                    if (dtProjectAssignSatrtAsEndDate != null && Convert.ToDateTime(dtProjectAssignSatrtAsEndDate).Date <= Convert.ToDateTime(objHRRF.BillingDate).Date)
                                    {
                                        lt.EndDate = Convert.ToDateTime(dtProjectAssignSatrtAsEndDate);
                                    }
                                    else if (dtProjectAssignEndDate != null && dtProjectAssignEndDate <= Convert.ToDateTime(objHRRF.AssignmentEndDate) && DateTime.UtcNow.Date.Equals(Convert.ToDateTime(objHRRF.BillingDate).Date))
                                    {
                                        if (Convert.ToDateTime(objHRRF.BillingDate) < dtProjectAssignEndDate)
                                            lt.EndDate = Convert.ToDateTime(objHRRF.BillingDate);
                                        else
                                            lt.EndDate = Convert.ToDateTime(dtProjectAssignEndDate);
                                    }
                                    else if (DateTime.UtcNow.Date >= (Convert.ToDateTime(objHRRF.BillingDate).Date))
                                    {
                                        if (ul > 0)
                                            lt.EndDate = Convert.ToDateTime(objHRRF.AssignmentEndDate);
                                        else
                                        {
                                            if (Convert.ToDateTime(objHRRF.BillingDate) >= Convert.ToDateTime(lt.StartDate))
                                                lt.EndDate = Convert.ToDateTime(objHRRF.BillingDate);
                                            else
                                                lt.EndDate = lt.StartDate;

                                        }
                                    }
                                    else
                                    {
                                        lt.EndDate = Convert.ToDateTime(objHRRF.BillingDate);
                                    }

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
                                    db.ProjectAssignmenthistories.Add(pash);
                                    #endregion

                                    db.Entry(lt).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                }
                            }
                            else
                            {
                                int benchUtilization = 0;
                                if (DateTime.UtcNow.Date >= Convert.ToDateTime(objHRRF.BillingDate).Date)
                                {
                                    if (Utilzation == null)
                                    {
                                        benchUtilization = 100 - Convert.ToInt32(objHRRF.Utilization);
                                    }
                                    else
                                    {
                                        benchUtilization = Convert.ToInt32(Utilzation - Convert.ToInt32(objHRRF.Utilization));
                                    }

                                    //projAssignment.Utilization = Utilzation;

                                }
                                else if (DateTime.UtcNow.Date < Convert.ToDateTime(objHRRF.BillingDate).Date)
                                {
                                    benchUtilization = 100;
                                }
                                if (benchUtilization > 0)
                                {
                                    #region First Bench Record if Bench record not exist for the Employee
                                    DateTime stdat = new DateTime();
                                    DateTime etdat = new DateTime();

                                    if (DateTime.UtcNow.Date >= Convert.ToDateTime(objHRRF.BillingDate).Date)
                                    {
                                        stdat = Convert.ToDateTime(objHRRF.BillingDate);
                                    }
                                    else
                                    {
                                        stdat = DateTime.UtcNow.Date;
                                    }
                                    if (dtProjectAssignSatrtAsEndDate != null && dtProjectAssignSatrtAsEndDate <= Convert.ToDateTime(objHRRF.BillingDate) && DateTime.UtcNow.Date.Equals(Convert.ToDateTime(objHRRF.BillingDate).Date))
                                    {
                                        etdat = Convert.ToDateTime(dtProjectAssignSatrtAsEndDate);
                                    }
                                    else if (dtProjectAssignEndDate != null && dtProjectAssignEndDate <= Convert.ToDateTime(objHRRF.AssignmentEndDate) && DateTime.UtcNow.Date.Equals(Convert.ToDateTime(objHRRF.BillingDate).Date))
                                    {
                                        if (Convert.ToDateTime(objHRRF.BillingDate) < dtProjectAssignEndDate)
                                            etdat = Convert.ToDateTime(objHRRF.BillingDate);
                                        else
                                            etdat = Convert.ToDateTime(dtProjectAssignEndDate);
                                    }
                                    else if (DateTime.UtcNow.Date >= (Convert.ToDateTime(objHRRF.BillingDate).Date))
                                    {
                                        etdat = Convert.ToDateTime(objHRRF.AssignmentEndDate);
                                    }
                                    else
                                    {
                                        etdat = Convert.ToDateTime(objHRRF.BillingDate);
                                    }


                                    bool dojbenck = true;

                                    var objpras11 = db.ProjectAssignments.Where(b => b.EmployeeId == empId1
                                    && ((b.StartDate >= stdat
                                    && b.StartDate <= etdat) || (b.EndDate >= stdat && b.EndDate <= etdat) ||
                                    (b.StartDate <= stdat && b.EndDate >= etdat))).ToList();

                                    if (objpras11.Count > 0)
                                    {
                                        var sumUtilization = objpras11.Sum(p => p.Utilization);
                                        if ((sumUtilization + Convert.ToInt32(objHRRF.Utilization)) > 100)
                                            dojbenck = false;

                                    }
                                    if (dojbenck)
                                    {
                                        var unass = (from bpr in db.PracticeWiseBenchCodes
                                                     join pt in db.Projects on bpr.BenchCode equals pt.ProjectCode
                                                     join emp in db.Employees on pt.ProjectManagerId equals emp.EmployeeId
                                                     where bpr.CostCenter.ToLower().Contains(objEmployeeAssignment.CostCenter.ToLower())
                                                     select new
                                                     {
                                                         Projectcode = pt.ProjectCode,
                                                         ProjectName = pt.ProjectName,
                                                         projectid = pt.ProjectId,
                                                         projmanger = emp.LastName + "," + emp.FirstName + " " + emp.MiddleName,
                                                         Billingstatus = bpr.BillingStatus,
                                                         Practice = bpr.Practice
                                                     }).Distinct().ToList();

                                        if (objEmployeeAssignment.CostCenter.ToLower() != "testing")
                                        {

                                            unass = unass.Where(p => p.Practice.ToLower() == objEmployeeAssignment.Practice.ToLower()).ToList();
                                        }

                                        ProjectAssignment projAssignment = new ProjectAssignment();
                                        projAssignment.ProjectCode = unass.FirstOrDefault().Projectcode;
                                        projAssignment.ProjectID = unass.FirstOrDefault().projectid;
                                        projAssignment.ProjectName = unass.FirstOrDefault().ProjectName;
                                        projAssignment.StartDate = stdat;
                                        projAssignment.EndDate = etdat;
                                        // billdatge == getdate

                                        projAssignment.Utilization = benchUtilization;
                                        projAssignment.EmployeeId = empId1;
                                        // need to check future date and active and inactive
                                        projAssignment.IsActive = true;
                                        projAssignment.Assigned_By = unass.FirstOrDefault().projmanger;
                                        projAssignment.Assigned_Date = System.DateTime.UtcNow;
                                        projAssignment.BillingStatus = unass.FirstOrDefault().Billingstatus;
                                        if (projAssignment.BillingStatus == "Bench")
                                        {
                                            projAssignment.Category = "Deployable Bench";
                                            projAssignment.Bechstatus = "Free Pool";
                                        }
                                        db.ProjectAssignments.Add(projAssignment);
                                        #endregion

                                        #region Selected Project Assignment History
                                        ProjectAssignmenthistory projBenchHistory = new ProjectAssignmenthistory();
                                        projBenchHistory.AssignmentId = 0;
                                        projBenchHistory.ProjectCode = projAssignment.ProjectCode;
                                        projBenchHistory.ProjectName = projAssignment.ProjectName;
                                        projBenchHistory.ProjectID = projAssignment.ProjectID;
                                        projBenchHistory.Assigned_ByOld = null;
                                        projBenchHistory.BillingStatusOld = null;
                                        projBenchHistory.EmployeeId = empId1;
                                        projBenchHistory.EnddateOld = null;
                                        projBenchHistory.IsActiveOld = null;
                                        projBenchHistory.StartDateOld = null;
                                        projBenchHistory.UtilizationOld = null;
                                        projBenchHistory.modifiedBy = Convert.ToInt32(Session["EmployeeId"]);
                                        projBenchHistory.ModifiedDate = DateTime.UtcNow;
                                        projBenchHistory.UtilizationNew = projAssignment.Utilization;
                                        projBenchHistory.StartDateNew = projAssignment.StartDate;
                                        projBenchHistory.IsActiveNew = projAssignment.IsActive;
                                        projBenchHistory.EndDateNew = projAssignment.EndDate;
                                        projBenchHistory.BillingStatusNew = projAssignment.BillingStatus;
                                        projBenchHistory.Assigned_byNew = projAssignment.Assigned_By;
                                        db.ProjectAssignmenthistories.Add(projBenchHistory);
                                        #endregion
                                    }
                                }
                                else
                                {
                                    if ((Convert.ToDateTime(objHRRF.BillingDate).Date - Convert.ToDateTime(DOJC).Date).TotalDays > 0)
                                    {
                                        bool dojbenck = true;
                                        var objpras11 = db.ProjectAssignments.Where(b => b.EmployeeId == empId1 && ((b.StartDate >= DOJC && b.StartDate <= (objHRRF.BillingDate)
                                        || (b.EndDate >= DOJC && b.EndDate <= objHRRF.BillingDate)
                               || (b.StartDate <= DOJC && b.EndDate >= objHRRF.BillingDate)))).ToList();

                                        if (objpras11.Count > 0)
                                        {
                                            var sumUtilization = objpras11.Sum(p => p.Utilization);
                                            if ((sumUtilization + Convert.ToInt32(objHRRF.Utilization)) > 100)
                                                dojbenck = false;

                                        }
                                        if (dojbenck)
                                        {
                                            #region First Bench Record we get diff b/w doj to billing date 
                                            var unass = (from bpr in db.PracticeWiseBenchCodes
                                                         join pt in db.Projects on bpr.BenchCode equals pt.ProjectCode
                                                         join emp in db.Employees on pt.ProjectManagerId equals emp.EmployeeId
                                                         where bpr.CostCenter.ToLower().Contains(objEmployeeAssignment.CostCenter.ToLower())
                                                         select new
                                                         {
                                                             Projectcode = pt.ProjectCode,
                                                             ProjectName = pt.ProjectName,
                                                             projectid = pt.ProjectId,
                                                             projmanger = emp.LastName + "," + emp.FirstName + " " + emp.MiddleName,
                                                             Billingstatus = bpr.BillingStatus,
                                                             Practice = bpr.Practice
                                                         }).Distinct().ToList();

                                            if (objEmployeeAssignment.CostCenter.ToLower() != "testing")
                                            {

                                                unass = unass.Where(p => p.Practice.ToLower() == objEmployeeAssignment.Practice.ToLower()).ToList();
                                            }

                                            ProjectAssignment projAssignment = new ProjectAssignment();
                                            projAssignment.ProjectCode = unass.FirstOrDefault().Projectcode;
                                            projAssignment.ProjectID = unass.FirstOrDefault().projectid;
                                            projAssignment.ProjectName = unass.FirstOrDefault().ProjectName;
                                            //projAssignment.StartDate = unass.BillingDate;
                                            projAssignment.StartDate = DOJC;

                                            projAssignment.EndDate = Convert.ToDateTime(objHRRF.BillingDate);

                                            // billdatge == getdate

                                            projAssignment.Utilization = 100;
                                            projAssignment.EmployeeId = empId1;
                                            // need to check future date and active and inactive
                                            if (DateTime.UtcNow >= Convert.ToDateTime(objHRRF.BillingDate))
                                                projAssignment.IsActive = false;
                                            else
                                                projAssignment.IsActive = true;
                                            projAssignment.Assigned_By = unass.FirstOrDefault().projmanger;
                                            projAssignment.Assigned_Date = System.DateTime.UtcNow;
                                            projAssignment.BillingStatus = unass.FirstOrDefault().Billingstatus;
                                            if (projAssignment.BillingStatus == "Bench")
                                            {
                                                projAssignment.Category = "Deployable Bench";
                                                projAssignment.Bechstatus = "Free Pool";
                                            }
                                            db.ProjectAssignments.Add(projAssignment);
                                            #endregion

                                            #region Selected Project Assignment History we get diff b/w doj to billing date 
                                            ProjectAssignmenthistory projBenchHistory = new ProjectAssignmenthistory();
                                            projBenchHistory.AssignmentId = 0;
                                            projBenchHistory.ProjectCode = projAssignment.ProjectCode;
                                            projBenchHistory.ProjectName = projAssignment.ProjectName;
                                            projBenchHistory.ProjectID = projAssignment.ProjectID;
                                            projBenchHistory.Assigned_ByOld = null;
                                            projBenchHistory.BillingStatusOld = null;
                                            projBenchHistory.EmployeeId = empId1;
                                            projBenchHistory.EnddateOld = null;
                                            projBenchHistory.IsActiveOld = null;
                                            projBenchHistory.StartDateOld = null;
                                            projBenchHistory.UtilizationOld = null;
                                            projBenchHistory.modifiedBy = Convert.ToInt32(Session["EmployeeId"]);
                                            projBenchHistory.ModifiedDate = DateTime.UtcNow;
                                            projBenchHistory.UtilizationNew = projAssignment.Utilization;
                                            projBenchHistory.StartDateNew = projAssignment.StartDate;
                                            projBenchHistory.IsActiveNew = projAssignment.IsActive;
                                            projBenchHistory.EndDateNew = projAssignment.EndDate;
                                            projBenchHistory.BillingStatusNew = projAssignment.BillingStatus;
                                            projBenchHistory.Assigned_byNew = projAssignment.Assigned_By;
                                            db.ProjectAssignmenthistories.Add(projBenchHistory);
                                            #endregion
                                        }
                                    }
                                }
                            }
                            #endregion



                            var objProject = db.Projects.Where(a => a.ProjectCode == objHRRF.ProjectCode || a.ProjectName == objHRRF.ProjectName).FirstOrDefault();
                            ProjectAssignment objProjectAssignment = new ProjectAssignment();
                            if (objProject != null)
                            {
                                // Assignment status

                                if (objEmployeeAssignment != null)
                                {
                                    objEmployeeAssignment.AssignmentStatus = emp_Assignment;
                                    objEmployeeAssignment.ProjectManagerId = objProject.ProjectManagerId;
                                    db.Entry(objEmployeeAssignment).State = System.Data.Entity.EntityState.Modified;
                                }
                                //var objProjectAssignment = db.ProjectAssignments;
                                #region ProjectAssignment
                                var objprojectmanagername = db.Employees.Where(a => a.EmployeeId == objProject.ProjectManagerId).FirstOrDefault();
                                string pmname = objprojectmanagername.LastName + "," + objprojectmanagername.FirstName + "." + objprojectmanagername.MiddleName;
                                int utl = Convert.ToInt32(objHRRF.Utilization);
                                objProjectAssignment.ProjectCode = objProject.ProjectCode;
                                objProjectAssignment.ProjectID = objProject.ProjectId;
                                objProjectAssignment.ProjectName = objProject.ProjectName;
                                objProjectAssignment.StartDate = objHRRF.BillingDate;
                                objProjectAssignment.EndDate = objHRRF.AssignmentEndDate;
                                objProjectAssignment.EmployeeId = empId1;
                                objProjectAssignment.Utilization = utl;
                                // need to check future date and active and inactive
                                if (DateTime.UtcNow.Date >= Convert.ToDateTime(objHRRF.BillingDate).Date)
                                {
                                    objProjectAssignment.IsActive = true;
                                }
                                else
                                {
                                    objProjectAssignment.IsActive = false;
                                }
                                objProjectAssignment.Assigned_By = pmname;
                                objProjectAssignment.Assigned_Date = System.DateTime.UtcNow;
                                if (objHRRF.Purpose.ToLower() == "project")
                                {
                                    objProjectAssignment.BillingStatus = objHRRF.ResourceType;
                                }
                                else
                                {
                                    objProjectAssignment.BillingStatus = (from bll in db.PracticeWiseBenchCodes
                                                                          where bll.BenchCode == objProject.ProjectCode
                                                                          select bll.BillingStatus).FirstOrDefault();
                                    if (objProjectAssignment.BillingStatus == "Bench")
                                    {
                                        objProjectAssignment.Category = "Deployable Bench";
                                        objProjectAssignment.Bechstatus = "Free Pool";
                                    }
                                }
                                db.ProjectAssignments.Add(objProjectAssignment);


                                #region Selected Project Assignment History

                                ProjectAssignmenthistory pash = new ProjectAssignmenthistory();
                                pash.AssignmentId = 0;
                                pash.ProjectCode = objProject.ProjectCode;
                                pash.ProjectName = objProject.ProjectName;
                                pash.ProjectID = objProject.ProjectId;
                                pash.Assigned_ByOld = null;
                                pash.BillingStatusOld = null;
                                pash.EmployeeId = empId1;
                                pash.EnddateOld = null;
                                pash.IsActiveOld = null;
                                pash.StartDateOld = null;
                                pash.UtilizationOld = null;
                                pash.modifiedBy = Convert.ToInt32(Session["EmployeeId"]);
                                pash.ModifiedDate = DateTime.UtcNow;
                                pash.UtilizationNew = utl;
                                pash.StartDateNew = objHRRF.BillingDate;
                                if (DateTime.UtcNow.Date >= Convert.ToDateTime(objHRRF.BillingDate).Date)
                                {
                                    pash.IsActiveNew = true;
                                }
                                else
                                {
                                    pash.IsActiveNew = false;
                                }
                                pash.EndDateNew = objHRRF.AssignmentEndDate;
                                if (objHRRF.Purpose.ToLower() == "project")
                                {
                                    pash.BillingStatusNew = objHRRF.ResourceType;
                                }
                                else
                                {
                                    pash.BillingStatusNew = (from bll in db.PracticeWiseBenchCodes
                                                             where bll.BenchCode == objProject.ProjectCode
                                                             select bll.BillingStatus).FirstOrDefault();
                                }
                                pash.Assigned_byNew = usermail.Split('@')[0].ToLower();
                                db.ProjectAssignmenthistories.Add(pash);
                                #endregion

                                #endregion
                            }
                            else
                            {
                                if (objHRRF.Purpose.ToLower() != "project")

                                {
                                    int utl = Convert.ToInt32(objHRRF.Utilization);
                                    int eml = Convert.ToInt32(EmployeeId);
                                    #region becnhfor util

                                    var unass = (from bpr in db.PracticeWiseBenchCodes
                                                 join pt in db.Projects on bpr.BenchCode equals pt.ProjectCode
                                                 join emp in db.Employees on pt.ProjectManagerId equals emp.EmployeeId
                                                 where bpr.Practice.ToLower().Equals(objHRRF.Practice.ToLower())
                                                 && bpr.CostCenter.ToLower().Contains(objHRRF.CostCenter.ToLower())
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
                                        ProjectAssignment prj1 = new ProjectAssignment();
                                        prj1.EmployeeId = empId1;
                                        prj1.ProjectID = unass[0].projectid;
                                        prj1.ProjectCode = unass[0].Projectcode;
                                        prj1.ProjectName = unass[0].ProjectName;
                                        prj1.StartDate = objHRRF.BillingDate;
                                        prj1.EndDate = objHRRF.AssignmentEndDate;
                                        if (DateTime.UtcNow.Date >= Convert.ToDateTime(objHRRF.BillingDate).Date)
                                        {
                                            prj1.IsActive = true;
                                        }
                                        else
                                        {
                                            prj1.IsActive = false;
                                        }
                                        prj1.Assigned_By = unass[0].projmanger;
                                        prj1.Assigned_Date = DateTime.UtcNow;
                                        prj1.BillingStatus = unass[0].Billingstatus;
                                        if (prj1.BillingStatus == "Bench")
                                        {
                                            prj1.Category = "Deployable Bench";
                                            prj1.Bechstatus = "Free Pool";
                                        }
                                        prj1.Utilization = utl;
                                        db.ProjectAssignments.Add(prj1);


                                        #region assignmenthistory
                                        ProjectAssignmenthistory pash = new ProjectAssignmenthistory();
                                        pash.AssignmentId = 0;
                                        pash.ProjectCode = unass[0].Projectcode;
                                        pash.ProjectName = unass[0].ProjectName;
                                        pash.ProjectID = unass[0].projectid;
                                        pash.Assigned_ByOld = null;
                                        pash.BillingStatusOld = null;
                                        pash.EmployeeId = empId1;
                                        pash.EnddateOld = null;
                                        pash.IsActiveOld = null;
                                        pash.StartDateOld = null;
                                        pash.UtilizationOld = null;
                                        pash.modifiedBy = Convert.ToInt32(Session["EmployeeId"]);
                                        pash.ModifiedDate = DateTime.UtcNow;
                                        pash.UtilizationNew = utl;
                                        pash.StartDateNew = objHRRF.BillingDate;
                                        if (DateTime.UtcNow.Date >= Convert.ToDateTime(objHRRF.BillingDate).Date)
                                        {
                                            pash.IsActiveNew = true;
                                        }
                                        else
                                        {
                                            pash.IsActiveNew = false;
                                        }
                                        pash.EndDateNew = objHRRF.AssignmentEndDate;
                                        pash.BillingStatusNew = unass[0].Billingstatus;
                                        pash.Assigned_byNew = unass[0].projmanger;
                                        db.ProjectAssignmenthistories.Add(pash);
                                        #endregion
                                    }

                                    #endregion
                                }

                            }
                            #endregion
                        }


                    }
                    db.SaveChanges();


                    string strDesc = "";
                    if (flag)
                    {
                        Employee RecruiterDetails = db.Employees.Find(Convert.ToInt32(RecruiterName));
                        if (RecruiterDetails != null)
                        {
                            strDesc = "assigned to '" + RecruiterDetails.FirstName + " " + RecruiterDetails.LastName + "' and ";
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(RequestStatus))
                    {
                        string strDescription = HRRFNumber + " - " + "has been " + strDesc + "changed to '" + RequestStatus + "' status";

                        string strRemarks = FulfilmentRemarks.Trim();

                        switch (RequestStatus.ToLower())
                        {
                            //case "fulfilled":
                            //    strRemarks = FulfilmentRemarks.Trim();
                            //    break;
                            case "cancelled":
                                strDescription += " on " + Convert.ToDateTime(externalhireUpdate.CancelDate).ToString("dd-MMM-yyyy");
                                strRemarks = FulfilmentRemarks.Trim();
                                break;
                        }

                        // Capturing TR History
                        TRScernarioHistroy(HRRFNumber, strDescription, strRemarks, Convert.ToInt32(Session["EmployeeId"]), PrevRequestStatus);
                    }

                    if (Session["EmployeeId"] != null)
                    {
                        Employee objEmp = db.Employees.Find(Session["EmployeeId"]);
                        string strStatusChangedBy = "";
                        if (objEmp != null)
                        {
                            strStatusChangedBy = objEmp.FirstName.Trim() + (!string.IsNullOrWhiteSpace(objEmp.MiddleName) ? " " + objEmp.MiddleName + " " : " ") + objEmp.LastName.Trim();
                            List<string> lstRecipients = new List<string>();

                            ConfigurationManager.AppSettings["ITS-OPS"].Split(',').ToList<string>().ForEach(Ops => lstRecipients.Add(Ops)); // mail to ITS-OPS

                            lstRecipients.Add(Session["EmployeeId"].ToString());
                            
                            HRRFHistory hrrfhistory = db.HRRFHistories.Where(hr => hr.HistoryDescription.ToLower().Contains("has been assigned") && hr.HRRFNumber == HRRFNumber).FirstOrDefault();
                            if (hrrfhistory != null)
                            {
                                string TR_Assignedby = (hrrfhistory.ModifiedBy).ToString();
                                if (!string.IsNullOrEmpty(TR_Assignedby))
                                {
                                    lstRecipients.Add(TR_Assignedby);
                                }
                            }
                            if (!string.IsNullOrEmpty(externalhireUpdate.RecruiterName))
                            {
                                lstRecipients.Add(externalhireUpdate.RecruiterName);
                            }
                            //db.RoleMasters.Where(r => r.Role == "RL").Select(r => r.EmployeeId.ToString()).ToList<string>().ForEach(RLs => lstRecipients.Add(RLs)); // mail to all RLs
                            //List<string> lstRLEmp = db.RoleMasters.Where(r => r.Role == "RL" && r.ApplicationCode == "TALENTREQ").Select(r => r.EmployeeId.ToString()).ToList();
                            //List<string> lstRLActiveEmp = db.Employees.Where(e => lstRLEmp.Contains(e.EmployeeId.ToString()) && e.IsActive == true && e.LocationType == strLocationType).Select(emp => emp.EmployeeId.ToString()).ToList();

                            //lstRLActiveEmp.ForEach(RLs => lstRecipients.Add(RLs)); // mail to all active RLs

                            if (externalhireUpdate.RequestStatus.ToLower() == "fulfilled" || externalhireUpdate.RequestStatus.ToLower() == "cancelled")
                            {
                                lstRecipients.Add(objHRRF.HRRFCreatedBy.ToString()); // mail to TR Created By
                                if (externalhireUpdate.RequestStatus.ToLower() == "fulfilled")
                                {
                                    ConfigurationManager.AppSettings["Assurance"].Split(',').ToList<string>().ForEach(Assurance => lstRecipients.Add(Assurance));
                                }
                            }
                                string strNotificationMessage = ConfigurationManager.AppSettings["ChangeStatusByRL"].Replace("#TRXXXX", externalhireUpdate.HRRFNumber) + "'" + externalhireUpdate.RequestStatus + "' by " + strStatusChangedBy + ".";
                            string loginEmp_Name = "";
                            int loginEmp_Id = Convert.ToInt32(Session["EmployeeId"]);
                            Employee obj_Emp = db.Employees.Find(loginEmp_Id);
                            if (obj_Emp != null)
                            {
                                loginEmp_Name = obj_Emp.FirstName + " " + (!string.IsNullOrWhiteSpace(obj_Emp.MiddleName) ? obj_Emp.MiddleName + " " : "") + obj_Emp.LastName;
                            }
                            string strNotification_Subject = ConfigurationManager.AppSettings["TrProposedExternalRes_NOtification"] + loginEmp_Name;

                            foreach (string recipient in lstRecipients.Distinct())
                            {
                                InsertNotification(Convert.ToString(Session["EmployeeId"]), recipient, strNotificationMessage, strNotification_Subject, externalhireUpdate.HRRFNumber, string.Empty);
                            }
                        }
                    }
                }
                else
                {
                    result.Data = errormsg;
                }
            }
                //catch (Exception ex)
                //{
                //    //some code to handle error
                //    ErrorHandling expcls = new ErrorHandling();
                //    expcls.Error(ex);
                //    return null;
                //}
                catch (Exception ex)
                {

                    Common.WriteExceptionErrorLog(ex);
                    return Json("Error", JsonRequestBehavior.AllowGet);
                }
                return result;
        }
        else
            {
                //ermsg = "Session expired";
                return Json("Sessionexpired", JsonRequestBehavior.AllowGet);
        //return RedirectToAction("SessionExpire", "Signout");
    }
}
#region old method
//commented by Hemanth
//[HttpPost]
//public ActionResult Save([Bind(Include = "ExternalHireId,RecruiterName,Name,EmployeeId,RequestStatus,FulfilmentRemarks,DOJ,FulfilmentDate,CancellationReason,HRRFNumber,HRRFId")] ExternalHire externalhire)
//{
//    try
//    {
//        //string HRRFNumber = Session["hrrfno"].ToString();
//        //HRRF objHRRFs = db.HRRFs.Where(h => h.HRRFNumber.Equals(HRRFNumber)).FirstOrDefault();
//        //string PrevRequestStatus = objHRRFs.RequestStatus;

//        externalhire.Name = Request.Form["txtResourceName"];

//        if (!string.IsNullOrWhiteSpace(Request.Form["txtEmpId"]))
//        {
//            externalhire.EmployeeId = Convert.ToInt32(Request.Form["txtEmpId"]);
//        }
//        externalhire.RequestStatus = Request.Form["ddlRequestStatus"];
//        externalhire.FulfilmentRemarks = Request.Form["txtFulfilmentRemarks"];
//        externalhire.DOJ = string.IsNullOrEmpty(Request.Form["DOJ"]) ? new DateTime?() : DateTime.ParseExact(Request.Form["DOJ"], "MM/dd/yyyy", null);//DateTime.Parse(Request.Form["DOJ"]); //string.IsNullOrEmpty(Request.Form["DOJ"].ToString()) ? (DateTime?)null : DateTime.Parse(Request.Form["DOJ"]);
//        //externalhire.FulfilmentDate = DateTime.ParseExact(Request.Form["FulfilmentDate"], "MM/dd/yyyy", CultureInfo.InvariantCulture);// string.IsNullOrEmpty(Request.Form["FulfilmentDate"].ToString()) ? (DateTime?)null : DateTime.Parse(Request.Form["FulfilmentDate"]);
//        externalhire.FulfilmentDate = string.IsNullOrEmpty(Request.Form["FulfilmentDate"]) ? new DateTime?() : DateTime.ParseExact(Request.Form["FulfilmentDate"], "MM/dd/yyyy", null);
//        externalhire.CancellationReason = Request.Form["txtCancelRemarks"];
//        externalhire.CancelDate = string.IsNullOrEmpty(Request.Form["DOC"]) ? new DateTime?() : DateTime.ParseExact(Request.Form["DOC"], "MM/dd/yyyy", null);
//        externalhire.HRRFId = Convert.ToInt32(Session["hrrfId"]);
//        externalhire.HRRFNumber = Session["hrrfno"].ToString();
//        externalhire.RecruiterName = (Request.Form["RecruiterName"] == null) ? "" : Request.Form["RecruiterName"].ToString();
//        db.ExternalHires.Add(externalhire);

//        //Below code is added by Sarath on 05 Mar 2016 
//        //Reason: Update ModifiedBy and ModifiedDate columns in the ExternalFulFillment table.
//        if (Session["EmployeeId"] != null)
//        {
//            ExternalFulfillment externalFulFillmentHire = db.ExternalFulfillments.Where(eff => eff.HRRFNumber.Equals(externalhire.HRRFNumber)).FirstOrDefault<ExternalFulfillment>();
//            //List<string> lstRLEmp = db.RoleMasters.Where(r => r.Role == "RL").Select(r => r.EmployeeId.ToString()).ToList();
//            //List<string> lstRLActiveEmp = db.Employees.Where(e => lstRLEmp.Contains(e.EmployeeId.ToString()) && e.IsActive == true).Select(emp => emp.EmployeeId.ToString()).ToList();
//            //ViewData["_RecruiterName"] = lstRLActiveEmp.OrderBy(ord => ord).ToList();

//            if (externalFulFillmentHire != null)
//            {
//                externalFulFillmentHire.ModifiedBy = Convert.ToInt32(Session["EmployeeId"]);
//                externalFulFillmentHire.ModifiedDate = DateTime.UtcNow;

//                db.Entry(externalFulFillmentHire).State = System.Data.Entity.EntityState.Modified;
//            }
//        }


//        string strLocationType = "";
//        HRRF objHRRF = db.HRRFs.Where(h => h.HRRFNumber.Equals(externalhire.HRRFNumber)).FirstOrDefault();

//        if (objHRRF != null)
//        {
//            strLocationType = (objHRRF.LocationType == "1") ? "ONSITE" : "OFFSHORE";

//            if (externalhire.RequestStatus == "Fulfilled")
//            {
//                objHRRF.ResourceName = externalhire.Name;
//                objHRRF.RequestStatus = externalhire.RequestStatus;

//                //db.HRRFs.Add(objHRRF);
//            }
//            else
//            {
//                objHRRF.RequestStatus = externalhire.RequestStatus;
//            }

//            db.Entry(objHRRF).State = System.Data.Entity.EntityState.Modified;
//        }

//        db.SaveChanges();

//        string strDesc = "";
//        if (!string.IsNullOrWhiteSpace(externalhire.RecruiterName.Trim()))
//        {
//            Employee RecruiterDetails = db.Employees.Find(Convert.ToInt32(externalhire.RecruiterName));
//            if (RecruiterDetails != null)
//            {
//                strDesc = "assigned to '" + RecruiterDetails.FirstName + " " + RecruiterDetails.LastName + "' and ";
//            }

//            //strDescription = externalhire.HRRFNumber + " - " + "has been assigned to '" + strRecruiterName + "' and changed to '" + externalhire.RequestStatus + "' status";
//        }

//        if (!string.IsNullOrWhiteSpace(externalhire.RequestStatus))
//        {
//            string strDescription = externalhire.HRRFNumber + " - " + "has been " + strDesc + "changed to '" + externalhire.RequestStatus + "' status";
//            string strRemarks = externalhire.FulfilmentRemarks.Trim();

//            switch (externalhire.RequestStatus.ToLower())
//            {
//                //case "fulfilled":
//                //    strRemarks = externalhire.FulfilmentRemarks.Trim();
//                //    break;
//                case "cancelled":
//                    strDescription += " on " + externalhire.CancelDate;
//                    strRemarks = externalhire.CancellationReason.Trim();
//                    break;
//            }

//            // Capturing TR History
//            TRScernarioHistroy(externalhire.HRRFNumber, strDescription, strRemarks, Convert.ToInt32(Session["EmployeeId"]), objHRRF.RequestStatus);
//        }

//        if (Session["EmployeeId"] != null)
//        {
//            Employee objEmp = db.Employees.Find(Session["EmployeeId"]);
//            string strStatusChangedBy = "";
//            if (objEmp != null)
//            {
//                strStatusChangedBy = objEmp.FirstName.Trim() + (!string.IsNullOrWhiteSpace(objEmp.MiddleName) ? " " + objEmp.MiddleName + " " : " ") + objEmp.LastName.Trim();
//                List<string> lstRecipients = new List<string>();

//                ConfigurationManager.AppSettings["ITS-OPS"].Split(',').ToList<string>().ForEach(Ops => lstRecipients.Add(Ops)); // mail to ITS-OPS
//                //db.RoleMasters.Where(r => r.Role == "RL").Select(r => r.EmployeeId.ToString()).ToList<string>().ForEach(RLs => lstRecipients.Add(RLs)); // mail to all RLs

//                List<string> lstRLEmp = db.RoleMasters.Where(r => r.Role == "RL").Select(r => r.EmployeeId.ToString()).ToList();
//                List<string> lstRLActiveEmp = db.Employees.Where(e => lstRLEmp.Contains(e.EmployeeId.ToString()) && e.IsActive == true && e.LocationType == strLocationType).Select(emp => emp.EmployeeId.ToString()).ToList();

//                lstRLActiveEmp.ForEach(RLs => lstRecipients.Add(RLs)); // mail to all active RLs

//                if (externalhire.RequestStatus.ToLower() == "fulfilled" || externalhire.RequestStatus.ToLower() == "cancelled")
//                {
//                    lstRecipients.Add(objHRRF.HRRFCreatedBy.ToString()); // mail to TR Created By
//                }

//                string strNotificationMessage = ConfigurationManager.AppSettings["ChangeStatusByRL"].Replace("#TRXXXX", externalhire.HRRFNumber) + "'" + externalhire.RequestStatus + "' by " + strStatusChangedBy + ".";

//                foreach (string recipient in lstRecipients.Distinct())
//                {
//                    InsertNotification(Convert.ToString(Session["EmployeeId"]), recipient, strNotificationMessage, ConfigurationManager.AppSettings["TrProposedExternalRes_NOtification"], externalhire.HRRFNumber, string.Empty);
//                }
//            }
//        }

//        return RedirectToAction("Index", "ExternalReport");
//    }
//    catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
//    {
//        Exception raise = dbEx;
//        foreach (var validationErrors in dbEx.EntityValidationErrors)
//        {
//            foreach (var validationError in validationErrors.ValidationErrors)
//            {
//                string message = string.Format("{0}:{1}",
//                    validationErrors.Entry.Entity.ToString(),
//                    validationError.ErrorMessage);
//                // raise a new exception nesting
//                // the current instance as InnerException
//                raise = new InvalidOperationException(message, raise);
//            }
//        }
//        ErrorHandling expcls = new ErrorHandling();
//        expcls.Error(raise);
//        throw raise;
//    }

//}
//public ActionResult UpdateExternalHire(string ResourceName, string RecruiterName, string EmployeeId, string RequestStatus, int ExternalHireID, string FulfilmentRemarks, string Doj, string FulfilmentDate, string CancelReason, string dtCancel)
//{
//    JsonResult result = new JsonResult();
//    result.Data = "Success";
//    result.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
//    string HRRFNumber = string.Empty;
//    string To = string.Empty;
//    int EmpId;
//    try
//    {
//        //var externalhireUpdate = (from exthire in db.ExternalHires where exthire.ExternalHireId == ExternalHireID select exthire).Single();
//        //HRRFNumber = (from exthire in db.ExternalHires where exthire.ExternalHireId == ExternalHireID select exthire.HRRFNumber).Single();

//        ExternalHire externalhireUpdate = db.ExternalHires.Find(ExternalHireID);
//        HRRFNumber = externalhireUpdate.HRRFNumber;

//        bool flag = false;
//        if (externalhireUpdate.RecruiterName != RecruiterName)
//        {
//            flag = true;
//        }

//        string PrevRequestStatus = externalhireUpdate.RequestStatus;

//        string name = User.Identity.Name.Split('\\')[1].ToLower();
//        EmpId = Convert.ToInt32((from data in db.Employees where (data.UserName.Contains(name)) select data.EmployeeId).FirstOrDefault());
//        To = Convert.ToString((from Toid in db.Employees where Toid.EmployeeId == EmpId select Toid.SupervisorId).Single());

//        externalhireUpdate.Name = ResourceName;
//        if (!string.IsNullOrWhiteSpace(EmployeeId))
//        {
//            externalhireUpdate.EmployeeId = Convert.ToInt32(EmployeeId);
//        }
//        //externalhireUpdate.EmployeeId 
//        externalhireUpdate.RequestStatus = RequestStatus;
//        externalhireUpdate.RecruiterName = RecruiterName;
//        externalhireUpdate.FulfilmentRemarks = FulfilmentRemarks;
//        externalhireUpdate.DOJ = string.IsNullOrEmpty(Doj) ? new DateTime?() : DateTime.ParseExact(Doj, "MM/dd/yyyy", null);
//        externalhireUpdate.FulfilmentDate = string.IsNullOrEmpty(FulfilmentDate) ? new DateTime?() : Convert.ToDateTime(FulfilmentDate, CultureInfo.GetCultureInfo("en-US"));
//        //externalhireUpdate.DOJ = DateTime.ParseExact(Doj,"MM/dd/yyyy",CultureInfo.InvariantCulture); 
//        //externalhireUpdate.FulfilmentDate = DateTime.ParseExact(FulfilmentDate, "MM/dd/yyyy", CultureInfo.InvariantCulture);
//        externalhireUpdate.CancellationReason = CancelReason;
//        externalhireUpdate.CancelDate = string.IsNullOrEmpty(dtCancel) ? new DateTime?() : DateTime.ParseExact(dtCancel, "MM/dd/yyyy", null);

//        db.Entry(externalhireUpdate).State = System.Data.Entity.EntityState.Modified;
//        //db.SaveChanges();

//        string strLocationType = "";
//        HRRF objHRRF = db.HRRFs.Where(h => h.HRRFNumber.Equals(HRRFNumber)).FirstOrDefault();

//        if (objHRRF != null)
//        {
//            strLocationType = (objHRRF.LocationType == "1") ? "ONSITE" : "OFFSHORE";

//            if (RequestStatus == "Fulfilled")
//            {
//                objHRRF.ResourceName = ResourceName;
//            }

//            objHRRF.RequestStatus = RequestStatus;

//            db.Entry(objHRRF).State = System.Data.Entity.EntityState.Modified;
//            db.SaveChanges();
//        }

//        string strDesc = "";
//        if (flag)
//        {
//            Employee RecruiterDetails = db.Employees.Find(Convert.ToInt32(RecruiterName));
//            if (RecruiterDetails != null)
//            {
//                strDesc = "assigned to '" + RecruiterDetails.FirstName + " " + RecruiterDetails.LastName + "' and ";
//            }
//        }

//        if (!string.IsNullOrWhiteSpace(RequestStatus))
//        {
//            string strDescription = HRRFNumber + " - " + "has been " + strDesc + "changed to '" + RequestStatus + "' status";

//            string strRemarks = FulfilmentRemarks.Trim();

//            switch (RequestStatus.ToLower())
//            {
//                //case "fulfilled":
//                //    strRemarks = FulfilmentRemarks.Trim();
//                //    break;
//                case "cancelled":
//                    strDescription += " on " + Convert.ToDateTime(externalhireUpdate.CancelDate).ToString("dd-MMM-yyyy");
//                    strRemarks = CancelReason.Trim();
//                    break;
//            }

//            // Capturing TR History
//            TRScernarioHistroy(HRRFNumber, strDescription, strRemarks, Convert.ToInt32(Session["EmployeeId"]), PrevRequestStatus);
//        }

//        if (Session["EmployeeId"] != null)
//        {
//            Employee objEmp = db.Employees.Find(Session["EmployeeId"]);
//            string strStatusChangedBy = "";
//            if (objEmp != null)
//            {
//                strStatusChangedBy = objEmp.FirstName.Trim() + (!string.IsNullOrWhiteSpace(objEmp.MiddleName) ? " " + objEmp.MiddleName + " " : " ") + objEmp.LastName.Trim();
//                List<string> lstRecipients = new List<string>();

//                ConfigurationManager.AppSettings["ITS-OPS"].Split(',').ToList<string>().ForEach(Ops => lstRecipients.Add(Ops)); // mail to ITS-OPS
//                //db.RoleMasters.Where(r => r.Role == "RL").Select(r => r.EmployeeId.ToString()).ToList<string>().ForEach(RLs => lstRecipients.Add(RLs)); // mail to all RLs
//                List<string> lstRLEmp = db.RoleMasters.Where(r => r.Role == "RL").Select(r => r.EmployeeId.ToString()).ToList();
//                List<string> lstRLActiveEmp = db.Employees.Where(e => lstRLEmp.Contains(e.EmployeeId.ToString()) && e.IsActive == true && e.LocationType == strLocationType).Select(emp => emp.EmployeeId.ToString()).ToList();

//                lstRLActiveEmp.ForEach(RLs => lstRecipients.Add(RLs)); // mail to all active RLs

//                if (externalhireUpdate.RequestStatus.ToLower() == "fulfilled" || externalhireUpdate.RequestStatus.ToLower() == "cancelled")
//                {
//                    lstRecipients.Add(objHRRF.HRRFCreatedBy.ToString()); // mail to TR Created By
//                }

//                string strNotificationMessage = ConfigurationManager.AppSettings["ChangeStatusByRL"].Replace("#TRXXXX", externalhireUpdate.HRRFNumber) + "'" + externalhireUpdate.RequestStatus + "' by " + strStatusChangedBy + ".";

//                foreach (string recipient in lstRecipients.Distinct())
//                {
//                    InsertNotification(Convert.ToString(Session["EmployeeId"]), recipient, strNotificationMessage, ConfigurationManager.AppSettings["TrProposedExternalRes_NOtification"], externalhireUpdate.HRRFNumber, string.Empty);
//                }
//            }
//        }
//    }
//    catch (Exception ex)
//    {
//        //some code to handle error
//        ErrorHandling expcls = new ErrorHandling();
//        expcls.Error(ex);
//        return null;
//    }
//    return result;
//}

#endregion

/// <summary>
/// Insert Notifications
/// </summary>
/// <param name="From"></param>
/// <param name="To"></param>
/// <param name="NotificationMsg"></param>
/// <param name="NOtificationType"></param>
/// <param name="HrrfNumber"></param>
/// <param name="Remarks"></param>
/// 
private void InsertNotification(string From, string To, string NotificationMsg, string NOtificationType, string HrrfNumber, string Remarks)
        {
            var hrrfResultQualify = db.HRRFs.Where(h => h.HRRFNumber == HrrfNumber).ToList().FirstOrDefault();
            Notification tblNotification = new Notification();
            tblNotification.NotificationType = NOtificationType;
            tblNotification.NotificationDate = System.DateTime.UtcNow;
            tblNotification.NotificationFrom = Convert.ToInt32(From);
            tblNotification.NotificationTo = Convert.ToInt32(To);
            var Body = NotificationMsg + "<br/> <b> Job Description: </b>" + hrrfResultQualify.JobDescription + "<br/><br/>" + MailingContent(HrrfNumber); // + ConfigurationManager.AppSettings["TRNUMber"].ToString() + HrrfNumber + Remarks;
            var Body1 = NotificationMsg + "<br/> <b> First Level Tech Panel: </b>" + hrrfResultQualify.TECHPANEL + "<br/><br/>" + "<br/> <b> Second Level Tech Panel: </b>" + hrrfResultQualify.SECONDTECHPANEL + "<br/><br/>" + MailingContent(HrrfNumber); // + ConfigurationManager.AppSettings["TRNUMber"].ToString() + HrrfNumber + Remarks;
            tblNotification.IsActive = true;
            tblNotification.AssetID = HrrfNumber;
            tblNotification.ApplicationCode = ConfigurationManager.AppSettings["AppCodeTR"].ToString();
            tblNotification.IsFromGroupID = true;
            tblNotification.FromGroupID = "donotreply@trianz.com";

            string talentoURL = ConfigurationManager.AppSettings["Talento"];
            string body = string.Empty;
            string LinktoOpen = "Please Click on link to View the details of Submitted Talent Request number #" + HrrfNumber + " : " + talentoURL;
            using (StreamReader reader = new StreamReader(Common.DownloadFromS3Stream("Views/Shared/EmailTemplate.html")))
            {
                body = reader.ReadToEnd();
            }
            body = body.Replace("{ToUserName}", db.Employees.Where(e => e.EmployeeId == tblNotification.NotificationTo).Select(s => s.FirstName + " " + s.LastName).FirstOrDefault());
            // body = body.Replace("{FromUserName}", db.Employees.Where(e => e.EmployeeId == tblNotification.NotificationFrom).Select(s => s.FirstName + " " + s.LastName).FirstOrDefault());
            body = body.Replace("{FromUserName}", "Talento Team");
            body = body.Replace("{Description}", Body);
            body = body.Replace("{LinktoOpen}", LinktoOpen);

            tblNotification.NotificationMessage = body;

            string IsEmailSent = System.Configuration.ConfigurationManager.AppSettings["IsEmailSent"].ToString();
            bool IsEmail = Convert.ToBoolean(IsEmailSent);
            if (IsEmail == true)
            {
                db.Notifications.Add(tblNotification);

            }
            db.SaveChanges();
        }

        public string MailingContent(String hrrfnumber)
        {
            try
            {
                string mailcon = "";
                if (!string.IsNullOrEmpty(hrrfnumber))
                {
                    //var mstrlookup = (from skill in db.HRRFSkills
                    //                  join masterLkup in db.MasterLookUps on skill.Skills equals masterLkup.LookupID
                    //                  where skill.HRRFNumber == hrrfnumber
                    //                  select new
                    //                  {
                    //                      masterLkup.Description
                    //                  }).ToList();
                    //var SkillData = string.Join(",", mstrlookup.Select(s => s.Description).ToArray());
                    var skilmast_value = (from skill in db.HRRFSkills_ExpertiseLevel
                                          join skilmast in db.SkillMasters on skill.SkillId equals skilmast.SkillId
                                          where skill.HRRFNumber == hrrfnumber
                                          select new
                                          {
                                              skilmast.Skillset
                                          }).ToList();
                    var SkillData = string.Join(",", skilmast_value.Select(s => s.Skillset).ToArray());

                    var objHRRF = db.HRRFs.Where(p => p.HRRFNumber == hrrfnumber).FirstOrDefault();
                    var projectDetails = db.Projects.Where(a => a.ProjectName == objHRRF.ProjectName).FirstOrDefault();

                    string contractValue = "";
                    if (objHRRF.IsContracting == true)
                    {
                        contractValue = "Yes";
                    }
                    else if (objHRRF.IsContracting == false)
                    {
                        contractValue = "No";
                    }
                    string startdate = Convert.ToDateTime(objHRRF.AssignmentStartDate).ToString("dd MMM yyyy");
                    string enddate = Convert.ToDateTime(objHRRF.AssignmentEndDate).ToString("dd MMM yyyy");
                    string billingdate = Convert.ToDateTime(objHRRF.BillingDate).ToString("dd MMM yyyy");

                    mailcon += "<table border = '1' style = 'color: #2b4d88; font-size: 12px;'><tr><th>Account Detials</th><th>Assignment Detials</th><th>Request Detials</th></tr>";
                    mailcon += "<tr><td><b>Practice: </b>" + objHRRF.Practice + "</td><td><b>Start date: </b>"
                        + startdate + "</td><td><b>Role: </b>" + objHRRF.RoleRequired + "</td></tr>";

                    mailcon += "<tr><td><b>Purpose: </b>" + objHRRF.Purpose + "</td><td><b>End Date: </b>"
                         + enddate + "</td><td><b>Grade: </b>" + objHRRF.Grade + "</td></tr>";

                    mailcon += "<tr><td><b>AccountName: </b>" + objHRRF.AccountName + "</td><td><b>Billing Date: </b>"
                          + billingdate + "</td><td><b>Experience: </b>" + objHRRF.ExpFrom + "</td></tr>";

                    mailcon += "<tr><td><b>Request Type: </b>" + objHRRF.RequestType + "</td><td><b>Location: </b>"
                          + objHRRF.LocationName + "</td><td><b>No.Of Position: </b>" + objHRRF.Positions + "</td></tr>";

                    mailcon += "<tr><td><b>Engagement Type: </b>" + objHRRF.EnagagementType + "</td><td><b>Client Interview: </b>"
                         + objHRRF.ClientInterview + "</td><td><b>Request Reason: </b>" + objHRRF.RequestReason + "</td></tr>";

                    mailcon += "<tr><td><b>Cost Center: </b>" + objHRRF.CostCenter + "</td><td><b>Skills: </b>"
                         + SkillData + "</td><td><b>Is Contracting: </b>" + contractValue + "</td></tr>";

                    mailcon += "<tr><td><b>Project Manager: </b>" + projectDetails.ProjectManager + "</td><td><b>Delivery Manager: </b>"
                        + projectDetails.DeliveryManager + "</td></tr></table>";

                    return mailcon;
                }
                else
                {
                    return mailcon;
                }
            }
            catch (Exception ex)
            {
                ErrorHandling expcls = new ErrorHandling();
                expcls.Error(ex);
                return null;
            }
        }
        public void TRScernarioHistroy(string HRRFNumber, string Description, string Remarks, int EmpIdForCreatedBy, string PrevRequestStatus)
        {
            try
            {
                var HrrfHistory = new HRRFHistory();
                HrrfHistory.HRRFNumber = HRRFNumber;
                HrrfHistory.HistoryDescription = Description;
                HrrfHistory.ModifiedBy = EmpIdForCreatedBy;
                HrrfHistory.Remarks = Remarks;
                HrrfHistory.PrevRequestStatus = PrevRequestStatus;
                HrrfHistory.ModifiedDate = DateTime.UtcNow;
                db.HRRFHistories.Add(HrrfHistory);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                ErrorHandling expcls = new ErrorHandling();
                expcls.Error(ex);
            }
        }

        public ActionResult GetActiveRLsBasedonLocation(string HRRFNumber)
        {
            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            {
                try { 
                HRRF objHRRF = db.HRRFs.Where(h => h.HRRFNumber == HRRFNumber).FirstOrDefault();

            if (objHRRF != null)
            {
                string strLocationType = (objHRRF.LocationType == "1") ? "ONSITE" : "OFFSHORE";

                List<string> lstRLEmp = db.RoleMasters.Where(r => r.Role == "RL" && r.ApplicationCode == "TALENTREQ").Select(r => r.EmployeeId.ToString()).ToList();
                List<string> lstRLActiveEmp = db.Employees
                                                .Where(e => lstRLEmp.Contains(e.EmployeeId.ToString()) && e.IsActive == true && e.LocationType == strLocationType)
                                                .Select(emp => emp.EmployeeId.ToString()).ToList();

                List<SelectListItem> lstRLEmployees = (from dt in db.Employees.
                                                        Where(n => lstRLActiveEmp.Contains(n.EmployeeId.ToString()))
                                                       select new SelectListItem
                                                       {
                                                           Text = dt.FirstName + " " + dt.LastName,
                                                           Value = dt.EmployeeId.ToString()
                                                       }
                                                      ).ToList();

                return Json(lstRLEmployees, JsonRequestBehavior.AllowGet);
            }

            return Json("", JsonRequestBehavior.AllowGet);
            }
                catch (Exception ex)
            {

                Common.WriteExceptionErrorLog(ex);
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
        public ActionResult ValidateHRRFNo(string duplicateHrrfNo)
        {
            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            {

                try
                {
                var result = db.HRRFs.Where(p => p.HRRFNumber == duplicateHrrfNo).Select(p => p.HRRFNumber).FirstOrDefault();
                if (result == null)
                {
                    return Json("false", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("true", JsonRequestBehavior.AllowGet);
                }

            }
                //catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
                //{
                //    Exception raise = dbEx;
                //    foreach (var validationErrors in dbEx.EntityValidationErrors)
                //    {
                //        foreach (var validationError in validationErrors.ValidationErrors)
                //        {
                //            string message = string.Format("{0}:{1}",
                //                validationErrors.Entry.Entity.ToString(),
                //                validationError.ErrorMessage);
                //            // raise a new exception nesting
                //            // the current instance as InnerException
                //            raise = new InvalidOperationException(message, raise);
                //        }
                //    }
                //    ErrorHandling expcls = new ErrorHandling();
                //    expcls.Error(raise);
                //    throw raise;
                //}
                catch (Exception ex)
                {

                    Common.WriteExceptionErrorLog(ex);
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
        public JsonResult GetEmployeeDateofJoin(string EmployeeID)
        {
            try
            {
                JsonResult js = new JsonResult();

                //int EMpid = Convert.ToInt32(EmployeeID);

                Char value = '-';
                Boolean result = EmployeeID.Contains(value);
                if (result)
                {
                    EmployeeID = EmployeeID.Substring(0, EmployeeID.IndexOf("-"));   // for getting full with empid and name

                }
                int EMpid = Convert.ToInt32(EmployeeID);

                EmployeeDoJ jst = new EmployeeDoJ();

                var emply = db.Employees.Where(p => p.EmployeeId == EMpid).FirstOrDefault();
                if (emply != null)
                {
                    jst.EmployeeID = emply.EmployeeId;
                    jst.DOJ = emply.DateOfJoin;
                    jst.Message = "Data Exists";
                }
                else
                {
                    jst.EmployeeID = 0;
                    jst.DOJ = DateTime.UtcNow;
                    jst.Message = "Employee ID not exists";
                }
                js.Data = jst;
                js.JsonRequestBehavior = JsonRequestBehavior.AllowGet;

                return js;
            }
            catch (Exception ex)
            {

                Common.WriteExceptionErrorLog(ex);
                return Json("Error", JsonRequestBehavior.AllowGet);
            }
        }

        public void GetExternalExcel(string flag)
        {
            try
            {
                var HrrfDetailsExport = new List<HRRF>();

                //if (flag != null && flag.ToLower() == "external")
                //{
                //    HrrfDetailsExport = db.HRRFs.Where(h => h.RequestType.ToLower() == "external" && h.RequestStatus.ToLower() != "fulfilled").ToList();
                //}
                List<HRRFExportToExcel> exportToExcel = new List<HRRFExportToExcel>();
                exportToExcel = db.Database.SqlQuery<HRRFExportToExcel>
                      ("exec Sp_GetExternalFulfillmentDetails").ToList<HRRFExportToExcel>();
                //foreach (var item in HrrfDetailsExport)
                //{
                //    HRRFExportToExcel objHRRFExportToExcel = new HRRFExportToExcel();

                //    objHRRFExportToExcel.HRRFID = item.HRRFID;
                //    objHRRFExportToExcel.HRRFNumber = item.HRRFNumber;
                //    objHRRFExportToExcel.OldHRRFNumber = item.OldHRRFNumber;
                //    objHRRFExportToExcel.CreatedDate = Convert.ToDateTime(item.HRRFCreatedDate);
                //    objHRRFExportToExcel.Ageing = (item.HRRFCreatedDate == null) ? 0 : ((DateTime.UtcNow.Date - Convert.ToDateTime(Convert.ToDateTime(item.HRRFCreatedDate).ToString("dd MMM yyyy"))).Days);
                //    objHRRFExportToExcel.AgeingBucket = (objHRRFExportToExcel.Ageing < 30) ? "0 - 29 D" : (objHRRFExportToExcel.Ageing < 60) ? "30 - 59 D" : (objHRRFExportToExcel.Ageing < 90) ? "60 - 89 D" : ">=90 D";
                //    objHRRFExportToExcel.Purpose = item.Purpose;
                //    objHRRFExportToExcel.ProjectNameWithCode = (item.Purpose.ToLower() == "proactive hire" || item.Purpose.ToLower() == "opportunity") ? "" : item.ProjectName + " --- " + item.ProjectCode;
                //    objHRRFExportToExcel.OpportunityCode = item.OpportunityCode;
                //    objHRRFExportToExcel.OpportunityName = item.OpportunityName;
                //    objHRRFExportToExcel.AccountName = item.AccountName;
                //    objHRRFExportToExcel.ServiceLine = item.Practice;

                //    objHRRFExportToExcel.RequestType = item.RequestType;

                //    objHRRFExportToExcel.JobDescription = item.JobDescription;
                //    objHRRFExportToExcel.Grade = (item.Grade == null) ? 0 : Convert.ToInt32(item.Grade);
                //    objHRRFExportToExcel.RequestStatus = item.RequestStatus;
                //    objHRRFExportToExcel.LocationName = item.LocationName;
                //    objHRRFExportToExcel.Positions = item.Positions;
                //    objHRRFExportToExcel.ResourceName = item.ResourceName;
                //    if (item.AssignmentStartDate != null)
                //    {
                //        objHRRFExportToExcel.AssignmentStartDate = Convert.ToDateTime(item.AssignmentStartDate);
                //        if (item.RequestStatus.ToLower() != "cancelled" && item.RequestStatus.ToLower() != "draft"
                //            && item.RequestStatus.ToLower() != "terminated" && item.RequestStatus.ToLower() != "fulfilled")
                //        {
                //            DateTime dtAssignmentStartDate = Convert.ToDateTime(item.AssignmentStartDate);
                //            DateTime dtCurrentDate = DateTime.UtcNow.Date;

                //            int result = DateTime.Compare(dtCurrentDate, dtAssignmentStartDate);

                //            if (result > 0)
                //            {
                //                objHRRFExportToExcel.OverDue = (dtCurrentDate - dtAssignmentStartDate).Days.ToString() + " day(s)";
                //            }
                //            else
                //            {
                //                objHRRFExportToExcel.OverDue = "0 day(s)";
                //            }
                //        }
                //    }
                //    else
                //    {
                //        objHRRFExportToExcel.OverDue = "0 days(s)";
                //    }
                //    objHRRFExportToExcel.DemandType = item.DemandType;
                //    objHRRFExportToExcel.RequestReason = item.RequestReason;

                //    bool isNumeric = false;
                //    if (item.LocationType != null)
                //    {
                //        int n;
                //        isNumeric = int.TryParse(item.LocationType, out n);

                //        if (isNumeric)
                //        {
                //            int LocationTypeID = Convert.ToInt32(item.LocationType);
                //            if(LocationTypeID == 1)
                //            {
                //                objHRRFExportToExcel.LocationType = "Onsite";
                //            }
                //            else if (LocationTypeID == 2)
                //            {
                //                objHRRFExportToExcel.LocationType = "OffShore";
                //            }
                //            else
                //            {
                //                objHRRFExportToExcel.LocationType = "";
                //            }
                //           // objHRRFExportToExcel.LocationType = db.MasterLookUps.Where(ml => ml.LookupType.Equals("LocationType") && ml.SeqNumber.Equals(LocationTypeID)).FirstOrDefault().LookupName;
                //        }
                //        else
                //        {
                //            objHRRFExportToExcel.LocationType = item.LocationType;
                //        }
                //    }
                //    DesignationMaster objDesignationMaster = db.DesignationMasters.Where(dm => dm.Practice.Contains(item.Practice) && dm.Grade == item.Grade && dm.DesignationCode == item.RoleRequired).FirstOrDefault();
                //    if (objDesignationMaster != null)
                //    {
                //        objHRRFExportToExcel.RoleRequired = objDesignationMaster.DesignationName;
                //    }
                //    else
                //    {
                //        objHRRFExportToExcel.RoleRequired = item.RoleRequired;
                //    }
                //    objHRRFExportToExcel.CostCenter = item.CostCenter;
                //    Int32 empID = Convert.ToInt32(item.HRRFCreatedBy);
                //    Employee emp = db.Employees.Where(e => e.EmployeeId.Equals(empID)).FirstOrDefault();

                //    if (emp != null)
                //    {

                //        objHRRFExportToExcel.HRRFCreatedById = item.HRRFCreatedBy;
                //        objHRRFExportToExcel.HRRFCreatedByName = emp.FirstName + " " + emp.LastName;
                //    }
                //    else
                //    {
                //        objHRRFExportToExcel.HRRFCreatedById = 0;
                //        objHRRFExportToExcel.HRRFCreatedByName = "";
                //    }

                //    var strPrimarySkills = (from skills in db.HRRFSkills
                //                            join mLookup in db.MasterLookUps on skills.Skills equals mLookup.LookupID
                //                            where mLookup.LookupType == "PrimarySkills" && skills.HRRFNumber == item.HRRFNumber && skills.IsPrimary == true
                //                            select new
                //                            {
                //                                mLookup.ParentCode,
                //                                mLookup.LookupName
                //                            }).ToList();

                //    if (strPrimarySkills.Count > 0)
                //    {
                //        Int32 seqNum = Convert.ToInt32(strPrimarySkills[0].ParentCode);
                //        var querySkillCategory = db.MasterLookUps.Where(m => m.SeqNumber.Equals(seqNum) && m.LookupType.Equals("SkillCategory")).FirstOrDefault();

                //        objHRRFExportToExcel.SkillCategory = querySkillCategory.LookupName;
                //    }
                //    else
                //    {
                //        objHRRFExportToExcel.SkillCategory = "";
                //    }

                //    foreach (var primary in strPrimarySkills)
                //    {
                //        objHRRFExportToExcel.PrimarySkillSet += primary.LookupName + "; ";
                //    }

                //    if (objHRRFExportToExcel.PrimarySkillSet != null)
                //    {
                //        objHRRFExportToExcel.PrimarySkillSet = objHRRFExportToExcel.PrimarySkillSet.TrimEnd(new char[] { ' ', ';' });
                //    }
                //    else
                //    {
                //        objHRRFExportToExcel.PrimarySkillSet = "";
                //    }

                //    ExternalHire objExternalHire = db.ExternalHires.Where(eh => eh.HRRFNumber.Equals(item.HRRFNumber)).OrderByDescending(eh => eh.ExternalHireId).FirstOrDefault();
                //    if (objExternalHire != null)
                //    {
                //        if (objExternalHire.FulfilmentDate != null)
                //        {
                //            objHRRFExportToExcel.ExpectedFulfilmentDate = Convert.ToDateTime(objExternalHire.FulfilmentDate);
                //        }
                //        //objHRRFExportToExcel.ExpectedFulfilmentDate = (objExternalHire.FulfilmentDate != null) ? Convert.ToDateTime(objExternalHire.FulfilmentDate).ToString("dd/MM/yyyy") : "";
                //        objHRRFExportToExcel.FulfillmentRemarks = objExternalHire.FulfilmentRemarks;
                //        if (objExternalHire.DOJ != null)
                //        {
                //            objHRRFExportToExcel.DOJ = Convert.ToDateTime(objExternalHire.DOJ);
                //        }

                //        // objHRRFExportToExcel.DOJ = (objExternalHire.DOJ != null) ? Convert.ToDateTime(objExternalHire.DOJ).ToString("dd-MMM-yyyy") : "";
                //        objHRRFExportToExcel.JoiningMonth = (objExternalHire.DOJ != null) ? System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Convert.ToDateTime(objExternalHire.DOJ).Month) : "";
                //        if (objExternalHire.CancelDate != null)
                //        {
                //            objHRRFExportToExcel.CancelDate = Convert.ToDateTime(objExternalHire.CancelDate);
                //        }

                //        objHRRFExportToExcel.CancelRemarks = objExternalHire.CancellationReason;

                //        if (item.RequestStatus != null)
                //        {
                //            if (item.RequestType.ToLower().Contains("external") && item.RequestStatus.ToLower() == "fulfilled")
                //            {
                //                if (objExternalHire.RequestStatus != null)
                //                {
                //                    if (objExternalHire.RequestStatus.ToLower() == "fulfilled")
                //                    {
                //                        if (objExternalHire.DOJ != null)
                //                        {
                //                            objHRRFExportToExcel.FulfillmentDate = Convert.ToDateTime(objExternalHire.DOJ);
                //                        }
                //                        objHRRFExportToExcel.EmployeeId = objExternalHire.EmployeeId.ToString();
                //                    }
                //                }
                //            }
                //        }

                //        if (!string.IsNullOrWhiteSpace(objExternalHire.RecruiterName))
                //        {
                //            Employee objRecruiterDetails = db.Employees.Find(Convert.ToInt32(objExternalHire.RecruiterName));
                //            if (objRecruiterDetails != null)
                //            {
                //                objHRRFExportToExcel.RecruiterName = objRecruiterDetails.FirstName + " " + objRecruiterDetails.LastName;
                //            }
                //        }
                //    }

                //    List<HRRFHistory> objHRRFHistory = db.HRRFHistories.Where(eh => eh.HRRFNumber.Equals(item.HRRFNumber)).ToList();
                //    if (objHRRFHistory != null)
                //    {
                //        HRRFHistory history1 = objHRRFHistory.Where(ie => ie.HistoryDescription.ToLower().Equals(item.HRRFNumber.ToLower() + " - has been converted to external")).FirstOrDefault();
                //        if (history1 != null && history1.ModifiedDate != null)
                //        {
                //            objHRRFExportToExcel.DateFromIntToExt = Convert.ToDateTime(history1.ModifiedDate);
                //        }

                //        HRRFHistory history2 = objHRRFHistory.Where(ie => ie.HistoryDescription.ToLower().Equals(item.HRRFNumber.ToLower() + " - has been converted to hold")).FirstOrDefault();
                //        if (history2 != null && history2.ModifiedDate != null)
                //        {
                //            objHRRFExportToExcel.DateOfHold = Convert.ToDateTime(history2.ModifiedDate);
                //        }
                //    }
                //    else
                //    {
                //        objHRRFExportToExcel.DateFromIntToExt = null;
                //        objHRRFExportToExcel.DateOfHold = null;
                //    }

                //    exportToExcel.Add(objHRRFExportToExcel);
                //}

                // Export to excel code starts from here.
                #region Export to Excel

                using (ExcelPackage package = new ExcelPackage())
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("HRRF Information");
                    worksheet.TabColor = System.Drawing.Color.Green;
                    worksheet.DefaultRowHeight = 18f;
                    worksheet.Row(1).Height = 20f;

                    using (var range = worksheet.Cells[1, 1, 1, 40])
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
                    worksheet.Cells[1, 1].Value = "HRRF Number";
                    worksheet.Cells[1, 2].Value = "Old HRRF Number";
                    worksheet.Cells[1, 3].Value = "Created Date";
                    worksheet.Cells[1, 4].Value = "Ageing";
                    worksheet.Cells[1, 5].Value = "Ageing Bucket";
                    worksheet.Cells[1, 6].Value = "Purpose";
                    worksheet.Cells[1, 7].Value = "Project Name with Code";
                    worksheet.Cells[1, 8].Value = "Opportunity Code";
                    worksheet.Cells[1, 9].Value = "Opportunity Name";
                    worksheet.Cells[1, 10].Value = "Account Name";
                    worksheet.Cells[1, 11].Value = "Practice";
                    worksheet.Cells[1, 12].Value = "Cost Center";
                    worksheet.Cells[1, 13].Value = "Request Type";
                    worksheet.Cells[1, 14].Value = "Skill Category";
                    worksheet.Cells[1, 15].Value = "Primary Skill Set";
                    worksheet.Cells[1, 16].Value = "Job Description";
                    worksheet.Cells[1, 17].Value = "Grade";
                    worksheet.Cells[1, 18].Value = "RequestStaus";
                    worksheet.Cells[1, 19].Value = "Fulfillment Date";
                    worksheet.Cells[1, 20].Value = "Fulfillment Remarks";
                    worksheet.Cells[1, 21].Value = "Expected Fulfillment Date";
                    worksheet.Cells[1, 22].Value = "Location Type";
                    worksheet.Cells[1, 23].Value = "Location Name";
                    worksheet.Cells[1, 24].Value = "Positions";
                    worksheet.Cells[1, 26].Value = "Resource Name";
                    worksheet.Cells[1, 27].Value = "Assignment Start Date";
                    worksheet.Cells[1, 28].Value = "OverDue";
                    worksheet.Cells[1, 29].Value = "Demand Type";
                    worksheet.Cells[1, 30].Value = "Request Reason";
                    worksheet.Cells[1, 31].Value = "Role Required";
                    worksheet.Cells[1, 32].Value = "Created By EmpID";
                    worksheet.Cells[1, 33].Value = "Created By EmpName";
                    worksheet.Cells[1, 34].Value = "Date of Joining";
                    worksheet.Cells[1, 35].Value = "Joining Month";
                    worksheet.Cells[1, 36].Value = "Date from Internal to External";
                    worksheet.Cells[1, 37].Value = "Date of Hold";
                    worksheet.Cells[1, 38].Value = "Cancel Date";
                    worksheet.Cells[1, 39].Value = "Cancel Reason";
                    worksheet.Cells[1, 40].Value = "Recruiter Name";

                    //Set default column width
                    worksheet.DefaultColWidth = 18f;

                    worksheet.Column(1).Width = 13f;
                    worksheet.Column(2).AutoFit(20f);
                    worksheet.Column(7).Width = 40f;
                    worksheet.Column(8).Width = 16f;
                    worksheet.Column(9).Width = 18f;
                    worksheet.Column(10).Width = 47f;
                    worksheet.Column(14).Width = 35f;
                    worksheet.Column(15).Width = 50f;
                    worksheet.Column(16).Width = 100f;
                    worksheet.Column(20).Width = 100f;
                    worksheet.Column(26).Width = 30f;
                    worksheet.Column(27).Width = 21f;
                    worksheet.Column(29).Width = 40f;
                    worksheet.Column(31).Width = 25f;
                    worksheet.Column(33).Width = 30f;
                    worksheet.Column(36).AutoFit();
                    worksheet.Column(40).Width = 30f;

                    //Add the each row
                    for (int rowIndex = 0, row = 2; rowIndex < exportToExcel.Count; rowIndex++, row++) // row indicates number of rows
                    {
                        worksheet.Cells[row, 1].Value = exportToExcel[rowIndex].HRRFNumber;
                        worksheet.Cells[row, 2].Value = exportToExcel[rowIndex].OldHRRFNumber;

                        worksheet.Cells[row, 3].Value = exportToExcel[rowIndex].CreatedDate;
                        worksheet.Cells[row, 3].Style.Numberformat.Format = "dd-MMM-yyyy";
                        worksheet.Cells[row, 3].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                        worksheet.Cells[row, 4].Value = exportToExcel[rowIndex].Ageing;
                        worksheet.Cells[row, 5].Value = exportToExcel[rowIndex].AgeingBucket;
                        worksheet.Cells[row, 6].Value = exportToExcel[rowIndex].Purpose;
                        worksheet.Cells[row, 7].Value = exportToExcel[rowIndex].ProjectNameWithCode;
                        worksheet.Cells[row, 8].Value = exportToExcel[rowIndex].OpportunityCode;
                        worksheet.Cells[row, 9].Value = exportToExcel[rowIndex].OpportunityName;
                        worksheet.Cells[row, 10].Value = exportToExcel[rowIndex].AccountName;
                        worksheet.Cells[row, 11].Value = exportToExcel[rowIndex].ServiceLine;
                        worksheet.Cells[row, 12].Value = exportToExcel[rowIndex].CostCenter;
                        worksheet.Cells[row, 13].Value = exportToExcel[rowIndex].RequestType;
                        worksheet.Cells[row, 14].Value = exportToExcel[rowIndex].SkillCategory;
                        worksheet.Cells[row, 15].Value = exportToExcel[rowIndex].PrimarySkillSet;
                        worksheet.Cells[row, 16].Value = exportToExcel[rowIndex].JobDescription;
                        worksheet.Cells[row, 17].Value = exportToExcel[rowIndex].Grade;
                        worksheet.Cells[row, 18].Value = exportToExcel[rowIndex].RequestStatus;

                        worksheet.Cells[row, 19].Value = exportToExcel[rowIndex].FulfillmentDate;
                        worksheet.Cells[row, 19].Style.Numberformat.Format = "dd-MMM-yyyy";
                        worksheet.Cells[row, 19].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                        worksheet.Cells[row, 20].Value = exportToExcel[rowIndex].FulfillmentRemarks;

                        worksheet.Cells[row, 21].Value = exportToExcel[rowIndex].ExpectedFulfilmentDate;
                        worksheet.Cells[row, 21].Style.Numberformat.Format = "dd-MMM-yyyy";
                        worksheet.Cells[row, 21].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                        worksheet.Cells[row, 22].Value = exportToExcel[rowIndex].LocationType;
                        worksheet.Cells[row, 23].Value = exportToExcel[rowIndex].LocationName;
                        worksheet.Cells[row, 24].Value = exportToExcel[rowIndex].Positions;
                        worksheet.Cells[row, 25].Value = exportToExcel[rowIndex].EmployeeId;
                        worksheet.Cells[row, 26].Value = exportToExcel[rowIndex].ResourceName;

                        worksheet.Cells[row, 27].Value = exportToExcel[rowIndex].AssignmentStartDate;
                        worksheet.Cells[row, 27].Style.Numberformat.Format = "dd-MMM-yyyy";
                        worksheet.Cells[row, 27].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                        worksheet.Cells[row, 28].Value = exportToExcel[rowIndex].OverDue;
                        worksheet.Cells[row, 29].Value = exportToExcel[rowIndex].DemandType;
                        worksheet.Cells[row, 30].Value = exportToExcel[rowIndex].RequestReason;
                        worksheet.Cells[row, 31].Value = exportToExcel[rowIndex].RoleRequired;
                        worksheet.Cells[row, 32].Value = exportToExcel[rowIndex].HRRFCreatedById;
                        worksheet.Cells[row, 33].Value = exportToExcel[rowIndex].HRRFCreatedByName;

                        worksheet.Cells[row, 34].Value = exportToExcel[rowIndex].DOJ;
                        worksheet.Cells[row, 34].Style.Numberformat.Format = "dd-MMM-yyyy";
                        worksheet.Cells[row, 34].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                        worksheet.Cells[row, 35].Value = exportToExcel[rowIndex].JoiningMonth;

                        worksheet.Cells[row, 36].Value = exportToExcel[rowIndex].DateFromIntToExt;
                        worksheet.Cells[row, 36].Style.Numberformat.Format = "dd-MMM-yyyy";
                        worksheet.Cells[row, 36].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                        worksheet.Cells[row, 37].Value = exportToExcel[rowIndex].DateOfHold;
                        worksheet.Cells[row, 37].Style.Numberformat.Format = "dd-MMM-yyyy";
                        worksheet.Cells[row, 37].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                        worksheet.Cells[row, 38].Value = exportToExcel[rowIndex].CancelDate;
                        worksheet.Cells[row, 38].Style.Numberformat.Format = "dd-MMM-yyyy";
                        worksheet.Cells[row, 38].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                        worksheet.Cells[row, 39].Value = exportToExcel[rowIndex].CancelRemarks;
                        worksheet.Cells[row, 40].Value = exportToExcel[rowIndex].RecruiterName;

                        if (row % 2 == 1)
                        {
                            using (var range = worksheet.Cells[row, 1, row, 40])
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
                    Response.AddHeader("content-disposition", "attachment;filename=TR-Details" + DateTime.UtcNow.ToString("_dd-MMM-yyyy_hh-mm-ss") + ".xlsx");

                    Response.Charset = "";
                    Response.ContentType = "application/vnd.ms-excel";
                    StringWriter sw = new StringWriter();
                    Response.BinaryWrite(fileBytes);
                    Response.End();
                }

                #endregion

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
        public void ExporttoExcel()
        {
            try
            {
                List<HRRF> allHRRFS = new List<HRRF>();
                List<Employee> TotalEmployees = new List<Employee>();

                using (TrianzOperationsEntities dc = new TrianzOperationsEntities())
                {
                    allHRRFS = dc.HRRFs.ToList();
                }
                WebGrid grid = new WebGrid(source: allHRRFS, canPage: false, canSort: false);
                string gridData = grid.GetHtml(
                columns: grid.Columns(
                  grid.Column("HRRFNumber", "HRRF"),
                  grid.Column("JobDescription", "Demand Description"),
                  grid.Column("Practice", "Practice"),
                  grid.Column("ProjectName", "Project"),
                  grid.Column("LocationName", "Location")

                )
                ).ToString();
                Response.ClearContent();
                Response.AddHeader("content-disposition", "attachment; filename=HRRFInfo.xls");
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
    }

    public class ExtFulfillment
    {
        public int ExternalHireId { get; set; }
        public string RecruiterId { get; set; }
        public string RecruiterName { get; set; }
        public string RequestStatus { get; set; }
        public string FulfilmentRemarks { get; set; }
        public Nullable<System.DateTime> DOJ { get; set; }
        public Nullable<System.DateTime> FulfilmentDate { get; set; }
        public string CancellationReason { get; set; }
        public string HRRFNumber { get; set; }
        public Nullable<long> HRRFId { get; set; }
        public string Name { get; set; }
        public Nullable<int> EmployeeId { get; set; }
        public int? ModifiedBy { get; set; }
        public Nullable<DateTime> ModifiedDate { get; set; }
        public string Practice { get; set; }
        public Nullable<System.DateTime> CancelDate { get; set; }
    }
    public class EmployeeDoJ
    {
        public int EmployeeID { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> DOJ { get; set; }
        public string Message { get; set; }
    }
    public class hrrfdetails
    {
        public long HRRFID { get; set; }
        public string HRRFNumber { get; set; }
        public string Purpose { get; set; }
        public string ProjectName { get; set; }
        public string ProjectCode { get; set; }
        public string OpportunityName { get; set; }
        public string OpportunityCode { get; set; }
        public string DemandType { get; set; }
        public Nullable<int> Grade { get; set; }
        public string RoleRequired { get; set; }
        public string ExpFrom { get; set; }
        public string ExpTo { get; set; }
        public string EnagagementType { get; set; }
        public string ResourceName { get; set; }
        public string Domain { get; set; }
        public string JobDescription { get; set; }
        public Nullable<System.DateTime> AssignmentStartDate { get; set; }
        public Nullable<System.DateTime> AssignmentEndDate { get; set; }
        public string LocationType { get; set; }
        public string LocationName { get; set; }
        public string RequestReason { get; set; }
        public string Remarks { get; set; }
        public string RequestType { get; set; }
        public string RequestStatus { get; set; }
        public string ClientInterview { get; set; }
        public Nullable<bool> CallCompleted { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public Nullable<int> HRRFCreatedBy { get; set; }
        public Nullable<System.DateTime> HRRFCreatedDate { get; set; }
        public Nullable<System.DateTime> HRRFSubmitedDate { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public Nullable<int> Positions { get; set; }
        public string Practice { get; set; }
        public string TRParent { get; set; }
        public Nullable<bool> Isparent { get; set; }
        public string VisaType { get; set; }
        public string AccountName { get; set; }
        public string OldHRRFNumber { get; set; }
        public string CostCenter { get; set; }
        public Nullable<int> ReplacementEmpId { get; set; }
        public string ResourceType { get; set; }
        public Nullable<bool> IsContracting { get; set; }
        public string Impact { get; set; }
        public string Criticality { get; set; }
        public string Utilization { get; set; }
        public Nullable<System.DateTime> BillingDate { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> InternalExpectedFulfilmentDate { get; set; }
        public string CancelReason { get; set; }
        public string DuplicateHRRFNo { get; set; }
        public string TECHPANEL { get; set; }
        public string SECONDTECHPANEL { get; set; }
        public bool chkflag { get; set; }
    }
}