using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Trianz.Enterprise.Operations.Models;
using OfficeOpenXml;
using Trianz.Enterprise.Operations.ViewModel;
using Trianz.Enterprise.Operations.General;

using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Session;
namespace Trianz.Enterprise.Operations.Controllers
{
    public class BenchEmployeeController : Controller
    {
        TrianzOperationsEntities _db = new TrianzOperationsEntities();

        TrianzOperationsEntities Db = new TrianzOperationsEntities();
     
        public ActionResult BenchEmployeeView()
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                try
                {
                    List<string> Categories = new List<string>() { "Deployable Bench", "NonDeployable Bench", "Reserved" };
                    ViewData["_Categories"] = Categories;
                    List<string> AccountName = new List<string>();
                    AccountName = (from project in _db.Projects
                                   where project.SOWEndDate >= DateTime.Now
                                   orderby project.AccountName
                                   select project.AccountName).Distinct().ToList();
                    AccountName.Add("Other");
                    ViewData["_AccountName"] = AccountName;

                    //newly added code
                  
                    var AllProjects = (from pro in _db.Projects
                                           // where pro.IsActive == true
                                       where pro.IsActive == true && pro.SOWEndDate >= DateTime.Now
                                       select new 
                                       {
                                           Projectcode = pro.ProjectCode,
                                           ProjectName = pro.ProjectCode + " " + pro.ProjectName
                                       }).OrderBy(o => o.ProjectName).ToList();
                    ViewData["AllProjects"]  = AllProjects;
                   
                    var GetOpportunitieswithcode = (from data in _db.Opportunities.Where(opp => opp.sales_Stage.Replace("- ", "") == "Stage 6 Close Won"
                                                   || opp.sales_Stage.Replace("- ", "") == "Stage 5 Likely Booking" || opp.sales_Stage.Replace("- ", "") == "Stage 4 Negotiate"
                                                   || opp.sales_Stage.Replace("- ", "") == "Stage 2 Develop" || opp.sales_Stage.Replace("- ", "") == "Stage 0 Prospect"
                                                   || opp.sales_Stage.Replace("- ", "") == "Stage 1 Engage" || opp.sales_Stage.Replace("- ", "") == "Stage 3 Solution" && opp.sales_Stage != "Closed Dropped")
                                            select new 
                                            {
                                                potential_no = data.potential_no,
                                                potentialname = data.potential_no + " " + data.potentialname
                                            }).OrderBy(x => x.potentialname).ToList();
             
                   ViewData["Opportunitieswithcode"] = new SelectList(GetOpportunitieswithcode, "potential_no", "potentialname");
                    //newly added code                 
                    List<BenchResourceViewModel> BenchResourceViewModelList = new List<BenchResourceViewModel>();
                    BenchResourceViewModelList = Db.Set<BenchResourceViewModel>().FromSqlRaw("exec sp_GetBenchResourceDetails").ToList();


                    foreach (var item in BenchResourceViewModelList)
                    {
                        if (item.ReservedOppName != null & item.ReservedProjCodeName != null)
                        {
                            ViewData["Opportunitieswithcode" + item.EmployeeId] = new SelectList(GetOpportunitieswithcode, "potential_no", "potentialname", item.ReservedOppCode);
                            ViewData["AllProjects" + item.EmployeeId] = new SelectList(AllProjects, "Projectcode", "ProjectName", item.ReservedProjCode);
                        }
                        else if (item.ReservedOppName != null & item.ReservedProjCodeName == null)
                        { 
                                ViewData["Opportunitieswithcode" + item.EmployeeId] = new SelectList(GetOpportunitieswithcode, "potential_no", "potentialname", item.ReservedOppCode);
                                ViewData["AllProjects" + item.EmployeeId] = new SelectList(AllProjects, "Projectcode", "ProjectName", null);
                        }

                        else if (item.ReservedOppName == null & item.ReservedProjCodeName != null)
                        {
                            ViewData["Opportunitieswithcode" + item.EmployeeId] = new SelectList(GetOpportunitieswithcode, "potential_no", "potentialname",null);
                            ViewData["AllProjects" + item.EmployeeId] = new SelectList(AllProjects, "Projectcode", "ProjectName", item.ReservedProjCode);
                        }
                        else
                        {
                            ViewData["Opportunitieswithcode" + item.EmployeeId] = new SelectList(GetOpportunitieswithcode, "potential_no", "potentialname", null);
                            ViewData["AllProjects" + item.EmployeeId] = new SelectList(AllProjects, "Projectcode", "ProjectName", null); ;
                        }
                    }
                   
                    return View(BenchResourceViewModelList);
                }
                catch (Exception ex)
                {
                    Common.WriteExceptionErrorLog(ex);
                    return RedirectToAction("Error", "Error");
                    //return Json("Error");
                }
            }
            else
            {
                //ermsg = "Session expired";
                //return Json("Sessionexpired");
                return RedirectToAction("SessionExpire", "Signout");
            }
        }
        

       //[HttpGet]
       // public ActionResult GetOppurtunityByEmployeeID(int Employeeid)
       // {

       //     if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
       //     {
       //         try
       //         {
       //             List<BenchResourceViewModel> BenchResourceViewModelList = new List<BenchResourceViewModel>();
       //             BenchResourceViewModelList = Db.Set<BenchResourceViewModel>().FromSqlRaw("exec sp_GetBenchResourceDetails").ToList();
       //             var oppurtunitycodewithname = BenchResourceViewModelList.Where(i => i.EmployeeId == Employeeid).Select(i => i.ReservedOppCodeName).FirstOrDefault();
       //             var oppurtintyval = "";
       //             if (oppurtunitycodewithname != null)
       //             {
       //                 oppurtintyval = oppurtunitycodewithname;
       //             }
       //             else
       //             {
       //                 oppurtintyval = "";
       //             }
       //             return Json(oppurtintyval);
       //         }
       //         catch (Exception ex)
       //         {
       //             Common.WriteExceptionErrorLog(ex);
       //             // return RedirectToAction("Error", "Error");
       //             return Json("Error");
       //         }
       //     }
       //     else
       //     {
       //          return Json("Sessionexpired");
       //     }

       // }
        public ActionResult ExportReport()
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                try
                {
                    return Json("success");
                }
                catch (Exception ex)
                {
                    Common.WriteExceptionErrorLog(ex);
                   // return RedirectToAction("Error", "Error");
                    return Json("Error");
                }
            }
            else
            {
                //ermsg = "Session expired";
                return Json("Sessionexpired");
                //return RedirectToAction("SessionExpire", "Signout");
            }
        }
        
                  public ActionResult GenerateProjectInfo()
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                try
                {
                    string Role = (string)HttpContext.Session.GetString("Role");
                    // List<BenchEmployees> BenchEmployeesInfo = new List<BenchEmployees>();
                    List<BenchResourceViewModel> BenchResourceViewModelList = new List<BenchResourceViewModel>();
                    BenchResourceViewModelList = Db.Set<BenchResourceViewModel>().FromSqlRaw("exec sp_GetBenchResourceDetails").ToList();
                    //foreach (BenchResourceViewModel emp in BenchResourceViewModelList)
                    //{
                    //    string non1 = "0";
                    //    non1 = (Convert.ToDateTime(DateTime.Now) - Convert.ToDateTime(emp.StartDate)).TotalDays.ToString();
                    //    decimal non = 0;
                    //    non = Math.Round(Convert.ToDecimal(non1), 0);
                    //    if (non <= 30)
                    //        emp.Ageing = "0-30";
                    //    else if (non > 30 && non < 60)
                    //        emp.Ageing = "30-60";
                    //    else if (non > 60 && non < 90)
                    //        emp.Ageing = "60-90";
                    //    else if (non > 90 && non < 120)
                    //        emp.Ageing = "90-120";
                    //    else if (non > 120 && non < 150)
                    //        emp.Ageing = "120-150";
                    //    else if (non > 150 && non < 180)
                    //        emp.Ageing = "150-180";
                    //    else if (non > 180)
                    //        emp.Ageing = ">180";
                    //    emp.NonBillableCountDays = non.ToString();
                    //    if (emp.Supervisor != "")
                    //    {
                    //        Employee objSupervisorEmp = Db.Employees.Find(Convert.ToInt32(emp.Supervisor));
                    //        if (objSupervisorEmp != null)
                    //        {
                    //            emp.Supervisor = (objSupervisorEmp.FirstName.Trim() + (!string.IsNullOrWhiteSpace(objSupervisorEmp.MiddleName) ? " " + objSupervisorEmp.MiddleName.Trim() + " " : " ") + objSupervisorEmp.LastName.Trim());
                    //        }
                    //        else
                    //        {
                    //            emp.Supervisor = " ";
                    //        }
                    //    }

                    //}            

                    #region Export to Excel

                    using (ExcelPackage package = new ExcelPackage())
            {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Project Information");
                        worksheet.TabColor = System.Drawing.Color.Green;
                        worksheet.DefaultRowHeight = 18f;
                        worksheet.Row(1).Height = 20f;

                        using (var range = worksheet.Cells[1, 1, 1, 36])
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
                        worksheet.Cells[1, 1].Value = "Employee ID";
                        worksheet.Cells[1, 2].Value = "Employee Name";
                        worksheet.Cells[1, 3].Value = "Grade";
                       // worksheet.Cells[1, 4].Value = "Employee Practice";
                        worksheet.Cells[1, 4].Value = "Practice A";


                        worksheet.Cells[1, 5].Value = "LoctaionType";
                        worksheet.Cells[1, 6].Value = "Location";
                        worksheet.Cells[1, 7].Value = "Project Code";
                        worksheet.Cells[1, 8].Value = "Project Name";
                        worksheet.Cells[1, 9].Value = "ProjectManager";
                        worksheet.Cells[1, 10].Value = "Supervisor";
                        worksheet.Cells[1, 11].Value = "Start Date";
                        worksheet.Cells[1, 12].Value = "End Date";
                        worksheet.Cells[1, 13].Value = "Utilization";
                        worksheet.Cells[1, 14].Value = "Bench Ageing";
                        worksheet.Cells[1, 15].Value = "TRNumber";
                        worksheet.Cells[1, 16].Value = "Bench Category";
                        worksheet.Cells[1, 17].Value = "Bench Sub-Category";
                        worksheet.Cells[1, 18].Value = "Client Mapping";
                        worksheet.Cells[1, 19].Value = "Opportunity No / Reserved Project Code";
                        worksheet.Cells[1, 20].Value = "Opporuntiy Name/ Reserved Project Name";

                        worksheet.Cells[1, 21].Value = "Skills";
                        worksheet.Cells[1, 22].Value = "Bench Remarks";
			   
                        worksheet.Cells[1, 23].Value = "Remarks Updated Date";
                        worksheet.Cells[1, 24].Value = "Resume Status(Word)";
                        worksheet.Cells[1, 25].Value = "Resume(Word) Submitted Date";
                        worksheet.Cells[1, 26].Value = "Resume Status(PPT)";
                        worksheet.Cells[1, 27].Value = "Resume(PPT) Submitted Date";
				 
					   worksheet.Cells[1, 28].Value = "Expected Billing Date";
                        worksheet.Cells[1, 29].Value = "Mapped Date";
                        worksheet.Cells[1, 30].Value = "Reminder Date";
                        worksheet.Cells[1, 31].Value = "Reserved ProjectCode";
                        worksheet.Cells[1, 32].Value = "NonBillableCountDays";
                        worksheet.Cells[1, 33].Value = "Head Count";
                        worksheet.Cells[1, 34].Value = "Date Of Join";
                        worksheet.Cells[1, 35].Value = "Business Group";
                        worksheet.Cells[1, 36].Value = "Employee Practice";

                        // worksheet.Cells[1, 34].Value = "Skills";
                        //worksheet.Cells[1, 35].Value = "Skill";
                        // worksheet.Cells[1, 34].Value = "Reserved ProjectCode";
                        //worksheet.Cells[1, 34].Value = "Reserved ProjectName";
                        //worksheet.Cells[1, 35].Value = "Reserved OppCode";
                        //worksheet.Cells[1, 36].Value = "Reserved OppName";


                        worksheet.Column(1).Width = 16f;
                        worksheet.Column(2).AutoFit(34f);
                        worksheet.Column(3).AutoFit(12f);
                        worksheet.Column(4).AutoFit(20f);
                        worksheet.Column(5).AutoFit(18f);
                        worksheet.Column(6).AutoFit(18f);
                        worksheet.Column(7).AutoFit(16f);
                        worksheet.Column(8).AutoFit(30f);
                        worksheet.Column(9).AutoFit(24f);
                        worksheet.Column(10).AutoFit(24f);
                        worksheet.Column(11).AutoFit(22f);
                        worksheet.Column(12).AutoFit(22f);
                        worksheet.Column(13).AutoFit(10f);
                        worksheet.Column(14).AutoFit(10f);
                        worksheet.Column(15).AutoFit(22f);
                        worksheet.Column(16).AutoFit(26f);
                        worksheet.Column(17).AutoFit(26f);
                        worksheet.Column(18).AutoFit(50f);
                        worksheet.Column(19).AutoFit(25f);
                        worksheet.Column(20).AutoFit(32f);

                        worksheet.Column(21).AutoFit(70f);
                        worksheet.Column(22).AutoFit(84f);
                        worksheet.Column(23).AutoFit(20f);
                        worksheet.Column(24).AutoFit(20f);
                        worksheet.Column(25).AutoFit(20f);
                        worksheet.Column(26).AutoFit(20f);
                        worksheet.Column(27).AutoFit(20f);
                        worksheet.Column(28).AutoFit(20f);
                        worksheet.Column(29).AutoFit(20f);
                        worksheet.Column(30).AutoFit(20f);
                        worksheet.Column(31).AutoFit(20f);
                        worksheet.Column(32).AutoFit(20f);
                        worksheet.Column(33).AutoFit(16f);
                         worksheet.Column(34).AutoFit(20f);
                        worksheet.Column(35).AutoFit(32f);
                        worksheet.Column(36).AutoFit(20f);
                        //worksheet.Column(37).AutoFit(24f);
                        //worksheet.Column(38).AutoFit(28f);
                       // worksheet.Column(37).AutoFit(28f);

                        //Add the each row
                        for (int rowIndex = 0, row = 2; rowIndex < BenchResourceViewModelList.Count; rowIndex++, row++) // row indicates number of rows
                        {
                            worksheet.Cells[row, 1].Value = BenchResourceViewModelList[rowIndex].EmployeeId;
                            worksheet.Cells[row, 2].Value = BenchResourceViewModelList[rowIndex].EmployeeName;
                            worksheet.Cells[row, 3].Value = BenchResourceViewModelList[rowIndex].Grade;
                            worksheet.Cells[row, 4].Value = BenchResourceViewModelList[rowIndex].PracticeA;

                           // worksheet.Cells[row, 5].Value = BenchResourceViewModelList[rowIndex].LocationType;
                            if (BenchResourceViewModelList[rowIndex].LocationType == "ONSITE")
                            {
                                worksheet.Cells[row, 5].Value = "ONSHORE";
                            }
                            else
                            {
                                worksheet.Cells[row, 5].Value = BenchResourceViewModelList[rowIndex].LocationType;
                            }
                            worksheet.Cells[row, 6].Value = BenchResourceViewModelList[rowIndex].Location;
                            worksheet.Cells[row, 7].Value = BenchResourceViewModelList[rowIndex].ProjectCode;
                            worksheet.Cells[row, 8].Value = BenchResourceViewModelList[rowIndex].ProjectName;
                            worksheet.Cells[row, 9].Value = BenchResourceViewModelList[rowIndex].ProjectManager;
                            worksheet.Cells[row, 10].Value = BenchResourceViewModelList[rowIndex].Supervisor;
                            worksheet.Cells[row, 11].Value = BenchResourceViewModelList[rowIndex].StartDate;
                            worksheet.Cells[row, 11].Style.Numberformat.Format = "dd-MMM-yyyy";
                            worksheet.Cells[row, 11].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                            worksheet.Cells[row, 12].Value = BenchResourceViewModelList[rowIndex].EndDate;
                            worksheet.Cells[row, 12].Style.Numberformat.Format = "dd-MMM-yyyy";
                            worksheet.Cells[row, 12].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                            worksheet.Cells[row, 13].Value = BenchResourceViewModelList[rowIndex].Utilization;
                            worksheet.Cells[row, 14].Value = BenchResourceViewModelList[rowIndex].Ageing;
                            worksheet.Cells[row, 15].Value = BenchResourceViewModelList[rowIndex].HRRFNumber;
                            worksheet.Cells[row, 16].Value = BenchResourceViewModelList[rowIndex].Category;
                            worksheet.Cells[row, 17].Value = BenchResourceViewModelList[rowIndex].Bechstatus;
                            worksheet.Cells[row, 18].Value = BenchResourceViewModelList[rowIndex].Customer;



                            if (BenchResourceViewModelList[rowIndex].ReservedOppCode == null || BenchResourceViewModelList[rowIndex].ReservedOppCode == "")
                            {
                                worksheet.Cells[row, 19].Value = BenchResourceViewModelList[rowIndex].ReservedProjCode;
                            }
                            else
                            {
                                worksheet.Cells[row, 19].Value = BenchResourceViewModelList[rowIndex].ReservedOppCode;
                            }
                            worksheet.Cells[row, 19].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;

                            if (BenchResourceViewModelList[rowIndex].ReservedOppName == null || BenchResourceViewModelList[rowIndex].ReservedOppName == "")
                            {
                                worksheet.Cells[row, 20].Value = BenchResourceViewModelList[rowIndex].ReservedProjName;
                            }
                            else
                            {
                                worksheet.Cells[row, 20].Value = BenchResourceViewModelList[rowIndex].ReservedOppName;
                            }
                            worksheet.Cells[row, 20].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;


                            worksheet.Cells[row, 21].Value = BenchResourceViewModelList[rowIndex].BenchSkills;
                            worksheet.Cells[row, 21].Style.WrapText = false;

                            worksheet.Cells[row, 22].Value = BenchResourceViewModelList[rowIndex].Remarks;

                            worksheet.Cells[row, 23].Value = BenchResourceViewModelList[rowIndex].Assigned_Date;
                            worksheet.Cells[row, 23].Style.Numberformat.Format = "dd-MMM-yyyy";
                            worksheet.Cells[row, 23].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                            worksheet.Cells[row, 24].Value = BenchResourceViewModelList[rowIndex].Resumestatus;
				            worksheet.Cells[row, 25].Value = BenchResourceViewModelList[rowIndex].ResumeUpdatedDate;
                            worksheet.Cells[row, 25].Style.Numberformat.Format = "dd-MMM-yyyy";
                            worksheet.Cells[row, 25].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;



                            worksheet.Cells[row, 26].Value = BenchResourceViewModelList[rowIndex].PPTResumestatus;
                            worksheet.Cells[row, 27].Value = BenchResourceViewModelList[rowIndex].ResumePdfUpdatedDate;
                            worksheet.Cells[row, 27].Style.Numberformat.Format = "dd-MMM-yyyy";
                            worksheet.Cells[row, 27].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;




                            worksheet.Cells[row, 28].Value = BenchResourceViewModelList[rowIndex].ExpectedBillingDate;
                            worksheet.Cells[row, 28].Style.Numberformat.Format = "dd-MMM-yyyy";
                            worksheet.Cells[row, 28].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                            worksheet.Cells[row, 29].Value = BenchResourceViewModelList[rowIndex].MappedDate;
                            worksheet.Cells[row, 29].Style.Numberformat.Format = "dd-MMM-yyyy";
                            worksheet.Cells[row, 29].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                            worksheet.Cells[row, 30].Value = BenchResourceViewModelList[rowIndex].ActionReminder;
                            worksheet.Cells[row, 30].Style.Numberformat.Format = "dd-MMM-yyyy";
                            worksheet.Cells[row, 30].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                            worksheet.Cells[row, 31].Value = BenchResourceViewModelList[rowIndex].ReservedProjCode;
                            worksheet.Cells[row, 32].Value = BenchResourceViewModelList[rowIndex].NonBillableCountDays;
                            worksheet.Cells[row, 32].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;


                            worksheet.Cells[row, 33].Value = BenchResourceViewModelList[rowIndex].Headcount;

                            worksheet.Cells[row, 34].Value = BenchResourceViewModelList[rowIndex].DateOfJoin;
                            worksheet.Cells[row, 34].Style.Numberformat.Format = "dd-MMM-yyyy";
                            worksheet.Cells[row, 34].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                            worksheet.Cells[row, 35].Value = BenchResourceViewModelList[rowIndex].BusinessGroup;
                            worksheet.Cells[row, 36].Value = BenchResourceViewModelList[rowIndex].Practice;


                            if (row % 2 == 1)
                            {
                                using (var range = worksheet.Cells[row, 1, row, 36])
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
                        
                        Response.Headers.Add("content-disposition", "attachment;filename=" + "Bench Resources" + DateTime.Now.ToString("_dd-MMM-yyyy_hh-mm-ss") + ".xlsx");

                        
                        Response.ContentType = "application/vnd.ms-excel";
                        //StringWriter sw = new StringWriter();
                        Response.Body.Write(fileBytes);
                        
                    }

                    #endregion
                    return new EmptyResult();
                }
                catch (Exception ex)
                {
                    Common.WriteExceptionErrorLog(ex);
                    return RedirectToAction("Error", "Error");
                    //return Json("Error");
                }
            }
            else
            {
                //ermsg = "Session expired";
               // return Json("Sessionexpired");
                return RedirectToAction("SessionExpire", "Signout");
            }
        }

        public ActionResult Savedetails(List<SelectedBenchEmployees> SelectedBenchEmployees)
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                try
                {

                foreach (var eachEmployee in SelectedBenchEmployees)
                {
                    Int32 EmpID = Convert.ToInt32(eachEmployee.EmployeeId);
                    Int32 AssignmentId = Convert.ToInt32(eachEmployee.AssignmentID);
                    var updatebenchemployees = _db.ProjectAssignments.Where
                        (b => b.Assignment_Id == AssignmentId).FirstOrDefault();
                    updatebenchemployees.HRRFNumber = eachEmployee.HRRFNumber;
                    updatebenchemployees.Category = eachEmployee.Category;
                    updatebenchemployees.Bechstatus = eachEmployee.BenchStatus;
                    updatebenchemployees.Customer = eachEmployee.Customer;
                    updatebenchemployees.Remarks = eachEmployee.Remarks;
                    updatebenchemployees.ExpectedBillingDate = eachEmployee.ExpectedBillingDate;
                    updatebenchemployees.MappedDate = eachEmployee.MappedDate;
                    updatebenchemployees.ActionReminder = eachEmployee.ActionReminder;
                    updatebenchemployees.ReservedProjectCode = eachEmployee.ReservedProjectCode;
                    updatebenchemployees.BenchSkills = eachEmployee.BenchSkills;
                        
                        var ReservedProName = _db.Projects.Where(b => b.ProjectCode == eachEmployee.ReservedProjCode && b.IsActive == true).Select(b => b.ProjectName).FirstOrDefault();
                        var ReservedOppName = _db.Opportunities.Where(b => b.potential_no == eachEmployee.ReservedOppCode).Select(b => b.potentialname).FirstOrDefault();
                        updatebenchemployees.ReservedOppCode = eachEmployee.ReservedOppCode;
                        updatebenchemployees.ReservedOppName = ReservedOppName;
                        updatebenchemployees.ReservedProjCode = eachEmployee.ReservedProjCode;
                        updatebenchemployees.ReservedProjName = ReservedProName;

                        _db.SaveChanges();

                }
                    return Json("true");

            }
                catch (Exception ex)
                {
                    Common.WriteExceptionErrorLog(ex);
                   // return RedirectToAction("Error", "Error");
                    return Json("Error");
                }

            }
            else
            {
                //ermsg = "Session expired";
                return Json("Sessionexpired");
                //return RedirectToAction("SessionExpire", "Signout");
            }
        }

        public ActionResult GetBenchStatus(string categoryvalue, string benchstatusvalue)
        {
            //if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            //{

                try
                {
                    List<string> BenchStatusList = new List<string>();
                    if (categoryvalue.ToLower() == "deployable bench")
                    {
                        BenchStatusList.Add("Free Pool");
                        BenchStatusList.Add("Partial Bench");
                    }
                    else if(categoryvalue.ToLower() == "nondeployable bench")
                    {
                        BenchStatusList.Add("BGV");
                        BenchStatusList.Add("Freshers");
                        BenchStatusList.Add("Long Leave");                
                        BenchStatusList.Add("Maternity Leave");
                        BenchStatusList.Add("Medical Leave");
                        BenchStatusList.Add("Resigned");
                        BenchStatusList.Add("Intern");

                }
                    else if (categoryvalue.ToLower() == "reserved")
                    {
                        BenchStatusList.Add("Mapped");
                        BenchStatusList.Add("Sow Pending");
                    }
                //ViewData["_Categories"] = BenchStatusList;
                //if (benchstatusvalue != null)
                //{
                //    foreach (string strBenchStatus in (List<string>)ViewData["_Categories"])
                //    {
                //        if (strBenchStatus == benchstatusvalue)
                //        {                                           

                //        }
                //    }
                //}
                return Json(BenchStatusList);

            }
            catch (Exception ex)
            {
                Common.WriteExceptionErrorLog(ex);
                return RedirectToAction("Error", "Error");
                //return Json("Error");
            }

        }

        ///Latest Code for BenchResource Report
        public ActionResult BenchResourceReportView()
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)

            {
                try
                {

                    List<BenchResourceDataViewModel> BenchResourceViewModelList = new List<BenchResourceDataViewModel>();
                    BenchResourceViewModelList = Db.Set<BenchResourceDataViewModel>().FromSqlRaw("exec sp_GetBenchResourceDetails").ToList();


                    return View(BenchResourceViewModelList);
                }
                catch (Exception ex)
                {
                    Common.WriteExceptionErrorLog(ex);
                    return RedirectToAction("Error", "Error");

                }
            }
            else
            {
                return RedirectToAction("SessionExpire", "Signout");
            }
        }
        public ActionResult ExportBenchReport()
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                try
                {
                    return Json("success");
                }
                catch (Exception ex)
                {
                    Common.WriteExceptionErrorLog(ex);
                    return Json("Error");
                }
            }
            else
            {
                return Json("Sessionexpired");
            }
        }
         public ActionResult GenerateBenchReport()
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                try
                {
                    string Role = (string)HttpContext.Session.GetString("Role");
                    List<BenchEmployees> BenchEmployeesInfo = new List<BenchEmployees>();
                    List<BenchResourceViewModel> BenchResourceViewModelList = new List<BenchResourceViewModel>();
                    BenchResourceViewModelList = Db.Set<BenchResourceViewModel>().FromSqlRaw("exec sp_GetBenchResourceListofDetails").ToList();


                    // hrr report 
                    List<HRRFReportDetails> TRReportViewModelList = new List<HRRFReportDetails>();
                    TRReportViewModelList = Db.Set<HRRFReportDetails>().FromSqlRaw("exec sp_GetHRRFReportDetails").ToList();

                    List<ProjectReleaseDetails> projectReleaseResourceList = new List<ProjectReleaseDetails>();
                    projectReleaseResourceList = Db.Set<ProjectReleaseDetails>().FromSqlRaw("exec sp_GetProjectReleaseDetails").ToList();

                    List<FutureReleasesData> FutureReleasesDatalist = new List<FutureReleasesData>();
                    FutureReleasesDatalist = Db.Set<FutureReleasesData>().FromSqlRaw("exec sp_GetFutureReleasesData").ToList();


                    #region Export to Excel

                    using (ExcelPackage package = new ExcelPackage())
            {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Bench Resource Report");
                        ExcelWorksheet trworksheet = package.Workbook.Worksheets.Add("TR Report");
                        ExcelWorksheet projectworksheet = package.Workbook.Worksheets.Add("Project Release Report");
                        ExcelWorksheet Releasesdataworksheet = package.Workbook.Worksheets.Add("Future Releases Report");

                        worksheet.TabColor = System.Drawing.Color.Green;
                        worksheet.DefaultRowHeight = 18f;
                        worksheet.Row(1).Height = 20f;

                        using (var range = worksheet.Cells[1, 1, 1, 38])
{
                        range.Style.Font.Bold = true;
                            range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                            range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#5B9BD5"));
                            range.Style.Font.Color.SetColor(System.Drawing.Color.White);

                            range.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                            range.Style.Border.Right.Color.SetColor(System.Drawing.Color.LightGray);
                        }


                       // worksheet.Cells[1, 1].Value = "Billing Status";
                        worksheet.Cells[1, 1].Value = "Bench Category";
                        worksheet.Cells[1, 2].Value = "Bench Sub-Category";
                        worksheet.Cells[1, 3].Value = "Non Billable Count Days";
                       worksheet.Cells[1, 4].Value = "Employee ID";
                        worksheet.Cells[1, 5].Value = "Employee Name";
                        worksheet.Cells[1, 6].Value = "Grade";
                        worksheet.Cells[1, 7].Value = "Practice A";
                        worksheet.Cells[1, 8].Value = "Loctaion Type";
                        worksheet.Cells[1, 9].Value = "Client Mapping";
                        worksheet.Cells[1, 10].Value = "Opportunity No / Reserved Project Code";
					    worksheet.Cells[1, 11].Value = "Opporuntiy Name/ Reserved Project Name";
			            worksheet.Cells[1, 12].Value = "Skills";
                        worksheet.Cells[1, 13].Value = "Bench Remarks";
                        worksheet.Cells[1, 14].Value = "Remarks Updated Date";
                        worksheet.Cells[1, 15].Value = "Resume Status(Word)";
                        worksheet.Cells[1, 16].Value = "Resume(Word) Submitted Date";
                        worksheet.Cells[1, 17].Value = "Resume Status(PPT)";
                        worksheet.Cells[1, 18].Value = "Resume(PPT) Submitted Date";												   
					    worksheet.Cells[1, 19].Value = "TR Number";
                        worksheet.Cells[1, 20].Value = "TR Proposed Date";
                        worksheet.Cells[1, 21].Value = "TL/DH feedback";
                        worksheet.Cells[1, 22].Value = "Feedback Updated Date";
                        worksheet.Cells[1, 23].Value = "Updated By";
                       // worksheet.Cells[1, 24].Value = "Reserved Project Code";
                        worksheet.Cells[1, 24].Value = "Project Code";
                        worksheet.Cells[1, 25].Value = "Project Name";
                        worksheet.Cells[1, 26].Value = "Project Manager";
                        worksheet.Cells[1, 27].Value = "Supervisor";
                        worksheet.Cells[1, 28].Value = "Start Date";
                        worksheet.Cells[1, 29].Value = "End Date";
                        worksheet.Cells[1, 30].Value = "Utilization";
                        worksheet.Cells[1, 31].Value = "Expected Billing Date";
                        worksheet.Cells[1, 32].Value = "Mapped Date";
                        worksheet.Cells[1, 33].Value = "Reminder Date";
                        worksheet.Cells[1, 34].Value = "Business Group";
                        worksheet.Cells[1, 35].Value = "Employee Practice";
                        worksheet.Cells[1, 36].Value = "Location";
                        worksheet.Cells[1, 37].Value = "Head Count";
                        worksheet.Cells[1, 38].Value = "Date Of Join";

                        //worksheet.Cells[1, 40].Value = "Reserved Project Code";
                        //worksheet.Cells[1, 41].Value = "Reserved Project Name";
                        //worksheet.Cells[1, 42].Value = "Reserved OppCode";
                        //worksheet.Cells[1, 43].Value = "Reserved OppName";


                        worksheet.Column(1).Width = 16f;
			   
				        worksheet.Column(2).AutoFit(24f);
                        worksheet.Column(3).AutoFit(12f);
                        worksheet.Column(4).AutoFit(20f);
                        worksheet.Column(5).AutoFit(12f);
                        worksheet.Column(6).AutoFit(12f);
                        worksheet.Column(7).AutoFit(10f);
                        worksheet.Column(8).AutoFit(20f);
                        worksheet.Column(9).AutoFit(40f);
                        worksheet.Column(10).AutoFit(25f);
                        worksheet.Column(11).AutoFit(32f);
                        worksheet.Column(12).AutoFit(60f);
                        worksheet.Column(13).AutoFit(27f);
                        worksheet.Column(14).AutoFit(24f);
                        worksheet.Column(15).AutoFit(14f);
                        worksheet.Column(16).AutoFit(10f);
                        worksheet.Column(17).AutoFit(18f);
                        worksheet.Column(18).AutoFit(18f);
                        worksheet.Column(19).AutoFit(18f);
                        worksheet.Column(20).AutoFit(24f);
                        worksheet.Column(21).AutoFit(28f);
                        worksheet.Column(22).AutoFit(20f);
                        worksheet.Column(23).AutoFit(14f);
                       // worksheet.Column(24).AutoFit(28f);
                        worksheet.Column(24).AutoFit(20f);
                        worksheet.Column(25).AutoFit(28f);
                        worksheet.Column(26).AutoFit(16f);
                        worksheet.Column(27).AutoFit(16f);
                        worksheet.Column(28).AutoFit(10f);
                        worksheet.Column(29).AutoFit(20f);
                        worksheet.Column(30).AutoFit(14f);
                        worksheet.Column(31).AutoFit(14f);
                        worksheet.Column(32).AutoFit(28f);
                        worksheet.Column(33).AutoFit(18f);
                        worksheet.Column(34).AutoFit(14f);
                        worksheet.Column(35).AutoFit(13f);
                        worksheet.Column(36).AutoFit(14f);
                        worksheet.Column(37).AutoFit(13f);
                        worksheet.Column(38).AutoFit(24f);
                        //worksheet.Column().AutoFit(24f);
                        //worksheet.Column(41).AutoFit(28f);
                        //worksheet.Column(42).AutoFit(24f);
                        //worksheet.Column(43).AutoFit(28f);

                        for (int rowIndex = 0, row = 2; rowIndex < BenchResourceViewModelList.Count; rowIndex++, row++) // row indicates number of rows
                        {
                            //worksheet.Cells[row, 1].Value = BenchResourceViewModelList[rowIndex].Bechstatus;
                            worksheet.Cells[row, 1].Value = BenchResourceViewModelList[rowIndex].Category;
                            worksheet.Cells[row, 2].Value = BenchResourceViewModelList[rowIndex].Bechstatus;
                            worksheet.Cells[row, 3].Value = BenchResourceViewModelList[rowIndex].NonBillableCountDays;
                            worksheet.Cells[row, 3].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                            worksheet.Cells[row, 4].Value = BenchResourceViewModelList[rowIndex].EmployeeId;
                            worksheet.Cells[row, 5].Value = BenchResourceViewModelList[rowIndex].EmployeeName;
                            worksheet.Cells[row, 6].Value = BenchResourceViewModelList[rowIndex].Grade;

																																												 
																																												  
						    worksheet.Cells[row, 7].Value = BenchResourceViewModelList[rowIndex].PracticeA;

                            worksheet.Cells[row, 8].Value = BenchResourceViewModelList[rowIndex].LocationType;

                            
                            if (BenchResourceViewModelList[rowIndex].LocationType == "ONSITE")
                            {
                                worksheet.Cells[row, 8].Value = "ONSHORE";
                            }
                            else
                            {
                                worksheet.Cells[row, 8].Value = BenchResourceViewModelList[rowIndex].LocationType;
                            }
                            worksheet.Cells[row, 9].Value = BenchResourceViewModelList[rowIndex].Customer;


                            if (BenchResourceViewModelList[rowIndex].ReservedOppCode == null || BenchResourceViewModelList[rowIndex].ReservedOppCode == "")
                            {
                                worksheet.Cells[row, 10].Value = BenchResourceViewModelList[rowIndex].ReservedProjCode;
                            }
                            else
                            {
                                worksheet.Cells[row, 10].Value = BenchResourceViewModelList[rowIndex].ReservedOppCode;
                            }
                            worksheet.Cells[row, 10].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;

                            if (BenchResourceViewModelList[rowIndex].ReservedOppName == null || BenchResourceViewModelList[rowIndex].ReservedOppName == "")
                            {
                                worksheet.Cells[row, 11].Value = BenchResourceViewModelList[rowIndex].ReservedProjName;
                            }
                            else
                            {
                                worksheet.Cells[row, 11].Value = BenchResourceViewModelList[rowIndex].ReservedOppName;
                            }

                            worksheet.Cells[row, 11].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                            worksheet.Cells[row, 12].Value = BenchResourceViewModelList[rowIndex].BenchSkills;
                            worksheet.Cells[row, 13].Value = BenchResourceViewModelList[rowIndex].Remarks;
                            worksheet.Cells[row, 14].Value = BenchResourceViewModelList[rowIndex].Assigned_Date;
                            worksheet.Cells[row, 14].Style.Numberformat.Format = "dd-MMM-yyyy";
                            worksheet.Cells[row, 14].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
																																				  
																																												 

					       //worksheet.Cells[row, 14].Value = BenchResourceViewModelList[rowIndex].ResumeSubmittedDate;
                            //worksheet.Cells[row, 15].Value = BenchResourceViewModelList[rowIndex].TRNumber;
                            //worksheet.Cells[row, 16].Value = BenchResourceViewModelList[rowIndex].TRProposedDate;
																																																						
																																																						
																																																									
					       //worksheet.Cells[row, 17].Value = BenchResourceViewModelList[rowIndex].TLORDHfeedback;
                            //worksheet.Cells[row, 18].Value = BenchResourceViewModelList[rowIndex].FeedbackUpdatedDate

                            worksheet.Cells[row, 15].Value = BenchResourceViewModelList[rowIndex].Resumestatus;
				            worksheet.Cells[row, 16].Value = BenchResourceViewModelList[rowIndex].ResumeUpdatedDate;
					        worksheet.Cells[row, 16].Style.Numberformat.Format = "dd-MMM-yyyy";
                            worksheet.Cells[row, 16].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
																																					  
                            worksheet.Cells[row, 17].Value = BenchResourceViewModelList[rowIndex].PPTResumestatus;
						    worksheet.Cells[row, 18].Value = BenchResourceViewModelList[rowIndex].ResumePdfUpdatedDate;
                            worksheet.Cells[row, 18].Style.Numberformat.Format = "dd-MMM-yyyy";
                            worksheet.Cells[row, 18].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                            //worksheet.Cells[row, 19].Value = BenchResourceViewModelList[rowIndex].TRNumber;
                            //worksheet.Cells[row, 20].Value = BenchResourceViewModelList[rowIndex].TRProposedDate;
                            //worksheet.Cells[row, 21].Value = BenchResourceViewModelList[rowIndex].TLORDHfeedback;
                            //worksheet.Cells[row, 22].Value = BenchResourceViewModelList[rowIndex].FeedbackUpdatedDate


                            worksheet.Cells[row, 23].Value = BenchResourceViewModelList[rowIndex].UpdatedBy;

                          //  worksheet.Cells[row, 24].Value = BenchResourceViewModelList[rowIndex].ReservedProjectCode;

                            worksheet.Cells[row, 24].Value = BenchResourceViewModelList[rowIndex].ProjectCode;
                            worksheet.Cells[row, 25].Value = BenchResourceViewModelList[rowIndex].ProjectName;

                            worksheet.Cells[row, 26].Value = BenchResourceViewModelList[rowIndex].ProjectManager;
                            worksheet.Cells[row, 27].Value = BenchResourceViewModelList[rowIndex].Supervisor;

                            worksheet.Cells[row, 28].Value = BenchResourceViewModelList[rowIndex].StartDate;
                            worksheet.Cells[row, 28].Style.Numberformat.Format = "dd-MMM-yyyy";
                            worksheet.Cells[row, 28].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
			               worksheet.Cells[row, 29].Value = BenchResourceViewModelList[rowIndex].EndDate;
				            worksheet.Cells[row, 29].Style.Numberformat.Format = "dd-MMM-yyyy";
                            worksheet.Cells[row, 29].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
			                worksheet.Cells[row, 30].Value = BenchResourceViewModelList[rowIndex].Utilization;
		                   worksheet.Cells[row, 31].Value = BenchResourceViewModelList[rowIndex].ExpectedBillingDate;
                            worksheet.Cells[row, 31].Style.Numberformat.Format = "dd-MMM-yyyy";
                            worksheet.Cells[row, 31].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
				            worksheet.Cells[row, 32].Value = BenchResourceViewModelList[rowIndex].MappedDate;
                            worksheet.Cells[row, 32].Style.Numberformat.Format = "dd-MMM-yyyy";
                            worksheet.Cells[row, 32].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                            worksheet.Cells[row, 33].Value = BenchResourceViewModelList[rowIndex].ActionReminder;
                            worksheet.Cells[row, 33].Style.Numberformat.Format = "dd-MMM-yyyy";
                            worksheet.Cells[row, 33].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                            worksheet.Cells[row, 34].Value = BenchResourceViewModelList[rowIndex].BusinessGroup;
                            worksheet.Cells[row, 35].Value = BenchResourceViewModelList[rowIndex].Practice;
                            worksheet.Cells[row, 36].Value = BenchResourceViewModelList[rowIndex].Location;
                            worksheet.Cells[row, 37].Value = BenchResourceViewModelList[rowIndex].Headcount;

                            worksheet.Cells[row, 38].Value = BenchResourceViewModelList[rowIndex].DateOfJoin;
                            worksheet.Cells[row, 38].Style.Numberformat.Format = "dd-MMM-yyyy";
                            worksheet.Cells[row, 38].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                           
                            if (row % 2 == 1)
                            {
                                using (var range = worksheet.Cells[row, 1, row, 38])
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



                        //ProjectRelaseReport
                        projectworksheet.TabColor = System.Drawing.Color.Green;
                        projectworksheet.DefaultRowHeight = 18f;
                        projectworksheet.Row(1).Height = 20f;
                        int LastRowforProjectreleasecount = 19;
                        using (var range = projectworksheet.Cells[1, 1, 1, LastRowforProjectreleasecount])
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
                        projectworksheet.Cells[1, 1].Value = "Employee Id";
                        projectworksheet.Cells[1, 2].Value = "Employee Name";
                        projectworksheet.Cells[1, 3].Value = "Practice A";
                        projectworksheet.Cells[1, 4].Value = "LoctaionType";
                        projectworksheet.Cells[1, 5].Value = "Skills";
                        projectworksheet.Cells[1, 6].Value = "Designation";
                        projectworksheet.Cells[1, 7].Value = "Project Code";
                        projectworksheet.Cells[1, 8].Value = "Project Name";
                        projectworksheet.Cells[1, 9].Value = "Release Date";
                        projectworksheet.Cells[1, 10].Value = "Client/Project";
                        projectworksheet.Cells[1, 11].Value = "Reason for Release";
                        projectworksheet.Cells[1, 12].Value = "Have discussed with resource on project movement (Y/N)";
                        projectworksheet.Cells[1, 13].Value = "Skills Good At";
                        projectworksheet.Cells[1, 14].Value = "Detailed Feedback";
                        projectworksheet.Cells[1, 15].Value = "Resource Performance";
                        projectworksheet.Cells[1, 16].Value = "Released By";
                        projectworksheet.Cells[1, 17].Value = "Released Audit Date";
                        projectworksheet.Cells[1, 18].Value = "Org Group";
                        projectworksheet.Cells[1, 19].Value = "Org Sub Group";
						


                        projectworksheet.Column(1).Width = 14f;
                        projectworksheet.Column(2).AutoFit(30f);
                        projectworksheet.Column(3).AutoFit(14f);
                        projectworksheet.Column(4).AutoFit(14f);
                        projectworksheet.Column(5).AutoFit(70f);
                        projectworksheet.Column(6).AutoFit(22f);
                        projectworksheet.Column(7).AutoFit(12f);
                        projectworksheet.Column(8).AutoFit(32f);
                        projectworksheet.Column(9).AutoFit(18f);
                        projectworksheet.Column(10).AutoFit(12f);
                        projectworksheet.Column(11).AutoFit(45f);
                        projectworksheet.Column(12).AutoFit(47f);
                        projectworksheet.Column(13).AutoFit(45f);
                        projectworksheet.Column(14).AutoFit(40f);
                        projectworksheet.Column(15).AutoFit(20f);
                        projectworksheet.Column(16).AutoFit(14f);
                        projectworksheet.Column(17).AutoFit(20f);
                        projectworksheet.Column(18).AutoFit(16f);
                        projectworksheet.Column(19).AutoFit(16f);


                        //Add the each row
                        for (int rowIndex = 0, row = 2; rowIndex < projectReleaseResourceList.Count; rowIndex++, row++) // row indicates number of rows
                        {

                            projectworksheet.Cells[row, 1].Value = projectReleaseResourceList[rowIndex].EmployeeId;
                            projectworksheet.Cells[row, 2].Value = projectReleaseResourceList[rowIndex].EmployeeName;
                            projectworksheet.Cells[row, 3].Value = projectReleaseResourceList[rowIndex].PracticeA;
                            projectworksheet.Cells[row, 4].Value = projectReleaseResourceList[rowIndex].LocationType;
                            projectworksheet.Cells[row, 5].Value = projectReleaseResourceList[rowIndex].Skills;
                            projectworksheet.Cells[row, 6].Value = projectReleaseResourceList[rowIndex].Designation;

                            projectworksheet.Cells[row, 7].Value = projectReleaseResourceList[rowIndex].ProjectCode;
                            projectworksheet.Cells[row, 8].Value = projectReleaseResourceList[rowIndex].ProjectName;


                            projectworksheet.Cells[row, 9].Value = projectReleaseResourceList[rowIndex].ReleaseDate;
                            projectworksheet.Cells[row, 9].Style.Numberformat.Format = "dd-MMM-yyyy";

                            projectworksheet.Cells[row, 10].Value = projectReleaseResourceList[rowIndex].ClientorProject;

                            projectworksheet.Cells[row, 11].Value = projectReleaseResourceList[rowIndex].ReasonForRelease;
                            projectworksheet.Cells[row, 12].Value = projectReleaseResourceList[rowIndex].IsDiscResourceOnProject;


                            projectworksheet.Cells[row, 13].Value = projectReleaseResourceList[rowIndex].SkillsGoodAt;
                            projectworksheet.Cells[row, 14].Value = projectReleaseResourceList[rowIndex].DetailedFeedBack;
                            projectworksheet.Cells[row, 15].Value = projectReleaseResourceList[rowIndex].ResourcePerformance;
                            projectworksheet.Cells[row, 16].Value = projectReleaseResourceList[rowIndex].ReleasedBy;

                            projectworksheet.Cells[row, 17].Value = projectReleaseResourceList[rowIndex].ReleaseAuditDate;
                            projectworksheet.Cells[row, 17].Style.Numberformat.Format = "dd-MMM-yyyy";

                            projectworksheet.Cells[row, 18].Value = projectReleaseResourceList[rowIndex].OrganisationGroup;

                            projectworksheet.Cells[row, 19].Value = projectReleaseResourceList[rowIndex].OrganisationSubGroup;

                            if (row % 2 == 1)
                            {
                                using (var range = projectworksheet.Cells[row, 1, row, 19])
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

                        // TR Report
                        trworksheet.TabColor = System.Drawing.Color.Green;
                        trworksheet.DefaultRowHeight = 18f;
                        trworksheet.Row(1).Height = 20f;
                        int LastRow = 46;
                        using (var range = trworksheet.Cells[1, 1, 1, LastRow])
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
                        trworksheet.Cells[1, 1].Value = "TR Number";
                        trworksheet.Cells[1, 2].Value = "Request Type";
                        trworksheet.Cells[1, 3].Value = "BU";
                        trworksheet.Cells[1, 4].Value = "Org Group";
                        trworksheet.Cells[1, 5].Value = "Org Sub Group";
                        trworksheet.Cells[1, 6].Value = "Practice A";
                        trworksheet.Cells[1, 7].Value = "Location Type";
                        trworksheet.Cells[1, 8].Value = "Account Name";
                        trworksheet.Cells[1, 9].Value = "Raised by";
                        trworksheet.Cells[1, 10].Value = "Validated By";
                        trworksheet.Cells[1, 11].Value = "Skill";
                        trworksheet.Cells[1, 12].Value = "Role Required";
                        trworksheet.Cells[1, 13].Value = "Grade";
                        trworksheet.Cells[1, 14].Value = "Criticality";
                        trworksheet.Cells[1, 15].Value = "Reason For Raising TR (Brief Description)";
                        trworksheet.Cells[1, 16].Value = "Submitted Date";
                        trworksheet.Cells[1, 17].Value = "Ageing";
                        trworksheet.Cells[1, 18].Value = "Current Status ";
                        trworksheet.Cells[1, 19].Value = "Remark";
                        trworksheet.Cells[1, 20].Value = "Updated on ";
                        trworksheet.Cells[1, 21].Value = "Updated By";
                        trworksheet.Cells[1, 22].Value = "TR Remarks";
                        trworksheet.Cells[1, 23].Value = "TR Candidate ";
                        trworksheet.Cells[1, 24].Value = "TR DOJ";
                        trworksheet.Cells[1, 25].Value = "Skill2";
                        trworksheet.Cells[1, 26].Value = "TA Lead";
                        trworksheet.Cells[1, 27].Value = "Recruiter";
                        trworksheet.Cells[1, 28].Value = "First Level Technical Panel";
                        trworksheet.Cells[1, 29].Value = "Second Level Technical Panel";
                        trworksheet.Cells[1, 30].Value = "Client Interview";
                        trworksheet.Cells[1, 31].Value = "Opportunity NO/ Project Code";
                        trworksheet.Cells[1, 32].Value = "Opporuntiy Name/ Project Name";
                        trworksheet.Cells[1, 33].Value = "Sales Stage";
                        trworksheet.Cells[1, 34].Value = "Assignment Start Date";
                        trworksheet.Cells[1, 35].Value = "Billing Date";
                        trworksheet.Cells[1, 36].Value = "Replaced EmpID";
                        trworksheet.Cells[1, 37].Value = "Replaced EmpName";
                        trworksheet.Cells[1, 38].Value = "Skill Cluster";
                        trworksheet.Cells[1, 39].Value = "Skill Code";
                        trworksheet.Cells[1, 40].Value = "Practice";
                        trworksheet.Cells[1, 41].Value = "Location Name";
                        trworksheet.Cells[1, 42].Value = "Employment Type";
                        trworksheet.Cells[1, 43].Value = "BO SPOC";
                        trworksheet.Cells[1, 44].Value = "Request Status";
                        trworksheet.Cells[1, 45].Value = "DH Approval Date";
                        trworksheet.Cells[1, 46].Value = "Impact";

                        trworksheet.Column(1).Width = 16f;
                        trworksheet.Column(2).AutoFit(17f);
                        trworksheet.Column(3).AutoFit(18f);
                        trworksheet.Column(4).AutoFit(19f);
                        trworksheet.Column(5).AutoFit(18f);
                        trworksheet.Column(6).AutoFit(18f);
                        trworksheet.Column(7).AutoFit(17f);
                        trworksheet.Column(8).AutoFit(37f);
                        trworksheet.Column(9).AutoFit(32f);
                        trworksheet.Column(10).AutoFit(24f);
                        trworksheet.Column(11).AutoFit(45f);
                        trworksheet.Column(12).AutoFit(32f);
                        trworksheet.Column(13).AutoFit(10f);
                        trworksheet.Column(14).AutoFit(10f);
                        trworksheet.Column(15).AutoFit(36f);
                        trworksheet.Column(16).AutoFit(27f);
                        trworksheet.Column(17).AutoFit(12f);
                        trworksheet.Column(18).AutoFit(24f);
                        trworksheet.Column(19).AutoFit(20f);
                        trworksheet.Column(20).AutoFit(20f);
                        trworksheet.Column(21).AutoFit(32f);
                        trworksheet.Column(22).AutoFit(20f);
                        trworksheet.Column(23).AutoFit(20f);
                        trworksheet.Column(24).AutoFit(20f);
                        trworksheet.Column(25).AutoFit(20f);
                        trworksheet.Column(26).AutoFit(19f);
                        trworksheet.Column(27).AutoFit(20f);
                        trworksheet.Column(28).AutoFit(32f);
                        trworksheet.Column(29).AutoFit(20f);
                        trworksheet.Column(30).AutoFit(12f);
                        trworksheet.Column(31).AutoFit(24f);
                        trworksheet.Column(32).AutoFit(14f);
                        trworksheet.Column(33).AutoFit(20f);
                        trworksheet.Column(34).AutoFit(22f);
                        trworksheet.Column(35).AutoFit(20f);
                        trworksheet.Column(36).AutoFit(20f);
                        trworksheet.Column(37).AutoFit(25f);
                        trworksheet.Column(38).AutoFit(20f);
                        trworksheet.Column(39).AutoFit(20f);
                        trworksheet.Column(40).AutoFit(18f);
                        trworksheet.Column(41).AutoFit(18f);
                        trworksheet.Column(42).AutoFit(20f);
                        trworksheet.Column(43).AutoFit(20f);
                        trworksheet.Column(44).AutoFit(24f);
                        trworksheet.Column(45).AutoFit(24f);
                        trworksheet.Column(46).AutoFit(19f);
                        //Add the each row
                        for (int rowIndex = 0, row = 2; rowIndex < TRReportViewModelList.Count; rowIndex++, row++) // row indicates number of rows
                        {

                            trworksheet.Cells[row, 1].Value = TRReportViewModelList[rowIndex].HRRFNumber;
                            trworksheet.Cells[row, 2].Value = TRReportViewModelList[rowIndex].RequestType;
                            //  trworksheet.Cells[row, 3].Value = TRReportViewModelList[rowIndex].BU;
                            trworksheet.Cells[row, 4].Value = TRReportViewModelList[rowIndex].OrganizationGroup;
                            trworksheet.Cells[row, 5].Value = TRReportViewModelList[rowIndex].OrganizationSubGroup;
                            trworksheet.Cells[row, 6].Value = TRReportViewModelList[rowIndex].PracticeA;

                            trworksheet.Cells[row, 7].Value = TRReportViewModelList[rowIndex].LocationType;
                            trworksheet.Cells[row, 8].Value = TRReportViewModelList[rowIndex].AccountName;


                            trworksheet.Cells[row, 9].Value = TRReportViewModelList[rowIndex].UpdatedBy;
                            //trworksheet.Cells[row, 10].Value = TRReportViewModelList[rowIndex].ValidatedBy;

                            trworksheet.Cells[row, 11].Value = TRReportViewModelList[rowIndex].CSkill;
                            trworksheet.Cells[row, 12].Value = TRReportViewModelList[rowIndex].RoleRequired;


                            trworksheet.Cells[row, 13].Value = TRReportViewModelList[rowIndex].Grade;
                            trworksheet.Cells[row, 14].Value = TRReportViewModelList[rowIndex].Criticality;
                            trworksheet.Cells[row, 15].Value = TRReportViewModelList[rowIndex].ReasonRaisingTR;


                            trworksheet.Cells[row, 16].Value = TRReportViewModelList[rowIndex].HRRFSubmitedDate;
                            trworksheet.Cells[row, 16].Style.Numberformat.Format = "dd-MMM-yyyy";

                            trworksheet.Cells[row, 17].Value = TRReportViewModelList[rowIndex].Ageing;

                            trworksheet.Cells[row, 18].Value = TRReportViewModelList[rowIndex].CurrentStatus;

                            trworksheet.Cells[row, 19].Value = TRReportViewModelList[rowIndex].Remarks;
                            if (TRReportViewModelList[rowIndex].UpdatedOn == null)
                            {
                                trworksheet.Cells[row, 20].Value = "";
                            }
                            else
                            {
                                trworksheet.Cells[row, 20].Value = TRReportViewModelList[rowIndex].UpdatedOn;
                            }
                            trworksheet.Cells[row, 20].Style.Numberformat.Format = "dd-MMM-yyyy";

                            // trworksheet.Cells[row, 20].Value = TRReportViewModelList[rowIndex].UpdatedOn;
                            trworksheet.Cells[row, 21].Value = TRReportViewModelList[rowIndex].UpdatedBy;
                            // trworksheet.Cells[row, 19].Style.Numberformat.Format = "dd-MMM-yyyy";

                            //  trworksheet.Cells[row, 20].Value = TRReportViewModelList[rowIndex].TRRemarks;

                            if (TRReportViewModelList[rowIndex].TRRemarks == null)
                            {
                                trworksheet.Cells[row, 22].Value = "";
                            }
                            else
                            {
                                trworksheet.Cells[row, 22].Value = TRReportViewModelList[rowIndex].TRRemarks;
                            }
                            //  trworksheet.Cells[row, 23].Value = TRReportViewModelList[rowIndex].TRCandidate;
                            //trworksheet.Cells[row, 24].Value = TRReportViewModelList[rowIndex].TRDOJ;
                            //  trworksheet.Cells[row, 25].Value = TRReportViewModelList[rowIndex].Skill2;                           
                            //   trworksheet.Cells[row, 25].Value = TRReportViewModelList[rowIndex].;
                            if (TRReportViewModelList[rowIndex].TALeadName == null)
                            {
                                trworksheet.Cells[row, 26].Value = "";
                            }
                            else
                            {
                                trworksheet.Cells[row, 26].Value = TRReportViewModelList[rowIndex].TALeadName;
                            }
                            if (TRReportViewModelList[rowIndex].Recruitername == null)
                            {
                                trworksheet.Cells[row, 27].Value = "";
                            }
                            else
                            {
                                trworksheet.Cells[row, 27].Value = TRReportViewModelList[rowIndex].Recruitername;
                            }
                            trworksheet.Cells[row, 28].Value = TRReportViewModelList[rowIndex].FirstLevelTechnicalPanel;
                            trworksheet.Cells[row, 29].Value = TRReportViewModelList[rowIndex].SECONDTECHPANEL;
                            trworksheet.Cells[row, 30].Value = TRReportViewModelList[rowIndex].ClientInterview;

                            trworksheet.Cells[row, 31].Value = TRReportViewModelList[rowIndex].ProjectCode;
                            trworksheet.Cells[row, 32].Value = TRReportViewModelList[rowIndex].ProjectName;
                            trworksheet.Cells[row, 33].Value = TRReportViewModelList[rowIndex].Stage;
                            trworksheet.Cells[row, 34].Value = TRReportViewModelList[rowIndex].AssignmentStartDate;
                            trworksheet.Cells[row, 34].Style.Numberformat.Format = "dd-MMM-yyyy";

                            trworksheet.Cells[row, 35].Value = TRReportViewModelList[rowIndex].BillingDate;
                            trworksheet.Cells[row, 35].Style.Numberformat.Format = "dd-MMM-yyyy";
                            if (TRReportViewModelList[rowIndex].ReplacementEmpID == null)
                            {
                                trworksheet.Cells[row, 36].Value = "";
                            }
                            else
                            {
                                trworksheet.Cells[row, 36].Value = TRReportViewModelList[rowIndex].ReplacementEmpID;
                            }
                            if (TRReportViewModelList[rowIndex].ReplacementName == null)
                            {
                                trworksheet.Cells[row, 37].Value = "";
                            }
                            else
                            {
                                trworksheet.Cells[row, 37].Value = TRReportViewModelList[rowIndex].ReplacementName;
                            }

                            trworksheet.Cells[1, 38].Value = "Skill Cluster";
                            trworksheet.Cells[1, 39].Value = "Skill Code";
                            trworksheet.Cells[1, 40].Value = "Practice";
                            trworksheet.Cells[1, 41].Value = "Location Name";
                            trworksheet.Cells[1, 42].Value = "Employment Type";
                            trworksheet.Cells[1, 43].Value = "BO SPOC";
                            trworksheet.Cells[1, 44].Value = "Request Status";
                            trworksheet.Cells[1, 45].Value = "DH Approval Date";
                            trworksheet.Cells[1, 46].Value = "Impact";



                            trworksheet.Cells[row, 38].Value = TRReportViewModelList[rowIndex].SkillCluster;
                            trworksheet.Cells[row, 39].Value = TRReportViewModelList[rowIndex].SkillCode;
                            trworksheet.Cells[row, 40].Value = TRReportViewModelList[rowIndex].Practice;
                            trworksheet.Cells[row, 41].Value = TRReportViewModelList[rowIndex].LocationName;

                            //trworksheet.Cells[1, 42].Value = "Employment Type";
                            // trworksheet.Cells[row, 42].Value = TRReportViewModelList[rowIndex].EmploymentType;
                            if (TRReportViewModelList[rowIndex].BoSpoc == null)
                            {
                                trworksheet.Cells[row, 43].Value = "";
                            }
                            else
                            {
                                trworksheet.Cells[row, 43].Value = TRReportViewModelList[rowIndex].BoSpoc;
                            }
                            //    trworksheet.Cells[row, 38].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                            trworksheet.Cells[row, 44].Value = TRReportViewModelList[rowIndex].RequestStatus;

                            trworksheet.Cells[row, 45].Value = TRReportViewModelList[rowIndex].DHApprovaerDate;
                            trworksheet.Cells[row, 45].Style.Numberformat.Format = "dd-MMM-yyyy";
                            trworksheet.Cells[row, 46].Value = TRReportViewModelList[rowIndex].Impact;

                            if (row % 2 == 1)
                            {
                                using (var range = trworksheet.Cells[row, 1, row, 46])
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

                        Releasesdataworksheet.TabColor = System.Drawing.Color.Green;
                        Releasesdataworksheet.DefaultRowHeight = 18f;
                        Releasesdataworksheet.Row(1).Height = 20f;

                        using (var range = Releasesdataworksheet.Cells[1, 1, 1, 18])
{
                        range.Style.Font.Bold = true;
                            range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                            range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#5B9BD5"));
                            range.Style.Font.Color.SetColor(System.Drawing.Color.White);

                            range.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                            range.Style.Border.Right.Color.SetColor(System.Drawing.Color.LightGray);
                        }


                        Releasesdataworksheet.Cells[1, 1].Value = "Employee ID";
                        Releasesdataworksheet.Cells[1, 2].Value = "Employee Name";
                        Releasesdataworksheet.Cells[1, 3].Value = "Grade";
                        Releasesdataworksheet.Cells[1, 4].Value = "Org Group";
                        Releasesdataworksheet.Cells[1, 5].Value = "Org Sub Group";
                        Releasesdataworksheet.Cells[1, 6].Value = "Practice A";
                        Releasesdataworksheet.Cells[1, 7].Value = "LoctaionType";
                        Releasesdataworksheet.Cells[1, 8].Value = "Project Code";
                        Releasesdataworksheet.Cells[1, 9].Value = "Project Name";
                        Releasesdataworksheet.Cells[1, 10].Value = "Skills";
                        Releasesdataworksheet.Cells[1, 11].Value = "SOW End Date ";
                        Releasesdataworksheet.Cells[1, 12].Value = "SOW Status";
                        Releasesdataworksheet.Cells[1, 13].Value = "Roll Off Date";
                        Releasesdataworksheet.Cells[1, 14].Value = "Project Manager";
                        Releasesdataworksheet.Cells[1, 15].Value = "Delivery Manager";
                        Releasesdataworksheet.Cells[1, 16].Value = "Supervisor";
                        Releasesdataworksheet.Cells[1, 17].Value = "Resume";
                        Releasesdataworksheet.Cells[1, 18].Value = "Resume updated date";




                        Releasesdataworksheet.Column(1).Width = 15f;
                        Releasesdataworksheet.Column(2).AutoFit(21f);
                        Releasesdataworksheet.Column(3).AutoFit(9f);
                        Releasesdataworksheet.Column(4).AutoFit(16f);
                        Releasesdataworksheet.Column(5).AutoFit(14f);
                        Releasesdataworksheet.Column(6).AutoFit(16f);
                        Releasesdataworksheet.Column(7).AutoFit(19f);
                        Releasesdataworksheet.Column(8).AutoFit(17f);
                        Releasesdataworksheet.Column(9).AutoFit(26f);
                        Releasesdataworksheet.Column(10).AutoFit(26f);
                        Releasesdataworksheet.Column(11).AutoFit(22f);
                        Releasesdataworksheet.Column(12).AutoFit(22f);
                        Releasesdataworksheet.Column(13).AutoFit(24f);
                        Releasesdataworksheet.Column(14).AutoFit(25f);
                        Releasesdataworksheet.Column(15).AutoFit(25f);
                        Releasesdataworksheet.Column(16).AutoFit(26f);
                        Releasesdataworksheet.Column(17).AutoFit(20f);
                        Releasesdataworksheet.Column(18).AutoFit(20f);


                        for (int rowIndex = 0, row = 2; rowIndex < FutureReleasesDatalist.Count; rowIndex++, row++) // row indicates number of rows
                        {
                            Releasesdataworksheet.Cells[1, 1].Value = "Employee ID";
                            Releasesdataworksheet.Cells[1, 2].Value = "Employee Name";
                            Releasesdataworksheet.Cells[1, 3].Value = "Grade";
                            Releasesdataworksheet.Cells[1, 4].Value = "Org Group";
                            Releasesdataworksheet.Cells[1, 5].Value = "Org Sub Group";
                            Releasesdataworksheet.Cells[1, 6].Value = "Practice A";
                            Releasesdataworksheet.Cells[1, 7].Value = "LoctaionType";
                            Releasesdataworksheet.Cells[1, 8].Value = "Project Code";
                            Releasesdataworksheet.Cells[1, 9].Value = "Project Name";
                            Releasesdataworksheet.Cells[1, 10].Value = "Skills";
                            Releasesdataworksheet.Cells[1, 11].Value = "SOW End Date ";
                            Releasesdataworksheet.Cells[1, 12].Value = "SOW Status";
                            Releasesdataworksheet.Cells[1, 13].Value = "Roll Off Date";
                            Releasesdataworksheet.Cells[1, 14].Value = "Project Manager";
                            Releasesdataworksheet.Cells[1, 15].Value = "Delivery Manager";
                            Releasesdataworksheet.Cells[1, 16].Value = "Supervisor";
                            Releasesdataworksheet.Cells[1, 17].Value = "Resume";
                            Releasesdataworksheet.Cells[1, 18].Value = "Resume updated date";

                            Releasesdataworksheet.Cells[row, 1].Value = FutureReleasesDatalist[rowIndex].EmployeeId;
                            Releasesdataworksheet.Cells[row, 2].Value = FutureReleasesDatalist[rowIndex].EmployeeName;
                            Releasesdataworksheet.Cells[row, 3].Value = FutureReleasesDatalist[rowIndex].Grade;
                            Releasesdataworksheet.Cells[row, 4].Value = FutureReleasesDatalist[rowIndex].OrganizationGroup;
                            Releasesdataworksheet.Cells[row, 5].Value = FutureReleasesDatalist[rowIndex].OrganizationSubGroup;
                            Releasesdataworksheet.Cells[row, 6].Value = FutureReleasesDatalist[rowIndex].PracticeA;
                            Releasesdataworksheet.Cells[row, 7].Value = FutureReleasesDatalist[rowIndex].LocationType;
                            Releasesdataworksheet.Cells[row, 8].Value = FutureReleasesDatalist[rowIndex].ProjectCode;
                            Releasesdataworksheet.Cells[row, 9].Value = FutureReleasesDatalist[rowIndex].ProjectName;
                            Releasesdataworksheet.Cells[row, 10].Value = FutureReleasesDatalist[rowIndex].Skills;
                            Releasesdataworksheet.Cells[row, 11].Value = FutureReleasesDatalist[rowIndex].EndDate;
                            Releasesdataworksheet.Cells[row, 11].Style.Numberformat.Format = "dd-MMM-yyyy";
                            // Releasesdataworksheet.Cells[row, 12].Value = FutureReleasesDatalist[rowIndex].SOWStatus;
							


                            Releasesdataworksheet.Cells[row, 13].Value = FutureReleasesDatalist[rowIndex].RollOffDate;
                            Releasesdataworksheet.Cells[row, 13].Style.Numberformat.Format = "dd-MMM-yyyy";

                            Releasesdataworksheet.Cells[row, 14].Value = FutureReleasesDatalist[rowIndex].ProjectManager;
                            Releasesdataworksheet.Cells[row, 15].Value = FutureReleasesDatalist[rowIndex].DeliveryManager;
                            Releasesdataworksheet.Cells[row, 16].Value = FutureReleasesDatalist[rowIndex].Supervisor;

                            if (FutureReleasesDatalist[rowIndex].ResumeFlag.ToString() == null)
                            {
                                Releasesdataworksheet.Cells[row, 17].Value = "NO";
                            }
                            else
                            {
                                Releasesdataworksheet.Cells[row, 17].Value = "Yes";
                            }
                            //  Releasesdataworksheet.Cells[row, 18].Value = FutureReleasesDatalist[rowIndex].ResumeupdatedDate;
                            if (row % 2 == 1)
                            {
                                using (var range = Releasesdataworksheet.Cells[row, 1, row, 18])
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
                        
                        Response.Headers.Add("content-disposition", "attachment;filename=" + "BO Report" + DateTime.Now.ToString("_dd-MMM-yyyy_hh-mm-ss") + ".xlsx");

                        
                        Response.ContentType = "application/vnd.ms-excel";
                        // StringWriter sw = new StringWriter();
                        Response.Body.Write(fileBytes);
                        
                    }

                    #endregion
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

                return RedirectToAction("SessionExpire", "Signout");
            }
        }
        ///Latest Code for BenchResource Report
    }
   
    public class SelectedBenchEmployees
    {
        public int EmployeeId { get; set; }
        public int AssignmentID { get; set; }
        public string HRRFNumber { get; set; }
        public string Category { get; set; }
        public string BenchStatus { get; set; }
        public string Customer { get; set; }
        public string Remarks { get; set; }
        public Nullable<System.DateTime> ExpectedBillingDate { get; set; }
        public Nullable<System.DateTime> MappedDate { get; set; }
        public Nullable<System.DateTime> ActionReminder { get; set; }
        public string ReservedProjectCode { get; set; }

        public string BenchSkills { get; set; }


        public string ReservedProjCode { get; set; }
        public string ReservedProName { get; set; }
        public string ReservedOppCode { get; set; }
        public string ReservedOppName { get; set; }
    }
}
