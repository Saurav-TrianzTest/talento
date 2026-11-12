using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.Data.SqlClient;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Trianz.Enterprise.Operations.Models;
using Trianz.Enterprise.Operations.ViewModel;
using System.Data.Common;
using System.Data;
using System.Threading.Tasks;
using Newtonsoft.Json;


using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Session;
namespace Trianz.Enterprise.Operations.Controllers
{
    public class FinanceReportsController : Controller
    {
        private TrianzOperationsEntities db = new TrianzOperationsEntities();
        // GET: FinanceReports

        public ActionResult AddInternalSpcProjects()
        {
            var viewResult = new FinanceReports();
            try
            {

                viewResult.InternalSpecificDetails = (from intsp in db.InternalSpecifics
                                                      select new InternalSpecificCodes
                                                      {
                                                          ProjectCode = intsp.ProjectCode,
                                                          ProjectName = intsp.ProjectName,
                                                          SNo = intsp.SNo
                                                      }).ToList();

            }
            catch (Exception ex)
            {

                //Common.WriteErrorLog(ex.Message + ex.StackTrace);
            }
            return View(viewResult.InternalSpecificDetails);


        }
        [AcceptVerbs("POST")]
        public JsonResult Autocomplete(string term)
        {
            var result = new List<KeyValuePair<string, string>>();
            DateTime currentDate = System.DateTime.Now.Date;
            IList<SelectListItem> List = new List<SelectListItem>();

            var Projects = (from pro in db.Projects
                            select new SelectListItem
                            {
                                Text = pro.ProjectCode,
                                Value = pro.ProjectCode
                            }).Distinct().ToList();

            foreach (var item in Projects)
            {
                result.Add(new KeyValuePair<string, string>(item.Value.ToString(), item.Text));
            }
            var result3 = result.Where(s => s.Key.ToLower().Contains
                          (term.ToLower())).Select(w => w).ToList();
            return Json(result3);
        }
        [AcceptVerbs("POST")]
        public JsonResult Autocompletename(string term)
        {
            var result = new List<KeyValuePair<string, string>>();
            DateTime currentDate = System.DateTime.Now.Date;
            IList<SelectListItem> List = new List<SelectListItem>();

            var Projects = (from pro in db.Projects
                            select new SelectListItem
                            {
                                Text = pro.ProjectCode,
                                Value = pro.ProjectName
                            }).Distinct().ToList();

            foreach (var item in Projects)
            {
                result.Add(new KeyValuePair<string, string>(item.Value.ToString(), item.Text));
            }
            var result3 = result.Where(s => s.Key.ToLower().Contains
                          (term.ToLower())).Select(w => w).ToList();
            return Json(result3);
        }
        public ActionResult SaveProjectCode(string ProjectCode, string ProjectName)
        {


            InternalSpecific intsp = new InternalSpecific();

            intsp.ProjectCode = ProjectCode;
            intsp.ProjectName = ProjectName;
            db.InternalSpecifics.Add(intsp);
            db.SaveChanges();
            return Redirect("AddInternalSpcProjects");
        }

        public ActionResult DeleteProjectCode(int SNo)
        {
            try
            {


                InternalSpecific Interspc = db.InternalSpecifics.Find(SNo);
                db.InternalSpecifics.Remove(Interspc);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                //Common.WriteErrorLog(ex.Message + ex.StackTrace);
            }
            return RedirectToAction("AddInternalSpcProjects");

            //return RedirectToAction("AddInternalSpcProjects", "FinanceReports", new { SNo = SNo });
        }
        public ActionResult FinanceBenchReport()
        {

            //ViewData["BenchReport"] = GetBenchReport(DateTime StartDate, DateTime EndDate).ToList();


            return View();
        }

        public List<FinanceBenchDetails> GetBenchReport(DateTime StartDate, DateTime EndDate)
        {

            List<FinanceBenchDetails> BenchReport = new List<FinanceBenchDetails>();
            SqlParameter sqlStartDate = new SqlParameter("@StartDate", SqlDbType.DateTime);
            sqlStartDate.Value = StartDate;
            SqlParameter sqlEndDate = new SqlParameter("@EndDate", SqlDbType.DateTime);
            sqlEndDate.Value = EndDate;
            var BenchReportresult = db.Set<FinanceBenchDetails>().FromSqlRaw("exec  sp_FinanceBenchReport @StartDate,@EndDate", sqlStartDate, sqlEndDate).ToList<FinanceBenchDetails>();

            return BenchReportresult;
        }
        [HttpGet]
        public ActionResult BenchSearchData(DateTime StartDate, DateTime EndDate)
        {


            //var emp = new FinanceReports();

            //emp.BenchReports = GetBenchReport(StartDate, EndDate);
            return PartialView("_BenchReportView", GetBenchReport(StartDate, EndDate));

        }
        public ActionResult ExportBenchReport(DateTime StartDate, DateTime EndDate)
        {
            return Json("success");
        }
        public async Task<ActionResult> GenerateBenchReport(DateTime StartDate, DateTime EndDate)
        {
            var BenchReport = GetBenchReport(StartDate, EndDate);
            ViewData["BenchReport"] = BenchReport;

            #region Export to Excel

            using (ExcelPackage package = new ExcelPackage())
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Bench Report");
                worksheet.TabColor = System.Drawing.Color.Green;
                worksheet.DefaultRowHeight = 18f;
                worksheet.Row(1).Height = 20f;

                using (var range = worksheet.Cells[1, 1, 1, 19])
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
                worksheet.Cells[1, 1].Value = "EmployeeID"; ;

                worksheet.Cells[1, 2].Value = "EmployeeName";
                worksheet.Cells[1, 3].Value = "WorkLocation";
                worksheet.Cells[1, 4].Value = "Site";
                worksheet.Cells[1, 5].Value = "Grade";
                worksheet.Cells[1, 6].Value = "DOJ";
                worksheet.Cells[1, 7].Value = "RelievingDate";
                worksheet.Cells[1, 8].Value = "ClientName";
                worksheet.Cells[1, 9].Value = "BusinessGroup";
                worksheet.Cells[1, 10].Value = "ProjectCode";
                worksheet.Cells[1, 11].Value = "Projectname";
                worksheet.Cells[1, 12].Value = "EmployeeType";
                //worksheet.Cells[1,10].Value = "StandardHours";
                worksheet.Cells[1, 13].Value = "RollonDate";
                worksheet.Cells[1, 14].Value = "HC";
                worksheet.Cells[1, 15].Value = "Aging";
                worksheet.Cells[1, 16].Value = "Deployable";
                worksheet.Cells[1, 17].Value = "CustomerName";
                worksheet.Cells[1, 18].Value = "ExpectedBillingDate";

                worksheet.Cells[1, 19].Value = "MappedDate";


                worksheet.DefaultColWidth = 18f;



                //Add the each row
                for (int rowIndex = 0, row = 2; rowIndex < BenchReport.Count; rowIndex++, row++) // row indicates number of rows
                {

                    worksheet.Cells[row, 1].Value = BenchReport[rowIndex].EmployeeID;

                    worksheet.Cells[row, 2].Value = BenchReport[rowIndex].EmployeeName;


                    worksheet.Cells[row, 3].Value = BenchReport[rowIndex].Worklocation;
                    worksheet.Cells[row, 4].Value = BenchReport[rowIndex].site;
                    worksheet.Cells[row, 5].Value = BenchReport[rowIndex].Grade;

                    worksheet.Cells[row, 6].Value = BenchReport[rowIndex].DOJ;
                    worksheet.Cells[row, 6].Style.Numberformat.Format = "mm/dd/yyyy";
                    worksheet.Cells[row, 6].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                    worksheet.Cells[row, 7].Value = BenchReport[rowIndex].RelievingDate;
                    worksheet.Cells[row, 7].Style.Numberformat.Format = "mm/dd/yyyy";
                    worksheet.Cells[row, 7].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                    worksheet.Cells[row, 8].Value = BenchReport[rowIndex].ClientName;
                    worksheet.Cells[row, 9].Value = BenchReport[rowIndex].BusinessGroup;
                    worksheet.Cells[row, 10].Value = BenchReport[rowIndex].ProjectCode;
                    worksheet.Cells[row, 11].Value = BenchReport[rowIndex].ProjectName;
                    worksheet.Cells[row, 12].Value = BenchReport[rowIndex].EmployeeType;
                    //worksheet.Cells[row, 10].Value = BenchReport[rowIndex].StandaredHours;

                    worksheet.Cells[row, 13].Value = BenchReport[rowIndex].StartDate;
                    worksheet.Cells[row, 13].Style.Numberformat.Format = "mm/dd/yyyy";
                    worksheet.Cells[row, 13].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                    worksheet.Cells[row, 14].Value = BenchReport[rowIndex].HC;

                    worksheet.Cells[row, 15].Value = BenchReport[rowIndex].Aging;
                    worksheet.Cells[row, 16].Value = BenchReport[rowIndex].Deployable;
                    worksheet.Cells[row, 17].Value = BenchReport[rowIndex].CustomerName;
                    worksheet.Cells[row, 18].Value = BenchReport[rowIndex].ExpectedBillingDate;
                    worksheet.Cells[row, 18].Style.Numberformat.Format = "mm/dd/yyyy";
                    worksheet.Cells[row, 18].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                    worksheet.Cells[row, 19].Value = BenchReport[rowIndex].MappedDate;
                    worksheet.Cells[row, 19].Style.Numberformat.Format = "mm/dd/yyyy";
                    worksheet.Cells[row, 19].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;



                    if (row % 2 == 1)
                    {
                        using (var range = worksheet.Cells[row, 1, row, 19])
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
                // Response.Buffer is not available in ASP.NET Core
                Response.Headers.Add("content-disposition", "attachment;filename=" + "Bench-Report" + DateTime.Now.ToString("_dd-MMM-yyyy_hh-mm-ss") + ".xlsx");

                // Response.Charset is not available in ASP.NET Core
                Response.ContentType = "application/vnd.ms-excel";
                StringWriter sw = new StringWriter();
                await Response.Body.WriteAsync(fileBytes, 0, fileBytes.Length);
                // Response.End() is not available in ASP.NET Core
            }

            #endregion

            return new EmptyResult();
        }
        public ActionResult FinanceShadowReport()
        {

            return View();
        }

        private List<FinanceShadowDetails> GetShadowReport(DateTime StartDate, DateTime EndDate)
        {

            List<FinanceShadowDetails> ShadowReport = new List<FinanceShadowDetails>();

            var ShadowReportresult = db.Set<FinanceShadowDetails>().FromSqlRaw("exec sp_FinanceShadowReport @StartDate,@EndDate", new SqlParameter("StartDate", StartDate), new SqlParameter("EndDate", EndDate)).ToList<FinanceShadowDetails>();

            return ShadowReportresult;
        }
        [HttpGet]
        public ActionResult ShadowSearchData(DateTime StartDate, DateTime EndDate)
        {



            return PartialView("_ShadowReportView", GetShadowReport(StartDate, EndDate));

        }
        public ActionResult ExportShadowReport(DateTime StartDate, DateTime EndDate)
        {
            return Json("success");
        }
        public async Task<ActionResult> GenerateShadowReport(DateTime StartDate, DateTime EndDate)
        {
            var ShadowReport = GetShadowReport(StartDate, EndDate);
            ViewData["ShadowReport"] = ShadowReport;

            #region Export to Excel

            using (ExcelPackage package = new ExcelPackage())
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Shadow Report");
                worksheet.TabColor = System.Drawing.Color.Green;
                worksheet.DefaultRowHeight = 18f;
                worksheet.Row(1).Height = 20f;

                using (var range = worksheet.Cells[1, 1, 1, 15])
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
                worksheet.Cells[1, 1].Value = "EmployeeID";

                worksheet.Cells[1, 2].Value = "EmployeeName";
                worksheet.Cells[1, 3].Value = "WorkLocation";
                worksheet.Cells[1, 4].Value = "Site";
                worksheet.Cells[1, 5].Value = "Grade";
                worksheet.Cells[1, 6].Value = "DOJ";
                worksheet.Cells[1, 7].Value = "RelivingDate";
                worksheet.Cells[1, 8].Value = "ClientName";
                worksheet.Cells[1, 9].Value = "BusinessGroup";
                worksheet.Cells[1, 10].Value = "ProjectCode";
                worksheet.Cells[1, 11].Value = "Projectname";
                worksheet.Cells[1, 12].Value = "EmployeeType";
                // worksheet.Cells[1, 10].Value = "StandardHours";
                worksheet.Cells[1, 13].Value = "RollonDate";
                worksheet.Cells[1, 14].Value = "HC";
                worksheet.Cells[1, 15].Value = "Aging";


                worksheet.DefaultColWidth = 18f;



                //Add the each row
                for (int rowIndex = 0, row = 2; rowIndex < ShadowReport.Count; rowIndex++, row++) // row indicates number of rows
                {

                    worksheet.Cells[row, 1].Value = ShadowReport[rowIndex].EmployeeID;

                    worksheet.Cells[row, 2].Value = ShadowReport[rowIndex].EmployeeName;


                    worksheet.Cells[row, 3].Value = ShadowReport[rowIndex].Worklocation;
                    worksheet.Cells[row, 4].Value = ShadowReport[rowIndex].site;
                    worksheet.Cells[row, 5].Value = ShadowReport[rowIndex].Grade;


                    worksheet.Cells[row, 6].Value = ShadowReport[rowIndex].DOJ;
                    worksheet.Cells[row, 6].Style.Numberformat.Format = "mm/dd/yyyy";
                    worksheet.Cells[row, 6].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                    worksheet.Cells[row, 7].Value = ShadowReport[rowIndex].RelievingDate;
                    worksheet.Cells[row, 7].Style.Numberformat.Format = "mm/dd/yyyy";
                    worksheet.Cells[row, 7].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;




                    worksheet.Cells[row, 8].Value = ShadowReport[rowIndex].ClientName;
                    worksheet.Cells[row, 9].Value = ShadowReport[rowIndex].BusinessGroup;

                    worksheet.Cells[row, 10].Value = ShadowReport[rowIndex].ProjectCode;

                    worksheet.Cells[row, 11].Value = ShadowReport[rowIndex].ProjectName;
                    worksheet.Cells[row, 12].Value = ShadowReport[rowIndex].EmployeeType;
                    //worksheet.Cells[row, 10].Value = ShadowReport[rowIndex].StandaredHours;

                    worksheet.Cells[row, 13].Value = ShadowReport[rowIndex].StartDate;
                    worksheet.Cells[row, 13].Style.Numberformat.Format = "mm/dd/yyyy";
                    worksheet.Cells[row, 13].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                    worksheet.Cells[row, 14].Value = ShadowReport[rowIndex].HC;

                    worksheet.Cells[row, 15].Value = ShadowReport[rowIndex].Aging;







                    if (row % 2 == 1)
                    {
                        using (var range = worksheet.Cells[row, 1, row, 15])
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
                // Response.Buffer is not available in ASP.NET Core
                Response.Headers.Add("content-disposition", "attachment;filename=" + "Shadow-Report" + DateTime.Now.ToString("_dd-MMM-yyyy_hh-mm-ss") + ".xlsx");

                // Response.Charset is not available in ASP.NET Core
                Response.ContentType = "application/vnd.ms-excel";
                StringWriter sw = new StringWriter();
                await Response.Body.WriteAsync(fileBytes, 0, fileBytes.Length);
                // Response.End() is not available in ASP.NET Core
            }

            #endregion

            return new EmptyResult();
        }
        public ActionResult FinanceInternalReport()
        {

            return View();
        }

        public ActionResult InternalSearchData(DateTime StartDate, DateTime EndDate)
        {


            //var emp = new FinanceReports();

            //emp.BenchReports = GetBenchReport(StartDate, EndDate);
            return PartialView("_InternalReportView", GetInternalReport(StartDate, EndDate));

        }

        private List<FinanceInternalDetails> GetInternalReport(DateTime StartDate, DateTime EndDate)
        {

            List<FinanceInternalDetails> InternalReport = new List<FinanceInternalDetails>();

            var InternalReportresult = db.Set<FinanceInternalDetails>().FromSqlRaw("exec sp_FinanceInternalReport @StartDate,@EndDate", new SqlParameter("startDate", StartDate), new SqlParameter("endDate", EndDate)).ToList<FinanceInternalDetails>();

            return InternalReportresult;
        }
        public ActionResult ExportInternalReport(DateTime StartDate, DateTime EndDate)
        {
            return Json("success");
        }
        public async Task<ActionResult> GenerateInternalReport(DateTime StartDate, DateTime EndDate)
        {
            var InternalReport = GetInternalReport(StartDate, EndDate);
            ViewData["InternalReport"] = InternalReport;

            #region Export to Excel

            using (ExcelPackage package = new ExcelPackage())
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Internal Report");
                worksheet.TabColor = System.Drawing.Color.Green;
                worksheet.DefaultRowHeight = 18f;
                worksheet.Row(1).Height = 20f;

                using (var range = worksheet.Cells[1, 1, 1, 15])
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
                worksheet.Cells[1, 1].Value = "EmployeeID";

                worksheet.Cells[1, 2].Value = "EmployeeName";
                worksheet.Cells[1, 3].Value = "WorkLocation";
                worksheet.Cells[1, 4].Value = "Site";
                worksheet.Cells[1, 5].Value = "Grade";
                worksheet.Cells[1, 6].Value = "DOJ";
                worksheet.Cells[1, 7].Value = "RelievingDate";
                worksheet.Cells[1, 8].Value = "ClientName";
                worksheet.Cells[1, 9].Value = "BusinessGroup";
                worksheet.Cells[1, 10].Value = "ProjectCode";
                worksheet.Cells[1, 11].Value = "Projectname";
                worksheet.Cells[1, 12].Value = "EmployeeType";
                //worksheet.Cells[1,10].Value = "StandardHours";
                worksheet.Cells[1, 13].Value = "RollonDate";
                worksheet.Cells[1, 14].Value = "HC";
                worksheet.Cells[1, 15].Value = "Aging";


                worksheet.DefaultColWidth = 18f;



                //Add the each row
                for (int rowIndex = 0, row = 2; rowIndex < InternalReport.Count; rowIndex++, row++) // row indicates number of rows
                {

                    worksheet.Cells[row, 1].Value = InternalReport[rowIndex].EmployeeID;

                    worksheet.Cells[row, 2].Value = InternalReport[rowIndex].EmployeeName;


                    worksheet.Cells[row, 3].Value = InternalReport[rowIndex].Worklocation;
                    worksheet.Cells[row, 4].Value = InternalReport[rowIndex].site;
                    worksheet.Cells[row, 5].Value = InternalReport[rowIndex].Grade;


                    worksheet.Cells[row, 6].Value = InternalReport[rowIndex].DOJ;
                    worksheet.Cells[row, 6].Style.Numberformat.Format = "mm/dd/yyyy";
                    worksheet.Cells[row, 6].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                    worksheet.Cells[row, 7].Value = InternalReport[rowIndex].RelievingDate;
                    worksheet.Cells[row, 7].Style.Numberformat.Format = "mm/dd/yyyy";
                    worksheet.Cells[row, 7].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;



                    worksheet.Cells[row, 8].Value = InternalReport[rowIndex].ClientName;
                    worksheet.Cells[row, 9].Value = InternalReport[rowIndex].BusinessGroup;
                    worksheet.Cells[row, 10].Value = InternalReport[rowIndex].ProjectCode;
                    worksheet.Cells[row, 11].Value = InternalReport[rowIndex].ProjectName;
                    worksheet.Cells[row, 12].Value = InternalReport[rowIndex].EmployeeType;
                    //worksheet.Cells[row, 10].Value = InternalReport[rowIndex].StandaredHours;

                    worksheet.Cells[row, 13].Value = InternalReport[rowIndex].StartDate;
                    worksheet.Cells[row, 13].Style.Numberformat.Format = "mm/dd/yyyy";
                    worksheet.Cells[row, 13].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                    worksheet.Cells[row, 14].Value = InternalReport[rowIndex].HC;

                    worksheet.Cells[row, 15].Value = InternalReport[rowIndex].Aging;







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
                // Response.Buffer is not available in ASP.NET Core
                Response.Headers.Add("content-disposition", "attachment;filename=" + "Internal-Report" + DateTime.Now.ToString("_dd-MMM-yyyy_hh-mm-ss") + ".xlsx");

                // Response.Charset is not available in ASP.NET Core
                Response.ContentType = "application/vnd.ms-excel";
                StringWriter sw = new StringWriter();
                await Response.Body.WriteAsync(fileBytes, 0, fileBytes.Length);
                // Response.End() is not available in ASP.NET Core
            }

            #endregion

            return new EmptyResult();
        }

        public ActionResult FinanceContractorsReport()
        {
            return View();
        }
        public ActionResult ContractorsSearchData(DateTime StartDate, DateTime EndDate)
        {


            //var emp = new FinanceReports();

            //emp.BenchReports = GetBenchReport(StartDate, EndDate);
            return PartialView("_ContractorReportView", GetContractorsReport(StartDate, EndDate));

        }

        private List<FinanceContractorsDetails> GetContractorsReport(DateTime StartDate, DateTime EndDate)
        {

            List<FinanceContractorsDetails> ContractorsReport = new List<FinanceContractorsDetails>();

            var ContractorsReportresult = db.Set<FinanceContractorsDetails>().FromSqlRaw("exec sp_FinanceContractorsReport @StartDate,@EndDate", new SqlParameter("startDate", StartDate), new SqlParameter("endDate", EndDate)).ToList<FinanceContractorsDetails>();

            return ContractorsReportresult;
        }
        public ActionResult ExportContractorsReport(DateTime StartDate, DateTime EndDate)
        {
            return Json("success");
        }
        public async Task<ActionResult> GenerateContractorsReport(DateTime StartDate, DateTime EndDate)
        {

            var ContractorsReport = GetContractorsReport(StartDate, EndDate);
            ViewData["ContractorsReport"] = ContractorsReport;

            #region Export to Excel

            using (ExcelPackage package = new ExcelPackage())
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Contractors Report");
                worksheet.TabColor = System.Drawing.Color.Green;
                worksheet.DefaultRowHeight = 18f;
                worksheet.Row(1).Height = 20f;

                using (var range = worksheet.Cells[1, 1, 1, 21])
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
                worksheet.Cells[1, 1].Value = "EmployeeID";

                worksheet.Cells[1, 2].Value = "EmployeeName";
                worksheet.Cells[1, 3].Value = "WorkLocation";
                worksheet.Cells[1, 4].Value = "Site";
                worksheet.Cells[1, 5].Value = "Grade";
                worksheet.Cells[1, 6].Value = "ClientName";
                worksheet.Cells[1, 7].Value = "BusinessGroup";
                worksheet.Cells[1, 8].Value = "ProjectCode";
                worksheet.Cells[1, 9].Value = "Projectname";
                worksheet.Cells[1, 10].Value = "BillingStatus";
                worksheet.Cells[1, 11].Value = "DOJ";
                worksheet.Cells[1, 12].Value = "RelievingDate";
                worksheet.Cells[1, 13].Value = "ABillable";
                worksheet.Cells[1, 14].Value = "ANBillable";
                worksheet.Cells[1, 15].Value = "UABillable";
                worksheet.Cells[1, 16].Value = "UANBillable";

                worksheet.Cells[1, 17].Value = "TotalBillable";
                worksheet.Cells[1, 18].Value = "TotalNonBillable";

                worksheet.Cells[1, 19].Value = "StandardHours";
                worksheet.Cells[1, 20].Value = "Utilization";

                worksheet.Cells[1, 21].Value = "Aging";


                worksheet.DefaultColWidth = 18f;



                //Add the each row
                for (int rowIndex = 0, row = 2; rowIndex < ContractorsReport.Count; rowIndex++, row++) // row indicates number of rows
                {

                    worksheet.Cells[row, 1].Value = ContractorsReport[rowIndex].EmployeeID;

                    worksheet.Cells[row, 2].Value = ContractorsReport[rowIndex].EmployeeName;


                    worksheet.Cells[row, 3].Value = ContractorsReport[rowIndex].Worklocation;
                    worksheet.Cells[row, 4].Value = ContractorsReport[rowIndex].site;
                    worksheet.Cells[row, 5].Value = ContractorsReport[rowIndex].Grade;
                    worksheet.Cells[row, 6].Value = ContractorsReport[rowIndex].ClientName;
                    worksheet.Cells[row, 7].Value = ContractorsReport[rowIndex].BusinessGroup;
                    worksheet.Cells[row, 8].Value = ContractorsReport[rowIndex].ProjectCode;
                    worksheet.Cells[row, 9].Value = ContractorsReport[rowIndex].ProjectName;
                    worksheet.Cells[row, 10].Value = ContractorsReport[rowIndex].BillingStatus;

                    worksheet.Cells[row, 11].Value = ContractorsReport[rowIndex].DOJ;
                    worksheet.Cells[row, 11].Style.Numberformat.Format = "mm/dd/yyyy";
                    worksheet.Cells[row, 11].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                    worksheet.Cells[row, 12].Value = ContractorsReport[rowIndex].RelievingDate;
                    worksheet.Cells[row, 12].Style.Numberformat.Format = "mm/dd/yyyy";
                    worksheet.Cells[row, 12].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                    worksheet.Cells[row, 13].Value = ContractorsReport[rowIndex].ABillable;
                    worksheet.Cells[row, 14].Value = ContractorsReport[rowIndex].ANBillable;
                    worksheet.Cells[row, 15].Value = ContractorsReport[rowIndex].UABillable;
                    worksheet.Cells[row, 16].Value = ContractorsReport[rowIndex].UANBillable;
                    worksheet.Cells[row, 17].Value = ContractorsReport[rowIndex].TotalBillable;
                    worksheet.Cells[row, 18].Value = ContractorsReport[rowIndex].TotalNonBillable;
                    worksheet.Cells[row, 19].Value = ContractorsReport[rowIndex].TotalHours;
                    worksheet.Cells[row, 20].Value = ContractorsReport[rowIndex].Utilization;


                    worksheet.Cells[row, 21].Value = ContractorsReport[rowIndex].Aging;







                    if (row % 2 == 1)
                    {
                        using (var range = worksheet.Cells[row, 1, row, 21])
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
                // Response.Buffer is not available in ASP.NET Core
                Response.Headers.Add("content-disposition", "attachment;filename=" + "Contractors-Report" + DateTime.Now.ToString("_dd-MMM-yyyy_hh-mm-ss") + ".xlsx");

                // Response.Charset is not available in ASP.NET Core
                Response.ContentType = "application/vnd.ms-excel";
                StringWriter sw = new StringWriter();
                await Response.Body.WriteAsync(fileBytes, 0, fileBytes.Length);
                // Response.End() is not available in ASP.NET Core
            }

            #endregion

            return new EmptyResult();
        }

        public ActionResult FinanceInternalspcReport()
        {

            return View();
        }

        public ActionResult InternalspcSearchData(DateTime StartDate, DateTime EndDate)
        {


            //var emp = new FinanceReports();

            //emp.BenchReports = GetBenchReport(StartDate, EndDate);
            return PartialView("_InternalSpecificReportView", GetInternalspcReport(StartDate, EndDate));

        }

        private List<FinanceInternalspecificDetails> GetInternalspcReport(DateTime StartDate, DateTime EndDate)
        {

            List<FinanceInternalspecificDetails> InternalspcReport = new List<FinanceInternalspecificDetails>();

            var InternalspcReportresult = db.Set<FinanceInternalspecificDetails>().FromSqlRaw("exec sp_FinanceInternalSpecificReport @StartDate,@EndDate", new SqlParameter("startDate", StartDate), new SqlParameter("endDate", EndDate)).ToList<FinanceInternalspecificDetails>();

            return InternalspcReportresult;
        }
        public ActionResult ExportInternalspcReport(DateTime StartDate, DateTime EndDate)
        {
            return Json("success");
        }
        public async Task<ActionResult> GenerateInternalspcReport(DateTime StartDate, DateTime EndDate)
        {
            var InternalspcReport = GetInternalspcReport(StartDate, EndDate);
            ViewData["InternalSpcReport"] = InternalspcReport;

            #region Export to Excel

            using (ExcelPackage package = new ExcelPackage())
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Internal-Specific Report");
                worksheet.TabColor = System.Drawing.Color.Green;
                worksheet.DefaultRowHeight = 18f;
                worksheet.Row(1).Height = 20f;

                using (var range = worksheet.Cells[1, 1, 1, 15])
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
                worksheet.Cells[1, 1].Value = "EmployeeID";

                worksheet.Cells[1, 2].Value = "EmployeeName";
                worksheet.Cells[1, 3].Value = "WorkLocation";
                worksheet.Cells[1, 4].Value = "Site";
                worksheet.Cells[1, 5].Value = "Grade";
                worksheet.Cells[1, 6].Value = "DOJ";
                worksheet.Cells[1, 7].Value = "RelievingDate";
                worksheet.Cells[1, 8].Value = "ClientName";
                worksheet.Cells[1, 9].Value = "BusinessGroup";
                worksheet.Cells[1, 10].Value = "ProjectCode";
                worksheet.Cells[1, 11].Value = "Projectname";
                worksheet.Cells[1, 12].Value = "EmployeeType";
                //worksheet.Cells[1, 10].Value = "StandardHours";
                worksheet.Cells[1, 13].Value = "RollonDate";
                worksheet.Cells[1, 14].Value = "HC";
                worksheet.Cells[1, 15].Value = "Aging";


                worksheet.DefaultColWidth = 18f;



                //Add the each row
                for (int rowIndex = 0, row = 2; rowIndex < InternalspcReport.Count; rowIndex++, row++) // row indicates number of rows
                {

                    worksheet.Cells[row, 1].Value = InternalspcReport[rowIndex].EmployeeID;

                    worksheet.Cells[row, 2].Value = InternalspcReport[rowIndex].EmployeeName;


                    worksheet.Cells[row, 3].Value = InternalspcReport[rowIndex].Worklocation;
                    worksheet.Cells[row, 4].Value = InternalspcReport[rowIndex].site;
                    worksheet.Cells[row, 5].Value = InternalspcReport[rowIndex].Grade;

                    worksheet.Cells[row, 6].Value = InternalspcReport[rowIndex].DOJ;
                    worksheet.Cells[row, 6].Style.Numberformat.Format = "mm/dd/yyyy";
                    worksheet.Cells[row, 6].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                    worksheet.Cells[row, 7].Value = InternalspcReport[rowIndex].RelievingDate;
                    worksheet.Cells[row, 7].Style.Numberformat.Format = "mm/dd/yyyy";
                    worksheet.Cells[row, 7].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                    worksheet.Cells[row, 8].Value = InternalspcReport[rowIndex].ClientName;
                    worksheet.Cells[row, 9].Value = InternalspcReport[rowIndex].BusinessGroup;
                    worksheet.Cells[row, 10].Value = InternalspcReport[rowIndex].ProjectCode;
                    worksheet.Cells[row, 11].Value = InternalspcReport[rowIndex].ProjectName;
                    worksheet.Cells[row, 12].Value = InternalspcReport[rowIndex].EmployeeType;
                    //worksheet.Cells[row, 10].Value = InternalReport[rowIndex].StandaredHours;

                    worksheet.Cells[row, 13].Value = InternalspcReport[rowIndex].StartDate;
                    worksheet.Cells[row, 13].Style.Numberformat.Format = "mm/dd/yyyy";
                    worksheet.Cells[row, 13].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                    worksheet.Cells[row, 14].Value = InternalspcReport[rowIndex].HC;

                    worksheet.Cells[row, 15].Value = InternalspcReport[rowIndex].Aging;







                    if (row % 2 == 1)
                    {
                        using (var range = worksheet.Cells[row, 1, row, 15])
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
                // Response.Buffer is not available in ASP.NET Core
                Response.Headers.Add("content-disposition", "attachment;filename=" + "Internal-SpecificReport" + DateTime.Now.ToString("_dd-MMM-yyyy_hh-mm-ss") + ".xlsx");

                // Response.Charset is not available in ASP.NET Core
                Response.ContentType = "application/vnd.ms-excel";
                StringWriter sw = new StringWriter();
                await Response.Body.WriteAsync(fileBytes, 0, fileBytes.Length);
                // Response.End() is not available in ASP.NET Core
            }

            #endregion

            return new EmptyResult();
        }


        public ActionResult FinanceRawdataReport()
        {

            return View();
        }

        public ActionResult RawDataSearchData(DateTime StartDate, DateTime EndDate)
        {

            return PartialView("_RawDataReportView", GetRawDataReport(StartDate, EndDate));

        }

        private List<FinanceRawDataDetails> GetRawDataReport(DateTime StartDate, DateTime EndDate)
        {

            List<FinanceRawDataDetails> rawdataReport = new List<FinanceRawDataDetails>();

            var rawdataReportresult = db.Set<FinanceRawDataDetails>().FromSqlRaw("exec sp_FinanceRawDataReport @StartDate,@EndDate", new SqlParameter("startDate", StartDate), new SqlParameter("endDate", EndDate)).ToList<FinanceRawDataDetails>();

            return rawdataReportresult;
        }
        public ActionResult ExportRawDataReport(DateTime StartDate, DateTime EndDate)
        {
            return Json("success");
        }
        public async Task<ActionResult> GenerateRawDataReport(DateTime StartDate, DateTime EndDate)
        {
            var RawDataReport = GetRawDataReport(StartDate, EndDate);
            ViewData["RawDataReport"] = RawDataReport;

            #region Export to Excel

            using (ExcelPackage package = new ExcelPackage())
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("RawData Report");
                worksheet.TabColor = System.Drawing.Color.Green;
                worksheet.DefaultRowHeight = 18f;
                worksheet.Row(1).Height = 20f;

                using (var range = worksheet.Cells[1, 1, 1, 32])
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
                worksheet.Cells[1, 1].Value = "EmployeeID";
                worksheet.Cells[1, 2].Value = "EmployeeName";
                worksheet.Cells[1, 3].Value = "Grade";
                worksheet.Cells[1, 4].Value = "EmployeeType";
                worksheet.Cells[1, 5].Value = "LocationType";
                worksheet.Cells[1, 6].Value = "Location";
                worksheet.Cells[1, 7].Value = "ClientName";
                worksheet.Cells[1, 8].Value = "BusinessGroup";
                worksheet.Cells[1, 9].Value = "ProjectCode";
                worksheet.Cells[1, 10].Value = "Projectname";
                worksheet.Cells[1, 11].Value = "BillingType";
                worksheet.Cells[1, 12].Value = "HC";
                worksheet.Cells[1, 13].Value = "BillingStatus";
                worksheet.Cells[1, 14].Value = "DOJ";
                worksheet.Cells[1, 15].Value = "RelievingDate";
                worksheet.Cells[1, 16].Value = "CostCenter";
                worksheet.Cells[1, 17].Value = "ServiceLine";
                worksheet.Cells[1, 18].Value = "BEorGTS";
                worksheet.Cells[1, 19].Value = "StartDate";
                worksheet.Cells[1, 20].Value = "EndDate";
                worksheet.Cells[1, 21].Value = "ProjectStartDate";
                worksheet.Cells[1, 22].Value = "ProjectEndDate";
                worksheet.Cells[1, 23].Value = "ProjectSL";
                worksheet.Cells[1, 24].Value = "MappedDate";
                worksheet.Cells[1, 25].Value = "ExpectedBillingDate";
                worksheet.Cells[1, 26].Value = "ABillable";
                worksheet.Cells[1, 27].Value = "ANBillable";
                worksheet.Cells[1, 28].Value = "UABillable";
                worksheet.Cells[1, 29].Value = "UANBillable";
                worksheet.Cells[1, 30].Value = "TotalBillable";
                worksheet.Cells[1, 31].Value = "TotalNonBillable";
                worksheet.Cells[1, 32].Value = "TotalHours";


                worksheet.DefaultColWidth = 18f;



                //Add the each row
                for (int rowIndex = 0, row = 2; rowIndex < RawDataReport.Count; rowIndex++, row++) // row indicates number of rows
                {

                    worksheet.Cells[row, 1].Value = RawDataReport[rowIndex].EmployeeID;

                    worksheet.Cells[row, 2].Value = RawDataReport[rowIndex].EmployeeName;


                    worksheet.Cells[row, 3].Value = RawDataReport[rowIndex].Grade;
                    worksheet.Cells[row, 4].Value = RawDataReport[rowIndex].EmployeeType;
                    worksheet.Cells[row, 5].Value = RawDataReport[rowIndex].LocationType;

                    worksheet.Cells[row, 6].Value = RawDataReport[rowIndex].Location;
                    //worksheet.Cells[row, 6].Style.Numberformat.Format = "dd-MMM-yyyy";
                    //worksheet.Cells[row, 6].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                    worksheet.Cells[row, 7].Value = RawDataReport[rowIndex].ClientName;
                    worksheet.Cells[row, 8].Value = RawDataReport[rowIndex].BusinessGroup;
                    worksheet.Cells[row, 9].Value = RawDataReport[rowIndex].ProjectCode;
                    worksheet.Cells[row, 10].Value = RawDataReport[rowIndex].ProjectName;
                    worksheet.Cells[row, 11].Value = RawDataReport[rowIndex].BillingType;
                    //worksheet.Cells[row, 10].Value = InternalReport[rowIndex].StandaredHours;

                    worksheet.Cells[row, 12].Value = RawDataReport[rowIndex].HC;
                    //worksheet.Cells[row, 11].Style.Numberformat.Format = "dd-MMM-yyyy";
                    //worksheet.Cells[row, 11].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                    worksheet.Cells[row, 13].Value = RawDataReport[rowIndex].BillingStatus;

                    worksheet.Cells[row, 14].Value = RawDataReport[rowIndex].DOJ;
                    worksheet.Cells[row, 14].Style.Numberformat.Format = "MM/dd/yyyy";
                    worksheet.Cells[row, 14].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                    worksheet.Cells[row, 15].Value = RawDataReport[rowIndex].RelievingDate;
                    worksheet.Cells[row, 15].Style.Numberformat.Format = "MM/dd/yyyy";
                    worksheet.Cells[row, 15].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                    worksheet.Cells[row, 16].Value = RawDataReport[rowIndex].CostCenter;
                    worksheet.Cells[row, 17].Value = RawDataReport[rowIndex].ServiceLine;
                   
                    worksheet.Cells[row, 18].Value = RawDataReport[rowIndex].BEorGTS;

                    worksheet.Cells[row, 19].Value = RawDataReport[rowIndex].StartDate;
                    worksheet.Cells[row, 19].Style.Numberformat.Format = "MM/dd/yyyy";
                    worksheet.Cells[row, 19].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                    worksheet.Cells[row, 20].Value = RawDataReport[rowIndex].EndDate;
                    worksheet.Cells[row, 20].Style.Numberformat.Format = "MM/dd/yyyy";
                    worksheet.Cells[row, 20].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                    worksheet.Cells[row, 21].Value = RawDataReport[rowIndex].ProjectStartDate;
                    worksheet.Cells[row, 21].Style.Numberformat.Format = "MM/dd/yyyy";
                    worksheet.Cells[row, 21].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                    worksheet.Cells[row, 22].Value = RawDataReport[rowIndex].ProjectEndDate;
                    worksheet.Cells[row, 22].Style.Numberformat.Format = "MM/dd/yyyy";
                    worksheet.Cells[row, 22].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                    worksheet.Cells[row, 23].Value = RawDataReport[rowIndex].ProjectSL;

                    worksheet.Cells[row, 24].Value = RawDataReport[rowIndex].MappedDate;
                    worksheet.Cells[row, 24].Style.Numberformat.Format = "MM/dd/yyyy";
                    worksheet.Cells[row, 24].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                    worksheet.Cells[row, 25].Value = RawDataReport[rowIndex].ExpectedBillingDate;
                    worksheet.Cells[row, 25].Style.Numberformat.Format = "MM/dd/yyyy";
                    worksheet.Cells[row, 25].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                    worksheet.Cells[row, 26].Value = RawDataReport[rowIndex].ABillable;
                    worksheet.Cells[row, 27].Value = RawDataReport[rowIndex].ANBillable;
                    worksheet.Cells[row, 28].Value = RawDataReport[rowIndex].UABillable;
                    worksheet.Cells[row, 29].Value = RawDataReport[rowIndex].UANBillable;
                    worksheet.Cells[row, 30].Value = RawDataReport[rowIndex].TotalBillable;
                    worksheet.Cells[row, 31].Value = RawDataReport[rowIndex].TotalNonBillable;
                    worksheet.Cells[row, 32].Value = RawDataReport[rowIndex].TotalHours;







                    if (row % 2 == 1)
                    {
                        using (var range = worksheet.Cells[row, 1, row, 32])
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
                // Response.Buffer is not available in ASP.NET Core
                Response.Headers.Add("content-disposition", "attachment;filename=" + "Raw-Data" + DateTime.Now.ToString("_dd-MMM-yyyy_hh-mm-ss") + ".xlsx");

                // Response.Charset is not available in ASP.NET Core
                Response.ContentType = "application/vnd.ms-excel";
                StringWriter sw = new StringWriter();
                await Response.Body.WriteAsync(fileBytes, 0, fileBytes.Length);
                // Response.End() is not available in ASP.NET Core
            }

            #endregion

            return new EmptyResult();
        }

        public ActionResult FinanceUtilizationReport()
        {

            return View();
        }

        public ActionResult UtilizationSearchData(DateTime StartDate, DateTime EndDate)
        {

            return PartialView("_UtilizationReportView", GetUtilizationReport(StartDate, EndDate));

        }

        private List<FinanceUtilizationDetails> GetUtilizationReport(DateTime StartDate, DateTime EndDate)
        {

            var UtilizationReportresult = db.Set<FinanceUtilizationDetails>().FromSqlRaw("exec sp_UtilizationReport @StartDate,@EndDate", new SqlParameter("startDate", StartDate), new SqlParameter("endDate", EndDate)).ToList<FinanceUtilizationDetails>();
            HttpContext.Session.SetString("UtilizationReportresult", JsonConvert.SerializeObject(UtilizationReportresult));
            return UtilizationReportresult;
        }
        public ActionResult ExportUtilizationReport(DateTime StartDate, DateTime EndDate)
        {
            return Json("success");
        }
        //public ActionResult GenerateUtilizationReport()
        //{
        //    //var UtilizationReport = GetUtilizationReport(StartDate, EndDate);
        //    //ViewData["UtilizationReport"] = UtilizationReport;


        //    //List<FinanceUtilizationDetails> ett = new List<FinanceUtilizationDetails>();
        //    //Session["Errors"] = TempData["errodata"];
        //    //ett = (List<FinanceUtilizationDetails>)Session["Errors"];


        //    GridView gv = new GridView();
        //    gv.DataSource = Session["UtilizationReportresult"];
        //    gv.DataBind();

        //    return new DownloadFileActionResult((GridView)gv, "Utilization-Report.xls");


        //}
    }

}