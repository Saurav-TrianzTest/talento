using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using X.PagedList;
using Trianz.Enterprise.Operations.General;
using Trianz.Enterprise.Operations.Models;
using Microsoft.EntityFrameworkCore;

namespace Trianz.Enterprise.Operations.Controllers
{
    public class MasterLookUpsController : Controller
    {
        private TrianzOperationsEntities db = new TrianzOperationsEntities();

        // GET: MasterLookUps
        public ActionResult Index(string sortOrder, string currentFilter, string searchString, int? page)
        {
            try
            {
                //Below single statement is added by Sarath, for security reason to access RoleMaster page.
                TempData["IsRoleMasterPageAccess"] = null;

                ViewBag.CurrentSort = sortOrder;
                ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "LookUpType" : "";
                //  ViewBag.DateSortParm = sortOrder == "Date" ? "date_desc" : "Date";

                if (searchString != null)
                {
                    page = 1;
                }
                else
                {
                    searchString = currentFilter;
                }

                ViewBag.CurrentFilter = searchString;

                var students = from s in db.MasterLookUps
                               select s;
                if (!String.IsNullOrEmpty(searchString))
                {
                    students = students.Where(s => s.LookupType.Contains(searchString));
                }
                List<MasterLookUp> objlst = new List<MasterLookUp>();
                foreach (var role in students)
                {
                    MasterLookUp obj = new MasterLookUp();
                    obj.LookupID = role.LookupID;
                    obj.ApplicationCode = role.ApplicationCode;
                    obj.LookupCode = role.LookupCode;
                    obj.LookupType = role.LookupType;
                    obj.LookupName = role.LookupName;
                    obj.Description = role.Description;
                    obj.SeqNumber = role.SeqNumber;
                    obj.ParentCode = role.ParentCode;
                    obj.ParentName = role.ParentName;
                    obj.Field1 = role.Field1;
                    obj.Field2 = role.Field2;
                    obj.Field3 = role.Field3;
                    obj.Active = role.Active;
                    obj.DateCreated = role.DateCreated;
                    obj.DateModified = role.DateModified;
                    obj.CreatedBy = role.CreatedBy;
                    obj.ModifiedBy = role.ModifiedBy;
                    objlst.Add(obj);
                }
                switch (sortOrder)
                {
                    case "LookUpType":
                        objlst = objlst.OrderByDescending(s => s.LookupType).ToList();
                        break;
                    default:  // Name ascending 
                        objlst = objlst.OrderBy(s => s.LookupType).ToList();
                        break;
                }

                int pageSize = 10;
                int pageNumber = (page ?? 1);
                return View(objlst.ToPagedList(pageNumber, pageSize));

            }
            catch (Exception ex)
            {
                ErrorHandling expcls = new ErrorHandling();
                expcls.Error(ex);
                return null;

            }
        }

        // GET: MasterLookUps/Details/5
        public ActionResult Details(long? id)
        {
            try
            {
                if (id == null)
                {
                    return BadRequest();
                }
                MasterLookUp masterLookUp = db.MasterLookUps.Find(id);
                if (masterLookUp == null)
                {
                    return NotFound();
                }
                return View(masterLookUp);

            }
            catch (Exception ex)
            {
                ErrorHandling expcls = new ErrorHandling();
                expcls.Error(ex);
                return null;

            }
        }

        // GET: MasterLookUps/Create
        public ActionResult Create()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                ErrorHandling expcls = new ErrorHandling();
                expcls.Error(ex);
                return null;

            }
        }

        // POST: MasterLookUps/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Microsoft.AspNetCore.Mvc.Bind] MasterLookUp masterLookUp)
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
                    masterLookUp.CreatedBy = Convert.ToString(empId);
                    masterLookUp.DateCreated = DateTime.Now.Date;
                    masterLookUp.ModifiedBy = empId.ToString();
                    masterLookUp.DateModified = DateTime.Now.Date;
                    db.MasterLookUps.Add(masterLookUp);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }

                return View(masterLookUp);
            }
            catch (Exception ex)
            {
                ErrorHandling expcls = new ErrorHandling();
                expcls.Error(ex);
                return null;

            }
        }

        // GET: MasterLookUps/Edit/5
        public ActionResult Edit(long? id)
        {
            try
            {
                if (id == null)
                {
                    return BadRequest();
                }
                MasterLookUp masterLookUp = db.MasterLookUps.Find(id);
                if (masterLookUp == null)
                {
                    return NotFound();
                }
                return View(masterLookUp);
            }
            catch (Exception ex)
            {
                ErrorHandling expcls = new ErrorHandling();
                expcls.Error(ex);
                return null;

            }
        }

        // POST: MasterLookUps/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Microsoft.AspNetCore.Mvc.Bind] MasterLookUp masterLookUp)
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
                    var row = db.MasterLookUps.Where(r => r.LookupID == masterLookUp.LookupID).FirstOrDefault();
                    row.Active = masterLookUp.Active;
                    row.ApplicationCode = masterLookUp.ApplicationCode;
                    row.LookupCode = masterLookUp.LookupCode;
                    row.LookupID = masterLookUp.LookupID;
                    row.LookupName = masterLookUp.LookupName;
                    row.LookupType = masterLookUp.LookupType;
                    row.Description = masterLookUp.Description;
                    row.SeqNumber = masterLookUp.SeqNumber;
                    row.ParentCode = masterLookUp.ParentCode;
                    row.ParentName = masterLookUp.ParentName;
                    row.ModifiedBy = empId.ToString();
                    row.DateModified = DateTime.Now.Date;

                    db.Entry(row).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                return View(masterLookUp);
            }
            catch (Exception ex)
            {
                ErrorHandling expcls = new ErrorHandling();
                expcls.Error(ex);
                return null;

            }
        }

        // GET: MasterLookUps/Delete/5
        public ActionResult Delete(long? id)
        {
            try
            {
                if (id == null)
                {
                    return BadRequest();
                }
                MasterLookUp masterLookUp = db.MasterLookUps.Find(id);
                if (masterLookUp == null)
                {
                    return NotFound();
                }
                return View(masterLookUp);
            }
            catch (Exception ex)
            {
                ErrorHandling expcls = new ErrorHandling();
                expcls.Error(ex);
                return null;

            }
        }

        // POST: MasterLookUps/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            try
            {
                MasterLookUp masterLookUp = db.MasterLookUps.Find(id);
                db.MasterLookUps.Remove(masterLookUp);
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
    }
}
