using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Configuration;
using System.Web;
using System.Web.Mvc;
using PagedList;
using Trianz.Enterprise.Operations.Models;
using Trianz.Enterprise.Operations.General;

namespace Trianz.Enterprise.Operations.Controllers
{
    public class RoleMasterController : Controller
    {
        private TrianzOperationsEntities db = new TrianzOperationsEntities();
        List<RoleMasters> objlst = new List<RoleMasters>();
        // GET: RoleMaster
        public ActionResult Index(string sortOrder, string currentFilter, string searchString, int? page)
        {
            try
            {
                if (TempData["IsRoleMasterPageAccess"] == null)
                {
                    return RedirectToAction("index", "login");
                }
                else
                { 
                    if (Convert.ToBoolean(TempData["IsRoleMasterPageAccess"]) == false)
                    {
                        return RedirectToAction("index", "login");
                    }
                }

                ViewBag.CurrentSort = sortOrder;
                ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "EmployeeId" : "";
                ViewBag.DateSortParm = String.IsNullOrEmpty(sortOrder) ? "CreatedDate" : "";

                if (searchString != null)
                {
                    page = 1;
                }
                else
                {
                    searchString = currentFilter;
                }

                ViewBag.CurrentFilter = searchString;

                var RoleDetails = from e in db.RoleMasters where e.Practice != null && e.ApplicationCode == "TALENTREQ" select e;
                var EmployeeDetails = from e in db.Employees select e;

                foreach (var role in RoleDetails)
                {
                    RoleMasters objRoleMasters = new RoleMasters();
                    objRoleMasters.EmployeeId = role.EmployeeId;
                    objRoleMasters.Role = role.Role;
                    objRoleMasters.RoleId = role.RoleId;
                    objRoleMasters.Practice = role.Practice;
                    objRoleMasters.CreatedDate = Convert.ToDateTime(role.CreatedDate).Date;
                    objRoleMasters.ModifiedDate = Convert.ToDateTime(role.ModifiedDate).Date;
                    objRoleMasters.CreatedBy = role.CreatedBy;
                    objRoleMasters.ModifiedBy = role.ModifiedBy;
                    int proposed = Convert.ToInt32(objRoleMasters.EmployeeId);
                    var proposedbyname = EmployeeDetails.Where(e => e.EmployeeId == proposed).FirstOrDefault();
                    objRoleMasters.EmpName = proposedbyname.FirstName + " " + proposedbyname.LastName;
                    int proposed1 = Convert.ToInt32(objRoleMasters.ModifiedBy);
                    var proposedbyname1 = EmployeeDetails.Where(e => e.EmployeeId == proposed1).FirstOrDefault();
                    objRoleMasters.EmpName1 = proposedbyname1.FirstName + " " + proposedbyname1.LastName;
                    objlst.Add(objRoleMasters);
                }
                ViewData["Rolelist"] = objlst;
                if (!String.IsNullOrEmpty(searchString))
                {
                    objlst = objlst.Where(s => Convert.ToString(s.EmployeeId).Contains(searchString)).ToList();
                }
                switch (sortOrder)
                {
                    case "EmployeeId":
                        objlst = objlst.OrderByDescending(s => s.EmployeeId).ToList();
                        break;
                    case "CreatedDate":
                        objlst = objlst.OrderBy(s => s.CreatedDate).ToList();
                        break;
                    default:  // Name ascending 
                        objlst = objlst.OrderBy(s => s.EmployeeId).ToList();
                        break;
                }

                int pageSize = 10;
                int pageNumber = (page ?? 1);

                TempData.Keep("IsRoleMasterPageAccess");

                return View(objlst.ToPagedList(pageNumber, pageSize));
            }
            catch (Exception ex)
            {
                ErrorHandling expcls = new ErrorHandling();
                expcls.Error(ex);
                return null;

            }
        }

        // GET: RoleMaster/Details/5
        public ActionResult Details(long? id)
        {
            try
            {
                if (TempData["IsRoleMasterPageAccess"] == null)
                {
                    return RedirectToAction("index", "login");
                }
                else
                {
                    TempData.Keep("IsRoleMasterPageAccess");

                    if (id == null)
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                    }
                    RoleMaster roleMaster = db.RoleMasters.Find(id);
                    if (roleMaster == null)
                    {
                        return HttpNotFound();
                    }

                    return View(roleMaster);
                }
            }
            catch (Exception ex)
            {
                ErrorHandling expcls = new ErrorHandling();
                expcls.Error(ex);
                return null;

            }
        }

        // GET: RoleMaster/Create
        public ActionResult Create()
        {
            try
            {
                if (TempData["IsRoleMasterPageAccess"] == null)
                {
                    return RedirectToAction("index", "login");
                }
                else
                {
                    TempData.Keep("IsRoleMasterPageAccess");

                    string pathValue = ConfigurationManager.AppSettings["RoleManagement"];
                    List<string> lst = new List<string>();
                    lst = pathValue.Split(',').ToList();
                    ViewData["Role"] = lst;
                    //var ServiceLine = (from service in db.MasterLookUps
                    //                   orderby service.LookupName
                    //                   where service.LookupType == "ServiceLine"
                    //                   select service.LookupName).ToList();

                    var ServiceLine = (from data in db.PracticeWiseBenchCodes
                                       select new
                                       {
                                           LookupName = data.Practice,
                                       }).Distinct().ToList().OrderBy(p => p.LookupName);
                    ViewData["ServiceLine"] = ServiceLine;

                    return View();
                }
            }
            catch (Exception ex)
            {
                ErrorHandling expcls = new ErrorHandling();
                expcls.Error(ex);
                return null;

            }
        }

        // POST: RoleMaster/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "RoleId,EmployeeId,Role,Practice")] RoleMaster roleMaster)
        {
            try
            {
                if (ModelState.IsValid)
                {
					//string CreatingUser = User.Identity.Name.Split('\\')[1].ToUpper();
					string usermail = Common.GetAzureLoggedInUserID();
					var empId = (from r in db.Employees
                                 where r.Email == usermail
                                 select r.EmployeeId).FirstOrDefault();
                    roleMaster.CreatedBy = Convert.ToInt64(empId.ToString());

                    roleMaster.CreatedDate = DateTime.UtcNow.Date;
                    roleMaster.ModifiedBy = Convert.ToInt64(empId.ToString());
                    roleMaster.ApplicationCode = "TALENTREQ";
                    roleMaster.ModifiedDate = DateTime.UtcNow.Date;
                    db.RoleMasters.Add(roleMaster);
                    db.SaveChanges();

                    return RedirectToAction("Index");
                }

                return View(roleMaster);
            }
            catch (Exception ex)
            {
                ErrorHandling expcls = new ErrorHandling();
                expcls.Error(ex);
                return null;

            }
        }

        // GET: RoleMaster/Edit/5
        public ActionResult Edit(long? id)
        {
            try
            {
                if (TempData["IsRoleMasterPageAccess"] == null)
                {
                    return RedirectToAction("index", "login");
                }
                else
                {
                    TempData.Keep("IsRoleMasterPageAccess");

                    if (id == null)
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                    }
                    string pathValue = ConfigurationManager.AppSettings["RoleManagement"];
                    List<string> lst = new List<string>();
                    lst = pathValue.Split(',').ToList();
                    ViewData["_Role"] = lst;
                    //var ServiceLine = (from service in db.MasterLookUps
                    //                   orderby service.LookupName
                    //                   where service.LookupType == "ServiceLine"
                    //                   select service.LookupName).ToList();

                    //ViewData["_ServiceLine"] = ServiceLine;
                    var ServiceLine = (from data in db.PracticeWiseBenchCodes
                                       select new
                                       {
                                           LookupName = data.Practice,
                                       }).Distinct().ToList().OrderBy(p => p.LookupName);
                    ViewData["_ServiceLine"] = ServiceLine;
                    RoleMaster roleMaster = db.RoleMasters.Find(id);
                    if (roleMaster == null)
                    {
                        return HttpNotFound();
                    }

                    return View(roleMaster);
                }
            }
            catch (Exception ex)
            {
                ErrorHandling expcls = new ErrorHandling();
                expcls.Error(ex);
                return null;
            }
        }

        // POST: RoleMaster/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "RoleId,EmployeeId,Role,Practice,ModifiedBy")] RoleMaster roleMaster)
        {
            try
            {
                if (ModelState.IsValid)
                {

					//string CreatingUser = User.Identity.Name.Split('\\')[1].ToUpper();
					string usermail = Common.GetAzureLoggedInUserID();
					var empId = (from r in db.Employees
                                 where r.Email == usermail
                                 select r.EmployeeId).FirstOrDefault();

                    var row = db.RoleMasters.Where(r => r.RoleId == roleMaster.RoleId && r.ApplicationCode == "TALENTREQ").FirstOrDefault();
                    row.ModifiedBy = Convert.ToInt64(empId.ToString());
                    row.ModifiedDate = DateTime.UtcNow;
                    row.Practice = roleMaster.Practice;
                    row.Role = roleMaster.Role;
                    db.Entry(row).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                return View(roleMaster);
            }
            catch (Exception ex)
            {
                ErrorHandling expcls = new ErrorHandling();
                expcls.Error(ex);
                return null;

            }
        }

        // GET: RoleMaster/Delete/5
        public ActionResult Delete(long? id)
        {
            try
            {
                if (TempData["IsRoleMasterPageAccess"] == null)
                {
                    return RedirectToAction("index", "login");
                }
                else
                {
                    TempData.Keep("IsRoleMasterPageAccess");

                    if (id == null)
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                    }
                    RoleMaster roleMaster = db.RoleMasters.Find(id);
                    if (roleMaster == null)
                    {
                        return HttpNotFound();
                    }

                    return View(roleMaster);
                }
            }
            catch (Exception ex)
            {
                ErrorHandling expcls = new ErrorHandling();
                expcls.Error(ex);
                return null;
            }
        }

        // POST: RoleMaster/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            try
            {
                RoleMaster roleMaster = db.RoleMasters.Find(id);
                db.RoleMasters.Remove(roleMaster);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ErrorHandling expcls = new ErrorHandling();
                expcls.Error(ex);
                return null;

            }
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    db.Dispose();
                }
                base.Dispose(disposing);
            }
            catch (Exception ex)
            {
                ErrorHandling expcls = new ErrorHandling();
                expcls.Error(ex);


            }
        }

        public JsonResult GetAllSearchProducts(string name = "")
        {
            try
            {
                //get all products

                var RoleDetails = from e in db.RoleMasters select e;
                var EmployeeDetails = from e in db.Employees select e;

                foreach (var role in RoleDetails)
                {
                    RoleMasters objRoleMasters = new RoleMasters();
                    objRoleMasters.EmployeeId = role.EmployeeId;
                    objRoleMasters.Role = role.Role;
                    objRoleMasters.RoleId = role.RoleId;
                    objRoleMasters.Practice = role.Practice;
                    objRoleMasters.CreatedDate = Convert.ToDateTime(role.CreatedDate).Date;
                    objRoleMasters.ModifiedDate = Convert.ToDateTime(role.ModifiedDate).Date;
                    objRoleMasters.CreatedBy = role.CreatedBy;
                    objRoleMasters.ModifiedBy = role.ModifiedBy;
                    int proposed = Convert.ToInt32(objRoleMasters.CreatedBy);
                    var proposedbyname = EmployeeDetails.Where(e => e.EmployeeId == proposed).FirstOrDefault();
                    objRoleMasters.EmpName = proposedbyname.FirstName + " " + proposedbyname.LastName;
                    int proposed1 = Convert.ToInt32(objRoleMasters.ModifiedBy);
                    var proposedbyname1 = EmployeeDetails.Where(e => e.EmployeeId == proposed1).FirstOrDefault();
                    objRoleMasters.EmpName1 = proposedbyname1.FirstName + " " + proposedbyname1.LastName;
                    objlst.Add(objRoleMasters);
                }
                objlst = objlst.Where(s => Convert.ToString(s.EmployeeId).StartsWith(name)).ToList();

                return Json(objlst, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ErrorHandling expcls = new ErrorHandling();
                expcls.Error(ex);
                return null;

            }
        }

        public JsonResult CheckEmployeeId(int Empid)
        {
            string Result = string.Empty;
			//string name = User.Identity.Name.Split('\\')[1].ToLower();
			string usermail = Common.GetAzureLoggedInUserID();
			int LoginEmpID = (from data in db.Employees where (data.Email.Equals(usermail)) select data.EmployeeId).FirstOrDefault();
            string stringOM = (from om in db.RoleMasters where( om.EmployeeId == LoginEmpID && om.ApplicationCode == "TALENTREQ" )select om.Role).FirstOrDefault();

            List<RoleMaster> IsExists = db.RoleMasters.Where(a => a.EmployeeId == Empid && a.ApplicationCode == "TALENTREQ").ToList();
            if (IsExists.Count > 0)
                //If Empployee having OM Role, he can create multiple Prctice's                
                if (stringOM =="OM" && LoginEmpID != Empid)
                {
                    Result = "false";
                }
            else
                Result = "true";
                        
            return Json(Result, JsonRequestBehavior.AllowGet);
        }
    }
}
