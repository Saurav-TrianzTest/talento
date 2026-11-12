
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Trianz.Enterprise.Operations.Models;
using PagedList;
using Trianz.Enterprise.Operations.General;

namespace Trianz.Enterprise.Operations.Controllers
{
    public class DesignationMastersController : Controller
    {
        private TrianzOperationsEntities db = new TrianzOperationsEntities();
        List<DesignationMaster> objlist = new List<DesignationMaster>();

        // GET: DesignationMasters
        public ActionResult Index(string sortOrder, string currentFilter, string searchString, int? page, string ddlGrade, string ddlPractice)
        {
            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            {
                try
                {
                    //Below single statement is added by Sarath, for security reason to access RoleMaster page.
                    TempData["IsRoleMasterPageAccess"] = null;

                    var Grades = (from service in db.MasterLookUps
                                  where service.LookupType == "Grade"
                                  select service.LookupName).ToList();
                    ViewData["grades"] = Grades;

                    //var Practice = (from data in db.MasterLookUps.Where(x => x.LookupType == "ServiceLine")
                    //                select new
                    //                {
                    //                    LookupName = data.Description,
                    //                    Description = data.LookupName,
                    //                }).ToList().OrderBy(p => p.LookupName);
                    //ViewData["_Practice"] = Practice;
                    var Practice = (from data in db.PracticeWiseBenchCodes
                                    select new
                                    {
                                        LookupName = data.Practice,
                                    }).Distinct().ToList().OrderBy(p => p.LookupName);
                    ViewData["_Practice"] = Practice;

                    ViewBag.CurrentSort = sortOrder;
                    if (searchString != null)
                    {
                        page = 1;
                    }
                    else
                    {
                        searchString = currentFilter;
                    }
                    ViewBag.CurrentFilter = searchString;
                    var emps = from e in db.DesignationMasters select e;

                    foreach (var des in emps)
                    {
                        DesignationMaster designationmaster = new DesignationMaster();
                        designationmaster.DesignationID = des.DesignationID;
                        designationmaster.DesignationCode = des.DesignationCode;
                        designationmaster.DesignationName = des.DesignationName;
                        designationmaster.Practice = des.Practice;
                        designationmaster.Grade = des.Grade;
                        designationmaster.CreatedDate = des.CreatedDate;
                        designationmaster.JobDescription = des.JobDescription;
                        objlist.Add(designationmaster);
                    }
                    if (!String.IsNullOrEmpty(searchString))
                    {
                        objlist = objlist.Where(s => s.DesignationName.ToLower().Contains(searchString.ToLower())).ToList();
                    }

                    if (!String.IsNullOrEmpty(ddlGrade))
                    {
                        objlist = objlist.Where(s => s.Grade.ToString() == ddlGrade).ToList();
                    }

                    if (!String.IsNullOrEmpty(ddlPractice))
                    {
                        objlist = objlist.Where(s => s.Practice.Contains(ddlPractice)).ToList();
                    }

                    // var TrNumber = from e in Db.ProposeAssociates select e;

                    objlist = objlist.OrderByDescending(s => s.CreatedDate).ToList();

                    return View(objlist);
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

        public ActionResult Details(long? id)
        {
            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            {
                if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DesignationMaster designationMaster = db.DesignationMasters.Find(id);
            if (designationMaster == null)
            {
                return HttpNotFound();
            }
           
            return View(designationMaster);
        }
            else
            {
                //ermsg = "Session expired";
                //return Json("Sessionexpired", JsonRequestBehavior.AllowGet);
                return RedirectToAction("SessionExpire", "Signout");
            }
        }

        // GET: DesignationMasters/Create
        public ActionResult Create()
        {
            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            {
                try
                {

                    DesignationMaster dm = new DesignationMaster();
                    //var ServiceLine = (from service in db.MasterLookUps
                    //                   where service.LookupType == "ServiceLine"
                    //                   select service.LookupName).ToList();
                    var ServiceLine = (from data in db.PracticeWiseBenchCodes
                                       select new
                                       {
                                           LookupName = data.Practice,
                                       }).Distinct().ToList().OrderBy(p => p.LookupName);
                    ViewData["ServiceLine"] = ServiceLine;

                    var Grades = (from service in db.MasterLookUps
                                  where service.LookupType == "Grade"
                                  select service.LookupName).ToList();
                    ViewData["grades"] = Grades;
                    return View();
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

        // POST: DesignationMasters/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(FormCollection formcollection)
        {
            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        DesignationMaster designationMaster = new DesignationMaster();
                        string usermail = Common.GetAzureLoggedInUserID();
                        //string UName = User.Identity.Name.Substring(7);
                        var EmployeeID = (from emp in db.Employees
                                          where emp.Email == usermail && emp.IsActive == true
                                          select new
                                          {
                                              EmployeeId = emp.EmployeeId
                                          }).FirstOrDefault();
                        designationMaster.DesignationID = Convert.ToInt32(formcollection["DesignationID"]);
                        designationMaster.DesignationCode = formcollection["DesignationCode"];
                        designationMaster.DesignationName = formcollection["DesignationName"];
                        designationMaster.Grade = Convert.ToInt32(formcollection["Grade"]);
                        designationMaster.Practice = formcollection["Practice"];
                        designationMaster.JobDescription = formcollection["JobDescription"];
                        designationMaster.CreatedBy = Convert.ToInt32(EmployeeID.EmployeeId);
                        designationMaster.CreatedDate = DateTime.UtcNow;
                        db.DesignationMasters.Add(designationMaster);
                        db.SaveChanges();
                        return RedirectToAction("Index");
                    }
                    catch (Exception ex)
                    {
                        Common.WriteExceptionErrorLog(ex);
                        return RedirectToAction("Error", "Error");
                        //return Json("Error", JsonRequestBehavior.AllowGet);
                    }

                }

                return View();
        }
            else
            {
                //ermsg = "Session expired";
                //return Json("Sessionexpired", JsonRequestBehavior.AllowGet);
                return RedirectToAction("SessionExpire", "Signout");
            }
        }

        // GET: DesignationMasters/Edit/5
        public ActionResult Edit(long? id)
        {
            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            {

                try
                {
                    //var ServiceLine = (from service in db.MasterLookUps
                    //                   where service.LookupType == "ServiceLine"
                    //                   select service.LookupName).ToList();
                    var ServiceLine = (from data in db.PracticeWiseBenchCodes
                                       select new
                                       {
                                           LookupName = data.Practice,
                                       }).Distinct().ToList().OrderBy(p => p.LookupName);
                    ViewData["ServiceLine"] = ServiceLine;

                    var Grades = (from service in db.MasterLookUps
                                  where service.LookupType == "Grade"
                                  select service.LookupName).ToList();
                    ViewData["grades"] = Grades;

                    var praclist = (from e in db.DesignationMasters where e.DesignationID == id select e.Practice).ToList();
                    ViewData["praclist"] = praclist;

                    var selectedPractice = new List<SelectListItem>();
                    foreach (DesignationMaster item in db.DesignationMasters.Where(e => e.DesignationID == id))
                    {
                        selectedPractice.Add(new SelectListItem() { Text = item.Practice, Value = item.DesignationID.ToString() });
                    }
                    ViewData["selectedPractice"] = selectedPractice;

                    //var sp = new List<SelectListItem>();
                    //foreach (MasterLookUp item in db.MasterLookUps.Where(e => e.LookupType == "ServiceLine"))
                    //    {
                    //    sp.Add(new SelectListItem() { Text = item.LookupName, Value = item.LookupType });
                    //}
                    //ViewData["selectedPractice"] = sp;
                    if (id == null)
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                    }
                    DesignationMaster designationMaster = db.DesignationMasters.Find(id);
                    if (designationMaster == null)
                    {
                        return HttpNotFound();
                    }
                    return View(designationMaster);
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


        // POST: DesignationMasters/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(FormCollection formcollection)
        {
            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            {
                try
            {
                if (ModelState.IsValid)
                {
                    if (Session["Role"] != null && Session["Role"].ToString().ToUpper() == "OM")
                    {
                        DesignationMaster designationMaster = db.DesignationMasters.Find(Convert.ToInt32(formcollection["DesignationID"]));
                        if (designationMaster != null)
                        {
                            designationMaster.JobDescription = formcollection["JobDescription"];

                            db.Entry(designationMaster).State = EntityState.Modified;
                            db.SaveChanges();
                            return RedirectToAction("Index");
                        }
                    }
                    else
                    {
                        DesignationMaster designationMaster = new DesignationMaster();
                        string usermail = Common.GetAzureLoggedInUserID();
                        //string UName = User.Identity.Name.Substring(7);
                        var EmployeeID = (from emp in db.Employees
                                          where emp.Email == usermail && emp.IsActive == true
                                          select new
                                          {
                                              EmployeeId = emp.EmployeeId
                                          }).FirstOrDefault();
                        designationMaster.DesignationID = Convert.ToInt32(formcollection["DesignationID"]);
                        designationMaster.DesignationCode = formcollection["DesignationCode"];
                        designationMaster.DesignationName = formcollection["DesignationName"];
                        designationMaster.Grade = Convert.ToInt32(formcollection["Grade"]);
                        designationMaster.Practice = formcollection["Practice"];

                        designationMaster.JobDescription = formcollection["JobDescription"];
                        //designationMaster.CreatedBy = Convert.ToInt32(formcollection["CreatedBy"]);
                        //designationMaster.CreatedDate = Convert.ToDateTime(formcollection["CreatedDate"]);
                        designationMaster.ModifiedBy = Convert.ToInt32(EmployeeID.EmployeeId);
                        designationMaster.ModifiedDate = DateTime.UtcNow;
                        db.Entry(designationMaster).State = EntityState.Modified;
                        db.SaveChanges();
                        return RedirectToAction("Index");
                    }
                }
                return View();
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                Exception raise = ex;
                foreach (var validationErrors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        string message = string.Format("{0}:{1}",
                            validationErrors.Entry.Entity.ToString(),
                            validationError.ErrorMessage);
                        // raise a new exception nesting
                        // the current instance as InnerException
                        raise = new InvalidOperationException(message, raise);
                    }
                }
                ErrorHandling expcls = new ErrorHandling();
                expcls.Error(raise);
                throw raise;
            }
        }
            else
            {
                //ermsg = "Session expired";
                //return Json("Sessionexpired", JsonRequestBehavior.AllowGet);
                return RedirectToAction("SessionExpire", "Signout");
            }
        }


        // GET: DesignationMasters/Delete/5
        public ActionResult Delete(long? id)
        {
            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            {

                if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DesignationMaster designationMaster = db.DesignationMasters.Find(id);
            if (designationMaster == null)
            {
                return HttpNotFound();
            }
            return View(designationMaster);
        }
            else
            {
                //ermsg = "Session expired";
                //return Json("Sessionexpired", JsonRequestBehavior.AllowGet);
                return RedirectToAction("SessionExpire", "Signout");
            }
        }

        // POST: DesignationMasters/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            {
                try
                {
                    DesignationMaster designationMaster = db.DesignationMasters.Find(id);
                    db.DesignationMasters.Remove(designationMaster);
                    db.SaveChanges();
                    return RedirectToAction("Index");
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
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public JsonResult CheckDisignationCode(string DesigCode)
        {
            if (Convert.ToInt32(Session["EmployeeId"]) > 0)
            {
                try
                {

                    string Result = string.Empty;

                    List<DesignationMaster> listmaster = db.DesignationMasters.Where(d => d.DesignationCode == DesigCode).ToList();

                    if (listmaster.Count > 0)
                        Result = "false";
                    else
                        Result = "true";
                    return Json(Result, JsonRequestBehavior.AllowGet);
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

    }
}
