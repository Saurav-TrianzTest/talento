using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Trianz.Enterprise.Operations.Models;
using System.IO;
using OfficeOpenXml;
using Trianz.Enterprise.Operations.General;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Session;
namespace Trianz.Enterprise.Operations.Controllers
{
    public class CompetencyController : Controller
    {
        TrianzOperationsEntities db = new TrianzOperationsEntities();
        // GET: Competency
        public ActionResult Index()
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {

                return View();
        }
            else
            {
                //ermsg = "Session expired";
                //return Json("Sessionexpired");
                return RedirectToAction("SessionExpire", "Signout");
            }
        }
        public ActionResult Autocomplete(string term)
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {

                var result = new List<KeyValuePair<string, string>>();
            DateTime currentDate = System.DateTime.Now.Date;
            IList<SelectListItem> List = new List<SelectListItem>();

            try
            {
                var SkillCategory = (from pro in db.SkillMasters
                                     where pro.IsActive == true
                                     orderby pro.SkillCategory
                                     select new SelectListItem
                                     {
                                         Text = pro.SkillCategory.ToString(),
                                         Value = pro.SkillCategory
                                     }).Distinct().ToList();

                foreach (var item in SkillCategory)
                {
                    result.Add(new KeyValuePair<string, string>(item.Value.ToString(), item.Text));
                }
                var result3 = result.Where(s => s.Key.ToLower().Contains
                              (term.ToLower())).Select(w => w).ToList();
                return Json(result3);
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
        public ActionResult SkillCategory()
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                try
            {
                List<SelectListItem> SkillCategoryList = new List<SelectListItem>();

                SkillCategoryList = (from c in db.SkillMasters
                                     where c.IsActive == true 
                                     select new SelectListItem
                                     {
                                         Text = c.SkillCategory,
                                         Value = c.SkillCategory.ToString()
                                     }).Distinct().ToList();
                ViewBag.SkillCategoryList = SkillCategoryList.ToList();

                List<SelectListItem> PracticeList = new List<SelectListItem>();
                PracticeList = (from c in db.Employees
                                select new SelectListItem
                                {
                                    Text = c.Practice,
                                    Value = c.Practice.ToString()

                                }).Distinct().ToList();

                ViewBag.PracticeList = PracticeList.ToList();



                List<SkillMaster> SkillCategory = new List<SkillMaster>();

                SkillCategory = (from obj in db.SkillMasters
                                 where obj.IsActive == true
                                 select obj).GroupBy(n => new { n.SkillCategory, })
                                               .Select(g => g.FirstOrDefault())
                                               .ToList();
                return View(SkillCategory);
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
        public ActionResult SkillSet()
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                try
            {
                
                List<SelectListItem> SkillCategoryList = new List<SelectListItem>();

                SkillCategoryList = (from c in db.SkillMasters
                                     where c.IsActive == true
                                     select new SelectListItem
                                     {
                                         Text = c.SkillCategory,
                                         Value = c.SkillCategory.ToString()
                                     }).Distinct().ToList();
                ViewBag.SkillCategoryList = SkillCategoryList.ToList();


                List<SelectListItem> PracticeList = new List<SelectListItem>();
                PracticeList = (from c in db.Employees
                                select new SelectListItem
                                {
                                    Text = c.Practice,
                                    Value = c.Practice.ToString()

                                }).Distinct().ToList();

                ViewBag.PracticeList = PracticeList.ToList();
                List<SkillMaster> SkillSet = new List<SkillMaster>();

                SkillSet = (from obj in db.SkillMasters
                            where obj.IsActive == true
                            select obj).GroupBy(n => new { n.SkillCategory, })
                                               .Select(g => g.FirstOrDefault())
                                               .ToList();
                return View(SkillSet);
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
        
        public ActionResult AddSkillCategory(string SkillCategory, string Practice)
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                try
                {
                    var exist = db.SkillMasters.Where(p => p.SkillCategory == SkillCategory && p.IsActive == true).Select(p => p.SkillCategory).FirstOrDefault();


                    if (exist != null)
                    {
                        return Json("false");

                    }
                    if (Practice == "")
                    {
                        var skillcategory = new SkillMaster()
                        {
                            SkillCategory = SkillCategory,
                            IsActive = true,
                            Practice = null

                        };
                        db.SkillMasters.Add(skillcategory);
                        db.SaveChanges();
                    }
                    else
                    {
                        var skillcategory = new SkillMaster()
                        {
                            SkillCategory = SkillCategory,
                            IsActive = true,
                            Practice = Practice

                        };
                        db.SkillMasters.Add(skillcategory);
                        db.SaveChanges();
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
        public ActionResult UpdateSkillCategory(string SkillCategory, string ExistingSkillCategory, string Practice)
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            { 
                try

            {

                if (ExistingSkillCategory.ToUpper().Trim() == SkillCategory.ToUpper().Trim())
                {

                    var data = db.SkillMasters.Where(p => p.SkillCategory == ExistingSkillCategory && p.IsActive == true).ToList();
                    if (data != null)
                    {
                        if (Practice == "")
                        {
                            foreach (var p in data)
                            {
                                p.SkillCategory = SkillCategory;
                                p.Practice = null;
                            }
                            db.SaveChanges();
                        }
                        else
                        {
                            foreach (var p in data)
                            {
                                p.SkillCategory = SkillCategory;
                                p.Practice = Practice;
                            }
                            db.SaveChanges();

                        }


                    }
                    return Json("true");
                }
                else
                {
                    var exist = db.SkillMasters.Where(p => p.SkillCategory == SkillCategory && p.IsActive == true).Select(p => p.SkillCategory).FirstOrDefault();

                    if (exist != null)
                    {
                        return Json("false");
                    }

                    else
                    {
                        var updtdata = db.SkillMasters.Where(p => p.SkillCategory == ExistingSkillCategory && p.IsActive == true).ToList();
                        if (updtdata != null)
                        {
                            if (Practice == "")
                            {
                                foreach (var p in updtdata)
                                {
                                    p.SkillCategory = SkillCategory;
                                    p.Practice = null;
                                }
                                db.SaveChanges();
                            }
                            else
                            {
                                foreach (var p in updtdata)
                                {
                                    p.SkillCategory = SkillCategory;
                                    p.Practice = Practice;
                                }
                                db.SaveChanges();

                            }

                        }
                        return Json("true");
                    }
                }
            }

                catch (Exception ex)
                {
                    Common.WriteExceptionErrorLog(ex);
                    //return RedirectToAction("Error", "Error");
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
        public ActionResult DeleteSkillCategory(string SkillCategory)
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                try
                {
                    var skillcategory = db.SkillMasters.Where(p => p.SkillCategory == SkillCategory).ToList();
                    if (skillcategory != null)
                    {
                        foreach (var p in skillcategory)
                            p.IsActive = false;
                        db.SaveChanges();
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
        public ActionResult GetSkillSetbySkillCategory(string SortDirection, string SkillCategory)
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                try
            {
                //int skip = PageNo.HasValue ? PageNo.Value - 1 : 0;
                //int NoOfPages = 0;
                if (SortDirection == null || SortDirection == "")
                    ViewBag.SortDirection = "ASCE";
                else
                    ViewBag.SortDirection = SortDirection;
                List<SkillMaster> SkillSetList = new List<SkillMaster>();

            
              
                    
                        if ((SkillCategory != null && SkillCategory != string.Empty))
                        {
                            SkillSetList = db.SkillMasters.Where(q => q.SkillCategory == SkillCategory && q.IsActive == true && q.Skillset != null).OrderBy(o => o.Skillset).ToList();
                    //NoOfPages = db.SkillMaster.Where(q => q.SkillCategory == SkillCategory && q.IsActive == true && q.Skillset != null).OrderBy(o => o.Skillset).Count() / 5;
                }
                        else
                        {
                   
                    SkillSetList = db.SkillMasters.Where(q => q.IsActive == true && q.Skillset != null).OrderBy(o => o.Skillset).ToList();
                    //NoOfPages = db.SkillMaster.Where(q =>  q.IsActive == true && q.Skillset != null).OrderBy(o => o.Skillset).Count() / 5;
                }

                      

                    
                    ViewBag.NoOfRecardsCount = db.SkillMasters.Where(q => q.SkillCategory == SkillCategory && q.IsActive == true && q.Skillset != null).OrderBy(o => o.Skillset).Count();
                    //ViewBag.PageCount = NoOfPages;
                

                List<SelectListItem> PracticeList = new List<SelectListItem>();
                PracticeList = (from c in db.Employees
                                select new SelectListItem
                                {
                                    Text = c.Practice,
                                    Value = c.Practice.ToString()

                                }).Distinct().ToList();

                ViewBag.PracticeList = PracticeList.ToList();
                return PartialView("_GetSkillSetDetails", SkillSetList);
            }
                catch (Exception ex)
                {
                    Common.WriteExceptionErrorLog(ex);
                    //return RedirectToAction("Error", "Error");
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

        public ActionResult AddSkillSet(string SkillCategory, string SkillSet)
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                try
            {
                var SkillCat = db.SkillMasters.Where(p => p.SkillCategory == SkillCategory && p.IsActive == true).ToList();

                var exist = db.SkillMasters.Where(p => p.SkillCategory == SkillCategory && p.Skillset == SkillSet && p.IsActive == true).Select(p => p.Skillset).FirstOrDefault();
                if (exist != null)
                {
                    return Json("false");

                }
                var Practice = db.SkillMasters.Where(p => p.SkillCategory == SkillCategory && p.IsActive == true).Select(p => p.Practice).FirstOrDefault();
                if(SkillCat != null)
                {
                    if (SkillCat.Count == 1)
                    {
                        if (string.IsNullOrEmpty(SkillCat[0].Skillset))
                        {
                            SkillCat[0].Skillset = SkillSet;
                           
                        }
                        else
                        {
                            var skillSet = new SkillMaster()
                            {
                                SkillCategory = SkillCategory,
                                Skillset = SkillSet,
                                IsActive = true,
                                Practice = Practice
                            };
                            db.SkillMasters.Add(skillSet);
                            
                        }
                    }
                    else
                    {
                        var skillSet = new SkillMaster()
                        {
                            SkillCategory = SkillCategory,
                            Skillset = SkillSet,
                            IsActive = true,
                            Practice = Practice
                        };
                        db.SkillMasters.Add(skillSet);
                        
                    }
                }
                db.SaveChanges();
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
        public ActionResult DeleteSkillSet(int SkillID, string SkillCategory, string SkillSet)

        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {

                try
                {
                var skillSet = db.SkillMasters.Where(p => p.SkillCategory == SkillCategory && p.SkillId == SkillID && p.Skillset == SkillSet).ToList();
                if (skillSet != null)
                {
                    foreach (var p in skillSet)
                        p.IsActive = false;
                    db.SaveChanges();
                }
                return Json("true");
            }
                catch (Exception ex)
                {
                    Common.WriteExceptionErrorLog(ex);
                    //return RedirectToAction("Error", "Error");
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

        public ActionResult UpdateSkillSet(int SkillID, string SkillCategory, string SkillSet, string ExistingPratice)
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                try
            {
                var Practice = db.SkillMasters.Where(p => p.SkillCategory == SkillCategory && p.IsActive == true).Select(p => p.Practice).FirstOrDefault();

                var exist = db.SkillMasters.Where(p => p.SkillCategory == SkillCategory && p.Skillset == SkillSet && p.Practice == Practice && p.IsActive == true && p.SkillId != SkillID).Select(p => p.Skillset).FirstOrDefault();
                if (exist != null)
                {
                    return Json("false");

                }


                var data = db.SkillMasters.Where(p => p.SkillCategory == SkillCategory && p.SkillId == SkillID).FirstOrDefault();
                {
                    data.Skillset = SkillSet;
                    data.Practice = Practice;
                    db.SaveChanges();

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

        public ActionResult ExportReport()
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {


                return Json("success");
        }
            else
            {
                //ermsg = "Session expired";
                return Json("Sessionexpired");
                //return RedirectToAction("SessionExpire", "Signout");
            }
        }
        public ActionResult GenerateSkillReport(string Skills)
        {
            try
            {
                int? EmpId = Convert.ToInt32(HttpContext.Session.GetString("EmployeeId"));
                Skills = (Skills.ToLower() == "Skillcategory") ? "" : Skills.ToUpper();
                List<SkillMaster> skillsreport = new List<SkillMaster>();
                if (Skills != null && Skills != string.Empty)
                {
                    skillsreport = db.SkillMasters.Where(q => q.SkillCategory == Skills && q.IsActive == true && q.Skillset != null).OrderBy(o => o.Skillset).ToList();

                }
                else
                {
                    skillsreport = db.SkillMasters.Where(q => q.IsActive == true && q.Skillset != null).OrderBy(o => o.Skillset).ToList();
                }




                #region Export to Excel

                using (ExcelPackage package = new ExcelPackage())
            {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("TR Information");
                    worksheet.TabColor = System.Drawing.Color.Green;
                    worksheet.DefaultRowHeight = 18f;
                    worksheet.Row(1).Height = 20f;

                    using (var range = worksheet.Cells[1, 1, 1, 47])
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
                    worksheet.Cells[1, 1].Value = "SkillCategory";
                    worksheet.Cells[1, 2].Value = "Skillset";
                    worksheet.Cells[1, 3].Value = "Practice";





                    worksheet.DefaultColWidth = 18f;

                    worksheet.Column(1).Width = 13f;
                    worksheet.Column(2).AutoFit(20f);


                    //Add the each row
                    for (int rowIndex = 0, row = 2; rowIndex < skillsreport.Count; rowIndex++, row++) // row indicates number of rows
                    {

                        worksheet.Cells[row, 1].Value = skillsreport[rowIndex].SkillCategory;
                        worksheet.Cells[row, 2].Value = skillsreport[rowIndex].Skillset;

                        worksheet.Cells[row, 3].Value = skillsreport[rowIndex].Practice;


                        if (row % 2 == 1)
                        {
                            using (var range = worksheet.Cells[row, 1, row, 3])
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
                    
                    Response.Headers.Add("content-disposition", "attachment;filename=" + ((Skills == "") ? "My" : Skills) + "SkillReport" + DateTime.Now.ToString("_dd-MMM-yyyy_hh-mm-ss") + ".xlsx");

                    
                    Response.ContentType = "application/vnd.ms-excel";
                    StringWriter sw = new StringWriter();
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

        public class SkillCategoryList
        {
            public string SkillCategory { get; set; }
            public string Skillset { get; set; }
            public bool IsActive { get; set; }
            public string Practice { get; set; }
        }
        
    }
}
