using OfficeOpenXml;
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Trianz.Enterprise.Operations.Models;
using Trianz.Enterprise.Operations.ViewModel;

using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Session;
namespace Trianz.Enterprise.Operations.Controllers
{
    public class TeamTrackController : Controller
    {
        TrianzOperationsEntities _db = new TrianzOperationsEntities();
        // GET: TeamTrack

        public ActionResult TeamTrack()
        {
            try

            {
                //var tracks = (from em in _db.Employees
                //              where em.IsActive == true && em.Track != null
                //              orderby em.Track ascending
                //              select em.Track).Distinct().ToList();


                var tracks = (from em in _db.Employees
                              where em.IsActive == true && em.Track != null
                              orderby em.Track ascending
                              select em.Track).Distinct().ToList();

                var digitizationTracks = (from em in _db.TrackSkillSets
                                          where em.IsActive == true && em.Practice == "Digitization"
                                          orderby em.SkillName ascending
                                          select em.SkillName).Distinct().ToList();

                var analyticsTracks = (from em in _db.TrackSkillSets
                                       where em.IsActive == true && em.Practice == "Analytics"
                                       orderby em.SkillName ascending
                                       select em.SkillName).Distinct().ToList();

                var imsTracks = (from em in _db.TrackSkillSets
                                 where em.IsActive == true && em.Practice.Contains("IMS")
                                 orderby em.SkillName ascending
                                 select em.SkillName).Distinct().ToList();

                var cloudSolutionTracks = (from em in _db.TrackSkillSets
                                           where em.IsActive == true && em.Practice == "Cloud Solutions"
                                           orderby em.SkillName ascending
                                           select em.SkillName).Distinct().ToList();

                ViewBag.Tracks = new SelectList(tracks);
                ViewBag.DigitizationTracks = new SelectList(digitizationTracks);
                ViewBag.AnalyticsTracks = new SelectList(analyticsTracks);
                ViewBag.ImsTracks = new SelectList(imsTracks);
                ViewBag.CloudSolutionTracks = new SelectList(cloudSolutionTracks);


                int? empID = Convert.ToInt32(HttpContext.Session.GetString("EmployeeId"));

                var role = _db.RoleMasters.Where(r => r.EmployeeId == empID && r.ApplicationCode == "TALENTREQ").Select(p => p.Role).FirstOrDefault();




                // var empId = new SqlParameter("@LoginId", empID == 1? (object)DBNull.Value : empID);
                //var ReportDetails = _db.Set<TeamTrackViewModel>().FromSqlRaw("exec sp_MidYearDetailedReportfor18 @AppraisalId,@WorkflowName", empID).ToList();

                List<TeamTrackViewModel> tms;
              


                if (role == "LDAdmin")
                {
                    tms = (from em in _db.Employees
                           join pa in _db.ProjectAssignments on em.EmployeeId equals pa.EmployeeId
                           where em.IsActive == true && pa.IsActive == true
                           //&& em.Practice == practice && pa.ProjectCode==projectCode

                           select new TeamTrackViewModel
                           {
                               EmployeeID = em.EmployeeId,
                               EmployeeName = em.FirstName + " " + em.MiddleName + " " + em.LastName,
                               Location = em.Location,
                               Designation = em.Designation,
                               Track = em.Track,
                               Grade = em.Grade.ToString(),
                               Practice = em.Practice,
                               Projectcode = pa.ProjectCode
                           }).ToList();

                    var  practice = tms.GroupBy(x => x.Practice).Select(x => x.FirstOrDefault()).ToList();

                    var projectNames = (from projects in _db.Projects
                                        where projects.SOWEndDate >= DateTime.Now
                                        select new
                                        {
                                            ProjectName = projects.ProjectCode + "-" + projects.ProjectName,
                                            ProjectCode = projects.ProjectCode
                                        }).Distinct().ToList();

                    ViewData["ProjectList"] = projectNames;
                    ViewData["Practilist"] = practice;

                }



                else
                {
                    tms = (from em in _db.Employees
                           join pa in _db.ProjectAssignments on em.EmployeeId equals pa.EmployeeId
                           where em.IsActive == true && em.SupervisorId == empID && pa.IsActive == true
                           //&& em.Practice == practice && pa.ProjectCode == projectCode
                           select new TeamTrackViewModel
                           {
                               EmployeeID = em.EmployeeId,
                               EmployeeName = em.FirstName + " " + em.MiddleName + " " + em.LastName,
                               Location = em.Location,
                               Designation = em.Designation,
                               Track = em.Track,
                               Grade = em.Grade.ToString(),
                               Practice = em.Practice,
                               Projectcode = pa.ProjectCode

                           }).Distinct().ToList();

                    var practice = tms.GroupBy(x => x.Practice).Select(x => x.FirstOrDefault()).ToList();

                    var projectNames = (from ps in _db.ProjectAssignments
                                        join pt in _db.Projects on ps.ProjectCode equals pt.ProjectCode
                                        join emp in _db.Employees on ps.EmployeeId equals emp.EmployeeId
                                        where pt.SOWEndDate >= DateTime.Now && emp.SupervisorId == empID
                                        && emp.IsActive == true && ps.IsActive == true
                                        select new
                                        {
                                            projectName = pt.ProjectCode + "-" + pt.ProjectName,
                                            ProjectCode = pt.ProjectCode
                                        }).Distinct().ToList();

                    ViewData["ProjectList"] = projectNames;
                    ViewData["Practilist"] = practice;

                }
                

              


                return View(tms);

            }
            catch (Exception ex)
            {
                ErrorHandling expcls = new ErrorHandling();
                expcls.Error(ex);
                return null;

            }
        }



        public ActionResult SaveTrackData(List<TrackData> trackData)
        {
            try
            {
                int empID = Convert.ToInt32(HttpContext.Session.GetString("EmployeeId"));
                if (trackData.Count() > 0)
                {
                    for (int i = 0; i < trackData.Count(); i++)
                    {
                        var employeeid = Convert.ToInt32(trackData[i].EmployeeID);
                        var trackOldData = trackData[i].TrackOldData;
                        var trackNewData = trackData[i].TrackNewData;
                        var data = _db.Employees.Where(k => k.EmployeeId == employeeid).FirstOrDefault();
                        data.Track = trackNewData;
                        data.ModifiedDate = DateTime.Now;
                        var result = new TeamTrackHistory()
                        {

                            EmployeeID = employeeid,
                            Track_Old = trackOldData,
                            Track_New = trackNewData,
                            ModifiedBy = empID,
                            ModifiedDate = DateTime.Now

                        };
                        _db.TeamTrackHistories.Add(result);

                    }

                    _db.SaveChanges();

                }
                return Json(true);
            }
            catch (Exception ex)
            {
                ErrorHandling expcls = new ErrorHandling();
                expcls.Error(ex);
                return Json(false);

            }




        }


        public ActionResult TeamTrackReport()
        {

            try
            {
                int empID = Convert.ToInt32(HttpContext.Session.GetString("EmployeeId"));


                var role = _db.RoleMasters.Where(r => r.EmployeeId == empID && r.ApplicationCode == "TALENTREQ").Select(p => p.Role).FirstOrDefault();




                // var empId = new SqlParameter("@LoginId", empID == 1? (object)DBNull.Value : empID);
                //var ReportDetails = _db.Set<TeamTrackViewModel>().FromSqlRaw("exec sp_MidYearDetailedReportfor18 @AppraisalId,@WorkflowName", empID).ToList();

                List<TeamTrackViewModel> data;

                if (role == "LDAdmin")
                {
                    data = (from em in _db.Employees
                            join pa in _db.ProjectAssignments on em.EmployeeId equals pa.EmployeeId
                            where em.IsActive == true && pa.IsActive == true
                            //&& em.Practice == practice && pa.ProjectCode==projectCode

                            select new TeamTrackViewModel
                            {
                                EmployeeID = em.EmployeeId,
                                EmployeeName = em.FirstName + " " + em.MiddleName + " " + em.LastName,
                                Location = em.Location,
                                Designation = em.Designation,
                                Track = em.Track,
                                Grade = em.Grade.ToString(),
                                Practice = em.Practice,
                                Projectcode = pa.ProjectCode
                            }).ToList();
                }
                else
                {
                    data = (from em in _db.Employees
                            join pa in _db.ProjectAssignments on em.EmployeeId equals pa.EmployeeId
                            where em.IsActive == true && em.SupervisorId == empID && pa.IsActive == true
                            //&& em.Practice == practice && pa.ProjectCode == projectCode
                            select new TeamTrackViewModel
                            {
                                EmployeeID = em.EmployeeId,
                                EmployeeName = em.FirstName + " " + em.MiddleName + " " + em.LastName,
                                Location = em.Location,
                                Designation = em.Designation,
                                Track = em.Track,
                                Grade = em.Grade.ToString(),
                                Practice = em.Practice,
                                Projectcode = pa.ProjectCode

                            }).Distinct().ToList();
                }
                var s = (from p in data
                         select new
                         {

                             EmployeeID = p.EmployeeID,
                             EmployeeName = p.EmployeeName,
                             Location = p.Location,
                             Grade = p.Grade,
                             Designation = p.Designation,
                             Track = p.Track,
                             Practice=p.Practice,
                             Projectcode=p.Projectcode

                         }).OrderBy(p => p.EmployeeID).ToList();

                #region Export to Excel

                using (ExcelPackage package = new ExcelPackage())
            {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Team Track Report");
                    worksheet.TabColor = System.Drawing.Color.Green;
                    worksheet.DefaultRowHeight = 18f;
                    worksheet.Row(1).Height = 20f;

                    using (var range = worksheet.Cells[1, 1, 1, 8])
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
                    worksheet.Cells[1, 3].Value = "Location";
                    worksheet.Cells[1, 4].Value = "Designation";
                    worksheet.Cells[1, 5].Value = "Practice";
                    worksheet.Cells[1, 6].Value = "Project";
                    worksheet.Cells[1, 7].Value = "Grade";
                    worksheet.Cells[1, 8].Value = "Track";


                    worksheet.Column(1).Width = 20f;
                    worksheet.Column(2).AutoFit(42f);
                    worksheet.Column(3).AutoFit(42f);
                    worksheet.Column(4).AutoFit(42f);
                    worksheet.Column(5).AutoFit(12f);
                    worksheet.Column(6).AutoFit(42f);
                    worksheet.Column(7).AutoFit(42f);
                    worksheet.Column(8).AutoFit(42f);


                    //Add the each row
                    for (int rowIndex = 0, row = 2; rowIndex < s.Count; rowIndex++, row++) // row indicates number of rows
                    {
                        worksheet.Cells[row, 1].Value = s[rowIndex].EmployeeID;
                        worksheet.Cells[row, 2].Value = s[rowIndex].EmployeeName;
                        worksheet.Cells[row, 3].Value = s[rowIndex].Location;
                        worksheet.Cells[row, 4].Value = s[rowIndex].Designation;
                        worksheet.Cells[row, 5].Value = s[rowIndex].Practice;
                        worksheet.Cells[row, 6].Value = s[rowIndex].Projectcode;
                        worksheet.Cells[row, 7].Value = s[rowIndex].Grade;
                        worksheet.Cells[row, 8].Value = s[rowIndex].Track;

                        if (row % 2 == 1)
                        {
                            using (var range = worksheet.Cells[row, 1, row, 8])
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
                    
                    Response.Headers.Add("content-disposition", "attachment;filename=" + "Team Track Report" + DateTime.Now.ToString("_dd-MMM-yyyy_hh-mm-ss") + ".xlsx");

                    
                    Response.ContentType = "application/vnd.ms-excel";
                    //StringWriter sw = new StringWriter();
                    Response.Body.Write(fileBytes);
                    

                }

                #endregion

                return new EmptyResult();
            }
            catch (Exception ex)
            {
                ErrorHandling expcls = new ErrorHandling();
                expcls.Error(ex);
                return Json(false);

            }

        }


        public JsonResult GetTrackHistoryByEmployeeId(int empId)
        {
            try
            {
                List<TeamTrackHistoryViewModel> trackhistory = new List<TeamTrackHistoryViewModel>();
                trackhistory = (from p in _db.TeamTrackHistories
                                join e in _db.Employees on p.ModifiedBy equals e.EmployeeId
                                where p.EmployeeID == empId
                                select new TeamTrackHistoryViewModel
                                {
                                    EmployeeID = p.EmployeeID,
                                    Track_Old = p.Track_Old == null ? "" : p.Track_Old,
                                    Track_New = p.Track_New,
                                    ModifiedDate = p.ModifiedDate.ToString(),
                                    ModifiedBy = e.FirstName
                                }

                               ).ToList();


                return Json(trackhistory);

            }
            catch (Exception ex)
            {
                ErrorHandling expcls = new ErrorHandling();
                expcls.Error(ex);
                return Json(false);

            }

        }

     
        public ActionResult GetTrackTablebyPracticeandProject(string practice, string projectCode)
        {
            try
            {


                int? empID = Convert.ToInt32(HttpContext.Session.GetString("EmployeeId"));

                var role = _db.RoleMasters.Where(r => r.EmployeeId == empID && r.ApplicationCode == "TALENTREQ").Select(p => p.Role).FirstOrDefault();
                List<TeamTrackViewModel> tms;

                if (role == "LDAdmin")
                {
                    tms = (from em in _db.Employees
                           join pa in _db.ProjectAssignments on em.EmployeeId equals pa.EmployeeId
                           where em.IsActive == true && pa.IsActive == true
                           //&& em.Practice == practice && pa.ProjectCode==projectCode

                           select new TeamTrackViewModel
                           {
                               EmployeeID = em.EmployeeId,
                               EmployeeName = em.FirstName + " " + em.MiddleName + " " + em.LastName,
                               Location = em.Location,
                               Designation = em.Designation,
                               Track = em.Track,
                               Grade = em.Grade.ToString(),
                               Practice = em.Practice,
                               Projectcode = pa.ProjectCode
                           }).ToList();
                }

                else
                {
                    tms = (from em in _db.Employees
                           join pa in _db.ProjectAssignments on em.EmployeeId equals pa.EmployeeId
                           where em.IsActive == true && em.SupervisorId == empID && pa.IsActive == true
                           //&& em.Practice == practice && pa.ProjectCode == projectCode
                           select new TeamTrackViewModel
                           {
                               EmployeeID = em.EmployeeId,
                               EmployeeName = em.FirstName + " " + em.MiddleName + " " + em.LastName,
                               Location = em.Location,
                               Designation = em.Designation,
                               Track = em.Track,
                               Grade = em.Grade.ToString(),
                               Practice = em.Practice,
                               Projectcode = pa.ProjectCode

                           }).Distinct().ToList();


                }
                if (practice != null && practice != string.Empty && (!practice.Contains("Practice")))
                {
                    tms = tms.Where(x => x.Practice == practice).Distinct().ToList();
                }
                if (projectCode != null && projectCode != string.Empty)
                {
                    tms = tms.Where(x => x.Projectcode == projectCode).Distinct().ToList();
                }
                //var tracks = (from em in _db.Employees
                //              where em.IsActive == true && em.Track != null
                //              orderby em.Track ascending
                //              select em.Track).Distinct().ToList();
                var tracks = (from em in _db.TrackSkillSets
                              where em.IsActive == true && em.Practice == practice
                              orderby em.SkillName ascending
                              select em.SkillName).Distinct().ToList();

                var digitizationTracks = (from em in _db.TrackSkillSets
                                          where em.IsActive == true && em.Practice == "Digitization"
                                          orderby em.SkillName ascending
                                          select em.SkillName).Distinct().ToList();

                var analyticsTracks = (from em in _db.TrackSkillSets
                                       where em.IsActive == true && em.Practice == "Analytics"
                                       orderby em.SkillName ascending
                                       select em.SkillName).Distinct().ToList();

                var imsTracks = (from em in _db.TrackSkillSets
                                 where em.IsActive == true && em.Practice.Contains("IMS")
                                 orderby em.SkillName ascending
                                 select em.SkillName).Distinct().ToList();

                var cloudSolutionTracks = (from em in _db.TrackSkillSets
                                           where em.IsActive == true && em.Practice == "Cloud Solutions"
                                           orderby em.SkillName ascending
                                           select em.SkillName).Distinct().ToList();

                ViewBag.Tracks = new SelectList(tracks);
                ViewBag.DigitizationTracks = new SelectList(digitizationTracks);
                ViewBag.AnalyticsTracks = new SelectList(analyticsTracks);
                ViewBag.ImsTracks = new SelectList(imsTracks);
                ViewBag.CloudSolutionTracks = new SelectList(cloudSolutionTracks);


                return PartialView("_GetTrackTablebyPractice", tms);

            }
            catch (Exception ex)
            {
                ErrorHandling expcls = new ErrorHandling();
                expcls.Error(ex);
                return null;

            }


        }

    }
}
