using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Trianz.Enterprise.Operations.Models;
using OfficeOpenXml;
using Trianz.Enterprise.Operations.General;

namespace Trianz.Enterprise.Operations.Controllers
{
    public class ProjectsMasterController : Controller
    {
        TrianzOperationsEntities db = new TrianzOperationsEntities();

        // GET: ProjectsMaster
        public ActionResult Index()

        {
            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            {
                try
                {

                    List<Project> lstProjects = db.Projects.Where(p => p.IsActive.Equals(true)).ToList();

                    return View(lstProjects);
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
        public ActionResult ExportReport()
        {
            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            {
                try
                {
                     return Json("success", JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    Common.WriteExceptionErrorLog(ex);
                    //return RedirectToAction("Error", "Error");
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
        public ActionResult GenerateProjectInfo(string strPractice, string strProject, string strBillingStatus)
        {
            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            {
                try
                {
                    string Role = (string)Session["Role"];

                    List<Project> ProjectsInfo = db.Projects.Where(p => p.IsActive.Equals(true)).ToList();

                    #region Export to Excel

                    using (ExcelPackage package = new ExcelPackage())
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Project Information");
                        worksheet.TabColor = System.Drawing.Color.Green;
                        worksheet.DefaultRowHeight = 18f;
                        worksheet.Row(1).Height = 20f;

                        using (var range = worksheet.Cells[1, 1, 1, 7])
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
                        worksheet.Cells[1, 1].Value = "Account Name";
                        worksheet.Cells[1, 2].Value = "Project Code";
                        worksheet.Cells[1, 3].Value = "Project Name";
                        worksheet.Cells[1, 4].Value = "SOW Start Date";
                        worksheet.Cells[1, 5].Value = "SOW End Date";
                        worksheet.Cells[1, 6].Value = "Project Manager";
                        worksheet.Cells[1, 7].Value = "Billing Type";

                        worksheet.Column(1).Width = 42f;
                        worksheet.Column(2).AutoFit(12f);
                        worksheet.Column(3).AutoFit(42f);
                        worksheet.Column(4).AutoFit(12f);
                        worksheet.Column(5).AutoFit(12f);
                        worksheet.Column(6).AutoFit(42f);
                        worksheet.Column(7).AutoFit(42f);


                        //Add the each row
                        for (int rowIndex = 0, row = 2; rowIndex < ProjectsInfo.Count; rowIndex++, row++) // row indicates number of rows
                        {
                            worksheet.Cells[row, 1].Value = ProjectsInfo[rowIndex].AccountName;
                            worksheet.Cells[row, 2].Value = ProjectsInfo[rowIndex].ProjectCode;
                            worksheet.Cells[row, 3].Value = ProjectsInfo[rowIndex].ProjectName;

                            worksheet.Cells[row, 4].Value = ProjectsInfo[rowIndex].SOWStartDate;
                            worksheet.Cells[row, 4].Style.Numberformat.Format = "dd-MMM-yyyy";
                            worksheet.Cells[row, 4].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                            worksheet.Cells[row, 5].Value = ProjectsInfo[rowIndex].SOWEndDate;
                            worksheet.Cells[row, 5].Style.Numberformat.Format = "dd-MMM-yyyy";
                            worksheet.Cells[row, 5].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                            worksheet.Cells[row, 6].Value = ProjectsInfo[rowIndex].ProjectManager;
                            worksheet.Cells[row, 7].Value = ProjectsInfo[rowIndex].BillingType;


                            if (row % 2 == 1)
                            {
                                using (var range = worksheet.Cells[row, 1, row, 7])
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
                        Response.AddHeader("content-disposition", "attachment;filename=" + "Project Master" + DateTime.UtcNow.ToString("_dd-MMM-yyyy_hh-mm-ss") + ".xlsx");

                        Response.Charset = "";
                        Response.ContentType = "application/vnd.ms-excel";
                        //StringWriter sw = new StringWriter();
                        Response.BinaryWrite(fileBytes);
                        Response.End();
                    }

                    #endregion

                    return new EmptyResult();
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
    }
}