using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Xml;
using Trianz.Enterprise.Operations.Models;
using Trianz.Enterprise.Operations.General;

namespace Trianz.Enterprise.Operations.Controllers
{

    public class NotificationsController : Controller
    {
        private TrianzOperationsEntities db = new TrianzOperationsEntities();
        //ServiceAgent AgentService = new ServiceAgent();
        // GET: Notifications
        public ActionResult Mynotifications()
        {
            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            {
                try
                {
                    //Below single statement is added by Sarath, for security reason to access RoleMaster page.
                    TempData["IsRoleMasterPageAccess"] = null;

                    //Identifying prev page
                    TempData["PrevPage"] = Request.Url.Segments[Request.Url.Segments.Count() - 1];
                    int EmpID = 0;

                    //string UName = User.Identity.Name.Substring(7);
                    string usermail = Common.GetAzureLoggedInUserID();
                    var EmployeeID = (from emp in db.Employees
                                      where emp.Email == usermail && emp.IsActive == true
                                      select new
                                      {
                                          EmployeeId = emp.EmployeeId
                                      }).FirstOrDefault();
                    EmpID = Convert.ToInt32(EmployeeID.EmployeeId);

                    #region timeSheet                  
                    var Timesheets = (from tsHome in db.Timesheets
                                      join emp in db.Employees on tsHome.EmployeeId equals emp.EmployeeId
                                      where tsHome.SupervisorId == EmpID && tsHome.Status.ToLower() == "sent"
                                      select new Notifications
                                      {
                                          ApplicationCode = "TIMESHEET",
                                          FromDate = tsHome.FromDate,
                                          ToDate = tsHome.ToDate,
                                          Grade = emp.Grade.ToString(),
                                          Role = emp.Designation,
                                          JobDescrption = "",
                                          //EmployeeId = tsHome.EmployeeId,
                                          //TotalHours = tsHome.TotalHours,
                                          //EmployeeName = emp.FirstName + " " + emp.LastName,
                                          //ApprovedBy = tsHome.ApprovedBy,
                                          //ApproverName = (from s in db.Employees where s.EmployeeId == EmpID select s.FirstName + " " + s.LastName).FirstOrDefault(),// appr.FirstName + " " + appr.LastName,
                                          AssetID = tsHome.TimesheetID.ToString(),
                                          NotificationMessage = "This is to inform you that the timesheet for the period " + tsHome.FromDate + "-" + tsHome.ToDate + " has been submited, and Waiting for your Approvals."
                                          // Status = tsHome.Status
                                      }).ToList();
                    #endregion

                    #region for hrrf or trs

                    string Practice = Session["Practice"].ToString().ToLower();
                    var RoleResults = db.RoleMasters.ToList().Where(role => role.EmployeeId == EmpID && role.ApplicationCode == "TALENTREQ").ToList();

                    if (RoleResults != null && RoleResults.Count > 0)
                    {
                        //if (RoleResults.Role == "DH")
                        if (RoleResults.Any(r => r.Role == "DH"))
                        {
                            #region External TRs

                            if (Session["EmployeeId"] != null)
                            {
                                string emp = Session["EmployeeId"].ToString();
                                var Externalhrrfs = (from bpr in db.PracticeWiseBenchCodes
                                                     join EF in db.ExternalFulfillments on bpr.Practice.ToLower() equals EF.Practice.ToLower()
                                                     join H in db.HRRFs on EF.HRRFNumber equals H.HRRFNumber into tmpExterFulfill
                                                     from H in tmpExterFulfill.DefaultIfEmpty()
                                                     where (EF.ApprovalStatus == null || EF.ApprovalStatus == "")
                                                     && H.RequestStatus.ToLower() == "pending for dh approval"
                                                     && H.RequestType.ToLower() == "external"
                                                     && H.CostCenter.ToLower().Contains(bpr.CostCenter.ToLower())
                                                     && bpr.DeliveryHeadID.Contains(emp)
                                                     && bpr.TRGrade.Contains(H.Grade.ToString())
                                                     && ((H.Purpose != "Project" && H.ProjectCode == bpr.BenchCode)
                                                     || (H.Purpose == "Project") || (H.Purpose== "Opportunity"))

                                                     select new
                                                     {
                                                         HRRFNumber = H.HRRFNumber,
                                                         Grade = H.Grade,
                                                         jobdescrption = H.CLSTRJD,
                                                         Role = H.RoleRequired,
                                                         Purpose = H.Purpose,
                                                         TRProjectCode = H.ProjectCode,
                                                         ProjectName=H.ProjectName,
                                                         AccountName =H.AccountName,
                                                         SkillCode = H.SkillCode,
                                                         SkillCluster=H.SkillCluster//,										
                                                                                      // PracticebenchCode = bpr.BenchCode

                                                     }).Distinct();


                                foreach (var hrrf in Externalhrrfs)
                                {

                                    var rol = (from rl in db.DesignationMasters
                                               where rl.DesignationCode == hrrf.Role
                                               select new
                                               {
                                                   rl.DesignationName
                                               }).FirstOrDefault();
                                    string rolname = hrrf.Role;
                                    if (rol != null)
                                    {
                                        rolname = rol.DesignationName;
                                    }

                                    Notifications objnotifications = new Notifications();
                                    objnotifications.ApplicationCode = ConfigurationManager.AppSettings["AppCodeTR"].ToString();
                                    objnotifications.AssetID = hrrf.HRRFNumber;
                                    objnotifications.Grade = hrrf.Grade.ToString();
                                    objnotifications.Role = rolname;
                                    objnotifications.JobDescrption = hrrf.jobdescrption;
								    objnotifications.ProjectCode = hrrf.TRProjectCode;
                                    objnotifications.ProjectName = hrrf.ProjectName;
                                    objnotifications.AccountName = hrrf.AccountName;
                                    objnotifications.SkillCode = hrrf.SkillCode;
                                    objnotifications.SkillCluster = hrrf.SkillCluster;	
                                    objnotifications.NotificationMessage = "This is inform you that the Talent Request, which is converted to external fulfilment has been pending for your approvals.";
                                    Timesheets.Add(objnotifications);

                                }
                                #region oldcode

                                //if (Session["EmployeeId"].ToString() == "100816") // Sujit Sahoo and Practice = "SG&A" and CostCenter = "R&KM","STRATEGY & RESEARCH","CEO"
                                //{
                                //    var Externalhrrfs = (from EF in db.ExternalFulfillments
                                //                         join H in db.HRRFs on EF.HRRFNumber equals H.HRRFNumber into tmpExterFulfill
                                //                         from H in tmpExterFulfill.DefaultIfEmpty()
                                //                         where (EF.ApprovalStatus == null || EF.ApprovalStatus == "")
                                //                         && H.RequestStatus.ToLower() == "pending for dh approval"
                                //                         && H.RequestType.ToLower() == "external"
                                //                         && EF.Practice.ToLower() == "sg&a"
                                //                         && (H.CostCenter.ToLower() == "r&km" || H.CostCenter.ToLower() == "strategy & research" || H.CostCenter.ToLower() == "ceo")
                                //                         select EF);

                                //    foreach (var hrrf in Externalhrrfs)
                                //    {
                                //        Notifications objnotifications = new Notifications();

                                //        objnotifications.ApplicationCode = ConfigurationManager.AppSettings["AppCodeTR"].ToString();
                                //        objnotifications.AssetID = hrrf.HRRFNumber;
                                //        objnotifications.NotificationMessage = "This is inform you that the Talent Request, which is converted to external fulfilment has been pending for your approvals.";

                                //        Timesheets.Add(objnotifications);
                                //    }
                                //}
                                //else if (Session["EmployeeId"].ToString() == "60126") // Anusuya R Chaman and Practice = "SG&A" and CostCenter = "ADMIN","FINANCE"
                                //{
                                //    var Externalhrrfs = (from EF in db.ExternalFulfillments
                                //                         join H in db.HRRFs on EF.HRRFNumber equals H.HRRFNumber into tmpExterFulfill
                                //                         from H in tmpExterFulfill.DefaultIfEmpty()
                                //                         where (EF.ApprovalStatus == null || EF.ApprovalStatus == "")
                                //                         && H.RequestStatus.ToLower() == "pending for dh approval"
                                //                         && H.RequestType.ToLower() == "external"
                                //                         && EF.Practice.ToLower() == "sg&a" && (H.CostCenter.ToLower() == "admin" || H.CostCenter.ToLower() == "finance")
                                //                         select EF);

                                //    foreach (var hrrf in Externalhrrfs)
                                //    {
                                //        Notifications objnotifications = new Notifications();

                                //        objnotifications.ApplicationCode = ConfigurationManager.AppSettings["AppCodeTR"].ToString();
                                //        objnotifications.AssetID = hrrf.HRRFNumber;
                                //        objnotifications.NotificationMessage = "This is inform you that the Talent Request, which is converted to external fulfilment has been pending for your approvals.";

                                //        Timesheets.Add(objnotifications);
                                //    }
                                //}
                                //else if (Session["EmployeeId"].ToString() == "103406") // Prasad Prabhakar and Practice = "SG&A" and CostCenter = "BDM","HR"
                                //{
                                //    var Externalhrrfs = (from EF in db.ExternalFulfillments
                                //                         join H in db.HRRFs on EF.HRRFNumber equals H.HRRFNumber into tmpExterFulfill
                                //                         from H in tmpExterFulfill.DefaultIfEmpty()
                                //                         where (EF.ApprovalStatus == null || EF.ApprovalStatus == "")
                                //                         && H.RequestStatus.ToLower() == "pending for dh approval"
                                //                         && H.RequestType.ToLower() == "external"
                                //                         && EF.Practice.ToLower() == "sg&a" && (H.CostCenter.ToLower() == "bdm" || H.CostCenter.ToLower() == "hr")
                                //                         select EF);

                                //    foreach (var hrrf in Externalhrrfs)
                                //    {
                                //        Notifications objnotifications = new Notifications();

                                //        objnotifications.ApplicationCode = ConfigurationManager.AppSettings["AppCodeTR"].ToString();
                                //        objnotifications.AssetID = hrrf.HRRFNumber;
                                //        objnotifications.NotificationMessage = "This is inform you that the Talent Request, which is converted to external fulfilment has been pending for your approvals.";

                                //        Timesheets.Add(objnotifications);
                                //    }
                                //}
                                //else if (Session["EmployeeId"].ToString() == "101404") // Prashant V.S. Bhavaraju and Practice = "SG&A" and CostCenter = "MARKETING"
                                //{
                                //    var Externalhrrfs = (from EF in db.ExternalFulfillments
                                //                         join H in db.HRRFs on EF.HRRFNumber equals H.HRRFNumber into tmpExterFulfill
                                //                         from H in tmpExterFulfill.DefaultIfEmpty()
                                //                         where (EF.ApprovalStatus == null || EF.ApprovalStatus == "")
                                //                         && H.RequestStatus.ToLower() == "pending for dh approval"
                                //                         && H.RequestType.ToLower() == "external"
                                //                         && H.Practice.ToLower() == "sg&a" && H.CostCenter == "marketing"
                                //                         select EF);

                                //    foreach (var hrrf in Externalhrrfs)
                                //    {
                                //        Notifications objnotifications = new Notifications();

                                //        objnotifications.ApplicationCode = ConfigurationManager.AppSettings["AppCodeTR"].ToString();
                                //        objnotifications.AssetID = hrrf.HRRFNumber;
                                //        objnotifications.NotificationMessage = "This is inform you that the Talent Request, which is converted to external fulfilment has been pending for your approvals.";

                                //        Timesheets.Add(objnotifications);
                                //    }
                                //}
                                //else if (Session["EmployeeId"].ToString() == "101034") // Gangadhar Aka and Practice = "SG&A" and CostCenter = "IT"
                                //{
                                //    var Externalhrrfs = (from EF in db.ExternalFulfillments
                                //                         join H in db.HRRFs on EF.HRRFNumber equals H.HRRFNumber into tmpExterFulfill
                                //                         from H in tmpExterFulfill.DefaultIfEmpty()
                                //                         where (EF.ApprovalStatus == null || EF.ApprovalStatus == "")
                                //                         && H.RequestStatus.ToLower() == "pending for dh approval"
                                //                         && H.RequestType.ToLower() == "external"
                                //                         && H.Practice.ToLower() == "sg&a" && H.CostCenter.ToLower() == "it"
                                //                         select EF);

                                //    foreach (var hrrf in Externalhrrfs)
                                //    {
                                //        Notifications objnotifications = new Notifications();

                                //        objnotifications.ApplicationCode = ConfigurationManager.AppSettings["AppCodeTR"].ToString();
                                //        objnotifications.AssetID = hrrf.HRRFNumber;
                                //        objnotifications.NotificationMessage = "This is inform you that the Talent Request, which is converted to external fulfilment has been pending for your approvals.";

                                //        Timesheets.Add(objnotifications);
                                //    }
                                //}
                                //else if (Session["EmployeeId"].ToString() == "101441") // Vivek	Sambasivam and Practice = "IMS" (all IMS)
                                //{
                                //    var Externalhrrfs = (from EF in db.ExternalFulfillments
                                //                         join H in db.HRRFs on EF.HRRFNumber equals H.HRRFNumber into tmpExterFulfill
                                //                         from H in tmpExterFulfill.DefaultIfEmpty()
                                //                         where (EF.ApprovalStatus == null || EF.ApprovalStatus == "")
                                //                         && H.RequestStatus.ToLower() == "pending for dh approval"
                                //                         && H.RequestType.ToLower() == "external"
                                //                         && EF.Practice.Contains("IMS")
                                //                         select EF);

                                //    foreach (var hrrf in Externalhrrfs)
                                //    {
                                //        Notifications objnotifications = new Notifications();

                                //        objnotifications.ApplicationCode = ConfigurationManager.AppSettings["AppCodeTR"].ToString();
                                //        objnotifications.AssetID = hrrf.HRRFNumber;
                                //        objnotifications.NotificationMessage = "This is inform you that the Talent Request, which is converted to external fulfilment has been pending for your approvals.";

                                //        Timesheets.Add(objnotifications);
                                //    }
                                //}
                                //else if (Session["EmployeeId"].ToString() == "103237") // Vishwanath MS and Practice = "Cloud Solutions"
                                //{
                                //    var Externalhrrfs = (from EF in db.ExternalFulfillments
                                //                         join H in db.HRRFs on EF.HRRFNumber equals H.HRRFNumber into tmpExterFulfill
                                //                         from H in tmpExterFulfill.DefaultIfEmpty()
                                //                         where (EF.ApprovalStatus == null || EF.ApprovalStatus == "")
                                //                         && H.RequestStatus.ToLower() == "pending for dh approval"
                                //                         && H.RequestType.ToLower() == "external"
                                //                         && EF.Practice.ToLower() == "cloud solutions"
                                //                         select EF);

                                //    foreach (var hrrf in Externalhrrfs)
                                //    {
                                //        Notifications objnotifications = new Notifications();

                                //        objnotifications.ApplicationCode = ConfigurationManager.AppSettings["AppCodeTR"].ToString();
                                //        objnotifications.AssetID = hrrf.HRRFNumber;
                                //        objnotifications.NotificationMessage = "This is inform you that the Talent Request, which is converted to external fulfilment has been pending for your approvals.";

                                //        Timesheets.Add(objnotifications);
                                //    }
                                //}
                                //else if (Session["EmployeeId"].ToString() == "102968") // Ganesh Arunachala and Practice = "IT SERVICES OTHERS" and CostCenter = "QUALITY"
                                //{
                                //    var Externalhrrfs = (from EF in db.ExternalFulfillments
                                //                         join H in db.HRRFs on EF.HRRFNumber equals H.HRRFNumber into tmpExterFulfill
                                //                         from H in tmpExterFulfill.DefaultIfEmpty()
                                //                         where (EF.ApprovalStatus == null || EF.ApprovalStatus == "")
                                //                         && H.RequestStatus.ToLower() == "pending for dh approval"
                                //                         && H.RequestType.ToLower() == "external"
                                //                         && EF.Practice.ToLower() == "it services others"
                                //                         && H.CostCenter.ToLower() == "quality"
                                //                         select EF);

                                //    foreach (var hrrf in Externalhrrfs)
                                //    {
                                //        Notifications objnotifications = new Notifications();

                                //        objnotifications.ApplicationCode = ConfigurationManager.AppSettings["AppCodeTR"].ToString();
                                //        objnotifications.AssetID = hrrf.HRRFNumber;
                                //        objnotifications.NotificationMessage = "This is inform you that the Talent Request, which is converted to external fulfilment has been pending for your approvals.";

                                //        Timesheets.Add(objnotifications);
                                //    }
                                //}
                                //else
                                //{
                                //    var Externalhrrfs = (from EF in db.ExternalFulfillments
                                //                         join H in db.HRRFs on EF.HRRFNumber equals H.HRRFNumber into tmpExterFulfill
                                //                         from H in tmpExterFulfill.DefaultIfEmpty()
                                //                         where (EF.ApprovalStatus == null || EF.ApprovalStatus == "")
                                //                         && H.RequestStatus.ToLower() == "pending for dh approval"
                                //                         && H.RequestType.ToLower() == "external"
                                //                         && (EF.Practice != "SG&A" && !EF.Practice.Contains("IMS")
                                //                         && EF.Practice.ToLower() != "cloud solutions")
                                //                         select new
                                //                         {
                                //                             ExternalFulfilID = EF.ExternalFulfilID,
                                //                             HRRFNumber = EF.HRRFNumber,
                                //                             ProjectName = EF.ProjectName,
                                //                             ProjectCode = EF.ProjectCode,
                                //                             Role = EF.Role,
                                //                             Grade = EF.Grade,
                                //                             Skills = EF.Skills,
                                //                             Practice = EF.Practice,
                                //                             Location = EF.Location,
                                //                             Account = EF.Account,
                                //                             ApprovalStatus = EF.ApprovalStatus,
                                //                             Remarks = EF.Remarks,
                                //                             RequestedBy = EF.RequestedBy,
                                //                             RequestedDate = EF.RequestedDate,
                                //                             ApprovedBy = EF.ApprovedBy,
                                //                             ApprovedDate = EF.ApprovedDate,
                                //                             HRRFID = EF.HRRFID,
                                //                             CostCenter = H.CostCenter

                                //                         });

                                //    foreach (var hrrf in Externalhrrfs)
                                //    {
                                //        if (hrrf.Practice.ToLower() != "it services others" || (hrrf.Practice.ToLower() == "it services others"
                                //                                                                && hrrf.CostCenter.ToLower() != "quality"))
                                //        {
                                //            Notifications objnotifications = new Notifications();

                                //            if (Session["EmployeeId"].ToString() == "103673" && hrrf.Grade <= 4) // Ashutosh Uprety
                                //            {
                                //                objnotifications.ApplicationCode = ConfigurationManager.AppSettings["AppCodeTR"].ToString();
                                //                objnotifications.AssetID = hrrf.HRRFNumber;
                                //                objnotifications.NotificationMessage = "This is inform you that the Talent Request, which is converted to external fulfilment has been pending for your approvals.";

                                //                Timesheets.Add(objnotifications);
                                //            }
                                //            else if (Session["EmployeeId"].ToString() != "103673" && hrrf.Grade > 4)
                                //            {
                                //                objnotifications.ApplicationCode = ConfigurationManager.AppSettings["AppCodeTR"].ToString();
                                //                objnotifications.AssetID = hrrf.HRRFNumber;
                                //                objnotifications.NotificationMessage = "This is inform you that the Talent Request, which is converted to external fulfilment has been pending for your approvals.";

                                //                Timesheets.Add(objnotifications);
                                //            }
                                //        }
                                //    }
                                //}
                                #endregion
                            }

                            #endregion

                            #region Internal TRs

                            if (Session["EmployeeId"] != null)
                            {
                                //if (Session["EmployeeId"].ToString() == "100816") // Sujit Sahoo
                                //{
                                //    var hrrfs = (from h in db.HRRFs
                                //                 join pa in db.ProposeAssociates on h.HRRFNumber equals pa.HRRFNumber
                                //                 where h.RequestStatus.ToLower() == "pending for dh approval"
                                //                 && pa.PracticeStatus.ToLower() == "proposed"
                                //                 && h.Practice.ToLower() == "sg&a"
                                //                 && (h.CostCenter.ToLower() == "r&km" || h.CostCenter.ToLower() == "finance" || h.CostCenter.ToLower() == "ceo")
                                //                 select h);

                                //    foreach (var hrrf in hrrfs)
                                //    {
                                //        Notifications objnotifications = new Notifications();

                                //        objnotifications.ApplicationCode = ConfigurationManager.AppSettings["AppCodeTR"].ToString();
                                //        objnotifications.AssetID = hrrf.HRRFNumber;
                                //        objnotifications.NotificationMessage = "This is inform you that the Talent Request, which is converted to internal fulfilment has been pending for your approvals.";

                                //        Timesheets.Add(objnotifications);
                                //    }
                                //}
                                //else if (Session["EmployeeId"].ToString() == "60126") // Anusuya R Chaman
                                //{
                                //    var hrrfs = (from h in db.HRRFs
                                //                 join pa in db.ProposeAssociates on h.HRRFNumber equals pa.HRRFNumber
                                //                 where h.RequestStatus.ToLower() == "pending for dh approval"
                                //                 && pa.PracticeStatus.ToLower() == "proposed"
                                //                 && h.Practice.ToLower() == "sg&a"
                                //                 && (h.CostCenter.ToLower() == "admin" || h.CostCenter.ToLower() == "finance")
                                //                 select h);

                                //    foreach (var hrrf in hrrfs)
                                //    {
                                //        Notifications objnotifications = new Notifications();

                                //        objnotifications.ApplicationCode = ConfigurationManager.AppSettings["AppCodeTR"].ToString();
                                //        objnotifications.AssetID = hrrf.HRRFNumber;
                                //        objnotifications.NotificationMessage = "This is inform you that the Talent Request, which is converted to internal fulfilment has been pending for your approvals.";

                                //        Timesheets.Add(objnotifications);
                                //    }
                                //}
                                //else if (Session["EmployeeId"].ToString() == "103406") // Prasad Prabhakar
                                //{
                                //    var hrrfs = (from h in db.HRRFs
                                //                 join pa in db.ProposeAssociates on h.HRRFNumber equals pa.HRRFNumber
                                //                 where h.RequestStatus.ToLower() == "pending for dh approval"
                                //                 && pa.PracticeStatus.ToLower() == "proposed"
                                //                 && h.Practice.ToLower() == "sg&a"
                                //                 && (h.CostCenter.ToLower() == "bdm" || h.CostCenter.ToLower() == "hr")
                                //                 select h);

                                //    foreach (var hrrf in hrrfs)
                                //    {
                                //        Notifications objnotifications = new Notifications();

                                //        objnotifications.ApplicationCode = ConfigurationManager.AppSettings["AppCodeTR"].ToString();
                                //        objnotifications.AssetID = hrrf.HRRFNumber;
                                //        objnotifications.NotificationMessage = "This is inform you that the Talent Request, which is converted to internal fulfilment has been pending for your approvals.";

                                //        Timesheets.Add(objnotifications);
                                //    }
                                //}
                                //else if (Session["EmployeeId"].ToString() == "101404") // Prashant V.S. Bhavaraju
                                //{
                                //    var hrrfs = (from h in db.HRRFs
                                //                 join pa in db.ProposeAssociates on h.HRRFNumber equals pa.HRRFNumber
                                //                 where h.RequestStatus.ToLower() == "pending for dh approval"
                                //                 && pa.PracticeStatus.ToLower() == "proposed"
                                //                 && h.Practice.ToLower() == "sg&a"
                                //                 && (h.CostCenter.ToLower() == "marketing")
                                //                 select h);

                                //    foreach (var hrrf in hrrfs)
                                //    {
                                //        Notifications objnotifications = new Notifications();

                                //        objnotifications.ApplicationCode = ConfigurationManager.AppSettings["AppCodeTR"].ToString();
                                //        objnotifications.AssetID = hrrf.HRRFNumber;
                                //        objnotifications.NotificationMessage = "This is inform you that the Talent Request, which is converted to internal fulfilment has been pending for your approvals.";

                                //        Timesheets.Add(objnotifications);
                                //    }
                                //}
                                //else if (Session["EmployeeId"].ToString() == "101034") // Gangadhar Aka
                                //{
                                //    var hrrfs = (from h in db.HRRFs
                                //                 join pa in db.ProposeAssociates on h.HRRFNumber equals pa.HRRFNumber
                                //                 where h.RequestStatus.ToLower() == "pending for dh approval"
                                //                 && pa.PracticeStatus.ToLower() == "proposed"
                                //                 && h.Practice.ToLower() == "sg&a"
                                //                 && (h.CostCenter.ToLower() == "it")
                                //                 select h);

                                //    foreach (var hrrf in hrrfs)
                                //    {
                                //        Notifications objnotifications = new Notifications();

                                //        objnotifications.ApplicationCode = ConfigurationManager.AppSettings["AppCodeTR"].ToString();
                                //        objnotifications.AssetID = hrrf.HRRFNumber;
                                //        objnotifications.NotificationMessage = "This is inform you that the Talent Request, which is converted to internal fulfilment has been pending for your approvals.";

                                //        Timesheets.Add(objnotifications);
                                //    }
                                //}
                                //else if (Session["EmployeeId"].ToString() == "101441") // Vivek	Sambasivam
                                //{
                                //    var hrrfs = (from h in db.HRRFs
                                //                 join pa in db.ProposeAssociates on h.HRRFNumber equals pa.HRRFNumber
                                //                 where h.RequestStatus.ToLower() == "pending for dh approval"
                                //                 && pa.PracticeStatus.ToLower() == "proposed"
                                //                 && h.Practice.Contains("IMS")
                                //                 select h);

                                //    foreach (var hrrf in hrrfs)
                                //    {
                                //        Notifications objnotifications = new Notifications();

                                //        objnotifications.ApplicationCode = ConfigurationManager.AppSettings["AppCodeTR"].ToString();
                                //        objnotifications.AssetID = hrrf.HRRFNumber;
                                //        objnotifications.NotificationMessage = "This is inform you that the Talent Request, which is converted to internal fulfilment has been pending for your approvals.";

                                //        Timesheets.Add(objnotifications);
                                //    }
                                //}
                                //else if (Session["EmployeeId"].ToString() == "103237") // Vishwanath MS
                                //{
                                //    var hrrfs = (from h in db.HRRFs
                                //                 join pa in db.ProposeAssociates on h.HRRFNumber equals pa.HRRFNumber
                                //                 where h.RequestStatus.ToLower() == "pending for dh approval"
                                //                 && pa.PracticeStatus.ToLower() == "proposed"
                                //                 && h.Practice.ToLower() == "cloud solutions"
                                //                 select h);

                                //    foreach (var hrrf in hrrfs)
                                //    {
                                //        Notifications objnotifications = new Notifications();

                                //        objnotifications.ApplicationCode = ConfigurationManager.AppSettings["AppCodeTR"].ToString();
                                //        objnotifications.AssetID = hrrf.HRRFNumber;
                                //        objnotifications.NotificationMessage = "This is inform you that the Talent Request, which is converted to internal fulfilment has been pending for your approvals.";

                                //        Timesheets.Add(objnotifications);
                                //    }
                                //}
                                //else if (Session["EmployeeId"].ToString() == "102968") // Ganesh Arunachala
                                //{
                                //    var hrrfs = (from h in db.HRRFs
                                //                 join pa in db.ProposeAssociates on h.HRRFNumber equals pa.HRRFNumber
                                //                 where h.RequestStatus.ToLower() == "pending for dh approval"
                                //                 && pa.PracticeStatus.ToLower() == "proposed"
                                //                 && h.Practice.ToLower() == "it services others"
                                //                 && h.CostCenter.ToLower() == "quality"
                                //                 select h);

                                //    foreach (var hrrf in hrrfs)
                                //    {
                                //        Notifications objnotifications = new Notifications();

                                //        objnotifications.ApplicationCode = ConfigurationManager.AppSettings["AppCodeTR"].ToString();
                                //        objnotifications.AssetID = hrrf.HRRFNumber;
                                //        objnotifications.NotificationMessage = "This is inform you that the Talent Request, which is converted to internal fulfilment has been pending for your approvals.";

                                //        Timesheets.Add(objnotifications);
                                //    }
                                //}
                                //else
                                //{
                                //    var hrrfs = (from h in db.HRRFs
                                //                 join pa in db.ProposeAssociates on h.HRRFNumber equals pa.HRRFNumber
                                //                 where h.RequestStatus.ToLower() == "pending for dh approval"
                                //                 && pa.PracticeStatus.ToLower() == "proposed"
                                //                 && (h.Practice != "SG&A" && !h.Practice.Contains("IMS")
                                //                 && h.Practice.ToLower() != "cloud solutions")
                                //                 select new
                                //                 {
                                //                     Grade = h.Grade,
                                //                     HRRFNumber = h.HRRFNumber,
                                //                     Practice = h.Practice,
                                //                     CostCenter = h.CostCenter


                                //                 });

                                //    foreach (var hrrf in hrrfs)
                                //    {
                                //        if (hrrf.Practice.ToLower() != "it services others" || (hrrf.Practice.ToLower() == "it services others"
                                //                                                                && hrrf.CostCenter.ToLower() != "quality"))
                                //        {
                                //            Notifications objnotifications = new Notifications();

                                //            if (Session["EmployeeId"].ToString() == "103673" && hrrf.Grade <= 4) // Ashutosh Uprety
                                //            {
                                //                objnotifications.ApplicationCode = ConfigurationManager.AppSettings["AppCodeTR"].ToString();
                                //                objnotifications.AssetID = hrrf.HRRFNumber;
                                //                objnotifications.NotificationMessage = "This is inform you that the Talent Request, which is converted to internal fulfilment has been pending for your approvals.";

                                //                Timesheets.Add(objnotifications);
                                //            }
                                //            else if (Session["EmployeeId"].ToString() != "103673" && hrrf.Grade > 4) // Satish Katragadda
                                //            {
                                //                objnotifications.ApplicationCode = ConfigurationManager.AppSettings["AppCodeTR"].ToString();
                                //                objnotifications.AssetID = hrrf.HRRFNumber;
                                //                objnotifications.NotificationMessage = "This is inform you that the Talent Request, which is converted to internal fulfilment has been pending for your approvals.";

                                //                Timesheets.Add(objnotifications);
                                //            }
                                //        }
                                //    }
                                //}
                            }

                            #endregion

                            //ViewBag.Role = RoleResults.Role;
                            ViewBag.Role = "DH";
                        }

                        var EmpRoles = db.RoleMasters.ToList().Where(role => role.EmployeeId == EmpID && role.ApplicationCode == "TALENTREQ").FirstOrDefault();
                        if (EmpRoles != null && EmpRoles.Role != "OM")
                        {
                            #region Internal TRs

                            var hrrfs = (from h in db.HRRFs
                                         join pa in db.ProposeAssociates on h.HRRFNumber equals pa.HRRFNumber
                                         where h.RequestStatus.ToLower() == "pending for pm approval"
                                         && pa.PracticeStatus.ToLower() == "proposed"
                                         && h.HRRFCreatedBy == EmpID
                                         select new
                                         {
                                             HRRFNumber = h.HRRFNumber,
                                             Grade = h.Grade,
                                             jobdescrption = h.JobDescription,
                                             Role = h.RoleRequired

                                         });

                            foreach (var hrrf in hrrfs)
                            {
                                var rol = (from rl in db.DesignationMasters
                                           where rl.DesignationCode == hrrf.Role
                                           select new
                                           {
                                               rl.DesignationName
                                           }).FirstOrDefault();
                                string rolname = hrrf.Role;
                                if (rol != null)
                                {
                                    rolname = rol.DesignationName;
                                }
                                Notifications objnotifications = new Notifications();
                                objnotifications.Grade = hrrf.Grade.ToString();
                                objnotifications.JobDescrption = hrrf.jobdescrption;
                                objnotifications.Role = rolname;
                                objnotifications.ApplicationCode = ConfigurationManager.AppSettings["AppCodeTR"].ToString();
                                objnotifications.AssetID = hrrf.HRRFNumber;
                                objnotifications.NotificationMessage = "This is inform you that the Talent Request, which is converted to internal fulfilment has been pending for your approvals.";

                                Timesheets.Add(objnotifications);
                            }

                            #endregion

                            #region External TRs

                            // need to write code for External TRs
                            #endregion
                            ViewBag.Role = EmpRoles.Role;
                        }
                    }
                    else // by default he/she is PM.
                    {
                        #region Internal TRs

                        var hrrfs = (from h in db.HRRFs
                                     join pa in db.ProposeAssociates on h.HRRFNumber equals pa.HRRFNumber
                                     where h.RequestStatus.ToLower() == "pending for pm approval"
                                     && pa.PracticeStatus.ToLower() == "proposed"
                                     && h.HRRFCreatedBy == EmpID
                                     select new
                                     {
                                         HRRFNumber = h.HRRFNumber,
                                         Grade = h.Grade,
                                         jobdescrption = h.JobDescription,
                                         Role = h.RoleRequired

                                     });
                        foreach (var hrrf in hrrfs)
                        {
                            var rol = (from rl in db.DesignationMasters
                                       where rl.DesignationCode == hrrf.Role
                                       select new
                                       {
                                           rl.DesignationName
                                       }).FirstOrDefault();
                            string rolname = hrrf.Role;
                            if (rol != null)
                            {
                                rolname = rol.DesignationName;
                            }
                            Notifications objnotifications = new Notifications();
                            objnotifications.Grade = hrrf.Grade.ToString();
                            objnotifications.JobDescrption = hrrf.jobdescrption;
                            objnotifications.Role = rolname;
                            objnotifications.ApplicationCode = ConfigurationManager.AppSettings["AppCodeTR"].ToString();
                            objnotifications.AssetID = hrrf.HRRFNumber;
                            objnotifications.NotificationMessage = "This is inform you that the Talent Request, which is converted to internal fulfilment has been pending for your approvals.";

                            Timesheets.Add(objnotifications);
                        }
                        #endregion
                        #region External TRs
                        // need to write code for External TRs
                        #endregion
                        ViewBag.Role = "PM"; //RoleResults.Role;
                    }

                    #endregion

                    #region Skills

                    var TotalEmps = db.Employees.Where(e => e.SupervisorId == EmpID && e.IsActive == true).ToList();
                    //if(TotalEmps.Count>0)
                    //{
                        Session["loginId"] = EmpID;
                   // }
                    // var emps= db.Employees.Where(e => e.SupervisorId == EmpID && e.IsActive == true).ToList();
                    var skillsToBeApproved = new List<Notifications>();
                    if (TotalEmps.Count > 0 && Session["LeadId"] == null)
                    {
                        foreach (var emp in TotalEmps)
                        {
                            string status = "Submitted"; // ConfigurationManager.AppSettings["skillstatus"];
                            var yettpprove = db.EmployeeSkills_New.Where(e => e.EmployeeId == emp.EmployeeId && e.SkillStatus == status).FirstOrDefault();

                            if (yettpprove != null)
                            {
                                var empt = (from et in db.Employees
                                            where et.EmployeeId == emp.EmployeeId
                                            select new
                                            {
                                                name = et.FirstName + " " + et.MiddleName + " " + et.LastName
                                            })
                                    .FirstOrDefault();
                                string empname = "";
                                if (empt != null)
                                    empname = empt.name;

                                Notifications objnotifications = new Notifications();
                                objnotifications.ApplicationCode = ConfigurationManager.AppSettings["AppcodeSkills"].ToString();
                                objnotifications.AssetID = yettpprove.EmployeeId.ToString();
                                objnotifications.RequestName = empname.ToString();
                                objnotifications.NotificationMessage = "This is inform you that the Skills Approvals Pending for " + empname.ToString();
                                objnotifications.ApprovalSubmissionDate = db.EmployeeSkills_New.Where(c => c.EmployeeId == yettpprove.EmployeeId).Select(x => x.ApprovalSubmissionDate).Max();
                                //objnotifications.ApprovalSubmissionDate = (from x in db.EmployeeSkills_New where x.EmployeeId = yettpprove.EmployeeId  select x.ApprovalSubmissionDate).Max();
                                //objnotifications.ApprovalSubmissionDate  = (from x in db.EmployeeSkills_New where x.EmployeeId = yettpprove.EmployeeId
                                //                                            select x.ApprovalSubmissionDate).Max();
                                //objnotifications.ApprovalSubmissionDate = (from y in db.EmployeeSkills_New where y.ApprovalSubmissionDate != null) select y.ApprovalSubmissionDate.Value).Max();


                                //db.EmployeeSkills_New.Select(max).Where(i => i.EmployeeId = yettpprove.EmployeeId)
                                //Timesheets.Add(objnotifications);
                                skillsToBeApproved.Add(objnotifications);
                            }

                        }
                        ViewBag.SkillsToBeApproved = skillsToBeApproved;
                    }
                    #endregion
                    #region Skills
                    #region oldcode
                    //var TotalEmps = db.Employees.Where(e => e.SupervisorId == EmpID);
                    //var skillsToBeApproved = new List<Notifications>();
                    //foreach (var emp in TotalEmps)
                    //{
                    //    string status = ConfigurationManager.AppSettings["skillstatus"];
                    //    var yettpprove = db.EmployeeSkills.Where(e => e.EmployeeId == emp.EmployeeId && e.Status == status).FirstOrDefault();

                    //    if (yettpprove != null)
                    //    {
                    //        Notifications objnotifications = new Notifications();
                    //        objnotifications.ApplicationCode = ConfigurationManager.AppSettings["AppcodeSkills"].ToString();
                    //        objnotifications.AssetID = yettpprove.Employee.EmployeeId.ToString();
                    //        objnotifications.RequestName = (yettpprove.Employee.FirstName + " " + yettpprove.Employee.MiddleName + " " + yettpprove.Employee.LastName).ToString();
                    //        objnotifications.NotificationMessage = "This is inform you that the Skills Approvals Pending for " + yettpprove.Employee.FirstName + " " + yettpprove.Employee.LastName;

                    //        //Timesheets.Add(objnotifications);
                    //        skillsToBeApproved.Add(objnotifications);
                    //    }

                    //}

                    //ViewBag.SkillsToBeApproved = skillsToBeApproved;
                    #endregion
                    #endregion
                    //if (Timesheets.Count > 0)
                    //    return View(Timesheets.OrderByDescending(a => a.AssetID));
                    //else
                    //    return View(Timesheets);
                    #region ClusterLead
                   else if (TotalEmps.Count == 0 && Session["LeadId"] != null)
                    {
                        EmpID = Convert.ToInt32(Session["LeadId"].ToString());
                        List<ClusterLead> lstCluster = new List<ClusterLead>();

                        lstCluster = db.Database.SqlQuery<ClusterLead>("exec GetClusterTeamMember_test").ToList();
                        lstCluster = lstCluster.Where(m => m.EmployeeId == EmpID).ToList();
                        var skillsToBeApprovedcluster = new List<Notifications>();
                        if (lstCluster.Count > 0)
                        {
                            foreach (var emp in lstCluster)
                            {
                                string status = "Submitted";
                                string status1 = "Approved";
                                // ConfigurationManager.AppSettings["skillstatus"];
                                                             //  var yettpprove = db.EmployeeSkills_New.Where(e => e.EmployeeId == emp.TeamMemberID && e.SkillStatus == status).FirstOrDefault();
                               // var yettpprove = db.EmployeeSkills_New.Where(e => e.EmployeeId == emp.TeamMemberID && e.IsReviewed == null && e.SkillStatus == status).FirstOrDefault();

                                var yettpprove = db.EmployeeSkills_New.Where(e => e.EmployeeId == emp.TeamMemberID &&(e.SkillStatus == status || e.SkillStatus == status1) && (e.IsReviewed == null || e.IsReviewed ==false)).FirstOrDefault();
                                                          
                                if (yettpprove != null)
                                {
                                    var empt = (from et in db.Employees
                                                where et.EmployeeId == emp.TeamMemberID
                                                select new
                                                {
                                                    name = et.FirstName + " " + et.MiddleName + " " + et.LastName
                                                })
                                        .FirstOrDefault();
                                    string empname = "";
                                    if (empt != null)
                                        empname = empt.name;

                                    Notifications objnotifications = new Notifications();
                                    objnotifications.ApplicationCode = ConfigurationManager.AppSettings["AppcodeSkills"].ToString();
                                    objnotifications.AssetID = yettpprove.EmployeeId.ToString();
                                    objnotifications.RequestName = empname.ToString();
                                    objnotifications.NotificationMessage = "This is inform you that the Skills Approvals Pending for " + empname.ToString();
                                    objnotifications.ApprovalSubmissionDate = db.EmployeeSkills_New.Where(c => c.EmployeeId == yettpprove.EmployeeId).Select(x => x.ApprovalSubmissionDate).Max();
                                    skillsToBeApprovedcluster.Add(objnotifications);
                                }
                            }

                            ViewBag.SkillsToBeApproved = skillsToBeApprovedcluster;

                        }
                        Console.WriteLine("");
                    }
                    #endregion

                    #region ClusterLead and manager
                   else if (TotalEmps.Count > 0 && Session["LeadId"] != null)
                    {
                        EmpID = Convert.ToInt32(Session["LeadId"].ToString());
                        List<ClusterLead> lstCluster = new List<ClusterLead>();

                        lstCluster = db.Database.SqlQuery<ClusterLead>("exec GetClusterTeamMember_test").ToList();
                        lstCluster = lstCluster.Where(m => m.EmployeeId == EmpID).ToList();
                        var skillsToBeApprovedcluster = new List<Notifications>();
                        if (TotalEmps.Count > 0)
                        {
                            foreach (var emp in TotalEmps)
                            {
                                string status = "Submitted"; // ConfigurationManager.AppSettings["skillstatus"];
                                var yettpprove = db.EmployeeSkills_New.Where(e => e.EmployeeId == emp.EmployeeId && e.SkillStatus == status).FirstOrDefault();

                                if (yettpprove != null)
                                {
                                    var empt = (from et in db.Employees
                                                where et.EmployeeId == emp.EmployeeId
                                                select new
                                                {
                                                    name = et.FirstName + " " + et.MiddleName + " " + et.LastName
                                                })
                                        .FirstOrDefault();
                                    string empname = "";
                                    if (empt != null)
                                        empname = empt.name;

                                    Notifications objnotifications = new Notifications();
                                    objnotifications.ApplicationCode = ConfigurationManager.AppSettings["AppcodeSkills"].ToString();
                                    objnotifications.AssetID = yettpprove.EmployeeId.ToString();
                                    objnotifications.RequestName = empname.ToString();
                                    objnotifications.NotificationMessage = "This is inform you that the Skills Approvals Pending for " + empname.ToString();
                                    objnotifications.ApprovalSubmissionDate = db.EmployeeSkills_New.Where(c => c.EmployeeId == yettpprove.EmployeeId).Select(x => x.ApprovalSubmissionDate).Max();
                                    //objnotifications.ApprovalSubmissionDate = (from x in db.EmployeeSkills_New where x.EmployeeId = yettpprove.EmployeeId  select x.ApprovalSubmissionDate).Max();
                                    //objnotifications.ApprovalSubmissionDate  = (from x in db.EmployeeSkills_New where x.EmployeeId = yettpprove.EmployeeId
                                    //                                            select x.ApprovalSubmissionDate).Max();
                                    //objnotifications.ApprovalSubmissionDate = (from y in db.EmployeeSkills_New where y.ApprovalSubmissionDate != null) select y.ApprovalSubmissionDate.Value).Max();                                   

                                    //db.EmployeeSkills_New.Select(max).Where(i => i.EmployeeId = yettpprove.EmployeeId)
                                    //Timesheets.Add(objnotifications);
                                    skillsToBeApprovedcluster.Add(objnotifications);
                                }
                            }

                        }

                        if (lstCluster.Count > 0)
                        {
                            foreach (var emp in lstCluster)
                            {
                                string status = "Submitted"; // ConfigurationManager.AppSettings["skillstatus"];
                                                             // var yettpprove = db.EmployeeSkills_New.Where(e => e.EmployeeId == emp.TeamMemberID && e.SkillStatus == status).FirstOrDefault();
                               var yettpprove = db.EmployeeSkills_New.Where(e => e.EmployeeId == emp.TeamMemberID && (e.SkillStatus == status || e.SkillStatus == "Approved") && ( e.IsReviewed == null || e.IsReviewed == false)).FirstOrDefault();
                                //var yettpprove = db.EmployeeSkills_New.Where(e => e.EmployeeId == emp.TeamMemberID && e.SkillStatus == status).FirstOrDefault();  
                                if (yettpprove != null)
                                {
                                    var empt = (from et in db.Employees
                                                where et.EmployeeId == emp.TeamMemberID
                                                select new
                                                {
                                                    name = et.FirstName + " " + et.MiddleName + " " + et.LastName
                                                })
                                        .FirstOrDefault();
                                    string empname = "";
                                    if (empt != null)
                                        empname = empt.name;

                                    Notifications objnotifications = new Notifications();
                                    objnotifications.ApplicationCode = ConfigurationManager.AppSettings["AppcodeSkills"].ToString();
                                    objnotifications.AssetID = yettpprove.EmployeeId.ToString();
                                    objnotifications.RequestName = empname.ToString();
                                    objnotifications.NotificationMessage = "This is inform you that the Skills Approvals Pending for " + empname.ToString();
                                    objnotifications.ApprovalSubmissionDate = db.EmployeeSkills_New.Where(c => c.EmployeeId == yettpprove.EmployeeId).Select(x => x.ApprovalSubmissionDate).Max();
                                    skillsToBeApprovedcluster.Add(objnotifications);
                                }
                            }
                        }
                        ViewBag.SkillsToBeApproved = skillsToBeApprovedcluster;

                        Console.WriteLine("");
                    }
                    #endregion
                    return View(Timesheets.OrderByDescending(a => a.AssetID));
                }

                //catch (Exception ex)
                //{
                //    // ApplicationLog.Error(ex.ToString(), "Error occured in Create HRRF");
                //    ErrorHandling expcls = new ErrorHandling();
                //    expcls.Error(ex);
                //    return View();
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
        // GET: Notifications/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Notifications/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Notifications/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Notifications/Edit/5
        public ActionResult Edit(string id, string AppCode)
        {

            try
            { 
            if (AppCode == "TALENTREQ")
            {
                TempData["hrrfno"] = id;
                Session["Hrrfno"] = id;
                return RedirectToAction("ProposeAssociate", "ProposeAssociate");
            }
            else if (AppCode == "SKILLS")
            {
                return RedirectToAction("GetEmployeeDetails", "SkillsApproval", new { EmployeeID = id });
            }
            else
            {

                TempData["TimesheetID"] = id;
                //return RedirectToAction("MyTimeSheetWeekWise", "MyTimeSheetWeekWise");
                //return Redirect("http://172.16.0.217/Timesheet/MyTimeSheetWeekWise/MyTimeSheetWeekWise?TimesheetID="+ id);
                return Redirect(System.Configuration.ConfigurationManager.AppSettings["TimesheetURL"].ToString());
            }
            }
            catch (Exception ex)
            {

                Common.WriteExceptionErrorLog(ex);
                return RedirectToAction("Error", "Error");
            }

        }

        // POST: Notifications/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Notifications/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Notifications/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        public ActionResult MyNotification()
        {
            return View();
        }

        public string RejectItem(string AppCode, string AppId, string _Remarks)
        {
            string IsSuccess = string.Empty;
            try
            {
                #region timesheet

                //if (AppCode == ConfigurationManager.AppSettings["AppCodeTimesheet"])
                //{
                //    int TimesheetID = Convert.ToInt32(AppId);
                //    if (TimesheetID != 0)
                //    {
                //        var GetTimeSheetForApproval = (from tSheet in db.Timesheets where tSheet.TimesheetID == TimesheetID select tSheet).FirstOrDefault();
                //        GetTimeSheetForApproval.ApprovedBy = GetTimeSheetForApproval.ProjectManagerID;
                //        GetTimeSheetForApproval.Status = ConfigurationManager.AppSettings["_Reject"];

                //        var TimesheetDetails = (from s in db.Timesheets
                //                                where s.TimesheetID == TimesheetID
                //                                select new
                //                                {
                //                                    FromDate = s.FromDate,
                //                                    ToDate = s.ToDate
                //                                }).FirstOrDefault();
                //        Notification tblNotification = new Notification();
                //        tblNotification.NotificationType = ConfigurationManager.AppSettings["_Reject"];
                //        tblNotification.NotificationDate = System.DateTime.UtcNow;
                //        tblNotification.NotificationFrom = (from s in db.Timesheets where s.TimesheetID == TimesheetID select s.ProjectManagerID).FirstOrDefault();
                //        tblNotification.NotificationTo = (from s in db.Timesheets where s.TimesheetID == TimesheetID select s.EmployeeId).FirstOrDefault();
                //        var Body = ConfigurationManager.AppSettings["ApprovePrefix"] + TimesheetDetails.FromDate.Date.ToString("MM/dd/yyyy") + " - " + TimesheetDetails.ToDate.Date.ToString("MM/dd/yyyy") + ConfigurationManager.AppSettings["rejectSuffix"] + ". </br> </br> Remsrks: " + _Remarks;
                //        tblNotification.IsActive = true;
                //        tblNotification.AssetID = Convert.ToString(TimesheetID);
                //        tblNotification.ApplicationCode = ConfigurationManager.AppSettings["AppCodeTimesheet"].ToString();
                //        string body = string.Empty;
                //        using (StreamReader reader = new StreamReader(Common.DownloadFromS3Stream("Views/Shared/EmailTemplate.html")))
                //        {
                //            body = reader.ReadToEnd();
                //        }
                //        body = body.Replace("{ToUserName}", db.Employees.Where(e => e.EmployeeId == tblNotification.NotificationTo).Select(s => s.FirstName + " " + s.LastName).FirstOrDefault());
                //        body = body.Replace("{FromUserName}", db.Employees.Where(e => e.EmployeeId == tblNotification.NotificationFrom).Select(s => s.FirstName + " " + s.LastName).FirstOrDefault());
                //        body = body.Replace("{Description}", Body);

                //        tblNotification.NotificationMessage = body;


                //        string IsEmailSent = System.Configuration.ConfigurationManager.AppSettings["IsEmailSent"].ToString();
                //        bool IsEmail = Convert.ToBoolean(IsEmailSent);
                //        if (IsEmail == true)
                //        {
                //            db.Notifications.Add(tblNotification);

                //        }
                //        db.SaveChanges();



                //        TimesheetComments comments = new TimesheetComments();
                //        comments.TimesheetID = TimesheetID;
                //        comments.CommentedDate = System.DateTime.UtcNow;
                //        var ManagerName = (from s in db.Employees
                //                           where s.EmployeeId == GetTimeSheetForApproval.ProjectManagerID
                //                           select new
                //                           {
                //                               ManagerName = s.FirstName + " - " + s.LastName
                //                           }).SingleOrDefault();
                //        comments.Comments = ConfigurationManager.AppSettings["TimesheetRejectby"] + ManagerName.ManagerName;
                //        db.TimesheetComment.Add(comments);

                //        if (_Remarks != "")
                //        {
                //            TimesheetComments tscomments = new TimesheetComments();
                //            tscomments.TimesheetID = TimesheetID;
                //            tscomments.CommentedDate = System.DateTime.UtcNow;
                //            tscomments.Comments = ConfigurationManager.AppSettings["rejectcomments"] + _Remarks;
                //            db.TimesheetComment.Add(tscomments);
                //        }


                //        db.SaveChanges();
                //    }
                //}
                #endregion

                #region hrrfs
                if (AppCode == ConfigurationManager.AppSettings["AppCodeTR"].ToString())
                {
					//string UName = User.Identity.Name.Substring(7);
					string usermail = Common.GetAzureLoggedInUserID();
					var EmployeeID = (from emp in db.Employees
                                      where emp.Email == usermail && emp.IsActive == true
                                      select new
                                      {
                                          EmployeeId = emp.EmployeeId
                                      }).FirstOrDefault();
                    // int EmpID = Convert.ToInt32(EmployeeID.EmployeeId);

                    string HRRFNumber = AppId;
                    TempData["hrrfno"] = HRRFNumber;
                    //var objHRRF = db.HRRFs.Where(c => c.HRRFNumber == HRRFNumber).FirstOrDefault();
                    //var hrrfAcceptReject = db.ExternalFulfillments.Where(h => h.HRRFNumber == HRRFNumber).ToList().FirstOrDefault();
                    //hrrfAcceptReject.ApprovalStatus = ConfigurationManager.AppSettings["_Reject"] ;
                    //hrrfAcceptReject.Remarks = _Remarks; // needs to get from popup
                    //hrrfAcceptReject.ApprovedBy = EmployeeID.EmployeeId.ToString(); //need to get logged in user id
                    //hrrfAcceptReject.ApprovedDate = DateTime.UtcNow;
                    // db.Entry(hrrfAcceptReject).State = System.Data.Entity.EntityState.Modified;

                    //string HRRFNumber = Session["hrrfno"].ToString();
                    //TempData["hrrfno"] = HRRFNumber;
                    var objHRRF = db.HRRFs.Where(c => c.HRRFNumber == HRRFNumber).FirstOrDefault();
                    var hrrfAcceptReject = db.ExternalFulfillments.Where(h => h.HRRFNumber == HRRFNumber).ToList().FirstOrDefault();
                    hrrfAcceptReject.ApprovalStatus = ConfigurationManager.AppSettings["_Reject"];
                    hrrfAcceptReject.Remarks = _Remarks; // needs to get from popup
                    hrrfAcceptReject.ApprovedBy = objHRRF.HRRFCreatedBy.ToString(); //need to get logged in user id
                    hrrfAcceptReject.ApprovedDate = DateTime.UtcNow;
                    db.Entry(hrrfAcceptReject).State = System.Data.Entity.EntityState.Modified;
                    // hrrf request type Status
                    objHRRF.RequestType = ConfigurationManager.AppSettings["I_RequestType"].ToString();
                    objHRRF.RequestStatus = ConfigurationManager.AppSettings["TR_Qualified"].ToString();
                    db.Entry(objHRRF).State = System.Data.Entity.EntityState.Modified;
                    //HRRF Histroy
                    var objHrrfHistory = new HRRFHistory();
                    objHrrfHistory.HRRFNumber = HRRFNumber;
                    objHrrfHistory.HistoryDescription = HRRFNumber + "-" + "has been Rejected for External";
                    objHrrfHistory.ModifiedBy = Convert.ToInt32(Session["EmployeeId"]);
                    objHrrfHistory.ModifiedDate = DateTime.UtcNow;
                    db.HRRFHistories.Add(objHrrfHistory);


                    //notifications

                    Notification tblNotification = new Notification();
                    tblNotification.NotificationType = ConfigurationManager.AppSettings["TrExtFulfilRej_NOtification"].ToString();
                    tblNotification.NotificationDate = System.DateTime.UtcNow;

                    var raisedby = db.HRRFs.Where(r => r.HRRFNumber == AppId).FirstOrDefault();
                    tblNotification.NotificationFrom = EmployeeID.EmployeeId;
                    tblNotification.NotificationTo = raisedby.HRRFCreatedBy;
                    var Body = ConfigurationManager.AppSettings["ExternalFullReject"] + ConfigurationManager.AppSettings["TRNUMber"] + HRRFNumber + "</br> </br>" + ConfigurationManager.AppSettings["Remarks"] + _Remarks;
                    tblNotification.IsActive = true;
                    tblNotification.AssetID = Convert.ToString(HRRFNumber);
                    tblNotification.ApplicationCode = ConfigurationManager.AppSettings["AppCodeTR"].ToString();

                    string body = string.Empty;
                    using (StreamReader reader = new StreamReader(Common.DownloadFromS3Stream("Views/Shared/EmailTemplate.html")))
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
                #endregion
            }
            catch (Exception ex)
            {
                ErrorHandling expcls = new ErrorHandling();
                expcls.Error(ex);
            }


            return IsSuccess;
        }

        public string AcceptItem(string AppCode, string AppId, string _Remarks)
        {
            string IsSuccess = string.Empty;
            try
            {
                #region timesheet code


                //if (AppCode == ConfigurationManager.AppSettings["AppCodeTimesheet"])
                //{
                //    int TimeSheetID = Convert.ToInt32(AppId);
                //    var TimesheetDetails = (from t in db.Timesheets where t.TimesheetID == TimeSheetID select new { FromDate = t.FromDate, ToDate = t.ToDate }).FirstOrDefault();
                //    var GetStatusofTimeSheet = (from s in db.Timesheets where s.TimesheetID == TimeSheetID select s.Status).FirstOrDefault();
                //    if (GetStatusofTimeSheet != ConfigurationManager.AppSettings["_Approve"])
                //    {
                //        var GetTimeSheetForApproval = (from tSheet in db.Timesheets where tSheet.TimesheetID == TimeSheetID select tSheet).FirstOrDefault();
                //        GetTimeSheetForApproval.ApprovedBy = GetTimeSheetForApproval.ProjectManagerID;
                //        GetTimeSheetForApproval.syncdate = null;
                //        GetTimeSheetForApproval.Status = ConfigurationManager.AppSettings["_Approve"];

                //        Notification tblNotification = new Notification();
                //        tblNotification.NotificationType = ConfigurationManager.AppSettings["_Approve"];
                //        tblNotification.NotificationDate = System.DateTime.UtcNow;
                //        tblNotification.NotificationFrom = (from s in db.Timesheets where s.TimesheetID == TimeSheetID select s.ProjectManagerID).FirstOrDefault();
                //        tblNotification.NotificationTo = (from s in db.Timesheets where s.TimesheetID == TimeSheetID select s.EmployeeId).FirstOrDefault();
                //        var Body = ConfigurationManager.AppSettings["ApprovePrefix"] + TimesheetDetails.FromDate.Date.ToString("MM/dd/yyyy") + "-" + TimesheetDetails.ToDate.Date.ToString("MM/dd/yyyy") + ConfigurationManager.AppSettings["ApproveSuffix"];
                //        tblNotification.IsActive = true;
                //        tblNotification.AssetID = Convert.ToString(TimeSheetID);
                //        tblNotification.ApplicationCode = ConfigurationManager.AppSettings["AppCodeTimesheet"];
                //        string body = string.Empty;
                //        using (StreamReader reader = new StreamReader(Common.DownloadFromS3Stream("Views/Shared/EmailTemplate.html")))
                //        {
                //            body = reader.ReadToEnd();
                //        }
                //        body = body.Replace("{ToUserName}", db.Employees.Where(e => e.EmployeeId == tblNotification.NotificationTo).Select(s => s.FirstName + " " + s.LastName).FirstOrDefault());
                //        body = body.Replace("{FromUserName}", db.Employees.Where(e => e.EmployeeId == tblNotification.NotificationFrom).Select(s => s.FirstName + " " + s.LastName).FirstOrDefault());
                //        body = body.Replace("{Description}", Body);

                //        tblNotification.NotificationMessage = body;




                //        TimesheetComments comments = new TimesheetComments();
                //        comments.TimesheetID = TimeSheetID;
                //        comments.CommentedDate = System.DateTime.UtcNow;
                //        var ManagerName = (from name in db.Employees
                //                           where name.EmployeeId == GetTimeSheetForApproval.ProjectManagerID
                //                           select new
                //                           {
                //                               ManagerName = name.FirstName + " " + name.LastName
                //                           }).FirstOrDefault();
                //        comments.Comments = ConfigurationManager.AppSettings["TimesheetAppoveby"] + ManagerName.ManagerName;
                //        db.TimesheetComment.Add(comments);

                //        string IsEmailSent = System.Configuration.ConfigurationManager.AppSettings["IsEmailSent"].ToString();
                //        bool IsEmail = Convert.ToBoolean(IsEmailSent);
                //        if (IsEmail == true)
                //        {
                //            db.Notifications.Add(tblNotification);

                //        }
                //        db.SaveChanges();
                //    }
                //}
                #endregion

                #region hrrfs
                if (AppCode == ConfigurationManager.AppSettings["AppCodeTR"])
                {
					//string UName = User.Identity.Name.Substring(7);
					string usermail = Common.GetAzureLoggedInUserID();
					var EmployeeID = (from emp in db.Employees
                                      where emp.Email == usermail && emp.IsActive == true
                                      select new
                                      {
                                          EmployeeId = emp.EmployeeId
                                      }).FirstOrDefault();
                    // int EmpID = Convert.ToInt32(EmployeeID.EmployeeId);

                    string HRRFNumber = AppId;
                    //TempData["hrrfno"] = HRRFNumber;
                    //var objHRRF = db.HRRFs.Where(c => c.HRRFNumber == HRRFNumber).FirstOrDefault();
                    //var hrrfAcceptReject = db.ExternalFulfillments.Where(h => h.HRRFNumber == HRRFNumber).ToList().FirstOrDefault();
                    //hrrfAcceptReject.ApprovalStatus = ConfigurationManager.AppSettings["_Accept"]; 
                    //hrrfAcceptReject.Remarks = _Remarks; // needs to get from popup
                    //hrrfAcceptReject.ApprovedBy = EmployeeID.EmployeeId.ToString(); //need to get logged in user id
                    //hrrfAcceptReject.ApprovedDate = DateTime.UtcNow;
                    //db.Entry(hrrfAcceptReject).State = System.Data.Entity.EntityState.Modified;


                    //TempData["hrrfno"] = HRRFNumber;

                    // update hrrf status
                    var objHRRF = db.HRRFs.Where(c => c.HRRFNumber == HRRFNumber).FirstOrDefault();
                    objHRRF.RequestStatus = ConfigurationManager.AppSettings["E_RequestType"].ToString();

                    db.Entry(objHRRF).State = System.Data.Entity.EntityState.Modified;

                    ExternalFulfillment hrrfAcceptReject = db.ExternalFulfillments.Where(h => h.HRRFNumber == HRRFNumber).FirstOrDefault();
                    hrrfAcceptReject.ApprovalStatus = ConfigurationManager.AppSettings["ExternalAccepted"].ToString();
                    hrrfAcceptReject.ApprovedBy = objHRRF.HRRFCreatedBy.ToString(); //need to get logged in user id
                    hrrfAcceptReject.ApprovedDate = DateTime.UtcNow;
                    hrrfAcceptReject.Remarks = _Remarks;

                    db.Entry(hrrfAcceptReject).State = System.Data.Entity.EntityState.Modified;

                    //hrrf histroy
                    var objHrrfHistory = new HRRFHistory();
                    objHrrfHistory.HRRFNumber = HRRFNumber;
                    objHrrfHistory.HistoryDescription = HRRFNumber + "-" + "has been Accepted for External";
                    objHrrfHistory.ModifiedBy = Convert.ToInt32(Session["EmployeeId"]);
                    objHrrfHistory.ModifiedDate = DateTime.UtcNow;

                    db.HRRFHistories.Add(objHrrfHistory);

                    //notifications
                    Notification tblNotification = new Notification();
                    tblNotification.NotificationType = ConfigurationManager.AppSettings["TrExtFulfilAcep_NOtification"];
                    tblNotification.NotificationDate = System.DateTime.UtcNow;

                    var raisedby = db.HRRFs.Where(r => r.HRRFNumber == AppId).FirstOrDefault();
                    tblNotification.NotificationFrom = EmployeeID.EmployeeId;
                    tblNotification.NotificationTo = raisedby.HRRFCreatedBy;
                    var Body = ConfigurationManager.AppSettings["ExternalFullAccept"] + ConfigurationManager.AppSettings["TRNUMber"] + HRRFNumber + ConfigurationManager.AppSettings["Remarks"] + _Remarks;
                    tblNotification.IsActive = true;
                    tblNotification.AssetID = Convert.ToString(HRRFNumber);
                    tblNotification.ApplicationCode = ConfigurationManager.AppSettings["AppCodeTR"];
                    string body = string.Empty;
                    using (StreamReader reader = new StreamReader(Common.DownloadFromS3Stream("Views/Shared/EmailTemplate.html")))
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
                #endregion
            }
            catch (Exception ex)
            {
                ErrorHandling expcls = new ErrorHandling();
                expcls.Error(ex);
            }


            return IsSuccess;
        }
    }
}
