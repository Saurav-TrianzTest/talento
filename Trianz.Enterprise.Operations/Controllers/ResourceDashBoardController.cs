using OfficeOpenXml;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web.Helpers;
using System.Configuration;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;
using Trianz.Enterprise.Operations.General;
using Trianz.Enterprise.Operations.Models;
using Trianz.Enterprise.Operations.ViewModel;
using OfficeOpenXml.Style;

namespace Trianz.Enterprise.Operations.Controllers
{
    public class ResourceDashBoardController : Controller
    {
        private TrianzOperationsEntities db = new TrianzOperationsEntities();

        public ActionResult Index()
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
        public ActionResult ResourceInformation()
        {
            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            {
                try {
                    //Below single statement is added by Sarath, for security reason to access RoleMaster page.
                    TempData["IsRoleMasterPageAccess"] = null;

                    List<HRRF> allHRRFS = new List<HRRF>();

                    using (TrianzOperationsEntities dc = new TrianzOperationsEntities())
                    {
                        allHRRFS = dc.HRRFs.ToList();
                    }

                    TempData["abc"] = allHRRFS;

                    //*Pie Chart Data*//

                    List<Employee> TotalEmployees = db.Employees.ToList();
                    List<BenchEmployee> TotalBenchEmployees = db.BenchEmployees.ToList();
                    List<HRRF> TotalDemands = db.HRRFs.ToList();

                    var countofEmployees = (from EmployeesCount in TotalEmployees.AsEnumerable()

                                            select EmployeesCount).Count();
                    var countofBenchEmployees = (from BenchEmployeesCount in TotalBenchEmployees.AsEnumerable()

                                                 select BenchEmployeesCount).Count();

                    var DemandCount = (from TRCount in TotalDemands.AsEnumerable()
                                       select TRCount).Count();

                    //*Bar Chart Data*//

                    var PracticeNames = (from e in db.HRRFs
                                         where e.Practice != null
                                         group e by e.Practice into newGroup
                                         orderby newGroup.Key
                                         select newGroup).ToList();

                    var EmployessByPractice = (from e in db.Employees
                                               where e.AssignmentStatus == "Non-Billed"
                                               group e by e.Practice into newGroup
                                               orderby newGroup.Key
                                               select newGroup).ToList();


                    TempData["123"] = ChartData.GetLineAreaChartData(PracticeNames, EmployessByPractice);


                    return View(ChartData.GetPieChartData(countofEmployees, countofBenchEmployees, DemandCount));
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
        public void ExporttoExcel()
        {         
            List<HRRF> allHRRFS = new List<HRRF>();
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
              grid.Column("LocationType", "Location")

            )
            ).ToString();
            Response.ClearContent();
            Response.AddHeader("content-disposition", "attachment; filename=HRRFInfo.xls");
            Response.ContentType = "application/excel";
            Response.Write(gridData);
            Response.End();

        }

           

        public ActionResult ITSResourceDashboard()
        {
            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            {
                try
                {
                    int empId = Convert.ToInt32(Session["EmployeeId"]);
            int? DeliveryManagerId = 0;
            DeliveryManagerId = (from P in db.Projects
                                 where P.DELIVERY_MANAGER_ID == empId && P.IsActive == true
                                 select P.DELIVERY_MANAGER_ID).FirstOrDefault();
            List<string> dashBoardViewers = Environment.GetEnvironmentVariable("ITSDashboardViewers") ?? "".Split(',').ToList();

            //bool IsValid1 = lstAccessRoles.Where(e => empId.Contains(e)).Any();
            bool flag = dashBoardViewers.Contains(empId.ToString());
            bool IsValid = false;
            if ((DeliveryManagerId != 0 && DeliveryManagerId != null) || flag)
                IsValid = true;
            if (!IsValid)
            {
                return RedirectToAction("NotAuthorized", "Error");
            }
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
              //  return Json("Sessionexpired", JsonRequestBehavior.AllowGet);
                return RedirectToAction("SessionExpire", "Signout");
            }
        }

      
        public ActionResult GetDashBoardDetails()
        {
            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            {
                var emp = new ValidationModel();
                emp.ITSDashboard = GenerateITSReport();
                var summary = GenerateITSReportSummary();
                
                emp.ITDashBoardDetails = new Dictionary<string, List<ITDashBoardDetailsSummary>>();
                foreach (var group in summary)
                {
                    if (emp.ITDashBoardDetails.ContainsKey(group.OrgGroup))
                    {
                        var current = emp.ITDashBoardDetails[group.OrgGroup];
                        current.Add(group);
                        emp.ITDashBoardDetails[group.OrgGroup] = current;
                    }
                    else
                    {
                        var currentvalue = new List<ITDashBoardDetailsSummary>();
                        currentvalue.Add(group);
                        emp.ITDashBoardDetails.Add(group.OrgGroup, currentvalue);
                    }
                }

                //var previousweeksdata = GeneratePreviousWeeksReport();
                //var currentweekdata = GenerateCurrentWeeksReport();
                //var nextweekdata = GenerateNextWeeksReport();

                //emp.PreviousWeekDashBoardDetails = new Dictionary<string, List<ITDashBoardDetailsSummary>>();
                //foreach (var group in previousweeksdata)
                //{
                //    if (emp.PreviousWeekDashBoardDetails.ContainsKey(group.OrgGroup))
                //    {
                //        var current = emp.PreviousWeekDashBoardDetails[group.OrgGroup];
                //        current.Add(group);
                //        emp.PreviousWeekDashBoardDetails[group.OrgGroup] = current;
                //    }
                //    else
                //    {
                //        var currentvalue = new List<ITDashBoardDetailsSummary>();
                //        currentvalue.Add(group);
                //        emp.PreviousWeekDashBoardDetails.Add(group.OrgGroup, currentvalue);
                //    }
                //}


                //emp.CurrentWeekDashBoardDetails = new Dictionary<string, List<ITDashBoardDetailsSummary>>();
                //foreach (var group in currentweekdata)
                //{
                //    if (emp.CurrentWeekDashBoardDetails.ContainsKey(group.OrgGroup))
                //    {
                //        var current = emp.CurrentWeekDashBoardDetails[group.OrgGroup];
                //        current.Add(group);
                //        emp.CurrentWeekDashBoardDetails[group.OrgGroup] = current;
                //    }
                //    else
                //    {
                //        var currentvalue = new List<ITDashBoardDetailsSummary>();
                //        currentvalue.Add(group);
                //        emp.CurrentWeekDashBoardDetails.Add(group.OrgGroup, currentvalue);
                //    }
                //}


                //emp.NextWeekITDashBoardDetails = new Dictionary<string, List<ITDashBoardDetailsSummary>>();
                //foreach (var group in nextweekdata)
                //{
                //    if (emp.NextWeekITDashBoardDetails.ContainsKey(group.OrgGroup))
                //    {
                //        var current = emp.NextWeekITDashBoardDetails[group.OrgGroup];
                //        current.Add(group);
                //        emp.NextWeekITDashBoardDetails[group.OrgGroup] = current;
                //    }
                //    else
                //    {
                //        var currentvalue = new List<ITDashBoardDetailsSummary>();
                //        currentvalue.Add(group);
                //        emp.NextWeekITDashBoardDetails.Add(group.OrgGroup, currentvalue);
                //    }
                //}


                return PartialView("_GenerateDashboardPartialView", emp);
            }
            else
            {
                //ermsg = "Session expired";
                return Json("Sessionexpired", JsonRequestBehavior.AllowGet);
                //return RedirectToAction("SessionExpire", "Signout");
            }
        }
        private List<ITDashBoardDetailsSummary> GenerateITSReportSummary()
        {
            int empId = Convert.ToInt32(Session["EmployeeId"]);

            List<ITDashBoardDetailsSummary> lstITSReportSummary = new List<ITDashBoardDetailsSummary>();
            lstITSReportSummary = db.Database.SqlQuery<ITDashBoardDetailsSummary>
              ("exec GetITSResourceDashBoardDetailsSummary @EmployeeId", new SqlParameter("EmployeeId", empId)).ToList<ITDashBoardDetailsSummary>();

            return lstITSReportSummary;
        }

        private List<ITSResourceDashboard> GenerateITSReport()
        {
            int empId = Convert.ToInt32(Session["EmployeeId"]);
            List<ITSResourceDashboard> lstEmployeeSkillsReport = new List<ITSResourceDashboard>();

            lstEmployeeSkillsReport = db.Database.SqlQuery<ITSResourceDashboard>
               ("exec GetITSResourceDashBoardDetails @EmployeeId", new SqlParameter("EmployeeId", empId)).ToList<ITSResourceDashboard>();

            return lstEmployeeSkillsReport;
        }

        private List<ITDashBoardDetailsSummary> GeneratePreviousWeeksReport()
        {
            int empId = Convert.ToInt32(Session["EmployeeId"]);
            List<ITDashBoardDetailsSummary> lstPreviousITSReportSummary = new List<ITDashBoardDetailsSummary>();
            lstPreviousITSReportSummary = db.Database.SqlQuery<ITDashBoardDetailsSummary>
               ("exec PreviouWeekDetailsSummary @EmployeeId", new SqlParameter("EmployeeId", empId)).ToList<ITDashBoardDetailsSummary>();
            return lstPreviousITSReportSummary;
        }

        private List<ITDashBoardDetailsSummary> GenerateCurrentWeeksReport()
        {
            int empId = Convert.ToInt32(Session["EmployeeId"]);
            List<ITDashBoardDetailsSummary> lstpresentITSReportSummary = new List<ITDashBoardDetailsSummary>();
            lstpresentITSReportSummary = db.Database.SqlQuery<ITDashBoardDetailsSummary>
               ("exec CurrentWeekDetailsSummary @EmployeeId", new SqlParameter("EmployeeId", empId)).ToList<ITDashBoardDetailsSummary>();
            return lstpresentITSReportSummary;
        }

        private List<ITDashBoardDetailsSummary> GenerateNextWeeksReport()
        {
            int empId = Convert.ToInt32(Session["EmployeeId"]);
            List<ITDashBoardDetailsSummary> lstnextITSReportSummary = new List<ITDashBoardDetailsSummary>();
            lstnextITSReportSummary = db.Database.SqlQuery<ITDashBoardDetailsSummary>
               ("exec NextWeekDetailsSummary @EmployeeId", new SqlParameter("EmployeeId", empId)).ToList<ITDashBoardDetailsSummary>();
            return lstnextITSReportSummary;
        }

        public ActionResult ExportReport()
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
        public ActionResult GetITSReportSummaryforPieChart()
        {
            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            {
                try
                {
                    List<ITSResourceDashboard> lstEmpSkills = GenerateITSReport();
                    List<ITDashBoardDetailsSummary> lstReport = GenerateITSReportSummary();

                    List<ITDashBoardDetailsChart> lstchartdata = new List<ITDashBoardDetailsChart>();
                    lstchartdata = lstReport.GroupBy(n => n.OrgGroup)
                         .Select(n => new ITDashBoardDetailsChart
                         {
                             OrgGroup = n.Key,
                             GroupbyTotal = n.Sum(y => y.Onsite + y.Offshore)
                         })
                         .ToList();

                    lstchartdata.RemoveAt(lstchartdata.Count - 1);                   
                    return Json(lstchartdata, JsonRequestBehavior.AllowGet);
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
        [HttpPost]
        public JsonResult GetITSReportSummaryforBarChart()
        {
            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            {
                try
                {
                    List<ITDashBoardDetailsSummary> lstReport = new List<ITDashBoardDetailsSummary>();                 
                    lstReport = GenerateITSReportSummary();
                    List<ITDashBoardDetailsChart> groupchart = new List<ITDashBoardDetailsChart>();
                    groupchart = lstReport.GroupBy(n => n.OrgGroup)
                            .Select(n => new ITDashBoardDetailsChart
                            {
                                OrgGroup = n.Key,
                                OffshoreGroupbyTotal = n.Sum(y => y.Offshore),
                                OnsiteGroupbyTotal = n.Sum(y => y.Onsite)                             
                            }).ToList();          
                    groupchart.RemoveAt(groupchart.Count - 1);
                    return Json(groupchart, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json("Sessionexpired", JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json("Sessionexpired", JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GenerateEmployeeSkillReport()
        {
            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            {
                try
                {
                    List<ITSResourceDashboard> lstEmpSkills = GenerateITSReport();
                    List<ITDashBoardDetailsSummary> lstReport = GenerateITSReportSummary();

                    using (ExcelPackage package = new ExcelPackage())
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("ITS Resource Information");
                        ExcelWorksheet worksheetone = package.Workbook.Worksheets.Add("ITS Resource Report Summary");
                        worksheet.TabColor = System.Drawing.Color.Green;
                        worksheet.DefaultRowHeight = 18f;
                        worksheet.Row(1).Height = 20f;

                        List<string> lstOpsStatus = Environment.GetEnvironmentVariable("OpsStatus") ?? "".Split(',').ToList();

                        bool IsValid = lstOpsStatus.Where(e => Session["EmployeeId"].ToString().Contains(e)).Any();
                        //int nocloumn = 46;
                        //if (IsValid)
                        //    nocloumn = 47;
                        int nocloumn = 54;
                        if (IsValid)
                            nocloumn = 55;

                        worksheetone.TabColor = System.Drawing.Color.Blue;
                        worksheetone.DefaultRowHeight = 18f;
                        worksheetone.Row(1).Height = 20f;
                        int numcloumn = 5;

                        using (var range = worksheetone.Cells[1, 1, 1, numcloumn])
                        {
                            range.Style.Font.Bold = true;
                            range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                            range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#5B9BD5"));
                            range.Style.Font.Color.SetColor(System.Drawing.Color.White);
                            range.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                        }
                        //Add the headers for worksheetone                       
                        worksheetone.Cells[1, 1].Value = "Organization Group";
                        worksheetone.Cells[1, 2].Value = "Organization Sub Group";
                        worksheetone.Cells[1, 3].Value = "OFFSHORE";
                        worksheetone.Cells[1, 4].Value = "ONSITE";
                        worksheetone.Cells[1, 5].Value = "Total Count";
                        worksheetone.DefaultColWidth = 18f;
                        worksheetone.Column(1).Width = 20f;
                        worksheetone.Column(2).AutoFit(20f);
                        worksheetone.Column(3).AutoFit(30f);
                        worksheetone.Column(4).AutoFit(30f);
                        worksheetone.Column(5).AutoFit(30f);

                        string strOrgGroup = "";
                        int SubTotalOnSite = 0;
                        int SubTotalOffShore = 0;

                        int ExcelRow = 2;

                        List<ClusterLead> lstclusterLeads = new List<ClusterLead>();
                        lstclusterLeads = db.Database.SqlQuery<ClusterLead>("exec GetClusterTeamMember").ToList();

                        for (int rowIndex = 0, row = 2; rowIndex < lstReport.Count; rowIndex++, row++) // row indicates number of rows
                        {
                            if (strOrgGroup == "")
                            {

                                strOrgGroup = lstReport[rowIndex].OrgGroup;
                                SubTotalOnSite = lstReport[rowIndex].Onsite;
                                SubTotalOffShore = lstReport[rowIndex].Offshore;
                            }
                            if (strOrgGroup != null && strOrgGroup == lstReport[rowIndex].OrgGroup)
                            {
                                if (rowIndex == 0)
                                {
                                    SubTotalOnSite = lstReport[rowIndex].Onsite;
                                    SubTotalOffShore = lstReport[rowIndex].Offshore;
                                }
                                else

                                {
                                    SubTotalOnSite = SubTotalOnSite + lstReport[rowIndex].Onsite;
                                    SubTotalOffShore = SubTotalOffShore + lstReport[rowIndex].Offshore;

                                }
                                //SubTotalOnSite = SubTotalOnSite + lstReport[rowIndex].Onsite;
                                //SubTotalOffShore = SubTotalOffShore + lstReport[rowIndex].Offshore;
                            }
                            if (strOrgGroup != null && strOrgGroup != lstReport[rowIndex].OrgGroup)
                            {
                                worksheetone.Cells[ExcelRow, 1].Value = "";
                                worksheetone.Cells[ExcelRow, 2].Value = "Total";
                                worksheetone.Cells[ExcelRow, 3].Value = SubTotalOffShore;
                                worksheetone.Cells[ExcelRow, 4].Value = SubTotalOnSite;
                                worksheetone.Cells[ExcelRow, 5].Value = SubTotalOnSite + SubTotalOffShore;
                                //worksheetone.SelectedRange[ExcelRow, 1].Style.Font.Bold = true;
                                //worksheetone.SelectedRange[ExcelRow, 2].Style.Font.Bold = true;
                                //worksheetone.SelectedRange[ExcelRow, 3].Style.Font.Color.SetColor(System.Drawing.Color.Black);
                                //worksheetone.SelectedRange[ExcelRow, 4].Style.Font.Color.SetColor(System.Drawing.Color.Black);
                                //worksheetone.SelectedRange[ExcelRow, 5].Style.Font.Color.SetColor(System.Drawing.Color.Black);
                                using (var range = worksheetone.Cells[ExcelRow, 1, ExcelRow, 5])
                                {
                                    range.Style.Font.Bold = true;
                                    range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#9BC2E5"));
                                    range.Style.Font.Color.SetColor(System.Drawing.Color.Black);
                                    range.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                    range.Style.Border.Right.Color.SetColor(System.Drawing.Color.LightGray);
                                }
                                //worksheetone.SelectedRange[ExcelRow, 1].Style.Font.Bold = true;
                                //worksheetone.SelectedRange[ExcelRow, 2].Style.Font.Bold = true;
                                //worksheetone.SelectedRange[ExcelRow, 3].Style.Font.Bold = true;
                                //worksheetone.SelectedRange[ExcelRow, 4].Style.Font.Bold = true;
                                //worksheetone.SelectedRange[ExcelRow, 5].Style.Font.Bold = true;
                                ExcelRow = ExcelRow + 1;
                               
                                strOrgGroup = lstReport[rowIndex].OrgGroup;
                                SubTotalOnSite = lstReport[rowIndex].Onsite;
                                SubTotalOffShore = lstReport[rowIndex].Offshore;
                            }
                            worksheetone.Cells[ExcelRow, 1].Value = lstReport[rowIndex].OrgGroup;
                            worksheetone.Cells[ExcelRow, 2].Value = lstReport[rowIndex].OrgSubGroup;
                            worksheetone.Cells[ExcelRow, 3].Value = lstReport[rowIndex].Offshore;
                            worksheetone.Cells[ExcelRow, 4].Value = lstReport[rowIndex].Onsite;
                            worksheetone.Cells[ExcelRow, 5].Value = lstReport[rowIndex].TotalCount;
                            //worksheetone.SelectedRange[ExcelRow, 1].Style.Font.Bold = true;
                            // worksheetone.SelectedRange[ExcelRow, 2].Style.Font.Bold = true;
                            //worksheetone.SelectedRange[ExcelRow, 3].Style.Font.Bold = true;
                            //worksheetone.SelectedRange[ExcelRow, 4].Style.Font.Bold = true;
                            //worksheetone.SelectedRange[ExcelRow, 5].Style.Font.Bold = true;

                            if (lstReport[rowIndex].OrgGroup == "Grand Total" && lstReport[rowIndex].OrgSubGroup == "Grand Total")
                            {
                                worksheetone.Cells[ExcelRow, 1].Value = "";
                                using (var range = worksheetone.Cells[ExcelRow, 1, ExcelRow, 5])
                                {
                                    range.Style.Font.Bold = true;
                                    range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#002060"));
                                    range.Style.Font.Color.SetColor(System.Drawing.Color.White);
                                    range.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                    range.Style.Border.Right.Color.SetColor(System.Drawing.Color.LightGray);
                                }
                                //worksheetone.SelectedRange[ExcelRow, 1].Style.Font.Bold = true;
                                //worksheetone.SelectedRange[ExcelRow, 2].Style.Font.Bold = true;
                                //worksheetone.SelectedRange[ExcelRow, 3].Style.Font.Bold = true;
                                //worksheetone.SelectedRange[ExcelRow, 4].Style.Font.Bold = true;
                                //worksheetone.SelectedRange[ExcelRow, 5].Style.Font.Bold = true;
                            }


                            ExcelRow = ExcelRow + 1;
                        }

                        using (var range = worksheet.Cells[1, 1, 1, nocloumn])
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
                        worksheet.Cells[1, 1].Value = "Employee No";
                        worksheet.Cells[1, 2].Value = "UniqueId";
                        worksheet.Cells[1, 3].Value = "Employee Name";
                        worksheet.Cells[1, 4].Value = "Designation";
                        worksheet.Cells[1, 5].Value = "Role";
                        worksheet.Cells[1, 6].Value = "Grade code";
                        worksheet.Cells[1, 7].Value = "Employee Type";
                        worksheet.Cells[1, 8].Value = "Joining date";
                        worksheet.Cells[1, 9].Value = "Employee mailId";
                        worksheet.Cells[1, 10].Value = "Location";
                        worksheet.Cells[1, 11].Value = "LocationType";

                        //MM//worksheet.Cells[1, 12].Value = "Onsite/OffShore";
                        //worksheet.Cells[1, 10].Value = "Parent Organisation";
                        worksheet.Cells[1, 12].Value = "Practice";
                        worksheet.Cells[1, 13].Value = "Practice A";
                        worksheet.Cells[1, 14].Value = "Revenue Practice";
                        worksheet.Cells[1, 15].Value = "Client Name";
                        worksheet.Cells[1, 16].Value = "Project Practice";
                        worksheet.Cells[1, 17].Value = "Project Code";
                        worksheet.Cells[1, 18].Value = "Project Name";
                        worksheet.Cells[1, 19].Value = "Bill Type";
                        worksheet.Cells[1, 20].Value = "Project Sub-Type";
                        worksheet.Cells[1, 21].Value = "Head Count";
                        worksheet.Cells[1, 22].Value = "Billing Status";
                        worksheet.Cells[1, 23].Value = "Bench Category";
                        worksheet.Cells[1, 24].Value = "Bench Sub-Category";
                        worksheet.Cells[1, 25].Value = "Client Mapping";
                        worksheet.Cells[1, 26].Value = "Bench Remarks";
                        worksheet.Cells[1, 27].Value = "Expected Billing Date";
                        worksheet.Cells[1, 28].Value = "Mapped Date";
                        worksheet.Cells[1, 29].Value = "Reserved Project Code";
                        worksheet.Cells[1, 30].Value = "Roll On Date";
                        worksheet.Cells[1, 31].Value = "Roll Off Date";
                        worksheet.Cells[1, 32].Value = "SOW End Date";
                        worksheet.Cells[1, 33].Value = "SOW Status";
                        worksheet.Cells[1, 34].Value = "Project Manager";
                        worksheet.Cells[1, 35].Value = "Project Manager Email";
                        worksheet.Cells[1, 36].Value = "Delivery Manager";
                        worksheet.Cells[1, 37].Value = "Supervisor";
                        worksheet.Cells[1, 38].Value = "Non Billable Count Days";
                        worksheet.Cells[1, 39].Value = "Non Billable Days";
                        worksheet.Cells[1, 40].Value = "Bench Ageing";
                        worksheet.Cells[1, 41].Value = "Duration in Trianz (Months)";
                        worksheet.Cells[1, 42].Value = "Visa Type";
                        worksheet.Cells[1, 43].Value = "Serving Notice Period";
                        worksheet.Cells[1, 44].Value = "Date of Relieving";
                        worksheet.Cells[1, 45].Value = "Organisation";
                        worksheet.Cells[1, 46].Value = "Organisation Group";
                        worksheet.Cells[1, 47].Value = "Business Group";
                        worksheet.Cells[1, 48].Value = "Org Group";
                        worksheet.Cells[1, 49].Value = "Org Sub Group";
                        worksheet.Cells[1, 50].Value = "Cluster Name";
                        worksheet.Cells[1, 51].Value = "Cluster Role";
                        worksheet.Cells[1, 52].Value = "Skills";
                        worksheet.Cells[1, 53].Value = "Cost Center Name";
                        if (IsValid)
                        {
                            worksheet.Cells[1, 54].Value = "Ops Status";
                        }
                        //worksheet.Cells[1, 29].Value = "Competancy";
                        //worksheet.Cells[1, 30].Value = "Skill";
                        //worksheet.Cells[1, 31].Value = "Last Used";
                        //worksheet.Cells[1, 32].Value = "Expertise Level";
                        //worksheet.Cells[1, 33].Value = "Sub Skill4";
                        //worksheet.Cells[1, 34].Value = "Sub Skill5";
                        worksheet.DefaultColWidth = 18f;
                        worksheet.Column(1).Width = 12f;
                        worksheet.Column(2).AutoFit(20f);
                        worksheet.Column(3).AutoFit(30f);
                        worksheet.Column(4).AutoFit(30f);
                        worksheet.Column(5).AutoFit(30f);
                        worksheet.Column(6).AutoFit(15f);
                        worksheet.Column(7).Width = 20f;
                        worksheet.Column(8).AutoFit(15f);
                        worksheet.Column(9).AutoFit(30f);
                        worksheet.Column(10).AutoFit(18f);
                        worksheet.Column(11).AutoFit(20f);
                        worksheet.Column(12).AutoFit(20f);
                        worksheet.Column(13).Width = 20f;
                        worksheet.Column(14).AutoFit(20f);
                        worksheet.Column(15).AutoFit(25f);
                        worksheet.Column(16).AutoFit(20f);
                        worksheet.Column(17).AutoFit(20f);
                        worksheet.Column(18).AutoFit(30f);
                        worksheet.Column(19).AutoFit(20f);
                        worksheet.Column(20).AutoFit(20f);
                        worksheet.Column(21).AutoFit(18f);
                        worksheet.Column(22).AutoFit(22f);
                        worksheet.Column(23).AutoFit(25f);
                        worksheet.Column(24).AutoFit(25f);
                        worksheet.Column(25).AutoFit(10f);
                        worksheet.Column(26).AutoFit(10f);
                        worksheet.Column(27).AutoFit(10f);
                        worksheet.Column(28).AutoFit(10f);
                        worksheet.Column(29).AutoFit(25f);
                        worksheet.Column(30).AutoFit(20f);
                        worksheet.Column(31).AutoFit(20f);
                        worksheet.Column(32).AutoFit(20f);
                        worksheet.Column(33).AutoFit(20f);
                        worksheet.Column(34).AutoFit(22f);
                        worksheet.Column(35).AutoFit(30f);
                        worksheet.Column(36).AutoFit(22f);
                        worksheet.Column(37).AutoFit(22f);
                        worksheet.Column(38).AutoFit(20f);
                        worksheet.Column(39).AutoFit(30f);
                        worksheet.Column(40).AutoFit(20f);
                        worksheet.Column(41).AutoFit(25f);
                        worksheet.Column(42).AutoFit(20f);
                        worksheet.Column(43).AutoFit(30f);
                        worksheet.Column(44).AutoFit(20f);
                        worksheet.Column(45).AutoFit(30f);
                        worksheet.Column(46).AutoFit(20f);
                        worksheet.Column(47).AutoFit(30f);
                        worksheet.Column(48).AutoFit(22f);
                        worksheet.Column(49).AutoFit(22f);
                        worksheet.Column(50).AutoFit(32f);
                        worksheet.Column(51).AutoFit(20f);
                        worksheet.Column(52).AutoFit(40f);
                        worksheet.Column(53).AutoFit(20f);               
                        //MM  //worksheet.Column(45).AutoFit(30f);
                        //MM //worksheet.Column(46).AutoFit(30f);
                        if (IsValid)
                        {
                            //MM  // worksheet.Column(47).AutoFit(30f);
                            worksheet.Column(54).AutoFit(30f);
                        }
                        //Add the each row
                        string strCluster = "";
                        string strClusterLdMbr = "";
                        for (int rowIndex = 0, row = 2; rowIndex < lstEmpSkills.Count; rowIndex++, row++) // row indicates number of rows
                        {
                            List<ClusterLead> lstTempList = new List<ClusterLead>();
                            lstTempList = lstclusterLeads;
                            lstTempList = lstTempList.Where(p => p.TeamMemberID == lstEmpSkills[rowIndex].EmployeeID).ToList();
                            //if (lstTempList.Count == 0)
                            //{
                            //    strCluster = "";
                            //}
                            foreach (var clstr in lstTempList)
                            {
                                if (strCluster == "")
                                {
                                    strCluster = clstr.ClusterName;
                                }
                                else
                                {
                                    strCluster = strCluster + ", " + clstr.ClusterName;
                                }

                                if (strCluster != "")
                                {
                                    if (clstr.Role == "CL")
                                    {
                                        strClusterLdMbr = "Lead";
                                    }
                                    if (clstr.Role == "CM")
                                    {
                                        strClusterLdMbr = "Member";
                                    }
                                }
                            }
                            
                            worksheet.Cells[row, 1].Value = lstEmpSkills[rowIndex].EmployeeID;
                            worksheet.Cells[row, 2].Value = lstEmpSkills[rowIndex].UniqueId;
                            worksheet.Cells[row, 3].Value = lstEmpSkills[rowIndex].EmployeeName;
                            worksheet.Cells[row, 4].Value = lstEmpSkills[rowIndex].Designation;
                            worksheet.Cells[row, 5].Value = lstEmpSkills[rowIndex].Designation;
                            worksheet.Cells[row, 6].Value = lstEmpSkills[rowIndex].GradeCode;
                            worksheet.Cells[row, 7].Value = lstEmpSkills[rowIndex].EmployeeType;

                            worksheet.Cells[row, 8].Value = lstEmpSkills[rowIndex].DOj;
                            worksheet.Cells[row, 8].Style.Numberformat.Format = "dd-MMM-yyyy";
                            worksheet.Cells[row, 8].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                            worksheet.Cells[row, 9].Value = lstEmpSkills[rowIndex].Email;
                            worksheet.Cells[row, 10].Value = lstEmpSkills[rowIndex].Location;
                            //MM//   worksheet.Cells[row, 11].Value = lstEmpSkills[rowIndex].OnsiteOROffshore;
                            worksheet.Cells[row, 11].Value = lstEmpSkills[rowIndex].LocationType;
                            worksheet.Cells[row, 12].Value = lstEmpSkills[rowIndex].Practice;
                            if (lstEmpSkills[rowIndex].PracticeA == "NULL")
                            {
                                worksheet.Cells[row, 13].Value = "";
                            }
                            else
                            {
                                 worksheet.Cells[row, 13].Value = lstEmpSkills[rowIndex].PracticeA;                               
                            }
                           // worksheet.Cells[row, 13].Value = lstEmpSkills[rowIndex].Practice;

                            worksheet.Cells[row, 14].Value = lstEmpSkills[rowIndex].RevenuePractice;
                            worksheet.Cells[row, 15].Value = lstEmpSkills[rowIndex].AccountName;
                            worksheet.Cells[row, 16].Value = lstEmpSkills[rowIndex].ProjectPractice;
                            worksheet.Cells[row, 17].Value = lstEmpSkills[rowIndex].ProjectCode;
                            worksheet.Cells[row, 18].Value = lstEmpSkills[rowIndex].ProjectName;
                            worksheet.Cells[row, 19].Value = lstEmpSkills[rowIndex].billtype;
                            worksheet.Cells[row, 20].Value = lstEmpSkills[rowIndex].ProjectSubType;
                            worksheet.Cells[row, 21].Value = lstEmpSkills[rowIndex].headcount;
                            worksheet.Cells[row, 22].Value = lstEmpSkills[rowIndex].BillingStatus;

                            //MM// worksheet.Cells[row, 21].Value = lstEmpSkills[rowIndex].Category;
                            if (lstEmpSkills[rowIndex].Category == "NULL")
                            {
                                worksheet.Cells[row, 23].Value = "";
                            }
                            else
                            {
                                worksheet.Cells[row, 23].Value = lstEmpSkills[rowIndex].Category;
                            }

                            //MM// worksheet.Cells[row, 22].Value = lstEmpSkills[rowIndex].Benchstatus;
                            if (lstEmpSkills[rowIndex].Remarks == "NULL")
                            {
                                worksheet.Cells[row, 24].Value = "";
                            }
                            else
                            {
                                worksheet.Cells[row, 24].Value = lstEmpSkills[rowIndex].Benchstatus;
                            }
                            worksheet.Cells[row, 25].Value = lstEmpSkills[rowIndex].Customer;
                            if (lstEmpSkills[rowIndex].Remarks == "NULL")
                            {
                                worksheet.Cells[row, 26].Value = "";
                            }
                            else
                            {
                                worksheet.Cells[row, 26].Value = lstEmpSkills[rowIndex].Remarks;
                            }

                            worksheet.Cells[row, 27].Value = lstEmpSkills[rowIndex].ExpectedBillingDate;
                            worksheet.Cells[row, 27].Style.Numberformat.Format = "dd-MMM-yyyy";
                            worksheet.Cells[row, 27].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                            worksheet.Cells[row, 28].Value = lstEmpSkills[rowIndex].MappedDate;
                            worksheet.Cells[row, 28].Style.Numberformat.Format = "dd-MMM-yyyy";
                            worksheet.Cells[row, 28].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                            worksheet.Cells[row, 29].Value = lstEmpSkills[rowIndex].ReservedProjectCode;

                            worksheet.Cells[row, 30].Value = lstEmpSkills[rowIndex].rollondate;
                            worksheet.Cells[row, 30].Style.Numberformat.Format = "dd-MMM-yyyy";
                            worksheet.Cells[row, 30].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                            worksheet.Cells[row, 31].Value = lstEmpSkills[rowIndex].rolloffdate;
                            worksheet.Cells[row, 31].Style.Numberformat.Format = "dd-MMM-yyyy";
                            worksheet.Cells[row, 31].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                            worksheet.Cells[row, 32].Value = lstEmpSkills[rowIndex].SOWEndDate;
                            worksheet.Cells[row, 32].Style.Numberformat.Format = "dd-MMM-yyyy";
                            worksheet.Cells[row, 32].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                            worksheet.Cells[row, 33].Value = lstEmpSkills[rowIndex].SOWStatus;

                            worksheet.Cells[row, 34].Value = lstEmpSkills[rowIndex].ProjectManagerName;
                            worksheet.Cells[row, 35].Value = lstEmpSkills[rowIndex].ProjectManageremail;
                            worksheet.Cells[row, 36].Value = lstEmpSkills[rowIndex].DeliveryManager;
                            worksheet.Cells[row, 37].Value = lstEmpSkills[rowIndex].Supervisorname;
                            worksheet.Cells[row, 38].Value = lstEmpSkills[rowIndex].NonBillableCountDays;
                            worksheet.Cells[row, 39].Value = lstEmpSkills[rowIndex].NonBillableDays;
                            worksheet.Cells[row, 40].Value = lstEmpSkills[rowIndex].Ageing;
                            worksheet.Cells[row, 41].Value = lstEmpSkills[rowIndex].DurationidTrainz;
                            worksheet.Cells[row, 42].Value = lstEmpSkills[rowIndex].visaType;
                            worksheet.Cells[row, 43].Value = lstEmpSkills[rowIndex].Noticeperiod;

                            worksheet.Cells[row, 44].Value = lstEmpSkills[rowIndex].RelievingDate;
                            worksheet.Cells[row, 44].Style.Numberformat.Format = "dd-MMM-yyyy";
                            worksheet.Cells[row, 44].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                           

                            worksheet.Cells[row, 45].Value = lstEmpSkills[rowIndex].Organisation;
                            worksheet.Cells[row, 46].Value = lstEmpSkills[rowIndex].OrganisationGRoup;
                            worksheet.Cells[row, 47].Value = lstEmpSkills[rowIndex].BusinessGroup;
                            worksheet.Cells[row, 48].Value = lstEmpSkills[rowIndex].OrgGroup;
                            worksheet.Cells[row, 49].Value = lstEmpSkills[rowIndex].OrgSubGroup;
                            worksheet.Cells[row, 50].Value = strCluster;
                            worksheet.Cells[row, 51].Value = strClusterLdMbr;
                            worksheet.Cells[row, 52].Value = lstEmpSkills[rowIndex].Skills;
                            worksheet.Cells[row, 53].Value = lstEmpSkills[rowIndex].CostCenter;
                            if (IsValid)
                            {
                                worksheet.Cells[row, 54].Value = lstEmpSkills[rowIndex].OpsStatus;
                            }
                            strCluster = "";
                            strClusterLdMbr = "";
                            //worksheet.Cells[row, 29].Value = lstEmpSkills[rowIndex].Competancy;
                            //worksheet.Cells[row, 30].Value = lstEmpSkills[rowIndex].Skill;
                            //worksheet.Cells[row, 31].Value = lstEmpSkills[rowIndex].Lastused;
                            //worksheet.Cells[row, 32].Value = lstEmpSkills[rowIndex].Expertiselevel;
                            //worksheet.Cells[row, 33].Value = lstEmpSkills[rowIndex].SubSkill4;
                            //worksheet.Cells[row, 34].Value = lstEmpSkills[rowIndex].SubSkill5;

                            if (row % 2 == 1)
                            {
                                using (var range = worksheet.Cells[row, 1, row, nocloumn])
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
                        Response.AddHeader("content-disposition", "attachment;filename=ITS Resource Dashboard " + DateTime.Now.ToString("dd MMM yyyy") + ".xlsx");
                        Response.Charset = "";
                        Response.ContentType = "application/vnd.ms-excel";
                        StringWriter sw = new StringWriter();
                        Response.BinaryWrite(fileBytes);
                        Response.End();
                    }

                    return new EmptyResult();
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
        public ActionResult FulfilmentDashBoard()
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
        public ActionResult FulfilmentDashBoardPieChart()
        {
             if (Convert.ToInt32(Session["EmployeeId"]) > 0)              
                {
                try
                {
                    List<FullfilmentDashBoard> pieChartData = new List<FullfilmentDashBoard>();
            pieChartData = db.Database.SqlQuery<FullfilmentDashBoard>("exec GetFulfilmentDashBoardPieChart").ToList();
            return Json(pieChartData, JsonRequestBehavior.AllowGet);
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
        public ActionResult ResourceDashboardChat()
            {

            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            {
                try
                {
                    using (TrianzOperationsEntities dc = new TrianzOperationsEntities())
                {


                    List<Employee> TotalEmployees = db.Employees.ToList();
                    List<ProjectAssignment> ProjectAssignment = db.ProjectAssignments.ToList();

                    var billingStatus = (from p in db.ProjectAssignments
                                         join e in db.Employees on p.EmployeeId equals e.EmployeeId
                                         where e.IsActive == true && p.IsActive == true
                                         group p by p.BillingStatus into newGroup
                                         orderby newGroup.Key
                                         select newGroup).ToList();


                    return View(ResourceDashboardViewModel.GetLineAreaChartData(billingStatus));
                }

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
        public ActionResult FulfillmentReview()
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
        public void DownLoadReport()
        {
            //List<FulfillmentReview> fulfillment = new List<FulfillmentReview>();
            //var lst = db.Database.SqlQuery<List<object>>("exec GetFulfilledReview").ToList();

            // Connection string is read from ADO_CONNECTION_STRING environment variable
            string con = System.Environment.GetEnvironmentVariable("ADO_CONNECTION_STRING") ?? ConfigurationManager.ConnectionStrings["ADOConnectionString"].ConnectionString;
            SqlDataAdapter adp = new SqlDataAdapter("GetFulfilledReview", con);
            DataSet ds = new DataSet();
            adp.Fill(ds);

            //string xml = String.Empty;
            //XmlDocument xmlDoc = new XmlDocument();

            //XmlSerializer xmlSerializer = new XmlSerializer(ds.Tables[0].GetType());

            //using (MemoryStream xmlStream = new MemoryStream())
            //{
            //    xmlSerializer.Serialize(xmlStream, ds.Tables[0]);
            //    xmlStream.Position = 0;
            //    xmlDoc.Load(xmlStream);
            //    xml = xmlDoc.InnerXml;
            //}

            //var fName = string.Format("Fulfilment Review" + DateTime.Now.ToString("dd/mm/yyyy") + ".xls");

            //byte[] fileContents = Encoding.UTF8.GetBytes(xml);

            string attachment = "attachment; filename=FulfillmentReview" + DateTime.Now.ToString("dd/MM/yyyy") + ".xls";
            Response.ClearContent();
            Response.AddHeader("content-disposition", attachment);
            Response.ContentType = "application/vnd.ms-excel";

            System.IO.StringWriter stringWrite = new System.IO.StringWriter();
            System.Web.UI.HtmlTextWriter htmlWrite = new HtmlTextWriter(stringWrite);


            string tab = "";
            DataTable dt = new DataTable();
            dt = ds.Tables[0];
            foreach (DataColumn dc in dt.Columns)
            {
                Response.Write(tab + dc.ColumnName);
                tab = "\t";
            }
            Response.Write("\n");
            int i;
            foreach (DataRow dr in dt.Rows)
            {
                tab = "";
                for (i = 0; i < dt.Columns.Count; i++)
                {
                    if (i == 0)
                    {

                    }
                    Response.Write(tab + dr[i].ToString());
                    tab = "\t";
                }
                Response.Write("\n");
            }


            Response.End();

            //return File(fileContents, "application/vnd.ms-excel", fName);

        }       
        public ActionResult EmbeddingUrl()
        {
            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            {
                return View();
            }
            else
            {
                return RedirectToAction("SessionExpire", "Signout");
            }
        }
        
    }
       

    }