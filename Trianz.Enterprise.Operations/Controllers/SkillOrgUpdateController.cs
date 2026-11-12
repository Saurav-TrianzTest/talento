using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Trianz.Enterprise.Operations.General;
using Trianz.Enterprise.Operations.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Session;


namespace Trianz.Enterprise.Operations.Controllers
{
    public class SkillOrgUpdateController : Controller
    {

        private TrianzOperationsEntities db = new TrianzOperationsEntities();
        // GET: SkillOrgUpdate
        public ActionResult SkillOrgUpdate()
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                List<SelectListItem> lsCriticality = new List<SelectListItem>();              
                lsCriticality.Add(new SelectListItem { Value = "P0", Text = "P0" });
                lsCriticality.Add(new SelectListItem { Value = "P1", Text = "P1" });
                lsCriticality.Add(new SelectListItem { Value = "P2", Text = "P2" });
                lsCriticality.Add(new SelectListItem { Value = "P3", Text = "P3" });
                ViewData["Criticality"] = lsCriticality;
                
                List<string> lstRLEmp = db.RoleMasters.Where(r => r.Role == "RL" && r.ApplicationCode == "TALENTREQ").Select(r => r.EmployeeId.ToString()).ToList();
                List<string> lstRLActiveEmp = db.Employees
                                                .Where(e => lstRLEmp.Contains(e.EmployeeId.ToString()) && e.IsActive == true)
                                                .Select(emp => emp.EmployeeId.ToString()).ToList();

                List<SelectListItem> lstRLEmployees = (from dt in db.Employees.
                                    Where(n => lstRLActiveEmp.Contains(n.EmployeeId.ToString()) && n.IsActive == true)
                                                       select new SelectListItem
                                                       {
                                                           Text = dt.FirstName + " " + dt.LastName,
                                                           Value = dt.EmployeeId.ToString()
                                                       }
                                   ).ToList();
                ViewData["_RecruiterName"] = lstRLEmployees.OrderBy(ord => ord.Text).ToList();
                return View();
            }
            else
            {
                return RedirectToAction("SessionExpire", "Signout");
            }
        }
        [HttpGet]
        public ActionResult SkillOrgUpdatedetails(string TRNumber)
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                try
                {
                    HRRF hrrfdetails = db.HRRFs.Where(i => i.HRRFNumber == TRNumber).FirstOrDefault();


                    hrrfdetails.ResourceName = db.Employees.Where(b => b.EmployeeId == hrrfdetails.HRRFCreatedBy).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault();
                    // hrrfsomedate = new HRRF();
                    var hrrfdate = hrrfdetails.HRRFSubmitedDate == null ? "" : hrrfdetails.HRRFSubmitedDate.Value.ToString("dd-MM-yyyy");
                    var skill = hrrfdetails.CSKILL == null ? "" : hrrfdetails.CSKILL;

                    var Ageing = db.Set<HRRFExportToExcel>().FromSqlRaw
                      ("exec GetAgeing @HRRFNumber", new SqlParameter("@HRRFNumber", TRNumber)).First<HRRFExportToExcel>();

                    string recruiter = hrrfdetails.RECRTR == null ? "" : hrrfdetails.RECRTR;

                    string lead = hrrfdetails.HRNGMGRID == null ? "" : hrrfdetails.HRNGMGRID;

                   

                    var result = new
                    {
                        hrrfdetails.HRRFNumber,
                        hrrfdetails.Purpose,
                        hrrfdetails.Practice,
                        hrrfdetails.OrganizationGroup,
                        hrrfdetails.SkillCode,
                        hrrfdetails.HRRFCreatedBy,
                        hrrfdetails.ResourceName,
                        hrrfdate,
                        hrrfdetails.SkillCode_WIP,
                        hrrfdetails.Organization_SubGroup,
                        skill,
                        Ageing = Ageing.Ageing,
                        hrrfdetails.Criticality,
                        recruiter,
                        lead

                    };

                    return Json(result);
                }
                catch (Exception ex)
                {
                    Common.WriteExceptionErrorLog(ex);
                    return RedirectToAction("Error", "Error");
                    //return Json("Sessionexpired");
                }
            }
            else
            {
                // return RedirectToAction("SessionExpire", "Signout");
                return Json("Sessionexpired");
            }
        }
        [HttpPost]
        public ActionResult SkillOrgUpdateData(HRRF hrrfdata)
        {

            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                try
                {
                    var hrrfdetails = db.HRRFs.Where(i => i.HRRFNumber == hrrfdata.HRRFNumber).FirstOrDefault();
                    HRRFHistory hrrfdatahistory = new HRRFHistory();

                    var prevRectr = "";
                    var prevTALead = "";
                    var prevCrit = "";

                    int recruiterid=0, leadid=0;
                  //  string recruitername="", leadname="";

                    var prevRectrid = "";
                    var prevTALeadid = "";

                    if (hrrfdetails != null)
                    {
                        prevCrit = hrrfdetails.Criticality;
                        prevTALeadid = hrrfdetails.HRNGMGRID == null ? "" : hrrfdetails.HRNGMGRID;
                        prevRectrid = hrrfdetails.RECRTR == null ? "" : hrrfdetails.RECRTR;

                        recruiterid = Convert.ToInt32(hrrfdetails.RECRTR);
                        leadid = Convert.ToInt32(hrrfdetails.HRNGMGRID);

                        prevRectr = db.Employees.Where(b => b.EmployeeId == recruiterid).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault();

                        prevTALead = db.Employees.Where(b => b.EmployeeId == leadid).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault();

                        //HRRF hrrfdet = new HRRF();
                        hrrfdetails.Organization_SubGroup = hrrfdata.Organization_SubGroup;
                        hrrfdetails.SkillCode_WIP = hrrfdata.SkillCode_WIP;
                        hrrfdetails.Criticality = hrrfdata.Criticality;
                        hrrfdetails.HRNGMGRID = hrrfdata.HRNGMGRID;
                        hrrfdetails.RECRTR = hrrfdata.RECRTR;
                        db.Entry(hrrfdetails).State = EntityState.Modified;
                        db.SaveChanges();
                        //if (prevCrit != hrrfdata.Criticality)

                        //{
                        //    hrrfdatahistory.HRRFNumber = hrrfdata.HRRFNumber;
                        //    hrrfdatahistory.HistoryDescription = hrrfdata.HRRFNumber + " - Criticality changed from " + prevCrit + " to " + hrrfdata.Criticality;
                        //    hrrfdatahistory.ModifiedBy = Convert.ToInt32(HttpContext.Session.GetString("EmployeeId"));
                        //    hrrfdatahistory.ModifiedDate = DateTime.Now;
                        //    hrrfdatahistory.Remarks = "Update CRIT TALEAD RECTR";
                        //    hrrfdatahistory.PrevRequestStatus = prevCrit;
                        //    db.HRRFHistories.Add(hrrfdatahistory);
                        //    db.SaveChanges();

                        //}
                        //    recruiterid = Convert.ToInt32(hrrfdata.RECRTR);
                        //    leadid = Convert.ToInt32(hrrfdata.HRNGMGRID);

                        //    recruitername = db.Employees.Where(b => b.EmployeeId == recruiterid).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault();

                        //    leadname = db.Employees.Where(b => b.EmployeeId == leadid).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault();

                        //    if (prevTALeadid != hrrfdata.HRNGMGRID)
                        //    {
                        //        hrrfdatahistory.HRRFNumber = hrrfdata.HRRFNumber;
                        //        hrrfdatahistory.HistoryDescription = hrrfdata.HRRFNumber + " - TA Lead changed from " + prevTALead + " to " + leadname;
                        //        hrrfdatahistory.ModifiedBy = Convert.ToInt32(HttpContext.Session.GetString("EmployeeId"));
                        //        hrrfdatahistory.ModifiedDate = DateTime.Now;
                        //        hrrfdatahistory.Remarks = "Update CRIT TALEAD RECTR";
                        //        hrrfdatahistory.PrevRequestStatus = prevTALead;
                        //        db.HRRFHistories.Add(hrrfdatahistory);
                        //        db.SaveChanges();
                        //    }
                        //    if (prevRectrid != hrrfdata.RECRTR)

                        //    {
                        //        //Insert into HRRF HISTORY
                      
                        //        hrrfdatahistory.HRRFNumber = hrrfdata.HRRFNumber;
                        //        hrrfdatahistory.HistoryDescription = hrrfdata.HRRFNumber + " - Recruiter changed from " + prevRectr + " to " + recruitername;
                        //        hrrfdatahistory.ModifiedBy = Convert.ToInt32(HttpContext.Session.GetString("EmployeeId"));
                        //        hrrfdatahistory.ModifiedDate = DateTime.Now;
                        //        hrrfdatahistory.Remarks = "Update CRIT TALEAD RECTR";
                        //        hrrfdatahistory.PrevRequestStatus = prevRectr;
                        //        db.HRRFHistories.Add(hrrfdatahistory);

                        //        db.SaveChanges();
                        //    }
                        //}
                   }
                    return Json("true");
                }



                catch (Exception ex)
                {
                    Common.WriteExceptionErrorLog(ex);
                    //return RedirectToAction("Error", "Error");
                    return Json("Sessionexpired");
                }
            }
            else
            {
                return Json("Sessionexpired");

                //    return RedirectToAction("SessionExpire", "Signout");
            }
        }

        //Mounikas Changes

        public ActionResult TRProfilesInformation(BenchAssignment js)
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                try
                {
                    if (js.ProfilesInfoList != null)
                    {
                        return View(js);

                    }
                    else
                    {

                        return View();
                    }
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
                return RedirectToAction("SessionExpire", "Signout");
            }
        }
        public ActionResult GetTRProfilesInfobyHRRFNumber(string HrrfNumber)
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                try
                {
                    BenchAssignment js = new BenchAssignment();
                    // js.ProfilesDataList = db.ProfilesInformations.Where(i => i.HRRFNumber == HrrfNumber).ToList();

                    List<ProfilesInfo> RecruiterLedlst = new List<ProfilesInfo>();
                    RecruiterLedlst = db.Set<ProfilesInfo>().FromSqlRaw("exec GetProfilesByHrrfNumber @HrrfNumber",
                      new SqlParameter("HrrfNumber", HrrfNumber)).ToList<ProfilesInfo>();
                    js.ProfilesInfoList = RecruiterLedlst;

                    ViewBag.HRRFNUMBER = HrrfNumber;
                    return View("TRProfilesInformation", js);
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
                //return Json("Sessionexpired");
                return RedirectToAction("SessionExpire", "Signout");
            }
        }
        public ActionResult SaveTRProfilesInformation(ProfilesInformation profiledata)
        {

            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                try
                {
                    //var entity = db.AdminLogin.Where(x => x.ADMIN_ID == userid).SingleOrDefault();
                    //entity.ADMIN_PASSWORD = 'abc';
                    //db.Entry(entity).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    //db.SaveChanges();

                    var profiledetails = db.ProfilesInformations.Where(i => i.HRRFNumber == profiledata.HRRFNumber && i.ProfilesDate == profiledata.ProfilesDate).FirstOrDefault();
                    int Empid = Convert.ToInt32(HttpContext.Session.GetString("EmployeeId"));
                    ProfilesInformation prf = new ProfilesInformation();
                    if (profiledetails == null)
                    {
                        prf.HRRFNumber = profiledata.HRRFNumber;
                        prf.Profiles = profiledata.Profiles;
                        prf.L1 = profiledata.L1;
                        prf.L2 = profiledata.L2;
                        prf.HR = profiledata.HR;
                        prf.PendingInterviews = profiledata.PendingInterviews;
                        prf.ProfilesDate = profiledata.ProfilesDate;
                        prf.CreatedDate = DateTime.Now;
                        prf.ModifiedDate = DateTime.Now;
                        prf.CreatedBy = Empid;
                        prf.ModifiedBy = Empid;
                        db.ProfilesInformations.Add(prf);
                        db.SaveChanges();
                        // return RedirectToAction("GetTRProfilesInfobyHRRFNumber", "SkillOrgUpdate", new { HrrfNumber = profiledata.HRRFNumber });
                        return Json("true");
                    }
                    else
                    {

                        ProfilesInformation existingProfile = db.ProfilesInformations.Find(profiledetails.ProfilesInfoID);
                        if (existingProfile != null)
                        {
                            // prf.ProfilesInfoID = profiledetails.ProfilesInfoID;
                            existingProfile.HRRFNumber = profiledata.HRRFNumber;
                            existingProfile.Profiles = profiledata.Profiles;
                            existingProfile.L1 = profiledata.L1;
                            existingProfile.L2 = profiledata.L2;
                            existingProfile.HR = profiledata.HR;
                            existingProfile.PendingInterviews = profiledata.PendingInterviews;
                            existingProfile.ProfilesDate = profiledata.ProfilesDate;

                            existingProfile.CreatedDate = profiledetails.CreatedDate;
                            existingProfile.CreatedBy = profiledetails.CreatedBy;

                            existingProfile.ModifiedDate = DateTime.Now;
                            existingProfile.ModifiedBy = Empid;
                            // db.Entry(hrrfdetails).State = EntityState.Modified;
                            db.Entry(existingProfile).State = EntityState.Modified;
                            db.SaveChanges();
                            // return RedirectToAction("GetTRProfilesInfobyHRRFNumber", "SkillOrgUpdate", new { HrrfNumber = profiledata.HRRFNumber });

                        }
                        return Json("true");
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
                // return RedirectToAction("SessionExpire", "Signout");
                return Json("Sessionexpired");
            }
        }

        ////Cumulative Count of Profiles

        public ActionResult CumulativeCountofProfiles()
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                try
                {
                    BenchAssignment js = new BenchAssignment();
                    // js.ProfilesDataList = db.ProfilesInformations.Where(i => i.HRRFNumber == HrrfNumber).ToList();

                    List<ProfilesInfo> RecruiterLedlst = new List<ProfilesInfo>();
                    RecruiterLedlst = db.Set<ProfilesInfo>().FromSqlRaw("exec GetCumulativeCountForProfiles").ToList<ProfilesInfo>();
                    js.ProfilesInfoList = RecruiterLedlst;
                    return View(js);
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
                //return Json("Sessionexpired");
                return RedirectToAction("SessionExpire", "Signout");
            }
        }

        //Cumulative count of Profiles

        //Mounikas Changes



        // Srinivas Changes
        public ActionResult SearchTALeadandRecruiter()
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                try
                {
                    List<string> lstRLEmp = db.RoleMasters.Where(r => r.Role == "RL" && r.ApplicationCode == "TALENTREQ").Select(r => r.EmployeeId.ToString()).ToList();
                    List<string> lstRLActiveEmp = db.Employees
                                                    .Where(e => lstRLEmp.Contains(e.EmployeeId.ToString()) && e.IsActive == true)
                                                    .Select(emp => emp.EmployeeId.ToString()).ToList();

                    List<SelectListItem> lstRLEmployees = (from dt in db.Employees.
                                        Where(n => lstRLActiveEmp.Contains(n.EmployeeId.ToString()) && n.IsActive == true)
                                                           select new SelectListItem
                                                           {
                                                               Text = dt.FirstName + " " + dt.LastName,
                                                               Value = dt.EmployeeId.ToString()
                                                           }
                                       ).ToList();
                    //ViewData["_RecruiterName"] =lstRLEmployees.OrderBy(ord => ord.Text).ToList();
                    lstRLEmployees = lstRLEmployees.OrderBy(ord => ord.Text).ToList();
                    ViewData["_RecruiterName"] = new SelectList(lstRLEmployees, "Value", "Text");

                    ViewData["_LeadName"] = new SelectList(lstRLEmployees, "Value", "Text");

                    ValidationModel viewResult = new ValidationModel();
                    string recruiter = null;
                    string tAlead = null;
                    if (recruiter == null)
                    {
                        recruiter = "";
                    }
                    if (tAlead == null)
                    {
                        tAlead = "";
                    }

                    List<RecruiterLedDetails> RecruiterLedlst = new List<RecruiterLedDetails>();
                    RecruiterLedlst = db.Set<RecruiterLedDetails>().FromSqlRaw("exec GetRecruiterLed @recruiter, @tAlead",
                      new SqlParameter("recruiter", recruiter), new SqlParameter("@tAlead", tAlead)).ToList<RecruiterLedDetails>();

                    viewResult = new ValidationModel()
                    {
                        RecruiterLedlst = RecruiterLedlst
                    };

                    return View(viewResult);
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
                return RedirectToAction("SessionExpire", "Signout");
            }
        }
        public ActionResult RecruiterLedlst(int recruiter, int tAlead)
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                try
                {

                    ValidationModel viewResult = new ValidationModel();

                    List<string> lstRLEmp = db.RoleMasters.Where(r => r.Role == "RL" && r.ApplicationCode == "TALENTREQ").Select(r => r.EmployeeId.ToString()).ToList();
                    List<string> lstRLActiveEmp = db.Employees
                                                    .Where(e => lstRLEmp.Contains(e.EmployeeId.ToString()) && e.IsActive == true)
                                                    .Select(emp => emp.EmployeeId.ToString()).ToList();

                    List<SelectListItem> lstRLEmployees = (from dt in db.Employees.
                                        Where(n => lstRLActiveEmp.Contains(n.EmployeeId.ToString()) && n.IsActive == true)
                                                           select new SelectListItem
                                                           {
                                                               Text = dt.FirstName + " " + dt.LastName,
                                                               Value = dt.EmployeeId.ToString()
                                                           }
                                       ).ToList();


                    lstRLEmployees = lstRLEmployees.OrderBy(ord => ord.Text).ToList();
                    ViewData["_RecruiterName"] = new SelectList(lstRLEmployees, "Value", "Text", recruiter);

                    ViewData["_LeadName"] = new SelectList(lstRLEmployees, "Value", "Text", tAlead);

                    string str_recruiter = "";
                    string str_tAlead = "";
                    if (recruiter > 0)
                    {
                        str_recruiter = Convert.ToString(recruiter);
                    }
                    if (tAlead > 0)
                    {
                        str_tAlead = Convert.ToString(tAlead);
                    }
                    List<RecruiterLedDetails> RecruiterLedlst = new List<RecruiterLedDetails>();
                    RecruiterLedlst = db.Set<RecruiterLedDetails>().FromSqlRaw("exec GetRecruiterLed @recruiter, @tAlead",
                      new SqlParameter("recruiter", str_recruiter), new SqlParameter("@tAlead", str_tAlead)).ToList<RecruiterLedDetails>();

                    viewResult = new ValidationModel()
                    {
                        RecruiterLedlst = RecruiterLedlst
                    };
                    return View("SearchTALeadandRecruiter", viewResult);
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
        //Srinivas Changes


    }
}
