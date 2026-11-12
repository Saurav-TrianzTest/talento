using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Trianz.Enterprise.Operations.General;
using Trianz.Enterprise.Operations.Models;

namespace Trianz.Enterprise.Operations.Controllers
{
    public class SkillsApprovalController : Controller
    {
        private TrianzOperationsEntities db = new TrianzOperationsEntities();
        // GET: SkillsApproval
        public ActionResult GetAllReportees()
        {
            //string loginUserName = User.Identity.Name;
            //string[] Names = loginUserName.Split(new[] { "\\" }, StringSplitOptions.None);
            //string name = Names[1].ToLower();
            string usermail = Common.GetAzureLoggedInUserID();
            if (usermail != null)
            {

                int EmployeeId = (from data in db.Employees where (data.Email.Equals(usermail) && data.IsActive == true) select data.EmployeeId).FirstOrDefault();
                Employee employee = (from data in db.Employees.Where(x => x.EmployeeId == EmployeeId) select data).FirstOrDefault();
                List<Employee> _ReporteesList = new List<Employee>();
                _ReporteesList = db.Employees.Where(e => e.SupervisorId == employee.EmployeeId).ToList();
                //_ReporteesList = _ReporteesList.Where(s => s.EmployeeNewSkills.Count > 0 && s.EmployeeNewSkills.Where(c => c.SkillStatus.ToLower() == "Submitted").Count() > 0).ToList();
                // GetEmployeeDetails(_ReporteesList.First().EmployeeId);
                return View(_ReporteesList);
            }

            return View();
        }

        public ActionResult GetEmployeeDetails(int EmployeeID)

        {
            Employee emp = db.Employees.Where(e => e.EmployeeId == EmployeeID).FirstOrDefault();
            if (emp != null)
            {
                string NotYetApprove = "Submitted";//ConfigurationManager.AppSettings["Not Yet Approved"].ToString();
                string SkillApprove = "Approved";

                //List<EmployeeSkills_NewDetails> empSkils = (from data in db.EmployeeSkills_New
                //                                            join Ski in db.SkillMasters on data.SkillId equals Ski.SkillId
                //                                            join emp1 in db.Employees on data.EmployeeId equals emp1.EmployeeId
                //                                            where data.EmployeeId == emp.EmployeeId && data.SkillStatus == NotYetApprove
                //                                            select new EmployeeSkills_NewDetails
                //                                            {
                //                                                EmployeeSkillId = data.EmployeeSkillId,
                //                                                EmployeeId = data.EmployeeId,
                //                                                Skillname = Ski.Skillset,
                //                                                CompetincyName = Ski.SkillCategory,
                //                                                SkillId = Ski.SkillId,
                //                                                ApproverName = emp1.FirstName + "" + emp1.MiddleName + "" + emp1.LastName,
                //                                                ApproverId = 0,//emp.EmployeeId,
                //                                                Expertiselevel = data.Expertiselevel,
                //                                                TechnologyLastUsed = data.TechnologyLastUsed,
                //                                                TechnologyLastUsedmonth = "",
                //                                                TechnologyLastUsedyear = "",
                //                                                SkillStatus = data.SkillStatus,
                //                                                ApprovalDate = data.ApprovalDate,
                //                                                LastModifiedDate = data.LastModifiedDate

                //                                            }).ToList();
                int Loginuser = 0;
                //string usermail = Common.GetAzureLoggedInUserID();
                //var EEmployee = (from emp1 in db.Employees
                //                  where emp1.Email == usermail && emp1.IsActive == true
                //                  select new
                //                  {
                //                      EmployeeId = emp1.EmployeeId
                //                  }).FirstOrDefault();
                //if (Employee != null)
                //{
                //    Loginuser = Convert.ToInt32(Employee.EmployeeId);

                //}

                var Emplst = db.Employees.Where(e => e.EmployeeId == EmployeeID && e.IsActive == true).FirstOrDefault();
                int loginId = 0;
                if (Session["loginId"] != null)
                {
                     loginId = Convert.ToInt32(Session["loginId"].ToString());
                }
                var superviserId = Emplst.SupervisorId;
                //   if(superviserId==Loginuser)
                int LeadId = 0;
                //   LeadId = Convert.ToInt32(Session["LeadId"].ToString());
                List<ClusterLead> lstCluster = new List<ClusterLead>();

                lstCluster = db.Database.SqlQuery<ClusterLead>("exec GetClusterTeamMember_test").ToList();
                var clusterlead = lstCluster.Where(m => m.TeamMemberID == EmployeeID).FirstOrDefault();
                if (clusterlead != null)
                {
                    LeadId = clusterlead.EmployeeId;
                }

                List<EmployeeSkills_NewDetails> emplyeeskills = new List<EmployeeSkills_NewDetails>();
                List<EmployeeSkills_NewDetails> empSkils = new List<EmployeeSkills_NewDetails>();
                var employeeId = new SqlParameter("@EmployeeId", EmployeeID);
                emplyeeskills = db.Database.SqlQuery<EmployeeSkills_NewDetails>("exec GetEmployeeSkills @EmployeeId", employeeId).ToList();

                if (emplyeeskills.Count > 0)
                {
                    if (loginId != superviserId && loginId == LeadId)
                    {
                        empSkils = emplyeeskills.Where(p => (p.SkillStatus == NotYetApprove || p.SkillStatus == SkillApprove) && (p.IsReviewed == null || p.IsReviewed == false)).ToList();
                    }
                    else if (loginId == superviserId && loginId != LeadId)
                    {
                       // empSkils = emplyeeskills.Where(p => (p.SkillStatus == NotYetApprove || p.SkillStatus == SkillApprove) && (p.ApproverId==0)).ToList();
                        empSkils = emplyeeskills.Where(p => (p.SkillStatus == NotYetApprove || p.SkillStatus == SkillApprove) && (p.ApproverId == 0 || p.ApproverId == null)).ToList();
                    }
                    ViewData["empskills"] = empSkils;
                    ViewBag.EmpNameData = empSkils[0].EMployeeName;
                   
                }
                ViewBag.EmpIdData = emp.EmployeeId;
            }
            //GetAllReportees();
            return View("GetAllReportees");
        }

        public ActionResult UpdateSkillApproval(IList<int> SkillIds, bool IsApprove, int EmployeeId, string Remarks = "")
        {
            try
            {
                int Loginuser = 0;

                //string UName = User.Identity.Name.Substring(7);
                string usermail = Common.GetAzureLoggedInUserID();
                var EmployeeID = (from emp1 in db.Employees
                                  where emp1.Email == usermail && emp1.IsActive == true
                                  select new
                                  {
                                      EmployeeId = emp1.EmployeeId
                                  }).FirstOrDefault();
                Loginuser = Convert.ToInt32(EmployeeID.EmployeeId);

                string status = IsApprove == true ? ConfigurationManager.AppSettings["Approved"].ToString() : ConfigurationManager.AppSettings["Rejected"].ToString();
                foreach (int id in SkillIds)
                {
                    int emskillid = Convert.ToInt32(id);
                    EmployeeSkills_New empskill = db.EmployeeSkills_New.Where(s => s.EmployeeSkillId == emskillid).FirstOrDefault();

                    EmployeeSkillHistory em = new EmployeeSkillHistory();
                    em.SkillIDOld = empskill.SkillId;
                    em.EMployeeID = empskill.EmployeeId;
                    em.ExpertiselevelOld = empskill.Expertiselevel;
                    em.TechnologyLastUsedOld = empskill.TechnologyLastUsed;
                    em.PreviousStatus = empskill.SkillStatus;
                    em.Comments = empskill.SkillStatus + " " + Remarks;
                    em.Expertiselevel = empskill.Expertiselevel;
                    em.ModifiedDate = DateTime.UtcNow;
                    em.SkillID = empskill.SkillId;
                    em.TechnologyLastUsed = empskill.TechnologyLastUsed;
                    em.UpdateStatus = status;
                    em.ApproverID = Loginuser;
                    em.ApprovalDate = DateTime.UtcNow;
                    db.EmployeeSkillHistories.Add(em);

                    empskill.SkillStatus = status;
                    empskill.Remarks = Remarks;
                    //empskill.ApprovalDate = DateTime.UtcNow;
                    //empskill.ApproverId = Loginuser;
                    var empcount = db.Employees.Where(e => e.EmployeeId == EmployeeId && e.IsActive == true).ToList();

                    List<ClusterLead> lstCluster = new List<ClusterLead>();

                    lstCluster = db.Database.SqlQuery<ClusterLead>("exec GetClusterTeamMember_test").ToList();
                    var teammember = lstCluster.Where(m => m.EmployeeId == Loginuser && m.TeamMemberID == EmployeeId).ToList();

                    if (teammember.Count > 0)
                    {
                        if (teammember[0].TeamMemberID == EmployeeId && empcount[0].SupervisorId == Loginuser)
                        {
                            empskill.IsReviewed = true;
                            empskill.ReviewerId = Loginuser;
                            empskill.LastReviewdDate = DateTime.UtcNow;
                            empskill.ApprovalDate = DateTime.UtcNow;
                            empskill.ApproverId = Loginuser;
                            //  db.EmployeeSkills_New.Add(empskill);
                        }
                        else if (teammember[0].TeamMemberID == EmployeeId && empcount[0].SupervisorId != Loginuser)
                        {
                            empskill.IsReviewed = true;
                            empskill.ReviewerId = Loginuser;
                            empskill.LastReviewdDate = DateTime.UtcNow;
                        }
                    }
                    else if (empcount[0].SupervisorId == Loginuser)
                    {
                        empskill.ApprovalDate = DateTime.UtcNow;
                        empskill.ApproverId = Loginuser;
                    }
                }



                Employee emp = db.Employees.Where(e => e.EmployeeId == EmployeeId).FirstOrDefault();
                Notification tblNotification = new Notification();
                tblNotification.NotificationType = ConfigurationManager.AppSettings["SkillApprovalResponse_NotificationType"].ToString() + status;
                tblNotification.NotificationDate = System.DateTime.UtcNow;
                tblNotification.NotificationFrom = emp.SupervisorId;
                tblNotification.NotificationTo = emp.EmployeeId;
                if (status.ToLower() == "rejected")
                {
                    status = status + ". Remarks: " + Remarks;
                }
                var Body = ConfigurationManager.AppSettings["SkillApprovalResponse_NotificationMsg"].ToString() + status;
                tblNotification.IsActive = true;
                tblNotification.AssetID = ConfigurationManager.AppSettings["Skill_AppCode"];
                tblNotification.ApplicationCode = ConfigurationManager.AppSettings["Skill_AppCode"].ToString();

                string talentoURL = ConfigurationManager.AppSettings["Talento"];
                string LinktoOpen = "Please Click on link to View the status of skills " + talentoURL;
                string body = string.Empty;

                using (StreamReader reader = new StreamReader(Common.DownloadFromS3Stream("Views/Shared/EmailTemplate.html")))
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
                db.SaveChanges();
                //Below code is commented by sarath on 19 Mar 2016 and implemented new code below.
                //GetEmployeeDetails(EmployeeId);
                //return View("GetAllReportees");

                return Json("success", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ErrorHandling expcls = new ErrorHandling();
                expcls.Error(ex);
                return null;
            }
        }



        //    public ActionResult UpdateSkillApproval(IList<int> SkillIds, bool IsApprove, int EmployeeId, string Remarks = "")
        //    {
        //        try
        //        {
        //            int Loginuser = 0;

        ////string UName = User.Identity.Name.Substring(7);
        //string usermail = Common.GetAzureLoggedInUserID();
        //var EmployeeID = (from emp1 in db.Employees
        //                              where emp1.Email == usermail && emp1.IsActive==true
        //                              select new
        //                              {
        //                                  EmployeeId = emp1.EmployeeId
        //                              }).FirstOrDefault();
        //            Loginuser = Convert.ToInt32(EmployeeID.EmployeeId);

        //            string status = IsApprove == true ? ConfigurationManager.AppSettings["Approved"].ToString() : ConfigurationManager.AppSettings["Rejected"].ToString();
        //            foreach (int id in SkillIds)
        //            {
        //                int emskillid = Convert.ToInt32(id);
        //                EmployeeSkills_New empskill = db.EmployeeSkills_New.Where(s => s.EmployeeSkillId == emskillid).FirstOrDefault();

        //                EmployeeSkillHistory em = new EmployeeSkillHistory();
        //                em.SkillIDOld = empskill.SkillId;
        //                em.EMployeeID = empskill.EmployeeId;
        //                em.ExpertiselevelOld = empskill.Expertiselevel;
        //                em.TechnologyLastUsedOld = empskill.TechnologyLastUsed;
        //                em.PreviousStatus = empskill.SkillStatus;
        //                em.Comments = empskill.SkillStatus +" "+ Remarks;
        //                em.Expertiselevel = empskill.Expertiselevel;
        //                em.ModifiedDate = DateTime.UtcNow;
        //                em.SkillID = empskill.SkillId; 
        //                em.TechnologyLastUsed = empskill.TechnologyLastUsed;
        //                em.UpdateStatus = status;
        //                em.ApproverID = Loginuser;
        //                em.ApprovalDate = DateTime.UtcNow;
        //                db.EmployeeSkillHistories.Add(em);

        //                empskill.SkillStatus = status;
        //                empskill.Remarks = Remarks;
        //                empskill.ApprovalDate = DateTime.UtcNow;
        //                empskill.ApproverId = Loginuser;
        //            }
        //            Employee emp = db.Employees.Where(e => e.EmployeeId == EmployeeId).FirstOrDefault();
        //            Notification tblNotification = new Notification();
        //            tblNotification.NotificationType = ConfigurationManager.AppSettings["SkillApprovalResponse_NotificationType"].ToString() + status;
        //            tblNotification.NotificationDate = System.DateTime.UtcNow;
        //            tblNotification.NotificationFrom = emp.SupervisorId;
        //            tblNotification.NotificationTo = emp.EmployeeId;
        //            if (status.ToLower() == "rejected")
        //            {
        //                status = status + ". Remarks: " + Remarks;
        //            }
        //            var Body = ConfigurationManager.AppSettings["SkillApprovalResponse_NotificationMsg"].ToString() + status;
        //            tblNotification.IsActive = true;
        //            tblNotification.AssetID = ConfigurationManager.AppSettings["Skill_AppCode"];
        //            tblNotification.ApplicationCode = ConfigurationManager.AppSettings["Skill_AppCode"].ToString();

        //            string talentoURL = ConfigurationManager.AppSettings["Talento"];
        //            string LinktoOpen = "Please Click on link to View the status of skills " + talentoURL;
        //            string body = string.Empty;

        //            using (StreamReader reader = new StreamReader(Common.DownloadFromS3Stream("Views/Shared/EmailTemplate.html")))
        //            {
        //                body = reader.ReadToEnd();
        //            }
        //            body = body.Replace("{ToUserName}", db.Employees.Where(e => e.EmployeeId == tblNotification.NotificationTo).Select(s => s.FirstName + " " + s.LastName).FirstOrDefault());
        //            body = body.Replace("{FromUserName}", db.Employees.Where(e => e.EmployeeId == tblNotification.NotificationFrom).Select(s => s.FirstName + " " + s.LastName).FirstOrDefault());
        //            body = body.Replace("{Description}", Body);
        //            body = body.Replace("{LinktoOpen}", LinktoOpen);

        //            tblNotification.NotificationMessage = body;


        //            string IsEmailSent = System.Configuration.ConfigurationManager.AppSettings["IsEmailSent"].ToString();
        //            bool IsEmail = Convert.ToBoolean(IsEmailSent);
        //            if (IsEmail == true)
        //            {
        //                db.Notifications.Add(tblNotification);

        //            }
        //            db.SaveChanges();
        //            //Below code is commented by sarath on 19 Mar 2016 and implemented new code below.
        //            //GetEmployeeDetails(EmployeeId);
        //            //return View("GetAllReportees");

        //            return Json("success", JsonRequestBehavior.AllowGet);
        //        }
        //        catch (Exception ex)
        //        {
        //            ErrorHandling expcls = new ErrorHandling();
        //            expcls.Error(ex);
        //            return null;
        //        }
        //    }

    }
}