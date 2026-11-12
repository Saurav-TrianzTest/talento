using OfficeOpenXml;
using System;
using System.Web.Helpers;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using Microsoft.EntityFrameworkCore;
using System.IO;
//using System.DirectoryServices;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authentication;
using System.Threading.Tasks;
//using Trianz.Enterprise.Operations.EmployeeService;
using Trianz.Enterprise.Operations.Models;
using Trianz.Enterprise.Operations.General;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Session;
namespace Trianz.Enterprise.Operations.Controllers
{
    public class HomeController : Controller
    {

        string loginUserName = string.Empty;
        string userEmailAddress = string.Empty;
        TrianzOperationsEntities Db = new TrianzOperationsEntities();
        List<HRRF> objlst = new List<HRRF>();
        //ServiceAgent AgentService = new ServiceAgent();

        public ActionResult Index(string ddlPractise = "", string ddlMyRequest = "", string searchString = "")
        {
            try
            {
                //return RedirectToAction("undermaintenance", "Home");

                //Below single statement is added by Sarath, for security reason to access RoleMaster page.
                TempData["IsRoleMasterPageAccess"] = null;

                //Identifying prev page
                TempData["PrevPage"] = HttpContext.Request.Path;

                GetServiceLine();
				//   string name = "smiley.gupta"; //"vindya.darsi"; //"hemasree.koganti"; //"abdul.shaik"; //"chaitanya.kondaveeti"; //"pradeep.h"; //"sreevenkata.babu"; // "sudharani.a"; //"satish.katragadda"; //"lubna.shumaila"; //"abdulbari.indoor"; //"priyanka.ganapuram"; //user.identity.name.split('\\')[1].tolower();
				//ViewBag.username = name;

				//string name = User.Identity.Name.Split('\\')[1].ToLower();
				string usermail = Common.GetAzureLoggedInUserID();
				if (usermail != null)
                {

                    int EmployeeId = (from data in Db.Employees where (data.Email.Equals(usermail) && data.IsActive==true) select data.EmployeeId).FirstOrDefault();
                    if (EmployeeId != null)
                    {
                        Employee employee = (from data in Db.Employees.Where(x => x.EmployeeId == EmployeeId) select data).FirstOrDefault();
                        var RoleResults = Db.RoleMasters.ToList().Where(role => role.EmployeeId == employee.EmployeeId && role.ApplicationCode == "TALENTREQ").FirstOrDefault();
                        string RoleValues = ConfigurationManager.AppSettings["RoleManagement"];
                        string DefaultRole = ConfigurationManager.AppSettings["DefaultRole"];
                        List<string> lst = new List<string>();
                        lst = RoleValues.Split(',').ToList();
                        if (RoleResults != null)
                        {
                            HttpContext.Session.SetString("Role", RoleResults.Role.ToString());
                            //   int? Grade = 0; // employee.Grade;
                            // HttpContext.Session.SetString("Grade", Grade.ToString());
                            HttpContext.Session.SetString("Grade", employee.Grade.ToString());
                            HttpContext.Session.SetString("EmployeeId", employee.EmployeeId.ToString());
                            HttpContext.Session.SetString("Practice", RoleResults.Practice.ToString());
                        }
                        else
                        {
                            HttpContext.Session.SetString("Role", "PM".ToString()); //DefaultRole;
                            HttpContext.Session.SetString("Grade", employee.Grade.ToString());
                            HttpContext.Session.SetString("EmployeeId", employee.EmployeeId.ToString());
                            HttpContext.Session.SetString("Practice", employee.Practice.ToString());

                            //return RedirectToAction("AddNewSkills", "EmployeeSkills");
                        }

                        HttpContext.Session.SetString("Practices", ddlPractise.ToString());
                        HttpContext.Session.SetString("Request", ddlMyRequest.ToString());
                        HttpContext.Session.SetString("Search", searchString.ToString());

                        //if (HttpContext.Session.GetString("Role").ToString() == "RL")
                        //{
                        //    return RedirectToAction("Index", "ExternalReport");
                        //}
                        //else
                        //{
                        var validationModel = new List<ValidationModel>();
                        validationModel = GetParentChild(ddlMyRequest, ddlPractise, searchString.ToLower().Trim());

                        return View(validationModel);
                        //}
                    }
                }
                return RedirectToAction("Unavailable", "Home");
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                Exception raise = dbEx;
                // EF Core DbUpdateException does not have EntityValidationErrors - skip validation error iteration
                ErrorHandling expcls = new ErrorHandling();
                expcls.Error(raise);

                throw raise;
            }


        }

        public void GetServiceLine()
        {
            try
            {
                //var ServiceLine = (from serviceLine in Db.MasterLookUps.Where(x => x.LookupType == "ServiceLine")
                //                   select new
                //                   {
                //                       LookupName = serviceLine.LookupName,
                //                       Description = serviceLine.Description,
                //                       LookupCode = serviceLine.LookupCode,
                //                       SeqNumber = serviceLine.SeqNumber
                //                   }).OrderBy(p => p.LookupName).ToList();
                var ServiceLine = (from data in Db.PracticeWiseBenchCodes
                                   select new
                                   {
                                       LookupName = data.Practice,
                                   }).Distinct().ToList().OrderBy(p => p.LookupName);
                ViewData["ServiceLine"] = ServiceLine;
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                Exception raise = dbEx;
                // EF Core DbUpdateException does not have EntityValidationErrors - skip validation error iteration
                ErrorHandling expcls = new ErrorHandling();
                expcls.Error(raise);

                throw raise;
            }
        }

        public async Task SignOut()
        {
            try
            {
                HttpContext.Session.Clear();
                // Session.Abandon() is not available in ASP.NET Core
                // Session.RemoveAll() is not available in ASP.NET Core
                await HttpContext.SignOutAsync();
            }
            catch (Exception ex)
            {
                Redirect(ConfigurationManager.AppSettings["Talento"]);
            }

        }

        public void GetExcel(string flag)
        {
            try
            {
                var HrrfDetailsExport = new List<HRRF>();

                if (flag == null)
                {
                    string ddlPractise = string.Empty;
                    if (HttpContext.Session.GetString("Practices") != null)
                    {
                        ddlPractise = HttpContext.Session.GetString("Practices").ToString();
                    }

                    string ddlMyRequest = string.Empty;
                    if (HttpContext.Session.GetString("Request") != null)
                    {
                        ddlMyRequest = HttpContext.Session.GetString("Request").ToString();
                    }

                    string searchString = string.Empty;
                    if (HttpContext.Session.GetString("Search") != null)
                    {
                        searchString = HttpContext.Session.GetString("Search").ToString();
                    }

                    if (!string.IsNullOrEmpty(searchString.ToLower().Trim()))
                    {
                        HrrfDetailsExport = (from Hrrf in Db.HRRFs
                                             where (Hrrf.HRRFNumber == searchString) || (Hrrf.TRParent == searchString)
                                             select Hrrf).OrderByDescending(a => a.HRRFID).ToList();
                    }
                    else
                    {
                        int employeeId = Convert.ToInt32(HttpContext.Session.GetString("EmployeeId"));

                        switch (ddlMyRequest.ToLower())
                        {
                            case "myrequest":
                                if (ddlPractise == "")
                                {
                                    HrrfDetailsExport = (from Hrrf in Db.HRRFs
                                                         where (Hrrf.HRRFCreatedBy == employeeId)
                                                         select Hrrf).OrderByDescending(a => a.HRRFID).ToList();
                                }
                                else
                                {
                                    HrrfDetailsExport = (from Hrrf in Db.HRRFs
                                                         where (Hrrf.HRRFCreatedBy == employeeId && Hrrf.Practice == ddlPractise)
                                                         select Hrrf).OrderByDescending(a => a.HRRFID).ToList();
                                }
                                break;

                            case "all":
                                if (ddlPractise == "")
                                {
                                    HrrfDetailsExport = Db.HRRFs.OrderByDescending(a => a.HRRFID).ToList();
                                }
                                else
                                {
                                    HrrfDetailsExport = Db.HRRFs.Where(h => h.Practice.Equals(ddlPractise)).OrderByDescending(a => a.HRRFID).ToList();
                                }
                                break;

                            case "open":
                                if (ddlPractise == "")
                                {
                                    HrrfDetailsExport = Db.HRRFs.Where(Hrrf => Hrrf.RequestStatus.ToLower() != "cancelled"
                                                   && Hrrf.RequestStatus.ToLower() != "draft"
                                                   && Hrrf.RequestStatus.ToLower() != "terminated"
                                                   && Hrrf.RequestStatus.ToLower() != "fulfilled").OrderByDescending(a => a.HRRFID).ToList();
                                }
                                else
                                {
                                    HrrfDetailsExport = Db.HRRFs.Where(Hrrf => Hrrf.Practice.Equals(ddlPractise)
                                                   && Hrrf.RequestStatus.ToLower() != "cancelled"
                                                   && Hrrf.RequestStatus.ToLower() != "draft"
                                                   && Hrrf.RequestStatus.ToLower() != "terminated"
                                                   && Hrrf.RequestStatus.ToLower() != "fulfilled").OrderByDescending(a => a.HRRFID).ToList();
                                }
                                break;

                            default:
                                if (ddlPractise == "")
                                {
                                    HrrfDetailsExport = Db.HRRFs.Where(Hrrf => Hrrf.Isparent == true && Hrrf.HRRFCreatedBy == employeeId).OrderByDescending(a => a.HRRFID).ToList();
                                }
                                else
                                {
                                    HrrfDetailsExport = Db.HRRFs.Where(Hrrf => Hrrf.Isparent == true && Hrrf.HRRFCreatedBy == employeeId && Hrrf.Practice == ddlPractise).OrderByDescending(a => a.HRRFID).ToList();
                                }

                                break;
                        }
                    }
                }
                else if (flag != null && flag.ToLower() == "external")
                {
                    HrrfDetailsExport = Db.HRRFs.Where(h => h.RequestType.ToLower() == "external" && h.RequestStatus.ToLower() != "fulfilled").ToList();
                }


                List<HRRFExportToExcel> exportToExcel = new List<HRRFExportToExcel>();

                foreach (var item in HrrfDetailsExport)
                {
                    HRRFExportToExcel objHRRFExportToExcel = new HRRFExportToExcel();

                    objHRRFExportToExcel.HRRFID = item.HRRFID;
                    objHRRFExportToExcel.HRRFNumber = item.HRRFNumber;
                    objHRRFExportToExcel.OldHRRFNumber = item.OldHRRFNumber;
                    objHRRFExportToExcel.CreatedDate = Convert.ToDateTime(item.HRRFCreatedDate);
                    objHRRFExportToExcel.Ageing = (item.HRRFCreatedDate == null) ? 0 : ((DateTime.Today - Convert.ToDateTime(Convert.ToDateTime(item.HRRFCreatedDate).ToString("dd MMM yyyy"))).Days);
                    objHRRFExportToExcel.AgeingBucket = (objHRRFExportToExcel.Ageing < 30) ? "0 - 29 D" : (objHRRFExportToExcel.Ageing < 60) ? "30 - 59 D" : (objHRRFExportToExcel.Ageing < 90) ? "60 - 89 D" : ">=90 D";
                    objHRRFExportToExcel.Purpose = item.Purpose;
                    objHRRFExportToExcel.ProjectNameWithCode = (item.Purpose.ToLower() == "proactive hire" || item.Purpose.ToLower() == "opportunity") ? "" : item.ProjectName + " --- " + item.ProjectCode;
                    objHRRFExportToExcel.OpportunityCode = item.OpportunityCode;
                    objHRRFExportToExcel.OpportunityName = item.OpportunityName;
                    objHRRFExportToExcel.AccountName = item.AccountName;
                    objHRRFExportToExcel.ServiceLine = item.Practice;

                    objHRRFExportToExcel.RequestType = item.RequestType;

                    objHRRFExportToExcel.JobDescription = item.JobDescription;
                    objHRRFExportToExcel.Grade = (item.Grade == null) ? 0 : Convert.ToInt32(item.Grade);
                    objHRRFExportToExcel.RequestStatus = item.RequestStatus;

                    if (item.RequestType.ToLower() == "internal" && item.RequestStatus.ToLower() == "fulfilled")
                    {
                        ProposeAssociate objPropAssociate = Db.ProposeAssociates.Where(pa => pa.HRRFNumber == item.HRRFNumber && pa.PracticeStatus.ToLower() == "accepted").FirstOrDefault();

                        if (objPropAssociate != null)
                        {
                            objHRRFExportToExcel.EmployeeId = objPropAssociate.EmpID;
                            if (objPropAssociate.ApprovedDate != null)
                            {
                                objHRRFExportToExcel.FulfillmentDate = Convert.ToDateTime(objPropAssociate.ApprovedDate);
                            }
                        }
                    }


                    objHRRFExportToExcel.LocationName = item.LocationName;
                    objHRRFExportToExcel.Positions = item.Positions;
                    objHRRFExportToExcel.ResourceName = item.ResourceName;
                    if (item.AssignmentStartDate != null)
                    {
                        objHRRFExportToExcel.AssignmentStartDate = Convert.ToDateTime(item.AssignmentStartDate);
                        if (item.RequestStatus.ToLower() != "cancelled" && item.RequestStatus.ToLower() != "draft"
                            && item.RequestStatus.ToLower() != "terminated" && item.RequestStatus.ToLower() != "fulfilled")
                        {
                            DateTime dtAssignmentStartDate = Convert.ToDateTime(item.AssignmentStartDate);
                            DateTime dtCurrentDate = DateTime.Today;

                            int result = DateTime.Compare(dtCurrentDate, dtAssignmentStartDate);

                            if (result > 0)
                            {
                                objHRRFExportToExcel.OverDue = (dtCurrentDate - dtAssignmentStartDate).Days.ToString() + " day(s)";
                            }
                            else
                            {
                                objHRRFExportToExcel.OverDue = "0 day(s)";
                            }
                        }
                    }
                    else
                    {
                        //objHRRFExportToExcel.AssignmentStartDate = "";
                        objHRRFExportToExcel.OverDue = "0 days(s)";
                    }
                    objHRRFExportToExcel.DemandType = item.DemandType;
                    objHRRFExportToExcel.RequestReason = item.RequestReason;

                    bool isNumeric = false;
                    if (item.LocationType != null)
                    {
                        int n;
                        isNumeric = int.TryParse(item.LocationType, out n);

                        if (isNumeric)
                        {
                            int LocationTypeID = Convert.ToInt32(item.LocationType);
                            objHRRFExportToExcel.LocationType = Db.MasterLookUps.Where(ml => ml.LookupType.Equals("LocationType") && ml.SeqNumber.Equals(LocationTypeID)).FirstOrDefault().LookupName;
                        }
                        else
                        {
                            objHRRFExportToExcel.LocationType = item.LocationType;
                        }
                    }


                    DesignationMaster objDesignationMaster = Db.DesignationMasters.Where(dm => dm.Practice.Contains(item.Practice) && dm.Grade == item.Grade && dm.DesignationCode == item.RoleRequired).FirstOrDefault();
                    if (objDesignationMaster != null)
                    {
                        objHRRFExportToExcel.RoleRequired = objDesignationMaster.DesignationName;
                    }
                    else
                    {
                        objHRRFExportToExcel.RoleRequired =item.RoleRequired;
                    }

                    Int32 empID = Convert.ToInt32(item.HRRFCreatedBy);
                    Employee emp = Db.Employees.Where(e => e.EmployeeId.Equals(empID)).FirstOrDefault();

                    if (emp != null)
                    {
                        objHRRFExportToExcel.CostCenter = item.CostCenter;
                        objHRRFExportToExcel.HRRFCreatedById = item.HRRFCreatedBy;
                        objHRRFExportToExcel.HRRFCreatedByName = emp.FirstName + " " + emp.LastName;
                    }
                    else
                    {
                        objHRRFExportToExcel.CostCenter = "";
                        objHRRFExportToExcel.HRRFCreatedById = 0;
                        objHRRFExportToExcel.HRRFCreatedByName = "";
                    }

                    var strPrimarySkills = (from skills in Db.HRRFSkills
                                            join mLookup in Db.MasterLookUps on skills.Skills equals mLookup.LookupID
                                            where mLookup.LookupType == "PrimarySkills" && skills.HRRFNumber == item.HRRFNumber && skills.IsPrimary == true
                                            select new
                                            {
                                                mLookup.ParentCode,
                                                mLookup.LookupName
                                            }).ToList();

                    if (strPrimarySkills.Count > 0)
                    {
                        Int32 seqNum = Convert.ToInt32(strPrimarySkills[0].ParentCode);
                        var querySkillCategory = Db.MasterLookUps.Where(m => m.SeqNumber.Equals(seqNum) && m.LookupType.Equals("SkillCategory")).FirstOrDefault();

                        objHRRFExportToExcel.SkillCategory = querySkillCategory.LookupName;
                    }
                    else
                    {
                        objHRRFExportToExcel.SkillCategory = "";
                    }

                    foreach (var primary in strPrimarySkills)
                    {
                        objHRRFExportToExcel.PrimarySkillSet += primary.LookupName + "; ";
                    }

                    if (objHRRFExportToExcel.PrimarySkillSet != null)
                    {
                        objHRRFExportToExcel.PrimarySkillSet = objHRRFExportToExcel.PrimarySkillSet.TrimEnd(new char[] { ' ', ';' });
                    }
                    else
                    {
                        objHRRFExportToExcel.PrimarySkillSet = "";
                    }

                    ExternalHire objExternalHire = Db.ExternalHires.Where(eh => eh.HRRFNumber.Equals(item.HRRFNumber)).OrderByDescending(eh => eh.ExternalHireId).FirstOrDefault();
                    if (objExternalHire != null)
                    {
                        if (objExternalHire.FulfilmentDate != null)
                        {
                            objHRRFExportToExcel.ExpectedFulfilmentDate = Convert.ToDateTime(objExternalHire.FulfilmentDate);
                        }
                        //objHRRFExportToExcel.ExpectedFulfilmentDate = (objExternalHire.FulfilmentDate != null) ? Convert.ToDateTime(objExternalHire.FulfilmentDate).ToString("dd/MM/yyyy") : "";
                        objHRRFExportToExcel.FulfillmentRemarks = objExternalHire.FulfilmentRemarks;
                        if (objExternalHire.DOJ != null)
                        {
                            objHRRFExportToExcel.DOJ = Convert.ToDateTime(objExternalHire.DOJ);
                        }

                        // objHRRFExportToExcel.DOJ = (objExternalHire.DOJ != null) ? Convert.ToDateTime(objExternalHire.DOJ).ToString("dd-MMM-yyyy") : "";
                        objHRRFExportToExcel.JoiningMonth = (objExternalHire.DOJ != null) ? System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Convert.ToDateTime(objExternalHire.DOJ).Month) : "";
                        if (objExternalHire.CancelDate != null)
                        {
                            objHRRFExportToExcel.CancelDate = Convert.ToDateTime(objExternalHire.CancelDate);
                        }

                        objHRRFExportToExcel.CancelRemarks = objExternalHire.CancellationReason;

                        if (item.RequestStatus != null)
                        {
                            if (item.RequestType.ToLower().Contains("external") && item.RequestStatus.ToLower() == "fulfilled")
                            {
                                if (objExternalHire.RequestStatus != null)
                                {
                                    if (objExternalHire.RequestStatus.ToLower() == "fulfilled")
                                    {
                                        if (objExternalHire.DOJ != null)
                                        {
                                            objHRRFExportToExcel.FulfillmentDate = Convert.ToDateTime(objExternalHire.DOJ);
                                        }
                                        objHRRFExportToExcel.EmployeeId = objExternalHire.EmployeeId.ToString();
                                    }
                                }
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(objExternalHire.RecruiterName))
                        {
                            Employee objRecruiterDetails = Db.Employees.Find(Convert.ToInt32(objExternalHire.RecruiterName));
                            if (objRecruiterDetails != null)
                            {
                                objHRRFExportToExcel.RecruiterName = objRecruiterDetails.FirstName + " " + objRecruiterDetails.LastName;
                            }
                        }
                    }

                    List<HRRFHistory> objHRRFHistory = Db.HRRFHistories.Where(eh => eh.HRRFNumber.Equals(item.HRRFNumber)).ToList();
                    if (objHRRFHistory != null)
                    {
                        HRRFHistory history1 = objHRRFHistory.Where(ie => ie.HistoryDescription.ToLower().Equals(item.HRRFNumber.ToLower() + " - has been converted to external")).FirstOrDefault();
                        if (history1 != null && history1.ModifiedDate != null)
                        {
                            objHRRFExportToExcel.DateFromIntToExt = Convert.ToDateTime(history1.ModifiedDate);
                        }

                        HRRFHistory history2 = objHRRFHistory.Where(ie => ie.HistoryDescription.ToLower().Equals(item.HRRFNumber.ToLower() + " - has been converted to hold")).FirstOrDefault();
                        if (history2 != null && history2.ModifiedDate != null)
                        {
                            objHRRFExportToExcel.DateOfHold = Convert.ToDateTime(history2.ModifiedDate);
                        }
                    }
                    else
                    {
                        objHRRFExportToExcel.DateFromIntToExt = null;
                        objHRRFExportToExcel.DateOfHold = null;
                    }

                    exportToExcel.Add(objHRRFExportToExcel);
                }

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
                    worksheet.Cells[1, 39].Value = "Cancel Remarks";
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
                    
                    Response.Headers.Add("content-disposition", "attachment;filename=TR-Details" + DateTime.Now.ToString("_dd-MMM-yyyy_hh-mm-ss") + ".xlsx");

                    
                    Response.ContentType = "application/vnd.ms-excel";
                    StringWriter sw = new StringWriter();
                    Response.Body.Write(fileBytes);
                    
                }

                #endregion

                #region Commented Code
                //if (string.IsNullOrEmpty(searchString))
                //{
                //    if (ddlMyRequest == "ALL" || !string.IsNullOrEmpty(ddlPractise) && (ddlMyRequest != "MyRequest"))
                //    {
                //        if (ddlPractise == "")
                //        {
                //            ddlPractise = null;
                //        }
                //        HrrfDetailsExport = (from Hrrf in Db.HRRFs
                //                             where (Hrrf.Practice == ddlPractise || ddlPractise == null)
                //                             select Hrrf).OrderByDescending(a => a.HRRFID).ToList();

                //    }
                //    else
                //    {
                //        if (ddlPractise == "")
                //        {
                //            ddlPractise = null;
                //        }
                //        int employeeId = Convert.ToInt32(HttpContext.Session.GetString("EmployeeId"));


                //        HrrfDetailsExport = (from Hrrf in Db.HRRFs
                //                             where (Hrrf.HRRFCreatedBy == employeeId) &&
                //                                 (Hrrf.Practice == ddlPractise || ddlPractise == null)
                //                             select Hrrf).OrderByDescending(a => a.HRRFID).ToList();
                //    }
                //}
                //else
                //{
                //    if (searchString == "")
                //    {
                //        searchString = null;
                //    }
                //    HrrfDetailsExport = (from Hrrf in Db.HRRFs
                //                         where (Hrrf.HRRFNumber == searchString) || (Hrrf.TRParent == searchString)
                //                         select Hrrf).OrderByDescending(a => a.HRRFID).ToList();

                //} 
                #endregion

                //WebGrid grid = new WebGrid(source: exportToExcel, canPage: false, canSort: false);
                //string gridData = grid.GetHtml(
                //                        columns: grid.Columns
                //                        (   
                //                            //grid.Column("HRRFID", "HRRFID"),
                //                            grid.Column("HRRFNumber", "HRRF"),
                //                            grid.Column("OldHRRFNumber", "Old HRRF Number"),
                //                            grid.Column("CreatedDate", "Created Date"),
                //                            grid.Column("Ageing", "Ageing"),
                //                            grid.Column("AgeingBucket", "Ageing Bucket"),
                //                            grid.Column("Purpose", "Purpose"),
                //                            grid.Column("ProjectNameWithCode", "Project Name with Code"),
                //                            grid.Column("OpportunityCode", "Opportunity Code"),
                //                            grid.Column("OpportunityName", "Opportunity Name"),
                //                            grid.Column("AccountName", "Account Name"),
                //                            grid.Column("ServiceLine", "Practice"),
                //                            grid.Column("CostCenter", "Cost Center"),
                //                            grid.Column("RequestType", "Request Type"),
                //                            grid.Column("SkillCategory", "Skill Category"),
                //                            grid.Column("PrimarySkillSet", "Primary Skill Set"),
                //                            grid.Column("JobDescription", "Job Description"),
                //                            grid.Column("Grade"),
                //                            grid.Column("RequestStatus", "RequestStaus"),
                //                            grid.Column("FulfillmentRemarks", "Fulfillment Remarks"),
                //                            grid.Column("LocationType", "Location Type"),
                //                            grid.Column("LocationName", "Location Name"),
                //                            grid.Column("Positions"),
                //                            grid.Column("ResourceName", "Resource Name"),
                //                            grid.Column("AssignmentStartDate", "Assignment Start Date"),
                //                            grid.Column("DemandType", "Demand Type"),
                //                            grid.Column("RequestReason", "Request Reason"),
                //                            grid.Column("RoleRequired", "Role Required"),
                //                            grid.Column("HRRFCreatedById", "Created By EmpID"),
                //                            grid.Column("HRRFCreatedByName", "Created By EmpName"),
                //                            grid.Column("DOJ", "Date of Joining"),
                //                            grid.Column("JoiningMonth", "Joining Month"),
                //                            grid.Column("DateFromIntToExt", "Date from Internal to External"),
                //                            grid.Column("DateOfHold", "Date of Hold")
                //                       )).ToString();
                //Response.Clear();
                //Response.Headers.Add("content-disposition", "attachment; filename=HRRFInfo" + DateTime.Now.ToString("_dd-MMM-yyyy_hh-mm-ss") + ".xls");
                //Response.ContentType = "application/excel";
                //Response.WriteAsync(gridData);
                //
            }

            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                Exception raise = dbEx;
                // EF Core DbUpdateException does not have EntityValidationErrors - skip validation error iteration
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
                Response.Clear();
                Response.Headers.Add("content-disposition", "attachment; filename=HRRFInfo.xls");
                Response.ContentType = "application/excel";
                Response.WriteAsync(gridData);
                
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                Exception raise = dbEx;
                // EF Core DbUpdateException does not have EntityValidationErrors - skip validation error iteration
                ErrorHandling expcls = new ErrorHandling();
                expcls.Error(raise);
                throw raise;
            }

        }

        public List<ValidationModel> GetParentChild(string ddlMyRequest, string ddlPractise, string searchString)
        {
            try
            {
                List<ValidationModel> objDetails = new List<ValidationModel>();
                if (string.IsNullOrEmpty(searchString))
                {
                    if (ddlMyRequest == "ALL") // || !string.IsNullOrEmpty(ddlPractise) && (ddlMyRequest != "MyRequest"))
                    {
                        if (ddlPractise == "")
                        {
                            ddlPractise = null;
                        }
                        var HRRFsParent = (from Hrrf in Db.HRRFs
                                           where (Hrrf.Isparent == true) && (Hrrf.Practice == ddlPractise || ddlPractise == null)
                                           select Hrrf).OrderByDescending(a => a.HRRFID);
                        if (HRRFsParent != null)
                        {
                            foreach (var TRParents in HRRFsParent)
                            {
                                var childsrecords = Db.HRRFs.Where(a => a.TRParent.Equals(TRParents.TRParent) && a.Isparent == false).ToList();

                                objDetails.Add(new ValidationModel { HRRFParentChild = TRParents, HRRFChild = childsrecords });
                                string strPractice = TRParents.Practice;
                                int? intGrade = TRParents.Grade;
                                string strRolerequired = TRParents.RoleRequired;
                                DesignationMaster objDesignationMaster = Db.DesignationMasters.Where(dm => dm.Practice.Contains(strPractice) && dm.Grade == intGrade && dm.DesignationCode == strRolerequired).FirstOrDefault();

                                if (objDesignationMaster != null)
                                {
                                    TRParents.RoleRequired = objDesignationMaster.DesignationName;
                                }
                                else
                                {
                                    TRParents.RoleRequired = strRolerequired;
                                }
                                if (TRParents.Purpose.ToLower() == "opportunity")
                                {
                                    TRParents.ProjectName = "Opportunity: " + TRParents.OpportunityName;
                                }
                                else if (TRParents.Purpose.ToLower() == "project")
                                {
                                    TRParents.ProjectName = "Project: " + TRParents.ProjectName;
                                }
                                else if (TRParents.Purpose.ToLower() == "proactive hire")
                                {
                                    TRParents.ProjectName = "Proactive Hire";
                                }
                            }

                        }
                    }
                    else if (ddlMyRequest.ToLower() == "open")
                    {
                        if (!string.IsNullOrEmpty(ddlPractise))
                        {
                            var HRRFsParent = (from Hrrf in Db.HRRFs
                                               where (Hrrf.Isparent == true)
                                               && (Hrrf.Practice == ddlPractise)
                                               && (Hrrf.RequestStatus.ToLower() != "cancelled")
                                               && (Hrrf.RequestStatus.ToLower() != "draft")
                                               && (Hrrf.RequestStatus.ToLower() != "terminated")
                                               && (Hrrf.RequestStatus.ToLower() != "fulfilled")
                                               select Hrrf).OrderByDescending(a => a.HRRFID);

                            if (HRRFsParent != null)
                            {
                                foreach (var TRParents in HRRFsParent)
                                {
                                    var childsrecords = Db.HRRFs.Where(a => a.TRParent.Equals(TRParents.TRParent) && a.Isparent == false).ToList();

                                    objDetails.Add(new ValidationModel { HRRFParentChild = TRParents, HRRFChild = childsrecords });
                                    string strPractice = TRParents.Practice;
                                    int? intGrade = TRParents.Grade;
                                    string strRolerequired = TRParents.RoleRequired;
                                    DesignationMaster objDesignationMaster = Db.DesignationMasters.Where(dm => dm.Practice.Contains(strPractice) && dm.Grade == intGrade && dm.DesignationCode == strRolerequired).FirstOrDefault();

                                    if (objDesignationMaster != null)
                                    {
                                        TRParents.RoleRequired = objDesignationMaster.DesignationName;
                                    }
                                    else
                                    {
                                        TRParents.RoleRequired = strRolerequired;
                                    }
                                    if (TRParents.Purpose.ToLower() == "opportunity")
                                    {
                                        TRParents.ProjectName = "Opportunity: " + TRParents.OpportunityName;
                                    }
                                    else if (TRParents.Purpose.ToLower() == "project")
                                    {
                                        TRParents.ProjectName = "Project: " + TRParents.ProjectName;
                                    }
                                    else if (TRParents.Purpose.ToLower() == "proactive hire")
                                    {
                                        TRParents.ProjectName = "Proactive Hire";
                                    }
                                }

                            }
                        }
                        else
                        {
                            var HRRFsParent = (from Hrrf in Db.HRRFs
                                               where (Hrrf.Isparent == true)
                                               && (Hrrf.RequestStatus.ToLower() != "cancelled")
                                               && (Hrrf.RequestStatus.ToLower() != "draft")
                                               && (Hrrf.RequestStatus.ToLower() != "terminated")
                                               && (Hrrf.RequestStatus.ToLower() != "fulfilled")
                                               select Hrrf).OrderByDescending(a => a.HRRFID);

                            if (HRRFsParent != null)
                            {
                                foreach (var TRParents in HRRFsParent)
                                {
                                    var childsrecords = Db.HRRFs.Where(a => a.TRParent.Equals(TRParents.TRParent) && a.Isparent == false).ToList();

                                    objDetails.Add(new ValidationModel { HRRFParentChild = TRParents, HRRFChild = childsrecords });
                                    string strPractice = TRParents.Practice;
                                    int? intGrade = TRParents.Grade;
                                    string strRolerequired = TRParents.RoleRequired;
                                    DesignationMaster objDesignationMaster = Db.DesignationMasters.Where(dm => dm.Practice.Contains(strPractice) && dm.Grade == intGrade && dm.DesignationCode == strRolerequired).FirstOrDefault();

                                    if (objDesignationMaster != null)
                                    {
                                        TRParents.RoleRequired = objDesignationMaster.DesignationName;
                                    }
                                    else
                                    {
                                        TRParents.RoleRequired = strRolerequired;
                                    }
                                    if (TRParents.Purpose.ToLower() == "opportunity")
                                    {
                                        TRParents.ProjectName = "Opportunity: " + TRParents.OpportunityName;
                                    }
                                    else if (TRParents.Purpose.ToLower() == "project")
                                    {
                                        TRParents.ProjectName = "Project: " + TRParents.ProjectName;
                                    }
                                    else if (TRParents.Purpose.ToLower() == "proactive hire")
                                    {
                                        TRParents.ProjectName = "Proactive Hire";
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (ddlPractise == "")
                        {
                            ddlPractise = null;
                        }
                        int employeeId = Convert.ToInt32(HttpContext.Session.GetString("EmployeeId"));
                        var HRRFsParent = (from Hrrf in Db.HRRFs
                                           where Hrrf.Isparent == true &&
                                           (Hrrf.HRRFCreatedBy == employeeId) &&
                                           (Hrrf.Practice == ddlPractise || ddlPractise == null)
                                           select Hrrf).OrderByDescending(a => a.HRRFID);
                        if (HRRFsParent != null)
                        {
                            foreach (var TRParents in HRRFsParent)
                            {
                                var childsrecords = Db.HRRFs.Where(a => a.TRParent.Equals(TRParents.TRParent) && a.Isparent == false).ToList();
                                objDetails.Add(new ValidationModel { HRRFParentChild = TRParents, HRRFChild = childsrecords });
                                string strPractice = TRParents.Practice;
                                int? intGrade = TRParents.Grade;

                                string strRolerequired = TRParents.RoleRequired;
                                DesignationMaster objDesignationMaster = Db.DesignationMasters.Where(dm => dm.Practice.Contains(strPractice) && dm.Grade == intGrade && dm.DesignationCode == strRolerequired).FirstOrDefault();

                                if (objDesignationMaster != null)
                                {
                                    TRParents.RoleRequired = objDesignationMaster.DesignationName;
                                }
                                else
                                {
                                    TRParents.RoleRequired = strRolerequired;
                                }
                                if (TRParents.Purpose.ToLower() == "opportunity")
                                {
                                    TRParents.ProjectName = "Opportunity: " + TRParents.OpportunityName;
                                }
                                else if (TRParents.Purpose.ToLower() == "project")
                                {
                                    TRParents.ProjectName = "Project: " + TRParents.ProjectName;
                                }
                                else if (TRParents.Purpose.ToLower() == "proactive hire")
                                {
                                    TRParents.ProjectName = "Proactive Hire";
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (searchString == "")
                    {
                        searchString = null;
                    }
                    var check = Db.HRRFs.Where(h => h.HRRFNumber.Contains(searchString)).FirstOrDefault(); //(from tr in Db.HRRFs where tr.HRRFNumber == searchString select tr).Single();

                    //if tr is parent
                    if (check != null)
                    {
                        if (check.Isparent == true)
                        {
                            var HRRFsParent = (from Hrrf in Db.HRRFs
                                               where Hrrf.Isparent == true &&
                                               Hrrf.HRRFNumber.Contains(searchString)
                                               select Hrrf).OrderByDescending(a => a.HRRFID);
                            foreach (var TRParents in HRRFsParent)
                            {

                                var childsrecords = Db.HRRFs.Where(a => a.TRParent.Equals(TRParents.TRParent) && a.Isparent == false).ToList();
                                objDetails.Add(new ValidationModel { HRRFParentChild = TRParents, HRRFChild = childsrecords });

                                string strPractice = TRParents.Practice;
                                int? intGrade = TRParents.Grade;
                                string strRolerequired = TRParents.RoleRequired;
                                DesignationMaster objDesignationMaster = Db.DesignationMasters.Where(dm => dm.Practice.Contains(strPractice) && dm.Grade == intGrade && dm.DesignationCode == strRolerequired).FirstOrDefault();

                                if (objDesignationMaster != null)
                                {
                                    TRParents.RoleRequired = objDesignationMaster.DesignationName;
                                }
                                else
                                {
                                    TRParents.RoleRequired = strRolerequired;
                                }
                                if (TRParents.Purpose.ToLower() == "opportunity")
                                {
                                    TRParents.ProjectName = "Opportunity: " + TRParents.OpportunityName;
                                }
                                else if (TRParents.Purpose.ToLower() == "project")
                                {
                                    TRParents.ProjectName = "Project: " + TRParents.ProjectName;
                                }
                                else if (TRParents.Purpose.ToLower() == "proactive hire")
                                {
                                    TRParents.ProjectName = "Proactive Hire";
                                }
                            }
                        } //end if
                        else
                        {
                            //child logic
                            var HRRFsChild = (from Hrrf in Db.HRRFs
                                              where Hrrf.HRRFNumber.Contains(searchString)
                                              select Hrrf).OrderByDescending(a => a.HRRFID).ToList();

                            foreach (var TRParents in HRRFsChild)
                            {
                                List<HRRF> child = new List<HRRF>();
                                var childsrecords = child.ToList();
                                objDetails.Add(new ValidationModel { HRRFParentChild = TRParents, HRRFChild = childsrecords });

                                string strPractice = TRParents.Practice;
                                int? intGrade = TRParents.Grade;
                                string strRolerequired = TRParents.RoleRequired;
                                DesignationMaster objDesignationMaster = Db.DesignationMasters.Where(dm => dm.Practice.Contains(strPractice) && dm.Grade == intGrade && dm.DesignationCode == strRolerequired).FirstOrDefault();

                                if (objDesignationMaster != null)
                                {
                                    TRParents.RoleRequired = objDesignationMaster.DesignationName;
                                }
                                else
                                {
                                    TRParents.RoleRequired = strRolerequired;
                                }
                                if (TRParents.Purpose.ToLower() == "opportunity")
                                {
                                    TRParents.ProjectName = "Opportunity: " + TRParents.OpportunityName;
                                }
                                else if (TRParents.Purpose.ToLower() == "project")
                                {
                                    TRParents.ProjectName = "Project: " + TRParents.ProjectName;
                                }
                                else if (TRParents.Purpose.ToLower() == "proactive hire")
                                {
                                    TRParents.ProjectName = "Proactive Hire";
                                }
                            }
                        }
                    }
                    //end else
                }
                return objDetails;
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                Exception raise = dbEx;
                // EF Core DbUpdateException does not have EntityValidationErrors - skip validation error iteration
                ErrorHandling expcls = new ErrorHandling();
                expcls.Error(raise);

                throw raise;
            }
        }

        public JsonResult GetAllHrrfNum(string name = "")
        {
            try
            {
                string Name = name.ToUpper();
                var HRRFDetails = from e in Db.HRRFs select e;

                foreach (var role in HRRFDetails)
                {
                    HRRF objRoleMasters = new HRRF();
                    objRoleMasters.HRRFID = role.HRRFID;
                    objRoleMasters.HRRFNumber = role.HRRFNumber.ToUpper();

                    objlst.Add(objRoleMasters);
                }
                objlst = objlst.Where(s => Convert.ToString(s.HRRFNumber).StartsWith(Name)).ToList();

                return Json(objlst);
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                Exception raise = dbEx;
                // EF Core DbUpdateException does not have EntityValidationErrors - skip validation error iteration
                ErrorHandling expcls = new ErrorHandling();
                expcls.Error(raise);

                throw raise;
            }
        }

        //public ActionResult LogOff()
        //{
        //    HttpContext.Session.SetString("Role", null.ToString()); //it's my session variable
        //    Session.Clear();
        //    Session.Abandon();
        //    ViewBag.state = "expired";
        //    //FormsAuthentication.SignOut(); //you write this when you use FormsAuthentication
        //    return View("SessionExpire_Msg");
        //    /* return RedirectToAction("Login", "Account");*/// redirect to page that states "Session Expired.Please re login to continue".
        //}

        public ActionResult undermaintenance()
        {
            return View();
        }

        public ActionResult Unavailable()
        {
            return View();
        }
    }
}
