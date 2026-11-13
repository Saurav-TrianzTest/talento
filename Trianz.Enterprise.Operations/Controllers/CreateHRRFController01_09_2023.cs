using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Trianz.Enterprise.Operations.General;
using Trianz.Enterprise.Operations.Models;

namespace Trianz.Enterprise.Operations.Controllers
{
    public class CreateHRRFController : Controller
    {

        private TrianzOperationsEntities db = new TrianzOperationsEntities();
        ServiceAgent AgentService = new ServiceAgent();
        // Get constant values from web.config
        string TR_Terminate = Environment.GetEnvironmentVariable("TR_Terminate") ?? "";
        string TR_Submit = Environment.GetEnvironmentVariable("TR_Submit") ?? "";
        string TR_Submitted = Environment.GetEnvironmentVariable("TR_Submitted") ?? "";
        string TR_Save = Environment.GetEnvironmentVariable("TR_Save") ?? "";
        string TR_Draft = Environment.GetEnvironmentVariable("TR_Draft") ?? "";
        string TR_TerminateCheck = Environment.GetEnvironmentVariable("TR_TerminateCheck") ?? "";
        string TR_CompanyCode = Environment.GetEnvironmentVariable("TR_CompanyCode") ?? "";
        string TR_Internal = Environment.GetEnvironmentVariable("TR_Internal") ?? "";
        string TR_Clear = Environment.GetEnvironmentVariable("TR_Clear") ?? "";
        string IsEmailSent = Environment.GetEnvironmentVariable("IsEmailSent") ?? "";

        // GET: CreateHRRF
        public ActionResult Index()
        {
            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            {
                try
                {
                    ServiceAgent agentservice = new ServiceAgent();
                    return View(db.HRRFs.ToList());
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

        public string GetPrimaryPracticeByName(string opportunity)
        {
            string primaryPractice = (from data in db.Opportunities.Where(x => x.potentialname.ToLower() == opportunity.ToLower()) select data.PrimaryPractice).FirstOrDefault();
            return primaryPractice;
        }
        public ActionResult ExportCumulativeReport()
        {
            return Json("success", JsonRequestBehavior.AllowGet);
        }
        public ActionResult GenerateCumulativeFReport(string strPractice)
        {
            List < HRRFSkillCluster > list = new List<HRRFSkillCluster>();
            list = db.Database.SqlQuery<HRRFSkillCluster>("USP_GetCumulativeReportByPractice @Practice",
                new System.Data.SqlClient.SqlParameter("Practice", strPractice)).ToList();


            #region Export to Excel

            using (ExcelPackage package = new ExcelPackage())
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("CumulativeReport Information");
                worksheet.TabColor = System.Drawing.Color.Green;
                worksheet.DefaultRowHeight = 18f;
                worksheet.Row(1).Height = 20f;

                using (var range = worksheet.Cells[1, 1, 1, 4])
                {
                    range.Style.Font.Bold = true;
                    range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#5B9BD5"));
                    range.Style.Font.Color.SetColor(System.Drawing.Color.White);

                    range.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    range.Style.Border.Right.Color.SetColor(System.Drawing.Color.LightGray);
                }

                // Column Headers
                worksheet.Cells[1, 1].Value = "Skill Cluster";
                worksheet.Cells[1, 2].Value = "Skill";
                worksheet.Cells[1, 3].Value = "Skill Code";
                worksheet.Cells[1, 4].Value = "TR Count";
               

                //Set default column width
                worksheet.DefaultColWidth = 18f;
                worksheet.Column(1).Width = 50f;
                worksheet.Column(2).Width = 50f;
                worksheet.Column(3).Width = 40f;
                worksheet.Column(4).Width = 25f;
                //Add the each row
                for (int rowIndex = 0, row = 2; rowIndex < list.Count; rowIndex++, row++) 

                {

                    worksheet.Cells[row, 1].Value = list[rowIndex].SkillCluster;
                    worksheet.Cells[row, 2].Value = list[rowIndex].Skill;
                    worksheet.Cells[row, 3].Value = list[rowIndex].SkillCode;
                    worksheet.Cells[row, 4].Value = list[rowIndex].HRRFCount;
                    if (row % 2 == 1)
                    {
                        using (var range = worksheet.Cells[row, 1, row, 4])
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
                Response.AddHeader("content-disposition", "attachment;filename=CumulativeReport.xlsx");

                Response.Charset = "";
                Response.ContentType = "application/vnd.ms-excel";
                StringWriter sw = new StringWriter();
                Response.BinaryWrite(fileBytes);
                Response.End();
            }

            #endregion

            return new EmptyResult();
        }

        public ActionResult GetSkillClusterByPractice(string strPractice)
        {
            List<HRRFSkillCluster> list = new List<HRRFSkillCluster>();
            list = db.Database.SqlQuery<HRRFSkillCluster>("USP_GetCumulativeReportByPractice @Practice",
                new System.Data.SqlClient.SqlParameter("Practice", strPractice)).ToList();
            return PartialView("");
        }

        // POST: CreateHRRF/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "HRRFID,HRRFNumber,Purpose,AccountName,CostCenter,ProjectName,ProjectCode,DemandType,ResourceType,Criticality,IsContracting,Grade,RoleRequired,ExpFrom,ExpTo,ReservationStatus,ServiceLine,Costcenter,AccountNamewithID,EnagagementType,ForReplacementPosition,PrimarySkills,SecondarySkills,Domain,TechPanel,TechPanel2,ResourceExpectedDate,AssignmentStartDate,AssignmentEndDate,BillingDate,LocationType,LocationName,OtherLocation,OnsiteLocation,VisaType,ShiftInfo,TravelInfo,RequestReason,Remarks,RequestType,ReplacementEmpId,Justification,RequestStatus,ClientInterview,InfoByHiringManager,InfoByTCOTeam,CallCompleted,FullfilmentDate,ValidationInfo,RequesterName,RequesterEmailId,IsActive,HRRFCreatedBy,HRRFCreatedDate,HRRFSubmitedDate,ModifiedBy,ModifiedDate,Impact,Discipline,RoleGroup,CoreSkill,IntermediateSkill,AdvancedSkill,SpecificPlatform,Certifications,IsSpecialClient,IsIndustryDomainSkill,SpecialClient,IndustryDomainSkill")] HRRF hRRF)
        {
            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        db.HRRFs.Add(hRRF);
                        db.SaveChanges();
                        return RedirectToAction("Index");
                    }
                    return View(hRRF);
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

        public ActionResult SkillClusterView()
        {
            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            {
                try
                {
                    List<HRRFSkillCluster> list = new List<HRRFSkillCluster>();
                    list = db.Database.SqlQuery<HRRFSkillCluster>("USP_GetSkillClusterData").ToList();
                    return View(list);
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

        // POST: CreateHRRF/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.

        public void savehrrfAuditHistory(HRRF hrrfvalues, HRRF hrrfvaluesnew)
        {
            // string msg = "";
            try
            {
                HRRFAudit hrrfaud = new HRRFAudit();
                hrrfaud.HRRFID = hrrfvalues.HRRFID;
                hrrfaud.HRRFNumber = hrrfvalues.HRRFNumber;
                hrrfaud.Purpose = hrrfvalues.Purpose;
                hrrfaud.ProjectName = hrrfvalues.ProjectName;
                hrrfaud.ProjectCode = hrrfvalues.ProjectCode;
                hrrfaud.Grade = hrrfvalues.Grade;
                hrrfaud.RoleRequired = hrrfvalues.RoleRequired;
                hrrfaud.AssignmentStartDate = hrrfvalues.AssignmentStartDate;
                hrrfaud.AssignmentEndDate = hrrfvalues.AssignmentEndDate;
                hrrfaud.LocationType = hrrfvalues.LocationType;
                hrrfaud.LocationName = hrrfvalues.LocationName;
                hrrfaud.Practice = hrrfvalues.Practice;
                hrrfaud.AccountName = hrrfvalues.AccountName;
                hrrfaud.CostCenter = hrrfvalues.CostCenter;
                hrrfaud.ResourceType = hrrfvalues.ResourceType;
                hrrfaud.EnagagementType = hrrfvalues.EnagagementType;
                hrrfaud.DemandType = hrrfvalues.DemandType;
                hrrfaud.JobDescription = hrrfvalues.JobDescription;
                hrrfaud.RequestReason = hrrfvalues.RequestReason;
                hrrfaud.Impact = hrrfvalues.Impact;
                hrrfaud.Criticality = hrrfvalues.Criticality;
                hrrfaud.Utilization = hrrfvalues.Utilization;
                hrrfaud.BillingDate = hrrfvalues.BillingDate;
                hrrfaud.IsContracting = hrrfvalues.IsContracting;

                hrrfaud.PurposeNew = hrrfvaluesnew.Purpose;
                hrrfaud.ProjectNameNew = hrrfvaluesnew.ProjectName;
                hrrfaud.ProjectCodeNew = hrrfvaluesnew.ProjectCode;
                hrrfaud.GradeNew = hrrfvaluesnew.Grade;
                hrrfaud.RoleRequiredNew = hrrfvaluesnew.RoleRequired;
                hrrfaud.AssignmentStartDateNew = hrrfvaluesnew.AssignmentStartDate;
                hrrfaud.AssignmentEndDateNew = hrrfvaluesnew.AssignmentEndDate;
                hrrfaud.LocationTypeNew = hrrfvaluesnew.LocationType;
                hrrfaud.LocationNameNew = hrrfvaluesnew.LocationName;
                hrrfaud.PracticeNew = hrrfvaluesnew.Practice;
                hrrfaud.AccountNameNew = hrrfvaluesnew.AccountName;
                hrrfaud.CostCenterNew = hrrfvaluesnew.CostCenter;
                hrrfaud.ResourceTypeNew = hrrfvaluesnew.ResourceType;
                hrrfaud.EnagagementTypeNew = hrrfvaluesnew.EnagagementType;
                hrrfaud.DemandTypeNew = hrrfvaluesnew.DemandType;
                hrrfaud.JobDescriptionNew = hrrfvaluesnew.JobDescription;
                hrrfaud.RequestReasonNew = hrrfvaluesnew.RequestReason;
                hrrfaud.ImpactNew = hrrfvaluesnew.Impact;
                hrrfaud.CriticalityNew = hrrfvaluesnew.Criticality;
                hrrfaud.UtilizationNew = hrrfvaluesnew.Utilization;
                hrrfaud.BillingDateNew = hrrfvaluesnew.BillingDate;
                hrrfaud.IsContractingNew = hrrfvaluesnew.IsContracting;
                hrrfaud.ModifiedDate = DateTime.Now;

                hrrfaud.Discipline = hrrfvalues.Discipline;
                hrrfaud.RoleGroup = hrrfvalues.RoleGroup;
                hrrfaud.CoreSkill = hrrfvalues.CoreSkill;
                hrrfaud.IntermediateSkill = hrrfvalues.IntermediateSkill;
                hrrfaud.AdvancedSkill = hrrfvalues.AdvancedSkill;
                hrrfaud.SpecificPlatform = hrrfvalues.SpecificPlatform;
                hrrfaud.Certifications = hrrfvalues.Certifications;
                hrrfaud.SpecialClient = hrrfvalues.SpecialClient;
                hrrfaud.IndustryDomainSkill = hrrfvalues.IndustryDomainSkill;
                hrrfaud.IsSpecialClient = hrrfvalues.IsSpecialClient;
                hrrfaud.IsIndustryDomainSkill = hrrfvalues.IsIndustryDomainSkill;
                
                hrrfaud.DisciplineNew = hrrfvaluesnew.Discipline;
                hrrfaud.RoleGroupNew = hrrfvaluesnew.RoleGroup;
                hrrfaud.CoreSkillNew = hrrfvaluesnew.CoreSkill;
                hrrfaud.IntermediateSkillNew = hrrfvaluesnew.IntermediateSkill;
                hrrfaud.AdvancedSkillNew = hrrfvaluesnew.AdvancedSkill;
                hrrfaud.SpecificPlatformNew = hrrfvaluesnew.SpecificPlatform;
                hrrfaud.CertificationsNew = hrrfvaluesnew.Certifications;
                hrrfaud.SpecialClientNew = hrrfvaluesnew.SpecialClient;
                hrrfaud.IndustryDomainSkillNew = hrrfvaluesnew.IndustryDomainSkill;
                hrrfaud.IsSpecialClientNew = hrrfvaluesnew.IsSpecialClient;
                hrrfaud.IsIndustryDomainSkillNew = hrrfvaluesnew.IsIndustryDomainSkill;
                db.HRRFAudits.Add(hrrfaud);
                //   db.Entry(hrrfaud).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

            }
            catch (Exception ex)
            {
                ErrorHandling expcls = new ErrorHandling();
                expcls.Error(ex);
            }

        }

        [HttpPost]
        public ActionResult ViewHRRF(FormCollection form, [Bind(Include = "HRRFID,HRRFNumber,Purpose,AccountName,RequestReason,Utilization,CostCenter,ProjectName,ProjectCode,Practice,DemandType,ResourceType,Criticality,IsContracting,Grade,RoleRequired,ExpFrom,ExpTo,EnagagementType,Domain,JobDescription,TechPanel,SecondTechPanel,ResourceExpectedDate,AssignmentStartDate,AssignmentEndDate,BillingDate,LocationType,LocationName,OtherLocation,ClientInterview,HRRFSubmitedDate,ModifiedBy,ModifiedDate,Positions,HRRFCreatedBy,HRRFCreatedDate,TRParent,IsActive,Isparent,RequestType,ReplacementEmpId,OpportunityCode,OpportunityName,VisaType,RequisitionID,Impact,Discipline,RoleGroup,CoreSkill,IntermediateSkill,AdvancedSkill,SpecificPlatform,Certifications,IsSpecialClient,IsIndustryDomainSkill,SpecialClient,IndustryDomainSkill,JobDescription,OrganizationGroup")] HRRF hRRF, string submit)
        {
            // 
            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            {
               
 //Common.WriteErrorLog("HrrfID : " + hRRF.HRRFNumber + "HRRFNUMBER : " + hRRF.HRRFNumber + "Purpose : " + hRRF.Purpose + "AccountName : " + hRRF.AccountName
 //+ "RequestReason : " + hRRF.RequestReason + "Utilization : " + hRRF.Utilization + "CostCenter : " + hRRF.CostCenter + "ProjectName : " + hRRF.ProjectName
 //+ "ProjectCode : " + hRRF.ProjectCode + "Practice : " + hRRF.Practice + "DemandType : " + hRRF.DemandType + "ResourceType : " + hRRF.ResourceType
 //+ "Criticality : " + hRRF.Criticality + "IsContracting : " + hRRF.IsContracting + "Grade : " + hRRF.Grade + "RoleRequired : " + hRRF.RoleRequired
 //+ "ExpFrom : " + hRRF.ExpFrom + "ExpTo : " + hRRF.ExpTo + "EnagagementType : " + hRRF.EnagagementType + "Domain : " + hRRF.Domain
 //+ "JobDescription : " + hRRF.JobDescription + "TechPanel : " + hRRF.TECHPANEL + "SecondTechPanel : " + hRRF.SECONDTECHPANEL /*+ "ResourceExpectedDate : " + hRRF.res*/
 //+ "AssignmentStartDate : " + hRRF.AssignmentStartDate + "AssignmentEndDate : " + hRRF.AssignmentEndDate + "BillingDate : " + hRRF.BillingDate
 //+ "LocationType : " + hRRF.LocationType);

                try
                {
                    List<ValidationModel> validationModel = new List<ValidationModel>();                   
                    string ViewName = "~/Views/Home/Index.cshtml";
                    // new code by team 26thsep2022
                    List<string> reason = new List<string>();
                    reason.Add("Replacement");
                    reason.Add("Attrition risk");
                    reason.Add("Rotation");
                    // new code by team 26thsep2022
                    if (ModelState.IsValid)
                    {
                        string hrrf_number = hRRF.HRRFNumber;
                        var hrrfvalues = db.HRRFs.AsNoTracking().Where(x => x.HRRFNumber == hrrf_number).FirstOrDefault();
                        string ProjectCode = Request.Form["ProjectCode"];
                        string ProjectName = (from data in db.Projects.Where(x => x.ProjectCode.ToLower() == ProjectCode.ToLower()) select data.ProjectName).FirstOrDefault();

                        //  db.Entry(lt).State = System.Data.Entity.EntityState.Modified;
                        //db.SaveChanges();

                        int EmpIdForCreatedBy = Convert.ToInt32(Session["EmployeeId"]);

                        //string ProjectName = Request.Form["ProjectName"];
                        //string ProjectCode = (from data in db.Projects.Where(x => x.ProjectName.ToLower() == ProjectName.ToLower()) select data.ProjectCode).FirstOrDefault();

                        string PrimarySkills = Request.Form["hiddenPrimarySkills"]; string SecondarySkills = Request.Form["hiddenSecondarySkills"];
                        string client = Request.Form["Clientint"];
                        if (!string.IsNullOrEmpty(client)) { client = "Yes"; } else { client = "No"; }
                        if (submit == TR_Submit)
                        {
                            string ch = getChanges(hRRF, PrimarySkills, SecondarySkills);
                            //if (ch == "")
                            //{

                            //}
                            //else
                            //{
                            string SubjectName = "Submitted";
                            int positions = Convert.ToInt32(hRRF.Positions);
                            string HRRFNumber = string.Empty;
                            string trParentNum = hRRF.HRRFNumber;
                            string CompanyCode = TR_CompanyCode;
                            for (int countPostion = 1; countPostion <= positions; countPostion++)
                            {
                                if (countPostion == 1)
                                {
                                    hRRF.RequestStatus = TR_Submitted;
                                    string opp = form["HRRF.OpportunityCode"];
                                    var opps = db.Opportunities.Where(x => x.potential_no == opp).FirstOrDefault();
                                    if (opps != null)
                                    {
                                        hRRF.OpportunitySalestage = opps.sales_Stage;
                                    }
                                    hRRF.RequestReason = Request.Form["RequestReason"];
                                    hRRF.CostCenter = Request.Form["CostCenter"];
                                    hRRF.Impact = Request.Form["Impact"];
                                    hRRF.Criticality = Request.Form["Criticality"];
                                    hRRF.Utilization = form["HRRF.Utilization"];
                                    hRRF.ProjectCode = ProjectCode;
                                    hRRF.RequisitionID = Request.Form["RequisitionID"];
                                    hRRF.IsContracting = Convert.ToBoolean(Request.Form["IsContracting"]); //(Request.Form["IsContracting"].ToLower() == "yes") ? true : false;
                                    hRRF.ProjectName = ProjectName;
                                    hRRF.ResourceType = Request.Form["ResourceType"];

                                    //hRRF.Discipline = Request.Form["Discipline"].ToString();
                                    //hRRF.RoleGroup = Request.Form["RoleGroup"].ToString();
                                    hRRF.CoreSkill = form["HRRF.CoreSkill"];//Request.Form["CoreSkill"].ToString();
                                    hRRF.IntermediateSkill = form["HRRF.IntermediateSkill"]; //Request.Form["IntermediateSkill"].ToString();
                                    hRRF.AdvancedSkill = form["HRRF.AdvancedSkill"];// Request.Form["AdvancedSkill"].ToString();
                                    hRRF.SpecificPlatform = Request.Form["SpecificPlatform"].ToString();
                                    hRRF.Certifications = Request.Form["Certifications"].ToString();
                                    hRRF.SpecialClient = form["HRRF.SpecialClient"].ToString();
                                    hRRF.IndustryDomainSkill = form["HRRF.IndustryDomainSkill"].ToString();
                                    hRRF.IsSpecialClient = Convert.ToBoolean(form["HRRF.IsSpecialClient"]);
                                    hRRF.IsIndustryDomainSkill = Convert.ToBoolean(form["HRRF.IsIndustryDomainSkill"]);
                                    hRRF.ShiftTime = Convert.ToString(Request.Form["ShiftTime"]);
                                    hRRF.JobDescription = form["HRRF.JobDescription"];
                                    hRRF.SkillCluster = Convert.ToString(Request.Form["SkillCluster"]);
                                    hRRF.PrimaryPractice = Request.Form["hdnPrimaryPractice"];

                                    hRRF.BILLRATE = string.IsNullOrEmpty(Request.Form["BillRate"].ToString()) ? (Decimal?)null : Convert.ToDecimal(Request.Form["BillRate"]);
                                    hRRF.MAXSAL = string.IsNullOrEmpty(Request.Form["MAXSAL"].ToString()) ? (Double?)null : Convert.ToDouble(Request.Form["MAXSAL"]);
                                    hRRF.CSKILL = Convert.ToString(Request.Form["CSkill"]);
                                    var skillCode = db.PRTCSKILLCLUSTERs.Where(x => x.PRACTICE == hRRF.Practice && x.SKILLCLUSTER == hRRF.SkillCluster && x.CSKILLS == hRRF.CSKILL).FirstOrDefault();
                                    if (skillCode != null)
                                    {
                                        hRRF.SkillCode = skillCode.SKILLCODE;
                                        hRRF.CLSTRDESC = db.SKILLCLSTRJDs.Where(x => x.SKILLCODE == skillCode.SKILLCODE).Select(x => x.CLSTRDESC).FirstOrDefault();
                                    }
                                    hRRF.CLSTRJD = form["HRRF.CLSTRJD"];
                                    if ((hRRF.RequestStatus == "Fulfilled" || hRRF.RequestStatus == "Cancelled" || hRRF.RequestStatus == "Terminate" || hRRF.RequestStatus == "On Hold") && hrrfvalues.ExternalWebSite == "Yes")
                                    {
                                        hRRF.UPDATEEXTERNAL = "Yes";
                                    }

                                    hRRF.ExternalWebSite = "No";

                                    if (hRRF.CLSTRJD == string.Empty || hRRF.CLSTRJD == null)
                                    {
                                        hRRF.CLSTRJD = hRRF.SkillCluster + " " + hRRF.SkillCode;
                                    }
                                    // new code by team 26thsep2022
                                    if (!reason.Contains(hRRF.RequestReason))
                                    {
                                        hRRF.ReplacementEmpId = null;
                                    }
                                    else
                                    {
                                        if (!string.IsNullOrWhiteSpace(form["HRRF.ReplacementEmpId"]))
                                        {
                                            hRRF.ReplacementEmpId = Convert.ToInt32(form["HRRF.ReplacementEmpId"]);
                                        }
                                    }
                                    //// new code by team 26thsep2022
                                    ///
                                    //if (!string.IsNullOrWhiteSpace(form["HRRF.ReplacementEmpId"]))
                                    //{
                                    //    hRRF.ReplacementEmpId = Convert.ToInt32(form["HRRF.ReplacementEmpId"]);
                                    //}
                                    if (Request.Form["LocationName"].ToLower() == "other")
                                    {
                                        hRRF.LocationName = Request.Form["OtherLocation"];
                                    }
                                    else
                                    {
                                        hRRF.LocationName = Request.Form["LocationName"];
                                    }
                                    if (Request.Form["RoleRequired"].ToLower() == "other")
                                    {
                                        hRRF.RoleRequired = Request.Form["OtherRole"];
                                    }
                                    else
                                    {
                                        hRRF.RoleRequired = Request.Form["RoleRequired"];
                                    }
                                    hRRF.ClientInterview = client;
                                    hRRF.Positions = 1;
                                    hRRF.HRRFSubmitedDate = DateTime.Now;
                                    hRRF.InternalExpectedFulfilmentDate = DateTime.Now.AddDays(5);
                                    hRRF.ModifiedBy = EmpIdForCreatedBy;
                                    hRRF.ModifiedDate = DateTime.Now;
                                    hRRF.OrganizationGroup = Request.Form["OrganizationGroup"];
                                    db.Entry(hRRF).State = EntityState.Modified;


                                    if (Request.Form["LocationName"].ToLower() == "other")
                                    {
                                        var result = (from val in db.MasterLookUps
                                                      where val.LookupType.ToLower() == "onsitelocation"
                                                      select val).Count();

                                        MasterLookUp objMasterLookup = new MasterLookUp();
                                        objMasterLookup.ApplicationCode = "TALENTREQ";
                                        objMasterLookup.LookupCode = Request.Form["OtherLocation"].ToString();
                                        objMasterLookup.LookupType = "OnSiteLocation";
                                        objMasterLookup.LookupName = Request.Form["OtherLocation"].ToString();
                                        objMasterLookup.Description = Request.Form["OtherLocation"].ToString();
                                        objMasterLookup.SeqNumber = result + 1;
                                        objMasterLookup.ParentCode = "1";
                                        objMasterLookup.ParentName = "LocationType";
                                        objMasterLookup.Active = true;
                                        objMasterLookup.DateCreated = DateTime.Now;
                                        objMasterLookup.DateModified = DateTime.Now;

                                        db.MasterLookUps.Add(objMasterLookup);
                                    }

                                    db.SaveChanges();
                                    var hrrfvaluesnew = db.HRRFs.AsNoTracking().Where(x => x.HRRFNumber == hrrf_number).FirstOrDefault();
                                    savehrrfAuditHistory(hrrfvalues, hrrfvaluesnew);
                                    //saveing data to history
                                    if (ch.Length != 0)
                                    {
                                        InsertRecordsToHistroy(hRRF.HRRFNumber, hRRF.HRRFNumber + "-" + hRRF.RequestStatus + "(Edited:" + ch + ")", EmpIdForCreatedBy, "Draft");
                                    }
                                    else
                                    {
                                        InsertRecordsToHistroy(hRRF.HRRFNumber, hRRF.HRRFNumber + "-" + hRRF.RequestStatus, EmpIdForCreatedBy, "Draft");
                                    }
                                    //  EditExistingHRRFSkills(PrimarySkills, SecondarySkills, trParentNum);
                                    SaveHRRFSkills(PrimarySkills, trParentNum);
                                    //if (ch.Length != 0)
                                    //{
                                    if ((hRRF.RequestStatus).ToLower() != "draft")
                                    {
                                        InsertRecordsToNotifications(trParentNum, SubjectName, ch);
                                    }
                                    //}
                                }
                                else
                                {
                                    //string NumberingSystem = AgentService.NumberingSystem(CompanyCode, "", "");
                                    //if (!string.IsNullOrEmpty(NumberingSystem))
                                    //{
                                    //    string[] SimplyNumber = NumberingSystem.Split(new[] { ":" }, StringSplitOptions.None);
                                    //    string output = SimplyNumber[1].Replace('{', '/').Replace('}', ' ').Replace('"', ' ');
                                    //    HRRFNumber = output.Trim();
                                    //}
                                    string branchCde = "";
                                    string sequenceSffix = "";
                                    long sequenceNmber = 0;
                                    string typeCde = "";
                                    ObjectParameter Result1 = new ObjectParameter("Result1", typeof(string));
                                    string datastoredproc = db.sp_NumberingSystemGenerateNumber(CompanyCode, branchCde, sequenceSffix, sequenceNmber, typeCde, Result1).ToString();
                                    string NumberingSystem = Result1.Value.ToString();
                                    HRRFNumber = NumberingSystem;



                                    //project info data
                                    hRRF.HRRFNumber = HRRFNumber;
                                    string opp = form["HRRF.OpportunityCode"];
                                    var opps = db.Opportunities.Where(x => x.potential_no == opp).FirstOrDefault();
                                    if (opps != null)
                                    {
                                        hRRF.OpportunitySalestage = opps.sales_Stage;
                                    }
                                    hRRF.TRParent = trParentNum;
                                    hRRF.Isparent = false;
                                    hRRF.RequestStatus = TR_Submitted;
                                    hRRF.RequestReason = Request.Form["RequestReason"];
                                    hRRF.CostCenter = Request.Form["CostCenter"];
                                    hRRF.Impact = Request.Form["Impact"];
                                    hRRF.Criticality = Request.Form["Criticality"];
                                    hRRF.Utilization = form["HRRF.Utilization"];
                                    hRRF.ProjectCode = ProjectCode;
                                    hRRF.RequisitionID = Request.Form["RequisitionID"];
                                    hRRF.IsContracting = Convert.ToBoolean(Request.Form["IsContracting"]); //(Request.Form["IsContracting"].ToLower() == "yes") ? true : false;
                                    hRRF.ResourceType = Request.Form["ResourceType"];

                                    // new code by team 26thsep2022
                                    if (!reason.Contains(hRRF.RequestReason))
                                    {
                                        hRRF.ReplacementEmpId = null;
                                    }
                                    else
                                    {
                                        if (!string.IsNullOrWhiteSpace(form["HRRF.ReplacementEmpId"]))
                                        {
                                            hRRF.ReplacementEmpId = Convert.ToInt32(form["HRRF.ReplacementEmpId"]);
                                        }
                                    }
                                    //// new code by team 26thsep2022
                                    hRRF.ProjectName = ProjectName;
                                    //if (!string.IsNullOrWhiteSpace(form["HRRF.ReplacementEmpId"]))
                                    //{
                                    //    hRRF.ReplacementEmpId = Convert.ToInt32(form["HRRF.ReplacementEmpId"]);
                                    //}
                                    if (Request.Form["LocationName"].ToLower() == "other")
                                    {
                                        hRRF.LocationName = Request.Form["OtherLocation"];
                                    }
                                    else
                                    {
                                        hRRF.LocationName = Request.Form["LocationName"];
                                    }
                                    if (Request.Form["RoleRequired"].ToLower() == "other")
                                    {
                                        hRRF.RoleRequired = Request.Form["OtherRole"];
                                    }
                                    else
                                    {
                                        hRRF.RoleRequired = Request.Form["RoleRequired"];
                                    }
                                    hRRF.ClientInterview = client;
                                    hRRF.HRRFSubmitedDate = DateTime.Now;
                                    hRRF.ModifiedBy = EmpIdForCreatedBy;
                                    hRRF.ModifiedDate = DateTime.Now;
                                    //    hRRF.HRRFCreatedBy = EmpIdForCreatedBy;
                                    //    hRRF.HRRFCreatedDate = DateTime.Now;
                                    hRRF.InternalExpectedFulfilmentDate = DateTime.Now.AddDays(5);
                                    hRRF.Positions = 1;
                                    hRRF.OrganizationGroup = Request.Form["OrganizationGroup"];
                                    db.HRRFs.Add(hRRF);
                                    db.SaveChanges();
                                    var hrrfvaluesnew = db.HRRFs.AsNoTracking().Where(x => x.HRRFNumber == hrrf_number).FirstOrDefault();
                                    savehrrfAuditHistory(hrrfvalues, hrrfvaluesnew);
                                    //saveing data to history
                                    InsertRecordsToHistroy(hRRF.HRRFNumber, hRRF.HRRFNumber + "-" + TR_Submitted, EmpIdForCreatedBy, "Draft");
                                    // insert primary and secondary skills
                                    //  EditExistingHRRFSkills(PrimarySkills, SecondarySkills, HRRFNumber);
                                    SaveHRRFSkills(PrimarySkills, HRRFNumber);
                                    //get OMs based on practice selected in herrf or tr
                                    //if (ch.Length != 0)
                                    //{
                                    if ((hRRF.RequestStatus).ToLower() != "draft")
                                    {
                                        InsertRecordsToNotifications(HRRFNumber, SubjectName, ch);
                                    }
                                    //}
                                }
                            }
                            //}
                            return RedirectToAction("Index", "trHome");
                        }
                        else if (submit == TR_Save)
                        {
                            string ch = getChanges(hRRF, PrimarySkills, SecondarySkills);
                            //if (ch == "")
                            //{

                            //}
                            //else
                            //{
                            string SubjectName = "Edited";
                            int positions = Convert.ToInt32(hRRF.Positions);
                            string HRRFNumber = string.Empty;
                            string trParentNum = hRRF.HRRFNumber;
                            string CompanyCode = TR_CompanyCode;
                            for (int countPostion = 1; countPostion <= positions; countPostion++)
                            {
                                if (countPostion == 1)
                                {
                                    hRRF.ProjectName = ProjectName;
                                    hRRF.ClientInterview = client;
                                    hRRF.Positions = 1;
                                    hRRF.RequestReason = Request.Form["RequestReason"];
                                    hRRF.CostCenter = Request.Form["CostCenter"];
                                    hRRF.PrimaryPractice = Request.Form["hdnPrimaryPractice"];
                                    hRRF.JDNotMatching = Request.Form["JDNotMatching"] != null ? Request.Form["JDNotMatching"] : "No";
                                    hRRF.Impact = Request.Form["Impact"];
                                    hRRF.Criticality = Request.Form["Criticality"];
                                    string opp = form["HRRF.OpportunityCode"];
                                    var opps = db.Opportunities.Where(x => x.potential_no == opp).FirstOrDefault();
                                    if (opps != null)
                                    {
                                        hRRF.OpportunitySalestage = opps.sales_Stage;
                                    }
                                    hRRF.Utilization = form["HRRF.Utilization"];
                                    //    if (!string.IsNullOrWhiteSpace(Request.Form["InternalExpectedFulfilmentDate"]))
                                    if (!string.IsNullOrWhiteSpace(form["HRRF.InternalExpectedFulfilmentDate"]))
                                    {
                                        hRRF.InternalExpectedFulfilmentDate = Convert.ToDateTime(form["HRRF.InternalExpectedFulfilmentDate"]);
                                    }
                                    hRRF.ProjectCode = ProjectCode;
                                    hRRF.RequisitionID = Request.Form["RequisitionID"];
                                    hRRF.IsContracting = Convert.ToBoolean(Request.Form["HRRF.IsContracting"]); //(Request.Form["IsContracting"].ToLower() == "yes") ? true : false;
                                    hRRF.ResourceType = Request.Form["ResourceType"];

                                    // new code by team 26thsep2022
                                    if (!reason.Contains(hRRF.RequestReason))
                                    {
                                        hRRF.ReplacementEmpId = null;
                                    }
                                    else
                                    {
                                        if (!string.IsNullOrWhiteSpace(form["HRRF.ReplacementEmpId"]))
                                        {
                                            hRRF.ReplacementEmpId = Convert.ToInt32(form["HRRF.ReplacementEmpId"]);
                                        }
                                    }
                                    //// new code by team 26thsep2022

                                    //if (!string.IsNullOrWhiteSpace(form["HRRF.ReplacementEmpId"]))
                                    //{
                                    //    hRRF.ReplacementEmpId = Convert.ToInt32(form["HRRF.ReplacementEmpId"]);
                                    //}
                                    if (Request.Form["LocationName"].ToLower() == "other")
                                    {
                                        hRRF.LocationName = Request.Form["OtherLocation"];
                                    }
                                    else
                                    {
                                        hRRF.LocationName = Request.Form["LocationName"];
                                    }
                                    if (Request.Form["RoleRequired"].ToLower() == "other")
                                    {
                                        hRRF.RoleRequired = Request.Form["OtherRole"];
                                    }
                                    else
                                    {
                                        hRRF.RoleRequired = Request.Form["RoleRequired"];
                                    }
                                    //   hRRF.HRRFSubmitedDate = DateTime.Now;
                                    hRRF.ModifiedBy = EmpIdForCreatedBy;
                                    hRRF.ModifiedDate = DateTime.Now;
                                    // hRRF.Discipline = Request.Form["Discipline"].ToString();
                                    //hRRF.RoleGroup = Request.Form["RoleGroup"].ToString();
                                    hRRF.CoreSkill = form["HRRF.CoreSkill"];// Request.Form["CoreSkill"].ToString();
                                    hRRF.IntermediateSkill = form["HRRF.IntermediateSkill"];// Request.Form["IntermediateSkill"].ToString();
                                    hRRF.AdvancedSkill = form["HRRF.AdvancedSkill"];// Request.Form["AdvancedSkill"].ToString();
                                    hRRF.SpecificPlatform = Request.Form["SpecificPlatform"].ToString();
                                    hRRF.Certifications = Request.Form["Certifications"].ToString();
                                    hRRF.SpecialClient = form["HRRF.SpecialClient"].ToString();
                                    hRRF.IndustryDomainSkill = form["HRRF.IndustryDomainSkill"].ToString();
                                    hRRF.IsSpecialClient = Convert.ToBoolean(form["HRRF.IsSpecialClient"]);
                                    hRRF.IsIndustryDomainSkill = Convert.ToBoolean(form["HRRF.IsIndustryDomainSkill"]);
                                    hRRF.RequestStatus = TempData["RequestType"].ToString();
                                    hRRF.ShiftTime = Convert.ToString(Request.Form["ShiftTime"]);
                                    hRRF.JobDescription = form["HRRF.JobDescription"];
                                    hRRF.SkillCluster = Convert.ToString(Request.Form["SkillCluster"]);
                                    hRRF.BILLRATE = string.IsNullOrEmpty(Request.Form["BillRate"].ToString()) ? (Decimal?)null : Convert.ToDecimal(Request.Form["BillRate"]);
                                    hRRF.MAXSAL = string.IsNullOrEmpty(Request.Form["MAXSAL"].ToString()) ? (Double?)null : Convert.ToDouble(Request.Form["MAXSAL"]);
                                    hRRF.CSKILL = Convert.ToString(Request.Form["CSkill"]);
                                    var skillCode = db.PRTCSKILLCLUSTERs.Where(x => x.PRACTICE == hRRF.Practice && x.SKILLCLUSTER == hRRF.SkillCluster && x.CSKILLS == hRRF.CSKILL).FirstOrDefault();
                                    if (skillCode != null)
                                    {
                                        hRRF.SkillCode = skillCode.SKILLCODE;
                                        hRRF.CLSTRDESC = db.SKILLCLSTRJDs.Where(x => x.SKILLCODE == skillCode.SKILLCODE).Select(x => x.CLSTRDESC).FirstOrDefault();
                                    }
                                    hRRF.CLSTRJD = form["HRRF.CLSTRJD"];
                                    if ((hRRF.RequestStatus == "Fulfilled" || hRRF.RequestStatus == "Cancelled" || hRRF.RequestStatus == "Terminate" || hRRF.RequestStatus == "On Hold") && hrrfvalues.ExternalWebSite == "Yes")
                                    {
                                        hRRF.UPDATEEXTERNAL = "Yes";
                                    }
                                    hRRF.ExternalWebSite = "No";
                                    if (hRRF.CLSTRJD == string.Empty || hRRF.CLSTRJD == null)
                                    {
                                        hRRF.CLSTRJD = hRRF.SkillCluster + " " + hRRF.SkillCode;
                                    }
                                    hRRF.OrganizationGroup = Request.Form["OrganizationGroup"].ToString(); 
                                    db.Entry(hRRF).State = EntityState.Modified;


                                    if (Request.Form["LocationName"].ToLower() == "other")
                                    {
                                        var result = (from val in db.MasterLookUps
                                                      where val.LookupType.ToLower() == "onsitelocation"
                                                      select val).Count();

                                        MasterLookUp objMasterLookup = new MasterLookUp();
                                        objMasterLookup.ApplicationCode = "TALENTREQ";
                                        objMasterLookup.LookupCode = Request.Form["OtherLocation"].ToString();
                                        objMasterLookup.LookupType = "OnSiteLocation";
                                        objMasterLookup.LookupName = Request.Form["OtherLocation"].ToString();
                                        objMasterLookup.Description = Request.Form["OtherLocation"].ToString();
                                        objMasterLookup.SeqNumber = result + 1;
                                        objMasterLookup.ParentCode = "1";
                                        objMasterLookup.ParentName = "LocationType";
                                        objMasterLookup.Active = true;
                                        objMasterLookup.DateCreated = DateTime.Now;
                                        objMasterLookup.DateModified = DateTime.Now;

                                        db.MasterLookUps.Add(objMasterLookup);
                                    }

                                    var externalFulfillment = db.ExternalFulfillments.Where(ext => ext.HRRFNumber == trParentNum).FirstOrDefault();

                                    if (externalFulfillment != null)
                                    {
                                        externalFulfillment.Practice = hRRF.Practice;

                                        db.Entry(externalFulfillment).State = EntityState.Modified;
                                    }

                                    db.SaveChanges();
                                    var hrrfvaluesnew = db.HRRFs.AsNoTracking().Where(x => x.HRRFNumber == hrrf_number).FirstOrDefault();
                                    savehrrfAuditHistory(hrrfvalues, hrrfvaluesnew);
                                    //  EditExistingHRRFSkills(PrimarySkills, SecondarySkills, trParentNum);
                                    SaveHRRFSkills(PrimarySkills, trParentNum);
                                    //saveing data to history
                                    //  if (TempData["RequestType"].ToString() == TR_Draft)
                                    //{ 
                                    if (ch.Length != 0)
                                    {
                                        InsertRecordsToHistroy(hRRF.HRRFNumber, hRRF.HRRFNumber + "-" + TempData["RequestType"].ToString() + "(Edited:" + ch + ")", EmpIdForCreatedBy, null);
                                    }
                                    else
                                    {
                                        InsertRecordsToHistroy(hRRF.HRRFNumber, hRRF.HRRFNumber + "-" + TempData["RequestType"].ToString(), EmpIdForCreatedBy, null);
                                    }
                                    if (ch.Length != 0)
                                    {
                                        if ((hRRF.RequestStatus).ToLower() != "draft")
                                        {
                                            InsertRecordsToNotifications(trParentNum, SubjectName, ch);
                                        }
                                    }

                                    // }

                                }
                                else
                                {
                                    //string NumberingSystem = AgentService.NumberingSystem(CompanyCode, "", "");
                                    //if (!string.IsNullOrEmpty(NumberingSystem))
                                    //{
                                    //    string[] SimplyNumber = NumberingSystem.Split(new[] { ":" }, StringSplitOptions.None);
                                    //    string output = SimplyNumber[1].Replace('{', '/').Replace('}', ' ').Replace('"', ' ');
                                    //    HRRFNumber = output.Trim();
                                    //}
                                    string branchCde = "";
                                    string sequenceSffix = "";
                                    long sequenceNmber = 0;
                                    string typeCde = "";
                                    ObjectParameter Result1 = new ObjectParameter("Result1", typeof(string));
                                    string datastoredproc = db.sp_NumberingSystemGenerateNumber(CompanyCode, branchCde, sequenceSffix, sequenceNmber, typeCde, Result1).ToString();
                                    string NumberingSystem = Result1.Value.ToString();
                                    HRRFNumber = NumberingSystem;
                                    //project info data
                                    hRRF.HRRFNumber = HRRFNumber;
                                    hRRF.TRParent = trParentNum;
                                    hRRF.RequestReason = Request.Form["RequestReason"];
                                    hRRF.CostCenter = Request.Form["CostCenter"];
                                    hRRF.PrimaryPractice = Request.Form["hdnPrimaryPractice"];
                                    hRRF.JDNotMatching = Request.Form["JDNotMatching"] != null ? Request.Form["JDNotMatching"] : "No";
                                    hRRF.Impact = Request.Form["Impact"];
                                    hRRF.Criticality = Request.Form["Criticality"];
                                    hRRF.Utilization = form["HRRF.Utilization"];
                                    string opp = form["HRRF.OpportunityCode"];
                                    var opps = db.Opportunities.Where(x => x.potential_no == opp).FirstOrDefault();
                                    if (opps != null)
                                    {
                                        hRRF.OpportunitySalestage = opps.sales_Stage;
                                    }
                                    if (!string.IsNullOrWhiteSpace(form["HRRF.InternalExpectedFulfilmentDate"]))
                                    {
                                        hRRF.InternalExpectedFulfilmentDate = Convert.ToDateTime(form["HRRF.InternalExpectedFulfilmentDate"]);
                                    }
                                    hRRF.ProjectName = ProjectName;
                                    hRRF.IsContracting = Convert.ToBoolean(Request.Form["IsContracting"]); //(Request.Form["IsContracting"].ToLower() == "yes") ? true : false;
                                    hRRF.ResourceType = Request.Form["ResourceType"];
                                    hRRF.ProjectCode = ProjectCode;
                                    hRRF.RequisitionID = Request.Form["RequisitionID"];

                                    // new code by team 26thsep2022
                                    if (!reason.Contains(hRRF.RequestReason))
                                    {
                                        hRRF.ReplacementEmpId = null;
                                    }
                                    else
                                    {
                                        if (!string.IsNullOrWhiteSpace(form["HRRF.ReplacementEmpId"]))
                                        {
                                            hRRF.ReplacementEmpId = Convert.ToInt32(form["HRRF.ReplacementEmpId"]);
                                        }
                                    }
                                    //// new code by team 26thsep2022

                                    //if (!string.IsNullOrWhiteSpace(form["HRRF.ReplacementEmpId"]))
                                    //{
                                    //    hRRF.ReplacementEmpId = Convert.ToInt32(form["HRRF.ReplacementEmpId"]);
                                    //}
                                    if (Request.Form["LocationName"].ToLower() == "other")
                                    {
                                        hRRF.LocationName = Request.Form["OtherLocation"];
                                    }
                                    else
                                    {
                                        hRRF.LocationName = Request.Form["LocationName"];
                                    }
                                    if (Request.Form["RoleRequired"].ToLower() == "other")
                                    {
                                        hRRF.RoleRequired = Request.Form["OtherRole"];
                                    }
                                    else
                                    {
                                        hRRF.RoleRequired = Request.Form["RoleRequired"];
                                    }
                                    hRRF.Isparent = false;
                                    hRRF.RequestStatus = TR_Draft;
                                    hRRF.ClientInterview = client;
                                    hRRF.ModifiedBy = EmpIdForCreatedBy;
                                    hRRF.ModifiedDate = DateTime.Now;
                                    //           hRRF.HRRFSubmitedDate = DateTime.Now;
                                    //           hRRF.HRRFCreatedBy = EmpIdForCreatedBy;
                                    //          hRRF.HRRFCreatedDate = DateTime.Now;
                                    hRRF.Positions = 1;
                                    // hRRF.Discipline = Request.Form["Discipline"].ToString();
                                    // hRRF.RoleGroup = Request.Form["RoleGroup"].ToString();
                                    // hRRF.CoreSkill = Request.Form["CoreSkill"].ToString();
                                    // hRRF.IntermediateSkill = Request.Form["IntermediateSkill"].ToString();
                                    //hRRF.AdvancedSkill = Request.Form["AdvancedSkill"].ToString();
                                    hRRF.SpecificPlatform = Request.Form["SpecificPlatform"].ToString();
                                    hRRF.Certifications = Request.Form["Certifications"].ToString();
                                    hRRF.SpecialClient = form["HRRF.SpecialClient"].ToString();
                                    hRRF.IndustryDomainSkill = form["HRRF.IndustryDomainSkill"].ToString();
                                    hRRF.IsSpecialClient = Convert.ToBoolean(form["HRRF.IsSpecialClient"]);
                                    hRRF.IsIndustryDomainSkill = Convert.ToBoolean(form["HRRF.IsIndustryDomainSkill"]);
                                    hRRF.SkillCluster = Convert.ToString(Request.Form["SkillCluster"]);
                                    hRRF.BILLRATE = string.IsNullOrEmpty(Request.Form["BillRate"].ToString()) ? (Decimal?)null : Convert.ToDecimal(Request.Form["BillRate"]);
                                    hRRF.MAXSAL = string.IsNullOrEmpty(Request.Form["MAXSAL"].ToString()) ? (Double?)null : Convert.ToDouble(Request.Form["MAXSAL"]);

                                    hRRF.CSKILL = Convert.ToString(Request.Form["CSkill"]);
                                    var skillCode = db.PRTCSKILLCLUSTERs.Where(x => x.PRACTICE == hRRF.Practice && x.SKILLCLUSTER == hRRF.SkillCluster && x.CSKILLS == hRRF.CSKILL).FirstOrDefault();
                                    if (skillCode != null)
                                    {
                                        hRRF.SkillCode = skillCode.SKILLCODE;
                                        hRRF.CLSTRDESC = db.SKILLCLSTRJDs.Where(x => x.SKILLCODE == skillCode.SKILLCODE).Select(x => x.CLSTRDESC).FirstOrDefault();
                                    }

                                    hRRF.CLSTRJD = Request.Form["CLSTRJD"];
                                    if ((hRRF.RequestStatus == "Fulfilled" || hRRF.RequestStatus == "Cancelled" || hRRF.RequestStatus == "Terminate" || hRRF.RequestStatus == "On Hold") && hRRF.ExternalWebSite == "Yes")
                                    {
                                        hRRF.UPDATEEXTERNAL = "Yes";
                                    }
                                    hRRF.ExternalWebSite = "No";
                                    if (hRRF.CLSTRJD == string.Empty || hRRF.CLSTRJD == null)
                                    {
                                        hRRF.CLSTRJD = hRRF.SkillCluster + " " + hRRF.SkillCode;
                                    }

                                    hRRF.OrganizationGroup = Request.Form["OrganizationGroup"].ToString();
                                    db.HRRFs.Add(hRRF);

                                    var externalFulfillment = db.ExternalFulfillments.Where(ext => ext.HRRFNumber == trParentNum).FirstOrDefault();

                                    if (externalFulfillment != null)
                                    {
                                        externalFulfillment.Practice = hRRF.Practice;

                                        db.Entry(externalFulfillment).State = EntityState.Modified;
                                    }

                                    db.SaveChanges();
                                    var hrrfvaluesnew = db.HRRFs.AsNoTracking().Where(x => x.HRRFNumber == hrrf_number).FirstOrDefault();
                                    savehrrfAuditHistory(hrrfvalues, hrrfvaluesnew);
                                    //saveing data to history
                                    InsertRecordsToHistroy(hRRF.HRRFNumber, hRRF.HRRFNumber + "-" + TR_Draft, EmpIdForCreatedBy, null);
                                    //   EditExistingHRRFSkills(PrimarySkills, SecondarySkills, HRRFNumber);
                                    SaveHRRFSkills(PrimarySkills, HRRFNumber);
                                    if (ch.Length != 0)
                                    {
                                        if ((hRRF.RequestStatus).ToLower() != "draft")
                                        {
                                            InsertRecordsToNotifications(HRRFNumber, SubjectName, ch);
                                        }
                                    }
                                }
                            }

                            return RedirectToAction("Index", "trHome");


                        }
                        else if (submit == TR_TerminateCheck)
                        {
                            hRRF.RequestStatus = TR_Terminate;
                            hRRF.ClientInterview = client;
                            hRRF.ModifiedBy = Convert.ToInt32(Session["EmployeeId"]);
                            hRRF.ModifiedDate = DateTime.Now;
                            hRRF.IsActive = false;
                            db.Entry(hRRF).State = EntityState.Modified;
                            db.SaveChanges();
                            //saveing data to history
                            InsertRecordsToHistroy(hRRF.HRRFNumber, hRRF.HRRFNumber + "-" + TR_Terminate, EmpIdForCreatedBy, null);

                            return RedirectToAction("Index", "trHome");
                        }
                        else if (submit == TR_Clear)
                        {
                            return RedirectToAction("ViewHRRF", "CreateHRRF", new { HRRFNumber = hRRF.HRRFNumber });
                        }
                    }
                    //Common.WriteErrorLog("User not found in Employee Table" + UName);
                    //var errors = ModelState.Select(x => x.Value.Errors)
                    //       .Where(y => y.Count > 0)
                    //       .ToList();
                    var message = string.Join(" | ", ModelState.Values
                                    .SelectMany(v => v.Errors)
                                    .Select(e => e.ErrorMessage));
                    Common.WriteErrorLog("Model state Validation errors" + message);
                    //     var errors =
                    //from value in ModelState.Values
                    //where value.ValidationState == ModelValidationState.Invalid
                    //select value;
                    //foreach (var item in errors)
                    //{
                    //    Common.WriteErrorLog("Model state Validation errors" + item.ToString());
                    //}                  
                    //  Common.WriteErrorLog("Model state" + validationModel);        
                    //  Common.WriteErrorLog("Model state Validation Failed  in Employee Table");
                    return View(ViewName, validationModel);

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

        public ActionResult ViewHRRF(string HRRFNumber = "")
        {
            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            {
                try
                {
                    var GetOpportunities = (from data in db.Opportunities.Where(opp => opp.sales_Stage.Replace("- ", "") == "Stage 6 Close Won"
           || opp.sales_Stage.Replace("- ", "") == "Stage 5 Likely Booking" || opp.sales_Stage.Replace("- ", "") == "Stage 4 Negotiate"
           || opp.sales_Stage.Replace("- ", "") == "Stage 2 Develop" || opp.sales_Stage.Replace("- ", "") == "Stage 0 Prospect"
           || opp.sales_Stage.Replace("- ", "") == "Stage 1 Engage" || opp.sales_Stage.Replace("- ", "") == "Stage 3 Solution" && opp.sales_Stage != "Closed Dropped")
                                            select new
                                            {
                                                opportunityname = data.potentialname,
                                                opportunitycode = data.potential_no
                                            }).OrderBy(x => x.opportunityname).ToList();


                    ViewData["_opportunities"] = GetOpportunities;

                    var viewResult = new ValidationModel();
                    CommonActionForGetAllFieldsData();
                    // ValidationModel result = new ValidationModel();
                    //HRRF HRRFDetails = (from data in db.HRRFs.Where(x => x.HRRFNumber == HRRFNumber) select data).FirstOrDefault();
                    var HRRFDetails = (from data in db.HRRFs.Where(x => x.HRRFNumber == HRRFNumber)
                                       select new HRRFData
                                       {
                                           HRRFID = data.HRRFID,
                                           LocationType = data.LocationType,
                                           Grade = data.Grade,
                                           RequestStatus = data.RequestStatus,
                                           Practice = data.Practice.Trim(),
                                           Purpose = data.Purpose,
                                           CostCenter = data.CostCenter,
                                           RoleRequired = data.RoleRequired,
                                           ClientInterview = data.ClientInterview,
                                           HRRFCreatedBy = data.HRRFCreatedBy,
                                           HRRFCreatedDate = data.HRRFCreatedDate,
                                           TRParent = data.TRParent,
                                           IsActive = data.IsActive,
                                           Isparent = data.Isparent,
                                           RequestType = data.RequestType,
                                           HRRFNumber = data.HRRFNumber,
                                           OpportunityName = data.OpportunityName,
                                           OpportunityCode = data.OpportunityCode,
                                           ProjectName = data.ProjectName,
                                           ProjectCode = data.ProjectCode,
                                           EnagagementType = data.EnagagementType,
                                           DemandType = data.DemandType,
                                           ExpFrom = data.ExpFrom,
                                           ExpTo = data.ExpTo,
                                           IsContracting = data.IsContracting,
                                           ReplacementEmpId = data.ReplacementEmpId,
                                           RequestReason = data.RequestReason,
                                           AssignmentStartDate = data.AssignmentStartDate,
                                           AssignmentEndDate = data.AssignmentEndDate,
                                           BillingDate = data.BillingDate,
                                           InternalExpectedFulfilmentDate = data.InternalExpectedFulfilmentDate,
                                           Positions = data.Positions,
                                           LocationName = data.LocationName,
                                           VisaType = data.VisaType,
                                           Domain = data.Domain,
                                           CLSTRJD = data.CLSTRJD,
                                           JobDescription = data.JobDescription,
                                           TECHPANEL = data.TECHPANEL,
                                           SECONDTECHPANEL = data.SECONDTECHPANEL,
                                           AccountName = data.AccountName,
                                           Impact = data.Impact,
                                           Criticality = data.Criticality,
                                           ResourceType = data.ResourceType,
                                           Utilization = data.Utilization,
                                           OpportunitySalestage = data.OpportunitySalestage,
                                           RequisitionID = data.RequisitionID,
                                           Discipline = data.Discipline,
                                           RoleGroup = data.RoleGroup,
                                           CoreSkill = data.CoreSkill,
                                           IntermediateSkill = data.IntermediateSkill,
                                           AdvancedSkill = data.AdvancedSkill,
                                           SpecificPlatForm = data.SpecificPlatform,
                                           Certifications = data.Certifications,
                                           IsIndustryDomainSkill = data.IsIndustryDomainSkill,
                                           IsSpecialClient = data.IsSpecialClient,
                                           SpecialClient = data.SpecialClient,
                                           IndustryDomainSkill = data.IndustryDomainSkill,
                                           ShiftTime = data.ShiftTime,
                                           SkillCluster = data.SkillCluster,
                                           BillRate = data.BILLRATE,
                                           MAXSAL = data.MAXSAL,
                                           CSkill = data.CSKILL,
                                           SkillCode = data.SkillCode,
                                           ClusterDesc = data.CLSTRDESC,
                                           PrimaryPractice = data.PrimaryPractice,
                                           JDNotMatching = data.JDNotMatching,
                                           OrganizationGroup = data.OrganizationGroup,
                                       }).Distinct().ToList();
                    //result.HRRFData = HRRFDetails;



                    var HRRFEnterprise = (from skills in db.HRRFSkills_ExpertiseLevel
                                          join skilMas in db.SkillMasters on skills.SkillId equals skilMas.SkillId
                                          join enterprise in db.ExpertiseLevels on skills.ExpertiseLevel equals enterprise.LevelName
                                          where skills.HRRFNumber == HRRFNumber
                                          select new SkillData
                                          {
                                              HRRFSkillId = skills.HrrfSkillid,
                                              SkillId = skills.SkillId,
                                              IsMandatory = skills.IsMandatoy,
                                              ExpertiseLevel = enterprise.LevelName,
                                              Competency = skilMas.SkillCategory,
                                              SkillName = skilMas.Skillset

                                          }).ToList();
                    //if(HRRFEnterprise == null)
                    //{
                    //  var TRSkill =  db.HRRFs.Where(x => x.HRRFNumber == HRRFNumber).FirstOrDefault();
                    //  TRSkill.Skil
                    //}
                    //result.HRRFExpertiseLevel = HRRFEnterprise;
                    // HRRFDetails.ProjectName = HRRFDetails.ProjectName + " " + HRRFDetails.ProjectCode;
                    int LocationType = Convert.ToInt32(HRRFDetails.First().LocationType);
                    int Grade = Convert.ToInt32(HRRFDetails.First().Grade);
                    TempData["RequestType"] = HRRFDetails.First().RequestStatus;
                    //TempData["CostCenter"] = HRRFDetails.CostCenter;
                    //TempData["ReplacementEmpId"] = HRRFDetails.ReplacementEmpId;
                    GetOppertunityCode(HRRFDetails.First().OpportunityName);
                    GetDisciplines(HRRFDetails.First().Practice);
                    GetRoleGroup(Grade, HRRFDetails.First().Practice);
                    GetSkillCluster(HRRFDetails.First().Practice);
                    GetCSkill(HRRFDetails.First().Practice, HRRFDetails.First().SkillCluster);
                    GetCoreSkills(Grade, HRRFDetails.First().RoleGroup, HRRFDetails.First().Practice);

                    GetAdvancedSkills(Grade, HRRFDetails.First().RoleGroup, HRRFDetails.First().Practice);
                    GetintermediateSkills(Grade, HRRFDetails.First().RoleGroup, HRRFDetails.First().Practice);

                    GetSpecificPlatForms();
                    GetCertifications();
                    GetLocations(LocationType);
                    GetDesignations(HRRFDetails.First().Practice, Grade, HRRFDetails.First().CostCenter);
                    GetExperience(HRRFDetails.First().Grade.ToString());
                    GetRequestReason(HRRFDetails.First().Purpose);
                    GetCostcenter(HRRFDetails.First().Practice);
                    GetAccount_SGA(HRRFDetails.First().CostCenter, HRRFDetails.First().Practice, HRRFDetails.First().Purpose);
                    GetProjectSFilterAccount(HRRFDetails.First().AccountName, HRRFDetails.First().Practice, HRRFDetails.First().CostCenter, HRRFDetails.First().Purpose);
                    GetPractices(HRRFDetails.First().Purpose);
                    var hrrfdetails = HRRFDetails;
                    var Expdetails = HRRFEnterprise;
                    viewResult = new ValidationModel()
                    {
                        HRRFData = hrrfdetails.ToList(),
                        HRRFExpertiseLevel = Expdetails.ToList()

                    };
                    if (HRRFDetails[0].OrganizationGroup != null)
                    {
                        string Status = HRRFDetails[0].OrganizationGroup;
                        List<SelectListItem> OrgGroups = new List<SelectListItem>();
                        //lstBillingStatus.Add(new SelectListItem { Value = "All", Text = "All" });
                        OrgGroups.Add(new SelectListItem { Value = "Concierto", Text = "Concierto" });
                        OrgGroups.Add(new SelectListItem { Value = "Digital Platforms", Text = "Digital Platforms" });
                        OrgGroups.Add(new SelectListItem { Value = "GTS - Services", Text = "GTS - Services" });
                        OrgGroups.Add(new SelectListItem { Value = "Others", Text = "Others" });
                        ViewData["_OrgGroups"] = new SelectList(OrgGroups, "Value", "Text", Status);
                    }
                    else
                    {
                        string Status = "Select";
                        List<SelectListItem> OrgGroups = new List<SelectListItem>();
                        //lstBillingStatus.Add(new SelectListItem { Value = "All", Text = "All" });
                        OrgGroups.Add(new SelectListItem { Value = "Concierto", Text = "Concierto" });
                        OrgGroups.Add(new SelectListItem { Value = "Digital Platforms", Text = "Digital Platforms" });
                        OrgGroups.Add(new SelectListItem { Value = "GTS - Services", Text = "GTS - Services" });
                        OrgGroups.Add(new SelectListItem { Value = "Others", Text = "Others" });
                        ViewData["_OrgGroups"] = new SelectList(OrgGroups, "Value", "Text", Status);
                    }
                
                    return View("ViewHRRF", viewResult);

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

        private void GetOppertunityCode(string potentialName)
        {
            var GetOpportunities = (from data in db.Opportunities.Where(opp => opp.sales_Stage.Replace("- ", "") == "Stage 6 Close Won"
       || opp.sales_Stage.Replace("- ", "") == "Stage 5 Likely Booking" || opp.sales_Stage.Replace("- ", "") == "Stage 4 Negotiate"
       || opp.sales_Stage.Replace("- ", "") == "Stage 2 Develop" || opp.sales_Stage.Replace("- ", "") == "Stage 0 Prospect"
       || opp.sales_Stage.Replace("- ", "") == "Stage 1 Engage" || opp.sales_Stage.Replace("- ", "") == "Stage 3 Solution" && opp.sales_Stage != "Closed Dropped")
                                    select new
                                    {
                                        opportunityname = data.potentialname,
                                        opportunitycode = data.potential_no
                                    }).Where(x=>x.opportunityname== potentialName).OrderBy(x => x.opportunityname).ToList();


            ViewData["_opportunitiesCode"] = GetOpportunities;
        }

        public ValidationModel CommonActionForGetAllFieldsData()
        {
            try
            {
                var Cancel_ReasonList = from cancel in db.TR_CancelReason orderby cancel.CancelReasonName select cancel.CancelReasonName;
                ViewData["_Cancel_ReasonList"] = Cancel_ReasonList;
                List<string> vistalist = new List<string>() { "None", "B1", "H-1B", "L-1A", "L-1B" };
                ViewData["_VisaTypesList"] = vistalist;
                ValidationModel model = new ValidationModel();
                var MasterLookUps = AgentService.GetLookupdata();
                List<MasterLookUpNew> objMasterLookups = new List<Models.MasterLookUpNew>();
                List<string> visalist = new List<string>() { "None", "B1", "H-1B", "L-1A", "L-1B" };
                
                ViewData["_VisaTypesList"] = visalist;


                //var OrgGroups = new List<string>() { "Concierto", "Digital Platforms", "GTS - Services" };
                //ViewData["_OrgGroups"] = OrgGroups;

                string Status = "Select";
                List<SelectListItem> OrgGroups = new List<SelectListItem>();
                //lstBillingStatus.Add(new SelectListItem { Value = "All", Text = "All" });
                OrgGroups.Add(new SelectListItem { Value = "Concierto", Text = "Concierto" });
                OrgGroups.Add(new SelectListItem { Value = "Digital Platforms", Text = "Digital Platforms" });
                OrgGroups.Add(new SelectListItem { Value = "GTS - Services", Text = "GTS - Services" });
                OrgGroups.Add(new SelectListItem { Value = "Others", Text = "Others" });
                ViewData["_OrgGroups"] = new SelectList(OrgGroups, "Value", "Text", Status);

                if (MasterLookUps != null)
                {
                    var jsonObj = new JavaScriptSerializer().Deserialize<MasterLookUpNew[]>(MasterLookUps);
                    foreach (var obj in jsonObj)
                    {
                        MasterLookUpNew objnot = new Models.MasterLookUpNew();

                        objnot.Active = obj.Active;
                        objnot.ApplicationCode = obj.ApplicationCode;
                        objnot.CreatedBy = obj.CreatedBy;
                        objnot.DateCreated = obj.DateCreated;
                        objnot.DateModified = obj.DateModified;
                        objnot.Description = obj.Description;
                        objnot.Field1 = obj.Field1;
                        objnot.Field2 = obj.Field2;
                        objnot.Field3 = obj.Field3;
                        objnot.LookupCode = obj.LookupCode;
                        objnot.LookupID = obj.LookupID;
                        objnot.LookupName = obj.LookupName;
                        objnot.LookupType = obj.LookupType;
                        objnot.ParentCode = obj.ParentCode;
                        objnot.ParentName = obj.ParentName;
                        objnot.LookupType = obj.LookupType;
                        objnot.SeqNumber = obj.SeqNumber;
                        objMasterLookups.Add(objnot);
                    }
                }
                else
                {
                    foreach (var obj in db.MasterLookUps)
                    {
                        MasterLookUpNew objnot = new Models.MasterLookUpNew();

                        objnot.Active = obj.Active;
                        objnot.ApplicationCode = obj.ApplicationCode;
                        objnot.CreatedBy = obj.CreatedBy;
                        objnot.DateCreated = obj.DateCreated;
                        objnot.DateModified = obj.DateModified;
                        objnot.Description = obj.Description;
                        objnot.Field1 = obj.Field1;
                        objnot.Field2 = obj.Field2;
                        objnot.Field3 = obj.Field3;
                        objnot.LookupCode = obj.LookupCode;
                        objnot.LookupID = obj.LookupID;
                        objnot.LookupName = obj.LookupName;
                        objnot.LookupType = obj.LookupType;
                        objnot.ParentCode = obj.ParentCode;
                        objnot.ParentName = obj.ParentName;
                        objnot.LookupType = obj.LookupType;
                        objnot.SeqNumber = obj.SeqNumber;
                        objMasterLookups.Add(objnot);
                    }
                }

                // List<MasterLookUp> MasterLookUps = db.MasterLookUps.ToList();
                var Purpose = (from purpose in objMasterLookups.Where(x => x.LookupType == "Purpose")
                               select new
                               {
                                   purposeName = purpose.LookupName,
                                   Description = purpose.Description,
                                   LookupCode = purpose.LookupCode,
                                   SeqNumber = purpose.SeqNumber
                               }).ToList().OrderBy(p => p.purposeName);

                ViewData["_Purpose"] = Purpose;

				//var Projects = (from project in db.Projects
				//                where project.IsActive == true
				//                select new
				//                {
				//                    ProjectCode = project.ProjectName,
				//                    ProjectName = project.ProjectName + " " + project.ProjectCode,
				//                    Code = project.ProjectCode,
				//                    ProjectId = project.ProjectId,
				//                }).ToList().OrderBy(p => p.ProjectName);
				//ViewData["_Projects"] = Projects;

				//var AccountName = (from project in db.Projects orderby project.AccountName select project.AccountName).Distinct();
				//ViewData["_AccountName"] = AccountName;

				//List<string> CostCenter = new List<string>() { "Admin", "BDM", "CEO", "Delivery", "Finance", "HR", "IT", "Marketing", "Quality", "R&KM", "TCO", "Testing" };
				//ViewData["_CostCenter"] = CostCenter.OrderBy(ord => ord).ToList();

				var GetOpportunities = (from data in db.Opportunities.Where(opp => opp.sales_Stage.Replace("- ", "") == "Stage 6 Close Won"
		   || opp.sales_Stage.Replace("- ", "") == "Stage 5 Likely Booking" || opp.sales_Stage.Replace("- ", "") == "Stage 4 Negotiate"
		   || opp.sales_Stage.Replace("- ", "") == "Stage 2 Develop" || opp.sales_Stage.Replace("- ", "") == "Stage 0 Prospect"
		   || opp.sales_Stage.Replace("- ", "") == "Stage 1 Engage" || opp.sales_Stage.Replace("- ", "") == "Stage 3 Solution" && opp.sales_Stage != "Closed Dropped")
										select new
										{
											opportunityname = data.potentialname,
											opportunitycode = data.potential_no
										}).OrderBy(x=>x.opportunityname).ToList();


				ViewData["_opportunities"] = GetOpportunities;
                var Impact = (from critic in objMasterLookups.Where(x => x.LookupType == "Criticality")
                                   select new
                                   {
                                       LookupName = critic.LookupName,
                                       Description = critic.Description,
                                       LookupCode = critic.LookupCode,
                                       SeqNumber = critic.SeqNumber
                                   }).ToList().OrderBy(p => p.LookupName);
                ViewData["_Impact"] = Impact;
                

                var ExpertiseLevel = (from explevel in db.ExpertiseLevels
                                      select new
                                      {
                                          LookupName = explevel.LevelName,
                                          Description = explevel.LevelName,
                                          LookupCode = explevel.ExpertiseId
                                      }).ToList().OrderByDescending(p => p.LookupCode);
                ViewData["_ExpertiseLevel"] = ExpertiseLevel;

                List<string> Utilization = new List<string>() { "25", "50", "75", "100" };
                ViewData["_Utilization"] = Utilization;

                var Grade = from grade in objMasterLookups.Where(x => x.LookupType == "Grade")
                            select new
                            {
                                LookupName = grade.LookupName,
                                Description = grade.Description,
                                LookupCode = grade.LookupCode,
                                SeqNumber = grade.SeqNumber
                            };
                ViewData["_Grade"] = Grade;
                var ReservationStatus = from reservationStatus in objMasterLookups.Where(x => x.LookupType == "ReservationStatus")
                                        select new
                                        {
                                            LookupName = reservationStatus.LookupName,
                                            Description = reservationStatus.Description,
                                            LookupCode = reservationStatus.LookupCode,
                                            SeqNumber = reservationStatus.SeqNumber
                                        };
                ViewData["_ReservationStatus"] = ReservationStatus;


                var EngagementType = (from engagementType in objMasterLookups.Where(x => x.LookupType == "EngagementType")
                                      select new
                                      {
                                          LookupName = engagementType.LookupName,
                                          Description = engagementType.Description,
                                          LookupCode = engagementType.LookupCode,
                                          SeqNumber = engagementType.SeqNumber
                                      }).ToList().OrderBy(p => p.LookupName);
                ViewData["_EngagementType"] = EngagementType;
                var SkillCategory = from skillCategory in objMasterLookups.Where(x => x.LookupType == "SkillCategory").OrderBy(o => o.LookupName)
                                    select new
                                    {
                                        LookupName = skillCategory.LookupName,
                                        Description = skillCategory.Description,
                                        LookupCode = skillCategory.LookupCode,
                                        SeqNumber = skillCategory.SeqNumber
                                    };
                ViewData["_SkillCategory"] = SkillCategory;

                var LocationType = (from locationType in objMasterLookups.Where(x => x.LookupType == "LocationType")
                                    select new
                                    {
                                        LookupName = locationType.LookupName,
                                        Description = locationType.Description,
                                        LookupCode = locationType.LookupCode,
                                        SeqNumber = locationType.SeqNumber
                                    }).ToList().OrderBy(p => p.LookupName);
                ViewData["_LocationType"] = LocationType;

                var shiftDetails = (from shift in db.ShiftMasters
                             select new
                             {
                                 ShiftValue=shift.ShiftValue,
                                 ShiftName=shift.ShiftName
                             });
                ViewData["_ShiftDetails"] = shiftDetails;
                var DemandType = (from demandType in objMasterLookups.Where(x => x.LookupType == "DemandType")
                                  select new
                                  {
                                      LookupName = demandType.LookupName,
                                      Description = demandType.Description,
                                      LookupCode = demandType.LookupCode,
                                      SeqNumber = demandType.SeqNumber
                                  }).ToList().OrderBy(d => d.LookupName);
                ViewData["_DemandType"] = DemandType;
                var Domain = (from domain in objMasterLookups.Where(x => x.LookupType == "Domain")
                              select new
                              {
                                  LookupName = domain.LookupName,
                                  Description = domain.Description,
                                  LookupCode = domain.LookupCode,
                                  SeqNumber = domain.SeqNumber
                              }).ToList().OrderBy(p => p.LookupName);
                ViewData["_Domain"] = Domain;
                var distinctResult = from c in db.Employees
                                     group c by c.Practice into uniqueIds
                                     select uniqueIds.FirstOrDefault();

                //var Practice = (from data in db.PracticeWiseBenchCodes.Where(x => x.Practice.ToLower() != "sg&a")
                //                select new
                //                {
                //                    LookupName = data.Practice
                //                }).Distinct().ToList().OrderBy(p => p.LookupName);


                //ViewData["_Practice"] = Practice;
                return model;
            }
            //catch (Exception ex)
            //{

            //    Common.WriteExceptionErrorLog(ex);
            //    return RedirectToAction("Error", "Error");
            //}
            catch (Exception ex)
            {
                ErrorHandling expcls = new ErrorHandling();
                expcls.Error(ex);
                return null;
            }

        }

        public ActionResult HRRFCreation()
        {
            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            {
                try
                {
                    var GetOpportunities = (from data in db.Opportunities.Where(opp => opp.sales_Stage.Replace("- ", "") == "Stage 6 Close Won"
          || opp.sales_Stage.Replace("- ", "") == "Stage 5 Likely Booking" || opp.sales_Stage.Replace("- ", "") == "Stage 4 Negotiate"
          || opp.sales_Stage.Replace("- ", "") == "Stage 2 Develop" || opp.sales_Stage.Replace("- ", "") == "Stage 0 Prospect"
          || opp.sales_Stage.Replace("- ", "") == "Stage 1 Engage" || opp.sales_Stage.Replace("- ", "") == "Stage 3 Solution" && opp.sales_Stage != "Closed Dropped")
                                            select new
                                            {
                                                opportunityname = data.potentialname,
                                                opportunitycode = data.potential_no
                                            }).OrderBy(x => x.opportunityname).ToList();


                    ViewData["_opportunities"] = GetOpportunities;
                    CommonActionForGetAllFieldsData();
                    return View("HRRFCreation");
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
        public ActionResult HRRFCreation(string submit)
        {
            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            {
                try
                {

                    string CountHrrf = "";
                    int positions = 0;
                    string HRRFNumber = string.Empty;
                    string trParentNum = string.Empty;
                    bool isParent = true;
                    string clientIntrview = Request.Form["Clientint"];
                    if (!string.IsNullOrEmpty(clientIntrview)) { } else { clientIntrview = "No"; }
                    var HRRF = new HRRF();
                    string ProjectCode = Request.Form["Projects"];

                  //  string OrgGroup = Request.Form["OrgGroup"];

                    string RequestReason = Request.Form["RequestReason"];
                    string hdnRequestReason = Request.Form["hdnRequestReason"];

                    int EmpIdForCreatedBy = Convert.ToInt32(Session["EmployeeId"]);

                    if (Convert.ToInt32(Session["EmployeeId"]) > 0)
                    {
                        string PrimarySkills = Request.Form["hiddenPrimarySkills"]; string SecondarySkills = Request.Form["hiddenSecondarySkills"];

                        string ProjectName = (from data in db.Projects.Where(x => x.ProjectCode.ToLower() == ProjectCode.ToLower()) select data.ProjectName).FirstOrDefault();

                        if (submit == TR_Submit)
                        {
                            string SubjectName = "Submitted";
                            positions = Convert.ToInt32(Request.Form["NoofPositions"]);
                            for (int countPostion = 1; countPostion <= positions; countPostion++)
                            {
                                string CompanyCode = TR_CompanyCode;


                                //string NumberingSystem = AgentService.NumberingSystem(CompanyCode, "", "");
                                //if (!string.IsNullOrEmpty(NumberingSystem))
                                //{
                                //    string[] SimplyNumber = NumberingSystem.Split(new[] { ":" }, StringSplitOptions.None);
                                //    string output = SimplyNumber[1].Replace('{', '/').Replace('}', ' ').Replace('"', ' ');
                                //    HRRFNumber = output.Trim();
                                //}


                                //For Testing Environment
                                // new code
                                string branchCde = "";
                                string sequenceSffix = "";
                                long sequenceNmber = 0;
                                string typeCde = "";
                                ObjectParameter Result1 = new ObjectParameter("Result1", typeof(string));
                                string datastoredproc = db.sp_NumberingSystemGenerateNumber(CompanyCode, branchCde, sequenceSffix, sequenceNmber, typeCde, Result1).ToString();
                                string NumberingSystem = Result1.Value.ToString();
                                HRRFNumber = NumberingSystem;
                                //new code
                                //For Testing Environment



                                long HRRFid = (from data in db.HRRFs.Where(x => x.HRRFNumber == HRRFNumber)
                                               select data.HRRFID).FirstOrDefault();
                                if (countPostion == 1)
                                {
                                    trParentNum = HRRFNumber;
                                    isParent = true;

                                    if (Request.Form["OffshoreLocation"].ToLower() == "other")
                                    {
                                        var result = (from val in db.MasterLookUps
                                                      where val.LookupType.ToLower() == "onsitelocation"
                                                      select val).Count();

                                        MasterLookUp objMasterLookup = new MasterLookUp();
                                        objMasterLookup.ApplicationCode = "TALENTREQ";
                                        objMasterLookup.LookupCode = Request.Form["OtherLocation"].ToString();
                                        objMasterLookup.LookupType = "OnSiteLocation";
                                        objMasterLookup.LookupName = Request.Form["OtherLocation"].ToString();
                                        objMasterLookup.Description = Request.Form["OtherLocation"].ToString();
                                        objMasterLookup.SeqNumber = result + 1;
                                        objMasterLookup.ParentCode = "1";
                                        objMasterLookup.ParentName = "LocationType";
                                        objMasterLookup.Active = true;
                                        objMasterLookup.DateCreated = DateTime.Now;
                                        objMasterLookup.DateModified = DateTime.Now;

                                        db.MasterLookUps.Add(objMasterLookup);
                                    }
                                }
                                else
                                {
                                    isParent = false;
                                }

                                if (HRRFid == 0)
                                {
                                    //project info data
                                    HRRF.HRRFNumber = HRRFNumber.Trim();
                                    HRRF.TRParent = trParentNum;
                                    HRRF.Purpose = Request.Form["Purpose"];
                                    HRRF.AccountName = Request.Form["AccountName"];
                                    HRRF.CostCenter = Request.Form["CostCenter"];
                                    HRRF.Impact = Request.Form["Impact"];
                                    HRRF.Criticality = Request.Form["Criticality"];
                                    HRRF.Utilization = Request.Form["Utilization"];
                                    HRRF.IsContracting = Convert.ToBoolean(Request.Form["IsContracting"]); //(Request.Form["IsContracting"].ToLower() == "true") ? true : false;
                                    HRRF.ResourceType = Request.Form["ResourceType"];
                                    HRRF.RequestReason = Request.Form["RequestReason"];
                                    if (!string.IsNullOrWhiteSpace(Request.Form["ReplacementEmpId"]))
                                    {
                                        HRRF.ReplacementEmpId = Convert.ToInt32(Request.Form["ReplacementEmpId"]);
                                    }
                                    HRRF.ProjectName = ProjectName;
                                    HRRF.ProjectCode = ProjectCode;
                                    //if (HRRF.Purpose == "Opportunity")
                                    //{
                                    //    HRRF.ProjectName = "";
                                    //    HRRF.ProjectCode = "";
                                    //}
                                    HRRF.OpportunityName = Request.Form["txtOpportunity"];
                                    HRRF.OpportunityCode = Request.Form["txtOpportunityCode"];
                                    var opps = db.Opportunities.Where(x => x.potential_no == HRRF.OpportunityCode).FirstOrDefault();
                                    if (opps != null)
                                    {
                                        HRRF.OpportunitySalestage = opps.sales_Stage;
                                    }
                                    // HRRF.DemandType = Request.Form["DemandType"];
                                    HRRF.Grade = Convert.ToInt32(Request.Form["Grade"]);
                                    string getrole = Request.Form["Role"];

                                    HRRF.RoleRequired = Request.Form["Role"];

                                    if (Request.Form["Role"].ToLower() == "other")
                                    {
                                        HRRF.RoleRequired = Request.Form["OtherRole"];
                                    }

                                    HRRF.ExpFrom = Request.Form["ExpFrom"];
                                    // HRRF.ExpTo = Request.Form["ExpTo"];

                                    string strEngagementType = "";
                                    //switch (Request.Form["EngagementType"].ToString().ToLower().Trim())
                                    //{
                                    //    case "co_man":
                                    //        strEngagementType = "Co-Managed";
                                    //        break;
                                    //    case "consult":
                                    //        strEngagementType = "Consulting";
                                    //        break;
                                    //    case "mantrz":
                                    //        strEngagementType = "Managed by Trianz";
                                    //        break;
                                    //    case "engag":
                                    //        strEngagementType = "Staff Augmentation";
                                    //        break;
                                    //    default:
                                    //        strEngagementType = Request.Form["EngagementType"].Trim();
                                    //        break;
                                    //}
                                    HRRF.EnagagementType = strEngagementType; //Request.Form["EngagementType"];
                                    HRRF.Positions = 1;
                                    HRRF.Practice = Request.Form["Practice"];
                                    //skill set tab data
                                    HRRF.Domain = Request.Form["Domain"];

                                    HRRF.TECHPANEL = Request.Form["TechPanel"];
                                    HRRF.SECONDTECHPANEL = Request.Form["TechPanel2"];
                                    //timeline tab data
                                    IFormatProvider theCultureInfo = new System.Globalization.CultureInfo("en-us", true);
                                    HRRF.AssignmentStartDate = string.IsNullOrEmpty(Request.Form["ExpectedStart"].ToString()) ? (DateTime?)null : DateTime.Parse(Request.Form["ExpectedStart"].ToString(), theCultureInfo);
                                    HRRF.AssignmentEndDate = string.IsNullOrEmpty(Request.Form["ExpectedEnd"].ToString()) ? (DateTime?)null : DateTime.Parse(Request.Form["ExpectedEnd"].ToString(), theCultureInfo);
                                    HRRF.BillingDate = string.IsNullOrEmpty(Request.Form["BillingDate"].ToString()) ? (DateTime?)null : DateTime.Parse(Request.Form["BillingDate"].ToString(), theCultureInfo);
                                    HRRF.LocationType = Request.Form["LocationType"];
                                    HRRF.LocationName = Request.Form["OffshoreLocation"];

                                    if (Request.Form["OffshoreLocation"].ToLower() == "other")
                                    {
                                        HRRF.LocationName = Request.Form["OtherLocation"];
                                    }

                                    HRRF.VisaType = Request.Form["ddlVisaTypes"];
                                    HRRF.RequisitionID = Request.Form["RequisitionID"];
                                    HRRF.ClientInterview = clientIntrview;
                                    HRRF.RequestStatus = TR_Submitted;
                                    HRRF.RequestType = TR_Internal;
                                    HRRF.HRRFCreatedBy = EmpIdForCreatedBy;
                                    HRRF.Isparent = isParent;
                                    HRRF.IsActive = true;
                                    HRRF.HRRFCreatedDate = DateTime.Now;
                                    HRRF.InternalExpectedFulfilmentDate = DateTime.Now.AddDays(5);
                                    HRRF.HRRFSubmitedDate = DateTime.Now;

                                    HRRF.PrimaryPractice = Request.Form["hdnPrimaryPractice"];
                                    HRRF.JDNotMatching = Request.Form["JDNotMatching"] != null ? Request.Form["JDNotMatching"] : "No";
                                    HRRF.Discipline = Request.Form["Discipline"];
                                    HRRF.RoleGroup = Request.Form["RoleGroup"];
                                    //HRRF.CoreSkill = Request.Form["CoreSkill"];
                                    // HRRF.IntermediateSkill = Request.Form["IntermediateSkill"];
                                    // HRRF.AdvancedSkill = Request.Form["AdvancedSkill"];
                                    HRRF.SpecificPlatform = Request.Form["SpecificPlatform"];
                                    HRRF.Certifications = Request.Form["Certifications"];
                                    HRRF.SpecialClient = Request.Form["SpecialClient"];
                                    HRRF.IndustryDomainSkill = Request.Form["IndustryDomainSkill"];
                                    HRRF.IsSpecialClient = Convert.ToBoolean(Request.Form["IsSpecialClient"]);
                                    HRRF.IsIndustryDomainSkill = Convert.ToBoolean(Request.Form["IsIndustryDomainSkill"]);
                                    HRRF.ShiftTime = Request.Form["ShiftTime"];
                                    HRRF.JobDescription = Request.Form["JobDescription"];
                                    HRRF.SkillCluster = Request.Form["SkillCluster"];
                                    HRRF.BILLRATE = string.IsNullOrEmpty(Request.Form["BillRate"].ToString()) ? (Decimal?)null : Convert.ToDecimal(Request.Form["BillRate"]);
                                    HRRF.MAXSAL = string.IsNullOrEmpty(Request.Form["MAXSAL"].ToString()) ? (Double?)null : Convert.ToDouble(Request.Form["MAXSAL"]);
                                    HRRF.CSKILL = Request.Form["CSkill"];
                                    var skillCode = db.PRTCSKILLCLUSTERs.Where(x => x.PRACTICE == HRRF.Practice && x.SKILLCLUSTER == HRRF.SkillCluster && x.CSKILLS == HRRF.CSKILL).FirstOrDefault();
                                    if (skillCode != null)
                                    {
                                        HRRF.SkillCode = skillCode.SKILLCODE;
                                        HRRF.CLSTRDESC = db.SKILLCLSTRJDs.Where(x => x.SKILLCODE == skillCode.SKILLCODE).Select(x => x.CLSTRDESC).FirstOrDefault();
                                    }

                                    HRRF.CLSTRJD = Request.Form["CLSTRJD"];
                                    HRRF.ExternalWebSite = "No";
                                    if (HRRF.CLSTRJD == string.Empty || HRRF.CLSTRJD == null)
                                    {
                                        HRRF.CLSTRJD = HRRF.SkillCluster + " " + HRRF.SkillCode;
                                    }
                                    HRRF.OrganizationGroup = Request.Form["OrganizationGroup"];
                                    db.HRRFs.Add(HRRF);
                                    db.SaveChanges();

                                    //string HRRFList = AgentService.CreateHRRF(HRRF);
                                    //inserting into history data
                                    InsertRecordsToHistroy(HRRFNumber, HRRFNumber + "-" + TR_Submitted, EmpIdForCreatedBy, null);
                                    SaveHRRFSkills(PrimarySkills, HRRFNumber);
                                    //   SubmitHRRFSkills(PrimarySkills, SecondarySkills, HRRFNumber);
                                    //get OMs based on practice selected in herrf or tr
                                    InsertRecordsToNotifications(HRRFNumber, SubjectName, "");

                                }

                                CountHrrf = CountHrrf + HRRFNumber + ",";
                            }
                            if (CountHrrf != null)
                            {
                                TempData["HRRFNumber"] = CountHrrf;

                                TempData["Message"] = " has been created successfully";
                                //   TempData.Keep("hrrfno");
                                return RedirectToAction("Index", "trHome");
                            }
                        }
                        else if (submit == TR_Save)
                        {

                            positions = Convert.ToInt32(Request.Form["NoofPositions"]);
                            for (int countPostion = 1; countPostion <= positions; countPostion++)
                            {
                                string CompanyCode = TR_CompanyCode;

                                //string NumberingSystem = AgentService.NumberingSystem(CompanyCode, "", "");
                                //if (!string.IsNullOrEmpty(NumberingSystem))
                                //{
                                //    string[] SimplyNumber = NumberingSystem.Split(new[] { ":" }, StringSplitOptions.None);
                                //    string output = SimplyNumber[1].Replace('{', '/').Replace('}', ' ').Replace('"', ' ');
                                //    HRRFNumber = output.Trim();
                                //}

                                ////For Testing Environment
                                //// new code
                                string branchCde = "";
                                string sequenceSffix = "";
                                long sequenceNmber = 0;
                                string typeCde = "";
                                ObjectParameter Result1 = new ObjectParameter("Result1", typeof(string));
                                string datastoredproc = db.sp_NumberingSystemGenerateNumber(CompanyCode, branchCde, sequenceSffix, sequenceNmber, typeCde, Result1).ToString();
                                string NumberingSystem = Result1.Value.ToString();
                                HRRFNumber = NumberingSystem;
                                ////new code
                                ////For Testing Environment

                                long HRRFid = (from data in db.HRRFs.Where(x => x.HRRFNumber == HRRFNumber)
                                               select data.HRRFID).FirstOrDefault();
                                if (countPostion == 1)
                                {
                                    trParentNum = HRRFNumber;
                                    isParent = true;

                                    if (Request.Form["OffshoreLocation"].ToLower() == "other")
                                    {
                                        var result = (from val in db.MasterLookUps
                                                      where val.LookupType.ToLower() == "onsitelocation"
                                                      select val).Count();

                                        MasterLookUp objMasterLookup = new MasterLookUp();
                                        objMasterLookup.ApplicationCode = "TALENTREQ";
                                        objMasterLookup.LookupCode = Request.Form["OtherLocation"].ToString();
                                        objMasterLookup.LookupType = "OnSiteLocation";
                                        objMasterLookup.LookupName = Request.Form["OtherLocation"].ToString();
                                        objMasterLookup.Description = Request.Form["OtherLocation"].ToString();
                                        objMasterLookup.SeqNumber = result + 1;
                                        objMasterLookup.ParentCode = "1";
                                        objMasterLookup.ParentName = "LocationType";
                                        objMasterLookup.Active = true;
                                        objMasterLookup.DateCreated = DateTime.Now;
                                        objMasterLookup.DateModified = DateTime.Now;

                                        db.MasterLookUps.Add(objMasterLookup);
                                    }
                                }
                                else
                                {
                                    isParent = false;
                                }

                                if (HRRFid == 0)
                                {
                                    //project info data
                                    HRRF.HRRFNumber = HRRFNumber.Trim();
                                    HRRF.TRParent = trParentNum;
                                    HRRF.Purpose = Request.Form["Purpose"];
                                    HRRF.AccountName = Request.Form["AccountName"];
                                    HRRF.CostCenter = Request.Form["CostCenter"];
                                    HRRF.Impact = Request.Form["Impact"];
                                    HRRF.Criticality = Request.Form["Criticality"];
                                    HRRF.Utilization = Request.Form["Utilization"];
                                    HRRF.IsContracting = Convert.ToBoolean(Request.Form["IsContracting"]); //(Request.Form["IsContracting"].ToLower() == "on") ? true : false;
                                    HRRF.ResourceType = Request.Form["ResourceType"];
                                    HRRF.ProjectName = ProjectName;
                                    HRRF.PrimaryPractice = Request.Form["hdnPrimaryPractice"];
                                    HRRF.JDNotMatching = Request.Form["JDNotMatching"] != null ? Request.Form["JDNotMatching"] : "No";
                                    HRRF.RequestReason = Request.Form["RequestReason"];
                                    if (!string.IsNullOrWhiteSpace(Request.Form["ReplacementEmpId"]))
                                    {
                                        HRRF.ReplacementEmpId = Convert.ToInt32(Request.Form["ReplacementEmpId"]);
                                    }
                                    HRRF.ProjectCode = ProjectCode;
                                    //if (HRRF.Purpose == "Opportunity")
                                    //{
                                    //    HRRF.ProjectName = "";
                                    //    HRRF.ProjectCode = "";
                                    //}
                                    HRRF.OpportunityName = Request.Form["txtOpportunity"];
                                    HRRF.OpportunityCode = Request.Form["txtOpportunityCode"];
                                    var opps = db.Opportunities.Where(x => x.potential_no == HRRF.OpportunityCode).FirstOrDefault();
                                    if (opps != null)
                                    {
                                        HRRF.OpportunitySalestage = opps.sales_Stage;
                                    }
                                    // HRRF.DemandType = Request.Form["DemandType"];
                                    HRRF.Grade = Convert.ToInt32(Request.Form["Grade"]);
                                    HRRF.RoleRequired = Request.Form["Role"];
                                    if (Request.Form["Role"].ToLower() == "other")
                                    {
                                        HRRF.RoleRequired = Request.Form["OtherRole"];
                                    }
                                    HRRF.ExpFrom = Request.Form["ExpFrom"];
                                    //   HRRF.ExpTo = Request.Form["ExpTo"];
                                    string strEngagementType = "";
                                    //switch (Request.Form["EngagementType"].ToString().ToLower().Trim())
                                    //{
                                    //    case "co_man":
                                    //        strEngagementType = "Co-Managed";
                                    //        break;
                                    //    case "consult":
                                    //        strEngagementType = "Consulting";
                                    //        break;
                                    //    case "mantrz":
                                    //        strEngagementType = "Managed by Trianz";
                                    //        break;
                                    //    case "engag":
                                    //        strEngagementType = "Staff Augmentation";
                                    //        break;
                                    //    default:
                                    //        strEngagementType = Request.Form["EngagementType"].Trim();
                                    //        break;
                                    //}
                                    HRRF.EnagagementType = strEngagementType; //Request.Form["EngagementType"];
                                    HRRF.Positions = 1;
                                    HRRF.Practice = Request.Form["Practice"];
                                    //skill set tab data
                                    HRRF.Domain = Request.Form["Domain"];

                                    HRRF.TECHPANEL = Request.Form["TechPanel"];
                                    HRRF.SECONDTECHPANEL = Request.Form["TechPanel2"];
                                    //timeline tab data
                                    IFormatProvider theCultureInfo = new System.Globalization.CultureInfo("en-us", true);
                                    HRRF.AssignmentStartDate = string.IsNullOrEmpty(Request.Form["ExpectedStart"].ToString()) ? (DateTime?)null : DateTime.Parse(Request.Form["ExpectedStart"].ToString(), theCultureInfo);
                                    HRRF.AssignmentEndDate = string.IsNullOrEmpty(Request.Form["ExpectedEnd"].ToString()) ? (DateTime?)null : DateTime.Parse(Request.Form["ExpectedEnd"].ToString(), theCultureInfo);
                                    HRRF.BillingDate = string.IsNullOrEmpty(Request.Form["BillingDate"].ToString()) ? (DateTime?)null : DateTime.Parse(Request.Form["BillingDate"].ToString(), theCultureInfo);
                                    HRRF.LocationType = Request.Form["LocationType"];
                                    HRRF.LocationName = Request.Form["OffShoreLocation"];
                                    if (Request.Form["OffshoreLocation"].ToLower() == "other")
                                    {
                                        HRRF.LocationName = Request.Form["OtherLocation"];
                                    }
                                    HRRF.VisaType = Request.Form["ddlVisaTypes"];
                                    HRRF.RequisitionID = Request.Form["RequisitionID"];
                                    HRRF.ClientInterview = clientIntrview;
                                    HRRF.RequestStatus = TR_Draft;
                                    HRRF.RequestType = TR_Internal;
                                    HRRF.HRRFCreatedBy = EmpIdForCreatedBy;
                                    HRRF.Isparent = isParent;
                                    HRRF.IsActive = true;
                                    HRRF.HRRFCreatedDate = DateTime.Now;
                                    // HRRF.HRRFSubmitedDate = DateTime.Now;

                                    HRRF.Discipline = Request.Form["Discipline"];
                                    HRRF.RoleGroup = Request.Form["RoleGroup"];
                                    //HRRF.CoreSkill = Request.Form["CoreSkill"];
                                    // HRRF.IntermediateSkill = Request.Form["IntermediateSkill"];
                                    // HRRF.AdvancedSkill = Request.Form["AdvancedSkill"];
                                    HRRF.SpecificPlatform = Request.Form["SpecificPlatform"];
                                    HRRF.Certifications = Request.Form["Certifications"];
                                    HRRF.SpecialClient = Request.Form["SpecialClient"];
                                    HRRF.IndustryDomainSkill = Request.Form["IndustryDomainSkill"];
                                    HRRF.IsSpecialClient = Convert.ToBoolean(Request.Form["IsSpecialClient"]);
                                    HRRF.IsIndustryDomainSkill = Convert.ToBoolean(Request.Form["IsIndustryDomainSkill"]);
                                    HRRF.ShiftTime = Request.Form["ShiftTime"];
                                    HRRF.JobDescription = Request.Form["JobDescription"];
                                    HRRF.SkillCluster = Request.Form["SkillCluster"];

                                    HRRF.BILLRATE = string.IsNullOrEmpty(Request.Form["BillRate"].ToString()) ? (Decimal?)null : Convert.ToDecimal(Request.Form["BillRate"]);
                                    HRRF.MAXSAL = string.IsNullOrEmpty(Request.Form["MAXSAL"].ToString()) ? (Double?)null : Convert.ToDouble(Request.Form["MAXSAL"]);
                                    HRRF.CSKILL = Request.Form["CSkill"];
                                    var skillCode = db.PRTCSKILLCLUSTERs.Where(x => x.PRACTICE == HRRF.Practice && x.SKILLCLUSTER == HRRF.SkillCluster && x.CSKILLS == HRRF.CSKILL).FirstOrDefault();
                                    if (skillCode != null)
                                    {
                                        HRRF.SkillCode = skillCode.SKILLCODE;
                                        HRRF.CLSTRDESC = db.SKILLCLSTRJDs.Where(x => x.SKILLCODE == skillCode.SKILLCODE).Select(x => x.CLSTRDESC).FirstOrDefault();
                                    }

                                    HRRF.CLSTRJD = Request.Form["CLSTRJD"]; //Request.Unvalidated["CLSTRJD"];
                                    HRRF.ExternalWebSite = "No";
                                    if (HRRF.CLSTRJD == string.Empty || HRRF.CLSTRJD == null)
                                    {
                                        HRRF.CLSTRJD = HRRF.SkillCluster + " " + HRRF.SkillCode;
                                    }
                                    HRRF.OrganizationGroup = Request.Form["OrganizationGroup"];
                                    db.HRRFs.Add(HRRF);
                                    db.SaveChanges();
                                    // string HRRFList = AgentService.CreateHRRF(HRRF);
                                    //       SubmitHRRFSkills(PrimarySkills, SecondarySkills, HRRFNumber);
                                    SaveHRRFSkills(PrimarySkills, HRRFNumber);
                                    // saving hrrf history 
                                    InsertRecordsToHistroy(HRRFNumber, HRRFNumber + "-" + TR_Draft, EmpIdForCreatedBy, null);

                                }

                                CountHrrf = CountHrrf + HRRFNumber + ",";
                            }

                            if (CountHrrf != null)
                            {
                                TempData["HRRFNumber"] = CountHrrf;
                                TempData["Message"] = " has been saved successfully";
                                // TempData.Keep("hrrfno");
                                return RedirectToAction("Index", "trHome");
                            }
                        }
                        else if (submit == TR_Clear)
                        {
                            return RedirectToAction("HRRFCreation", "CreateHRRF");
                        }
                    }

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

        public void SaveHRRFSkills(string NewSkills, string HRRFNumber)
        {
            IList<string> PrimarySkillIds = NewSkills.TrimEnd(';').Split(';');
            try
            {
                bool isMandatory;
                if (!string.IsNullOrEmpty(HRRFNumber))
                {
                    if (!string.IsNullOrEmpty(NewSkills))
                    {

                        foreach (string PSkill in PrimarySkillIds)
                        {

                            if (PSkill.Split(':')[3].ToLower() == "yes")
                                isMandatory = true;
                            else
                                isMandatory = false;
                            int SkillId = Convert.ToInt32(PSkill.Split(':')[1]);
                            //  int rating = Convert.ToInt32(PSkill.Split(':')[1]);
                            int HRRFSkillId = (from data in db.HRRFSkills_ExpertiseLevel.Where(m => m.HRRFNumber == HRRFNumber && m.SkillId == SkillId) select data.HrrfSkillid).FirstOrDefault();
                            var HRRFPrimarySkills = db.HRRFSkills_ExpertiseLevel.Find(HRRFSkillId);
                            if (HRRFPrimarySkills != null)
                            {
                                var HRRFPrimarySkill = new HRRFSkills_ExpertiseLevel();
                                HRRFPrimarySkill.HRRFNumber = HRRFNumber;

                                HRRFPrimarySkill.SkillId = Convert.ToInt32(PSkill.Split(':')[1]);
                                HRRFPrimarySkill.ExpertiseLevel = Convert.ToString(PSkill.Split(':')[2]);
                                // HRRFPrimarySkill.ExpertiseLevel = 1;
                                HRRFPrimarySkill.IsMandatoy = isMandatory;
                                HRRFPrimarySkill.ModifiedBy = Convert.ToString(Session["EmployeeId"]);
                                HRRFPrimarySkill.ModifiedDate = DateTime.Now;
                                //db.Entry(HRRFPrimarySkills).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                            else
                            {
                                var HRRFPrimarySkill = new HRRFSkills_ExpertiseLevel();
                                HRRFPrimarySkill.HRRFNumber = HRRFNumber;
                                HRRFPrimarySkill.SkillId = Convert.ToInt32(PSkill.Split(':')[1]);
                                HRRFPrimarySkill.ExpertiseLevel = Convert.ToString(PSkill.Split(':')[2]);
                                //  HRRFPrimarySkill.ExpertiseLevel = 2;
                                HRRFPrimarySkill.IsMandatoy = isMandatory;
                                HRRFPrimarySkill.CreatedBy = Convert.ToString(Session["EmployeeId"]);
                                HRRFPrimarySkill.CreatedDate = DateTime.Now;
                                db.HRRFSkills_ExpertiseLevel.Add(HRRFPrimarySkill);
                                db.SaveChanges();
                            }
                        }
                    }
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

        public ActionResult deleteSkills(string HRRFSkillId, string HrrfNumber)
        {
            try
            {
                int HRRFId = Convert.ToInt32(HRRFSkillId);
                var chk = db.HRRFSkills_ExpertiseLevel.Where(x => x.HRRFNumber == HrrfNumber && x.HrrfSkillid == HRRFId).FirstOrDefault();
                if (chk != null)
                {

                    HRRFSkills_ExpertiseLevel objSkill = new HRRFSkills_ExpertiseLevel();
                    objSkill = db.HRRFSkills_ExpertiseLevel.Single(skill => skill.HrrfSkillid == HRRFId);
                    db.HRRFSkills_ExpertiseLevel.Remove(objSkill);
                    db.SaveChanges();

                    return Json("success", JsonRequestBehavior.AllowGet);
                }


                else
                {
                    return Json("Fail", JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {

                Common.WriteExceptionErrorLog(ex);
                return Json("Error", JsonRequestBehavior.AllowGet);



            }
        }

        public ActionResult GetPrimarySkills(string CategoryId)
        {
            try
            {
                List<MasterLookUp> MasterLookUps = (from data in db.MasterLookUps.OrderBy(p => p.LookupName).Where(x => x.LookupType == "PrimarySkills" && x.ParentCode == CategoryId) select data).ToList();
                var PrimSkillsList = new List<SelectListItem>();
                if (MasterLookUps != null)
                {
                    foreach (MasterLookUp item in MasterLookUps)
                    {
                        PrimSkillsList.Add(new SelectListItem { Text = item.Description, Value = item.LookupID.ToString() });
                    }
                }
                return Json(PrimSkillsList, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Common.WriteExceptionErrorLog(ex);
                return Json("Error", JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult SubmitHRRFSkills(string PrimarySkills, string SecondarySkills, string HRRFNumber)
        {
            try
            {
                IList<string> PrimarySkillIds = PrimarySkills.TrimEnd(';').Split(';');
                IList<string> SecondarySkillIds = SecondarySkills.TrimEnd(';').Split(';');

                if (!string.IsNullOrEmpty(HRRFNumber))
                {
                    if (!string.IsNullOrEmpty(PrimarySkills))
                    {
                        foreach (string PSkill in PrimarySkillIds)
                        {
                            long id = Convert.ToInt64(PSkill.Split(':')[0]);
                            int rating = Convert.ToInt32(PSkill.Split(':')[1]);
                            long HRRFSkillId = (from data in db.HRRFSkills.Where(m => m.HRRFNumber == HRRFNumber && m.Skills == id) select data.HRRFSkillId).FirstOrDefault();
                            var HRRFPrimarySkills = db.HRRFSkills.Find(HRRFSkillId);
                            if (HRRFPrimarySkills != null)
                            {
                                var HRRFPrimarySkill = new HRRFSkill();
                              //  HRRFPrimarySkill.HRRFNumber = HRRFNumber;
                                HRRFPrimarySkill.IsPrimary = true;
                                HRRFPrimarySkill.Skills = Convert.ToInt64(PSkill.Split(':')[0]);
                                HRRFPrimarySkill.Rating = Convert.ToInt32(PSkill.Split(':')[1]);
                                HRRFPrimarySkill.CreatedBy = Convert.ToInt32(Session["EmployeeId"]);
                                HRRFPrimarySkill.CreatedDate = DateTime.Now;
                                db.Entry(HRRFPrimarySkills).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                            else
                            {
                                var HRRFPrimarySkill = new HRRFSkill();
                              //  HRRFPrimarySkill.HRRFNumber = HRRFNumber;
                                HRRFPrimarySkill.IsPrimary = true;
                                HRRFPrimarySkill.Skills = Convert.ToInt64(PSkill.Split(':')[0]);
                                HRRFPrimarySkill.Rating = Convert.ToInt32(PSkill.Split(':')[1]);
                                HRRFPrimarySkill.CreatedBy = Convert.ToInt32(Session["EmployeeId"]);
                                HRRFPrimarySkill.CreatedDate = DateTime.Now;
                                db.HRRFSkills.Add(HRRFPrimarySkill);
                                db.SaveChanges();
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(SecondarySkills))
                    {
                        foreach (string SSkill in SecondarySkillIds)
                        {
                            long Sid = Convert.ToInt64(SSkill.Split(':')[0]);
                            int rating = Convert.ToInt32(SSkill.Split(':')[1]);
                            long HRRFSkillId = (from data in db.HRRFSkills.Where(m => m.HRRFNumber == HRRFNumber && m.Skills == Sid) select data.HRRFSkillId).FirstOrDefault();
                            var HRRFSecondarySkills = db.HRRFSkills.Find(HRRFSkillId);
                            if (HRRFSecondarySkills != null)
                            {
                                // var HRRFSecondarySkills = new HRRFSkill();
                             //  HRRFSecondarySkills.HRRFNumber = HRRFNumber;
                                HRRFSecondarySkills.IsPrimary = false;
                                HRRFSecondarySkills.Skills = Convert.ToInt64(SSkill.Split(':')[0]);
                                HRRFSecondarySkills.Rating = Convert.ToInt32(SSkill.Split(':')[1]);
                                HRRFSecondarySkills.CreatedBy = Convert.ToInt32(Session["EmployeeId"]);
                                HRRFSecondarySkills.CreatedDate = DateTime.Now;
                                db.Entry(HRRFSecondarySkills).State = EntityState.Modified;
                                // db.HRRFSkills.Add(HRRFSecondarySkills);
                                db.SaveChanges();
                            }
                            else
                            {
                                var HRRFSecondarySkill = new HRRFSkill();
                              //  HRRFSecondarySkill.HRRFNumber = HRRFNumber;
                                HRRFSecondarySkill.IsPrimary = false;
                                HRRFSecondarySkill.Skills = Convert.ToInt64(SSkill.Split(':')[0]);
                                HRRFSecondarySkill.Rating = Convert.ToInt32(SSkill.Split(':')[1]);
                                HRRFSecondarySkill.CreatedBy = Convert.ToInt32(Session["EmployeeId"]);
                                HRRFSecondarySkill.CreatedDate = DateTime.Now;
                                db.HRRFSkills.Add(HRRFSecondarySkill);
                                db.SaveChanges();
                            }
                        }
                    }
                }
                return View();
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
            //    return null;
            //}
            catch (Exception ex)
            {
                Common.WriteExceptionErrorLog(ex);
                return RedirectToAction("Error", "Error");
            }

        }

        public ActionResult EditExistingHRRFSkills(string PrimarySkills, string SecondarySkills, string HRRFNumber)
        {
            try
            {
                IList<string> PrimarySkillIds = PrimarySkills.TrimEnd(';').Split(';');
                IList<string> SecondarySkillIds = SecondarySkills.TrimEnd(';').Split(';');
                if (!string.IsNullOrEmpty(HRRFNumber))
                {
                    var ExistingSkillIds = (from data in db.HRRFSkills.Where(x => x.HRRFNumber == HRRFNumber) select data.HRRFSkillId).ToList();
                    if (ExistingSkillIds != null && ExistingSkillIds.Count() > 0)
                    {
                        foreach (var hIds in ExistingSkillIds)
                        {
                            var histroyIds = db.HRRFSkills.Find(hIds);
                            db.HRRFSkills.Remove(histroyIds);
                        }
                        db.SaveChanges();
                    }
                    if (!string.IsNullOrEmpty(PrimarySkills))
                    {
                        foreach (string PSkill in PrimarySkillIds)
                        {
                            var HRRFPrimarySkill = new HRRFSkill();
                          //  HRRFPrimarySkill.HRRFNumber = HRRFNumber;
                            HRRFPrimarySkill.IsPrimary = true;
                            HRRFPrimarySkill.Skills = Convert.ToInt64(PSkill.Split(':')[0]);
                            HRRFPrimarySkill.Rating = Convert.ToInt32(PSkill.Split(':')[1]);
                            HRRFPrimarySkill.CreatedBy = Convert.ToInt32(Session["EmployeeId"]);
                            HRRFPrimarySkill.CreatedDate = DateTime.Now;
                            db.HRRFSkills.Add(HRRFPrimarySkill);
                            db.SaveChanges();
                        }
                    }
                    if (!string.IsNullOrEmpty(SecondarySkills))
                    {
                        foreach (string SSkill in SecondarySkillIds)
                        {
                            var HRRFSecondarySkill = new HRRFSkill();
                            HRRFSecondarySkill.HRRFNumber = HRRFNumber;
                            HRRFSecondarySkill.IsPrimary = false;
                            HRRFSecondarySkill.Skills = Convert.ToInt64(SSkill.Split(':')[0]);
                            HRRFSecondarySkill.Rating = Convert.ToInt32(SSkill.Split(':')[1]);
                            HRRFSecondarySkill.CreatedBy = Convert.ToInt32(Session["EmployeeId"]);
                            HRRFSecondarySkill.CreatedDate = DateTime.Now;
                            db.HRRFSkills.Add(HRRFSecondarySkill);
                            db.SaveChanges();
                        }
                    }
                }

                return View();
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
            catch (Exception ex)
            {

                Common.WriteExceptionErrorLog(ex);
                return RedirectToAction("Error", "Error");

            }

        }

        public ActionResult GetLocations(int LocationType)
        {
            try
            {
                if (LocationType != 0)
                {

                    if (LocationType == 1)
                    {
                        var Languages = (from offShoreLocation in db.MasterLookUps.Where(x => x.LookupType == "OnSiteLocation" && x.ParentCode == LocationType.ToString())
                                         select new
                                         {
                                             LookupName = offShoreLocation.LookupName,
                                             //Description = offShoreLocation.Description,
                                             LookupCode = offShoreLocation.LookupCode,
                                             SeqNumber = offShoreLocation.LookupID
                                         }).OrderBy(p => p.LookupName).ToList();
                        Languages.Add(new { LookupName = "Other", LookupCode = "Other", SeqNumber = (long)(Languages.Count + 1) });

                        ViewData["_Locations"] = Languages;

                        //var visalist = (from OnsiteVisaList in db.HRRFs.Where(v => v.LocationType == LocationType.ToString())select OnsiteVisaList.VisaType);
                        //List<string> visalist = new List<string>() { "None", "B1", "H-1B", "L-1A", "L-1B" };
                        //ViewData["_VisaTypesList"] = visalist;

                        return Json(Languages, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        var Languages = (from onSiteLocation in db.MasterLookUps
                                         .Where(x => x.LookupType == "OffShoreLocation" 
                                         && x.ParentCode == LocationType.ToString()
                                         && x.LookupName!="Noida"
                                         && x.LookupName!="Mumbai"
                                         )
                                         select new
                                         {
                                             LookupName = onSiteLocation.LookupName,
                                             Description = onSiteLocation.Description,
                                             LookupCode = onSiteLocation.LookupCode,
                                             SeqNumber = onSiteLocation.LookupID
                                         }).OrderBy(p => p.LookupName).ToList();
                        ViewData["_Locations"] = Languages;

                        return Json(Languages, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(string.Empty, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {

                Common.WriteExceptionErrorLog(ex);
                return Json("Error", JsonRequestBehavior.AllowGet);

            }
        }

        public ActionResult GetExperience(string Grade)
        {
            try
            {
                if (!string.IsNullOrEmpty(Grade))
                {
                    var ExpFromTo = from expFromTo in db.MasterLookUps.Where(x => x.LookupType == "ExpFrom" && x.ParentCode == Grade)
                                    select new
                                    {
                                        LookupName = expFromTo.LookupName,
                                        LookupCode = expFromTo.LookupCode,
                                    };

                    ViewData["_ExpFrom"] = ExpFromTo;
                    return Json(ExpFromTo, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(string.Empty, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {

                Common.WriteExceptionErrorLog(ex);
                return Json("Error", JsonRequestBehavior.AllowGet);

            }
        }

        public ActionResult GetDesignations(string Practice, int Grade, string costcenter)
        {
            try
            {
                HRRF Result = new HRRF();
                List<RoleBinding> listRoleAssign = new List<RoleBinding>();
                //if (!string.IsNullOrEmpty(Practice))
                    if (costcenter!="")
                    {
                    if (Practice== "Digital Apps")
                    {
                        Practice = "Business Apps";
                    }
                    //listRoleAssign = (from roles in db.DesignationMasters.Where(x => x.Practice.Contains(Practice) && x.Grade == Grade && x.CostCenter.Contains(costcenter))
                        listRoleAssign = (from roles in db.DesignationMasters.Where(x => x.Grade == Grade && x.CostCenter.Contains(costcenter))
                                         select new RoleBinding
                                         {
                                             LookupName = roles.DesignationName,
                                             LookupCode = roles.DesignationCode,
                                         }).OrderBy(p => p.LookupName).ToList();
                    ViewData["_Role"] = listRoleAssign;
                    return Json(listRoleAssign, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(string.Empty, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {

                Common.WriteExceptionErrorLog(ex);

                return Json("Error", JsonRequestBehavior.AllowGet);

            }
        }

        public void GetServiceLine()
        {
            try
            {
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
                ViewData["ServiceLine"] = ServiceLine;
            }
            catch (Exception ex)
            {
                ErrorHandling expcls = new ErrorHandling();
                expcls.Error(ex);

            }
        }

        [HttpPost]
        public ActionResult GetExistingPrimarySkills(string HRRFNumber)
        {
            try
            {
                // var GetPrimarySkills = (from data in db.HRRFSkills.Where(x => x.HRRFNumber == HRRFNumber && x.IsPrimary == true) select data).ToList();
                var GetPrimarySkills = from skills in db.HRRFSkills
                                       join mLookup in db.MasterLookUps on skills.Skills equals mLookup.LookupID
                                       where mLookup.LookupType == "PrimarySkills" && skills.HRRFNumber == HRRFNumber && skills.IsPrimary == true
                                       select new
                                       {
                                           Skillid = skills.Skills,
                                           Skills = mLookup.LookupName,
                                           Rating = skills.Rating,
                                       };

                return Json(GetPrimarySkills, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                Common.WriteExceptionErrorLog(ex);

                return Json("Error", JsonRequestBehavior.AllowGet);

            }
        }

        [HttpPost]
        public ActionResult GetExistingSecondarySkills(string HRRFNumber)
        {
            try
            {
                // var GetPrimarySkills = (from data in db.HRRFSkills.Where(x => x.HRRFNumber == HRRFNumber && x.IsPrimary == true) select data).ToList();
                var GetSecondSkills = from skills in db.HRRFSkills
                                      join mLookup in db.MasterLookUps on skills.Skills equals mLookup.LookupID
                                      where mLookup.LookupType == "PrimarySkills" && skills.HRRFNumber == HRRFNumber && skills.IsPrimary == false
                                      select new
                                      {
                                          Skillid = skills.Skills,
                                          Skills = mLookup.LookupName,
                                          Rating = skills.Rating,
                                      };

                return Json(GetSecondSkills, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                Common.WriteExceptionErrorLog(ex);

                return Json("Error", JsonRequestBehavior.AllowGet);

            }
        }

        public void InsertRecordsToHistroy(string HRRFNumber, string Description, int EmpIdForCreatedBy, string PreviousRequestStatus)
        {
            try
            {
                var HrrfHistory = new HRRFHistory();

                HrrfHistory.HRRFNumber = HRRFNumber;
                HrrfHistory.HistoryDescription = Description;
                HrrfHistory.ModifiedBy = EmpIdForCreatedBy;
                HrrfHistory.ModifiedDate = DateTime.Now;
                HrrfHistory.PrevRequestStatus = PreviousRequestStatus;

                db.HRRFHistories.Add(HrrfHistory);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                ErrorHandling expcls = new ErrorHandling();
                expcls.Error(ex);
                //throw;
            }
        }
        public void InsertRecordsToNotifications(string HRRFNumber, string SubjectName, string ch)
        {
            try
            {
                string[] strITOPS = Environment.GetEnvironmentVariable("ITS-OPS") ?? "".Split(',');               
                var objHRRF = db.HRRFs.Where(p => p.HRRFNumber == HRRFNumber).FirstOrDefault();
                // var oms = (db.RoleMasters.Where(s => (s.Practice == objHRRF.Practice && s.Role == "OM" && s.ApplicationCode == "TALENTREQ") || strITOPS.Contains(s.EmployeeId.ToString().Trim())).ToList());
                if (objHRRF != null)
                {                   
                    var oms = (from r in db.RoleMasters
                               join e in db.Employees on r.EmployeeId equals e.EmployeeId
                               where ((r.Role == "OM" && r.ApplicationCode == "TALENTREQ" && r.Practice == objHRRF.Practice) || strITOPS.Contains(r.EmployeeId.ToString().Trim())) && e.IsActive == true
                               select new
                               {
                                   e.EmployeeId

                               }).ToList();             

                    //var oms = (db.RoleMasters.Where(s => (s.Practice == objHRRF.Practice && s.Role == "OM" && s.ApplicationCode == "TALENTREQ") || strITOPS.Contains(s.EmployeeId.ToString().Trim())).ToList());
                    var projectDetails = db.Projects.Where(p => p.ProjectName == objHRRF.ProjectName && p.IsActive == true).FirstOrDefault();                   
                    Employee objEmp = db.Employees.Find(objHRRF.HRRFCreatedBy);
                    if (objEmp != null)
                    {
                        string HRRFCreatedBy = objEmp.FirstName + " " + (!string.IsNullOrWhiteSpace(objEmp.MiddleName) ? objEmp.MiddleName + " " : "") + objEmp.LastName;
                        string HRRFPractice = objHRRF.Practice;
                        string HRRFJobdescription = objHRRF.JobDescription;
                        string ClusterJD = objHRRF.JobDescription;
                        string HRRFTechPanel = objHRRF.TECHPANEL;
                        string HRRFTechPanel2 = objHRRF.SECONDTECHPANEL;
                        string loginEmpName = "";
                        int loginEmpId = Convert.ToInt32(Session["EmployeeId"]);
                        Employee objEmp2 = db.Employees.Find(loginEmpId);
                        if (objEmp2 != null)
                        {
                            loginEmpName = objEmp2.FirstName + " " + (!string.IsNullOrWhiteSpace(objEmp2.MiddleName) ? objEmp2.MiddleName + " " : "") + objEmp2.LastName;
                        }                      
                        string HRRFPurpose = "";                       
                        switch (objHRRF.Purpose.ToLower())
                        {
                            case "project":
                                HRRFPurpose = "'<b>" + objHRRF.ProjectName + "</b>' project";
                                break;
                            case "opportunity":
                                HRRFPurpose = "'<b>" + objHRRF.OpportunityName + "</b>' opportunity";
                                break;
                            case "proactive hire":
                                HRRFPurpose = "'<b>Proactive</b>'";
                                break;
                            case "corporate function":
                                HRRFPurpose = "'<b>corporate function</b>'";
                                break;
                        }
                        
                        //foreach (var omIds in oms)
                        //{
                        //Notification 
                        Notification tblNotification = new Notification();
                        if (SubjectName == "Edited")
                        {
                            tblNotification.NotificationType = HRRFNumber.Trim() + " " + Environment.GetEnvironmentVariable("TrEdited_NOtification") ?? "";
                        }
                        else
                        {
                            tblNotification.NotificationType = HRRFNumber.Trim() + " " + Environment.GetEnvironmentVariable("TrSubmitted_NOtification") ?? "";
                        }
                        tblNotification.NotificationDate = System.DateTime.Now;
                        tblNotification.NotificationFrom = Convert.ToInt32(Session["EmployeeId"]);
                        tblNotification.NotificationTo = Convert.ToInt32(Session["EmployeeId"]);
                        string deliverymanager = "";
                        
                        if (HRRFNumber.Trim() != "TR19704")
                        {
                            if (projectDetails.DELIVERY_MANAGER_ID != null && projectDetails.DELIVERY_MANAGER_ID > 0)
                            {
                                deliverymanager = (projectDetails.DELIVERY_MANAGER_ID).ToString();
                            }
                            if (projectDetails.ProjectManagerId != null && projectDetails.ProjectManagerId > 0)
                            {
                                if (deliverymanager == "")
                                    deliverymanager = (projectDetails.ProjectManagerId).ToString();
                                else if (deliverymanager != projectDetails.ProjectManagerId.ToString())
                                {
                                    deliverymanager = deliverymanager + "," + (projectDetails.ProjectManagerId).ToString();
                                }
                            }
                            
                            oms = oms.Distinct().ToList();
                            foreach (var omIds in oms)
                            {
                                if (deliverymanager == "")
                                {
                                    deliverymanager = omIds.EmployeeId.ToString();
                                }
                                else
                                {
                                    deliverymanager = deliverymanager + "," + omIds.EmployeeId.ToString();

                                }
                            }
                        }
                        else
                        {
                            deliverymanager = Environment.GetEnvironmentVariable("TRRestricted") ?? "";
                        }
                        List<string> deliverymanagerlst = deliverymanager.Split(',').ToList();
                        
                        deliverymanagerlst = deliverymanagerlst.Distinct().ToList();
                        deliverymanager = string.Join(",", deliverymanagerlst.ToArray());
                        tblNotification.CC = deliverymanager;
                        
                        StringBuilder sbBody = new StringBuilder();
                        if (tblNotification.NotificationType == HRRFNumber.Trim() + " Edited")
                        {
                            sbBody.Append(Environment.GetEnvironmentVariable("TrEdited") ?? "");
                            sbBody.Append(" <b>" + loginEmpName + "</b> (EmpID: <b>" + loginEmpId + "</b>).");
                        }
                        else
                        {
                            sbBody.Append(Environment.GetEnvironmentVariable("TrSubmitted") ?? "");
                            sbBody.Append(" <b>" + HRRFCreatedBy + "</b> (EmpID: <b>" + objEmp.EmployeeId + " </b>).");
                        }
                        sbBody.Append(Environment.GetEnvironmentVariable("TRNUMber") ?? "" + "<b>" + HRRFNumber.Trim() + ".</b><br/>");
                        
                        //sbBody.Append(" <b> Job Description: </b>" + HRRFJobdescription + "<br/><br/>");
                        sbBody.Append(" <b> Job Description: </b>" + ClusterJD + "<br/><br/>");
                        sbBody.Append(" <b> First Level Tech Panel: </b>" + HRRFTechPanel + "<br/><br/>");
                        sbBody.Append(" <b> Second Level Tech Panel: </b>" + HRRFTechPanel2 + "<br/><br/>");              
                        sbBody.Append(MailingContent(HRRFNumber));
                        
                        if (tblNotification.NotificationType == HRRFNumber.Trim() + " Edited")
                        {
                            sbBody.Append("<b>" + Environment.GetEnvironmentVariable("EditedFields") ?? "" + "</b>" + "<br/>" + ch + "<br/>");
                        }
                        //var Body = Environment.GetEnvironmentVariable("TrSubmitted") ?? ConfigurationManager.AppSettings["TrSubmitted"].ToString() + Environment.GetEnvironmentVariable("TRNUMber") ?? ConfigurationManager.AppSettings["TRNUMber"].ToString() + HRRFNumber.Trim();
                        
                        tblNotification.IsActive = true;
                        tblNotification.AssetID = Convert.ToString(HRRFNumber.Trim());
                        tblNotification.ApplicationCode = Environment.GetEnvironmentVariable("AppCodeTR") ?? "";
                        tblNotification.IsFromGroupID = true;
                        tblNotification.FromGroupID = "donotreply_Talento@trianz.com";
                        
                        string talentoURL = Environment.GetEnvironmentVariable("Talento") ?? "";
                        string body = string.Empty;
                        
                        string Editlink = "";
                        if (tblNotification.NotificationType == HRRFNumber.Trim() + " Edited")
                        {
                            Editlink = "Please click on link to view the details of Submitted Talent Request number #";
                        }
                        else
                        {
                            Editlink = "Please click on link to view the details of Submitted Talent Request number #";
                        }
                        string LinktoOpen = Editlink + HRRFNumber + " : " + talentoURL;
                        using (StreamReader reader = new StreamReader(Server.MapPath("~/Views/Shared/EmailTemplate.html")))
                        {
                            body = reader.ReadToEnd();
                        }
                        
                        body = body.Replace("{ToUserName}", db.Employees.Where(e => e.EmployeeId == tblNotification.NotificationTo).Select(s => s.FirstName + " " + s.LastName).FirstOrDefault());
                        body = body.Replace("{FromUserName}", "Talento Team");
                        body = body.Replace("{Description}", sbBody.ToString());
                        body = body.Replace("{LinktoOpen}", LinktoOpen);
                        
                        tblNotification.NotificationMessage = body;
                        
                        bool IsEmail = Convert.ToBoolean(IsEmailSent);
                        if (IsEmail == true)
                        {
                            db.Notifications.Add(tblNotification);
                            db.SaveChanges();
                        }
                        //  }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandling expcls = new ErrorHandling();
                expcls.Error(ex);
                throw;
            }
        }

        //public void InsertRecordsToNotifications(string HRRFNumber, string SubjectName, string ch)
        //{
        //    try
        //    {
        //        string[] strITOPS = Environment.GetEnvironmentVariable("ITS-OPS") ?? ConfigurationManager.AppSettings["ITS-OPS"].Split(',');

        //        var objHRRF = db.HRRFs.Where(p => p.HRRFNumber == HRRFNumber).FirstOrDefault();
        //       //  var oms = (db.RoleMasters.Where(s => (s.Practice == objHRRF.Practice && s.Role == "OM" && s.ApplicationCode == "TALENTREQ") || strITOPS.Contains(s.EmployeeId.ToString().Trim())).ToList());


        //        var oms = (from rls in db.RoleMasters
        //                  join Emp in db.Employees on rls.EmployeeId equals Emp.EmployeeId 
        //                  where (rls.Practice == objHRRF.Practice && rls.Role == "OM" && rls.ApplicationCode == "TALENTREQ" && Emp.IsActive == true) || strITOPS.Contains(rls.EmployeeId.ToString().Trim())
        //                  select new
        //                  {
        //                      EmployeeId = rls.EmployeeId,
        //                  }).OrderBy(p => p.EmployeeId).ToList();

        //        //var GetSecondSkills = from skills in db.HRRFSkills
        //        //                      join mLookup in db.MasterLookUps on skills.Skills equals mLookup.LookupID
        //        //                      where mLookup.LookupType == "PrimarySkills" && skills.HRRFNumber == HRRFNumber && skills.IsPrimary == false
        //        //                      select new
        //        //                      {
        //        //                          Skillid = skills.Skills,
        //        //                          Skills = mLookup.LookupName,
        //        //                          Rating = skills.Rating,
        //        //                      };


        //        var projectDetails = db.Projects.Where(a => a.ProjectName == objHRRF.ProjectName).FirstOrDefault();

        //        if (objHRRF != null)
        //        {
        //            Employee objEmp = db.Employees.Find(objHRRF.HRRFCreatedBy);
        //            if (objEmp != null)
        //            {
        //                string HRRFCreatedBy = objEmp.FirstName + " " + (!string.IsNullOrWhiteSpace(objEmp.MiddleName) ? objEmp.MiddleName + " " : "") + objEmp.LastName;
        //                string HRRFPractice = objHRRF.Practice;
        //                string HRRFJobdescription = objHRRF.JobDescription;
        //                string ClusterJD = objHRRF.JobDescription;
        //                string HRRFTechPanel = objHRRF.TECHPANEL;
        //                string HRRFTechPanel2 = objHRRF.SECONDTECHPANEL;
        //                string loginEmpName = "";
        //                int loginEmpId = Convert.ToInt32(Session["EmployeeId"]);
        //                Employee objEmp2 = db.Employees.Find(loginEmpId);
        //                if (objEmp2 != null)
        //                {
        //                    loginEmpName = objEmp2.FirstName + " " + (!string.IsNullOrWhiteSpace(objEmp2.MiddleName) ? objEmp2.MiddleName + " " : "") + objEmp2.LastName;
        //                }

        //                string HRRFPurpose = "";

        //                switch (objHRRF.Purpose.ToLower())
        //                {
        //                    case "project":
        //                        HRRFPurpose = "'<b>" + objHRRF.ProjectName + "</b>' project";
        //                        break;
        //                    case "opportunity":
        //                        HRRFPurpose = "'<b>" + objHRRF.OpportunityName + "</b>' opportunity";
        //                        break;
        //                    case "proactive hire":
        //                        HRRFPurpose = "'<b>Proactive</b>'";
        //                        break;
        //                    case "corporate function":
        //                        HRRFPurpose = "'<b>corporate function</b>'";
        //                        break;
        //                }

        //                //foreach (var omIds in oms)
        //                //{
        //                //Notification 
        //                Notification tblNotification = new Notification();
        //                if (SubjectName == "Edited")
        //                {
        //                    tblNotification.NotificationType = HRRFNumber.Trim() + " " + Environment.GetEnvironmentVariable("TrEdited_NOtification") ?? ConfigurationManager.AppSettings["TrEdited_NOtification"].ToString();
        //                }
        //                else
        //                {
        //                    tblNotification.NotificationType = HRRFNumber.Trim() + " " + Environment.GetEnvironmentVariable("TrSubmitted_NOtification") ?? ConfigurationManager.AppSettings["TrSubmitted_NOtification"].ToString();
        //                }
        //                tblNotification.NotificationDate = System.DateTime.Now;
        //                tblNotification.NotificationFrom = Convert.ToInt32(Session["EmployeeId"]);
        //                tblNotification.NotificationTo = Convert.ToInt32(Session["EmployeeId"]);
        //                string deliverymanager = "";
        //                if (HRRFNumber.Trim() != "TR19704")
        //                {
        //                    if (projectDetails.DELIVERY_MANAGER_ID != null && projectDetails.DELIVERY_MANAGER_ID > 0)
        //                    {
        //                        deliverymanager = (projectDetails.DELIVERY_MANAGER_ID).ToString();
        //                    }
        //                    if (projectDetails.ProjectManagerId != null && projectDetails.ProjectManagerId > 0)
        //                    {
        //                        if (deliverymanager == "")
        //                            deliverymanager = (projectDetails.ProjectManagerId).ToString();
        //                        else
        //                            deliverymanager = deliverymanager + "," + (projectDetails.ProjectManagerId).ToString();
        //                    }
        //                    if (oms != null && oms.Count() > 0)
        //                    {
        //                        foreach (var omIds in oms)
        //                        {
        //                            if (deliverymanager == "")
        //                            {
        //                                deliverymanager = omIds.EmployeeId.ToString();
        //                            }
        //                            else
        //                            {
        //                                deliverymanager = deliverymanager + "," + omIds.EmployeeId.ToString();
        //                            }
        //                        }
        //                    }
        //                }
        //               else 
        //                {
        //                    deliverymanager = Environment.GetEnvironmentVariable("TRRestricted") ?? ConfigurationManager.AppSettings["TRRestricted"].ToString();
        //                }

        //                tblNotification.CC = deliverymanager;


        //                StringBuilder sbBody = new StringBuilder();
        //                if (tblNotification.NotificationType == HRRFNumber.Trim() + " Edited")
        //                {
        //                    sbBody.Append(Environment.GetEnvironmentVariable("TrEdited") ?? ConfigurationManager.AppSettings["TrEdited"].ToString());
        //                    sbBody.Append(" <b>" + loginEmpName + "</b> (EmpID: <b>" + loginEmpId + "</b>).");
        //                }
        //                else
        //                {
        //                    sbBody.Append(Environment.GetEnvironmentVariable("TrSubmitted") ?? ConfigurationManager.AppSettings["TrSubmitted"].ToString());
        //                    sbBody.Append(" <b>" + HRRFCreatedBy + "</b> (EmpID: <b>" + objEmp.EmployeeId + " </b>).");
        //                }
        //                sbBody.Append(Environment.GetEnvironmentVariable("TRNUMber") ?? ConfigurationManager.AppSettings["TRNUMber"].ToString() + "<b>" + HRRFNumber.Trim() + ".</b><br/>");

        //                //sbBody.Append(" <b> Job Description: </b>" + HRRFJobdescription + "<br/><br/>");
        //                sbBody.Append(" <b> Job Description: </b>" + ClusterJD + "<br/><br/>");
        //                sbBody.Append(" <b> First Level Tech Panel: </b>" + HRRFTechPanel + "<br/><br/>");
        //                sbBody.Append(" <b> Second Level Tech Panel: </b>" + HRRFTechPanel2 + "<br/><br/>");


        //                sbBody.Append(MailingContent(HRRFNumber));


        //                if (tblNotification.NotificationType == HRRFNumber.Trim() + " Edited")
        //                {
        //                    sbBody.Append("<b>" + Environment.GetEnvironmentVariable("EditedFields") ?? ConfigurationManager.AppSettings["EditedFields"] + "</b>" + "<br/>" + ch + "<br/>");
        //                }
        //                //var Body = Environment.GetEnvironmentVariable("TrSubmitted") ?? ConfigurationManager.AppSettings["TrSubmitted"].ToString() + Environment.GetEnvironmentVariable("TRNUMber") ?? ConfigurationManager.AppSettings["TRNUMber"].ToString() + HRRFNumber.Trim();

        //                tblNotification.IsActive = true;
        //                tblNotification.AssetID = Convert.ToString(HRRFNumber.Trim());
        //                tblNotification.ApplicationCode = Environment.GetEnvironmentVariable("AppCodeTR") ?? ConfigurationManager.AppSettings["AppCodeTR"].ToString();
        //                tblNotification.IsFromGroupID = true;
        //                tblNotification.FromGroupID = "donotreply_Talento@trianz.com";

        //                string talentoURL = Environment.GetEnvironmentVariable("Talento") ?? ConfigurationManager.AppSettings["Talento"];
        //                string body = string.Empty;

        //                string Editlink = "";
        //                if (tblNotification.NotificationType == HRRFNumber.Trim() + " Edited")
        //                {
        //                    Editlink = "Please click on link to view the details of Submitted Talent Request number #";
        //                }
        //                else
        //                {
        //                    Editlink = "Please click on link to view the details of Submitted Talent Request number #";
        //                }
        //                string LinktoOpen = Editlink + HRRFNumber + " : " + talentoURL;
        //                using (StreamReader reader = new StreamReader(Server.MapPath("~/Views/Shared/EmailTemplate.html")))
        //                {
        //                    body = reader.ReadToEnd();
        //                }

        //                body = body.Replace("{ToUserName}", db.Employees.Where(e => e.EmployeeId == tblNotification.NotificationTo).Select(s => s.FirstName + " " + s.LastName).FirstOrDefault());
        //                body = body.Replace("{FromUserName}", "Talento Team");
        //                body = body.Replace("{Description}", sbBody.ToString());
        //                body = body.Replace("{LinktoOpen}", LinktoOpen);

        //                tblNotification.NotificationMessage = body;

        //                bool IsEmail = Convert.ToBoolean(IsEmailSent);
        //                if (IsEmail == true)
        //                {
        //                    db.Notifications.Add(tblNotification);
        //                    db.SaveChanges();
        //                }
        //                //  }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorHandling expcls = new ErrorHandling();
        //        expcls.Error(ex);
        //        throw;
        //    }
        //}

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
        public ActionResult GetProjectForPM()
        {
            try
            {
                int EmployeeId = Convert.ToInt32(Session["EmployeeId"]);
                var PMProject = from PMProjects in db.ProjectAssignments
                                where PMProjects.EmployeeId == EmployeeId
                                select new
                                {
                                    ProjectCode = PMProjects.ProjectCode,
                                    ProjectName = PMProjects.ProjectName,
                                };
                string ProjectName = PMProject.Select(x => x.ProjectName).FirstOrDefault();
                return Json(ProjectName, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Common.WriteExceptionErrorLog(ex);
                return Json("Error", JsonRequestBehavior.AllowGet);
            }
        }
        
        public ActionResult GetJDBasedOnDesignation(string DesignationCode)
        {
            try
            {
                var GetDesignation = from Designations in db.DesignationMasters
                                     where Designations.DesignationCode == DesignationCode
                                     select new
                                     {
                                         JD = Designations.JobDescription,
                                     };
                string esignation = GetDesignation.Select(x => x.JD).FirstOrDefault();
                return Json(esignation, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ErrorHandling expcls = new ErrorHandling();
                expcls.Error(ex);
                return null;
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

        public string getChanges(HRRF h1, string pSkills, string sSkills)
        {
            if (pSkills.Length > 0)
            {
                pSkills = pSkills.Replace(pSkills.Substring(pSkills.IndexOf(':'), (pSkills.Length - pSkills.LastIndexOf(':')) - 1), "");
            }

            if (sSkills.Length > 0)
            {
                sSkills = sSkills.Replace(sSkills.Substring(sSkills.IndexOf(':'), (sSkills.Length - sSkills.LastIndexOf(':')) - 1), "");
            }

            TrianzOperationsEntities db_obj = new TrianzOperationsEntities();
            List<string> changes = new List<string>();
            string ch = "";
            PropertyInfo[] properties = typeof(HRRF).GetProperties();
            var hrrf = (from x in db_obj.HRRFs where x.HRRFNumber == h1.HRRFNumber select x).FirstOrDefault();
            var primary = (from x in db_obj.HRRFSkills_ExpertiseLevel where x.HRRFNumber == h1.HRRFNumber select x.SkillId);
            var sec = (from x in db_obj.HRRFSkills where x.HRRFNumber == h1.HRRFNumber && x.IsPrimary == false select x.Skills);
            string ps = string.Join(":", primary);
            string ss = string.Join(":", sec);
            string[] c1 = ps.Split(':');   //splitting primary skills, fetched from db and forming a string array
            string[] c2 = ss.Split(':');   //splitting secondary skills, fetched from db and forming a string array
            pSkills = pSkills.TrimEnd(';');
            sSkills = sSkills.TrimEnd(';');
            pSkills = pSkills.Replace(';', ':');
            sSkills = sSkills.Replace(';', ':');
            string[] c3 = pSkills.Split(':');   //splitting primary skills, fetched from user and forming a string array
            string[] c4 = sSkills.Split(':');   //splitting secondary skills, fetched from user and forming a string array

            //  if (ps != pSkills)
            if (!string.IsNullOrEmpty(pSkills))
            {
                changes.Add("PrimarySkills");
            }

            if (ss != sSkills)
            {
                changes.Add("SecondarySkills");
            }

            //if (c1.Length == c3.Length / 2 || c1.Length == c3.Length)
            //{
            //    for (int i = 0; i < c1.Length; i++)
            //    {
            //        if (i == 0)
            //        {
            //            if (c1[i] != c3[i])
            //            {
            //                changes.Add("PrimarySkills");
            //                break;
            //            }
            //        }
            //        else
            //        {
            //            if (c1[i] != c3[i + 1])
            //            {
            //                changes.Add("PrimarySkills");
            //                break;
            //            }
            //        }

            //    }
            //}
            //else
            //{
            //    changes.Add("PrimarySkills");
            //}
            //if (c2.Length == c4.Length / 2 || c2.Length == c4.Length)
            //{
            //    for (int i = 0; i < c2.Length; i++)
            //    {
            //        if (i == 0)
            //        {
            //            if (c2[i] != c4[i])
            //            {
            //                changes.Add("SecondarySkills");
            //                break;
            //            }
            //        }
            //        else
            //        {
            //            if (c2[i] != c4[i + 1])
            //            {
            //                changes.Add("SecondarySkills");
            //                break;
            //            }
            //        }
            //    }
            //}
            //else
            //{
            //    changes.Add("SecondarySkills");
            //}

            foreach (PropertyInfo pi in properties)
            {
                var value1 = typeof(HRRF).GetProperty(pi.Name).GetValue(hrrf);
                var value2 = typeof(HRRF).GetProperty(pi.Name).GetValue(h1);
                if (value1 != null && value2 != null)
                {
                    if (value1.ToString() != value2.ToString())
                    {
                        changes.Add(string.Format(pi.Name));
                    }
                }
            }

            if (changes.Count != 0)
            {
                foreach (var x in changes)
                {
                    ch = ch + x + ",";
                }
            }
            return ch = ch.TrimEnd(',');
        }

        public ActionResult GetRequestReason(String Purpose)
        {
            try
            {
                if (!string.IsNullOrEmpty(Purpose))
                {
                    var RequestReason = db.MasterLookUps.Where(rh => rh.LookupType == Purpose && rh.Active == true).ToList();
                    ViewData["_RequestReason"] = RequestReason;

                    List<string> ResourceType = new List<string>();
                    if (Purpose.ToLower() == "project")
                    {
                        //ResourceType.Add("Billable");
                        //ResourceType.Add("Buffer");
                        ResourceType.Add("Billed");
                        ResourceType.Add("Business Operations");
                        //ResourceType.Add("BSA");
                        ResourceType.Add("Delivery");
                        ResourceType.Add("Internal");
                        ResourceType.Add("Shadow");

                        ResourceType.Add("Management");
                        ResourceType.Add("Presales");
                        ResourceType.Add("Internal Application");
                        ResourceType.Add("ESS");
                        ResourceType.Add("Account Ops");
                        ResourceType.Add("Solution Development");
                        ResourceType.Add("Intern");
                        ResourceType.Add("Practice Support");

                    }
                    else if (Purpose.ToLower() == "opportunity")
                    {
                        ResourceType.Add("Billed");
                        ResourceType.Add("Business Operations");
                        //ResourceType.Add("BSA");
                        ResourceType.Add("Delivery");
                        ResourceType.Add("Internal");
                        ResourceType.Add("Shadow");
                    }
                    else if (Purpose.ToLower() == "corporate function")
                    {
                        ResourceType.Add("Billed");
                        ResourceType.Add("Internal");
                        ResourceType.Add("Shadow");
                    }
                    else if (Purpose.ToLower() == "proactive hire")
                    {
                        //ResourceType.Add("Buffer");

                        ResourceType.Add("Bench");
                        //ResourceType.Add("Business Operations");
                        //ResourceType.Add("BSA");
                        //ResourceType.Add("Delivery");
                        //ResourceType.Add("Investment");
                        //ResourceType.Add("Management");
                    }
                    //else if (Purpose.ToLower() == "opportunity")
                    //{
                    //    ResourceType.Add("Buffer");
                    //}

                    ViewData["_ResourceType"] = ResourceType.OrderBy(ord => ord).ToList();
                    return Json(RequestReason, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(string.Empty, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {

                Common.WriteExceptionErrorLog(ex);
                return Json("Error", JsonRequestBehavior.AllowGet);

            }
        }

        public ActionResult GetOtherLocation(String OtherLocation)
        {
            try
            {
                if (!string.IsNullOrEmpty(OtherLocation))
                {
                    OtherLocation = Request.Form["OtherLocation"];
                    var otherloc = db.MasterLookUps.Where(OL => OL.LookupType.ToLower() == "onsitelocation" && OL.ParentCode == "1"
                    && OL.LookupName == OtherLocation).Any();

                    return Json(otherloc, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(string.Empty, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {

                Common.WriteExceptionErrorLog(ex);
                return Json("Error", JsonRequestBehavior.AllowGet);


            }
        }

        public ActionResult GetAccount_SGA(string Cost_Center_Value, string Prac_tice, string Pur_pose)
        {
            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            {
                try
                {
                    if (!string.IsNullOrEmpty(Cost_Center_Value))
                    {
                        string Role = (string)Session["Role"];  // PM, OM
                        int empID = (int)Session["EmployeeId"]; //101098 Manger ID;

                        if (Pur_pose.ToLower() != "project" && Pur_pose.ToLower() != "opportunity")
                        {
                            var AccountName = (from pCodes in db.PracticeWiseBenchCodes
                                               join proj in db.Projects on pCodes.BenchCode equals proj.ProjectCode
                                               //where pCodes.Practice.ToLower() == Prac_tice.ToLower() &&
                                               where
                                              pCodes.CostCenter.ToLower() == Cost_Center_Value.ToLower()
                                               select new
                                               {
                                                   Account = proj.AccountName
                                               }).Distinct().OrderBy(p => p.Account).ToList();
                            ViewData["_AccountName"] = AccountName;
                            return Json(AccountName, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {


                            var AccountName = (from projects in db.Projects
                                               where projects.IsActive == true
                                              // where projects.IsActive == true && projects.SOWEndDate >= DateTime.Now
                                               orderby projects.AccountName
                                               select new
                                               {
                                                   Account = projects.AccountName
                                               }).Distinct().OrderBy(p => p.Account).ToList();

                            ViewData["_AccountName"] = AccountName;
                            return Json(AccountName, JsonRequestBehavior.AllowGet);

                        }
                    }

                    else
                    {
                        return Json(string.Empty, JsonRequestBehavior.AllowGet);
                    }
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
        
        public ActionResult GetCertifications()
        {
            try
            {
                var certificationMasters = db.CertificationMasters.ToList().OrderBy(x => x.CertificationName);
                ViewData["_certifications"] = certificationMasters;
                return Json(certificationMasters, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                Common.WriteExceptionErrorLog(ex);
                return Json("Error", JsonRequestBehavior.AllowGet);

            }
        }

       
        public ActionResult GetSkillCode(string Practice,string SkillCluster,string SkillCode)
        {
            try
            {
                var _skillCode = db.PRTCSKILLCLUSTERs.Where(x =>x.PRACTICE.ToLower().Equals(Practice.ToLower())&& x.SKILLCLUSTER.ToLower() == SkillCluster.ToLower() && x.CSKILLS.ToLower() == SkillCode.ToLower())
                    .Select(x => x.SKILLCODE).SingleOrDefault();
                var _clusterDisc = db.SKILLCLSTRJDs.Where(x => x.SKILLCODE == _skillCode).Select(x => x.CLSTRDESC).FirstOrDefault();
                var _clusterJD = db.SKILLCLSTRJDs.Where(x => x.SKILLCODE == _skillCode).Select(x => x.CLSTRJD).FirstOrDefault();

                var data = new
                {
                    SkillCode = _skillCode==null?string.Empty: _skillCode,
                    ClusterDisc= _clusterDisc==null?string.Empty: _clusterDisc,
                    ClusterJD= _clusterJD==null?string.Empty: _clusterJD
                };
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                Common.WriteExceptionErrorLog(ex);
                return Json("Error", JsonRequestBehavior.AllowGet);


            }
        }
        public ActionResult GetCSkill(string Practice,string SkillCluster)
        {
            try
            {
                Common.WriteErrorLog("GetSkillCode Method Inside"+ SkillCluster);
                var _skillCode = (from data in db.PRTCSKILLCLUSTERs.Where(x => x.SKILLCLUSTER.ToLower().Equals(SkillCluster.Trim().ToLower())&& x.PRACTICE.ToLower().Equals(Practice.ToLower()))
                                     select new
                                     {
                                         CSkill = data.CSKILLS
                                     }).Distinct().ToList().OrderBy(p => p.CSkill);
                ViewData["_CSkill"] = _skillCode;

                return Json(_skillCode, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                Common.WriteExceptionErrorLog(ex);
                return Json("Error", JsonRequestBehavior.AllowGet);


            }
        }
        public ActionResult GetSkillCluster(string practice)
        {
            try
            {
                Common.WriteErrorLog("GetSkillCluster Method Inside");
                var _skillCluster = (from data in db.PRTCSKILLCLUSTERs.Where(x => x.PRACTICE.ToLower().Equals(practice.Trim().ToLower()))
                                  select new
                                  {
                                      SkillCluster = data.SKILLCLUSTER
                                  }).Distinct().ToList().OrderBy(p => p.SkillCluster);
                ViewData["_skillCluster"] = _skillCluster;

                return Json(_skillCluster, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                Common.WriteExceptionErrorLog(ex);
                return Json("Error", JsonRequestBehavior.AllowGet);


            }
        }
        public ActionResult GetCoreSkills(int grade,string roleGroup,string practice)
        {
            try
            {
                
                var standardSkillMasters = db.SKILLSCOREs.Where(x=> x.GRADE==grade.ToString() && x.ROLEGROUP==roleGroup&&x.PRACTICE== practice).ToList().OrderBy(x => x.SKILLS);
                ViewData["_coreSkills"] = standardSkillMasters;
                return Json(standardSkillMasters, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                Common.WriteExceptionErrorLog(ex);
                return Json("Error", JsonRequestBehavior.AllowGet);


            }
        }
        public ActionResult GetAdvancedSkills(int grade, string roleGroup, string practice)
        {
            try
            {
                var standardSkillMasters = db.SKILLSADVs.Where(x => x.GRADE == grade.ToString() && x.ROLEGROUP == roleGroup&&x.PRACTICE==practice).ToList().OrderBy(x => x.SKILLS);
                ViewData["_advancedSkills"] = standardSkillMasters;
                return Json(standardSkillMasters, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                Common.WriteExceptionErrorLog(ex);
                return Json("Error", JsonRequestBehavior.AllowGet);

            }
        }

        public ActionResult GetintermediateSkills(int grade, string roleGroup, string practice)
        {
            try
            {
                var standardSkillMasters = db.SKILLSINTRMs.Where(x => x.GRADE == grade.ToString() && x.ROLEGROUP == roleGroup&&x.PRACTICE== practice).ToList().OrderBy(x => x.SKILLS);
                ViewData["_intermediateSkills"] = standardSkillMasters;
                return Json(standardSkillMasters, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                Common.WriteExceptionErrorLog(ex);
                return Json("Error", JsonRequestBehavior.AllowGet);


            }
        }

        public ActionResult GetSpecificPlatForms()
        {
            try
            {
                var specificFormMasters = db.SpecificPlatFormMasters.ToList().OrderBy(x=>x.SpecificPlatFormName);
                ViewData["_specificPlatForm"] = specificFormMasters;
                return Json(specificFormMasters, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                Common.WriteExceptionErrorLog(ex);
                return Json("Error", JsonRequestBehavior.AllowGet);

            }
        }
        public ActionResult GetDisciplines(string practice)
        {
            try
            {
                var disciplineMasters = db.DisciplineMasters.Where(x => x.Practice == practice.Trim()).ToList();
                ViewData["_Discipline"] = disciplineMasters;
                return Json(disciplineMasters, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                Common.WriteExceptionErrorLog(ex);
                return RedirectToAction("Error", "Error");

            }
        }
        public ActionResult GetRoleGroup(int grade,string practice)
        {
            try
            {
           
                var list = db.RoleGroupMasters.Where(x=>x.Grade==grade&&x.Practice== practice).GroupBy(x => x.RoleGroupName).Select(n => new
                {
                    RoleGroupName = n.Key,
                    RoleGroupNameCount = n.Count()
                }).OrderBy(y => y.RoleGroupName).ToList();
                ViewData["_roleGroup"] = list;
                return Json(list, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                Common.WriteExceptionErrorLog(ex);
                return Json("Error", JsonRequestBehavior.AllowGet);

            }
        }
        public ActionResult GetCostcenter(string Practice)
        {
            try
            {
                if (!string.IsNullOrEmpty(Practice))
                {

                    var CostCenter = (from data in db.PracticeWiseBenchCodes.Where(x => x.Practice.ToLower().Equals(Practice.Trim().ToLower()))
                                      select new
                                      {
                                          costcenterlist = data.CostCenter
                                      }).Distinct().ToList().OrderBy(p => p.costcenterlist);
                    if (Practice.ToLower() == "business engagement")
                    {
                        var CostCenterfilter = CostCenter.Where(x => !x.costcenterlist.Contains("Delivery")).ToList();
                        ViewData["_Costcenter"] = CostCenterfilter;
                        return Json(CostCenterfilter, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        ViewData["_Costcenter"] = CostCenter;
                        return Json(CostCenter, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(string.Empty, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {

                Common.WriteExceptionErrorLog(ex);
                return Json("Error", JsonRequestBehavior.AllowGet);
            }
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult Autocomplete(string term)
        {
            try
            {
                var result = new List<KeyValuePair<string, string>>();
                DateTime dt = DateTime.Now;
                IList<SelectListItem> List = new List<SelectListItem>();

                var Empdetails = (from pro in db.Employees
                                      // where pro.IsActive == true
                                  select new SelectListItem
                                  {
                                      Text = pro.FirstName + " " + pro.MiddleName + " " + pro.LastName,
                                      Value = pro.EmployeeId + "-" + pro.FirstName + " " + pro.MiddleName + " " + pro.LastName


                                  }).Distinct().ToList();

                foreach (var item in Empdetails)
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
                return Json("Error", JsonRequestBehavior.AllowGet);

            }
        

        }



        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult GetOpportunityByNameorCode(string inputType, string Opportunity)
        {
            try
            {
                // List<clsOpportunities> GetOpportunities = JsonConvert.DeserializeObject<List<clsOpportunities>>(AgentService.GetOpportunityByNameorCode());
                List<Opportunity> GetOpportunities = db.Opportunities.Where(opp => opp.sales_Stage.Replace("- ", "") == "Stage 6 Close Won"
                   || opp.sales_Stage.Replace("- ", "") == "Stage 5 Likely Booking" || opp.sales_Stage.Replace("- ", "") == "Stage 4 Negotiate"
                   || opp.sales_Stage.Replace("- ", "") == "Stage 2 Develop" || opp.sales_Stage.Replace("- ", "") == "Stage 0 Prospect"
                   || opp.sales_Stage.Replace("- ", "") == "Stage 1 Engage" || opp.sales_Stage.Replace("- ", "") == "Stage 3 Solution").OrderBy(x => x.potentialname).ToList();

                List<Opportunity> filteredOpportunities = new List<Opportunity>();
                if (GetOpportunities.Count > 0)
                {
                    if (inputType.ToUpper() == "OPPNAME")
                    {
                        filteredOpportunities =
                           GetOpportunities.Where(O => O.potentialname.Equals(Opportunity)).ToList();
                    }
                    else
                    {
                        filteredOpportunities =
                             GetOpportunities.Where(O => O.potential_no.Equals(Opportunity)).ToList();
                    }
                    return Json(filteredOpportunities, JsonRequestBehavior.AllowGet);
                }

                return Json("", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                Common.WriteExceptionErrorLog(ex);
                return Json("Error", JsonRequestBehavior.AllowGet);
            }
        }

        [AcceptVerbs(HttpVerbs.Post)]
        //public JsonResult GetOpportunityByNameorCode(string inputType, string opportunity, string flag)
        //{
        //    var result = new List<KeyValuePair<string, string>>();

        //    List<Opportunities> GetOpportunities = db.Opportunities.Where(opp => opp.sales_Stage.ToLower() == "closed won" || opp.sales_Stage.ToLower() == "likely booking" || opp.sales_Stage.ToLower() == "negotiation").ToList();

        //    foreach (var item in GetOpportunities)
        //    {
        //        result.Add(new KeyValuePair<string, string>(item.potential_no.ToString(), item.potentialname));
        //    }

        //    if (GetOpportunities.Count > 0)
        //    {
        //        var filteredResult = new List<KeyValuePair<string, string>>();

        //        if (inputType.ToUpper() == "OPPNAME")
        //        {
        //            if (flag.ToLower() == "exactly")
        //            {
        //                filteredResult = result.Where(s => s.Value.ToLower().Equals(opportunity.ToLower())).Select(w => w).ToList();
        //            }
        //            else
        //            {
        //                filteredResult = result.Where(s => s.Value.ToLower().Contains(opportunity.ToLower())).Select(w => w).ToList();
        //            }
        //        }
        //        else if (inputType.ToUpper() == "OPPCODE")
        //        {
        //            if (flag.ToLower() == "exactly")
        //            {
        //                filteredResult = result.Where(s => s.Key.ToLower().Equals(opportunity.ToLower())).Select(w => w).ToList();
        //            }
        //            else
        //            {
        //                filteredResult = result.Where(s => s.Key.ToLower().Contains(opportunity.ToLower())).Select(w => w).ToList();
        //            }
        //        }

        //        return Json(filteredResult, JsonRequestBehavior.AllowGet);
        //    }

        //    return Json("", JsonRequestBehavior.AllowGet);
        //}

        public ActionResult GetAllOpportunities()
        {
            #region Creating opportunities in our DB for the first time or to be added new opportunities.
            List<clsOpportunities> GetOpportunities = JsonConvert.DeserializeObject<List<clsOpportunities>>(AgentService.GetOpportunityByNameorCode());
            int count = 0;
            foreach (clsOpportunities objOpportunities in GetOpportunities)
            {
                //    Opportunity objOpp = new Opportunity();
                //    objOpp.potentialname = objOpportunities.potentialname;
                //    objOpp.potential_no = objOpportunities.potential_no;
                //    objOpp.sales_stage = objOpportunities.sales_stage;

                //    db.Opportunities.Add(objOpp);
                //    db.SaveChanges();
                count++;
            }
            #endregion

            // Below code is not required at all and commented by Sarath on 21-Jul-2016 at 12.30pm
            //List<Opportunity> GetOpportunities = db.Opportunities.ToList();

            //TempData["AllOpportunities"] = GetOpportunities;
            //TempData.Keep("AllOpportunities");



            return Json(count, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetProjectSFilterAccount(string Account, string Practice_value, string cost, string Pur_pose)
        {
            try
            {
                JsonResult js = new JsonResult();
                string Role = (string)Session["Role"];  // PM, OM
                int empID = (int)Session["EmployeeId"]; //101098 Manger ID;

                if (Pur_pose.ToLower() != "project" && Pur_pose.ToLower() != "opportunity")
                {
                    var projectname = (from pCode in db.PracticeWiseBenchCodes
                                       join projt in db.Projects on pCode.BenchCode equals projt.ProjectCode
                                       //where pCode.Practice.ToLower() == Practice_value.ToLower() &&
                                       where pCode.CostCenter.ToLower() == cost.ToLower()
                                       select new
                                       {
                                           ProjectCode = projt.ProjectCode,
                                           ProjectName = projt.ProjectCode + "-" + projt.ProjectName
                                       }).Distinct().OrderBy(p => p.ProjectName).ToList();
                    ViewData["_Projects"] = projectname;
                    js.Data = projectname;

                }
                else
                {



                    var _AllProjectNames = (from projects in db.Projects.
                                           Where(p => p.IsActive == true && p.SOWEndDate >= DateTime.Today
                                           )
                                            select new
                                            {
                                                AccountName = projects.AccountName,
                                                ProjectName = projects.ProjectCode + "-" + projects.ProjectName,
                                                ProjectCode = projects.ProjectCode
                                            }).Distinct().OrderBy(p => p.ProjectName).ToList();

                    if (Account != null && Account != string.Empty)
                        _AllProjectNames = _AllProjectNames.Where(p => p.AccountName.ToLower() == Account.ToLower()).ToList();
                    ViewData["_Projects"] = _AllProjectNames;
                    js.Data = _AllProjectNames;
                }

                js.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
                return (js);
            }

            catch (Exception ex)
            {

                Common.WriteExceptionErrorLog(ex);
                return Json("Error", JsonRequestBehavior.AllowGet);
            }
        }
            
        

        public JsonResult GetTechPanelName(string Skill)
        {
            var techPanel = db.SKILLSCOREs.Where(x => x.SKILLS.ToLower() == Skill.ToLower()).FirstOrDefault();
            return Json(techPanel, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetPractices(String Purpose)
        {
            try
            {
                if (!string.IsNullOrEmpty(Purpose))
                {
                    var Practice = (from data in db.PracticeWiseBenchCodes.Where(x => x.Practice.ToLower() != "sg&a" && x.Practice.ToLower() != "business engagement")
                                    select new
                                    {
                                        LookupName = data.Practice
                                    }).Distinct().ToList().OrderBy(p => p.LookupName);

                    //var Practice = (from data in db.PracticeWiseBenchCodes.Where(x => x.Practice == "Analytics"
                    //                || x.Practice == "CIS" 
                    //                || x.Practice == "Digital Engineering"
                    //                || x.Practice == "Technology & Architecture Group")
                    //                select new
                    //                {
                    //                    LookupName = data.Practice
                    //                }).Distinct().ToList().OrderBy(p => p.LookupName);


                    ViewData["_Practice"] = Practice;
                    return Json(Practice, JsonRequestBehavior.AllowGet);

                }
                else
                {
                    return Json(string.Empty, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {

                Common.WriteExceptionErrorLog(ex);
                return Json("Error", JsonRequestBehavior.AllowGet);
            }
        }

          public ActionResult GetPractice()
          {
            try
            {
                    var Practice = (from data in db.PracticeWiseBenchCodes.Where(x => x.Practice.ToLower() != "sg&a" && x.Practice.ToLower() != "business engagement")
                                    select new
                                    {
                                        LookupName = data.Practice
                                    }).Distinct().ToList().OrderBy(p => p.LookupName);

                    return Json(Practice, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                Common.WriteExceptionErrorLog(ex);
                return Json("Error", JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult IsReplacemntExist(string replacementId, string trNo)
        {
            try
            {
                int data = 0;
                //if (replacementId.Substring(0, replacementId.IndexOf("-"))
                Char value = '-';
                Boolean result = replacementId.Contains(value);
                if (result)
                {
                    replacementId = replacementId.Substring(0, replacementId.IndexOf("-"));   // for getting full with empid and name
                }
                //string replaaceid = replacementId.Substring(0, replacementId.IndexOf("-"));
                long? id = 0;

                if (int.TryParse(replacementId, out int value1))
                {
                    id = value1;
                }               
                if (id != 106403) //hakeem to to skip this emp id
                {
                    if (id == 0)
                    {
                        data = 0;
                    }
                    else
                    {
                        if (trNo != null && trNo != string.Empty)
                        {
                            data = db.HRRFs.Where(p => p.RequestReason == "Replacement" && p.ReplacementEmpId == id && p.RequestStatus != "Fulfilled" && p.HRRFNumber != trNo && p.RequestStatus != "Cancelled").ToList().Count();
                        }
                        else //Added - && p.RequestStatus != "Cancelled" , to fix the issue for not able to create a TR for replacement, when already one is cancelled and trying to create another.
                        {
                            data = db.HRRFs.Where(p => p.RequestReason == "Replacement" && p.ReplacementEmpId == id && p.RequestStatus != "Fulfilled" && p.RequestStatus != "Cancelled").ToList().Count();
                        }
                    }

                }
                
                if (data > 0)
                {
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(false, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Common.WriteExceptionErrorLog(ex);
                return Json("Error", JsonRequestBehavior.AllowGet);
            }



        }

        //public JsonResult IsReplacemntExist(string replacementId, string trNo)
        //{
        //    try
        //    {
        //        int data = 0;
        //        //if (replacementId.Substring(0, replacementId.IndexOf("-"))
        //        Char value = '-';
        //        Boolean result = replacementId.Contains(value);
        //        if (result)
        //        {
        //            replacementId = replacementId.Substring(0, replacementId.IndexOf("-"));   // for getting full with empid and name
        //        }
        //        //string replaaceid = replacementId.Substring(0, replacementId.IndexOf("-"));

        //        long id = Convert.ToInt32(replacementId);
        //        if (id != 106403) //hakeem to to skip this emp id
        //        {
        //            if (trNo != null && trNo != string.Empty)
        //            {
        //                data = db.HRRFs.Where(p => p.RequestReason == "Replacement" && p.ReplacementEmpId == id && p.RequestStatus != "Fulfilled" && p.HRRFNumber != trNo && p.RequestStatus != "Cancelled").ToList().Count();
        //            }
        //            else //Added - && p.RequestStatus != "Cancelled" , to fix the issue for not able to create a TR for replacement, when already one is cancelled and trying to create another.
        //            {
        //                data = db.HRRFs.Where(p => p.RequestReason == "Replacement" && p.ReplacementEmpId == id && p.RequestStatus != "Fulfilled" && p.RequestStatus != "Cancelled").ToList().Count();
        //            }
        //        }

        //        if (data > 0)
        //        {
        //            return Json(true, JsonRequestBehavior.AllowGet);
        //        }
        //        else
        //        {
        //            return Json(false, JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Common.WriteExceptionErrorLog(ex);
        //        return Json("Error", JsonRequestBehavior.AllowGet);
        //    }

        //}

        public ActionResult SessionMessage()
        {
            try
            {
                if (Convert.ToInt32(Session["EmployeeId"]) > 0)
                {
                    return Json("true", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("Sessionexpired", JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {

                Common.WriteExceptionErrorLog(ex);
                return Json("Error", JsonRequestBehavior.AllowGet);
            }

        }

        //public ActionResult EXCHANGERATE(string billingDate)
        //{
        //    try
        //    {
        //        DateTime hs;
        //        if (billingDate.Contains("/"))
        //        {
        //            hs = DateTime.ParseExact(billingDate, "MM/dd/yyyy", null);
        //        }
        //        else
        //        {
        //            hs = Convert.ToDateTime(billingDate);
        //        }
        //        var obj = db.EXCHANGERATEs.FirstOrDefault();
        //        //var obj = db.EXCHANGERATEs.Where(a => a.RATEDATE.Value.Year == hs.Year).FirstOrDefault();  //From database year == my Billing date year
        //        return Json(obj, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        Common.WriteExceptionErrorLog(ex);
        //        return Json("Error", JsonRequestBehavior.AllowGet);
        //    }
        //}

        public ActionResult EXCHANGERATE()
        {

            try
            {
                var obj = db.EXCHANGERATEs.FirstOrDefault();
                return Json(obj, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                Common.WriteExceptionErrorLog(ex);
                return Json("Error", JsonRequestBehavior.AllowGet);
            }
        }

    }

    public class clsOpportunities
    {
        //public string potentialid { get; set; }
        public string potential_no { get; set; }
        public string potentialname { get; set; }
        public string sales_stage { get; set; }
    }
    public class SkillData
    {
        public int HRRFSkillId { get; set; }
        public int? SkillId { get; set; }
        public string Competency { get; set; }
        public string ExpertiseLevel { get; set; }
        public bool? IsMandatory { get; set; }
        public string SkillName { get; set; }
    }
    public class HRRFData
    {
        public long HRRFID { get; set; }
        public string HRRFNumber { get; set; }
        public string SkillCluster { get; set; }

        public decimal? BillRate { get; set; }

        public double? MAXSAL { get; set; }
        public string CSkill { get; set; }
        public string SkillCode { get; set; }
         public string  PrimaryPractice{get;set;}
        public string JDNotMatching { get; set; }
        public string ClusterDesc { get; set; }
        public string CLSTRJD { get; set; }
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
       // [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> AssignmentStartDate { get; set; }
      //  [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
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
      //  [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> BillingDate { get; set; }

        public Nullable<System.DateTime> InternalExpectedFulfilmentDate { get; set; }
        public string CancelReason { get; set; }
        public string DuplicateHRRFNo { get; set; }
        public string TECHPANEL { get; set; }
        public string SECONDTECHPANEL { get; set; }
        public string OpportunitySalestage { get; set; }
        public string RequisitionID { get; set; }
        public string Discipline { get; set; }
        public string RoleGroup { get; set; }
        public string CoreSkill { get; set; }
        public string IntermediateSkill { get; set; }
        public string AdvancedSkill { get; set; }
        public string SpecificPlatForm { get; set; }
        public string Certifications { get; set; }
        public  Nullable<bool> IsSpecialClient { get; set; }
        public Nullable<bool> IsIndustryDomainSkill { get; set; }
        public string SpecialClient { get; set; }
        public string IndustryDomainSkill { get; set; }
        public string ShiftTime { get; set; }
        public string OrganizationGroup { get; set; }
    }
}
