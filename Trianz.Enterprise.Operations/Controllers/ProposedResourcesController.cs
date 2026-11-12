using X.PagedList;
using System;
using System.Web.Helpers;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Trianz.Enterprise.Operations.Models;
using System.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Session;

namespace Trianz.Enterprise.Operations.Controllers
{
    public class ProposedResourcesController : Controller
    {
        // GET: ProposedResources
        TrianzOperationsEntities Db = new TrianzOperationsEntities();
        public ActionResult ProposedResourcesView(string sortOrder, string currentFilter,string ddlStatus, string searchString, int? page,string text)
        {
            //Below single statement is added by Sarath, for security reason to access RoleMaster page.
            TempData["IsRoleMasterPageAccess"] = null;

            List<ProposeAssociate> listpropose = new List<ProposeAssociate>();
            List<string> liststatus = new List<string>() {ConfigurationManager.AppSettings["App"].ToString(), ConfigurationManager.AppSettings["Rej"].ToString(), ConfigurationManager.AppSettings["Pro"].ToString() };
            ViewBag.CurrentSort = sortOrder;
            //ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "HRRF Number desc" : "";
            ViewData["Status"] = liststatus;
            HttpContext.Session.SetString("Status", ddlStatus.ToString());
            HttpContext.Session.SetString("searchString", searchString.ToString());
            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }
            ViewBag.CurrentFilter = searchString;
            var emps = (from e in Db.ProposeAssociates select e).ToList();
            var temp = emps;
            var SelectedList = new List<ProposeAssociate>();
            
            foreach (var emp in emps)
            {
                ProposeAssociate pa = new ProposeAssociate();
                pa.ProposedID = emp.ProposedID;
                pa.HRRFNumber = emp.HRRFNumber;
                pa.EmpID = emp.EmpID;
                pa.EmpName = emp.EmpName;
                pa.ProjectName = emp.ProjectName;
                pa.Grade = emp.Grade;
                //Practice Status
                if (!string.IsNullOrEmpty(emp.PracticeStatus))
                {
                    pa.PracticeStatus = emp.PracticeStatus;
                }
                else
                {
                    pa.PracticeStatus = "";
                }
                pa.PracticeRemarks = emp.PracticeRemarks;
                //proposed by
                if (!string.IsNullOrEmpty(emp.ProposedBy))
                {
                    int proposed = Convert.ToInt32(emp.ProposedBy);
                    var proposedbyname = Db.Employees.Where(e => e.EmployeeId == proposed).FirstOrDefault();
                    pa.ProposedBy = proposedbyname.FirstName + " " + proposedbyname.LastName;
                }
                else
                {
                    pa.ProposedBy = "";
                }
                //approved by
                //if (!string.IsNullOrEmpty(emp.ApprovedBy))
                //{
                //    int approved = Convert.ToInt32(emp.ApprovedBy);
                //    var approvedbyname = Db.Employees.Where(e => e.EmployeeId == approved).FirstOrDefault();
                //    pa.ApprovedBy = approvedbyname.FirstName + " " + approvedbyname.LastName;
                //}
                //else
                //{
                //    pa.ApprovedBy = "";
                //}
                
                listpropose.Add(pa);
            }
            if (!String.IsNullOrEmpty(searchString))
            {
               
                listpropose = listpropose.Where(s => s.EmpID .Contains(searchString)).ToList();
            }
            if (!String.IsNullOrEmpty(ddlStatus))
            {

                listpropose = listpropose.Where(s => s.PracticeStatus== ddlStatus).ToList();
            }
                        
            #region "New Implementation"
            if (!String.IsNullOrEmpty(text))
            {

                listpropose = listpropose.Where(s => s.EmpID.ToString().StartsWith(text)).ToList();
                if (listpropose.Count() == 0)
                {
                    listpropose = temp;
                }
                else {
                    SelectedList = listpropose;
                }
            }

            if (!String.IsNullOrEmpty(text))
            {
                if (SelectedList.Count() == 0)
                {
                    listpropose = listpropose.Where(s => s.EmpName.ToUpper().Contains(text.ToUpper())).ToList();
                    if (listpropose.Count == 0)
                    {
                        listpropose = temp;
                    }
                    else
                    {
                        SelectedList = listpropose;
                    }
                }
            }

            if (!String.IsNullOrEmpty(text))
            {
                if (SelectedList.Count() == 0)
                {
                    listpropose = listpropose.Where(s => s.Grade.ToString().Equals(text)).ToList();
                    if (listpropose.Count == 0)
                    {
                        listpropose = temp;
                    }
                    else
                    {
                        SelectedList = listpropose;
                    }
                }
            }

            if (!String.IsNullOrEmpty(text))
            {
                if (SelectedList.Count() == 0)
                {
                    listpropose = listpropose.Where(s => s.Practice.ToUpper().StartsWith(text.ToUpper())).ToList();
                    if (listpropose.Count == 0)
                    {
                        listpropose = temp;
                    }
                    else
                    {
                        SelectedList = listpropose;
                    }
                }
            }
            #endregion

            // var TrNumber = from e in Db.ProposeAssociates select e;

            listpropose = listpropose.OrderByDescending(s => s.HRRFNumber).ToList();

            return View(listpropose);
        }
        public void GetExcel()
        {
            try
            {
                var HrrfDetailsExport = new List<ProposeAssociate>();

                List<ProposeAssociate> listpropose = new List<ProposeAssociate>();

                string ddlStatus = string.Empty;
                if (HttpContext.Session.GetString("Status") != null)
                {
                    ddlStatus = HttpContext.Session.GetString("Status").ToString();
                }
                string SearchEmployee = string.Empty;
                if (HttpContext.Session.GetString("searchString") != null)
                {
                    SearchEmployee = HttpContext.Session.GetString("searchString").ToString();
                }

                if (string.IsNullOrEmpty(SearchEmployee))
                {
                    if (ddlStatus == "Accepted" || ddlStatus == "Proposed" || ddlStatus == "Rejected")
                    {
                        if (ddlStatus == "")
                        {
                            ddlStatus = null;
                        }
                        HrrfDetailsExport = (from propose in Db.ProposeAssociates
                                             where (propose.PracticeStatus == ddlStatus || ddlStatus == null)
                                             select propose).OrderByDescending(a => a.EmpID).ToList();


                        //ProposeAssociate pa = new ProposeAssociate();
                        GetEmpName(HrrfDetailsExport, listpropose);



                    }
                    else
                    {
                        HrrfDetailsExport = (from propose in Db.ProposeAssociates select propose).ToList();
                        GetEmpName(HrrfDetailsExport, listpropose);
                    }


                }
                else
                {
                    if (SearchEmployee == "")
                    {
                        SearchEmployee = null;
                    }
                    HrrfDetailsExport = (from propose in Db.ProposeAssociates
                                         where (propose.EmpID == SearchEmployee || SearchEmployee == null)
                                         select propose).OrderByDescending(a => a.EmpID).ToList();

                }



                WebGrid grid = new WebGrid(source: listpropose, canPage: false, canSort: false);
                string gridData = grid.GetHtml(
                columns: grid.Columns(
                  grid.Column("HRRFNumber", "HRRFNumber"),
                  grid.Column("EmpId", "EmpId"),
                  grid.Column("EmpName", "EmpName"),
                  grid.Column("ProjectName", "Project"),
                  grid.Column("Grade", "Grade"),
                  grid.Column("PracticeStatus", "Status"),
                  grid.Column("PracticeRemarks", "Remarks"),
                  grid.Column("ProposedBy", "ProposedBy")
                               )
                ).ToString();
                Response.Clear();
                Response.Headers.Add("content-disposition", "attachment; filename=ProposeInfo.xls");
                Response.ContentType = "application/excel";
                Response.WriteAsync(gridData);
                

            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                Exception raise = dbEx;
                // EF Core DbUpdateException does not have EntityValidationErrors - skip validation error iteration
                ErrorHandling expcls = new ErrorHandling();
                expcls.Error(raise);
                throw raise;
            }

        }

        private void GetEmpName(List<ProposeAssociate> HrrfDetailsExport, List<ProposeAssociate> listpropose)
        {
            foreach (ProposeAssociate a in HrrfDetailsExport)
            {
                Int32 id = Convert.ToInt32(a.ProposedBy);
                var proposedbyname = Db.Employees.Where(e => e.EmployeeId == id).FirstOrDefault();
                a.ProposedBy = proposedbyname.FirstName + " " + proposedbyname.LastName;
                listpropose.Add(a);

            }
        }
    }
}
