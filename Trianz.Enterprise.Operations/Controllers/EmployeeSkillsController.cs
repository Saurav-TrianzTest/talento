using System;
using System.Collections.Generic;
using System.Configuration;
using Microsoft.Data.SqlClient;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Trianz.Enterprise.Operations.Models;
using OfficeOpenXml;
using Trianz.Enterprise.Operations.General;
using Microsoft.AspNetCore.Session;
using Microsoft.Extensions.Hosting;

namespace Trianz.Enterprise.Operations.Controllers
{
    public class EmployeeSkillsController : Controller
    {
    private readonly IWebHostEnvironment Env;
        private TrianzOperationsEntities db = new TrianzOperationsEntities();
        //ServiceAgent AgentService = new ServiceAgent();
        // GET: EmployeeSkills
        public ActionResult Index()

        {
            return View();
        }
        public IList<SelectListItem> GetYear()
        {
            int startingyear = 1990;
            int numberOfYears = DateTime.Now.Year - startingyear;
            var startYear = startingyear;
            var endYear = startingyear + numberOfYears;

            var yearList = new List<SelectListItem>();
            for (var i = startYear; i <= endYear; i++)
            {
                yearList.Add(new SelectListItem() { Value = i.ToString(), Text = i.ToString() });
            }

            //List<SelectListItem> lstCompetency = db.SkillMasters.Where(s => s.Skillset.ToLower().Equals(Skillname.ToLower())).
            //           Select(s => new SelectListItem { Text = s.SkillCategory, Value = s.SkillId.ToString() })
            //           .Distinct()
            //           .OrderBy(o => o.Text)
            //           .ToList();


            var orderbylist = yearList.OrderByDescending(o => o.Value).ToList();
            return orderbylist;
        }
        // GET: EmployeeSkills/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: EmployeeSkills/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: EmployeeSkills/Create
        [HttpPost]
        public ActionResult Create(IFormCollection collection)
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

        // GET: EmployeeSkills/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: EmployeeSkills/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, IFormCollection collection)
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

        // GET: EmployeeSkills/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: EmployeeSkills/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, IFormCollection collection)
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

        public JsonResult InserttoDb(IFormCollection form, IFormFile file)
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {

                JsonResult jst = new JsonResult(null);
                try
                {
                    if (ModelState.IsValid)
                    {
                        //EmployeeDoc empData = new EmployeeDoc();

                        //string name = User.Identity.Name.Split('\\')[1].ToLower();
                        string usermail = Common.GetAzureLoggedInUserID();
                        Employee objEmp = db.Employees.Where(e => e.Email.Equals(usermail) && e.IsActive == true).FirstOrDefault();
                        if (objEmp != null)
                        {
                            int id = objEmp.EmployeeId;
                            //empData = db.EmployeeDocs.Where(r => r.Employeeid == id).FirstOrDefault();
                            if (HttpContext.Request.Form.Files["file"] == null)
                            {
                                jst.Value = "File, Please Upload Your file";
                            }
                            else if (HttpContext.Request.Form.Files["file"].Length > 0)
                            {
                                string fileExtension =
                                              System.IO.Path.GetExtension(HttpContext.Request.Form.Files["file"].FileName);
                                int MaxContentLength = 1024 * 1024 * 4; //Size = 4 MB
                                                                        // string[] AllowedFileExtensions = new string[] {  ".pdf",".txt",".doc",".docx" };
                                string[] AllowedFileExtensions = new string[] { ".docx", ".doc", ".pptx" };

                                if (!AllowedFileExtensions.Contains(fileExtension))
                                {
                                    jst.Value = "Please file of type: " + string.Join(", ", AllowedFileExtensions);
                                    //TempData["notSuccess"] = "Error! Resume format should be in .docx ";

                                }
                                else if (HttpContext.Request.Form.Files["file"].Length > MaxContentLength)
                                {
                                    jst.Value = "Your file is too large, maximum allowed size is: " + MaxContentLength + " MB";
                                }
                                else
                                {
                                    if (objEmp != null)
                                    {
                                        var fileName = Path.GetFileName(HttpContext.Request.Form.Files["file"].FileName);
                                        string fileTypeExtension = Path.GetExtension(HttpContext.Request.Form.Files["file"].FileName);

                                        byte[] result = ConvertToBytes(HttpContext.Request.Form.Files["file"]);
                                        if (fileTypeExtension == ".pptx")
                                        {
                                            objEmp.ResumePdf = result;
                                            objEmp.ResumePdfUpdatedDate = DateTime.Now;
                                        }
                                        else
                                        {
                                            objEmp.Resumes = result;
                                            objEmp.ResumeUpdatedDate = DateTime.Now;
                                        }


                                        db.Entry(objEmp).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                                        db.SaveChanges();
                                        Employee objEmp1 = db.Employees.Where(e => e.Email.Equals(usermail) && e.IsActive == true).FirstOrDefault();
                                        ViewBag.EmpCV = objEmp1.Resumes;
                                        ViewBag.EmpPdfCV = objEmp1.ResumePdf;
                                        ViewBag.DocFileName = objEmp1.FirstName + "_" + objEmp1.EmployeeId;
                                        ViewBag.PptFileName = objEmp1.FirstName + "_" + objEmp1.EmployeeId;

                                        jst.Value = "Success";
                                        //TempData["Success"] = "Upload successful";
                                    }
                                }
                            }
                            else
                            {
                                jst.Value = "File size should be greater than zero";
                            }
                        }
                    }

                    //return ;
                }
                //catch (Exception ex)
                //{
                //    jst.Value = "Error! Please try again";
                //   // TempData["notSuccess"] = "Error! Please try again";
                //    // return RedirectToAction("AddNewSkills");
                //}
                catch (Exception ex)
                {

                    Common.WriteExceptionErrorLog(ex);
                    return Json("Error");
                }
                return jst;
            }
            else
            {
                //ermsg = "Session expired";
                return Json("Sessionexpired");
                //return RedirectToAction("SessionExpire", "Signout");
            }
        }
        public byte[] ConvertToBytes(IFormFile file)
        {
            byte[] filebytes = null;
            var reader = new BinaryReader(file.OpenReadStream());
            filebytes = reader.ReadBytes((int)file.Length);
            return filebytes;
        }

        public ActionResult GetPrimarySkillsByCategory(string CategorySeqId)
        {
            try
            {
                List<MasterLookUp> MasterLookUps = db.MasterLookUps.ToList();
                var SkillsList = new List<SelectListItem>();
                if (MasterLookUps != null)
                {
                    string Primaryskills = ConfigurationManager.AppSettings["PrimarySkills"].ToString();
                    foreach (MasterLookUp item in MasterLookUps.OrderBy(p => p.LookupName).Where(x => x.LookupType == Primaryskills && x.ParentCode == CategorySeqId))
                    {
                        SkillsList.Add(new SelectListItem { Text = item.Description, Value = item.LookupID.ToString() });
                    }
                }
                return Json(SkillsList);
            }
            catch (Exception ex)
            {

                Common.WriteExceptionErrorLog(ex);
                return Json("Error");
            }
        }

        public ActionResult GetSecondarySkillsByCategory(string CategorySeqId)
        {
            try
            {
                List<MasterLookUp> MasterLookUps = db.MasterLookUps.ToList();
                var SecSkillsList = new List<SelectListItem>();
                if (MasterLookUps != null)
                {
                    string Secondaryskills = ConfigurationManager.AppSettings["SecondarySkills"].ToString();
                    foreach (MasterLookUp item in MasterLookUps.Where(x => x.LookupType == Secondaryskills && x.ParentCode == CategorySeqId))
                    {
                        SecSkillsList.Add(new SelectListItem { Text = item.Description, Value = item.LookupID.ToString() });
                    }
                }
                return Json(SecSkillsList);
            }
            catch (Exception ex)
            {

                Common.WriteExceptionErrorLog(ex);
                return Json("Error");
            }
        }

        public ActionResult AddNewSkills()
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                try
                {
                    //Below single statement is added by Sarath, for security reason to access RoleMaster page.
                    TempData["IsRoleMasterPageAccess"] = null;
                    ViewBag.AllYears =   GetYear();

                    //List<MasterLookUp> MasterLookUps = AgentService.GetLookupdata();
                    List<MasterLookUp> MasterLookUps = db.MasterLookUps.ToList();
                    var SkillCategory = new List<SelectListItem>();
                    if (MasterLookUps != null)
                    {
                        string SkillCategry = ConfigurationManager.AppSettings["SkillCategory"].ToString();
                        foreach (MasterLookUp item in MasterLookUps.Where(x => x.LookupType == SkillCategry))
                        {
                            SkillCategory.Add(new SelectListItem() { Text = item.Description, Value = item.SeqNumber.ToString() });
                        }
                    }
                    string exp = ConfigurationManager.AppSettings["ExpFrom"].ToString();
                    var ExpFrom = from expFrom in MasterLookUps.Where(x => x.LookupType == exp)
                                  select new
                                  {
                                      LookupName = expFrom.LookupName,
                                      Description = expFrom.Description,
                                      LookupCode = expFrom.LookupCode,
                                      SeqNumber = expFrom.SeqNumber
                                  };
                    ViewData["Exp"] = ExpFrom;
                    ViewData["SkillCategory"] = SkillCategory;
                    //string loginUserName = User.Identity.Name;
                    string usermail = Common.GetAzureLoggedInUserID();
                    //string[] Names = loginUserName.Split(new[] { "\\" }, StringSplitOptions.None);

                    //string name = "shaik.hakeem"; //"bharath.kapuraveni"; //"sreevenkata.babu"; //"sudharani.a"; //"satish.katragadda"; //"abdul.shaik"; // "lubna.shumaila"; //"abdulbari.indoor"; //Names[1].ToLower();
                    //ViewBag.UserName = name;
                    // string name = "PRATHYUSHA.DESHPANDE";

                    //string name = Names[1].ToLower();

                    if (usermail != null)
                    {
                        int EmployeeId = (from data in db.Employees where (data.Email.Equals(usermail) && data.IsActive == true) select data.EmployeeId).FirstOrDefault();
                        Employee employee = (from data in db.Employees.Where(x => x.EmployeeId == EmployeeId) select data).FirstOrDefault();
                        if (employee != null)
                        {
                            List<EmployeeSkills_NewDetails> _objSkills = new List<EmployeeSkills_NewDetails>();
                            //List<EmployeeSkills_New> _objSkills_New = new List<EmployeeSkills_New>();
                            _objSkills = GetEmployeeSkill(employee.EmployeeId.ToString()).ToList();
                            var reportingManger = db.Employees.Where(e => e.EmployeeId == employee.SupervisorId).Select(em => em.FirstName + " " + em.LastName).FirstOrDefault();
                            //var ApprovalManager = reportingManger.FirstName +" "+reportingManger.LastName;
                            ViewData["reportingmanager"] = reportingManger;
                            ViewData["employeeSkills"] = _objSkills;
                            //ViewData["employeenewSkills"] = GetEmployeeSkill_New().Where(emp1 => emp1.EmployeeId == employee.EmployeeId).ToList();
                            ViewData["AssignmentDetails"] = GetEmployeeAssignmentDetails(employee.EmployeeId);
                            ViewData["MyAddressDetails"] = "";
                            ViewData["CurrentLocation_Country"] = employee.Location + "," + employee.Region;
                            ViewData["PermanentLocation_Country"] = "";

                            ViewBag.EmpCV = employee.Resumes;
                            ViewBag.EmpPdfCV = employee.ResumePdf;
                            ViewBag.DocFileName = employee.FirstName + "_" + employee.EmployeeId;
                            ViewBag.PptFileName = employee.FirstName + "_" + employee.EmployeeId;
                            ViewBag.IsTabAccess = false;
                            if (HttpContext.Session.GetString("Role") != null)
                            {
                                if (HttpContext.Session.GetString("Role").ToString() == "OM" || HttpContext.Session.GetString("Role").ToString() == "DH" || HttpContext.Session.GetString("EmployeeId").ToString() == "103170") // Surya.Medisetti
                                {
                                    ViewBag.IsTabAccess = true;
                                }
                                else //if (HttpContext.Session.GetString("Role").ToString() == "RL" || HttpContext.Session.GetString("Role").ToString() == "SM" || HttpContext.Session.GetString("Role").ToString() == "PM")
                                {
                                    List<Employee> lstTeam = db.Employees.Where(emp => emp.SupervisorId == employee.EmployeeId).ToList();

                                    if (lstTeam.Count > 0)
                                    {
                                        ViewBag.IsTabAccess = true;
                                    }
                                }
                            }


                            EmployeeAddressInformation objEAI = db.EmployeeAddressInformations.Where(eai => eai.EmployeeId.Equals(employee.EmployeeId)).FirstOrDefault();

                            if (objEAI != null)
                            {
                                System.Text.StringBuilder sbCurrentAddress = new System.Text.StringBuilder();

                                if (!string.IsNullOrWhiteSpace(objEAI.CurrentAddressDoorNo))
                                {
                                    sbCurrentAddress.Append("<h4 class='bold sub-headtext' style='font-size: 16px;'>My Current Address</h4>");
                                    sbCurrentAddress.Append("<p style='font-size: 13px'>" + objEAI.CurrentAddressDoorNo + "</p>");
                                }
                                if (!string.IsNullOrWhiteSpace(objEAI.CurrentAddressStreet))
                                {
                                    sbCurrentAddress.Append("<p style='font-size: 13px; margin-bottom: 5px;'>" + objEAI.CurrentAddressStreet + "</p>");
                                }
                                if (!string.IsNullOrWhiteSpace(objEAI.CurrentAddressArea))
                                {
                                    sbCurrentAddress.Append("<p style='font-size: 13px; margin-bottom: 5px;'>" + objEAI.CurrentAddressArea + "</p>");
                                }
                                if (!string.IsNullOrWhiteSpace(objEAI.CurrentAddressCity))
                                {
                                    sbCurrentAddress.Append("<p style='font-size: 13px; margin-bottom: 5px;'>" + objEAI.CurrentAddressCity + "</p>");
                                }
                                if (!string.IsNullOrWhiteSpace(objEAI.CurrentAddressState))
                                {
                                    sbCurrentAddress.Append("<p style='font-size: 13px; margin-bottom: 5px;'>" + objEAI.CurrentAddressState + "</p>");
                                }
                                if (!string.IsNullOrWhiteSpace(objEAI.CurrentAddressCountry))
                                {
                                    sbCurrentAddress.Append("<p style='font-size: 13px; margin-bottom: 5px;'>" + objEAI.CurrentAddressCountry + "</p>");
                                }
                                if (!string.IsNullOrWhiteSpace(objEAI.CurrentAddressPostalCode))
                                {
                                    sbCurrentAddress.Append("<p style='font-size: 13px; margin-bottom: 5px;'>" + objEAI.CurrentAddressPostalCode + "</p>");
                                }

                                ViewData["MyAddressDetails"] = sbCurrentAddress.ToString();
                                // ViewData["CurrentLocation_Country"] = objEAI.CurrentLocation + "," + objEAI.CurrentAddressCountry;
                                ViewData["PermanentLocation_Country"] = objEAI.PermanentAddressCity + "," + objEAI.PermanentAddressCountry;
                            }
                        }

                        return View(employee);
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
                //return Json("Sessionexpired");
                return RedirectToAction("SessionExpire", "Signout");
            }
        }
        private IList<ProjectAssignment> GetEmployeeAssignmentDetails(int employeeId)
        {
            IList<ProjectAssignment> ProjectAssignments = new List<ProjectAssignment>();
            ProjectAssignments = db.ProjectAssignments.Where(p => p.EmployeeId == employeeId).ToList();
            return ProjectAssignments;
        }

        public ActionResult SubmitEmployeeDetails(string PrimarySkills, string SecondarySkills)
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                try
                {
                    bool CanNotify = false;
                    if (!string.IsNullOrEmpty(PrimarySkills.TrimEnd(';')) || !string.IsNullOrEmpty(SecondarySkills.TrimEnd(';')))
                    {
                        IList<string> PrimarySkillIds = new List<string>();
                        PrimarySkillIds = !string.IsNullOrEmpty(PrimarySkills) ? PrimarySkills.TrimEnd(';').Split(';') : PrimarySkillIds;
                        IList<string> SecondarySkillIds = new List<string>();
                        SecondarySkillIds = !string.IsNullOrEmpty(SecondarySkills) ? SecondarySkills.TrimEnd(';').Split(';') : SecondarySkillIds;
                        //string loginUserName = User.Identity.Name;
                        // string loginUserName = "PRATHYUSHA.DESHPANDE";
                        //string[] Names = loginUserName.Split(new[] { "\\" }, StringSplitOptions.None);
                        //string name = Names[1].ToLower();

                        //string name = "bharath.kapuraveni";
                        string usermail = Common.GetAzureLoggedInUserID();
                        if (usermail != null)
                        {

                            int EmployeeID = (from data in db.Employees where (data.Email.Equals(usermail) && data.IsActive == true) select data.EmployeeId).FirstOrDefault();
                            Employee employee = (from data in db.Employees.Where(x => x.EmployeeId == EmployeeID) select data).FirstOrDefault();

                            foreach (string PSkill in PrimarySkillIds)
                            {
                                long id = Convert.ToInt64(PSkill.Split(':')[0]);
                                EmployeeSkill empskill = db.EmployeeSkills.Where(sk => sk.EmployeeId == employee.EmployeeId && sk.Skills == id).FirstOrDefault();
                                if (empskill == null)
                                {
                                    EmployeeSkill updatedskill = db.EmployeeSkills.Where(sk => sk.EmployeeId == employee.EmployeeId && sk.IsPrimary == false && sk.Skills == id).FirstOrDefault();
                                    if (updatedskill != null)
                                        db.EmployeeSkills.Remove(updatedskill);
                                    MasterLookUp MLookup = db.MasterLookUps.Where(m => m.LookupID == id).FirstOrDefault();
                                    var EmpPrimarySkills = new EmployeeSkill();
                                    EmpPrimarySkills.EmployeeId = employee.EmployeeId;
                                    EmpPrimarySkills.EmployeeName = employee.FirstName + " " + employee.MiddleName + " " + employee.LastName;
                                    EmpPrimarySkills.Employee = employee;
                                    EmpPrimarySkills.IsPrimary = true;
                                    EmpPrimarySkills.Skills = Convert.ToInt64(PSkill.Split(':')[0]);
                                    EmpPrimarySkills.MasterLookUp = MLookup;
                                    EmpPrimarySkills.Rating = Convert.ToInt32(PSkill.Split(':')[1]);
                                    EmpPrimarySkills.SkillDescription = MLookup.Description;
                                    EmpPrimarySkills.Status = ConfigurationManager.AppSettings["Not Yet Approved"].ToString();
                                    db.EmployeeSkills.Add(EmpPrimarySkills);
                                    CanNotify = true;
                                }

                            }
                            foreach (string SSkill in SecondarySkillIds)
                            {
                                long Sid = Convert.ToInt64(SSkill.Split(':')[0]);
                                EmployeeSkill empsskill = db.EmployeeSkills.Where(sk => sk.EmployeeId == employee.EmployeeId && sk.Skills == Sid).FirstOrDefault();
                                if (empsskill == null)
                                {

                                    MasterLookUp MsLookup = db.MasterLookUps.Where(m => m.LookupID == Sid).FirstOrDefault();
                                    var EmpSecondarySkills = new EmployeeSkill();
                                    EmpSecondarySkills.EmployeeId = employee.EmployeeId;
                                    EmpSecondarySkills.Employee = employee;
                                    EmpSecondarySkills.EmployeeName = employee.FirstName + " " + employee.MiddleName + " " + employee.LastName;
                                    EmpSecondarySkills.IsPrimary = false;
                                    EmpSecondarySkills.Skills = Convert.ToInt64(SSkill.Split(':')[0]);
                                    EmpSecondarySkills.Rating = Convert.ToInt32(SSkill.Split(':')[1]);
                                    EmpSecondarySkills.MasterLookUp = MsLookup;
                                    EmpSecondarySkills.SkillDescription = MsLookup.Description;
                                    EmpSecondarySkills.Status = ConfigurationManager.AppSettings["Not Yet Approved"].ToString();
                                    db.EmployeeSkills.Add(EmpSecondarySkills);
                                    CanNotify = true;
                                }
                            }
                            if (CanNotify)
                            {
                                //Notification 
                                Notification tblNotification = new Notification();
                                tblNotification.NotificationType = ConfigurationManager.AppSettings["SkillApprovalRequest_NotificationType"].ToString();
                                tblNotification.NotificationDate = System.DateTime.Now;
                                tblNotification.NotificationFrom = employee.EmployeeId;
                                tblNotification.NotificationTo = employee.SupervisorId;
                                var Body = ConfigurationManager.AppSettings["SkillApprovalRequest_Notification"].ToString() + employee.FirstName + " " + employee.LastName;
                                tblNotification.IsActive = true;
                                tblNotification.AssetID = ConfigurationManager.AppSettings["Skill_AppCode"];
                                tblNotification.ApplicationCode = ConfigurationManager.AppSettings["Skill_AppCode"].ToString();

                                string talentoURL = ConfigurationManager.AppSettings["Talento"];
                                string LinktoOpen = "Please Click on link to View the skills submitted for Approval " + talentoURL;
                                string body = string.Empty;
                                using (StreamReader reader = new StreamReader(Path.Combine(Env.ContentRootPath, "Views/Shared/EmailTemplate.html")))
                                {
                                    body = reader.ReadToEnd();
                                }
                                body = body.Replace("{ToUserName}", db.Employees.Where(e => e.EmployeeId == tblNotification.NotificationTo).Select(s => s.FirstName + " " + s.LastName).FirstOrDefault());
                                body = body.Replace("{FromUserName}", db.Employees.Where(e => e.EmployeeId == tblNotification.NotificationFrom).Select(s => s.FirstName + " " + s.LastName).FirstOrDefault());
                                body = body.Replace("{Description}", Body);
                                body = body.Replace("{LinktoOpen}", LinktoOpen);

                                tblNotification.NotificationMessage = body;
                                string IsEmailSent = System.Configuration.ConfigurationManager.AppSettings["IsEmailSent"].ToString();
                                bool IsEmail = Convert.ToBoolean(IsEmailSent);
                                if (IsEmail == true)
                                {
                                    db.Notifications.Add(tblNotification);
                                }
                            }

                            db.SaveChanges();
                        }
                    }
                    //return RedirectToAction("AddNewSkills", "EmployeeSkills");
                    return Json("Success");
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
                    return Json("Error");
                }
                ////catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
                ////{
                ////    Exception raise = dbEx;
                ////    foreach (var validationErrors in dbEx)
                ////    {
                ////        foreach (var validationError in validationErrors.ValidationErrors)
                ////        {
                ////            string message = string.Format("{0}:{1}",
                ////                validationErrors.Entry.Entity.ToString(),
                ////                validationError.ErrorMessage);
                ////            // raise a new exception nesting
                ////            // the current instance as InnerException
                ////            raise = new InvalidOperationException(message, raise);
                ////        }
                ////    }
                ////    throw raise;
                ////}
            }
            else
            {
                //ermsg = "Session expired";
                return Json("Sessionexpired");
                //return RedirectToAction("SessionExpire", "Signout");
            }
        }
        private List<Training> GetEmployeeTrainingDetails()
        {
            List<Training> objStudent = new List<Training>();

            var _objuserdetail = (from data in db.Trainings
                                  select data);
            foreach (var item in _objuserdetail)
            {
                objStudent.Add(new Training { ProgramName = item.ProgramName, StartDate = item.StartDate, EndDate = item.EndDate, AttendanceStatus = item.AttendanceStatus, TotalHours = item.TotalHours, Source = item.Source, Category = item.Category, Mode = item.Mode });
                // objStudent.Add(new EmployeeSkill { EmployeeId = item.EmployeeId, SkillDescription = item.MasterLookUp.Description, IsPrimary = item.IsPrimary,Comments=item.Comments});
            }
            return objStudent;
        }

        private List<EmployeeSkills_NewDetails> GetEmployeeSkill(string EmpID)
        {
            //int EmpId = Convert.ToInt32(HttpContext.Session.GetString("EmployeeId"));
            Int32 EmployeeId = Convert.ToInt32(EmpID);
            List<EmployeeSkills_NewDetails> objStudent = new List<EmployeeSkills_NewDetails>();
            var employeeId = new SqlParameter("@EmployeeId", EmployeeId);
            objStudent = db.Set<EmployeeSkills_NewDetails>().FromSqlRaw("exec GetEmployeeSkills @EmployeeId", employeeId).ToList();
            //var _objuserdetail = (from data in db.EmployeeSkills_New
            //                      select data);
            //foreach (var item in objStudent)
            //{
            //    objStudent.Add(new EmployeeSkills_NewDetails { Skillname = item.Skillname,CompetincyName = item.CompetincyName, Expertiselevel = item.Expertiselevel, TechnologyLastUsed = item.TechnologyLastUsed, SkillStatus = item.SkillStatus });
            //    // objStudent.Add(new EmployeeSkill { EmployeeId = item.EmployeeId, SkillDescription = item.MasterLookUp.Description, IsPrimary = item.IsPrimary,Comments=item.Comments});
            //}
            return objStudent;
        }

        public void DownloadMyResume(int EmployeeID, int flag)
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                try
                {

                    var objEmpDOC = (from ed in db.Employees
                                     where ed.EmployeeId.Equals(EmployeeID)
                                     select new
                                     {
                                         EmpId = ed.EmployeeId,
                                         EmpName = ed.FirstName, //+ ((ed.MiddleName.Trim() != string.Empty) ? " " + ed.MiddleName + " " : " ") + ed.LastName,
                                         EmpCV = ed.Resumes,
                                         EmpCVPdf = ed.ResumePdf
                                     }
                            ).FirstOrDefault();
                    //int Flag = 0;

                    int count = 0;
                    if (objEmpDOC != null)
                    {
                        string strFileName = objEmpDOC.EmpName.Replace(' ', '_') + "_" + objEmpDOC.EmpId;

                        Response.Clear();
                        
                        if (objEmpDOC.EmpCV != null && flag == 1)
                        {
                            ++count;
                            Response.ContentType = "application/word";
                            Response.Headers.Add("content-disposition", "attachment;filename=" + strFileName + ".doc");     // to open file prompt Box open or Save file         
                            Response.Body.Write((byte[])objEmpDOC.EmpCV);
                        }
                        if (objEmpDOC.EmpCVPdf != null && flag == 2)
                        {
                            ++count;
                            Response.ContentType = "application/x-mspowerpoint";
                            Response.Headers.Add("content-disposition", "attachment;filename=" + strFileName + ".pptx");     // to open file prompt Box open or Save file         
                            Response.Body.Write((byte[])objEmpDOC.EmpCVPdf);
                        }
                        
                        
                        
                    }

                    if (count == 0)
                    {
                        TempData["IsFile"] = "File is not available";

                        Response.Clear();
                        
                        
                        
                        

                    }
                    // return RedirectToAction("AddNewSkills");


                }
                catch (Exception ex)
                {

                    Common.WriteExceptionErrorLog(ex);
                    // return RedirectToAction("Error", "Error");
                }
            }
            else
            {
                //ermsg = "Session expired";
                //return Json("Sessionexpired");
                //return RedirectToAction("SessionExpire", "Signout");
            }
            //return RedirectToAction("AddNewSkills");
        }
        public ActionResult GetEmployeeDetailsByID(string EmpID)
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                try
                {
                    Int32 EmployeeId = Convert.ToInt32(EmpID);

                    ViewData["EmployeeAddressDetails"] = "";
                    ViewData["ReportingManager"] = "";

                    EmployeeAddressInformation objEAI = db.EmployeeAddressInformations.Where(eai => eai.EmployeeId.Equals(EmployeeId)).FirstOrDefault();

                    if (objEAI != null)
                    {
                        System.Text.StringBuilder sbCurrentAddress = new System.Text.StringBuilder();

                        if (!string.IsNullOrWhiteSpace(objEAI.CurrentAddressDoorNo))
                        {
                            sbCurrentAddress.Append("<h4 class='bold sub-headtext' style='font-size: 16px;'>Present Address</h4>");
                            sbCurrentAddress.Append("<p style='font-size: 13px'>" + objEAI.CurrentAddressDoorNo + "</p>");
                        }
                        if (!string.IsNullOrWhiteSpace(objEAI.CurrentAddressStreet))
                        {
                            sbCurrentAddress.Append("<p style='font-size: 13px; margin-bottom: 5px;'>" + objEAI.CurrentAddressStreet + "</p>");
                        }
                        if (!string.IsNullOrWhiteSpace(objEAI.CurrentAddressArea))
                        {
                            sbCurrentAddress.Append("<p style='font-size: 13px; margin-bottom: 5px;'>" + objEAI.CurrentAddressArea + "</p>");
                        }
                        if (!string.IsNullOrWhiteSpace(objEAI.CurrentAddressCity))
                        {
                            sbCurrentAddress.Append("<p style='font-size: 13px; margin-bottom: 5px;'>" + objEAI.CurrentAddressCity + "</p>");
                        }
                        if (!string.IsNullOrWhiteSpace(objEAI.CurrentAddressState))
                        {
                            sbCurrentAddress.Append("<p style='font-size: 13px; margin-bottom: 5px;'>" + objEAI.CurrentAddressState + "</p>");
                        }
                        if (!string.IsNullOrWhiteSpace(objEAI.CurrentAddressCountry))
                        {
                            sbCurrentAddress.Append("<p style='font-size: 13px; margin-bottom: 5px;'>" + objEAI.CurrentAddressCountry + "</p>");
                        }
                        if (!string.IsNullOrWhiteSpace(objEAI.CurrentAddressPostalCode))
                        {
                            sbCurrentAddress.Append("<p style='font-size: 13px; margin-bottom: 5px;'>" + objEAI.CurrentAddressPostalCode + "</p>");
                        }

                        ViewData["EmployeeAddressDetails"] = sbCurrentAddress.ToString();
                    }

                    ViewData["ProjectAssignmentDetails"] = GetEmployeeAssignmentDetails(EmployeeId);
                    ViewData["SearchedEmployeeSkills"] = GetEmployeeSkill(EmployeeId.ToString()).ToList();
                    ViewData["EmployeeTrainingDetails"] = GetEmployeeTrainingDetails().Where(emp => emp.EmployeeID == EmployeeId).ToList();
                    //  ViewData["SearchedEmployeenewSkills"] = GetEmployeeSkill_New().Where(emp => emp.EmployeeId == EmployeeId).ToList();

                    Employee objEmp = db.Employees.Find(Convert.ToInt32(EmployeeId));
                    if (objEmp != null)
                    {
                        var reportingManger = db.Employees.Where(e => e.EmployeeId == objEmp.SupervisorId).Select(em => em.FirstName + " " + em.LastName).FirstOrDefault();
                        ViewData["ReportingManager"] = reportingManger;
                    }

                    return PartialView("_EmployeeDetails", objEmp);
                }
                catch (Exception ex)
                {

                    Common.WriteExceptionErrorLog(ex);
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
        public ActionResult DeleteSkillByID(Int32 SkillId)
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                try
                {
                    if (SkillId != 0)
                    {
                        EmployeeSkill objDeleteSkill = db.EmployeeSkills.Find(SkillId);

                        if (objDeleteSkill != null)
                        {
                            db.EmployeeSkills.Remove(objDeleteSkill);
                            db.SaveChanges();

                            return Json("deleted");
                        }
                    }
                    return Json("");
                }
                catch (Exception ex)
                {

                    Common.WriteExceptionErrorLog(ex);
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


        private List<SkillMasterinfo> Skilmasterlst()
        {
            int empId = Convert.ToInt32(HttpContext.Session.GetString("EmployeeId"));

            List<SkillMasterinfo> lstskilmaster = new List<SkillMasterinfo>();

            lstskilmaster = (from sm in db.SkillMasters
                             select new SkillMasterinfo
                             {
                                 Skillset = sm.Skillset,
                                 SkillId = sm.SkillId
                             }).ToList();

            return lstskilmaster;
        }

        public JsonResult GetCompetencyList(string Skillname, string flag)
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                try
                {
                    var result = new List<KeyValuePair<string, string>>();

                    List<SelectListItem> lstCompetency = db.SkillMasters.Where(s => s.Skillset.ToLower().Equals(Skillname.ToLower())).
                        Select(s => new SelectListItem { Text = s.SkillCategory, Value = s.SkillId.ToString() })
                        .Distinct()
                        .OrderBy(o => o.Text)
                        .ToList();

                    foreach (var item in lstCompetency)
                    {
                        result.Add(new KeyValuePair<string, string>(item.Text, item.Value));
                    }

                    if (lstCompetency.Count > 0)
                    {
                        var filteredResult = new List<KeyValuePair<string, string>>();

                        filteredResult = result.Select(w => w).ToList();


                        if (filteredResult.Count == 0)
                        {
                            filteredResult.Add(new KeyValuePair<string, string>("-- not found with keyword --", ""));
                        }

                        return Json(filteredResult);
                    }
                    return Json("");
                }

                catch (Exception ex)
                {

                    Common.WriteExceptionErrorLog(ex);
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

        private List<EmployeeSkills_NewDetails> SelectSkill()
        {
            int empId = Convert.ToInt32(HttpContext.Session.GetString("EmployeeId"));


            List<EmployeeSkills_NewDetails> lstskillmaster = new List<EmployeeSkills_NewDetails>();

            lstskillmaster = (from pl in db.SkillMasters
                              join p in db.EmployeeSkills_New on pl.SkillId equals p.SkillId

                              where p.EmployeeId == empId
                              select new EmployeeSkills_NewDetails
                              {
                                  Skillname = pl.Skillset,
                                  SkillId = pl.SkillId

                              }).ToList<EmployeeSkills_NewDetails>();

            return lstskillmaster;

        }
        public JsonResult GetSkillsListByCompetency(string skill, string flag)
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                try
                {
                    var result = new List<KeyValuePair<string, string>>();


                    List<SelectListItem> lstSkill = db.SkillMasters
                        //.Where(m => m.SkillCategory.Equals(cmptny))
                        .Distinct()
                    .Select(s => new SelectListItem { Text = s.Skillset, Value = s.Skillset.ToString() })
                    .OrderBy(o => o.Text)
                    .ToList();

                    foreach (var item in lstSkill)
                    {
                        if (item.Text != null && item.Text != string.Empty)
                            result.Add(new KeyValuePair<string, string>(item.Text, item.Value));
                    }

                    if (lstSkill.Count > 0)
                    {
                        var filteredResult = new List<KeyValuePair<string, string>>();

                        if (flag.ToLower() == "exactly")
                        {
                            filteredResult = result.Where(s => s.Key.ToLower().Equals(skill.ToLower())).Distinct().Select(w => w).ToList();
                        }
                        else if (skill.ToLower().Length > 0)
                        {
                            if (skill != string.Empty && skill != null)
                                filteredResult = result.Where(s => s.Key.ToLower().Contains(skill.ToLower())).Distinct().Select(w => w).ToList();
                        }
                        else
                        {
                            filteredResult = result.Select(w => w).Distinct().ToList();
                        }

                        if (filteredResult.Count == 0)
                        {
                            filteredResult.Add(new KeyValuePair<string, string>("-- not found with keyword --", ""));
                        }

                        return Json(filteredResult);
                    }
                    return Json("");
                }
                catch (Exception ex)
                {

                    Common.WriteExceptionErrorLog(ex);
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
        public ActionResult GetTrainings()
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                try
                {
                    using (TrianzOperationsEntities _db = new TrianzOperationsEntities())
                    {
                        int EmpId = Convert.ToInt32(HttpContext.Session.GetString("EmployeeId"));
                        List<EmployeeTrainingDetails> traingdetails = new List<EmployeeTrainingDetails>();
                        // String Message = "There are no trainings";
                        //var employeeId = new SqlParameter("@EmployeeId", EmpId);
                        traingdetails = (from tr in _db.Trainings
                                         where tr.EmployeeID == EmpId
                                         select new EmployeeTrainingDetails

                                         {
                                             ProgramName = tr.ProgramName,
                                             StartDate = tr.StartDate,
                                             EndDate = tr.EndDate,
                                             EmployeeID = tr.EmployeeID,
                                             Location = tr.Location,
                                             Practice = tr.Practice,
                                             AttendanceStatus = tr.AttendanceStatus,
                                             TotalHours = tr.TotalHours,
                                             Source = tr.Source,
                                             Category = tr.Category,
                                             Mode = tr.Mode


                                         }).ToList();

                        return PartialView("_MyTrainings", traingdetails);



                    }
                }
                //catch (Exception ex)
                //{
                //    string message =
                //                           "EXCEPTION TYPE:" + ex.GetType() + Environment.NewLine +
                //                           "EXCEPTION MESSAGE: " + ex.Message + Environment.NewLine +
                //                           "STACK TRACE: " + ex.StackTrace + Environment.NewLine;
                //    System.Diagnostics.StackTrace trace = new System.Diagnostics.StackTrace(ex, true);

                //    if (ex.InnerException != null)
                //    {
                //        message += "---BEGIN InnerException--- " + Environment.NewLine +
                //                   "Exception type " + ex.InnerException.GetType() + Environment.NewLine +
                //                   "Exception message: " + ex.InnerException.Message + Environment.NewLine +
                //                   "Stack trace: " + ex.InnerException.StackTrace + Environment.NewLine +
                //                   "---END Inner Exception";
                //    }


                //    return View("error");
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
                //return Json("Sessionexpired");
                return RedirectToAction("SessionExpire", "Signout");
            }
        }


        //added for resume download
        [HttpGet]
        public ActionResult IsResumecheck(int empid)
        {
            int result = 0;
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                var emp = db.Employees.Where(a => a.EmployeeId == empid).FirstOrDefault();
                if (emp != null)
                {
                    if (emp.Resumes != null)
                    {
                        result += 1;
                    }
                    if (emp.ResumePdf != null)
                    {
                        result += 2;
                    }
                }
            }
            return Json(result);
        }
		[HttpGet]		 
        public ActionResult IsEmployeeActive(int empid)
        {
            int result = 0;
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                var emp = db.Employees.Where(a => a.EmployeeId == empid && a.IsActive==true).FirstOrDefault();
                if (emp != null)
                {
                    if (emp.AnniversaryMailerFlag ==true)
                    {
                        result += 1;
                    }
                    if (emp.BirthdayMailerFlag ==true)
                    {
                        result += 2;
                    }
                }
            }
            return Json(result);
        }	 
        public ActionResult GetSkilldetails()
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                try
                {
                    {
                        int empId = Convert.ToInt32(HttpContext.Session.GetString("EmployeeId"));
                        
                        var lstSkill = from pl in db.SkillMasters
                                       join p in db.EmployeeSkills_New on pl.SkillId equals p.SkillId
                                       where p.EmployeeId == empId
                                       select pl.Skillset;
                        ViewBag.lstSkill = lstSkill.ToList();
                        EmployeeSkills_NewDetails obj = new EmployeeSkills_NewDetails();
                        ValidationModel objvalidation = new ValidationModel();
                        objvalidation.lstskills = Skilmasterlst();
                        objvalidation.lstselectskills = SelectSkill();
                        ViewBag.AllYears = GetYear();
                        int EmpId = Convert.ToInt32(HttpContext.Session.GetString("EmployeeId"));
                        List<EmployeeSkills_NewDetails> emplyeeskills = new List<EmployeeSkills_NewDetails>();
                        var employeeId = new SqlParameter("@EmployeeId", EmpId);
                        // emplyeeskills = _db.Set<EmployeeSkills_NewDetails>().FromSqlRaw("exec GetEmployeeSkills @EmployeeId", employeeId).ToList();
                        objvalidation.EmployeeSkills_NewDetailslst = db.Set<EmployeeSkills_NewDetails>().FromSqlRaw("exec GetEmployeeSkills @EmployeeId", employeeId).ToList();
                        return PartialView("_AddNewSkills", objvalidation);
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
                //return Json("Sessionexpired");
                return RedirectToAction("SessionExpire", "Signout");
            }
        }
        //public ActionResult GetSkilldetails()
        //{
        //    if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
        //    {


        //        try
        //        {
        //            using (TrianzOperationsEntities _db = new TrianzOperationsEntities())
        //            {
        //                ViewBag.AllYears = GetYear();
        //                int EmpId = Convert.ToInt32(HttpContext.Session.GetString("EmployeeId"));
        //                var lstSkill = from pl in db.SkillMasters
        //                               join p in db.EmployeeSkills_New on pl.SkillId equals p.SkillId
        //                               where p.EmployeeId == EmpId
        //                               select pl.Skillset;
        //                ViewBag.lstSkill = lstSkill.ToList();
        //                List<EmployeeSkills_NewDetails> emplyeeskills = new List<EmployeeSkills_NewDetails>();
        //                ValidationModel objvalidation = new ValidationModel();
        //                objvalidation.lstskills = Skilmasterlst();
        //                objvalidation.lstselectskills = SelectSkill();
        //                var employeeId = new SqlParameter("@EmployeeId", EmpId);
        //               // emplyeeskills = _db.Set<EmployeeSkills_NewDetails>().FromSqlRaw("exec GetEmployeeSkills @EmployeeId", employeeId).ToList();
        //                objvalidation.EmployeeSkills_NewDetailslst = _db.Set<EmployeeSkills_NewDetails>().FromSqlRaw("exec GetEmployeeSkills @EmployeeId", employeeId).ToList();

        //                return PartialView("_AddNewSkills", objvalidation);
        //            }


        //        }
        //        //catch (Exception ex)
        //        //{
        //        //    string message =
        //        //           "EXCEPTION TYPE:" + ex.GetType() + Environment.NewLine +
        //        //           "EXCEPTION MESSAGE: " + ex.Message + Environment.NewLine +
        //        //           "STACK TRACE: " + ex.StackTrace + Environment.NewLine;
        //        //    System.Diagnostics.StackTrace trace = new System.Diagnostics.StackTrace(ex, true);

        //        //    if (ex.InnerException != null)
        //        //    {
        //        //        message += "---BEGIN InnerException--- " + Environment.NewLine +
        //        //                   "Exception type " + ex.InnerException.GetType() + Environment.NewLine +
        //        //                   "Exception message: " + ex.InnerException.Message + Environment.NewLine +
        //        //                   "Stack trace: " + ex.InnerException.StackTrace + Environment.NewLine +
        //        //                   "---END Inner Exception";
        //        //    }


        //        //    return View("error");
        //        //}
        //        catch (Exception ex)
        //        {

        //            Common.WriteExceptionErrorLog(ex);
        //            return RedirectToAction("Error", "Error");
        //        }
        //    }
        //    else
        //    {
        //        //ermsg = "Session expired";
        //        //return Json("Sessionexpired");
        //        return RedirectToAction("SessionExpire", "Signout");
        //    }
        //}
        public ActionResult SaveSkilldata(string skillid, string skill, string Expertise, string Mont, string Year,
            string employeeskillID, string IsPrimaryBool)
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                bool IsPrimary;
             
                if (IsPrimaryBool == "true")
                    IsPrimary = true;
                else
                    IsPrimary = false;

                try
                {
                    int emskillid = 0;
                    int skilid = 0;
                    emskillid = Convert.ToInt32(employeeskillID);
                    skilid = Convert.ToInt32(skillid);
                    int EmpId = Convert.ToInt32(HttpContext.Session.GetString("EmployeeId"));
                    if (EmpId > 0)
                    {
                        if (emskillid == 0)
                        {
                            EmployeeSkills_New emt = new EmployeeSkills_New();
                            emt.SkillId = skilid;
                            emt.SkillStatus = "Draft";
                            emt.Expertiselevel = Expertise;
                            emt.TechnologyLastUsed = Mont + "," + Year;
                            emt.EmployeeId = EmpId;
                            emt.LastModifiedDate = DateTime.Now;
                            emt.IsPrimary = IsPrimary;
                            db.EmployeeSkills_New.Add(emt);

                        }
                        else
                        {
                            var _empskill = (from st in db.EmployeeSkills_New
                                             where st.EmployeeSkillId == emskillid
                                             select st
                                       ).FirstOrDefault();

                            if (_empskill != null)
                            {
                                if (_empskill.SkillStatus != "Draft")
                                {
                                    EmployeeSkillHistory em = new EmployeeSkillHistory();
                                    em.SkillIDOld = _empskill.SkillId;
                                    em.EMployeeID = _empskill.EmployeeId;
                                    em.ExpertiselevelOld = _empskill.Expertiselevel;
                                    em.TechnologyLastUsedOld = _empskill.TechnologyLastUsed;
                                    em.PreviousStatus = _empskill.SkillStatus;
                                    em.Comments = _empskill.SkillStatus + " Skill Updated";
                                    em.Expertiselevel = Expertise;
                                    em.ModifiedDate = DateTime.Now;
                                    em.SkillID = skilid;
                                    em.TechnologyLastUsed = Mont + "," + Year;
                                    em.UpdateStatus = "Modified";
                                    em.IsPrimary = IsPrimary;                                   
                                    db.EmployeeSkillHistories.Add(em);

                                    _empskill.SkillStatus = "Draft";
                                }
                                _empskill.SkillId = skilid;
                                _empskill.TechnologyLastUsed = Mont + "," + Year;
                                _empskill.LastModifiedDate = DateTime.Now;
                                _empskill.Expertiselevel = Expertise;
                                _empskill.IsPrimary = IsPrimary;
                            }

                        }

                        db.SaveChanges();

                        return Json("true");
                    }
                    else
                    {
                        return Json("false");
                    }


                }
                //catch (Exception ex)
                //{
                //    string message =
                //           "EXCEPTION TYPE:" + ex.GetType() + Environment.NewLine +
                //           "EXCEPTION MESSAGE: " + ex.Message + Environment.NewLine +
                //           "STACK TRACE: " + ex.StackTrace + Environment.NewLine;
                //    System.Diagnostics.StackTrace trace = new System.Diagnostics.StackTrace(ex, true);

                //    if (ex.InnerException != null)
                //    {
                //        message += "---BEGIN InnerException--- " + Environment.NewLine +
                //                   "Exception type " + ex.InnerException.GetType() + Environment.NewLine +
                //                   "Exception message: " + ex.InnerException.Message + Environment.NewLine +
                //                   "Stack trace: " + ex.InnerException.StackTrace + Environment.NewLine +
                //                   "---END Inner Exception";
                //    }


                //    return View("error");
                //}
                catch (Exception ex)
                {

                    Common.WriteExceptionErrorLog(ex);
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
        public ActionResult DeleteEmployeeSkillByID(string SkillId)
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                try
                {

                    int skill = 0;
                    skill = Convert.ToInt32(SkillId);
                    if (skill != 0)
                    {
                        EmployeeSkills_New objDeleteSkill = db.EmployeeSkills_New.Find(skill);

                        if (objDeleteSkill != null)
                        {
                            EmployeeSkillHistory em = new EmployeeSkillHistory();
                            // em.SkillIDOld = objDeleteSkill.SkillId;
                            //em.EMployeeID = objDeleteSkill.EmployeeId;
                            //em.ExpertiselevelOld = objDeleteSkill.Expertiselevel;
                            //em.TechnologyLastUsedOld = objDeleteSkill.TechnologyLastUsed;
                            //em.PreviousStatus = objDeleteSkill.SkillStatus;
                            //em.Comments = "Skill deleted";
                            //em.Expertiselevel = objDeleteSkill.Expertiselevel;
                            // em.ModifiedDate = DateTime.Now;
                            //em.SkillID = objDeleteSkill.SkillId;
                            //em.TechnologyLastUsed = objDeleteSkill.TechnologyLastUsed;
                            //em.UpdateStatus = "Deleted";
                            //db.EmployeeSkillHistories.Add(em);

                            db.EmployeeSkills_New.Remove(objDeleteSkill);
                            db.SaveChanges();

                            return Json("deleted");
                        }
                    }
                    return Json("");
                }
                catch (Exception ex)
                {

                    Common.WriteExceptionErrorLog(ex);
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


      

        public ActionResult SendforApproval(List<EmployeeSkills_New> Skillslist)
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {

                try
                {
                    int EmpId = Convert.ToInt32(HttpContext.Session.GetString("EmployeeId"));

                    List<EmployeeSkills_New> Exatingskills = new List<EmployeeSkills_New>();
                    if (Skillslist.Count > 0)
                    {
                        for (int i = 0; i < Skillslist.Count; i++)
                        {
                            int emskillid = Convert.ToInt32(Skillslist[i].EmployeeSkillId);
                            var skil = (from et in db.EmployeeSkills_New
                                        where et.EmployeeSkillId == emskillid
                                        select et).ToList();
                            if (skil != null)
                            {
                                if (skil.Count > 0)
                                {
                                    EmployeeSkillHistory em = new EmployeeSkillHistory();
                                    //em.SkillIDOld = skil[0].SkillId;
                                    em.EMployeeID = skil[0].EmployeeId;
                                    em.ExpertiselevelOld = skil[0].Expertiselevel;
                                    em.TechnologyLastUsedOld = skil[0].TechnologyLastUsed;
                                    em.PreviousStatus = skil[0].SkillStatus;
                                    em.Comments = "Skill Submitted";
                                    em.Expertiselevel = skil[0].Expertiselevel;
                                    em.ApprovalSubmissionDate = DateTime.Now;
                                    em.IsPrimary = skil[0].IsPrimary;
                                    em.SkillID = skil[0].SkillId;
                                    em.TechnologyLastUsed = skil[0].TechnologyLastUsed;
                                    em.UpdateStatus = "Submitted";
                                    db.EmployeeSkillHistories.Add(em);

                                    skil[0].SkillStatus = "Submitted";
                                    skil[0].ApprovalSubmissionDate = DateTime.Now;
                                    db.SaveChanges();
                                }
                            }

                        }

                    }
                    return Json("true");

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
                //return Json("Sessionexpired");
                return RedirectToAction("SessionExpire", "Signout");
            }
        }
      public void DownloadTrianzResumeTemplate(int flag)
        {
                if (flag == 1)
                {
                    byte[] fileBytes = System.IO.File.ReadAllBytes(Path.Combine(Env.ContentRootPath, "Content", "Document", "Profile doc trianz.docx"));
                    string fileName = "Profile doc trianz.docx";
                    Response.ContentType = "application/word";
                    Response.Headers.Add("content-disposition", "attachment;filename=" + fileName);
                    Response.Body.Write((byte[])fileBytes);
                }
                if(flag==2)
                {
                    byte[] fileBytes = System.IO.File.ReadAllBytes(Path.Combine(Env.ContentRootPath, "Content", "Document", "Trianz Profile.pptx"));
                    string fileName = "Trianz Profile.pptx";
                    Response.ContentType = "application/x-mspowerpoint";
                    Response.Headers.Add("content-disposition", "attachment;filename=" + fileName);      
                    Response.Body.Write((byte[])fileBytes);
                }
               if (flag == 3)
               {
                byte[] fileBytes = System.IO.File.ReadAllBytes(Path.Combine(Env.ContentRootPath, "Content", "Document", "Training Sample Template.xlsx"));
                string fileName = "Training Sample Template.xlsx";
                Response.ContentType = "application/ms-excel";
                Response.Headers.Add("content-disposition", "attachment;filename=" + fileName);            
                Response.Body.Write((byte[])fileBytes);
               }
                
                
                
            }
      
        public FileResult DownloadTrianzProfileTemplate()
        {
            byte[] fileBytes = System.IO.File.ReadAllBytes(Path.Combine(Env.ContentRootPath, "Content", "Document", "Trianz Resume Template - 2016.docx"));
            string fileName = "Trianz Resume Template - 2016.docx";
         //   return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
             return File(fileBytes, "application/octet-stream", fileName);
        }


        public ActionResult GenerateSkillsetReport()
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                try
                {

                    var lstSkillSet = db.SkillMasters.Where(sk => sk.IsActive == true).OrderBy(sk => sk.SkillCategory).ToList();
                    // var lstSkillSet = db.SkillMaster.OrderBy(s => s.SkillCategory).Select(s=>s).ToList();
                    using (ExcelPackage package = new ExcelPackage())
            {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Skill Repository");
                        worksheet.TabColor = System.Drawing.Color.Green;
                        worksheet.DefaultRowHeight = 30f;
                        worksheet.Row(1).Height = 20f;

                        using (var range = worksheet.Cells[1, 1, 1, 2])
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
                        worksheet.Cells[1, 1].Value = "Skill Set";
                        worksheet.Cells[1, 2].Value = "Skill Category";


                        worksheet.DefaultColWidth = 18f;
                        worksheet.Column(1).Width = 12f;
                        worksheet.Column(2).AutoFit(42f);

                        //Add the each row
                        for (int rowIndex = 0, row = 2; rowIndex < lstSkillSet.Count; rowIndex++, row++) // row indicates number of rows
                        {
                            worksheet.Cells[row, 1].Value = lstSkillSet[rowIndex].Skillset;
                            worksheet.Cells[row, 2].Value = lstSkillSet[rowIndex].SkillCategory;

                            if (row % 2 == 1)
                            {
                                using (var range = worksheet.Cells[row, 1, row, 13])//21
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
                        
                        Response.Headers.Add("content-disposition", "attachment;filename=SkillRepository" + ".xlsx");

                        
                        Response.ContentType = "application/vnd.ms-excel";
                        StringWriter sw = new StringWriter();
                        Response.Body.Write(fileBytes);
                        
                    }

                    return new EmptyResult();
                }
                catch (Exception ex)
                {

                    Common.WriteExceptionErrorLog(ex);
                    return Json("Error");
                }

            }
            else
            {
                //ermsg = "Session expired";
                //return Json("Sessionexpired");
                return RedirectToAction("SessionExpire", "Signout");
            }
        }
        public JsonResult GetDomains()
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                try
                {
                    JsonResult js = new JsonResult(null);
                    List<DomainValue> dom_value = new List<DomainValue>();
                    int EmpId = Convert.ToInt32(HttpContext.Session.GetString("EmployeeId"));
                    var employeeId = new SqlParameter("@EmployeeId", EmpId);
                    dom_value = db.Set<DomainValue>().FromSqlRaw("exec GetDomainValue @EmployeeId", employeeId).ToList();
                    js.Value = dom_value;
                    return js;

                }
                catch (Exception ex)
                {

                    Common.WriteExceptionErrorLog(ex);
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

        public JsonResult GetEmployeeDomains()
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                try
                {

                    JsonResult js = new JsonResult(null);

                    int EmpId = Convert.ToInt32(HttpContext.Session.GetString("EmployeeId"));
                    if (EmpId > 0)
                    {
                        List<DomainValue> dom_value = new List<DomainValue>();
                        dom_value = (from ed in db.EmployeeWiseDomains
                                     join d in db.Domains on
                                     ed.DomainID equals d.DomainID
                                     where ed.EmployeeID == EmpId
                                     select new DomainValue() { DomainId = ed.DomainID.Value, DomainName = d.DomainName }).ToList();
                        js.Value = dom_value;
                    }

                    return js;

                }
                catch (Exception ex)
                {

                    Common.WriteExceptionErrorLog(ex);
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
        public void SaveDomainIndustry(string DomainValue)
        {
            try
            {
                string[] val = DomainValue.Split(',');
                int EmpId = Convert.ToInt32(HttpContext.Session.GetString("EmployeeId"));
                if (EmpId > 0)
                {
                    for (int i = 0; i < val.Length; i++)
                    {
                        if (val[i] != null && val[i] != string.Empty)
                        {
                            int id = Convert.ToInt32(val[i]);
                            var EmployeeWiseDomain = new EmployeeWiseDomain();
                            EmployeeWiseDomain.EmployeeID = EmpId;
                            EmployeeWiseDomain.DomainID = id;
                            EmployeeWiseDomain.CreatedDate = DateTime.Now;
                            db.EmployeeWiseDomains.Add(EmployeeWiseDomain);

                            db.SaveChanges();
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                string message =
                       "EXCEPTION TYPE:" + ex.GetType() + Environment.NewLine +
                       "EXCEPTION MESSAGE: " + ex.Message + Environment.NewLine +
                       "STACK TRACE: " + ex.StackTrace + Environment.NewLine;
                System.Diagnostics.StackTrace trace = new System.Diagnostics.StackTrace(ex, true);

                if (ex.InnerException != null)
                {
                    message += "---BEGIN InnerException--- " + Environment.NewLine +
                               "Exception type " + ex.InnerException.GetType() + Environment.NewLine +
                               "Exception message: " + ex.InnerException.Message + Environment.NewLine +
                               "Stack trace: " + ex.InnerException.StackTrace + Environment.NewLine +
                               "---END Inner Exception";
                }


                // return View("error");
            }

        }
        public ActionResult deletedomain(string DOMId)
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                try
                {
                    int HRRFId = Convert.ToInt32(DOMId);
                    int EmpId = Convert.ToInt32(HttpContext.Session.GetString("EmployeeId"));
                    var chk = db.EmployeeWiseDomains.Where(x => x.DomainID == HRRFId && x.EmployeeID == EmpId).FirstOrDefault();
                    if (chk != null)
                    {
                        EmployeeWiseDomain empdom = new EmployeeWiseDomain();
                        empdom = db.EmployeeWiseDomains.Single(domv => domv.DomainID == HRRFId && domv.EmployeeID == EmpId);
                        db.EmployeeWiseDomains.Remove(empdom);
                        db.SaveChanges();

                        return Json("success");
                    }
                    else
                    {
                        return Json("Fail");
                    }
                }
                catch (Exception ex)
                {

                    Common.WriteExceptionErrorLog(ex);
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
        public ActionResult deleteIndustry(string INDId)
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                try
                {
                    int HRRFId = Convert.ToInt32(INDId);
                    int EmpId = Convert.ToInt32(HttpContext.Session.GetString("EmployeeId"));
                    var chk = db.EmployeeWiseIndustries.Where(x => x.IndustryID == HRRFId && x.EmployeeID == EmpId).FirstOrDefault();
                    if (chk != null)
                    {
                        EmployeeWiseIndustry empInd = new EmployeeWiseIndustry();
                        empInd = db.EmployeeWiseIndustries.Single(domv => domv.IndustryID == HRRFId && domv.EmployeeID == EmpId);
                        db.EmployeeWiseIndustries.Remove(empInd);
                        db.SaveChanges();

                        return Json("success");
                    }
                    else
                    {
                        return Json("Fail");
                    }
                }
                catch (Exception ex)
                {

                    Common.WriteExceptionErrorLog(ex);
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

        public JsonResult GetIndustry()
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                try
                {

                    JsonResult js = new JsonResult(null);
                    List<IndustryValue> Industry_Value = new List<IndustryValue>();
                    int EmpId = Convert.ToInt32(HttpContext.Session.GetString("EmployeeId"));
                    var employeeId = new SqlParameter("@EmployeeId", EmpId);
                    Industry_Value = db.Set<IndustryValue>().FromSqlRaw("exec GetIndustryValue @EmployeeId", employeeId).ToList();
                    js.Value = Industry_Value;
                    return js;
                }
                catch (Exception ex)
                {

                    Common.WriteExceptionErrorLog(ex);
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
        public JsonResult GetEmployeeIndustry()
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                try
                {
                    JsonResult js = new JsonResult(null);

                    int EmpId = Convert.ToInt32(HttpContext.Session.GetString("EmployeeId"));
                    if (EmpId > 0)
                    {
                        List<IndustryValue> Industry_Value = new List<IndustryValue>();
                        Industry_Value = (from ed in db.EmployeeWiseIndustries
                                          join I in db.Industries on
                                        ed.IndustryID equals I.IndustryID
                                          where ed.EmployeeID == EmpId
                                          select new IndustryValue() { IndustryId = ed.IndustryID.Value, IndustryName = I.IndustryName }).ToList();


                        //db.EmployeeWiseDomains.Where(e => e.EmployeeID == EmpId).ToList();


                        js.Value = Industry_Value;
                    }

                    return js;
                }
                catch (Exception ex)
                {

                    Common.WriteExceptionErrorLog(ex);
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
        public void SaveIndustryvalue(string IndustryValue)
        {

            try
            {
                string[] val = IndustryValue.Split(',');
                int EmpId = Convert.ToInt32(HttpContext.Session.GetString("EmployeeId"));
                if (EmpId > 0)
                {
                    for (int i = 0; i < val.Length; i++)
                    {
                        if (val[i] != null && val[i] != string.Empty)
                        {
                            int id = Convert.ToInt32(val[i]);
                            var EmployeeWiseIndustry = new EmployeeWiseIndustry();
                            EmployeeWiseIndustry.EmployeeID = EmpId;
                            EmployeeWiseIndustry.IndustryID = id;
                            EmployeeWiseIndustry.CreatedDate = DateTime.Now;
                            db.EmployeeWiseIndustries.Add(EmployeeWiseIndustry);

                            db.SaveChanges();
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                string message =
                       "EXCEPTION TYPE:" + ex.GetType() + Environment.NewLine +
                       "EXCEPTION MESSAGE: " + ex.Message + Environment.NewLine +
                       "STACK TRACE: " + ex.StackTrace + Environment.NewLine;
                System.Diagnostics.StackTrace trace = new System.Diagnostics.StackTrace(ex, true);

                if (ex.InnerException != null)
                {
                    message += "---BEGIN InnerException--- " + Environment.NewLine +
                               "Exception type " + ex.InnerException.GetType() + Environment.NewLine +
                               "Exception message: " + ex.InnerException.Message + Environment.NewLine +
                               "Stack trace: " + ex.InnerException.StackTrace + Environment.NewLine +
                               "---END Inner Exception";
                }


                // return View("error");
            }

        }

        public class SkillSetReport
        {
            public string SkillCategory { get; set; }
            public string SkillSet { get; set; }

        }

        public JsonResult GetSkillsListByID(string skill)
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                try
                {
                    var skillid = db.SkillMasters.Where(p => p.Skillset.Equals(skill)).ToList();
                    return Json(skillid);
                }
                catch (Exception ex)
                {
                    Common.WriteExceptionErrorLog(ex);
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
        //public class EmployeeWiseDomain
        //{
        //    public int EmployeeDomainID { get; set; }
        //    public Nullable<int> EmployeeId { get; set; }
        //    public Nullable<int> DomainId { get; set; }
        //    public DateTime CreatedDate { get; set; }
        //}
        public class DomainValue
        {
            public string DomainName { get; set; }
            public int DomainId { get; set; }
        }
        public class IndustryValue
        {
            public int IndustryId { get; set; }
            public string IndustryName { get; set; }
        }
    }
}
