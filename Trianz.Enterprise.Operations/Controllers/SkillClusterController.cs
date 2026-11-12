using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Trianz.Enterprise.Operations.General;
using Trianz.Enterprise.Operations.Models;
using Trianz.Enterprise.Operations.ViewModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Session;
namespace Trianz.Enterprise.Operations.Controllers
{
    public class SkillClusterController : Controller
    {
        private TrianzOperationsEntities db = new TrianzOperationsEntities();
        public ActionResult Index()
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                return View(db.PRTCSKILLCLUSTERs.ToList());
            }
            else
            {
                //ermsg = "Session expired";
               // return Json("Sessionexpired");
                return RedirectToAction("SessionExpire", "Signout");
            }
        }

        public ActionResult Create()
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                try
                {
                    SkillCluster Cluster = new SkillCluster();
                    GetSkillCluster();
                    return View(Cluster);
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

        private void GetSkillCluster()
        {
          //  (from data in db.PracticeWiseBenchCodes.Where(x => x.Practice.ToLower() != "sg&a" && x.Practice.ToLower() != "business engagement")
                var Practice = (from data in db.PracticeWiseBenchCodes.Where(x => x.Practice.ToLower() != "business engagement")
                                select new
                                {
                                    LookupName = data.Practice
                                }).Distinct().ToList().OrderBy(p => p.LookupName);
                ViewData["_Practice"] = Practice;
            
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection form, [Microsoft.AspNetCore.Mvc.Bind] SkillCluster skillCluster)
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                try
                {
                    SkillCluster skillCluster1 = new SkillCluster();
                    string skillCode = form["SKILLCODE"];
                    string practice = form["PRACTICE"];
                    string skillClusters = form["SKILLCLUSTER"];
                    string cskill = form["CSKILLS"];
                    string JD = form["CLSTRJD"];
                    var isSkillCodeExist = db.PRTCSKILLCLUSTERs.Where(x => x.SKILLCODE == skillCode).FirstOrDefault();
                    var isSkillCodeExists = db.SKILLCLSTRJDs.Where(x => x.SKILLCODE == skillCode).FirstOrDefault();
                    GetSkillCluster();
                    if (isSkillCodeExists != null && isSkillCodeExist != null)
                    {
                        ModelState.AddModelError("SKILLCODE", "Skill Code already exist...");
                        return View(skillCluster1);
                    }
                    else if (!skillCode.Contains("_"))
                    {
                        ModelState.AddModelError("SKILLCODE", "Skill Code format not valid...");
                        return View(skillCluster1);
                    }
                    PRTCSKILLCLUSTER prtSkill = new PRTCSKILLCLUSTER()
                    {
                        SKILLCODE = skillCode,
                        CSKILLS = cskill,
                        PRACTICE = practice,
                        SKILLCLUSTER = skillClusters
                    };
                    db.PRTCSKILLCLUSTERs.Add(prtSkill);
                    SKILLCLSTRJD sKILLCLSTRJD = new SKILLCLSTRJD()
                    {
                        SKILLCODE = skillCode,
                        CLSTRJD = JD
                    };
                    db.SKILLCLSTRJDs.Add(sKILLCLSTRJD);
                    db.SaveChanges();
                    return RedirectToAction("Index", "SkillCluster");
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
        [HttpGet]
        public ActionResult Edit(string SKILLCODE)
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                try
                {
                    var jd = db.SKILLCLSTRJDs.Where(y => y.SKILLCODE == SKILLCODE).FirstOrDefault();
                    var skill = db.PRTCSKILLCLUSTERs.Where(y => y.SKILLCODE == SKILLCODE).FirstOrDefault();
                    SkillCluster cluster = new SkillCluster();
                    if (skill != null)
                    {
                        cluster.CLSTRJD = jd != null ? jd.CLSTRJD : "";
                        cluster.SKILLCODE = skill.SKILLCODE;
                        cluster.PRACTICE = skill.PRACTICE;
                        cluster.CSKILLS = skill.CSKILLS;
                        cluster.SKILLCLUSTER = skill.SKILLCLUSTER;
                    }
                    return View(cluster);
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


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(IFormCollection form,[Microsoft.AspNetCore.Mvc.Bind] SkillCluster skillCluster)
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                try
                {
                    string skillCode = Request.Form["SKILLCODE"].ToString();
                    string jd = Request.Form["CLSTRJD"];
                    var skillJD = db.SKILLCLSTRJDs.Where(x => x.SKILLCODE == skillCode).FirstOrDefault();
                    if (skillJD != null)
                    {
                        skillJD.CLSTRJD = jd;
                        skillJD.SKILLCODE = skillCode;
                        db.Entry(skillJD).State = EntityState.Modified;
                        db.SaveChanges();
                        //ViewData["message"] = "Description updated suscessfully...";
                    }
                    return RedirectToAction("Index", "SkillCluster");
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


        [HttpGet]
        public ActionResult Delete(string SKILLCODE)
        {
            if (Convert.ToInt32(HttpContext.Session.GetString("EmployeeId")) > 0)
            {
                try
                {
                    var jd = db.SKILLCLSTRJDs.Where(y => y.SKILLCODE == SKILLCODE).FirstOrDefault();
                    var skill = db.PRTCSKILLCLUSTERs.Where(y => y.SKILLCODE == SKILLCODE).FirstOrDefault();
                    var skillcode = db.HRRFs.Where(a => a.SkillCode == SKILLCODE).FirstOrDefault();
                    if (skillcode != null)
                    {
                        // ModelState.AddModelError("SKILLCODE", "Skill Code already exist...");
                        //return RedirectToAction("Index");
                        return Json(false);
                    }
                    else
                    {
                        PRTCSKILLCLUSTER_delete objcluster = new PRTCSKILLCLUSTER_delete();
                        objcluster.PRACTICE = skill.PRACTICE;
                        objcluster.SKILLCLUSTER = skill.SKILLCLUSTER;
                        objcluster.CSKILLS = skill.CSKILLS;
                        objcluster.SKILLCODE = skill.SKILLCODE;
                        objcluster.CreatedBy = Convert.ToInt32(HttpContext.Session.GetString("EmployeeId"));
                        objcluster.CreatedDate = DateTime.Now;
                        db.PRTCSKILLCLUSTER_delete.Add(objcluster);
                        db.SaveChanges();
                        SKILLCLSTRJD_Delete objskill = new SKILLCLSTRJD_Delete();
                        objskill.SKILLCODE = jd.SKILLCODE;
                        objskill.CLSTRDESC = jd.CLSTRDESC;
                        objskill.CLSTRJD = jd.CLSTRJD;
                        objskill.CLSTRJDVER = jd.CLSTRJDVER;
                        objskill.CreatedBy = Convert.ToInt32(HttpContext.Session.GetString("EmployeeId"));
                        objskill.CreatedDate = DateTime.Now;
                        db.SKILLCLSTRJD_Delete.Add(objskill);
                        db.SaveChanges();
                        var removeskillcluster = db.SKILLCLSTRJDs.Remove(jd);
                        var removeprtskill = db.PRTCSKILLCLUSTERs.Remove(skill);
                        db.SaveChanges();
                        return Json(true);
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
                //ermsg = "Session expired";
                //return Json("Sessionexpired");
                return RedirectToAction("SessionExpire", "Signout");
            }

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
