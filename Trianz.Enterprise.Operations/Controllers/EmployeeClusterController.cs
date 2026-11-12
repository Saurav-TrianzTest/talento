using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Trianz.Enterprise.Operations.General;
using Trianz.Enterprise.Operations.Models;

namespace Trianz.Enterprise.Operations.Controllers
{
    public class EmployeeClusterController : Controller
    {
        private TrianzOperationsEntities db = new TrianzOperationsEntities();

        // GET: LeadMapping
        public ActionResult Index()
        {
            int Empid = 0;
            string strRole = null;
            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            {
                try
                {
                    if (Session["ClusterManager"].ToString() == "1")
                    {
                        strRole = "OM";
                    }
                    if (strRole == null)
                    {
                        strRole = Session["Role"].ToString();
                    }
                    ViewBag.review = "false";
                    ViewBag.IsInactive = false;
                    ViewBag.Isresume = false;
                    ViewBag.ClusterLead = "checked";
                    if (Session["LeadId"] != null)
                    {
                        Empid = Convert.ToInt32(Session["LeadId"].ToString());
                    }
                    ViewBag.Role = strRole;
                
                    // lstCluster = db.Database.SqlQuery<ClusterLead>("exec GetClusterTeamMember").ToList();
                    List<ClusterLead> lstCluster = new List<ClusterLead>();

                    lstCluster = db.Database.SqlQuery<ClusterLead>("exec GetClusterTeamMember_test").ToList();
                    if ((strRole == "OM" || strRole == "DH") && Session["LeadId"] == null)
                    {
                        //lstCluster = lstCluster.Where(p => p.Role == "CL").ToList();
                        //lstCluster = db.Database.SqlQuery<ClusterLead>("exec GetClusterTeamMember").ToList();
                        var Practice = (from pr in db.PracticeMasters
                                        select new
                                        {
                                            PracticeName = pr.PracticeName,
                                            PracticeID = pr.PracticeID
                                        }).Where(p => p.PracticeID < 4).Distinct().ToList().OrderBy(p => p.PracticeName);

                        ViewData["Practic"] = new SelectList(Practice, "PracticeID", "PracticeName");

                        var _skillCluster = (from data in db.ClusterMasters
                                             select new
                                             {
                                                 ClusterName = data.ClusterName,
                                                 ClusterId = data.ClusterID,
                                                 PrimaryCluster = data.PrimaryCluster
                                             }).Where(p => p.PrimaryCluster == "N").Distinct().ToList().OrderBy(p => p.ClusterName);
                        List<SelectListItem> data1 = new List<SelectListItem>();

                        data1.Add(new SelectListItem { Text = "Architect", Value = "A" });
                        data1.Add(new SelectListItem { Text = "Project Management", Value = "P" });
                        //  List<SelectListItem> list = new List<SelectListItem>();

                        foreach (var item in _skillCluster)
                        {
                            data1.Add(new SelectListItem()
                            {
                                Text = item.ClusterName,
                                Value = Convert.ToString(item.ClusterId)
                            });
                        }


                        ViewData["_skillClusterLead"] = new SelectList(data1, "Value", "Text");
                        //    ViewData["_skillClusterLead"] = new SelectList(_skillCluster, "ClusterId", "ClusterName");

                        var leadNamelst = (from pl in db.PracticeClusterLeads
                                           join p in db.PracticeMasters on pl.PracticeId equals p.PracticeID
                                           join c in db.ClusterMasters on pl.ClusterId equals c.ClusterID
                                           join e in db.Employees on pl.LeadID equals e.EmployeeId
                                           where pl.LeadID == Empid
                                           select new
                                           {
                                               LeadName = e.FirstName + " " + e.LastName,
                                               LeadID = pl.LeadID
                                           }).Distinct().ToList().OrderBy(l => l.LeadName);
                        ViewData["_LeadName"] = new SelectList(leadNamelst, "ClusterId", "ClusterName");

                        List<ClusterLead> lstReport;
                        lstReport = db.Database.SqlQuery<ClusterLead>("exec GetClusterTeamMember_test").ToList<ClusterLead>();

                        List<ClusterLead> lstmembercount;
                        List<ClusterLead> lstLeadcount;
                        List<ClusterLead> praticeMemberlst =new List<ClusterLead>() ;
                        List<ClusterLead> praticeLeadlst= new List<ClusterLead>();
                 

                        lstmembercount = lstReport.Where(p => p.Role == "CM").ToList();
                        praticeMemberlst = lstmembercount.GroupBy(n => n.PracticeName)
                                .Select(n => new ClusterLead
                                {
                                    PracticeName = n.Key,
                                    PracticeMemberCount = n.Count()
                                }).ToList();

                        ViewBag.praticeMemberlst = praticeMemberlst;
                        lstLeadcount = lstReport.Where(p => p.Role == "CL").ToList();

                        praticeLeadlst = lstLeadcount.GroupBy(n => n.PracticeName)
                                 .Select(n => new ClusterLead
                                 {
                                     PracticeName = n.Key,
                                     PracticeLeadCount = n.Count()
                                 }).ToList();
                        ViewBag.praticeLeadlst = praticeLeadlst;

                        List<ClusterLead> groupdmemberList = new List<ClusterLead>();
                        groupdmemberList = lstmembercount.GroupBy(n => n.ClusterName)
                                .Select(n => new ClusterLead
                                {
                                    ClusterName= n.Key,
                                    ClusterMemberCount = n.Count()
                                }).ToList();
                        ViewBag.groupdmemberList = groupdmemberList;

                        List<ClusterLead> leadList = new List<ClusterLead>();
                        leadList = lstLeadcount.GroupBy(n => n.ClusterName)
                                .Select(n => new ClusterLead
                                {
                                    ClusterName = n.Key,
                                    ClusterLeadCount = n.Count()
                                }).ToList();
                        ViewBag.leadList = leadList;

                    }
                    else if (Empid > 0)
                    {
                        //  lstCluster = db.Database.SqlQuery<ClusterLead>("exec [GetClusterTeamMemberByID] @EmployeeId", new System.Data.SqlClient.SqlParameter("EmployeeId", Empid)).ToList();
                        lstCluster = lstCluster.Where(m => m.EmployeeId == Empid).ToList();
                        

                        var Practicelst = (from pl in db.PracticeClusterLeads
                                           join p in db.PracticeMasters on pl.PracticeId equals p.PracticeID
                                           join c in db.ClusterMasters on pl.ClusterId equals c.ClusterID
                                           where pl.LeadID == Empid
                                           select new
                                           {
                                               PracticeName = p.PracticeName,
                                               PracticeID = p.PracticeID
                                           }).Distinct().ToList().OrderBy(p => p.PracticeName);
                        ViewData["Practic"] = new SelectList(Practicelst, "PracticeID", "PracticeName");
                        var Clusterlst = (from pl in db.PracticeClusterLeads
                                          join p in db.PracticeMasters on pl.PracticeId equals p.PracticeID
                                          join c in db.ClusterMasters on pl.ClusterId equals c.ClusterID
                                          where pl.LeadID == Empid
                                          select new
                                          {
                                              ClusterName = c.ClusterName,
                                              ClusterId = c.ClusterID
                                          }).Distinct().ToList().OrderBy(p => p.ClusterName);
                        ViewData["_skillClusterLead"] = new SelectList(Clusterlst, "ClusterId", "ClusterName");

                        List<ClusterLead> lstmembercount;
                        List<ClusterLead> lstLeadcount;
                        List<ClusterLead> praticeMemberlst = new List<ClusterLead>();
                        List<ClusterLead> praticeLeadlst = new List<ClusterLead>();


                        lstmembercount = lstCluster.Where(p => p.Role == "CM").ToList();
                        praticeMemberlst = lstmembercount.GroupBy(n => n.PracticeName)
                                .Select(n => new ClusterLead
                                {
                                    PracticeName = n.Key,
                                    PracticeMemberCount = n.Count()
                                }).ToList();

                        ViewBag.praticeMemberlst = praticeMemberlst;
                        lstLeadcount = lstCluster.Where(p => p.Role == "CL").ToList();

                        praticeLeadlst = lstLeadcount.GroupBy(n => n.PracticeName)
                                 .Select(n => new ClusterLead
                                 {
                                     PracticeName = n.Key,
                                     PracticeLeadCount = n.Count()
                                 }).ToList();
                        ViewBag.praticeLeadlst = praticeLeadlst;

                        List<ClusterLead> groupdmemberList = new List<ClusterLead>();
                        groupdmemberList = lstmembercount.GroupBy(n => n.ClusterName)
                                .Select(n => new ClusterLead
                                {
                                    ClusterName = n.Key,
                                    ClusterMemberCount = n.Count()
                                }).ToList();
                        ViewBag.groupdmemberList = groupdmemberList;

                        List<ClusterLead> leadList = new List<ClusterLead>();
                        leadList = lstLeadcount.GroupBy(n => n.ClusterName)
                                .Select(n => new ClusterLead
                                {
                                    ClusterName = n.Key,
                                    ClusterLeadCount = n.Count()
                                }).ToList();
                        ViewBag.leadList = leadList;

                    }
                    ValidationModel objclusterlst = new ValidationModel();
                    objclusterlst.ClusterLeadlst = lstCluster;
                    return View(objclusterlst);

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
        public JsonResult SkillReview(string Empid)
        {
            try
            {
                if (Convert.ToInt32(Session["EmployeeId"]) > 0)
                {

                    int empid = Convert.ToInt32(Empid);

                    var lstemp = (from pl in db.EmployeeSkills_New
                                  join p in db.SkillMasters on pl.SkillId equals p.SkillId
                                  where pl.EmployeeId == empid
                                  select new
                                  {
                                      SkillId = pl.SkillId,
                                      Skillname = p.Skillset,
                                      IsReviewed = pl.IsReviewed
                                  }).Distinct().ToList();
                    return Json(lstemp, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    //return Json("error", JsonRequestBehavior.AllowGet);
                    return Json("Sessionexpired", JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Common.WriteExceptionErrorLog(ex);
              //  return Json("Error", JsonRequestBehavior.AllowGet);
                return Json("Sessionexpired", JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetClusterdetailsByreview(string review, string resume)
        {
            int reviewid = 0;
            int resumeid = 0;
           // ViewBag.Role = "OM";
            if (review == "true"||review=="1")
            {
                // reviewid = Convert.ToInt32(review);
                reviewid = 1;
                ViewBag.review = reviewid;
                ViewBag.IsInactive = true;
            }
            //else
            //{
            //    ViewBag.IsInactive = false;
            //}
            if (resume=="true" || resume=="1")
            {
                // resumeid = Convert.ToInt32(resume);
                resumeid = 1;
                ViewBag.Isresume = true;
            }
            //else
            //{
            //    ViewBag.Isresume = false;
            //}
            int Empid = 0;
            List<ClusterLead> lstReport = new List<ClusterLead>();
            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            {
                try
                {
                 var Practice = (from pr in db.PracticeMasters
                                    select new
                                    {
                                        PracticeName = pr.PracticeName,
                                        PracticeID = pr.PracticeID
                                    }).Where(p => p.PracticeID < 4).Distinct().ToList().OrderBy(p => p.PracticeName);

                    ViewData["Practic"] = new SelectList(Practice, "PracticeID", "PracticeName");

                    var _skillCluster = (from data in db.ClusterMasters
                                         select new
                                         {
                                             ClusterName = data.ClusterName,
                                             ClusterId = data.ClusterID,
                                             PrimaryCluster = data.PrimaryCluster
                                         }).Where(p => p.PrimaryCluster == "N").Distinct().ToList().OrderBy(p => p.ClusterName);
                    List<SelectListItem> data1 = new List<SelectListItem>();

                    data1.Add(new SelectListItem { Text = "Architect", Value = "A" });
                    data1.Add(new SelectListItem { Text = "Project Management", Value = "P" });
                    //  List<SelectListItem> list = new List<SelectListItem>();

                    foreach (var item in _skillCluster)
                    {
                        data1.Add(new SelectListItem()
                        {
                            Text = item.ClusterName,
                            Value = Convert.ToString(item.ClusterId)
                        });
                    }


                    ViewData["_skillClusterLead"] = new SelectList(data1, "Value", "Text");
                    //    ViewData["_skillClusterLead"] = new SelectList(_skillCluster, "ClusterId", "ClusterName");

                    var leadNamelst = (from pl in db.PracticeClusterLeads
                                       join p in db.PracticeMasters on pl.PracticeId equals p.PracticeID
                                       join c in db.ClusterMasters on pl.ClusterId equals c.ClusterID
                                       join e in db.Employees on pl.LeadID equals e.EmployeeId
                                       where pl.LeadID == Empid
                                       select new
                                       {
                                           LeadName = e.FirstName + " " + e.LastName,
                                           LeadID = pl.LeadID
                                       }).Distinct().ToList().OrderBy(l => l.LeadName);
                    ViewData["_LeadName"] = new SelectList(leadNamelst, "ClusterId", "ClusterName");

                    lstReport = db.Database.SqlQuery<ClusterLead>("exec GetClusterTeamMember_test").ToList();


                    int leadID = 0;
                    if (Session["LeadId"] != null )
                    {
                        leadID = Convert.ToInt32(Session["LeadId"].ToString());
                        if (reviewid == 1 && resumeid == 0)
                        {
                            lstReport = lstReport.Where(p => p.ReviewFlag == reviewid && p.EmployeeId == leadID).ToList();
                        }
                        if (resumeid == 1 && reviewid == 0)
                        {
                            lstReport = lstReport.Where(p => p.IsResume == 1 && p.ReviewFlag == 0 && p.EmployeeId == leadID).ToList();
                        }
                        if (resumeid == 1 && reviewid == 1)
                        {
                            lstReport = lstReport.Where(p => p.IsResume == 1 && p.ReviewFlag == 1 && p.EmployeeId == leadID).ToList();
                        }
                    }
                    else
                    {
                        if (reviewid == 1 && resumeid == 0)
                        {
                            lstReport = lstReport.Where(p => p.ReviewFlag == reviewid).ToList();
                        }
                        if (resumeid == 1 && reviewid == 0)
                        {
                            lstReport = lstReport.Where(p => p.IsResume == 1 && p.ReviewFlag == 0).ToList();
                        }
                        if (resumeid == 1 && reviewid == 1)
                        {
                            lstReport = lstReport.Where(p => p.IsResume == 1 && p.ReviewFlag == 1).ToList();
                        }
                    }
                


                    //if (reviewid == 1 && resumeid==0)
                    //    {
                    //    lstReport = lstReport.Where(p => p.ReviewFlag == reviewid).ToList();
                    //   }
                    //  if (resumeid ==1 && reviewid ==0)
                    //  {
                    //    lstReport = lstReport.Where(p => p.IsResume == 1 && p.ReviewFlag==0).ToList();
                    //  }
                    //if (resumeid == 1 && reviewid == 1)
                    // {
                    //    lstReport = lstReport.Where(p => p.IsResume == 1 && p.ReviewFlag == 1).ToList();
                    // }

            }

                catch (Exception ex)
                {
                    Common.WriteExceptionErrorLog(ex);
                    return RedirectToAction("Error", "Error");
                    //return Json("Error", JsonRequestBehavior.AllowGet);
                }
                ValidationModel objclusterlst = new ValidationModel();
                objclusterlst.ClusterLeadlst = lstReport;
                return View("Index", objclusterlst);
            }
            else
            {
                return RedirectToAction("SessionExpire", "Signout");
            }
        }
        public ActionResult EmployeeClusterInfo(int prac, int cluster, int lead)
        {
            int Empid = 0;
            string strRole = null;
            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            {
                if (Session["ClusterManager"].ToString() == "1")
            {
                strRole = "OM";
            }
            if (strRole == null)
            {
                strRole = Session["Role"].ToString();
            }
            ViewBag.Role = strRole;
           
                try
                {
                    List<ClusterLead> lstReport = new List<ClusterLead>();
                    lstReport = db.Database.SqlQuery<ClusterLead>("exec GetClusterTeamMember_test").ToList();
                  
                    if (Session["LeadId"] != null)
                    {
                        Empid = Convert.ToInt32(Session["LeadId"].ToString());
                    }
                    if ((strRole == "OM" || strRole == "DH") && Session["LeadId"] == null)
                    {
                        //lstReport = lstReport.Where(p => p.Role == "CL").ToList();

                        var Practice = (from pr in db.PracticeMasters
                                        select new
                                        {
                                            PracticeName = pr.PracticeName,
                                            PracticeID = pr.PracticeID
                                        }).Where(p => p.PracticeID < 4).Distinct().ToList().OrderBy(p => p.PracticeName);
                        ViewData["Practic"] = new SelectList(Practice, "PracticeID", "PracticeName", prac);
                        if ((strRole == "OM" || strRole == "DH") && prac == 0)
                        {
                            var _skillCluster = (from pl in db.PracticeClusterLeads
                                                 join p in db.PracticeMasters on pl.PracticeId equals p.PracticeID
                                                 join c in db.ClusterMasters on pl.ClusterId equals c.ClusterID
                                                 //where pl.PracticeId == prac
                                                 select new
                                                 {
                                                     ClusterName = c.ClusterName,
                                                     ClusterId = c.ClusterID,
                                                     PrimaryCluster = c.PrimaryCluster,
                                                 }).Where(p => p.PrimaryCluster == "N").Distinct().ToList().OrderBy(p => p.ClusterName);
                            List<SelectListItem> datalist = new List<SelectListItem>();
                            //if (prac == 0)
                            //{
                            datalist.Add(new SelectListItem { Text = "Architect", Value = "A" });
                            datalist.Add(new SelectListItem { Text = "Project Management", Value = "P" });
                            //  List<SelectListItem> list = new List<SelectListItem>();
                            //}

                            foreach (var item in _skillCluster)
                            {
                                datalist.Add(new SelectListItem()
                                {
                                    Text = item.ClusterName,
                                    Value = Convert.ToString(item.ClusterId)
                                });
                            }
                            string clustertemp = cluster.ToString();
                            if (clustertemp == "1000")
                            {
                                clustertemp = "A";
                            }
                            if (clustertemp == "1001")
                            {
                                clustertemp = "P";
                            }

                            ViewData["_skillClusterLead"] = new SelectList(datalist, "Value", "Text", clustertemp);
                        }
                        else
                        {
                            var _skillCluster = (from pl in db.PracticeClusterLeads
                                                 join p in db.PracticeMasters on pl.PracticeId equals p.PracticeID
                                                 join c in db.ClusterMasters on pl.ClusterId equals c.ClusterID
                                                 where pl.PracticeId == prac
                                                 select new
                                                 {
                                                     ClusterName = c.ClusterName,
                                                     ClusterId = c.ClusterID,
                                                     PrimaryCluster = c.PrimaryCluster,
                                                 }).Where(p => p.PrimaryCluster == "N").Distinct().ToList().OrderBy(p => p.ClusterName);

                            List<SelectListItem> dataitem = new List<SelectListItem>();
                            //if (prac == 0)
                            //{
                            dataitem.Add(new SelectListItem { Text = "Architects", Value = "A" });
                            dataitem.Add(new SelectListItem { Text = "Project Managers", Value = "P" });
                            //  List<SelectListItem> list = new List<SelectListItem>();                          
                            //}
                            foreach (var item in _skillCluster)
                            {
                                dataitem.Add(new SelectListItem()
                                {
                                    Text = item.ClusterName,
                                    Value = Convert.ToString(item.ClusterId)
                                });
                            }

                            string clustertemp = cluster.ToString();
                            if (clustertemp == "1000")
                            {
                                clustertemp = "A";
                            }
                            if (clustertemp == "1001")
                            {
                                clustertemp = "P";
                            }

                            ViewData["_skillClusterLead"] = new SelectList(dataitem, "Value", "Text", clustertemp);
                            //ViewData["_skillClusterLead"] = new SelectList(dataitem, "Value", "Text", cluster);
                        }
                        var leadNamelst = (from pl in db.PracticeClusterLeads
                                           join p in db.PracticeMasters on pl.PracticeId equals p.PracticeID
                                           join c in db.ClusterMasters on pl.ClusterId equals c.ClusterID
                                           join e in db.Employees on pl.LeadID equals e.EmployeeId
                                           where pl.LeadID == Empid
                                           select new
                                           {
                                               LeadName = e.FirstName + " " + e.LastName,
                                               LeadID = pl.LeadID
                                           }).Distinct().ToList().OrderBy(l => l.LeadName);
                        ViewData["_LeadName"] = new SelectList(leadNamelst, "ClusterId", "ClusterName");
                        if (Session["ClusterManager"].ToString() == "1")
                        {
                            strRole = "OM";
                        }
                        if (strRole == null)
                        {
                            strRole = Session["Role"].ToString();
                        }
                        List<ClusterLead> lstCluster = new List<ClusterLead>();
                        if (Session["LeadId"] != null)
                        {
                            Empid = Convert.ToInt32(Session["LeadId"].ToString());
                        }
                        //lstCluster = db.Database.SqlQuery<ClusterLead>("exec GetClusterTeamMember").ToList();

                        string strClusterLead = "";
                        string strClusterMember = "";
                        if (prac.ToString() != "0")
                        {
                            strClusterLead = "CL";
                            //strClusterMember = "CM";
                        }
                        if (prac.ToString() == "0" && cluster.ToString() == "0")
                        {
                            strClusterLead = "CL";
                            strClusterMember = "CM";
                        }
                        if (prac.ToString() == "0" && cluster.ToString() != "0")
                        {
                            strClusterLead = "CL";
                            strClusterMember = "CM";
                        }
                        if (prac.ToString() != "0" && cluster.ToString() != "0")
                        {
                            strClusterLead = "CL";
                            strClusterMember = "CM";
                        }
                        if ((strRole == "OM" || strRole == "DH"))
                        {
                            if (prac.ToString() != "0")
                            {
                                lstReport = lstReport.Where(p => p.PracticeId == prac || p.PracticeId == 4 || p.PracticeId == 5).ToList();
                            }
                            if (cluster.ToString() != "0" && cluster != 1000 && cluster != 1001)
                            {
                                lstReport = lstReport.Where(p => p.ClusterId == cluster).ToList();
                            }
                            if (cluster == 1000)
                            {
                                lstReport = lstReport.Where(p => p.PrimaryCluster == "A").ToList();
                            }
                            if (cluster == 1001)
                            {
                                lstReport = lstReport.Where(p => p.PrimaryCluster == "P").ToList();
                            }

                            //if (chkLead=="CL" && checMember == "CM")
                            if (strClusterLead == "CL" & strClusterMember == "")
                            {
                                lstReport = lstReport.Where(p => p.Role == strClusterLead).ToList();
                            }
                            if (strClusterLead == "" & strClusterMember == "CM")
                            {
                                lstReport = lstReport.Where(p => p.Role == strClusterMember).ToList();
                            }


                        }
                        //ViewBag.ClusterLead = "";
                        //ViewBag.ClusterMember = "";
                        //if (chckClsterLead != "")
                        //{
                        //    ViewBag.ClusterLead = "checked";
                        //}
                        //if (chckClsterMember != "")
                        //{
                        //    ViewBag.ClusterMember = "checked";
                        //}
                        //List<ClusterLead> lstReport;
                        //lstReport = db.Database.SqlQuery<ClusterLead>("exec GetClusterTeamMember").ToList<ClusterLead>();

                        List<ClusterLead> lstmembercount;
                        List<ClusterLead> lstLeadcount;
                        List<ClusterLead> praticeMemberlst = new List<ClusterLead>();
                        List<ClusterLead> praticeLeadlst = new List<ClusterLead>();


                        lstmembercount = lstReport.Where(p => p.Role == "CM").ToList();
                        lstLeadcount = lstReport.Where(p => p.Role == "CL").ToList();

                        praticeMemberlst = lstmembercount.GroupBy(n => n.PracticeName)
                                    .Select(n => new ClusterLead
                                    {
                                        PracticeName = n.Key,
                                        PracticeMemberCount = n.Count()
                                    }).ToList();

                        ViewBag.praticeMemberlst = praticeMemberlst;
                        lstLeadcount = lstReport.Where(p => p.Role == "CL").ToList();

                        praticeLeadlst = lstLeadcount.GroupBy(n => n.PracticeName)
                                 .Select(n => new ClusterLead
                                 {
                                     PracticeName = n.Key,
                                     PracticeLeadCount = n.Count()
                                 }).ToList();
                        ViewBag.praticeLeadlst = praticeLeadlst;

                        List<ClusterLead> groupdmemberList = new List<ClusterLead>();
                        groupdmemberList = lstmembercount.GroupBy(n => n.ClusterName)
                                .Select(n => new ClusterLead
                                {
                                    ClusterName = n.Key,
                                    ClusterMemberCount = n.Count()
                                }).ToList();
                        ViewBag.groupdmemberList = groupdmemberList;

                        List<ClusterLead> leadList = new List<ClusterLead>();
                        leadList = lstLeadcount.GroupBy(n => n.ClusterName)
                                .Select(n => new ClusterLead
                                {
                                    ClusterName = n.Key,
                                    ClusterLeadCount = n.Count()
                                }).ToList();
                        ViewBag.leadList = leadList;
                    }
                    //newly added code need to confirm
                    //else if ((strRole == "OM" || strRole == "DH") &&  Session["LeadId"] != null)
                    //{
                    //    lstReport = lstReport.Where(m => m.EmployeeId == Empid).ToList();

                    //    var Practicelst = (from pl in db.PracticeClusterLeads
                    //                       join p in db.PracticeMasters on pl.PracticeId equals p.PracticeID
                    //                       join c in db.ClusterMasters on pl.ClusterId equals c.ClusterID
                    //                       where pl.LeadID == Empid
                    //                       select new
                    //                       {
                    //                           PracticeName = p.PracticeName,
                    //                           PracticeID = p.PracticeID
                    //                       }).Distinct().ToList().OrderBy(p => p.PracticeName);
                    //    ViewData["Practic"] = new SelectList(Practicelst, "PracticeID", "PracticeName", prac);
                    //    var Clusterlst = (from pl in db.PracticeClusterLeads
                    //                      join p in db.PracticeMasters on pl.PracticeId equals p.PracticeID
                    //                      join c in db.ClusterMasters on pl.ClusterId equals c.ClusterID
                    //                      where pl.LeadID == Empid
                    //                      select new
                    //                      {
                    //                          ClusterName = c.ClusterName,
                    //                          ClusterId = c.ClusterID
                    //                      }).Distinct().ToList().OrderBy(p => p.ClusterName);
                    //    ViewData["_skillClusterLead"] = new SelectList(Clusterlst, "ClusterId", "ClusterName", cluster);

                    //    List<ClusterLead> lstmembercount;
                    //    List<ClusterLead> lstLeadcount;
                    //    List<ClusterLead> praticeMemberlst = new List<ClusterLead>();
                    //    List<ClusterLead> praticeLeadlst = new List<ClusterLead>();

                    //    lstmembercount = lstReport.Where(p => p.Role == "CM").ToList();
                    //    lstLeadcount = lstReport.Where(p => p.Role == "CL").ToList();

                    //    praticeMemberlst = lstmembercount.GroupBy(n => n.PracticeName)
                    //            .Select(n => new ClusterLead
                    //            {
                    //                PracticeName = n.Key,
                    //                PracticeMemberCount = n.Count()
                    //            }).ToList();

                    //    ViewBag.praticeMemberlst = praticeMemberlst;
                    //    lstLeadcount = lstReport.Where(p => p.Role == "CL").ToList();

                    //    praticeLeadlst = lstLeadcount.GroupBy(n => n.PracticeName)
                    //             .Select(n => new ClusterLead
                    //             {
                    //                 PracticeName = n.Key,
                    //                 PracticeLeadCount = n.Count()
                    //             }).ToList();
                    //    ViewBag.praticeLeadlst = praticeLeadlst;

                    //    List<ClusterLead> groupdmemberList = new List<ClusterLead>();
                    //    groupdmemberList = lstmembercount.GroupBy(n => n.ClusterName)
                    //            .Select(n => new ClusterLead
                    //            {
                    //                ClusterName = n.Key,
                    //                ClusterMemberCount = n.Count()
                    //            }).ToList();
                    //    ViewBag.groupdmemberList = groupdmemberList;

                    //    List<ClusterLead> leadList = new List<ClusterLead>();
                    //    leadList = lstLeadcount.GroupBy(n => n.ClusterName)
                    //            .Select(n => new ClusterLead
                    //            {
                    //                ClusterName = n.Key,
                    //                ClusterLeadCount = n.Count()
                    //            }).ToList();
                    //    ViewBag.leadList = leadList;

                    //    if (prac.ToString() != "0")
                    //    {
                    //        lstReport = lstReport.Where(p => p.PracticeId == prac).ToList();
                    //    }
                    //    if (cluster.ToString() != "0")
                    //    {
                    //        lstReport = lstReport.Where(p => p.ClusterId == cluster).ToList();
                    //    }
                    //    if (prac.ToString() != "0" && cluster.ToString() != "0")
                    //    {
                    //        lstReport = lstReport.Where(p => p.PracticeId == prac && p.ClusterId == cluster).ToList();
                    //    }

                    //}
                    //newly added code need to confirm

                    else if (strRole != "OM" &&  strRole != "DH" && Session["LeadId"] != null)
                    {
                        lstReport = lstReport.Where(m => m.EmployeeId == Empid).ToList();

                        var Practicelst = (from pl in db.PracticeClusterLeads
                                           join p in db.PracticeMasters on pl.PracticeId equals p.PracticeID
                                           join c in db.ClusterMasters on pl.ClusterId equals c.ClusterID
                                           where pl.LeadID == Empid
                                           select new
                                           {
                                               PracticeName = p.PracticeName,
                                               PracticeID = p.PracticeID
                                           }).Distinct().ToList().OrderBy(p => p.PracticeName);
                        ViewData["Practic"] = new SelectList(Practicelst, "PracticeID", "PracticeName", prac);
                        var Clusterlst = (from pl in db.PracticeClusterLeads
                                          join p in db.PracticeMasters on pl.PracticeId equals p.PracticeID
                                          join c in db.ClusterMasters on pl.ClusterId equals c.ClusterID
                                          where pl.LeadID == Empid
                                          select new
                                          {
                                              ClusterName = c.ClusterName,
                                              ClusterId = c.ClusterID
                                          }).Distinct().ToList().OrderBy(p => p.ClusterName);
                        ViewData["_skillClusterLead"] = new SelectList(Clusterlst, "ClusterId", "ClusterName", cluster);

                        List<ClusterLead> lstmembercount;
                        List<ClusterLead> lstLeadcount;
                        List<ClusterLead> praticeMemberlst = new List<ClusterLead>();
                        List<ClusterLead> praticeLeadlst = new List<ClusterLead>();

                        lstmembercount = lstReport.Where(p => p.Role == "CM").ToList();
                        lstLeadcount = lstReport.Where(p => p.Role == "CL").ToList();

                        praticeMemberlst = lstmembercount.GroupBy(n => n.PracticeName)
                                .Select(n => new ClusterLead
                                {
                                    PracticeName = n.Key,
                                    PracticeMemberCount = n.Count()
                                }).ToList();

                        ViewBag.praticeMemberlst = praticeMemberlst;
                        lstLeadcount = lstReport.Where(p => p.Role == "CL").ToList();

                        praticeLeadlst = lstLeadcount.GroupBy(n => n.PracticeName)
                                 .Select(n => new ClusterLead
                                 {
                                     PracticeName = n.Key,
                                     PracticeLeadCount = n.Count()
                                 }).ToList();
                        ViewBag.praticeLeadlst = praticeLeadlst;

                        List<ClusterLead> groupdmemberList = new List<ClusterLead>();
                        groupdmemberList = lstmembercount.GroupBy(n => n.ClusterName)
                                .Select(n => new ClusterLead
                                {
                                    ClusterName = n.Key,
                                    ClusterMemberCount = n.Count()
                                }).ToList();
                        ViewBag.groupdmemberList = groupdmemberList;

                        List<ClusterLead> leadList = new List<ClusterLead>();
                        leadList = lstLeadcount.GroupBy(n => n.ClusterName)
                                .Select(n => new ClusterLead
                                {
                                    ClusterName = n.Key,
                                    ClusterLeadCount = n.Count()
                                }).ToList();
                        ViewBag.leadList = leadList;

                        if (prac.ToString() != "0")
                        {
                            lstReport = lstReport.Where(p => p.PracticeId == prac).ToList();
                        }
                        if (cluster.ToString() != "0")
                        {
                            lstReport = lstReport.Where(p => p.ClusterId == cluster).ToList();
                        }
                        if (prac.ToString() != "0" && cluster.ToString() != "0")
                        {
                            lstReport = lstReport.Where(p => p.PracticeId == prac && p.ClusterId == cluster).ToList();
                        }

                    }
                    ValidationModel objclusterlst = new ValidationModel();
                    objclusterlst.ClusterLeadlst = lstReport;
                    //return View("Index", lstReport);

                    return View("Index", objclusterlst);
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
                return RedirectToAction("SessionExpire", "Signout");
            }
        }



        public ActionResult ExportReport()
        {
            try
            {
                if (Convert.ToInt32(Session["EmployeeId"]) > 0)
                {
                    return Json("success", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    //ermsg = "Session expired";
                   return Json("Sessionexpired", JsonRequestBehavior.AllowGet);
                   // return RedirectToAction("SessionExpire", "Signout");
                }
            }
            catch (Exception ex)
            {
                Common.WriteExceptionErrorLog(ex);
                return Json("Error", JsonRequestBehavior.AllowGet);
            //    return Json("Sessionexpired", JsonRequestBehavior.AllowGet);

            }
        }
        public ActionResult GenerateHRRFReport()
        {
            try
            {
                
                if (Convert.ToInt32(Session["EmployeeId"]) > 0)
                {
                    int? EmpId = Convert.ToInt32(Session["EmployeeId"]);
                    List<ClusterLead> lstHRRF = new List<ClusterLead>();
                    lstHRRF = db.Database.SqlQuery<ClusterLead>("exec GetClusterTeamMember_test").ToList();
                    int LastRow;
                    LastRow = 10;
                    //lstHRRF = lstHRRF.Where(x => x.Remarks != "This is an auto generated TR during bulk assignment" && x.Remarks != "This is an auto generated TR during new employee creation").ToList();
                    #region Export to Excel
                    using (ExcelPackage package = new ExcelPackage())
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("ClusterEmployeeSkillReport");
                        worksheet.TabColor = System.Drawing.Color.Green;
                        worksheet.DefaultRowHeight = 18f;
                        worksheet.Row(1).Height = 20f;
                        using (var range = worksheet.Cells[1, 1, 1, LastRow])
                        {
                            range.Style.Font.Bold = true;
                            range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                            range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#5B9BD5"));
                            range.Style.Font.Color.SetColor(System.Drawing.Color.White);
                            range.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                            range.Style.Border.Right.Color.SetColor(System.Drawing.Color.LightGray);
                        }
                        worksheet.Cells[1, 1].Value = "Team Member Name";
                        //worksheet.Cells[1, 2].Value = "Old TR Number";
                        worksheet.Cells[1, 2].Value = "Practice";
                        worksheet.Cells[1, 3].Value = "Cluster Name";
                        worksheet.Cells[1, 4].Value = "Cluster Lead Name";
                        worksheet.Cells[1, 5].Value = "Account Name";
                        worksheet.Cells[1, 6].Value = "Project Code";
                        worksheet.Cells[1, 7].Value = "Manager";
                        worksheet.Cells[1, 8].Value = "Skills";
                        worksheet.Cells[1, 9].Value = "CL Reviewed";
                        worksheet.Cells[1, 10].Value = "ResumeUploaded";


                        //Set default column width
                        worksheet.DefaultColWidth = 50f;

                        worksheet.Column(1).Width = 40f;
                        worksheet.Column(2).AutoFit(20f);
                        worksheet.Column(3).Width = 20f;
                        worksheet.Column(4).AutoFit(20f);
                        worksheet.Column(5).Width = 30f;
                        worksheet.Column(6).Width = 18f;
                        worksheet.Column(7).Width = 30f;
                        worksheet.Column(8).Width = 60f;
                        worksheet.Column(9).Width = 20f;
                        worksheet.Column(10).Width = 20f;



                        //Add the each row
                        for (int rowIndex = 0, row = 2; rowIndex < lstHRRF.Count; rowIndex++, row++) // row indicates number of rows
                        {

                            worksheet.Cells[row, 1].Value = lstHRRF[rowIndex].TeamMemberName;
                            //worksheet.Cells[row, 2].Value = lstHRRF[rowIndex].OldHRRFNumber;

                            worksheet.Cells[row, 2].Value = lstHRRF[rowIndex].PracticeName;
                            //worksheet.Cells[row, 2].Style.Numberformat.Format = "dd-MMM-yyyy";
                            //worksheet.Cells[row, 2].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                            worksheet.Cells[row, 3].Value = lstHRRF[rowIndex].ClusterName;
                            worksheet.Cells[row, 4].Value = lstHRRF[rowIndex].ClusterLeadName;
                            worksheet.Cells[row, 5].Value = lstHRRF[rowIndex].AccountName;
                            worksheet.Cells[row, 6].Value = lstHRRF[rowIndex].ProjectCode;
                            worksheet.Cells[row, 7].Value = lstHRRF[rowIndex].SupervisorName;
                            //worksheet.Cells[row, 8].Style.WrapText = true;
                            worksheet.Cells[row, 8].Value = lstHRRF[rowIndex].SkillSet;
                            if (lstHRRF[rowIndex].ReviewFlag == 1)
                            {
                                worksheet.Cells[row, 9].Value = "Yes";
                            }
                            else
                            {
                                worksheet.Cells[row, 9].Value = "No";

                            }
                            if (lstHRRF[rowIndex].IsResume == 1)
                            {
                                worksheet.Cells[row, 10].Value = "Yes";
                            }
                            else
                            {
                                worksheet.Cells[row, 10].Value = "No";
                            }

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

                        Byte[] fileBytes = package.GetAsByteArray();
                        Response.Clear();
                        Response.Buffer = true;
                        Response.AddHeader("content-disposition", "attachment;filename=" + "ClusterEmployeeSkill" + "-Report" + DateTime.UtcNow.ToString("_dd-MMM-yyyy_hh-mm-ss") + ".xlsx");

                        Response.Charset = "";
                        Response.ContentType = "application/vnd.ms-excel";
                        StringWriter sw = new StringWriter();
                        Response.BinaryWrite(fileBytes);
                        Response.End();
                    }

                    #endregion

                    return new EmptyResult();
                }

                else

                {
                    return RedirectToAction("SessionExpire", "Signout");
                }
            }
            catch (Exception ex)
            {
                Common.WriteExceptionErrorLog(ex);
                //return Json("Error", JsonRequestBehavior.AllowGet);

                return RedirectToAction("SessionExpire", "Signout");
            }
        }
        public ActionResult DeleteEmployeeSkillByID(string SkillId)
        {
            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
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
                            // em.ModifiedDate = DateTime.UtcNow;
                            //em.SkillID = objDeleteSkill.SkillId;
                            //em.TechnologyLastUsed = objDeleteSkill.TechnologyLastUsed;
                            //em.UpdateStatus = "Deleted";
                            //db.EmployeeSkillHistories.Add(em);

                            db.EmployeeSkills_New.Remove(objDeleteSkill);
                            db.SaveChanges();

                            return Json("deleted", JsonRequestBehavior.AllowGet);
                        }
                    }
                    return Json("", JsonRequestBehavior.AllowGet);
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

        public ActionResult SaveReviewedSkills(List<EmployeeSkills_NewDetails> Skillslist)
        {
            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            {
                try
                {
                    int LogedinEmpId = Convert.ToInt32(Session["EmployeeId"]);
                    List<EmployeeSkills_New> Exatingskills = new List<EmployeeSkills_New>();
                    if (Skillslist.Count > 0)
                    {
                        foreach (var item in Skillslist)
                        {
                            int emskillid = Convert.ToInt32(item.EmployeeId);
                            var reviewdkills = db.EmployeeSkills_New.Where(i => i.SkillId == item.SkillId && i.EmployeeId == item.EmployeeId).FirstOrDefault();
                            if (item.IsPrimary == false && reviewdkills == null)
                                 {
                                EmployeeSkills_New em = new EmployeeSkills_New();
                                em.EmployeeId = item.EmployeeId;
                                em.SkillId = item.SkillId;
                                em.LastModifiedDate = DateTime.UtcNow;
                                db.EmployeeSkills_New.Add(em);
                                db.SaveChanges();
                            }
                                else
                               {
                               
                                var reviewkills = db.EmployeeSkills_New.Where(i => i.SkillId == item.SkillId && i.EmployeeId == item.EmployeeId).FirstOrDefault();
                             
                                reviewkills.EmployeeId = item.EmployeeId;
                                reviewkills.SkillId = item.SkillId;
                                reviewkills.ReviewerId = item.LeadID;
                                reviewkills.LastReviewdDate = DateTime.UtcNow;
                                reviewkills.IsReviewed = item.Reviewed;
                                reviewkills.Expertiselevel = reviewdkills.Expertiselevel;
                                db.Entry(reviewkills).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();

                            }
                        }
                    }
                    return Json("true", JsonRequestBehavior.AllowGet);

                }
                catch (Exception ex)
                {
                    Common.WriteExceptionErrorLog(ex);
                    //return RedirectToAction("Error", "Error");
                    return Json("Sessionexpired", JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
               return Json("Sessionexpired", JsonRequestBehavior.AllowGet);
            //    return RedirectToAction("SessionExpire", "Signout");
            }
        }
    }
}

