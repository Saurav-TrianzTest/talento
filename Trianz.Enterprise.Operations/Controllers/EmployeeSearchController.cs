using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Trianz.Enterprise.Operations.General;
using Trianz.Enterprise.Operations.Models;

namespace Trianz.Enterprise.Operations.Controllers
{

    public class EmployeeSearchController : Controller
    {
        TrianzOperationsEntities db = new TrianzOperationsEntities();
        public ActionResult Index(string currentFilter, int? page, string ddlAccountName,string ddlStatus, string searchskill, int? EmpId)
        {
            string billingStatus;
            if (ddlStatus == null || ddlStatus == "" || ddlStatus == "Select Status")
            {
                billingStatus = "All";
            }
            else
            {
                billingStatus = ddlStatus;
            }

            string accountName;
            if (ddlAccountName == null || ddlAccountName == "" || ddlAccountName == "Select AccountName")
            {
                accountName = "";
            }
            else
            {
                accountName = ddlAccountName;
            }
            List<EmployeeInfoDetails> emp = new List<EmployeeInfoDetails>();
            if (searchskill != null)
            {
                page = 1;
            }
            ViewBag.CurrentFilter = searchskill;

            ViewBag.Subject = "Employee Resume";

            ViewBag.Message = "Please find attached the resume.";

            ViewBag.Email = "";

            List<SelectListItem> lstBillingStatus = new List<SelectListItem>();  
            lstBillingStatus.Add(new SelectListItem { Value = "Billed", Text = "Billed" });
            lstBillingStatus.Add(new SelectListItem { Value = "Bench", Text = "Bench" });
            lstBillingStatus.Add(new SelectListItem { Value = "Shadow", Text = "Shadow" });
            ViewData["StatusType"] = new SelectList(lstBillingStatus, "Value", "Text", billingStatus);

            //Latest dropdown for accountname
            var AccountName = (from projects in db.Projects
                               where projects.IsActive == true
                               // where projects.IsActive == true && projects.SOWEndDate >= DateTime.Now
                               orderby projects.AccountName
                               select new
                               {
                                   Account = projects.AccountName
                               }).Distinct().OrderBy(p => p.Account).ToList();

            ViewData["_AccountName"] = AccountName;

            //Latest dropdown for accountname
            if (searchskill == null)
            {
                searchskill = "";
            }

            emp = db.Database.SqlQuery<EmployeeInfoDetails>
          ("exec [GetEmployeeSkillsInfo_AccountName] @Skillsetlist,@BillingStatus,@AccountName", new SqlParameter("Skillsetlist", searchskill),
          new SqlParameter("BillingStatus", billingStatus), new SqlParameter("AccountName", accountName)).ToList<EmployeeInfoDetails>();
            List<EmployeeInfoDetails> lstcount = emp;
            //var rcount = lstcount.Where(e => e.ResumeFlag == 1).Count();
            var rcount = db.Employees.Where(s => s.IsActive == true && s.Resumes == null).Count();
            ViewBag.Resumecount = rcount;

            //var Resumenotcount = lstcount.Where(e => e.ResumeFlag == 0).Count();
            var Resumenotcount = db.Employees.Where(s => s.IsActive == true && s.Resumes != null).Count();
            ViewBag.ResumeNotupdatedcount = Resumenotcount;

            //var SkillsnotupdatedCount = db.EmployeeSkills_New.Where(s => s.SkillStatus == "Draft").Count();
            List<EmployeeInfoDetails> lstempskillnot = new List<EmployeeInfoDetails>();
            lstempskillnot = db.Database.SqlQuery<EmployeeInfoDetails>
          ("exec GetEmployeeSkillsNotbound").ToList<EmployeeInfoDetails>();

            //var SkillsSubmittedCount = db.EmployeeSkills_New.Where(s => s.SkillStatus == "Approved").Count();
                  

            ViewBag.SkillsnotupdatedCount = lstempskillnot.Count;

            List<EmployeeInfoDetails> upadtedskillemp = new List<EmployeeInfoDetails>();
            upadtedskillemp = db.Database.SqlQuery<EmployeeInfoDetails>
          ("exec GetEmployeeSkillsbound").ToList<EmployeeInfoDetails>();

            ViewBag.SkillsupdatedCount = upadtedskillemp.Count;
            if (EmpId > 0)
            {
                emp = emp.Where(e => e.EmployeeId == EmpId).ToList();
            }
            ValidationModel obj = new ValidationModel();
            obj.EmpSkillInfo = emp;
           var skillcount1 = new List<string>();
            if (searchskill.Trim().Contains(","))
            {
                skillcount1 = searchskill.Split(',').Select(a => a.ToLower().Trim()).ToList();
            }
            else
            {
                var strskill = searchskill.ToLower().Trim();
                skillcount1.Add(strskill.ToLower().Trim());

            }
            skillcount1 = skillcount1.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
            //obj.EmpSkillInfo.ForEach(m =>
            //{
            //    var skillcount2 = m.SkillName.Split(',').Select(a => a.Trim()).ToList();
            //    int count = skillcount2.Intersect(skillcount1).Count();
            //    if (count!=0)
            //    {
            //        m.Percentage = ((decimal)count / skillcount1.Count()) * 100;

            //    }



            //});

            foreach (var item in obj.EmpSkillInfo)
            {

                    var skillcount2 = item.SkillName.Split(',').Select(a => a.ToLower().Trim()).ToList();
                    int count = skillcount2.Intersect(skillcount1).Count();
                    if (count != 0)
                    {
                        item.Percentage = ((decimal)count / skillcount1.Count()) * 100;

                    }

            }
            if (searchskill!="")
            {
                var Percentagevalue = Environment.GetEnvironmentVariable("Percentage") ?? "";
                obj.EmpSkillInfo = obj.EmpSkillInfo.Where(m => (int)m.Percentage >= Convert.ToInt32(Percentagevalue)).ToList();
                obj.EmpSkillInfo = obj.EmpSkillInfo.OrderBy(m => String.IsNullOrEmpty(m.BillingStatus)).ThenBy(m => m.BillingStatus).ThenByDescending(m => m.Percentage).ToList();
            }
            
            //obj.EmpSkillInfo = obj.EmpSkillInfo.OrderBy(m => m.BillingStatus).ThenByDescending(m => m.Percentage).ToList();
            //obj.EmpSkillInfo = obj.EmpSkillInfo.OrderBy(m => String.IsNullOrEmpty(m.BillingStatus)).ThenBy(m => m.BillingStatus).ThenByDescending(m => m.Percentage).ToList();
            return View(obj);
        }

        public ActionResult FilterIndex(string HRRFNumber, int? EmpId)
        {
            string currentFilter = null; int? page = null; string ddlStatus = null; string searchskill = null;string ddlAccountName = null;
            string billingStatus;
            if (ddlStatus == null || ddlStatus == "" || ddlStatus == "Select Status")
            {
                billingStatus = "All";
            }
            else
            {
                billingStatus = ddlStatus;
            }


            string accountName;
            if (ddlAccountName == null || ddlAccountName == "" || ddlAccountName == "Select AccountName")
            {
                accountName = "";
            }
            else
            {
                accountName = ddlAccountName;
            }

            //Latest dropdown for accountname
            var AccountName = (from projects in db.Projects
                               where projects.IsActive == true
                               // where projects.IsActive == true && projects.SOWEndDate >= DateTime.Now
                               orderby projects.AccountName
                               select new
                               {
                                   Account = projects.AccountName
                               }).Distinct().OrderBy(p => p.Account).ToList();

            ViewData["_AccountName"] = AccountName;

            //Latest dropdown for accountname
            List<EmployeeInfoDetails> emp = new List<EmployeeInfoDetails>();
            if (searchskill != null)
            {
                page = 1;
            }
            ViewBag.CurrentFilter = searchskill;

            ViewBag.Subject = "Employee Resume";

            ViewBag.Message = "Please find attached the resume.";
            List<SelectListItem> lstBillingStatus = new List<SelectListItem>();
            lstBillingStatus.Add(new SelectListItem { Value = "Billed", Text = "Billed" });
            lstBillingStatus.Add(new SelectListItem { Value = "Bench", Text = "Bench" });
            lstBillingStatus.Add(new SelectListItem { Value = "Shadow", Text = "Shadow" });
            var lstsearchskill = new List<string>();
            ViewData["StatusType"] = new SelectList(lstBillingStatus, "Value", "Text", billingStatus);
            searchskill = db.HRRFs.Where(m => m.HRRFNumber == HRRFNumber).Select(a => a.CSKILL.Trim()).FirstOrDefault();
            if (searchskill != null || searchskill.Trim() != "NULL")
            {
                lstsearchskill = searchskill.Split(',').ToList();
                lstsearchskill = lstsearchskill.Select(t => t.Trim()).ToList();
                searchskill = String.Join(",", lstsearchskill);
            }
            //ViewData["FIndex"] = "F";
            if (searchskill == null || searchskill.Trim() == "NULL")
            {
                searchskill = "";
            }
            //ViewData["searchskill"] = searchskill;
            if (EmpId != null)
            {
                emp = db.Database.SqlQuery<EmployeeInfoDetails>
        ("exec [GetEmployeeSkillsInfo] @Skillsetlist,@BillingStatus", new SqlParameter("Skillsetlist", ""),
        new SqlParameter("BillingStatus", billingStatus)).ToList<EmployeeInfoDetails>();
                emp = emp.Where(m => m.EmployeeId == EmpId).ToList();
            }
            else
            {
                emp = db.Database.SqlQuery<EmployeeInfoDetails>
        ("exec [GetEmployeeSkillsInfo] @Skillsetlist,@BillingStatus", new SqlParameter("Skillsetlist", searchskill),
        new SqlParameter("BillingStatus", billingStatus)).ToList<EmployeeInfoDetails>();
            }

            emp = emp.OrderBy(m => m.BillingStatus).ToList();
            emp = emp.OrderBy(m => m.Percentage).ToList();
            List<EmployeeInfoDetails> lstcount = emp;
            var rcount = lstcount.Where(e => e.ResumeFlag == 1).Count();
            ViewBag.Resumecount = rcount;
            var Resumenotcount = lstcount.Where(e => e.ResumeFlag == 0).Count();
            ViewBag.ResumeNotupdatedcount = Resumenotcount;
            var SkillsnotupdatedCount = db.EmployeeSkills_New.Where(s => s.SkillStatus == "Draft").Count();
            var SkillsSubmittedCount = db.EmployeeSkills_New.Where(s => s.SkillStatus == "Submitted" || s.SkillStatus == "Approved").Count();
            ViewBag.SkillsnotupdatedCount = SkillsnotupdatedCount;
            var HrrfNumberCount = db.HRRFs.Where(s => s.HRRFNumber == HRRFNumber).Select(a => a.HRRFNumber).FirstOrDefault();
            var SkillsCount = db.HRRFs.Where(s => s.HRRFNumber == HRRFNumber).Select(a => a.CSKILL).FirstOrDefault();
            var StatusCount = db.HRRFs.Where(s => s.HRRFNumber == HRRFNumber).Select(a => a.RequestStatus).FirstOrDefault();
            var CriticalityCount = db.HRRFs.Where(s => s.HRRFNumber == HRRFNumber).Select(a => a.Criticality).FirstOrDefault();
            var StartDateCount = db.HRRFs.Where(s => s.HRRFNumber == HRRFNumber).Select(a => a.AssignmentStartDate).FirstOrDefault();
            var EndDateCount = db.HRRFs.Where(s => s.HRRFNumber == HRRFNumber).Select(a => a.AssignmentEndDate).FirstOrDefault();
            var GradeCount = db.HRRFs.Where(s => s.HRRFNumber == HRRFNumber).Select(a => a.Grade).FirstOrDefault();
            ViewBag.HrrfNumberCount = HrrfNumberCount;
            ViewBag.SkillsCount = SkillsCount;
            ViewBag.StatusCount = StatusCount;
            ViewBag.CriticalityCount = CriticalityCount;
            ViewBag.StartDateCount = StartDateCount == null ? null : StartDateCount.Value.ToString("dd-MMM-yyyy");
            ViewBag.EndDateCount = EndDateCount == null ? null : EndDateCount.Value.ToString("dd-MMM-yyyy");
            ViewBag.GradeCount = GradeCount;
            if (EmpId > 0)
            {
                emp = emp.Where(e => e.EmployeeId == EmpId).ToList();
            }
            int? grdaddcount = int.Parse(GradeCount.ToString()) + 1;
            int? grdsubcount = int.Parse(GradeCount.ToString()) - 1;
            List<string> grdlist = new List<string>();
            grdlist.Add(GradeCount.ToString());
            grdlist.Add(grdsubcount.ToString());
            grdlist.Add(grdaddcount.ToString());
            emp.ForEach(m =>
            {
                m.Grade = db.Employees.Where(a => a.EmployeeId == m.EmployeeId).Select(a => a.Grade.ToString()).FirstOrDefault();
                //m.Grade = grdlist.Contains(m.Grade.ToString()) ? m.Grade : m.Grade;        
            });
            ValidationModel obj = new ValidationModel();
            if (EmpId != null)
            {

                ViewBag.empid = EmpId;
                obj.EmpSkillInfo = emp;
            }
            else
            {              
                emp = emp.Where(a => grdlist.Contains(a.Grade.ToString())).ToList();

                //emp = emp.Where(m => m.StartDate <= StartDateCount && m.EndDate >= EndDateCount).ToList();
                //emp = emp.Where(m => m.StartDate <= m.EndDate).ToList();
                ViewBag.empid = EmpId;
                obj.EmpSkillInfo = emp;
                var skillcount1 = SkillsCount.Split(',').Select(a => a.ToLower().Trim() == "mvc" ? "asp.net mvc" : a.ToLower().Trim()).ToList();
                obj.EmpSkillInfo.ForEach(m =>
                {
                    var skillcount2 = m.SkillName.Split(',').Select(a => a.ToLower().Trim()).ToList();
                    int count = skillcount2.Intersect(skillcount1).Count();
                    m.Percentage = ((decimal)count / skillcount1.Count()) * 100;
                });
                var Percentagevalue = Environment.GetEnvironmentVariable("Percentage") ?? "";
                obj.EmpSkillInfo = obj.EmpSkillInfo.Where(m => (int)m.Percentage >= Convert.ToInt32(Percentagevalue)).ToList();
                
                // obj.EmpSkillInfo = obj.EmpSkillInfo.OrderBy(m => m.BillingStatus).ThenByDescending(m => m.Percentage).ToList();
                obj.EmpSkillInfo = obj.EmpSkillInfo.OrderBy(m => String.IsNullOrEmpty(m.BillingStatus)).ThenBy(m => m.BillingStatus).ThenByDescending(m => m.Percentage).ToList();                
                //how to order by based on value in linq for grade
                obj.EmpSkillInfo = obj.EmpSkillInfo.OrderBy(d =>
                {
                    var index = grdlist.IndexOf(d.Grade);
                    return index == -1 ? int.MaxValue : index;
                }).ToList();                             
            }      
            return View("Index", obj);
        }
        public ActionResult GetEmployeeDetailsByID(string EmpID)
        {
            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            {
                try
                {
                    Int32 EmployeeId = Convert.ToInt32(EmpID);

                    List<Employee> lstemp = new List<Employee>();
                    lstemp = GetEmployee(EmployeeId);

                    if (lstemp[0].EmployeeSkills.Count> 0)
                    {
                        lstemp[0].EmployeeSkills = null;
                    }
                       
                    lstemp[0].FirstName = lstemp[0].FirstName + " " + lstemp[0].LastName;

                    ValidationModel obj = new ValidationModel();
                    obj.Empsearch = lstemp;
                    obj.ProjectAssignmentinfo = GetEmployeeAssignmentDetails(EmployeeId);
                    obj.EmployeeSkillInfo = GetEmployeeSkill(EmployeeId.ToString()).ToList();
                    obj.Traininginfo = GetEmployeeTrainingDetails().Where(emp => emp.EmployeeID == EmployeeId).ToList();
                  return Json(obj, JsonRequestBehavior.AllowGet);
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
        private List<Employee> GetEmployee(int EmployeeId)
        {
            List<Employee> objemp = new List<Employee>();
            objemp = db.Employees.Where(e => e.EmployeeId == EmployeeId).ToList();
            return objemp;
        }
        private IList<ProjectAssignment> GetEmployeeAssignmentDetails(int employeeId)
        {
            IList<ProjectAssignment> ProjectAssignments = new List<ProjectAssignment>();
            ProjectAssignments = db.ProjectAssignments.Where(p => p.EmployeeId == employeeId).OrderByDescending(s=>s.StartDate).OrderByDescending(e=>e.EndDate).ToList();
      
            return ProjectAssignments;
        }

        private List<EmployeeSkills_NewDetails> GetEmployeeSkill(string EmpID)
        {
            Int32 EmployeeId = Convert.ToInt32(EmpID);
            List<EmployeeSkills_NewDetails> empskilllst = new List<EmployeeSkills_NewDetails>();
            var employeeId = new SqlParameter("@EmployeeId", EmployeeId);
            empskilllst = db.Database.SqlQuery<EmployeeSkills_NewDetails>("exec GetEmployeeSkills @EmployeeId", employeeId).ToList();
            empskilllst = empskilllst.Where(s => s.SkillStatus == "Approved").ToList();
            return empskilllst;
        }
        private List<Training> GetEmployeeTrainingDetails()
        {
            List<Training> objStudent = new List<Training>();

            var _objuserdetail = (from data in db.Trainings
                                  select data);
            foreach (var item in _objuserdetail)
            {
                objStudent.Add(new Training { ProgramName = item.ProgramName, StartDate = item.StartDate, EndDate = item.EndDate, AttendanceStatus = item.AttendanceStatus, TotalHours = item.TotalHours, Source = item.Source, Category = item.Category, Mode = item.Mode });
             }
            return objStudent;
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult GetEmailst(string email)
        {
            try
            {
                var result = new List<KeyValuePair<string, string>>();
                DateTime dt = DateTime.Now;
                IList<SelectListItem> List = new List<SelectListItem>();

                var Empdetails = (from pro in db.Employees
                                  select new SelectListItem
                                  {
                                      Text = pro.Email,
                                      Value = pro.Email.ToString()
                                  }).Distinct().ToList();

                foreach (var item in Empdetails)
                {
                    result.Add(new KeyValuePair<string, string>(item.Value, item.Text));
                }
                var result3 = result.Where(s => s.Key.ToLower().Contains
                              (email.ToLower())).Select(w => w).ToList();
                return Json(result3, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                Common.WriteExceptionErrorLog(ex);
                return Json("Error", JsonRequestBehavior.AllowGet);

            }
        }
        public ActionResult EmailSend(int EmpId, string subject, string message, string emailid)
        {
            //string  emails= emailid;
            string Searchskill = "", ddlStatus = "";

            if (Searchskill == null)
            {
                Searchskill = "";
            }
            string SmtpuserCommon = Environment.GetEnvironmentVariable("EmailFrom") ?? "";
            string billingStatus;
            if (ddlStatus == null || ddlStatus == "" || ddlStatus == "Select Status")
            {
                billingStatus = "All";
            }
            else
            {
                billingStatus = ddlStatus;
            }
            List<EmployeeInfoDetails> emp = new List<EmployeeInfoDetails>();
            emp = db.Database.SqlQuery<EmployeeInfoDetails>("exec [GetEmployeeSkillsInfo] @Skillsetlist,@BillingStatus", new SqlParameter("Skillsetlist", Searchskill),
                   new SqlParameter("BillingStatus", billingStatus)).ToList<EmployeeInfoDetails>();
            emp = emp.Where(e => e.EmployeeId == EmpId).ToList();
            if (emp != null)
            {
                string MailBodyforAuditee = GetTableBodyForEmaployeeMail(message);
                SendMailToEmployee(emailid, SmtpuserCommon, MailBodyforAuditee, EmpId, emp[0].EmployeeName, subject);
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
                // return RedirectToAction("Index", "EmployeeSearch");
            }
            else
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }

        }
        public string GetTableBodyForEmaployeeMail(string message)
        {
            try
            {
                int empId = Convert.ToInt32(Session["EmployeeId"]);
                StringBuilder sb = new StringBuilder();
                sb.Append(message);
                sb.Append("<br/>");
                sb.Append("<br/>");
                return sb.ToString();
            }
            catch (Exception ex)
            {
                Common.WriteExceptionErrorLog(ex);
                return ex.Message;
            }
        }
        public bool SendMailToEmployee(string toMail, string fromMail, string message, int empId, string empname, string subject)
        {
            try
            {
                #region "Mail"

                string portNo = Environment.GetEnvironmentVariable("PortNo") ?? "";
                string host = Environment.GetEnvironmentVariable("HostNo") ?? "";
                string Smtpuser = Environment.GetEnvironmentVariable("awssmtpUser") ?? "";
                string Smtppassword = Environment.GetEnvironmentVariable("awssmtpPass") ?? "";


                MailMessage mm = new MailMessage();
                mm.Subject = subject;
                mm.From = new MailAddress(fromMail);


                if (toMail.Contains(','))
                {
                    var str = String.Join(",", toMail);
                    string[] TomailId = str.Split(',');
                    foreach (string email in TomailId)
                    {
                        if (email == " ")
                        {
                        }
                        else
                        {
                            mm.To.Add(new MailAddress(email));
                            EmailAudit objemail = new EmailAudit();
                            var Emp = (from ed in db.Employees
                                       where ed.Email.Equals(email.Trim())
                                       select new
                                       {
                                           EmployeeId = ed.EmployeeId
                                       }).FirstOrDefault();

                            if (Emp != null)
                            {
                                objemail.MailSubject = subject;
                                objemail.ToMailEmpId = Emp.EmployeeId;
                                objemail.RresourceEmpID = empId;
                                objemail.Mailbody = message;
                                objemail.CreatedDate = DateTime.Now;
                                objemail.MailSent = "Y";
                                db.EmailAudits.Add(objemail);
                                db.SaveChanges();
                            }

                        }
                    }

                }
                else
                {
                    mm.To.Add(new MailAddress(toMail));
                    EmailAudit objemail = new EmailAudit();
                    var Emp = (from ed in db.Employees
                               where ed.Email.Equals(toMail)
                               select new
                               {
                                   EmployeeId = ed.EmployeeId
                               }).FirstOrDefault();

                    if (Emp != null)
                    {
                        objemail.MailSubject = subject;
                        objemail.ToMailEmpId = Emp.EmployeeId;
                        objemail.RresourceEmpID = empId;
                        objemail.Mailbody = message;
                        objemail.CreatedDate = DateTime.Now;
                        objemail.MailSent = "Y";
                        db.EmailAudits.Add(objemail);
                        db.SaveChanges();
                    }

                }

                byte[] fileData;
                string fileName, x;
                var name = from d in db.Employees where d.EmployeeId == empId select d.FirstName;
                var objEmpDOC = (from ed in db.Employees
                                 where ed.EmployeeId.Equals(empId)
                                 select new
                                 {
                                     EmpCV = ed.Resumes
                                 }).FirstOrDefault();
                fileData = (byte[])objEmpDOC.EmpCV.ToArray();
                fileName = name.First();
                x = fileName.ToString();

                Stream stream = new MemoryStream(fileData);
                Attachment attachment = new Attachment(stream, "" + "Resume" + "_" + empname + "" + ".docx");
                attachment.ContentType = new ContentType("application/msword");
                mm.Attachments.Add(attachment);
                AlternateView av1 = AlternateView.CreateAlternateViewFromString(message, null, MediaTypeNames.Text.Html);
                mm.AlternateViews.Add(av1);
                mm.IsBodyHtml = true;



                SmtpClient smtp = new SmtpClient();
                smtp.Host = host;
                smtp.Port = Convert.ToInt32(portNo);
                smtp.EnableSsl = true;
                smtp.Credentials = new NetworkCredential(Smtpuser, Smtppassword);
                smtp.Send(mm);
                #endregion

                return true;
            }
            catch (Exception ex)
            {
                Common.WriteExceptionErrorLog(ex);
                return false;
            }
        }
        public ActionResult FileDownload(int EmpId)
        {
            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            {
                byte[] fileData;
                string fileName, x;
                var name = from d in db.Employees where d.EmployeeId == EmpId select d.FirstName;
                var objEmpDOC = (from ed in db.Employees
                                 where ed.EmployeeId.Equals(EmpId)
                                 select new
                                 {
                                     EmpCV = ed.Resumes
                                 }).FirstOrDefault();
                fileData = (byte[])objEmpDOC.EmpCV.ToArray();
                fileName = name.First();
                x = fileName.ToString();
                return File(fileData, ".docx", x + "_Resume.docx");
            }
            else
            {
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
                    return Json("Error", JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json("Sessionexpired", JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult ExportEmployeeDetails(string currentFilter, int? page, string ddlStatus, string Searchskill, string ddlAccountName)
        {
            try
            {
                int? EmpId = Convert.ToInt32(Session["EmployeeId"]);
                string billingStatus;
                if (ddlStatus == null || ddlStatus == "")
                {
                    billingStatus = "All";
                }
                else
                {
                    billingStatus = ddlStatus;
                }
                string accountName;
                if (ddlAccountName == null || ddlAccountName == "" || ddlAccountName == "Select AccountName")
                {
                    accountName = "";
                }
                else
                {
                    accountName = ddlAccountName;
                }
                List<EmployeeInfoDetails> lstemp = new List<EmployeeInfoDetails>();
                if (Searchskill != null)
                {
                    page = 1;
                }
                ViewBag.CurrentFilter = Searchskill;

                List<SelectListItem> lstBillingStatus = new List<SelectListItem>();
                lstBillingStatus.Add(new SelectListItem { Value = "All", Text = "All" });
                lstBillingStatus.Add(new SelectListItem { Value = "Billed", Text = "Billed" });
                lstBillingStatus.Add(new SelectListItem { Value = "Bench", Text = "Bench" });
                lstBillingStatus.Add(new SelectListItem { Value = "Shadow", Text = "Shadow" });
                ViewData["StatusType"] = new SelectList(lstBillingStatus, "Value", "Text", billingStatus);

                if (Searchskill == null)
                {
                    Searchskill = "";
                }
                List<EmployeeInfoDetails> emp = new List<EmployeeInfoDetails>();
                emp = db.Database.SqlQuery<EmployeeInfoDetails>
                      ("exec [GetEmployeeSkillsInfo_AccountName] @Skillsetlist,@BillingStatus,@AccountName", new SqlParameter("Skillsetlist", Searchskill),
                      new SqlParameter("BillingStatus", billingStatus), new SqlParameter("AccountName", accountName)).ToList<EmployeeInfoDetails>();
                List<EmployeeInfoDetails> lstcount = emp;
              
                ValidationModel obj = new ValidationModel();
                obj.EmpSkillInfo = emp;
                var skillcount1 = new List<string>();
                if (Searchskill.Trim().Contains(","))
                {
                    skillcount1 = Searchskill.Split(',').Select(a => a.ToLower().Trim()).ToList();
                }
                else
                {
                    var strskill = Searchskill.ToLower().Trim();
                    skillcount1.Add(strskill.ToLower().Trim());

                }
                skillcount1 = skillcount1.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                foreach (var item in obj.EmpSkillInfo)
                {
                    var skillcount2 = item.SkillName.Split(',').Select(a => a.ToLower().Trim()).ToList();
                    int count = skillcount2.Intersect(skillcount1).Count();
                    if (count != 0)
                    {
                        item.Percentage = ((decimal)count / skillcount1.Count()) * 100;

                    }
                }
                List<EmployeeInfoDetails> empinfodetails = new List<EmployeeInfoDetails>();
                empinfodetails = obj.EmpSkillInfo;
               
                lstemp = db.Database.SqlQuery<EmployeeInfoDetails>
               ("exec GetEmployeeSkillsbound") .ToList<EmployeeInfoDetails>();
                
                List<EmployeeInfoDetails> lstempskillnot = new List<EmployeeInfoDetails>();
                lstempskillnot = db.Database.SqlQuery<EmployeeInfoDetails>
               ("exec GetEmployeeSkillsNotbound").ToList<EmployeeInfoDetails>();

                int LastRow;
                #region Export to Excel
                using (ExcelPackage package = new ExcelPackage())
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Skills Updated List");
                    ExcelWorksheet worksheetone = package.Workbook.Worksheets.Add("Skills Not Updated List");
                    ExcelWorksheet worksheettwo = package.Workbook.Worksheets.Add("Employee Details List");

                    worksheet.TabColor = System.Drawing.Color.Green;
                    worksheet.DefaultRowHeight = 20f;
                    worksheet.Row(1).Height = 20f;
                    LastRow = 3;
                    using (var range = worksheet.Cells[1, 1, 1, LastRow])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                        range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#5B9BD5"));
                        range.Style.Font.Color.SetColor(System.Drawing.Color.White);
                        range.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        range.Style.Border.Right.Color.SetColor(System.Drawing.Color.LightGray);
                    }
                    worksheet.Cells[1, 1].Value = "Employee Id";
                    worksheet.Cells[1, 2].Value = "Employee Name";                    
                    worksheet.Cells[1, 3].Value = "Resume Upload";
                    //Set default column width
                    worksheet.DefaultColWidth = 30f;
                    worksheet.Column(1).Width = 18f;
                    worksheet.Column(2).Width = 32f;
                    worksheet.Column(3).Width = 14f;                 
                    for (int rowIndex = 0, row = 2; rowIndex < lstemp.Count; rowIndex++, row++) // row indicates number of rows
                    {

                        worksheet.Cells[row, 1].Value = lstemp[rowIndex].EmployeeId;
                        worksheet.Cells[row, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;

                        worksheet.Cells[row, 2].Value = lstemp[rowIndex].EmployeeName;
                        worksheet.Cells[row, 2].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;

                        if (lstemp[rowIndex].ResumeFlag == 1)
                        {
                            worksheet.Cells[row, 3].Value = "Yes";

                        }
                        else
                        {
                            worksheet.Cells[row, 3].Value = "No";

                        }
                        worksheet.Cells[row, 3].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;

                        if (row % 2 == 1)
                        {
                            using (var range = worksheet.Cells[row, 1, row, LastRow])
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

                    //skill not bound Employee
                    worksheetone.TabColor = System.Drawing.Color.Green;
                    worksheetone.DefaultRowHeight = 20f;
                    worksheetone.Row(1).Height = 20f;
                    LastRow = 3;
                    using (var range = worksheetone.Cells[1, 1, 1, LastRow])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                        range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#5B9BD5"));
                        range.Style.Font.Color.SetColor(System.Drawing.Color.White);
                        range.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        range.Style.Border.Right.Color.SetColor(System.Drawing.Color.LightGray);
                    }
                    worksheetone.Cells[1, 1].Value = "Employee Id";
                    worksheetone.Cells[1, 2].Value = "Employee Name";
                    worksheetone.Cells[1, 3].Value = "Resume Upload";

                    //Set default column width
                    worksheetone.DefaultColWidth = 13f;
                    worksheetone.Column(1).Width = 18f;
                    worksheetone.Column(2).Width = 32f;
                    worksheetone.Column(3).Width = 14f;
                 
                    //Add the each row for employee with skills
                    for (int rowIndex = 0, row = 2; rowIndex < lstempskillnot.Count; rowIndex++, row++) // row indicates number of rows
                    {

                        worksheetone.Cells[row, 1].Value = lstempskillnot[rowIndex].EmployeeId;
                        worksheetone.Cells[row, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;

                        worksheetone.Cells[row, 2].Value = lstempskillnot[rowIndex].EmployeeName;
                        worksheetone.Cells[row, 2].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;

                        if (lstempskillnot[rowIndex].ResumeFlag == 1)
                        {
                            worksheetone.Cells[row, 3].Value = "Yes";

                        }
                        else
                        {
                            worksheetone.Cells[row, 3].Value = "No";

                        }
                        worksheetone.Cells[row, 3].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;

                        if (row % 2 == 1)
                        {
                            using (var range = worksheetone.Cells[row, 1, row, LastRow])
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

                    //Latest code of Mounika
                    worksheettwo.TabColor = System.Drawing.Color.Green;
                    worksheettwo.DefaultRowHeight = 20f;
                    worksheettwo.Row(1).Height = 20f;
                    LastRow = 11;
                    using (var range = worksheettwo.Cells[1, 1, 1, LastRow])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                        range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#5B9BD5"));
                        range.Style.Font.Color.SetColor(System.Drawing.Color.White);
                        range.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        range.Style.Border.Right.Color.SetColor(System.Drawing.Color.LightGray);
                    }
                    worksheettwo.Cells[1, 1].Value = "Employee Id";
                    worksheettwo.Cells[1, 2].Value = "Employee Name";
                    worksheettwo.Cells[1, 3].Value = "Project Code";
                    worksheettwo.Cells[1, 4].Value = "Project Name";
                    worksheettwo.Cells[1, 5].Value = "Account Name";
                    worksheettwo.Cells[1, 6].Value = "Utilization";
                    worksheettwo.Cells[1, 7].Value = "Grade";
                    worksheettwo.Cells[1, 8].Value = "Billing Status";
                    worksheettwo.Cells[1, 9].Value = "Start Date";
                    worksheettwo.Cells[1, 10].Value = "End Date";
                    worksheettwo.Cells[1, 11].Value = "Skill Name";
                  
                    //Set default column width
                    worksheettwo.DefaultColWidth = 13f;
                    worksheettwo.Column(1).Width = 14f;
                    worksheettwo.Column(2).Width = 32f;
                    worksheettwo.Column(3).Width = 14f;
                    worksheettwo.Column(4).Width = 38f;
                    worksheettwo.Column(5).Width = 38f;
                    worksheettwo.Column(6).Width = 10f;
                    worksheettwo.Column(7).Width = 9f;
                    worksheettwo.Column(8).Width = 12f;
                    worksheettwo.Column(9).Width = 16f;
                    worksheettwo.Column(10).Width = 16f;
                    worksheettwo.Column(11).Width = 64f;
                   
                    //Add the each row for employee with skills
                    for (int rowIndex = 0, row = 2; rowIndex < empinfodetails.Count; rowIndex++, row++) // row indicates number of rows
                    {

                        worksheettwo.Cells[row, 1].Value = empinfodetails[rowIndex].EmployeeId;
                        worksheettwo.Cells[row, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;

                        worksheettwo.Cells[row, 2].Value = empinfodetails[rowIndex].EmployeeName;
                        worksheettwo.Cells[row, 2].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;

                        worksheettwo.Cells[row, 3].Value = empinfodetails[rowIndex].ProjectCode;                       
                        worksheettwo.Cells[row, 3].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;

                        worksheettwo.Cells[row,4].Value = empinfodetails[rowIndex].ProjectName;
                        worksheettwo.Cells[row, 4].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;

                        worksheettwo.Cells[row, 5].Value = empinfodetails[rowIndex].AccountName;
                        worksheettwo.Cells[row, 5].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;

                        worksheettwo.Cells[row, 6].Value = empinfodetails[rowIndex].Utilization;
                        worksheettwo.Cells[row, 6].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                        worksheettwo.Cells[row, 7].Value = empinfodetails[rowIndex].Grade;
                        worksheettwo.Cells[row, 7].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                        worksheettwo.Cells[row,8].Value = empinfodetails[rowIndex].BillingStatus;
                        worksheettwo.Cells[row, 8].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                        
                        worksheettwo.Cells[row, 9].Value = empinfodetails[rowIndex].StartDate;
                        worksheettwo.Cells[row, 9].Style.Numberformat.Format = "dd-MMM-yyyy";
                        worksheettwo.Cells[row, 9].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                        worksheettwo.Cells[row, 10].Value = empinfodetails[rowIndex].EndDate;
                        worksheettwo.Cells[row, 10].Style.Numberformat.Format = "dd-MMM-yyyy";
                        worksheettwo.Cells[row, 10].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                        worksheettwo.Cells[row, 11].Value = empinfodetails[rowIndex].SkillName;
                        worksheettwo.Cells[row, 11].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                        
                        if (row % 2 == 1)
                        {
                            using (var range = worksheettwo.Cells[row, 1, row, LastRow])
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
                    //Latest Code of Mounika

                    Byte[] fileBytes = package.GetAsByteArray();
                    Response.Clear();
                    Response.Buffer = true;
                    Response.AddHeader("content-disposition", "attachment;filename=" + "EmployeeSearch" + "-Report" + DateTime.Now.ToString("_dd-MMM-yyyy_hh-mm-ss") + ".xlsx");

                    Response.Charset = "";
                    Response.ContentType = "application/vnd.ms-excel";
                    StringWriter sw = new StringWriter();
                    Response.BinaryWrite(fileBytes);
                    Response.End();
                }

                #endregion

                return new EmptyResult();
            }
            catch (Exception ex)
            {
                Common.WriteExceptionErrorLog(ex);
                //return Json("Error", JsonRequestBehavior.AllowGet);
                return RedirectToAction("Error", "Error");
            }
        }

        //BenchResources
    }
}





