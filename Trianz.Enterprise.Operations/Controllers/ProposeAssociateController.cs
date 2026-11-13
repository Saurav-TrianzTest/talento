using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Trianz.Enterprise.Operations.General;
using Trianz.Enterprise.Operations.Models;

namespace Trianz.Enterprise.Operations.Controllers
{
    public class ProposeAssociateController : Controller
    {
        TrianzOperationsEntities db = new TrianzOperationsEntities();
        // Get all Constant
        string role_OM = Environment.GetEnvironmentVariable("Role_OM") ?? "";
        string role_PM = Environment.GetEnvironmentVariable("Role_PM") ?? "";
        //string role_PH = Environment.GetEnvironmentVariable("Role_PH") ?? System.Configuration.ConfigurationManager.AppSettings["Role_PH"].ToString();
        string role_DH = Environment.GetEnvironmentVariable("Role_DH") ?? "";
        string role_SM = Environment.GetEnvironmentVariable("Role_SM") ?? "";
        string role_DM = Environment.GetEnvironmentVariable("Role_DM") ?? "";
        string role_LDAdmin = Environment.GetEnvironmentVariable("Role_LDAdmin") ?? "";
        string role_Finance = Environment.GetEnvironmentVariable("Role_Finance") ?? "";
        string role_CompetencyAdmin = Environment.GetEnvironmentVariable("Role_CompetencyAdmin") ?? "";
        string role_RL = Environment.GetEnvironmentVariable("Role_RL") ?? "";

        string delivery_recommend = Environment.GetEnvironmentVariable("D_Recommend") ?? "";
        string practice_recommend = Environment.GetEnvironmentVariable("P_Recommend") ?? "";
        string praticeAccept = Environment.GetEnvironmentVariable("PraticeAccept") ?? "";
        string praticeReject = Environment.GetEnvironmentVariable("PraticeReject") ?? "";
        string fulfilled = Environment.GetEnvironmentVariable("fulfilledStatus") ?? "";
        string ResumePending = Environment.GetEnvironmentVariable("ResumePendingStatus") ?? "";
        string emp_Assignment = Environment.GetEnvironmentVariable("AssignmentStatus") ?? "";
        string qualified = Environment.GetEnvironmentVariable("TR_Qualified") ?? "";
        string convertInternal = Environment.GetEnvironmentVariable("I_RequestType") ?? "";
        string convertExternal = Environment.GetEnvironmentVariable("E_RequestType") ?? "";
        string PendingForDHApproval = Environment.GetEnvironmentVariable("PendingForDHApproval") ?? "";
        string PendingForPMApproval = Environment.GetEnvironmentVariable("PendingForPMApproval") ?? "";
        string external_Accept = Environment.GetEnvironmentVariable("ExternalAccepted") ?? "";
        string external_Reject = Environment.GetEnvironmentVariable("ExternalRejected") ?? "";
        string requestMoreInfo = Environment.GetEnvironmentVariable("TR_RequestMoreInfo") ?? "";

        string tr_Terminate = Environment.GetEnvironmentVariable("TR_Terminate") ?? "";
        string skill_Approved = Environment.GetEnvironmentVariable("Skill_Approved") ?? "";
        string emp_status_Free = Environment.GetEnvironmentVariable("Emp_status_Free") ?? "";
        string emp_ResourceStatus = Environment.GetEnvironmentVariable("Emp_ResourceStatus") ?? "";
        string IsEmailSent = Environment.GetEnvironmentVariable("IsEmailSent") ?? "";

        int clientInterviewYes = Convert.ToInt32(Environment.GetEnvironmentVariable("AddNumberofBillingDatesClientInterview") ?? "");
        int clientInterviewNo = Convert.ToInt32(Environment.GetEnvironmentVariable("AddNumberofBillingDatesClientInterviewNo") ?? "");
        string Submitted = Environment.GetEnvironmentVariable("SubmittedStatus") ?? "";
        /// <summary>
        /// GET: ProposeAssociate view based on HRRF number
        /// </summary>
        /// <param name="hrrfno"></param>
        /// <returns></returns>
        public ActionResult ProposeAssociate(string hrrfno, string EmpId, string SearchStringEmp, string skillbillingst, string searchFromDate, string searchToDate)
        {
            try
            {
                ViewBag.EmpId = EmpId==null?"": EmpId;
                var Cancel_ReasonList = from cancel in db.TR_CancelReason orderby cancel.CancelReasonName select cancel.CancelReasonName;
                ViewData["_Cancel_ReasonList"] = Cancel_ReasonList;
                //List<HRRFRemarksinfo> TRremaklst = new List<HRRFRemarksinfo>();
                //TRremaklst = db.Database.SqlQuery<TR_Remakrinfo>("exec GetRemarkEmp").ToList<TR_Remakrinfo>();
                //ValidationModel objvalidate = new ValidationModel();
                var emp = new ValidationModel();

                var remakrinfos= db.Database.SqlQuery<HRRFRemarksinfo>("exec GetRemarkEmp").ToList<HRRFRemarksinfo>();

                var statusinfos = db.Database.SqlQuery<HRRFStatusHistoryDetails>("exec GetHRRFStatusHistory").ToList<HRRFStatusHistoryDetails>();

                string id="";

                if (TempData["hrrnumber"]!=null && hrrfno==null)
                    {
                    id = TempData.Peek("hrrnumber").ToString();
                }
                if (TempData["hrrnumber"] == null && hrrfno == null)
                {
                    id = Session["Hrrfno"].ToString();
                }
                if (remakrinfos.Count > 0)
                {
                    remakrinfos = db.Database.SqlQuery<HRRFRemarksinfo>("exec GetRemarkEmp").ToList<HRRFRemarksinfo>();
                    if (hrrfno != null && id == "")
                    {
                        remakrinfos = remakrinfos.Where(tr => tr.HrrfNo == hrrfno).OrderByDescending(h=>h.Submitteddate).ToList();

                        //ViewBag.TRremaklst = objvalidate.TR_Remakrinfo;
                    }
                    if (hrrfno == null && id != "")
                    {
                        remakrinfos = remakrinfos.Where(tr => tr.HrrfNo == id).OrderByDescending(h => h.Submitteddate).ToList();

                        //ViewBag.TRremaklst = objvalidate.TR_Remakrinfo;
                    }
                }

                else
                {
                    ViewBag.TRremaklst = null;

                }

                if (statusinfos.Count > 0)
                {
                    statusinfos = db.Database.SqlQuery<HRRFStatusHistoryDetails>("exec GetHRRFStatusHistory").ToList<HRRFStatusHistoryDetails>();

                    if (hrrfno != null && id == "")
                    {
               
                        statusinfos = statusinfos.Where(s => s.HrrfNo == hrrfno).OrderByDescending(st => st.Submitteddate).ToList();
                    }
                    if (hrrfno == null && id != "")
                    {
                        statusinfos = statusinfos.Where(s => s.HrrfNo == id).OrderByDescending(st => st.Submitteddate).ToList();
                    }
                }

                else
                {
                    ViewBag.TRstatuslst = null;

                }
                if (TempData["PrevPage"] != null)
                {
                    TempData.Keep("PrevPage");
                }

                if (hrrfno != null)
                {
                    Session["hrrfno"] = hrrfno.Trim().ToString();
                }
                int? hrrfCreatedBy = 0;
                if (hrrfno != null)
                {
                    hrrfCreatedBy = (from hr in db.HRRFs
                                     where hr.HRRFNumber == hrrfno
                                     select hr.HRRFCreatedBy).FirstOrDefault();
                }
                var employeeId = Session["EmployeeId"];
                ViewBag.HRRFCreatedBy = hrrfCreatedBy;
                List<string> dashBoardViewers = Environment.GetEnvironmentVariable("DHAndRLRoleUsers") ?? "".Split(',').ToList();
                bool flag = dashBoardViewers.Contains(employeeId.ToString());
                if (flag)
                    ViewBag.ShowApprovalButton = true;
                if (Session["Role"] != null && Session["Role"].ToString() == "OM")
                {
                    Session["IsPractice"] = true;
                }
                else
                {
                    if (hrrfno != null)
                    {
                        Session["hrrfno"] = hrrfno.Trim().ToString();
                        // get practice for hrrf and logged-in user
                        // then redirect  
                        string userLoggedinPractice = string.Empty;
                        if (Session["Practice"] != null)
                        {
                            userLoggedinPractice = Session["Practice"].ToString();
                        }

                        var hrrfPractice = db.HRRFs.Where(h => h.HRRFNumber == hrrfno).FirstOrDefault();
                        if (hrrfPractice.Practice.ToLower() == userLoggedinPractice.ToLower())
                        {
                            bool IsPractice = true;
                            Session["IsPractice"] = IsPractice;
                        }
                        else
                        {
                            bool IsPractice = false;
                            Session["IsPractice"] = IsPractice;
                        }
                    }
                    else
                    {
                        if (TempData["hrrfno"] != null)
                        {
                            string TempHrrfNo = TempData["hrrfno"].ToString();
                            Session["hrrfno"] = TempHrrfNo;
                            string userLoggedinPractice = Session["Practice"].ToString();
                            var hrrfPractice = db.HRRFs.Where(h => h.HRRFNumber == TempHrrfNo).FirstOrDefault();
                            if (hrrfPractice.Practice.ToLower() == userLoggedinPractice.ToLower())
                            {
                                bool IsPractice = true;
                                Session["IsPractice"] = IsPractice;
                            }
                            else
                            {
                                bool IsPractice = false;
                                Session["IsPractice"] = IsPractice;
                            }
                        }
                    }
                }

                #region ConvertChecking
                var objConvertExternalCount = db.ExternalHires.Where(c => c.HRRFNumber == hrrfno).ToList();
                if (objConvertExternalCount.Count > 0)
                {
                    ViewBag.isRecord = true;
                }
                else
                {
                    ViewBag.isRecord = false;
                }
                #endregion



                var viewResult = new ValidationModel();
                string hrrf = Session["hrrfno"].ToString();

                // to bind hrrf history
                IList<TRHistory> _history = new List<TRHistory>();
                var trHistoryobj = db.HRRFHistories.Where(h => h.HRRFNumber == hrrf).OrderByDescending(c => c.ModifiedDate).ToList();
                var trRequestStatus = db.HRRFs.Where(h => h.HRRFNumber == hrrf).ToString();
                if (trRequestStatus == Environment.GetEnvironmentVariable("Submitted") ?? "")
                {

                }
                foreach (var hr in trHistoryobj)
                {
                    TRHistory objTRHistory = new TRHistory();
                    objTRHistory.ModifiedDate = hr.ModifiedDate;
                    objTRHistory.HistoryDescription = hr.HistoryDescription;
                    objTRHistory.Remarks = hr.Remarks;
                    objTRHistory.ModifiedEmpName = db.Employees.Where(e => e.EmployeeId == hr.ModifiedBy).Select(c => c.FirstName + " " + c.LastName).FirstOrDefault();
                    _history.Add(objTRHistory);
                    //hr.ModifiedEmpName = db.Employees.Where(e => e.EmployeeId == hr.ModifiedBy).Select(c=>c.FirstName+" "+c.LastName).FirstOrDefault();
                }
                ViewData["HRRFHistory"] = _history;

                if (Session["Role"] != null && Session["IsPractice"] != null)
                {
                    string role = Session["Role"].ToString();
                    bool ispracticeCheck = Convert.ToBoolean(Session["IsPractice"]);
                    // Operation Manager or Pratice Manager
                    if (role == role_OM)
                    {
                        GetHRRF(hrrf);
                        //if (EmpId != null && EmpId != "")
                        //{
                        GetListOfSearchedRecommendResourcesbymagic(EmpId, hrrf, "Bench", searchFromDate, searchToDate);
                        //}
                        //else
                        //{
                        //    GetListRecommendResources();
                        //}
                        // GetProposeAssociates(hrrf);
                        GetListOfResourceList(hrrf, praticeReject);
                        var skillMatchResourceResult = ViewData["BenchEmployeesWithMatchedSkills"] as List<TRMagicData>;
                        var HRRFDataResult = ViewData["HRRFData"] as List<HRRF>;
                        var ProposeAssociateDataResult = ViewData["ProposeAssociatesData"] as List<ProposedResourcesList>;

                        if (HRRFDataResult != null)
                        {
                            if (HRRFDataResult.Count > 0)
                            {
                                if (HRRFDataResult[0].RequestStatus.ToLower().Contains("pending"))
                                {
                                    string requestStatus = HRRFDataResult[0].RequestStatus;
                                    string practice = HRRFDataResult[0].Practice;
                                    string costcenter = HRRFDataResult[0].CostCenter;

                                    Int32 grade = (HRRFDataResult[0].Grade == null) ? 0 : Convert.ToInt32(HRRFDataResult[0].Grade);
                                    Int32 empID = (HRRFDataResult[0].HRRFCreatedBy == null) ? 0 : Convert.ToInt32(HRRFDataResult[0].HRRFCreatedBy);

                                    ViewBag.ApproverName = GetApprovalName(requestStatus, practice, costcenter, grade, empID, HRRFDataResult[0].HRRFNumber);
                                    ViewBag.EmployeeID = empID;
                                }
                            }
                        }
                        var hrrfskillinfo = (from hr in db.HRRFSkills_ExpertiseLevel
                                             join skil in db.SkillMasters on hr.SkillId equals skil.SkillId
                                             join expt in db.ExpertiseLevels on hr.ExpertiseLevel equals expt.LevelName
                                             where hr.HRRFNumber == hrrf
                                             select new HRRFSkillsDetails
                                             {
                                                 SkillId = skil.SkillId,
                                                 SkillCategory = skil.SkillCategory,
                                                 Skillset = skil.Skillset,
                                                 ExpertiseLevel = expt.LevelName,
                                                 IsMandatoy = hr.IsMandatoy == true ? "Yes" : "NO"
                                             }).ToList();

                        if (Request.QueryString != null)
                        {
                            if (Request.QueryString["flag"] != null)
                            {
                                ViewBag.IsButtonEnabled = Convert.ToBoolean(Request.QueryString["flag"]);
                            }
                        }


                        viewResult = new ValidationModel()
                        {
                            HRRFs = HRRFDataResult,
                            HRRFSkills_Expertise = hrrfskillinfo,
                            //BenchEmployee = skillMatchResourceResult,
                            TRMagicDetails = skillMatchResourceResult,
                            //ProposeAssociates = ProposeAssociateDataResult,
                            TRHistorys = _history.ToList(), //newforgrid
                            PropsedList = ProposeAssociateDataResult,
                            ExternalFulfillments = db.ExternalFulfillments.Where(f => f.HRRFNumber == hrrf).ToList(),
                            HRRFRemarksinfo = remakrinfos,
                            HRRFStatusHistoryDetails = statusinfos
                        };
                    }
                    // practice Manager
                    if (role == role_PM)
                    {
                        GetHRRF(hrrf);
                        //GetProposeAssociates(hrrf);
                        GetListOfResourceList(hrrf, praticeReject);
                        var HRRFDataResult = ViewData["HRRFData"] as List<HRRF>;
                        var ProposeAssociateDataResult = ViewData["ProposeAssociatesData"] as List<ProposedResourcesList>;

                        if (HRRFDataResult != null)
                        {
                            if (HRRFDataResult.Count > 0)
                            {
                                if (HRRFDataResult[0].RequestStatus.ToLower().Contains("pending"))
                                {
                                    string requestStatus = HRRFDataResult[0].RequestStatus;
                                    string practice = HRRFDataResult[0].Practice;
                                    string costcenter = HRRFDataResult[0].CostCenter;
                                    Int32 grade = (HRRFDataResult[0].Grade == null) ? 0 : Convert.ToInt32(HRRFDataResult[0].Grade);
                                    Int32 empID = (HRRFDataResult[0].HRRFCreatedBy == null) ? 0 : Convert.ToInt32(HRRFDataResult[0].HRRFCreatedBy);

                                    ViewBag.ApproverName = GetApprovalName(requestStatus, practice, costcenter, grade, empID, HRRFDataResult[0].HRRFNumber);
                                    ViewBag.EmployeeID = empID;
                                }
                            }
                        }
                        var hrrfskillinfo = (from hr in db.HRRFSkills_ExpertiseLevel
                                             join skil in db.SkillMasters on hr.SkillId equals skil.SkillId
                                             join expt in db.ExpertiseLevels on hr.ExpertiseLevel equals expt.LevelName
                                             where hr.HRRFNumber == hrrf
                                             select new HRRFSkillsDetails
                                             {
                                                 SkillId = skil.SkillId,
                                                 SkillCategory = skil.SkillCategory,
                                                 Skillset = skil.Skillset,
                                                 ExpertiseLevel = expt.LevelName,
                                                 IsMandatoy = hr.IsMandatoy == true ? "Yes" : "NO"
                                             }).ToList();

                        viewResult = new ValidationModel()
                        {
                            HRRFs = HRRFDataResult,
                            HRRFSkills_Expertise = hrrfskillinfo,
                            //ProposeAssociates = ProposeAssociateDataResult,
                            PropsedList = ProposeAssociateDataResult,
                            TRHistorys = _history.ToList(),  ////newforgrid
                            ExternalFulfillments = db.ExternalFulfillments.Where(f => f.HRRFNumber == hrrf).ToList(),
                            HRRFRemarksinfo = remakrinfos,
                            HRRFStatusHistoryDetails = statusinfos
                        };

                        //ViewBag.EmployeeID =  (Session["EmployeeId"] != null) ? Session["EmployeeId"] : 0;
                    }
                    // Pratice Head
                    if (role == role_DH || role == role_RL || role == role_SM || role == role_LDAdmin || role == role_CompetencyAdmin || role == role_Finance)
                    {
                        GetHRRF(hrrf);
                        //GetProposeAssociates(hrrf);
                        GetListOfResourceList(hrrf, praticeReject);
                        var HRRFDataResult = ViewData["HRRFData"] as List<HRRF>;
                        var ProposeAssociateDataResult = ViewData["ProposeAssociatesData"] as List<ProposedResourcesList>;

                        //ExternalFulfillment objExternalFulfilment = db.ExternalFulfillments.Where(ef => ef.HRRFNumber == hrrf).FirstOrDefault();
                        //if (objExternalFulfilment !=null)
                        //{
                        //    ViewBag.CanEditByRL = (objExternalFulfilment.ApprovalStatus != null) ? (objExternalFulfilment.ApprovalStatus.ToLower() == "accepted") ? true : false : false;
                        //}
                        if (HRRFDataResult != null)
                        {
                            if (HRRFDataResult.Count > 0)
                            {
                                if (HRRFDataResult[0].RequestStatus.ToLower().Contains("pending"))
                                {
                                    string requestStatus = HRRFDataResult[0].RequestStatus;
                                    string practice = HRRFDataResult[0].Practice;
                                    string costcenter = HRRFDataResult[0].CostCenter;
                                    Int32 grade = (HRRFDataResult[0].Grade == null) ? 0 : Convert.ToInt32(HRRFDataResult[0].Grade);
                                    Int32 empID = (HRRFDataResult[0].HRRFCreatedBy == null) ? 0 : Convert.ToInt32(HRRFDataResult[0].HRRFCreatedBy);

                                    ViewBag.ApproverName = GetApprovalName(requestStatus, practice, costcenter, grade, empID, HRRFDataResult[0].HRRFNumber);
                                    ViewBag.EmployeeID = empID;
                                }
                            }
                        }

                        if (Request.QueryString != null)
                        {
                            if (Request.QueryString["flag"] != null)
                            {
                                ViewBag.IsButtonEnabled = Convert.ToBoolean(Request.QueryString["flag"]);
                            }
                        }
                        var hrrfskillinfo = (from hr in db.HRRFSkills_ExpertiseLevel
                                             join skil in db.SkillMasters on hr.SkillId equals skil.SkillId
                                             join expt in db.ExpertiseLevels on hr.ExpertiseLevel equals expt.LevelName
                                             where hr.HRRFNumber == hrrf
                                             select new HRRFSkillsDetails
                                             {
                                                 SkillId = skil.SkillId,
                                                 SkillCategory = skil.SkillCategory,
                                                 Skillset = skil.Skillset,
                                                 ExpertiseLevel = expt.LevelName,
                                                 IsMandatoy = hr.IsMandatoy == true ? "Yes" : "NO"
                                             }).ToList();


                        viewResult = new ValidationModel()
                        {
                            HRRFs = HRRFDataResult,
                            HRRFSkills_Expertise = hrrfskillinfo,
                            //ProposeAssociates = ProposeAssociateDataResult,
                            PropsedList = ProposeAssociateDataResult,
                            TRHistorys = _history.ToList(),   //newforgrid
                            ExternalFulfillments = db.ExternalFulfillments.Where(f => f.HRRFNumber == hrrf).ToList(),
                            HRRFRemarksinfo = remakrinfos,
                            HRRFStatusHistoryDetails = statusinfos
                        };

                        if (role == role_DH)
                        {
                            ViewBag.CanDHApprove = IsValidDH(viewResult);
                        }
                    }
                    //For Admin
                    if (role == "Admin")
                    {
                        GetHRRF(hrrf);
                        //GetProposeAssociates(hrrf);
                        GetListOfResourceList(hrrf, praticeReject);
                        var HRRFDataResult = ViewData["HRRFData"] as List<HRRF>;
                        var ProposeAssociateDataResult = ViewData["ProposeAssociatesData"] as List<ProposedResourcesList>;

                        if (HRRFDataResult != null)
                        {
                            if (HRRFDataResult.Count > 0)
                            {
                                if (HRRFDataResult[0].RequestStatus.ToLower().Contains("pending"))
                                {
                                    string requestStatus = HRRFDataResult[0].RequestStatus;
                                    string practice = HRRFDataResult[0].Practice;
                                    string costcenter = HRRFDataResult[0].CostCenter;
                                    Int32 grade = (HRRFDataResult[0].Grade == null) ? 0 : Convert.ToInt32(HRRFDataResult[0].Grade);
                                    Int32 empID = (HRRFDataResult[0].HRRFCreatedBy == null) ? 0 : Convert.ToInt32(HRRFDataResult[0].HRRFCreatedBy);

                                    ViewBag.ApproverName = GetApprovalName(requestStatus, practice, costcenter, grade, empID, HRRFDataResult[0].HRRFNumber);
                                    ViewBag.EmployeeID = empID;
                                }
                            }
                        }
                        var hrrfskillinfo = (from hr in db.HRRFSkills_ExpertiseLevel
                                             join skil in db.SkillMasters on hr.SkillId equals skil.SkillId
                                             join expt in db.ExpertiseLevels on hr.ExpertiseLevel equals expt.LevelName
                                             where hr.HRRFNumber == hrrf
                                             select new HRRFSkillsDetails
                                             {
                                                 SkillId = skil.SkillId,
                                                 SkillCategory = skil.SkillCategory,
                                                 Skillset = skil.Skillset,
                                                 ExpertiseLevel = expt.LevelName,
                                                 IsMandatoy = hr.IsMandatoy == true ? "Yes" : "NO"
                                             }).ToList();

                        viewResult = new ValidationModel()
                        {
                            HRRFs = HRRFDataResult,
                            HRRFSkills_Expertise = hrrfskillinfo,
                            //ProposeAssociates = ProposeAssociateDataResult,
                            PropsedList = ProposeAssociateDataResult,
                            ExternalFulfillments = db.ExternalFulfillments.Where(f => f.HRRFNumber == hrrf).ToList(),
                            HRRFRemarksinfo = remakrinfos,
                            HRRFStatusHistoryDetails = statusinfos
                        };

                    }
                    // Delivery Manager
                    if (role == role_DM)
                    {
                        GetHRRF(hrrf);
                        //GetProposeAssociates(hrrf);
                        GetListOfResourceList(hrrf, "Approved");
                        var HRRFDataResult = ViewData["HRRFData"] as List<HRRF>;
                        var ProposeAssociateDataResult = ViewData["ProposeAssociatesData"] as List<ProposedResourcesList>;

                        if (HRRFDataResult != null)
                        {
                            if (HRRFDataResult.Count > 0)
                            {
                                if (HRRFDataResult[0].RequestStatus.ToLower().Contains("pending"))
                                {
                                    string requestStatus = HRRFDataResult[0].RequestStatus;
                                    string practice = HRRFDataResult[0].Practice;
                                    string costcenter = HRRFDataResult[0].CostCenter;
                                    Int32 grade = (HRRFDataResult[0].Grade == null) ? 0 : Convert.ToInt32(HRRFDataResult[0].Grade);
                                    Int32 empID = (HRRFDataResult[0].HRRFCreatedBy == null) ? 0 : Convert.ToInt32(HRRFDataResult[0].HRRFCreatedBy);

                                    ViewBag.ApproverName = GetApprovalName(requestStatus, practice, costcenter, grade, empID, HRRFDataResult[0].HRRFNumber);
                                    ViewBag.EmployeeID = empID;
                                }
                            }
                        }
                        var hrrfskillinfo = (from hr in db.HRRFSkills_ExpertiseLevel
                                             join skil in db.SkillMasters on hr.SkillId equals skil.SkillId
                                             join expt in db.ExpertiseLevels on hr.ExpertiseLevel equals expt.LevelName
                                             where hr.HRRFNumber == hrrf
                                             select new HRRFSkillsDetails
                                             {
                                                 SkillId = skil.SkillId,
                                                 SkillCategory = skil.SkillCategory,
                                                 Skillset = skil.Skillset,
                                                 ExpertiseLevel = expt.LevelName,
                                                 IsMandatoy = hr.IsMandatoy == true ? "Yes" : "NO"
                                             }).ToList();


                        viewResult = new ValidationModel()
                        {
                            HRRFs = HRRFDataResult,
                            HRRFSkills_Expertise = hrrfskillinfo,
                            PropsedList = ProposeAssociateDataResult,
                            HRRFRemarksinfo = remakrinfos,
                            HRRFStatusHistoryDetails = statusinfos
                            //ProposeAssociates = ProposeAssociateDataResult.Where(e => e.DeliveryStatus != "Approved").ToList() 
                        };

                    }
                    return View(viewResult);
                }
                
                return View(viewResult);
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

        private string GetApprovalName(string requestStatus, string practice, string costcenter, Int32 grade, Int32 empID, string Hrrfnumber)
        {
            string employeeName = "- ";
            if (requestStatus.ToLower() == PendingForDHApproval.ToLower())
            {
                string emp = empID.ToString();

                var getapprover = (from bpr in db.PracticeWiseBenchCodes
                                   join EF in db.ExternalFulfillments on bpr.Practice.ToLower() equals EF.Practice.ToLower()
                                   join H in db.HRRFs on EF.HRRFNumber equals H.HRRFNumber into tmpExterFulfill
                                   from H in tmpExterFulfill.DefaultIfEmpty()
                                   where H.CostCenter.ToLower().Equals(bpr.CostCenter.ToLower())
                                   //&& bpr.DeliveryHeadID.Contains(emp)
                                   && H.HRRFNumber == Hrrfnumber
                                  && bpr.TRGrade.Contains(grade.ToString())
                                   && bpr.Practice.ToLower().Equals(practice.ToLower())
                                   && bpr.CostCenter.ToLower().Contains(costcenter.ToLower())
                                   && ((H.Purpose != "Project" && H.ProjectCode == bpr.BenchCode) || (H.Purpose == "Project") || (H.Purpose == "Opportunity"))
                                   select new
                                   {
                                       deliveryheadeID = bpr.DeliveryHeadID
                                   }).Distinct().ToList();

                if (getapprover != null)
                {
                    if (getapprover.Count > 0)
                    {
                        int emplid = Convert.ToInt32(getapprover[0].deliveryheadeID);
                        employeeName = "- " + (from empnam in db.Employees
                                               where empnam.EmployeeId == emplid
                                               select empnam.FirstName + " " + empnam.MiddleName + " " + empnam.LastName).FirstOrDefault();

                    }
                }
                //if (practice.ToLower() == "sg&a")
                //{
                //    if (costcenter.ToLower() == "r&km" || costcenter.ToLower() == "strategy & research" || costcenter.ToLower() == "ceo")
                //    {
                //        employeeName = " - Sujit Sahoo";
                //    }
                //    else if (costcenter.ToLower() == "admin" || costcenter.ToLower() == "finance")
                //    {
                //        employeeName = " - Anusuya R Chaman";
                //    }
                //    else if (costcenter.ToLower() == "bdm" || costcenter.ToLower() == "hr")
                //    {
                //        employeeName = " - Prasad Prabhakar";
                //    }
                //    else if (costcenter.ToLower() == "marketing")
                //    {
                //        employeeName = " - Prashant V.S. Bhavaraju";
                //    }
                //    else if (costcenter.ToLower() == "it")
                //    {
                //        employeeName = " - Gangadhar Aka";
                //    }

                //}
                //else if (practice.ToUpper().Contains("IMS"))
                //{
                //    employeeName = " - Vivek Sambasivam";
                //}
                //else if (practice.ToUpper() == "CLOUD SOLUTIONS")
                //{
                //    employeeName = " - Vishwanath MS";
                //}
                //else if (practice.ToLower() == "it services others" && costcenter.ToLower() == "quality")
                //{
                //    employeeName = " - Ganesh Arunachala";
                //}

                //else
                //{
                //    if (grade <= 4)
                //    {
                //        employeeName = " - Ashutosh Uprety";
                //    }
                //    else
                //    {
                //        employeeName = " - Satish Katragadda";
                //    }
                //}
            }
            else if (requestStatus.ToLower() == PendingForPMApproval.ToLower())
            {
                Employee objEmp = db.Employees.Where(e => e.EmployeeId == empID).FirstOrDefault();

                if (objEmp != null)
                {
                    employeeName = " - " + objEmp.FirstName + " " + objEmp.LastName;
                }
            }

            return employeeName;
        }

        private bool IsValidDH(ValidationModel viewResult)
        {
            if (Session["EmployeeId"] != null)
            {
                string emp = Session["EmployeeId"].ToString();

                if (viewResult.HRRFs.Count() == 1)
                {
                    if (viewResult.HRRFs.First().RequestStatus.ToLower() == "pending for dh approval" && viewResult.HRRFs.First().RequestType.ToLower() == "external")
                    {
                        if (viewResult.ExternalFulfillments.Count() == 1)
                        {
                            string hrrfnu = viewResult.HRRFs.First().HRRFNumber;
                            var Externalhrrfs = (from bpr in db.PracticeWiseBenchCodes
                                                 join EF in db.ExternalFulfillments on bpr.Practice.ToLower() equals EF.Practice.ToLower()
                                                 join H in db.HRRFs on EF.HRRFNumber equals H.HRRFNumber into tmpExterFulfill
                                                 from H in tmpExterFulfill.DefaultIfEmpty()
                                                 where
                                                 //(EF.ApprovalStatus == null || EF.ApprovalStatus == "") &&
                                                 //H.RequestStatus.ToLower() == "pending for dh approval"
                                                 //&& H.RequestType.ToLower() == "external"
                                                H.HRRFNumber == hrrfnu &&
                                                 H.CostCenter.ToLower().Contains(bpr.CostCenter.ToLower())
                                                 && bpr.DeliveryHeadID.Contains(emp)
                                                && bpr.TRGrade.Contains(H.Grade.ToString())
                                                 && ((H.Purpose != "Project" && H.ProjectCode == bpr.BenchCode)
                                                 || (H.Purpose == "Project"))

                                                 select EF).Distinct().ToList();
                            if (Externalhrrfs != null)
                            {
                                if (Externalhrrfs.Count > 0)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }



                #region External TRs dh approval code
                //if (Session["EmployeeId"].ToString() == "100816") // Sujit Sahoo
                //{
                //    if (viewResult.HRRFs.Count() == 1)
                //    {
                //        if (viewResult.HRRFs.First().RequestStatus.ToLower() == "pending for dh approval" && viewResult.HRRFs.First().RequestType.ToLower() == "external")
                //        {
                //            if (viewResult.ExternalFulfillments.Count() == 1)
                //            {
                //                if (viewResult.ExternalFulfillments.First().Practice.ToLower() == "sg&a")
                //                {
                //                    if (viewResult.HRRFs.First().CostCenter.ToLower() == "r&km" || viewResult.HRRFs.First().CostCenter.ToLower() == "strategy & research" || viewResult.HRRFs.First().CostCenter.ToLower() == "ceo")
                //                    {
                //                        return true;
                //                    }
                //                }
                //            }
                //        }
                //    }
                //}
                //else if (Session["EmployeeId"].ToString() == "60126") // Anusuya R Chaman
                //{
                //    if (viewResult.HRRFs.Count() == 1)
                //    {
                //        if (viewResult.HRRFs.First().RequestStatus.ToLower() == "pending for dh approval" && viewResult.HRRFs.First().RequestType.ToLower() == "external")
                //        {
                //            if (viewResult.ExternalFulfillments.Count() == 1)
                //            {
                //                if (viewResult.ExternalFulfillments.First().Practice.ToLower() == "sg&a")
                //                {
                //                    if (viewResult.HRRFs.First().CostCenter.ToLower() == "admin" || viewResult.HRRFs.First().CostCenter.ToLower() == "finance")
                //                    {
                //                        return true;
                //                    }
                //                }
                //            }
                //        }
                //    }
                //}
                //else if (Session["EmployeeId"].ToString() == "103406") // Prasad Prabhakar
                //{
                //    if (viewResult.HRRFs.Count() == 1)
                //    {
                //        if (viewResult.HRRFs.First().RequestStatus.ToLower() == "pending for dh approval" && viewResult.HRRFs.First().RequestType.ToLower() == "external")
                //        {
                //            if (viewResult.ExternalFulfillments.Count() == 1)
                //            {
                //                if (viewResult.ExternalFulfillments.First().Practice.ToLower() == "sg&a")
                //                {
                //                    if (viewResult.HRRFs.First().CostCenter.ToLower() == "bdm" || viewResult.HRRFs.First().CostCenter.ToLower() == "hr")
                //                    {
                //                        return true;
                //                    }
                //                }
                //            }
                //        }
                //    }
                //}
                //else if (Session["EmployeeId"].ToString() == "101404") // Prashant V.S. Bhavaraju
                //{
                //    if (viewResult.HRRFs.Count() == 1)
                //    {
                //        if (viewResult.HRRFs.First().RequestStatus.ToLower() == "pending for dh approval" && viewResult.HRRFs.First().RequestType.ToLower() == "external")
                //        {
                //            if (viewResult.ExternalFulfillments.Count() == 1)
                //            {
                //                if (viewResult.ExternalFulfillments.First().Practice.ToLower() == "sg&a")
                //                {
                //                    if (viewResult.HRRFs.First().CostCenter.ToLower() == "marketing")
                //                    {
                //                        return true;
                //                    }
                //                }
                //            }
                //        }
                //    }
                //}
                //else if (Session["EmployeeId"].ToString() == "101034") // Gangadhar Aka

                //{
                //    if (viewResult.HRRFs.Count() == 1)
                //    {
                //        if (viewResult.HRRFs.First().RequestStatus.ToLower() == "pending for dh approval" && viewResult.HRRFs.First().RequestType.ToLower() == "external")
                //        {
                //            if (viewResult.ExternalFulfillments.Count() == 1)
                //            {
                //                if (viewResult.ExternalFulfillments.First().Practice.ToLower() == "sg&a")
                //                {
                //                    if (viewResult.HRRFs.First().CostCenter.ToLower() == "it")
                //                    {
                //                        return true;
                //                    }
                //                }
                //            }
                //        }
                //    }
                //}
                //else if (Session["EmployeeId"].ToString() == "101441") // Vivek	Sambasivam
                //{
                //    if (viewResult.HRRFs.Count() == 1)
                //    {
                //        if (viewResult.HRRFs.First().RequestStatus.ToLower() == "pending for dh approval" && viewResult.HRRFs.First().RequestType.ToLower() == "external")
                //        {
                //            if (viewResult.ExternalFulfillments.Count() == 1)
                //            {
                //                if (viewResult.ExternalFulfillments.First().Practice.Contains("IMS"))
                //                {
                //                    return true;
                //                }
                //            }
                //        }
                //    }
                //}
                //else if (Session["EmployeeId"].ToString() == "103237") // Vishwanath MS
                //{
                //    if (viewResult.HRRFs.Count() == 1)
                //    {
                //        if (viewResult.HRRFs.First().RequestStatus.ToLower() == "pending for dh approval" && viewResult.HRRFs.First().RequestType.ToLower() == "external")
                //        {
                //            if (viewResult.ExternalFulfillments.Count() == 1)
                //            {
                //                if (viewResult.ExternalFulfillments.First().Practice.ToLower() == "cloud solutions")
                //                {
                //                    return true;
                //                }
                //            }
                //        }
                //    }
                //}
                //else if (Session["EmployeeId"].ToString() == "102968") // Ganesh Arunachala
                //{
                //    if (viewResult.HRRFs.Count() == 1)
                //    {
                //        if (viewResult.HRRFs.First().RequestStatus.ToLower() == "pending for dh approval" && viewResult.HRRFs.First().RequestType.ToLower() == "external")
                //        {
                //            if (viewResult.ExternalFulfillments.Count() == 1)
                //            {
                //                if (viewResult.ExternalFulfillments.First().Practice.ToLower() == "it services others" && viewResult.HRRFs.First().CostCenter.ToLower() == "quality")
                //                {
                //                    return true;
                //                }
                //            }
                //        }
                //    }
                //}
                //else
                //{
                //    if (viewResult.HRRFs.Count() == 1)
                //    {
                //        if (viewResult.HRRFs.First().RequestStatus.ToLower() == "pending for dh approval"
                //            && viewResult.HRRFs.First().RequestType.ToLower() == "external")
                //        {
                //            if (viewResult.ExternalFulfillments.Count() == 1)
                //            {
                //                if (viewResult.ExternalFulfillments.First().Practice != "SG&A" &&
                //                    !viewResult.ExternalFulfillments.First().Practice.Contains("IMS") &&
                //                    viewResult.ExternalFulfillments.First().Practice.ToLower() != "cloud solutions")
                //                {
                //                    if (viewResult.ExternalFulfillments.First().Practice.ToLower() != "it services others"
                //                        || (viewResult.ExternalFulfillments.First().Practice.ToLower() == "it services others"
                //                        && viewResult.HRRFs.First().CostCenter.ToLower() != "quality"))
                //                    {
                //                        if (Session["EmployeeId"].ToString() == "103673" && viewResult.HRRFs.First().Grade <= 4) // Ashutosh Uprety
                //                        {
                //                            return true;
                //                        }
                //                        else if (Session["EmployeeId"].ToString() != "103673" && viewResult.HRRFs.First().Grade > 4)
                //                        {
                //                            return true;
                //                        }
                //                    }
                //                }
                //            }
                //        }
                //    }
                //}
                #endregion

                #region Internal TRs dh approval code
                //if (Session["EmployeeId"].ToString() == "100816") // Sujit Sahoo
                //{
                //    if (viewResult.HRRFs.Count() == 1)
                //    {
                //        if (viewResult.HRRFs.First().RequestStatus.ToLower() == "pending for dh approval" &&
                //            viewResult.HRRFs.First().RequestType.ToLower() == "internal" &&
                //            viewResult.HRRFs.First().Practice.ToLower() == "sg&a" &&
                //            (viewResult.HRRFs.First().CostCenter.ToLower() == "r&km" || viewResult.HRRFs.First().CostCenter.ToLower() == "strategy & research" || viewResult.HRRFs.First().CostCenter.ToLower() == "ceo"))
                //        {
                //            if (viewResult.ProposeAssociates.Count() == 1)
                //            {
                //                if (viewResult.ProposeAssociates.First().PracticeStatus.ToLower() == "proposed")
                //                {
                //                    return true;
                //                }
                //            }
                //        }
                //    }
                //}
                //else if (Session["EmployeeId"].ToString() == "60126") // Anusuya R Chaman 
                //{
                //    if (viewResult.HRRFs.Count() == 1)
                //    {
                //        if (viewResult.HRRFs.First().RequestStatus.ToLower() == "pending for dh approval" &&
                //            viewResult.HRRFs.First().RequestType.ToLower() == "internal" &&
                //            viewResult.HRRFs.First().Practice.ToLower() == "sg&a" &&
                //            (viewResult.HRRFs.First().CostCenter.ToLower() == "admin" || viewResult.HRRFs.First().CostCenter.ToLower() == "finance"))
                //        {
                //            if (viewResult.ProposeAssociates.Count() == 1)
                //            {
                //                if (viewResult.ProposeAssociates.First().PracticeStatus.ToLower() == "proposed")
                //                {
                //                    return true;
                //                }
                //            }
                //        }
                //    }
                //}
                //else if (Session["EmployeeId"].ToString() == "103406") // Prasad Prabhakar 
                //{
                //    if (viewResult.HRRFs.Count() == 1)
                //    {
                //        if (viewResult.HRRFs.First().RequestStatus.ToLower() == "pending for dh approval" &&
                //            viewResult.HRRFs.First().RequestType.ToLower() == "internal" &&
                //            viewResult.HRRFs.First().Practice.ToLower() == "sg&a" &&
                //            (viewResult.HRRFs.First().CostCenter.ToLower() == "bdm" || viewResult.HRRFs.First().CostCenter.ToLower() == "hr"))
                //        {
                //            if (viewResult.ProposeAssociates.Count() == 1)
                //            {
                //                if (viewResult.ProposeAssociates.First().PracticeStatus.ToLower() == "proposed")
                //                {
                //                    return true;
                //                }
                //            }
                //        }
                //    }
                //}
                //else if (Session["EmployeeId"].ToString() == "101404") // Prashant V.S. Bhavaraju
                //{
                //    if (viewResult.HRRFs.Count() == 1)
                //    {
                //        if (viewResult.HRRFs.First().RequestStatus.ToLower() == "pending for dh approval" &&
                //            viewResult.HRRFs.First().RequestType.ToLower() == "internal" &&
                //            viewResult.HRRFs.First().Practice.ToLower() == "sg&a" &&
                //            viewResult.HRRFs.First().CostCenter.ToLower() == "marketing")
                //        {
                //            if (viewResult.ProposeAssociates.Count() == 1)
                //            {
                //                if (viewResult.ProposeAssociates.First().PracticeStatus.ToLower() == "proposed")
                //                {
                //                    return true;
                //                }
                //            }
                //        }
                //    }
                //}
                //else if (Session["EmployeeId"].ToString() == "101034") // Gangadhar Aka
                //{
                //    if (viewResult.HRRFs.Count() == 1)
                //    {
                //        if (viewResult.HRRFs.First().RequestStatus.ToLower() == "pending for dh approval" &&
                //            viewResult.HRRFs.First().RequestType.ToLower() == "internal" &&
                //            viewResult.HRRFs.First().Practice.ToLower() == "sg&a" &&
                //            viewResult.HRRFs.First().CostCenter.ToLower() == "it")
                //        {
                //            if (viewResult.ProposeAssociates.Count() == 1)
                //            {
                //                if (viewResult.ProposeAssociates.First().PracticeStatus.ToLower() == "proposed")
                //                {
                //                    return true;
                //                }
                //            }
                //        }
                //    }
                //}
                //else if (Session["EmployeeId"].ToString() == "101441") // Vivek	Sambasivam
                //{
                //    if (viewResult.HRRFs.First().RequestStatus.ToLower() == "pending for dh approval" &&
                //            viewResult.HRRFs.First().RequestType.ToLower() == "internal" &&
                //            viewResult.HRRFs.First().Practice.Contains("IMS"))
                //    {
                //        if (viewResult.ProposeAssociates.Count() == 1)
                //        {
                //            if (viewResult.ProposeAssociates.First().PracticeStatus.ToLower() == "proposed")
                //            {
                //                return true;
                //            }
                //        }
                //    }
                //}
                //else if (Session["EmployeeId"].ToString() == "103237") // Vishwanath MS
                //{
                //    if (viewResult.HRRFs.Count() == 1)
                //    {
                //        if (viewResult.HRRFs.First().RequestStatus.ToLower() == "pending for dh approval" &&
                //            viewResult.HRRFs.First().RequestType.ToLower() == "internal" &&
                //            viewResult.HRRFs.First().Practice.ToLower() == "cloud solutions")
                //        {
                //            if (viewResult.ProposeAssociates.Count() == 1)
                //            {
                //                if (viewResult.ProposeAssociates.First().PracticeStatus.ToLower() == "proposed")
                //                {
                //                    return true;
                //                }
                //            }
                //        }
                //    }
                //}
                //else if (Session["EmployeeId"].ToString() == "102968") // Ganesh Arunachala
                //{
                //    if (viewResult.HRRFs.Count() == 1)
                //    {
                //        if (viewResult.HRRFs.First().RequestStatus.ToLower() == "pending for dh approval" &&
                //            viewResult.HRRFs.First().RequestType.ToLower() == "internal" &&
                //            viewResult.HRRFs.First().Practice.ToLower() == "it services others" &&
                //            viewResult.HRRFs.First().CostCenter.ToLower() == "quality")
                //        {
                //            if (viewResult.ProposeAssociates.Count() == 1)
                //            {
                //                if (viewResult.ProposeAssociates.First().PracticeStatus.ToLower() == "proposed")
                //                {
                //                    return true;
                //                }
                //            }
                //        }
                //    }
                //}
                //else
                //{
                //    if (viewResult.HRRFs.First().RequestStatus.ToLower() == "pending for dh approval" &&
                //            viewResult.HRRFs.First().RequestType.ToLower() == "internal" &&
                //            (viewResult.HRRFs.First().Practice != "SG&G" &&
                //             !viewResult.HRRFs.First().Practice.Contains("IMS") &&
                //             viewResult.HRRFs.First().Practice.ToLower() != "cloud solutions")
                //        )
                //    {
                //        if (viewResult.ProposeAssociates.Count() == 1)
                //        {
                //            if (viewResult.ProposeAssociates.First().PracticeStatus.ToLower() == "proposed")
                //            {
                //                if (viewResult.HRRFs.First().Practice.ToLower() != "it services others"
                //                    || (viewResult.HRRFs.First().Practice.ToLower() == "it services others"
                //                    && viewResult.HRRFs.First().CostCenter.ToLower() != "quality"))
                //                {
                //                    if (Session["EmployeeId"].ToString() == "103673" && viewResult.HRRFs.First().Grade <= 4) // Ashutosh Uprety
                //                    {
                //                        return true;
                //                    }
                //                    else if (Session["EmployeeId"].ToString() != "103673" && viewResult.HRRFs.First().Grade > 4)
                //                    {
                //                        return true;
                //                    }
                //                }
                //            }
                //        }
                //    }
                //}
                #endregion
            }

            return false;
        }

        //public ActionResult AssociateSearch(string SearchEmployee)
        //{
        //    List<Employee> emplist = new List<Employee>();
        //    var emp1 = (from e in db.Employees where e.EmployeeId.ToString().Equals(SearchEmployee) select e);
        //    //emp1 = emp1.Where(s => s.EmployeeId.ToString().Contains(SearchEmployee)).ToList();
        //    return View();
        //}
        /// <summary>
        /// Get Hrrf based on HRRF Number
        /// </summary>
        /// <param name="HRRFNO"></param>
        /// <returns></returns>
        public List<HRRF> GetHRRF(string HRRFNO)
        {
            // get based on sessions logic here before loading
            var hrrfLog = db.HRRFs.Where(h => h.HRRFNumber == HRRFNO).ToList();
            foreach (var item in (List<HRRF>)hrrfLog)
            {
                string strPractice = item.Practice;
                string strcostcenter = item.CostCenter;
                int? intGrade = item.Grade;
                string strRolerequired = item.RoleRequired;
                IFormatProvider theCultureInfo = new System.Globalization.CultureInfo("en-us", true);
                DesignationMaster objDesignationMaster = db.DesignationMasters.Where(dm => dm.Practice.Contains(strPractice) && dm.Grade == intGrade && dm.CostCenter.Contains(item.CostCenter) && dm.DesignationCode == strRolerequired).FirstOrDefault();

                if (objDesignationMaster != null)
                {
                    item.RoleRequired = objDesignationMaster.DesignationName;
                }
                else
                {
                    item.RoleRequired = strRolerequired;
                }
            }

            // getting hrrf skills by smiley 
            //var mstrlookup = (from skill in db.HRRFSkills
            //                  join masterLkup in db.MasterLookUps on skill.Skills equals masterLkup.LookupID
            //                  where skill.HRRFNumber == HRRFNO
            //                  select new
            //                  {
            //                      masterLkup.Description
            //                  }).ToList();
            //var SkillData = string.Join(",", mstrlookup.Select(s => s.Description).ToArray());

            var skilmast_value = (from skill in db.HRRFSkills_ExpertiseLevel
                                  join skilmast in db.SkillMasters on skill.SkillId equals skilmast.SkillId
                                  where skill.HRRFNumber == HRRFNO
                                  select new
                                  {
                                      skilmast.Skillset
                                  }).ToList();
            var SkillData = string.Join(",", skilmast_value.Select(s => s.Skillset).ToArray());
            ViewData["SkillData"] = SkillData;
            ViewData["HRRFData"] = hrrfLog.ToList();
            return hrrfLog.ToList();
        }

        /// <summary>
        /// Get Propose Associates for the HRRFnumber
        /// </summary>
        /// <param name="HRRFNO"></param>
        /// <returns></returns>
        public List<ProposeAssociate> GetProposeAssociates(string HRRFNO)
        {
            // get based on sessions logic here before loading
            var propLog = db.ProposeAssociates.Where(h => h.HRRFNumber == HRRFNO && h.PracticeStatus != praticeReject).ToList();
            ViewData["ProposeAssociatesData"] = propLog.ToList();
            return propLog.ToList();
        }

        /// <summary>
        /// Get Bench employees based on skill match 
        /// </summary>
        /// <returns></returns>
        public List<BenchEmployees> GetListOfSearchedRecommendResources(string EmpId)
        {
            List<BenchEmployees> lstBenchEmployes = new List<BenchEmployees>();

            Int32 EmployeeID = 0;
            #region skillmatch

            bool isNumeric = false;
            if (EmpId != null)
            {
                int n;
                isNumeric = int.TryParse(EmpId, out n);

                if (isNumeric)
                {
                    EmployeeID = Convert.ToInt32(EmpId);
                }

            }
            var skillMatch1 = (from empskills in db.EmployeeSkills
                               join hrrfskills in db.HRRFSkills on empskills.Skills equals hrrfskills.Skills
                               join hrrf in db.HRRFs on hrrfskills.HRRFNumber equals hrrf.HRRFNumber
                               join emp in db.Employees on empskills.EmployeeId equals emp.EmployeeId
                               join prjAssgn in db.ProjectAssignments on empskills.EmployeeId equals prjAssgn.EmployeeId into tempPrjAssgn
                               from prjAssgn in tempPrjAssgn.DefaultIfEmpty()
                               where hrrfskills.IsPrimary == empskills.IsPrimary
                                 && emp.IsActive == true && prjAssgn.IsActive == true
                               && empskills.Status == skill_Approved //&& emp.EmployeeId == EmployeeID
                               select new
                               {
                                   hrrfskills.Skills,
                                   empskills.SkillDescription,
                                   hrrfskills.Rating,
                                   hrrfskills.IsPrimary,
                                   emp.FirstName,
                                   emp.LastName,
                                   emp.EmployeeId,
                                   emp.Location,
                                   prjAssgn.EndDate
                               }).ToList();

            if (isNumeric)
            {
                skillMatch1 = skillMatch1.Where(x => x.EmployeeId.Equals(EmployeeID)).ToList();

                if (skillMatch1.Count == 0)
                {
                    skillMatch1 = (from e in db.Employees
                                   where e.EmployeeId.Equals(EmployeeID)
                                   select new
                                   {
                                       Skills = (long?)0,
                                       SkillDescription = "",
                                       Rating = (int?)0,
                                       IsPrimary = (bool?)true,
                                       FirstName = e.FirstName,
                                       LastName = e.LastName,
                                       EmployeeId = e.EmployeeId,
                                       Location = e.Location,
                                       EndDate = e.ModifiedDate
                                   }).ToList();
                }
            }
            else
            {
                skillMatch1 = skillMatch1.Where(x => x.SkillDescription.ToLower().Contains(EmpId.ToLower())).ToList();
            }


            #endregion
            if (skillMatch1.Count == 0)
            {
                //TempData["notsuccess"] = "Skills not found for this EmployeeId";
                TempData["notsuccess"] = "Skills not found.";
            }

            var lstDistskillMatch1 = skillMatch1.GroupBy(x => x.EmployeeId).Select(y => y.First()).ToList();

            #region Below code is commented and implemented by Sarath on 22 Mar 2016

            foreach (var test in lstDistskillMatch1)
            {
                BenchEmployees objBenchEmployes = new BenchEmployees();
                objBenchEmployes.EmployeeId = Convert.ToInt32(test.EmployeeId);
                objBenchEmployes.EmployeeName = test.FirstName + " " + test.LastName;
                objBenchEmployes.Location = test.Location;
                //var skillresult = skillMatch.Where(s => s.EmployeeId == test.EmployeeId).ToList();
                var skillresult = skillMatch1.GroupBy(x => x.EmployeeId).Select(y => y.First()).ToList();


                //Primary skills filter
                var lstprimarySkill = skillresult.Where(x => x.IsPrimary == true && x.EmployeeId == objBenchEmployes.EmployeeId).ToList();
                if (lstprimarySkill.Count > 0)
                {
                    var primaryskill1 = string.Join(",", lstprimarySkill.Select(s => s.SkillDescription).ToArray());
                    objBenchEmployes.PrimarySkills = primaryskill1;
                }
                //secondary skills filter
                var lstsecondarySkill = skillresult.Where(x => x.IsPrimary == false && x.EmployeeId == objBenchEmployes.EmployeeId).ToList();
                if (lstsecondarySkill.Count > 0)
                {
                    var secondaryskill2 = string.Join(",", lstsecondarySkill.Select(s => s.SkillDescription).ToArray());
                    objBenchEmployes.SecondarySkills = secondaryskill2;
                }
                //skills Rating sum based on hrrf matched skills
                List<int?> rating = skillresult.Where(x => x.EmployeeId == objBenchEmployes.EmployeeId).Select(r => r.Rating).ToList();
                int? sumRating = rating.Sum();
                objBenchEmployes.Rating = sumRating;
                lstBenchEmployes.Add(objBenchEmployes);
            }
            #endregion

            string HRRFNumber = Session["hrrfno"].ToString();
            var skillMatch = (from empskills in db.EmployeeSkills
                              join hrrfskills in db.HRRFSkills on empskills.Skills equals hrrfskills.Skills
                              //join masterLkup in db.MasterLookUps on hrrfskills.Skills equals masterLkup.LookupID
                              join hrrf in db.HRRFs on hrrfskills.HRRFNumber equals hrrf.HRRFNumber
                              join emp in db.Employees on empskills.EmployeeId equals emp.EmployeeId
                              join prjAssgn in db.ProjectAssignments on empskills.EmployeeId equals prjAssgn.EmployeeId into tempPrjAssgn
                              from prjAssgn in tempPrjAssgn.DefaultIfEmpty()
                              where HRRFNumber == hrrfskills.HRRFNumber && hrrf.HRRFNumber == HRRFNumber
                              && hrrfskills.IsPrimary == empskills.IsPrimary
                              && emp.IsActive == true && prjAssgn.IsActive == true
                              && empskills.Status == skill_Approved
                              && hrrf.AssignmentStartDate > prjAssgn.EndDate
                              && (hrrf.Grade == emp.Grade || hrrf.Grade - 1 == emp.Grade)
                              //&& emp.ResourceStatus == emp_ResourceStatus     --  because of not valid data from EMployee in DB


                              select new
                              {
                                  hrrfskills.Skills,
                                  empskills.SkillDescription,
                                  //masterLkup.Description,
                                  hrrfskills.Rating,
                                  hrrfskills.IsPrimary,
                                  emp.FirstName,
                                  emp.LastName,
                                  emp.EmployeeId,
                                  emp.Location,
                                  prjAssgn.EndDate
                              }).ToList();

            //#endregion


            var lstDistskillMatch = skillMatch.GroupBy(x => x.EmployeeId).Select(y => y.First()).ToList();


            foreach (var test in lstDistskillMatch)
            {
                BenchEmployees objBenchEmployes = new BenchEmployees();
                objBenchEmployes.EmployeeId = Convert.ToInt32(test.EmployeeId);
                objBenchEmployes.EmployeeName = test.FirstName + " " + test.LastName;
                objBenchEmployes.Location = test.Location;

                #region CodeforSkillmatchedRecords 
                //Priyanka Ganapuram 
                var skillresult = skillMatch.Where(s => s.EmployeeId == test.EmployeeId).ToList();
                //var skillresult = skillMatch.GroupBy(x => x.EmployeeId).Select(y => y.First()).ToList();

                var skillMatchData = (from hrrf in db.HRRFSkills
                                      join emp in db.EmployeeSkills on hrrf.Skills equals emp.Skills
                                      where emp.Status == skill_Approved
                                      where hrrf.HRRFNumber == HRRFNumber && hrrf.IsPrimary == emp.IsPrimary
                                      select new
                                      {
                                          hrrf.Skills,
                                          emp.SkillDescription,
                                          hrrf.Rating,
                                          hrrf.IsPrimary,
                                          emp.EmployeeId
                                      }).ToList();
                var skillresultSet = skillMatchData.Where(s => s.EmployeeId == test.EmployeeId).ToList();

                #endregion

                //Primary skills filter
                var lstprimarySkill = skillresultSet.Where(x => x.IsPrimary == true).ToList();
                if (lstprimarySkill.Count > 0)
                {
                    var primaryskill1 = string.Join(",", lstprimarySkill.Select(s => s.SkillDescription).ToArray());
                    objBenchEmployes.PrimarySkills = primaryskill1;
                }
                //secondary skills filter
                var lstsecondarySkill = skillresultSet.Where(x => x.IsPrimary == false).ToList();
                if (lstsecondarySkill.Count > 0)
                {
                    var secondaryskill2 = string.Join(",", lstsecondarySkill.Select(s => s.SkillDescription).ToArray());
                    objBenchEmployes.SecondarySkills = secondaryskill2;
                }
                //skills Rating sum based on hrrf matched skills
                List<int?> rating = skillresultSet.Select(r => r.Rating).ToList();
                int? sumRating = rating.Sum();
                objBenchEmployes.Rating = sumRating;
                lstBenchEmployes.Add(objBenchEmployes);
            }


            ViewData["BenchEmployeesWithMatchedSkills"] = lstBenchEmployes;
            return lstBenchEmployes;
        }
        public List<TRMagicData> GetListOfSearchedRecommendResourcesbymagic(string EmpId, string Hrrfn, string skillbillingst, string searchFromDate, string searchToDate)
        {
            List<TRMagicData> lstBenchEmployes = new List<TRMagicData>();

            string EmployeeID = "";

            List<TRMagicData> lstHRRF = new List<TRMagicData>();
            if (skillbillingst == "Search")
            {
                #region EmpSearch

                if (EmpId != null && EmpId != string.Empty)
                {

                    EmployeeID = EmpId;
                    lstHRRF = db.Database.SqlQuery<TRMagicData>("exec GetTrMagicForEmployee @hrrf_number, @EmployeeID",
           new System.Data.SqlClient.SqlParameter("hrrf_number", Hrrfn),
            new System.Data.SqlClient.SqlParameter("EmployeeID", EmployeeID)).ToList<TRMagicData>();



                }

                #endregion
            }
            else
            {
                #region Filter search
                DateTime? stdate = null;
                DateTime? etdate = null;
                if (skillbillingst == "Dates")
                {
                    stdate = Convert.ToDateTime(searchFromDate);
                    etdate = Convert.ToDateTime(searchToDate);

                }
                else
                {
                    stdate = null;
                    etdate = null;
                }
                if (skillbillingst != string.Empty && skillbillingst != null)
                    skillbillingst = (skillbillingst == "All") ? null : skillbillingst;
                else
                    skillbillingst = "Bench";


                lstHRRF = db.Database.SqlQuery<TRMagicData>("exec GetEmployeeDetailsByHRRFNumber @hrrf_number, @start_date,@end_date, @grade,@skills,@Billing_status,@location,@Practice",

               new System.Data.SqlClient.SqlParameter("hrrf_number", Hrrfn == null ? (object)DBNull.Value : Hrrfn),
                   // new System.Data.SqlClient.SqlParameter("start_date", ValueOrDBNullIfZero(stdate)),
                   new System.Data.SqlClient.SqlParameter("start_date", stdate == null ? (object)DBNull.Value : stdate),
                   new System.Data.SqlClient.SqlParameter("end_date", etdate == null ? (object)DBNull.Value : etdate),
                    new System.Data.SqlClient.SqlParameter("grade", (object)DBNull.Value),
                     new System.Data.SqlClient.SqlParameter("skills", (object)DBNull.Value),
                      new System.Data.SqlClient.SqlParameter("Billing_status", skillbillingst == null ? (object)DBNull.Value : skillbillingst),
                       new System.Data.SqlClient.SqlParameter("location", (object)DBNull.Value),
                        new System.Data.SqlClient.SqlParameter("Practice", (object)DBNull.Value)).ToList<TRMagicData>();
                #endregion
            }

            ViewData["BenchEmployeesWithMatchedSkills"] = lstHRRF;
            return lstHRRF;
        }

        public ActionResult GetSearchData(string EmpId, string HRRFNO, string BillingStatus, string Startdate, string Enddate)
        {
            try { 
            var hrrfLog = db.HRRFs.Where(h => h.HRRFNumber == HRRFNO).ToList();
            GetListOfSearchedRecommendResourcesbymagic(EmpId, HRRFNO, BillingStatus, Startdate, Enddate);
            var skillMatchResourceResult = ViewData["BenchEmployeesWithMatchedSkills"] as List<TRMagicData>;
            ViewData["HRRFDataserch"] = hrrfLog.ToList();
            var HRRFDataResult = ViewData["HRRFDataserch"] as List<HRRF>;
            var viewResult = new ValidationModel()
            {
                HRRFs = HRRFDataResult,
                //BenchEmployee = skillMatchResourceResult
                TRMagicDetails = skillMatchResourceResult

            };
            return PartialView("_PartialProposedAssociate", viewResult);
            }
            catch (Exception ex)
            {

                Common.WriteExceptionErrorLog(ex);
                return RedirectToAction("Error", "Error");
            }
        }

        public List<TRMagicData> GetListOfRecommendResourcesbymagic(string Hrrfn)
        {
            List<TRMagicData> lstBenchEmployes = new List<TRMagicData>();
            
            #region EmpSearch


            DateTime? stdate = null;
            DateTime? etdate = null;

            string skillbillingst = "Bench";


            var lstHRRF = db.Database.SqlQuery<TRMagicData>("exec GetEmployeeDetailsByHRRFNumber @hrrf_number, @start_date,@end_date, @grade,@skills,@Billing_status,@location,@Practice",
                          new System.Data.SqlClient.SqlParameter("hrrf_number", Hrrfn),
                                new System.Data.SqlClient.SqlParameter("start_date", stdate == null ? (object)DBNull.Value : stdate),
                new System.Data.SqlClient.SqlParameter("end_date", etdate == null ? (object)DBNull.Value : etdate),
                 new System.Data.SqlClient.SqlParameter("grade", (object)DBNull.Value),
                  new System.Data.SqlClient.SqlParameter("skills", (object)DBNull.Value),
                   new System.Data.SqlClient.SqlParameter("Billing_status", skillbillingst == null ? (object)DBNull.Value : skillbillingst),
                    new System.Data.SqlClient.SqlParameter("location", (object)DBNull.Value),
                     new System.Data.SqlClient.SqlParameter("Practice", (object)DBNull.Value)).ToList<TRMagicData>();

            #endregion
            ViewData["BenchEmployeesWithMatchedSkills"] = lstHRRF;
            return lstHRRF;
        }
        public List<ProposedResourcesList> GetListOfResourceList(string Hrrfn, string PracticeStatus)
        {

            List<ProposedResourcesList> lstHRRF = new List<ProposedResourcesList>();
            var propsedlist = "";

            if (PracticeStatus != "" && PracticeStatus != string.Empty)
            {
                var propLog = string.Join(",", db.ProposeAssociates.Where(p => p.HRRFNumber == Hrrfn && p.PracticeStatus != PracticeStatus)
                                .Select(p => p.EmpID.ToString()));
                propsedlist = propLog;
            }
            else
            {
                var propLog = string.Join(",", db.ProposeAssociates.Where(p => p.HRRFNumber == Hrrfn)
                                                .Select(p => p.EmpID.ToString()));
                propsedlist = propLog;
            }

            #region EmpSearch

            if (propsedlist != null && propsedlist != string.Empty)
            {

                lstHRRF = db.Database.SqlQuery<ProposedResourcesList>("exec GetTrMagicForEmployee @hrrf_number, @EmployeeID",
                new System.Data.SqlClient.SqlParameter("hrrf_number", Hrrfn),
                new System.Data.SqlClient.SqlParameter("EmployeeID", propsedlist)).ToList<ProposedResourcesList>();
            }

            #endregion
            ViewData["ProposeAssociatesData"] = lstHRRF;
            return lstHRRF;
        }
        public ActionResult GetAssignmentDetails(string EMPID)
        {
            try { 
            int empi = 0;
            empi = Convert.ToInt32(EMPID);
            var etdet = db.Database.SqlQuery<EmpoloyeeAssigmentDetails>("exec GetEMployeeAssignmnetDetails @EmployeeID",
                                                           new System.Data.SqlClient.SqlParameter("EmployeeID", empi)).ToList<EmpoloyeeAssigmentDetails>();

            var viewResult = new ValidationModel()
            {
                EMPProjectAssignmentDetails = etdet

            };

            return PartialView("_DisplayEmployeeAssigments", viewResult);
            }
            catch (Exception ex)
            {

                Common.WriteExceptionErrorLog(ex);
                return Json("Error", JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetEmployeeSkill(string EmpID)
        {
            try { 
            Int32 EmployeeId = Convert.ToInt32(EmpID);

            var objStudent = db.Database.SqlQuery<EmployeeSkills_NewDetails>("exec GetEmployeeSkills @EmployeeId",
                  new System.Data.SqlClient.SqlParameter("EmployeeId", EmployeeId)).ToList<EmployeeSkills_NewDetails>();

            var viewResult = new ValidationModel()
            {
                EmployeeSkillInfo = objStudent

            };

            return PartialView("_DisplayEmployeeSkill", viewResult);
            }
            catch (Exception ex)
            {

                Common.WriteExceptionErrorLog(ex);
                return Json("Error", JsonRequestBehavior.AllowGet);
            }
        }

        public List<BenchEmployees> GetListRecommendResources()
        {
            List<BenchEmployees> lstBenchEmployes = new List<BenchEmployees>();
            string HRRFNumber = Session["hrrfno"].ToString();
            //Int32 EmployeeID = 0;
            #region skillmatch

            var skillMatch = (from empskills in db.EmployeeSkills
                              join hrrfskills in db.HRRFSkills on empskills.Skills equals hrrfskills.Skills
                              //join masterLkup in db.MasterLookUps on hrrfskills.Skills equals masterLkup.LookupID
                              join hrrf in db.HRRFs on hrrfskills.HRRFNumber equals hrrf.HRRFNumber
                              join emp in db.Employees on empskills.EmployeeId equals emp.EmployeeId
                              join prjAssgn in db.ProjectAssignments on empskills.EmployeeId equals prjAssgn.EmployeeId into tempPrjAssgn
                              from prjAssgn in tempPrjAssgn.DefaultIfEmpty()
                              where HRRFNumber == hrrfskills.HRRFNumber && hrrf.HRRFNumber == HRRFNumber
                              && hrrfskills.IsPrimary == empskills.IsPrimary
                              && empskills.Status == skill_Approved
                              && prjAssgn.EndDate > hrrf.AssignmentStartDate
                              && (hrrf.Grade == emp.Grade || hrrf.Grade - 1 == emp.Grade)
                              //&& emp.ResourceStatus == emp_ResourceStatus     --  because of not valid data from EMployee in DB
                              && emp.IsActive == true

                              select new
                              {
                                  hrrfskills.Skills,
                                  empskills.SkillDescription,
                                  //masterLkup.Description,
                                  hrrfskills.Rating,
                                  hrrfskills.IsPrimary,
                                  emp.FirstName,
                                  emp.LastName,
                                  emp.EmployeeId,
                                  emp.Location,
                                  prjAssgn.EndDate
                              }).ToList();

            #endregion


            var lstDistskillMatch = skillMatch.GroupBy(x => x.EmployeeId).Select(y => y.First()).ToList();


            foreach (var test in lstDistskillMatch)
            {
                BenchEmployees objBenchEmployes = new BenchEmployees();
                objBenchEmployes.EmployeeId = Convert.ToInt32(test.EmployeeId);
                objBenchEmployes.EmployeeName = test.FirstName + " " + test.LastName;
                objBenchEmployes.Location = test.Location;

                #region CodeforSkillmatchedRecords 
                //Priyanka Ganapuram 
                var skillresult = skillMatch.Where(s => s.EmployeeId == test.EmployeeId).ToList();
                //var skillresult = skillMatch.GroupBy(x => x.EmployeeId).Select(y => y.First()).ToList();

                var skillMatchData = (from hrrf in db.HRRFSkills
                                      join emp in db.EmployeeSkills on hrrf.Skills equals emp.Skills
                                      where emp.Status == skill_Approved
                                      where hrrf.HRRFNumber == HRRFNumber && hrrf.IsPrimary == emp.IsPrimary
                                      select new
                                      {
                                          hrrf.Skills,
                                          emp.SkillDescription,
                                          hrrf.Rating,
                                          hrrf.IsPrimary,
                                          emp.EmployeeId
                                      }).ToList();
                var skillresultSet = skillMatchData.Where(s => s.EmployeeId == test.EmployeeId).ToList();

                #endregion

                //Primary skills filter
                var lstprimarySkill = skillresultSet.Where(x => x.IsPrimary == true).ToList();
                if (lstprimarySkill.Count > 0)
                {
                    var primaryskill1 = string.Join(",", lstprimarySkill.Select(s => s.SkillDescription).ToArray());
                    objBenchEmployes.PrimarySkills = primaryskill1;
                }
                //secondary skills filter
                var lstsecondarySkill = skillresultSet.Where(x => x.IsPrimary == false).ToList();
                if (lstsecondarySkill.Count > 0)
                {
                    var secondaryskill2 = string.Join(",", lstsecondarySkill.Select(s => s.SkillDescription).ToArray());
                    objBenchEmployes.SecondarySkills = secondaryskill2;
                }
                //skills Rating sum based on hrrf matched skills
                List<int?> rating = skillresultSet.Select(r => r.Rating).ToList();
                int? sumRating = rating.Sum();
                objBenchEmployes.Rating = sumRating;
                lstBenchEmployes.Add(objBenchEmployes);
            }
            ViewData["BenchEmployeesWithMatchedSkills"] = lstBenchEmployes;
            return lstBenchEmployes;
        }
        
        /// <summary>
        /// Saving Propose associcates
        /// </summary>
        /// <param name="chkAssociateId"></param>
        /// <param name="HRRFNumber"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult RecommendProposeAssociate(string[] chkAssociateId, string HRRFNumber)
        {
            try
            {
                HRRFNumber = Session["hrrfno"].ToString();


                TempData["hrrfno"] = HRRFNumber;
                List<string> recommendedalready = new List<string>();
                if (chkAssociateId != null && HRRFNumber != null)
                {
                    bool chkforpropsed = true;
                    string errormsg = "";
                    foreach (var CheckedItem1 in chkAssociateId)
                    {

                        int id = Convert.ToInt32(CheckedItem1);
                        var objEmployee = db.Employees.Where(b => b.EmployeeId == id).ToList().FirstOrDefault();

                        var objprojectAssignment = db.ProjectAssignments.Where(b => b.EmployeeId == objEmployee.EmployeeId).ToList().FirstOrDefault();


                        var objHRRF = db.HRRFs.Where(c => c.HRRFNumber == HRRFNumber).FirstOrDefault();

                        var objPropose = db.ProposeAssociates.Where(p => p.EmpID == CheckedItem1 &&
                                            (p.PracticeStatus == "Accepted")).ToList();


                        chkforpropsed = objEmployee.DateOfJoin.Date <= Convert.ToDateTime(objHRRF.BillingDate).Date ? true : false;  //Emp DateOfJoin check with BillingDate

                        if (chkforpropsed)
                        {
                            //tr billing ,endate need to check with previous are future projects
                            var objpras1 = db.ProjectAssignments.Where(b => b.EmployeeId == objEmployee.EmployeeId && !(b.BillingStatus.Contains("Bench")) &&
                            ((b.StartDate >= objHRRF.BillingDate && b.StartDate <= objHRRF.AssignmentEndDate) || (b.EndDate >= objHRRF.BillingDate && b.EndDate <= objHRRF.AssignmentEndDate)
                            || (b.StartDate <= objHRRF.BillingDate && b.EndDate >= objHRRF.AssignmentEndDate))).ToList();

                            if (objpras1.Count > 0)
                            {
                                var sumUtilization = objpras1.Sum(p => p.Utilization);
                                if ((sumUtilization + Convert.ToInt32(objHRRF.Utilization)) > 100)
                                {
                                    chkforpropsed = false;
                                    errormsg = objEmployee.FirstName + " already assigned to some other project with selected dates";
                                    break;
                                }
                            }



                        }
                        else
                        {
                            errormsg = "Employee date of joining should be less than billing date of the TR.";
                            break;
                        }

                        //}
                    }
                    if (!chkforpropsed)
                    {
                        recommendedalready.Add(errormsg);
                    }

                }
                if (recommendedalready.Count > 0)
                {
                    var recommendedAlready = string.Join(",", recommendedalready.ToArray());
                }
                return Json(recommendedalready);
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
        public JsonResult ResourceProposement(string[] chkAssociateId, string HRRFNumber)
        {
            JsonResult js = new JsonResult();
            try
            {
                HRRFNumber = Session["hrrfno"].ToString();
                TempData["hrrfno"] = HRRFNumber;
                if (chkAssociateId != null && HRRFNumber != null)
                {

                    string strPrevReqStatus = "";
                    var hrrfResultQualify = db.HRRFs.Where(h => h.HRRFNumber == HRRFNumber).ToList().FirstOrDefault();
                    var projectDetails = db.Projects.Where(a => a.ProjectName == hrrfResultQualify.ProjectName).FirstOrDefault();
                    if (hrrfResultQualify != null)
                    {
                        int HRRFCreatedBy = Convert.ToInt32(hrrfResultQualify.HRRFCreatedBy);

                        if (HRRFCreatedBy != 0)
                        {
                            RoleMaster objRoleMaster = db.RoleMasters.Where(r => r.EmployeeId == HRRFCreatedBy && r.ApplicationCode == "TALENTREQ").FirstOrDefault();
                            if (objRoleMaster == null) // by default he/she is PM.
                            {
                                strPrevReqStatus = hrrfResultQualify.RequestStatus;
                                hrrfResultQualify.RequestStatus = PendingForPMApproval;
                            }
                            else
                            {
                                if (objRoleMaster.Role != role_OM)
                                //if (objRoleMaster.Role == role_PM)
                                {
                                    strPrevReqStatus = hrrfResultQualify.RequestStatus;
                                    hrrfResultQualify.RequestStatus = PendingForPMApproval;
                                }
                                else
                                {
                                    strPrevReqStatus = hrrfResultQualify.RequestStatus;
                                    hrrfResultQualify.RequestStatus = qualified;
                                }
                            }
                        }
                    }
                    foreach (var CheckedItem in chkAssociateId)
                    {
                        int id = Convert.ToInt32(CheckedItem);
                        var objEmployee = db.Employees.Where(b => b.EmployeeId == id).ToList().FirstOrDefault();

                        var objHRRF = db.HRRFs.Where(c => c.HRRFNumber == HRRFNumber).FirstOrDefault();


                        #region SkillMatchRecords 
                        var skillMatchData = (from hrrf in db.HRRFSkills_ExpertiseLevel
                                              join emp in db.EmployeeSkills_New on hrrf.SkillId equals emp.SkillId
                                              join sm in db.SkillMasters on emp.SkillId equals sm.SkillId
                                              where emp.SkillStatus == skill_Approved
                                              where hrrf.HRRFNumber == HRRFNumber && hrrf.IsMandatoy == true
                                              select new
                                              {
                                                  sm.Skillset,
                                                  hrrf.ExpertiseLevel,
                                                  hrrf.IsMandatoy,
                                                  emp.EmployeeId
                                              }).ToList();
                        var skillresult = skillMatchData.Where(s => s.EmployeeId == id).ToList();
                        #endregion

                        #region SaveProposeResource

                        //remove previous propsed records of this employee
                        bool rlechk = true;
                        var roled = db.RoleMasters.Where(p => p.EmployeeId == objHRRF.HRRFCreatedBy && p.ApplicationCode == "TALENTREQ").FirstOrDefault();
                        if (roled != null)
                        {
                            if (roled.Role == "OM")
                            {
                                rlechk = false;
                                var proposerec = db.ProposeAssociates.Where(x => x.EmpID == CheckedItem).ToList();
                                if (proposerec != null)
                                {
                                    foreach (var mt in proposerec)
                                    {
                                        if (mt.PracticeStatus == "Proposed" && mt.HRRFNumber != HRRFNumber)
                                        {
                                            var hrrfcr = db.HRRFs.Where(x => x.HRRFNumber == mt.HRRFNumber && x.RequestStatus != "Fulfilled").FirstOrDefault();
                                            if (hrrfcr != null)
                                            {
                                                InsertNotification(Session["EmployeeId"].ToString(), Convert.ToString(hrrfcr.HRRFCreatedBy),
                                                    "This is to inform you that " + objEmployee.FirstName + " " + objEmployee.LastName + "(" + CheckedItem + ")" + " Resource accepted for another TR so we are removing this resource from your propsed list for the " + mt.HRRFNumber,
                                                    "Proposed Resource Removing", mt.HRRFNumber, "<b>" +
                                                    Environment.GetEnvironmentVariable("ProposedResource") ?? "" + "</b>" + mt.EmpName, null);


                                                string strPrevReqStatus1 = hrrfcr.RequestStatus;
                                                var prop = db.ProposeAssociates.Where(i => i.HRRFNumber == mt.HRRFNumber
                                                           && i.PracticeStatus == "Proposed" && i.EmpID != CheckedItem).ToList();
                                                if (prop.Count() == 0)
                                                {
                                                    hrrfcr.RequestStatus = "Qualified";

                                                }

                                                TRScernarioHistroy(mt.HRRFNumber, objEmployee.FirstName + " " + objEmployee.MiddleName + " " + objEmployee.LastName
                                                    + " - " + "accepted for another TR removing the resource from propsed list for the" + " - " + HRRFNumber,
                                                    null, Convert.ToInt32(Session["EmployeeId"]), strPrevReqStatus1);

                                            }
                                            ProposeAssociate pt = db.ProposeAssociates.Find(mt.ProposedID);
                                            db.ProposeAssociates.Remove(pt);
                                        }
                                        else if (mt.PracticeStatus == "Rejected")
                                        {
                                            ProposeAssociate pt = db.ProposeAssociates.Find(mt.ProposedID);
                                            db.ProposeAssociates.Remove(pt);
                                        }

                                    }
                                }
                            }
                        }
                        if (rlechk)
                        {
                            var proposerec = db.ProposeAssociates.Where(x => x.EmpID == CheckedItem && x.HRRFNumber == HRRFNumber).ToList();
                            if (proposerec != null)
                            {
                                foreach (var mt in proposerec)
                                {
                                    if (mt.PracticeStatus == "Rejected")
                                    {
                                        ProposeAssociate pt = db.ProposeAssociates.Find(mt.ProposedID);
                                        db.ProposeAssociates.Remove(pt);
                                    }
                                }
                            }
                        }
                        ProposeAssociate objProposeAssociate = new ProposeAssociate();
                        objProposeAssociate.HRRFNumber = HRRFNumber;
                        objProposeAssociate.EmpID = CheckedItem;
                        objProposeAssociate.EmpName = objEmployee.FirstName + " " + objEmployee.LastName;
                        objProposeAssociate.ProjectName = objHRRF.ProjectName;
                        objProposeAssociate.ProjectCode = objHRRF.ProjectCode;
                        objProposeAssociate.EmpRole = string.Empty;
                        objProposeAssociate.Grade = objEmployee.Grade.ToString();
                        // matched list of skills for employee as per HRRF/TR
                        if (skillresult.Count > 0)
                        {
                            var skills = string.Join(",", skillresult.Select(s => s.Skillset).ToArray());
                            objProposeAssociate.Skills = skills;
                        }
                        objProposeAssociate.Practice = objEmployee.Practice;//  objBenchEmployee.Practice;
                                                                            // objProposeAssociate.AssociateAcceptanceStatus = string.Empty;
                                                                            // objProposeAssociate.AssociateRemarks = string.Empty;
                        objProposeAssociate.DeliveryStatus = delivery_recommend;
                        objProposeAssociate.DeliveryRemarks = string.Empty;
                        objProposeAssociate.PracticeStatus = practice_recommend;
                        objProposeAssociate.ProposedBy = Session["EmployeeId"].ToString(); //objHRRF.HRRFCreatedBy.ToString();
                        objProposeAssociate.ProposedDate = DateTime.Now;
                        objProposeAssociate.ApprovedBy = objHRRF.ModifiedBy.ToString();
                        // objProposeAssociate.ApproverEmailId = objHRRF.RequesterEmailId;
                        objProposeAssociate.ApprovedDate = DateTime.Now;
                        objProposeAssociate.ModifiedBy = objHRRF.ModifiedBy;
                        objProposeAssociate.ModifiedDate = DateTime.Now;
                        objProposeAssociate.HRRFID = objHRRF.HRRFID;

                        db.ProposeAssociates.Add(objProposeAssociate);
                        //HRRF Histroy
                        TRScernarioHistroy(HRRFNumber, objEmployee.FirstName + " " + objEmployee.LastName + " " + "has been Proposed for" + " - " + HRRFNumber, objProposeAssociate.DeliveryRemarks, Convert.ToInt32(Session["EmployeeId"]), strPrevReqStatus);
                        //saving proposed resources
                        db.SaveChanges();
                        // configure smtp mail server here 
                        string deliverymanager = "";
                        if (projectDetails.DELIVERY_MANAGER_ID != null && projectDetails.DELIVERY_MANAGER_ID > 0)
                        {
                            deliverymanager = (projectDetails.DELIVERY_MANAGER_ID).ToString();
                        }
                        if (projectDetails.ProjectManagerId != null && projectDetails.ProjectManagerId > 0)
                        {
                            if (deliverymanager == "")
                                deliverymanager = (projectDetails.ProjectManagerId).ToString();
                            else
                                deliverymanager = deliverymanager + "," + (projectDetails.ProjectManagerId).ToString();
                        }
                        int empid = Convert.ToInt32(CheckedItem);
                        var proposedEmpName = db.Employees.Where(p => p.EmployeeId == empid).FirstOrDefault();
                        var isvalidbusinessgroup = db.IncludedBussinessGroups.Where(i => i.BusinessGroup.Contains(proposedEmpName.BusinessGroup)).FirstOrDefault();
                        var isinvalidemployetype = db.ExcludedEmployeeTypes.Where(i => i.EmployeeType.Contains(proposedEmpName.EmployeeType)).FirstOrDefault();
                        //added ops team in cc
                        InsertNotification(Session["EmployeeId"].ToString(), Convert.ToString(objHRRF.HRRFCreatedBy), Environment.GetEnvironmentVariable("TrProposedResources") ?? "", Environment.GetEnvironmentVariable("TrProposedRes_NOtification") ?? "", HRRFNumber, "<b>" + Environment.GetEnvironmentVariable("ProposedResource") ?? "" + "</b>" + proposedEmpName.FirstName + proposedEmpName.LastName, Environment.GetEnvironmentVariable("ITS-OPS") ?? "");
                        //lakshmi
                        if (isvalidbusinessgroup != null && isinvalidemployetype == null)
                        {
                            InsertNotification(Session["EmployeeId"].ToString(), CheckedItem, Environment.GetEnvironmentVariable("Proposedto") ?? "" + objHRRF.ProjectName, Environment.GetEnvironmentVariable("TrProposedRes_NOtification") ?? "", HRRFNumber, string.Empty, deliverymanager);
                        }
                        InsertNotification(Session["EmployeeId"].ToString(), Session["EmployeeId"].ToString(), Environment.GetEnvironmentVariable("TrProposedResources_by") ?? "", Environment.GetEnvironmentVariable("TrProposedRes_NOtification") ?? "", HRRFNumber, string.Empty, null);

                        #endregion
                        js.Data = true;

                    }
                }
            }
            //catch (Exception ex)
            //{
            //    js.Data = false;
            //}
            catch (Exception ex)
            {

                Common.WriteExceptionErrorLog(ex);
                return Json("Error", JsonRequestBehavior.AllowGet);
            }
            js.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return js;
        }
        /// <summary>
        /// Accept Proposed Resources   - PH
        /// </summary>
        /// <param name="ProposedId"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult AcceptProposedResource(string[] ProposedId)
        {
            try
            {
                string HRRFNumber = Session["hrrfno"].ToString();
                TempData["hrrfno"] = HRRFNumber;
                List<string> strAlreadyAcceptedList = new List<string>();
                int loginempID = Convert.ToInt32(Session["EmployeeId"]);

                foreach (string proposedEmpid in ProposedId)
                {
                    int? Utilzation = null;
                    #region SeudoCode
                    //change status
                    //remove bench record with that empid
                    //remove proposed candidate
                    // mail to employee and head  
                    #endregion
                    int id = Convert.ToInt32(proposedEmpid);
                    var objEmployee = db.Employees.Where(b => b.EmployeeId == id).ToList().FirstOrDefault();
                    var proposerec = db.ProposeAssociates.Where(x => x.EmpID == proposedEmpid).ToList();
                    if (proposerec != null)
                    {
                        foreach (var mt in proposerec)
                        {
                            if (mt.PracticeStatus == "Proposed" && mt.HRRFNumber != HRRFNumber)
                            {
                                var hrrfcr = db.HRRFs.Where(x => x.HRRFNumber == mt.HRRFNumber && x.RequestStatus != "Fulfilled" && x.RequestStatus != "Cancelled").FirstOrDefault();
                                if (hrrfcr != null)
                                {
                                    InsertNotification(Session["EmployeeId"].ToString(), Convert.ToString(hrrfcr.HRRFCreatedBy),
                                        "This is to inform you that " + objEmployee.FirstName + " " + objEmployee.LastName + "(" + ProposedId + ")" + " Resource accepted for another TR so we are removing this resource from your propsed list for the " + mt.HRRFNumber,
                                        "Proposed Resource Removing", mt.HRRFNumber, "<b>" +
                                        Environment.GetEnvironmentVariable("ProposedResource") ?? "" + "</b>" + mt.EmpName, null);
                                    string strPrevReqStatus = hrrfcr.RequestStatus;
                                    var prop = db.ProposeAssociates.Where(i => i.HRRFNumber == mt.HRRFNumber
                                               && i.PracticeStatus == "Proposed" && i.EmpID != proposedEmpid).ToList();
                                    if (prop.Count() == 0)
                                    {
                                        hrrfcr.RequestStatus = "Qualified";

                                    }

                                    TRScernarioHistroy(mt.HRRFNumber, objEmployee.FirstName + " " + objEmployee.MiddleName + " " + objEmployee.LastName
                                        + " - " + "accepted for another TR removing the resource from propsed list for the" + " - " + HRRFNumber,
                                        null, Convert.ToInt32(Session["EmployeeId"]), strPrevReqStatus);

                                    //var hrrfhistr = db.HRRFHistories.Where(x => x.HRRFNumber == mt.HRRFNumber && x.PrevRequestStatus == "Qualified").OrderByDescending(x => x.ModifiedDate).FirstOrDefault();
                                    //if (hrrfhistr != null)
                                    //{
                                    //    HRRFHistory pti = db.HRRFHistories.Find(hrrfhistr.AuditID, 0);
                                    //    db.HRRFHistories.Remove(pti);
                                    //}

                                }
                                ProposeAssociate pt = db.ProposeAssociates.Find(mt.ProposedID);
                                db.ProposeAssociates.Remove(pt);

                            }
                            else if (mt.PracticeStatus == "Accepted")
                            {
                                ProposeAssociate pt = db.ProposeAssociates.Find(mt.ProposedID);
                                db.ProposeAssociates.Remove(pt);

                            }
                            db.SaveChanges();
                        }
                    }

                    int propsedEmployeeid = Convert.ToInt32(proposedEmpid);
                    var objEmployeeAssignment = db.Employees.Where(a => a.EmployeeId == propsedEmployeeid).FirstOrDefault();
                    var propsedLog = db.ProposeAssociates.Where(h => h.EmpID == proposedEmpid && h.PracticeStatus == praticeAccept).ToList();
                    if (propsedLog.Count < 1)
                    {
                        // check for no of positions
                        var objHrrfNoofPostions = db.HRRFs.Where(c => c.HRRFNumber == HRRFNumber).FirstOrDefault();
                        if (objHrrfNoofPostions != null)
                        {
                            var objHRRF = db.HRRFs.Where(c => c.HRRFNumber == HRRFNumber).FirstOrDefault();
                            var objPropose = db.ProposeAssociates.Where(p => p.HRRFNumber == HRRFNumber && p.EmpID == proposedEmpid).FirstOrDefault();
                            var objProposePostions = db.ProposeAssociates.Where(p => p.HRRFNumber == HRRFNumber && p.PracticeStatus == praticeAccept).ToList();


                            DateTime dtStartDate = Convert.ToDateTime(objHRRF.BillingDate);
                            DateTime dtEndDate = Convert.ToDateTime(objHRRF.AssignmentEndDate);

                            var objpras1 = db.ProjectAssignments.Where(b => b.EmployeeId == propsedEmployeeid
                            && !(b.BillingStatus.Contains("Bench")) && ((b.StartDate >= dtStartDate
                            && b.StartDate <= dtEndDate) || (b.EndDate >= dtStartDate && b.EndDate <= dtEndDate) ||
                            (b.StartDate <= dtStartDate && b.EndDate >= dtEndDate))).ToList();
                            if (objpras1.Count() > 0)
                            {
                                var sumUtilization = objpras1.Sum(p => p.Utilization);
                                Utilzation = 100 - sumUtilization;
                            }

                            //to checkprevious Bench 
                            #region Existing Bench Record
                            var previousprojectass = db.ProjectAssignments.Where(p => p.IsActive == true && p.EmployeeId == propsedEmployeeid && p.BillingStatus == "Bench").ToList();
                            var dtProjectAssignSatrtAsEndDate = db.ProjectAssignments.Where(pa => pa.EmployeeId == propsedEmployeeid
                                            && DateTime.Now <= pa.StartDate).OrderBy(pa => pa.StartDate).Select(pa => pa.StartDate).FirstOrDefault();
                            var dtProjectAssignEndDate = db.ProjectAssignments.Where(pa => pa.EmployeeId == propsedEmployeeid && pa.IsActive == true && pa.BillingStatus != "Bench"
                                   && DateTime.Now <= pa.EndDate).OrderBy(pa => pa.EndDate).Select(pa => pa.EndDate).FirstOrDefault();
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
                                    pash.modifiedBy = loginempID;
                                    pash.ModifiedDate = DateTime.Now;


                                    #region Exisitng Bench Record Update based on selected Dates
                                    // Selecting top 1 Start Date order by desc to Set Bench Enda date
                                    int? uil = null;
                                    if (DateTime.Now.Date >= (Convert.ToDateTime(objHRRF.BillingDate).Date))
                                    {
                                        if (Utilzation == null)
                                        {
                                            uil = 100 - Convert.ToInt32(objHRRF.Utilization);
                                        }
                                        else
                                        {
                                            uil = Utilzation - Convert.ToInt32(objHRRF.Utilization);
                                        }
                                    }
                                    else
                                        uil = lt.Utilization;

                                    if (dtProjectAssignSatrtAsEndDate != null &&
                                        Convert.ToDateTime(dtProjectAssignSatrtAsEndDate).Date <= Convert.ToDateTime(objHRRF.BillingDate).Date)
                                    {
                                        lt.EndDate = Convert.ToDateTime(dtProjectAssignSatrtAsEndDate);
                                    }
                                    else if (dtProjectAssignEndDate != null && dtProjectAssignEndDate <= Convert.ToDateTime(objHRRF.AssignmentEndDate) && DateTime.Now.Date.Equals(Convert.ToDateTime(objHRRF.BillingDate).Date))
                                    {
                                        if (Convert.ToDateTime(objHRRF.BillingDate) < dtProjectAssignEndDate)
                                            lt.EndDate = Convert.ToDateTime(objHRRF.BillingDate);
                                        else
                                            lt.EndDate = Convert.ToDateTime(dtProjectAssignEndDate);
                                    }
                                    else if (DateTime.Now.Date >= (Convert.ToDateTime(objHRRF.BillingDate).Date))
                                    {
                                        if (uil > 0)
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
                                    // billdatge == getdate


                                    if (uil > 0)
                                    {
                                        lt.Utilization = uil;
                                    }
                                    else
                                        lt.IsActive = false;

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
                                if (DateTime.Now.Date >= Convert.ToDateTime(objHRRF.BillingDate).Date)
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
                                else if (DateTime.Now.Date < Convert.ToDateTime(objHRRF.BillingDate).Date)
                                {
                                    benchUtilization = 100;
                                }
                                if (benchUtilization > 0)
                                {
                                    #region First Bench Record if Bench record not exist for the Employee
                                    DateTime stdat = new DateTime();
                                    DateTime etdat = new DateTime();

                                    if (DateTime.Now.Date >= Convert.ToDateTime(objHRRF.BillingDate).Date)
                                    {
                                        stdat = Convert.ToDateTime(objHRRF.BillingDate);
                                    }
                                    else
                                    {
                                        stdat = DateTime.Now.Date;
                                    }
                                    if (dtProjectAssignSatrtAsEndDate != null && dtProjectAssignSatrtAsEndDate <= Convert.ToDateTime(objHRRF.BillingDate) && DateTime.Now.Date.Equals(Convert.ToDateTime(objHRRF.BillingDate).Date))
                                    {
                                        etdat = Convert.ToDateTime(dtProjectAssignSatrtAsEndDate);
                                    }
                                    else if (dtProjectAssignEndDate != null && dtProjectAssignEndDate <= Convert.ToDateTime(objHRRF.AssignmentEndDate) && DateTime.Now.Date.Equals(Convert.ToDateTime(objHRRF.BillingDate).Date))
                                    {
                                        if (Convert.ToDateTime(objHRRF.BillingDate) < dtProjectAssignEndDate)
                                            etdat = Convert.ToDateTime(objHRRF.BillingDate);
                                        else
                                            etdat = Convert.ToDateTime(dtProjectAssignEndDate);
                                    }
                                    else if (DateTime.Now.Date >= (Convert.ToDateTime(objHRRF.BillingDate).Date))
                                    {
                                        etdat = Convert.ToDateTime(objHRRF.AssignmentEndDate);
                                    }
                                    else
                                    {
                                        etdat = Convert.ToDateTime(objHRRF.BillingDate);
                                    }


                                    bool dojbenck = true;

                                    var objpras11 = db.ProjectAssignments.Where(b => b.EmployeeId == propsedEmployeeid
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

                                        if (objEmployeeAssignment.CostCenter.ToLower() != "testing" && unass != null)
                                        {

                                            unass = unass.Where(p => p.Practice.ToLower() == objEmployeeAssignment.Practice.ToLower()).ToList();
                                        }
                                        if (unass.Count > 0)
                                        {
                                            ProjectAssignment projAssignment = new ProjectAssignment();
                                            projAssignment.ProjectCode = unass.FirstOrDefault().Projectcode;
                                            projAssignment.ProjectID = unass.FirstOrDefault().projectid;
                                            projAssignment.ProjectName = unass.FirstOrDefault().ProjectName;
                                            projAssignment.StartDate = stdat;
                                            projAssignment.EndDate = etdat;
                                            projAssignment.Utilization = benchUtilization;
                                            projAssignment.EmployeeId = propsedEmployeeid;
                                            // need to check future date and active and inactive
                                            projAssignment.IsActive = true;
                                            projAssignment.Assigned_By = unass.FirstOrDefault().projmanger;
                                            projAssignment.Assigned_Date = System.DateTime.Now;
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
                                            projBenchHistory.EmployeeId = propsedEmployeeid;
                                            projBenchHistory.EnddateOld = null;
                                            projBenchHistory.IsActiveOld = null;
                                            projBenchHistory.StartDateOld = null;
                                            projBenchHistory.UtilizationOld = null;
                                            projBenchHistory.modifiedBy = loginempID;
                                            projBenchHistory.ModifiedDate = DateTime.Now;
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


                            ProjectAssignment objProjectAssignment = new ProjectAssignment();
                            // Check with no of positions
                            // proposed resource count
                            int positions = Convert.ToInt32(objProposePostions.Count);
                            //hrrf postions count
                            int hrrfpositions = Convert.ToInt32(objHRRF.Positions);
                            if (positions < hrrfpositions)
                            {
                                //object mapping
                                var objProposing = db.ProposeAssociates.Where(p => p.EmpID == proposedEmpid && p.HRRFNumber == HRRFNumber).FirstOrDefault();
                                objHRRF.ResourceName = objPropose.EmpID + "(" + objPropose.EmpName + ")";
                                objProposing.PracticeStatus = praticeAccept;
                                db.Entry(objProposing).State = System.Data.Entity.EntityState.Modified;
                                // fulfill positions
                                var objHrrfFulfillment = db.HRRFs.Where(p => p.HRRFNumber == HRRFNumber).FirstOrDefault();
                                string strPrevReqStatus = objHrrfFulfillment.RequestStatus;
                                objHrrfFulfillment.RequestStatus = fulfilled;
                                objHrrfFulfillment.ModifiedDate = DateTime.Now;
                                db.Entry(objHrrfFulfillment).State = System.Data.Entity.EntityState.Modified;
                                // HRRF histroy 
                                TRScernarioHistroy(HRRFNumber, objPropose.EmpName + " - " + "has been Accepted for" + " - " + HRRFNumber, objHrrfFulfillment.Remarks, Convert.ToInt32(Session["EmployeeId"]), strPrevReqStatus);
                                TRScernarioHistroy(HRRFNumber, objPropose.EmpName + " - " + "has been Fulfilled for" + " - " + HRRFNumber, objHrrfFulfillment.Remarks, Convert.ToInt32(Session["EmployeeId"]), strPrevReqStatus);
                                // Project Assignment & Change Assignment status for Employee in EmployeeTable


                                #region ManagerAssignment
                                // get project ManagerId
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
                                    objProjectAssignment.EmployeeId = propsedEmployeeid;
                                    objProjectAssignment.Utilization = utl;
                                    // need to check future date and active and inactive
                                    if (DateTime.Now.Date >= Convert.ToDateTime(objHRRF.BillingDate).Date)
                                    {
                                        objProjectAssignment.IsActive = true;
                                    }
                                    else
                                    {
                                        objProjectAssignment.IsActive = false;
                                    }
                                    objProjectAssignment.Assigned_By = name;
                                    objProjectAssignment.Assigned_Date = System.DateTime.Now;
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
                                    pash.EmployeeId = propsedEmployeeid;
                                    pash.EnddateOld = null;
                                    pash.IsActiveOld = null;
                                    pash.StartDateOld = null;
                                    pash.UtilizationOld = null;
                                    pash.modifiedBy = loginempID;
                                    pash.ModifiedDate = DateTime.Now;
                                    pash.UtilizationNew = utl;
                                    pash.StartDateNew = objHRRF.BillingDate;
                                    if (DateTime.Now.Date >= Convert.ToDateTime(objHRRF.BillingDate).Date)
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
                                            prj1.EmployeeId = propsedEmployeeid;
                                            prj1.ProjectID = unass[0].projectid;
                                            prj1.ProjectCode = unass[0].Projectcode;
                                            prj1.ProjectName = unass[0].ProjectName;
                                            prj1.StartDate = objHRRF.BillingDate;
                                            prj1.EndDate = objHRRF.AssignmentEndDate;
                                            if (DateTime.Now.Date >= Convert.ToDateTime(objHRRF.BillingDate).Date)
                                            {
                                                prj1.IsActive = true;
                                            }
                                            else
                                            {
                                                prj1.IsActive = false;
                                            }
                                            prj1.Assigned_By = unass[0].projmanger;
                                            prj1.Assigned_Date = DateTime.Now;
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
                                            pash.EmployeeId = propsedEmployeeid;
                                            pash.EnddateOld = null;
                                            pash.IsActiveOld = null;
                                            pash.StartDateOld = null;
                                            pash.UtilizationOld = null;
                                            pash.modifiedBy = loginempID;
                                            pash.ModifiedDate = DateTime.Now;
                                            pash.UtilizationNew = utl;
                                            pash.StartDateNew = objHRRF.BillingDate;
                                            if (DateTime.Now.Date >= Convert.ToDateTime(objHRRF.BillingDate).Date)
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

                                // final saving after assigning and updating
                                db.SaveChanges();
                                var proposedEmpName = db.Employees.Where(p => p.EmployeeId == propsedEmployeeid).FirstOrDefault();
                                var isvalidbusinessgroup = db.IncludedBussinessGroups.Where(i => i.BusinessGroup.Contains(proposedEmpName.BusinessGroup)).FirstOrDefault();
                                var isinvalidemployetype = db.ExcludedEmployeeTypes.Where(i => i.EmployeeType.Contains(proposedEmpName.EmployeeType)).FirstOrDefault();
                                // email configure added ops team in cc
                                InsertNotification(Session["EmployeeId"].ToString(), Convert.ToString(objHRRF.HRRFCreatedBy), Environment.GetEnvironmentVariable("TrAcceptedResources") ?? "", Environment.GetEnvironmentVariable("TrProposedResAce_NOtification") ?? "", HRRFNumber, "<b>" + Environment.GetEnvironmentVariable("AcceptedResource") ?? "" + ":</b>" + proposedEmpName.FirstName + proposedEmpName.LastName, Environment.GetEnvironmentVariable("ITS-OPS") ?? ""+","+ Environment.GetEnvironmentVariable("Assurance") ?? "");
                                //lakshmi
                                if (isvalidbusinessgroup != null && isinvalidemployetype == null)
                                {
                                    InsertNotification(Session["EmployeeId"].ToString(), proposedEmpid, Environment.GetEnvironmentVariable("Acceptedto") ?? "" + objHRRF.ProjectName, Environment.GetEnvironmentVariable("AcceptedResource") ?? "", HRRFNumber, string.Empty, null);
                                }
                                InsertNotification(Session["EmployeeId"].ToString(), Session["EmployeeId"].ToString(), Environment.GetEnvironmentVariable("TrAcceptedResources_by") ?? "", Environment.GetEnvironmentVariable("TrProposedRes_NOtification") ?? "" + objHRRF.ProjectName, HRRFNumber, string.Empty, null);
                            }
                        }
                    }
                    // to give alert to user for accepted resources -- PENDING
                    else if (propsedLog.Count >= 1)
                    {
                        var propsed = db.ProposeAssociates.Where(h => h.EmpID == proposedEmpid && h.HRRFNumber == HRRFNumber).FirstOrDefault();
                        string empName = propsed.EmpName;
                        strAlreadyAcceptedList.Add(empName);
                    }
                    DataSet retVal = new DataSet();
                    string ConnectionString = System.Environment.GetEnvironmentVariable("ADO_CONNECTION_STRING") ?? ConfigurationManager.ConnectionStrings["ADOConnectionString"].ConnectionString;
                    //EntityConnection entityConn = (EntityConnection)db.Database.Connection;
                    SqlConnection sqlConn = new SqlConnection(ConnectionString);
                    try
                    {

                        SqlCommand cmdReport = new SqlCommand("PrcAssignDefaultBenchUsingGrade", sqlConn);
                        SqlDataAdapter daReport = new SqlDataAdapter(cmdReport);
                        using (cmdReport)
                        {
                            SqlParameter empIdPram = new SqlParameter("@EmployeeId", proposedEmpid);
                            cmdReport.CommandType = CommandType.StoredProcedure;
                            cmdReport.Parameters.Add(empIdPram);
                            daReport.Fill(retVal);
                        }

                    }
                    //catch (Exception ex)
                    //{
                    //    ErrorHandling expcls = new ErrorHandling();
                    //    expcls.Error(ex);
                    //    return null;
                    //}
                    catch (Exception ex)
                    {

                        Common.WriteExceptionErrorLog(ex);
                        return Json("Error", JsonRequestBehavior.AllowGet);
                    }
                    finally
                    {
                        sqlConn.Close();
                    }
                }
                var AlreadyAccepted = "";
                if (strAlreadyAcceptedList.Count > 0)
                {
                    AlreadyAccepted = string.Join(",", strAlreadyAcceptedList.ToArray());
                }

                return Json(AlreadyAccepted);
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

        #region oldcode
        //commentby hemanth
        //public JsonResult AcceptProposedResource(string[] ProposedId)
        //{
        //    try
        //    {
        //        string HRRFNumber = Session["hrrfno"].ToString();
        //        TempData["hrrfno"] = HRRFNumber;
        //        List<string> strAlreadyAcceptedList = new List<string>();
        //        foreach (string proposedEmpid in ProposedId)
        //        {
        //            #region SeudoCode
        //            //change status
        //            //remove bench record with that empid
        //            //remove proposed candidate
        //            // mail to employee and head  
        //            #endregion
        //            int propsedEmployeeid = Convert.ToInt32(proposedEmpid);
        //            var propsedLog = db.ProposeAssociates.Where(h => h.EmpID == proposedEmpid && h.PracticeStatus == praticeAccept).ToList();
        //            if (propsedLog.Count < 1)
        //            {
        //                // check for no of positions
        //                var objHrrfNoofPostions = db.HRRFs.Where(c => c.HRRFNumber == HRRFNumber).FirstOrDefault();
        //                if (objHrrfNoofPostions != null)
        //                {
        //                    var objHRRF = db.HRRFs.Where(c => c.HRRFNumber == HRRFNumber).FirstOrDefault();
        //                    var objPropose = db.ProposeAssociates.Where(p => p.HRRFNumber == HRRFNumber && p.EmpID == proposedEmpid).FirstOrDefault();
        //                    var objProposePostions = db.ProposeAssociates.Where(p => p.HRRFNumber == HRRFNumber && p.PracticeStatus == praticeAccept).ToList();
        //                    // Check with no of positions
        //                    // proposed resource count
        //                    int positions = Convert.ToInt32(objProposePostions.Count);
        //                    //hrrf postions count
        //                    int hrrfpositions = Convert.ToInt32(objHRRF.Positions);
        //                    if (positions < hrrfpositions)
        //                    {
        //                        //object mapping
        //                        var objProposing = db.ProposeAssociates.Where(p => p.EmpID == proposedEmpid && p.HRRFNumber == HRRFNumber).FirstOrDefault();
        //                        objHRRF.ResourceName = objPropose.EmpName;
        //                        objProposing.PracticeStatus = praticeAccept;
        //                        db.Entry(objProposing).State = System.Data.Entity.EntityState.Modified;
        //                        // fulfill positions
        //                        var objHrrfFulfillment = db.HRRFs.Where(p => p.HRRFNumber == HRRFNumber).FirstOrDefault();
        //                        string strPrevReqStatus = objHrrfFulfillment.RequestStatus;
        //                        objHrrfFulfillment.RequestStatus = fulfilled;
        //                        objHrrfFulfillment.ModifiedDate = DateTime.Now;
        //                        db.Entry(objHrrfFulfillment).State = System.Data.Entity.EntityState.Modified;
        //                        // HRRF histroy 
        //                        TRScernarioHistroy(HRRFNumber, objPropose.EmpName + " - " + "has been Accepted for" + " - " + HRRFNumber, objHrrfFulfillment.Remarks, Convert.ToInt32(Session["EmployeeId"]), strPrevReqStatus);
        //                        TRScernarioHistroy(HRRFNumber, objPropose.EmpName + " - " + "has been Fulfilled for" + " - " + HRRFNumber, objHrrfFulfillment.Remarks, Convert.ToInt32(Session["EmployeeId"]), strPrevReqStatus);
        //                        // Project Assignment & Change Assignment status for Employee in EmployeeTable
        //                        #region ProjectAssignment
        //                        var objProjectAssignment = db.ProjectAssignments.Where(a => a.EmployeeId == propsedEmployeeid).FirstOrDefault();
        //                        if (objProjectAssignment != null)
        //                        {
        //                            objProjectAssignment.ProjectCode = objHRRF.ProjectCode;
        //                            objProjectAssignment.ProjectID = Convert.ToInt32(objHRRF.HRRFID);
        //                            objProjectAssignment.ProjectName = objHRRF.ProjectName;
        //                            objProjectAssignment.StartDate = objHRRF.AssignmentStartDate;
        //                            objProjectAssignment.EndDate = objHRRF.AssignmentEndDate;
        //                            db.Entry(objProjectAssignment).State = System.Data.Entity.EntityState.Modified;
        //                        }
        //                        #endregion
        //                        #region ManagerAssignment
        //                        // get project ManagerId
        //                        var objProject = db.Projects.Where(a => a.ProjectCode == objHRRF.ProjectCode || a.ProjectName == objHRRF.ProjectName).FirstOrDefault();
        //                        if (objProject != null)
        //                        {
        //                            // Assignment status
        //                            var objEmployeeAssignment = db.Employees.Where(a => a.EmployeeId == propsedEmployeeid).FirstOrDefault();
        //                            objEmployeeAssignment.AssignmentStatus = emp_Assignment;
        //                            objEmployeeAssignment.ProjectManagerId = objProject.ProjectManagerId;
        //                            db.Entry(objEmployeeAssignment).State = System.Data.Entity.EntityState.Modified;
        //                        }
        //                        #endregion
        //                        // final saving after assigning and updating
        //                        db.SaveChanges();
        //                        var proposedEmpName = db.Employees.Where(p => p.EmployeeId == propsedEmployeeid).FirstOrDefault();
        //                        // email configure
        //                        InsertNotification(Session["EmployeeId"].ToString(), Convert.ToString(objHRRF.HRRFCreatedBy), Environment.GetEnvironmentVariable("TrAcceptedResources") ?? ConfigurationManager.AppSettings["TrAcceptedResources"], Environment.GetEnvironmentVariable("TrProposedResAce_NOtification") ?? ConfigurationManager.AppSettings["TrProposedResAce_NOtification"], HRRFNumber, Environment.GetEnvironmentVariable("AcceptedResource") ?? ConfigurationManager.AppSettings["AcceptedResource"] + proposedEmpName.FirstName + proposedEmpName.LastName);
        //                        InsertNotification(Session["EmployeeId"].ToString(), proposedEmpid, Environment.GetEnvironmentVariable("Acceptedto") ?? ConfigurationManager.AppSettings["Acceptedto"], Environment.GetEnvironmentVariable("Acceptedto") ?? ConfigurationManager.AppSettings["Acceptedto"] + objHRRF.ProjectName, HRRFNumber, string.Empty);
        //                        InsertNotification(Session["EmployeeId"].ToString(), Session["EmployeeId"].ToString(), Environment.GetEnvironmentVariable("TrAcceptedResources_by") ?? ConfigurationManager.AppSettings["TrAcceptedResources_by"], Environment.GetEnvironmentVariable("TrProposedRes_NOtification") ?? ConfigurationManager.AppSettings["TrProposedRes_NOtification"] + objHRRF.ProjectName, HRRFNumber, string.Empty);
        //                    }
        //                }
        //            }
        //            // to give alert to user for accepted resources -- PENDING
        //            else if (propsedLog.Count >= 1)
        //            {
        //                var propsed = db.ProposeAssociates.Where(h => h.EmpID == proposedEmpid && h.HRRFNumber == HRRFNumber).FirstOrDefault();
        //                string empName = propsed.EmpName;
        //                strAlreadyAcceptedList.Add(empName);
        //            }
        //        }
        //        var AlreadyAccepted = "";
        //        if (strAlreadyAcceptedList.Count > 0)
        //        {
        //            AlreadyAccepted = string.Join(",", strAlreadyAcceptedList.ToArray());
        //        }

        //        return Json(AlreadyAccepted);
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
        #endregion

        /// <summary>
        /// Reject Proposed Resources   - PH
        /// </summary>
        /// <param name="ProposedId"></param>
        /// <param name="_Msg"></param>
        /// <returns></returns>
        public ActionResult RejectProposedResource(string[] ProposedId, string _Msg)
        {
            try
            {
                string HRRFNumber = Session["hrrfno"].ToString();
                TempData["hrrfno"] = HRRFNumber;

                foreach (string pid in ProposedId)
                {
                    int propid = Convert.ToInt32(pid);
                    // change status  of propose resource to Rejected                 
                    var propsedLog = db.ProposeAssociates.Where(h => h.EmpID == pid && h.HRRFNumber == HRRFNumber).ToList().FirstOrDefault();
                    if (propsedLog != null)
                    {
                        var objHRRF = db.HRRFs.Where(c => c.HRRFNumber == HRRFNumber).FirstOrDefault();

                        var ExistpropsedLog = db.ProposeAssociates.Where(x => x.HRRFNumber == HRRFNumber
                        && x.EmpID != pid && x.PracticeStatus == "Proposed").ToList();
                        string strPrevRequestStatus = objHRRF.RequestStatus;
                        if (ExistpropsedLog.Count() == 0)
                        {
                            objHRRF.RequestStatus = qualified;
                        }

                        propsedLog.PracticeRemarks = _Msg;
                        propsedLog.PracticeStatus = praticeReject;
                        db.Entry(propsedLog).State = System.Data.Entity.EntityState.Modified;

                        TRScernarioHistroy(HRRFNumber, propsedLog.EmpName + " " + "has been Rejected for" + " - " + HRRFNumber, propsedLog.PracticeRemarks, Convert.ToInt32(Session["EmployeeId"]), strPrevRequestStatus);
                        db.SaveChanges();
                        int empid = Convert.ToInt32(pid);
                        var proposedEmpName = db.Employees.Where(p => p.EmployeeId == empid).FirstOrDefault();

                        InsertNotification(Session["EmployeeId"].ToString(), Convert.ToString(objHRRF.HRRFCreatedBy), Environment.GetEnvironmentVariable("TrRejectedResources") ?? "", Environment.GetEnvironmentVariable("TrProposedResRej_NOtification") ?? "", HRRFNumber, Environment.GetEnvironmentVariable("RejectedResource") ?? "" + proposedEmpName.FirstName + proposedEmpName.LastName + "<br><b>" + Environment.GetEnvironmentVariable("Remarks") ?? "" + "</b>" + _Msg, null);
                        InsertNotification(Session["EmployeeId"].ToString(), Session["EmployeeId"].ToString(), Environment.GetEnvironmentVariable("TrRejectedResources_by") ?? "", Environment.GetEnvironmentVariable("TrProposedResRej_NOtification") ?? "", HRRFNumber, "<br><b>" + Environment.GetEnvironmentVariable("Remarks") ?? "" + "</b>" + _Msg, null);
                    }
                }


                IList<TRHistory> _history = new List<TRHistory>();

                var trHistoryobj = db.HRRFHistories.Where(h => h.HRRFNumber == HRRFNumber).OrderByDescending(c => c.ModifiedDate).Take(5).ToList();

                foreach (var hr in trHistoryobj)
                {
                    TRHistory objTRHistory = new TRHistory();
                    objTRHistory.ModifiedDate = hr.ModifiedDate;
                    objTRHistory.HistoryDescription = hr.HistoryDescription;
                    objTRHistory.ModifiedEmpName = db.Employees.Where(e => e.EmployeeId == hr.ModifiedBy).Select(c => c.FirstName + " " + c.LastName).FirstOrDefault();
                    _history.Add(objTRHistory);
                    //hr.ModifiedEmpName = db.Employees.Where(e => e.EmployeeId == hr.ModifiedBy).Select(c=>c.FirstName+" "+c.LastName).FirstOrDefault();
                }

                ViewData["HRRFHistory"] = _history;

                //var viewResult1 = new ValidationModel()
                //{
                //    // ProposeAssociates = db.ProposeAssociates.ToList(),
                //    ProposeAssociates = db.ProposeAssociates.Where(r => r.PracticeStatus != praticeReject && r.HRRFNumber == HRRFNumber).ToList(),
                //    HRRFs = db.HRRFs.Where(h => h.HRRFNumber == HRRFNumber),
                //    BenchEmployee = GetListRecommendResources(),
                //    ExternalFulfillments = db.ExternalFulfillments.Where(f => f.HRRFNumber == HRRFNumber).ToList()
                //};
                return Json("", JsonRequestBehavior.AllowGet);
                //    return View("ProposeAssociate", viewResult1);
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

        /// <summary>
        /// Qualify HRRF
        /// </summary>
        /// <returns></returns>
        public ActionResult QualifyHRRF(string InternalExpectedFulfilmentDate,string inputDetails)
        {
            try
            {
                string HRRFNumber = Session["hrrfno"].ToString();
                //TempData["hrrfno"] = HRRFNumber;
                var hrrfResultQualify = db.HRRFs.Where(h => h.HRRFNumber == HRRFNumber).ToList().FirstOrDefault();
                var projectDetails = db.Projects.Where(a => a.ProjectName == hrrfResultQualify.ProjectName).FirstOrDefault();
                hrrfResultQualify.RequestStatus = qualified;
                IFormatProvider theCultureInfo = new System.Globalization.CultureInfo("en-us", true);


                //DateTime result = DateTime.ParseExact(InternalExpectedFulfilmentDate, "dd/MM/yyyy", theCultureInfo);
        

                //hrrfResultQualify.InternalExpectedFulfilmentDate = result;//DateTime.ParseExact(InternalExpectedFulfilmentDate, "g", theCultureInfo);
                hrrfResultQualify.InternalExpectedFulfilmentDate = DateTime.Parse(InternalExpectedFulfilmentDate);
                hrrfResultQualify.Remarks = inputDetails;
                db.Entry(hrrfResultQualify).State = System.Data.Entity.EntityState.Modified;
                //hrrf histroy
                //  TRScernarioHistroy(HRRFNumber, HRRFNumber + " - " + "has been Qualified","", Convert.ToInt32(Session["EmployeeId"]), hrrfResultQualify.RequestStatus);
                var objHrrfHistory = new HRRFHistory();
                objHrrfHistory.HRRFNumber = HRRFNumber;
                objHrrfHistory.Remarks = inputDetails;
                objHrrfHistory.HistoryDescription = HRRFNumber + " - " + "has been Qualified";
                objHrrfHistory.ModifiedBy = Convert.ToInt32(Session["EmployeeId"]);
                objHrrfHistory.ModifiedDate = DateTime.Now;
                objHrrfHistory.PrevRequestStatus = Environment.GetEnvironmentVariable("TR_Submitted") ?? "";
                db.HRRFHistories.Add(objHrrfHistory);
                db.SaveChanges();

                if (Session["EmployeeId"] != null)
                {
                    // Email Notifications
                    Employee objEmp = db.Employees.Find(Session["EmployeeId"]);
                    string strQualifiedBy = "";

                    if (objEmp != null)
                    {
                        strQualifiedBy = objEmp.FirstName.Trim() + (!string.IsNullOrWhiteSpace(objEmp.MiddleName) ? " " + objEmp.MiddleName + " " : " ") + objEmp.LastName.Trim();
                        string deliverymanager = "";
                        if (projectDetails.DELIVERY_MANAGER_ID != null && projectDetails.DELIVERY_MANAGER_ID > 0)
                        {
                            deliverymanager = (projectDetails.DELIVERY_MANAGER_ID).ToString();
                        }
                        if (projectDetails.ProjectManagerId != null && projectDetails.ProjectManagerId > 0)
                        {
                            if (deliverymanager == "")
                                deliverymanager = (projectDetails.ProjectManagerId).ToString();
                            else
                                deliverymanager = deliverymanager + "," + (projectDetails.ProjectManagerId).ToString();
                        }
                        // mail to TR Created By
                        InsertNotification(Convert.ToString(Session["EmployeeId"]), Convert.ToString(hrrfResultQualify.HRRFCreatedBy), Environment.GetEnvironmentVariable("TrQualified") ?? "" + " <b>" + strQualifiedBy + "</b>" + ".", Environment.GetEnvironmentVariable("TrQualified_NOtification") ?? "", HRRFNumber, string.Empty, deliverymanager);

                        List<string> lstRecipients = new List<string>();

                        lstRecipients.Add(Session["EmployeeId"].ToString()); // mail to self
                        Environment.GetEnvironmentVariable("ITS-OPS") ?? "".Split(',').ToList<string>().ForEach(Ops => lstRecipients.Add(Ops)); // mail to ITS-OPS

                        foreach (string recipient in lstRecipients.Distinct())
                        {
                            InsertNotification(Convert.ToString(Session["EmployeeId"]), recipient, Environment.GetEnvironmentVariable("TrQualified_By") ?? "" + " <b>" + strQualifiedBy + " </b>" + ".", Environment.GetEnvironmentVariable("TrQualified_NOtification") ?? "", HRRFNumber, string.Empty, null);
                        }
                    }
                }

                //var qualifybyname = db.Employees.Where(q => q.EmployeeId == objHrrfHistory.ModifiedBy).FirstOrDefault();

                //InsertNotification(Convert.ToString(Session["EmployeeId"]), Convert.ToString(hrrfResultQualify.HRRFCreatedBy), Environment.GetEnvironmentVariable("TrQualified") ?? ConfigurationManager.AppSettings["TrQualified"] + qualifybyname.FirstName + " " + qualifybyname.LastName + ".", Environment.GetEnvironmentVariable("TrQualified_NOtification") ?? ConfigurationManager.AppSettings["TrQualified_NOtification"], HRRFNumber, string.Empty);
                //InsertNotification(Convert.ToString(Session["EmployeeId"]), Convert.ToString(Session["EmployeeId"]), Environment.GetEnvironmentVariable("TrQualified_By") ?? ConfigurationManager.AppSettings["TrQualified_By"] + qualifybyname.FirstName + " " + qualifybyname.LastName + ".", Environment.GetEnvironmentVariable("TrQualified_NOtification") ?? ConfigurationManager.AppSettings["TrQualified_NOtification"], HRRFNumber, string.Empty);

                var viewResult = new ValidationModel()
                {
                    //ProposeAssociates = db.ProposeAssociates.ToList(),
                    PropsedList = GetListOfResourceList(HRRFNumber, ""),
                    HRRFs = db.HRRFs.Where(h => h.HRRFNumber == HRRFNumber),
                    //BenchEmployee = GetListRecommendResources()
                    TRMagicDetails = GetListOfRecommendResourcesbymagic(HRRFNumber)
                };

                TempData["RequestStatus"] = "Qualified";

                return RedirectToAction("ProposeAssociate", "ProposeAssociate", new { hrrfno = HRRFNumber });

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
                //return Json("Error", JsonRequestBehavior.AllowGet);
            }

        }

        /// <summary>
        /// Insert Notifications
        /// </summary>
        /// <param name="From"></param>
        /// <param name="To"></param>
        /// <param name="NotificationMsg"></param>
        /// <param name="NOtificationType"></param>
        /// <param name="HrrfNumber"></param>
        /// <param name="Remarks"></param>
        public void InsertNotification(string From, string To, string NotificationMsg, string NOtificationType, string HrrfNumber, string Remarks, string cc)
        {
            var hrrfResultQualify = db.HRRFs.Where(h => h.HRRFNumber == HrrfNumber).ToList().FirstOrDefault();
            Notification tblNotification = new Notification();
            tblNotification.NotificationType = NOtificationType;
            tblNotification.NotificationDate = System.DateTime.Now;
            tblNotification.NotificationFrom = Convert.ToInt32(From);
            tblNotification.NotificationTo = Convert.ToInt32(To);
            var Body = NotificationMsg + Environment.GetEnvironmentVariable("TRNUMber") ?? "" + HrrfNumber + "<br/> <b> Job Description: </b>" + hrrfResultQualify.JobDescription + "<br/><br/>" + MailingContent(HrrfNumber) + Remarks;
            tblNotification.IsActive = true;
            tblNotification.AssetID = HrrfNumber;
            tblNotification.ApplicationCode = Environment.GetEnvironmentVariable("AppCodeTR") ?? "";
            tblNotification.IsFromGroupID = true;
            tblNotification.FromGroupID = "donotreply_Talento@trianz.com";
            tblNotification.CC = cc;

            string talentoURL = Environment.GetEnvironmentVariable("Talento") ?? "";
            string body = string.Empty;
            string LinktoOpen = "Please Click on link to View the details of Submitted Talent Request number #" + HrrfNumber + " : " + talentoURL;
            using (StreamReader reader = new StreamReader(Server.MapPath("~/Views/Shared/EmailTemplate.html")))
            {
                body = reader.ReadToEnd();
            }
            body = body.Replace("{ToUserName}", db.Employees.Where(e => e.EmployeeId == tblNotification.NotificationTo).Select(s => s.FirstName + " " + s.LastName).FirstOrDefault());
            //body = body.Replace("{FromUserName}", db.Employees.Where(e => e.EmployeeId == tblNotification.NotificationFrom).Select(s => s.FirstName + " " + s.LastName).FirstOrDefault());
            body = body.Replace("{FromUserName}", "Talento Team");
            body = body.Replace("{Description}", Body);
            body = body.Replace("{LinktoOpen}", LinktoOpen);

            tblNotification.NotificationMessage = body;


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
                    //code writeen by smiley
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
        /// <summary>
        /// Convert HRRF to External fulfillment
        /// </summary>
        /// <returns></returns>
        public ActionResult ConvertHrrfToExternal(string comments)
        {
            try
            {
                string HRRFNumber = Session["hrrfno"].ToString();
                string strPrevRequestStatus = "";
                // TempData["hrrfno"] = HRRFNumber; 
                var hrrfResultQualify = db.HRRFs.Where(h => h.HRRFNumber == HRRFNumber).ToList().FirstOrDefault();
                if (hrrfResultQualify != null)
                {
                    strPrevRequestStatus = hrrfResultQualify.RequestStatus;

                    hrrfResultQualify.RequestType = convertExternal;
                    hrrfResultQualify.RequestStatus = PendingForDHApproval;
                    db.Entry(hrrfResultQualify).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }
                var objHRRF = db.HRRFs.Where(c => c.HRRFNumber == HRRFNumber).FirstOrDefault();
                var objHRRFHistory = db.HRRFHistories.Where(c => c.HRRFNumber == HRRFNumber).FirstOrDefault();
                var objHRRFConvert = db.ExternalFulfillments.Where(c => c.HRRFNumber == HRRFNumber).ToList();
                // To Convert To External
                if (objHRRFConvert.Count < 1)
                {
                    ExternalFulfillment objExternalFullfillment = new ExternalFulfillment();
                    objExternalFullfillment.HRRFNumber = HRRFNumber;
                    objExternalFullfillment.ProjectName = objHRRF.ProjectName;
                    // objExternalFullfillment.ProjectCode = objHRRF.ProjectCode;
                    objExternalFullfillment.Role = objHRRF.RoleRequired;
                    objExternalFullfillment.Grade = objHRRF.Grade;
                    // objExternalFullfillment.Skills = string.Empty;
                    objExternalFullfillment.Practice = objHRRF.Practice;
                    // objExternalFullfillment.Location = objHRRF.OffshoreLocation;
                    //objExternalFullfillment.Account = string.Empty;
                    //objExternalFullfillment.ApprovalStatus = string.Empty;

                    objExternalFullfillment.Remarks = comments.Trim();
                    objExternalFullfillment.RequestedBy = objHRRF.HRRFCreatedBy.ToString();
                    objExternalFullfillment.RequestedDate = DateTime.Now;
                    objExternalFullfillment.ApprovedBy = objHRRF.ModifiedBy.ToString();
                    objExternalFullfillment.ApprovedDate = DateTime.Now;
                    objExternalFullfillment.ModifiedBy = objHRRF.ModifiedBy;
                    objExternalFullfillment.ModifiedDate = DateTime.Now;
                    objExternalFullfillment.HRRFID = objHRRF.HRRFID;
                    db.ExternalFulfillments.Add(objExternalFullfillment);
                    //saving
                    db.SaveChanges();
                    //hrrf histroy 
                    TRScernarioHistroy(HRRFNumber, HRRFNumber + " - " + "has been Converted to External", objExternalFullfillment.Remarks, Convert.ToInt32(Session["EmployeeId"]), strPrevRequestStatus);

                }
                // Already Converted To External
                if (objHRRFConvert.Count >= 1)
                {
                    var trConvertToExternal = db.ExternalFulfillments.Where(h => h.HRRFNumber == HRRFNumber).ToList().FirstOrDefault();
                    if (objHRRFHistory != null)
                        objHRRFHistory.Remarks = "";
                    trConvertToExternal.Remarks = comments.Trim();
                    trConvertToExternal.ApprovalStatus = string.Empty;
                    TRScernarioHistroy(HRRFNumber, HRRFNumber + " - " + "has been Converted to External", trConvertToExternal.Remarks, Convert.ToInt32(Session["EmployeeId"]), strPrevRequestStatus);
                    db.SaveChanges();
                }


                if (Session["EmployeeId"] != null)
                {
                    // Email Notifications
                    Employee objEmp = db.Employees.Find(Session["EmployeeId"]);
                    string strConvertToExternalBy = "";

                    if (objEmp != null)
                    {
                        strConvertToExternalBy = objEmp.FirstName.Trim() + (!string.IsNullOrWhiteSpace(objEmp.MiddleName) ? " " + objEmp.MiddleName + " " : " ") + objEmp.LastName.Trim();
                        string ApproverId = DHNameFor_Mails(objHRRF.RequestStatus, objHRRF.Practice, objHRRF.CostCenter, Convert.ToInt32(objHRRF.Grade), Convert.ToInt32(objHRRF.HRRFCreatedBy), objHRRF.HRRFNumber);
                        // mail to TR Created By
                        InsertNotification(Convert.ToString(Session["EmployeeId"]), Convert.ToString(hrrfResultQualify.HRRFCreatedBy), Environment.GetEnvironmentVariable("TrConvertToExt") ?? "" + strConvertToExternalBy + ".", Environment.GetEnvironmentVariable("TrConverttoExt_NOtification") ?? "", HRRFNumber, string.Empty, null);

                        List<string> lstRecipients = new List<string>();

                        lstRecipients.Add(Session["EmployeeId"].ToString()); // mail to self
                        lstRecipients.Add(ApproverId); // mail to DH
                        Environment.GetEnvironmentVariable("ITS-OPS") ?? "".Split(',').ToList<string>().ForEach(Ops => lstRecipients.Add(Ops)); // mail to ITS-OPS

                        foreach (string recipient in lstRecipients.Distinct())
                        {
                            if (recipient != null && recipient != "") { 
                                InsertNotification(Convert.ToString(Session["EmployeeId"]), recipient, Environment.GetEnvironmentVariable("TrConvertToExt_By") ?? "" + strConvertToExternalBy + ".", Environment.GetEnvironmentVariable("TrConverttoExt_NOtification") ?? "", HRRFNumber, string.Empty, null);
                            }
                        }
                    }
                }

                ////EmailNotification(Environment.GetEnvironmentVariable("TrConverttoExt_NOtification") ?? ConfigurationManager.AppSettings["TrConverttoExt_NOtification"], Environment.GetEnvironmentVariable("TrConvertToExt") ?? ConfigurationManager.AppSettings["TrConvertToExt"], HRRFNumber, Environment.GetEnvironmentVariable("PHRole") ?? ConfigurationManager.AppSettings["PHRole"]);
                //InsertNotification(Session["EmployeeId"].ToString(), Session["EmployeeId"].ToString(), Environment.GetEnvironmentVariable("TrConvertToExt_By") ?? ConfigurationManager.AppSettings["TrConvertToExt_By"], Environment.GetEnvironmentVariable("TrConverttoExt_NOtification") ?? ConfigurationManager.AppSettings["TrConverttoExt_NOtification"], HRRFNumber, string.Empty);
                //InsertNotification(Session["EmployeeId"].ToString(), Convert.ToString(objHRRF.HRRFCreatedBy), Environment.GetEnvironmentVariable("TrConvertToExt") ?? ConfigurationManager.AppSettings["TrConvertToExt"], Environment.GetEnvironmentVariable("TrConverttoExt_NOtification") ?? ConfigurationManager.AppSettings["TrConverttoExt_NOtification"], HRRFNumber, string.Empty);

                return Json("success", JsonRequestBehavior.AllowGet);
                //return RedirectToAction("ProposeAssociate", "ProposeAssociate", new { hrrfno = HRRFNumber });
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

        /// <summary>
        /// Accept Converted External fulfillment
        /// </summary>
        /// <returns></returns>

        public ActionResult AcceptExternalHrrf()
        {
            try
            {
                string HRRFNumber = Session["hrrfno"].ToString();
                //TempData["hrrfno"] = HRRFNumber;
                var hrrfAcceptReject = db.ExternalFulfillments.Where(h => h.HRRFNumber == HRRFNumber).ToList().FirstOrDefault();
                hrrfAcceptReject.ApprovalStatus = external_Accept;//Accepted
                hrrfAcceptReject.ApprovedBy = Session["EmployeeId"].ToString(); //objHRRF.HRRFCreatedBy.ToString(); //need to get logged in user id
                hrrfAcceptReject.ApprovedDate = DateTime.Now;
                db.Entry(hrrfAcceptReject).State = System.Data.Entity.EntityState.Modified;
                // update hrrf status
                var objHRRF = db.HRRFs.Where(c => c.HRRFNumber == HRRFNumber).FirstOrDefault();
                string strLocationType = (objHRRF.LocationType == "1") ? "ONSITE" : "OFFSHORE";
                DateTime billingdate = Convert.ToDateTime(objHRRF.BillingDate);
                string PresentBillingDate = billingdate.ToString("dd-MMMM-yyyy");
                string NewBillingDate;

                //int client = clientInterviewYes;
                //objHRRF.InternalExpectedFulfilmentDate = DateTime.Now.AddDays(clientInterviewYes);
                // Client Interview changes start//
                if (objHRRF.ClientInterview == "Yes")
                {
                    objHRRF.BillingDate = billingdate.AddDays(clientInterviewYes);

                    NewBillingDate = (objHRRF.BillingDate).Value.ToString("dd-MMMM-yyyy");
                }
                else
                {
                    objHRRF.BillingDate = billingdate.AddDays(clientInterviewNo);
                    NewBillingDate = (objHRRF.BillingDate).Value.ToString("dd-MMMM-yyyy");
                }               
                // Client Interview changes End//
                string strPrevRequestStatus = objHRRF.RequestStatus;
                objHRRF.RequestStatus = "Resume Pending"; //convertExternal;
                db.Entry(objHRRF).State = System.Data.Entity.EntityState.Modified;
                //hrrf histroy
                TRScernarioHistroy(HRRFNumber, HRRFNumber + " - " + "has been Accepted for External", "", Convert.ToInt32(Session["EmployeeId"]), strPrevRequestStatus);
                TRScernarioHistroy(HRRFNumber, HRRFNumber + " - " + "has been Changed to 'Resume Pending' status", "", Convert.ToInt32(Session["EmployeeId"]), strPrevRequestStatus);
                if (objHRRF.ClientInterview == "Yes" && clientInterviewYes !=0)
                {
                    //Client Interview selected as No 60 days added to Billing date(30 - Mar - 2022) New Date 30 - May - 2022
                    TRScernarioHistroy(HRRFNumber, HRRFNumber + " - " + "Client Interview Selected as Yes 90 days added to Billing date (" + PresentBillingDate + " ) New Date (" + NewBillingDate + " ) ", "", Convert.ToInt32(Session["EmployeeId"]), strPrevRequestStatus);
                }
                else
                {
                    if(clientInterviewNo !=0)
                    { 
                    TRScernarioHistroy(HRRFNumber, HRRFNumber + " - " + "Client Interview Selected as No 60 days added to Billing date (" + PresentBillingDate + " ) New Date (" + NewBillingDate + " ) ", "", Convert.ToInt32(Session["EmployeeId"]), strPrevRequestStatus);
                    }
                }
                db.SaveChanges();                
                if (Session["EmployeeId"] != null)
                {
                    // Email Notifications
                    Employee objEmp = db.Employees.Find(Session["EmployeeId"]);
                    string strAcceptedBy = "";
                    if (objEmp != null)
                    {
                        strAcceptedBy = objEmp.FirstName.Trim() + (!string.IsNullOrWhiteSpace(objEmp.MiddleName) ? " " + objEmp.MiddleName + " " : " ") + objEmp.LastName.Trim();
                        List<string> lstRecipients = new List<string>();                      
                        lstRecipients.Add(Session["EmployeeId"].ToString()); // sent mail to self
                        lstRecipients.Add(objHRRF.HRRFCreatedBy.ToString()); // mail to TR Created By
                        Environment.GetEnvironmentVariable("ITS-OPS") ?? "".Split(',').ToList<string>().ForEach(Ops => lstRecipients.Add(Ops)); // mail to ITS-OPS                       
                        Environment.GetEnvironmentVariable("Mail-RL") ?? "".Split(',').ToList<string>().ForEach(RLs => lstRecipients.Add(RLs)); // mail to RL(Divya, Nithya, Parvez, Lokesh)
                                                                                                                                        //List<string> lstRLEmp = db.RoleMasters.Where(r => r.Role == "RL" && r.ApplicationCode == "TALENTREQ").Select(r => r.EmployeeId.ToString()).ToList();
                                                                                                                                        //List<string> lstRLActiveEmp = db.Employees.Where(e => lstRLEmp.Contains(e.EmployeeId.ToString()) && e.IsActive == true && e.LocationType == strLocationType).Select(emp => emp.EmployeeId.ToString()).ToList();
                        //lstRLActiveEmp.ForEach(RLs => lstRecipients.Add(RLs)); // mail to all active RLs
                        foreach (string recipient in lstRecipients.Distinct())
                        {
                            InsertNotification(Session["EmployeeId"].ToString(), recipient, Environment.GetEnvironmentVariable("ExternalFullAccept") ?? "" + strAcceptedBy + ",", Environment.GetEnvironmentVariable("TrExtFulfilAcep_NOtification") ?? "", objHRRF.HRRFNumber, "", null);
                        }
                    }
                    TempData["Message"] = "AcceptedExternal";
                }
                return RedirectToAction("ProposeAssociate", "ProposeAssociate", new { hrrfno = HRRFNumber });
            }
            //catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
            //{
            //    Exception raise = dbEx;
            //    foreach (var validationErrors in dbEx.EntityValidationErrors)
            //    {
            //        foreach (var validationError in validationErrors.ValidationErrors)
            //        {
            //            string message = string.Format("{0}:{1}",
            //            validationErrors.Entry.Entity.ToString(),
            //            validationError.ErrorMessage);
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

        /// <summary>
        /// Reject Converted External fulfillment
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ConvertToInternal(string HrrfNumber, string comments)
        {
            try
            {
                string HRRFNumber = HrrfNumber;
                string strPrevRequestStatus = "";
                var x = (from eh in db.ExternalHires where eh.HRRFNumber == HRRFNumber select eh).ToList();
                var hrrfData = (from hrrf in db.HRRFs where hrrf.HRRFNumber == HRRFNumber select hrrf).FirstOrDefault();
                if (hrrfData!=null)
                {
                    if (hrrfData.RequestType == "External" && hrrfData.ExternalWebSite == "Yes")
                    {
                        hrrfData.UPDATEEXTERNAL = "Yes";
                    }
                    db.SaveChanges();
                }
               
                if (x == null)
                {
                    var objHRRF = db.HRRFs.Where(c => c.HRRFNumber == HRRFNumber).FirstOrDefault();
                    // hrrf request type Status
                    strPrevRequestStatus = objHRRF.RequestStatus;
                    objHRRF.RequestType = convertInternal;
                    objHRRF.RequestStatus = qualified;
                    objHRRF.Remarks = comments;
                    objHRRF.InternalExpectedFulfilmentDate = DateTime.Now.AddDays(5);
                    
                    TRScernarioHistroy(HRRFNumber, HRRFNumber + " - " + "has been converted to Internal", objHRRF.Remarks, Convert.ToInt32(Session["EmployeeId"]), strPrevRequestStatus);
                    //saving
                    db.SaveChanges();
                    //InsertNotification(Session["EmployeeId"].ToString(), Session["EmployeeId"].ToString(), Environment.GetEnvironmentVariable("TrConvertToExt_By") ?? ConfigurationManager.AppSettings["TrConvertToExt_By"], Environment.GetEnvironmentVariable("TrConverttoExt_NOtification") ?? ConfigurationManager.AppSettings["TrConverttoExt_NOtification"], HRRFNumber, string.Empty);

                    //return RedirectToAction("Index", "ExternalReport");
                    return Json("success", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var objHRRF = db.HRRFs.Where(c => c.HRRFNumber == HRRFNumber).FirstOrDefault();
                    strPrevRequestStatus = objHRRF.RequestStatus;
                    objHRRF.RequestType = convertInternal;
                    objHRRF.RequestStatus = qualified;
                    objHRRF.InternalExpectedFulfilmentDate = DateTime.Now.AddDays(5);
                    objHRRF.Remarks = comments;
                    db.Entry(objHRRF).State = System.Data.Entity.EntityState.Modified;

                    TRScernarioHistroy(HRRFNumber, HRRFNumber + " - " + "has been converted to Internal", objHRRF.Remarks, Convert.ToInt32(Session["EmployeeId"]), strPrevRequestStatus);
                    //Get the ID of ExternalHire Table using HrrfNumber
                    int externalid = Convert.ToInt32((from eh in db.ExternalHires where eh.HRRFNumber == HRRFNumber select eh.ExternalHireId).FirstOrDefault());
                    if (externalid != 0)
                    {
                        ExternalHire externalhire = db.ExternalHires.Find(externalid);
                        db.ExternalHires.Remove(externalhire);
                        db.SaveChanges();
                    }
                    db.SaveChanges();
                    //return RedirectToAction("Index", "ExternalReport");

                    return Json("success", JsonRequestBehavior.AllowGet);
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

        public ActionResult RejectExternalHrrf(string comments)
        {
            try
            {
                string HRRFNumber = Session["hrrfno"].ToString();
                //TempData["hrrfno"] = HRRFNumber;
                var objHRRF = db.HRRFs.Where(c => c.HRRFNumber == HRRFNumber).FirstOrDefault();
                var hrrfAcceptReject = db.ExternalFulfillments.Where(h => h.HRRFNumber == HRRFNumber).ToList().FirstOrDefault();
                hrrfAcceptReject.ApprovalStatus = external_Reject;
                hrrfAcceptReject.Remarks = comments;
                hrrfAcceptReject.ApprovedBy = objHRRF.HRRFCreatedBy.ToString(); //need to get logged in user id
                hrrfAcceptReject.ApprovedDate = DateTime.Now;
                db.Entry(hrrfAcceptReject).State = System.Data.Entity.EntityState.Modified;
                // hrrf request type Status
                string strPrevRequestStatus = objHRRF.RequestStatus;
                objHRRF.RequestType = convertInternal;
                objHRRF.RequestStatus = qualified;
                db.Entry(objHRRF).State = System.Data.Entity.EntityState.Modified;
                //HRRF Histroy
                TRScernarioHistroy(HRRFNumber, HRRFNumber + " - " + "has been Rejected for External", hrrfAcceptReject.Remarks, Convert.ToInt32(Session["EmployeeId"]), strPrevRequestStatus);
                //saving
                db.SaveChanges();
                return Json("success", JsonRequestBehavior.AllowGet);
                //return RedirectToAction("ProposeAssociate", "ProposeAssociate", new { hrrfno = HRRFNumber });
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

        /// <summary>
        /// Approval by DM for Resources
        /// </summary>
        /// <param name="ProposedId"></param>
        /// <returns></returns>
        public ActionResult DMApproveProposedResource(int? ProposedId)
        {
            try {
            string HRRFNumber = Session["hrrfno"].ToString();
            TempData["hrrfno"] = HRRFNumber;
            var propsedLog = db.ProposeAssociates.Where(h => h.ProposedID == ProposedId).ToList().FirstOrDefault();
            if (propsedLog != null)
            {
                propsedLog.DeliveryStatus = "Approved";
                //propsedLog.DeliveryRemarks = "";
                db.Entry(propsedLog).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
            }
            return RedirectToAction("ProposeAssociate", "ProposeAssociate");
            }
            catch (Exception ex)
            {
                Common.WriteExceptionErrorLog(ex);
                return RedirectToAction("Error", "Error");
            }

        }

        /// <summary>
        /// Reject for resources by DM
        /// </summary>
        /// <param name="ProposedId"></param>
        /// <returns></returns>
        public ActionResult DMRejectProposedResource(int? ProposedId)
        {
            try { 
            string HRRFNumber = Session["hrrfno"].ToString();
            TempData["hrrfno"] = HRRFNumber;
            var propsedLog = db.ProposeAssociates.Where(h => h.ProposedID == ProposedId).ToList().FirstOrDefault();
            if (propsedLog != null)
            {
                propsedLog.DeliveryStatus = "Rejected";
                // propsedLog.DeliveryRemarks = "";
                db.Entry(propsedLog).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
            }
            return RedirectToAction("ProposeAssociate", "ProposeAssociate");
            }
            catch (Exception ex)
            {
                Common.WriteExceptionErrorLog(ex);
                return RedirectToAction("Error", "Error");
            }

        }

        /// <summary>
        ///  Request for more Info in case of TR Submitted
        /// </summary>
        /// <returns></returns>
        public ActionResult RequestForMoreInfo(string data)
        {
            try
            {
                string HRRFNumber = Session["hrrfno"].ToString();
                string strPrevRequestStatus = "";
                TempData["hrrfno"] = HRRFNumber;
                var hrrfno = HRRFNumber;
                var objHRRF = db.HRRFs.Where(c => c.HRRFNumber == HRRFNumber).FirstOrDefault();
                var hrrfRequestMoreInfo = db.HRRFs.Where(h => h.HRRFNumber == HRRFNumber).ToList().FirstOrDefault();
                strPrevRequestStatus = hrrfRequestMoreInfo.RequestStatus;
                hrrfRequestMoreInfo.RequestStatus = requestMoreInfo;
                hrrfRequestMoreInfo.HRRFCreatedBy = objHRRF.HRRFCreatedBy; //need to get logged in user id
                hrrfRequestMoreInfo.HRRFCreatedDate = DateTime.Now;
                hrrfRequestMoreInfo.Remarks = data;
                db.Entry(hrrfRequestMoreInfo).State = System.Data.Entity.EntityState.Modified;
                // Histroy
                TRScernarioHistroy(HRRFNumber, HRRFNumber + " - " + "has been Requested for More Info", hrrfRequestMoreInfo.Remarks, Convert.ToInt32(Session["EmployeeId"]), strPrevRequestStatus);
                //saving
                db.SaveChanges();
                //mail notification
                InsertNotification(Session["EmployeeId"].ToString(), Session["EmployeeId"].ToString(), Environment.GetEnvironmentVariable("TrRequestedForMoreInfo_BY") ?? "", Environment.GetEnvironmentVariable("TrRequestedFormoreInfo_NOtification") ?? "", HRRFNumber, "<b>" + Environment.GetEnvironmentVariable("Remarks") ?? "" + "</b>" + data, null);
                InsertNotification(Session["EmployeeId"].ToString(), Convert.ToString(objHRRF.HRRFCreatedBy), Environment.GetEnvironmentVariable("TrRequestedForMoreInfo") ?? "", Environment.GetEnvironmentVariable("TrRequestedFormoreInfo_NOtification") ?? "", HRRFNumber, "<b>" + Environment.GetEnvironmentVariable("Remarks") ?? "" + "</b>" + data, null);
                return RedirectToAction("Index", "trHome");
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

        //public ActionResult ReturnAcceptedResources(List<string> Acceptids)
        //{
        //    try
        //    {

        //        return Json(Acceptids, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {

        //        throw;
        //    }
        //}

        /// <summary>
        /// TR Terminated
        /// </summary>
        /// <returns></returns>
        public JsonResult TerminateTR(string remarks, string cancel_reasontext, string duplicatehrrfno)
        {

            try
            {
                string HRRFNumber = Session["hrrfno"].ToString();
                string strPrevRequestStatus = "";
                TempData["hrrfno"] = HRRFNumber;
                var objHRRF = db.HRRFs.Where(c => c.HRRFNumber == HRRFNumber).FirstOrDefault();
                var objExternalHire = db.ExternalHires.Where(c => c.HRRFNumber == HRRFNumber).FirstOrDefault();
                var hrrfTerminate = db.HRRFs.Where(h => h.HRRFNumber == HRRFNumber).ToList().FirstOrDefault();
                strPrevRequestStatus = hrrfTerminate.RequestStatus;
                hrrfTerminate.RequestStatus = tr_Terminate;
                hrrfTerminate.Remarks = remarks;
                hrrfTerminate.CancelReason = cancel_reasontext;
                hrrfTerminate.ModifiedBy = Convert.ToInt32(Session["EmployeeId"]);
                hrrfTerminate.ModifiedDate = DateTime.Now;
                if ((objHRRF.RequestType).ToLower().Contains("external"))
                {
                    if (objExternalHire != null)
                    {
                        objExternalHire.RequestStatus = tr_Terminate;
                        objExternalHire.CancelDate = DateTime.Now;
                        objExternalHire.CancellationReason = cancel_reasontext;
                        if (cancel_reasontext == "Duplicate" && duplicatehrrfno != "")
                            objExternalHire.DuplicateHRRFNo = duplicatehrrfno;
                    }
                    else
                    {
                        ExternalHire et = new ExternalHire();
                        et.Name = "";
                        et.RequestStatus = tr_Terminate;
                        et.FulfilmentRemarks = "";
                        et.DOJ = null;
                        et.FulfilmentDate = null;
                        et.CancellationReason = cancel_reasontext;
                        et.HRRFNumber = HRRFNumber;
                        et.HRRFId = objHRRF.HRRFID;
                        et.CancelDate = DateTime.Now;
                        et.EmployeeId = null;
                        et.RecruiterName = null;
                        if (cancel_reasontext == "Duplicate" && duplicatehrrfno != "")
                        et.DuplicateHRRFNo = duplicatehrrfno;
                        db.ExternalHires.Add(et);
                    }
                }
                else if (cancel_reasontext == "Duplicate" && duplicatehrrfno != "")
                {                   
                    var result = db.HRRFs.Where(p => p.HRRFNumber == duplicatehrrfno).Select(p => p.HRRFNumber).FirstOrDefault();
                    if (result == null)
                    {
                        return Json(false, JsonRequestBehavior.AllowGet);
                    }

                    hrrfTerminate.DuplicateHRRFNo = duplicatehrrfno;
                }
                db.Entry(hrrfTerminate).State = System.Data.Entity.EntityState.Modified;   
                // TR Histroy 
                TRScernarioHistroy(HRRFNumber, HRRFNumber + " - " + "has been Cancelled", hrrfTerminate.Remarks, Convert.ToInt32(Session["EmployeeId"]), strPrevRequestStatus);
                //saving
                db.SaveChanges();
                InsertNotification(Session["EmployeeId"].ToString(), Session["EmployeeId"].ToString(), Environment.GetEnvironmentVariable("TrTerminate") ?? "", Environment.GetEnvironmentVariable("TrTerminate_NOtification") ?? "", HRRFNumber, string.Empty, null);
                InsertNotification(Session["EmployeeId"].ToString(), Convert.ToString(objHRRF.HRRFCreatedBy), Environment.GetEnvironmentVariable("TrTerminate") ?? "", Environment.GetEnvironmentVariable("TrTerminate_NOtification") ?? "", HRRFNumber, string.Empty, null);
                return Json(HRRFNumber);
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

        public JsonResult SaveTechPanel(string TechPanel, string TechPanel2)
        {
            try
            {
                string HRRFNumber = Session["hrrfno"].ToString();
                TempData["hrrfno"] = HRRFNumber;
                var objHRRF = db.HRRFs.Where(c => c.HRRFNumber == HRRFNumber).FirstOrDefault();
                string reqstst = objHRRF.RequestStatus;
                objHRRF.TECHPANEL = TechPanel;
                objHRRF.SECONDTECHPANEL = TechPanel2;
                objHRRF.ModifiedBy = Convert.ToInt32(Session["EmployeeId"]);
                objHRRF.ModifiedDate = DateTime.Now;
                // TR Histroy 
                TRScernarioHistroy(HRRFNumber, HRRFNumber + reqstst + "(Edited:  ", null, Convert.ToInt32(Session["EmployeeId"]), reqstst);
                //saving
                db.SaveChanges();
                InsertNotification(Session["EmployeeId"].ToString(), Session["EmployeeId"].ToString(), "Edited", Environment.GetEnvironmentVariable("TrEdited_NOtification") ?? "", HRRFNumber, string.Empty, null);
                InsertNotification(Session["EmployeeId"].ToString(), Convert.ToString(objHRRF.HRRFCreatedBy), "Edited", Environment.GetEnvironmentVariable("TrEdited_NOtification") ?? "", HRRFNumber, string.Empty, null);
                return Json(HRRFNumber);
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

        /// <summary>
        /// TR Scenario History for All Actions - Common Method
        /// </summary>
        /// <param name="HRRFNumber"></param>
        /// <param name="Description"></param>
        /// <param name="EmpIdForCreatedBy"></param>
        public void TRScernarioHistroy(string HRRFNumber, string Description, string Remarks, int EmpIdForCreatedBy, string PrevRequestStatus)
        {
            try
            {
                var HrrfHistory = new HRRFHistory();
                HrrfHistory.HRRFNumber = HRRFNumber;

                if (Session["Role"].ToString().ToLower() == "dh" && Description.ToLower().Contains("has been rejected for"))
                {
                    HrrfHistory.HistoryDescription = Description + " ----  by " + EmpIdForCreatedBy;
                }
                else
                {
                    HrrfHistory.HistoryDescription = Description;
                }
                HrrfHistory.ModifiedBy = EmpIdForCreatedBy;
                HrrfHistory.Remarks = Remarks;
                HrrfHistory.ModifiedDate = DateTime.Now;
                HrrfHistory.PrevRequestStatus = PrevRequestStatus;
                db.HRRFHistories.Add(HrrfHistory);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                ErrorHandling expcls = new ErrorHandling();
                expcls.Error(ex);
            }
        }

        public ActionResult UpdateRequestStatus(String Flag, string comments, String Purpose)
        {
            try
            {
                var PrevStatus = Request.Form["RequestStatus"];
                if (Flag.Trim().ToLower() == "hold")   // added for reload issue of hold button
                {
                    string HRRFNumber = Session["hrrfno"].ToString();

                    //Update Request status in HRRF
                    var hrrf = db.HRRFs.Where(c => c.HRRFNumber == HRRFNumber).FirstOrDefault();
                    string strPrevRequestStatus = "";
                    if (hrrf != null)
                    {
                        strPrevRequestStatus = hrrf.RequestStatus;
                        hrrf.RequestStatus = "Hold";
                        hrrf.Remarks = comments;
                        db.Entry(hrrf).State = System.Data.Entity.EntityState.Modified;
                        // TR Histroy 
                        TRScernarioHistroy(HRRFNumber, HRRFNumber + " - " + "has been converted to Hold ", hrrf.Remarks, Convert.ToInt32(Session["EmployeeId"]), strPrevRequestStatus);
                        db.SaveChanges();
                    }
                    return Json("This TR has been hold successfully", JsonRequestBehavior.AllowGet);
                }
                else if (Flag.Trim().ToLower() == "unhold")    // added for reload issue of hold button
                {
                    string HRRFNumber = Session["hrrfno"].ToString();

                    //Update Request status in HRRF
                    var hrrf = db.HRRFs.Where(c => c.HRRFNumber == HRRFNumber).FirstOrDefault();
                    if (hrrf != null)
                    {
                        HRRFHistory objHRRFHistory = db.HRRFHistories.Where(c => c.HRRFNumber == HRRFNumber).OrderByDescending(i => i.PrevRequestStatus).FirstOrDefault();

                        if (objHRRFHistory != null)
                        {
                            if (objHRRFHistory.PrevRequestStatus != null)
                            {
                                hrrf.RequestStatus = objHRRFHistory.PrevRequestStatus;
                            }
                        }

                        db.Entry(hrrf).State = System.Data.Entity.EntityState.Modified;

                        // TR Histroy 
                        TRScernarioHistroy(HRRFNumber, HRRFNumber + " - has been unhold", "", Convert.ToInt32(Session["EmployeeId"]), hrrf.RequestStatus);
                        db.SaveChanges();
                    }

                    return Json("This TR has been unhold successfully", JsonRequestBehavior.AllowGet);
                }
                return Json("", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                Common.WriteExceptionErrorLog(ex);
                return Json("Error", JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult ApproveResourceByPM(string[] ProposedId)
        {
            try { 
            if (Session["hrrfno"] != null)
            {
                string HRRFNumber = Session["hrrfno"].ToString();
                int EmpID = Convert.ToInt32(Session["EmployeeId"]);
                string PropID = ProposedId[0];
                int proseEMpID = Convert.ToInt32(PropID);

                HRRF objHRRF = db.HRRFs.Where(h => h.HRRFNumber == HRRFNumber).FirstOrDefault();
                if (objHRRF != null)
                {
                    Employee objEmployee = db.Employees.Where(e => e.EmployeeId == EmpID).FirstOrDefault();
                    Employee objResource = db.Employees.Find(Convert.ToInt32(ProposedId[0]));

                    if (objEmployee != null)
                    {
                        // Save hrrf history in HRRFHistory table
                        TRScernarioHistroy(HRRFNumber, objResource.FirstName + " " + objResource.LastName + " has been approved for" + " - " + HRRFNumber, "", Convert.ToInt32(Session["EmployeeId"]), PendingForPMApproval);

                        // Assigned new request status to HRRF and save details in HRRF table
                        objHRRF.RequestStatus = PendingForDHApproval;

                        db.Entry(objHRRF).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                            
                        // Send Email notifications to respective members.
                        //InsertNotification(Session["EmployeeId"].ToString(), Convert.ToString(objHRRF.HRRFCreatedBy), Environment.GetEnvironmentVariable("TrAcceptedResources") ?? ConfigurationManager.AppSettings["TrAcceptedResources"], Environment.GetEnvironmentVariable("TrProposedResAce_NOtification") ?? ConfigurationManager.AppSettings["TrProposedResAce_NOtification"] + " - by PM", HRRFNumber, Environment.GetEnvironmentVariable("AcceptedResource") ?? ConfigurationManager.AppSettings["AcceptedResource"] + objResource.FirstName + " " + objResource.LastName);
                        //lakshmi
                        var proposedEmpName = db.Employees.Where(p => p.EmployeeId == proseEMpID).FirstOrDefault();
                        var isvalidbusinessgroup = db.IncludedBussinessGroups.Where(i => i.BusinessGroup.Contains(proposedEmpName.BusinessGroup)).FirstOrDefault();
                        var isinvalidemployetype = db.ExcludedEmployeeTypes.Where(i => i.EmployeeType.Contains(proposedEmpName.EmployeeType)).FirstOrDefault();

                        if (isvalidbusinessgroup != null && isinvalidemployetype == null)
                        {
                            InsertNotification(Session["EmployeeId"].ToString(), ProposedId[0], Environment.GetEnvironmentVariable("Acceptedto") ?? "" + objHRRF.ProjectName, Environment.GetEnvironmentVariable("TrProposedResAce_NOtification") ?? "" + " - by PM", HRRFNumber, string.Empty, null);
                        }
                        InsertNotification(Session["EmployeeId"].ToString(), Session["EmployeeId"].ToString(), Environment.GetEnvironmentVariable("TrAcceptedResources_by") ?? "", Environment.GetEnvironmentVariable("TrProposedResAce_NOtification") ?? "" + " - by PM", HRRFNumber, string.Empty, null);

                        ProposeAssociate objProposeResource = db.ProposeAssociates.Where(pa => pa.EmpID == PropID).FirstOrDefault();
                        if (objProposeResource != null)
                        {
                            InsertNotification(Session["EmployeeId"].ToString(), objProposeResource.ProposedBy, Environment.GetEnvironmentVariable("TrAcceptedResources") ?? "", Environment.GetEnvironmentVariable("TrProposedResAce_NOtification") ?? "" + " - by PM", HRRFNumber, "<b>" + Environment.GetEnvironmentVariable("AcceptedResource") ?? "" + ":</b>" + objResource.FirstName + " " + objResource.LastName, Environment.GetEnvironmentVariable("ITS-OPS") ?? ""+","+ Environment.GetEnvironmentVariable("Assurance") ?? "");
                        }
                    }
                }
                return Json(HRRFNumber, JsonRequestBehavior.AllowGet);
            }
            return Json("", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                Common.WriteExceptionErrorLog(ex);
                return RedirectToAction("Error", "Error");
            }

        }

        public ActionResult RejectResourceByPM(string[] ProposedId)
        {
            try { 
            if (Session["hrrfno"] != null)
            {
                string HRRFNumber = Session["hrrfno"].ToString();
                int EmpID = Convert.ToInt32(Session["EmployeeId"]);
                string PropID = ProposedId[0];

                HRRF objHRRF = db.HRRFs.Where(h => h.HRRFNumber == HRRFNumber).FirstOrDefault();
                if (objHRRF != null)
                {
                    Employee objEmployee = db.Employees.Where(e => e.EmployeeId == EmpID).FirstOrDefault();
                    Employee objResource = db.Employees.Find(Convert.ToInt32(ProposedId[0]));

                    if (objEmployee != null)
                    {
                        // Save hrrf history in HRRFHistory table
                        TRScernarioHistroy(HRRFNumber, objResource.FirstName + " " + objResource.LastName + " has been rejected for" + " - " + HRRFNumber, "", Convert.ToInt32(Session["EmployeeId"]), PendingForPMApproval);

                        // Assigned new request status to HRRF and save details in HRRF table
                        objHRRF.RequestStatus = qualified;

                        db.Entry(objHRRF).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();



                        //// Send Email notifications to respective members.
                        ////InsertNotification(Session["EmployeeId"].ToString(), Convert.ToString(objHRRF.HRRFCreatedBy), Environment.GetEnvironmentVariable("TrAcceptedResources") ?? ConfigurationManager.AppSettings["TrAcceptedResources"], Environment.GetEnvironmentVariable("TrProposedResAce_NOtification") ?? ConfigurationManager.AppSettings["TrProposedResAce_NOtification"] + " - by PM", HRRFNumber, Environment.GetEnvironmentVariable("AcceptedResource") ?? ConfigurationManager.AppSettings["AcceptedResource"] + objResource.FirstName + " " + objResource.LastName);
                        //InsertNotification(Session["EmployeeId"].ToString(), ProposedId[0], Environment.GetEnvironmentVariable("Acceptedto") ?? ConfigurationManager.AppSettings["Acceptedto"] + objHRRF.ProjectName, Environment.GetEnvironmentVariable("TrProposedResAce_NOtification") ?? ConfigurationManager.AppSettings["TrProposedResAce_NOtification"] + " - by PM", HRRFNumber, string.Empty);
                        //InsertNotification(Session["EmployeeId"].ToString(), Session["EmployeeId"].ToString(), Environment.GetEnvironmentVariable("TrAcceptedResources_by") ?? ConfigurationManager.AppSettings["TrAcceptedResources_by"], Environment.GetEnvironmentVariable("TrProposedResAce_NOtification") ?? ConfigurationManager.AppSettings["TrProposedResAce_NOtification"] + " - by PM", HRRFNumber, string.Empty);

                        //ProposeAssociate objProposeResource = db.ProposeAssociates.Where(pa => pa.EmpID == PropID).FirstOrDefault();
                        //if (objProposeResource != null)
                        //{
                        //    InsertNotification(Session["EmployeeId"].ToString(), objProposeResource.ProposedBy, Environment.GetEnvironmentVariable("TrAcceptedResources") ?? ConfigurationManager.AppSettings["TrAcceptedResources"], Environment.GetEnvironmentVariable("TrProposedResAce_NOtification") ?? ConfigurationManager.AppSettings["TrProposedResAce_NOtification"] + " - by PM", HRRFNumber, Environment.GetEnvironmentVariable("AcceptedResource") ?? ConfigurationManager.AppSettings["AcceptedResource"] + objResource.FirstName + " " + objResource.LastName);
                        //}
                    }
                }
                return Json(HRRFNumber, JsonRequestBehavior.AllowGet);
            }
            return Json("", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                Common.WriteExceptionErrorLog(ex);
                return RedirectToAction("Error", "Error");
            }

        }

        public JsonResult OMAcceptInternalProposedResource(string[] ProposedId)
        {
            try
            {

                DateTime todayDate = Convert.ToDateTime(DateTime.Now.Date);
                string HRRFNumber = Session["hrrfno"].ToString();
                TempData["hrrfno"] = HRRFNumber;
                List<string> strAlreadyAcceptedList = new List<string>();
                int loginempID = Convert.ToInt32(Session["EmployeeId"]);
                int? Utilzation = null;
                var Hrrffulldetails = db.HRRFs.Where(c => c.HRRFNumber == HRRFNumber).FirstOrDefault();

                int? hrrfcreator = Hrrffulldetails.HRRFCreatedBy;
                string[] strITOPS = Environment.GetEnvironmentVariable("ITS-OPS") ?? "".Split(',');
                var oms = (db.RoleMasters.Where(s => (s.Practice == Hrrffulldetails.Practice && s.Role == "OM" && s.ApplicationCode == "TALENTREQ") || strITOPS.Contains(s.EmployeeId.ToString().Trim())).ToList());


                RoleMaster objRoleMasterAllRoles = db.RoleMasters.Where(r1 => r1.EmployeeId == hrrfcreator && r1.ApplicationCode == "TALENTREQ").FirstOrDefault();
                if (objRoleMasterAllRoles != null)
                {
                    if (objRoleMasterAllRoles.Role == "OM")
                    {
                        foreach (string proposedEmpid in ProposedId)
                        {
                            #region SeudoCode
                            //change status
                            //remove bench record with that empid
                            //remove proposed candidate
                            // mail to employee and head  
                            #endregion
                            int propsedEmployeeid = Convert.ToInt32(proposedEmpid);
                            var objEmployeeAssignment = db.Employees.Where(a => a.EmployeeId == propsedEmployeeid).FirstOrDefault();
                            var propsedLog = db.ProposeAssociates.Where(h => h.EmpID == proposedEmpid && h.PracticeStatus == praticeAccept && h.HRRFNumber == HRRFNumber).ToList();
                            if (propsedLog.Count < 1)
                            {
                                // check for no of positions
                                var objHrrfNoofPostions = db.HRRFs.Where(c => c.HRRFNumber == HRRFNumber).FirstOrDefault();
                                if (objHrrfNoofPostions != null)
                                {
                                    var objHRRF = db.HRRFs.Where(c => c.HRRFNumber == HRRFNumber).FirstOrDefault();
                                    var objPropose = db.ProposeAssociates.Where(p => p.HRRFNumber == HRRFNumber && p.EmpID == proposedEmpid).FirstOrDefault();
                                    var objProposePostions = db.ProposeAssociates.Where(p => p.HRRFNumber == HRRFNumber && p.PracticeStatus == praticeAccept).ToList();


                                    DateTime dtStartDate = Convert.ToDateTime(objHRRF.BillingDate);
                                    DateTime dtEndDate = Convert.ToDateTime(objHRRF.AssignmentEndDate);

                                    var objpras1 = db.ProjectAssignments.Where(b => b.EmployeeId == propsedEmployeeid && !(b.BillingStatus.Contains("Bench")) && ((b.StartDate >= dtStartDate && b.StartDate <= dtEndDate) || (b.EndDate >= dtStartDate && b.EndDate <= dtEndDate) || (b.StartDate <= dtStartDate && b.EndDate >= dtEndDate))).ToList();
                                    if (objpras1.Count() > 0)
                                    {
                                        var sumUtilization = objpras1.Sum(p => p.Utilization);
                                        Utilzation = 100 - sumUtilization;
                                    }

                                    //to checkprevious Bench 
                                    #region Existing Bench Record
                                    var previousprojectass = db.ProjectAssignments.Where(p => p.IsActive == true && p.EmployeeId == propsedEmployeeid && p.BillingStatus == "Bench").ToList();
                                    var dtProjectAssignSatrtAsEndDate = db.ProjectAssignments.Where(pa => pa.EmployeeId == propsedEmployeeid
                                           && todayDate <= pa.StartDate).OrderBy(pa => pa.StartDate).Select(pa => pa.StartDate).FirstOrDefault();
                                    var dtProjectAssignEndDate = db.ProjectAssignments.Where(pa => pa.EmployeeId == propsedEmployeeid && pa.IsActive == true && pa.BillingStatus != "Bench"
                                           && todayDate <= pa.EndDate).OrderBy(pa => pa.EndDate).Select(pa => pa.EndDate).FirstOrDefault();
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
                                            pash.modifiedBy = loginempID;
                                            pash.ModifiedDate = DateTime.Now;

                                            #region Exisitng Bench Record Update based on selected Dates
                                            // Selecting top 1 Start Date order by desc to Set Bench Enda date
                                            // billdatge == getdate
                                            int? uil = null;
                                            if (DateTime.Now.Date >= (Convert.ToDateTime(objHRRF.BillingDate).Date))
                                            {
                                                if (Utilzation == null)
                                                {
                                                    uil = 100 - Convert.ToInt32(objHRRF.Utilization);
                                                }
                                                else
                                                {
                                                    uil = Utilzation - Convert.ToInt32(objHRRF.Utilization);
                                                }
                                            }
                                            else
                                            {
                                                uil = lt.Utilization;
                                            }
                                            if (dtProjectAssignSatrtAsEndDate != null && Convert.ToDateTime(dtProjectAssignSatrtAsEndDate).Date <= Convert.ToDateTime(objHRRF.BillingDate).Date)
                                            {
                                                lt.EndDate = Convert.ToDateTime(dtProjectAssignSatrtAsEndDate);
                                            }
                                            else if (dtProjectAssignEndDate != null && dtProjectAssignEndDate <= Convert.ToDateTime(objHRRF.AssignmentEndDate) && DateTime.Now.Date.Equals(Convert.ToDateTime(objHRRF.BillingDate).Date))
                                            {
                                                if (Convert.ToDateTime(objHRRF.BillingDate) < dtProjectAssignEndDate)
                                                    lt.EndDate = Convert.ToDateTime(objHRRF.BillingDate);
                                                else
                                                    lt.EndDate = Convert.ToDateTime(dtProjectAssignEndDate);
                                            }
                                            else if (DateTime.Now.Date >= (Convert.ToDateTime(objHRRF.BillingDate).Date))
                                            {
                                                if (uil > 0)
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


                                            //else if (DateTime.Now.Date < Convert.ToDateTime(objHRRF.BillingDate).Date)
                                            //{
                                            ////lt.StartDate = DateTime.Now;
                                            ////lt.Utilization = Utilzation ?? 100;
                                            //}
                                            if (uil > 0)
                                            {
                                                lt.Utilization = uil;

                                            }
                                            else
                                            {
                                                lt.IsActive = false;
                                            }
                                            #endregion

                                            pash.UtilizationNew = lt.Utilization;
                                            pash.StartDateNew = lt.StartDate;
                                            pash.EndDateNew = lt.EndDate;
                                            pash.IsActiveNew = lt.IsActive;
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
                                        if (DateTime.Now.Date >= Convert.ToDateTime(objHRRF.BillingDate).Date)
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
                                        else if (DateTime.Now.Date < Convert.ToDateTime(objHRRF.BillingDate).Date)
                                        {
                                            benchUtilization = 100;
                                        }
                                        if (benchUtilization > 0)
                                        {
                                            #region First Bench Record if Bench record not exist for the Employee
                                            DateTime stdat = new DateTime();
                                            DateTime etdat = new DateTime();

                                            if (DateTime.Now.Date >= Convert.ToDateTime(objHRRF.BillingDate).Date)
                                            {
                                                stdat = Convert.ToDateTime(objHRRF.BillingDate);
                                            }
                                            else
                                            {
                                                stdat = DateTime.Now.Date;
                                            }
                                            if (dtProjectAssignSatrtAsEndDate != null && dtProjectAssignSatrtAsEndDate <= Convert.ToDateTime(objHRRF.BillingDate) && DateTime.Now.Date.Equals(Convert.ToDateTime(objHRRF.BillingDate).Date))
                                            {
                                                etdat = Convert.ToDateTime(dtProjectAssignSatrtAsEndDate);
                                            }
                                            else if (dtProjectAssignEndDate != null && dtProjectAssignEndDate <= Convert.ToDateTime(objHRRF.AssignmentEndDate) && DateTime.Now.Date.Equals(Convert.ToDateTime(objHRRF.BillingDate).Date))
                                            {
                                                if (Convert.ToDateTime(objHRRF.BillingDate) < dtProjectAssignEndDate)
                                                    etdat = Convert.ToDateTime(objHRRF.BillingDate);
                                                else
                                                    etdat = Convert.ToDateTime(dtProjectAssignEndDate);
                                            }
                                            else if (DateTime.Now.Date >= (Convert.ToDateTime(objHRRF.BillingDate).Date))
                                            {
                                                etdat = Convert.ToDateTime(objHRRF.AssignmentEndDate);
                                            }
                                            else
                                            {
                                                etdat = Convert.ToDateTime(objHRRF.BillingDate);
                                            }


                                            bool dojbenck = true;

                                            var objpras11 = db.ProjectAssignments.Where(b => b.EmployeeId == propsedEmployeeid
                                            && ((b.StartDate >= stdat
                                            && b.StartDate <= etdat) || (b.EndDate >= stdat && b.EndDate <= etdat) ||
                                            (b.StartDate <= stdat && b.EndDate >= etdat))).ToList();

                                            if (objpras11.Count > 0)
                                            {
                                                var sumUtilization = objpras11.Sum(p => p.Utilization);
                                                if ((sumUtilization + Convert.ToInt32(objHRRF.Utilization)) > 100)
                                                {
                                                    dojbenck = false;
                                                }

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
                                                             }).ToList();
                                                if (unass != null)
                                                {
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

                                                projAssignment.Utilization = benchUtilization;
                                                projAssignment.EmployeeId = propsedEmployeeid;
                                                // need to check future date and active and inactive
                                                projAssignment.IsActive = true;
                                                projAssignment.Assigned_By = unass.FirstOrDefault().projmanger;
                                                projAssignment.Assigned_Date = System.DateTime.Now;
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
                                                projBenchHistory.EmployeeId = propsedEmployeeid;
                                                projBenchHistory.EnddateOld = null;
                                                projBenchHistory.IsActiveOld = null;
                                                projBenchHistory.StartDateOld = null;
                                                projBenchHistory.UtilizationOld = null;
                                                projBenchHistory.modifiedBy = loginempID;
                                                projBenchHistory.ModifiedDate = DateTime.Now;
                                                projBenchHistory.UtilizationNew = Utilzation;
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

                                    ProjectAssignment objProjectAssignment = new ProjectAssignment();
                                    // Check with no of positions
                                    // proposed resource count
                                    int positions = Convert.ToInt32(objProposePostions.Count);
                                    //hrrf postions count
                                    int hrrfpositions = Convert.ToInt32(objHRRF.Positions);
                                    if (positions < hrrfpositions)
                                    {
                                        //object mapping
                                        var objProposing = db.ProposeAssociates.Where(p => p.EmpID == proposedEmpid && p.HRRFNumber == HRRFNumber).FirstOrDefault();
                                        objHRRF.ResourceName = objPropose.EmpID + "(" + objPropose.EmpName + ")";
                                        objProposing.PracticeStatus = praticeAccept;
                                        db.Entry(objProposing).State = System.Data.Entity.EntityState.Modified;
                                        // fulfill positions
                                        var objHrrfFulfillment = db.HRRFs.Where(p => p.HRRFNumber == HRRFNumber).FirstOrDefault();
                                        string strPrevReqStatus = objHrrfFulfillment.RequestStatus;
                                        objHrrfFulfillment.RequestStatus = fulfilled;
                                        objHrrfFulfillment.ModifiedDate = DateTime.Now;
                                        db.Entry(objHrrfFulfillment).State = System.Data.Entity.EntityState.Modified;
                                        // HRRF histroy 
                                        TRScernarioHistroy(HRRFNumber, objPropose.EmpName + " - " + "has been Accepted for" + " - " + HRRFNumber, objHrrfFulfillment.Remarks, Convert.ToInt32(Session["EmployeeId"]), strPrevReqStatus);
                                        TRScernarioHistroy(HRRFNumber, objPropose.EmpName + " - " + "has been Fulfilled for" + " - " + HRRFNumber, objHrrfFulfillment.Remarks, Convert.ToInt32(Session["EmployeeId"]), strPrevReqStatus);
                                        // Project Assignment & Change Assignment status for Employee in EmployeeTable


                                        #region ManagerAssignment
                                        // get project ManagerId
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
                                            objProjectAssignment.EmployeeId = propsedEmployeeid;
                                            objProjectAssignment.Utilization = utl;
                                            // need to check future date and active and inactive
                                            if (DateTime.Now.Date >= Convert.ToDateTime(objHRRF.BillingDate).Date)
                                            {
                                                objProjectAssignment.IsActive = true;
                                            }
                                            else
                                            {
                                                objProjectAssignment.IsActive = false;
                                            }
                                            objProjectAssignment.Assigned_By = name;
                                            objProjectAssignment.Assigned_Date = System.DateTime.Now;
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
                                            pash.EmployeeId = propsedEmployeeid;
                                            pash.EnddateOld = null;
                                            pash.IsActiveOld = null;
                                            pash.StartDateOld = null;
                                            pash.UtilizationOld = null;
                                            pash.modifiedBy = loginempID;
                                            pash.ModifiedDate = DateTime.Now;
                                            pash.UtilizationNew = utl;
                                            pash.StartDateNew = objHRRF.BillingDate;
                                            if (DateTime.Now.Date >= Convert.ToDateTime(objHRRF.BillingDate).Date)
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
                                            if (objHRRF.Purpose.ToLower() == "project")
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
                                                    prj1.EmployeeId = propsedEmployeeid;
                                                    prj1.ProjectID = unass[0].projectid;
                                                    prj1.ProjectCode = unass[0].Projectcode;
                                                    prj1.ProjectName = unass[0].ProjectName;
                                                    prj1.StartDate = objHRRF.BillingDate;
                                                    prj1.EndDate = objHRRF.AssignmentEndDate;
                                                    if (DateTime.Now.Date >= Convert.ToDateTime(objHRRF.BillingDate).Date)
                                                    {
                                                        prj1.IsActive = true;
                                                    }
                                                    else
                                                    {
                                                        prj1.IsActive = false;
                                                    }
                                                    prj1.Assigned_By = unass[0].projmanger;
                                                    prj1.Assigned_Date = DateTime.Now;
                                                    prj1.BillingStatus = unass[0].Billingstatus;
                                                    prj1.Utilization = utl;
                                                    if (prj1.BillingStatus == "Bench")
                                                    {
                                                        prj1.Category = "Deployable Bench";
                                                        prj1.Bechstatus = "Free Pool";
                                                    }
                                                    db.ProjectAssignments.Add(prj1);


                                                    #region assignmenthistory
                                                    ProjectAssignmenthistory pash = new ProjectAssignmenthistory();
                                                    pash.AssignmentId = 0;
                                                    pash.ProjectCode = unass[0].Projectcode;
                                                    pash.ProjectName = unass[0].ProjectName;
                                                    pash.ProjectID = unass[0].projectid;
                                                    pash.Assigned_ByOld = null;
                                                    pash.BillingStatusOld = null;
                                                    pash.EmployeeId = propsedEmployeeid;
                                                    pash.EnddateOld = null;
                                                    pash.IsActiveOld = null;
                                                    pash.StartDateOld = null;
                                                    pash.UtilizationOld = null;
                                                    pash.modifiedBy = loginempID;
                                                    pash.ModifiedDate = DateTime.Now;
                                                    pash.UtilizationNew = utl;
                                                    pash.StartDateNew = objHRRF.BillingDate;
                                                    if (DateTime.Now.Date >= Convert.ToDateTime(objHRRF.BillingDate).Date)
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

                                        // final saving after assigning and updating
                                        db.SaveChanges();
                                        var proposedEmpName = db.Employees.Where(p => p.EmployeeId == propsedEmployeeid).FirstOrDefault();
                                        var isvalidbusinessgroup = db.IncludedBussinessGroups.Where(i => i.BusinessGroup.Contains(proposedEmpName.BusinessGroup)).FirstOrDefault();
                                        var isinvalidemployetype = db.ExcludedEmployeeTypes.Where(i => i.EmployeeType.Contains(proposedEmpName.EmployeeType)).FirstOrDefault();

                                        // email configure    added ops team in cc part                      
                                        InsertNotification(Session["EmployeeId"].ToString(), Convert.ToString(objHRRF.HRRFCreatedBy), Environment.GetEnvironmentVariable("TrAcceptedResources") ?? "", Environment.GetEnvironmentVariable("TrProposedResAce_NOtification") ?? "", HRRFNumber, "<b>" + Environment.GetEnvironmentVariable("AcceptedResource") ?? "" + ":</b>" + proposedEmpName.FirstName + proposedEmpName.LastName, Environment.GetEnvironmentVariable("ITS-OPS") ?? ""+","+ Environment.GetEnvironmentVariable("Assurance") ?? "");
                                        //lakshmi
                                        if (isvalidbusinessgroup != null && isinvalidemployetype == null)
                                        {
                                            InsertNotification(Session["EmployeeId"].ToString(), proposedEmpid, Environment.GetEnvironmentVariable("Acceptedto") ?? "" + objHRRF.ProjectName, Environment.GetEnvironmentVariable("AcceptedResource") ?? "", HRRFNumber, string.Empty, null);
                                        }
                                        InsertNotification(Session["EmployeeId"].ToString(), Session["EmployeeId"].ToString(), Environment.GetEnvironmentVariable("TrAcceptedResources_by") ?? "", Environment.GetEnvironmentVariable("TrProposedRes_NOtification") ?? "" + objHRRF.ProjectName, HRRFNumber, string.Empty, null);
                                    }
                                }
                            }
                            // to give alert to user for accepted resources -- PENDING
                            else if (propsedLog.Count >= 1)
                            {
                                var propsed = db.ProposeAssociates.Where(h => h.EmpID == proposedEmpid && h.HRRFNumber == HRRFNumber).FirstOrDefault();
                                string empName = propsed.EmpName;
                                strAlreadyAcceptedList.Add(empName);
                            }
                        }
                        var AlreadyAccepted = "";
                        if (strAlreadyAcceptedList.Count > 0)
                        {
                            AlreadyAccepted = string.Join(",", strAlreadyAcceptedList.ToArray());
                        }

                        return Json(AlreadyAccepted);
                    }
                    else
                    {
                        return Json("");
                    }
                }
                else
                {
                    return Json("");
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

        public ActionResult ValidateHRRFNo(string duplicatehrrfno)
        {
            try
            {
                var result = db.HRRFs.Where(p => p.HRRFNumber == duplicatehrrfno).Select(p => p.HRRFNumber).FirstOrDefault();
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

        public string DHNameFor_Mails(string requestStatus, string practice, string costcenter, Int32 grade, Int32 empID, string hrrfnumber)
        {
            string employid = "";
            if (requestStatus.ToLower() == PendingForDHApproval.ToLower())
            {
                string emp = empID.ToString();

                var getapprover = (from bpr in db.PracticeWiseBenchCodes
                                   join EF in db.ExternalFulfillments on bpr.Practice.ToLower() equals EF.Practice.ToLower()
                                   join H in db.HRRFs on EF.HRRFNumber equals H.HRRFNumber into tmpExterFulfill
                                   from H in tmpExterFulfill.DefaultIfEmpty()
                                   where H.CostCenter.ToLower().Contains(bpr.CostCenter.ToLower())
                                   //&& bpr.DeliveryHeadID.Contains(emp)
                                   && H.HRRFNumber == hrrfnumber
                                   && bpr.TRGrade.Contains(grade.ToString())
                                   && bpr.Practice.ToLower().Equals(practice.ToLower())
                                   && bpr.CostCenter.ToLower().Contains(costcenter.ToLower())
                                   && ((H.Purpose != "Project" && H.ProjectCode == bpr.BenchCode)
                                                 || (H.Purpose == "Project"))
                                   select new
                                   {
                                       deliveryheadeID = bpr.DeliveryHeadID
                                   }).Distinct().ToList();

                if (getapprover != null)
                {
                    if (getapprover.Count > 0)
                    {
                        int emplid = Convert.ToInt32(getapprover[0].deliveryheadeID);
                        //employeeName = "- " + (from empnam in db.Employees
                        //                       where empnam.EmployeeId == emplid
                        //                       select empnam.FirstName + " " + empnam.MiddleName + " " + empnam.LastName).FirstOrDefault();
                        employid = emplid.ToString();
                    }
                }
            }
            else if (requestStatus.ToLower() == PendingForPMApproval.ToLower())
            {
                Employee objEmp = db.Employees.Where(e => e.EmployeeId == empID).FirstOrDefault();

                if (objEmp != null)
                {
                    int emplid = objEmp.EmployeeId;
                    employid = emplid.ToString();
                    //employeeName = " - " + objEmp.FirstName + " " + objEmp.LastName;
                }
            }

            return employid;
        }

        [HttpPost]
        public ActionResult Reopen1(string HrrfNumber, string comments, string status, string request)
        {
            try
            {
                string HRRFNumber = HrrfNumber;
                string strPrevRequestStatus = "";
                var x = (from eh in db.ExternalHires where eh.HRRFNumber == HRRFNumber select eh).ToList();
                var hrrfData = (from hrrf in db.HRRFs where hrrf.HRRFNumber == HRRFNumber select hrrf).FirstOrDefault();
                if (hrrfData != null)
                {
                    if (hrrfData.RequestType == "External" && hrrfData.ExternalWebSite == "Yes")
                    {
                        hrrfData.UPDATEEXTERNAL = "Yes";
                    }
                    db.SaveChanges();
                }



                if (x == null)
                {
                    var objHRRF = db.HRRFs.Where(c => c.HRRFNumber == HRRFNumber).FirstOrDefault();
                    // hrrf request type Status
                    strPrevRequestStatus = objHRRF.RequestStatus;
                    //objHRRF.RequestType = convertInternal;



                    if (request == "External")
                    {
                        if (status == "Fulfilled")
                        {
                             TRScernarioHistroy(HRRFNumber, HRRFNumber + " - " + "It has been converted to  ResumePending status", comments, Convert.ToInt32(Session["EmployeeId"]), strPrevRequestStatus);
                          // TRScernarioHistroy(HRRFNumber, HRRFNumber + " - " + "It has been converted to  ResumePending status", objHRRF.Remarks, Convert.ToInt32(Session["EmployeeId"]), strPrevRequestStatus);

                            objHRRF.RequestStatus = ResumePending;
                        }
                        else if (status == "Cancelled")
                        {

                            string s = HrrfNumber + "" + " - has been Cancelled";   // in table getting more data but we need only for cancelled mode
                            var Cancelled = db.HRRFHistories.Where(c => c.HRRFNumber == HRRFNumber && c.HistoryDescription == s).FirstOrDefault();
                            if (Cancelled == null)
                            {
                                TRScernarioHistroy(HRRFNumber, HRRFNumber + " - " + "It has been converted to  " + PendingForDHApproval + " status", comments, Convert.ToInt32(Session["EmployeeId"]), strPrevRequestStatus);

                                //TRScernarioHistroy(HRRFNumber, HRRFNumber + " - " + "It has been converted to  " + PendingForDHApproval + " status", objHRRF.Remarks, Convert.ToInt32(Session["EmployeeId"]), strPrevRequestStatus);

                                objHRRF.RequestStatus = PendingForDHApproval;
                            }
                            else
                            {

                                TRScernarioHistroy(HRRFNumber, HRRFNumber + " - " + "It has been converted to  " + Cancelled.PrevRequestStatus + " status", comments, Convert.ToInt32(Session["EmployeeId"]), strPrevRequestStatus);
                                //TRScernarioHistroy(HRRFNumber, HRRFNumber + " - " + "It has been converted to  " + Cancelled.PrevRequestStatus + " status", objHRRF.Remarks, Convert.ToInt32(Session["EmployeeId"]), strPrevRequestStatus);

                                objHRRF.RequestStatus = Cancelled.PrevRequestStatus;
                            }

                        }
                    }




                    else if (request == "Internal")
                    {
                        if (status == "Fulfilled")
                        {
                            TRScernarioHistroy(HRRFNumber, HRRFNumber + " - " + "It has been converted to  " + qualified + " status", comments, Convert.ToInt32(Session["EmployeeId"]), strPrevRequestStatus);
                            //TRScernarioHistroy(HRRFNumber, HRRFNumber + " - " + "It has been converted to  " + qualified + " status", objHRRF.Remarks, Convert.ToInt32(Session["EmployeeId"]), strPrevRequestStatus);

                            objHRRF.RequestStatus = qualified;
                        }
                        else if (status == "Cancelled")
                        {

                            string s = HrrfNumber + "" + " - has been Cancelled";
                            var Cancelled = db.HRRFHistories.Where(c => c.HRRFNumber == HRRFNumber && c.HistoryDescription == s).FirstOrDefault();

                            if (Cancelled == null)
                            {
                                TRScernarioHistroy(HRRFNumber, HRRFNumber + " - " + "It has been converted to  " + Submitted + " status", comments, Convert.ToInt32(Session["EmployeeId"]), strPrevRequestStatus);

                               // TRScernarioHistroy(HRRFNumber, HRRFNumber + " - " + "It has been converted to  " + Submitted + " status", objHRRF.Remarks, Convert.ToInt32(Session["EmployeeId"]), strPrevRequestStatus);

                                objHRRF.RequestStatus = Submitted;
                            }
                            else
                            {
                                TRScernarioHistroy(HRRFNumber, HRRFNumber + " - " + "It has been converted to  " + Cancelled.PrevRequestStatus + " status", comments, Convert.ToInt32(Session["EmployeeId"]), strPrevRequestStatus);

                               // TRScernarioHistroy(HRRFNumber, HRRFNumber + " - " + "It has been converted to  " + Cancelled.PrevRequestStatus + " status", objHRRF.Remarks, Convert.ToInt32(Session["EmployeeId"]), strPrevRequestStatus);

                                objHRRF.RequestStatus = Cancelled.PrevRequestStatus;
                            }


                        }


                    }

                    objHRRF.ResourceName = null;
                    objHRRF.Remarks = comments;
                    objHRRF.InternalExpectedFulfilmentDate = DateTime.Now.AddDays(5);


                    //TRScernarioHistroy(HRRFNumber, HRRFNumber + " - " + "It has been converted to  ResumePending status", objHRRF.Remarks, Convert.ToInt32(Session["EmployeeId"]), strPrevRequestStatus);


                    //saving
                    db.SaveChanges();
                    //InsertNotification(Session["EmployeeId"].ToString(), Session["EmployeeId"].ToString(), Environment.GetEnvironmentVariable("TrConvertToExt_By") ?? ConfigurationManager.AppSettings["TrConvertToExt_By"], Environment.GetEnvironmentVariable("TrConverttoExt_NOtification") ?? ConfigurationManager.AppSettings["TrConverttoExt_NOtification"], HRRFNumber, string.Empty);



                    //return RedirectToAction("Index", "ExternalReport");
                    return Json("success", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var objHRRF = db.HRRFs.Where(c => c.HRRFNumber == HRRFNumber).FirstOrDefault();
                    strPrevRequestStatus = objHRRF.RequestStatus;
                    //objHRRF.RequestType = convertInternal;
                    //objHRRF.RequestStatus = ResumePending;

                    if (request == "External")
                    {
                        if (status == "Fulfilled")
                        {
                            TRScernarioHistroy(HRRFNumber, HRRFNumber + " - " + "It has been converted to  " + ResumePending + " status", comments, Convert.ToInt32(Session["EmployeeId"]), strPrevRequestStatus);

                           // TRScernarioHistroy(HRRFNumber, HRRFNumber + " - " + "It has been converted to  " + ResumePending + " status", objHRRF.Remarks, Convert.ToInt32(Session["EmployeeId"]), strPrevRequestStatus);

                            objHRRF.RequestStatus = ResumePending;
                        }
                        else if (status == "Cancelled")
                        {

                            string s = HrrfNumber + "" + " - has been Cancelled";
                            var Cancelled = db.HRRFHistories.Where(c => c.HRRFNumber == HRRFNumber && c.HistoryDescription == s).FirstOrDefault();
                            if (Cancelled == null)
                            {
                                TRScernarioHistroy(HRRFNumber, HRRFNumber + " - " + "It has been converted to  " + PendingForDHApproval + " status", comments, Convert.ToInt32(Session["EmployeeId"]), strPrevRequestStatus);

                              //  TRScernarioHistroy(HRRFNumber, HRRFNumber + " - " + "It has been converted to  " + PendingForDHApproval + " status", objHRRF.Remarks, Convert.ToInt32(Session["EmployeeId"]), strPrevRequestStatus);

                                objHRRF.RequestStatus = PendingForDHApproval;
                            }
                            else
                            {
                                TRScernarioHistroy(HRRFNumber, HRRFNumber + " - " + "It has been converted to  " + Cancelled.PrevRequestStatus + " status", comments, Convert.ToInt32(Session["EmployeeId"]), strPrevRequestStatus);

                              //  TRScernarioHistroy(HRRFNumber, HRRFNumber + " - " + "It has been converted to  " + Cancelled.PrevRequestStatus + " status", objHRRF.Remarks, Convert.ToInt32(Session["EmployeeId"]), strPrevRequestStatus);

                                objHRRF.RequestStatus = Cancelled.PrevRequestStatus;

                            }
                        }

                    }
                    else if (request == "Internal")
                    {
                        if (status == "Fulfilled")
                        {
                            TRScernarioHistroy(HRRFNumber, HRRFNumber + " - " + "It has been converted to  " + qualified + " status", comments, Convert.ToInt32(Session["EmployeeId"]), strPrevRequestStatus);

                          //  TRScernarioHistroy(HRRFNumber, HRRFNumber + " - " + "It has been converted to  " + qualified + " status", objHRRF.Remarks, Convert.ToInt32(Session["EmployeeId"]), strPrevRequestStatus);

                            objHRRF.RequestStatus = qualified;
                        }
                        else if (status == "Cancelled")
                        {

                            string s = HrrfNumber + "" + " - has been Cancelled";
                            var Cancelled = db.HRRFHistories.Where(c => c.HRRFNumber == HRRFNumber && c.HistoryDescription == s).FirstOrDefault();
                            if (Cancelled == null)
                            {
                                TRScernarioHistroy(HRRFNumber, HRRFNumber + " - " + "It has been converted to  " + Submitted + " status", comments, Convert.ToInt32(Session["EmployeeId"]), strPrevRequestStatus);

                               // TRScernarioHistroy(HRRFNumber, HRRFNumber + " - " + "It has been converted to  " + Submitted + " status", objHRRF.Remarks, Convert.ToInt32(Session["EmployeeId"]), strPrevRequestStatus);

                                objHRRF.RequestStatus = Submitted;
                            }
                            else
                            {
                                TRScernarioHistroy(HRRFNumber, HRRFNumber + " - " + "It has been converted to  " + Cancelled.PrevRequestStatus + " status", comments, Convert.ToInt32(Session["EmployeeId"]), strPrevRequestStatus);

                                //TRScernarioHistroy(HRRFNumber, HRRFNumber + " - " + "It has been converted to  " + Cancelled.PrevRequestStatus + " status", objHRRF.Remarks, Convert.ToInt32(Session["EmployeeId"]), strPrevRequestStatus);

                                objHRRF.RequestStatus = Cancelled.PrevRequestStatus;
                            }

                        }

                    }

                    objHRRF.ResourceName = null;
                    objHRRF.InternalExpectedFulfilmentDate = DateTime.Now.AddDays(5);
                    objHRRF.Remarks = comments;
                    db.Entry(objHRRF).State = System.Data.Entity.EntityState.Modified;



                    //TRScernarioHistroy(HRRFNumber, HRRFNumber + " - " + "has been changed to ResumePending status", objHRRF.Remarks, Convert.ToInt32(Session["EmployeeId"]), strPrevRequestStatus);
                    //Get the ID of ExternalHire Table using HrrfNumber
                    int externalid = Convert.ToInt32((from eh in db.ExternalHires where eh.HRRFNumber == HRRFNumber select eh.ExternalHireId).FirstOrDefault());
                    if (externalid != 0)
                    {
                        ExternalHire externalhire = db.ExternalHires.Find(externalid);
                        db.ExternalHires.Remove(externalhire);
                        db.SaveChanges();
                    }
                    db.SaveChanges();
                    //return RedirectToAction("Index", "ExternalReport");

                    return Json("success", JsonRequestBehavior.AllowGet);
                }

            }

            catch (Exception ex)
            {
                Common.WriteExceptionErrorLog(ex);
                return Json("Error", JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult SaveRemark(string remark)
        {
            int empid = Convert.ToInt32(Session["EmployeeId"]);
            string hrrf = string.Empty;

            if (Session["hrrfno"] != null)
            {
                hrrf = Session["hrrfno"].ToString();
            }
            if (empid > 0)
            {
                try
                {
                    HRRFRemark objhrrf = new HRRFRemark
                    {
                        HrrfNo = hrrf,
                        Remarks = remark,
                        Submitteddate = DateTime.Now,
                        SubmittedBy = Convert.ToString(Session["EmployeeId"])
                    };

                    db.HRRFRemarks.Add(objhrrf);
                    db.SaveChanges();

                    TempData["hrrnumber"] = hrrf;
                  
                    TempData.Keep("hrrnumber"); // above like is mandatory

                    return Json("true", JsonRequestBehavior.AllowGet);

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

        [HttpPost]
        public JsonResult SaveSatus(string status)
        {
            int empid = Convert.ToInt32(Session["EmployeeId"]);
            string hrrf = string.Empty;

            if (Session["hrrfno"] != null)
            {
                hrrf = Session["hrrfno"].ToString();
            }
            if (empid > 0)
            {
                try
                {
                    HRRFStatusHistory objhrrf = new HRRFStatusHistory
                    {
                        HrrfNo = hrrf,
                        Status = status,
                        Submitteddate = DateTime.Now,
                        SubmittedBy = Convert.ToString(Session["EmployeeId"])
                    };

                    db.HRRFStatusHistories.Add(objhrrf);
                    db.SaveChanges();

                    TempData["hrrnumber"] = hrrf;

                    TempData.Keep("hrrnumber"); // above like is mandatory

                    return Json("true", JsonRequestBehavior.AllowGet);

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
        public ActionResult SaveInternalDate(string InternalExpectedFulfilmentDate)
        {
            try
            {
                string HRRFNumber = Session["hrrfno"].ToString();
                //TempData["hrrfno"] = HRRFNumber;
                var hrrfResultQualify = db.HRRFs.Where(h => h.HRRFNumber == HRRFNumber).ToList().FirstOrDefault();

                //DateTime result = DateTime.ParseExact(InternalExpectedFulfilmentDate, "dd/MM/yyyy", theCultureInfo);
                //hrrfResultQualify.InternalExpectedFulfilmentDate = result;//DateTime.ParseExact(InternalExpectedFulfilmentDate, "g", theCultureInfo);
                hrrfResultQualify.InternalExpectedFulfilmentDate = DateTime.Parse(InternalExpectedFulfilmentDate);
                db.SaveChanges();
                return RedirectToAction("ProposeAssociate", "ProposeAssociate", new { hrrfno = HRRFNumber });

            }

            catch (Exception ex)
            {

                Common.WriteExceptionErrorLog(ex);
                return RedirectToAction("Error", "Error");
                //return Json("Error", JsonRequestBehavior.AllowGet);
            }

        }
    }



}
