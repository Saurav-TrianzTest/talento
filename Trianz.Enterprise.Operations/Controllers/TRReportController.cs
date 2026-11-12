using System;
using System.Web.Helpers;
using System.Collections.Generic;
using System.Data;
using Microsoft.EntityFrameworkCore;
using System.IO;
//using System.DirectoryServices;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Configuration;
using Trianz.Enterprise.Operations.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Session;

namespace Trianz.Enterprise.Operations.Controllers
{
    public class TRReportController : Controller
    {
        TrianzOperationsEntities db = new TrianzOperationsEntities();
        // GET: TRReport

        public ActionResult Index(string ddlCreateddate)
        {
            //Below single statement is added by Sarath, for security reason to access RoleMaster page.
            TempData["IsRoleMasterPageAccess"] = null;

            List<Employee> result = new List<Employee>();
            HRRF hrrfobj = new HRRF();
            List<string> list = new List<string>() { "Last 7 Days", "Last 14 Days", "Last 21 Days" };
            var hrrf = (from c in db.HRRFs
                        select c).ToList();



            //List<EmployeeMaster> empMaster = new List<EmployeeMaster>();
            //////empMaster = emp;

            //empMaster = hrrf.AsEnumerable().Select(p => new EmployeeMaster
            //{
            //    HRRF = new HRRF
            //    {
            //        HRRFNumber = p.HRRFNumber,
            //        HRRFCreatedDate = p.HRRFCreatedDate,
            //        Purpose = p.Purpose,
            //        JobDescription = p.JobDescription,
            //        ProjectName = p.ProjectName,
            //        ProjectCode = p.ProjectCode,
            //        OpportunityCode = p.OpportunityCode,
            //        OpportunityName = p.OpportunityName,
            //        Practice = p.Practice,
            //        LocationName = p.LocationName,
            //        HRRFSubmitedDate = p.HRRFSubmitedDate,
            //        RequestStatus = p.RequestStatus,
            //        AccountName = p.AccountName,
            //        Grade = p.Grade,
            //        Remarks = p.Remarks,

            //        AssignmentStartDate = p.AssignmentStartDate,
            //        AssignmentEndDate = p.AssignmentEndDate

            //    },
            //    empskill=new EmployeeSkill
            //    {
            //        SkillDescription = p.SkillDescription,
            //    }
            //}).ToList<EmployeeMaster>();


            HttpContext.Session.SetString("Request", ddlCreateddate.ToString());
            if (!String.IsNullOrEmpty(ddlCreateddate))
            {
                if (ddlCreateddate == "Last 7 Days")
                {
                    var x = DateTime.Today.AddDays(-7);

                    hrrf = hrrf.Where(c => c.HRRFCreatedDate >= x).OrderByDescending(c => c.HRRFNumber).ToList();

                }
                else if (ddlCreateddate == "Last 14 Days")
                {
                    var x = DateTime.Today.AddDays(-14);

                    hrrf = hrrf.Where(c => c.HRRFCreatedDate >= x).OrderByDescending(c => c.HRRFNumber).ToList();
                }
                else
                {
                    var x = DateTime.Today.AddDays(-21);
                    hrrf = hrrf.Where(c => c.HRRFCreatedDate >= x).OrderByDescending(c => c.HRRFNumber).ToList();

                }
            }
            //else
            //{

            //    hrrf = (from c in db.HRRFs where (c.HRRFCreatedDate > x) orderby c.HRRFNumber descending select c).ToList();
            //}
            return View(hrrf);
        }
        public void GetExcel()
        {
            try
            {
                //  var HrrfDetailsExport = new List<HRRF>();
                var hrrf1 = new List<HRRF>();

                string ddlcreateddate = string.Empty;
                if (HttpContext.Session.GetString("Request") != null)
                {
                    ddlcreateddate = HttpContext.Session.GetString("Request").ToString();
                }
                if (!string.IsNullOrEmpty(ddlcreateddate))
                {
                    if (ddlcreateddate == "Last 7 Days")
                    {
                        var x = DateTime.Today.AddDays(-7);
                        hrrf1 = (from c in db.HRRFs where (c.HRRFCreatedDate >= x) orderby c.HRRFNumber descending select c).ToList();
                    }
                    else if (ddlcreateddate == "Last 14 Days")
                    {
                        var x = DateTime.Today.AddDays(-14);
                        hrrf1 = (from c in db.HRRFs where (c.HRRFCreatedDate >= x) orderby c.HRRFNumber descending select c).ToList();
                    }
                    else
                    {
                        var x = DateTime.Today.AddDays(-21);
                        hrrf1 = (from c in db.HRRFs where (c.HRRFCreatedDate >= x) orderby c.HRRFNumber descending select c).ToList();
                    }

                }
                else
                {

                    hrrf1 = (from c in db.HRRFs orderby c.HRRFNumber descending select c).ToList();
                }
                WebGrid grid = new WebGrid(source: hrrf1, canPage: false, canSort: false);
                string gridData = grid.GetHtml(
                columns: grid.Columns(
                  grid.Column("HRRFNumber", "TR#"),

                  grid.Column("JobDescription", "Description"),

                  grid.Column("ProjectCode", "Project code"),
                  grid.Column("ProjectName", "ProjectName"),
                  grid.Column("Practice", "Practice"),
                   grid.Column("LocationName", "Location"),
                  grid.Column("HRRFSubmitedDate", "Submited Date", format: (item) => item.HRRFSubmitedDate == null ? string.Empty : string.Format("{0:MM-dd-yyyy}", item.HRRFSubmitedDate)),
                  grid.Column("RequestStatus", "Status"),
                  grid.Column("HRRFCreatedDate", "Created Date", format: (item) => item.HRRFCreatedDate == null ? string.Empty : string.Format("{0:MM-dd-yyyy}", item.HRRFCreatedDate)),
                  grid.Column("Purpose", "Purpose"),

                  grid.Column("OpportunityCode", "OpportunityCode"),
                  grid.Column("OpportunityName", "OpportunityName"),
                  grid.Column("AccountName", "AccountName"),
                  grid.Column("Grade", "Grade"),
                  grid.Column("Remarks", "Fullfilment Remarks"),

                  //grid.Column("SkillDescription", "Primary Skill Set"),
                  grid.Column("AssignmentStartDate", "Expected StartDate of Assignment", format: (item) => item.AssignmentStartDate == null ? string.Empty : string.Format("{0:yyyy-MM-dd}", item.AssignmentStartDate)),
                  grid.Column("AssignmentEndDate", "Expected Fulfilment Date", format: (item) => item.AssignmentEndDate == null ? string.Empty : string.Format("{0:yyyy-MM-dd}", item.AssignmentEndDate))

                  )).ToString();
                Response.Clear();
                Response.Headers.Add("content-disposition", "attachment; filename=TrReportInfo.xls");
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
        //external hire report 
        public void GetExternal()
        {
            try
            {
                //  var HrrfDetailsExport = new List<HRRF>();

                var hrrf1 = (from c in db.HRRFs
                             join eh in db.ExternalHires on c.HRRFNumber equals eh.HRRFNumber
                             where  eh.RequestStatus=="Fulfilled"
                             select new
                             {
                                 c.HRRFNumber,
                                 c.JobDescription,
                                 c.ProjectCode,
                                 c.ProjectName,
                                 c.Practice,
                                 c.LocationName,
                                 c.HRRFSubmitedDate,
                                 c.RequestStatus,
                                 c.HRRFCreatedDate,
                                 c.Purpose,
                                 c.OpportunityCode,
                                 c.OpportunityName,
                                 c.AccountName,
                                 c.Grade,
                                 c.AssignmentStartDate,
                                 c.AssignmentEndDate,
                                 c.Remarks,
                                 eh.FulfilmentDate,
                                 eh.DOJ
                             }).ToList();
                WebGrid grid = new WebGrid(source: hrrf1, canPage: false, canSort: false);
                string gridData = grid.GetHtml(
                columns: grid.Columns(
                  grid.Column("HRRFNumber", "TR#"),
                  grid.Column("ProjectCode", "Project code"),
                  grid.Column("ProjectName", "ProjectName"),
                  grid.Column("Practice", "Practice"),
                   grid.Column("LocationName", "Location"),
                  grid.Column("HRRFSubmitedDate", "Submited Date", format: (item) => item.HRRFSubmitedDate == null ? string.Empty : string.Format("{0:MM-dd-yyyy}", item.HRRFSubmitedDate)),
                  grid.Column("RequestStatus", "Status"),
                  grid.Column("HRRFCreatedDate", "Created Date", format: (item) => item.HRRFCreatedDate == null ? string.Empty : string.Format("{0:MM-dd-yyyy}", item.HRRFCreatedDate)),
                  grid.Column("Purpose", "Purpose"),
                  grid.Column("OpportunityCode", "OpportunityCode"),
                  grid.Column("OpportunityName", "OpportunityName"),
                  grid.Column("AccountName", "AccountName"),
                  grid.Column("Grade", "Grade"),
                  grid.Column("Remarks", "Fullfilment Remarks"),
                  //grid.Column("SkillDescription", "Primary Skill Set"),
                  grid.Column("AssignmentStartDate", "Expected StartDate of Assignment", format: (item) => item.AssignmentStartDate == null ? string.Empty : string.Format("{0:yyyy-MM-dd}", item.AssignmentStartDate)),
                  grid.Column("AssignmentEndDate", "Expected Fulfilment Date", format: (item) => item.AssignmentEndDate == null ? string.Empty : string.Format("{0:yyyy-MM-dd}", item.AssignmentEndDate)),
                  grid.Column("DOJ", "DOJ", format: (item) => item.DOJ == null ? string.Empty : string.Format("{0:yyyy-MM-dd}", item.DOJ)),
                  grid.Column("DOJ", "month of joining", format: (item) => item.DOJ == null ? string.Empty : string.Format("{0:MM}", item.DOJ)),
                  grid.Column("FulfilmentDate", "FulfilmentDate", format: (item) => item.FulfilmentDate == null ? string.Empty : string.Format("{0:yyyy-MM-dd}", item.FulfilmentDate)),
                  grid.Column("FulfilmentDate", "Fulfilment Month", format: (item) => item.FulfilmentDate == null ? string.Empty : string.Format("{0:MM}", item.FulfilmentDate))
                  )).ToString();
                Response.Clear();
                Response.Headers.Add("content-disposition", "attachment; filename=TrReportInfo.xls");
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
    }
}
